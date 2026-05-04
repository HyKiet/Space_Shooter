using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Tool nâng cấp hiệu ứng hình ảnh cho Main Menu:
/// 1. Thêm nền cuộn (Scrolling Background)
/// 2. Kích hoạt Particle Background (Sao, Thiên hà)
/// 3. Cấu hình vật thể bay (Meteors, Ships)
/// </summary>
public class MenuVFXSetupTool : EditorWindow
{
    [MenuItem("Tools/Enhance Menu Visuals")]
    public static void EnhanceMenu()
    {
        // 1. Tìm Canvas chính
        GameObject canvasObj = GameObject.Find("MainMenuCanvas");
        if (canvasObj == null)
        {
            // Thử tìm theo component MainMenuUI nếu tên khác
            MainMenuUI foundUI = Object.FindFirstObjectByType<MainMenuUI>();
            if (foundUI != null) canvasObj = foundUI.gameObject;
        }

        if (canvasObj == null)
        {
            Debug.LogError("[MenuVFXTool] Không tìm thấy MainMenuCanvas. Hãy chạy 'Tools/Setup Main Menu' trước.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(canvasObj, "Enhance Menu Visuals");

        // 2. Thiết lập Scrolling Background (Dùng RawImage để cuộn UV)
        Transform panelTransform = canvasObj.transform.Find("MainMenuPanel");
        if (panelTransform == null) panelTransform = canvasObj.transform;

        Transform bgTransform = panelTransform.Find("Background");
        if (bgTransform != null)
        {
            // Làm mờ background cũ để thấy hiệu ứng phía sau (sao, thiên thạch)
            Image bgImage = bgTransform.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = new Color(0.01f, 0.01f, 0.03f, 0.3f); // Giảm alpha xuống 0.3
            }

            // Tạo Scrolling Layer
            GameObject scrollObj = null;
            Transform existingScroll = bgTransform.Find("ScrollingLayer");
            if (existingScroll != null) scrollObj = existingScroll.gameObject;
            else
            {
                scrollObj = new GameObject("ScrollingLayer");
                scrollObj.transform.SetParent(bgTransform, false);
                scrollObj.transform.SetAsFirstSibling();
            }

            RawImage rawImg = scrollObj.GetComponent<RawImage>();
            if (rawImg == null) rawImg = scrollObj.AddComponent<RawImage>();

            // Tìm texture background
            Texture2D bgTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Backgroud.png");
            if (bgTex == null) bgTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/background-1156435.png");

            if (bgTex != null)
            {
                // Đảm bảo Texture được set là Repeat để cuộn mượt
                string texPath = AssetDatabase.GetAssetPath(bgTex);
                TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(texPath);
                if (ti.wrapMode != TextureWrapMode.Repeat)
                {
                    ti.wrapMode = TextureWrapMode.Repeat;
                    ti.SaveAndReimport();
                }

                rawImg.texture = bgTex;
                rawImg.color = new Color(1, 1, 1, 0.4f);
            }
            else
            {
                rawImg.color = new Color(0.1f, 0.1f, 0.2f, 0.5f);
            }

            RectTransform rt = scrollObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            MenuScrollingBackground scroller = scrollObj.GetComponent<MenuScrollingBackground>();
            if (scroller == null) scroller = scrollObj.AddComponent<MenuScrollingBackground>();
            scroller.rawImage = rawImg;
            scroller.scrollSpeed = new Vector2(0.015f, 0.01f);
        }

        // 3. Kích hoạt Particle Background (Sao nhấp nháy, tinh vân)
        GameObject pbgObj = GameObject.Find("MenuParticleBackground");
        if (pbgObj == null)
        {
            pbgObj = new GameObject("MenuParticleBackground");
            pbgObj.AddComponent<MenuParticleBackground>();
            Undo.RegisterCreatedObjectUndo(pbgObj, "Create Particle Background");
        }

        // 4. Cấu hình Drifter Spawner (Vật thể bay)
        MenuDrifterSpawner spawner = canvasObj.GetComponent<MenuDrifterSpawner>();
        if (spawner == null) spawner = canvasObj.AddComponent<MenuDrifterSpawner>();

        // Tự động quét prefabs thiên thạch và kẻ địch
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        List<GameObject> foundPrefabs = new List<GameObject>();
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string lp = path.ToLower();
            // Lọc các prefab phù hợp để làm vật thể trôi nổi
            if ((lp.Contains("meteor") || lp.Contains("asteroid") || lp.Contains("enemy") || lp.Contains("obstacle")) 
                && !lp.Contains("projectile") && !lp.Contains("effect") && !lp.Contains("pickup"))
            {
                GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (p != null) foundPrefabs.Add(p);
            }
        }

        if (foundPrefabs.Count > 0)
        {
            // Dùng SerializedObject để gán mảng chắc chắn hơn
            SerializedObject so = new SerializedObject(spawner);
            SerializedProperty prop = so.FindProperty("prefabsToSpawn");
            prop.ClearArray();
            prop.arraySize = foundPrefabs.Count;
            for (int i = 0; i < foundPrefabs.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = foundPrefabs[i];
            }
            
            // Tốc độ chậm lại một chút cho các vật thể lớn (trông chuyên nghiệp hơn)
            so.FindProperty("spawnInterval").floatValue = 1.2f;
            so.FindProperty("minSpeed").floatValue = 0.5f;
            so.FindProperty("maxSpeed").floatValue = 2.0f;

            // Làm cho thiên thạch cực to giống ảnh minh họa (từ 8 đến 15 lần)
            so.FindProperty("minScale").floatValue = 8.0f;
            so.FindProperty("maxScale").floatValue = 15.0f;
            
            so.ApplyModifiedProperties();
            Debug.Log($"[MenuVFXTool] Đã nạp {foundPrefabs.Count} vật thể vào Spawner.");
        }
        else
        {
            Debug.LogWarning("[MenuVFXTool] Không tìm thấy prefab nào phù hợp để làm vật thể bay.");
        }

        // 5. Đảm bảo Camera ở chế độ tốt cho Menu
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.01f, 0.01f, 0.02f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        Debug.Log("[MenuVFXTool] Hoàn tất! Đã thêm nền cuộn, hiệu ứng sao và vật thể bay.");
        Selection.activeGameObject = canvasObj;
    }
}
