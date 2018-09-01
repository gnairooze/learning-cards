using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LearningCards
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region attributes
        private const string TITLE = "Learning Cards";
        private List<Model> _Models;
        private DispatcherTimer _Timer = new DispatcherTimer();
        private Random _Random = new Random();
        private List<int> _PositionHistory = new List<int>();
        private int _Position = 0;
        #endregion

        #region properties
        internal List<Model> Models
        {
            get
            {
                return _Models;
            }
        }

        internal int Position
        {
            get
            {
                return this._Position;
            }
            set
            {
                this._Position = value;
                if(this.PositionHistoryIndex == -1)
                {
                    this.PositionHistory.Add(this._Position);
                }
                refreshPositionControls();
            }
        }

        internal List<int> PositionHistory
        {
            get
            {
                return this._PositionHistory;
            }
        }

        internal int PositionHistoryIndex { get; set; }
        #endregion

        #region window event handlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<Model> models; string message;
            _Timer.Tick += new EventHandler(Timer_Tick);
            this.Position = -1;
            this.PositionHistoryIndex = -1;
            btnForward.IsEnabled = false;
            btnBack.IsEnabled = false;
            setBackgoundColor();
            setWindowSize();

            bool loadedSuccessfully = Data.LoadData(Properties.Settings.Default.DataPath, true, out models, out message);

            if (!loadedSuccessfully)
            {
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (models.Count == 0)
            {
                MessageBox.Show("no content found", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this._Models = models;

            setFontSize(Properties.Settings.Default.FontSize);
            mnuPlayRandom.IsChecked = Properties.Settings.Default.PlayRandom;

            displayIntervalOnContextMenu();

            setTimer();

            displayModel(getModel());
            _Timer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveCurrentWindowSize();
        }
        #endregion

        #region context menu event handlers
        private void mnuFontIncrease_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.FontSize += 2;
            Properties.Settings.Default.Save();

            setFontSize(Properties.Settings.Default.FontSize);
        }

        private void mnuFontDecrease_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.FontSize -= 2;
            Properties.Settings.Default.Save();

            setFontSize(Properties.Settings.Default.FontSize);
        }

        private void mnuPlayRandom_Click(object sender, RoutedEventArgs e)
        {
            MenuItem myItem = (MenuItem)e.Source;

            if (myItem.IsCheckable)
            {
                Properties.Settings.Default.PlayRandom = myItem.IsChecked;
                Properties.Settings.Default.Save();
            }

            e.Handled = true;
        }

        private void mnuPausePlay_Click(object sender, RoutedEventArgs e)
        {
            string header = ((MenuItem)e.Source).Header.ToString();

            pausePlay(header);
        }

        private void mnuInterval_Click(object sender, RoutedEventArgs e)
        {
            MenuItem myItem = (MenuItem)e.Source;

            switch (myItem.Name)
            {
                case "mnuInterval3s":
                    Properties.Settings.Default.Interval = 3;
                    break;
                case "mnuInterval10s":
                    Properties.Settings.Default.Interval = 10;
                    break;
                case "mnuInterval20s":
                    Properties.Settings.Default.Interval = 20;
                    break;
                case "mnuInterval30s":
                    Properties.Settings.Default.Interval = 30;
                    break;
                case "mnuInterval1":
                    Properties.Settings.Default.Interval = 60;
                    break;
                case "mnuInterval3":
                    Properties.Settings.Default.Interval = 180;
                    break;
                case "mnuInterval5":
                    Properties.Settings.Default.Interval = 300;
                    break;
                case "mnuInterval10":
                    Properties.Settings.Default.Interval = 600;
                    break;
                case "mnuInterval15":
                    Properties.Settings.Default.Interval = 900;
                    break;
                case "mnuInterval30":
                    Properties.Settings.Default.Interval = 1800;
                    break;
                case "mnuInterval60":
                    Properties.Settings.Default.Interval = 3600;
                    break;
            }

            Properties.Settings.Default.Save();

            setTimer();
            displayIntervalOnContextMenu();
        }

        private void mnuViewSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(readSettings(), "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region history buttons event handlers
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.PositionHistoryIndex == 0 || this.PositionHistory.Count <= 1)
            {
                return;
            }

            if (this.PositionHistoryIndex == -1)
            {
                this.PositionHistoryIndex = this.PositionHistory.Count - 2;
            }
            else
            {
                if (this.PositionHistoryIndex >= 1)
                {
                    this.PositionHistoryIndex--;
                }
            }

            this.Position = this.PositionHistory.ElementAt(this.PositionHistoryIndex);

            Model model = this.Models.ElementAt(this.Position);

            displayModel(model);

            refreshPositionControls();
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            if (this.PositionHistoryIndex == -1 || this.PositionHistoryIndex == this.PositionHistory.Count - 1)
            {
                this.PositionHistoryIndex = -1;
                return;
            }

            this.PositionHistoryIndex++;
            this.Position = this.PositionHistory.ElementAt(PositionHistoryIndex);

            Model model = this.Models.ElementAt(this.Position);

            displayModel(model);

            refreshPositionControls();
        }
        #endregion

        #region  other event handlers
        private void lnkLocation_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (this.PositionHistoryIndex == -1)
            {
                displayModel(getModel());

                refreshPositionControls();
            }
            else
            {
                btnForward_Click(this, new RoutedEventArgs());
            }

        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (_Timer.IsEnabled)
            {
                pausePlay("pause");
            }
            else
            {
                pausePlay("play");
            }
        }
        #endregion

        private void refreshPositionControls()
        {
            //history is enabled and the current model did not reach the end of history
            if(this.PositionHistoryIndex != -1 && this.PositionHistoryIndex < this.PositionHistory.Count - 1)
            {
                btnForward.IsEnabled = true;
            }
            else
            {
                btnForward.IsEnabled = false;
            }
            
            if(this.PositionHistoryIndex != 0 && this.PositionHistory.Count > 1)
            {
                btnBack.IsEnabled = true;
            }
            else
            {
                btnBack.IsEnabled = false;
            }
        }

        private void displayIntervalOnContextMenu()
        {
            switch (Properties.Settings.Default.Interval)
            {
                case 3:
                    uncheckIntervals();
                    radioInterval3s.IsChecked = true;
                    break;
                case 10:
                    uncheckIntervals();
                    radioInterval10s.IsChecked = true;
                    break;
                case 20:
                    uncheckIntervals();
                    radioInterval20s.IsChecked = true;
                    break;
                case 30:
                    uncheckIntervals();
                    radioInterval30s.IsChecked = true;
                    break;
                case 60:
                    uncheckIntervals();
                    radioInterval1.IsChecked = true;
                    break;
                case 180:
                    uncheckIntervals();
                    radioInterval3.IsChecked = true;
                    break;
                case 300:
                    uncheckIntervals();
                    radioInterval5.IsChecked = true;
                    break;
                case 600:
                    uncheckIntervals();
                    radioInterval10.IsChecked = true;
                    break;
                case 900:
                    uncheckIntervals();
                    radioInterval15.IsChecked = true;
                    break;
                case 1800:
                    uncheckIntervals();
                    radioInterval30.IsChecked = true;
                    break;
                case 3600:
                    uncheckIntervals();
                    radioInterval60.IsChecked = true;
                    break;
            }
        }

        private void uncheckIntervals()
        {
            radioInterval3s.IsChecked = false;
            radioInterval10s.IsChecked = false;
            radioInterval20s.IsChecked = false;
            radioInterval30s.IsChecked = false;
            radioInterval1.IsChecked = false;
            radioInterval3.IsChecked = false;
            radioInterval5.IsChecked = false;
            radioInterval10.IsChecked = false;
            radioInterval15.IsChecked = false;
            radioInterval30.IsChecked = false;
            radioInterval60.IsChecked = false;
        }

        private void setTimer()
        {
            int minutes = Properties.Settings.Default.Interval / 60;
            int seconds = Properties.Settings.Default.Interval % 60;

            _Timer.Interval = new TimeSpan(0, minutes, seconds);
        }

        private void displayModel(Model model)
        {
            txtContent.Text = model.Content;
            txtContent.FlowDirection = model.LTR ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
            lnkLocation.NavigateUri = model.URL;
            lnkContent.Text = model.Location;
            lnkContent.FlowDirection = model.LTR ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;

            if(model.ImageLocation != string.Empty)
            {
                imgMain.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(model.ImageLocation)));
            }
            
            lblCardNo.Content = $"{this.Position+1} - {this.Models.Count}";
        }

        private Model getModel()
        {
            if (Properties.Settings.Default.PlayRandom)
            {
                this.Position = _Random.Next(0, this.Models.Count);
            }
            else
            {
                if(this.Position + 1 >= this.Models.Count)
                {
                    this.Position = 0;
                }
                else
                {
                    this.Position++;
                }
            }

            return this.Models.ElementAt(this.Position);
        }

        private void setFontSize(int fontSize)
        {
            txtContent.FontSize = fontSize;
            txtblkLinkContent.FontSize = fontSize;
        }

        private void setWindowSize()
        {
            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;

            if (Properties.Settings.Default.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void saveCurrentWindowSize()
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = this.Top;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Maximized = false;
            }

            Properties.Settings.Default.Save();
        }

        private void setBackgoundColor()
        {
            string[] csvColor = Properties.Settings.Default.BackgroundColor.Split(",".ToCharArray());

            if(csvColor.Length != 4)
            {
                MessageBox.Show("Invalid background color [argb]", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            byte[] scvColorValues = new byte[4];
            bool succeeded = byte.TryParse(csvColor[0], out scvColorValues[0]);
            succeeded &= byte.TryParse(csvColor[1], out scvColorValues[1]);
            succeeded &= byte.TryParse(csvColor[2], out scvColorValues[2]);
            succeeded &= byte.TryParse(csvColor[3], out scvColorValues[3]);

            if(!succeeded)
            {
                MessageBox.Show("Invalid background color [argb]", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SolidColorBrush color = new SolidColorBrush(Color.FromArgb(scvColorValues[0], scvColorValues[1], scvColorValues[2], scvColorValues[3]));

            grdMain.Background = color;
            txtContent.Background = color;
            txtContent.BorderBrush = color;
        }

        private string pluralS(int n)
        {
            if (n > 1)
            {
                return "s";
            }

            return string.Empty;
        }

        private string readSettings()
        {
            StringBuilder settings = new StringBuilder();
            settings.AppendLine($"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
            settings.AppendLine();
            settings.AppendLine($"+ Data file: {System.IO.Path.GetFullPath(Properties.Settings.Default.DataPath)}");
            settings.AppendLine();
            settings.AppendLine($"+ Font-Size: {Properties.Settings.Default.FontSize}");

            int minutes = Properties.Settings.Default.Interval / 60;
            int seconds = Properties.Settings.Default.Interval % 60;

            settings.AppendLine($"+ Play every {minutes} minute{pluralS(minutes)}, {seconds} second{pluralS(seconds)}");
            if (Properties.Settings.Default.PlayRandom)
            {
                settings.AppendLine("+ Play in random mode");
            }
            else
            {
                settings.AppendLine("+ Play in standard mode");
            }

            settings.AppendLine($"+ Window position (left, top): ({Properties.Settings.Default.Left}, {Properties.Settings.Default.Top})");
            settings.AppendLine($"+ Window size (width, height): ({Properties.Settings.Default.Width}, {Properties.Settings.Default.Height})");

            if (Properties.Settings.Default.Maximized)
            {
                settings.AppendLine("+ Window is maximized");
            }
            else
            {
                settings.AppendLine("+ Window is not maximized");
            }


            settings.AppendLine($"+ Background color (a,r,g,b): {Properties.Settings.Default.BackgroundColor}");

            return settings.ToString();
        }

        private void pausePlay(string action)
        {
            switch (action.ToLower())
            {
                case "pause":
                    _Timer.Stop();
                    mnuPausePlay.Header = btnPause.Content = "Play";
                    this.Title += " - Paused";
                    break;
                case "play":
                    _Timer.Start();
                    mnuPausePlay.Header = btnPause.Content = "Pause";
                    this.Title = TITLE;
                    break;
            }
        }
    }
}
