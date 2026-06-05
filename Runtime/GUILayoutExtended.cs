using System.Collections.Generic;
using UnityEngine;

namespace GuiEngine
{
    public class GUILayoutExtended
    {
        private static readonly Dictionary<string, bool> s_openStates = new Dictionary<string, bool>();
        private static readonly Dictionary<string, Vector2> s_scrollPositions = new Dictionary<string, Vector2>();
        private static readonly Dictionary<string, string> s_searchTexts = new Dictionary<string, string>();
        private const int ScrollThreshold = 10;
        private const float ProgressBarTextPadding = 4f;

        private static readonly Color DefaultProgressBackgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
        private static readonly Color DefaultProgressFillColor = new Color(0.24f, 0.5f, 0.85f, 1f);
        private static readonly Color DefaultProgressBorderColor = new Color(0.08f, 0.08f, 0.08f, 1f);

        public static bool Foldout(string text, bool value)
        {
            var key = $"Foldout:{text}";
            var isOpen = s_openStates.TryGetValue(key, out var currentOpen) ? currentOpen : value;
            var arrow = isOpen ? "▼" : "▶";

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (GUILayout.Button($"{arrow} {text}", GUI.skin.button, GUILayout.ExpandWidth(true)))
            {
                isOpen = !isOpen;
                s_openStates[key] = isOpen;
            }
            GUILayout.EndHorizontal();

            return isOpen;
        }

        public static float ProgressBar(float value, float maxValue = 1f)
        {
            return ProgressBarInternal(string.Empty, value, maxValue, false, DefaultProgressFillColor, DefaultProgressBackgroundColor, DefaultProgressBorderColor, 1f, TextAnchor.MiddleCenter);
        }

        public static float ProgressBar(string text, float value, float maxValue = 1f)
        {
            return ProgressBarInternal(text, value, maxValue, true, DefaultProgressFillColor, DefaultProgressBackgroundColor, DefaultProgressBorderColor, 1f, TextAnchor.MiddleCenter);
        }

        public static float ProgressBar(float value, float maxValue, Color fillColor, Color backgroundColor, Color borderColor, float borderThickness = 1f, TextAnchor textAlignment = TextAnchor.MiddleCenter)
        {
            return ProgressBarInternal(string.Empty, value, maxValue, false, fillColor, backgroundColor, borderColor, borderThickness, textAlignment);
        }

        public static float ProgressBar(string text, float value, float maxValue, Color fillColor, Color backgroundColor, Color borderColor, float borderThickness = 1f, TextAnchor textAlignment = TextAnchor.MiddleCenter)
        {
            return ProgressBarInternal(text, value, maxValue, true, fillColor, backgroundColor, borderColor, borderThickness, textAlignment);
        }

        private static float ProgressBarInternal(string text, float value, float maxValue, bool showLabel, Color fillColor, Color backgroundColor, Color borderColor, float borderThickness, TextAnchor textAlignment)
        {
            var normalized = maxValue <= 0f ? 0f : Mathf.Clamp01(value / maxValue);
            var percentage = Mathf.RoundToInt(normalized * 100f);
            borderThickness = Mathf.Max(0f, borderThickness);

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            if (showLabel)
            {
                GUILayout.Label(text, GUILayout.Width(120f));
            }

            var rect = GUILayoutUtility.GetRect(0f, 18f, GUILayout.ExpandWidth(true));

            DrawRect(rect, backgroundColor);

            if (borderThickness > 0f)
            {
                DrawBorder(rect, borderThickness, borderColor);
            }

            var innerRect = new Rect(
                rect.x + borderThickness,
                rect.y + borderThickness,
                Mathf.Max(0f, rect.width - borderThickness * 2f),
                Mathf.Max(0f, rect.height - borderThickness * 2f)
            );

            var fillRect = new Rect(innerRect.x, innerRect.y, innerRect.width * normalized, innerRect.height);
            DrawRect(fillRect, fillColor);

            var textRect = new Rect(
                rect.x + ProgressBarTextPadding,
                rect.y,
                Mathf.Max(0f, rect.width - (ProgressBarTextPadding * 2f)),
                rect.height
            );

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = textAlignment
            };

            GUI.Label(textRect, $"{percentage}%", labelStyle);
            GUILayout.EndHorizontal();

            return normalized;
        }

        public static T Enum<T>(string text, T value) where T : System.Enum
        {
            return Enum(text, value, null);
        }

        public static T Enum<T>(string text, T value, string[] options) where T : System.Enum
        {
            var names = System.Enum.GetNames(typeof(T));
            var values = (T[])System.Enum.GetValues(typeof(T));
            var key = $"{typeof(T).FullName}:{text}";
            var isOpen = s_openStates.TryGetValue(key, out var currentOpen) && currentOpen;
            var scrollPosition = s_scrollPositions.TryGetValue(key, out var currentScroll) ? currentScroll : Vector2.zero;
            var useScroll = names.Length > ScrollThreshold;
            var searchText = s_searchTexts.TryGetValue(key, out var currentSearch) ? currentSearch : string.Empty;

            var currentIndex = System.Array.IndexOf(names, value.ToString());
            var selectedLabel = currentIndex >= 0
                ? GetDisplayName(names, options, currentIndex)
                : value.ToString();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(text, GUILayout.ExpandWidth(false));

            if (GUILayout.Button(selectedLabel, GUILayout.ExpandWidth(true)))
            {
                isOpen = !isOpen;
                s_openStates[key] = isOpen;
            }

            GUILayout.EndHorizontal();

            if (!isOpen)
            {
                return value;
            }

            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));

            if (useScroll)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Search", GUILayout.Width(50f));
                searchText = GUILayout.TextField(searchText ?? string.Empty, GUILayout.ExpandWidth(true));
                s_searchTexts[key] = searchText;
                GUILayout.EndHorizontal();
            }

            if (useScroll)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));
            }

            var filteredIndices = GetFilteredIndices(names, options, searchText, useScroll);

            if (filteredIndices.Count == 0)
            {
                GUILayout.Label("No results");
            }
            else
            {
                foreach (var index in filteredIndices)
                {
                    var displayName = GetDisplayName(names, options, index);
                    var isSelected = EqualityComparer<T>.Default.Equals(value, values[index]);

                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Label(isSelected ? "[x]" : "   ", GUILayout.Width(24f));

                    if (GUILayout.Button(displayName, GUI.skin.button, GUILayout.ExpandWidth(true)))
                    {
                        value = values[index];
                        s_openStates[key] = false;

                        if (useScroll)
                        {
                            s_scrollPositions[key] = scrollPosition;
                            GUILayout.EndHorizontal();
                            GUILayout.EndScrollView();
                        }

                        GUILayout.EndVertical();
                        return value;
                    }

                    GUILayout.EndHorizontal();
                }
            }

            if (useScroll)
            {
                GUILayout.EndScrollView();
                s_scrollPositions[key] = scrollPosition;
            }

            GUILayout.EndVertical();
            return value;
        }

        private static void DrawRect(Rect rect, Color color)
        {
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            var oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = oldColor;
        }

        private static void DrawBorder(Rect rect, float thickness, Color color)
        {
            var top = new Rect(rect.x, rect.y, rect.width, thickness);
            var bottom = new Rect(rect.x, rect.yMax - thickness, rect.width, thickness);
            var left = new Rect(rect.x, rect.y, thickness, rect.height);
            var right = new Rect(rect.xMax - thickness, rect.y, thickness, rect.height);

            DrawRect(top, color);
            DrawRect(bottom, color);
            DrawRect(left, color);
            DrawRect(right, color);
        }

        private static List<int> GetFilteredIndices(string[] names, string[] options, string searchText, bool useScroll)
        {
            var indices = new List<int>();
            var filter = useScroll ? (searchText ?? string.Empty).Trim() : string.Empty;

            for (var i = 0; i < names.Length; i++)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    indices.Add(i);
                    continue;
                }

                var displayName = options != null && options.Length == names.Length ? options[i] : names[i];
                if (names[i].IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    displayName.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    indices.Add(i);
                }
            }

            return indices;
        }

        private static string GetDisplayName(string[] names, string[] options, int index)
        {
            if (options != null && options.Length == names.Length)
            {
                return options[index];
            }

            return names[index];
        }
    }
}
