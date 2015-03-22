using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Mineswooper.DAL;
using Mineswooper.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Mineswooper.ViewModel
{
    public enum Directions : int { Up = 1, Down = 2, Left = 3, Right = 4 };
    public class MainViewModel : ViewModelBase
    {
        #region Privates
        private ObservableCollection<ScoreEntry> scores;
        private static int cellSize = 40;
        private static int pelletSize = cellSize / 5;
        public static int characterSize = cellSize * 3 / 4;
        private bool scorePopupOpen = false;
        private bool rulesPopupOpen = false;
        private bool victoryOpen = false;
        private bool isUIEnabled = true;
        private string gameRules = "It's pacman m8, rack up those pellets to score and don't get eaten.";
        private GameField field = new GameField(23, 16);
        #endregion
        public MainViewModel()
        {
            PlayerScore = new ScoreEntry { Name = "Anonymous", Score = 0 };
            ShutdownCommand = new RelayCommand(() => { Application.Current.Shutdown(); });
            FieldResetCommand = new RelayCommand(() => { field.ResetField(); field.InitializeField(""); });
            ShowRules = new RelayCommand(() => { RulesOpen = true; IsUIEnabled = false; });
            CloseRules = new RelayCommand(() => { RulesOpen = false; IsUIEnabled = true; });
            ShowScores = new RelayCommand(() => { ScoreOpen = true; IsUIEnabled = false; });
            CloseScores = new RelayCommand(() => { ScoreOpen = false; IsUIEnabled = true; });
            MovePlayer = new RelayCommand<string>((e) =>
            {
                switch (e)
                {
                    case "Up":
                        field.MovePlayer(Directions.Up);
                        break;
                    case "Down":
                        field.MovePlayer(Directions.Down);
                        break;
                    case "Left":
                        field.MovePlayer(Directions.Left);
                        break;
                    case "Right":
                        field.MovePlayer(Directions.Right);
                        break;
                }
                RaisePropertyChanged("Player");
            });
            SaveScore = new RelayCommand(() =>
            {
                var entry = new ScoreEntry { Name = PlayerScore.Name, Date = PlayerScore.Date, Score = PlayerScore.Score };
                scores.Add(entry);
                scores = new ObservableCollection<ScoreEntry>(scores.OrderBy(e => e.Score).ToList());
                RaisePropertyChanged("ScoreEntries");
                using (var dc = new ScoreContext())
                {
                    dc.ScoreEntries.Add(entry);
                    dc.SaveChanges();
                }
                VictoryOpen = false;
                IsUIEnabled = true;
                field.ResetField();
                field.InitializeField("");
            });
            field.PropertyChanged += GameFieldEventHandler;
            using (var dc = new ScoreContext())
            {
                scores = new ObservableCollection<ScoreEntry>(dc.ScoreEntries.Select(e => e).OrderByDescending(e => e.Score).ToList());
            }

        }
        #region Relay commands
        public RelayCommand ShutdownCommand { get; set; }
        public RelayCommand FieldResetCommand { get; set; }
        public RelayCommand ShowRules { get; set; }
        public RelayCommand ShowScores { get; set; }
        public RelayCommand CloseRules { get; set; }
        public RelayCommand CloseScores { get; set; }
        public RelayCommand SaveScore { get; set; }
        public RelayCommand<string> MovePlayer { get; set; }
        #endregion
        #region Public properties
        public ObservableCollection<ScoreEntry> ScoreEntries
        {
            get { return scores; }
            set
            {
                if (scores == value) return;
                scores = value;
                RaisePropertyChanged("ScoreEntries");
            }
        }
        public bool IsUIEnabled
        {
            get { return isUIEnabled; }
            set
            {
                if (isUIEnabled == value) return;
                isUIEnabled = value;
                RaisePropertyChanged("IsUIEnabled");
            }
        }
        public bool RulesOpen
        {
            get { return rulesPopupOpen; }
            set
            {
                if (rulesPopupOpen == value) return;
                rulesPopupOpen = value;
                RaisePropertyChanged("RulesOpen");
            }
        }
        public bool ScoreOpen
        {
            get { return scorePopupOpen; }
            set
            {
                if (scorePopupOpen == value) return;
                scorePopupOpen = value;
                RaisePropertyChanged("ScoreOpen");
            }
        }
        public bool VictoryOpen
        {
            get { return victoryOpen; }
            set
            {
                if (victoryOpen == value) return;
                victoryOpen = value;
                RaisePropertyChanged("VictoryOpen");
            }
        }
        public int FieldWidth { get { return cellSize * (int)field.Size.X; } }
        public int FieldHeight { get { return cellSize * (int)field.Size.Y; } }
        public ScoreEntry PlayerScore { get; set; }
        public string Rules { get { return gameRules; } }
        public static int CellSize { get { return cellSize; } }
        public static int PelletSize { get { return pelletSize; } }
        public static int PelletCenterPosition { get { return cellSize / 2 - pelletSize / 2; } }
        public static int CharacterSize { get { return characterSize; } }
        public static int CharacterCenterPosition { get { return cellSize / 2 - characterSize / 2; } }
        public Point Player { get { return field.Player; } }
        public ObservableCollection<GameTile> Field { get { return field.Tiles; } }
        public ObservableCollection<Point> Ghosts { get { return field.Ghosts; } }
        public int Rows { get { return (int)field.Size.Y; } }
        public int Cols { get { return (int)field.Size.X; } }
        public int Score { get { return field.Score; } }
        #endregion
        private void SaveCurrentMap()//map editing not being used currently
        {
            StringBuilder map = new StringBuilder();
            foreach (var tile in field.Tiles)
            {
                if (tile.IsTraversable) map.Append("T");
                else map.Append("W");
            }
            string strMap = map.ToString();
            System.IO.File.WriteAllText(@"C:\Users\asus.pc\Desktop\job\task_projects\Mineswooper\Mineswooper\Model\defaultMap.txt", strMap);
        }
        private void GameFieldEventHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Score":
                    RaisePropertyChanged("Score");
                    break;
                case "Player":
                    RaisePropertyChanged("Player");
                    break;
                case "HasPellet":
                    RaisePropertyChanged("HasPellet");
                    break;
                case "IsTraversable":
                    RaisePropertyChanged("IsTraversable");
                    break;
                case "Defeat":
                    PlayerScore.Score = field.Score;
                    PlayerScore.Date = DateTime.Now;
                    VictoryOpen = true;
                    IsUIEnabled = false;
                    break;
            }
        }
    }
    #region Value converters
    public class CoordinateToCanvasPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MainViewModel.CellSize * ((int)(double)value - 1);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PlayerPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MainViewModel.CellSize * ((int)(double)value - 1) + MainViewModel.CharacterCenterPosition;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}