using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class DebugConsole : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField inputField;
    public ScrollRect scrollRect;
    public Transform content;
    public GameObject terminalRowPrefab;



    // Command dictionary
    private Dictionary<string, Action<string[]>> commands;

    //command history (saved while in game scene)
    private List<string> history => WindowManager.Instance.debugHistory;


    private void Awake()
    {
        RegisterCommands();

        // Automatically focus the input field when console opens
        inputField.ActivateInputField();
    }

    private void OnEnable()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        foreach (var line in history)
            CreateHistoryRow(line);

        inputField.ActivateInputField();
    }

    private void RegisterCommands()
    {
        commands = new Dictionary<string, Action<string[]>>();

        // Register your commands here
        commands["/help"] = CmdHelp;
        commands["/tp"] = CmdTeleport;
        commands["/give"] = CmdGive;
        commands["/heal"] = CmdHealth;
        commands["/clear"] = CmdClear;
    }

    // Called from InputField "On Submit"
    public void OnSubmit()
    {
        string input = inputField.text.Trim();
        if (input == "") return;

        inputField.text = "";
        AddHistory("> " + input);

        ParseCommand(input);

        // Refocus
        inputField.ActivateInputField();
        inputField.Select();
    }

    private void ParseCommand(string input)
    {
        string[] parts = input.Split(" ");
        string cmd = parts[0].ToLower();

        if (commands.ContainsKey(cmd))
        {
            commands[cmd].Invoke(parts);
        }
        else
        {
            AddHistory("Unknown command. Type /help for a list of commands.");
        }
    }

    // ------------------------------
    // Command Implementations
    // ------------------------------

    private void CmdHelp(string[] args)
    {
        AddHistory("Available commands:");
        AddHistory("/help - show this list");
        AddHistory("/tp <x> <y> - teleport player");
        AddHistory("/give <itemId> - give item to inventory");
        AddHistory("/heal <amount> - heal player");
        AddHistory("/clear - clear console");
    }

    private void CmdClear(string[] args)
    {
        history.Clear();

        foreach (Transform child in content)
            Destroy(child.gameObject);
    }

    private void CmdHealth(string[] args)
    {
        if(args.Length < 2)
        {
            AddHistory("Usage: /heal <amount>");
            return;
        }
        if (!float.TryParse(args[1], out float number))
        {
            AddHistory("Health must be a number");
            return;
        }
        if (number < 0){
            PlayerStateManager.Instance.inflictDamage(-number);
        }
        else
        {
            PlayerStateManager.Instance.heal(number);
        }
        AddHistory($"Player healed by ({number})");
    }

    private void CmdTeleport(string[] args)
    {
        if (args.Length < 3)
        {
            AddHistory("Usage: /tp <x> <y>");
            return;
        }

        if (!float.TryParse(args[1], out float x) ||
            !float.TryParse(args[2], out float y))
        {
            AddHistory("Coordinates must be numbers.");
            return;
        }

        // Find the actual player object
        PlayerMovement player = FindObjectOfType<PlayerMovement>();

        if (player == null)
        {
            AddHistory("Player not found in scene.");
            return;
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        Vector2 target = new Vector2(x, y);

        if (rb != null)
            rb.position = target;
        else
            player.transform.position = target;

        AddHistory($"Teleported to ({x}, {y})");
    }

    private void CmdGive(string[] args)
    {
        if (args.Length < 2)
        {
            AddHistory("Usage: /give <itemId> [amount]");
            return;
        }

        // Parse item ID
        if (!int.TryParse(args[1], out int itemId))
        {
            AddHistory("Item ID must be a number.");
            return;
        }

        // Parse optional amount
        int amount = 1;
        if (args.Length >= 3)
        {
            if (!int.TryParse(args[2], out amount))
            {
                AddHistory("Amount must be a number.");
                return;
            }
        }

        // Get the item from your database
        ItemSO item = ItemDatabase.instance.GetByID(itemId);
        if (item == null)
        {
            AddHistory($"Item with ID {itemId} not found.");
            return;
        }

        // Get the player's inventory
        Inventory inv = FindObjectOfType<Inventory>();
        if (inv == null)
        {
            AddHistory("Inventory not found on player.");
            return;
        }

        bool success = inv.AddItem(item, amount);

        if (success)
            AddHistory($"Added {amount}x {item.itemName} (ID: {itemId})");
        else
            AddHistory("Not enough space in inventory.");
    }

    // ------------------------------
    // UI Helpers
    // ------------------------------

    private void AddHistory(string text)
    {
        history.Add(text);
        CreateHistoryRow(text);

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void CreateHistoryRow(string text)
    {
        GameObject row = Instantiate(terminalRowPrefab, content);
        row.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }
}
