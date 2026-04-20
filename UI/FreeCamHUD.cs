using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SimpleFreeCam.UI
{
    internal class FreeCamHUD : MonoBehaviour
    {
        internal static FreeCamHUD Instance { get; private set; }
        internal enum WarningReason
        {
            None,
            DistanceWarning,
            ResetWarning
        }

        private GameObject hudCanvasObj;

        private Image statsBorder;
        private Image statsBackground;
        private TextMeshProUGUI statsText;

        private Image warningBorder;
        private Image warningBackground;
        private TextMeshProUGUI warningText;

        private static float ColorAlpha = 1f;

        private static Color Blackish_Background = new Color(0.04f, 0.04f, 0.04f, ColorAlpha);
        // base colors
        private static Color Green_Outline = new Color(0.49f, 0.82f, 0.49f, ColorAlpha);
        private static Color Green_Text = new Color(0.27f, 0.47f, 0.27f, ColorAlpha);
        // dirived colors
        private static Color Orange_Outline;
        private static Color Orange_Text;
        private static Color Distance_Outline;
        private static Color Distance_Text;
        private static Color Reset_Outline;
        private static Color Reset_Text;

        private static bool ShowDistanceWarning = false;
        private static bool ShouldHideUI = false;
        private static WarningReason currentWarningReason = WarningReason.None;

        private const float BoxWidth = 300f;
        private const float BoxHeight = 112f;
        private const float BottomMargin = 100f;
        private const float BorderSize = 4f;

        private const float Warning_BoxWidth = 540f;
        private const float Warning_BoxHeight = 80f;
        private const float Warning_BottomMargin = 250f;
        private const float Warning_BorderSize = 2f;

        private const int Text_Fontsize = 20;

        private static readonly string[] WarningStrings = 
        {
            $"! NO WARNING TO DISPLAY !\n<size=75%>! THIS MUST BE AN ERROR !</size>",
            $"! FREECAM TOO FAR FROM PLAYER !\n<size=75%>! MOVE CLOSER OR THE FREECAM WILL TELEPORT TO THE PLAYER ON NEXT USE !</size>",
            $"! RESET FREECAM TRANSFORM ENABLED !\n<size=75%>! THE FREECAM WILL TELEPORT TO THE PLAYER ON NEXT USE !</size>"
        };

        private static void InitializeColors()
        {
            const float OrangeHueShift = 0.72f;
            const float DistanceHueShift = 0.80f;
            const float ResetHueShift = 0.41f;

            Orange_Outline = ShiftHue(Green_Outline, OrangeHueShift);
            Orange_Text = ShiftHue(Green_Text, OrangeHueShift);

            Distance_Outline = ShiftHue(Green_Outline, DistanceHueShift);
            Distance_Text = ShiftHue(Green_Text, DistanceHueShift);

            Reset_Outline = ShiftHue(Green_Outline, ResetHueShift);
            Reset_Text = ShiftHue(Green_Text, ResetHueShift);
        }

        private static Color ShiftHue(Color color, float hueShift)
        {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            h = (h + hueShift) % 1.0f;
            return Color.HSVToRGB(h, s, v);
        }

        public static void HideUI_performed(InputAction.CallbackContext obj)
        {
            if (!obj.performed) return;

            if (FreeCamClass.isInFreeCam)
            {
                ShouldHideUI = !ShouldHideUI;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeColors();
            CreateTMPUI();
            UpdateOpacity(SimpleFreeCamPatchBase.instance.FreeCamConfigEntryFloatOpacity.Value);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void TriggerDistanceWarning(bool doWarning)
        {
            ShowDistanceWarning = doWarning;
        }

        private static bool ShouldShowWarning()
        {
            if (!SimpleFreeCamPatchBase.instance.FreeCamConfigDisableShowWarningReset.Value)
            {
                if (SimpleFreeCamPatchBase.instance.FreeCamConfigEntryResetTransform.Value)
                {
                    currentWarningReason = WarningReason.ResetWarning;
                    return true;
                }
            }

            if (!SimpleFreeCamPatchBase.instance.FreeCamConfigDisableShowWarningDistance.Value) {
                if (ShowDistanceWarning) {
                    currentWarningReason = WarningReason.DistanceWarning;
                    return true;
                }
            }
            return false;
        }

        private void Update()
        {
            bool shouldDrawUI = FreeCamClass.isInFreeCam;
            if (shouldDrawUI)
            {
                if (SimpleFreeCamPatchBase.instance.FreeCamConfigEntryDefaultUIVisiblity.Value)
                {
                    shouldDrawUI = !ShouldHideUI;
                }
                else
                {
                    shouldDrawUI = ShouldHideUI;
                }
            }

            if (hudCanvasObj.activeSelf != shouldDrawUI)
            {
                hudCanvasObj.SetActive(shouldDrawUI);
            }

            if (!shouldDrawUI) return;

            bool isLocked = FreeCamClass.lockFreeCam;
            statsBorder.color = isLocked ? Orange_Outline : Green_Outline;
            statsBackground.color = Blackish_Background;
            statsText.color = isLocked ? Orange_Text : Green_Text;

            string activeOrNot = isLocked ? "LOCKED" : "UNLOCKED";

            statsText.text = $"FOV: {FreeCamClass.GetFreecamFOV():F1}\nSpeed: {FreeCamClass.GetSpeed():F2}x\nFreecam {activeOrNot}";

            bool should = ShouldShowWarning();
            UpdateWarningColorsAndText();
            if (warningBorder.gameObject.activeSelf != should)
            {
                warningBorder.gameObject.SetActive(should);
            }
        }

        public static void UpdateOpacity(float newOpacity)
        {
            UpdateOpacity(newOpacity, false);
        }

        private static void UpdateOpacity(float newOpacity, bool onlyWarning)
        {
            if (Instance != null)
            {
                Instance.UpdateStaticColors();
            }

            float bgAlpha = newOpacity - 0.3f;
            float textAlpha = newOpacity;

            if (!onlyWarning)
            {
                ColorAlpha = newOpacity;

                Blackish_Background.a = textAlpha;

                Green_Outline.a = bgAlpha;
                Green_Text.a = textAlpha;

                Orange_Outline.a = bgAlpha;
                Orange_Text.a = textAlpha;
            }

            Distance_Outline.a = bgAlpha;
            Distance_Text.a = textAlpha;

            Reset_Outline.a = bgAlpha;
            Reset_Text.a = textAlpha;
        }

        public static void ResetHideUI()
        {
            if (SimpleFreeCamPatchBase.instance.FreeCamConfigEntryHideUI.Value)
            {
                ShouldHideUI = false;
            }
        }

        private void UpdateWarningColorsAndText()
        {
            warningText.text = GetWarningText();
            UpdateOpacity(SimpleFreeCamPatchBase.instance.FreeCamConfigEntryFloatOpacity.Value);
        }

        private static string GetWarningText()
        {
            int index = (int)currentWarningReason;
            if (index >= 0 && index < WarningStrings.Length)
            {
                return WarningStrings[index];
            }
            return WarningStrings[0];
        }

        private static Color GetWarningColor(int index)
        {
            bool isYellow = currentWarningReason == WarningReason.DistanceWarning;
            switch (index)
            {
                case 0: //outline
                    return isYellow ? Distance_Outline : Reset_Outline;
                case 1: //text
                    return isYellow ? Distance_Text : Reset_Text;
                default:
                    return new Color(0, 0, 0, 0);
            }
        }

        private void CreateTMPUI()
        {
            TMP_FontAsset inGameFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name.Contains("3270"));

            hudCanvasObj = new GameObject("FreeCamTMPCanvas");
            hudCanvasObj.transform.SetParent(this.transform);

            Canvas canvas = hudCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = hudCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;

            statsBorder = CreateUIBox(hudCanvasObj.transform, "StatsBorder", BoxWidth + BorderSize * 2, BoxHeight + BorderSize * 2, BottomMargin - BorderSize);
            statsBackground = CreateUIBox(statsBorder.transform, "StatsBackground", BoxWidth, BoxHeight, 0);

            RectTransform bgRect = statsBackground.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = new Vector2(BorderSize, BorderSize);
            bgRect.offsetMax = new Vector2(-BorderSize, -BorderSize);

            statsText = CreateUIText(statsBackground.transform, "StatsText", inGameFont);

            warningBorder = CreateUIBox(hudCanvasObj.transform, "WarningBorder", Warning_BoxWidth + Warning_BorderSize * 2, Warning_BoxHeight + Warning_BorderSize * 2, Warning_BottomMargin - Warning_BorderSize);
            warningBackground = CreateUIBox(warningBorder.transform, "WarningBackground", Warning_BoxWidth, Warning_BoxHeight, 0);

            RectTransform warnBgRect = warningBackground.GetComponent<RectTransform>();
            warnBgRect.anchorMin = Vector2.zero;
            warnBgRect.anchorMax = Vector2.one;
            warnBgRect.offsetMin = new Vector2(Warning_BorderSize, Warning_BorderSize);
            warnBgRect.offsetMax = new Vector2(-Warning_BorderSize, -Warning_BorderSize);

            warningText = CreateUIText(warningBackground.transform, "WarningText", inGameFont);

            warningText.text = WarningStrings[0];
            warningBackground.color = Blackish_Background;

            UpdateStaticColors();
            hudCanvasObj.SetActive(false);
        }

        private void UpdateStaticColors()
        {
            if (warningBorder != null) warningBorder.color = GetWarningColor(0);
            if (warningText != null) warningText.color = GetWarningColor(1);
        }

        private Image CreateUIBox(Transform parent, string name, float width, float height, float yOffset)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            Image img = obj.AddComponent<Image>();

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);

            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0, yOffset);

            return img;
        }

        private TextMeshProUGUI CreateUIText(Transform parent, string name, TMP_FontAsset font)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            TextMeshProUGUI tmpText = obj.AddComponent<TextMeshProUGUI>();
            tmpText.alignment = TextAlignmentOptions.CenterGeoAligned;
            tmpText.fontSize = Text_Fontsize;
            tmpText.fontStyle = FontStyles.Bold;
            if (font != null) tmpText.font = font;

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            return tmpText;
        }
    }
}
