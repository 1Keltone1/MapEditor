using UnityEngine;
using UnityEditor;

public class LevelVisualizer : MonoBehaviour
{
    [Header("Level Configuration")]
    public LevelData levelData;

    [Header("Visual Settings")]
    public bool showGizmos = true;
    public bool showTileLabels = false;
    public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        if (levelData == null || levelData.tiles == null) return;

        // Рисуем сетку
        DrawGridGizmo();

        // Рисуем все тайлы
        foreach (var tile in levelData.tiles)
        {
            DrawTileGizmo(tile);
        }
    }

    private void DrawGridGizmo()
    {
        if (levelData == null) return;

        // ИСПРАВЛЕНИЕ: Используем CalculateGridSize() вместо gridSize
        Vector2Int gridSize = levelData.CalculateGridSize();

        Gizmos.color = gridColor;

        // Рисуем вертикальные линии
        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector3 start = new Vector3(x, 0, 0);
            Vector3 end = new Vector3(x, gridSize.y, 0);
            Gizmos.DrawLine(start, end);
        }

        // Рисуем горизонтальные линии
        for (int y = 0; y <= gridSize.y; y++)
        {
            Vector3 start = new Vector3(0, y, 0);
            Vector3 end = new Vector3(gridSize.x, y, 0);
            Gizmos.DrawLine(start, end);
        }
    }

    private void DrawTileGizmo(TileData tile)
    {
        Vector3 position = new Vector3(tile.position.x + 0.5f, tile.position.y + 0.5f, 0);

        // Разные цвета для разных типов тайлов
        Color tileColor = GetTileColor(tile.tileId);
        Color borderColor = new Color(0, 0, 0, 0.5f);

        // Рисуем заполненный квадрат
        Gizmos.color = tileColor;
        Gizmos.DrawCube(position, Vector3.one * 0.9f);

        // Рисуем границу
        Gizmos.color = borderColor;
        Gizmos.DrawWireCube(position, Vector3.one);
    }

    private Color GetTileColor(string tileId)
    {
        switch (tileId?.ToLower())
        {
            case "ground":
                return new Color(0.6f, 0.4f, 0.2f); // Коричневый
            case "wall":
                return new Color(0.4f, 0.4f, 0.4f); // Серый
            case "coin":
                return Color.yellow;
            default:
                return new Color(1, 1, 1, 0.3f); // Прозрачный белый
        }
    }

    // Метод для получения всех тайлов в позиции (может пригодиться позже)
    public TileData GetTileAtPosition(Vector2Int position)
    {
        if (levelData == null || levelData.tiles == null) return null;
        return levelData.tiles.Find(t => t.position == position);
    }

    // Метод для проверки, есть ли тайл в позиции
    public bool HasTileAtPosition(Vector2Int position)
    {
        return GetTileAtPosition(position) != null;
    }
}