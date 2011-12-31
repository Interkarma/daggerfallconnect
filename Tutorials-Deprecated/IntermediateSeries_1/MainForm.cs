// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

namespace IntermediateSeries_Tutorial_1
{
    public partial class MainForm : Form
    {
        // Specify Arena2 path of local Daggerfall installation
        string MyArena2Path = "C:\\dosgames\\DAGGER\\ARENA2";

        // Instantiate ImageFileReader
        ImageFileReader MyImageFileReader = new ImageFileReader();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set MyImageFileReader Arena2 path
            MyImageFileReader.Arena2Path = MyArena2Path;

            // Set desired library type
            MyImageFileReader.LibraryType = LibraryTypes.Texture;

            // Add image files to list
            foreach (string FileName in MyImageFileReader.FileNames)
                FilenamesListBox.Items.Add(FileName);

            // Show default image file
            ShowImageFile(MyImageFileReader.FirstFileName);
        }

        private void ShowImageFile(string FileName)
        {
            // Clear panel
            ImagesPanel.Controls.Clear();

            // Get image file
            DFImageFile imageFile = MyImageFileReader.GetImageFile(FileName);

            // Set description
            DescriptionLabel.Text = imageFile.Description;

            // Loop through all records
            for (int r = 0; r < imageFile.RecordCount; r++)
            {
                // Loop through each frame for this record
                for (int f = 0; f < imageFile.GetFrameCount(r); f++)
                {
                    // Get managed bitmap
                    Bitmap bm = imageFile.GetManagedBitmap(r, f, false, true);

                    // Create a new picture box
                    PictureBox pb = new PictureBox();
                    pb.Width = bm.Width;
                    pb.Height = bm.Height;
                    pb.Image = bm;

                    // Add picture box to flow panel
                    ImagesPanel.Controls.Add(pb);
                }
            }
        }

        private void FilenamesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Show textures
            ShowImageFile((string)FilenamesListBox.SelectedItem);
        }
    }
}
