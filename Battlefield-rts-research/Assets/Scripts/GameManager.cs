using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Vector2 MapSize { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private Factions m_PlayerFaction = Factions.Humans;
    [SerializeField] private Factions m_EnemyFaction = Factions.Orcs;

    [Header("Events")]
    public UnityEvent OnGameEnded;
    public UnityEvent OnPlayerWon;
    public UnityEvent OnEnemyWon;

    public Factions PlayerFaction
    {
        get { return m_PlayerFaction; }
    }
    public Factions EnemyFaction
    {
        get { return m_EnemyFaction; }
    }

     [Header("Grid Settings")]
    [SerializeField] private Vector2 m_CellSize;
    [SerializeField] private int m_Columns = 10;
    [SerializeField] private int m_Rows = 10;

    [Header("Debug Grid")]
    [SerializeField] private bool m_ShowDebugGrid = true;
    [SerializeField] private bool m_DrawStatsMap = false;
    [SerializeField] private StatsType m_StatsType = StatsType.Morale;
    [SerializeField] private Factions m_DebugGridFaction;

    public StatsGrid StatsPlayerFaction
    {
        get { return m_StatsPlayerFaction; }
    }
    public StatsGrid StatsEnemyFaction
    {
        get { return m_StatsEnemyFaction; }
    }

    public UnitStats PlayerCommander { get { return m_PlayersCommander; } }
    public UnitStats EnemyCommander { get { return m_EnemiesCommander; } }

    private UnitStats m_PlayersCommander;
    private UnitStats m_EnemiesCommander;

    private SeperationGrid m_SeperationGrid;
    private UnitStatsHandler m_StatsHandler;
    private StatsGrid m_StatsPlayerFaction;
    private StatsGrid m_StatsEnemyFaction;
    private List<UnitStats> m_AllUnits;

    private int m_NextUnitId = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
       
        m_SeperationGrid = new SeperationGrid(m_CellSize, m_Columns, m_Rows);
        m_StatsHandler = new UnitStatsHandler(m_CellSize,m_Columns,m_Rows);

        MapSize = new Vector2(m_CellSize.x * m_Columns, m_CellSize.y * m_Rows);
        m_AllUnits = new List<UnitStats>();

        m_StatsPlayerFaction = m_StatsHandler.GetMap(m_PlayerFaction);
        m_StatsEnemyFaction = m_StatsHandler.GetMap(m_EnemyFaction);
    }

    public UnitStats[] GetUnitsInRadius(Factions factions, Vector2 position, float radius)
    {
        return m_SeperationGrid.GetUnitsInRadius(factions, position, radius);
    }

    public List<UnitStats> GetAllUnits(Factions factions)
    {
        return m_AllUnits.Where(unit => unit.Faction == factions)
            .ToList();
    }

    public UnitStats GetUnit(int unitId)
    {
        if (unitId == -1) return null;

        return m_AllUnits.Where(n => n.UNIT_ID == unitId).First() ?? null;
    }


    public void RegisterUnit(UnitStats unitStats)
    {
        if (unitStats.UNIT_ID != -1)
        {
            Debug.LogWarning("Unit already registered with ID: " + unitStats.UNIT_ID);
            return;
        }

        if(unitStats.IsCommander)
        {
            SetCommander(unitStats);
        }

        unitStats.AssignId(m_NextUnitId++);

        m_AllUnits.Add(unitStats);
        m_SeperationGrid.AddUnitToCell(unitStats);

    }

    public void RemoveUnit(UnitStats unitStats)
    {
        if (unitStats.UNIT_ID == -1)
        {
            Debug.LogWarning("Unit not registered, cannot remove: " + unitStats.name);
            return;
        }

        unitStats.AssignId(-1);

        m_AllUnits.Remove(unitStats);
        m_SeperationGrid.RemoveUnitFromCell(unitStats);
    }

    private void Update()
    {
        m_SeperationGrid.UpdateSeperation();
        m_StatsHandler.UpdateMorales(ref m_AllUnits);
        m_StatsPlayerFaction = m_StatsHandler.GetMap(m_PlayerFaction);
        m_StatsEnemyFaction = m_StatsHandler.GetMap(m_EnemyFaction);
    }

    private void OnDrawGizmos()
    {
        if (m_SeperationGrid != null && m_ShowDebugGrid)
        {
            m_SeperationGrid.DrawGizmos();
        }

        if(m_StatsHandler != null && m_DrawStatsMap)
        {
            StatsGrid statsGrid = m_StatsHandler.GetMap(m_DebugGridFaction);
            switch (m_StatsType)
            {
                case StatsType.Morale:
                    
                    statsGrid.DrawMoraleMap(Color.green, Color.blue);
                    break;
                case StatsType.AttackPower:
                    statsGrid.DrawAttackPowerMap(Color.yellow, Color.red);
                    break;
                default:
                    break;
            }
            
        }
    }

    private void SetCommander(UnitStats stats)
    {
        if(stats.Faction == PlayerFaction)
        {
            m_PlayersCommander = stats;
        }
        else if (stats.Faction == EnemyFaction)
        {
            m_EnemiesCommander = stats;
        }

        stats.OnDeath.AddListener(CheckWin);
    }

    private void CheckWin()
    {
        if(!m_PlayersCommander.IsAlive || !m_EnemiesCommander.IsAlive)
        {
            OnGameEnded?.Invoke();

            Debug.Log("Game Over! " + (m_PlayersCommander.IsAlive ? "Enemies Win!" : "Players Win!"));
            // Here you can implement your game over logic, like showing a UI or restarting the game.

            if(m_PlayersCommander.IsAlive)
            {
                OnPlayerWon?.Invoke();

                var allEnemyUnits = GetAllUnits(EnemyFaction);
                foreach(var enemyUnit in allEnemyUnits)
                {
                    RemoveUnit(enemyUnit);
                    Destroy(enemyUnit.gameObject);
                }
            }
            else if (m_EnemiesCommander.IsAlive)
            {
                OnEnemyWon?.Invoke();

                var allPlayerUnits = GetAllUnits(PlayerFaction);
                foreach (var playerUnit in allPlayerUnits)
                {
                    RemoveUnit(playerUnit);
                    Destroy(playerUnit.gameObject);
                }
            }

        }
    }
}

public struct SeparationGridCell
{
    public Vector2 Position;
    public List<UnitStats> Units;

    public SeparationGridCell(Vector2 position)
    {
        Position = position;
        Units = new List<UnitStats>();
    }
}

public class SeperationGrid
{
    private Vector2 m_CellSize;
    private int m_Columns;
    private int m_Rows;


    private SeparationGridCell[,] m_Cells;

    public SeperationGrid(Vector2 cellSize, int collums, int rows)
    {
        m_CellSize = cellSize;
        m_Columns = collums;
        m_Rows = rows;

        CreateGrid();
    }

    public UnitStats[] GetUnitsInRadius(Factions faction, Vector2 position, float radius)
    {
        List<UnitStats> unitsInRadius = new List<UnitStats>();
        for (int col = 0; col < m_Columns; col++)
        {
            for (int row = 0; row < m_Rows; row++)
            {
                Rect cellRect = new Rect(
                    m_Cells[col, row].Position - m_CellSize * 0.5f,
                    m_CellSize);
                Rect radiusRect = new Rect(
                    position - new Vector2(radius, radius),
                    new Vector2(radius * 2, radius * 2));
                if (cellRect.Overlaps(radiusRect))
                {
                    var cell = m_Cells[col, row];
                    foreach (var unit in cell.Units)
                    {
                        if (unit.Faction == faction && Vector2.Distance(unit.transform.position, position) <= radius)
                        {
                            unitsInRadius.Add(unit);
                        }
                    }
                }
            }
        }
        return unitsInRadius.ToArray();

    }

    public void UpdateSeperation()
    {
        for (int col = 0; col < m_Columns; col++)
        {
            for (int row = 0; row < m_Rows; row++)
            {
                CellCheck(col, row);
            }
        }
    }

    public void AddUnitToCell(UnitStats unit)
    {
        for (int col = 0; col < m_Columns; col++)
        {
            for (int row = 0; row < m_Rows; row++)
            {
                Vector2 cellPosition = m_Cells[col, row].Position;
                if (IsInChell(cellPosition, unit.transform.position))
                {
                    m_Cells[col, row].Units.Add(unit);
                    return;
                }
            }
        }
    }

    public void RemoveUnitFromCell(UnitStats unit)
    {
        for (int col = 0; col < m_Columns; col++)
        {
            for (int row = 0; row < m_Rows; row++)
            {
                var cell = m_Cells[col, row];
                if (cell.Units.Contains(unit))
                {
                    cell.Units.Remove(unit);
                    m_Cells[col, row] = cell;
                    return;
                }
            }
        }
    }

    public void DrawGizmos()
    {
        if (m_Cells != null)
        {
            for (int col = 0; col < m_Columns; col++)
            {
                for (int row = 0; row < m_Rows; row++)
                {
                    bool hasTestUnit = m_Cells[col, row].Units.Any(unit => unit.IsTestUnit);

                    Vector2 cellPosition = m_Cells[col, row].Position;
                    Gizmos.color = hasTestUnit ? Color.red : Color.green;

                    if (!hasTestUnit)
                        Gizmos.DrawWireCube(cellPosition, m_CellSize);
                    else
                        Gizmos.DrawCube(cellPosition, m_CellSize);

                    var cell = m_Cells[col, row];
                    foreach (var unitInCell in cell.Units)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere(unitInCell.transform.position, 0.2f);
                    }
                }
            }
        }

    }

    private void CellCheck(int col, int row)
    {
        var cell = m_Cells[col, row];
        for (int i = 0; i < cell.Units.Count; i++)
        {
            if (!IsInChell(cell.Position, cell.Units[i].transform.position))
            {
                var neighbours = GetNeighbourIndexs(col, row);
                foreach (var neighbour in neighbours)
                {
                    var neighbourCell = m_Cells[neighbour.x, neighbour.y];
                    if (IsInChell(neighbourCell.Position, cell.Units[i].transform.position))
                    {
                        m_Cells[neighbour.x, neighbour.y].Units.Add(cell.Units[i]);

                        cell.Units.RemoveAt(i);
                        --i;
                        break;
                    }
                }
            }
        }
        m_Cells[col, row] = cell;
    }

    private List<Vector2Int> GetNeighbourIndexs(int col, int row)
    {
        var cells = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; // Skip the current cell
                int newCol = col + i;
                int newRow = row + j;

                if (newCol >= 0 && newCol < m_Columns && newRow >= 0 && newRow < m_Rows)
                {
                    cells.Add(new Vector2Int(newCol, newRow));
                }
            }
        }
        return cells;
    }




    private bool IsInChell(Vector2 cellPosition, Vector2 point)
    {
        Vector2 cellTopLeft = cellPosition - m_CellSize * .5f;
        Vector2 cellBottomRight = cellPosition + m_CellSize * .5f;
        if (point.x >= cellTopLeft.x && point.x <= cellBottomRight.x &&
            point.y >= cellTopLeft.y && point.y <= cellBottomRight.y)
        {
            return true;
        }
        return false;
    }

    private void CreateGrid()
    {
        Vector2 offset = new Vector2(m_CellSize.x * m_Columns * .5f, m_CellSize.y * m_Rows * .5f) - (m_CellSize * .5f);
        m_Cells = new SeparationGridCell[m_Columns, m_Rows];
        for (int col = 0; col < m_Columns; col++)
        {
            for (int row = 0; row < m_Rows; row++)
            {
                Vector2 cellPosition = new Vector2(col * m_CellSize.x, row * m_CellSize.y) - offset;
                m_Cells[col, row] = new SeparationGridCell(cellPosition);
            }
        }
    }
}
