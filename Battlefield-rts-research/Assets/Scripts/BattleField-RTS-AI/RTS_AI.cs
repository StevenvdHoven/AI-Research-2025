using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Events;


public abstract class RTS_AI
{
    [Header("RTS AI Settings")]
    [SerializeField] protected Factions HostingFaction;
    protected float m_MissionCancelThreshold = 0.3f;

    private List<UnitMission> m_ActiveMissions = new List<UnitMission>();
    private Queue<UnitMission> m_MissionQueue = new Queue<UnitMission>();
    private bool m_IsUpdatingMissions = false;

    public RTS_AI(Factions faction)
    {
        HostingFaction = faction;
    }

    public virtual void DrawGizmos()
    {
        foreach (var mission in m_ActiveMissions)
        {
            if (mission.IsActive)
            {
                mission.DrawGizmos();
            }
        }
    }

    public abstract void StartArmy();
    public abstract float EvaluateArmy();

    protected abstract void OnMissionConversionRecommended(UnitMission mission, UnitMissionType recommedType);

    public virtual void OnExitLogic()
    {
        foreach (var mission in m_ActiveMissions)
        {
            mission.CancelMission();
            mission.UnlockUnits();
        }

        m_ActiveMissions.Clear();
    }

    protected int GetMissionTypeCount<T>() where T : UnitMission
    {
        return m_ActiveMissions.Count(m => m.GetType() == typeof(T));
    }

    protected int GetMissionTypeCount(Type missionType)
    {
        return m_ActiveMissions.Count(m => m.GetType() == missionType);
    }

    protected void AssignMission(UnitMission mission)
    {
        if(!m_IsUpdatingMissions)
        {
            mission.Execute();
            m_ActiveMissions.Add(mission);
        }
        else
        {
            m_MissionQueue.Enqueue(mission);
        }
    }

    protected void UpdateMissions()
    {
        while(m_MissionQueue.Count > 0)
        {
            var mission = m_MissionQueue.Dequeue();
            mission.Execute();
            m_ActiveMissions.Add(mission);
        }


        m_IsUpdatingMissions = true;
        foreach (var mission in m_ActiveMissions)
        {
            if(mission == null || !mission.IsActive || mission.Cancelled || mission.HasFailed || mission.IsComplete)
            {
                continue;
            }

            mission.UpdateMission();

            var recommendType = mission.GetMissionConverstionType();
            if(recommendType != UnitMissionType.None)
            {
                OnMissionConversionRecommended(mission, recommendType);
            }

            float score = mission.EvaluateMission();
            if (score < m_MissionCancelThreshold) // configurable threshold
            {
                Debug.Log($"[AI] Cancelling mission {mission.GetType().Name} due to low score: {score:F2}");
                mission.CancelMission();
            }

            if (!mission.IsActive)
            {
                mission.UnlockUnits();
            }

            
        }
        

        m_ActiveMissions.RemoveAll(m => !m.IsActive);
        m_IsUpdatingMissions = false;
    }

    protected UnitStats FindNearestEnemy(UnitStats self)
    {
        UnitStats closest = null;
        float minDist = float.MaxValue;

        foreach (var enemy in GetAllEnemyUnits())
        {
            if (enemy == null) continue;
            float dist = Vector2.Distance(enemy.transform.position, self.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }

    protected UnitStats[] GetAllAllyUnits()
    {
        return GameManager.Instance.GetAllUnits(HostingFaction).ToArray();
    }

    protected UnitStats[] GetAllEnemyUnits()
    {
        return GameManager.Instance.GetAllUnits(
            HostingFaction == Factions.Humans ? Factions.Orcs : Factions.Humans).ToArray();
    }

    public static float GetUnitAttackPower(UnitStats unitStats)
    {
        return unitStats.AttackDamage * unitStats.AttackSpeed * (1 - unitStats.DodgeChance);
    }

    public static float GetMoraleAverage(UnitStats[] units)
    {
        return (float)units.Average(u => u.Morale);
    }

    public static float GetAverageEnergy(UnitStats[] units)
    {
        return units.Average(u => u.Energy);
    }
}




