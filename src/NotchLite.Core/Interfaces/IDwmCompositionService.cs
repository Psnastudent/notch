namespace NotchLite.Core.Interfaces;

/// <summary>
/// Controls DWM composition effects (blur, transparency, acrylic/mica) on a window.
/// All <see cref="nint"/> parameters represent window handles (HWND).
/// </summary>
public interface IDwmCompositionService
{
    /// <summary>Enables blur-behind effect on the specified window.</summary>
    /// <param name="hwnd">The native window handle.</param>
    /// <returns><c>true</c> if the effect was applied successfully.</returns>
    bool EnableBlurBehind(nint hwnd);

    /// <summary>Sets the opacity (0 = fully transparent, 255 = fully opaque) on the specified window.</summary>
    /// <param name="hwnd">The native window handle.</param>
    /// <param name="opacity">Opacity level (0–255).</param>
    /// <returns><c>true</c> if the transparency was applied successfully.</returns>
    bool SetWindowTransparency(nint hwnd, byte opacity);

    /// <summary>Gets whether DWM desktop composition is currently enabled.</summary>
    bool IsCompositionEnabled { get; }
}
