using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class TileMigratorSubset : EditorWindow
{
    public Tilemap source;
    public Tilemap destination;
    public GameObject paletteAsset;

    private TileBase[] paletteTiles = new TileBase[0];
    private HashSet<TileBase> selectedTiles = new HashSet<TileBase>();
    private Vector2 scrollPos;
    private GameObject lastPaletteAsset;

    // Cache preview đã load được, tránh gọi lại GetAssetPreview mỗi frame
    private Dictionary<TileBase, Texture2D> previewCache = new Dictionary<TileBase, Texture2D>();

    [MenuItem("Tools/Migrate Tiles Between Tilemaps (Subset)")]
    public static void ShowWindow() => GetWindow<TileMigratorSubset>("Tile Migrator (Subset)");

    private void OnGUI()
    {
        source = (Tilemap)EditorGUILayout.ObjectField("Source Tilemap", source, typeof(Tilemap), true);
        destination = (Tilemap)EditorGUILayout.ObjectField("Destination Tilemap", destination, typeof(Tilemap), true);
        paletteAsset = (GameObject)EditorGUILayout.ObjectField("Palette Asset (từ Assets)", paletteAsset, typeof(GameObject), false);

        if (paletteAsset != lastPaletteAsset)
        {
            RefreshPaletteTileList();
            lastPaletteAsset = paletteAsset;
        }

        EditorGUILayout.Space();

        if (paletteTiles.Length > 0)
        {
            EditorGUILayout.LabelField($"Chọn các tile muốn chuyển ({selectedTiles.Count}/{paletteTiles.Length}):", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Chọn tất cả")) selectedTiles = new HashSet<TileBase>(paletteTiles);
            if (GUILayout.Button("Bỏ chọn tất cả")) selectedTiles.Clear();
            EditorGUILayout.EndHorizontal();

            bool stillLoadingAnyPreview = false;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(250));
            foreach (var tile in paletteTiles)
            {
                if (tile == null) continue;

                EditorGUILayout.BeginHorizontal();
                bool isSelected = selectedTiles.Contains(tile);
                bool newValue = EditorGUILayout.ToggleLeft(GUIContent.none, isSelected, GUILayout.Width(20));

                // Chỉ gọi GetAssetPreview nếu chưa có trong cache
                if (!previewCache.TryGetValue(tile, out Texture2D preview) || preview == null)
                {
                    preview = AssetPreview.GetAssetPreview(tile);

                    if (preview != null)
                    {
                        previewCache[tile] = preview;
                    }
                    else if (AssetPreview.IsLoadingAssetPreview(tile.GetInstanceID()))
                    {
                        // Preview đang được Unity generate ở background, chưa xong
                        stillLoadingAnyPreview = true;
                    }
                }

                if (preview != null)
                    GUILayout.Label(preview, GUILayout.Width(32), GUILayout.Height(32));
                else
                    GUILayout.Label("...", GUILayout.Width(32), GUILayout.Height(32)); // placeholder tạm, không nhấp nháy

                GUILayout.Label(tile.name);
                EditorGUILayout.EndHorizontal();

                if (newValue && !isSelected) selectedTiles.Add(tile);
                else if (!newValue && isSelected) selectedTiles.Remove(tile);
            }
            EditorGUILayout.EndScrollView();

            // Chỉ yêu cầu vẽ lại khi còn preview đang load, tránh Repaint vô tội vạ
            if (stillLoadingAnyPreview)
            {
                Repaint();
            }
        }
        else if (paletteAsset != null)
        {
            EditorGUILayout.HelpBox("Không tìm thấy Tilemap hoặc tile nào bên trong Palette asset này.", MessageType.Warning);
        }

        EditorGUILayout.Space();

        GUI.enabled = source != null && destination != null && selectedTiles.Count > 0;
        if (GUILayout.Button($"Migrate {selectedTiles.Count} tile đã chọn"))
        {
            MigrateTiles();
        }
        GUI.enabled = true;
    }

    private void RefreshPaletteTileList()
    {
        selectedTiles.Clear();
        previewCache.Clear(); // đổi palette thì cache preview cũ không còn hợp lệ
        paletteTiles = new TileBase[0];

        if (paletteAsset == null) return;

        Tilemap paletteTilemap = paletteAsset.GetComponentInChildren<Tilemap>();
        if (paletteTilemap == null) return;

        int count = paletteTilemap.GetUsedTilesCount();
        TileBase[] tiles = new TileBase[count];
        paletteTilemap.GetUsedTilesNonAlloc(tiles);

        paletteTiles = tiles.Where(t => t != null).Distinct().OrderBy(t => t.name).ToArray();
    }

    private void MigrateTiles()
    {
        BoundsInt bounds = source.cellBounds;
        int movedCount = 0;

        Undo.RegisterCompleteObjectUndo(source, "Migrate Tiles Source");
        Undo.RegisterCompleteObjectUndo(destination, "Migrate Tiles Destination");

        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase tile = source.GetTile(pos);
            if (tile == null) continue;

            if (selectedTiles.Contains(tile))
            {
                destination.SetTile(pos, tile);
                source.SetTile(pos, null);
                movedCount++;
            }
        }

        EditorUtility.SetDirty(source);
        EditorUtility.SetDirty(destination);
        Debug.Log($"Đã chuyển {movedCount} tile (trong tổng {selectedTiles.Count} loại tile được chọn) sang tilemap đích.");
    }
}