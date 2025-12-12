using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Input;

namespace ShimejiPlaygroundApp
{
    public class PlaygroundWindow : Window
    {
        private EditorSettings settings;
        private DispatcherTimer checkPositionTimer;
        private DateTime lastCheckTime;
        private Action returnToEditorAction;

        private const double OutOfScreenMargin = 20; // Puffer for out-of-screen

        public PlaygroundWindow(EditorSettings settings, Action returnToEditor)
        {
            this.settings = settings;
            this.returnToEditorAction = returnToEditor;
            InitializeWindow();
            StartPositionCheckTimer();
        }

        private void InitializeWindow()
        {
            this.Title = settings.WindowTitle;
            this.Width = settings.WindowWidth;
            this.Height = settings.WindowHeight;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.CanResize;
            this.AllowsTransparency = true;
            this.Background = System.Windows.Media.Brushes.Transparent;
            this.Topmost = false;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Grid grid = new Grid();

            if (System.IO.File.Exists(settings.BackgroundPath))
            {
                Image bg = new Image
                {
                    Source = new BitmapImage(new Uri(settings.BackgroundPath, UriKind.RelativeOrAbsolute)),
                    Stretch = System.Windows.Media.Stretch.Fill
                };
                grid.Children.Add(bg);
            }

          /*  Button closeButton = new Button
            {
                Content = "X",
                Width = 30,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5)
            };
            closeButton.Click += (s, e) => this.Close();
            grid.Children.Add(closeButton); */

            this.Content = grid;

            this.Opacity = 0;
            this.Loaded += (s, e) =>
            {
                var anim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
                this.BeginAnimation(Window.OpacityProperty, anim);
            };

            this.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            };

            this.KeyDown += PlaygroundWindow_KeyDown;

            this.Closing += (s, e) =>
            {
                checkPositionTimer?.Stop();
            };

            lastCheckTime = DateTime.Now;
        }

        private void PlaygroundWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                returnToEditorAction?.Invoke();
                this.Close();
            }
        }

        private void StartPositionCheckTimer()
        {
            checkPositionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            checkPositionTimer.Tick += (s, e) =>
            {
                var elapsed = (DateTime.Now - lastCheckTime).TotalSeconds;
                if (elapsed >= 10)
                    CheckWindowPosition();
            };
            checkPositionTimer.Start();
        }

        private void CheckWindowPosition()
        {
            var screen = SystemParameters.WorkArea;

            bool outOfScreen =
                Left + Width < 0 || Top + Height < 0 ||
                Left > screen.Right || Top > screen.Bottom;

            if (!outOfScreen) return;

            double targetLeft = (screen.Width - Width) / 2;
            double targetTop = (screen.Height - Height) / 2;

            var animLeft = new DoubleAnimation
            {
                From = Left,
                To = targetLeft,
                Duration = TimeSpan.FromSeconds(2.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };
            animLeft.Completed += (s, e) => Left = targetLeft;

            var animTop = new DoubleAnimation
            {
                From = Top,
                To = targetTop,
                Duration = TimeSpan.FromSeconds(2.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };
            animTop.Completed += (s, e) => Top = targetTop;

            this.BeginAnimation(Window.LeftProperty, animLeft);
            this.BeginAnimation(Window.TopProperty, animTop);

            lastCheckTime = DateTime.Now;
        }
    }
}