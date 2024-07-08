using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleShip
{
    [Activity(Label = "begameRand")]
    public class begameRand : Activity, View.IOnClickListener
    {
        private Button[,] buttons = new Button[10, 10];
        private LinearLayout gameGrid;
        private Button submit, regenerate;
        private int[,] arrSaveP = new int[10, 10];
        private List<Ship> ships = new List<Ship>();
        private Random random = new Random(); // Random object for ship placement
        private FrameLayout blackScreen;

        protected override void OnCreate(Bundle savedInstanceState) 
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.berandg);

            buttons = new Button[10, 10];
            submit = FindViewById<Button>(Resource.Id.submit);
            submit.SetOnClickListener(this);
            regenerate = FindViewById<Button>(Resource.Id.regenerate);
            regenerate.SetOnClickListener(this);

            //black screen 
            blackScreen = blackScreen = new FrameLayout(this)
            {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
                Background = new ColorDrawable(Color.Black),
                Visibility = ViewStates.Gone // Initially hide the black screen
            };
            AddContentView(blackScreen, blackScreen.LayoutParameters);


            GameBoard();
            PlaceShipsRandomly();

            Color color = Color.Red;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (arrSaveP[i, j] == 1)
                    {
                        buttons[i, j].SetBackgroundColor(color);
                    }
                }
            }
        }

        public void GameBoard()
        {
            buttons = new Button[10, 10];
            gameGrid = FindViewById<LinearLayout>(Resource.Id.begamer);
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
                gameGrid.AddView(rowLayout);
            }
        }

        public void PlaceShipsRandomly()
        {
            // Define sizes of ships
            int[] shipSizes = { 2, 3, 3, 4, 5 };
            foreach (int size in shipSizes)
            {
                Ship ship = new Ship(size);
                bool placed = false;

                while (!placed)
                {
                    int direction = random.Next(2); // 0 for horizontal, 1 for vertical
                    int row = random.Next(10);
                    int column = random.Next(10);

                    if (CanPlaceShip(row, column, size, direction))
                    {
                        PlaceShip(ship, row, column, size, direction);
                        placed = true;
                    }
                }
                ships.Add(ship);
            }
        }

        public bool CanPlaceShip(int row, int column, int size, int direction)
        {
            // Check if the ship will fit within the boundaries of the game board
            if ((direction == 0 && column + size > 10) || (direction == 1 && row + size > 10))
            {
                return false;
            }
            // Check if the ship overlaps with any existing ships
            for (int i = 0; i < size; i++)
            {
                if (direction == 0 && arrSaveP[row, column + i] == 1)
                {
                    return false;
                }
                else if (direction == 1 && arrSaveP[row + i, column] == 1)
                {
                    return false;
                }
            }

            return true;
        }

        private void ClearShips()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    arrSaveP[i, j] = 0;
                    buttons[i, j].SetBackgroundColor(Color.White);
                }
            }

        }

        public void PlaceShip(Ship ship, int row, int column, int size, int direction)
        {
            for (int i = 0; i < size; i++)
            {
                if (direction == 0)
                {
                    arrSaveP[row, column + i] = 1;
                    ship.AddCoordinate(row, column + i);
                }
                else
                {
                    arrSaveP[row + i, column] = 1;
                    ship.AddCoordinate(row + i, column);
                }
            }
        }

        public void OnClick(View v)
        {
            SharedPreferencesManager.SaveArrayToSharedPreferences(this, "arrSaveP", arrSaveP);
            if (submit == v)
            {
                BlackScreen();
            }
            else if (v == regenerate)
            {
                ClearShips();
                PlaceShipsRandomly();
                Color color = Color.Red;
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (arrSaveP[i, j] == 1)
                        {
                            buttons[i, j].SetBackgroundColor(color);
                        }
                    }
                }
            }
        }

        public void OnClick(object sender, EventArgs e)
        {
        }

        private void BlackScreen()
        {
            blackScreen.Visibility = ViewStates.Visible; // Make black screen View visible

            // Delay hiding black screen for 3 seconds
            new Handler().PostDelayed(() =>
            {
                blackScreen.Visibility = ViewStates.Gone;
                Intent intent = new Intent(this, typeof(gameRand));
                StartActivity(intent);
            }, 3000);
        }
    }

}