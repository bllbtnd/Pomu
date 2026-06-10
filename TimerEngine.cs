using AppKit;

namespace Pomu;

class TimerEngine
{
    const int TickIntervalMs = 1000;

    readonly SessionState _state;
    System.Threading.Timer? _timer;

    public event Action? OnTick;
    public event Action<Phase>? OnBlockCompleted;

    public TimerEngine(SessionState state)
    {
        _state = state;
    }

    public void Start()
    {
        Stop();
        _timer = new System.Threading.Timer(_ => Fire(), null, TickIntervalMs, TickIntervalMs);
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    void Fire()
    {
        NSApplication.SharedApplication.InvokeOnMainThread(() =>
        {
            Phase phaseBeforeTick = _state.Phase;
            _state.Tick();
            if (_state.JustCompleted())
                OnBlockCompleted?.Invoke(phaseBeforeTick);
            OnTick?.Invoke();
        });
    }
}
