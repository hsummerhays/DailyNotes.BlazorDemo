using Microsoft.JSInterop;

namespace DailyNotes.Blazor.Services;

public enum AppTheme
{
    Light,
    Dark,
    Device
}

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private AppTheme _currentTheme = AppTheme.Device;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public event Action? OnThemeChanged;

    public AppTheme CurrentTheme => _currentTheme;

    public async Task InitializeAsync()
    {
        var themeString = await _jsRuntime.InvokeAsync<string>("themeManager.getTheme");
        if (Enum.TryParse<AppTheme>(themeString, true, out var theme))
        {
            _currentTheme = theme;
        }
    }

    public async Task SetThemeAsync(AppTheme theme)
    {
        _currentTheme = theme;
        await _jsRuntime.InvokeVoidAsync("themeManager.setTheme", theme.ToString().ToLower());
        OnThemeChanged?.Invoke();
    }
}
