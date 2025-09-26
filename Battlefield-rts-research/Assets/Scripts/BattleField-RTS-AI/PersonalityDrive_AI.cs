using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoredMissionOption
{
    public AI_MissionSetting Setting;
    public PreviewMissionResult Result;

}

public struct PreviewMissionResult
{
    public float Score;
    public object ResultData;
}

public class PersonalityDrive_AI : RTS_AI
{
    private AI_Personality m_Personality;

    private UnitStats[] m_AllyUnits;
    private UnitStats[] m_EnemyUnits;

    private float m_AllyPower;
    private float m_EnemyPower;

    private float m_AverageEnemyProximity;

    private float m_PowerRatio;
    private float m_Tension;
    private float m_Urgency;
    private float m_TimeUntilNextEvaluate;

    private UnitStats[] m_AvaibleUnits;


    public PersonalityDrive_AI(Factions faction, AI_Personality personality) : base(faction)
    {
        m_Personality = personality;
        m_MissionCancelThreshold = m_Personality.MissionCancelThreshold;
    }

    public override void StartArmy()
    {

    }

    public override void DrawGizmos()
    {
        base.DrawGizmos();

        if(GameManager.Instance.PlayerCommander)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(GameManager.Instance.PlayerCommander.transform.position, m_Personality.CommanderCheckRadius);
        }
    }

    public override float EvaluateArmy()
    {
        UpdateMissions();

        m_EnemyUnits = GetAllEnemyUnits().Where(e => e.IsAlive).ToArray();
        m_AllyUnits = GetAllAllyUnits().Where(a => a.IsAlive).ToArray();

        m_EnemyPower = GetTotalAttackPower(m_EnemyUnits);
        m_AllyPower = GetTotalAttackPower(m_AllyUnits);
        m_AverageEnemyProximity = GetAverageProximity(m_AllyUnits, m_EnemyUnits);

        m_PowerRatio = m_EnemyPower / Mathf.Max(m_AllyPower, 0.01f);
        m_Tension = Mathf.Clamp01(m_PowerRatio) + (1f / Mathf.Max(m_AverageEnemyProximity, 0.1f));

        m_Urgency = Mathf.InverseLerp(1.5f, 0.5f, m_Tension); 
        m_TimeUntilNextEvaluate = Mathf.Lerp(2f, 0.2f, m_Urgency);


        m_AvaibleUnits = m_AllyUnits.Where(u => u != null && u.IsAlive && !u.MoralBroken && !u.IsLockedByMission).ToArray();
        if (m_AvaibleUnits.Length == 0) return 1f;

        var best = GetBestMission();

        if (best != null)
        {
            if(best.Result.Score < m_MissionCancelThreshold)
            {
                return m_TimeUntilNextEvaluate;
            }

            var mission = CreateMission(best.Setting.MissionType, best.Result);
            if (mission != null)
            {
                AssignMission(mission);
                float baseCooldown = 10f;
                float tensionFactor = Mathf.Clamp(m_Tension, 0.5f, 2f);
                MissionCooldownHandler.SetCooldown(best.Setting.MissionType, baseCooldown * tensionFactor);
            }
            Debug.Log($"[AI Evaluate] Chose mission: {best.Setting.MissionType}, Score: {best.Result.Score:F2}, Urgency: {m_Urgency:F2}, Evaluate in {m_TimeUntilNextEvaluate:F1}s");
        }

        return m_TimeUntilNextEvaluate;
    }

    protected override void OnMissionConversionRecommended(UnitMission mission, UnitMissionType recommedType)
    {
        var missionSetting = m_Personality.MissionSettings.FirstOrDefault(s => s.MissionType == recommedType);

        m_AvaibleUnits = mission.AssignedUnits.Where(n => n != null).ToArray();
        var previewResult = GetPreviewMissionScore(recommedType, m_AvaibleUnits.Length);
        if (previewResult.Score < m_MissionCancelThreshold)
        {
            return;
        }

        if(GetMissionTypeCount(GetMissionType(recommedType)) >= missionSetting.MaxConcurrent)
        {
            return;
        }

        if (m_AvaibleUnits.Length < missionSetting.TroopRange.x)
        {
            return;
        }

        var newMission = CreateMission(recommedType, previewResult);
        if (newMission != null)
        {
            Debug.Log($"[AI] Converting mission {mission.GetType().Name} to {recommedType} with score {previewResult.Score:F2}");
            mission.CancelMission();
            AssignMission(newMission);
        }
    }

    private ScoredMissionOption GetBestMission()
    {
        var possibleMissions = new List<ScoredMissionOption>();

        foreach (var setting in m_Personality.MissionSettings)
        {
            if (MissionCooldownHandler.IsOnCooldown(setting.MissionType))
                continue;

            if (GetMissionTypeCount(GetMissionType(setting.MissionType)) >= setting.MaxConcurrent)
                continue;

            if (m_AvaibleUnits.Length < setting.TroopRange.x)
                continue;

            int troopsToAssign = UnityEngine.Random.Range(setting.TroopRange.x, Mathf.Min(setting.TroopRange.y + 1, m_AvaibleUnits.Length));

            if(troopsToAssign <= 0 || troopsToAssign > m_AvaibleUnits.Length)
                continue;

            PreviewMissionResult result = GetPreviewMissionScore(setting.MissionType,troopsToAssign);

            possibleMissions.Add(new ScoredMissionOption
            {
                Setting = setting,
                Result = result
            });
        }

        return possibleMissions.OrderByDescending(m => m.Result.Score).FirstOrDefault();
    }

    private PreviewMissionResult GetPreviewMissionScore(UnitMissionType type, int troopsToAssign)
    {
        PreviewMissionResult result = new PreviewMissionResult();
        var squad = GetSquad(m_AvaibleUnits[0], m_AvaibleUnits, troopsToAssign);
        var allyMap = GameManager.Instance.StatsEnemyFaction;
        switch (type)
        {
            case UnitMissionType.Attack:
                EvaluateAttackMission(ref result,troopsToAssign,squad);
                break;
            case UnitMissionType.Defend:
                var mostImportantAlly = FindMostImportantAlly(m_AvaibleUnits);
                if (mostImportantAlly == null)
                    return result;
                result.Score = DefendMission.PreviewEvaluateMission(this, squad, mostImportantAlly);
                result.ResultData = new DefendMissionResult
                {
                    Squad = squad,
                    TargetDefend = mostImportantAlly
                };
                break;
            case UnitMissionType.Retreat:
                var safeZone = allyMap.GetMaxAttackPowerCell().Position;
                List<UnitStats> unitsNeedToRetreat = new List<UnitStats>();
                result.Score = RetreatMission.PreviewEvaluateMission(this, squad, safeZone,unitsNeedToRetreat);
                result.ResultData = new RetreatMissionResult
                {
                    Squad = unitsNeedToRetreat.ToArray(),
                    SafeZone = safeZone
                };
                break;
            case UnitMissionType.Move:
                var strongestCell = allyMap.GetMaxAttackPowerCell();
                var neighbouringCells = allyMap.GetNeigbouringCell(strongestCell);
                if (neighbouringCells.Length == 0)
                    return result;

                var weakestNeighbour = neighbouringCells.OrderBy(c => c.TotalAttackPower)
                    .FirstOrDefault();
                result.Score = MoveMission.PreviewEvaluateMission(this, squad, weakestNeighbour.Position);
                result.ResultData = new MoveMissonResult
                {
                    Squad = squad,
                    TargetPosition = weakestNeighbour.Position
                };
                break;
        }
        return result;
    }

    private void EvaluateAttackMission(ref PreviewMissionResult result,int troopsToAssign, UnitStats[] squad)
    {
        List<UnitStats> nearEnemies = new List<UnitStats>(troopsToAssign);

        var enemyMap = GameManager.Instance.StatsPlayerFaction;
        float totalEnemyPowerAtCommander = enemyMap.GetTotalAttackPowerInRadius(GameManager.Instance.PlayerCommander.transform.position, m_Personality.CommanderCheckRadius);

        float squadPower = GetTotalAttackPower(squad);

        Debug.Log($"[AI] Evaluating Attack Mission: Squad Power = {squadPower:F2}, Total Enemy Power at Commander = {totalEnemyPowerAtCommander:F2}");

        if(squadPower + m_Personality.CommanderPowerDifferenceError >= totalEnemyPowerAtCommander)
        {
            Debug.Log($"[AI] Squad is strong enough to attack Commander, preparing to attack Commander. Power of squad with error [{squadPower + m_Personality.CommanderPowerDifferenceError}]");
            nearEnemies.AddRange(GameManager.Instance.GetUnitsInRadius(GameManager.Instance.PlayerFaction, GameManager.Instance.PlayerCommander.transform.position, m_Personality.CommanderCheckRadius));

            result.Score = 1f;
            result.ResultData = new AttackMissionResult
            {
                Squad = squad,
                Targets = new List<UnitStats> { GameManager.Instance.PlayerCommander }
            };
            return;
        }
        
        Debug.Log($"[AI] Commander is too strong, looking for nearest enemy to attack.");


        var potentialTarget = FindNearestEnemy(m_AvaibleUnits[0]);
        
        nearEnemies.AddRange(GameManager.Instance.GetUnitsInRadius(GameManager.Instance.PlayerFaction, FindNearestEnemy(m_AvaibleUnits[0]).transform.position, 5));

        if (nearEnemies.Count == 0)
            return;

        result.Score = AttackMission.PreviewEvaluateMission(this, squad, nearEnemies);
        result.ResultData = new AttackMissionResult
        {
            Squad = squad,
            Targets = nearEnemies.ToList()
        };
    }

    private UnitMission CreateMission(UnitMissionType type, PreviewMissionResult precaculatedData)
    {
        switch (type)
        {
            case UnitMissionType.Attack:
                AttackMissionResult result = (AttackMissionResult)precaculatedData.ResultData;
                return new AttackMission(this, result.Squad, result.Targets);
            case UnitMissionType.Defend:
                DefendMissionResult defendResult = (DefendMissionResult)precaculatedData.ResultData;
                return new DefendMission(this, defendResult.Squad, defendResult.TargetDefend);
            case UnitMissionType.Retreat:
                RetreatMissionResult retreatResult = (RetreatMissionResult)precaculatedData.ResultData;
                return new RetreatMission(this, retreatResult.Squad, retreatResult.SafeZone);
            case UnitMissionType.Move:
                MoveMissonResult moveResult = (MoveMissonResult)precaculatedData.ResultData;
                return new MoveMission(this, moveResult.Squad, moveResult.TargetPosition);
            default:
                return null;
        }
    }

    private Type GetMissionType(UnitMissionType type)
    {
        switch (type)
        {
            case UnitMissionType.Attack:
                return typeof(AttackMission);
            case UnitMissionType.Defend:
                return typeof(DefendMission);
            case UnitMissionType.Retreat:
                return typeof(RetreatMission);
            case UnitMissionType.Move:
                return typeof(MoveMission);
            default:
                throw new ArgumentException($"Unknown mission type: {type}");
        }
    }

    protected float GetTotalAttackPower(UnitStats[] units)
    {
        return units.Sum(GetUnitAttackPower);
    }

    private UnitStats FindMostImportantAlly(UnitStats[] excluding)
    {
        var candidates = GetAllAllyUnits()
            .Where(u => u != null && u.IsAlive && !excluding.Contains(u))
            .OrderByDescending(u => u.IsCommander)
            .ThenBy(u => u.Morale)
            .ToList();

        return candidates.FirstOrDefault();
    }

    private UnitStats[] GetSquad(UnitStats sqaudAnchor, UnitStats[] units, int size)
    {
        List<UnitStats> squadList = new List<UnitStats> { sqaudAnchor };
        squadList.AddRange( units.OrderBy(u => Vector2.Distance(u.transform.position,sqaudAnchor.transform.position))
                    .Take(size - 1)
                    .ToArray());
        return squadList.ToArray();
    }

    private float GetAverageProximity(UnitStats[] allyUnits, UnitStats[] enemyUnits)
    {
        if (allyUnits.Length == 0 || enemyUnits.Length == 0)
            return float.MaxValue;

        float totalDistance = 0f;
        int count = 0;

        foreach (var ally in allyUnits)
        {
            float closest = float.MaxValue;

            foreach (var enemy in enemyUnits)
            {
                float dist = Vector2.Distance(ally.transform.position, enemy.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                }
            }

            totalDistance += closest;
            count++;
        }

        return totalDistance / count;
    }

    
}