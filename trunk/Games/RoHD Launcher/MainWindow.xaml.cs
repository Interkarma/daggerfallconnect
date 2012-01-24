// Project:         Ruins of Hill Deep - Playground Build
// Description:     Test environment for Ruins of Hill Deep development.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DeepEngine.Core;
using DeepEngine.WinForms;
#endregion

namespace Launcher
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Fields

        ConfigManager ini = new ConfigManager();
        bool playReady = false;
        List<DeepCore.DisplayModeDesc> enumeratedDisplayModes;

        #endregion

        #region Structures

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Methods

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

        /// <summary>
        /// Sets options from ini file.
        /// </summary>
        private void SetOptions()
        {
            // Add display modes to combo box
            enumeratedDisplayModes = DeepCore.EnumerateDisplayModes();
            foreach (var mode in enumeratedDisplayModes)
                ResolutionComboBox.Items.Add(mode.ResolutionText);

            // Set selected inex
            SetPreferredDisplayMode();

            // Set rendering options
            EnableFXAACheckBox.IsChecked = bool.Parse(ini.GetValue("Renderer", "fxaaEnabled"));
            EnableBloomCheckBox.IsChecked = bool.Parse(ini.GetValue("Renderer", "bloomEnabled"));
            WindowedModeCheckBox.IsChecked = bool.Parse(ini.GetValue("Renderer", "windowedMode"));

            // Set mouse options
            MouseSensitivitySlider.Value = float.Parse(ini.GetValue("Controls", "mouseLookSpeed"));
            InvertMouseVerticalCheckBox.IsChecked = bool.Parse(ini.GetValue("Controls", "invertMouseVertical"));
        }

        /// <summary>
        /// Sets display drop-down to match INI file.
        ///  If INI cannot be matched then the desktop resolution will be used.
        /// </summary>
        private void SetPreferredDisplayMode()
        {
            string iniResolutionText = ini.GetValue("Renderer", "displayResolution");

            // Attempt to find mode in supported
            foreach (var mode in enumeratedDisplayModes)
            {
                if (mode.ResolutionText == iniResolutionText)
                {
                    ResolutionComboBox.SelectedIndex = mode.Index;
                    return;
                }
            }

            // Preferred resolution not found. Attempt to find desktop reslution
            DisplayMode desktop = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            foreach (var mode in enumeratedDisplayModes)
            {
                if (mode.Mode.Width == desktop.Width && mode.Mode.Height == desktop.Height)
                {
                    ResolutionComboBox.SelectedIndex = mode.Index;
                    return;
                }
            }

            // If all else fails just set first available mode
            ResolutionComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Saves INI file configuration.
        /// </summary>
        private void SaveConfig()
        {
            // Do nothing if no ini loaded
            if (!ini.IsLoaded)
                return;

            // Set arena2 path
            ini.SetValue("Daggerfall", "arena2Path", Arena2PathTextBox.Text);

            // Set resolution text
            DeepCore.DisplayModeDesc desc = enumeratedDisplayModes[ResolutionComboBox.SelectedIndex];
            ini.SetValue("Renderer", "displayResolution", desc.ResolutionText);
            
            // Set rendering options
            ini.SetValue("Renderer", "fxaaEnabled", EnableFXAACheckBox.IsChecked.Value.ToString());
            ini.SetValue("Renderer", "bloomEnabled", EnableBloomCheckBox.IsChecked.Value.ToString());
            ini.SetValue("Renderer", "windowedMode", WindowedModeCheckBox.IsChecked.Value.ToString());

            // Set mouse options
            ini.SetValue("Controls", "mouseLookSpeed", MouseSensitivitySlider.Value.ToString());
            ini.SetValue("Controls", "invertMouseVertical", InvertMouseVerticalCheckBox.IsChecked.Value.ToString());

            // Save ini file
            ini.SaveFile();
        }

        #endregion

        #region Events

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Hide tabs
            SetArena2Tab.Visibility = System.Windows.Visibility.Hidden;
            OptionsTab.Visibility = System.Windows.Visibility.Hidden;
            AboutTab.Visibility = System.Windows.Visibility.Hidden;

            // Open ini file
            string appStartPath = System.Windows.Forms.Application.StartupPath;
            string configName = "config.ini";
            ini.LoadFile(System.IO.Path.Combine(appStartPath, configName));

            // Set arena2 path
            SetArena2Path(ini.GetValue("Daggerfall", "arena2Path"));

            // Set other options
            SetOptions();

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

        private void ExitLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveConfig();
            this.Close();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SaveConfig();
                this.Close();
            }
        }

        private void PlayLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Start game
            if (playReady)
            {
                string appStartPath = System.Windows.Forms.Application.StartupPath;
                string appName = "RoHD Playground.exe";
                System.Diagnostics.Process.Start(System.IO.Path.Combine(appStartPath, appName));
                this.Close();
            }
        }

        #endregion

    }
}
