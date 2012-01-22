using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DeepEngine.Core;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConfigManager config;
        bool playReady = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ExitLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Hide tabs
            SetArena2Tab.Visibility = System.Windows.Visibility.Hidden;
            OptionsTab.Visibility = System.Windows.Visibility.Hidden;
            AboutTab.Visibility = System.Windows.Visibility.Hidden;

            // Open ini file
            string startupPath = System.Windows.Forms.Application.StartupPath;
            string iniPath = System.IO.Path.Combine(startupPath, "rohd_playgrounds.ini");
            config = new ConfigManager(iniPath);

            // Set arena2 path
            SetArena2Path(config.GetValue("Daggerfall", "arena2Path"));

            // If path failed to open, show the set arena2 tab
            if (!playReady)
                MainTabControl.SelectedItem = SetArena2Tab;
            else
                MainTabControl.SelectedItem = AboutTab;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Browse for arena2 folder
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowNewFolderButton = false;
            dlg.Description = "Browse for ARENA2 folder.";
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            // Store path
            SetArena2Path(dlg.SelectedPath);

            // Throw a message if validor failed
            if (!playReady)
            {
                MessageBox.Show("Invalid ARENA2 path. Either path is incorrect or some files are missing.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Sets arena2 path.
        /// </summary>
        /// <param name="path">Full path to arena2 folder.</param>
        private void SetArena2Path(string path)
        {
            // Validate path
            DFValidator.ValidationResults results;
            DFValidator.ValidateArena2Folder(path, out results);

            // Set if valid
            if (results.AppearsValid)
            {
                // Display and save path
                Arena2PathLabel.Content = path;
                Arena2PathTextBox.Text = path;
                config.SetValue("Daggerfall", "arena2Path", path);
                config.SaveFile();

                // Enable play button
                PlayLabel.Cursor = Cursors.Hand;
                PlayLabel.Foreground = Brushes.Gold;
                PlayLabel.IsEnabled = true;
                playReady = true;
            }
            else
            {
                // Remove path display
                Arena2PathLabel.Content = "Not currently set";
                Arena2PathTextBox.Text = string.Empty;

                // Disable play button
                PlayLabel.Cursor = Cursors.Arrow;
                PlayLabel.Foreground = Brushes.Gray;
                PlayLabel.IsEnabled = false;
                playReady = false;
            }
        }

        private void OptionsLabel_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            MainTabControl.SelectedItem = OptionsTab;
        }

        private void SetArena2Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainTabControl.SelectedItem = SetArena2Tab;
        }

        private void AboutLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainTabControl.SelectedItem = AboutTab;
        }

    }
}
