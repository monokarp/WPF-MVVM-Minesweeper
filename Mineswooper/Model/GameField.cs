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
        private bool isInitialized;
        private int minesAmout;
        private int flagsCount;
        private int currentScore;
        private DispatcherTimer timer;
        #endregion

        #region Public Properties
        public Point Size { get; set; }
        public ObservableCollection<GameTile> Tiles { get; set; }
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
        public int PresumedMines
        {
            get { return flagsCount; }
            private set
            {
                if (value != flagsCount)
                {
                    flagsCount = value;
                    NotifyPropertyChanged("PresumedMines");
                }
            }
        }
        #endregion

        #region Game field initializers
        public GameField(int cols, int rows, int mines)
        {
            Size = new Point(cols, rows);
            this.minesAmout = mines;
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
            PresumedMines = 0;
            Score = 0;
            timer.Stop();
        }
        private void InitializeField(Point selectedPoint)//places the mines and updates tiles after the first reveal attempt, then performs it
        {
            Random rnd = new Random();
            Point randomPosition;
            for (int count = 0; count < this.minesAmout; )
            {
                randomPosition = new Point(rnd.Next(1, (int)Size.Y), rnd.Next(1, (int)Size.X));
                var randomRow = Math.Abs(randomPosition.X - selectedPoint.X);
                var randomCol = Math.Abs(randomPosition.Y - selectedPoint.Y);
                GameTile selectedTile = Tiles.FirstOrDefault(t => t.TilePosition.X == randomPosition.X && t.TilePosition.Y == randomPosition.Y);
                if (selectedTile != null && !selectedTile.IsMined)
                {
                    if (randomRow > 1 || randomCol > 1)//empty and atleast one tile away from the initial
                    {
                        selectedTile.IsMined = true;
                        count++;
                    } 
                }
            }
            PresumedMines = Tiles.Count(t => t.IsMined);
            foreach (var tile in Tiles) tile.AdjacentMines = Adjacent(tile.TilePosition).FindAll(t => t.IsMined).Count; //setting adjacent mines number
            foreach (var tile in Tiles) tile.UpdateTile();//updating the game tiles
        }
        #endregion

        #region Game tile click handlers
        public void GameTileRevealAttempt(Point selectedPoint)
        {
            GameTile selectedTile = Tiles.FirstOrDefault(t => t.TilePosition.X == selectedPoint.X && t.TilePosition.Y == selectedPoint.Y);
            if (!isInitialized)
            {
                InitializeField(selectedPoint);
                isInitialized = true;
                selectedTile.RevealTile();
                timer.Start();
            }
            if (!selectedTile.IsFlagged)
            {
                if (selectedTile.AdjacentMines != 0 && !selectedTile.IsMined)
                {
                    selectedTile.RevealTile();
                    CheckForVictory();
                }
                else if (selectedTile.IsMined) MinedTileRevealed();
                else CascadeReveal(selectedPoint);
            }
        }
        public void GameTileSetFlag(Point selectedPoint)
        {
            GameTile selectedTile = Tiles.FirstOrDefault(x => x.TilePosition.X == selectedPoint.X && x.TilePosition.Y == selectedPoint.Y);
            if (!selectedTile.IsRevealed && isInitialized)
            {
                if (selectedTile.IsFlagged) PresumedMines++;
                else PresumedMines--;
                selectedTile.ToggleFlag();
                CheckForVictory();
            }
        }
        public void GameTileCascadeRevealAttempt(Point selectedPoint)
        {
            var flaggedAdjacentTiles = Adjacent(selectedPoint).FindAll(t => t.IsFlagged);
            GameTile selectedTile = Tiles.FirstOrDefault(t => t.TilePosition.X == selectedPoint.X && t.TilePosition.Y == selectedPoint.Y);
            if (selectedTile.IsRevealed && flaggedAdjacentTiles.Count == selectedTile.AdjacentMines)
            {
                foreach (var tile in Adjacent(selectedPoint))
                    if (!tile.IsFlagged)
                    {
                        if (!tile.IsMined)
                        {
                            if (tile.AdjacentMines == 0) { CascadeReveal(tile.TilePosition); }
                            tile.RevealTile();
                        }
                        else MinedTileRevealed();
                    }
            }
        }
        #endregion

        #region GameTile Reveal Logic
        public void CascadeReveal(Point selectedPoint)//performs a cascade tile reveal while checking for the defeat conditions
        {
            var emptyAdjacentTiles = new List<GameTile>();
            foreach (var cur in Adjacent(selectedPoint))
            {
                if (cur.AdjacentMines == 0 && !cur.IsRevealed) emptyAdjacentTiles.Add(cur);
            }
            foreach (var cur in Adjacent(selectedPoint))
            {
                if (!cur.IsMined) cur.RevealTile();
                else MinedTileRevealed();
            }
            foreach (var cur in emptyAdjacentTiles) CascadeReveal(cur.TilePosition);
        }
        public List<GameTile> Adjacent(Point selectedPoint)//returns a list of tiles adjacent to the selected one, from 3 to 9 depending on the selected tile position
        {
            var adjacentTiles = new List<GameTile>();
            for (int row = (int)AdjacentPositions.Prev; row <= (int)AdjacentPositions.Next; row++)//adds the whole sqare 
                for (int col = (int)AdjacentPositions.Prev; col <= (int)AdjacentPositions.Next; col++)
                {
                    if (col != 0 || row != 0)//excludes the central tile
                    {
                        GameTile cur = Tiles.FirstOrDefault(x => x.TilePosition.X == selectedPoint.X + row && x.TilePosition.Y == selectedPoint.Y + col);
                        if (cur != null) adjacentTiles.Add(cur);
                    }
                }
            return adjacentTiles;
        }
        public void MinedTileRevealed()//defeat condition met
        {
            foreach (var tile in Tiles) tile.RevealTile();
            timer.Stop();
            NotifyPropertyChanged("Defeat");
        }
        public void CheckForVictory()
        {
            int minedTiles = Tiles.Count(x => x.IsMined);
            int flaggedTiles = Tiles.Count(x => x.IsFlagged && x.IsMined);
            int consealedTiles = Tiles.Count(x => !x.IsRevealed);
            if (flaggedTiles == minedTiles || consealedTiles == minedTiles)//victory condition met
            {
                foreach (var tile in Tiles) tile.RevealTile();
                timer.Stop();
                NotifyPropertyChanged("Victory");
                ResetField();
            }
        }
        #endregion
    }
}
