using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct DefendMissionResult
{
    public UnitStats[] Squad;
    public UnitStats TargetDefend;
}

public class DefendMission : UnitMission
{
    private UnitStats m_TargetDefend;
    private float m_AttackRange = 5f;
    private bool m_AttackingUnitClose;
    private float m_SafeTime = 0;

    public DefendMission(RTS_AI ai, IEnumerable<UnitStats> units, UnitStats targetDefend) : base(ai, units)
    {
        m_TargetDefend = targetDefend;
    }

    public static float PreviewEvaluateMission(RTS_AI ai, UnitStats[] units, UnitStats targetDefend)
    {
        if (units == null || units.Length == 0 || targetDefend == null || !targetDefend.IsAlive)
            return 0f;

        var nearbyEnemies = GameManager.Instance.GetUnitsInRadius(
            GameManager.Instance.PlayerFaction,
            targetDefend.transform.position,
            10f
        );

        if (targetDefend.IsCommander && targetDefend.MoralBroken)
            return 1f;

        if (nearbyEnemies.Length == 0)
            return 0f;

        float enemyPower = nearbyEnemies.Sum(RTS_AI.GetUnitAttackPower);

        float allyPower = units.Sum(RTS_AI.GetUnitAttackPower);

        float powerRatio = allyPower / Mathf.Max(enemyPower, 0.01f);

        float targetPriority = targetDefend.IsCommander ? 1.0f : 0.5f;

        float score = Mathf.Clamp01(powerRatio) * targetPriority;

        return score;
    }

    public override void DrawGizmos()
    {
        base.DrawGizmos();

        Vector2 avgPosition = AssignedUnits
            .Where(unit => unit != null && unit.IsAlive)
            .Aggregate(Vector2.zero, (current, unit) => current + (Vector2)unit.transform.position) / AssignedUnits.Count;

        Gizmos.color = Color.blue;
        foreach (var unit in AssignedUnits)
        {
            if (unit)
                Gizmos.DrawLine(unit.transform.position, avgPosition);
        }



        if (m_TargetDefend != null && m_TargetDefend.IsAlive)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(avgPosition, m_TargetDefend.transform.position);

            Gizmos.color = m_AttackingUnitClose ? Color.red : Color.green;
            Gizmos.DrawWireSphere(m_TargetDefend.transform.position, m_AttackRange);
        }
    }

    public override void Execute()
    {

        MoveUnitsToTarget();

    }

    public override float EvaluateMission()
    {
        return PreviewEvaluateMission(ai, AssignedUnits.ToArray(), m_TargetDefend);
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

    public override void UpdateMission()
    {
        base.UpdateMission();

        bool allUnitsAlive = AllUnitsAlive();
        bool allUnitsMoralBroken = AllUnitsMoralBroken();

        CheckForDefend();

        if(m_AttackingUnitClose)
        {
            m_SafeTime = Time.time;
        }

        float timeDelta = Time.time - m_SafeTime;
        if (timeDelta > 10 && m_TargetDefend)
        {
            Debug.Log($"Defend Mission completed: Safe time exceeded. Units alive: {allUnitsAlive}, Units moral broken: {allUnitsMoralBroken}");

            IsComplete = true;
        }
        else if (!m_TargetDefend || !allUnitsAlive || allUnitsMoralBroken)
        {
            Debug.Log($"Defend Mission failed: Target is dead [{m_TargetDefend == null}]. Units alive: {allUnitsAlive}, Units moral broken: {allUnitsMoralBroken}");
            Debug.Log($"Defend Mission failed: Units alive: {allUnitsMoralBroken}, Units moral broken: {allUnitsMoralBroken}");
            HasFailed = true;
        }
    }

    private void CheckForDefend()
    {
        if (!m_TargetDefend) return;

        var nearbyEnemyUnits = GameManager.Instance.GetUnitsInRadius(GameManager.Instance.PlayerFaction, m_TargetDefend.transform.position, m_AttackRange);
        if (nearbyEnemyUnits.Length <= 0)
        {
            m_AttackingUnitClose = false;
            MoveUnitsToTarget();
        }
        else if (!m_AttackingUnitClose)
        {
            m_AttackingUnitClose = true;
            m_SafeTime = Time.time;
            Debug.Log($"Defend Mission: Enemy units detected near {m_TargetDefend.name}. Preparing to attack.");
            AttackNearbyUnits(nearbyEnemyUnits);
        }
    }

    private void AttackNearbyUnits(UnitStats[] inComingUnits)
    {
        int amountOfUnits = inComingUnits.Length;
        amountOfUnits = Mathf.Min(amountOfUnits, AssignedUnits.Count);
        for (int i = 0; i < amountOfUnits; i++)
        {
            var unit = AssignedUnits[i];
            if (unit == null || !unit.IsAlive) continue;

            if (unit.TryGetComponent(out UnitCommandHandler commandhandler))
            {
                commandhandler.OverrideCommand(new UnitAttackCommand(unit, inComingUnits[i]));
            }
        }
    }

    private void MoveUnitsToTarget()
    {
        float angleRate = 360f / AssignedUnits.Count;
        foreach (var unit in AssignedUnits)
        {
            if (unit == null || !unit.IsAlive) continue;

            if (unit.TryGetComponent(out UnitCommandHandler commandhandler))
            {
                Vector2 directionOffset = new Vector2(Mathf.Cos(angleRate * Mathf.Deg2Rad), Mathf.Sin(angleRate * Mathf.Deg2Rad)) * 2f;

                commandhandler.OverrideCommand(new UnitMoveCommand(unit, m_TargetDefend.transform.position + (Vector3)directionOffset));
                angleRate += angleRate;
            }
        }
    }

    private bool AllUnitsAlive()
    {
        return AssignedUnits.TrueForAll(unit => unit != null && unit.IsAlive);
    }

    private bool AllUnitsMoralBroken()
    {
        return AssignedUnits.TrueForAll(unit => unit.MoralBroken);
    }


}
