// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/using System;

using System;
using DaggerfallConnect;

namespace Tutorial_BasicSeries_1
{
    /// <summary>
    /// This tutorial demonstrates basic enumeration of image files.
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

            // Output information about each file in library
            foreach (DFImageFile file in MyImageFileReader)
                Console.WriteLine("{0} - {1}", file.FileName, file.Description);
        }
    }
}
