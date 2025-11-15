using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

[InitializeOnLoad]
public static class EditorSwithScene
{
    public static Type m_toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
    public static ScriptableObject m_currentToolbar;

    private static GUIContent switchSceneBtContent;
    private static List<string> sceneAssetList;

    static EditorSwithScene()
    {
        EditorApplication.delayCall += OnUpdate;
    }

    [InitializeOnLoadMethod]
    static void Init()
    {
        sceneAssetList = new List<string>();
        var curOpenSceneName = EditorSceneManager.GetActiveScene().name;
        switchSceneBtContent = EditorGUIUtility.TrTextContentWithIcon(
            string.IsNullOrEmpty(curOpenSceneName) ? "Switch Scene" : curOpenSceneName,
            "切换场景",
            "UnityLogo"
        );
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        switchSceneBtContent.text = scene.name;
    }

    static void OnUpdate()
    {
        if (m_currentToolbar == null)
        {
            var toolbars = Resources.FindObjectsOfTypeAll(m_toolbarType);
            m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
            if (m_currentToolbar != null)
            {
                var root = m_currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                var rawRoot = root.GetValue(m_currentToolbar);
                var mRoot = rawRoot as VisualElement;
                RegisterCallback("ToolbarZoneRightAlign", GUIRight);

                void RegisterCallback(string root, Action cb)
                {
                    var toolbarZone = mRoot.Q(root);
                    var parent = new VisualElement()
                    {
                        style = { flexGrow = 1, flexDirection = FlexDirection.Row }
                    };
                    var container = new IMGUIContainer();
                    container.onGUIHandler += () => { cb?.Invoke(); };
                    parent.Add(container);
                    toolbarZone.Add(parent);
                }
            }
        }
    }

    static void GUIRight()
    {
        GUILayout.BeginHorizontal();
        if (EditorGUILayout.DropdownButton(
            switchSceneBtContent,
            FocusType.Passive,
            EditorStyles.toolbarPopup,
            GUILayout.MaxWidth(150)))
        {
            DrawSwithSceneDropdownMenus();
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 关键修改：从Build Settings获取场景列表
    /// </summary>
    static void DrawSwithSceneDropdownMenus()
    {
        GenericMenu popMenu = new GenericMenu();
        sceneAssetList.Clear();

        // 直接从Build Settings读取场景（按顺序）
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled && !string.IsNullOrEmpty(scene.path))
            {
                sceneAssetList.Add(scene.path);
                string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                popMenu.AddItem(new GUIContent(sceneName), false, menuIdx =>
                {
                    SwitchScene((int)menuIdx);
                }, sceneAssetList.Count - 1);
            }
        }

        // 如果Build Settings为空，显示警告并改用全项目搜索
        if (sceneAssetList.Count == 0)
        {
            popMenu.AddDisabledItem(new GUIContent("【无Build Settings场景】"));
            Debug.LogWarning("Build Settings中未添加任何场景！");
        }

        popMenu.ShowAsContext();
    }

    static void SwitchScene(int menuIdx)
    {
        if (menuIdx >= 0 && menuIdx < sceneAssetList.Count)
        {
            var scenePath = sceneAssetList[menuIdx];
            var curScene = EditorSceneManager.GetActiveScene();
            if (curScene.isDirty)
            {
                int opIndex = EditorUtility.DisplayDialogComplex(
                    "警告",
                    $"当前场景 {curScene.name} 未保存,是否保存?",
                    "保存", "取消", "不保存"
                );
                switch (opIndex)
                {
                    case 0: // 保存
                        if (!EditorSceneManager.SaveOpenScenes()) return;
                        break;
                    case 1: // 取消
                        return;
                }
            }
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
    }
}