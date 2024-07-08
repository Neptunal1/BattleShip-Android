using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace BattleShip
{
    [Activity(Label = "register")]
    public class register : Activity, View.IOnClickListener, IValueEventListener
    {
        private Button registerbtn, rbacklogin;
        private EditText userNameUp, passwordUp;
        private DatabaseReference databaseReference;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Register);

            registerbtn = FindViewById<Button>(Resource.Id.rregister);
            rbacklogin = FindViewById<Button>(Resource.Id.rbacklogin);
            userNameUp = FindViewById<EditText>(Resource.Id.rusername);
            passwordUp = FindViewById<EditText>(Resource.Id.rpassword);

            registerbtn.SetOnClickListener(this);
            rbacklogin.SetOnClickListener(this);

            FirebaseApp.InitializeApp(this);  // Initialize Firebase
            databaseReference = FirebaseDatabase.Instance.Reference.Child("users"); // Set up database reference
        }

        public void OnClick(View v)
        {
            if (v == registerbtn)
            {
                Registerbtn_Click();
            }
            else if (v == rbacklogin)
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            }
        }

        // Handle database response
        private void Registerbtn_Click()
        {
            string userName = userNameUp.Text.Trim();
            string password = passwordUp.Text.Trim();

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                Toast.MakeText(this, "Username and password are required", ToastLength.Short).Show();
                return;
            }

            // Check if the user already exists
            databaseReference.Child(userName).AddListenerForSingleValueEvent(this);
        }

        // Event handler for Firebase database value retrieval
        void IValueEventListener.OnCancelled(DatabaseError error)
        {
            RunOnUiThread(() => Toast.MakeText(this, "Error: " + error.Message, ToastLength.Short).Show());
        }

        void IValueEventListener.OnDataChange(DataSnapshot snapshot)
        {
            if (snapshot.Exists())
            {
                // User already exists, show error message
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "User already exists, Login!", ToastLength.Short).Show();
                    Intent intent = new Intent(this, typeof(MainActivity));
                    StartActivity(intent);
                });
            }
            else
            {
                string userName = userNameUp.Text.Trim();
                string password = passwordUp.Text.Trim();

                UserProfile profile = new UserProfile
                {
                    Username = userName,
                    Password = password,
                    Image = "", // or default image value
                    Wins = 0,
                    Losses = 0
                };

                
                databaseReference.Child(userName).Child("username").SetValue(profile.Username);
                databaseReference.Child(userName).Child("password").SetValue(profile.Password);
                databaseReference.Child(userName).Child("image").SetValue(profile.Image);
                databaseReference.Child(userName).Child("wins").SetValue(profile.Wins);
                databaseReference.Child(userName).Child("losses").SetValue(profile.Losses);

                var prefs = Application.Context.GetSharedPreferences("BattleShipPrefs", FileCreationMode.Private); // Save username, wins, and losses to SharedPreferences
                var editor = prefs.Edit();
                editor.PutString("username", userName);
                editor.PutInt("wins", 0);
                editor.PutInt("losses", 0);
                editor.Apply();


                // Registration successful, show toast message
                RunOnUiThread(() => Toast.MakeText(this, "Registration successful", ToastLength.Short).Show());
                StartActivity(new Intent(this, typeof(Menu)));
            }
        }
    }
}
