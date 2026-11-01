using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TitanoTrigger;

public sealed class InputController
{
    private CancellationTokenSource? active;
    private int? heldVirtualKey;
    private uint? heldMouseFlag;

    private const uint InputKeyboard = 1;
    private const uint InputMouse = 0;
    private const uint KeyUp = 0x0002;
    private const uint ScanCode = 0x0008;
    private const uint ExtendedKey = 0x0001;
    private const uint MapVirtualKeyToScanCodeEx = 4;
    private const uint LeftDown = 0x0002;
    private const uint LeftUp = 0x0004;
    private const uint RightDown = 0x0008;
    private const uint RightUp = 0x0010;
    private const uint MiddleDown = 0x0020;
    private const uint MiddleUp = 0x0040;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KEYBDINPUT keyboard;
        [FieldOffset(0)] public MOUSEINPUT mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort virtualKey;
        public ushort scan;
        public uint flags;
        public uint time;
        public UIntPtr extraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint flags;
        public uint time;
        public UIntPtr extraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint count, INPUT[] inputs, int size);
    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint code, uint mapType);

    private static void SendKeyboard(int key, bool down)
    {
        var mapped = MapVirtualKey((uint)key, MapVirtualKeyToScanCodeEx);
        if (mapped == 0) throw new InvalidOperationException("برای این کلید scan code ویندوز پیدا نشد.");
        var scan = (ushort)(mapped & 0xFF);
        var flags = ScanCode | (down ? 0u : KeyUp) | ((mapped & 0xFF00) == 0xE000 ? ExtendedKey : 0u);
        var input = new INPUT
        {
            type = InputKeyboard,
            data = new InputUnion
            {
                keyboard = new KEYBDINPUT
                {
                    virtualKey = 0,
                    scan = scan,
                    flags = flags,
                    extraInfo = UIntPtr.Zero
                }
            }
        };
        Send(input);
    }

    private static void SendMouse(uint flags)
    {
        var input = new INPUT
        {
            type = InputMouse,
            data = new InputUnion { mouse = new MOUSEINPUT { flags = flags, extraInfo = UIntPtr.Zero } }
        };
        Send(input);
    }

    private static void Send(INPUT input)
    {
        if (SendInput(1, [input], Marshal.SizeOf<INPUT>()) != 1)
        {
            var error = Marshal.GetLastWin32Error();
            var extra = error == 0
                ? "ویندوز ورودی را نپذیرفت. اگر برنامهٔ مقصد با دسترسی مدیر اجرا شده، این ابزار را هم با همان سطح دسترسی باز کنید."
                : $"ویندوز ورودی را نپذیرفت (کد {error}). اگر برنامهٔ مقصد با دسترسی مدیر اجرا شده، این ابزار را هم با همان سطح دسترسی باز کنید.";
            throw new InvalidOperationException(extra);
        }
    }

    private static (int? virtualKey, uint? downFlag, uint? upFlag) Resolve(string key) => key switch
    {
        "MOUSE_LEFT" => (null, LeftDown, LeftUp),
        "MOUSE_RIGHT" => (null, RightDown, RightUp),
        "MOUSE_MIDDLE" => (null, MiddleDown, MiddleUp),
        _ when RuleService.Keys.TryGetValue(key, out var virtualKey) => (virtualKey, null, null),
        _ => throw new InvalidOperationException("کلید مجاز نیست.")
    };

    private static void Down(int? virtualKey, uint? mouseFlag)
    {
        if (virtualKey is int key) SendKeyboard(key, true);
        else if (mouseFlag is uint flag) SendMouse(flag);
    }

    private static void Up(int? virtualKey, uint? mouseFlag)
    {
        if (virtualKey is int key) SendKeyboard(key, false);
        else if (mouseFlag is uint flag)
            SendMouse(flag switch { LeftDown => LeftUp, RightDown => RightUp, MiddleDown => MiddleUp, _ => throw new InvalidOperationException("دکمهٔ ماوس مجاز نیست.") });
    }

    public async Task Execute(Rule rule, CancellationToken outer)
    {
        if (active is not null) throw new InvalidOperationException("یک فرمان در حال اجراست.");
        var (virtualKey, mouseDown, _) = Resolve(rule.Key);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(outer);
        active = linked;
        try
        {
            Down(virtualKey, mouseDown);
            heldVirtualKey = virtualKey;
            heldMouseFlag = mouseDown;
            var started = Stopwatch.GetTimestamp();
            var targetTicks = (long)Math.Ceiling(rule.Duration * (double)Stopwatch.Frequency / 1000d);
            while (Stopwatch.GetTimestamp() - started < targetTicks)
            {
                var remainingTicks = targetTicks - (Stopwatch.GetTimestamp() - started);
                var remainingMs = Math.Max(1, (int)Math.Ceiling(remainingTicks * 1000d / Stopwatch.Frequency));
                await Task.Delay(Math.Min(remainingMs, 100), linked.Token);
            }
        }
        finally
        {
            try
            {
                if (heldVirtualKey is not null || heldMouseFlag is not null)
                    Up(heldVirtualKey, heldMouseFlag);
            }
            finally
            {
                heldVirtualKey = null;
                heldMouseFlag = null;
                active = null;
            }
        }
    }

    public async Task ExecuteTest(Rule rule)
    {
        // The target app is usually behind this window. Minimize first so Windows
        // restores the target as foreground before sending the test input.
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (System.Windows.Application.Current.MainWindow is { } window) window.WindowState = System.Windows.WindowState.Minimized;
        });
        await Task.Delay(250);
        await Execute(rule, CancellationToken.None);
    }

    public void Stop()
    {
        active?.Cancel();
        if (heldVirtualKey is not null || heldMouseFlag is not null)
        {
            try { Up(heldVirtualKey, heldMouseFlag); } catch { }
            heldVirtualKey = null;
            heldMouseFlag = null;
        }
    }
}
