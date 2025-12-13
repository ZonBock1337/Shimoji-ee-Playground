using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using System.Windows.Threading;

namespace ShimojiPlaygroundApp
{
    [Serializable]
    public class EditorSettings
    {
        public string WindowTitle = "Shimoji Playground";
        public double WindowWidth = 960;
        public double WindowHeight = 540;
        public string TopOverlayPath = "";
        public string BottomOverlayPath = "";
        public string LeftOverlayPath = "";
        public string RightOverlayPath = "";
        public double TopHeight = 100;
        public double BottomHeight = 100;
        public double LeftWidth = 50;
        public double RightWidth = 50;
        public bool StartDirectPlayground = false;
        public string SelectedPlayground = "Basic Playground";
        public bool MainWindowTopMost = false;
        public string BackgroundPath = "";
    }

    public class PlaygroundItem
    {
        public string Name { get; set; }
        public BitmapImage Icon { get; set; }
        public string Path { get; set; }
    }

    public partial class EditorWindow : Window
    {
        private EditorSettings settings;
        private string settingsFile = "editor_settings.xml";
        private ObservableCollection<PlaygroundItem> playgrounds = new ObservableCollection<PlaygroundItem>();
        private DispatcherTimer updateTimer;

        public EditorWindow()
        {
            InitializeComponent();
            PlaygroundComboBox.ItemsSource = playgrounds;

            LoadSettings();
            ApplySettingsToUI();
            LoadPlaygrounds();
            checkSkipEditor();

            updateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            updateTimer.Tick += (s, e) => LoadPlaygrounds();
            updateTimer.Start();
        }

        private void LoadSettings()
        {
            if (File.Exists(settingsFile))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EditorSettings));
                    using var stream = File.OpenRead(settingsFile);
                    settings = (EditorSettings)serializer.Deserialize(stream);
                }
                catch { settings = new EditorSettings(); }
            }
            else settings = new EditorSettings();
        }

        private void SaveSettings()
        {
            if (MessageBox.Show("Save settings? (Can't be undo)", "Save Settings", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                settings.WindowTitle = WindowTitleTextBox.Text;
                settings.WindowWidth = double.TryParse(WidthTextBox.Text, out double w) ? w : 960;
                settings.WindowHeight = double.TryParse(HeightTextBox.Text, out double h) ? h : 540;
                settings.TopOverlayPath = TopOverlayText.Text;
                settings.BottomOverlayPath = BottomOverlayText.Text;
                settings.LeftOverlayPath = LeftOverlayText.Text;
                settings.RightOverlayPath = RightOverlayText.Text;
                settings.TopHeight = double.TryParse(TopHeightText.Text, out double th) ? th : 100;
                settings.BottomHeight = double.TryParse(BottomHeightText.Text, out double bh) ? bh : 100;
                settings.LeftWidth = double.TryParse(LeftWidthText.Text, out double lw) ? lw : 50;
                settings.RightWidth = double.TryParse(RightWidthText.Text, out double rw) ? rw : 50;
                settings.StartDirectPlayground = StartDirectPlaygroundCheckBox.IsChecked ?? false;
                settings.SelectedPlayground = PlaygroundComboBox.SelectedItem is PlaygroundItem pi ? pi.Name : "Basic Playground";
                settings.BackgroundPath = BackgroundText.Text;
                settings.MainWindowTopMost = TopMostMainWindowCheckbox.IsChecked ?? false;

        XmlSerializer serializer = new XmlSerializer(typeof(EditorSettings));
                using var stream = File.Create(settingsFile);
                serializer.Serialize(stream, settings);
            }
        }

        private void ApplySettingsToUI()
        {
            WindowTitleTextBox.Text = settings.WindowTitle;
            WidthTextBox.Text = settings.WindowWidth.ToString();
            HeightTextBox.Text = settings.WindowHeight.ToString();
            TopHeightText.Text = settings.TopHeight.ToString();
            BottomHeightText.Text = settings.BottomHeight.ToString();
            LeftWidthText.Text = settings.LeftWidth.ToString();
            RightWidthText.Text = settings.RightWidth.ToString();
            BackgroundText.Text = settings.BackgroundPath.ToString();
            StartDirectPlaygroundCheckBox.IsChecked = settings.StartDirectPlayground;
            TopMostMainWindowCheckbox.IsChecked = settings.MainWindowTopMost;
        }

        private void LoadPlaygrounds()
        {
            string path = "playgrounds";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var dirs = Directory.GetDirectories(path).Select(d => Path.GetFileName(d)).ToList();

            foreach (var dir in dirs)
            {
                if (!playgrounds.Any(p => p.Name == dir))
                {
                    string folderPath = Path.Combine(path, dir);
                    string iconPath = Path.Combine(folderPath, "icon.png");
                    BitmapImage icon = LoadBitmap(iconPath);

                    playgrounds.Add(new PlaygroundItem
                    {
                        Name = dir,
                        Icon = icon,
                        Path = folderPath
                    });
                }
            }

            for (int i = playgrounds.Count - 1; i >= 0; i--)
            {
                if (!dirs.Contains(playgrounds[i].Name))
                    playgrounds.RemoveAt(i);
            }

            if (PlaygroundComboBox.SelectedItem == null && playgrounds.Any())
            {
                var savedPg = playgrounds.FirstOrDefault(p => p.Name == settings.SelectedPlayground);
                PlaygroundComboBox.SelectedItem = savedPg ?? playgrounds.First();
            }
        }

        private BitmapImage LoadBitmap(string path)
        {
            if (!File.Exists(path)) return null;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(Path.GetFullPath(path), UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private void PlaygroundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaygroundComboBox.SelectedItem is not PlaygroundItem selected) return;

            string folderPath = selected.Path;

            string mainImg = Path.Combine(folderPath, "playground.png");
            if (File.Exists(mainImg))
            {
                BackgroundText.Text = mainImg;
                PreviewImage.Source = LoadBitmap(mainImg);
            }
            else
            {
                BackgroundText.Text = "";
                PreviewImage.Source = null;
            }

            TopOverlayText.Text = File.Exists(Path.Combine(folderPath, "top.png")) ? Path.Combine(folderPath, "top.png") : "";
            BottomOverlayText.Text = File.Exists(Path.Combine(folderPath, "bottom.png")) ? Path.Combine(folderPath, "bottom.png") : "";
            LeftOverlayText.Text = File.Exists(Path.Combine(folderPath, "left.png")) ? Path.Combine(folderPath, "left.png") : "";
            RightOverlayText.Text = File.Exists(Path.Combine(folderPath, "right.png")) ? Path.Combine(folderPath, "right.png") : "";

            string settingsTxt = Path.Combine(folderPath, "settings.txt");
            if (File.Exists(settingsTxt))
            {
                foreach (var line in File.ReadAllLines(settingsTxt))
                {
                    if (line.StartsWith("TopHeight=") && double.TryParse(line.Substring(10), out double th))
                        TopHeightText.Text = th.ToString();
                    else if (line.StartsWith("BottomHeight=") && double.TryParse(line.Substring(13), out double bh))
                        BottomHeightText.Text = bh.ToString();
                    else if (line.StartsWith("LeftWidth=") && double.TryParse(line.Substring(10), out double lw))
                        LeftWidthText.Text = lw.ToString();
                    else if (line.StartsWith("RightWidth=") && double.TryParse(line.Substring(11), out double rw))
                        RightWidthText.Text = rw.ToString();
                }
            }
            if (!File.Exists(mainImg))
                MessageBox.Show($"Playground not found:\n{mainImg}");
        }

        private void ResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Reset settings? (Can't be undo)", "Reset Settings", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                settings = new EditorSettings();
                ApplySettingsToUI();
            }
        }

        private void OpenPlaygroundsFolder_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "playgrounds");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void LaunchPlayground()
        {
            PlaygroundWindow pg = new PlaygroundWindow(settings, ReturnFromPlayground);
            pg.Show();
            this.Hide();
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e) => SaveSettings();

        private void checkSkipEditor()
        {
            if (settings.StartDirectPlayground)
            {
                LaunchPlayground();
            }
        }

        private void checkTopMost()
        {
            if (settings.MainWindowTopMost)
            {
                settings.MainWindowTopMost = true;
            }
            else
            {
                settings.MainWindowTopMost = false;
            }
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            LaunchPlayground();
        }

        private void ReturnFromPlayground() => this.Show();

        private void EditorWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.X)
                ReturnFromPlayground();
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.R)
                RunButton_Click(null, null);
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.S)
                SaveSettings();
        }

        private void TopMostMainWindowCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            checkTopMost();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}