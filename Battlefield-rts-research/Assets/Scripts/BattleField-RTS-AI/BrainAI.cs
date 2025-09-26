using System.Collections;
using UnityEngine;

public class BrainAI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private AI_Personality m_Personality;

    private RTS_AI m_CurrentAI;
    private Coroutine m_AILoopRoutine;

    public void SetAIType(AI_Personality personality)
    {
        StopAI();

        m_CurrentAI = new PersonalityDrive_AI(GameManager.Instance.EnemyFaction, personality);
        m_CurrentAI.StartArmy();
        m_AILoopRoutine = StartCoroutine(AI_Loop());
    }

    public void StopAI()
    {
        if (m_CurrentAI != null)
        {
            m_CurrentAI.OnExitLogic();
            m_CurrentAI = null;
            StopCoroutine(m_AILoopRoutine);
        }
    }

    private void Start()
    {
        SetAIType(m_Personality);

    }

    private IEnumerator AI_Loop()
    {
        yield return new WaitForSeconds(5);

        while(m_CurrentAI != null)
        {
            float waitTime = m_CurrentAI.EvaluateArmy();
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void OnDrawGizmos()
    {
        if (m_CurrentAI != null)
            m_CurrentAI.DrawGizmos();

    }

}
