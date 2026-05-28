using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace TikTokMiniGameDemo.EditorTools
{
    /// <summary>
    /// One-click PlayerSettings + scene + build-target bootstrap for the demo.
    /// 一键完成 PlayerSettings、场景生成与 WebGL 平台切换的 demo 引导工具。
    /// </summary>
    public static class DemoSetup
    {
        /// <summary>
        /// Editor menu entry — runs the whole setup pipeline.
        /// 编辑器菜单入口——一次性把整套配置跑完。
        /// </summary>
        [MenuItem("TikTok Demo/Run Full Setup")]
        public static void RunAll()
        {
            try
            {
                ConfigurePlayerSettings();
                DemoSceneBuilder.BuildAndSave();
                SwitchToWebGL();
                Debug.Log("[DemoSetup] Done.");
            }
            catch (Exception e)
            {
                // EN: When running in headless / batch mode we exit so CI fails fast.
                // ZH: headless / 批处理模式下抛错就退出，方便 CI 失败立刻可见。
                Debug.LogError($"[DemoSetup] failed: {e}");
                EditorApplication.Exit(2);
                return;
            }
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Configure PlayerSettings the way the TikTok minigame container expects.
        /// 按 TikTok 小游戏容器的预期来配置 PlayerSettings。
        /// </summary>
        public static void ConfigurePlayerSettings()
        {
            // EN: companyName / productName are baked into the WebGL output and surfaced in the container UI.
            // ZH: companyName / productName 会写进 WebGL 产物，并在容器上下文中显示。
            PlayerSettings.companyName = "TikTok Minigame Demo";
            PlayerSettings.productName = "TikTok Minigame Demo";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.WebGL, "com.tiktok.minigame.demo");

            // EN: TT minigame requires IL2CPP for WebGL — Mono fallback is not supported.
            // ZH: TT 小游戏 WebGL 必须用 IL2CPP，Mono 不被支持。
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP);
            Debug.Log("[DemoSetup] Player settings configured.");
        }

        /// <summary>
        /// Switch active build target to WebGL if not already.
        /// 若当前 build target 不是 WebGL 就切换过去。
        /// </summary>
        public static void SwitchToWebGL()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                Debug.Log("[DemoSetup] Already WebGL.");
                return;
            }
            bool ok = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            Debug.Log($"[DemoSetup] Switched to WebGL: {ok}");
        }
    }
}
