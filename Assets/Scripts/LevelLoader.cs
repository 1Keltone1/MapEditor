using UnityEngine;
using System.Collections.Generic;

public class LevelLoader : MonoBehaviour
{
    [Header("Level Configuration")]
    public LevelData levelData;

    [Header("Tile Prefabs")]
    public GameObject groundTilePrefab;
    public GameObject wallTilePrefab;
    public GameObject coinPrefab;

    [Header("Visualization")]
    public bool autoLoadOnStart = true;
    public bool clearPreviousTiles = true;

    private List<GameObject> spawnedTiles = new List<GameObject>();

    private void Start()
    {
        if (autoLoadOnStart && levelData != null)
        {
            LoadLevel();
        }
    }

    public void LoadLevel()
    {
        if (levelData == null)
        {
            Debug.LogError("LevelData is not assigned!");
            return;
        }

        if (clearPreviousTiles)
        {
            ClearLevel();
        }

        foreach (var tileData in levelData.tiles)
        {
            GameObject tilePrefab = GetTilePrefab(tileData.tileId);
            if (tilePrefab != null)
            {
                Vector3 position = new Vector3(tileData.position.x + 0.5f, tileData.position.y + 0.5f, 0);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, this.transform);
                spawnedTiles.Add(tile);
            }
        }

        Debug.Log($"Level loaded: {spawnedTiles.Count} tiles");
    }


    public void ClearLevel()
    {
        foreach (var tile in spawnedTiles)
        {
            if (tile != null)
            {
                DestroyImmediate(tile);
            }
        }
        spawnedTiles.Clear();
    }

    private GameObject GetTilePrefab(string tileId)
    {
        switch (tileId?.ToLower())
        {
            case "ground":
                return groundTilePrefab;
            case "wall":
                return wallTilePrefab;
            case "coin":
                return coinPrefab;
            default:
                Debug.LogWarning($"Unknown tile type: {tileId}");
                return null;
        }
    }

    private Color GetTileColor(string tileId)
    {
        switch (tileId?.ToLower())
        {
            case "ground": return new Color(0.6f, 0.4f, 0.2f); // Коричневый
            case "wall": return new Color(0.4f, 0.4f, 0.4f);   // Серый
            case "coin": return Color.yellow;
            default: return Color.white;
        }
    }

    // Метод для обновления уровня при изменении LevelData
    public void RefreshLevel()
    {
        ClearLevel();
        LoadLevel();
    }
}