# Nectar Architecture & Design Specification: Reverse Engineering Blueprint

This document contains the complete technical blueprint, mathematical models, UI/UX aesthetics, animation curves, and Windows API integration patterns reverse-engineered from [kua8/Nectar](https://github.com/kua8/Nectar) (v1.10.0).

---

## 1. Visual Silhouette & The Organic Flare Geometry

The primary reason standard UI overlays look artificial is because they use rectangular rounded corners (`border-radius`) that float disconnected from the monitor bezel. Nectar solves this mathematically via **concave flares** where the notch meets the physical top bezel.

### The Mathematical Silhouette Path (`SurfaceShape.tsx`)
A single continuous SVG path generates both the clipping mask and the specular rim highlight:

```
Silhouette Path Definition for Top Notch:
M (x = -flare, y = 0)
A (rx = flare, ry = flare, 0, 0, 1, x = inset, y = flare)           <-- Concave Left Flare
V (y = height - cornerRadius)                                       <-- Left Vertical Wall
A (rx = radius, ry = radius, 0, 0, 0, x = cornerRadius, y = height) <-- Convex Bottom-Left Corner
H (x = width - cornerRadius)                                        <-- Flat Bottom Edge
A (rx = radius, ry = radius, 0, 0, 0, x = width - inset, y = h - r) <-- Convex Bottom-Right Corner
V (y = flare)                                                       <-- Right Vertical Wall
A (rx = flare, ry = flare, 0, 0, 1, x = width + flare, y = 0)       <-- Concave Right Flare
```

### Key Parameters:
- **`flare`**: `12px` to `18px` concave arc blending the notch directly into the physical screen frame.
- **`radius` (bottom corners)**: `18px` to `22px` rounded convex corners.
- **`inset`**: `0.5px` stroke alignment for sharp, sub-pixel specular borders (`--surface-outline`).

---

## 2. Glassmorphism, Specular Lighting & Wallpaper Blur

Nectar achieves realistic macOS glass without degrading framerates by layering three distinct rendering planes:

1. **Wallpaper Backdrop Sampling (`backdrop.ts` & `wallpaper.rs`)**:
   - In transparent windows (`WS_EX_LAYERED`), standard CSS `backdrop-filter: blur(...)` cannot sample desktop contents underneath.
   - Nectar queries the active Windows desktop wallpaper file path via registry (`HKEY_CURRENT_USER\Control Panel\Desktop\Wallpaper`).
   - Renders a localized crop of the wallpaper behind the notch/dock, blurred with high-pass Gaussian filters (`filter: blur(28px) saturate(140%) brightness(0.85)`).
2. **Glass Fill Layer (`.surface-fill`)**:
   - Background: `rgba(18, 19, 23, 0.72)` in Dark mode; `rgba(240, 240, 245, 0.75)` in Light mode.
   - Ambient gradient sheen: Linear top-to-bottom specular wash (`linear-gradient(180deg, rgba(255,255,255,0.08) 0%, rgba(255,255,255,0.01) 100%)`).
3. **Specular Rim Highlight (`.edge-outline`)**:
   - `stroke`: `rgba(255, 255, 255, 0.20)` at the top, fading to `rgba(255, 255, 255, 0.08)` along the base.
   - `stroke-width`: `1px`.
   - Drop shadow: `box-shadow: 0 16px 36px rgba(0, 0, 0, 0.55), 0 4px 12px rgba(0, 0, 0, 0.35)`.

---

## 3. Motion & Spring Physics Architecture

All animations use Framer Motion physics calibrated to match Apple's iOS / macOS Spring dynamics:

### Spring Physics Presets:
- **Island Expansion / Contraction**:
  ```ts
  transition: {
    type: "spring",
    stiffness: 340,
    damping: 30,
    mass: 0.8
  }
  ```
- **Tab / Content Crossfade**:
  ```ts
  transition: {
    duration: 0.22,
    ease: [0.16, 1, 0.3, 1] // Custom Apple Quintic Ease-Out
  }
  ```
- **Dock Icon Hover Magnification**:
  ```ts
  transition: {
    type: "spring",
    stiffness: 450,
    damping: 24,
    mass: 0.5
  }
  ```

---

## 4. Dock Magnification & Live Window Previews

### The Parabolic Magnification Formula (`Dock.tsx`)
When the cursor moves across the dock:
$$\text{dist} = |x_{\text{mouse}} - x_{\text{icon\_center}}|$$
$$\text{If } \text{dist} < \text{maxDist } (80\text{px}):$$
$$\text{scale} = 1.0 + (\text{scaleMax} - 1.0) \times \cos\left(\frac{\text{dist}}{\text{maxDist}} \times \frac{\pi}{2}\right)$$
$$\text{Where } \text{scaleMax} = 1.45 \quad (\text{i.e. } +45\% \text{ size increase at center}).$$

### Live Window Thumbnail Previews (`capture.rs`):
- Uses Windows 10/11 `Windows.Graphics.Capture.IGraphicsCaptureItemInterop`.
- Direct3D 11 swapchain frame pool: `DirectXPixelFormat::B8G8R8A8UIntNormalized`.
- Encodes frames via WIC (`WICBitmapEncoderNoCache`) directly into memory streams without writing temp files to disk.

---

## 5. System Hardware & Windows API Bridges (`src-tauri/src/services.rs`)

1. **Physical Laptop Backlight Brightness**:
   - **WMI Service**: `root\wmi:WmiMonitorBrightnessMethods` -> `WmiSetBrightness(Timeout: 1, Brightness: %)`.
   - Polling thread reads `root\wmi:WmiMonitorBrightness` -> `CurrentBrightness`.
2. **Master System Volume & Mute**:
   - Win32 CoreAudio: `IMMDeviceEnumerator` -> `eRender, eMultimedia` -> `IAudioEndpointVolume`.
   - Scalar volume tracking with `IAudioEndpointVolumeCallback` for instant physical keyboard volume key sync.
3. **Media Transport Session (GSMTC)**:
   - `GlobalSystemMediaTransportControlsSessionManager`:
     - Tracks active media sessions across Spotify, Edge, Chrome, Apple Music.
     - Extract artist, track title, seek position (`position_ms`), track duration (`duration_ms`), and thumbnail artwork streams.
4. **Window Foreground & Fullscreen Monitoring**:
   - Hook: `GetForegroundWindow()`, `GetWindowRect()`, `DwmGetWindowAttribute(DWMWA_EXTENDED_FRAME_BOUNDS)`.
   - Fullscreen detection logic: Matches client or window rect against `GetMonitorInfo()` to prevent window collisions.

---

## 6. Blueprint for Exact Reproduction

To achieve 100% parity with Nectar:
1. **Host Engine**: Run on **Tauri 2.0 (Rust) + React 18 + Vite + Tailwind + Framer Motion**.
2. **SVG Flare Silhouette**: Implement `SurfaceShape.tsx` with dynamic size observer for both Top Notch and Bottom Dock.
3. **Hardware Modules**: Connect Rust backends for `IAudioEndpointVolume`, WMI Backlight, and GSMTC.
4. **Dock Interaction**: Keep dock mode in `Fixed` or allow dynamic un-hiding to guarantee accessibility across screen recording sessions.
