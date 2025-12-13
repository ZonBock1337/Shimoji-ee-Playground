using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using System.Windows.Threading;

namespace ShimejiPlaygroundApp
{
    [Serializable]
    public class EditorSettings
    {
        public string WindowTitle = "Shimeji Playground";
        public double WindowWidth = 960;
        public double WindowHeight = 540;
        public string BackgroundPath = "assets/playground.png";
        public bool StartDirectPlayground = false;
    }

    public partial class EditorWindow : Window
    {
        private PlaygroundWindow playground;
        private string settingsFile = "editor_settings.xml";
        private EditorSettings settings;

        public EditorWindow()
        {
            InitializeComponent();
            LoadSettings();
            ApplySettingsToUI();

            InputBindings.Add(new KeyBinding(
                new RelayCommand(() => ReturnFromPlayground()),
                new KeyGesture(Key.S, ModifierKeys.Control)));

            if (settings.StartDirectPlayground)
                Dispatcher.BeginInvoke(new Action(LaunchPlayground), DispatcherPriority.ApplicationIdle);
        }

        private void LoadSettings()
        {
            if (File.Exists(settingsFile))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EditorSettings));
                    using (var stream = File.OpenRead(settingsFile))
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
                settings.WindowWidth = double.TryParse(WidthTextBox.Text, out double w) ? w : 500;
                settings.WindowHeight = double.TryParse(HeightTextBox.Text, out double h) ? h : 500;
                settings.BackgroundPath = SelectedImageText.Text;
                settings.StartDirectPlayground = StartDirectPlaygroundCheckBox.IsChecked ?? false;

                XmlSerializer serializer = new XmlSerializer(typeof(EditorSettings));
                using (var stream = File.Create(settingsFile))
                    serializer.Serialize(stream, settings);
            }
        }

        private void ApplySettingsToUI()
        {
            WindowTitleTextBox.Text = settings.WindowTitle;
            WidthTextBox.Text = settings.WindowWidth.ToString();
            HeightTextBox.Text = settings.WindowHeight.ToString();
            SelectedImageText.Text = settings.BackgroundPath;
            StartDirectPlaygroundCheckBox.IsChecked = settings.StartDirectPlayground;

            if (File.Exists(settings.BackgroundPath))
                PreviewImage.Source = new BitmapImage(new Uri(settings.BackgroundPath, UriKind.RelativeOrAbsolute));
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
            if (dlg.ShowDialog() == true)
            {
                SelectedImageText.Text = dlg.FileName;
                PreviewImage.Source = new BitmapImage(new Uri(dlg.FileName, UriKind.RelativeOrAbsolute));
            }
        }

        private void ResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset all settings?", "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                settings = new EditorSettings();
                ApplySettingsToUI();
            }
        }

        private void RunButton_Click(object sender, RoutedEventArgs e) => LaunchPlayground();

        private void LaunchPlayground()
        {
            // SaveSettings();
            playground = new PlaygroundWindow(settings, () =>
            {
                this.Show();
            });
            playground.Show();
            this.Hide();
        }

        private void ReturnFromPlayground()
        {
            if (playground != null)
            {
                playground.Close();
                this.Show();
            }
        }

        private void EditorWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.X && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                ReturnFromPlayground();
            else if (e.Key == Key.R && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                LaunchPlayground();
            else if (e.Key == Key.S && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                SaveSettings();
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action action;
        public RelayCommand(Action a) { action = a; }
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => action();
    }
}