namespace Client.Pages.Reading;

public static class ReadingTimeFormatter
{
    public static string Format(int totalMinutes)
    {
        var days = totalMinutes / 1440;
        var hours = (totalMinutes % 1440) / 60;
        var minutes = totalMinutes % 60;

        var parts = new List<string>();

        if (days > 0)
        {
            parts.Add($"{days} dygn");
        }

        if (hours > 0)
        {
            parts.Add($"{hours} tim");
        }

        if (minutes > 0 || parts.Count == 0)
        {
            parts.Add($"{minutes} min");
        }

        return string.Join(" ", parts);
    }
}
