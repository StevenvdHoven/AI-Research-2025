using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerUnitCommand : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Factions m_SelectedFaction;
    [SerializeField] private LayerMask m_UnitMask;

    [Header("References")]
    [SerializeField] private SelectionTool m_SelectionTool;
    [SerializeField] private GameObject m_ArrowIndicator;

    private bool m_RotatingDirection;
    private Vector2 m_FirstPosition;
    private Vector2 m_Direction;

    private void Start()
    {
        m_ArrowIndicator.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_SelectionTool.HasSelection || EventSystem.current.IsPointerOverGameObject()) return;

        if(Input.GetMouseButtonDown(1))
        {
            m_RotatingDirection = true;
            m_FirstPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            m_ArrowIndicator.SetActive(true);
            m_ArrowIndicator.transform.position = m_FirstPosition;
        }
        else if(Input.GetMouseButtonUp(1))
        {
            CheckMouseHit();
            m_RotatingDirection = false;
            m_ArrowIndicator.SetActive(false);
        }

        if (m_RotatingDirection)
        {
            UpdateArrowIndicator();
        }
    }

    private void UpdateArrowIndicator()
    {
        m_ArrowIndicator.transform.position = m_FirstPosition;
        Vector2 dir = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - m_FirstPosition;
        dir.Normalize();
        m_ArrowIndicator.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90);
        m_Direction = dir;
    }

    private void CheckMouseHit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Collider2D collider2D = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition), m_UnitMask);
        if (collider2D != null)
        {
            if(collider2D.TryGetComponent<UnitStats>(out UnitStats unitStats) &&
               unitStats.Faction != m_SelectedFaction)
            {
                CreateAttackCommand(unitStats);
            }
            else
            {
                CreateMoveCommand(m_FirstPosition,m_Direction);
            }
        }
        else
        {
            CreateMoveCommand(m_FirstPosition, m_Direction);
        }
    }

    private void CreateMoveCommand(Vector2 position, Vector2 dir)
    {
        var selectedUnits = m_SelectionTool.SelectedUnits;
        if (selectedUnits.Count == 0) return;

        Vector2 offset = (-new Vector2(-dir.y, dir.x) * 1.25f) * selectedUnits.Count * 0.5f;
        foreach (var item in selectedUnits)
        {
            if (item == null) continue;

            if(item.TryGetComponent(out UnitCommandHandler commandHandler))
            {
                commandHandler.OverrideCommand(new UnitMoveCommand(item,position + offset));
                offset += new Vector2(-dir.y,dir.x) * 1.25f;
            }
        }
    }

    private void CreateAttackCommand(UnitStats unitStats)
    {
        var selectedUnits = m_SelectionTool.SelectedUnits;
        if (selectedUnits.Count == 0) return;

        foreach (var item in selectedUnits)
        {
            if (item == null) continue;

            if (item.TryGetComponent(out UnitCommandHandler commandHandler))
            {
                commandHandler.OverrideCommand(new UnitAttackCommand(item, unitStats));
            }
        }
    }
}
