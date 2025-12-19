using System.Collections.Generic;

[System.Serializable]
public class ContainerSaveData
{
    public string id;
    public InventorySaveData items;
    public bool initialized;
    public bool wasOpened;
}
