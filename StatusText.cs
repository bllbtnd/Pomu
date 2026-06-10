namespace Pomu;

static class StatusText
{
    const int SecondsPerMinute = 60;
    const string AppName = "Pomu";

    public static string FormatStatusBar(SessionState state)
    {
        if (state.IsPaused && (state.Phase == Phase.Work || state.Phase == Phase.Rest))
            return $"{AppName} [paused]";

        switch (state.Phase)
        {
            case Phase.Idle:
                return AppName;
            case Phase.Done:
                return $"{AppName} done";
            case Phase.Work:
                return $"{AppName} {FormatBlockTime(state.Current!)}";
            case Phase.Rest:
                return $"{AppName} R {FormatBlockTime(state.Current!)}";
            default:
                return AppName;
        }
    }

    public static string FormatPopoverTime(BlockState block) => FormatBlockTime(block);

    public static string FormatBarCompact(SessionState state)
    {
        if (state.IsPaused)
            return "[paused]";
        var block = state.Current!;
        return block.Phase == Phase.Rest
            ? $"R {FormatBlockTime(block)}"
            : FormatBlockTime(block);
    }

    static string FormatBlockTime(BlockState block)
    {
        int value = block.DisplaySeconds;
        if (value < 0)
            return "-" + FormatOvertime(-value);
        return FormatCountdown(value);
    }

    static string FormatCountdown(int totalSeconds)
    {
        int minutes = totalSeconds / SecondsPerMinute;
        int seconds = totalSeconds % SecondsPerMinute;
        return $"{minutes:D2}:{seconds:D2}";
    }

    static string FormatOvertime(int totalSeconds)
    {
        int minutes = totalSeconds / SecondsPerMinute;
        int seconds = totalSeconds % SecondsPerMinute;
        return $"{minutes}:{seconds:D2}";
    }
}
