using System.Collections.Generic;
using UnityEngine;

namespace GuiEngine
{
    public class GUILayoutExtended
    {
        public enum HelpBoxType
        {
            Info,
            Warning,
            Error,
            Success
        }

        private sealed class ToastMessage
        {
            public string Message;
            public HelpBoxType Type;
            public float ExpiresAt;
        }

        private static readonly Dictionary<string, bool> s_openStates = new Dictionary<string, bool>();
        private static readonly Dictionary<string, Vector2> s_scrollPositions = new Dictionary<string, Vector2>();
        private static readonly Dictionary<string, string> s_searchTexts = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> s_numericTexts = new Dictionary<string, string>();
        private static readonly Dictionary<string, int> s_toolbarSelections = new Dictionary<string, int>();
        private static readonly List<ToastMessage> s_toasts = new List<ToastMessage>();

        private const int ScrollThreshold = 10;
        private const float ProgressBarTextPadding = 4f;
        private const float DefaultToastDuration = 2.5f;
        private const float ToastWidth = 280f;
        private const float ToastHeight = 28f;
        private const float ToastMargin = 10f;

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

        public static void SectionHeader(string title, string subtitle = null)
        {
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            var subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.75f, 0.75f, 0.75f, 1f) }
            };

            GUILayout.Space(4f);
            GUILayout.Label(title ?? string.Empty, titleStyle, GUILayout.ExpandWidth(true));

            if (!string.IsNullOrEmpty(subtitle))
            {
                GUILayout.Label(subtitle, subtitleStyle, GUILayout.ExpandWidth(true));
            }

            Separator();
        }

        public static void Separator(float thickness = 1f, Color? color = null, float padding = 4f)
        {
            thickness = Mathf.Max(1f, thickness);
            padding = Mathf.Max(0f, padding);

            var totalHeight = thickness + padding * 2f;
            var rect = GUILayoutUtility.GetRect(0f, totalHeight, GUILayout.ExpandWidth(true));
            var lineRect = new Rect(rect.x, rect.y + padding, rect.width, thickness);

            DrawRect(lineRect, color ?? new Color(0.25f, 0.25f, 0.25f, 1f));
        }

        public static void HelpBox(string message, HelpBoxType type)
        {
            GetHelpBoxColors(type, out var background, out var border, out var text);

            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                padding = new RectOffset(8, 8, 6, 6)
            };
            style.normal.textColor = text;

            var content = new GUIContent(message ?? string.Empty);
            var height = Mathf.Max(28f, style.CalcHeight(content, Mathf.Max(50f, Screen.width - 40f)) + 2f);
            var rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));

            DrawRect(rect, background);
            DrawBorder(rect, 1f, border);
            GUI.Label(rect, content, style);
        }

        public static string SearchField(string key, string value, string placeholder = "Search...")
        {
            var text = value ?? (s_searchTexts.TryGetValue(key, out var currentSearch) ? currentSearch : string.Empty);

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("🔎", GUILayout.Width(20f));

            text = GUILayout.TextField(text ?? string.Empty, GUILayout.ExpandWidth(true));
            var textRect = GUILayoutUtility.GetLastRect();

            if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(placeholder) && Event.current.type == EventType.Repaint)
            {
                var placeholderStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft
                };
                placeholderStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f, 1f);

                var placeholderRect = new Rect(textRect.x + 4f, textRect.y, textRect.width - 8f, textRect.height);
                GUI.Label(placeholderRect, placeholder, placeholderStyle);
            }

            if (GUILayout.Button("×", GUILayout.Width(24f)))
            {
                text = string.Empty;
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(key))
            {
                s_searchTexts[key] = text;
            }

            return text;
        }

        public static int Toolbar(string key, int selected, string[] tabs)
        {
            if (tabs == null || tabs.Length == 0)
            {
                return selected;
            }

            if (!string.IsNullOrEmpty(key) && s_toolbarSelections.TryGetValue(key, out var storedSelected))
            {
                selected = storedSelected;
            }

            selected = Mathf.Clamp(selected, 0, tabs.Length - 1);
            var newSelected = GUILayout.Toolbar(selected, tabs, GUILayout.ExpandWidth(true));

            if (!string.IsNullOrEmpty(key))
            {
                s_toolbarSelections[key] = newSelected;
            }

            return newSelected;
        }

        public static bool TagSelector(string key, IList<string> tags, ref string input, bool allowDuplicates = false)
        {
            var changed = false;
            input = input ?? string.Empty;

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            input = GUILayout.TextField(input, GUILayout.ExpandWidth(true));

            var shouldAdd = GUILayout.Button("+", GUILayout.Width(24f));
            if (!shouldAdd && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                shouldAdd = true;
                Event.current.Use();
            }

            if (shouldAdd)
            {
                var candidate = input.Trim();
                if (!string.IsNullOrEmpty(candidate) && tags != null && (allowDuplicates || !ContainsTag(tags, candidate)))
                {
                    tags.Add(candidate);
                    changed = true;
                }
                input = string.Empty;
            }
            GUILayout.EndHorizontal();

            if (tags == null || tags.Count == 0)
            {
                return changed;
            }

            for (var i = 0; i < tags.Count; i++)
            {
                GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true));
                GUILayout.Label(tags[i], GUILayout.ExpandWidth(true));

                if (GUILayout.Button("x", GUILayout.Width(24f)))
                {
                    tags.RemoveAt(i);
                    changed = true;
                    i--;
                }

                GUILayout.EndHorizontal();
            }

            if (!string.IsNullOrEmpty(key))
            {
                s_searchTexts[key] = input;
            }

            return changed;
        }

        public static int NumericField(string key, string text, int value, int step = 1, int? min = null, int? max = null)
        {
            var fieldText = s_numericTexts.TryGetValue(key, out var cached) ? cached : value.ToString();
            var changedByButtons = false;

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            if (!string.IsNullOrEmpty(text))
            {
                GUILayout.Label(text, GUILayout.Width(120f));
            }

            if (GUILayout.Button("-", GUILayout.Width(24f)))
            {
                value -= Mathf.Max(1, step);
                changedByButtons = true;
            }

            fieldText = GUILayout.TextField(fieldText, GUILayout.Width(90f));

            if (GUILayout.Button("+", GUILayout.Width(24f)))
            {
                value += Mathf.Max(1, step);
                changedByButtons = true;
            }

            GUILayout.EndHorizontal();

            if (changedByButtons)
            {
                value = Clamp(value, min, max);
                fieldText = value.ToString();
            }
            else if (int.TryParse(fieldText, out var parsed))
            {
                value = Clamp(parsed, min, max);
                fieldText = value.ToString();
            }

            s_numericTexts[key] = fieldText;
            return value;
        }

        public static float NumericField(string key, string text, float value, float step = 0.1f, float? min = null, float? max = null)
        {
            var fieldText = s_numericTexts.TryGetValue(key, out var cached) ? cached : value.ToString("0.###");
            var changedByButtons = false;

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            if (!string.IsNullOrEmpty(text))
            {
                GUILayout.Label(text, GUILayout.Width(120f));
            }

            if (GUILayout.Button("-", GUILayout.Width(24f)))
            {
                value -= Mathf.Max(0.0001f, step);
                changedByButtons = true;
            }

            fieldText = GUILayout.TextField(fieldText, GUILayout.Width(90f));

            if (GUILayout.Button("+", GUILayout.Width(24f)))
            {
                value += Mathf.Max(0.0001f, step);
                changedByButtons = true;
            }

            GUILayout.EndHorizontal();

            if (changedByButtons)
            {
                value = Clamp(value, min, max);
                fieldText = value.ToString("0.###");
            }
            else if (float.TryParse(fieldText, out var parsed))
            {
                value = Clamp(parsed, min, max);
                fieldText = value.ToString("0.###");
            }

            s_numericTexts[key] = fieldText;
            return value;
        }

        public static bool RangeSlider(string text, ref float from, ref float to, float minLimit, float maxLimit)
        {
            var oldFrom = from;
            var oldTo = to;

            minLimit = Mathf.Min(minLimit, maxLimit);
            maxLimit = Mathf.Max(minLimit, maxLimit);

            from = Mathf.Clamp(from, minLimit, maxLimit);
            to = Mathf.Clamp(to, minLimit, maxLimit);

            if (from > to)
            {
                from = to;
            }

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            if (!string.IsNullOrEmpty(text))
            {
                GUILayout.Label($"{text}: {from:0.##} - {to:0.##}", GUILayout.ExpandWidth(true));
            }

            from = GUILayout.HorizontalSlider(from, minLimit, to, GUILayout.ExpandWidth(true));
            to = GUILayout.HorizontalSlider(to, from, maxLimit, GUILayout.ExpandWidth(true));

            GUILayout.EndVertical();

            return !Mathf.Approximately(oldFrom, from) || !Mathf.Approximately(oldTo, to);
        }

        public static bool ReorderableList<T>(string title, IList<T> items, System.Func<T, string> getLabel = null)
        {
            if (!string.IsNullOrEmpty(title))
            {
                GUILayout.Label(title, GUI.skin.box, GUILayout.ExpandWidth(true));
            }

            if (items == null)
            {
                HelpBox("List is null", HelpBoxType.Error);
                return false;
            }

            var changed = false;

            for (var i = 0; i < items.Count; i++)
            {
                GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true));

                var label = getLabel != null ? getLabel(items[i]) : (items[i] != null ? items[i].ToString() : "<null>");
                GUILayout.Label(label, GUILayout.ExpandWidth(true));

                var previousEnabled = GUI.enabled;

                GUI.enabled = previousEnabled && i > 0;
                if (GUILayout.Button("↑", GUILayout.Width(24f)))
                {
                    Swap(items, i, i - 1);
                    changed = true;
                }

                GUI.enabled = previousEnabled && i < items.Count - 1;
                if (GUILayout.Button("↓", GUILayout.Width(24f)))
                {
                    Swap(items, i, i + 1);
                    changed = true;
                }

                GUI.enabled = previousEnabled;
                if (GUILayout.Button("x", GUILayout.Width(24f)))
                {
                    items.RemoveAt(i);
                    changed = true;
                    GUILayout.EndHorizontal();
                    i--;
                    continue;
                }

                GUILayout.EndHorizontal();
            }

            return changed;
        }

        public static void MiniStat(string label, string value, Color accent)
        {
            var rect = GUILayoutUtility.GetRect(0f, 42f, GUILayout.ExpandWidth(true));

            DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
            DrawBorder(rect, 1f, new Color(0.08f, 0.08f, 0.08f, 1f));

            var accentRect = new Rect(rect.x, rect.y, 4f, rect.height);
            DrawRect(accentRect, accent);

            var labelRect = new Rect(rect.x + 10f, rect.y + 4f, rect.width - 14f, 16f);
            var valueRect = new Rect(rect.x + 10f, rect.y + 18f, rect.width - 14f, 20f);

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft
            };
            labelStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f, 1f);

            var valueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };

            GUI.Label(labelRect, label ?? string.Empty, labelStyle);
            GUI.Label(valueRect, value ?? string.Empty, valueStyle);
        }

        public static bool AsyncButton(string text, bool isBusy, string busyText = "Loading...", params GUILayoutOption[] options)
        {
            var previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && !isBusy;

            var clicked = GUILayout.Button(isBusy ? busyText : text, options);

            GUI.enabled = previousEnabled;
            return !isBusy && clicked;
        }

        public static void ShowToast(string message, HelpBoxType type = HelpBoxType.Info, float duration = DefaultToastDuration)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            s_toasts.Add(new ToastMessage
            {
                Message = message,
                Type = type,
                ExpiresAt = Time.realtimeSinceStartup + Mathf.Max(0.25f, duration)
            });
        }

        public static void DrawToasts()
        {
            if (s_toasts.Count == 0)
            {
                return;
            }

            var now = Time.realtimeSinceStartup;
            s_toasts.RemoveAll(t => t.ExpiresAt <= now);

            if (s_toasts.Count == 0)
            {
                return;
            }

            for (var i = 0; i < s_toasts.Count; i++)
            {
                var toast = s_toasts[i];
                HelpBox(toast.Message, toast.Type);
            }
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

        private static bool ContainsTag(IList<string> tags, string value)
        {
            for (var i = 0; i < tags.Count; i++)
            {
                if (string.Equals(tags[i], value, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static int Clamp(int value, int? min, int? max)
        {
            if (min.HasValue && value < min.Value)
            {
                value = min.Value;
            }

            if (max.HasValue && value > max.Value)
            {
                value = max.Value;
            }

            return value;
        }

        private static float Clamp(float value, float? min, float? max)
        {
            if (min.HasValue && value < min.Value)
            {
                value = min.Value;
            }

            if (max.HasValue && value > max.Value)
            {
                value = max.Value;
            }

            return value;
        }

        private static void Swap<T>(IList<T> items, int indexA, int indexB)
        {
            var tmp = items[indexA];
            items[indexA] = items[indexB];
            items[indexB] = tmp;
        }

        private static void GetHelpBoxColors(HelpBoxType type, out Color background, out Color border, out Color text)
        {
            switch (type)
            {
                case HelpBoxType.Warning:
                    background = new Color(0.35f, 0.27f, 0.08f, 1f);
                    border = new Color(0.55f, 0.43f, 0.12f, 1f);
                    text = new Color(1f, 0.92f, 0.55f, 1f);
                    break;
                case HelpBoxType.Error:
                    background = new Color(0.35f, 0.1f, 0.1f, 1f);
                    border = new Color(0.55f, 0.18f, 0.18f, 1f);
                    text = new Color(1f, 0.75f, 0.75f, 1f);
                    break;
                case HelpBoxType.Success:
                    background = new Color(0.1f, 0.28f, 0.15f, 1f);
                    border = new Color(0.16f, 0.45f, 0.24f, 1f);
                    text = new Color(0.75f, 1f, 0.8f, 1f);
                    break;
                default:
                    background = new Color(0.12f, 0.2f, 0.3f, 1f);
                    border = new Color(0.2f, 0.35f, 0.52f, 1f);
                    text = new Color(0.8f, 0.9f, 1f, 1f);
                    break;
            }
        }
    }
}
