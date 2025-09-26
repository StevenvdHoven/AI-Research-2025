using UnityEngine;

public enum Factions
{
    Humans,
    Orcs
}


[CreateAssetMenu(fileName = "SharedUnitData", menuName = "ScriptableObjects/SharedUnitData", order = 1)]
public class SharedUnitData : ScriptableObject
{
    [Header("Unit Information")]
    public string Name;
    public Factions Faction;
    public Sprite UnitLogo;

    [Header("Health and Energy")]
    public int MaxHealth = 100;
    public int MaxEnergy = 40;

    [Header("Health Regeneration")]
    public float HealthRegenRate = 1;
    public float HealthRegenDelay = 1;

    [Header("Energy Regeneration")]
    public float EnergyRegenRate = 1;
    public float EnergyRegenDelay = 1;

    [Header("Movement")]
    public float MaxMovementSpeed = 6;
    public float Acceleration = 5;
    public float Decceleration = 10;

    [Header("Attack")]
    public int AttackDamage = 2;
    public float AttackRadius = 1.5f;
    public float AttackRange = 1f;
    public float AttackSpeed = 1;
    public int AttackCost = 10;

    [Header("Dodge")]
    public float DodgeChance = 0.05f;

    [Header("Morale")]
    public float MaxMorale = 100;
    public float MoraleRange = 5.0f;
    public float MoraleRegenRate = .2f;
    public float MoraleDecayRate = 0.2f;

}
