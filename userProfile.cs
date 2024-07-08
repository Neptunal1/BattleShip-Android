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
using Java.Util;

namespace BattleShip
{
    public class UserProfile
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Image { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
    }


}