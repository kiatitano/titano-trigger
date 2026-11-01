namespace TitanoTrigger;

public static class RuleService
{
    public static readonly Dictionary<string, byte> Keys = BuildKeys();

    static Dictionary<string, byte> BuildKeys()
    {
        var keys = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789") keys[c.ToString()] = (byte)c;
        for (var i = 1; i <= 12; i++) keys[$"F{i}"] = (byte)(111 + i);
        keys["SPACE"] = 32; keys["ENTER"] = 13; keys["TAB"] = 9; keys["ESC"] = 27;
        keys["UP"] = 38; keys["DOWN"] = 40; keys["LEFT"] = 37; keys["RIGHT"] = 39;
        keys["MOUSE_LEFT"] = 1; keys["MOUSE_RIGHT"] = 2; keys["MOUSE_MIDDLE"] = 4;
        return keys;
    }

    public static List<Rule> Validate(List<Rule>? input)
    {
        if (input is null || input.Count > 50) throw new InvalidOperationException("حداکثر ۵۰ قانون مجاز است.");
        var output = new List<Rule>();
        foreach (var (rule, index) in input.Select((value, index) => (value, index)))
        {
            if (rule.Min < 1 || rule.Max < rule.Min || rule.Max > 1_000_000_000)
                throw new InvalidOperationException($"بازهٔ مبلغ قانون {index + 1} معتبر نیست.");
            if (rule.Duration < 100 || rule.Duration > 10_000)
                throw new InvalidOperationException($"مدت قانون {index + 1} باید بین ۰٫۱ تا ۱۰ ثانیه باشد.");
            var key = rule.Key?.ToUpperInvariant() ?? "";
            if (!Keys.ContainsKey(key)) throw new InvalidOperationException($"کلید قانون {index + 1} پشتیبانی نمی‌شود.");
            if (string.IsNullOrWhiteSpace(rule.Id) || rule.Id.Length > 50)
                throw new InvalidOperationException($"شناسهٔ قانون {index + 1} معتبر نیست.");
            output.Add(rule with { Key = key });
        }
        for (var i = 0; i < output.Count; i++)
            for (var j = i + 1; j < output.Count; j++)
                if (output[i].Min <= output[j].Max && output[j].Min <= output[i].Max)
                    throw new InvalidOperationException("بازه‌های مبلغ نباید هم‌پوشانی داشته باشند.");
        return output;
    }

    public static Rule? Match(IEnumerable<Rule> rules, long amount) =>
        rules.FirstOrDefault(rule => amount >= rule.Min && amount <= rule.Max);
}
