namespace Pomu;

enum Phase { Idle, Work, Rest, Done }

class SessionConfig
{
    public int WorkSeconds;
    public int RestSeconds;
    public int TotalWorkBlocks;
    public string? FocusName;
}

class BlockState
{
    public Phase Phase;
    public int ScheduledSeconds;
    public int BaseScheduledSeconds;
    public int ElapsedSeconds;
    public int WorkBlockIndex;
    public int RestBlockIndex;
    public DateTime StartedAtUtc;
    public double PausedSeconds;

    public bool IsComplete => ElapsedSeconds >= ScheduledSeconds;
    public bool IsOvertime => IsComplete;
    public int OvertimeSeconds => IsComplete ? ElapsedSeconds - ScheduledSeconds : 0;
    public int DisplaySeconds => ScheduledSeconds - ElapsedSeconds;
}

class SessionState
{
    public SessionConfig? Config;
    public BlockState? Current;
    public bool IsPaused { get; private set; }
    public int TotalActiveSeconds;
    public int TotalPauseSeconds;
    public int TotalOvertimeSeconds;
    public int TotalWorkSeconds;
    public int TotalRestSeconds;
    public int CompletedWorkBlocks;
    public DateTime? StartedAt;
    public DateTime? FinishedAt;

    bool _justCompleted;
    DateTime _pauseStartedUtc;

    public Phase Phase => Current?.Phase ?? Phase.Idle;
    public bool CanAdvance => Current?.IsComplete ?? false;

    public void StartDay(SessionConfig config)
    {
        Config = config;
        IsPaused = false;
        _justCompleted = false;
        TotalActiveSeconds = 0;
        TotalPauseSeconds = 0;
        TotalOvertimeSeconds = 0;
        TotalWorkSeconds = 0;
        TotalRestSeconds = 0;
        CompletedWorkBlocks = 0;
        StartedAt = DateTime.UtcNow;
        FinishedAt = null;
        Current = NewBlock(Phase.Work, config.WorkSeconds, workIndex: 1, restIndex: 0);
    }

    public void Tick()
    {
        _justCompleted = false;
        if (Current == null) return;
        if (Phase != Phase.Work && Phase != Phase.Rest) return;
        if (IsPaused) return;

        bool wasComplete = Current.IsComplete;
        Current.ElapsedSeconds = RealElapsedSeconds(Current);
        if (!wasComplete && Current.IsComplete)
            _justCompleted = true;
    }

    public void TogglePause()
    {
        if (Current == null) return;
        if (Phase != Phase.Work && Phase != Phase.Rest) return;

        if (!IsPaused)
        {
            IsPaused = true;
            _pauseStartedUtc = DateTime.UtcNow;
        }
        else
        {
            ClosePause();
        }
    }

    void ClosePause()
    {
        if (!IsPaused || Current == null) return;
        double pausedFor = (DateTime.UtcNow - _pauseStartedUtc).TotalSeconds;
        Current.PausedSeconds += pausedFor;
        TotalPauseSeconds += (int)Math.Round(pausedFor);
        IsPaused = false;
    }

    public bool JustCompleted() => _justCompleted;

    public void Advance()
    {
        if (Config == null || Current == null) return;
        if (!Current.IsComplete) return;

        ClosePause();
        Current.ElapsedSeconds = RealElapsedSeconds(Current);
        AccumulateBlock();

        if (Current.Phase == Phase.Work)
        {
            bool isLastWork = Current.WorkBlockIndex >= Config.TotalWorkBlocks;
            if (isLastWork)
            {
                FinishedAt = DateTime.UtcNow;
                Current = new BlockState { Phase = Phase.Done };
            }
            else
            {
                Current = NewBlock(Phase.Rest, Config.RestSeconds, workIndex: 0, restIndex: Current.WorkBlockIndex);
            }
        }
        else if (Current.Phase == Phase.Rest)
        {
            Current = NewBlock(Phase.Work, Config.WorkSeconds, workIndex: Current.RestBlockIndex + 1, restIndex: 0);
        }

        _justCompleted = false;
    }

    public void EndDay()
    {
        if (Current == null) return;
        if (Phase != Phase.Work && Phase != Phase.Rest) return;

        ClosePause();
        Current.ElapsedSeconds = RealElapsedSeconds(Current);
        AccumulateBlock();
        FinishedAt = DateTime.UtcNow;
        Current = new BlockState { Phase = Phase.Done };
        _justCompleted = false;
    }

    void AccumulateBlock()
    {
        if (Current == null) return;

        TotalActiveSeconds += Current.ElapsedSeconds;
        TotalOvertimeSeconds += Current.OvertimeSeconds;

        if (Current.Phase == Phase.Work)
        {
            TotalWorkSeconds += Current.ElapsedSeconds;
            if (Current.IsComplete)
                CompletedWorkBlocks++;
        }
        else if (Current.Phase == Phase.Rest)
        {
            TotalRestSeconds += Current.ElapsedSeconds;
        }
    }

    public void Reset()
    {
        Config = null;
        Current = null;
        IsPaused = false;
        _justCompleted = false;
        TotalActiveSeconds = 0;
        TotalPauseSeconds = 0;
        TotalOvertimeSeconds = 0;
        TotalWorkSeconds = 0;
        TotalRestSeconds = 0;
        CompletedWorkBlocks = 0;
        StartedAt = null;
        FinishedAt = null;
    }

    static BlockState NewBlock(Phase phase, int scheduledSeconds, int workIndex, int restIndex)
    {
        return new BlockState
        {
            Phase = phase,
            ScheduledSeconds = scheduledSeconds,
            BaseScheduledSeconds = scheduledSeconds,
            ElapsedSeconds = 0,
            WorkBlockIndex = workIndex,
            RestBlockIndex = restIndex,
            StartedAtUtc = DateTime.UtcNow,
            PausedSeconds = 0
        };
    }

    static int RealElapsedSeconds(BlockState block)
    {
        double elapsed = (DateTime.UtcNow - block.StartedAtUtc).TotalSeconds - block.PausedSeconds;
        return Math.Max(0, (int)elapsed);
    }
}
