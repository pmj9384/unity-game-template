using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// 템플릿 방향 프리셋(2026-10-29). 리포는 하나로 두고 Hub 템플릿을 가로형·세로형 두 개로 패킹한다(tools/pack-templates.sh).
// 가로/세로가 실제로 다른 것만 바꾼다: 화면 방향(Player Settings), 캔버스 기준 해상도, 설정창 프리팹.
// 리포를 둘로 나누지 않은 이유: 매니저·세이브·UI 키트 수정을 두 번 하게 되고 한쪽만 고쳐져 갈라진다.
// 로비·하단 메뉴 배치는 아직 세로 기준이다(가로 로비는 다음 단계). 화면 방향의 SSOT는 ProjectSettings라 BuildScript는 방향을 덮어쓰지 않는다
public static class OrientationPreset
{
    private const string SafeAreaPath = "Assets/Prefabs/UI/SafeAreaPanel.prefab";
    private const string LandscapeSettings = "Assets/Prefabs/UI/Settings/SettingsPanel_Landscape.prefab";
    private const string PortraitSettings = "Assets/Prefabs/UI/Settings/SettingsPanel_Portrait.prefab";

    [MenuItem("Tools/Template/가로형 적용")]
    public static void ApplyLandscape()
    {
        Apply(landscape: true);
    }

    [MenuItem("Tools/Template/세로형 적용")]
    public static void ApplyPortrait()
    {
        Apply(landscape: false);
    }

    private static void Apply(bool landscape)
    {
        ApplyPlayerSettings(landscape);
        ApplyCanvasScalers(landscape);
        SwapSettingsPanel(landscape ? LandscapeSettings : PortraitSettings);
        AssetDatabase.SaveAssets();
        Debug.Log("[OrientationPreset] " + (landscape ? "가로형" : "세로형") + " 적용");
    }

    // 가로형은 양쪽 가로만, 세로형은 세로만 회전한다
    // PlayerSettings 정적 속성으로 바꾸면 배치 모드에서 ProjectSettings.asset에 저장되지 않았다(10-29 실제로 확인). 직렬화 값으로 바꾸고 저장한다
    private static void ApplyPlayerSettings(bool landscape)
    {
        Object asset = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
        var so = new SerializedObject(asset);
        so.FindProperty("defaultScreenOrientation").intValue = (int)UIOrientation.AutoRotation;
        so.FindProperty("allowedAutorotateToLandscapeLeft").boolValue = landscape;
        so.FindProperty("allowedAutorotateToLandscapeRight").boolValue = landscape;
        so.FindProperty("allowedAutorotateToPortrait").boolValue = !landscape;
        so.FindProperty("allowedAutorotateToPortraitUpsideDown").boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
    }

    // 짧은 변 1080 기준(UI 키트 크기의 전제). 가로는 높이에, 세로는 폭에 맞춘다(실험/2026-10-29-설정창-기본UI-표준-조사.md)
    private static void ApplyCanvasScalers(bool landscape)
    {
        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.enabled) { continue; }
            var scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
            foreach (CanvasScaler scaler in Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize) { continue; }
                scaler.referenceResolution = landscape ? new Vector2(1920f, 1080f) : new Vector2(1080f, 1920f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = landscape ? 1f : 0f;
                EditorUtility.SetDirty(scaler);
            }
            EditorSceneManager.SaveScene(scene);
        }
    }

    // SafeAreaPanel 안의 설정창(OutGameSettingsPanel)을 고른 프리팹 인스턴스로 바꾼다. 여는 쪽은 타입으로 찾으므로(ShowPopup<OutGameSettingsPanel>) 참조가 끊기지 않는다
    private static void SwapSettingsPanel(string prefabPath)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject root = PrefabUtility.LoadPrefabContents(SafeAreaPath);
        try
        {
            OutGameSettingsPanel old = root.GetComponentInChildren<OutGameSettingsPanel>(true);
            Transform parent = old.transform.parent;
            int index = old.transform.GetSiblingIndex();
            Object.DestroyImmediate(PrefabUtility.IsPartOfPrefabInstance(old.gameObject) ? PrefabUtility.GetOutermostPrefabInstanceRoot(old.gameObject) : old.gameObject);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = "SettingsPanel";
            instance.transform.SetSiblingIndex(index);
            instance.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, SafeAreaPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
