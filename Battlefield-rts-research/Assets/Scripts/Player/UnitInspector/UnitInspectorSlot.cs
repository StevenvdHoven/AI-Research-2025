using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnitInspectorSlot : PoolableObject
{
    [Header("References")]
    [SerializeField] private Image m_UnitLogo;

    public UnityEvent<UnitStats> OnClick;

    private UnitStats m_Stats;

    public void SelectUnitData()
    {
        OnClick?.Invoke(m_Stats);
    }

    public void AssignUnit(UnitStats stats)
    {
        m_Stats = stats;
        LoadData();
    }

    private void LoadData()
    {
        m_UnitLogo.sprite = m_Stats.UnitLogo;
        m_UnitLogo.SetNativeSize();
    }

    protected override void ResetObject()
    {
        base.ResetObject();
        OnClick.RemoveAllListeners();
    }
}
