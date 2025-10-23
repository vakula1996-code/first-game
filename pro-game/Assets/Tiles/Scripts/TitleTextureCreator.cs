using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileTextureCreator : MonoBehaviour
{
    [Header("Створити тайли автоматично")]
    [SerializeField] private bool createTiles = false;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        if (createTiles)
        {
            createTiles = false;
            CreateColoredTiles();
        }
    }
    
    private void CreateColoredTiles()
    {
        // Створюємо папку для тайлів якщо її немає
        if (!AssetDatabase.IsValidFolder("Assets/Tiles"))
        {
            AssetDatabase.CreateFolder("Assets", "Tiles");
        }
        
        // Створюємо кольорові тайли (Ретро/Пікселі стиль)
        CreateTile("WaterTile", new Color(0f, 0f, 1f)); // Яскраво-синій #0000FF
        CreateTile("SandTile", new Color(1f, 1f, 0f)); // Жовтий #FFFF00
        CreateTile("GrassTile", new Color(0f, 1f, 0f)); // Зелений #00FF00
        CreateTile("ForestTile", new Color(0f, 0.5f, 0f)); // Темно-зелений #008000
        CreateTile("MountainTile", new Color(0.5f, 0.5f, 0.5f)); // Сірий #808080
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("✓ Всі тайли успішно створені в папці Assets/Tiles!");
        Debug.Log("Тепер можеш підключити їх до MapGenerator в Inspector.");
    }
    
    private void CreateTile(string name, Color color)
    {
        // Створюємо текстуру
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        // Заповнюємо кольором
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point; // Піксельна графіка
        
        // Зберігаємо як PNG
        byte[] bytes = texture.EncodeToPNG();
        string texturePath = $"Assets/Tiles/{name}.png";
        System.IO.File.WriteAllBytes(texturePath, bytes);
        
        // Імпортуємо як спрайт
        AssetDatabase.ImportAsset(texturePath);
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
        
        // Створюємо Tile Asset
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        if (sprite != null)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.color = Color.white;
            
            string tilePath = $"Assets/Tiles/{name}.asset";
            AssetDatabase.CreateAsset(tile, tilePath);
            
            Debug.Log($"✓ Створено: {name}");
        }
    }
#endif
}