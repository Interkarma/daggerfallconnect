// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

using System;
using System.Text;
using System.IO;
using DaggerfallConnect.Utility;

#endregion

namespace DaggerfallConnect.Arena2
{
    /// <summary>
    /// Connects to a Daggerfall BSA file and extracts records as binary data.
    /// </summary>
    public class BsaFile
    {
        #region Class Variables

        /// <summary>
        /// Abstracts BSA file to a managed disk or memory stream.
        /// </summary>
        private FileProxy ManagedFile = new FileProxy();

        /// <summary>
        /// Contains the BSA file header data.
        /// </summary>
        private FileHeader Header;

        /// <summary>
        /// Array for directories where each item has a string for a name.
        /// </summary>
        private NameRecordDescriptor[] NameRecordDirectory;

        /// <summary>
        /// Array for directories where each item has a number for a name.
        /// </summary>
        private NumberRecordDescriptor[] NumberRecordDirectory;

        #endregion

        #region Class Structures

        /// <summary>
        /// Possible directory types enumeration.
        /// </summary>
        public enum DirectoryTypes
        {
            /// <summary>Each directory entry is a string.</summary>
            NameRecord = 0x0100,

            /// <summary>Each directory entry is an unsigned integer.</summary>
            NumberRecord = 0x0200,
        }

        /// <summary>
        /// Represents a BSA file header.
        /// </summary>
        private struct FileHeader
        {
            public long Position;
            public Int16 DirectoryCount;
            public DirectoryTypes DirectoryType;
            public long FirstRecordPosition;
        }

        /// <summary>
        /// A name record directory descriptor.
        /// </summary>
        private struct NameRecordDescriptor
        {
            public long Position;
            public String RecordName;
            public Int32 RecordSize;
            public long RecordPosition;
        }

        /// <summary>
        /// A number record directory descriptor.
        /// </summary>
        private struct NumberRecordDescriptor
        {
            public long Position;
            public UInt32 RecordId;
            public String RecordName;
            public Int32 RecordSize;
            public long RecordPosition;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BsaFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="FilePath">Absolute path to BSA file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public BsaFile(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            Load(FilePath, Usage, ReadOnly);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Number records in the loaded BSA file.
        /// </summary>
        public int Count
        {
            get { return Header.DirectoryCount; }
        }

        /// <summary>
        /// Type of directory used for this BSA file.
        /// </summary>
        public DirectoryTypes DirectoryType
        {
            get { return Header.DirectoryType; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load BSA file.
        /// </summary>
        /// <param name="FilePath">Absolute path to BSA file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            // Ensure filename ends with .BSA
            FilePath = FilePath.ToUpper();
            if (!FilePath.EndsWith(".BSA"))
                return false;

            // Load file into memory
            if (!ManagedFile.Load(FilePath, Usage, ReadOnly))
                return false;

            // Read file
            if (!Read())
                return false;

            return true;
        }

        /// <summary>
        /// Gets length of a record in bytes.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>Length of records in bytes.</returns>
        public int GetRecordLength(int Record)
        {
            // Validate
            if (Record >= Header.DirectoryCount)
                return 0;

            // Return length of this record
            switch (Header.DirectoryType)
            {
                case DirectoryTypes.NameRecord:
                    return NameRecordDirectory[Record].RecordSize;
                case DirectoryTypes.NumberRecord:
                    return NumberRecordDirectory[Record].RecordSize;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets name of a record as a string. This method is valid for BSA files with either a number or name-based directory.
        /// </summary>
        /// <param name="Record">Name of record.</param>
        /// <returns>Name of record as string.</returns>
        public string GetRecordName(int Record)
        {
            // Validate
            if (Record >= Header.DirectoryCount)
                return string.Empty;

            // Return name of this record
            switch (Header.DirectoryType)
            {
                case DirectoryTypes.NameRecord:
                    return NameRecordDirectory[Record].RecordName;
                case DirectoryTypes.NumberRecord:
                    return NumberRecordDirectory[Record].RecordName;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Gets ID of a number record. This method is valid only for BSA files with a number-based directory.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>ID of record.</returns>
        public uint GetRecordId(int Record)
        {
            // Validate
            if (Record >= Header.DirectoryCount || Header.DirectoryType != DirectoryTypes.NumberRecord)
                return 0;

            return NumberRecordDirectory[Record].RecordId;
        }

        /// <summary>
        /// Retrieves a record as a byte array.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>Byte array containing record data.</returns>
        public byte[] GetRecordBytes(int Record)
        {
            // Validate
            if (Record >= Header.DirectoryCount)
                return null;

            // Read record data into buffer
            BinaryReader reader = ManagedFile.GetReader(GetRecordPosition(Record));
            byte[] buffer = reader.ReadBytes(GetRecordLength(Record));

            return buffer;
        }

        /// <summary>
        /// Save new record data back to BSA file. WARNING: This will modify the BSA file. Ensure you have backups.
        ///  BSA file must have been opened with ReadOnly flag disabled.
        /// </summary>
        /// <param name="Record">The record to save back.</param>
        /// <param name="Buffer">The data to save back. This must be the same length as record data.</param>
        public void RewriteRecord(int Record, byte[] Buffer)
        {
            // Check data lengths
            if (Buffer.Length != GetRecordLength(Record))
                throw new Exception("Input array length and BSA record length do not match.");

            // Ensure file is writable
            if (ManagedFile.ReadOnly)
                throw new Exception(string.Format("BSA file '{0}' is read only.", ManagedFile.FilePath));

            // Ensure file usage is disk
            if (ManagedFile.Usage != FileUsage.UseDisk)
                throw new Exception("BSA file usage is not set to FileUsage.UseDisk. Can only save back to a disk file.");

            // Write data back into file
            BinaryWriter writer = ManagedFile.GetWriter(GetRecordPosition(Record));
            writer.Write(Buffer);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Retrieves a record as FileProxy object with usage of FileUsage.useMemory.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>FileProxy object.</returns>
        internal FileProxy GetRecordProxy(int Record)
        {
            // Validate
            if (Record >= Header.DirectoryCount)
                return null;

            // Read record data into buffer
            BinaryReader reader = ManagedFile.GetReader(GetRecordPosition(Record));
            byte[] buffer = reader.ReadBytes(GetRecordLength(Record));

            return new FileProxy(buffer, GetRecordName(Record));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get position (offset) of record in BSA file.
        /// </summary>
        /// <param name="Record">Index of record</param>
        private long GetRecordPosition(int Record)
        {
            switch (Header.DirectoryType)
            {
                case DirectoryTypes.NameRecord:
                    return NameRecordDirectory[Record].RecordPosition;
                case DirectoryTypes.NumberRecord:
                    return NumberRecordDirectory[Record].RecordPosition;
                default:
                    return -1;
            }
        }

        #endregion

        #region Readers

        /// <summary>
        /// Read file.
        /// </summary>
        private bool Read()
        {
            try
            {
                // Step through file
                BinaryReader reader = ManagedFile.GetReader();
                ReadHeader(reader);
                ReadDirectory(reader);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read header data.
        /// </summary>
        /// <param name="Reader">Reader to stream.</param>
        private void ReadHeader(BinaryReader Reader)
        {
            Reader.BaseStream.Position = 0;
            Header.Position = 0;
            Header.DirectoryCount = Reader.ReadInt16();
            Header.DirectoryType = (DirectoryTypes)Reader.ReadUInt16();
            Header.FirstRecordPosition = Reader.BaseStream.Position;
        }

        /// <summary>
        /// Read directory items.
        /// </summary>
        /// <param name="Reader">Reader to stream.</param>
        private void ReadDirectory(BinaryReader Reader)
        {
            if (Header.DirectoryType == DirectoryTypes.NameRecord)
            {
                // Create name record directory
                NameRecordDirectory = new NameRecordDescriptor[Header.DirectoryCount];

                // Read directory
                long recordPosition = Header.FirstRecordPosition;
                Reader.BaseStream.Position = ManagedFile.Length - 18 * Header.DirectoryCount;
                for (int i = 0; i < Header.DirectoryCount; i++)
                {
                    NameRecordDirectory[i].Position = Reader.BaseStream.Position;
                    NameRecordDirectory[i].RecordName = ManagedFile.ReadCString(Reader, 0);
                    Reader.BaseStream.Position = NameRecordDirectory[i].Position + 14;
                    NameRecordDirectory[i].RecordSize = Reader.ReadInt32();
                    NameRecordDirectory[i].RecordPosition = recordPosition;
                    recordPosition += NameRecordDirectory[i].RecordSize;
                }
            }
            else if (Header.DirectoryType == DirectoryTypes.NumberRecord)
            {
                // Create number record directory
                NumberRecordDirectory = new NumberRecordDescriptor[Header.DirectoryCount];

                // Read directory
                long recordPosition = Header.FirstRecordPosition;
                Reader.BaseStream.Position = ManagedFile.Length - 8 * Header.DirectoryCount;
                for (int i = 0; i < Header.DirectoryCount; i++)
                {
                    NumberRecordDirectory[i].Position = Reader.BaseStream.Position;
                    NumberRecordDirectory[i].RecordId = Reader.ReadUInt32();
                    NumberRecordDirectory[i].RecordName = NumberRecordDirectory[i].RecordId.ToString();
                    NumberRecordDirectory[i].RecordSize = Reader.ReadInt32();
                    NumberRecordDirectory[i].RecordPosition = recordPosition;
                    recordPosition += NumberRecordDirectory[i].RecordSize;
                }
            }
            else
            {
                throw new Exception("BSA file has an invalid DirectoryType.");
            }
        }

        #endregion
    }
}
