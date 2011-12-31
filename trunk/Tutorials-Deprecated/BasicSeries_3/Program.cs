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

namespace Tutorial_BasicSeries_3
{
    /// <summary>
    /// This tutorial demonstrates how to open a BSA file and read records.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Specify Arena2 path of local Daggerfall installation
            string MyArena2Path = "C:\\dosgames\\DAGGER\\ARENA2";

            // Path to BSA file
            string FilePath = Path.Combine(MyArena2Path, "BLOCKS.BSA");

            // Open BSA file
            BsaFile bsaFile = new BsaFile(
                FilePath,
                FileUsage.UseDisk,
                true);

            // Output some information about the file
            Console.WriteLine("BSA file {0} has {1} records",
                Path.GetFileName(FilePath),
                bsaFile.Count);

            // Output some information about a record
            int record = 0;
            int length = bsaFile.GetRecordLength(record);
            string name = bsaFile.GetRecordName(record);
            Console.WriteLine("Record {0} is {1} bytes long and named {2}",
                record,
                length,
                name);

            // Get the record data
            byte[] data = bsaFile.GetRecordBytes(record);

            // This byte array now contains the binary data for this record
            // You can query it using streams to read in whatever data you like
            // (i.e. work with the file formats directly)
            // Here the array is just ignored and will pass out of scope
            // before being garbage collected.
        }
    }
}
