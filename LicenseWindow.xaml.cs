using System.Windows;

namespace ShimojiPlaygroundApp
{
    public partial class LicenseWindow : Window
    {
        public bool Accepted { get; private set; }

        public LicenseWindow()
        {
            InitializeComponent();
            AgreeCheckBox.Checked += (s, e) => AgreeButton.IsEnabled = true;
            AgreeCheckBox.Unchecked += (s, e) => AgreeButton.IsEnabled = false;
        }

        private void Agree_Click(object sender, RoutedEventArgs e)
        {
            Accepted = true;
            Close();
        }

        private void Disagree_Click(object sender, RoutedEventArgs e)
        {
            Accepted = false;
            Close();
        }
    }
}