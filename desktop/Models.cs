namespace TitanoTrigger;

public sealed record Rule(string Id, long Min, long Max, int Duration, string Key);
public sealed record Donation(string Id, long Amount, double Time, string Name);
public sealed record Activity(string Id, string Kind, string Message, long? Amount, long Time);

public sealed class ViewState
{
    public bool Connected { get; init; }
    public string Account { get; init; } = "";
    public bool Armed { get; init; }
    public List<Rule> Rules { get; init; } = [];
    public List<Activity> History { get; init; } = [];
    public long? LastPoll { get; init; }
    public string Status { get; init; } = "";
    public bool Polling { get; init; }
    public int QueueSize { get; init; }
}
