using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UnitMission
{
    public List<UnitStats> AssignedUnits { get; private set; } = new List<UnitStats>();
    public bool IsComplete { get; protected set; }
    public bool HasFailed { get; protected set; }

    public bool Cancelled { get; protected set; }

    protected RTS_AI ai;

    public UnitMission(RTS_AI ai, IEnumerable<UnitStats> units)
    {
        this.ai = ai;
        AssignedUnits.AddRange(units);
        foreach (var unit in AssignedUnits)
        {
            unit.IsLockedByMission = true;
        }
    }

    public virtual void CancelMission()
    {
        Debug.Log($"Cancelling mission: {GetType().Name}");
        IsComplete = true;
        HasFailed = true;
        Cancelled = true;
    }

    public virtual UnitMissionType GetMissionConverstionType()
    {
        return UnitMissionType.None;
    }

    public abstract float EvaluateMission();

    public abstract void Execute();

    public virtual void UpdateMission()
    {
    }

    public virtual void OnRemove()
    {
        UnlockUnits();
    }

    public virtual void DrawGizmos()
    {

    }

    public void UnlockUnits()
    {
        Debug.Log($"Unlocking {AssignedUnits.Count} units from mission.");
        foreach (var unit in AssignedUnits)
        {
            unit.IsLockedByMission = false;
        }
        AssignedUnits.Clear();
    }

    

    public bool IsActive => !IsComplete && !HasFailed;
}
