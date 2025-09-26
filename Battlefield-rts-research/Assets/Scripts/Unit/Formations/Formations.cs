using System.Collections.Generic;
using UnityEngine;

public enum FormationType
{
    Line,
    Square,
    Circle,
    Triangle
}

public class FormationManager : MonoBehaviour
{
    public static Vector2[] GetFormationPositions(Vector2 targetPosition, int unitCount, FormationType formationType, float spacing = 1.0f, float rotation = 0.0f)
    {
        switch (formationType)
        {
            case FormationType.Line:
                return LineFormation.GetFormation(targetPosition, unitCount, spacing, rotation);
            case FormationType.Square:
                return SquareFormation.GetFormation(targetPosition, unitCount, spacing, rotation);
            case FormationType.Circle:
                return CircleFormation.GetFormation(targetPosition, unitCount, spacing, rotation);
            case FormationType.Triangle:
                return TriangleFormation.GetFormation(targetPosition, unitCount, spacing, rotation);
            default:
                return new Vector2[0];
        }
    }
}


public class LineFormation
{
    public static Vector2[] GetFormation(Vector2 targetPosition, int unitCount, float spacing = 1.0f, float rotation = 0.0f)
    {
        Vector2[] positions = new Vector2[unitCount];
        float half = (unitCount - 1) / 2f;
        float rad = Mathf.Deg2Rad * rotation;
        Vector2 right = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        for (int i = 0; i < unitCount; i++)
        {
            float offset = (i - half) * spacing;
            positions[i] = targetPosition + right * offset;
        }

        return positions;
    }
}

public class SquareFormation
{
    public static Vector2[] GetFormation(Vector2 targetPosition, int unitCount, float spacing = 1.0f, float rotation = 0.0f)
    {
        Vector2[] positions = new Vector2[unitCount];
        int size = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
        float offset = (size - 1) / 2f;
        float rad = Mathf.Deg2Rad * rotation;

        for (int i = 0; i < unitCount; i++)
        {
            int x = i % size;
            int y = i / size;
            Vector2 pos = new Vector2((x - offset) * spacing, (y - offset) * spacing);
            float rotatedX = pos.x * Mathf.Cos(rad) - pos.y * Mathf.Sin(rad);
            float rotatedY = pos.x * Mathf.Sin(rad) + pos.y * Mathf.Cos(rad);
            positions[i] = targetPosition + new Vector2(rotatedX, rotatedY);
        }

        return positions;
    }
}

public class CircleFormation
{
    public static  Vector2[] GetFormation(Vector2 targetPosition, int unitCount, float spacing = 1.0f, float rotation = 0.0f)
    {
        Vector2[] positions = new Vector2[unitCount];
        float radius = spacing * unitCount / (2 * Mathf.PI);
        float angleStep = 360f / unitCount;

        for (int i = 0; i < unitCount; i++)
        {
            float angle = rotation + i * angleStep;
            float rad = Mathf.Deg2Rad * angle;
            positions[i] = targetPosition + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        }

        return positions;
    }
}

public class TriangleFormation
{
    public static Vector2[] GetFormation(Vector2 targetPosition, int unitCount, float spacing = 1.0f, float rotation = 0.0f)
    {
        List<Vector2> positions = new List<Vector2>();
        int row = 1;

        while (positions.Count + row <= unitCount)
        {
            positions.AddRange(CreateRow(row, spacing));
            row++;
        }

        int remaining = unitCount - positions.Count;
        if (remaining > 0)
        {
            positions.AddRange(CreateRow(remaining, spacing));
        }

        // Apply rotation and translation
        float rad = Mathf.Deg2Rad * rotation;
        for (int i = 0; i < positions.Count; i++)
        {
            Vector2 p = positions[i];
            float x = p.x * Mathf.Cos(rad) - p.y * Mathf.Sin(rad);
            float y = p.x * Mathf.Sin(rad) + p.y * Mathf.Cos(rad);
            positions[i] = targetPosition + new Vector2(x, y);
        }

        return positions.ToArray();
    }

    private static List<Vector2> CreateRow(int count, float spacing)
    {
        List<Vector2> row = new List<Vector2>();
        float offsetX = (count - 1) * spacing / 2f;
        float y = -spacing * (count - 1);

        for (int i = 0; i < count; i++)
        {
            float x = i * spacing - offsetX;
            row.Add(new Vector2(x, y));
        }

        return row;
    }
}