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
    public class Ship
    {
        public int Size { get; }
        public List<int[]> Coordinates { get; }

        public Ship(int size)
        {
            Size = size;
            Coordinates = new List<int[]>();
        }

        public void AddCoordinate(int row, int column)
        {
            Coordinates.Add(new int[] { row, column });
        }
    }

}