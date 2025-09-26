using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCommandHandler : MonoBehaviour
{
    private IUnitCommand m_CurrentCommand;
    private Queue<IUnitCommand> m_PendingCommands;

    public IUnitCommand CurrentCommand
    {
        get { return m_CurrentCommand; }
    }

    private void Start()
    {
        m_PendingCommands = new Queue<IUnitCommand>();
        StartCoroutine(CommandCheck());
    }

    private void Update()
    {
        CommandLoop();
    }

    private IEnumerator CommandCheck()
    {
        while (gameObject.activeInHierarchy)
        {
            yield return new WaitUntil(() => m_PendingCommands.Count > 0 && m_CurrentCommand == null);

            if (m_CurrentCommand == null)
            {
                m_CurrentCommand = m_PendingCommands.Dequeue();
                m_CurrentCommand.StartCommand();
            }
            yield return null;
        }
    }

    private void CommandLoop()
    {
        if (m_CurrentCommand != null)
        {
            m_CurrentCommand.UpdateCommand();

            if (m_CurrentCommand.IsCommandComplete())
            {
                m_CurrentCommand.EndCommand();
                m_CurrentCommand = null;
            }
        }
    }

    public void AddCommand(IUnitCommand unitCommand)
    {
        m_PendingCommands.Enqueue(unitCommand);
    }

    public void OverrideCommand(IUnitCommand unitCommand)
    {
        if (m_CurrentCommand != null)
        {
            m_CurrentCommand.EndCommand();
        }
        m_CurrentCommand = unitCommand;
        m_CurrentCommand.StartCommand();
        m_PendingCommands.Clear();
    }
}

public interface IUnitCommand
{
    public void StartCommand();
    public void UpdateCommand();
    public void EndCommand();
    public bool IsCommandComplete();
}

public abstract class BaseUnitCommand : IUnitCommand
{
    protected UnitStats OwningUnit;

    public BaseUnitCommand(UnitStats unitStats)
    {
        OwningUnit = unitStats;
    }

    public abstract void StartCommand();
    public abstract void UpdateCommand();
    public abstract void EndCommand();
    public abstract bool IsCommandComplete();
}




