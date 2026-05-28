using System;
using System.IO;
using TikTokMiniGameDemo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TikTokMiniGameDemo.EditorTools
{
    /// <summary>
    /// Rebuilds the <c>MainMenu.unity</c> scene entirely from code so the layout never
    /// drifts. Triggered manually via the <c>TikTok Demo → Rebuild MainMenu Scene</c>
    /// menu item, or as part of <see cref="DemoSetup.RunAll"/>.
    /// 通过纯代码重建 <c>MainMenu.unity</c> 场景，避免布局飘移。
    /// 可通过菜单 <c>TikTok Demo → Rebuild MainMenu Scene</c> 手动触发，
    /// 也会被 <see cref="DemoSetup.RunAll"/> 调用。
    /// </summary>
    public static class DemoSceneBuilder
    {
        private const string ScenePath = "Assets/Demo/Scenes/MainMenu.unity";

        /// <summary>
        /// Build the demo scene in memory, save it to disk and register it as build scene #0.
        /// 在内存中构建 demo 场景、写回磁盘并注册为 Build Settings 中的 0 号场景。
        /// </summary>
        [MenuItem("TikTok Demo/Rebuild MainMenu Scene")]
        public static void BuildAndSave()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            BuildHierarchy();

            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[DemoSceneBuilder] Saved scene → {ScenePath} (ok={saved})");

            // EN: Replace Build Settings so this scene is always the boot scene.
            // ZH: 覆写 Build Settings，确保这个场景永远是启动场景。
            var buildScenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorBuildSettings.scenes = buildScenes;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Spawn all UI GameObjects under a fresh Canvas. Layout is a vertical 4-band stack.
        /// 在全新 Canvas 下生成所有 UI GameObject。布局为上下 4 段堆叠。
        /// </summary>
        public static void BuildHierarchy()
        {
            EnsureEventSystem();

            // EN: Single ScreenSpaceOverlay canvas sized for portrait phones (1080×1920 reference).
            // ZH: 单个 ScreenSpaceOverlay Canvas，按竖屏手机参考分辨率 1080×1920 设计。
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            // EN: Reference canvas 1080 × 1920. Vertical band layout, top→bottom:
            //   [0..240]     Header (title + status + init button)
            //   [240..1160]  Feature buttons scroll (~920 tall)
            //   [1160..1820] Log scroll (~660 tall)
            //   [1820..1920] Clear-log button row
            // ZH: 参考画布 1080 × 1920，自上而下分四段：
            //   [0..240]     头部（标题 + 状态 + Init 按钮）
            //   [240..1160]  功能按钮滚动区（约 920 高）
            //   [1160..1820] 日志滚动区（约 660 高）
            //   [1820..1920] 清空日志按钮所在底栏

            // Background — slate-800-ish, soft dark with a slight blue tilt
            // 背景——slate-800 风格，柔和的偏蓝深色
            var bg = CreatePanel("Background", canvasGo.transform, new Color(0.13f, 0.16f, 0.21f, 1));
            Stretch(bg.rectTransform);

            // EN: Feature buttons scroll — fills horizontally, sits between header and log.
            // ZH: 功能按钮滚动区——横向铺满，夹在头部和日志区之间。
            var btnScroll = CreateScrollView("FeatureScroll", canvasGo.transform);
            var btnScrollRT = btnScroll.GetComponent<RectTransform>();
            Anchor(btnScrollRT, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            btnScrollRT.offsetMin = new Vector2(24, 760);   // bottom: 100 clear-row + 660 log = 760
            btnScrollRT.offsetMax = new Vector2(-24, -260); // top: header strip is 240 + 20 padding

            var btnContent = btnScroll.GetComponent<ScrollRect>().content;
            var vlg = btnContent.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var csf = btnContent.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // EN: Template button as first child. Stays inactive — DemoMainMenu clones it
            //     at runtime via Instantiate() to spawn category headers and sub-buttons.
            // ZH: 首个子节点是模板按钮，保持 inactive。DemoMainMenu 运行时通过
            //     Instantiate() 克隆它来生成分类头与子按钮。
            var template = CreateButton("ButtonTemplate", btnContent.transform, "(template)");
            template.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 70);
            template.gameObject.SetActive(false);

            // EN: Log panel — bottom strip, above clear-log button.
            // ZH: 日志面板——底部条带，位于清空按钮上方。
            var logScroll = CreateScrollView("LogScroll", canvasGo.transform);
            var logScrollRT = logScroll.GetComponent<RectTransform>();
            Anchor(logScrollRT, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            logScrollRT.anchoredPosition = new Vector2(0, 100); // bottom edge 100 px above canvas bottom
            logScrollRT.sizeDelta = new Vector2(-48, 660);      // -48 horizontal stretch margin, 660 tall

            var logContent = logScroll.GetComponent<ScrollRect>().content;
            var logText = CreateText("LogText", logContent.transform, "", 24, FontStyle.Normal);
            logText.alignment = TextAnchor.UpperLeft;
            logText.horizontalOverflow = HorizontalWrapMode.Wrap;
            logText.verticalOverflow = VerticalWrapMode.Overflow;
            logText.color = new Color(0.92f, 0.92f, 0.92f);
            var logRT = logText.rectTransform;
            Anchor(logRT, Vector2.zero, Vector2.one, new Vector2(0, 1));
            logRT.offsetMin = new Vector2(16, 16);
            logRT.offsetMax = new Vector2(-16, -16);
            var logCsf = logContent.gameObject.AddComponent<ContentSizeFitter>();
            logCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var logVlg = logContent.gameObject.AddComponent<VerticalLayoutGroup>();
            logVlg.childControlWidth = true;
            logVlg.childControlHeight = true;
            logVlg.childForceExpandWidth = true;
            logVlg.childForceExpandHeight = false;

            // EN: Header — title + status + init button. Created after the scrolls so it
            //     ends up later in sibling order → renders on top of the scroll backgrounds
            //     where they overlap (UGUI's painter order = sibling order).
            // ZH: 头部——标题 + 状态 + Init 按钮。在滚动区之后创建，sibling 顺序更靠后
            //     → UGUI 画家算法会把它画在滚动区上面，避免被遮挡。
            var titleText = CreateText("Title", canvasGo.transform, "TikTok Mini Game Demo", 42, FontStyle.Bold);
            Anchor(titleText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            titleText.rectTransform.sizeDelta = new Vector2(0, 80);
            titleText.rectTransform.anchoredPosition = new Vector2(0, -50);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;

            var statusText = CreateText("Status", canvasGo.transform, "SDK status: -", 28, FontStyle.Normal);
            Anchor(statusText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            statusText.rectTransform.sizeDelta = new Vector2(0, 40);
            statusText.rectTransform.anchoredPosition = new Vector2(0, -110);
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.color = new Color(0.85f, 0.85f, 0.85f);
            statusText.supportRichText = true;

            var initBtn = CreateButton("InitSdkButton", canvasGo.transform, "Init SDK");
            Anchor(initBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            initBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 70);
            initBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -180);

            // Clear-log button — bottom-right corner, below log panel
            // 清空日志按钮——右下角，日志面板下方
            var clearBtn = CreateButton("ClearLogButton", canvasGo.transform, "Clear Log");
            var clearRT = clearBtn.GetComponent<RectTransform>();
            Anchor(clearRT, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
            clearRT.sizeDelta = new Vector2(240, 60);
            clearRT.anchoredPosition = new Vector2(-30, 20);

            // EN: Controller MonoBehaviour gets references to every UI element via the inspector
            //     fields it exposes. Wiring up here means a fresh rebuild always lands working.
            // ZH: Controller MonoBehaviour 通过 inspector 字段拿到所有 UI 引用。
            //     在这里直接接线，重建后立刻可用。
            var controllerGo = new GameObject("DemoMainMenu");
            controllerGo.transform.SetParent(canvasGo.transform, false);
            var controller = controllerGo.AddComponent<DemoMainMenu>();
            controller.ButtonContainer = btnContent;
            controller.TitleText = titleText;
            controller.LogText = logText;
            controller.LogScroll = logScroll.GetComponent<ScrollRect>();
            controller.ClearLogButton = clearBtn.GetComponent<Button>();
            controller.InitSdkButton = initBtn.GetComponent<Button>();
            controller.StatusText = statusText;
            controller.PackageVersion = ReadSdkPackageVersion();
        }

        /// <summary>
        /// Read the SDK plugin's <c>package.json</c> and pull the <c>version</c> field.
        /// Falls back to <c>"?"</c> if the file is missing or malformed.
        /// 读取 SDK 插件的 <c>package.json</c> 并取出 <c>version</c> 字段。
        /// 文件缺失或解析失败时回退到 <c>"?"</c>。
        /// </summary>
        private static string ReadSdkPackageVersion()
        {
            const string PackageJsonPath = "Assets/Plugins/com.tiktok.minigame/package.json";
            try
            {
                if (!File.Exists(PackageJsonPath)) return "?";
                var json = File.ReadAllText(PackageJsonPath);
                // EN: Minimal regex pull — avoids depending on a full JSON parser for one field.
                // ZH: 用最小正则取 version 字段，避免为了一个字段引入完整 JSON 库。
                var match = System.Text.RegularExpressions.Regex.Match(
                    json, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                return match.Success ? match.Groups[1].Value : "?";
            }
            catch
            {
                return "?";
            }
        }

        /// <summary>
        /// Ensure an EventSystem exists so UGUI input keeps working.
        /// 确保场景里有 EventSystem，UGUI 输入才能正常分发。
        /// </summary>
        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        private static Image CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        private static Text CreateText(string name, Transform parent, string content, int size, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleLeft;
            return t;
        }

        /// <summary>
        /// Create a Button whose displayed color comes from <c>Button.colors</c> directly.
        /// 创建按钮，显示色直接由 <c>Button.colors</c> 决定。
        /// </summary>
        /// <remarks>
        /// EN: <c>Selectable.Transition.ColorTint</c> multiplies <c>Image.color</c> by the
        ///     current state color. Setting <c>Image.color = white</c> lets the tint be the
        ///     actual displayed color — so hover can be brighter than normal, press darker.
        ///     If <c>Image.color</c> is already saturated, multiply can only darken further,
        ///     which inverts the expected hover-brighter feel.
        /// ZH: <c>Selectable.Transition.ColorTint</c> 会把 <c>Image.color</c> 与当前状态色相乘。
        ///     把 <c>Image.color</c> 设成白色，tint 就成了真正的显示色——这样 hover 才能比
        ///     normal 更亮、按下更暗。如果 Image.color 本身已经饱和，乘法只会越乘越暗，
        ///     反而把 hover 变深，违反直觉。
        /// </remarks>
        private static Button CreateButton(string name, Transform parent, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = Color.white;
            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.normalColor = new Color(0.27f, 0.55f, 0.95f);
            colors.highlightedColor = new Color(0.40f, 0.66f, 1.00f);
            colors.pressedColor = new Color(0.15f, 0.38f, 0.78f);
            colors.selectedColor = new Color(0.27f, 0.55f, 0.95f);
            colors.disabledColor = new Color(0.40f, 0.40f, 0.45f);
            btn.colors = colors;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var t = textGo.GetComponent<Text>();
            t.text = label;
            t.fontSize = 28;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            Stretch(t.rectTransform);
            return btn;
        }

        /// <summary>
        /// Create a ScrollRect with a clipped Viewport and Content. Uses <see cref="RectMask2D"/>
        /// rather than the classic <see cref="Mask"/> + transparent <see cref="Image"/> trick.
        /// 用 <see cref="RectMask2D"/> 做裁剪的 ScrollRect，不走 <see cref="Mask"/> + 透明
        /// <see cref="Image"/> 的旧组合。
        /// </summary>
        /// <remarks>
        /// EN: In Unity 2022.3+, <c>Mask + null-sprite Image + alpha ≈ 0</c> silently clips
        ///     every descendant: the Image generates no stencil geometry so the stencil buffer
        ///     never gets written, and children fail the kEqual stencil test → they vanish.
        ///     <c>RectMask2D</c> avoids stencils entirely — it clips by rect bounds in shader.
        /// ZH: 在 Unity 2022.3+ 里，<c>Mask + 空 sprite 的 Image + alpha 接近 0</c> 这套老技巧
        ///     会把子节点全部静默裁掉：Image 不生成 stencil 几何 → stencil 缓冲没有写入 →
        ///     子节点 kEqual 测试不通过就不画。<c>RectMask2D</c> 完全不走 stencil，按矩形边界
        ///     在 shader 里裁剪，更安全。
        /// </remarks>
        private static GameObject CreateScrollView(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.20f, 0.23f, 0.29f, 1f);
            var scroll = go.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(go.transform, false);
            Stretch(viewport.GetComponent<RectTransform>());

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0, 1);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0, 0);

            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRT;
            return go;
        }

        /// <summary>Anchor stretch to fill the parent. / 锚定铺满父节点。</summary>
        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>Set anchor min/max and pivot in one call. / 一次性设 anchorMin/Max + pivot。</summary>
        private static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
        }
    }
}
