using System;
using UnityEngine;

public interface IPlayerStateService
{
    GameObject GameObject { get; }
    Transform Transform { get; }

    MovementMode CurrentMode { get; }
    float Stamina { get; }
    float MaxStamina { get; }
    float Health { get; }
    float MaxHealth { get; }
    bool IsMoving { get; set; }
    bool IsDead { get; }

    event Action<MovementMode> OnMovementModeChanged;

    void InflictDamage(float amount);
    void AddHealth(float amount);
    void HealMax();
    void AddStamina(float amount);
    void StaminaMax();
    void SetSneak(bool active);
    void SetSprint(bool active);
}