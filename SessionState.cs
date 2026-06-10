namespace Pomu;

enum Phase { Idle, Work, Rest, Done }

class SessionConfig
{
    public int WorkSeconds;
    public int RestSeconds;
    public int TotalWorkBlocks;
}

class BlockState
{
    public Phase Phase;
    public int ScheduledSeconds;
    public int BaseScheduledSeconds;
    public int ElapsedSeconds;
    public int WorkBlockIndex;
    public int RestBlockIndex;

    public bool IsComplete => ElapsedSeconds >= ScheduledSeconds;
    public bool IsOvertime => IsComplete;
    public int OvertimeSeconds => IsComplete ? ElapsedSeconds - ScheduledSeconds : 0;
    public int DisplaySeconds => ScheduledSeconds - ElapsedSeconds;
}

class SessionState
{
    public SessionConfig? Config;
    public BlockState? Current;
    public bool IsPaused;

    bool _justCompleted;

    public Phase Phase => Current?.Phase ?? Phase.Idle;
    public bool CanAdvance => Current?.IsComplete ?? false;

    public void StartDay(SessionConfig config)
    {
        Config = config;
        IsPaused = false;
        _justCompleted = false;
        Current = new BlockState
        {
            Phase = Phase.Work,
            ScheduledSeconds = config.WorkSeconds,
            BaseScheduledSeconds = config.WorkSeconds,
            ElapsedSeconds = 0,
            WorkBlockIndex = 1,
            RestBlockIndex = 0
        };
    }

    public void Tick()
    {
        _justCompleted = false;
        if (IsPaused || Current == null) return;
        if (Phase != Phase.Work && Phase != Phase.Rest) return;

        bool wasComplete = Current.IsComplete;
        Current.ElapsedSeconds++;
        if (!wasComplete && Current.IsComplete)
            _justCompleted = true;
    }

    public bool JustCompleted() => _justCompleted;

    public void Advance()
    {
        if (Config == null || Current == null) return;
        if (!Current.IsComplete) return;

        if (Current.Phase == Phase.Work)
        {
            bool isLastWork = Current.WorkBlockIndex >= Config.TotalWorkBlocks;
            if (isLastWork)
            {
                Current = new BlockState { Phase = Phase.Done };
            }
            else
            {
                Current = new BlockState
                {
                    Phase = Phase.Rest,
                    ScheduledSeconds = Config.RestSeconds,
                    BaseScheduledSeconds = Config.RestSeconds,
                    ElapsedSeconds = 0,
                    WorkBlockIndex = 0,
                    RestBlockIndex = Current.WorkBlockIndex
                };
            }
        }
        else if (Current.Phase == Phase.Rest)
        {
            Current = new BlockState
            {
                Phase = Phase.Work,
                ScheduledSeconds = Config.WorkSeconds,
                BaseScheduledSeconds = Config.WorkSeconds,
                ElapsedSeconds = 0,
                WorkBlockIndex = Current.RestBlockIndex + 1,
                RestBlockIndex = 0
            };
        }

        _justCompleted = false;
    }

    public void Reset()
    {
        Config = null;
        Current = null;
        IsPaused = false;
        _justCompleted = false;
    }
}
