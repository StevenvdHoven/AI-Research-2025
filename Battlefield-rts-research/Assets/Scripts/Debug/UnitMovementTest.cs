using UnityEngine;
using UnityEngine.Events;

public class UnitMovementTest : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private UnitMovement m_UnitMovement;

    [Header("Test Events")]
    public UnityEvent OnAttack;


    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(1))
        {
            m_UnitMovement.TargetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            OnAttack?.Invoke();
        }
    }
}
