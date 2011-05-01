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
using System.IO;
using DaggerfallConnect;
#endregion

namespace DaggerfallConnect
{
    /// <summary>
    /// File usage enumeration.
    /// </summary>
    public enum FileUsage
    {
        /// <summary>Usage is not defined and will default to UseDisk if not specifed.</summary>
        Undefined,

        /// <summary>File is loaded and stored in a read-write memory buffer.</summary>
        UseMemory,

        /// <summary>File is opened as read-only from disk.</summary>
        UseDisk,
    }
}

namespace DaggerfallConnect.Utility
{
    /// <summary>
    /// This class abstracts a disk file or memory buffer to an object that can be emitted and read using binary streams.
    /// </summary>
    public class FileProxy
    {
        #region Class Variables

        /// <summary>
        /// Determines if file is read from disk (read-only file stream) or memory buffer (read-write memory stream).
        /// </summary>
        private FileUsage FileUsage;

        /// <summary>
        /// Has file been opened as read only or read-write.
        /// </summary>
        private bool IsReadOnly;

        /// <summary>
        /// Stream to file when using FileUsage.useDisk.
        /// </summary>
        private FileStream FileStream;

        /// <summary>
        /// Byte array when using FileUsage.useMemory.
        /// </summary>
        private byte[] FileBuffer;

        /// <summary>
        /// Full path of managed file regardless of usage.
        /// </summary>
        private string ManagedFilePath = string.Empty;

        /// <summary>
        /// Last exception thrown.
        /// </summary>
        private Exception MyLastException;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FileProxy()
        {
            FileUsage = FileUsage.Undefined;
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="FilePath">Absolute path to file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public FileProxy(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            Load(FilePath, Usage, ReadOnly);
        }

        /// <summary>
        /// Assign byte array constructor.
        /// </summary>
        /// <param name="Data">Byte array to assign (usage will be set to FileUsage.useMemory).</param>
        /// <param name="Name">Name, filename, or path  to describe memory buffer.</param>
        public FileProxy(byte[] Data, string Name)
        {
            FileBuffer = Data;
            ManagedFilePath = Name;
            FileUsage = FileUsage.UseMemory;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Length of managed file in bytes.
        /// </summary>
        public int Length
        {
            get
            {
                switch (FileUsage)
                {
                    case FileUsage.UseDisk:
                        if (FileStream == null) return 0; else return (int)FileStream.Length;
                    case FileUsage.UseMemory:
                        if (FileBuffer == null) return 0; else return FileBuffer.Length;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Get full path and filename of managed file. Derived from filename for disk files, or specified at construction for managed files.
        /// </summary>
        public string FilePath
        {
            get { return ManagedFilePath; }
        }

        /// <summary>
        /// Get filename of managed file without path.
        /// </summary>
        public string FileName
        {
            get { return Path.GetFileName(ManagedFilePath); }
        }

        /// <summary>
        /// Get directory path of managed file without filename.
        /// </summary>
        public string Directory
        {
            get { return Path.GetDirectoryName(ManagedFilePath); }
        }

        /// <summary>
        /// Get the file usage in effect for this managed file.
        /// </summary>
        public FileUsage Usage
        {
            get { return FileUsage; }
        }

        /// <summary>
        /// Access allowed to file.
        /// </summary>
        public bool ReadOnly
        {
            get { return IsReadOnly; }
        }

        /// <summary>
        /// Gets last exception thrown.
        /// </summary>
        public Exception LastException
        {
            get { return MyLastException; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load a file.
        /// </summary>
        /// <param name="FilePath">Absolute path to file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            // Determine file access settings
            FileAccess fileAccess;
            FileShare fileShare;
            if (ReadOnly)
            {
                fileAccess = FileAccess.Read;
                fileShare = FileShare.ReadWrite;
                IsReadOnly = true;
            }
            else
            {
                fileAccess = FileAccess.ReadWrite;
                fileShare = FileShare.Read;
                IsReadOnly = false;
            }

            // Load based on usage
            switch (Usage)
            {
                case FileUsage.UseMemory:
                    return LoadMemory(FilePath, fileAccess, fileShare);
                case FileUsage.UseDisk:
                default:
                    return LoadDisk(FilePath, fileAccess, fileShare);
            }
        }

        /// <summary>
        /// Close open file and free memory used for buffer.
        /// </summary>
        public void Close()
        {
            // Exit if no file being managed
            if (String.IsNullOrEmpty(ManagedFilePath))
                return;

            // Close based on type
            if (FileUsage == FileUsage.UseMemory)
                FileBuffer = null;
            else
                FileStream.Close();

            // Clear filename
            ManagedFilePath = String.Empty;
        }

        /// <summary>
        /// Gets a binary reader to managed file.
        /// </summary>
        /// <returns>BinaryReader to managed file with UTF8 encoding.</returns>
        public BinaryReader GetReader()
        {
            switch (FileUsage)
            {
                case FileUsage.UseMemory:
                    return new BinaryReader(GetMemoryStream(), Encoding.UTF8);
                case FileUsage.UseDisk:
                    return new BinaryReader(GetFileStream(), Encoding.UTF8);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get a binary reader to managed file starting at the specified position.
        /// </summary>
        /// <param name="Position">Position to start in stream (number of bytes from start of file).</param>
        /// <returns>BinaryReader to managed file with UTF8 encoding and set to specified position.</returns>
        public BinaryReader GetReader(long Position)
        {
            BinaryReader reader = GetReader();
            if (Position < reader.BaseStream.Length)
                reader.BaseStream.Position = Position;

            return reader;
        }

        /// <summary>
        /// Gets a binary writer to managed file.
        /// </summary>
        /// <returns>BinaryReader to managed file with UTF8 encoding.</returns>
        public BinaryWriter GetWriter()
        {
            switch (FileUsage)
            {
                case FileUsage.UseMemory:
                    return new BinaryWriter(GetMemoryStream(), Encoding.UTF8);
                case FileUsage.UseDisk:
                    return new BinaryWriter(GetFileStream(), Encoding.UTF8);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get a binary writer to managed file starting at the specified position.
        /// </summary>
        /// <param name="Position">Position to start in stream (number of bytes from start of file).</param>
        /// <returns>BinaryReader to managed file with UTF8 encoding and set to specified position.</returns>
        public BinaryWriter GetWriter(long Position)
        {
            BinaryWriter writer = GetWriter();
            if (Position < writer.BaseStream.Length)
                writer.BaseStream.Position = Position;

            return writer;
        }

        /// <summary>
        /// Reads a UTF8 string of bytes from the managed file.
        /// </summary>
        /// <param name="Position">Position to start reading in file (number of bytes from start of file).</param>
        /// <param name="ReadLength">Number of bytes to read (length=0 for null-terminated.)</param>
        /// <returns>String composed from bytes read (all NULLs are discarded).</returns>
        public string ReadCString(int Position, int ReadLength)
        {
            // End position must be less than length of stream
            if (Position + ReadLength > Length) return string.Empty;

            // Read from new stream
            BinaryReader reader = GetReader();
            reader.BaseStream.Position = Position;
            return ReadCString(reader, ReadLength);
        }

        /// <summary>
        /// Reads a UTF8 string of length bytes from the binary reader.
        /// </summary>
        /// <param name="Reader">Source reader.</param>
        /// <param name="ReadLength">Number of bytes to read (length=0 for null-terminated).</param>
        /// <returns>String composed from bytes read (all NULLs are discarded).</returns>
        public string ReadCString(BinaryReader Reader, int ReadLength)
        {
            // End position must be less than length of stream
            if (Reader.BaseStream.Position + ReadLength > Reader.BaseStream.Length) return string.Empty;

            string str = string.Empty;
            try
            {
                if (ReadLength > 0)
                {
                    // Specified length, dropping nulls
                    for (int i = 0; i < ReadLength; i++)
                    {
                        char c = Reader.ReadChar();
                        if (c != 0) str += c;
                    }
                }
                else
                {
                    // Null terminated
                    while (Reader.PeekChar() != 0)
                    {
                        str += Reader.ReadChar();
                    }

                    // Consume null char from reader to advance to next position
                    Reader.ReadChar();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                str = string.Empty;
            }

            return str;
        }

        /// <summary>
        /// Reads a UTF8 string of ReadLength bytes from the binary reader, then sets reader position to start + SkipLength.
        ///  This handles a special case where a character buffer may be null-terminated, but still be of a fixed length.
        ///  The remaining chars after the null may be filled with garbage. This method will read the
        ///  specified number of characters, then skip to Reader.BaseStream.Position + SkipLength.
        ///  When ReadLength=0, the string will be truncated at SkipLength if no null is found.
        /// </summary>
        /// <param name="Reader">Source reader.</param>
        /// <param name="ReadLength">Number of bytes to read (length=0 for null-terminated).</param>
        /// <param name="SkipLength">Number of bytes to skip from start position after read.</param>
        /// <returns>String composed from bytes read (all NULLs are discarded).</returns>
        public string ReadCStringSkip(BinaryReader Reader, int ReadLength, int SkipLength)
        {
            long pos = Reader.BaseStream.Position;
            string str = ReadCString(Reader, ReadLength);
            Reader.BaseStream.Position = pos + SkipLength;
            if (ReadLength == 0 && str.Length >= SkipLength)
                str = str.Substring(0, SkipLength);
            
            return str;
        }

        /// <summary>
        /// Reads next 2 bytes as a big-endian Int16.
        /// </summary>
        /// <param name="Reader">Source reader.</param>
        /// <returns>Big-endian Int16</returns>
        public Int16 beReadInt16(BinaryReader Reader)
        {
            return (Int16)endianSwapUInt16(Reader.ReadUInt16());
        }

        /// <summary>
        /// Reads next 2 bytes as a big-endian UInt16.
        /// </summary>
        /// <param name="Reader">Source reader.</param>
        /// <returns>Big-endian UInt16.</returns>
        public UInt16 beReadUInt16(BinaryReader Reader)
        {
            return endianSwapUInt16(Reader.ReadUInt16());
        }

        /// <summary>
        /// Reads next 4 bytes as a big-endian Int32.
        /// </summary>
        /// <param name="Reader">Source reader.</param>
        /// <returns>Big-endian Int32.</returns>
        public Int32 beReadInt32(BinaryReader Reader)
        {
            return (Int32)endianSwapUInt32(Reader.ReadUInt32());
        }

        /// <summary>
        /// Reads next 4 bytes as a big-endian UInt32.
        /// </summary>
        /// <param name="Reader">Source reader.</param>
        /// <returns>Big-endian Int32.</returns>
        public UInt32 beReadUInt32(BinaryReader Reader)
        {
            return endianSwapUInt32(Reader.ReadUInt32());
        }

        /// <summary>
        /// Swaps an unsigned 16-bit big-endian value to little-endian.
        /// </summary>
        /// <param name="Value">Source reader.</param>
        /// <returns>Little-endian UInt16.</returns>
        public UInt16 endianSwapUInt16(UInt16 Value)
        {
            return (UInt16)((Value >> 8) | (Value << 8));
        }

        /// <summary>
        /// Swaps an unsigned 32-bit big-endian value to little-endian.
        /// </summary>
        /// <param name="Value">Source reader.</param>
        /// <returns>Little-endian UInt32.</returns>
        public UInt32 endianSwapUInt32(UInt32 Value)
        {
            return (UInt32)((Value >> 24) | ((Value << 8) & 0x00FF0000) | ((Value >> 8) & 0x0000FF00) | (Value << 24));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets stream to disk file.
        /// </summary>
        /// <returns>FileStream object.</returns>
        private FileStream GetFileStream()
        {
            return FileStream;
        }

        /// <summary>
        /// Gets stream to memory file.
        /// </summary>
        /// <returns>FileStream object</returns>
        private MemoryStream GetMemoryStream()
        {
            return new MemoryStream(FileBuffer);
        }

        /// <summary>
        /// Loads a file into memory.
        /// </summary>
        /// <param name="FilePath">Absolute path of file to load.</param>
        /// <param name="FileAccess">Defines access to file.</param>
        /// <param name="FileShare">Defines shared access to file.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool LoadMemory(string FilePath, FileAccess FileAccess, FileShare FileShare)
        {
            // File must exist
            if (!File.Exists(FilePath))
                return false;

            // Load file into memory buffer
            try
            {
                FileStream file = File.Open(FilePath, FileMode.Open, FileAccess, FileShare);
                FileBuffer = new byte[file.Length];
                if (file.Length != file.Read(FileBuffer, 0, (int)file.Length))
                    return false;

                // Close file
                file.Close();
            }
            catch (Exception e)
            {
                MyLastException = e;
                Console.WriteLine(e.Message);
                return false;
            }

            // Store filename
            ManagedFilePath = FilePath;

            // Set usage
            FileUsage = FileUsage.UseMemory;

            return true;
        }

        /// <summary>
        /// Opens a file from disk.
        /// </summary>
        /// <param name="FilePath">Absolute path of file to load.</param>
        /// <param name="FileAccess">Defines access to file.</param>
        /// <param name="FileShare">Defines shared access to file.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool LoadDisk(string FilePath, FileAccess FileAccess, FileShare FileShare)
        {
            // File must exist
            if (!File.Exists(FilePath))
                return false;

            // Open file
            try
            {
                FileStream = File.Open(FilePath, FileMode.Open, FileAccess, FileShare);
                if (FileStream == null)
                    return false;
            }
            catch (Exception e)
            {
                MyLastException = e;
                Console.WriteLine(e.Message);
                return false;
            }

            // Store filename
            ManagedFilePath = FilePath;

            // Set usage
            FileUsage = FileUsage.UseDisk;

            return true;
        }

        #endregion
    }
}
