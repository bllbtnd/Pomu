using AppKit;
using CoreGraphics;
using Foundation;

namespace Pomu;

class PopoverController : NSViewController
{
    const float PopoverWidth = 280f;
    const float PopoverHeight = 320f;
    const string NoFocusTitle = "None";
    const float Margin = 16f;
    const float RowHeight = 24f;
    const float RowSpacing = 10f;
    const int SecondsPerMinute = 60;

    readonly SessionState _state;

    public event Action<SessionConfig>? OnStartDay;
    public event Action? OnTogglePause;
    public event Action? OnResetDay;
    public event Action? OnEndDay;
    public event Action? OnContinue;

    NSStepper? _sessionsStepper;
    NSStepper? _workStepper;
    NSStepper? _restStepper;
    NSTextField? _sessionsValue;
    NSTextField? _workValue;
    NSTextField? _restValue;
    NSTextField? _planLabel;
    NSPopUpButton? _focusPicker;

    public PopoverController(SessionState state)
    {
        _state = state;
    }

    public override void LoadView()
    {
        View = new NSView(new CGRect(0, 0, PopoverWidth, PopoverHeight));
        Render();
    }

    public void Render()
    {
        if (View == null) return;
        ClearSubviews();

        switch (_state.Phase)
        {
            case Phase.Idle:
                BuildIdle();
                break;
            case Phase.Work:
            case Phase.Rest:
                BuildActive();
                break;
            case Phase.Done:
                BuildDone();
                break;
        }
    }

    void ClearSubviews()
    {
        foreach (var sub in View.Subviews)
            sub.RemoveFromSuperview();
    }

    void BuildIdle()
    {
        float top = PopoverHeight - Margin;

        var title = MakeTitle("Pomu");
        title.Font = NSFont.BoldSystemFontOfSize(26)!;
        title.Frame = new CGRect(Margin, top - 32, PopoverWidth - 2 * Margin, 32);
        View.AddSubview(title);

        var credit = MakeLabel("by Botond Balla \u00b7 ballabotond.com");
        credit.Font = NSFont.SystemFontOfSize(10)!;
        credit.TextColor = NSColor.TertiaryLabel;
        credit.Frame = new CGRect(Margin, top - 32 - 14, PopoverWidth - 2 * Margin, 13);
        View.AddSubview(credit);

        float planHeight = 16f;
        float areaTop = top - 32 - 14 - 6;
        float areaBottom = Margin + 30 + 6 + planHeight + 4;
        float pitch = (areaTop - areaBottom) / 4;
        float slotOffset = (pitch - RowHeight) / 2;
        float y = areaTop - pitch + slotOffset;

        _sessionsStepper = MakeStepper(SettingsStore.MinSessions, SettingsStore.MaxSessions, SettingsStore.LoadSessions());
        _sessionsValue = MakeValueField(_sessionsStepper.IntValue);
        AddFieldRow("Sessions", _sessionsValue, _sessionsStepper, y);
        WireRow(_sessionsValue, _sessionsStepper);

        y -= pitch;
        _workStepper = MakeStepper(SettingsStore.MinWorkMin, SettingsStore.MaxWorkMin, SettingsStore.LoadWorkMin());
        _workValue = MakeValueField(_workStepper.IntValue);
        AddFieldRow("Work min", _workValue, _workStepper, y);
        WireRow(_workValue, _workStepper);

        y -= pitch;
        _restStepper = MakeStepper(SettingsStore.MinRestMin, SettingsStore.MaxRestMin, SettingsStore.LoadRestMin());
        _restValue = MakeValueField(_restStepper.IntValue);
        AddFieldRow("Rest min", _restValue, _restStepper, y);
        WireRow(_restValue, _restStepper);

        y -= pitch;
        AddFocusRow(y);

        _planLabel = MakeLabel(string.Empty);
        _planLabel.Alignment = NSTextAlignment.Center;
        _planLabel.Font = NSFont.SystemFontOfSize(11)!;
        _planLabel.TextColor = NSColor.SecondaryLabel;
        _planLabel.Frame = new CGRect(Margin, Margin + 30 + 6, PopoverWidth - 2 * Margin, planHeight);
        View.AddSubview(_planLabel);
        UpdatePlanLabel();

        var startButton = MakeButton("Start Day", StartDayClicked);
        startButton.KeyEquivalent = "\r";
        startButton.Frame = new CGRect(Margin, Margin, PopoverWidth - 2 * Margin, 30);
        View.AddSubview(startButton);
    }

    void WireRow(NSTextField field, NSStepper stepper)
    {
        stepper.Activated += (s, e) =>
        {
            field.StringValue = stepper.IntValue.ToString();
            UpdatePlanLabel();
        };
        field.Changed += (s, e) =>
        {
            if (int.TryParse(field.StringValue, out int typed))
            {
                int clamped = Math.Clamp(typed, (int)stepper.MinValue, (int)stepper.MaxValue);
                stepper.IntValue = clamped;
                if (clamped != typed)
                    field.StringValue = clamped.ToString();
                UpdatePlanLabel();
            }
        };
    }

    void UpdatePlanLabel()
    {
        if (_planLabel == null || _sessionsStepper == null || _workStepper == null || _restStepper == null) return;

        int sessions = _sessionsStepper.IntValue;
        int totalMinutes = sessions * _workStepper.IntValue + (sessions - 1) * _restStepper.IntValue;
        string length = totalMinutes >= 60
            ? $"{totalMinutes / 60}h {totalMinutes % 60:D2}m"
            : $"{totalMinutes}m";
        string ends = DateTime.Now.AddMinutes(totalMinutes).ToString("t");
        _planLabel.StringValue = $"{length} total \u00b7 ends around {ends}";
    }

    void AddFocusRow(float y)
    {
        var label = MakeLabel("Focus");
        label.Frame = new CGRect(Margin, y, 90, RowHeight);
        View.AddSubview(label);

        _focusPicker = new NSPopUpButton(new CGRect(Margin + 96, y - 2, PopoverWidth - 2 * Margin - 96, RowHeight + 4), false);
        _focusPicker.AddItem(NoFocusTitle);
        foreach (var name in FocusManager.ListFocusNames())
            _focusPicker.AddItem(name);

        string saved = SettingsStore.LoadFocusName();
        if (saved.Length > 0 && _focusPicker.ItemWithTitle(saved) != null)
            _focusPicker.SelectItem(saved);

        View.AddSubview(_focusPicker);
    }

    void AddFieldRow(string caption, NSTextField valueLabel, NSStepper stepper, float y)
    {
        var label = MakeLabel(caption);
        label.Frame = new CGRect(Margin, y, 90, RowHeight);
        View.AddSubview(label);

        valueLabel.Frame = new CGRect(PopoverWidth - Margin - 88, y, 56, RowHeight - 2);
        View.AddSubview(valueLabel);

        stepper.Frame = new CGRect(PopoverWidth - Margin - 24, y, 24, RowHeight);
        View.AddSubview(stepper);
    }

    void BuildActive()
    {
        var block = _state.Current!;
        float top = PopoverHeight - Margin;

        var phaseLabel = MakeLabel(PhaseText(block, _state.Config!.TotalWorkBlocks));
        phaseLabel.Alignment = NSTextAlignment.Center;
        phaseLabel.Frame = new CGRect(Margin, top - RowHeight, PopoverWidth - 2 * Margin, RowHeight);
        View.AddSubview(phaseLabel);

        var timeLabel = MakeTimeLabel(block);
        timeLabel.Frame = new CGRect(Margin, PopoverHeight / 2 + 10, PopoverWidth - 2 * Margin, 72);
        View.AddSubview(timeLabel);

        var progress = new NSProgressIndicator(new CGRect(Margin, PopoverHeight / 2 - 24, PopoverWidth - 2 * Margin, 16))
        {
            Style = NSProgressIndicatorStyle.Bar,
            Indeterminate = false,
            MinValue = 0,
            MaxValue = block.ScheduledSeconds,
            DoubleValue = Math.Min(block.ElapsedSeconds, block.ScheduledSeconds)
        };
        View.AddSubview(progress);

        float buttonY = Margin;

        if (block.IsComplete)
        {
            var continueButton = MakeButton("Continue", () => OnContinue?.Invoke());
            continueButton.Frame = new CGRect(Margin, buttonY + 38, PopoverWidth - 2 * Margin, 30);
            View.AddSubview(continueButton);
        }

        var pauseButton = MakeButton(_state.IsPaused ? "Resume" : "Pause", () => OnTogglePause?.Invoke());
        pauseButton.Frame = new CGRect(Margin, buttonY, (PopoverWidth - 2 * Margin) / 2 - 4, 30);
        View.AddSubview(pauseButton);

        var endButton = MakeButton("End Day", () => OnEndDay?.Invoke());
        endButton.Frame = new CGRect(PopoverWidth / 2 + 4, buttonY, (PopoverWidth - 2 * Margin) / 2 - 4, 30);
        View.AddSubview(endButton);
    }

    void BuildDone()
    {
        float top = PopoverHeight - Margin;

        var label = MakeTitle("Session complete");
        label.Alignment = NSTextAlignment.Center;
        label.Frame = new CGRect(Margin, top - RowHeight, PopoverWidth - 2 * Margin, RowHeight);
        View.AddSubview(label);

        bool crossedMidnight = _state.StartedAt?.ToLocalTime().Date != _state.FinishedAt?.ToLocalTime().Date;

        float y = top - RowHeight - RowSpacing - RowHeight;
        AddStatRow("From", FormatStamp(_state.StartedAt, crossedMidnight), y);
        y -= RowHeight;
        AddStatRow("To", FormatStamp(_state.FinishedAt, crossedMidnight), y);
        y -= RowHeight;
        string sessions = _state.Config != null
            ? $"{_state.CompletedWorkBlocks} of {_state.Config.TotalWorkBlocks}"
            : "-";
        AddStatRow("Sessions", sessions, y);
        y -= RowHeight;
        int totalSeconds = _state.StartedAt != null && _state.FinishedAt != null
            ? (int)(_state.FinishedAt.Value - _state.StartedAt.Value).TotalSeconds
            : _state.TotalActiveSeconds + _state.TotalPauseSeconds;
        AddStatRow("Total time", StatusText.FormatDuration(totalSeconds), y);
        y -= RowHeight;
        AddStatRow("Work", StatusText.FormatDuration(_state.TotalWorkSeconds), y);
        y -= RowHeight;
        AddStatRow("Rest", StatusText.FormatDuration(_state.TotalRestSeconds), y);
        y -= RowHeight;
        string avgWork = _state.CompletedWorkBlocks > 0
            ? StatusText.FormatDuration(_state.TotalWorkSeconds / _state.CompletedWorkBlocks)
            : "-";
        AddStatRow("Avg work", avgWork, y);
        y -= RowHeight;
        AddStatRow("Overtime", StatusText.FormatDuration(_state.TotalOvertimeSeconds), y);
        y -= RowHeight;
        AddStatRow("Paused", StatusText.FormatDuration(_state.TotalPauseSeconds), y);

        var resetButton = MakeButton("Reset Day", () => OnResetDay?.Invoke());
        resetButton.Frame = new CGRect(Margin, Margin, PopoverWidth - 2 * Margin, 30);
        View.AddSubview(resetButton);
    }

    static string FormatStamp(DateTime? stamp, bool withDate)
    {
        if (stamp == null) return "-";
        var local = stamp.Value.ToLocalTime();
        return withDate ? local.ToString("g") : local.ToString("t");
    }

    void AddStatRow(string caption, string value, float y)
    {
        var label = MakeLabel(caption);
        label.TextColor = NSColor.SecondaryLabel;
        label.Frame = new CGRect(Margin, y, 110, RowHeight);
        View.AddSubview(label);

        var valueLabel = MakeLabel(value);
        valueLabel.Alignment = NSTextAlignment.Right;
        valueLabel.Font = NSFont.MonospacedDigitSystemFontOfSize(13, NSFontWeight.Medium)!;
        valueLabel.Frame = new CGRect(PopoverWidth - Margin - 130, y, 130, RowHeight);
        View.AddSubview(valueLabel);
    }

    void StartDayClicked()
    {
        int sessions = _sessionsStepper!.IntValue;
        int workMin = _workStepper!.IntValue;
        int restMin = _restStepper!.IntValue;
        string focusTitle = _focusPicker?.SelectedItem?.Title ?? NoFocusTitle;
        string? focusName = focusTitle == NoFocusTitle ? null : focusTitle;
        SettingsStore.Save(workMin, restMin, sessions, focusName ?? string.Empty);

        var config = new SessionConfig
        {
            WorkSeconds = workMin * SecondsPerMinute,
            RestSeconds = restMin * SecondsPerMinute,
            TotalWorkBlocks = sessions,
            FocusName = focusName
        };
        OnStartDay?.Invoke(config);
    }

    static string PhaseText(BlockState block, int total)
    {
        return block.Phase == Phase.Work
            ? $"Work {block.WorkBlockIndex} of {total}"
            : $"Rest {block.RestBlockIndex} of {total}";
    }

    NSTextField MakeTimeLabel(BlockState block)
    {
        var field = MakeLabel(StatusText.FormatPopoverTime(block));
        field.Alignment = NSTextAlignment.Center;
        field.Font = NSFont.MonospacedDigitSystemFontOfSize(52, NSFontWeight.Medium)!;
        if (block.IsOvertime)
            field.TextColor = NSColor.Red;
        return field;
    }

    static NSTextField MakeTitle(string text)
    {
        var field = MakeLabel(text);
        field.Font = NSFont.BoldSystemFontOfSize(18)!;
        return field;
    }

    static NSTextField MakeLabel(string text)
    {
        return new NSTextField
        {
            StringValue = text,
            Editable = false,
            Bordered = false,
            Selectable = false,
            DrawsBackground = false,
            Font = NSFont.SystemFontOfSize(13)!
        };
    }

    static NSTextField MakeValueField(int value)
    {
        return new NSTextField
        {
            StringValue = value.ToString(),
            Editable = true,
            Bordered = true,
            BezelStyle = NSTextFieldBezelStyle.Rounded,
            Alignment = NSTextAlignment.Center,
            Font = NSFont.MonospacedDigitSystemFontOfSize(13, NSFontWeight.Regular)!
        };
    }

    static NSStepper MakeStepper(int min, int max, int value)
    {
        return new NSStepper
        {
            MinValue = min,
            MaxValue = max,
            IntValue = value,
            Increment = 1,
            ValueWraps = false
        };
    }

    static NSButton MakeButton(string title, Action onClick)
    {
        var button = new NSButton
        {
            Title = title,
            BezelStyle = NSBezelStyle.Rounded
        };
        button.SetButtonType(NSButtonType.MomentaryPushIn);
        button.Activated += (s, e) => onClick();
        return button;
    }
}
