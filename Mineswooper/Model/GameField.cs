using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace Mineswooper.Model
{
    public class GameField : INotifyPropertyChanged
    {
        private bool isInitialized;
        private int mines;
        private int presumedMines;
        private int score;
        private DispatcherTimer timer;
        public Point Size { get; set; }
        public ObservableCollection<GameTile> Tiles;
        public int Score
        {
            get
            {
                return score;
            }
            private set
            {
                if (value != score)
                {
                    score = value;
                    NotifyPropertyChanged("Score");
                }
            }
        }

        public int PresumedMines
        {
            get
            {
                return presumedMines;
            }
            private set
            {
                if (value != presumedMines)
                {
                    presumedMines = value;
                    NotifyPropertyChanged("PresumedMines");
                }
            }
        }


        #region Game field initializers
        public GameField(int x, int y, int mines)
        {
            Size = new Point(x, y);
            this.mines = mines;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler((s, e) => { Score++; });
            Reset();
        }
        public void Reset()
        {
            if (Tiles != null)
                Tiles.Clear();
            else
                Tiles = new ObservableCollection<GameTile>();
            isInitialized = false;
            for (int i = 1; i <= Size.Y; i++)
                for (int j = 1; j <= Size.X; j++)
                    Tiles.Add(new GameTile(i, j));
            PresumedMines = 0;
            Score = 0;
            timer.Stop();
        }
        private void Initialize(Point point)
        {
            int pool = this.mines;
            Random rnd = new Random();
            Point pos;

            while (pool > 0)
            {
                pos = new Point(rnd.Next(1, (int)Size.Y), rnd.Next(1, (int)Size.X));
                GameTile tile = Tiles.FirstOrDefault(k => k.Position.X == pos.X && k.Position.Y == pos.Y);
                if (tile != null && !tile.Mined)
                {
                    if (Math.Abs(pos.X - point.X) > 1 || Math.Abs(pos.Y - point.Y) > 1)//makes sure it's not already mined and is atleast one tile away from players click
                    {
                        tile.Mined = true;
                        pool--;
                    }
                }
            }
            PresumedMines = Tiles.Count(x => x.Mined);
            SetAdjacentMinesNumber();
            Update();

        }
        private void SetAdjacentMinesNumber()
        {
            foreach (var tile in Tiles)
            {
                tile.AdjacentMines = Adjacent(tile.Position).FindAll(x => x.Mined).Count;
            }
        }
        #endregion

        #region Game tile click handlers
        public void GameTileClick(Point point)
        {

            GameTile selected = Tiles.FirstOrDefault(x => x.Position.X == point.X && x.Position.Y == point.Y);
            if (!isInitialized)
            {
                Initialize(point);
                isInitialized = true;
                selected.Reveal();
                timer.Start();
            }
            if (!selected.Flagged)
            {
                if (selected.AdjacentMines != 0 && !selected.Mined)
                {
                    selected.Reveal();
                    CheckVictory();
                }
                else if (selected.Mined)
                    GameOver();
                else
                    CascadeReveal(point);
            }

        }
        public void GameTileFlag(Point point)
        {
            GameTile selected = Tiles.FirstOrDefault(x => x.Position.X == point.X && x.Position.Y == point.Y);
            if (!selected.Revealed && isInitialized)
            {
                if (selected.Flagged)
                    PresumedMines++;
                else
                    PresumedMines--;
                selected.ToggleFlag();
                CheckVictory();
            }


        }
        public void GameTileMultiClick(Point point)
        {
            GameTile selected = Tiles.FirstOrDefault(x => x.Position.X == point.X && x.Position.Y == point.Y);
            if (selected.Revealed && Adjacent(point).FindAll(x => x.Flagged).Count == selected.AdjacentMines)
                foreach (var tile in Adjacent(point))
                    if (!tile.Flagged)
                    {
                        if (!tile.Mined)
                        {
                            if (tile.AdjacentMines == 0)
                                CascadeReveal(tile.Position);
                            else
                                tile.Reveal();
                        }
                        else
                            GameOver();
                    }
        }
        #endregion

        #region Auxiliary
        public void Update()
        {
            foreach (GameTile tile in Tiles)
                tile.Update();
        }
        public void RevealAdjacent(Point point)
        {
            foreach (var cur in Adjacent(point))
            {
                if (!cur.Mined)
                    cur.Reveal();
                else
                    GameOver();
            }
        }
        public void CascadeReveal(Point point)
        {
            var adj = new List<GameTile>();

            foreach (var cur in Adjacent(point))
                if (cur.AdjacentMines == 0 && !cur.Revealed)
                    adj.Add(cur);
            RevealAdjacent(point);
            foreach (var cur in adj)
                CascadeReveal(cur.Position);
        }
        public List<GameTile> Adjacent(Point point)
        {
            List<GameTile> adj = new List<GameTile>();
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                {
                    var cur = Tiles.FirstOrDefault(x => x.Position.X == point.X + i && x.Position.Y == point.Y + j);
                    if (cur != null)
                        adj.Add(cur);
                }

            adj.Remove(adj.FirstOrDefault(x => x.Position.X == point.X && x.Position.Y == point.Y));
            return adj;
        }
        public void GameOver()
        {
            foreach (var tile in Tiles)
                tile.Reveal();
            timer.Stop();
            MessageBox.Show("BOOM");//!!
            Reset();
        }
        public void CheckVictory()
        {
            //var tilesList = Tiles.ToList();
            var minedTiles = Tiles.Count(x => x.Mined);
            var flaggedTiles = Tiles.Count(x => x.Flagged && x.Mined);
            var consealedTiles = Tiles.Count(x => !x.Revealed);

            if (flaggedTiles == minedTiles || consealedTiles == minedTiles)
            {
                foreach (var tile in Tiles)
                    tile.Reveal();
                timer.Stop();
                NotifyPropertyChanged("Victory");
                Reset();
            }
        }
        #endregion

        #region Property changed event
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
