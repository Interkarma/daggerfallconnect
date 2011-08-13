// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

using System;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

namespace Tutorial_BasicSeries_5
{
    /// <summary>
    /// This tutorial shows how to find the largest dungeon in every region.
    ///  Outputs text as comma separated values.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Specify Arena2 path of local Daggerfall installation
            string MyArena2Path = "C:\\dosgames\\DAGGER\\ARENA2";

            // Path to BSA file
            string FilePath = Path.Combine(MyArena2Path, "MAPS.BSA");

            // Open file
            MapsFile mapsFile = new MapsFile(
                FilePath,
                FileUsage.UseDisk,
                true);

            // Loop through regions
            for (int r = 0; r < mapsFile.RegionCount; r++)
            {
                // Get the region object
                DFRegion region = mapsFile.GetRegion(r);

                // Loop through locations to look for largest dungeon
                int maxDungeonBlocks = -1;
                DFLocation maxDungeonLocation = new DFLocation();
                for (int l = 0; l < region.LocationCount; l++)
                {
                    // Get the location object
                    DFLocation location = mapsFile.GetLocation(r, l);

                    // Continue if location does not have a dungeon
                    if (!location.HasDungeon)
                        continue;

                    // Find dungeon with most number of blocks
                    if (location.Dungeon.Blocks.Length > maxDungeonBlocks)
                    {
                        maxDungeonBlocks = location.Dungeon.Blocks.Length;
                        maxDungeonLocation = location;
                    }
                }

                // Output information if dungeon found
                if (maxDungeonBlocks != -1)
                {
                    Console.WriteLine("{0}, {1}, {2}",
                        region.Name,
                        maxDungeonLocation.Name,
                        maxDungeonLocation.Dungeon.Blocks.Length);
                }
            }
        }
    }
}
