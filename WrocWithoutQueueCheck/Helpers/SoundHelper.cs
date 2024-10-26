using NAudio.Wave;
using System.Threading;

namespace WrocWithoutQueueCheck.Helpers
{
    public static class SoundHelper
    {
        public static void PlaySuccess() => PlayNotificationSound("./Resources/success.mp3");
        public static void PlayError() => PlayNotificationSound("./Resources/error.mp3");

        private static void PlayNotificationSound(string filePath)
        {
            using (var audioFile = new AudioFileReader(filePath))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
