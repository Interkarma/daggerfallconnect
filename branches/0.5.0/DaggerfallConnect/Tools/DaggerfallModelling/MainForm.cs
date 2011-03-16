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

namespace DaggerfallModelling
{
    public partial class MainForm : Form
    {
        #region Class Variables

        private string arena2Path = "C:\\dosgames\\DAGGER\\ARENA2";
        private Arch3dFile arch3dFile;
        private BlocksFile blocksFile;
        private MapsFile mapsFile;

        private bool searchModels = true;
        private bool searchBlocks = true;
        private bool searchLocations = true;

        private Dictionary<int, uint> modelsFound;
        private Dictionary<int, string> blocksFound;
        private Dictionary<int, string> mapsFound;

        #endregion

        #region Class Structures

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            // Initialise connect objects
            arch3dFile = new Arch3dFile(Path.Combine(arena2Path, "ARCH3D.BSA"), FileUsage.UseDisk, true);
            blocksFile = new BlocksFile(Path.Combine(arena2Path, "BLOCKS.BSA"), FileUsage.UseDisk, true);
            mapsFile = new MapsFile(Path.Combine(arena2Path, "MAPS.BSA"), FileUsage.UseDisk, true);

            // Turn off auto-discard to speed up searches and layouts.
            // This increases memory footprint by ~150MB.
            //arch3dFile.AutoDiscard = false;
            //blocksFile.AutoDiscard = false;
            //mapsFile.AutoDiscard = false;
        }

        #endregion

        #region Form Events

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchResultsTreeView.Focus();
                DoSearch(SearchTextBox.Text);
            }
        }

        #endregion

        #region Search Methods

        private void DoSearch(string pattern)
        {
            // Clear existing search results
            SearchResultsTreeView.Nodes.Clear();

            // Disable search controls
            SearchTextBox.Enabled = false;
            ClearSearchButton.Enabled = false;

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

            // Enable search controls
            SearchTextBox.Enabled = true;
            ClearSearchButton.Enabled = true;

            // Clear searching image
            SearchResultsPanel.Controls.Remove(pb);
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

            // Try string to uint
            uint npattern;
            try
            {
                npattern = uint.Parse(pattern);
            }
            catch
            {
                searchOut = modelsFound;
                return;
            }

            // Search all models for a match
            for (int model = 0; model < arch3dFile.Count; model++)
            {
                Application.DoEvents();
                DFMesh dfMesh = arch3dFile.GetMesh(model);
                if (dfMesh.ObjectId == npattern)
                    modelsFound.Add(model, npattern);
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
                        mapsFound.Add(RegionLocationKey(region, location), dfRegion.MapNames[location]);
                }
            }

            searchOut = mapsFound;
        }

        private int RegionLocationKey(int region, int location)
        {
            return region * 10000 + location;
        }

        private bool ContainsCaseInsensitive(ref string source, ref string value)
        {
            int results = source.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);
            return results == -1 ? false : true;
        }

        #endregion

    }
}
