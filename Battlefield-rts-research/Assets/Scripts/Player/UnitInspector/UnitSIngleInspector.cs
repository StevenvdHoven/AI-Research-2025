using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitSIngleInspector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image m_UnitLogo;
    [SerializeField] private Image m_HealthFillImage;
    [SerializeField] private Image m_EnergyFillImage;
    [SerializeField] private Image m_MoraleImage;
    [SerializeField] private TextMeshProUGUI m_AttackDamageText;
    [SerializeField] private TextMeshProUGUI m_AttackSpeedText;
    [SerializeField] private TextMeshProUGUI m_AttackRangeText;
    [SerializeField] private TextMeshProUGUI m_UnitSpeedText;
    [SerializeField] private TextMeshProUGUI m_UnitDodgeChanceText;

    private UnitStats m_SelectStats;

    public void InspectUnit(UnitStats selectedStats)
    {
        if (m_SelectStats != null) ReleaseEvents();

        m_SelectStats = selectedStats;

        m_UnitLogo.sprite = selectedStats.UnitLogo;
        m_UnitLogo.SetNativeSize();

        m_HealthFillImage.fillAmount = (float)selectedStats.Health / selectedStats.MaxHealth;
        m_EnergyFillImage.fillAmount = (float)selectedStats.Energy / selectedStats.MaxEnergy;
        m_MoraleImage.fillAmount = (float)selectedStats.Morale / selectedStats.MaxMorale;
        m_AttackDamageText.text = selectedStats.AttackDamage.ToString();
        m_AttackSpeedText.text = selectedStats.AttackSpeed.ToString("F2");
        m_AttackRangeText.text = selectedStats.AttackRange.ToString("F2");
        m_UnitSpeedText.text = selectedStats.MaxMovementSpeed.ToString("F2");
        m_UnitDodgeChanceText.text = (selectedStats.DodgeChance * 100).ToString("F2") + "%";

        BindEvents();
    }

    private void BindEvents()
    {
        m_SelectStats.OnDamageTaken.AddListener(OnHealthChanged);
        m_SelectStats.OnHealthRegened.AddListener(OnHealthChanged);
        m_SelectStats.OnEnergyUsed.AddListener(OnEnergyChanged);
        m_SelectStats.OnEnergyRegened.AddListener(OnEnergyChanged);
        m_SelectStats.OnMoraleChanged.AddListener(OnMoraleChanged);
        m_SelectStats.OnDeath.AddListener(ReleaseEvents);
    }

    private void ReleaseEvents()
    {
        m_SelectStats.OnDamageTaken.RemoveListener(OnHealthChanged);
        m_SelectStats.OnHealthRegened.RemoveListener(OnHealthChanged);
        m_SelectStats.OnEnergyUsed.RemoveListener(OnEnergyChanged);
        m_SelectStats.OnEnergyRegened.RemoveListener(OnEnergyChanged);
        m_SelectStats.OnMoraleChanged.RemoveListener(OnMoraleChanged);
        m_SelectStats.OnDeath.RemoveListener(ReleaseEvents);
    }

    private void OnEnergyChanged(int currentEnergy)
    {
        m_EnergyFillImage.fillAmount = (float)currentEnergy / m_SelectStats.MaxEnergy;
    }
    private void OnHealthChanged(int currentHealth)
    {
        m_HealthFillImage.fillAmount = (float)currentHealth / m_SelectStats.MaxHealth;
    }

    private void OnMoraleChanged(float currentMoral)
    {
        m_MoraleImage.fillAmount = currentMoral / m_SelectStats.MaxMorale;
    }
}
