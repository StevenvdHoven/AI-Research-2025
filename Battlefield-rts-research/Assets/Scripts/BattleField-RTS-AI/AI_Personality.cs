using System.Collections.Generic;
using UnityEngine;

public enum UnitMissionType
{
    None,
    Attack,
    Defend,
    Retreat,
    Move,
    RetrieveUnit,
}

[System.Serializable]
public struct AI_MissionSetting
{
    public UnitMissionType MissionType;
    public int MaxConcurrent;
    public Vector2Int TroopRange;
}


[CreateAssetMenu(menuName = "RTS/AI Personality")]
public class AI_Personality : ScriptableObject
{
    public string PersonalityName;
    public float MissionCancelThreshold;
    public float CommanderCheckRadius = 7f;
    public float CommanderPowerDifferenceError = 1f;
    public List<AI_MissionSetting> MissionSettings;
}
