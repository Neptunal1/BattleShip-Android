using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleShip
{
    [Activity(Label = "gameRand")]
    public class gameFr : Activity , IValueEventListener
    {

        private Button[,] buttons = new Button[10, 10];
        private LinearLayout gameFrGrid;
        private int[,] arrShipPlacel = new int[10, 10];
        //private begame beGamme;
        private begame gameFriend;

        private int count1Hits = 0, count2Hits = 0, count = 0;

        private TextView turnText, player1, player2;
        private bool player1Turn = true;
        private string userName;
        private int wins = 0, losses = 0;

        private DatabaseReference databaseReference;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.gameP);
            buttons = new Button[10, 10];

            gameFriend = new begame();
            //randGame = (begameRand)Intent.GetSerializableExtra("BegameInstance");

            turnText = FindViewById<TextView>(Resource.Id.turnplayer);
            player1 = FindViewById<TextView>(Resource.Id.player1edit);
            player2 = FindViewById<TextView>(Resource.Id.player2edit);

            var prefs = Application.Context.GetSharedPreferences("BattleShipPrefs", FileCreationMode.Private);
            userName = prefs.GetString("username", null);
            wins = prefs.GetInt("wins", 0);                 // 0 is the default value if the key is not found
            losses = prefs.GetInt("losses", 0);            // 0 is the default value if the key is not found

            FirebaseApp.InitializeApp(Application.Context);
            databaseReference = FirebaseDatabase.Instance.Reference;

            Intent serviceIntent = new Intent(this, typeof(MusicService));
            StartService(serviceIntent);
            var usersRef = databaseReference.Child("users").Child(userName);
            usersRef.AddListenerForSingleValueEvent(this); // Add the ValueEventListener to the "users" node


            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    string buttonId = "button" + i + j;
                    int resId = Resources.GetIdentifier(buttonId, "id", PackageName);
                    buttons[i, j] = FindViewById<Button>(resId);
                    buttons[i, j].Click += OnClick;
                }
            }

            InitializeGameBoard();
            UpdateTurnNext();
            arrShipPlacel = SharedPreferencesManager.GetArrayFromSharedPreferences(this, "arrSaveP");
            CheckCount();
        }

        public void InitializeGameBoard()
        {
            buttons = new Button[10, 10];
            gameFrGrid = FindViewById<LinearLayout>(Resource.Id.gameP);
            int margin = 1; // Sets the margin between the button ( in dp)
            int dpMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, margin, Resources.DisplayMetrics);


            //Creating the board
            for (int i = 0; i < 10; i++)
            {
                LinearLayout rowLayout = new LinearLayout(this);
                rowLayout.Orientation = Orientation.Horizontal;

                for (int j = 0; j < 10; j++)
                {
                    Button btn = new Button(this);
                    btn.LayoutParameters = new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.MatchParent, 1);
                    btn.SetBackgroundColor(Color.White);
                    btn.Click += OnClick;

                    var lp = new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.MatchParent, 1);
                    lp.SetMargins(dpMargin, dpMargin, dpMargin, dpMargin);
                    btn.LayoutParameters = lp;

                    buttons[i, j] = btn;
                    rowLayout.AddView(btn);
                }
                gameFrGrid.AddView(rowLayout);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Stop the background music service
            Intent serviceIntent = new Intent(this, typeof(MusicService));
            StopService(serviceIntent);
        }
       
        protected override void OnPause()
        {
            base.OnPause();
            // Stop the background music service
            Intent serviceIntent = new Intent(this, typeof(MusicService));
            StopService(serviceIntent);
        }

        public void OnClick(View v)
        {
        }

        public void OnClick(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int row = -1;
            int column = -1;
            // Find the clicked button's position in the array
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (button == buttons[i, j])
                    {
                        row = i;
                        column = j;
                        break;
                    }
                }
            }
            if (CheckAnyColor(buttons[row, column]) == true)
                return;

            if(arrShipPlacel[row,column] == 1)
            {
                button.SetBackgroundColor(Color.Red);
                arrShipPlacel[row, column] = 0;

                if (player1Turn == true)
                {
                    count1Hits++;
                    player1.Text = "Player #1  Sunk: " + count1Hits  + " Ships";
                }
                else
                {
                    count2Hits++;
                    player2.Text = "Player #2  Sunk: " + count2Hits + " Ships";
                }
            }
            else
            {
                button.SetBackgroundColor(Color.Black);
            }

            player1Turn = !player1Turn;
            UpdateTurnNext();
            CheckWin();

        }
       
        public bool CheckAnyColor(Button btn)
        {
            // Get the current background color of the button
            Android.Graphics.Drawables.ColorDrawable drawable = btn.Background as Android.Graphics.Drawables.ColorDrawable;
            if (drawable != null)
            {
                Color color = drawable.Color;
                if (color == Color.Black || color == Color.Red || color == Color.Blue || color == Color.Yellow)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckColor(Button btn, Color color)
        {
            Color btnColor = (btn.Background as Android.Graphics.Drawables.ColorDrawable).Color;
            return btnColor == color;
        }

        public bool CheckWin()
        {
            // Check if all ships have been sunk
            if (count1Hits + count2Hits == count)
            {
                string winner;
                if (count1Hits > count2Hits)
                {
                    winner = "Player 1";
                    wins++;
                    databaseReference.Child("users").Child(userName).Child("wins").SetValue(wins);

                    var prefs = Application.Context.GetSharedPreferences("BattleShipPrefs", FileCreationMode.Private); // Save username, wins, and losses to SharedPreferences
                    var editor = prefs.Edit();
                    editor.PutString("username", userName);
                    editor.PutInt("wins", wins);
                    editor.PutInt("losses", losses);
                    editor.Apply();
                }
                else if (count1Hits < count2Hits)
                {
                    winner = "Player 2";
                    losses++;
                    databaseReference.Child("users").Child(userName).Child("losses").SetValue(losses);

                    var prefs = Application.Context.GetSharedPreferences("BattleShipPrefs", FileCreationMode.Private); // Save username, wins, and losses to SharedPreferences
                    var editor = prefs.Edit();
                    editor.PutString("username", userName);
                    editor.PutInt("wins", wins);
                    editor.PutInt("losses", losses);
                    editor.Apply();
                }
                else
                {
                    winner = "It's a tie!";
                }

                // Display the winner
                Toast.MakeText(this, $"{winner} wins!", ToastLength.Short).Show();
                DisableButtons(); //Nor the comptuer/player can contiune playing


                new Handler(Looper.MainLooper).PostDelayed(() => // Adding 10 seconds for the player to understand that the game end
                {
                    Intent intent = new Intent(this, typeof(Menu /* or EndGameActivity */));
                    StartActivity(intent);
                }, 10000); // 3000 milliseconds

                return true; // A winner has been announced 
            }

            return false; // No winner yet
        }

        public void UpdateTurnNext()
        {
            if (player1Turn == true)
                turnText.Text = "player 1's turn";
            else
                turnText.Text = "player 2's turn";
        }

        public void DisableButtons()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    buttons[i, j].Enabled = false;
                }
            }
        }

        public int CheckCount()
        {
            for (int i = 0; i < 10;i++)
            {
                for (int j = 0; j < 10; ++j)
                    if (arrShipPlacel[i, j] == 1)
                        count++;
            }
            return count;
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if (snapshot != null && snapshot.Exists())
            {
                // Extract and process the data from the snapshot as needed
                Log.Debug("Firebase", "Data: " + snapshot.Value);
            }
            else
            {
                Log.Debug("Firebase", "No data found.");
            }
        }

        public void OnCancelled(DatabaseError error)
        {
            if (error != null)
            {
                Log.Error("Firebase", "Database Error: " + error.Message);
            }
        }
    }
}