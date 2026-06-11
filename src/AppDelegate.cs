using AppKit;
using CoreGraphics;
using Foundation;

namespace Pomu;

[Register("AppDelegate")]
class AppDelegate : NSApplicationDelegate
{
    readonly SessionState _state = new();
    TimerEngine? _timer;
    NSStatusItem? _statusItem;
    NSPopover? _popover;
    PopoverController? _popoverController;

    public override void DidFinishLaunching(NSNotification notification)
    {
        NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Accessory;

        _timer = new TimerEngine(_state);
        _timer.OnTick += HandleTick;
        _timer.OnBlockCompleted += HandleBlockCompleted;

        BuildStatusItem();
        BuildPopover();
        UpdateStatusTitle();
    }

    void BuildStatusItem()
    {
        _statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
        if (_statusItem.Button != null)
        {
            _statusItem.Button.SendActionOn((NSEventType)(NSEventMask.LeftMouseUp | NSEventMask.RightMouseUp));
            _statusItem.Button.Activated += (s, e) => HandleButtonClick();
        }
    }

    void BuildPopover()
    {
        _popoverController = new PopoverController(_state);
        _popoverController.OnStartDay += HandleStartDay;
        _popoverController.OnTogglePause += HandleTogglePause;
        _popoverController.OnResetDay += HandleResetDay;
        _popoverController.OnEndDay += HandleEndDay;
        _popoverController.OnContinue += HandleContinue;

        _popover = new NSPopover
        {
            Behavior = NSPopoverBehavior.Transient,
            ContentViewController = _popoverController
        };
    }

    void HandleButtonClick()
    {
        var current = NSApplication.SharedApplication.CurrentEvent;
        bool isRightClick = current != null &&
            (current.Type == NSEventType.RightMouseUp ||
             (current.ModifierFlags & NSEventModifierMask.ControlKeyMask) != 0);

        if (isRightClick)
            ShowContextMenu();
        else
            TogglePopover();
    }

    void ShowContextMenu()
    {
        var button = _statusItem?.Button;
        if (button == null) return;

        var menu = new NSMenu();
        var quitItem = new NSMenuItem("Quit Pomu", (s, e) => NSApplication.SharedApplication.Terminate(this));
        quitItem.KeyEquivalent = "q";
        menu.AddItem(quitItem);

        var current = NSApplication.SharedApplication.CurrentEvent;
        if (current != null)
            NSMenu.PopUpContextMenu(menu, current, button);
    }

    void TogglePopover()
    {
        if (_popover == null || _statusItem?.Button == null) return;

        if (_popover.Shown)
        {
            _popover.PerformClose(_statusItem.Button);
        }
        else
        {
            RenderPopover();
            _popover.Show(CGRect.Empty, _statusItem.Button, NSRectEdge.MinYEdge);
        }
    }

    void RenderPopover()
    {
        _popoverController?.Render();
    }

    void HandleTick()
    {
        UpdateStatusTitle();
        if (_popover?.Shown == true)
            RenderPopover();
    }

    void HandleBlockCompleted(Phase completedPhase)
    {
        if (completedPhase == Phase.Work)
            SoundPlayer.PlayWorkComplete();
        else if (completedPhase == Phase.Rest)
            SoundPlayer.PlayRestComplete();
    }

    void HandleStartDay(SessionConfig config)
    {
        _state.StartDay(config);
        _timer?.Start();
        ApplyFocusForPhase();
        RefreshUi();
    }

    void ApplyFocusForPhase()
    {
        string? focus = _state.Config?.FocusName;
        if (focus == null) return;

        if (_state.Phase == Phase.Work)
            FocusManager.EnableFocus(focus);
        else
            FocusManager.DisableFocus(focus);
    }

    void HandleTogglePause()
    {
        _state.TogglePause();
        RefreshUi();
    }

    void HandleContinue()
    {
        _state.Advance();
        ApplyFocusForPhase();
        if (_state.Phase == Phase.Done)
            _timer?.Stop();
        RefreshUi();
    }

    void HandleEndDay()
    {
        if (!Confirm("End the day early? Your progress so far will be summarized.", "End Day")) return;
        _state.EndDay();
        ApplyFocusForPhase();
        _timer?.Stop();
        RefreshUi();
    }

    void HandleResetDay()
    {
        if (!Confirm("Reset the current session? This cannot be undone.", "Reset")) return;
        string? focus = _state.Config?.FocusName;
        if (focus != null)
            FocusManager.DisableFocus(focus);
        _state.Reset();
        _timer?.Stop();
        RefreshUi();
    }

    bool Confirm(string message, string confirmTitle)
    {
        var alert = new NSAlert
        {
            MessageText = message,
            AlertStyle = NSAlertStyle.Warning
        };
        alert.AddButton(confirmTitle);
        alert.AddButton("Cancel");
        return alert.RunModal() == (long)NSAlertButtonReturn.First;
    }

    void RefreshUi()
    {
        UpdateStatusTitle();
        RenderPopover();
    }

    void UpdateStatusTitle()
    {
        var button = _statusItem?.Button;
        if (button == null) return;

        bool counting = _state.Phase == Phase.Work || _state.Phase == Phase.Rest;
        if (counting)
        {
            button.Image = IconFactory.MenuBarIcon();
            button.Title = StatusText.FormatBarCompact(_state);
            button.ImagePosition = NSCellImagePosition.ImageLeft;
        }
        else
        {
            button.Title = string.Empty;
            button.Image = IconFactory.MenuBarIcon();
            button.ImagePosition = NSCellImagePosition.ImageOnly;
        }
    }
}
