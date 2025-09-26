using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public interface IDamageable
{
    public void TakeDamage(int amount);
}

public class UnitStats : MonoBehaviour, IDamageable
{
    [Header("Commander")]
    [SerializeField] bool m_IsCommander = false;

    [Header("SharedStats")]
    [SerializeField] private SharedUnitData m_SharedUnitData;

    [Header("References")]
    [SerializeField] private GameObject m_SelectionObject;

    [Header("Events")]
    public UnityEvent<int> OnDamageTaken;
    public UnityEvent<int> OnHealthRegened;
    public UnityEvent<int> OnEnergyUsed;
    public UnityEvent<int> OnEnergyRegened;
    public UnityEvent<float> OnMoraleChanged;
    public UnityEvent OnMoraleBroken;
    public UnityEvent OnMoraleFixed;
    public UnityEvent OnDeath;

    [Header("Debug")]
    public bool IsTestUnit = false;

    public bool IsCommander { get { return m_IsCommander; } }
    public int UNIT_ID { get { return m_UnitId; } }
    public string Name => m_SharedUnitData.Name;
    public Factions Faction => m_SharedUnitData.Faction;
    public Sprite UnitLogo => m_SharedUnitData.UnitLogo;
    public int MaxHealth => m_SharedUnitData.MaxHealth;
    public int MaxEnergy => m_SharedUnitData.MaxEnergy;
    public float HealthRegenRate => m_SharedUnitData.HealthRegenRate;
    public float HealthRegenDelay => m_SharedUnitData.HealthRegenDelay;
    public float EnergyRegenRate => m_SharedUnitData.EnergyRegenRate;
    public float EnergyRegenDelay => m_SharedUnitData.EnergyRegenDelay;
    public float MaxMovementSpeed => m_SharedUnitData.MaxMovementSpeed;
    public float Acceleration => m_SharedUnitData.Acceleration;
    public float Decceleration => m_SharedUnitData.Decceleration;
    public int AttackDamage => m_SharedUnitData.AttackDamage;
    public float AttackSpeed => m_SharedUnitData.AttackSpeed;
    public float AttackRadius => m_SharedUnitData.AttackRadius;
    public float AttackRange => m_SharedUnitData.AttackRange;
    public int AttackCost => m_SharedUnitData.AttackCost;
    public float DodgeChance => m_SharedUnitData.DodgeChance;
    public float MaxMorale => m_SharedUnitData.MaxMorale;
    public float MoraleRange => m_SharedUnitData.MoraleRange;
    public float MoraleRegenRate => m_SharedUnitData.MoraleRegenRate;
    public float MoraleDecayRate => m_SharedUnitData.MoraleDecayRate;

    public bool IsLockedByMission { get; set; } = false;
    public bool MoralBroken { get { return m_MoralBroken; } }

    public bool IsAlive { get { return m_Health > 0; } }

    public int Health { get { return m_Health; } }
    public float Energy { get { return m_Energy; } }
    public float Morale 
    { 
        get 
        { 
            return m_Morale; 
        } 
        set 
        { 
            
            m_Morale = value; 
            OnMoraleChanged?.Invoke(m_Morale);

            if(m_Morale <= 0 && !m_MoralBroken)
            {
                m_MoralBroken = true;
                OnMoraleBroken?.Invoke();
            }
            else if (m_Morale >= MaxMorale * 0.5f && m_MoralBroken)
            {
                OnMoraleFixed?.Invoke();
                m_MoralBroken = false;
            }
        } 
    }

    private int m_UnitId = -1;

    private int m_Health;
    private int m_Energy;
    private float m_Morale;
    private bool m_MoralBroken = false;

    private Coroutine m_HealthRegenRoutine;
    private Coroutine m_EnergyRegenRoutine;

    public void TakeDamage(int amount)
    {
        float dodgeRoll = Random.Range(0f, 1f);
        if (dodgeRoll < DodgeChance)
        {
            return;
        }

        m_Health -= amount;
        OnDamageTaken?.Invoke(m_Health);
        if (m_HealthRegenRoutine != null)
        {
            StopCoroutine(m_HealthRegenRoutine);
        }
        m_HealthRegenRoutine = StartCoroutine(HealthRegenerationLoop());

        if (m_Health <= 0)
        {
            OnDeath?.Invoke();
            GameManager.Instance.RemoveUnit(this);
            Destroy(gameObject);
        }
    }

    public bool UseEnergy(int amount)
    {
        if (m_Energy >= amount)
        {
            m_Energy -= amount;
            OnEnergyUsed?.Invoke(m_Energy);
            if (m_EnergyRegenRoutine != null)
            {
                StopCoroutine(m_EnergyRegenRoutine);
            }
            m_EnergyRegenRoutine = StartCoroutine(EnergyRegenerationLoop());
            return true;
        }
        return false;
    }

    public void ChangeSelection(bool selected)
    {
        m_SelectionObject.SetActive(selected);
    }

    public void AssignId(int unitId)
    {
        if (m_UnitId == -1) m_UnitId = unitId;
    }

    private void Start()
    {
        m_Health = MaxHealth;
        m_Energy = MaxEnergy;
        m_Morale = MaxMorale;

        GameManager.Instance.RegisterUnit(this);
        ChangeSelection(false);
    }

    private IEnumerator HealthRegenerationLoop()
    {
        yield return new WaitForSeconds(HealthRegenDelay);

        while (Health < MaxHealth)
        {
            yield return new WaitForSeconds(HealthRegenRate);
            m_Health += 1;
            OnHealthRegened?.Invoke(m_Health);
        }
        m_Health = MaxHealth;
        m_HealthRegenRoutine = null;
    }

    private IEnumerator EnergyRegenerationLoop()
    {
        yield return new WaitForSeconds(EnergyRegenDelay);

        while (m_Energy < MaxEnergy)
        {
            yield return new WaitForSeconds(EnergyRegenRate);
            m_Energy += 1;
            OnEnergyRegened?.Invoke(m_Energy);
        }
        m_Energy = MaxEnergy;
        m_EnergyRegenRoutine = null;
    }
}
