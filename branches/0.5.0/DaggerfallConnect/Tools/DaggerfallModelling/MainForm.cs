// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallModelling.ViewControls;
#endregion

namespace DaggerfallModelling
{
    public partial class MainForm : Form
    {
        #region Class Variables

        // Constants
        private const string ModelTag = "MDL";
        private const string BlockTag = "BLK";
        private const string LocationTag = "LCN";
        private const string VirtText = "VIRT";

        // DaggerfallConnect
        private Classes.AppSettings appSettings = new Classes.AppSettings();
        private Arch3dFile arch3dFile = new Arch3dFile();
        private BlocksFile blocksFile = new BlocksFile();
        private MapsFile mapsFile = new MapsFile();

        // Searching
        private int minSearchLength = 2;
        private bool searchModels = false;
        private bool searchBlocks = false;
        private bool searchLocations = true;
        private Dictionary<int, uint> modelsFound;
        private Dictionary<int, string> blocksFound;
        private Dictionary<int, string> mapsFound;

        // Views
        private bool blockViewAvailable = false;
        private bool locationViewAvailable = false;

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Methods

        private void ConnectArena2Path()
        {
            // Check Arena2 directory exists.
            // On app load, user will be prompted in MainForm_Load to set path.
            // While app is running, user will just return to whatever state the app was in.
            if (!Directory.Exists(appSettings.Arena2Path))
                return;

            // Reset resources
            if (ContentViewer.IsReady)
            {
                ContentViewer.TextureManager.ClearTextures();
                ContentViewer.TextureManager.ClearAtlases();
                ContentViewer.ModelManager.ClearModelData();
            }

            // Reset search
            SearchResultsTreeView.Nodes.Clear();
            ContentViewer.FilteredModelsArray = null;

            // Reset views
            AutoMapViewer.SetViewMode(AutoMapView.ViewModes.None);
            ContentViewer.ResetViews();

            try
            {
                // Initialise connect objects
                if (!arch3dFile.Load(Path.Combine(appSettings.Arena2Path, "ARCH3D.BSA"), FileUsage.UseDisk, true))
                    throw new Exception("Loading ARCH3D.BSA failed.");
                if (!blocksFile.Load(Path.Combine(appSettings.Arena2Path, "BLOCKS.BSA"), FileUsage.UseDisk, true))
                    throw new Exception("Loading BLOCKS.BSA failed.");
                if (!mapsFile.Load(Path.Combine(appSettings.Arena2Path, "MAPS.BSA"), FileUsage.UseDisk, true))
                    throw new Exception("Loading MAPS.BSA failed.");
            }
            catch (Exception e)
            {
                string msg = string.Format("Could not connect to Arena2 folder. Files may be missing or damaged.\n{0}", e.Message);
                MessageBox.Show(msg,
                    "Connect Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
#if DEBUG
                Console.WriteLine(msg);
#endif
                // Kill invalid path so user will be prompted to change
                appSettings.Arena2Path = string.Empty;
                return;
            }

            // Initialise automap viewer
            AutoMapViewer.BlocksFile = blocksFile;
            AutoMapViewer.MapsFile = mapsFile;

            // Initialise content host
            ContentViewer.SetArena2Path(appSettings.Arena2Path);

            // TEST: Set a specific view while testing content
            //DFLocation dfLocation = mapsFile.GetLocation("Wayrest", "Wayrest");
            //ContentViewer.ShowLocationExterior(dfLocation);
            //ContentViewer.CameraMode = ViewBase.CameraModes.Free;
            //UpdateActiveCameraMode();

            // Show thumbnails view
            //ContentViewer.ShowThumbnailsView();
        }

        private void BrowseArena2Path()
        {
            Dialogs.BrowseArena2Folder dlg = new DaggerfallModelling.Dialogs.BrowseArena2Folder();
            dlg.Arena2Path = appSettings.Arena2Path;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                appSettings.Arena2Path = dlg.Arena2Path;
                ShowArena2ConnectionState();
                ConnectArena2Path();
            }
        }

        private void ShowArena2ConnectionState()
        {
            // Show connected state
            if (Directory.Exists(appSettings.Arena2Path))
            {
                SetArena2ToolStripButton.Image = Properties.Resources.lightbulb;
                Arena2PathStatusLabel.Image = Properties.Resources.lightbulb;
                SetArena2ToolStripButton.ToolTipText = appSettings.Arena2Path;
                Arena2PathStatusLabel.Text = appSettings.Arena2Path;
                EnableSearch(true);
            }
            else
            {
                SetArena2ToolStripButton.Image = Properties.Resources.lightbulb_off;
                Arena2PathStatusLabel.Image = Properties.Resources.lightbulb_off;
                SetArena2ToolStripButton.ToolTipText = "Set Arena2 Folder";
                Arena2PathStatusLabel.Text = "Please set your Arena2 folder.";
                EnableSearch(false);
            }
        }

        private void EnableSearch(bool enable)
        {
            if (enable)
            {
                SearchModelsToolStripButton.Enabled = true;
                SearchBlocksToolStripButton.Enabled = true;
                SearchLocationsToolStripButton.Enabled = true;
                SearchLabel.Enabled = true;
                SearchTextBox.Enabled = true;
            }
            else
            {
                SearchModelsToolStripButton.Enabled = false;
                SearchBlocksToolStripButton.Enabled = false;
                SearchLocationsToolStripButton.Enabled = false;
                SearchLabel.Enabled = false;
                SearchTextBox.Enabled = false;
            }
        }

        private bool ActivateSearchResultsNode(TreeNode node)
        {
            // Look for node select
            if (node.Tag is string)
            {
                switch ((string)node.Tag)
                {
                    case ModelTag:
                        ContentViewer.ShowModelView(uint.Parse(node.Text), DFLocation.ClimateType.None);
                        break;
                    case BlockTag:
                        // Show view
                        blockViewAvailable = true;
                        ContentViewer.ShowBlockView(node.Text, DFLocation.ClimateType.None);
                        break;
                    case LocationTag:
                        int key, region, location;
                        if (int.TryParse(node.Name, out key))
                        {
                            // Show in automap
                            KeyToRegionLocation(key, out region, out location);
                            AutoMapViewer.ShowLocation(region, location);

                            // Set flags
                            blockViewAvailable = true;
                            locationViewAvailable = true;

                            // Show location
                            if (AutoMapViewer.ViewMode == AutoMapView.ViewModes.Dungeon)
                                ContentViewer.ShowLocationDungeon(AutoMapViewer.DFLocation);
                            else
                                ContentViewer.ShowLocationExterior(AutoMapViewer.DFLocation);
                            return true;
                        }
                        break;
                }
            }

            return false;
        }

        #endregion

        #region Form Events

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Show arena2 connection state
            ShowArena2ConnectionState();

            // Check search buttons at load
            SearchModelsToolStripButton.Checked = searchModels;
            SearchBlocksToolStripButton.Checked = searchBlocks;
            SearchLocationsToolStripButton.Checked = searchLocations;

            // Update toolbars
            UpdateToolbars();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            // Direct user to set Arena2 path if not a valid path
            if (!Directory.Exists(appSettings.Arena2Path))
                BrowseArena2Path();
            else
                ConnectArena2Path();

            // Exit if path still not set
            if (!Directory.Exists(appSettings.Arena2Path))
                return;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save settings
            appSettings.SaveSettings();
        }

        private void SetArena2ToolStripButton_Click(object sender, EventArgs e)
        {
            BrowseArena2Path();
        }

        private void SearchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Start search on enter key
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                DoSearch(SearchTextBox.Text);
            }
        }

        private void SearchResultsTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            // Handle expanding region nodes
            if (e.Node.ImageIndex == SearchResultsImageList.Images.IndexOfKey("region"))
                ExpandRegionNode(e.Node);
        }

        private void SearchResultsTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Activate node and exit if location loaded
            if (ActivateSearchResultsNode(e.Node) == true)
                return;

            // Clear map block browser
            AutoMapViewer.Clear();
        }

        private void SearchResultsTreeView_DoubleClick(object sender, EventArgs e)
        {
            // Activate node and exit if location loaded
            if (ActivateSearchResultsNode(SearchResultsTreeView.SelectedNode) == true)
                return;

            // Clear map block browser
            AutoMapViewer.Clear();
        }

        private void SearchModelsToolStripButton_Click(object sender, EventArgs e)
        {
            searchModels = true;
            searchBlocks = false;
            searchLocations = false;
            SearchModelsToolStripButton.Checked = true;
            SearchBlocksToolStripButton.Checked = false;
            SearchLocationsToolStripButton.Checked = false;
        }

        private void SearchBlocksToolStripButton_Click(object sender, EventArgs e)
        {
            searchModels = false;
            searchBlocks = true;
            searchLocations = false;
            SearchModelsToolStripButton.Checked = false;
            SearchBlocksToolStripButton.Checked = true;
            SearchLocationsToolStripButton.Checked = false;
        }

        private void SearchLocationsToolStripButton_Click(object sender, EventArgs e)
        {
            searchModels = false;
            searchBlocks = false;
            searchLocations = true;
            SearchModelsToolStripButton.Checked = false;
            SearchBlocksToolStripButton.Checked = false;
            SearchLocationsToolStripButton.Checked = true;
        }

        private void AutoMapView_ModeChanged(object sender, DaggerfallModelling.ViewControls.AutoMapView.ModeChangedEventArgs e)
        {
            // Enable mode changes based on allowed modes
            ExteriorModeToolStripButton.Enabled = (e.ExteriorModeAllowed) ? true : false;
            DungeonModeToolStripButton.Enabled = (e.DungeonModeAllowed) ? true : false;

            // Check modes
            switch (e.ViewMode)
            {
                case AutoMapView.ViewModes.Exterior:
                    ExteriorModeToolStripButton.Checked = true;
                    DungeonModeToolStripButton.Checked = false;
                    break;
                case AutoMapView.ViewModes.Dungeon:
                    ExteriorModeToolStripButton.Checked = false;
                    DungeonModeToolStripButton.Checked = true;
                    break;
                default:
                    ExteriorModeToolStripButton.Checked = false;
                    DungeonModeToolStripButton.Checked = false;
                    break;
            }
        }

        private void ExteriorModeToolStripButton_Click(object sender, EventArgs e)
        {
            AutoMapViewer.SetViewMode(AutoMapView.ViewModes.Exterior);
            if (ContentViewer.ViewMode == ViewHost.ViewModes.LocationView)
                ContentViewer.ShowLocationExterior();
            else
                ContentViewer.ShowBlockView(AutoMapViewer.SelectedBlockName, ContentViewer.LocationClimate);
        }

        private void DungeonModeToolStripButton_Click(object sender, EventArgs e)
        {
            AutoMapViewer.SetViewMode(AutoMapView.ViewModes.Dungeon);
            if (ContentViewer.ViewMode == ViewHost.ViewModes.LocationView)
                ContentViewer.ShowLocationDungeon();
            else
                ContentViewer.ShowBlockView(AutoMapViewer.SelectedBlockName, ContentViewer.LocationClimate);
        }

        private void AutoMapView_MouseOverBlockChanged(object sender, AutoMapView.BlockEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Name))
            {
                BlockNameToolStripLabel.Text = string.Empty;
                BlockNameToolStripLabel.Visible = false;
            }
            else
            {
                string text = string.Format("{0} [{1},{2}]", e.Name, e.X, e.Y);
                BlockNameToolStripLabel.Text = text;
                BlockNameToolStripLabel.Visible = true;
            }
        }

        private void AutoMapView_SelectedBlockChanged(object sender, AutoMapView.BlockEventArgs e)
        {
            // Display single block only if not in location mode
            if (ContentViewer.ViewMode != ViewHost.ViewModes.LocationView)
            {
                // Get climate type
                DFLocation.ClimateType climate = DFLocation.ClimateType.None;
                if (locationViewAvailable && AutoMapViewer.ViewMode == AutoMapView.ViewModes.Exterior)
                    climate = ContentViewer.LocationClimate;

                // Load block into view
                blockViewAvailable = true;
                ContentViewer.ShowBlockView(e.Name, climate);

                return;
            }

            // Move to block location
            ContentViewer.MoveToBlock(e.X, e.Y);
        }

        private void AboutToolStripButton_Click(object sender, EventArgs e)
        {
            Dialogs.AboutDialog dlg = new Dialogs.AboutDialog();
            dlg.ShowDialog();
        }

        /// <summary>
        /// Called when content view status message changed
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">StatusMessageEventArgs.</param>
        private void ContentView_StatusMessageChanged(object sender, ViewHost.StatusMessageEventArgs e)
        {
            ContentViewStatusLabel.Text = e.Message;
        }

        /// <summary>
        /// Called when content view mode changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">ViewModeChangedEventArgs.</param>
        private void ContentView_ViewModeChanged(object sender, ViewHost.ViewModeChangedEventArgs e)
        {
            // Update toolbars
            UpdateToolbars();
        }

        /// <summary>
        /// User selected top down camera.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        private void TopDownCameraToolStripButton_Click(object sender, EventArgs e)
        {
            ContentViewer.CameraMode = ViewBase.CameraModes.TopDown;
            UpdateActiveCameraMode();
        }

        /// <summary>
        /// User selected free down camera.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        private void FreeCameraToolStripButton_Click(object sender, EventArgs e)
        {
            ContentViewer.CameraMode = ViewBase.CameraModes.Free;
            UpdateActiveCameraMode();
        }

        /// <summary>
        /// Show dialog to export model to Collada format.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        private void ExportModelToolStripButton_Click(object sender, EventArgs e)
        {
            // Setup dialog
            Dialogs.BrowseExportModelDialog dlg = new Dialogs.BrowseExportModelDialog();
            dlg.OutputPath = appSettings.ColladaExportPath;
            dlg.Orientation = appSettings.ColladaExportOrientation;
            dlg.ImageFormat = appSettings.ColladaExportImageFormat;

            // Show dialog
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            // Create DFCollada
            Classes.DFCollada dfCollada = new DaggerfallModelling.Classes.DFCollada(appSettings.Arena2Path);

            // Set up vector
            switch (dlg.Orientation)
            {
                case 0:
                    dfCollada.UpVector = Classes.DFCollada.UpVectors.X_UP;
                    break;
                case 1:
                    dfCollada.UpVector = Classes.DFCollada.UpVectors.Y_UP;
                    break;
                case 2:
                    dfCollada.UpVector = Classes.DFCollada.UpVectors.Z_UP;
                    break;
                default:
                    dfCollada.UpVector = Classes.DFCollada.UpVectors.Y_UP;
                    break;
            }

            // Set image format
            switch (dlg.ImageFormat)
            {
                case 0:
                    dfCollada.ImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;
                case 1:
                    dfCollada.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
                    break;
                case 2:
                    dfCollada.ImageFormat = System.Drawing.Imaging.ImageFormat.Tiff;
                    break;
                case 3:
                    dfCollada.ImageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                    break;
                default:
                    dfCollada.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
                    break;
            }

            // Set output paths
            dfCollada.ModelOutputPath = dlg.OutputPath;
            dfCollada.ImageOutputRelativePath = "images";

            // Execute
            try
            {
                dfCollada.DFMeshToCollada(ContentViewer.ModelView.ModelID.Value);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }

            // Save settings
            appSettings.ColladaExportPath = dlg.OutputPath;
            appSettings.ColladaExportOrientation = dlg.Orientation;
            appSettings.ColladaExportImageFormat = dlg.ImageFormat;
        }

        #endregion

        #region Search Methods

        private void DoSearch(string pattern)
        {
            // Enforce minimum search length
            if (pattern.Length < minSearchLength)
            {
                string msg = string.Format("Search must be at least {0} characters", minSearchLength);
                MainTips.Show(msg, SearchTextBox, 0, -45, 2000);
                return;
            }

            // Clear map block browser
            AutoMapViewer.Clear();

            // Clear filtered model array and switch back to thumbnails
            ContentViewer.FilteredModelsArray = null;
            ContentViewer.ShowThumbnailsView();

            // Clear available view
            blockViewAvailable = false;
            locationViewAvailable = false;
            UpdateAvailableViews();

            // Disable search controls
            SearchPaneToolStrip.Enabled = false;
            SearchTextBox.Enabled = false;
            SearchResultsTreeView.Visible = false;

            // Halt content animation
            ContentViewer.EnableUpdates(false);

            // Drop in searching image
            PictureBox pb = new PictureBox();
            pb.Image = Properties.Resources.arrows64;
            pb.Size = new Size(pb.Image.Width, pb.Image.Height);
            pb.Location = new Point(SearchResultsPanel.Width / 2 - pb.Image.Width / 2, SearchResultsPanel.Height / 2 - pb.Image.Height / 2);
            SearchResultsPanel.Controls.Add(pb);
            pb.BringToFront();
            Application.DoEvents();

            // Perform search
            if (searchModels)
                SearchModels(ref pattern, out modelsFound);
            if (searchBlocks)
                SearchBlocks(ref pattern, out blocksFound);
            if (searchLocations)
                SearchMaps(ref pattern, out mapsFound);

            // Show search results
            ShowSearchResults();

            // Enable search controls
            SearchPaneToolStrip.Enabled = true;
            SearchTextBox.Enabled = true;
            SearchResultsTreeView.Visible = true;

            // Clear searching image
            SearchResultsPanel.Controls.Remove(pb);

            // Resume content animation
            ContentViewer.EnableUpdates(true);

            // Set focus to results tree
            SearchResultsTreeView.Focus();
        }

        private void SearchModels(ref string pattern, out Dictionary<int, uint> searchOut)
        {
            Dictionary<int, uint> modelsFound = new Dictionary<int, uint>();

            // Handle resource null
            if (arch3dFile == null)
            {
                searchOut = modelsFound;
                return;
            }

            // Check pattern is numeric
            uint id;
            if (!uint.TryParse(pattern, out id))
            {
                searchOut = modelsFound;
                return;
            }

            // Search all models for a match
            for (int model = 0; model < arch3dFile.Count; model++)
            {
                Application.DoEvents();
                uint objectId = arch3dFile.GetRecordId(model);
                string objectIdString = objectId.ToString();
                if (ContainsCaseInsensitive(ref objectIdString, ref pattern))
                    modelsFound.Add(model, objectId);
            }

            searchOut = modelsFound;
        }

        private void SearchBlocks(ref string pattern, out Dictionary<int, string> searchOut)
        {
            Dictionary<int, string> blocksFound = new Dictionary<int, string>();

            // Handle resource null
            if (blocksFile == null)
            {
                searchOut = blocksFound;
                return;
            }

            // Search all blocks for a match
            for (int block = 0; block < blocksFile.Count; block++)
            {
                Application.DoEvents();
                string name = blocksFile.GetBlockName(block);
                if (ContainsCaseInsensitive(ref name, ref pattern))
                    blocksFound.Add(block, name);
            }

            searchOut = blocksFound;
        }

        private void SearchMaps(ref string pattern, out Dictionary<int, string> searchOut)
        {
            Dictionary<int, string> mapsFound = new Dictionary<int, string>();

            // Handle resource null
            if (mapsFile == null)
            {
                searchOut = mapsFound;
                return;
            }

            // Search all regions and locations for match
            for (int region = 0; region < mapsFile.RegionCount; region++)
            {
                Application.DoEvents();
                DFRegion dfRegion = mapsFile.GetRegion(region);
                for (int location = 0; location < dfRegion.LocationCount; location++)
                {
                    if (ContainsCaseInsensitive(ref dfRegion.MapNames[location], ref pattern))
                        mapsFound.Add(RegionLocationToKey(region, location), dfRegion.MapNames[location]);
                }
            }

            searchOut = mapsFound;
        }

        private int RegionLocationToKey(int region, int location)
        {
            return region * 100000 + location;
        }

        private void KeyToRegionLocation(int key, out int region, out int location)
        {
            int s = key / 100000;
            region = s;
            location = key - (s * 100000);
        }

        private bool ContainsCaseInsensitive(ref string source, ref string value)
        {
            int results = source.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);
            return results == -1 ? false : true;
        }

        #endregion

        #region SearchResults Methods

        private void ShowSearchResults()
        {
            // Clear existing search results
            SearchResultsTreeView.Nodes.Clear();

            // Create results nodes
            TreeNode modelsNode, blocksNode, mapsNode;
            if (searchModels)
            {
                string modelsTitle = string.Format("Models [{0}]", modelsFound.Count);
                modelsNode = SearchResultsTreeView.Nodes.Add(
                    "modelsNode",
                    modelsTitle,
                    SearchResultsImageList.Images.IndexOfKey("find"),
                    SearchResultsImageList.Images.IndexOfKey("find"));
                ShowModelsFound(ref modelsNode);
            }
            if (searchBlocks)
            {
                string blocksTitle = string.Format("Blocks [{0}]", blocksFound.Count);
                blocksNode = SearchResultsTreeView.Nodes.Add(
                    "blocksNode",
                    blocksTitle,
                    SearchResultsImageList.Images.IndexOfKey("find"),
                    SearchResultsImageList.Images.IndexOfKey("find"));
                ShowBlocksFound(ref blocksNode);
            }
            if (searchLocations)
            {
                string locationsTitle = string.Format("Locations [{0}]", mapsFound.Count);
                mapsNode = SearchResultsTreeView.Nodes.Add(
                    "locationsNode",
                    locationsTitle,
                    SearchResultsImageList.Images.IndexOfKey("find"),
                    SearchResultsImageList.Images.IndexOfKey("find"));
                ShowLocationsFound(ref mapsNode);
            }

            // Sort results tree
            SearchResultsTreeView.Sort();
        }

        private void ShowModelsFound(ref TreeNode node)
        {
            // Create new filtered model array
            uint[] modelsArray = new uint[modelsFound.Count];

            int index = 0;
            foreach (var model in modelsFound)
            {
                // Populate tree view
                TreeNode modelNode = node.Nodes.Add(
                    model.Key.ToString(),
                    model.Value.ToString(),
                    SearchResultsImageList.Images.IndexOfKey("models"),
                    SearchResultsImageList.Images.IndexOfKey("models"));
                modelNode.Tag = ModelTag;

                // Populate filtered models
                modelsArray[index++] = model.Value;
            }

            // Assign filtered array to content view
            if (modelsArray.Length > 0)
                ContentViewer.FilteredModelsArray = modelsArray;
        }

        private void ShowBlocksFound(ref TreeNode node)
        {
            foreach (var block in blocksFound)
            {
                TreeNode blockNode = node.Nodes.Add(
                    block.Key.ToString(),
                    block.Value,
                    SearchResultsImageList.Images.IndexOfKey("blocks"),
                    SearchResultsImageList.Images.IndexOfKey("blocks"));
                blockNode.Tag = BlockTag;
            }
        }

        private void ShowLocationsFound(ref TreeNode node)
        {
            foreach (var map in mapsFound)
            {
                // Tick events to animate spinner
                Application.DoEvents();

                // Get region and location
                int region, location;
                KeyToRegionLocation(map.Key, out region, out location);

                // Add region node
                TreeNode regionNode = AddRegionNode(region, ref node);
            }
        }

        private TreeNode AddRegionNode(int region, ref TreeNode parent)
        {
            string regionName = mapsFile.GetRegionName(region);
            TreeNode regionNode = parent.Nodes[regionName];
            if (regionNode == null)
            {
                // Add new region node
                string regionTitle = string.Format("{0} [{1}]", regionName, 1);
                regionNode = parent.Nodes.Add(
                    regionName,
                    regionTitle,
                    SearchResultsImageList.Images.IndexOfKey("region"),
                    SearchResultsImageList.Images.IndexOfKey("region"));
                regionNode.Tag = 1;

                // Add single virtual location node to populate later
                regionNode.Nodes.Add(VirtText);
            }
            else
            {
                // Use existing region and increment count
                regionNode.Tag = (int)regionNode.Tag + 1;
                string regionTitle = string.Format("{0} [{1}]", regionName, (int)regionNode.Tag);
                regionNode.Text = regionTitle;
            }

            return regionNode;
        }

        private void ExpandRegionNode(TreeNode node)
        {
            // Does this node only have one member
            if (node.Nodes.Count != 1)
                return;

            // Is the only node virtTag
            if (node.Nodes[0].Text != VirtText)
                return;

            // Remove virtual node
            node.Nodes[0].Remove();

            // Get target region
            int targetRegion = mapsFile.GetRegionIndex(node.Name);

            // Build sorted dictionary of locations belonging to this region
            int duplicateCount = 2;
            SortedDictionary<string, int> sortedLocations = new SortedDictionary<string, int>();
            foreach (var map in mapsFound)
            {
                int region, location;
                KeyToRegionLocation(map.Key, out region, out location);
                if (region == targetRegion)
                {
                    try
                    {
                        sortedLocations.Add(map.Value, map.Key);
                    }
                    catch
                    {
                        // Duplicate name found, add counter and add it anyway
                        string newName = string.Format("{0} [{1}]", map.Value, duplicateCount++);
                        sortedLocations.Add(newName, map.Key);
                    }
                }
            }

            // Add sorted dictionary to tree
            foreach (var item in sortedLocations)
                AddLocation(item.Key, item.Value, node);
        }

        private void AddLocation(string locationName, int key, TreeNode parent)
        {
            int regionIndex, locationIndex;
            KeyToRegionLocation(key, out regionIndex, out locationIndex);
            DFRegion dfRegion = mapsFile.GetRegion(regionIndex);
            DFRegion.LocationTypes locationType = dfRegion.MapTable[locationIndex].Type;
            switch (locationType)
            {
                case DFRegion.LocationTypes.DungeonKeep:
                case DFRegion.LocationTypes.DungeonLabyrinth:
                case DFRegion.LocationTypes.DungeonRuin:
                    AddLocationNode(ref locationName, ref key, ref parent, "dungeons");
                    break;

                case DFRegion.LocationTypes.GraveyardCommon:
                case DFRegion.LocationTypes.GraveyardForgotten:
                    AddLocationNode(ref locationName, ref key, ref parent, "graveyards");
                    break;

                case DFRegion.LocationTypes.HomeFarms:
                case DFRegion.LocationTypes.HomePoor:
                case DFRegion.LocationTypes.HomeWealthy:
                case DFRegion.LocationTypes.HomeYourShips:
                    AddLocationNode(ref locationName, ref key, ref parent, "homes");
                    break;

                case DFRegion.LocationTypes.ReligionCoven:
                case DFRegion.LocationTypes.ReligionCult:
                case DFRegion.LocationTypes.ReligionTemple:
                    AddLocationNode(ref locationName, ref key, ref parent, "religions");
                    break;

                case DFRegion.LocationTypes.Tavern:
                    AddLocationNode(ref locationName, ref key, ref parent, "taverns");
                    break;

                case DFRegion.LocationTypes.TownCity:
                case DFRegion.LocationTypes.TownHamlet:
                case DFRegion.LocationTypes.TownVillage:
                    AddLocationNode(ref locationName, ref key, ref parent, "cities");
                    break;
            }
        }

        private void AddLocationNode(ref string name, ref int key, ref TreeNode parent, string imageKey)
        {
            TreeNode locationNode = parent.Nodes.Add(
                key.ToString(),
                name,
                SearchResultsImageList.Images.IndexOfKey(imageKey),
                SearchResultsImageList.Images.IndexOfKey(imageKey));
            locationNode.Tag = LocationTag;
        }

        #endregion

        #region View Change Events

        /// <summary>
        /// User selected thumbnails view.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs</param>
        private void ViewThumbsToolStripButton_Click(object sender, EventArgs e)
        {
            // Do nothing if this is already current view
            if (ContentViewer.ViewMode == ViewHost.ViewModes.ThumbnailView ||
                !ContentViewer.IsReady)
                return;

            // Set view
            ContentViewer.ShowThumbnailsView();
        }

        /// <summary>
        /// User selected model view.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs</param>
        private void ViewSingleModelToolStripButton_Click(object sender, EventArgs e)
        {
            // Do nothing if this is already current view
            if (ContentViewer.ViewMode == ViewHost.ViewModes.ModelView ||
                !ContentViewer.IsReady)
                return;

            // Set view.
            // Model id of null means just display what is already loaded.
            ContentViewer.ShowModelView(null, ContentViewer.LocationClimate);
        }

        /// <summary>
        /// User selected block view.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs</param>
        private void ViewBlockToolStripButton_Click(object sender, EventArgs e)
        {
            // Do nothing if this is already current view
            if (ContentViewer.ViewMode == ViewHost.ViewModes.BlockView ||
                !ContentViewer.IsReady)
                return;

            // Set view
            ContentViewer.ShowBlockView(AutoMapViewer.SelectedBlockName, ContentViewer.LocationClimate);
        }

        /// <summary>
        /// User selected location view.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs</param>
        private void ViewLocationToolStripButton_Click(object sender, EventArgs e)
        {
            // Do nothing if this is already current view
            if (ContentViewer.ViewMode == ViewHost.ViewModes.LocationView ||
                !ContentViewer.IsReady)
                return;

            // Switch host into appropriate content mode
            switch (AutoMapViewer.ViewMode)
            {
                case AutoMapView.ViewModes.Exterior:
                    ContentViewer.ShowLocationExterior();
                    break;
                case AutoMapView.ViewModes.Dungeon:
                    ContentViewer.ShowLocationDungeon();
                    break;
                default:
                    ContentViewer.ShowLocationExterior();
                    break;
            }
        }

        #endregion

        #region Toolbar Update Methods

        /// <summary>
        /// Updates toolbars.
        /// </summary>
        private void UpdateToolbars()
        {
            UpdateAvailableViews();
            UpdateActiveView();
            UpdateActiveCameraMode();
        }

        /// <summary>
        /// Enables or disables view modes based on context.
        ///  Thumbnail and single model views are always available, even when empty.
        ///  Block view is only available when a block has been loaded.
        ///  Location view is only available when a location has been loaded.
        /// </summary>
        private void UpdateAvailableViews()
        {
            ViewBlockToolStripButton.Enabled = blockViewAvailable;
            ViewLocationToolStripButton.Enabled = locationViewAvailable;
        }

        /// <summary>
        /// Shows active view as selected in toolbar.
        /// </summary>
        private void UpdateActiveView()
        {
            // Uncheck items
            ViewThumbsToolStripButton.Checked = false;
            ViewSingleModelToolStripButton.Checked = false;
            ViewBlockToolStripButton.Checked = false;
            ViewLocationToolStripButton.Checked = false;
            ExportModelToolStripButton.Enabled = false;

            // Check current item
            switch (ContentViewer.ViewMode)
            {
                case ViewHost.ViewModes.ThumbnailView:
                    ViewThumbsToolStripButton.Checked = true;
                    break;
                case ViewHost.ViewModes.ModelView:
                    ViewSingleModelToolStripButton.Checked = true;
                    ExportModelToolStripButton.Enabled = true;
                    break;
                case ViewHost.ViewModes.BlockView:
                    ViewBlockToolStripButton.Checked = true;
                    break;
                case ViewHost.ViewModes.LocationView:
                    ViewLocationToolStripButton.Checked = true;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Displays the camera mode based on active view.
        /// </summary>
        private void UpdateActiveCameraMode()
        {
            // Check camera mode
            TopDownCameraToolStripButton.Checked = false;
            FreeCameraToolStripButton.Checked = false;
            switch (ContentViewer.CameraMode)
            {
                case ViewBase.CameraModes.TopDown:
                    TopDownCameraToolStripButton.Enabled = true;
                    FreeCameraToolStripButton.Enabled = true;
                    TopDownCameraToolStripButton.Checked = true;
                    break;
                case ViewBase.CameraModes.Free:
                    TopDownCameraToolStripButton.Enabled = true;
                    FreeCameraToolStripButton.Enabled = true;
                    FreeCameraToolStripButton.Checked = true;
                    break;
                default:
                    TopDownCameraToolStripButton.Enabled = false;
                    FreeCameraToolStripButton.Enabled = false;
                    break;
            }
        }

        #endregion

        private void ToggleSearchPaneToolStripButton_Click(object sender, EventArgs e)
        {
            if (ToggleSearchPaneToolStripButton.Checked)
            {
                MainSplitContainer.Panel1Collapsed = true;
                ToggleSearchPaneToolStripButton.Checked = false;
                ToggleSearchPaneToolStripButton.Image = Properties.Resources.application_side_expand;
            }
            else
            {
                MainSplitContainer.Panel1Collapsed = false;
                ToggleSearchPaneToolStripButton.Checked = true;
                ToggleSearchPaneToolStripButton.Image = Properties.Resources.application_side_contract;
            }
        }

        private void TimeOfDayTrackBar_Scroll(object sender, EventArgs e)
        {

        }

    }
}
