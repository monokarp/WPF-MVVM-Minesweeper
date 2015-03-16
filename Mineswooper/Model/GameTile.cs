using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Mineswooper.Model
{
    public class GameTile : INotifyPropertyChanged
    {
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
        #region Privates
        private string tileContent;
        private bool isRevealed;
        private bool isFlagged;
        #endregion
        #region Public properties
        public Point TilePosition { get; set; }
        public bool IsMined { get; set; }
        public int AdjacentMines { get; set; }
        public string TileContent
        {
            get { return tileContent; }
            set
            {
                if (value != tileContent) tileContent = value;
                NotifyPropertyChanged("TileContent");
            }
        }
        public bool IsRevealed
        {
            get { return isRevealed; }
            set
            {
                if (value != isRevealed) isRevealed = value;
                NotifyPropertyChanged("IsRevealed");
            }
        }
        public bool IsFlagged
        {
            get { return isFlagged; }
            set
            {
                if (value != isFlagged) isFlagged = value;
                NotifyPropertyChanged("IsFlagged");
            }
        }
        #endregion
        public GameTile(int a, int b)
        {
            TilePosition = new Point(a, b);
            IsMined = false;
            IsRevealed = false;
            IsFlagged = false;
            AdjacentMines = 0;
            TileContent = "";
        }
        #region Tile methods
        public void UpdateTile()
        {
            if (IsRevealed)
            {
                if (IsMined) TileContent = "*";
                else if (AdjacentMines != 0) TileContent = AdjacentMines.ToString();
            }
        }
        public void RevealTile()
        {
            if (!IsRevealed)
            {
                IsRevealed = true;
                UpdateTile();
            }
        }
        public void ToggleFlag()
        {
            if (!IsFlagged)
            {
                TileContent = "!";
                IsFlagged = true;
            }
            else
            {
                TileContent = "";
                IsFlagged = false;
            }
        }
        #endregion
    }
}
