using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase.Database;
using Firebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BattleShip
{
    [Activity(Label = "gameRand")]
    public class gameRand : Activity, IValueEventListener
    {

        private Button[,] buttons = new Button[10, 10];
        private LinearLayout gameRandGrid;
        private begameRand randGame;
        private TextView player, comp;

        private int[,] arrShipPlacel = new int[10, 10];
        private int[,] playerBoard = new int[10, 10];
        private bool playerTurn = true, gameEnd = false; // Player starts first
        private int counterC = 0, counterP = 0, countAll = 0;
        private string userName;
        private int wins = 0, losses = 0;

        private DatabaseReference databaseReference;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.gameC);

            randGame = new begameRand();
            //randGame = (begameRand)Intent.GetSerializableExtra("BegameInstance");
            player = FindViewById<TextView>(Resource.Id.playeredit);
            comp = FindViewById<TextView>(Resource.Id.compedit);
            InitializeGameBoard();

            arrShipPlacel = SharedPreferencesManager.GetArrayFromSharedPreferences(this, "arrSaveP");

            FirebaseApp.InitializeApp(Application.Context);
            databaseReference = FirebaseDatabase.Instance.Reference;

            var prefs = Application.Context.GetSharedPreferences("BattleShipPrefs", FileCreationMode.Private);
            userName = prefs.GetString("username", null);
            wins = prefs.GetInt("wins", 0);                 // 0 is the default value if the key is not found
            losses = prefs.GetInt("losses", 0);            // 0 is the default value if the key is not found

            var usersRef = databaseReference.Child("users").Child(userName);
            usersRef.AddListenerForSingleValueEvent(this); // Add the ValueEventListener to the "users" node

            Intent serviceIntent = new Intent(this, typeof(MusicService));
            StartService(serviceIntent);

        }

        public void InitializeGameBoard()
        {
            buttons = new Button[10, 10];
            gameRandGrid = FindViewById<LinearLayout>(Resource.Id.gamec);
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
                gameRandGrid.AddView(rowLayout);
            }
        }

        public void OnClick(object sender, EventArgs e)
        {
            if (playerTurn == true && !gameEnd)
            {
                Button button = (Button)sender;
                int row = -1;
                int column = -1;

                for (int i = 0; i < 10; i++) //Checking which cell the player chose
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

                if (!CheckAnyColor(buttons[row, column]) == true) //making sure there no ships placed there already
                {
                    if (arrShipPlacel[row, column] == 1)
                    {
                        button.SetBackgroundColor(Color.Red);
                        counterP++;
                        player.Text = "Player Ships Sunk: " + counterP;
                    }
                    else
                    {
                        button.SetBackgroundColor(Color.Black);
                    }
                    CheckWin();
                    playerTurn = false;
                    ComputerTurn();
                }
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

        public void ComputerTurn()
        {
            if (gameEnd)
                return;

            Random random = new Random();
            int row, column;
            do
            {
                row = random.Next(0, 10);
                column = random.Next(0, 10);
            }
            while (CheckAnyColor(buttons[row, column]));

            if (arrShipPlacel[row, column] == 1)
            {
                buttons[row, column].SetBackgroundColor(Color.Blue);
                playerBoard[row, column] = 1;
                counterC++;
                comp.Text = "Computer Ships Sunk: " + counterC;
            }
            else
            {
                buttons[row, column].SetBackgroundColor(Color.Black);
            }
            buttons[row, column].Enabled = false;
            CheckWin();
            if (!gameEnd)
                playerTurn = true;
        }

        public void CheckWin()
        {
            int totalShipCells = NumCells();  // Total number of ship cells on the board
            int totalDiscovered = counterC + counterP;
            if (totalDiscovered == totalShipCells)
            {
                // All ship cells are discovered, determine the winner
                if (counterP > counterC)
                {
                    Toast.MakeText(this, "Player Wins!", ToastLength.Short).Show();
                    gameEnd = true;
                    wins++;
                    databaseReference.Child("users").Child(userName).Child("wins").SetValue(wins);

                    var prefs = Application.Context.GetSharedPreferences("BattleShipPrefs", FileCreationMode.Private); // Save username, wins, and losses to SharedPreferences
                    var editor = prefs.Edit();
                    editor.PutString("username", userName);
                    editor.PutInt("wins", wins);
                    editor.PutInt("losses", losses);
                    editor.Apply();
                }
                else if (counterC > counterP)
                {
                    Toast.MakeText(this, "Computer Wins!", ToastLength.Short).Show();
                    gameEnd = true;
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
                    Toast.MakeText(this, "It's a Tie!", ToastLength.Short).Show();
                    gameEnd = true;
                }

                DisableButtons(); //Nor the comptuer/player can contiune playing


                new Handler(Looper.MainLooper).PostDelayed(() => // Adding 10 seconds for the player to understand that the game end
                {
                    Intent intent = new Intent(this, typeof(Menu /* or EndGameActivity */));
                    StartActivity(intent);
                }, 10000); // 3000 milliseconds delay
            }
        }

        public void OnClick(View v)
        {

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
            var drawable = btn.Background as Android.Graphics.Drawables.ColorDrawable;
            if (drawable != null)
            {
                Color btnColor = drawable.Color;
                return btnColor == color;
            }
            return false;
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

        public int NumCells()
        {
            int countAll = 0;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (arrShipPlacel[i, j] == 1)
                        countAll++;
                }
            }
            return countAll;
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