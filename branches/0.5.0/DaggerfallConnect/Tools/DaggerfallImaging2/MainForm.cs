// Project:         DaggerfallImaging
// Description:     Explore and export bitmaps from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/using System;

#region Imports

using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Manina.Windows.Forms;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;

#endregion

namespace DaggerfallImaging2
{
    public partial class MainForm : Form
    {
        #region App Variables

        // Application constants
        private const string AppName = "DaggerfallImaging2";
        private const string ThumbnailsSubFolderName = "Thumbnails";
        private const string MySettingsFileName = "Settings.xml";
        private const string MyLibraryInfoSchemaName = "LibraryInfo";

        // User application data folders
        private static string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        private string SettingsFilePath = Path.Combine(AppDataFolder, MySettingsFileName);
        private string ThumbnailsOutputPath = Path.Combine(AppDataFolder, ThumbnailsSubFolderName);
        private string LibraryInfoFilePath = Path.Combine(AppDataFolder, MyLibraryInfoSchemaName + ".xml");

        // Working variables
        private bool AppClosing = false;
        private ImageFileReader DFImageReader = new ImageFileReader();
        private LibraryTypes SelectedLibraryType = LibraryTypes.None;
        private string SelectedFileName = string.Empty;
        private XmlDocument LibraryInfoXmlDocument = new XmlDocument();
        private CustomImageListViewRenderers.DFTilesRenderer TilesRenderer;
        private Manina.Windows.Forms.ImageListViewRenderers.DefaultRenderer ThumbnailsRenderer;
        private bool SuppressRebuildExportPreview = false;
        private bool ReadyToExport = false;
        private bool ExportInProgress = false;

        #endregion

        #region App Settings

        // Application settings (as saved and loaded in "Settings.xml"
        private const string Setting_Arena2Path = "Arena2Path";
        private const string Setting_IsMaximised = "IsMaximised";
        private const string Setting_LeftPanelWidth = "LeftPanelWidth";
        private const string Setting_ThumbnailRenderer = "ThumbnailRenderer";
        private const string Setting_ShowExportManager = "ShowExportManager";
        private const string Setting_ExportPath = "ExportPath";

        // Application defaults (deployed first time "Settings.xml" is created
        private const string Default_Arena2Path = "C:\\dosgames\\DAGGER\\ARENA2";
        private const int Default_IsMaximised = 0;
        private const int Default_LeftPanelWidth = 0;
        private const int Default_ThumbnailRenderer = 0;
        private const int Default_ShowExportManager = 0;
        private const string Default_ExportPath = "";

        // Application settings states (as read from "Settings.xml")
        private string State_Arena2Path = string.Empty;
        private int State_IsMaximised = 0;
        private int State_LeftPanelWidth = 0;
        private int State_ThumbnailRenderer = 0;
        private int State_ShowExportManager = 0;
        private string State_ExportPath = string.Empty;

        #endregion

        #region Class Structures

        // Selectable export formats matched by index to ExportFormatComboBox
        private enum ExportFormats
        {
            GIF = 0,
            JPG = 1,
            PNG = 2,
            TIF = 3,
            BMP = 4,
            ICO = 5
        }

        private struct ExportInfo
        {
            public string Arena2Path;
            public string ExportPath;
            public LibraryTypes LibraryType;
            public ExportFormats ExportFormat;
            public string[] ExportFileNames;
            public bool UseDescriptions;
            public bool Transparency;
        }

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            // Set image reader memory paramaters
            DFImageReader.Usage = FileUsage.UseMemory;
            DFImageReader.AutoDiscard = true;
        }

        #endregion

        #region Timers

        long StartTicks = 0;
        long CountTicks = 0;

        void StartTimer()
        {
            StartTicks = DateTime.Now.Ticks;
        }

        void EndTimer()
        {
            CountTicks = DateTime.Now.Ticks - StartTicks;
        }

        void ReportTimer(bool ShowMessageBox)
        {
            string text = string.Format("Completed in {0} ms.", (float)CountTicks / 10000.0f);
            Console.WriteLine(text);
            if (ShowMessageBox) MessageBox.Show(text);
        }

        private void ImageAnimTimer_Tick(object sender, EventArgs e)
        {
            FileImageFlowView.Tick();
        }

        #endregion

        #region MainForm Events

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Read settings from XML file
            ReadSettingsXml();

            // Set initial animation timer
            if (FileImageFlowView.AnimateImages)
            {
                ImageAnimTimer.Enabled = true;
                ToggleAnimationtoolStripButton.Checked = true;
            }
            else
            {
                ImageAnimTimer.Enabled = false;
                ToggleAnimationtoolStripButton.Checked = false;
            }

            // Deploy settings to app
            DeploySettingsState();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            // If Arena2 path invalid or not loaded from settings then prompt user for new path
            if (string.IsNullOrEmpty(State_Arena2Path))
                BrowseArena2Path();
            else
                DFImageReader.Arena2Path = State_Arena2Path;

            // Set initial export format
            ExportFormatComboBox.SelectedIndex = (int)ExportFormats.PNG;

            // Setup thumbnails folder
            SetupThumbnailsFolder();

            // Setup library info file
            SetupLibraryInfoFile();

            // Setup initial view
            SetupViews();
        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            // Handle export in progress
            if (ExportInProgress)
            {
                if (System.Windows.Forms.DialogResult.Yes == MessageBox.Show("An export is in progress. Cancel export and close application?", "Export In Progress", MessageBoxButtons.YesNo, MessageBoxIcon.Information))
                {
                    AppClosing = true;
                    if (ExportBackgroundWorker.IsBusy)
                        ExportBackgroundWorker.CancelAsync();
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Stop library worker thread if running
            if (LibraryThumbnailsBackgroundWorker.IsBusy)
                LibraryThumbnailsBackgroundWorker.CancelAsync();

            // Save settings
            WriteSettingsXml();
        }

        private void LibraryImageListView_Resize(object sender, EventArgs e)
        {
            Arena2PathStatusLabel.Width = LibraryImageListView.Width + 4;
        }

        #endregion

        #region Main ToolStrip Events

        private void SetArena2ToolStripButton_Click(object sender, EventArgs e)
        {
            BrowseArena2Path();
        }

        private void TextureFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set library
            if (SelectedLibraryType != LibraryTypes.Texture)
                SetLibraryType(LibraryTypes.Texture);

            // Rebuild export view
            RebuildExportPreview();
        }

        private void ImgFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set library
            if (SelectedLibraryType != LibraryTypes.Img)
                SetLibraryType(LibraryTypes.Img);

            // Rebuild export view
            RebuildExportPreview();
        }

        private void CifFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set library
            if (SelectedLibraryType != LibraryTypes.Cif)
                SetLibraryType(LibraryTypes.Cif);

            // Rebuild export view
            RebuildExportPreview();
        }

        private void RciFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set library
            if (SelectedLibraryType != LibraryTypes.Rci)
                SetLibraryType(LibraryTypes.Rci);

            // Rebuild export view
            RebuildExportPreview();
        }

        private void SkyFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set library
            if (SelectedLibraryType != LibraryTypes.Sky)
                SetLibraryType(LibraryTypes.Sky);

            // Rebuild export view
            RebuildExportPreview();
        }

        private void ViewTilesToolStripButton_Click(object sender, EventArgs e)
        {
            SetTilesRenderer();
        }

        private void ViewThumbnailsToolStripButton_Click(object sender, EventArgs e)
        {
            SetThumbnailsRenderer();
        }

        private void AboutToolStripButton_Click(object sender, EventArgs e)
        {
            Dialogs.AboutDialog dlg = new Dialogs.AboutDialog();
            dlg.ShowDialog();
        }

        private void ToggleAnimationtoolStripButton_Click(object sender, EventArgs e)
        {
            ToggleAnimation();
        }

        private void MakeTransparentToolStripButton_Click(object sender, EventArgs e)
        {
            ToggleTransparency();
        }

        private void ShowExportPaneToolStripButton_Click(object sender, EventArgs e)
        {
            if (ShowExportPaneToolStripButton.Checked)
            {
                // Hide export panel
                HideExportManager();
            }
            else
            {
                // Show export panel
                ShowExportManager();
            }
        }

        #endregion

        #region LibraryFilesContextMenu Events

        private void FileSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageListView.ImageListViewSelectedItemCollection SelectedItems = LibraryImageListView.SelectedItems;
            if (SelectedItems.Count != 1)
                return;

            // Toggle selected item check state
            if (SelectedItems[0].Checked)
                SelectedItems[0].Checked = false;
            else
                SelectedItems[0].Checked = true;

            // Refresh control
            LibraryImageListView.Refresh();
        }

        private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Suppress rebuilding export view while modifying check state
            SuppressRebuildExportPreview = true;

            // Check all items
            for (int i = 0; i < LibraryImageListView.Items.Count; i++)
            {
                LibraryImageListView.Items[i].Checked = true;
            }
            LibraryImageListView.Refresh();

            // Rebuild export preview
            SuppressRebuildExportPreview = false;
            RebuildExportPreview();

        }

        private void SelectNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Suppress rebuilding export view while modifying check state
            SuppressRebuildExportPreview = true;

            // Uncheck all items
            for (int i = 0; i < LibraryImageListView.Items.Count; i++)
            {
                LibraryImageListView.Items[i].Checked = false;
            }
            LibraryImageListView.Refresh();

            // Rebuild export preview
            SuppressRebuildExportPreview = false;
            RebuildExportPreview();
        }

        private void InvertSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Suppress rebuilding export view while modifying check state
            SuppressRebuildExportPreview = true;

            // Toggle check for all items
            for (int i = 0; i < LibraryImageListView.Items.Count; i++)
            {
                if (LibraryImageListView.Items[i].Checked)
                    LibraryImageListView.Items[i].Checked = false;
                else
                    LibraryImageListView.Items[i].Checked = true;
            }
            LibraryImageListView.Refresh();

            // Rebuild export preview
            SuppressRebuildExportPreview = false;
            RebuildExportPreview();
        }

        private void ShowExportPaneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ShowExportPaneToolStripButton.Checked)
                HideExportManager();
            else
                ShowExportManager();
        }

        private void LibraryFilesContextMenu_Opening(object sender, CancelEventArgs e)
        {
            // Mirror export pane check state
            ShowExportPaneToolStripMenuItem.Checked = ShowExportPaneToolStripButton.Checked;

            if (1 != LibraryImageListView.SelectedItems.Count)
                return;

            // Mirror select check state to context menu
            FileCheckedToolStripMenuItem.Checked = LibraryImageListView.SelectedItems[0].Checked;
        }

        private void FileDescriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create dialog
            Dialogs.SetDescription dlg = new Dialogs.SetDescription();
            dlg.Location = new Point(LibraryFilesContextMenu.Left, LibraryFilesContextMenu.Top);
            dlg.Description = ReadXmlDescription(SelectedFileName);
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                WriteXmlDescription(SelectedFileName, dlg.Description);
            }
        }

        #endregion

        #region Zoom Events

        private void ClearZoomTicks()
        {
            Zoom25MenuItem.Checked = false;
            Zoom50MenuItem.Checked = false;
            Zoom100MenuItem.Checked = false;
            Zoom200MenuItem.Checked = false;
            Zoom400MenuItem.Checked = false;
        }

        private void Zoom25MenuItem_Click(object sender, EventArgs e)
        {
            ClearZoomTicks();
            Zoom25MenuItem.Checked = true;
            FileImageFlowView.ZoomAmount = 0.25f;
            ZoomToolStripDropDownButton.Text = "25%";
        }

        private void Zoom50MenuItem_Click(object sender, EventArgs e)
        {
            ClearZoomTicks();
            Zoom50MenuItem.Checked = true;
            FileImageFlowView.ZoomAmount = 0.50f;
            ZoomToolStripDropDownButton.Text = "50%";
        }

        private void Zoom100MenuItem_Click(object sender, EventArgs e)
        {
            ClearZoomTicks();
            Zoom100MenuItem.Checked = true;
            FileImageFlowView.ZoomAmount = 1.0f;
            ZoomToolStripDropDownButton.Text = "100%";
        }

        private void Zoom200MenuItem_Click(object sender, EventArgs e)
        {
            ClearZoomTicks();
            Zoom200MenuItem.Checked = true;
            FileImageFlowView.ZoomAmount = 2.0f;
            ZoomToolStripDropDownButton.Text = "200%";
        }

        private void Zoom400MenuItem_Click(object sender, EventArgs e)
        {
            ClearZoomTicks();
            Zoom400MenuItem.Checked = true;
            FileImageFlowView.ZoomAmount = 4.0f;
            ZoomToolStripDropDownButton.Text = "400%";
        }

        private void Zoom25ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Zoom25MenuItem_Click(sender, e);
        }

        private void Zoom50ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Zoom50MenuItem_Click(sender, e);
        }

        private void Zoom100ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Zoom100MenuItem_Click(sender, e);
        }

        private void Zoom200ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Zoom200MenuItem_Click(sender, e);
        }

        private void Zoom400ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Zoom400MenuItem_Click(sender, e);
        }

        #endregion

        #region Application Management

        private void DeploySettingsState()
        {
            // Deploy IsMaximised
            if (1 == State_IsMaximised)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;

            // Deploy LeftPanelWidth
            if (State_LeftPanelWidth > MainSplitContainer.Panel1MinSize)
                MainSplitContainer.SplitterDistance = State_LeftPanelWidth;
            else
                MainSplitContainer.SplitterDistance = MainSplitContainer.Panel1MinSize;

            // Deploy ThumbnailRenderer
            switch (State_ThumbnailRenderer)
            {
                case 1:
                    SetThumbnailsRenderer();
                    break;
                case 0:
                default:
                    SetTilesRenderer();
                    break;
            }

            // Deploy ExportManagerOpen
            if (1 == State_ShowExportManager)
                ShowExportManager();
            else
                HideExportManager();

            // Deploy ExportPath
            ExportPathTextBox.Text = State_ExportPath;
        }

        private void UpdateSettingsState()
        {
            // Update IsMaximised
            if (this.WindowState == FormWindowState.Maximized)
                State_IsMaximised = 1;
            else
                State_IsMaximised = 0;

            // Update LeftPanelWidth
            State_LeftPanelWidth = MainSplitContainer.SplitterDistance;

            // Update ThumbnailRenderer
            if (ViewTilesToolStripButton.Checked)
                State_ThumbnailRenderer = 0;
            else if (ViewThumbnailsToolStripButton.Checked)
                State_ThumbnailRenderer = 1;
            else
                State_ThumbnailRenderer = 0;

            // Update ExportManagerOpen
            if (ShowExportPaneToolStripButton.Checked)
                State_ShowExportManager = 1;
            else
                State_ShowExportManager = 0;

            // Update ExportPath
            State_ExportPath = ExportPathTextBox.Text;
        }

        private bool BrowseArena2Path()
        {
            // Show dialog
            Dialogs.BrowseArena2Folder dialog = new Dialogs.BrowseArena2Folder();
            dialog.Arena2Path = State_Arena2Path;
            if (dialog.ShowDialog() != DialogResult.OK)
                return false;

            // Test path exists
            if (!Directory.Exists(dialog.Arena2Path))
            {
                MessageBox.Show(string.Format("Path '{0}' does not exist.", dialog.Arena2Path), "Path Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Store path and set reader
            State_Arena2Path = dialog.Arena2Path;
            DFImageReader.Arena2Path = dialog.Arena2Path;

            // Set library type
            SetLibraryType(LibraryTypes.None);

            // Rebuild export view
            RebuildExportPreview();

            return true;
        }

        private void SetLibraryType(LibraryTypes Type)
        {
            SelectedLibraryType = Type;
            DFImageReader.LibraryType = Type;
            SetupViews();
        }

        // Ensure thumbnails folder is created and warn user if all thumbnails need to be recreated.
        private bool SetupThumbnailsFolder()
        {
            // Create thumbnails folder if it doesn't exist
            try
            {
                // Test folder exists
                if (!Directory.Exists(ThumbnailsOutputPath))
                {
                    MessageBox.Show(Properties.Resources.ThumbnailsMissingText, Properties.Resources.ThumbnailsMissingTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Directory.CreateDirectory(ThumbnailsOutputPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        // Ensure library info xml is created and warn user if all info need to be regathered.
        private bool SetupLibraryInfoFile()
        {
            // Create library info xml file if it doesn't exist
            try
            {
                // Test file exists
                if (!File.Exists(LibraryInfoFilePath))
                {
                    // Create new file
                    MessageBox.Show(Properties.Resources.LibraryInfoMissingText, Properties.Resources.LibraryInfoMissingTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CreateXmlDocument(MyLibraryInfoSchemaName, LibraryInfoFilePath);
                }

                // Try to load file
                LibraryInfoXmlDocument.Load(LibraryInfoFilePath);
                if (!LibraryInfoXmlDocument.HasChildNodes)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        private void SetTilesRenderer()
        {
            // Set detail tiles renderer
            TilesRenderer = new CustomImageListViewRenderers.DFTilesRenderer(140);
            TilesRenderer.LibraryInfoXmlDocument = LibraryInfoXmlDocument;
            LibraryImageListView.SetRenderer(TilesRenderer);
            ViewTilesToolStripButton.Checked = true;
            ViewThumbnailsToolStripButton.Checked = false;
        }

        private void SetThumbnailsRenderer()
        {
            // Set thumbnails renderer
            ThumbnailsRenderer = new ImageListViewRenderers.DefaultRenderer();
            LibraryImageListView.SetRenderer(ThumbnailsRenderer);
            ViewTilesToolStripButton.Checked = false;
            ViewThumbnailsToolStripButton.Checked = true;
        }

        private void ShowExportManager()
        {
            ExportManagerPanel.Visible = true;
            ShowExportPaneToolStripButton.Checked = true;
            ImageViewPanel.Width = MainSplitContainer.Panel2.Width - ExportManagerPanel.Width;
        }

        private void HideExportManager()
        {
            ExportManagerPanel.Visible = false;
            ShowExportPaneToolStripButton.Checked = false;
            ImageViewPanel.Width = MainSplitContainer.Panel2.Width;
        }

        private void ToggleAnimation()
        {
            if (ToggleAnimationtoolStripButton.Checked)
            {
                // Disable animation
                ToggleAnimationtoolStripButton.Checked = false;
                FileImageFlowView.AnimateImages = false;
            }
            else
            {
                // Enable animation
                ToggleAnimationtoolStripButton.Checked = true;
                FileImageFlowView.AnimateImages = true;
            }
        }

        private void ToggleTransparency()
        {
            if (MakeTransparentToolStripButton.Checked)
            {
                // Disable transparency
                MakeTransparentToolStripButton.Checked = false;
                FileImageFlowView.Transparency = false;
            }
            else
            {
                // Enable transparency
                MakeTransparentToolStripButton.Checked = true;
                FileImageFlowView.Transparency = true;
            }
        }

        #endregion

        #region View Management

        private void SetupViews()
        {
            // Clear existing views
            ClearViews();

            // Show connection state
            ShowArena2ConnectionState();

            // Update button based on selected library
            UpdateLibraryButton();

            // Handle no library selected
            if (LibraryTypes.None == SelectedLibraryType)
                return;

            // Handle zero files
            if (0 == DFImageReader.FileCount)
            {
                MessageBox.Show(String.Format(Properties.Resources.NoImageFilesText, State_Arena2Path), Properties.Resources.NoImageFilesHeader, MessageBoxButtons.OK, MessageBoxIcon.Error);
                SelectedLibraryType = LibraryTypes.None;
                UpdateLibraryButton();
                return;
            }

            // Setup file list and previews
            ListFiles();
        }

        private void ClearViews()
        {
            // Clear views
            LibraryImageListView.Items.Clear();
            FileImageFlowView.DFImageFile = null;
            InfoStripStatusLabel.Text = string.Empty;
        }

        private void UpdateLibraryButton()
        {
            // Show connected text based on selection
            SelectLibraryToolStripDropDownButton.Image = Properties.Resources.connect;
            switch (SelectedLibraryType)
            {
                case LibraryTypes.Texture:
                    SelectLibraryToolStripDropDownButton.Text = "Texture Files";
                    break;
                case LibraryTypes.Img:
                    SelectLibraryToolStripDropDownButton.Text = "Img Files";
                    break;
                case LibraryTypes.Cif:
                    SelectLibraryToolStripDropDownButton.Text = "Cif Files";
                    break;
                case LibraryTypes.Rci:
                    SelectLibraryToolStripDropDownButton.Text = "Rci Files";
                    break;
                case LibraryTypes.Sky:
                    SelectLibraryToolStripDropDownButton.Text = "Sky Files";
                    break;
                default:
                    SelectLibraryToolStripDropDownButton.Image = Properties.Resources.disconnect;
                    SelectLibraryToolStripDropDownButton.Text = "Image Library";
                    break;
            }
        }

        private void ShowArena2ConnectionState()
        {
            // Show connected state
            if (Directory.Exists(State_Arena2Path))
            {
                SetArena2ToolStripButton.Image = Properties.Resources.lightbulb;
                Arena2PathStatusLabel.Image = Properties.Resources.lightbulb;
                SetArena2ToolStripButton.ToolTipText = State_Arena2Path;
                Arena2PathStatusLabel.Text = State_Arena2Path;
            }
            else
            {
                SetArena2ToolStripButton.Image = Properties.Resources.lightbulb_off;
                Arena2PathStatusLabel.Image = Properties.Resources.lightbulb_off;
                SetArena2ToolStripButton.ToolTipText = "Set Arena2 Folder";
                Arena2PathStatusLabel.Text = "Please set your Arena2 folder.";
            }
        }

        private void ListFiles()
        {
            // Exit if worker in progress
            if (LibraryThumbnailsBackgroundWorker.IsBusy)
                return;

            // Run preview worker thread
            LibraryToolStrip.Enabled = false;
            ActionStatusLabel.Text = "Processing files";
            LibraryThumbnailsBackgroundWorker.RunWorkerAsync();
        }

        private void ShowFileContents()
        {
            // Exit if no file selected
            if (string.IsNullOrEmpty(SelectedFileName))
                return;

            // Get the currently selected file
            DFImageFile DFImageFile = DFImageReader.GetImageFile(SelectedFileName);
            if (null == DFImageFile)
                return;

            FileImageFlowView.DFImageFile = DFImageFile;
        }

        #endregion

        #region Xml Methods

        private bool CreateSettingsXml()
        {
            try
            {
                // Create applications settings folder if it does not exist
                if (!Directory.Exists(AppDataFolder))
                    Directory.CreateDirectory(AppDataFolder);

                // Create new settings file
                XmlDocument SettingsXml = CreateXmlDocument(AppName, SettingsFilePath);

                // Append default settings
                AppendElement(SettingsXml, null, Setting_Arena2Path, Default_Arena2Path);
                AppendElement(SettingsXml, null, Setting_IsMaximised, Default_IsMaximised.ToString());
                AppendElement(SettingsXml, null, Setting_LeftPanelWidth, Default_LeftPanelWidth.ToString());
                AppendElement(SettingsXml, null, Setting_ThumbnailRenderer, Default_ThumbnailRenderer.ToString());
                AppendElement(SettingsXml, null, Setting_ShowExportManager, Default_ShowExportManager.ToString());
                AppendElement(SettingsXml, null, Setting_ExportPath, Default_ExportPath.ToString());

                // Save file
                SettingsXml.Save(SettingsFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        private bool ReadSettingsXml()
        {
            // Test settings file exists
            if (!File.Exists(SettingsFilePath))
            {
                if (!CreateSettingsXml())
                {
                    string error = string.Format("Failed to create settings file in '{0}'.", SettingsFilePath);
                    Console.WriteLine(error);
                    MessageBox.Show(error, "Settings File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                }
            }

            // Read settings
            try
            {
                // Open settings xml document
                XmlDocument SettingsXml = new XmlDocument();
                SettingsXml.Load(SettingsFilePath);

                // Read Arena2Path
                XmlNodeList nodes = SettingsXml.GetElementsByTagName(Setting_Arena2Path);
                State_Arena2Path = nodes.Item(0).InnerText;

                // Read IsMaximised
                nodes = SettingsXml.GetElementsByTagName(Setting_IsMaximised);
                State_IsMaximised = int.Parse(nodes.Item(0).InnerText);

                // Read LeftPanelWidth
                nodes = SettingsXml.GetElementsByTagName(Setting_LeftPanelWidth);
                State_LeftPanelWidth = int.Parse(nodes.Item(0).InnerText);

                // Read ThumbnailRenderer
                nodes = SettingsXml.GetElementsByTagName(Setting_ThumbnailRenderer);
                State_ThumbnailRenderer = int.Parse(nodes.Item(0).InnerText);

                // Read ShowExportManager
                nodes = SettingsXml.GetElementsByTagName(Setting_ShowExportManager);
                State_ShowExportManager = int.Parse(nodes.Item(0).InnerText);

                // Read ExportPath
                nodes = SettingsXml.GetElementsByTagName(Setting_ExportPath);
                State_ExportPath = nodes.Item(0).InnerText;

                // Ensure Arena2Path exists
                if (!Directory.Exists(State_Arena2Path))
                    throw new Exception(string.Format("'{0}' does not exist.", State_Arena2Path));

                // Ensure ExportPath exists
                if (!Directory.Exists(State_ExportPath))
                    State_ExportPath = string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        private bool WriteSettingsXml()
        {
            // Update state
            UpdateSettingsState();

            try
            {
                // Open settings xml document
                XmlDocument SettingsXml = new XmlDocument();
                SettingsXml.Load(SettingsFilePath);

                // Write Arena2Path
                XmlNodeList nodes = SettingsXml.GetElementsByTagName(Setting_Arena2Path);
                nodes.Item(0).InnerText = State_Arena2Path;

                // Write IsMaximised
                nodes = SettingsXml.GetElementsByTagName(Setting_IsMaximised);
                nodes.Item(0).InnerText = State_IsMaximised.ToString();

                // Write LeftPanelWidth
                nodes = SettingsXml.GetElementsByTagName(Setting_LeftPanelWidth);
                nodes.Item(0).InnerText = State_LeftPanelWidth.ToString();

                // Write ThumbnailRenderer
                nodes = SettingsXml.GetElementsByTagName(Setting_ThumbnailRenderer);
                nodes.Item(0).InnerText = State_ThumbnailRenderer.ToString();

                // Write ShowExportManager
                nodes = SettingsXml.GetElementsByTagName(Setting_ShowExportManager);
                nodes.Item(0).InnerText = State_ShowExportManager.ToString();

                // Write ExportPath
                nodes = SettingsXml.GetElementsByTagName(Setting_ExportPath);
                nodes.Item(0).InnerText = State_ExportPath;

                SettingsXml.Save(SettingsFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        private XmlDocument CreateXmlDocument(string SchemaName, string FileName)
        {
            // Ensure ".xml" is appended to document
            if (!FileName.EndsWith(".xml"))
                FileName += ".xml";

            // Create XML document, set schema, and get declaration
            XmlDocument document = new XmlDocument();
            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", "UTF-8", string.Empty);

            // Create XML writer and start file
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "    ";
            XmlWriter writer = XmlWriter.Create(FileName, settings);
            writer.WriteProcessingInstruction(declaration.Name, declaration.Value);
            writer.WriteStartElement(SchemaName);
            writer.Close();

            // Load new document
            document.Load(FileName);

            return document;
        }

        private XmlElement AppendElement(XmlDocument Document, XmlNode ParentNode, string ElementName, string ElementInnerText)
        {
            // If ParentNode isn't set then get root node
            if (null == ParentNode)
                ParentNode = Document.DocumentElement;

            // If ParentNode still isn't set then exit
            if (null == ParentNode)
                return null;

            // Create child element and return new node
            XmlElement element = Document.CreateElement(ElementName);
            element.InnerText = ElementInnerText;
            XmlNode node = ParentNode.AppendChild(element);
            return element;
        }

        private string ReadXmlDescription(string FileName)
        {
            if (null == LibraryInfoXmlDocument)
                return string.Empty;

            XmlNodeList Nodes = LibraryInfoXmlDocument.GetElementsByTagName(FileName);
            if (Nodes.Count > 0)
                return Nodes[0].InnerText;

            return string.Empty;
        }

        private bool WriteXmlDescription(string FileName, string Description)
        {
            if (null == LibraryInfoXmlDocument)
                return false;

            XmlNodeList Nodes = LibraryInfoXmlDocument.GetElementsByTagName(FileName);
            if (Nodes.Count > 0)
            {
                Nodes[0].InnerText = Description;
                LibraryInfoXmlDocument.Save(LibraryInfoFilePath);
                LibraryImageListView.Refresh();
                return true;
            }

            return false;
        }

        #endregion

        #region Library ListView Background Worker Methods

        // Stores image list items
        ImageListViewItem[] ImageListViewBatch;

        private void LibraryThumbnails_DoWork(object sender, DoWorkEventArgs e)
        {
            // Open library info xml
            bool LibraryInfoXmlNeedsSave = false;
            XmlDocument LibraryInfoXml = new XmlDocument();
            LibraryInfoXml.Load(LibraryInfoFilePath);

            // Get file count and create batch array
            int FileCount = DFImageReader.FileCount;
            ImageListViewBatch = new ImageListViewItem[FileCount];

            int Count = 0;
            string FileName = DFImageReader.FirstFileName;
            while (!string.IsNullOrEmpty(FileName))
            {
                // Handle pending cancel
                if (LibraryThumbnailsBackgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                // Find info node for this filename
                XmlNodeList Nodes = LibraryInfoXml.GetElementsByTagName(FileName);
                if (0 == Nodes.Count)
                {
                    // Add element
                    XmlElement NewElement = AppendElement(LibraryInfoXml, null, FileName, null);
                    LibraryInfoXmlNeedsSave = true;

                    // Load file for additional info
                    DFImageReader.LoadCurrentFile();

                    // Store description in element
                    NewElement.InnerText = DFImageReader.Description;

                    // Store record count as attribute
                    NewElement.SetAttribute("Records", DFImageReader.RecordCount.ToString());
                }

                // Optionally create thumbnail
                string ThumbImageFilePath = Path.Combine(ThumbnailsOutputPath, FileName + ".png");
                if (!File.Exists(ThumbImageFilePath))
                {
                    // Save thumbnail
                    Bitmap Preview = DFImageReader.GetPreview(128, 128, Color.Black);
                    Preview.Save(ThumbImageFilePath, ImageFormat.Png);
                }

                // Create image list item
                ImageListViewBatch[Count] = new ImageListViewItem(ThumbImageFilePath);
                ImageListViewBatch[Count].Text = FileName;

                // Increment processed count
                Count++;

                // Show progress
                float PercentComplete = (float)Count / (float)FileCount * 100.0f;
                LibraryThumbnailsBackgroundWorker.ReportProgress((int)PercentComplete);

                // Get next filename
                FileName = DFImageReader.NextFileName;
            }

            // Save library info xml if required
            if (LibraryInfoXmlNeedsSave)
            {
                // Save updated xml file
                LibraryInfoXml.Save(LibraryInfoFilePath);

                // Give updated library info to list view renderers
                TilesRenderer.LibraryInfoXmlDocument = LibraryInfoXml;
            }
        }

        private void LibraryThumbnails_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Update progress bar
            if (!LibraryThumbnailsBackgroundWorker.CancellationPending)
                ActionProgressBar.Value = e.ProgressPercentage;
        }

        private void LibraryThumbnails_RunWorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Execute batch
            if (!e.Cancelled)
            {
                // Add batched list items
                LibraryImageListView.Items.AddRange(ImageListViewBatch);
            }

            // Clear progress
            ActionStatusLabel.Text = string.Empty;
            ActionProgressBar.Value = 0;

            // Discard batch array
            ImageListViewBatch = null;

            // Re-enable library toolstrip
            LibraryToolStrip.Enabled = true;
        }

        #endregion

        #region LibraryImageListView Events

        private void LibraryImageListView_SelectionChanged(object sender, EventArgs e)
        {
            // No action if app closing
            if (AppClosing)
                return;

            // Exit if no selection
            if (1 != LibraryImageListView.SelectedItems.Count)
            {
                // Disable "select" and "description" context menu items if nothing selected
                FileCheckedToolStripMenuItem.Enabled = false;
                FileDescriptionToolStripMenuItem.Enabled = false;

                // Clear selection name and exit stage right
                SelectedFileName = string.Empty;
                return;
            }

            // Enable description and "select" context menu item only if export not running
            if (!ExportInProgress)
            {
                FileDescriptionToolStripMenuItem.Enabled = true;
                FileCheckedToolStripMenuItem.Enabled = true;
            }

            // Mirror item check state to context menu
            FileCheckedToolStripMenuItem.Checked = LibraryImageListView.SelectedItems[0].Checked;

            // Handle selecting same file
            if (SelectedFileName == LibraryImageListView.SelectedItems[0].Text)
                return;

            // Only first selection is valid (no multi-select support)
            SelectedFileName = LibraryImageListView.SelectedItems[0].Text;

            // Show the contents of selected file
            ShowFileContents();
        }

        private void LibraryImageListView_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Toggle check on space bar
            if (e.KeyChar == 0x20)
            {
                ImageListView.ImageListViewSelectedItemCollection SelectedItems = LibraryImageListView.SelectedItems;
                if (SelectedItems.Count != 1)
                    return;

                // Toggle selected item check state
                if (SelectedItems[0].Checked)
                    SelectedItems[0].Checked = false;
                else
                    SelectedItems[0].Checked = true;

                // Refresh control
                LibraryImageListView.Refresh();
            }
        }

        #endregion

        #region FileImageFlowView Events

        private void FileImageFlowView_ShowContextMenu(object sender, Classes.DFImageFlow.ShowContextMenuEventArgs e)
        {
            // Enable or disable clipboard copy command
            if (FileImageFlowView.SelectedIndex >= 0)
                CopyToolStripMenuItem.Enabled = true;
            else
                CopyToolStripMenuItem.Enabled = false;

            // Mirror toolbar check state to context menu
            AnimationToolStripMenuItem.Checked = ToggleAnimationtoolStripButton.Checked;
            TransparencyToolStripMenuItem.Checked = MakeTransparentToolStripButton.Checked;
            Zoom25ToolStripMenuItem.Checked = Zoom25MenuItem.Checked;
            Zoom50ToolStripMenuItem.Checked = Zoom50MenuItem.Checked;
            Zoom100ToolStripMenuItem.Checked = Zoom100MenuItem.Checked;
            Zoom200ToolStripMenuItem.Checked = Zoom200MenuItem.Checked;
            Zoom400ToolStripMenuItem.Checked = Zoom400MenuItem.Checked;

            // Show context menu
            Point pos = FileImageFlowView.PointToScreen(e.MousePos);
            ImagesContextMenuStrip.Show(pos);
        }

        private void FileImageFlowView_MouseOverItem(object sender, Classes.DFImageFlow.SelectedImageEventArgs e)
        {
            // Handle no selection
            if (-1 == e.FrameCount)
            {
                InfoStripStatusLabel.Text = string.Empty;
                return;
            }

            // Compose selected image information
            string Info = "Record = " + e.Record;
            if (e.AnimateImages && e.FrameCount > 1)
                Info += "; Frames = " + e.FrameCount;
            else if (!e.AnimateImages && e.FrameCount > 1)
                Info += "; Frame = " + e.Frame;
            Info += string.Format("; Size = {0} x {1}", e.Width, e.Height);

            // Show information
            InfoStripStatusLabel.Text = Info;
        }

        private void animationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleAnimation();
        }

        private void transparencyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleTransparency();
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (MakeTransparentToolStripButton.Checked)
            //{
            //    // Send selected image to clipboard, preserving transparency
            //    MemoryStream ms = new MemoryStream();
            //    FileImageFlowView.SeletedImage.Save(ms, ImageFormat.MemoryBmp);
            //    IDataObject dataObject = new DataObject();
            //    dataObject.SetData("BMP", true, ms);
            //    System.Windows.Forms.Clipboard.SetDataObject(dataObject, true);
            //}

            // Just copy the image
            Clipboard.SetImage(FileImageFlowView.SeletedImage);
        }

        #endregion

        #region Export Manager

        private void MultiCancelButton_Click(object sender, EventArgs e)
        {
            // Handle cancel when export in progress
            HideExportManager();
        }

        #endregion

        #region Unsorted methods to reallocate

        private void BrowseOutputFolderButton_Click(object sender, EventArgs e)
        {
            // Ask user for new output folder
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "Select export folder.";
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            // Copy selected folder into field
            ExportPathTextBox.Text = dlg.SelectedPath;
        }

        private void ExportPathTextBox_TextChanged(object sender, EventArgs e)
        {
            // Rebuild export view
            RebuildExportPreview();
        }

        private void ExportFormatComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Rebuild export view
            RebuildExportPreview();
        }

        private void LibraryImageListView_ItemCheckBoxClick(object sender, ItemEventArgs e)
        {
            // Do nothing is rebuild suppressed
            if (SuppressRebuildExportPreview)
                return;

            // Clear any selection in preview pane to stop a feedback effect created when unchecking the same item currently selected
            ExportPreviewTreeView.SelectedNode = null;

            // Get ExportFormatNode
            TreeNode ExportFormatNode = GetExportFormatNode();
            if (null == ExportFormatNode)
                return;

            // Remove any existing checked item of this name
            TreeNode[] Nodes = ExportFormatNode.Nodes.Find(e.Item.Text, false);
            if (1 == Nodes.Length)
                Nodes[0].Remove();

            // Add checked item of this name if checked
            if (e.Item.Checked)
            {
                // Add node
                AddFileExportNode(ExportFormatNode, e.Item.Text);

                // Sort list. This resets scroll position, need to find a way around that later.
                ExportPreviewTreeView.Sort();

                // Ensure expanded
                ExportFormatNode.Expand();
            }

            // Set export ready
            if (ExportFormatNode.Nodes.Count <= 0)
            {
                ReadyToExport = false;
                ExportButton.Enabled = false;
            }
            else
            {
                ReadyToExport = true;
                ExportButton.Enabled = true;
            }
        }

        private void AddFileExportNode(TreeNode ExportFormatNode, string FileName)
        {
            // Do nothing is rebuild suppressed
            if (SuppressRebuildExportPreview)
                return;
            
            // Compose node text
            string NodeText = FileName;
            if (UseDescriptionsCheckBox.Checked)
            {
                XmlNodeList Nodes = LibraryInfoXmlDocument.GetElementsByTagName(FileName);
                if (Nodes.Count > 0)
                    NodeText += string.Format(" ({0})", Nodes[0].InnerText);
            }

            // Add node
            TreeNode node = ExportFormatNode.Nodes.Add(FileName, NodeText, "folder.png", "folder.png");
            node.Tag = FileName;
        }

        private void UseDescriptionsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Rebuild export folders
            RebuildExportFolders();
        }

        private void RebuildExportPreview()
        {
            // Do nothing is rebuild suppressed
            if (SuppressRebuildExportPreview)
                return;

            // Clear existing nodes
            ExportPreviewTreeView.Nodes.Clear();

            // Create parent node
            TreeNode ExportPathRootNode;
            if (Directory.Exists(ExportPathTextBox.Text))
            {
                ExportPathRootNode = ExportPreviewTreeView.Nodes.Add("RootNode", ExportPathTextBox.Text, "drive.png", "drive.png");
            }
            else
            {
                // Add node
                ExportPathRootNode = ExportPreviewTreeView.Nodes.Add(null, ExportPathTextBox.Text, "drive_error.png", "drive_error.png");

                // Handle empty output foler
                if (string.IsNullOrEmpty(ExportPathTextBox.Text))
                {
                    // Empty folder, prompt user to set output folder
                    ExportPathRootNode.Nodes.Add(null, "Set output folder.", "information.png", "information.png");
                }
                else
                {
                    // Just an invald folder
                    ExportPathRootNode.Nodes.Add(null, "Output folder does not exist.", "error.png", "error.png");
                }

                // Expand to show message and exit
                ExportPathRootNode.Expand();
                return;
            }

            // Add library node
            TreeNode ExportLibraryNode;
            if (LibraryTypes.None == SelectedLibraryType)
            {
                ExportPathRootNode.Nodes.Add(null, "Select image library.", "information.png", "information.png");
                ExportPathRootNode.Expand();
                return;
            }
            else
            {
                ExportLibraryNode = ExportPathRootNode.Nodes.Add("ExportLibraryNode", GetLibraryFolderName(SelectedLibraryType), "folder.png", "folder.png");
                ExportPathRootNode.Expand();
            }

            // Add export format node
            ExportFormats format = (ExportFormats)ExportFormatComboBox.SelectedIndex;
            TreeNode ExportFormatNode = ExportLibraryNode.Nodes.Add("ExportFormatNode", GetFormatExtension(format), "folder.png", "folder.png");
            ExportLibraryNode.Expand();

            // Build selected folders
            RebuildExportFolders();
        }

        private TreeNode GetExportPathRootNode()
        {
            // Get root node
            return ExportPreviewTreeView.Nodes["RootNode"];
        }

        private TreeNode GetLibraryParentNode()
        {
            // Get root node
            TreeNode ExportPathRootNode = GetExportPathRootNode();
            if (null == ExportPathRootNode)
                return null;

            // Get library parent node
            return ExportPathRootNode.Nodes["ExportLibraryNode"];
        }

        private TreeNode GetExportFormatNode()
        {
            // Get library parent node
            TreeNode ExportLibraryNode = GetLibraryParentNode();
            if (null == ExportLibraryNode)
                return null;

            // Get format parent node
            return ExportLibraryNode.Nodes["ExportFormatNode"];
        }

        private void RebuildExportFolders()
        {
            // Do nothing is rebuild suppressed
            if (SuppressRebuildExportPreview)
                return;

            // Get ExportFormatNode
            TreeNode ExportFormatNode = GetExportFormatNode();
            if (null == ExportFormatNode)
                return;

            // Clear any existing items
            ExportFormatNode.Nodes.Clear();

            // Add checked image files to preview
            ImageListView.ImageListViewCheckedItemCollection CheckedItems = LibraryImageListView.CheckedItems;
            foreach (ImageListViewItem item in CheckedItems)
                AddFileExportNode(ExportFormatNode, item.Text);

            // Expand format node
            ExportFormatNode.Expand();

            // Set export ready
            if (ExportFormatNode.Nodes.Count <= 0)
            {
                ReadyToExport = false;
                ExportButton.Enabled = false;
            }
            else
            {
                ReadyToExport = true;
                ExportButton.Enabled = true;
            }
        }

        private string GetLibraryFolderName(LibraryTypes Type)
        {
            switch (Type)
            {
                case LibraryTypes.Texture:
                    return "TEXTURE Library";
                case LibraryTypes.Img:
                    return "IMG Library";
                case LibraryTypes.Cif:
                    return "CIF Library";
                case LibraryTypes.Rci:
                    return "RCI Library";
                case LibraryTypes.Sky:
                    return "SKY Library";
                default:
                    return "UNKNOWN";
            }
        }

        private string GetFormatExtension(ExportFormats Format)
        {
            switch (Format)
            {
                case ExportFormats.GIF:
                    return "GIF";
                case ExportFormats.JPG:
                    return "JPG";
                case ExportFormats.PNG:
                    return "PNG";
                case ExportFormats.TIF:
                    return "TIF";
                case ExportFormats.BMP:
                    return "BMP";
                case ExportFormats.ICO:
                    return "ICO";
                default:
                    return "UNKNOWN";
            }
        }

        private ImageFormat GetImageFormat(ExportFormats Format)
        {
            switch (Format)
            {
            case ExportFormats.GIF:
                    return ImageFormat.Gif;
                case ExportFormats.JPG:
                    return ImageFormat.Jpeg;
                case ExportFormats.PNG:
                    return ImageFormat.Png;
                case ExportFormats.TIF:
                    return ImageFormat.Tiff;
                case ExportFormats.BMP:
                    return ImageFormat.Bmp;
                case ExportFormats.ICO:
                    return ImageFormat.Icon;
                default:
                    return ImageFormat.Png;
            }
        }

        private void ExportPreviewTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Find selected item by tag. Not too efficient, but this is not the primary means of navigation. It's just a quick way to link the lists together.
            int found = -1;
            for (int i = 0; i < LibraryImageListView.Items.Count; i++)
            {
                if (LibraryImageListView.Items[i].Text == (string)e.Node.Tag)
                {
                    found = i;
                    break;
                }
            }

            // Select and show item in library image list view
            if (-1 != found)
            {
                LibraryImageListView.ClearSelection();
                LibraryImageListView.Items[found].Selected = true;
                LibraryImageListView.EnsureVisible(found);
            }
        }

        private void EnableExportControls(bool Enable)
        {
            // Enable or disable all controls related to checking and exporting
            LibraryImageListView.ShowCheckBoxes = Enable;
            ExportFormatComboBox.Enabled = Enable;
            ExportPathTextBox.Enabled = Enable;
            BrowseOutputFolderButton.Enabled = Enable;
            SaveTransparencyCheckBox.Enabled = Enable;
            UseDescriptionsCheckBox.Enabled = Enable;
            FileCheckedToolStripMenuItem.Enabled = Enable;
            FileDescriptionToolStripMenuItem.Enabled = Enable;
            SelectAllToolStripMenuItem.Enabled = Enable;
            SelectNoneToolStripMenuItem.Enabled = Enable;
            InvertSelectedToolStripMenuItem.Enabled = Enable;
            SetArena2ToolStripButton.Enabled = Enable;

            // Enable or disable export button
            if (ReadyToExport)
                ExportButton.Enabled = Enable;
            else
                ExportButton.Enabled = false;
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            // Exit if not ready for export or export already in progress
            if (!ReadyToExport || ExportInProgress)
                return;

            // Test export folder
            string ExportPath = ExportPathTextBox.Text;
            if (!Directory.Exists(ExportPath))
            {
                MessageBox.Show(string.Format("Export path '{0}' does not exist.", ExportPath), "Path Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Ensure library folder exists
            string LibraryFolder = Path.Combine(ExportPath, GetLibraryFolderName(SelectedLibraryType));
            if (!Directory.Exists(LibraryFolder))
            {
                // Attempt to create folder
                try
                {
                    Directory.CreateDirectory(LibraryFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Failed to create folder '{0}'. {1}", LibraryFolder, ex.Message), "Failed to Create Folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Ensure format folder exists
            ExportFormats format = (ExportFormats)ExportFormatComboBox.SelectedIndex;
            string FormatFolder = Path.Combine(LibraryFolder, GetFormatExtension(format));
            if (!Directory.Exists(FormatFolder))
            {
                // Attempt to create folder
                try
                {
                    Directory.CreateDirectory(FormatFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Failed to create folder '{0}'. {1}", FormatFolder, ex.Message), "Failed to Create Folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            
            // Get export filenames
            int index = 0;
            string[] ExportFileNames = new string[LibraryImageListView.CheckedItems.Count];
            ImageListView.ImageListViewCheckedItemCollection CheckedItems = LibraryImageListView.CheckedItems;
            foreach (ImageListViewItem item in CheckedItems)
                ExportFileNames[index++] = item.Text;

            // Set export in progress flag and disable export-related controls
            ExportInProgress = true;
            EnableExportControls(false);
            SuppressRebuildExportPreview = true;

            // Construct parameter bundle
            ExportInfo info = new ExportInfo();
            info.Arena2Path = State_Arena2Path;
            info.ExportPath = FormatFolder;
            info.ExportFormat = (ExportFormats)ExportFormatComboBox.SelectedIndex;
            info.LibraryType = SelectedLibraryType;
            info.ExportFileNames = ExportFileNames;
            info.UseDescriptions = UseDescriptionsCheckBox.Checked;
            info.Transparency = SaveTransparencyCheckBox.Checked;

            // Start export
            ExportBackgroundWorker.RunWorkerAsync(info);
        }

        #endregion

        #region ExportBackgroundWorker Methods

        private bool ExportBitmap(Bitmap SrcBitmap, string OutputPath, string FileName, ExportFormats Format)
        {
            // Append extension to filename
            FileName = string.Format("{0}.{1}", FileName, GetFormatExtension(Format));

            // Get managed ImageFormat
            ImageFormat ManagedFormat = GetImageFormat(Format);

            // Save final bitmap
            string FilePath = Path.Combine(OutputPath, FileName);
            SrcBitmap.Save(FilePath, ManagedFormat);

            return true;
        }

        private void ExportBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get parameter bundle
            ExportInfo info = (ExportInfo)e.Argument;

            // Instantiate a unique ImageFileReader for the export that will not conflict with normal browsing
            ImageFileReader ExportReader = new ImageFileReader(info.Arena2Path);
            ExportReader.LibraryType = info.LibraryType;

            // Work through all files in the array
            int index = 0;
            foreach (string FileName in info.ExportFileNames)
            {
                // Handle cancel
                if (ExportBackgroundWorker.CancellationPending)
                    return;

                // Compose destination folder
                string DestinationFolder = FileName;
                if (info.UseDescriptions)
                {
                    XmlNodeList Nodes = LibraryInfoXmlDocument.GetElementsByTagName(FileName);
                    if (Nodes.Count > 0)
                        DestinationFolder += string.Format(" ({0})", Nodes[0].InnerText);
                }

                // Create destination folder
                string DestinationPath = Path.Combine(info.ExportPath, DestinationFolder);
                try
                {
                    Directory.CreateDirectory(DestinationPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                // Get image file
                DFImageFile ImageFile = ExportReader.GetImageFile(FileName);
                if (null == ImageFile)
                    continue;

                // Export each frame of each record
                for (int record = 0; record < ImageFile.RecordCount; record++)
                {
                    for (int frame = 0; frame < ImageFile.GetFrameCount(record); frame++)
                    {
                        // Get this image
                        Bitmap bmp = ImageFile.GetManagedBitmap(record, frame, false, info.Transparency);

                        // Send to export
                        string fn = string.Format("{0}-{1}", record, frame);
                        ExportBitmap(bmp, DestinationPath, fn, info.ExportFormat);
                    }
                }

                // File complete, report on progress
                float PercentComplete = (float)index++ / (float)info.ExportFileNames.Length * 100.0f;
                ExportBackgroundWorker.ReportProgress((int)PercentComplete);
            }
        }

        private void ExportBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Take no action if library update is in progress
            if (LibraryThumbnailsBackgroundWorker.IsBusy)
                return;

            // Take no action if cancellation pending
            if (ExportBackgroundWorker.CancellationPending)
                return;

            // Update percent and status information
            ActionStatusLabel.Text = "Export in progress";
            ActionProgressBar.Value = e.ProgressPercentage;
        }

        private void ExportBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Just exit if app closing as controls will be invalidated
            if (AppClosing)
                return;

            // Turn back on export controls
            EnableExportControls(true);

            // Stop suppressing export preview and rebuild
            SuppressRebuildExportPreview = false;
            RebuildExportPreview();

            // Clear action status
            if (!LibraryThumbnailsBackgroundWorker.IsBusy)
            {
                ActionStatusLabel.Text = string.Empty;
                ActionProgressBar.Value = 0;
            }

            // Export no longer in progress
            ExportInProgress = false;
        }

        #endregion

    }
}
