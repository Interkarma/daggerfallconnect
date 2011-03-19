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

namespace DaggerfallModelling
{
    public partial class MainForm : Form
    {
        #region Class Variables

        private string arena2Path = "C:\\dosgames\\DAGGER\\ARENA2";
        private Arch3dFile arch3dFile;
        private BlocksFile blocksFile;
        private MapsFile mapsFile;

        private int minSearchLength = 2;

        private bool searchModels = false;
        private bool searchBlocks = false;
        private bool searchLocations = true;

        private Dictionary<int, uint> modelsFound;
        private Dictionary<int, string> blocksFound;
        private Dictionary<int, string> mapsFound;

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            // Initialise connect objects
            arch3dFile = new Arch3dFile(Path.Combine(arena2Path, "ARCH3D.BSA"), FileUsage.UseDisk, true);
            blocksFile = new BlocksFile(Path.Combine(arena2Path, "BLOCKS.BSA"), FileUsage.UseDisk, true);
            mapsFile = new MapsFile(Path.Combine(arena2Path, "MAPS.BSA"), FileUsage.UseDisk, true);

            // Initialise map browser
            autoMapView1.BlocksFile = blocksFile;
            autoMapView1.MapsFile = mapsFile;
        }

        #endregion

        #region Form Events

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
            // Look for location select
            if (e.Node.Tag is string)
            {
                if ("LOC" == (string)e.Node.Tag)
                {
                    int key, region, location;
                    if (int.TryParse(e.Node.Name, out key))
                    {
                        KeyToRegionLocation(key, out region, out location);
                        autoMapView1.ShowLocation(region, location);
                        return;
                    }
                }
            }

            // Clear map block browser
            autoMapView1.Clear();
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
            if (e.ExteriorModeAllowed) ExteriorModeToolStripButton.Enabled = true; else ExteriorModeToolStripButton.Enabled = false;
            if (e.DungeonModeAllowed) DungeonModeToolStripButton.Enabled = true; else DungeonModeToolStripButton.Enabled = false;

            // Uncheck all modes
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
            autoMapView1.SetViewMode(AutoMapView.ViewModes.Exterior);
        }

        private void DungeonModeToolStripButton_Click(object sender, EventArgs e)
        {
            autoMapView1.SetViewMode(AutoMapView.ViewModes.Dungeon);
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
            autoMapView1.Clear();

            // Disable search controls
            SearchTextBox.Enabled = false;
            ClearSearchButton.Enabled = false;
            SearchResultsTreeView.Visible = false;

            // Drop in searching image
            PictureBox pb = new PictureBox();
            pb.Image = Properties.Resources.arrows64;
            pb.Size = new Size(pb.Image.Width, pb.Image.Height);
            pb.Location = new Point(SearchResultsPanel.Width / 2 - pb.Image.Width / 2, SearchResultsPanel.Height / 2 - pb.Image.Height / 2);
            SearchResultsPanel.Controls.Add(pb);
            pb.BringToFront();
            Application.DoEvents();

            if (searchModels)
                SearchModels(ref pattern, out modelsFound);
            if (searchBlocks)
                SearchBlocks(ref pattern, out blocksFound);
            if (searchLocations)
                SearchMaps(ref pattern, out mapsFound);

            // Show search results
            ShowSearchResults();

            // Enable search controls
            SearchTextBox.Enabled = true;
            ClearSearchButton.Enabled = true;
            SearchResultsTreeView.Visible = true;

            // Clear searching image
            SearchResultsPanel.Controls.Remove(pb);

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

            // Search all models for a match
            for (int model = 0; model < arch3dFile.Count; model++)
            {
                Application.DoEvents();
                DFMesh dfMesh = arch3dFile.GetMesh(model);
                string objectId = dfMesh.ObjectId.ToString();
                if (ContainsCaseInsensitive(ref objectId, ref pattern))
                    modelsFound.Add(model, dfMesh.ObjectId);
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
                DFBlock dfBlock = blocksFile.GetBlock(block);
                if (ContainsCaseInsensitive(ref dfBlock.Name, ref pattern))
                    blocksFound.Add(block, dfBlock.Name);
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
                    SearchResultsImageList.Images.IndexOfKey("models"),
                    SearchResultsImageList.Images.IndexOfKey("models"));
                ShowModelsFound(ref modelsNode);
            }
            if (searchBlocks)
            {
                string blocksTitle = string.Format("Blocks [{0}]", blocksFound.Count);
                blocksNode = SearchResultsTreeView.Nodes.Add(
                    "blocksNode",
                    blocksTitle,
                    SearchResultsImageList.Images.IndexOfKey("blocks"),
                    SearchResultsImageList.Images.IndexOfKey("blocks"));
                ShowBlocksFound(ref blocksNode);
            }
            if (searchLocations)
            {
                string locationsTitle = string.Format("Locations [{0}]", mapsFound.Count);
                mapsNode = SearchResultsTreeView.Nodes.Add(
                    "locationsNode",
                    locationsTitle,
                    SearchResultsImageList.Images.IndexOfKey("locations"),
                    SearchResultsImageList.Images.IndexOfKey("locations"));
                ShowMapsFound(ref mapsNode);
            }

            // Sort results tree
            SearchResultsTreeView.Sort();
        }

        private void ShowModelsFound(ref TreeNode node)
        {
            foreach (var model in modelsFound)
            {
                node.Nodes.Add(
                    model.Key.ToString(),
                    model.Value.ToString(),
                    SearchResultsImageList.Images.IndexOfKey("models"),
                    SearchResultsImageList.Images.IndexOfKey("models"));
            }
        }

        private void ShowBlocksFound(ref TreeNode node)
        {
            foreach (var block in blocksFound)
            {
                node.Nodes.Add(
                    block.Key.ToString(),
                    block.Value,
                    SearchResultsImageList.Images.IndexOfKey("blocks"),
                    SearchResultsImageList.Images.IndexOfKey("blocks"));
            }
        }

        private void ShowMapsFound(ref TreeNode node)
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
                regionNode.Nodes.Add("VIRT");
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

            // Is the only node "VIRT"
            if (node.Nodes[0].Text != "VIRT")
                return;

            // Remove virtual node
            node.Nodes[0].Remove();

            // Get target region
            int targetRegion = mapsFile.GetRegionIndex(node.Name);

            // Build sorted dictionary of locations belonging to this region
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
                        // Duplicate name found, burn it with fire
                    }
                }
            }

            // Add sorted dictionary to tree
            foreach (var item in sortedLocations)
                AddLocation(item.Key, item.Value, node);
        }

        private void AddLocation(string locationName, int key, TreeNode parent)
        {
            DFRegion dfRegion = mapsFile.GetRegion(parent.Name);
            int locationIndex = dfRegion.MapNameLookup[locationName];
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

            locationNode.Tag = "LOC";
        }

        #endregion

    }
}
