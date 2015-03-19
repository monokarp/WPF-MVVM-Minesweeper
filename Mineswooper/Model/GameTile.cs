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
        private bool isTraversable=false;
        private bool hasPellet=false;
        #endregion
        #region Public properties
        public Point TilePosition { get; set; }
       
        public bool IsTraversable
        {
            get { return isTraversable; }
            set
            {
                if (value != isTraversable) isTraversable = value;
                NotifyPropertyChanged("IsTraversable");
            }
        }
        public bool HasPellet
        {
            get { return hasPellet; }
            set
            {
                if (value != hasPellet) hasPellet = value;
                NotifyPropertyChanged("HasPellet");
            }
        }
        #endregion
        public GameTile(int a, int b)
        {
            TilePosition = new Point(a, b);
        }
        #region Tile methods
     
        #endregion
    }
}
