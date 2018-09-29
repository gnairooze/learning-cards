using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for MaterialWindow.xaml
    /// </summary>
    public partial class MaterialWindow : Window
    {
        public MaterialWindow()
        {
            InitializeComponent();
        }

        #region attributes
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
                if (this.PositionHistoryIndex == -1)
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
            bool succeeded = loadData(Properties.Settings.Default.DataPath);
            if(!succeeded)
            {
                return;
            }

            setEventHandlers();

            setContextMenu();

            resetControls();
           
            setTimer();

            start();
        }

        private void start()
        {
            displayModel(getModel());
            _Timer.Start();
        }

        private void setEventHandlers()
        {
            _Timer.Tick += new EventHandler(Timer_Tick);
        }

        private void resetControls()
        {
            setBackgoundColor();
            setWindowSize();
            setFontSize(Properties.Settings.Default.FontSize);
            setContentAlignment();
            setLocationAlignment();
            setAlwaysOnTop();
            setFullscreen(Properties.Settings.Default.Fullscreen);
        }

        private void setContextMenu()
        {
            displayFontSizeOnContextMenu();
            displayPlayRandomOnContextMenu();
            displayIntervalOnContextMenu();
            displayAlwaysOnTopOnContextMenu();
            displayFullscreenOnContextMenu();
            displayAlignContentOnContextMenu();
            displayAlignLocationOnContextMenu();
        }

        private bool loadData(string dataPath)
        {
            List<Model> models; string message;
            this.Title = Properties.Settings.Default.Title;

            bool loadedSuccessfully = Data.LoadData(dataPath, true, out models, out message);

            if (!loadedSuccessfully)
            {
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (models.Count == 0)
            {
                MessageBox.Show("no content found", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            this._Models = models;

            this.Position = -1;
            this.PositionHistoryIndex = -1;
            btnForward.IsEnabled = false;
            btnBack.IsEnabled = false;
            this.PositionHistory.Clear();

            return true;
        }

        private void setContentAlignment()
        {
            switch (Properties.Settings.Default.ContentAlign.ToLower())
            {
                case "left":
                    txtContent.TextAlignment = TextAlignment.Left;
                    break;
                case "right":
                    txtContent.TextAlignment = TextAlignment.Right;
                    break;
                case "center":
                    txtContent.TextAlignment = TextAlignment.Center;
                    break;
            }
        }

        private void setLocationAlignment()
        {
            switch (Properties.Settings.Default.LocationAlign.ToLower())
            {
                case "left":
                    txtblkLinkContent.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case "right":
                    txtblkLinkContent.HorizontalAlignment = HorizontalAlignment.Right;
                    break;
                case "center":
                    txtblkLinkContent.HorizontalAlignment = HorizontalAlignment.Center;
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveCurrentWindowSize();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                if (Properties.Settings.Default.Fullscreen)
                {
                    setFullscreen(false);
                    displayFullscreenOnContextMenu();
                }
            }

            e.Handled = true;
        }
        #endregion

        #region context menu event handlers
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

        private void mnuAlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            MenuItem myItem = (MenuItem)e.Source;

            if (myItem.IsCheckable)
            {
                Properties.Settings.Default.AlwaysOnTop = myItem.IsChecked;
                Properties.Settings.Default.Save();

                setAlwaysOnTop();
            }

            e.Handled = true;
        }

        private void mnuFullscreen_Click(object sender, RoutedEventArgs e)
        {
            MenuItem myItem = (MenuItem)e.Source;

            if (myItem.IsCheckable)
            {
                Properties.Settings.Default.Fullscreen = myItem.IsChecked;
                Properties.Settings.Default.Save();

                setFullscreen(myItem.IsChecked);
            }

            e.Handled = true;
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

        private void mnuSetFontSize_Click(object sender, RoutedEventArgs e)
        {
            MenuItem myItem = (MenuItem)e.Source;

            switch (myItem.Name)
            {
                case "mnuFont8":
                    Properties.Settings.Default.FontSize = 8;
                    break;
                case "mnuFont10":
                    Properties.Settings.Default.FontSize = 10;
                    break;
                case "mnuFont12":
                    Properties.Settings.Default.FontSize = 12;
                    break;
                case "mnuFont14":
                    Properties.Settings.Default.FontSize = 14;
                    break;
                case "mnuFont16":
                    Properties.Settings.Default.FontSize = 16;
                    break;
                case "mnuFont18":
                    Properties.Settings.Default.FontSize = 18;
                    break;
                case "mnuFont20":
                    Properties.Settings.Default.FontSize = 20;
                    break;
                case "mnuFont22":
                    Properties.Settings.Default.FontSize = 22;
                    break;
                case "mnuFont24":
                    Properties.Settings.Default.FontSize = 24;
                    break;
                case "mnuFont26":
                    Properties.Settings.Default.FontSize = 26;
                    break;
                case "mnuFont28":
                    Properties.Settings.Default.FontSize = 28;
                    break;
                case "mnuFont30":
                    Properties.Settings.Default.FontSize = 30;
                    break;
                case "mnuFont32":
                    Properties.Settings.Default.FontSize = 32;
                    break;
                case "mnuFont34":
                    Properties.Settings.Default.FontSize = 34;
                    break;
                case "mnuFont36":
                    Properties.Settings.Default.FontSize = 36;
                    break;
                case "mnuFont38":
                    Properties.Settings.Default.FontSize = 38;
                    break;
                case "mnuFont40":
                    Properties.Settings.Default.FontSize = 40;
                    break;
                case "mnuFont80":
                    Properties.Settings.Default.FontSize = 80;
                    break;
                case "mnuFont120":
                    Properties.Settings.Default.FontSize = 120;
                    break;
            }

            Properties.Settings.Default.Save();

            displayFontSizeOnContextMenu();
            setFontSize(Properties.Settings.Default.FontSize);
        }

        private void mnuAlignContent_Click(object sender, RoutedEventArgs e)
        {
            MenuItem myItem = (MenuItem)e.Source;

            switch (myItem.Name)
            {
                case "mnuAlignContentLeft":
                    Properties.Settings.Default.ContentAlign = "Left";
                    break;
                case "mnuAlignContentCenter":
                    Properties.Settings.Default.ContentAlign = "Center";
                    break;
                case "mnuAlignContentRight":
                    Properties.Settings.Default.ContentAlign = "Right";
                    break;
            }

            Properties.Settings.Default.Save();

            displayAlignContentOnContextMenu();
            setContentAlignment();
        }

        private void mnuAlignLocation_Click(object sender, RoutedEventArgs e)
        {
            MenuItem myItem = (MenuItem)e.Source;

            switch (myItem.Name)
            {
                case "mnuAlignLocationLeft":
                    Properties.Settings.Default.LocationAlign = "Left";
                    break;
                case "mnuAlignLocationCenter":
                    Properties.Settings.Default.LocationAlign = "Center";
                    break;
                case "mnuAlignLocationRight":
                    Properties.Settings.Default.LocationAlign = "Right";
                    break;
            }

            Properties.Settings.Default.Save();

            displayAlignLocationOnContextMenu();
            setLocationAlignment();
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

        private void displayFontSizeOnContextMenu()
        {
            switch (Properties.Settings.Default.FontSize)
            {
                case 8:
                    uncheckFontSize();
                    radioFont8.IsChecked = true;
                    break;
                case 10:
                    uncheckFontSize();
                    radioFont10.IsChecked = true;
                    break;
                case 12:
                    uncheckFontSize();
                    radioFont12.IsChecked = true;
                    break;
                case 14:
                    uncheckFontSize();
                    radioFont14.IsChecked = true;
                    break;
                case 16:
                    uncheckFontSize();
                    radioFont16.IsChecked = true;
                    break;
                case 18:
                    uncheckFontSize();
                    radioFont18.IsChecked = true;
                    break;
                case 20:
                    uncheckFontSize();
                    radioFont20.IsChecked = true;
                    break;
                case 22:
                    uncheckFontSize();
                    radioFont22.IsChecked = true;
                    break;
                case 24:
                    uncheckFontSize();
                    radioFont24.IsChecked = true;
                    break;
                case 26:
                    uncheckFontSize();
                    radioFont26.IsChecked = true;
                    break;
                case 28:
                    uncheckFontSize();
                    radioFont28.IsChecked = true;
                    break;
                case 30:
                    uncheckFontSize();
                    radioFont30.IsChecked = true;
                    break;
                case 32:
                    uncheckFontSize();
                    radioFont32.IsChecked = true;
                    break;
                case 34:
                    uncheckFontSize();
                    radioFont34.IsChecked = true;
                    break;
                case 36:
                    uncheckFontSize();
                    radioFont36.IsChecked = true;
                    break;
                case 38:
                    uncheckFontSize();
                    radioFont38.IsChecked = true;
                    break;
                case 40:
                    uncheckFontSize();
                    radioFont40.IsChecked = true;
                    break;
                case 80:
                    uncheckFontSize();
                    radioFont80.IsChecked = true;
                    break;
                case 120:
                    uncheckFontSize();
                    radioFont120.IsChecked = true;
                    break;
            }
        }

        private void displayAlignContentOnContextMenu()
        {
            switch (Properties.Settings.Default.ContentAlign.ToLower())
            {
                case "left":
                    uncheckContentAlign();
                    radioAlignContentLeft.IsChecked = true;
                    break;
                case "center":
                    uncheckContentAlign();
                    radioAlignContentCenter.IsChecked = true;
                    break;
                case "right":
                    uncheckContentAlign();
                    radioAlignContentRight.IsChecked = true;
                    break;
            }
        }

        private void displayAlignLocationOnContextMenu()
        {
            switch (Properties.Settings.Default.LocationAlign.ToLower())
            {
                case "left":
                    uncheckLocationAlign();
                    radioAlignLocationLeft.IsChecked = true;
                    break;
                case "center":
                    uncheckLocationAlign();
                    radioAlignLocationCenter.IsChecked = true;
                    break;
                case "right":
                    uncheckLocationAlign();
                    radioAlignLocationRight.IsChecked = true;
                    break;
            }
        }

        private void displayFullscreenOnContextMenu()
        {
            mnuFullscreen.IsChecked = Properties.Settings.Default.Fullscreen;
        }

        private void displayAlwaysOnTopOnContextMenu()
        {
            mnuAlwaysOnTop.IsChecked = Properties.Settings.Default.AlwaysOnTop;
        }

        private void displayPlayRandomOnContextMenu()
        {
            mnuPlayRandom.IsChecked = Properties.Settings.Default.PlayRandom;
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

        private void uncheckFontSize()
        {
            radioFont8.IsChecked = false;
            radioFont10.IsChecked = false;
            radioFont12.IsChecked = false;
            radioFont14.IsChecked = false;
            radioFont16.IsChecked = false;
            radioFont18.IsChecked = false;
            radioFont20.IsChecked = false;
            radioFont22.IsChecked = false;
            radioFont24.IsChecked = false;
            radioFont26.IsChecked = false;
            radioFont28.IsChecked = false;
            radioFont30.IsChecked = false;
            radioFont32.IsChecked = false;
            radioFont34.IsChecked = false;
            radioFont36.IsChecked = false;
            radioFont38.IsChecked = false;
            radioFont40.IsChecked = false;
        }

        private void uncheckContentAlign()
        {
            radioAlignContentLeft.IsChecked = false;
            radioAlignContentCenter.IsChecked = false;
            radioAlignContentRight.IsChecked = false;
        }

        private void uncheckLocationAlign()
        {
            radioAlignLocationLeft.IsChecked = false;
            radioAlignLocationCenter.IsChecked = false;
            radioAlignLocationRight.IsChecked = false;
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

        #region import export handlers
        private void mnuImportCSV_Click(object sender, RoutedEventArgs e)
        {
            if (_Timer.IsEnabled)
            {
                btnPause_Click(sender, e);
            }
            
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "CSV packages (*.zip)|*.zip|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string message;
                Data.ImportCSVPackage(openFileDialog.FileName, out message);

                if (!string.IsNullOrEmpty(message))
                {
                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                }

                loadSettingsFromFile();

                loadData(Properties.Settings.Default.DataPath);

                setContextMenu();

                resetControls();

                btnPause_Click(sender, e);
            }
        }

        private void loadSettingsFromFile()
        {
            var xml = new System.Xml.XmlDocument();
            xml.Load(@".\Resources\app.config");

            var settings = xml.SelectSingleNode("configuration/userSettings/LearningCards.Properties.Settings");

            for (int i = 0; i < settings.ChildNodes.Count; i++)
            {
                var setting = settings.ChildNodes[i];
                string value = setting.ChildNodes[0].InnerText;

                switch (setting.Attributes["name"].Value)
                {
                    case "Interval":
                        Properties.Settings.Default.Interval = int.Parse(value);
                        break;
                    case "FontSize":
                        Properties.Settings.Default.FontSize = int.Parse(value);
                        break;
                    case "Top":
                        Properties.Settings.Default.Top = int.Parse(value);
                        break;
                    case "Left":
                        Properties.Settings.Default.Left = int.Parse(value);
                        break;
                    case "Height":
                        Properties.Settings.Default.Height = int.Parse(value);
                        break;
                    case "Width":
                        Properties.Settings.Default.Width = int.Parse(value);
                        break;
                    case "Maximized":
                        Properties.Settings.Default.Maximized = bool.Parse(value);
                        break;
                    case "PlayRandom":
                        Properties.Settings.Default.PlayRandom = bool.Parse(value);
                        break;
                    case "BackgroundColor":
                        Properties.Settings.Default.BackgroundColor = value;
                        break;
                    case "DataPath":
                        Properties.Settings.Default.DataPath = value;
                        break;
                    case "ContentAlign":
                        Properties.Settings.Default.ContentAlign = value;
                        break;
                    case "LocationAlign":
                        Properties.Settings.Default.LocationAlign = value;
                        break;
                    case "AlwaysOnTop":
                        Properties.Settings.Default.AlwaysOnTop = bool.Parse(value);
                        break;
                    case "Fullscreen":
                        Properties.Settings.Default.Fullscreen = bool.Parse(value);
                        break;
                    case "Title":
                        Properties.Settings.Default.Title = value;
                        break;
                }
            }

            Properties.Settings.Default.Save();

            xml = null;
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

        #region change screen elements
        private void setAlwaysOnTop()
        {
            this.Topmost = Properties.Settings.Default.AlwaysOnTop;
        }

        private void setFullscreen(bool isFullscreen)
        {
            Properties.Settings.Default.Fullscreen = isFullscreen;
            Properties.Settings.Default.Save();

            if (isFullscreen)
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
                this.Topmost = true;
            }
            else
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                setWindowSize();
                setAlwaysOnTop();
            }
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
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
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

            if (csvColor.Length != 4)
            {
                MessageBox.Show("Invalid background color [argb]", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            byte[] scvColorValues = new byte[4];
            bool succeeded = byte.TryParse(csvColor[0], out scvColorValues[0]);
            succeeded &= byte.TryParse(csvColor[1], out scvColorValues[1]);
            succeeded &= byte.TryParse(csvColor[2], out scvColorValues[2]);
            succeeded &= byte.TryParse(csvColor[3], out scvColorValues[3]);

            if (!succeeded)
            {
                MessageBox.Show("Invalid background color [argb]", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SolidColorBrush color = new SolidColorBrush(Color.FromArgb(scvColorValues[0], scvColorValues[1], scvColorValues[2], scvColorValues[3]));

            grdMain.Background = color;
            txtContent.Background = color;
            txtContent.BorderBrush = color;
        }
        #endregion

        #region play cards
        private void refreshPositionControls()
        {
            //history is enabled and the current model did not reach the end of history
            if (this.PositionHistoryIndex != -1 && this.PositionHistoryIndex < this.PositionHistory.Count - 1)
            {
                btnForward.IsEnabled = true;
            }
            else
            {
                btnForward.IsEnabled = false;
            }

            if (this.PositionHistoryIndex != 0 && this.PositionHistory.Count > 1)
            {
                btnBack.IsEnabled = true;
            }
            else
            {
                btnBack.IsEnabled = false;
            }
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

            if (model.ImageLocation != string.Empty)
            {
                FileStream stream = File.OpenRead(System.IO.Path.GetFullPath(model.ImageLocation));

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                imgMain.Source = bitmap;

                stream.Close();
                stream.Dispose();
                stream = null;
            }

            string cardNoContent = $"{this.Position + 1} - {this.Models.Count}";
            lblCardNo.Content = cardNoContent;
        }

        private Model getModel()
        {
            if (Properties.Settings.Default.PlayRandom)
            {
                this.Position = _Random.Next(0, this.Models.Count);
            }
            else
            {
                if (this.Position + 1 >= this.Models.Count)
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

        private string pluralS(int n)
        {
            if (n > 1)
            {
                return "s";
            }

            return string.Empty;
        }

        private void pausePlay(string action)
        {
            switch (action.ToLower())
            {
                case "pause":
                    _Timer.Stop();
                    this.cntntPause.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                    this.Title += " - Paused";
                    break;
                case "play":
                    _Timer.Start();
                    this.cntntPause.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                    this.Title = Properties.Settings.Default.Title;
                    break;
            }
        }
        #endregion

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

            if (Properties.Settings.Default.AlwaysOnTop)
            {
                settings.AppendLine("+ Always on top is active");
            }
            else
            {
                settings.AppendLine("+ Always on top is not active");
            }

            if (Properties.Settings.Default.Fullscreen)
            {
                settings.AppendLine("+ Fullscreen is active");
            }
            else
            {
                settings.AppendLine("+ Fullscreen is not active");
            }

            settings.AppendLine($"+ Content is aligned to {Properties.Settings.Default.ContentAlign}");
            settings.AppendLine($"+ Location is aligned to {Properties.Settings.Default.LocationAlign}");

            settings.AppendLine($"+ Background color (a,r,g,b): {Properties.Settings.Default.BackgroundColor}");

            return settings.ToString();
        }

    }
}
