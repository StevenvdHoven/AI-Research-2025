using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct AttackMissionResult
{
    public List<UnitStats> Targets;
    public UnitStats[] Squad;
}

public class AttackMission : UnitMission
{
    private List<UnitStats> Targets;
    private const float groupRadius = 10f; // Max distance between targets to be considered one group

    public AttackMission(RTS_AI ai, IEnumerable<UnitStats> units, IEnumerable<UnitStats> targets)
        : base(ai, units)
    {
        this.Targets = targets?.Where(t => t != null && t.IsAlive).ToList() ?? new List<UnitStats>();
        FilterTargetGroup();
    }

    public static float PreviewEvaluateMission(RTS_AI ai, UnitStats[] units, IEnumerable<UnitStats> targetGroup)
    {
        var validTargets = targetGroup?.Where(t => t != null && t.IsAlive).ToList();
        if (units == null || units.Length == 0 || validTargets == null || validTargets.Count == 0)
            return 0f;

        // Get center point of all targets
        Vector2 center = Vector2.zero;
        foreach (var target in validTargets)
            center += (Vector2)target.transform.position;
        center /= validTargets.Count;

        var enemyMap = GameManager.Instance.StatsPlayerFaction;
        var allyMap = GameManager.Instance.StatsEnemyFaction;

        float radius = 5f;
        float enemyPowerInArea = enemyMap.GetTotalAttackPowerInRadius(center, radius);
        float allyPowerInArea = allyMap.GetTotalAttackPowerInRadius(center, radius);
        if (allyPowerInArea <= 0f)
            allyPowerInArea = units.Sum(RTS_AI.GetUnitAttackPower);

        float powerRatio = allyPowerInArea / Mathf.Max(enemyPowerInArea, 0.01f);
        float avgDistance = units.Average(unit => Vector2.Distance(unit.transform.position, center));
        float distanceScore = Mathf.InverseLerp(20f, 0f, avgDistance);
        float powerScore = Mathf.Clamp01(powerRatio);

        float finalScore = (0.6f * powerScore) + (0.4f * distanceScore);
        return finalScore;
    }

    public override float EvaluateMission()
    {
        return PreviewEvaluateMission(ai, AssignedUnits.ToArray(), Targets);
    }

    public override void Execute()
    {
        List<UnitStats> closedList = new List<UnitStats>();
        foreach (var unit in AssignedUnits)
        {
            if (unit == null || !unit.IsAlive)
                continue;

            if (unit.TryGetComponent(out UnitCommandHandler commandHandler))
            {
                // Choose the closest alive target
                var closestTarget = Targets
                    .Where(t => t != null && t.IsAlive && !closedList.Contains(t))
                    .OrderBy(t => Vector2.Distance(unit.transform.position, t.transform.position))
                    .FirstOrDefault();

                if (closestTarget != null)
                {
                    commandHandler.OverrideCommand(new UnitAttackCommand(unit, closestTarget));
                    closedList.Add(closestTarget);
                }
            }
        }
    }

    public override void UpdateMission()
    {
        base.UpdateMission();

        AssignedUnits.RemoveAll(u => u == null || !u.IsAlive);

        Targets.RemoveAll(t => t == null || !t.IsAlive);

        bool allUnitsAlive = AllUnitsAlive();
        bool allUnitsMoralBroken = AllUnitsMoralBroken();

        if (Targets.Count == 0)
        {
            Debug.Log("Attack Mission complete: All targets dead.");
            IsComplete = true;
        }
        else if (!allUnitsAlive || allUnitsMoralBroken)
        {
            Debug.Log("Attack Mission failed: Units dead or morale broken.");
            HasFailed = true;
        }
        else
        {
            FilterTargetGroup(); // Remove targets too far from the group
        }

        CheckForTargets();
    }

    private void CheckForTargets()
    {
        Dictionary<UnitStats, int> currentAssignments = Targets.ToDictionary(t => t, t => 0);

        // Reassign units without valid target
        foreach (var unit in AssignedUnits)
        {
            if (unit == null || !unit.IsAlive)
                continue;

            if (!unit.TryGetComponent(out UnitCommandHandler commandHandler))
                continue;

            var currentCommand = commandHandler.CurrentCommand as UnitAttackCommand;
            var currentTarget = currentCommand?.Target;

            bool needsNewTarget = currentTarget == null || !currentTarget.IsAlive || !Targets.Contains(currentTarget);

            if (needsNewTarget)
            {
                // First: pick target with 0 assignments, closest first
                var newTarget = currentAssignments
                    .Where(kvp => kvp.Key != null && kvp.Key.IsAlive && kvp.Value == 0)
                    .OrderBy(kvp => Vector2.Distance(unit.transform.position, kvp.Key.transform.position))
                    .Select(kvp => kvp.Key)
                    .FirstOrDefault();

                // Fallback: pick closest alive target
                newTarget ??= currentAssignments
                    .Where(kvp => kvp.Key != null && kvp.Key.IsAlive)
                    .OrderBy(kvp => Vector2.Distance(unit.transform.position, kvp.Key.transform.position))
                    .Select(kvp => kvp.Key)
                    .FirstOrDefault();

                if (newTarget != null)
                {
                    currentAssignments[newTarget]++;
                    commandHandler.OverrideCommand(new UnitAttackCommand(unit, newTarget));
                }
            }
            else
            {
                currentAssignments[currentTarget]++;
            }
        }
    }

    private void FilterTargetGroup()
    {
        if (Targets.Count <= 1)
            return;

        var center = Targets.Aggregate(Vector2.zero, (sum, t) => sum + (Vector2)t.transform.position) / Targets.Count;
        Targets = Targets
            .Where(t => Vector2.Distance(center, t.transform.position) <= groupRadius)
            .ToList();
    }

    public override void CancelMission()
    {
        foreach (var unit in AssignedUnits)
        {
            if (unit != null && unit.IsAlive)
            {
                if (unit.TryGetComponent(out UnitCommandHandler commandHandler))
                {
                    commandHandler.OverrideCommand(new UnitMoveCommand(unit, unit.transform.position));
                    unit.IsLockedByMission = false;
                }
            }
        }

        base.CancelMission();
    }

    public override UnitMissionType GetMissionConverstionType()
    {
        float totalAttackPower = AssignedUnits.Sum(RTS_AI.GetUnitAttackPower);
        Vector3 avgPosition = AssignedUnits
            .Where(unit => unit != null && unit.IsAlive)
            .Aggregate(Vector3.zero, (current, unit) => current + unit.transform.position) / AssignedUnits.Count;

        var nearbyEnemies = GameManager.Instance.GetUnitsInRadius(
            GameManager.Instance.PlayerFaction,
            avgPosition,
            10f
        );

        float enemyPower = nearbyEnemies.Sum(RTS_AI.GetUnitAttackPower);
        float powerRatio = totalAttackPower / Mathf.Max(enemyPower, 0.01f);
        bool moralBroken = AssignedUnits.TrueForAll(u => u.MoralBroken);

        if (powerRatio < 0.6f || moralBroken)
            return UnitMissionType.Retreat;

        return UnitMissionType.None;
    }

    public override void DrawGizmos()
    {
        base.DrawGizmos();

        Vector3 avgPosition = AssignedUnits
            .Where(unit => unit != null && unit.IsAlive)
            .Aggregate(Vector3.zero, (current, unit) => current + unit.transform.position) / AssignedUnits.Count;

        Gizmos.color = Color.red;
        foreach (var unit in AssignedUnits)
        {
            if (unit)
                Gizmos.DrawLine(unit.transform.position, avgPosition);
        }

        Gizmos.color = Color.yellow;
        foreach (var target in Targets)
        {
            if (target != null)
                Gizmos.DrawLine(avgPosition, target.transform.position);
        }
    }

    private bool AllUnitsAlive() => AssignedUnits.All(unit => unit != null && unit.IsAlive);
    private bool AllUnitsMoralBroken() => AssignedUnits.All(unit => unit.MoralBroken);
}

