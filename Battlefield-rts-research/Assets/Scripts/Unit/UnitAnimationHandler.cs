using UnityEngine;

public class UnitAnimationHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string MovementParameterName = "IsMoving";

    [Header("References")]
    [SerializeField] private Animator m_Animator;
    [SerializeField] private UnitMovement m_Movement;

    // Update is called once per frame
    void Update()
    {
        if(m_Animator && m_Movement)
        {
            m_Animator.SetBool(MovementParameterName, m_Movement.Velocity.magnitude > 0.1f);
        }
    }
}
