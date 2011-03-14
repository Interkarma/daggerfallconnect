// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/using System;

using System;
using System.Drawing;
using DaggerfallConnect;

namespace Tutorial_BasicSeries_2
{
    /// <summary>
    /// This tutorial demonstrates how to work with a single image file.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Specify Arena2 path of local Daggerfall installation
            string MyArena2Path = "C:\\dosgames\\DAGGER\\ARENA2";

            // Instantiate ImageFileReader
            ImageFileReader MyImageFileReader = new ImageFileReader(MyArena2Path);

            // Set desired library type
            MyImageFileReader.LibraryType = LibraryTypes.Texture;

            // Load TEXTURE.285 file
            MyImageFileReader.LoadFile("TEXTURE.285");
            
            // Output some information about this file
            Console.WriteLine("Image file {0} has {1} records",
                MyImageFileReader.FileName,
                MyImageFileReader.RecordCount);

            // Get image file to work with
            DFImageFile imageFile = MyImageFileReader.ImageFile;

            // Loop through all records
            for (int r = 0; r < imageFile.RecordCount; r++)
            {
                // Output some information about this record
                int frameCount = imageFile.GetFrameCount(r);
                Size sz = imageFile.GetSize(r);
                Console.WriteLine("Record {0} has {1} frames and is {2}x{3} pixels",
                    r, frameCount,
                    sz.Width,
                    sz.Height);

                // Get first frame of record as a managed bitmap
                Bitmap managedBitmap = imageFile.GetManagedBitmap(r, 0, true, false);
            }
        }
    }
}
