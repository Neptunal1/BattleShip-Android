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
    [Activity(Label = "Menu")]
    public class Menu : Activity, View.IOnClickListener
    {
        Button beforegame, aboutme, howtoplay, profilepage;
        string username;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            beforegame = FindViewById<Button>(Resource.Id.beforeg);
            aboutme = FindViewById<Button>(Resource.Id.maboutme);
            howtoplay = FindViewById<Button>(Resource.Id.mhowplay);
            profilepage = FindViewById<Button>(Resource.Id.fprofile);

            beforegame.SetOnClickListener(this);
            aboutme.SetOnClickListener(this);
            howtoplay.SetOnClickListener(this);
            profilepage.SetOnClickListener(this);

        }

        public void OnClick(View v)
        {
            if (v == beforegame)
            {
                Intent intent = new Intent(this, typeof(GameMode));
                StartActivity(intent);
            }

            if (v == aboutme)
            {
                Intent intent = new Intent(this, typeof(aboutme));
                StartActivity(intent);
            }

            if (v == howtoplay)
            {
                Intent intent = new Intent(this, typeof(howtoplay));
                StartActivity(intent);
            }
            if (v == profilepage)
            {
                Intent intent = new Intent(this, typeof(profiles));
                StartActivity(intent);
            }
        }
    }
}