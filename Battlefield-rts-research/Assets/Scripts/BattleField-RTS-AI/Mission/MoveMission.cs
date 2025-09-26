using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct MoveMissonResult
{
    public Vector2 TargetPosition;
    public UnitStats[] Squad;
}


public class MoveMission : UnitMission
{
    private Vector2 m_TargetPosition;
    public MoveMission(RTS_AI ai, IEnumerable<UnitStats> units, Vector2 targetPosition) : base(ai, units)
    {
        m_TargetPosition = targetPosition;
    }

    public static float PreviewEvaluateMission(RTS_AI ai, UnitStats[] units, Vector2 targetPosition)
    {
        if(units == null || units.Length == 0)
            return 0f;

        var allyMap = GameManager.Instance.StatsEnemyFaction;

        float totalPowerInCell = allyMap.GetCellAtPosition(targetPosition).TotalAttackPower;
        float squadPower = units.Sum(RTS_AI.GetUnitAttackPower);
        float difference = squadPower - totalPowerInCell;

        if(difference <= 0f)
            return 0f;

        return 1f;
    }

    public override float EvaluateMission()
    {
        return PreviewEvaluateMission(ai, AssignedUnits.ToArray(), m_TargetPosition);
    }

    public override void Execute()
    {
        var formationPositions = FormationManager.GetFormationPositions(m_TargetPosition, AssignedUnits.Count, FormationType.Square, 1.0f);
        int index = 0;
        foreach (var unit in AssignedUnits)
        {
            if(unit.TryGetComponent(out UnitCommandHandler commandHandler))
            {
                commandHandler.OverrideCommand(new UnitMoveCommand(unit, formationPositions[index]));
                ++index;
            }
        }
    }

    public override void UpdateMission()
    {
        base.UpdateMission();

        AssignedUnits.RemoveAll(unit => unit == null || !unit.IsAlive);

        if (AssignedUnits.Count == 0)
        {
            IsComplete = true;
            return;
        }

        foreach (var unit in AssignedUnits)
        {
            float distance = Vector2.Distance(unit.transform.position, m_TargetPosition);
            if (distance < 5f || UnitIsIntrouble(unit,2f))
            {
                unit.IsLockedByMission = false;
            }
        }

        AssignedUnits.RemoveAll(unit => !unit.IsLockedByMission);
    }

    private bool UnitIsIntrouble(UnitStats stats,float visionRange)
    {
        var enemyUnits = GameManager.Instance.GetUnitsInRadius(GameManager.Instance.PlayerFaction, stats.transform.position, visionRange);
        if (enemyUnits == null || enemyUnits.Length == 0)
            return false;

        return true;

    }

    public override void DrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(m_TargetPosition, 5f);
        foreach (var unit in AssignedUnits)
        {
            if (unit != null && unit.IsAlive)
            {
                Gizmos.DrawLine(unit.transform.position, m_TargetPosition);
            }
        }
    }
} 

