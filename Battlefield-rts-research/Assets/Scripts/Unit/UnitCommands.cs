using UnityEngine;

public class UnitMoveCommand : BaseUnitCommand
{
    private Vector2 m_TargetPosition;
    private UnitMovement m_UnitMovement;

    public UnitMoveCommand(UnitStats unitStats, Vector2 targetPosition) : base(unitStats)
    {
        m_TargetPosition = targetPosition;
        m_UnitMovement = unitStats.GetComponent<UnitMovement>();
    }

    public override void EndCommand()
    {
    }

    public override bool IsCommandComplete()
    {
        float distanceToTarget = Vector2.Distance(OwningUnit.transform.position, m_TargetPosition);
        return distanceToTarget < 2.0f;
    }

    public override void StartCommand()
    {
        m_UnitMovement.TargetPosition = m_TargetPosition;
    }

    public override void UpdateCommand()
    {
    }
}

public class UnitAttackCommand : BaseUnitCommand
{
    public UnitStats Target { get { return m_TargetUnit; } }

    private UnitStats m_TargetUnit;
    private UnitMovement m_UnitMovement;
    private UnitAttack m_UnitAttack;

    public UnitAttackCommand(UnitStats unitStats, UnitStats targetUnit) : base(unitStats)
    {
        m_TargetUnit = targetUnit;
        m_UnitMovement = unitStats.GetComponent<UnitMovement>();
        m_UnitAttack = unitStats.GetComponent<UnitAttack>();
    }
    public override void EndCommand()
    {
        m_UnitAttack.StopAttacking();
    }

    public override bool IsCommandComplete()
    {
        return m_TargetUnit == null || m_TargetUnit.Health <= 0;
    }

    public override void StartCommand()
    {
        m_UnitMovement.GetSteeringBehaviour<Arrive>().ArrivalRadius = OwningUnit.AttackRange;
    }

    public override void UpdateCommand()
    {
        if (m_TargetUnit == null) return;

        float squaredDistance = (m_TargetUnit.transform.position - OwningUnit.transform.position).sqrMagnitude;
        if (squaredDistance < OwningUnit.AttackRange * OwningUnit.AttackRange)
        {
            if(!m_UnitAttack.IsAttacking)
            {
                m_UnitAttack.StartAttacking();
            }
            return;
        }
        else if(m_UnitAttack.IsAttacking)
        {
            m_UnitAttack.StopAttacking();
        }

        m_UnitMovement.TargetPosition = m_TargetUnit.transform.position;
    }
}

