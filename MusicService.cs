using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;

namespace BattleShip
{
    [Service]
    public class MusicService : Service
    {
        MediaPlayer mediaPlayer1;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            mediaPlayer1 = MediaPlayer.Create(this, Resource.Raw.backsound);
            mediaPlayer1.Start();
            mediaPlayer1.Looping = true; // Set looping to true here

            return StartCommandResult.NotSticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            mediaPlayer1.Stop();
            mediaPlayer1.Release();
        }
    }
}
