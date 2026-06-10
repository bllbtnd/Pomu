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
./scripts/make_app.sh
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
5. Repeat until you are done. The finish screen shows a small recap: total time including overtime, how much of it was overtime, and how long you sat paused.

The day always ends on a work block, since finishing on a break never made sense to me. The menu bar shows the time left while you work or rest. If you run over it just keeps counting up in red as negative time so you can see how far past you went. It does not mess with your other blocks, they always run their full length.

Pause and Resume freeze the clock whenever you need to step away. Reset Day wipes everything and takes you back to the setup screen. Right click the menu bar icon to quit the app.

## Focus modes (optional)

Pomu can flip a macOS Focus on while you work and off while you rest or when the day ends. Sadly this needs a small workaround. Apple has no public API for Focus modes, third party apps cannot even read the list of your Focus modes, let alone switch them. The only sanctioned way is the Shortcuts app, since a shortcut can contain a Set Focus action and any app is allowed to run shortcuts by name. So for each Focus you want, make two shortcuts:

1. `Pomu Study On` containing a single Set Focus action that turns your Focus on.
2. `Pomu Study Off` containing a Set Focus action that turns it off.

Name them exactly like that, `Pomu` plus your label plus `On` or `Off`. Any pair like this shows up as `Study` in the Focus dropdown on the start screen. Make as many pairs as you like, each one becomes its own dropdown entry, like `Pomu Deep Work On` and `Pomu Deep Work Off` showing up as `Deep Work`. Both halves of a pair must exist or the entry will not appear. The list refreshes every time you open the popover, no restart needed.

The shortcuts do not even have to set a Focus. The On one runs at the start of every work block and the Off one at every break and at the end of the day, so you can stuff anything in there: focus plus hiding notifications plus opening your notes app, whatever. Pick None if you do not want any of it.

The first time Pomu runs a shortcut macOS may ask for permission, just allow it once.

That is the whole thing. Personal use, no warranty, have fun.
