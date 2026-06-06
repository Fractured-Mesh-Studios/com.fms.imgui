# IMGUI Window System

A lightweight Unity package for building draggable, resizable IMGUI-based windows with optional input toggling, persistence, mobile scaling, and reusable child window components.

## Documentation

- [`GUILayoutExtended` reference](./GUILayoutExtended.md)

## Namespace

- Runtime: `GuiEngine`
- Editor: `GuiEditor`

## Package contents

### Runtime

- `Window` — Core window component that renders a `GUI.Window`, supports drag, resize, clamp, styling, and persistence.
- `ToggleWindow` — Toggles a `Window` using a Unity `InputAction`.
- `WindowCloseButton` — Adds a close button inside a window.
- `WindowEvent` — Exposes IMGUI mouse events while the window is open.
- `WindowMobile` — Applies a GUI scale matrix for mobile-oriented layouts.

### Editor

- `WindowEditor` — Custom inspector for easier configuration.

---

## Overview

`Window` is the central component. Attach it to a `GameObject` and it will render an IMGUI window during `OnGUI()`.

The window can:

- open and close programmatically,
- be moved by dragging a configurable header area,
- be resized from the bottom-right corner,
- clamp its movement to the screen bounds,
- persist position and size with `PlayerPrefs`,
- render child components that implement `IWindow`.

---

## Requirements

- Unity with IMGUI support enabled.
- The package uses the **Input System** (`UnityEngine.InputSystem`).
- C# 9.0 / `.NET Standard 2.1`.

---

## Quick start

1. Create a `GameObject` in the scene.
2. Add the `Window` component.
3. Configure the window in the Inspector.
4. Enable `drag` and/or `resize` if desired.
5. Call `Open()` from code, or add `ToggleWindow` to toggle it via input.

Example:

```csharp
using GuiEngine;
using UnityEngine;

public class ExampleWindowSpawner : MonoBehaviour
{
    [SerializeField] private Window window;

    private void Start()
    {
        window.Open();
    }
}
```

---

## Core component: `Window`

### Main inspector fields

- `title` — Window title shown in the IMGUI header.
- `tooltip` — Tooltip for the window title.
- `icon` — Optional title icon.
- `resizeIcon` — Optional resize indicator icon.
- `drag` — Enables dragging.
- `resize` — Enables resizing.
- `clamp` — Prevents the window from leaving the screen while dragging.
- `color` — Window tint.
- `opacity` — Window transparency.
- `dragSize` — Size of the draggable header region.
- `rect` — Position and size of the window.

### Runtime API

- `Open()` — Opens the window.
- `Close()` — Closes the window.
- `Toggle()` — Toggles the open state.
- `isOpen` — Returns the current visibility state.
- `width` / `height` — Current size in pixels.

### Open/close callbacks

`Window` exposes two events:

- `onOpen`
- `onClose`

They receive the current open state as a `bool`.

Example:

```csharp
window.onOpen += isOpen => Debug.Log($"Window opened: {isOpen}");
window.onClose += isOpen => Debug.Log($"Window closed: {isOpen}");
```

---

## Persistence with `PlayerPrefs`

The package can save and restore window position and size automatically.

### What gets saved

- `rect.x`
- `rect.y`
- `rect.width`
- `rect.height`

### When it saves

The window state is saved:

- when the window is closed,
- when the component is disabled,
- after drag or resize ends.

### How the key is built

By default, the persistence key is human-readable and scoped per scene and window:

`GuiEngine.<SceneName>.<WindowName>`

Then the system appends:

- `.x`
- `.y`
- `.width`
- `.height`

Example:

- `GuiEngine.MirrorBasic.IMGUI_Example.x`
- `GuiEngine.MirrorBasic.IMGUI_Example.width`

### Custom key

If you need full control, set `m_playerPrefsKey` in the component data. This is useful when:

- two windows share the same title,
- you want to migrate old saved layouts,
- you want a custom storage key.

> Note: the field is hidden in the Inspector, so it is typically configured from code or via serialization tooling.

---

## Drag and resize behavior

### Dragging

When `drag` is enabled, the top region defined by `dragSize` becomes the draggable area.

- If the pointer starts inside that area, the window can be moved.
- If `clamp` is enabled, movement is constrained to the screen.

### Resizing

When `resize` is enabled, the window can be resized from the bottom-right corner.

- `resizeIcon` is drawn inside the resize handle area.
- Width and height are clamped to a safe range based on the current screen size.

---

## Adding controls inside the window

Any child component that implements `IWindow` can render inside the window.

```csharp
namespace GuiEngine
{
    public class ExampleLabel : MonoBehaviour, IWindow
    {
        public void OnWindowGUI(int id)
        {
            GUILayout.Label("Hello from a child component");
        }
    }
}
```

Attach the script to a child object of the same window hierarchy. The parent `Window` automatically finds `IWindow` children in `OnEnable()`.

---

## Close button: `WindowCloseButton`

`WindowCloseButton` adds a simple close button to the window content.

### Usage

1. Add `WindowCloseButton` to a child object under the `Window` hierarchy.
2. Configure its appearance:
   - `color`
   - `text`
   - `size`
   - `offset`
3. During rendering, it draws a button using the parent window size.
4. Clicking it calls `window.Close()`.

This component requires a parent `Window`.

---

## Window input events: `WindowEvent`

`WindowEvent` exposes IMGUI pointer events while the window is open.

### Provided UnityEvents

- `onMouseDown`
- `onMouseUp`
- `onMouseEnter`
- `onMouseExit`
- `onMouseDrag`

### Typical use cases

- play sounds on click,
- highlight controls on hover,
- trigger custom behaviors when the pointer enters the window area,
- react to drag gestures.

### Example

```csharp
using GuiEngine;
using UnityEngine;

public class ExampleWindowEvents : WindowEvent
{
    protected override void OnMouseEnterEvent()
    {
        Debug.Log("Pointer entered the window.");
    }
}
```

The component only evaluates events while the parent window is open.

---

## Input toggle: `ToggleWindow`

`ToggleWindow` is a convenience component that opens or closes the associated `Window` using a Unity `InputAction`.

### Default behavior

If no binding is configured, the component restores a default binding to:

- `Keyboard Space`

### Setup

1. Add `ToggleWindow` to the same `GameObject` as `Window`.
2. Assign or edit the `OnToggle` input action.
3. Press the bound key or action to toggle the window.

### Example

The component listens to the `performed` callback and calls:

- `window.Open()` when closed,
- `window.Close()` when open.

---

## Mobile scaling: `WindowMobile`

`WindowMobile` is an optional helper for projects that design the UI at a fixed reference resolution.

### Behavior

It temporarily scales `GUI.matrix` before drawing the window and restores it afterward.

Default reference resolution:

- `1080 x 1920`

### Use case

Use this when you want IMGUI windows to visually match a mobile-first layout.

### Important

Because this component changes `GUI.matrix`, it should be used carefully if other IMGUI systems also draw in the same frame.

---

## Custom inspector: `WindowEditor`

The package includes a custom editor that organizes the most relevant properties into sections:

- Header
- Transform
- Render

This improves editing workflow and keeps the inspector cleaner for package users.

---

## Recommended setup patterns

### Simple window

- Add `Window`
- Set `title`
- Enable `drag`
- Optionally enable `resize`
- Call `Open()` from code

### Persistent window

- Keep `m_persistLayout` enabled
- Use a unique `title` per window
- If needed, assign a custom `m_playerPrefsKey`

### Interactive window

- Add child components that implement `IWindow`
- Add `WindowEvent` for input reactions
- Add `WindowCloseButton` if you want an in-window close control

---

## Notes and best practices

- Keep window titles unique within the same scene when using automatic persistence.
- Use `clamp` for HUD-style windows that should remain visible.
- Avoid overlapping `dragSize` with interactive controls unless you intentionally want those controls to act as the drag area.
- If you are combining this system with other IMGUI layers, test the order of drawing carefully.

---

## File summary

- `Packages/com.fms.imgui/Runtime/Window.cs`
- `Packages/com.fms.imgui/Runtime/ToggleWindow.cs`
- `Packages/com.fms.imgui/Runtime/WindowCloseButton.cs`
- `Packages/com.fms.imgui/Runtime/WindowEvent.cs`
- `Packages/com.fms.imgui/Runtime/WindowMobile.cs`
- `Packages/com.fms.imgui/Editor/WindowEditor.cs`

---

## License

Add your package license here if applicable.
