using UnityEngine;
using System.Runtime.InteropServices;

namespace Wobble.Audio
{
    public sealed class HapticManager
    {
        public bool Enabled { get; private set; }
        public HapticManager(bool enabled) => Enabled = enabled;
        public void SetEnabled(bool value) => Enabled = value;
        public void Light() => Vibrate(0);
        public void Perfect() => Vibrate(1);
        public void GameOver() => Vibrate(2);

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void WobbleImpact(int strength);
#endif

        private void Vibrate(int strength)
        {
            if (!Enabled || !Application.isMobilePlatform) return;
#if UNITY_IOS && !UNITY_EDITOR
            WobbleImpact(strength);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            if (vibrator == null || !vibrator.Call<bool>("hasVibrator")) return;
            using var effectType = new AndroidJavaClass("android.os.VibrationEffect");
            using var effect = effectType.CallStatic<AndroidJavaObject>("createOneShot", strength == 2 ? 28L : 9L, strength == 0 ? 25 : strength == 1 ? 48 : 90);
            vibrator.Call("vibrate", effect);
#endif
        }
    }
}
