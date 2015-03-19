using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace Mineswooper.Model
{
    public class GameField : INotifyPropertyChanged
    {
        #region Property changed event
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Privates
        private enum AdjacentPositions : int { Prev = -1, Cur = 0, Next = 1 };
        private static string defaultMap = System.IO.File.ReadAllText(@"C:\Users\asus.pc\Desktop\job\task_projects\Mineswooper\Mineswooper\Model\defaultMap.txt");
        private bool isInitialized;
        private int currentScore;
        private Point player;
        private DispatcherTimer timer;
        #endregion

        #region Public Properties
        public Point Size { get; set; }
        public ObservableCollection<GameTile> Tiles { get; set; }
        public Point Player
        {
            get { return player; }
            private set
            {
                if (value != player)
                {
                    player = value;
                    NotifyPropertyChanged("Player");
                }
            }
        }
        public ObservableCollection<Point> Ghosts { get; private set; }
        public int Score
        {
            get { return currentScore; }
            private set
            {
                if (value != currentScore)
                {
                    currentScore = value;
                    NotifyPropertyChanged("Score");
                }
            }
        }
        #endregion

        #region Game field initializers
        public GameField(int cols, int rows)
        {
            Size = new Point(cols, rows);
            Ghosts = new ObservableCollection<Point>();
            Player = default(Point);
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += (s, e) => Score++;
            ResetField();
        }
        public void ResetField()
        {
            if (Tiles != null) Tiles.Clear();
            else Tiles = new ObservableCollection<GameTile>();
            isInitialized = false;
            for (int row = 1; row <= Size.Y; row++)
            {
                for (int col = 1; col <= Size.X; col++) Tiles.Add(new GameTile(row, col));
            }
            InitializeField("");
            Score = 0;
            timer.Stop();
        }
        private void InitializeField(string recievedMap)
        {
            //there has to be some validation logic
            Random rnd = new Random();
            Point randomPosition;
            GameTile randomTile;
            char[] map = defaultMap.ToCharArray();
            for (int count = 0; count < map.Length; count++)
            {
                if (map[count] == 'T') { Tiles[count].IsTraversable = true; Tiles[count].HasPellet = true; }
            }
            while (Player == default(Point))
            {
                randomPosition = new Point(rnd.Next(1, (int)Size.Y), rnd.Next(1, (int)Size.X));
                randomTile = Tiles.FirstOrDefault(t => t.TilePosition.X == randomPosition.X && t.TilePosition.Y == randomPosition.Y);
                if (randomTile.IsTraversable)
                {
                    Player = new Point(randomTile.TilePosition.X, randomTile.TilePosition.Y);
                    randomTile.HasPellet = false;
                }
            }
            isInitialized = true;
        }
        #endregion
        public void ToggleTraversable(Point position)
        {
            var cur = Tiles.FirstOrDefault(t => t.TilePosition.X == position.X && t.TilePosition.Y == position.Y);
            if (cur.IsTraversable) cur.IsTraversable = false;
            else cur.IsTraversable = true;
        }

    }
}
