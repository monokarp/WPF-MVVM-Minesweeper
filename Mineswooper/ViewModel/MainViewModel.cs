using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Mineswooper.DAL;
using Mineswooper.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mineswooper.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<ScoreEntry> scores;
        private const int cellSize = 40;
        public int FieldWidth { get; set; }
        public int FieldHeight { get; set; }
        private bool scorePopupOpen = false;
        private bool rulesPopupOpen = false;
        private bool victoryOpen = false;
        private bool _UIEnabled = true;
        private ScoreContext context = new ScoreContext();
        private string gameRules = "It's mineswoopr m8, reveal any tile to start the game.\nLeft click reveals a tile, right click flags a consealed tile, middle click reveals adjacent tiles in case all presumed adjacent mines are flagged. Each revealed tile shows the amount of mines in adjacent tiles.\nFlag every mine on the field or reveal every non-mined tile to win. \nTime elapsed is your final score.";

        private GameField field = new GameField(30, 16, 99);

        #region Relay commands
        public RelayCommand ShutdownCommand { get; set; }
        public RelayCommand<Point> GameTileClick { get; set; }
        public RelayCommand<Point> GameTileMultiClick { get; set; }
        public RelayCommand<Point> GameTileFlag { get; set; }
        public RelayCommand FieldResetCommand { get; set; }
        public RelayCommand ShowRules { get; set; }
        public RelayCommand ShowScores { get; set; }
        public RelayCommand CloseRules { get; set; }
        public RelayCommand CloseScores { get; set; }
        public RelayCommand SaveScore { get; set; }
        public MainViewModel()
        {
            FieldWidth = cellSize * (int)field.Size.X;
            FieldHeight = cellSize * (int)field.Size.Y;
            PlayerScore = new ScoreEntry { Name = "Anonymous", Score = 0 };
            scores = new ObservableCollection<ScoreEntry>(context.ScoreEntries);

            ShutdownCommand = new RelayCommand(() => { context.Dispose(); Application.Current.Shutdown(); });
            GameTileClick = new RelayCommand<Point>((p) => { field.GameTileClick(p); });
            GameTileMultiClick = new RelayCommand<Point>((p) => { field.GameTileMultiClick(p); });
            GameTileFlag = new RelayCommand<Point>((p) => { field.GameTileFlag(p); });
            FieldResetCommand = new RelayCommand(() => { field.Reset(); });

            ShowRules = new RelayCommand(() => { RulesOpen = true; IsUIEnabled = false; });
            CloseRules = new RelayCommand(() => { RulesOpen = false; IsUIEnabled = true; });

            ShowScores = new RelayCommand(() => { ScoreOpen = true; IsUIEnabled = false; });
            CloseScores = new RelayCommand(() => { ScoreOpen = false; IsUIEnabled = true; });

            SaveScore = new RelayCommand(() =>
            {
                scores.Add(PlayerScore);
                context.ScoreEntries.Add(PlayerScore);
                context.SaveChangesAsync();
                VictoryOpen = false;
                IsUIEnabled = true;
            });
            field.PropertyChanged += GameField_PropertyChanged;
        }
        #endregion

        #region Public properties
        public ScoreEntry PlayerScore { get; set; }
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
        public string Rules { get { return gameRules; } }
        public bool IsUIEnabled
        {
            get { return _UIEnabled; }
            set
            {
                if (_UIEnabled == value) return;
                _UIEnabled = value;
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

        public int CellSize { get { return cellSize; } }
        public ObservableCollection<GameTile> Field
        {
            get
            {
                return field.Tiles;
            }
        }

        public int Rows
        {
            get
            {
                return (int)field.Size.Y;
            }
        }

        public int Cols
        {
            get
            {
                return (int)field.Size.X;
            }
        }
        public int Mines
        {
            get
            {
                return field.PresumedMines;
            }

        }
        public int Score
        {
            get
            {
                return field.Score;
            }
        }
        #endregion

        #region Property changed event
        private void GameField_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Score":
                    RaisePropertyChanged("Score");
                    break;
                case "PresumedMines":
                    RaisePropertyChanged("Mines");
                    break;
                case "Victory":
                    PlayerScore.Score = field.Score;
                    PlayerScore.Date = DateTime.Now;
                    VictoryOpen = true;
                    IsUIEnabled = false;
                    break;
            }
        }

        #endregion


    }
}
