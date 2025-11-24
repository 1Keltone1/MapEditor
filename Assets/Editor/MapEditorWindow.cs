using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class MapEditorWindow : EditorWindow
{
    private LevelData currentLevelData;
    private string selectedTileId;
    private Vector2 scrollPosition;
    private bool isPaintingMode = true;
    private bool isEraseMode = false;

    private Stack<EditorAction> undoStack = new Stack<EditorAction>();
    private Stack<EditorAction> redoStack = new Stack<EditorAction>();
    private const int MAX_UNDO_HISTORY = 50;

    [MenuItem("Tools/2D Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditorWindow>("2D Map Editor");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        HandleHotkeys();
        DrawLevelHeader();
        DrawTilePalette();
        DrawTools();
        DrawCurrentLevelInfo();
        DrawUndoRedoInfo();
    }

    private void HandleHotkeys()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            // Переключение режимов P/E
            if (e.keyCode == KeyCode.P)
            {
                isPaintingMode = true;
                isEraseMode = false;
                e.Use();
                Repaint();
            }
            else if (e.keyCode == KeyCode.E)
            {
                isPaintingMode = false;
                isEraseMode = true;
                selectedTileId = null;
                e.Use();
                Repaint();
            }
            // Отмена действия Ctrl+Z
            else if (e.keyCode == KeyCode.Z && e.control)
            {
                UndoLastAction();
                e.Use();
            }
            // Повтор действия Ctrl+Y
            else if (e.keyCode == KeyCode.Y && e.control)
            {
                RedoLastAction();
                e.Use();
            }
        }
    }

    private void DrawUndoRedoInfo()
    {
        GUILayout.Space(10);
        GUILayout.Label("History", EditorStyles.boldLabel);
        GUILayout.Label($"Undo: {undoStack.Count} actions");
        GUILayout.Label($"Redo: {redoStack.Count} actions");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Undo") && undoStack.Count > 0)
        {
            UndoLastAction();
        }
        if (GUILayout.Button("Redo") && redoStack.Count > 0)
        {
            RedoLastAction();
        }
        GUILayout.EndHorizontal();
    }

    private void DrawLevelHeader()
    {
        GUILayout.BeginHorizontal();
        currentLevelData = (LevelData)EditorGUILayout.ObjectField("Level Data", currentLevelData, typeof(LevelData), false);

        if (GUILayout.Button("New", GUILayout.Width(60)))
        {
            CreateNewLevel();
        }

        if (GUILayout.Button("Save", GUILayout.Width(60)) && currentLevelData != null)
        {
            SaveLevel();
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }

    private void DrawTilePalette()
    {
        GUILayout.Label("Tile Palette", EditorStyles.boldLabel);

        if (GUILayout.Button("Ground Tile"))
        {
            selectedTileId = "ground";
            isPaintingMode = true;
            isEraseMode = false;
        }

        if (GUILayout.Button("Wall Tile"))
        {
            selectedTileId = "wall";
            isPaintingMode = true;
            isEraseMode = false;
        }

        if (GUILayout.Button("Coin"))
        {
            selectedTileId = "coin";
            isPaintingMode = true;
            isEraseMode = false;
        }

        GUILayout.Label($"Selected: {selectedTileId ?? "None"}");
        GUILayout.Space(10);
    }

    private void DrawTools()
    {
        GUILayout.Label("Tools", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Paint Tool (P)"))
        {
            isPaintingMode = true;
            isEraseMode = false;
        }

        if (GUILayout.Button("Erase Tool (E)"))
        {
            isPaintingMode = false;
            isEraseMode = true;
            selectedTileId = null;
        }
        GUILayout.EndHorizontal();

        GUILayout.Label($"Mode: {(isPaintingMode ? "PAINTING" : isEraseMode ? "ERASING" : "SELECT")}");
        GUILayout.Space(10);
    }

    private void DrawCurrentLevelInfo()
    {
        if (currentLevelData != null)
        {
            GUILayout.Label("Current Level Info", EditorStyles.boldLabel);
            GUILayout.Label($"Name: {currentLevelData.levelName}");
            GUILayout.Label($"Tiles: {currentLevelData.tiles.Count}");

            Vector2Int gridSize = currentLevelData.CalculateGridSize();
            GUILayout.Label($"Grid: {gridSize.x}x{gridSize.y} (auto)");
        }
    }

    private void CreateNewLevel()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }

        LevelData newLevel = ScriptableObject.CreateInstance<LevelData>();
        AssetDatabase.CreateAsset(newLevel, "Assets/Data/NewLevel.asset");
        AssetDatabase.SaveAssets();
        currentLevelData = newLevel;

        Debug.Log("Создан новый уровень: " + AssetDatabase.GetAssetPath(newLevel));
    }

    private void SaveLevel()
    {
        EditorUtility.SetDirty(currentLevelData);
        AssetDatabase.SaveAssets();
        Debug.Log("Уровень сохранен: " + currentLevelData.name);
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (currentLevelData == null) return;

        HandleSceneInput();
        DrawGrid();
    }

    private void HandleSceneInput()
    {
        Event e = Event.current;

        Vector2 mousePosition = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
        Vector2Int gridPosition = WorldToGridPosition(mousePosition);

        DrawGridPreview(gridPosition);

        Vector2Int gridSize = currentLevelData.CalculateGridSize();

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (isPaintingMode && !string.IsNullOrEmpty(selectedTileId))
            {
                PlaceTileWithUndo(gridPosition, gridSize);
                e.Use();
            }
            else if (isEraseMode)
            {
                RemoveTileWithUndo(gridPosition);
                e.Use();
            }
        }

        if (e.type == EventType.MouseDrag && e.button == 0)
        {
            if (isPaintingMode && !string.IsNullOrEmpty(selectedTileId))
            {
                PlaceTileWithUndo(gridPosition, gridSize);
                e.Use();
            }
            else if (isEraseMode)
            {
                RemoveTileWithUndo(gridPosition);
                e.Use();
            }
        }
    }

    private void PlaceTileWithUndo(Vector2Int position, Vector2Int gridSize)
    {
        if (position.x < 0 || position.x >= gridSize.x ||
            position.y < 0 || position.y >= gridSize.y)
        {
            return;
        }

        TileData existingTile = currentLevelData.tiles.Find(t => t.position == position);
        EditorAction action;

        if (existingTile != null)
        {
            // Сохраняем состояние до изменения
            action = new EditorAction
            {
                actionType = ActionType.ModifyTile,
                position = position,
                previousTileId = existingTile.tileId,
                newTileId = selectedTileId
            };
            existingTile.tileId = selectedTileId;
        }
        else
        {
            // Создаем новый тайл
            var newTile = new TileData
            {
                position = position,
                tileId = selectedTileId,
                layer = 0
            };
            currentLevelData.tiles.Add(newTile);

            action = new EditorAction
            {
                actionType = ActionType.AddTile,
                position = position,
                previousTileId = null,
                newTileId = selectedTileId
            };
        }

        AddActionToUndoStack(action);
        EditorUtility.SetDirty(currentLevelData);
        Repaint();
    }

    private void RemoveTileWithUndo(Vector2Int position)
    {
        TileData tileToRemove = currentLevelData.tiles.Find(t => t.position == position);
        if (tileToRemove != null)
        {
            // Сохраняем информацию об удаляемом тайле
            EditorAction action = new EditorAction
            {
                actionType = ActionType.RemoveTile,
                position = position,
                previousTileId = tileToRemove.tileId,
                newTileId = null
            };

            currentLevelData.tiles.Remove(tileToRemove);
            AddActionToUndoStack(action);

            EditorUtility.SetDirty(currentLevelData);
            Repaint();
        }
    }

    private void AddActionToUndoStack(EditorAction action)
    {
        undoStack.Push(action);

        // Ограничиваем размер истории
        if (undoStack.Count > MAX_UNDO_HISTORY)
        {
            // Удаляем самые старые действия
            var tempStack = new Stack<EditorAction>();
            while (undoStack.Count > MAX_UNDO_HISTORY - 1)
            {
                tempStack.Push(undoStack.Pop());
            }
            undoStack.Clear();
            while (tempStack.Count > 0)
            {
                undoStack.Push(tempStack.Pop());
            }
        }

        // Очищаем стек повтора при новом действии
        redoStack.Clear();
    }

    private void UndoLastAction()
    {
        if (undoStack.Count == 0)
        {
            Debug.Log("Nothing to undo");
            return;
        }

        EditorAction action = undoStack.Pop();
        redoStack.Push(action);

        ApplyUndoAction(action);
        EditorUtility.SetDirty(currentLevelData);
        Repaint();
    }

    private void RedoLastAction()
    {
        if (redoStack.Count == 0)
        {
            Debug.Log("Nothing to redo");
            return;
        }

        EditorAction action = redoStack.Pop();
        undoStack.Push(action);

        ApplyRedoAction(action);
        EditorUtility.SetDirty(currentLevelData);
        Repaint();
    }

    private void ApplyUndoAction(EditorAction action)
    {
        switch (action.actionType)
        {
            case ActionType.AddTile:
                // Отмена добавления - удаляем тайл
                currentLevelData.tiles.RemoveAll(t => t.position == action.position);
                break;

            case ActionType.RemoveTile:
                // Отмена удаления - восстанавливаем тайл
                currentLevelData.tiles.Add(new TileData
                {
                    position = action.position,
                    tileId = action.previousTileId,
                    layer = 0
                });
                break;

            case ActionType.ModifyTile:
                // Отмена изменения - восстанавливаем предыдущее состояние
                TileData tile = currentLevelData.tiles.Find(t => t.position == action.position);
                if (tile != null)
                {
                    tile.tileId = action.previousTileId;
                }
                break;
        }
    }

    private void ApplyRedoAction(EditorAction action)
    {
        switch (action.actionType)
        {
            case ActionType.AddTile:
                // Повтор добавления - добавляем тайл
                currentLevelData.tiles.Add(new TileData
                {
                    position = action.position,
                    tileId = action.newTileId,
                    layer = 0
                });
                break;

            case ActionType.RemoveTile:
                // Повтор удаления - удаляем тайл
                currentLevelData.tiles.RemoveAll(t => t.position == action.position);
                break;

            case ActionType.ModifyTile:
                // Повтор изменения - применяем новое состояние
                TileData tile = currentLevelData.tiles.Find(t => t.position == action.position);
                if (tile != null)
                {
                    tile.tileId = action.newTileId;
                }
                break;
        }
    }

    private Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x),
            Mathf.RoundToInt(worldPos.y)
        );
    }

    private void DrawGrid()
    {
        if (currentLevelData == null) return;

        Vector2Int gridSize = currentLevelData.CalculateGridSize();
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector3 start = new Vector3(x, 0, 0);
            Vector3 end = new Vector3(x, gridSize.y, 0);
            Handles.DrawLine(start, end);
        }

        for (int y = 0; y <= gridSize.y; y++)
        {
            Vector3 start = new Vector3(0, y, 0);
            Vector3 end = new Vector3(gridSize.x, y, 0);
            Handles.DrawLine(start, end);
        }
    }

    private void DrawGridPreview(Vector2Int gridPos)
    {
        if (isPaintingMode && !string.IsNullOrEmpty(selectedTileId))
        {
            Handles.color = Color.green;
        }
        else if (isEraseMode)
        {
            Handles.color = Color.red;
        }
        else
        {
            Handles.color = Color.blue;
        }

        Handles.DrawWireCube(new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0), Vector3.one);
    }

    private void PlaceTile(Vector2Int position, Vector2Int gridSize)
    {
        if (position.x < 0 || position.x >= gridSize.x ||
            position.y < 0 || position.y >= gridSize.y)
        {
            return;
        }

        TileData existingTile = currentLevelData.tiles.Find(t => t.position == position);

        if (existingTile != null)
        {
            existingTile.tileId = selectedTileId;
        }
        else
        {
            currentLevelData.tiles.Add(new TileData
            {
                position = position,
                tileId = selectedTileId,
                layer = 0
            });
        }

        EditorUtility.SetDirty(currentLevelData);
        Repaint();
    }

    private void RemoveTile(Vector2Int position)
    {
        int removedCount = currentLevelData.tiles.RemoveAll(t => t.position == position);
        if (removedCount > 0)
        {
            EditorUtility.SetDirty(currentLevelData);
            Repaint();
        }
    }
}

public enum ActionType
{
    AddTile,
    RemoveTile,
    ModifyTile
}

[System.Serializable]
public class EditorAction
{
    public ActionType actionType;
    public Vector2Int position;
    public string previousTileId;
    public string newTileId;
}