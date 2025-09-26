using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct RetreatMissionResult
{
    public UnitStats[] Squad;
    public Vector2 SafeZone;
}

public class RetreatMission : UnitMission
{
    private Vector2 m_SafeZone;

    public RetreatMission(RTS_AI ai, IEnumerable<UnitStats> units, Vector2 safeZone) : base(ai, units)
    {
        m_SafeZone = safeZone;
    }

    public static float PreviewEvaluateMission(RTS_AI ai, UnitStats[] units, Vector2 safeZone, List<UnitStats> unitsNeedToRetreat)
    {
        if (units == null || units.Length == 0)
            return 0f;

        var enemyMap = GameManager.Instance.StatsPlayerFaction;
        var allyMap = GameManager.Instance.StatsEnemyFaction;

        float threatRadius = 5f;
        float totalDistance = 0f;
        int aliveUnits = 0;

        unitsNeedToRetreat.Clear();

        foreach (var unit in units)
        {
            if (unit == null || !unit.IsAlive)
                continue;

            aliveUnits++;
            totalDistance += Vector2.Distance(unit.transform.position, safeZone);

            float enemyPower = enemyMap.GetTotalAttackPowerInRadius(unit.transform.position, threatRadius);
            float allyPower = allyMap.GetTotalAttackPowerInRadius(unit.transform.position, threatRadius);

            if (enemyPower > allyPower * 1.25f)
            {
                unitsNeedToRetreat.Add(unit); // Mark unit as needing retreat
            }
        }

        if (unitsNeedToRetreat.Count == 0 || aliveUnits == 0)
            return 0f;

        float avgDistance = totalDistance / aliveUnits;
        float distanceScore = Mathf.InverseLerp(20f, 0f, avgDistance); // closer = higher score
        float aliveScore = aliveUnits / (float)units.Length;

        float finalScore = (distanceScore * 0.6f) + (aliveScore * 0.4f);
        return finalScore;
    }

    public override float EvaluateMission()
    {
        return PreviewEvaluateMission(ai, AssignedUnits.ToArray(), m_SafeZone,new List<UnitStats>());
    }

    public override void UpdateMission()
    {
        base.UpdateMission();

        AssignedUnits.RemoveAll(unit => unit == null || !unit.IsAlive || !unit.IsLockedByMission);

        foreach (var unit in AssignedUnits)
        {

            float distanceToSafeZone = Vector2.Distance(unit.transform.position, m_SafeZone);
            if (unit != null && unit.IsAlive && distanceToSafeZone < 20f)
            {
                unit.IsLockedByMission = false;
            }
        }
        

        if (AssignedUnits.Count == 0)
        {
            IsComplete = true;
        }
    }

    public override void Execute()
    {
        float angleRate = 360f / AssignedUnits.Count;
        foreach (var unit in AssignedUnits)
        {
            if(unit == null || !unit.IsAlive)

            if(unit.TryGetComponent(out UnitCommandHandler commandHandler))
            {
                Vector2 direction = new Vector2(Mathf.Cos(angleRate * Mathf.Deg2Rad), Mathf.Sin(angleRate * Mathf.Deg2Rad));
                Vector2 retreatPosition = m_SafeZone + direction * 2f;
                commandHandler.OverrideCommand(new UnitMoveCommand(unit, retreatPosition));
            }
        }
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

    public override void DrawGizmos()
    {
        base.DrawGizmos();

        foreach(var unit in AssignedUnits)
        {
            if (unit != null && unit.IsAlive)
            {
                float distance = Vector2.Distance(unit.transform.position, m_SafeZone);
                Gizmos.color = Color.Lerp(Color.red, Color.green, Mathf.InverseLerp(20f, 0f, distance));
                Gizmos.DrawLine(unit.transform.position, m_SafeZone);
            }
        }
    }
}
