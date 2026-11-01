using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TitanoTrigger;

public sealed class ReymitClient
{
    private readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(12) };
    private static readonly Regex TokenPattern = new("^[A-Za-z0-9_-]{20,200}$", RegexOptions.Compiled);

    async Task<JsonElement> GetData(string token, string suffix, CancellationToken cancellationToken)
    {
        if (!TokenPattern.IsMatch(token)) throw new InvalidOperationException("توکن ریمیت نامعتبر است.");
        var url = "https://api.reymit.ir/user/" + Uri.EscapeDataString(token) + suffix;
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.ParseAdd("application/json");
        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"پاسخ ریمیت: HTTP {(int)response.StatusCode}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var root = document.RootElement;
        if (!root.TryGetProperty("ok", out var ok) || ok.ValueKind != JsonValueKind.True)
            throw new InvalidOperationException("ریمیت اتصال را تأیید نکرد.");
        if (!root.TryGetProperty("data", out var data)) throw new InvalidOperationException("پاسخ ریمیت شناخته نشد.");
        return data.Clone();
    }

    public async Task<string> Verify(string token, CancellationToken cancellationToken = default)
    {
        var data = await GetData(token, "", cancellationToken);
        if (!data.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String)
            throw new InvalidOperationException("پاسخ حساب ریمیت شناخته نشد.");
        return (name.GetString() ?? "")[..Math.Min(name.GetString()?.Length ?? 0, 80)];
    }

    public async Task<List<Donation>> Recent(string token, CancellationToken cancellationToken = default)
    {
        var data = await GetData(token, "/donates/last-donates", cancellationToken);
        if (!data.TryGetProperty("donates", out var items) || items.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("فهرست دونیت ریمیت شناخته نشد.");
        var result = new List<Donation>();
        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("id", out var idValue)) continue;
            var id = idValue.ValueKind switch
            {
                JsonValueKind.String => idValue.GetString(),
                JsonValueKind.Number => idValue.GetRawText(),
                _ => null
            };
            if (!item.TryGetProperty("toman_amount", out var amountValue)) continue;
            var hasAmount = amountValue.ValueKind == JsonValueKind.String
                ? long.TryParse(amountValue.GetString(), out var amount)
                : amountValue.TryGetInt64(out amount);
            if (!hasAmount || amount < 0) continue;
            if (!item.TryGetProperty("time", out var timeValue)) continue;
            var hasTime = timeValue.ValueKind == JsonValueKind.String
                ? double.TryParse(timeValue.GetString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var time)
                : timeValue.TryGetDouble(out time);
            if (!hasTime) continue;
            if (string.IsNullOrWhiteSpace(id) || double.IsNaN(time) || double.IsInfinity(time)) continue;
            string name = "حامی";
            if (item.TryGetProperty("censored_name", out var censored) && censored.ValueKind == JsonValueKind.String)
                name = censored.GetString() ?? "حامی";
            else if (item.TryGetProperty("name", out var rawName) && rawName.ValueKind == JsonValueKind.String)
                name = rawName.GetString() ?? "حامی";
            result.Add(new Donation(id, amount, time, name[..Math.Min(name.Length, 80)]));
        }
        return result;
    }
}
