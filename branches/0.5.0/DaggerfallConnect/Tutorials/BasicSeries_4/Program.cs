// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/using System;

using System;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

namespace Tutorial_BasicSeries_4
{
    /// <summary>
    /// This tutorial shows how to open ARCH3D.BSA and read basic information about a 3D object.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Specify Arena2 path of local Daggerfall installation
            string MyArena2Path = "C:\\dosgames\\DAGGER\\ARENA2";

            // Path to BSA file
            string FilePath = Path.Combine(MyArena2Path, "ARCH3D.BSA");

            // Open file
            Arch3dFile arch3dFile = new Arch3dFile(
                FilePath,
                FileUsage.UseDisk,
                true);

            // Output some information about the file
            Console.WriteLine("{0} has {1} records",
                Path.GetFileName(FilePath),
                arch3dFile.Count);

            // Get a mesh record
            int record = 5557;
            DFMesh mesh = arch3dFile.GetMesh(record);

            // Output some information about this record
            Console.WriteLine("Record {0} has {1} total vertices and ObjectID {2}",
                record,
                mesh.TotalVertices,
                mesh.ObjectId);

            // Output the texture archives used for this mesh
            Console.WriteLine("The following textures are used:");
            foreach (DFMesh.DFSubMesh sm in mesh.SubMeshes)
            {
                Console.WriteLine("TEXTURE.{0:000} (Record {1})",
                    sm.TextureArchive,
                    sm.TextureRecord);
            }
        }
    }
}
