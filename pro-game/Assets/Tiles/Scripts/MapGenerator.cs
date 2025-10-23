using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Налаштування карти")]
    [SerializeField] private int width = 50;
    [SerializeField] private int height = 50;
    [SerializeField] private float scale = 10f;
    [SerializeField] private int seed = 0;
    
    [Header("Пороги terrain")]
    [SerializeField] private float waterThreshold = 0.3f;
    [SerializeField] private float sandThreshold = 0.4f;
    [SerializeField] private float grassThreshold = 0.6f;
    [SerializeField] private float forestThreshold = 0.75f;
    
    [Header("Компоненти")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase waterTile;
    [SerializeField] private TileBase sandTile;
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase forestTile;
    [SerializeField] private TileBase mountainTile;
    
    [Header("Редагування")]
    [SerializeField] private bool editMode = false;
    [SerializeField] private TileType selectedTileType = TileType.Grass;
    [SerializeField] private int brushSize = 1;
    
    private float[,] noiseMap;
    private Camera mainCamera;
    
    public enum TileType
    {
        Water,
        Sand,
        Grass,
        Forest,
        Mountain
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        GenerateMap();
    }
    
    void Update()
    {
        // Перевірка натискання миші
        if (editMode && Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            EditTile();
        }
        
        // Перевірка клавіатури
        if (Keyboard.current == null) return;
        
        // Гарячі клавіші для зміни типу тайла
        if (Keyboard.current.digit1Key.wasPressedThisFrame) selectedTileType = TileType.Water;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) selectedTileType = TileType.Sand;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) selectedTileType = TileType.Grass;
        if (Keyboard.current.digit4Key.wasPressedThisFrame) selectedTileType = TileType.Forest;
        if (Keyboard.current.digit5Key.wasPressedThisFrame) selectedTileType = TileType.Mountain;
        
        // Зміна розміру пензля
        if (Keyboard.current.equalsKey.wasPressedThisFrame || Keyboard.current.numpadPlusKey.wasPressedThisFrame)
            brushSize = Mathf.Min(brushSize + 1, 5);
        if (Keyboard.current.minusKey.wasPressedThisFrame || Keyboard.current.numpadMinusKey.wasPressedThisFrame)
            brushSize = Mathf.Max(brushSize - 1, 1);
        
        // Регенерація карти
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            seed = Random.Range(0, 10000);
            GenerateMap();
        }
    }
    
    public void GenerateMap()
    {
        if (tilemap == null)
        {
            Debug.LogWarning("Tilemap ще не підключений, спробуй пізніше");
            return;
        }
        
        tilemap.ClearAllTiles();
        noiseMap = GenerateNoiseMap();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseValue = noiseMap[x, y];
                TileBase tile = GetTileByValue(noiseValue);
                
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                tilemap.SetTile(tilePos, tile);
            }
        }
        
        Debug.Log($"Карта згенерована! Seed: {seed}");
    }
    
    private float[,] GenerateNoiseMap()
    {
        float[,] map = new float[width, height];
        Random.InitState(seed);
        
        float offsetX = Random.Range(0f, 999f);
        float offsetY = Random.Range(0f, 999f);
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float sampleX = (x + offsetX) / scale;
                float sampleY = (y + offsetY) / scale;
                
                float noise = Mathf.PerlinNoise(sampleX, sampleY);
                map[x, y] = noise;
            }
        }
        
        return map;
    }
    
    private TileBase GetTileByValue(float value)
    {
        if (value < waterThreshold) return waterTile;
        if (value < sandThreshold) return sandTile;
        if (value < grassThreshold) return grassTile;
        if (value < forestThreshold) return forestTile;
        return mountainTile;
    }
    
    private TileBase GetTileByType(TileType type)
    {
        switch (type)
        {
            case TileType.Water: return waterTile;
            case TileType.Sand: return sandTile;
            case TileType.Grass: return grassTile;
            case TileType.Forest: return forestTile;
            case TileType.Mountain: return mountainTile;
            default: return grassTile;
        }
    }
    
    private void EditTile()
    {
        if (Mouse.current == null) return;
        
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
        Vector3Int tilePos = tilemap.WorldToCell(worldPos);
        
        TileBase selectedTile = GetTileByType(selectedTileType);
        
        // Малювання з урахуванням розміру пензля
        for (int x = -brushSize + 1; x < brushSize; x++)
        {
            for (int y = -brushSize + 1; y < brushSize; y++)
            {
                Vector3Int pos = new Vector3Int(tilePos.x + x, tilePos.y + y, 0);
                
                // Перевірка, чи тайл в межах карти
                if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                {
                    tilemap.SetTile(pos, selectedTile);
                }
            }
        }
    }
    
    // Метод для зміни seed через UI
    public void SetSeed(int newSeed)
    {
        seed = newSeed;
        GenerateMap();
    }
    
    // Метод для зміни масштабу
    public void SetScale(float newScale)
    {
        scale = newScale;
        GenerateMap();
    }
    
    // Метод для перемикання режиму редагування
    public void ToggleEditMode()
    {
        editMode = !editMode;
        Debug.Log($"Режим редагування: {(editMode ? "Увімкнено" : "Вимкнено")}");
    }
    
    void OnGUI()
    {
        if (editMode)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            style.padding = new RectOffset(10, 10, 10, 10);
            
            GUI.Label(new Rect(10, 10, 300, 30), $"Режим редагування: ТАК", style);
            GUI.Label(new Rect(10, 35, 300, 30), $"Обраний тайл: {selectedTileType}", style);
            GUI.Label(new Rect(10, 60, 300, 30), $"Розмір пензля: {brushSize}", style);
            GUI.Label(new Rect(10, 85, 400, 30), "1-5: вибір тайла | +/-: розмір | R: регенерація", style);
        }
    }
}