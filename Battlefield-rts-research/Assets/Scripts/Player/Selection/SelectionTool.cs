using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionTool : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Factions m_ChosenFaction;
    [SerializeField] private float m_SelectionTimeThreshold = 0.5f;
    [SerializeField] private LayerMask m_UnitMask;

    [Header("References")]
    [SerializeField] private Image m_SelectionImage;

    [Header("Events")]
    public UnityEvent<UnitStats> OnSingleUnitSelected;
    public UnityEvent<UnitStats[]> OnSelectionMade;

    private bool m_IsSelecting = false;
    private float m_SelectionTime;

    private Vector2 m_FirstPosition;
    private Vector2 m_SecondPosition;

    public bool HasSelection
    {
        get { return m_SelectedUnits.Count > 0; }
    }

    public List<UnitStats> SelectedUnits { get { return m_SelectedUnits.ToList(); } }
    private List<UnitStats> m_SelectedUnits;

    public void ClearSelection()
    {
        for (int i = 0; i < m_SelectedUnits.Count; i++)
        {
            if(m_SelectedUnits[i] == null) continue;

            m_SelectedUnits[i].ChangeSelection(false);
        }
        m_SelectedUnits.Clear();
    }

    public void SelectFaction(Factions faction)
    {
        m_ChosenFaction = faction;
        Debug.Log("Selected Faction: " + m_ChosenFaction);
    }

    private void Start()
    {
        m_SelectedUnits = new List<UnitStats>();
        m_SelectionImage.enabled = false;

        // Ensure the selection image is set to a default size
        m_SelectionImage.rectTransform.sizeDelta = Vector2.zero;
        m_SelectionImage.rectTransform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            ClearSelection();

            m_SelectionTime = 0;
            m_IsSelecting = true;
            m_SelectionImage.enabled = true;
            MarkPosition(ref m_FirstPosition);
        }
        else if (Input.GetMouseButtonUp(0) && m_IsSelecting)
        {
            m_IsSelecting = false;

            if(m_SelectionTime > m_SelectionTimeThreshold)
            {
                MakeSelection();
            }
            else
            {
                SingleSelection();
            }
           

            m_SelectionImage.enabled = false;
        }

        if (m_IsSelecting) m_SelectionTime += Time.deltaTime;

        UpdateSelectionFrame();
    }

    private void UpdateSelectionFrame()
    {
        if (!m_IsSelecting) return;

        MarkPosition(ref m_SecondPosition);

        Vector2 screenFirst = Camera.main.WorldToScreenPoint(m_FirstPosition);
        Vector2 screenSecond = Camera.main.WorldToScreenPoint(m_SecondPosition);

        Vector2 selectionStart = Vector2.Min(screenFirst, screenSecond);
        Vector2 selectionEnd = Vector2.Max(screenFirst, screenSecond);

        Vector2 center = (selectionStart + selectionEnd) / 2;
        Vector2 size = selectionEnd - selectionStart;

        m_SelectionImage.rectTransform.position = center;
        m_SelectionImage.rectTransform.sizeDelta = size;

    }

    private void MakeSelection()
    {
        Vector2 selectionStart = Vector2.Min(m_FirstPosition, m_SecondPosition);
        Vector2 selectionEnd = Vector2.Max(m_FirstPosition, m_SecondPosition);

        GameManager.Instance.GetAllUnits(m_ChosenFaction).ForEach(unit =>
        {
            Vector2 unitPosition = unit.transform.position;

            if (unitPosition.x >= selectionStart.x && unitPosition.x <= selectionEnd.x &&
                unitPosition.y >= selectionStart.y && unitPosition.y <= selectionEnd.y)
            {
                m_SelectedUnits.Add(unit);
                unit.ChangeSelection(true);
            }
        });

        OnSelectionMade?.Invoke(m_SelectedUnits.ToArray());
    }

    private void SingleSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Collider2D collider2D = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition), m_UnitMask);
        if (collider2D != null)
        {
            if(collider2D.TryGetComponent(out UnitStats unit))
            {
                OnSingleUnitSelected?.Invoke(unit);

                if(unit.Faction == m_ChosenFaction)
                {
                    m_SelectedUnits.Add(unit);
                    unit.ChangeSelection(true);
                }
            }
        }
    }



    private void MarkPosition(ref Vector2 position)
    {
        position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    

}
