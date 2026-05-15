using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using SaveWorld.Game.Core;

namespace SaveWorld.Editor
{
    /// <summary>
    /// 空微信小游戏场景创建工具
    /// 菜单路径: WeChat → Create Empty Scene
    /// 创建最小化场景并自动设为构建目标
    /// </summary>
    public static class CreateEmptyScene
    {
        private const string ScenePath = "Assets/Scenes/Empty.unity";

        [MenuItem("WeChat/Create Empty Scene", false, 100)]
        public static void Create()
        {
            // 确认是否覆盖已有场景
            if (System.IO.File.Exists(ScenePath))
            {
                if (!EditorUtility.DisplayDialog(
                    "场景已存在",
                    $"场景 {ScenePath} 已存在，是否覆盖？",
                    "覆盖",
                    "取消"))
                {
                    return;
                }
            }

            // 创建新场景
            var scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single
            );

            // ---- Main Camera ----
            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 1f);
            camera.orthographic = false;
            cameraGo.tag = "MainCamera";

            // ---- Directional Light ----
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.color = Color.white;

            // ---- Canvas ----
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            var canvasScaler = canvasGo.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(750, 1334);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // ---- EventSystem ----
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();

            // ---- EmptyBootstrap ----
            var bootstrapGo = new GameObject("EmptyBootstrap");
            bootstrapGo.AddComponent<EmptyBootstrap>();

            // 保存场景
            try
            {
                // 确保目录存在
                var dir = System.IO.Path.GetDirectoryName(ScenePath);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                EditorSceneManager.SaveScene(scene, ScenePath);
                Debug.Log($"[CreateEmptyScene] 空场景已创建: {ScenePath}");
                Debug.Log("[CreateEmptyScene] 场景包含: Main Camera, Directional Light, Canvas, EventSystem, EmptyBootstrap");

                // 自动添加到 Build Settings 并设为唯一启用场景
                AddToBuildSettings();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CreateEmptyScene] 保存场景失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 将 Empty.unity 添加到 Build Settings，并禁用其他场景
        /// </summary>
        private static void AddToBuildSettings()
        {
            var newScenes = new List<EditorBuildSettingsScene>();

            // 保留原有场景引用但设为禁用
            foreach (var existing in EditorBuildSettings.scenes)
            {
                newScenes.Add(new EditorBuildSettingsScene(existing.path, false));
            }

            // 查找或新增 Empty 场景
            bool emptyFound = false;
            for (int i = 0; i < newScenes.Count; i++)
            {
                if (newScenes[i].path == ScenePath)
                {
                    newScenes[i] = new EditorBuildSettingsScene(ScenePath, true);
                    emptyFound = true;
                    break;
                }
            }
            if (!emptyFound)
            {
                newScenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            }

            EditorBuildSettings.scenes = newScenes.ToArray();

            Debug.Log("[CreateEmptyScene] Build Settings 已更新: Empty.unity 设为唯一启用场景");
            Debug.Log("[CreateEmptyScene] 准备就绪！请执行「微信小游戏 → 构建」");
        }
    }
}
