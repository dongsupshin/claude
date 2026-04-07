using System.Media;

namespace FocusGuard.Services;

/// <summary>
/// Plays notification sounds using built-in Windows system sounds.
/// No external audio files or paid APIs required.
/// </summary>
public static class SoundService
{
    public static void PlayWorkComplete()
    {
        SystemSounds.Exclamation.Play();
    }

    public static void PlayBreakComplete()
    {
        SystemSounds.Asterisk.Play();
    }

    public static void PlayEyeCareReminder()
    {
        SystemSounds.Hand.Play();
    }

    public static void PlayGenericNotification()
    {
        SystemSounds.Beep.Play();
    }
}
