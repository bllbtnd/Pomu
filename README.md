# Pomu

A tiny macOS menu bar Pomodoro timer. This is a quick, completely vibecoded project. I just needed a decent Pomodoro app to help me grind for my BSc state exam, so I built one instead of hunting through the App Store.

It lives in the menu bar, shows a little tomato when idle and the running time while a session is going, and that is about it.

## Build and run

You need the .NET macOS workload:

```
dotnet workload install macos
```

Then build the app bundle:

```
./make_app.sh
open Pomu.app
```

Or just run it straight from source:

```
dotnet run --project Pomu.csproj
```

## How to use

1. Click the tomato in the menu bar to open the popover.
2. Set how many work sessions you want, plus the work and rest length in minutes.
3. Hit Start Day.
4. Work until the timer hits zero. A sound plays, then click Continue to roll into the break.
5. Repeat until you are done.

The day always ends on a work block, since finishing on a break never made sense to me. The menu bar shows the time left while you work or rest. If you run over it just keeps counting up in red as negative time so you can see how far past you went. It does not mess with your other blocks, they always run their full length.

Pause and Resume freeze the clock whenever you need to step away. Reset Day wipes everything and takes you back to the setup screen.

That is the whole thing. Personal use, no warranty, have fun.
