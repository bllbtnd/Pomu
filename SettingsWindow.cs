using Foundation;

namespace Pomu;

static class SettingsStore
{
    const string WorkMinKey = "pomu_work_min";
    const string RestMinKey = "pomu_rest_min";
    const string SessionsKey = "pomu_sessions";
    const string FocusKey = "pomu_focus";

    public const int DefaultWorkMin = 25;
    public const int DefaultRestMin = 5;
    public const int DefaultSessions = 4;

    public const int MinSessions = 1;
    public const int MaxSessions = 12;
    public const int MinWorkMin = 1;
    public const int MaxWorkMin = 120;
    public const int MinRestMin = 1;
    public const int MaxRestMin = 60;

    public static int LoadWorkMin() => Read(WorkMinKey, DefaultWorkMin, MinWorkMin, MaxWorkMin);
    public static int LoadRestMin() => Read(RestMinKey, DefaultRestMin, MinRestMin, MaxRestMin);
    public static int LoadSessions() => Read(SessionsKey, DefaultSessions, MinSessions, MaxSessions);

    public static string LoadFocusName() =>
        NSUserDefaults.StandardUserDefaults.StringForKey(FocusKey) ?? string.Empty;

    public static void Save(int workMin, int restMin, int sessions, string focusName)
    {
        var defaults = NSUserDefaults.StandardUserDefaults;
        defaults.SetInt(workMin, WorkMinKey);
        defaults.SetInt(restMin, RestMinKey);
        defaults.SetInt(sessions, SessionsKey);
        defaults.SetString(focusName, FocusKey);
        defaults.Synchronize();
    }

    static int Read(string key, int fallback, int min, int max)
    {
        var defaults = NSUserDefaults.StandardUserDefaults;
        if (defaults[key] == null) return fallback;
        int value = (int)defaults.IntForKey(key);
        if (value < min || value > max) return fallback;
        return value;
    }
}
