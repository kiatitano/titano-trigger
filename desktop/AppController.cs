using System.IO;
using System.Text.Json;

namespace TitanoTrigger;

public sealed class AppController
{
    private readonly ReymitClient reymit = new();
    private readonly InputController input = new();
    private readonly Action<ViewState> publish;
    private readonly string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TitanoTrigger", "settings.json");
    private string token = "";
    private string account = "";
    private List<Rule> rules = [];
    private List<Activity> history = [];
    private HashSet<string> seen = [];
    private readonly Queue<(Rule rule, Donation donation)> queue = new();
    private bool armed;
    private bool polling;
    private bool processing;
    private bool starting;
    private double armedAt;
    private long? lastPoll;
    private string status = "برای شروع، توکن ریمیت را وصل کنید.";
    private CancellationTokenSource? runCancellation;
    private int generation;

    public AppController(Action<ViewState> publish)
    {
        this.publish = publish;
        try
        {
            if (File.Exists(settingsPath))
                rules = RuleService.Validate(JsonSerializer.Deserialize<List<Rule>>(File.ReadAllText(settingsPath), MainWindow.JsonOptions) ?? []);
        }
        catch { rules = []; }
    }

    public ViewState State() => new()
    {
        Connected = token.Length > 0, Account = account, Armed = armed, Rules = [.. rules], History = [.. history],
        LastPoll = lastPoll, Status = status, Polling = polling, QueueSize = queue.Count
    };
    void Publish() => publish(State());
    void Log(string kind, string message, long? amount = null)
    {
        history.Insert(0, new Activity(Guid.NewGuid().ToString("N"), kind, message, amount, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        if (history.Count > 30) history.RemoveRange(30, history.Count - 30);
        Publish();
    }

    public async Task<ViewState> Connect(string candidate)
    {
        Stop("در حال بررسی اتصال...");
        token = ""; account = ""; seen.Clear(); Publish();
        candidate = candidate.Trim();
        var name = await reymit.Verify(candidate);
        var baseline = await reymit.Recent(candidate);
        token = candidate; account = name;
        seen = baseline.Select(item => item.Id).ToHashSet();
        status = "ریمیت متصل است. برای اجرا، کنترل را روشن کنید.";
        Log("info", $"اتصال ریمیت برای {account} تأیید شد.");
        return State();
    }

    public ViewState Disconnect()
    {
        Stop("ریمیت قطع شد."); token = ""; account = ""; seen.Clear(); Publish(); return State();
    }

    public ViewState SetRules(List<Rule>? inputRules)
    {
        if (armed || starting) throw new InvalidOperationException("برای ویرایش قوانین، کنترل را متوقف کنید.");
        var validated = RuleService.Validate(inputRules);
        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
        File.WriteAllText(settingsPath, JsonSerializer.Serialize(validated, MainWindow.JsonOptions));
        rules = validated; status = "قوانین ذخیره شدند."; Publish(); return State();
    }

    public async Task<ViewState> Start()
    {
        if (token.Length == 0) throw new InvalidOperationException("ابتدا ریمیت را وصل کنید.");
        if (rules.Count == 0) throw new InvalidOperationException("حداقل یک قانون تعریف کنید.");
        if (armed || starting) return State();
        starting = true;
        var thisGeneration = ++generation;
        try
        {
            var startedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
            var baseline = await reymit.Recent(token);
            if (thisGeneration != generation) return State();
            seen = baseline.Where(item => item.Time < startedAt).Select(item => item.Id).ToHashSet();
            armedAt = startedAt;
            armed = true;
            runCancellation = new CancellationTokenSource();
            status = "در حال پایش دونیت‌های ریمیت";
            Log("info", "کنترل روشن شد. دونیت‌های قبلی اجرا نمی‌شوند.");
            _ = RunLoop(runCancellation.Token, thisGeneration);
            return State();
        }
        finally { starting = false; }
    }

    async Task RunLoop(CancellationToken cancellationToken, int thisGeneration)
    {
        while (armed && !cancellationToken.IsCancellationRequested && thisGeneration == generation)
        {
            polling = true; Publish();
            try
            {
                var items = await reymit.Recent(token, cancellationToken);
                if (!armed || thisGeneration != generation) return;
                var fresh = items.Where(item => !seen.Contains(item.Id) && item.Time >= armedAt).Reverse().ToList();
                foreach (var item in items) seen.Add(item.Id);
                if (seen.Count > 500) seen = items.Select(item => item.Id).ToHashSet();
                foreach (var item in fresh)
                {
                    if (!armed || cancellationToken.IsCancellationRequested) return;
                    var rule = RuleService.Match(rules, item.Amount);
                    if (rule is null) { Log("skip", $"{item.Name} · خارج از بازه‌های تعریف‌شده", item.Amount); continue; }
                    if (queue.Count >= 10) { Log("skip", "صف فرمان‌ها پر است؛ این دونیت اجرا نشد.", item.Amount); continue; }
                    queue.Enqueue((rule, item));
                    Log("queued", $"{item.Name} · {rule.Key} در صف اجرا", item.Amount);
                }
                _ = ProcessQueue(cancellationToken, thisGeneration);
                lastPoll = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                status = "در حال پایش دونیت‌های ریمیت";
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return; }
            catch (Exception error)
            {
                Log("error", $"پایش یا اجرا ناموفق بود: {error.Message}");
                Stop("اتصال یا اجرای ورودی شکست خورد؛ کنترل متوقف شد.");
                return;
            }
            finally { polling = false; Publish(); }
            try { await Task.Delay(2000, cancellationToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    async Task ProcessQueue(CancellationToken cancellationToken, int thisGeneration)
    {
        if (processing) return;
        processing = true;
        try
        {
            while (armed && queue.Count > 0 && !cancellationToken.IsCancellationRequested && thisGeneration == generation)
            {
                var (rule, donation) = queue.Dequeue();
                Publish();
                await input.Execute(rule, cancellationToken);
                Log("success", $"{donation.Name} · {rule.Key} برای {rule.Duration / 1000.0:0.#} ثانیه", donation.Amount);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception error)
        {
            Log("error", $"اجرای ورودی ناموفق بود: {error.Message}");
            Stop("اجرای ورودی شکست خورد؛ کنترل متوقف شد.");
        }
        finally
        {
            processing = false;
            Publish();
            if (armed && queue.Count > 0 && runCancellation is not null)
                _ = ProcessQueue(runCancellation.Token, generation);
        }
    }

    public ViewState Stop(string reason = "کنترل متوقف شد.")
    {
        generation++;
        armed = false;
        runCancellation?.Cancel();
        runCancellation = null;
        queue.Clear();
        input.Stop();
        status = reason;
        Publish();
        return State();
    }

    public async Task<ViewState> Test(string id)
    {
        if (armed || starting) throw new InvalidOperationException("تست فقط در حالت توقف مجاز است.");
        var rule = rules.FirstOrDefault(item => item.Id == id) ?? throw new InvalidOperationException("قانون پیدا نشد.");
        await input.ExecuteTest(rule);
        Log("success", $"تست دستی {rule.Key} انجام شد.");
        return State();
    }
}
