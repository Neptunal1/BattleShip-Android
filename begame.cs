using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;


namespace BattleShip
{
    [Activity(Label = "begame")]
    public class begame : Activity, View.IOnClickListener
    {
        private ImageButton[,] cells = new ImageButton[10, 10];
        private LinearLayout gameGrid;
        private Button submit, orientation;
        private int[,] arrSaveP = new int[10, 10];
        private int countShips = 0;                 // Number of ships he can place
        private bool isPlaced = false;
        private Android.Widget.Orientation shipOrientation = Android.Widget.Orientation.Horizontal;

        private int[] shipImages = { Resource.Drawable.shipsmall, Resource.Drawable.shipmedium, Resource.Drawable.shipbig };
        private List<int> shipsOnBoard = new List<int>();
        private List<int> removedShips = new List<int>();

        private FrameLayout blackScreen; 


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.beforegame);

            gameGrid = FindViewById<LinearLayout>(Resource.Id.beforegamel);
            submit = FindViewById<Button>(Resource.Id.submit);
            orientation = FindViewById<Button>(Resource.Id.orientation_button);
            orientation.SetOnClickListener(this);
            submit.SetOnClickListener(this);


            //black screen 
            blackScreen = blackScreen = new FrameLayout(this)
            {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
                Background = new ColorDrawable(Color.Black),
                Visibility = ViewStates.Gone // Initially hide the black screen
            };
            AddContentView(blackScreen, blackScreen.LayoutParameters);


            InitializeGameBoard(); // Initialize cells array gameBoard
        }

        private void InitializeGameBoard()
        {
            // Creating the board
            for (int i = 0; i < 10; i++)
            {
                LinearLayout rowLayout = new LinearLayout(this)
                {
                    Orientation = Orientation.Horizontal
                };

                for (int j = 0; j < 10; j++)
                {
                    ImageButton cell = new ImageButton(this)
                    {
                        LayoutParameters = new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.MatchParent, 1)
                    };

                    cell.SetBackgroundColor(Color.White);
                    cell.SetImageResource(Resource.Drawable.WhitePic);
                    cell.SetAdjustViewBounds(true);

                    var lp = new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.MatchParent, 1);
                    int margin = 1; // Sets the margin between the cell (in dp)
                    int dpMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, margin, Resources.DisplayMetrics);
                    lp.SetMargins(dpMargin, dpMargin, dpMargin, dpMargin);
                    cell.LayoutParameters = lp;

                    cell.Id = View.GenerateViewId(); // Generate a unique ID for each cell
                    cell.Click += Cell_Click; // Add click listener

                    cells[i, j] = cell;
                    rowLayout.AddView(cell);
                }
                gameGrid.AddView(rowLayout);
            }
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            if (sender is ImageButton imageButton)
            {
                // Check if the cell contains a ship
                int row = -1, column = -1;

                // Find the clicked cell's position in the array
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (imageButton == cells[i, j])
                        {
                            row = i;
                            column = j;
                            break;
                        }
                    }
                }

                if (row != -1 && column != -1 && arrSaveP[row, column] == 1)
                {
                    // Cell contains a ship, remove it
                    RemoveShip(row, column);
                }
                else
                {
                    // Place a ship in the cell
                    PlaceShip(row, column);
                }
            }
            else
            {
                Log.Error("OnClick Error", "Clicked view is not an ImageButton.");
            }
        }

        public void OnClick(View v)
        {
            if (v.Id == Resource.Id.submit)
            {
                if (countShips < 3)
                {
                    Toast.MakeText(this, "Please place all ships before submitting", ToastLength.Short).Show();
                    return;
                }

                SharedPreferencesManager.SaveArrayToSharedPreferences(this, "arrSaveP", arrSaveP);
                BlackScreen(); // moving into the "gameFr" activity and showing black screen for 3 seconds 

                //Intent intent = new Intent(this, typeof(gameFr));
                //StartActivity(intent);
            }
            else if (v.Id == Resource.Id.orientation_button)
            {
                shipOrientation = (shipOrientation == Android.Widget.Orientation.Horizontal) ? Android.Widget.Orientation.Vertical : Android.Widget.Orientation.Horizontal;
            }
            else if (v is ImageButton imageButton)
            {
                int row = -1, column = -1;

                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (imageButton == cells[i, j])
                        {
                            row = i;
                            column = j;
                            break;
                        }
                    }
                }
                if (row != -1 && column != -1)
                {
                    PlaceShip(row, column);
                }
            }
            else
            {
                Log.Error("OnClick Error", "Clicked view is not an ImageButton.");
            }
        }
    
        private void BlackScreen()
        {
            blackScreen.Visibility = ViewStates.Visible; // Make black screen View visible

            // Delay hiding black screen for 3 seconds
            new Handler().PostDelayed(() =>
            {
                blackScreen.Visibility = ViewStates.Gone;
                Intent intent = new Intent(this, typeof(gameFr));
                StartActivity(intent);
            }, 3000);
        }

        public void PlaceShip(int row, int column)
        {
            int shipSize = 0; // Size of the ship being placed

            switch (countShips)
            {
                case 0:
                    shipSize = 2; // First ship size
                    break;
                case 1:
                    shipSize = 3; // Second ship size
                    break;
                case 2:
                    shipSize = 4; // Third ship size
                    break;
                default:
                    Toast.MakeText(this, "Cannot place more ships", ToastLength.Short).Show();
                    return;
            }

            // Check if there's enough space to place the ship
            if ((shipOrientation == Android.Widget.Orientation.Horizontal && column + shipSize > 10) ||
                (shipOrientation == Android.Widget.Orientation.Vertical && row + shipSize > 10))
            {
                Toast.MakeText(this, "Not enough space to place the ship", ToastLength.Short).Show();
                return;
            }

            for (int i = 0; i < shipSize; i++) // Check if the cells already have ships
            {
                int x = (shipOrientation == Android.Widget.Orientation.Horizontal) ? row : row + i;
                int y = (shipOrientation == Android.Widget.Orientation.Horizontal) ? column + i : column;
                if (arrSaveP[x, y] == 1)
                {
                    Toast.MakeText(this, "Cannot overlap ships", ToastLength.Short).Show();
                    return;
                }
            }

            // Check if the removed ship is in the list of removed ships
            //if (removedShips.Count > 0 && !removedShips.Contains(countShips))
            //{
            //    Toast.MakeText(this, "You can only place a ship that was removed previously.", ToastLength.Short).Show();
            //    return;
            //}

            // Place the ship in the cells
            for (int i = 0; i < shipSize; i++)
            {
                int x = (shipOrientation == Android.Widget.Orientation.Horizontal) ? row : row + i;
                int y = (shipOrientation == Android.Widget.Orientation.Horizontal) ? column + i : column;

                cells[x, y].SetImageResource(shipImages[countShips]);
                cells[x, y].Tag = shipImages[countShips]; // Set the tag to store the resource ID
                arrSaveP[x, y] = 1;
            }

            // If a ship was successfully placed, increment countShips
            countShips++;
            removedShips.Remove(countShips - 1); // Remove the placed ship from removedShips list
        }

        public void RemoveShip(int row, int column)
        {
            if (arrSaveP[row, column] != 1)
            {
                Log.Warn("RemoveShip Warning", "Clicked cell does not contain a ship.");
                return;
            }

            // Determine ship coordinates
            List<(int, int)> shipCoordinates = new List<(int, int)>();
            DetermineShipCoordinates(row, column, ref shipCoordinates);

            int resourceId = (int)cells[row, column].Tag;

            foreach (var (x, y) in shipCoordinates) // Clear ship from grid and array
            {
                cells[x, y].SetImageResource(Resource.Drawable.WhitePic);
                cells[x, y].Tag = Resource.Drawable.WhitePic; // Clear the tag
                arrSaveP[x, y] = 0;
            }

            removedShips.Add(countShips - 1); // Add the removed ship to the list of removed ships by its size
            shipsOnBoard = GetShipsOnBoard(); // Update the list of ships currently on the board
            countShips--; // Decrement countShips after removing a ship
        }

        private void DetermineShipCoordinates(int row, int column, ref List<(int, int)> shipCoordinates)
        {
            // Depth-first search to determine ship coordinates
            Stack<(int, int)> stack = new Stack<(int, int)>();
            stack.Push((row, column));

            while (stack.Count > 0)
            {
                var (x, y) = stack.Pop();

                if (x < 0 || x >= 10 || y < 0 || y >= 10 || arrSaveP[x, y] != 1)
                {
                    continue;
                }

                // Mark as visited
                arrSaveP[x, y] = -1;
                shipCoordinates.Add((x, y));

                // Explore adjacent cells
                stack.Push((x - 1, y)); // Up
                stack.Push((x + 1, y)); // Down
                stack.Push((x, y - 1)); // Left
                stack.Push((x, y + 1)); // Right
            }

            // Restore the visited cells to 1 (since we don't want to change the state in this method)
            foreach (var (x, y) in shipCoordinates)
            {
                arrSaveP[x, y] = 1;
            }
        }

        public List<int> GetShipsOnBoard()
        {
            List<int> shipsOnBoard = new List<int>();

            // Iterate over the arrSaveP array to check the image resources set for each cell
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (arrSaveP[i, j] == 1) // Cell contains a ship
                    {
                        int resourceId = (int)cells[i, j].Tag;
                        if (!shipsOnBoard.Contains(resourceId)) // Check if the ship is not already in the list
                        {
                            shipsOnBoard.Add(resourceId);
                        }
                    }
                }
            }

            return shipsOnBoard;
        }


    }
}


//public void PlaceShip(ImageButton cell)
//{
//    if (counterBig == 3)
//    {
//        Toast.MakeText(this, "Cannot place more ships", ToastLength.Short).Show();
//        return;
//    }

//    int row = -1, column = -1;

//    // Find the clicked cell's position in the array
//    for (int i = 0; i < 10; i++)
//    {
//        for (int j = 0; j < 10; j++)
//        {
//            if (cell == cells[i, j])
//            {
//                row = i;
//                column = j;
//                break;
//            }
//        }
//    }

//    if (row == -1 || column == -1)
//    {
//        Log.Error("PlaceShip Error", "Cell position not found in array.");
//        return;
//    }

//    if (arrSaveP[row, column] == 1)
//    {
//        // Cell already contains a ship, remove it
//        cell.SetImageResource(Resource.Drawable.WhitePic);
//        arrSaveP[row, column] = 0;
//        counterBig--;
//    }
//    else
//    {
//        // Place a ship in the cell
//        cell.SetImageResource(shipImages[counterBig]);
//        arrSaveP[row, column] = 1;
//        counterBig++;
//    }
//}






//public void OnClick(object sender, EventArgs e)
//{
//    if (isPlaced)
//    {

//    }
//    if (counterBig == 3)
//    {
//        Toast.MakeText(this, "cant anymore", ToastLength.Short).Show();
//    }
//    else if (counterBig < 3)
//    {
//        Button button = (Button)sender;
//        int row = -1;
//        int column = -1;

//        bool res = false;  // if res == false ("") res = true ("X")
//                           // Find the clicked button's position in the array
//        for (int i = 0; i < 10; i++)
//        {
//            for (int j = 0; j < 10; j++)
//            {
//                if (button == buttons[i, j])
//                {
//                    row = i;
//                    column = j;
//                    break;
//                }
//            }
//        }

//        // Check if the button already has been selected
//        if (CheckColor(buttons[row, column], Color.Red))
//        {
//            // Remove the X from the button
//            buttons[row, column].SetBackgroundColor(Color.White);
//            res = false;
//        }
//        else
//        {
//            // Place an X on the button
//            buttons[row, column].SetBackgroundColor(Color.Red);
//            res = true;
//        }

//        FindPlaces(row, column, res);
//        SharedPreferencesManager.SaveArrayToSharedPreferences(this, "arrSaveP", arrSaveP);
//    }
//}







//    public void PlaceShip(ImageButton cell)
//{
//    int shipSize = 0; // Size of the ship being placed

//    switch (counterBig)
//    {
//        case 0:
//            shipSize = 2; // First ship size
//            break;
//        case 1:
//            shipSize = 3; // Second ship size
//            break;
//        case 2:
//            shipSize = 4; // Third ship size
//            break;
//        default:
//            Toast.MakeText(this, "Cannot place more ships", ToastLength.Short).Show();
//            return;
//    }

//    int row = -1, column = -1;

//    // Find the clicked cell's position in the array
//    for (int i = 0; i < 10; i++)
//    {
//        for (int j = 0; j < 10; j++)
//        {
//            if (cell == cells[i, j])
//            {
//                row = i;
//                column = j;
//                break;
//            }
//        }
//    }

//    if (row == -1 || column == -1)
//    {
//        Log.Error("PlaceShip Error", "Cell position not found in array.");
//        return;
//    }

//    // Check if there's enough space to place the ship
//    if (column + shipSize > 10)
//    {
//        Toast.MakeText(this, "Not enough space to place the ship", ToastLength.Short).Show();
//        return;
//    }

//    // Check if the cells are already occupied by another ship
//    for (int i = 0; i < shipSize; i++)
//    {
//        if (arrSaveP[row, column + i] == 1)
//        {
//            Toast.MakeText(this, "Cannot overlap ships", ToastLength.Short).Show();
//            return;
//        }
//    }

//    // Place the ship in the cells
//    for (int i = 0; i < shipSize; i++)
//    {
//        cells[row, column + i].SetImageResource(shipImages[counterBig]);
//        arrSaveP[row, column + i] = 1;
//    }

//    counterBig++;
//}




