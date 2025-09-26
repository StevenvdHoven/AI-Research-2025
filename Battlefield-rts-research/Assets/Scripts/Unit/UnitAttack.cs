using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UnitStats))]
public class UnitAttack : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private LayerMask m_TargetMask;


    [Header("Events")]
    public UnityEvent OnAttack;

    private UnitStats m_UnitStats;

    public bool IsAttacking { get { return m_IsAttacking; } }
    private bool m_IsAttacking = false;

    private float AttackTimer = 0f;

    public void StartAttacking()
    {
        m_IsAttacking = true;
    }

    public void StopAttacking()
    {
        m_IsAttacking = false;
    }

    private void Start()
    {
        m_UnitStats = GetComponent<UnitStats>();
        StartCoroutine(AttackingLoop());
    }

    private IEnumerator AttackingLoop()
    {
        while(gameObject.activeInHierarchy)
        {
            yield return new WaitUntil(() => m_IsAttacking);

            if(AttackTimer <= 0)
            {
                if(TryAttack())
                {
                    AttackTimer = m_UnitStats.AttackSpeed;
                }
            }
            else
            {
                AttackTimer -= Time.deltaTime;
            }
        }
    }

    private bool TryAttack()
    {
        if (m_UnitStats.Energy >= m_UnitStats.AttackCost)
        {
            if(m_UnitStats.UseEnergy(m_UnitStats.AttackCost))
            {
                OnAttack?.Invoke();

                Vector2 attackPos = transform.position + transform.right * transform.localScale.x * m_UnitStats.AttackRange;
                Collider2D[] colliders = Physics2D.OverlapCircleAll(attackPos, m_UnitStats.AttackRadius, m_TargetMask);
                if (colliders.Length == 0) return true;

                Collider2D targetHit = colliders.Where(n => n.GetComponent<UnitStats>().Faction != m_UnitStats.Faction).FirstOrDefault();
                if (targetHit == null) return true;

                if (targetHit != null && targetHit.TryGetComponent(out UnitStats targetUnit) &&
                   targetUnit.Faction != m_UnitStats.Faction)
                {
                    targetUnit.TakeDamage(m_UnitStats.AttackDamage);
                }
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if(m_UnitStats == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_UnitStats.AttackRadius);
        Gizmos.DrawLine(transform.position, transform.position + transform.right * transform.localScale.x * m_UnitStats.AttackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + transform.right * transform.localScale.x * m_UnitStats.AttackRange, m_UnitStats.AttackRadius);
    }
}
