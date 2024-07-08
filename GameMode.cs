using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleShip
{
    [Activity(Label = "GameMode")]
    public class GameMode : Activity, View.IOnClickListener
    {
        Button computer, friend;
        private string username;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.choosemode);
            computer = FindViewById<Button>(Resource.Id.compgame);
            friend = FindViewById<Button>(Resource.Id.frgame);
            computer.SetOnClickListener(this);
            friend.SetOnClickListener(this);

            username = Intent.GetStringExtra("username") ?? string.Empty;
        }

        public void OnClick(View v)
        {
            if (v == computer)
            {
                Intent intent = new Intent(this, typeof(begameRand));
                intent.PutExtra("username", username);
                StartActivity(intent);
            }

            if (v == friend)
            {
                Intent intent = new Intent(this, typeof(begame));
                intent.PutExtra("username", username);
                StartActivity(intent);
            }
        }
    }
}