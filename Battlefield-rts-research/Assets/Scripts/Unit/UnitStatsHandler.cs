using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum StatsType
{
    Morale,
    AttackPower
}

public class StatsGrid
{
    public Vector2 CellSize { get; private set; }
    public int Columns { get; private set; }
    public int Rows { get; private set; }
    public StatsCell[,] Cells { get; private set; }

    public float MaxMorale { get; private set; }
    public float MinMorale { get; private set; }

    public float MinAttackPower { get; private set; }
    public float MaxAttackPower { get; private set; }

    public StatsGrid(Vector2 cellSize, int columns, int rows)
    {
        CellSize = cellSize;
        Columns = columns;
        Rows = rows;

        Vector2 offset = new Vector2(CellSize.x * Columns * .5f, CellSize.y * Rows * .5f) - (CellSize * .5f);
        Cells = new StatsCell[Columns, Rows];
        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Vector2 cellPosition = new Vector2(col * CellSize.x, row * CellSize.y) - offset;
                Cells[col, row] = new StatsCell(cellPosition);
            }
        }

        MaxMorale = 0;
        MinMorale = int.MaxValue;

        MaxAttackPower = 0;
        MinAttackPower = float.MaxValue;
       
    }

    public void UpdateStats(UnitStats unit)
    {
        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Rect cellRect = new Rect(Cells[col, row].Position - CellSize * 0.5f, CellSize);
                if (cellRect.Contains(unit.transform.position))
                {
                    Cells[col, row].TotalMorale += unit.Morale;
                    Cells[col, row].TotalAttackPower += RTS_AI.GetUnitAttackPower(unit);

                    MaxMorale = Mathf.Max(MaxMorale, Cells[col, row].TotalMorale);
                    MinMorale = Mathf.Min(MinMorale, Cells[col, row].TotalMorale);

                    MaxAttackPower = Mathf.Max(MaxAttackPower, Cells[col, row].TotalAttackPower);
                    MinAttackPower = Mathf.Min(MinAttackPower, Cells[col, row].TotalAttackPower);
                    return;
                }
            }
        }
    }

    public float GetTotalMoraleInRadius(Vector2 position, float radius)
    {
        float total = 0;
        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Rect cellRect = new Rect(Cells[col, row].Position - CellSize * 0.5f, CellSize);
                Rect checkRect = new Rect(position - new Vector2(radius, radius), new Vector2(radius * 2, radius * 2));
                if (cellRect.Overlaps(checkRect))
                {
                    total += Cells[col, row].TotalMorale;
                }
            }
        }
        return total;
    }

    public float GetTotalAttackPowerInRadius(Vector2 position, float radius)
    {
        float total = 0;
        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Rect cellRect = new Rect(Cells[col, row].Position - CellSize * 0.5f, CellSize);
                Rect checkRect = new Rect(position - new Vector2(radius, radius), new Vector2(radius * 2, radius * 2));
                if (cellRect.Overlaps(checkRect))
                {
                    total += Cells[col, row].TotalAttackPower;
                }
            }
        }
        return total;
    }

    public StatsCell GetCellAtPosition(Vector2 postion)
    {
        for(int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Rect cellRect = new Rect(Cells[col, row].Position - CellSize * 0.5f, CellSize);
                if (cellRect.Contains(postion))
                {
                    return Cells[col, row];
                }
            }
        }
        return new StatsCell(Vector2.zero);
    }

    public StatsCell GetMinAttackPowerCell()
    {
        StatsCell minCell = Cells[0, 0];
        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                if (Cells[col, row].TotalAttackPower < minCell.TotalAttackPower)
                {
                    minCell = Cells[col, row];
                }
            }
        }
        return minCell;
    }

    public StatsCell GetMaxAttackPowerCell()
    {
        StatsCell maxCell = Cells[0, 0];
        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                if (Cells[col, row].TotalAttackPower > maxCell.TotalAttackPower)
                {
                    maxCell = Cells[col, row];
                }
            }
        }
        return maxCell;
    }

    public StatsCell[] GetNeigbouringCell(StatsCell cell)
    {
        var indexes = GetCellIndexes(cell);

        List<StatsCell> neighbours = new List<StatsCell>();
        for (int col = indexes.Item1 - 1; col <= indexes.Item1 + 1; col++)
        {
            for (int row = indexes.Item2 - 1; row <= indexes.Item2 + 1; row++)
            {
                if (col >= 0 && col < Columns && row >= 0 && row < Rows && !(col == indexes.Item1 && row == indexes.Item2))
                {
                    neighbours.Add(Cells[col, row]);
                }
            }
        }
        return neighbours.ToArray();
    }

    private (int, int) GetCellIndexes(StatsCell cell)
    {
        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                if (Cells[col, row].Position == cell.Position)
                {
                    return (col, row);
                }
            }
        }
        return (0, 0);
    }

        public void DrawMoraleMap(Color minMoraleColor, Color maxMoraleColor)
    {
        foreach (var cell in Cells)
        {
            Color color = Color.Lerp(minMoraleColor, maxMoraleColor, Mathf.InverseLerp(MinMorale, MaxMorale, cell.TotalMorale));

            if (cell.TotalMorale == 0) color = Color.gray;
            color.a = 0.5f;
            Gizmos.color = color;
            Gizmos.DrawCube(cell.Position, CellSize);
        }
    }

    public void DrawAttackPowerMap(Color minMoraleColor, Color maxMoraleColor)
    {
        foreach (var cell in Cells)
        {
            Color color = Color.Lerp(minMoraleColor, maxMoraleColor, Mathf.InverseLerp(MinMorale, MaxMorale, cell.TotalMorale));

            if (cell.TotalMorale == 0) color = Color.gray;
            color.a = 0.5f;
            Gizmos.color = color;
            Gizmos.DrawCube(cell.Position, CellSize);
        }
    }
}
public struct StatsCell
{
    public Vector2 Position;
    public float TotalMorale;
    public float TotalAttackPower;

    public StatsCell(Vector2 position)
    {
        Position = position;
        TotalMorale = 0;
        TotalAttackPower = 0;
    }
}

public class UnitStatsHandler
{
    // Morale Grid Settings
    private Vector2 m_CellSize;
    private int m_Columns;
    private int m_Rows;

    public UnitStatsHandler(Vector2 cellSize, int collums, int rows)
    {
        m_CellSize = cellSize;
        m_Columns = collums;
        m_Rows = rows;
    }

    public void UpdateMorales(ref List<UnitStats> allUnits)
    {
        foreach (var unit in allUnits)
        {
            CaculateMorale(unit);
        }

    }

    private void CaculateMorale(UnitStats unit)
    {
        var closeAllies = GameManager.Instance.GetUnitsInRadius(unit.Faction, unit.transform.position, unit.MoraleRange);

        List<UnitStats> closeEnemies = new List<UnitStats>();
        foreach (Factions faction in System.Enum.GetValues(typeof(Factions)))
        {
            if (faction != unit.Faction)
            {
                closeEnemies.AddRange(GameManager.Instance.GetUnitsInRadius(faction, unit.transform.position, unit.MoraleRange));
            }
        }

        int alliesNearby = closeAllies.Length;
        int enemiesNearby = closeEnemies.Count;
        float moraleChange = alliesNearby == enemiesNearby ? 0 : (alliesNearby > enemiesNearby ? unit.MoraleRegenRate * alliesNearby : -unit.MoraleDecayRate * enemiesNearby);
        
        unit.Morale = Mathf.Clamp(unit.Morale + moraleChange, 0f, unit.MaxMorale);

    }

    public StatsGrid GetMap(Factions faction)
    {
        var allUnitsOfFaction = GameManager.Instance.GetAllUnits(faction);
        StatsGrid moraleGrid = new StatsGrid(m_CellSize, m_Columns, m_Rows);

        foreach (var unit in allUnitsOfFaction)
        {
            moraleGrid.UpdateStats(unit);
        }
        return moraleGrid;
    }

}
