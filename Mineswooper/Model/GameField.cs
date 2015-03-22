using Mineswooper.ViewModel;
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
        private static int ghostAmount = 5;
        private static string defaultMap = System.IO.File.ReadAllText(@"C:\Users\asus.pc\Desktop\job\task_projects\Mineswooper\Mineswooper\Model\defaultMap.txt");
        private bool isInitialized;
        private int score;
        private Point player;
        private ObservableCollection<Point> ghosts;
        private DispatcherTimer timer;
        #endregion
        #region Public Properties
        public Point Size { get; set; }
        public ObservableCollection<GameTile> Tiles { get; set; }
        public Point Player
        {
            get { return player; }
            set
            {
                if (value != player)
                {
                    player = value;
                    NotifyPropertyChanged("Player");
                }
            }
        }
        public ObservableCollection<Point> Ghosts
        {
            get { return ghosts; }
            set
            {
                if (value != ghosts)
                {
                    ghosts = value;
                    NotifyPropertyChanged("Ghosts");
                }
            }
        }
        public int Score
        {
            get { return score; }
            private set
            {
                if (value != score)
                {
                    score = value;
                    NotifyPropertyChanged("Score");
                }
            }
        }
        #endregion
        #region Game field initializers
        public GameField(int cols, int rows)
        {
            Size = new Point(cols, rows);
            ResetField();
            InitializeField("");
        }
        public void ResetField()//sets the gamefield to the clear state with no game objects on
        {
            var rnd = new Random();
            isInitialized = false;
            Score = 0;
            Player = default(Point);

            if (Tiles != null) Tiles.Clear();
            else Tiles = new ObservableCollection<GameTile>();
            if (Ghosts != null) Ghosts.Clear();
            else Ghosts = new ObservableCollection<Point>();
            timer = new DispatcherTimer();

            for (int row = 1; row <= Size.Y; row++)
            {
                for (int col = 1; col <= Size.X; col++) Tiles.Add(new GameTile(row, col));
            }
        }
        public void InitializeField(string recievedMap)//sets the reset gamefield to the ready state with all game objects on
        {
            //there has to be some map string validation logic here in case theres a map choice option
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
            for (int ghosts = 0; ghosts < ghostAmount; )
            {
                randomPosition = new Point(rnd.Next(1, (int)Size.Y), rnd.Next(1, (int)Size.X));
                randomTile = Tiles.FirstOrDefault(t => t.TilePosition.X == randomPosition.X && t.TilePosition.Y == randomPosition.Y);
                var adjustedX = Math.Abs(randomPosition.X - Player.X);
                var adjustedY = Math.Abs(randomPosition.Y - Player.Y);
                if (adjustedX > 5 || adjustedY > 5)
                {
                    if (randomTile.IsTraversable)
                    {
                        Ghosts.Add(new Point(randomTile.TilePosition.X, randomTile.TilePosition.Y));
                        ghosts++;
                    }
                }
            }
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += (s, e) =>                                                 //ghost movement
            {
                for (int count = Ghosts.Count; count-- > 0; )
                {
                    var cur = Ghosts[count];
                    List<Directions> randomDirection = TraversibleDirections(cur);
                    MoveCharacter(ref cur, randomDirection[rnd.Next(randomDirection.Count)]);
                    if (CheckCollision(cur)) break;
                    Ghosts[count] = cur;
                }
            };
        }
        #endregion
        #region Game field logic
        public List<Directions> TraversibleDirections(Point p)//checks which directions are traversible from the specified point
        {
            var result = new List<Directions>();
            var up = Tiles.FirstOrDefault(t => t.TilePosition.X == p.X - 1 && t.TilePosition.Y == p.Y);
            var down = Tiles.FirstOrDefault(t => t.TilePosition.X == p.X + 1 && t.TilePosition.Y == p.Y);
            var left = Tiles.FirstOrDefault(t => t.TilePosition.X == p.X && t.TilePosition.Y == p.Y - 1);
            var right = Tiles.FirstOrDefault(t => t.TilePosition.X == p.X && t.TilePosition.Y == p.Y + 1);

            if (up != null && up.IsTraversable) result.Add(Directions.Up);
            if (down != null && down.IsTraversable) result.Add(Directions.Down);
            if (left != null && left.IsTraversable) result.Add(Directions.Left);
            if (right != null && right.IsTraversable) result.Add(Directions.Right);

            return result;
        }
        public void MoveCharacter(ref Point character, Directions direction)//checks if the destination is traversible
        {
            GameTile destination = default(GameTile);
            Point charPosition = new Point(character.X, character.Y);
            switch (direction)
            {
                case Directions.Up:
                    destination = Tiles.FirstOrDefault(t => t.TilePosition.X == charPosition.X - 1 && t.TilePosition.Y == charPosition.Y);
                    break;
                case Directions.Down:
                    destination = Tiles.FirstOrDefault(t => t.TilePosition.X == charPosition.X + 1 && t.TilePosition.Y == charPosition.Y);
                    break;
                case Directions.Left:
                    destination = Tiles.FirstOrDefault(t => t.TilePosition.X == charPosition.X && t.TilePosition.Y == charPosition.Y - 1);
                    break;
                case Directions.Right:
                    destination = Tiles.FirstOrDefault(t => t.TilePosition.X == charPosition.X && t.TilePosition.Y == charPosition.Y + 1);
                    break;
            }
            if (destination != default(GameTile))
            {
                if (destination.IsTraversable)
                {
                    character = new Point(destination.TilePosition.X, destination.TilePosition.Y);
                    if (!isInitialized)
                    {
                        timer.Start();
                        isInitialized = true;
                    }
                }
            }
        }
        public void MovePlayer(Directions direction)
        {
            MoveCharacter(ref player, direction);
            foreach (var g in ghosts) CheckCollision(g);
            var cur = Tiles.FirstOrDefault(t => t.TilePosition.X == player.X && t.TilePosition.Y == player.Y);
            if (cur != default(GameTile))
            {
                if (cur.HasPellet)
                {
                    Score += 100;
                    cur.HasPellet = false;
                }
            }
            if (Tiles.Count(t => t.HasPellet) == 0)
            {
                timer.Stop();
                timer.IsEnabled = false;
                NotifyPropertyChanged("Victory");
            }
        }
        public bool CheckCollision(Point p)
        {
            if (p.X == player.X && p.Y == player.Y)
            {
                timer.Stop();
                timer.IsEnabled = false;
                NotifyPropertyChanged("Defeat");
                return true;
            }
            return false;
        }

        public void ToggleTraversable(Point position)//map editing not being used currently
        {
            var cur = Tiles.FirstOrDefault(t => t.TilePosition.X == position.X && t.TilePosition.Y == position.Y);
            if (cur.IsTraversable) cur.IsTraversable = false;
            else cur.IsTraversable = true;
        }
        #endregion
    }
}
