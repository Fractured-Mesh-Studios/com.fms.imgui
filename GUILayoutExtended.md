# IMGUI `GUILayoutExtended`

Utilities for building quick UI with Unity IMGUI.

## Available controls

### Existing
- `Foldout(string text, bool value)`
- `ProgressBar(float value, float maxValue = 1f)`
- `ProgressBar(string text, float value, float maxValue = 1f)`
- `ProgressBar(float value, float maxValue, Color fillColor, Color backgroundColor, Color borderColor, float borderThickness = 1f, TextAnchor textAlignment = TextAnchor.MiddleCenter)`
- `ProgressBar(string text, float value, float maxValue, Color fillColor, Color backgroundColor, Color borderColor, float borderThickness = 1f, TextAnchor textAlignment = TextAnchor.MiddleCenter)`
- `Enum<T>(string text, T value) where T : System.Enum`
- `Enum<T>(string text, T value, string[] options) where T : System.Enum`

### New
- `SectionHeader(string title, string subtitle = null)`
- `Separator(float thickness = 1f, Color? color = null, float padding = 4f)`
- `HelpBox(string message, GUILayoutExtended.HelpBoxType type)`
- `SearchField(string key, string value, string placeholder = "Search...")`
- `Toolbar(string key, int selected, string[] tabs)`
- `TagSelector(string key, IList<string> tags, ref string input, bool allowDuplicates = false)`
- `NumericField(string key, string text, int value, int step = 1, int? min = null, int? max = null)`
- `NumericField(string key, string text, float value, float step = 0.1f, float? min = null, float? max = null)`
- `RangeSlider(string text, ref float from, ref float to, float minLimit, float maxLimit)`
- `ReorderableList<T>(string title, IList<T> items, System.Func<T, string> getLabel = null)`
- `MiniStat(string label, string value, Color accent)`
- `AsyncButton(string text, bool isBusy, string busyText = "Loading...", params GUILayoutOption[] options)`
- `ShowToast(string message, GUILayoutExtended.HelpBoxType type = GUILayoutExtended.HelpBoxType.Info, float duration = 2.5f)`
- `DrawToasts()`

## `HelpBoxType`
- `Info`
- `Warning`
- `Error`
- `Success`

## Quick example

```csharp
using System.Collections.Generic;
using GuiEngine;
using UnityEngine;

public class GuiExample : MonoBehaviour
{
    private string search = string.Empty;
    private int tab;
    private int players = 8;
    private float minLevel = 1f;
    private float maxLevel = 20f;
    private string tagInput = string.Empty;
    private readonly List<string> tags = new List<string>();

    private void OnGUI()
    {
        GUILayoutExtended.SectionHeader("Lobby", "Control demo");

        search = GUILayoutExtended.SearchField("demo-search", search, "Search...");
        tab = GUILayoutExtended.Toolbar("demo-tabs", tab, new[] { "Overview", "Servers", "Players" });

        GUILayoutExtended.HelpBox("Configuration loaded", GUILayoutExtended.HelpBoxType.Success);

        GUILayoutExtended.TagSelector("demo-tags", tags, ref tagInput);
        players = GUILayoutExtended.NumericField("demo-players", "Players", players, 1, 1, 64);

        GUILayoutExtended.RangeSlider("Level", ref minLevel, ref maxLevel, 1f, 60f);

        if (GUILayoutExtended.AsyncButton("Connect", false))
        {
            GUILayoutExtended.ShowToast("Connected", GUILayoutExtended.HelpBoxType.Success);
        }

        GUILayoutExtended.DrawToasts();
    }
}
```

## Note
If used inside an IMGUI window (`GUI.Window`), call `DrawToasts()` inside that window's draw flow.
