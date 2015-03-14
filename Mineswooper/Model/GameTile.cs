using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mineswooper.Model
{
    public class GameTile : INotifyPropertyChanged
    {
        private string content;
        private bool revealed;
        private bool flagged;
        public GameTile(int a, int b)
        {
            Position = new Point(a, b);
            Mined = false;
            Revealed = false;// good for debugging
            Flagged = false;
            AdjacentMines = 0;
            Content = "";
        }
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

        #region Public properties
        public Point Position { get; set; }
        public bool Mined { get; set; }
        public int AdjacentMines { get; set; }
        public string Content
        {
            get
            {
                return content;
            }
            set
            {
                if (value != content)
                {
                    content = value;
                    NotifyPropertyChanged("Content");
                }
            }
        }

        public bool Revealed
        {
            get
            {
                return revealed;
            }
            set
            {
                if (value != revealed)
                    revealed = value;
                NotifyPropertyChanged("Revealed");
            }
        }
        public bool Flagged
        {
            get { return flagged; }
            set
            {
                if (value != flagged)
                    flagged = value;
                NotifyPropertyChanged("Flagged");
            }
        }

        #endregion

        #region Tile methods
        public void Update()
        {
            if (Revealed)
            {
                
                if (Mined)
                    Content = "*";
                else if (AdjacentMines != 0)
                    Content = AdjacentMines.ToString();
                //Content = Position.ToString();
            }
        }
        public void Reveal()
        {
            if (!Revealed)
            {
                Revealed = true;
                Update();
            }
        }
        public void ToggleFlag()
        {
            if (!Flagged)
            {
                Content = "!";
                Flagged = true;
            }
            else
            {
                Content = "";
                Flagged = false;
            }
        }
        #endregion
    }
}
