using AppKit;
using AudioToolbox;

namespace Pomu;

static class SoundPlayer
{
    const string WorkCompleteSound = "Glass";
    const string RestCompleteSound = "Submarine";
    const uint FallbackSystemSoundId = 1005;

    static NSSound? _current;

    public static void PlayWorkComplete() => Play(WorkCompleteSound);

    public static void PlayRestComplete() => Play(RestCompleteSound);

    static void Play(string name)
    {
        _current?.Stop();

        var sound = NSSound.FromName(name);
        if (sound == null)
        {
            PlayFallback();
            return;
        }

        _current = sound;
        if (!sound.Play())
            PlayFallback();
    }

    static void PlayFallback()
    {
        var systemSound = new SystemSound(FallbackSystemSoundId);
        systemSound.PlaySystemSound();
    }
}
