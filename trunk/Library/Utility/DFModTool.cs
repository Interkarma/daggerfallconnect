// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace DaggerfallConnect.Utility
{

    /// <summary>
    /// Class for modifying certain values in Daggerfall's files
    ///  to aid research.
    ///  These methods are special-purpose, and not intended for general editing.
    ///  Do not use unless you know exactly what you
    ///  are doing and always make a backup of your game files beforehand.
    /// </summary>
    public class DFModTool
    {

        #region Class Variables

        private string arena2Path;

        #endregion

        #region Class Structures

        /// <summary>
        /// Pak chunk.
        /// </summary>
        private struct PakRun
        {
            public UInt16 Count;
            public Byte Value;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arena2Path">Path to ARENA2 folder..</param>
        public DFModTool(string arena2Path)
        {
            this.arena2Path = arena2Path;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Changes action record at specified position.
        /// </summary>
        /// <param name="blockName">Name of block file.</param>
        /// <param name="position">Start position of action record.</param>
        /// <param name="axis">Rotation axis.</param>
        /// <param name="duration">Duration of action.</param>
        /// <param name="magnitude">Magnitude of action.</param>
        public void ModActionRecord(string blockName, long position, byte axis, UInt16 duration, UInt16 magnitude)
        {
            // Load resources
            string path = Path.Combine(arena2Path, "BLOCKS.BSA");
            BsaFile bsaFile = new BsaFile(
                path,
                FileUsage.UseDisk,
                false);

            // Get binary record
            int index = bsaFile.GetRecordIndex(blockName);
            byte[] buffer = bsaFile.GetRecordBytes(index);

            // Create stream to binary record
            MemoryStream ms = new MemoryStream(buffer);
            ms.Position = position;

            // Write new data
            BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8);
            writer.Write(axis);
            writer.Write(duration);
            writer.Write(magnitude);
            writer.Close();

            // Save back binary record
            bsaFile.RewriteRecord(index, buffer);
        }

        /// <summary>
        /// ARCH3D.BSA: Modify base texture assigned to plane.
        /// </summary>
        /// <param name="MeshID">ID of mesh.</param>
        /// <param name="planeIndex">Plane index.</param>
        /// <param name="textureArchive">New texture archive index to set.</param>
        /// <param name="textureRecord">New texture record inex to set.</param>
        public void ModPlaneTexture(uint MeshID, int planeIndex, int textureArchive, int textureRecord)
        {
            // Load resources
            string path = Path.Combine(arena2Path, "ARCH3D.BSA");
            Arch3dFile arch3dFile = new Arch3dFile(
                path,
                FileUsage.UseMemory,
                true);
            BsaFile bsaFile = new BsaFile(
                path,
                FileUsage.UseDisk,
                false);

            // Get mesh record
            int index = arch3dFile.GetRecordIndex(MeshID);
            arch3dFile.LoadRecord(index);
            Arch3dFile.MeshRecord record = arch3dFile.MeshRecords[index];

            // Get binary record
            byte[] buffer = bsaFile.GetRecordBytes(index);

            // Compose new texture bitfield
            UInt16 textureBitfield = (UInt16)((textureArchive << 7) + textureRecord);

            // Get start position of plane header
            long position = record.PureMesh.Planes[planeIndex].Header.Position;

            // Offset to texture bitfield
            position += 2;

            // Create stream to binary record
            MemoryStream ms = new MemoryStream(buffer);
            ms.Position = position;

            // Write new bitfield
            BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8);
            writer.Write(textureBitfield);
            writer.Close();

            // Save back binary record
            bsaFile.RewriteRecord(index, buffer);
        }

        /// <summary>
        /// *.PAK: Writes a PAK file from the specified buffer.
        ///  Source buffer must be 1001x500 (500500) bytes.
        /// </summary>
        /// <param name="buffer">Source buffer to save in PAK format.</param>
        /// <param name="path">Destination path and filename.</param>
        public void WritePakFile(byte[] buffer, string path)
        {
            const int pakLength = 500500;

            // Test buffer length
            if (buffer.Length != pakLength)
            {
                Console.WriteLine("Buffer incorrect length.");
                return;
            }

            // Create lists to receive header and PakRun chunks.
            List<UInt32> header = new List<uint>();
            List<PakRun> chunks = new List<PakRun>();

            // Build new pak file
            UInt32 offset = 2000;
            for (int row = 0; row < 500; row++)
            {
                // Add offset to row
                header.Add(offset);

                // Set starting value
                PakRun pk;
                UInt16 count = 0;
                Byte value = buffer[row * 1001];

                // Process row
                for (int col = 0; col < 1001; col++)
                {
                    // Read next value
                    Byte newValue = buffer[row * 1001 + col];

                    if (newValue == value)
                    {
                        // Increment count
                        count++;
                    }
                    else
                    {
                        // Start new run
                        pk = new PakRun();
                        pk.Count = count;
                        pk.Value = value;
                        chunks.Add(pk);
                        value = newValue;
                        count = 1;
                        offset += 3;
                    }
                }

                // Always add chunk at end of row
                pk = new PakRun();
                pk.Count = count;
                pk.Value = value;
                chunks.Add(pk);
                offset += 3;
            }

            // Write header to pak file
            FileStream fs = File.Create(path);
            BinaryWriter writer = new BinaryWriter(fs, Encoding.UTF8);
            foreach (UInt32 pos in header)
            {
                writer.Write(pos);
            }

            // Write chunks to pak file
            foreach (PakRun chunk in chunks)
            {
                writer.Write(chunk.Count);
                writer.Write(chunk.Value);
            }
            writer.Close();
        }

        #endregion

    }

}
