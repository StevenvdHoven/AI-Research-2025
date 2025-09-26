using System.Collections.Generic;
using UnityEngine;

public static class MissionCooldownHandler
{
    private static Dictionary<UnitMissionType, float> cooldowns = new();

    public static void SetCooldown(UnitMissionType type, float duration)
    {
        cooldowns[type] = Time.time + duration;
    }

    public static bool IsOnCooldown(UnitMissionType type)
    {
        return cooldowns.ContainsKey(type) && cooldowns[type] > Time.time;
    }
}
