using UnityEngine;

public class UnitInspector : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private GameObject m_UnitSlot;

    [Header("Referecnces")]
    [SerializeField] private SelectionTool m_SelectionTool;
    [SerializeField] private GameObject m_UnitSlotsPanel;
    [SerializeField] private GameObject m_SingleInspectorPanel;
    [SerializeField] private UnitSIngleInspector m_SingleInspector;
    [SerializeField] private Transform m_Grid;

    private ObjectPool m_UnitSlotPool;

    private void Start()
    {
        m_UnitSlotPool = new ObjectPool(m_UnitSlot, 10, transform);

        m_UnitSlotsPanel.SetActive(false);
        m_SingleInspectorPanel.SetActive(false);

        m_SelectionTool.OnSelectionMade.AddListener(LoadSelection);
        m_SelectionTool.OnSingleUnitSelected.AddListener((UnitStats unit) => 
            {
                ClearSelection();
                m_SingleInspectorPanel.SetActive(true);
                m_SingleInspector.InspectUnit(unit);
            }
        );
    }

    public void LoadSelection(UnitStats[] stats)
    {
        if (stats.Length == 0)
        {
            ClearSelection();
            return;
        }

        m_UnitSlotsPanel.SetActive(true);
        m_SingleInspectorPanel.SetActive(true);

        m_UnitSlotPool.PoolAll();
        m_SingleInspector.InspectUnit(stats[0]);
        foreach (UnitStats stat in stats)
        {
            PoolableObject slot = m_UnitSlotPool.GetObject();
            slot.SpawnObject(Vector3.zero);
            slot.transform.SetParent(m_Grid, false);
            UnitInspectorSlot inspectorSlot = slot.GetComponent<UnitInspectorSlot>();
            inspectorSlot.AssignUnit(stat);
            inspectorSlot.OnClick.AddListener(m_SingleInspector.InspectUnit);
        }
    }

    public void ClearSelection()
    {
        m_UnitSlotPool.PoolAll();
        m_UnitSlotsPanel.SetActive(false);
        m_SingleInspectorPanel.SetActive(false);
    }
}
