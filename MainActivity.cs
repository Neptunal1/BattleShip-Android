using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Xamarin.Essentials;
using Firebase.Database;
using Firebase;
using System;
using System.Collections.Generic;
using System.Collections;

namespace BattleShip
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, View.IOnClickListener, IValueEventListener
    {
        private Button login, goregister;
        private EditText userNameIn, passwordIn;
        private DatabaseReference databaseReference;

        private string currentUser;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.login);
            userNameIn = FindViewById<EditText>(Resource.Id.lusername);
            passwordIn = FindViewById<EditText>(Resource.Id.lpassword);
            login = FindViewById<Button>(Resource.Id.login);
            goregister = FindViewById<Button>(Resource.Id.lsignup);
            login.SetOnClickListener(this);
            goregister.SetOnClickListener(this);

            FirebaseApp.InitializeApp(Application.Context);
            databaseReference = FirebaseDatabase.Instance.Reference;

        }
        public void OnClick(View v)  
        {
            if (v == login)
            {
                string userName = userNameIn.Text, password = passwordIn.Text;
                SignInUser(userName, password);
            }
            if (v == goregister)
            {
                Intent intent = new Intent(this, typeof(register));
                StartActivity(intent);
            }
        }

        private void SignInUser(string userName, string password)
        {
            var usersRef = databaseReference.Child("users");
            usersRef.Child(userName).AddListenerForSingleValueEvent(this); // Check if the user exists in the database
        }

        void IValueEventListener.OnCancelled(DatabaseError error) //  Event handler for Firebase database value retrieval
        {
            RunOnUiThread(() => Toast.MakeText(this, "Failed to check user", ToastLength.Short).Show());
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if (snapshot.Exists() && snapshot.Value != null)
            {
                try
                {
                    //Getting the infro from the fireabse
                    string passwordFromDb = snapshot.Child("password").Value?.ToString();
                    var winsData = snapshot.Child("wins").GetValue(true);
                    int wins = Convert.ToInt32(winsData);
                    var lossesData = snapshot.Child("losses").GetValue(true);
                    int losses = Convert.ToInt32(lossesData);

                    if (passwordFromDb != null && passwordFromDb == passwordIn.Text)
                    {
                        currentUser = snapshot.Key;
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "Sign in successful", ToastLength.Short).Show();

                            var prefs = Application.Context.GetSharedPreferences("BattleShipPrefs", FileCreationMode.Private); // Save username, wins, and losses to SharedPreferences
                            var editor = prefs.Edit();
                            editor.PutString("username", currentUser);
                            editor.PutInt("wins", wins); 
                            editor.PutInt("losses", losses);
                            editor.Apply();

                            Intent intent = new Intent(this, typeof(Menu));
                            StartActivity(intent);
                        });
                    }
                    else
                    {
                        RunOnUiThread(() => Toast.MakeText(this, "Incorrect password", ToastLength.Short).Show());
                    }
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() => Toast.MakeText(this, "Error: " + ex.Message, ToastLength.Short).Show());
                }
            }
            else
            {
                RunOnUiThread(() => Toast.MakeText(this, "User does not exist", ToastLength.Short).Show());
            }
        }

    }
}
