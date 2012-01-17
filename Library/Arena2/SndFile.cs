// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DaggerfallConnect.Utility;
#endregion

namespace DaggerfallConnect.Arena2
{

    /// <summary>
    /// Connects to DAGGER.SND to enumerate and extract sound data.
    /// </summary>
    public class SndFile
    {

        #region Class Variables

        /// <summary>
        /// Auto-discard behaviour enabled or disabled.
        /// </summary>
        private bool AutoDiscardValue = true;

        /// <summary>
        /// The last record opened. Used by auto-discard logic.
        /// </summary>
        private int LastSound = -1;

        /// <summary>
        /// The BsaFile representing DAGGER.SND.
        /// </summary>
        private BsaFile BsaFile = new BsaFile();

        /// <summary>
        /// Array of decomposed sound records.
        /// </summary>
        internal SoundRecord[] Sounds;

        #endregion

        #region Class Structures

        /// <summary>
        /// Represents a single sound record.
        /// </summary>
        internal struct SoundRecord
        {
            public FileProxy MemoryFile;
            public DFSound DFSound;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// If true then decomposed sound records will be destroyed every time a different sound is fetched.
        ///  If false then decomposed sound records will be maintained until DiscardRecord() or DiscardAllRecords() is called.
        ///  Turning off auto-discard will speed up sound retrieval times at the expense of RAM. For best results, disable
        ///  auto-discard and impose your own caching scheme based on your application needs.
        /// </summary>
        public bool AutoDiscard
        {
            get { return AutoDiscardValue; }
            set { AutoDiscardValue = value; }
        }

        /// <summary>
        /// Number of BSA records in DAGGER.SND.
        /// </summary>
        public int Count
        {
            get { return BsaFile.Count; }
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// Gets default DAGGER.SND filename.
        /// </summary>
        static public string Filename
        {
            get { return "DAGGER.SND"; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SndFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="FilePath">Absolute path to DAGGER.SND.</param>
        /// <param name="Usage">Determines if the BSA file will read from disk or memory.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public SndFile(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            Load(FilePath, Usage, ReadOnly);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load DAGGER.SND file.
        /// </summary>
        /// <param name="FilePath">Absolute path to DAGGER.SND file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            // Validate filename
            FilePath = FilePath.ToUpper();
            if (!FilePath.EndsWith("DAGGER.SND"))
                return false;

            // Load file
            if (!BsaFile.Load(FilePath, Usage, ReadOnly))
                return false;

            // Create records array
            Sounds = new SoundRecord[BsaFile.Count];

            return true;
        }

        /// <summary>
        /// Get a sound from index.
        /// </summary>
        /// <param name="Sound">SoundEffect.</param>
        /// <returns>DFSound.</returns>
        public DFSound GetSound(int Sound)
        {
            // Just return sound if already loaded
            if (Sounds[Sound].DFSound.WaveHeader != null &&
                Sounds[Sound].DFSound.WaveData != null)
            {
                return Sounds[Sound].DFSound;
            }

            // Discard previous sound
            if (AutoDiscard == true && LastSound != -1)
            {
                DiscardSound(LastSound);
            }

            // Load sound data
            Sounds[Sound].MemoryFile = BsaFile.GetRecordProxy(Sound);
            if (Sounds[Sound].MemoryFile == null)
                return new DFSound();

            // Attempt to read sound
            ReadSound(Sound);

            return Sounds[Sound].DFSound;
        }

        /// <summary>
        /// Get a sound from SoundEffects enumeration.
        /// </summary>
        /// <param name="SoundEffect">SoundEffect.</param>
        /// <returns>DFSound.</returns>
        public DFSound GetSound(SoundEffects SoundEffect)
        {
            // Attempt to read sound
            return GetSound((int)SoundEffect);
        }

        /// <summary>
        /// Helper method to get an entire WAV file in a memory stream.
        /// </summary>
        /// <param name="Sound">Sound index.</param>
        /// <returns>Wave file in MemoryStream.</returns>
        public MemoryStream GetStream(int Sound)
        {
            // Get sound
            DFSound dfSound = GetSound(Sound);
            if (dfSound.WaveHeader == null ||
                dfSound.WaveData == null)
            {
                return null;
            }

            // Create stream
            byte[] data = new byte[dfSound.WaveHeader.Length + dfSound.WaveData.Length];
            MemoryStream ms = new MemoryStream(data);

            // Write header and data
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write(dfSound.WaveHeader);
            writer.Write(dfSound.WaveData);

            // Reset start position in stream
            ms.Position = 0;

            return ms;
        }

        /// <summary>
        /// Helper method to get an entire WAV file in a memory stream.
        /// </summary>
        /// <param name="SoundEffect">SoundEffect.</param>
        /// <returns>Wave file in MemoryStream.</returns>
        public MemoryStream GetStream(SoundEffects SoundEffect)
        {
            return GetStream((int)SoundEffect);
        }

        /// <summary>
        /// Discard a sound from memory.
        /// </summary>
        /// <param name="Sound">Index of sound to discard.</param>
        public void DiscardSound(int Sound)
        {
            // Validate
            if (Sound >= BsaFile.Count)
                return;

            // Discard memory files and other data
            if (Sounds[Sound].MemoryFile != null) Sounds[Sound].MemoryFile.Close();
            Sounds[Sound].MemoryFile = null;
            Sounds[Sound].DFSound = new DFSound();
        }

        #endregion

        #region Readers

        /// <summary>
        /// Read a sound.
        /// </summary>
        /// <param name="Sound">Sound index.</param>
        private bool ReadSound(int Sound)
        {
            try
            {
                CreatePcmHeader(Sound);
                ReadWaveData(Sound);
            }
            catch (Exception e)
            {
                DiscardSound(Sound);
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads wave data for the sound.
        /// </summary>
        /// <param name="Sound">Sound index.</param>
        private void ReadWaveData(int Sound)
        {
            // The entire BSA record is just raw sound bytes
            Sounds[Sound].DFSound.Name = Sounds[Sound].MemoryFile.FileName;
            Sounds[Sound].DFSound.WaveData = Sounds[Sound].MemoryFile.Buffer;
        }

        /// <summary>
        /// Creates a PCM header for the sound, including the DATA prefix preceding raw sound bytes.
        /// </summary>
        /// <param name="Sound">Sound index.</param>
        private void CreatePcmHeader(int Sound)
        {
            Int32 headerLength = 44;
            Int32 dataLength = Sounds[Sound].MemoryFile.Length;
            Int32 fileLength = dataLength + 36;

            String sRIFF = "RIFF";
            String sWAVE = "WAVE";
            String sFmtID = "fmt ";
            String sDataID = "data";
            Int32 nFmtLength = 16;
            Int16 nFmtFormat = 1;
            Int16 nFmtChannels = 1;
            Int32 nFmtSampleRate = 11025;
            Int32 nFmtAvgBytesPerSec = 11025;
            Int16 nFmtBlockAlign = 1;
            Int16 nFmtBitsPerSample = 8;

            // Create header bytes
            byte[] header = new byte[headerLength];

            // Create memory stream and writer
            MemoryStream ms = new MemoryStream(header);
            BinaryWriter writer = new BinaryWriter(ms);

            // Write the RIFF tag and file length
            writer.Write(sRIFF.ToCharArray());
            writer.Write((Int32)fileLength);

            // Write the WAVE tag and fmt header
            writer.Write(sWAVE.ToCharArray());
            writer.Write(sFmtID.ToCharArray());

            // Write fmt information
            writer.Write(nFmtLength);
            writer.Write(nFmtFormat);
            writer.Write(nFmtChannels);
            writer.Write(nFmtSampleRate);
            writer.Write(nFmtAvgBytesPerSec);
            writer.Write(nFmtBlockAlign);
            writer.Write(nFmtBitsPerSample);

            // Write PCM data prefix
            writer.Write(sDataID.ToCharArray());
            writer.Write(dataLength);

            // Close writer
            writer.Close();

            // Assign header to sound
            Sounds[Sound].DFSound.WaveHeader = header;
        }

        #endregion

        #region SoundEffect Enumeration

        /// <summary>
        /// Enumeration for all the sound effects in Daggerfall.
        /// </summary>
        public enum SoundEffects
        {
        }

        #endregion

    }

}
