using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Web.WebView2.Core;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace TitanoTrigger;

public partial class MainWindow : Window
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };
    private readonly AppController controller;
    private bool webReady;
    private IntPtr windowHandle;
    private bool shortcutRegistered;
    private bool allowExit;
    private bool englishUi = LoadEnglishPreference();
    private Forms.NotifyIcon? trayIcon;
    private Forms.ToolStripMenuItem? openTrayItem;
    private Forms.ToolStripMenuItem? exitTrayItem;
    private readonly bool smoke = Environment.GetCommandLineArgs().Contains("--smoke");
    private static string LanguagePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TitanoTrigger", "language.txt");

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint virtualKey);
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public MainWindow()
    {
        InitializeComponent();
        if (smoke) { ShowInTaskbar = false; WindowState = WindowState.Minimized; }
        controller = new AppController(Publish);
        ApplyShellLanguage();
        SourceInitialized += (_, _) =>
        {
            windowHandle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(windowHandle)?.AddHook(WindowMessage);
            shortcutRegistered = RegisterHotKey(windowHandle, 1, 0x0002 | 0x0004, 0x7B);
        };
        Loaded += (_, _) => InitializeTrayIcon();
        Loaded += async (_, _) => await InitializeBrowser();
        Closing += OnClosing;
        Closed += (_, _) =>
        {
            controller.Stop("برنامه بسته شد.");
            if (trayIcon is not null) { trayIcon.Visible = false; trayIcon.Dispose(); trayIcon = null; }
            if (shortcutRegistered) UnregisterHotKey(windowHandle, 1);
        };
    }

    void InitializeTrayIcon()
    {
        if (smoke || trayIcon is not null) return;
        var menu = new Forms.ContextMenuStrip();
        openTrayItem = new Forms.ToolStripMenuItem();
        exitTrayItem = new Forms.ToolStripMenuItem();
        openTrayItem.Click += (_, _) => RestoreFromTray();
        exitTrayItem.Click += (_, _) => ExitApplication();
        menu.Items.Add(openTrayItem);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(exitTrayItem);
        trayIcon = new Forms.NotifyIcon
        {
            Text = "Titano Trigger — آمادهٔ پایش",
            Icon = Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? Drawing.SystemIcons.Application,
            ContextMenuStrip = menu
        };
        trayIcon.DoubleClick += (_, _) => RestoreFromTray();
        ApplyShellLanguage();
    }

    private static bool LoadEnglishPreference()
    {
        try { return File.ReadAllText(LanguagePath).Trim().Equals("en", StringComparison.OrdinalIgnoreCase); }
        catch { return false; }
    }

    private string SetLanguage(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.String)
        {
            englishUi = value.GetString()?.Equals("en", StringComparison.OrdinalIgnoreCase) == true;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LanguagePath)!);
                File.WriteAllText(LanguagePath, englishUi ? "en" : "fa");
            }
            catch { }
            ApplyShellLanguage();
        }
        return englishUi ? "en" : "fa";
    }

    private void ApplyShellLanguage()
    {
        if (MinimizeButton is null || MaximizeButton is null || CloseButton is null) return;
        MinimizeButton.ToolTip = englishUi ? "Minimize" : "کوچک‌کردن";
        MaximizeButton.ToolTip = WindowState == WindowState.Maximized
            ? englishUi ? "Restore" : "بازگردانی"
            : englishUi ? "Maximize" : "بزرگ‌نمایی";
        CloseButton.ToolTip = englishUi ? "Minimize to system tray" : "کوچک‌کردن به سینی سیستم";
        if (openTrayItem is not null) openTrayItem.Text = englishUi ? "Open Titano Trigger" : "باز کردن Titano Trigger";
        if (exitTrayItem is not null) exitTrayItem.Text = englishUi ? "Exit" : "خروج کامل";
        if (trayIcon is not null)
            trayIcon.Text = englishUi
                ? controller.State().Armed ? "Titano Trigger — live control active" : "Titano Trigger — ready in background"
                : controller.State().Armed ? "Titano Trigger — پایش دونیت فعال است" : "Titano Trigger — در پس‌زمینه آماده است";
    }

    void OnClosing(object? sender, System.ComponentModel.CancelEventArgs args)
    {
        if (allowExit || smoke) return;
        args.Cancel = true;
        MinimizeToTray();
    }

    void MinimizeToTray()
    {
        if (trayIcon is null) InitializeTrayIcon();
        ShowInTaskbar = false;
        Hide();
        if (trayIcon is not null)
        {
            ApplyShellLanguage();
            trayIcon.Visible = true;
            trayIcon.ShowBalloonTip(1800, "Titano Trigger", englishUi
                ? "The app is still running. Double-click the tray icon to reopen it."
                : "برنامه در پس‌زمینه باز می‌ماند. برای بازکردن، روی آیکن کنار ساعت دوبار کلیک کنید.", Forms.ToolTipIcon.Info);
        }
    }

    void RestoreFromTray()
    {
        if (trayIcon is not null) trayIcon.Visible = false;
        ShowInTaskbar = true;
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    void ExitApplication()
    {
        allowExit = true;
        trayIcon?.Dispose();
        trayIcon = null;
        controller.Stop("برنامه بسته شد.");
        Close();
    }

    void Minimize_Click(object sender, RoutedEventArgs args) => WindowState = WindowState.Minimized;

    void Maximize_Click(object sender, RoutedEventArgs args) => ToggleMaximize();

    void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        MaximizeButton.Content = WindowState == WindowState.Maximized ? "▢" : "□";
        MaximizeButton.ToolTip = WindowState == WindowState.Maximized
            ? englishUi ? "Restore" : "بازگردانی"
            : englishUi ? "Maximize" : "بزرگ‌نمایی";
    }

    void CloseToTray_Click(object sender, RoutedEventArgs args) => MinimizeToTray();

    void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs args)
    {
        if (args.ClickCount == 2) ToggleMaximize();
        else if (args.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    IntPtr WindowMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == 0x0312 && wParam == 1)
        {
            controller.Stop("توقف اضطراری با میانبر انجام شد.");
            handled = true;
        }
        return IntPtr.Zero;
    }

    async Task InitializeBrowser()
    {
        try
        {
            var profile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TitanoTrigger", "WebView2");
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: profile);
            await Browser.EnsureCoreWebView2Async(environment);
            var core = Browser.CoreWebView2;
            core.Settings.AreDevToolsEnabled = false;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreHostObjectsAllowed = false;
            core.Settings.IsWebMessageEnabled = true;
            core.SetVirtualHostNameToFolderMapping("titano.local", Path.Combine(AppContext.BaseDirectory, "ui"), CoreWebView2HostResourceAccessKind.DenyCors);
            core.NavigationStarting += (_, args) =>
            {
                if (!args.Uri.StartsWith("https://titano.local/", StringComparison.OrdinalIgnoreCase)) args.Cancel = true;
            };
            core.NewWindowRequested += (_, args) => args.Handled = true;
            core.WebMessageReceived += OnWebMessage;
            if (smoke) core.NavigationCompleted += async (_, args) =>
            {
                var ready = args.IsSuccess && await Browser.ExecuteScriptAsync("Boolean(window.titano && document.getElementById('armButton') && document.querySelector('.hero-card'))") == "true" && RuleChecksPass();
                Environment.ExitCode = ready ? 0 : 2;
                Close();
            };
            webReady = true;
            core.Navigate("https://titano.local/index.html");
        }
        catch (Exception error)
        {
            if (smoke) { Environment.ExitCode = 2; Close(); return; }
            var message = englishUi
                ? $"Could not start WebView2. Install the Microsoft Edge WebView2 Runtime.\n\n{error.Message}"
                : $"راه‌اندازی WebView2 ناموفق بود. نسخهٔ Microsoft Edge WebView2 Runtime را نصب کنید.\n\n{error.Message}";
            System.Windows.MessageBox.Show(message, "Titano Trigger", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    void Publish(ViewState state)
    {
        if (!webReady) return;
        Browser.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { type = "state", value = state }, JsonOptions));
    }

    async void OnWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        if (!args.Source.StartsWith("https://titano.local/", StringComparison.OrdinalIgnoreCase)) return;
        int id = 0;
        try
        {
            using var document = JsonDocument.Parse(args.WebMessageAsJson);
            var root = document.RootElement;
            id = root.GetProperty("id").GetInt32();
            var op = root.GetProperty("op").GetString();
            root.TryGetProperty("value", out var value);
            object? result = op switch
            {
                "state" => controller.State(),
                "language" => SetLanguage(value),
                "connect" => await controller.Connect(value.GetString() ?? ""),
                "disconnect" => controller.Disconnect(),
                "rules" => controller.SetRules(JsonSerializer.Deserialize<List<Rule>>(value.GetRawText(), JsonOptions)),
                "start" => await controller.Start(),
                "stop" => controller.Stop(),
                "test" => await controller.Test(value.GetString() ?? ""),
                "link" => OpenLink(value.GetString() ?? ""),
                _ => throw new InvalidOperationException("درخواست ناشناخته است.")
            };
            Browser.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { type = "response", id, value = result }, JsonOptions));
        }
        catch (Exception error)
        {
            var safeMessage = error is HttpRequestException or TaskCanceledException ? "ارتباط با ریمیت برقرار نشد؛ اتصال اینترنت را بررسی کنید." : error.Message;
            Browser.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { type = "response", id, error = safeMessage }, JsonOptions));
        }
    }

    static object? OpenLink(string service)
    {
        var url = service switch
        {
            "instagram" => "https://instagram.com/kiatitano",
            "youtube" => "https://youtube.com/@kiatitano",
            "reymit" => "https://reymit.ir/APIs",
            _ => null
        };
        if (url is not null) Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        return null;
    }

    static bool RuleChecksPass()
    {
        var valid = RuleService.Validate([new Rule("a", 100, 200, 500, "F1")]);
        if (RuleService.Match(valid, 150)?.Key != "F1" || RuleService.Match(valid, 201) is not null) return false;
        try { RuleService.Validate([.. valid, new Rule("b", 200, 300, 500, "A")]); return false; }
        catch (InvalidOperationException) { return true; }
    }
}
