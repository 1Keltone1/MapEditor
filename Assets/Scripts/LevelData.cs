using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewLevel", menuName = "2D Map Editor/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName = "New Level";
    public List<TileData> tiles = new List<TileData>();

    // ”бираем фиксированный gridSize и вычисл€ем автоматически
    public Vector2Int CalculateGridSize()
    {
        if (tiles == null || tiles.Count == 0)
            return new Vector2Int(20, 10); // –азмер по умолчанию дл€ пустого уровн€

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var tile in tiles)
        {
            minX = Mathf.Min(minX, tile.position.x);
            maxX = Mathf.Max(maxX, tile.position.x);
            minY = Mathf.Min(minY, tile.position.y);
            maxY = Mathf.Max(maxY, tile.position.y);
        }

        // ƒобавл€ем отступы по кра€м
        int width = maxX - minX + 5;
        int height = maxY - minY + 5;

        return new Vector2Int(Mathf.Max(20, width), Mathf.Max(10, height));
    }
}

[Serializable]
public class TileData
{
    public Vector2Int position;
    public string tileId;
    public int layer;
}