// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;

#endregion

namespace DaggerfallConnect
{
    /// <summary>
    /// Image library types enumeration.
    /// </summary>
    public enum LibraryTypes
    {
        /// <summary>No library.</summary>
        None,

        /// <summary>Texture files.</summary>
        Texture,

        /// <summary>Img Files.</summary>
        Img,

        /// <summary>Cif Files.</summary>
        Cif,

        /// <summary>Rci Files.</summary>
        Rci,

        /// <summary>Sky Files.</summary>
        Sky,
    }

    #region IEnumerator Class

    /// <summary>
    /// IEnumerator for library files.
    /// </summary>
    internal class LibraryEnumerator : IEnumerator
    {
        /// <summary>Reference to the image file reader.</summary>
        private ImageFileReader _ImageFileReader;

        /// <summary>Index of current enumerator position.</summary>
        private int Index = -1;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ImageFileReader">An instantiated ImageFileReader object with library type set.</param>
        public LibraryEnumerator(ImageFileReader ImageFileReader)
        {
            _ImageFileReader = ImageFileReader;
            Index = -1;
        }

        /// <summary>
        /// Resets index to start.
        /// </summary>
        public void Reset()
        {
            Index = -1;
        }

        public object Current
        {
            get { return _ImageFileReader.GetImageFile(Index); }
        }

        public bool MoveNext()
        {
            Index++;
            if (Index >= _ImageFileReader.FileCount)
                return false;
            else
                return true;
        }
    }

    #endregion

    /// <summary>
    /// A class for enumerating and reading all Daggerfall image files in a uniform manner.
    ///  Provides caching and hashing to store and retrieve file data after being loaded.
    ///  Known unsupported/invalid files will be filtered out automatically.
    ///  Handles loading correct palette for each file.
    ///  Manual enumerators (GetFirstFile, GetNextFile, etc.) are separate to IEnumerator. Usage can be mixed without any problems.
    /// </summary>
    public class ImageFileReader : IEnumerable
    {
        #region Class Variables

        /// <summary>Current Arena2 path.</summary>
        private string MyArena2Path = string.Empty;

        /// <summary>File usage behaviour for all image files.</summary>
        private FileUsage MyFileUsage = FileUsage.UseMemory;

        /// <summary>Current library used for simple enumeration methods.</summary>
        private LibraryTypes MyLibraryType = LibraryTypes.None;

        /// <summary>Index of current file for simple enumeration methods.</summary>
        private int MyFileIndex = -1;

        /// <summary>Last open library type for auto-discard logic.</summary>
        private LibraryTypes LastOpenLibraryType = LibraryTypes.None;

        /// <summary>Last open file index for auto-discard logic.</summary>
        private int LastOpenFileIndex = -1;

        /// <summary>Auto-discard behaviour enabled or disabled.</summary>
        private bool AutoDiscardValue = false;

        // String arrays containing file names only
        private string[] TextureFileNames;
        private string[] ImgFileNames;
        private string[] CifFileNames;
        private string[] RciFileNames;
        private string[] SkyFileNames;

        // Lists containing image file data
        private List<ImageFileItem> TextureFileItems;
        private List<ImageFileItem> ImgFileItems;
        private List<ImageFileItem> CifFileItems;
        private List<ImageFileItem> RciFileItems;
        private List<ImageFileItem> SkyFileItems;

        // Hashtables for finding index to list based on filename
        private Hashtable TextureHashtable;
        private Hashtable ImgHashtable;
        private Hashtable CifHashtable;
        private Hashtable RciHashtable;
        private Hashtable SkyHashtable;

        #endregion

        #region Class Structures

        /// <summary>
        /// Collates image file information.
        /// </summary>
        private struct ImageFileItem
        {
            public string FileName;
            public LibraryTypes LibraryType;
            public DFImageFile ImageFile;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ImageFileReader()
        {
        }

        /// <summary>
        /// Constructor that specifies Arena2 path for image file operations.
        /// </summary>
        /// <param name="Arena2Path">Absolute path to Arena2 folder.</param>
        public ImageFileReader(string Arena2Path)
        {
            SetArena2Path(Arena2Path);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the Arena2 path containing Daggerfall's image file collection.
        /// </summary>
        public string Arena2Path
        {
            get { return MyArena2Path; }
            set { SetArena2Path(value); }
        }

        /// <summary>
        /// Gets or sets file usage behaviour for loading files. Defaults to FileUsage.UseMemory.
        /// </summary>
        public FileUsage Usage
        {
            get { return MyFileUsage; }
            set { MyFileUsage = value; }
        }

        /// <summary>
        /// Gets or sets auto-discard behaviour for open files. If true old file data will be garbage collected.
        /// </summary>
        public bool AutoDiscard
        {
            get { return AutoDiscardValue; }
            set { AutoDiscardValue = value; }
        }

        /// <summary>
        /// Gets or sets the library type to enumerate. Sets enumerator to start of list in specified library.
        /// </summary>
        public LibraryTypes LibraryType
        {
            get { return MyLibraryType; }
            set
            {
                // Set library and clear index
                MyLibraryType = value;
                MyFileIndex = -1;

                // Index library files
                if (!IndexLibraryFiles())
                {
                    MyLibraryType = LibraryTypes.None;
                    return;
                }

                // Go to first file
                GetFirstFileName();
            }
        }

        /// <summary>
        /// Gets first filename of current library type and sets enumerator to start of list.
        /// </summary>
        public string FirstFileName
        {
            get { return GetFirstFileName(); }
        }

        /// <summary>
        /// Gets last filename of current library and sets enumerator to end of list.
        /// </summary>
        public string LastFileName
        {
            get { return GetLastFileName(); }
        }

        /// <summary>
        /// Gets next filename and increments enumerator posiiton.
        /// </summary>
        public string NextFileName
        {
            get { return GetNextFileName(); }
        }

        /// <summary>
        /// Gets previous filename and decrements enumerator position.
        /// </summary>
        public string PreviousFileName
        {
            get { return GetPreviousFileName(); }
        }

        /// <summary>
        /// Gets or sets current filename and relocates enumerator position.
        /// </summary>
        public string FileName
        {
            get { return GetFileName(MyLibraryType, MyFileIndex); }
            set { LoadFile(value); }
        }

        /// <summary>
        /// Gets description of current file.
        /// </summary>
        public string Description
        {
            get { return GetFileDescription(MyLibraryType, MyFileIndex); }
        }

        /// <summary>
        /// Gets number of files in current library.
        /// </summary>
        public int FileCount
        {
            get { return GetFileCount(MyLibraryType); }
        }

        /// <summary>
        /// Gets all file names in current library.
        /// </summary>
        public string[] FileNames
        {
            get { return GetFileNames(MyLibraryType); }
        }

        /// <summary>
        /// Gets number of image records in current file.
        /// </summary>
        public int RecordCount
        {
            get { return GetFileRecordCount(MyLibraryType, MyFileIndex); }
        }

        /// <summary>
        /// Gets current image file. Does not change enumerator position.
        /// </summary>
        public DFImageFile ImageFile
        {
            get { return GetImageFile(MyFileIndex); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads file at the current enumerator position.
        /// </summary>
        /// <returns>True if successful, otherwise false.</returns>
        public bool LoadCurrentFile()
        {
            return LoadFile(MyLibraryType, MyFileIndex);
        }

        /// <summary>
        /// Loads the specified filename and relocates enumerator position.
        ///  If file is not part of the current library, ImageFileReader will change to the appropriate library.
        /// </summary>
        /// <param name="FileName">Name of file.</param>
        /// <returns>DFImageFile object.</returns>
        public DFImageFile LoadFile(string FileName)
        {
            // Change library type based on filename
            LibraryTypes CurrentLibraryType = LibraryType;
            LibraryType = ParseLibraryType(FileName);
            if (LibraryTypes.None == LibraryType)
            {
                LibraryType = CurrentLibraryType;
                return null;
            }

            // Get index of file
            int Index = GetFileIndex(MyLibraryType, FileName);
            if (-1 == Index)
                return null;

            // Load file
            if (LoadFile(MyLibraryType, Index))
                return ImageFile;
            else
                return null;
        }

        /// <summary>
        /// Get a preview of the current file. As many images as possible will be laid out onto the preview surface.
        /// </summary>
        /// <param name="Width">Width of preview surface.</param>
        /// <param name="Height">Height of preview surface.</param>
        /// <param name="Background">Colour of background.</param>
        /// <returns>Bitmap object.</returns>
        public Bitmap GetPreview(int Width, int Height, Color Background)
        {
            // File must be loaded to get description
            if (!LoadFile(MyLibraryType, MyFileIndex))
                return new Bitmap(4,4);

            // Get file preview
            return GetFileItem(MyLibraryType, MyFileIndex).ImageFile.GetPreview(Width, Height, Background); 
        }

        /// <summary>
        /// Gets named image file object from the current library. Does not change enumerator position.
        /// </summary>
        /// <param name="FileName">Name of file.</param>
        /// <returns>DFImageFile object.</returns>
        public DFImageFile GetImageFile(string FileName)
        {
            // Get index of file
            int Index = GetFileIndex(MyLibraryType, FileName);
            return GetImageFile(MyLibraryType, Index);
        }

        /// <summary>
        /// Gets image file object by index, which must be within range (less than FileCount).
        /// </summary>
        /// <param name="Index">Index of file.</param>
        /// <returns>DFImageFile object.</returns>
        public DFImageFile GetImageFile(int Index)
        {
            // Validate
            if (Index < 0 || Index >= FileCount)
                return null;

            return GetImageFile(MyLibraryType, Index);
        }

        /// <summary>
        /// Discards all cached image data.
        /// </summary>
        public void DiscardAll()
        {
            // Discard file name arrays
            TextureFileNames = null;
            ImgFileNames = null;
            CifFileNames = null;
            RciFileNames = null;
            SkyFileNames = null;

            // Discard file arrays
            TextureFileItems = null;
            ImgFileItems = null;
            CifFileItems = null;
            RciFileItems = null;
            SkyFileItems = null;

            // Discard hashtables
            TextureHashtable = null;
            ImgHashtable = null;
            CifHashtable = null;
            RciHashtable = null;
            SkyHashtable = null;

            // Clear current library type, forcing files to be reindexed on next change
            MyLibraryType = LibraryTypes.None;
            LastOpenLibraryType = LibraryTypes.None;
            LastOpenFileIndex = -1;
        }

        /// <summary>
        /// Gets IEnumerator for the current library type.
        /// </summary>
        /// <returns>IEnumerator object.</returns>
        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)new LibraryEnumerator(this);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Set new Arena2 path.
        /// </summary>
        /// <param name="Arena2Path">Absolute path to Arena2 folder.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool SetArena2Path(string Arena2Path)
        {
            // Test path exists
            if (!Directory.Exists(Arena2Path))
                return false;

            // Exit if same path
            if (MyArena2Path == Arena2Path)
                return true;

            // Discard any previous path indexing
            DiscardAll();

            // Store path
            MyArena2Path = Arena2Path;

            return true;
        }

        /// <summary>
        /// Index all image files of current library type.
        /// </summary>
        /// <returns>True if successful, otherwise false.</returns>
        private bool IndexLibraryFiles()
        {
            // Exit if path not set
            if (string.IsNullOrEmpty(MyArena2Path))
                return false;

            // Exit if already created
            if (GetFileCount(MyLibraryType) > 0)
                return true;

            // Get full paths to all files based on library type
            string[] FilePaths;
            switch (MyLibraryType)
            {
                case LibraryTypes.Texture:
                    FilePaths = Directory.GetFiles(MyArena2Path, "TEXTURE.???");
                    break;
                case LibraryTypes.Img:
                    FilePaths = Directory.GetFiles(MyArena2Path, "*.IMG");
                    break;
                case LibraryTypes.Cif:
                    FilePaths = Directory.GetFiles(MyArena2Path, "*.CIF");
                    break;
                case LibraryTypes.Rci:
                    FilePaths = Directory.GetFiles(MyArena2Path, "*.RCI");
                    break;
                case LibraryTypes.Sky:
                    FilePaths = Directory.GetFiles(MyArena2Path, "SKY??.DAT");
                    break;
                default:
                    return false;
            }

            // At least one file must be found
            if (0 == FilePaths.Length)
                return false;

            // Create new file list and hashtable objects
            CreateFileList(MyLibraryType);

            // Index and hash these files
            IndexLibrary(ref FilePaths, MyLibraryType);

            return true;
        }

        /// <summary>
        /// Index all files of specified library type.
        /// </summary>
        /// <param name="FilePaths">Array of absolute paths to files.</param>
        /// <param name="Type">Library type.</param>
        /// <returns></returns>
        private bool IndexLibrary(ref string[] FilePaths, LibraryTypes Type)
        {
            // Create file name arrays
            switch (Type)
            {
                case LibraryTypes.Texture:
                    TextureFileNames = new string[(FilePaths.Length - Arena2.TextureFile.UnsupportedFilenames.Length)];
                    break;
                case LibraryTypes.Img:
                    ImgFileNames = new string[(FilePaths.Length - Arena2.TextureFile.UnsupportedFilenames.Length)];
                    break;
                case LibraryTypes.Cif:
                    CifFileNames = new string[FilePaths.Length];
                    break;
                case LibraryTypes.Rci:
                    RciFileNames = new string[FilePaths.Length];
                    break;
                case LibraryTypes.Sky:
                    SkyFileNames = new string[FilePaths.Length];
                    break;
            }

            // Index and hash files
            int Index = 0;
            TextureFile TextureFile = new TextureFile();
            ImgFile ImgFile = new ImgFile();
            foreach (string FilePath in FilePaths)
            {
                // Get filename only
                string FileName = Path.GetFileName(FilePath);

                // Handle unsupported TEXTURE files
                if (LibraryTypes.Texture == Type)
                    if (!TextureFile.IsFilenameSupported(FileName))
                        continue;

                // Handle unsupported IMG files
                if (LibraryTypes.Img == Type)
                    if (!ImgFile.IsFilenameSupported(FileName))
                        continue;

                // Store file name in array
                switch (Type)
                {
                    case LibraryTypes.Texture:
                        TextureFileNames[Index] = FileName;
                        break;
                    case LibraryTypes.Img:
                        ImgFileNames[Index] = FileName;
                        break;
                    case LibraryTypes.Cif:
                        CifFileNames[Index] = FileName;
                        break;
                    case LibraryTypes.Rci:
                        RciFileNames[Index] = FileName;
                        break;
                    case LibraryTypes.Sky:
                        SkyFileNames[Index] = FileName;
                        break;
                }

                // Create file item
                ImageFileItem Item = new ImageFileItem();
                Item.FileName = FileName;
                Item.LibraryType = Type;
                Item.ImageFile = null;

                // Add file item based on type
                switch (Type)
                {
                    case LibraryTypes.Texture:
                        TextureFileItems.Add(Item);
                        break;
                    case LibraryTypes.Img:
                        ImgFileItems.Add(Item);
                        break;
                    case LibraryTypes.Cif:
                        CifFileItems.Add(Item);
                        break;
                    case LibraryTypes.Rci:
                        RciFileItems.Add(Item);
                        break;
                    case LibraryTypes.Sky:
                        SkyFileItems.Add(Item);
                        break;
                }

                // Hash file index based on type
                switch (Type)
                {
                    case LibraryTypes.Texture:
                        TextureHashtable.Add(FileName, Index);
                        break;
                    case LibraryTypes.Img:
                        ImgHashtable.Add(FileName, Index);
                        break;
                    case LibraryTypes.Cif:
                        CifHashtable.Add(FileName, Index);
                        break;
                    case LibraryTypes.Rci:
                        RciHashtable.Add(FileName, Index);
                        break;
                    case LibraryTypes.Sky:
                        SkyHashtable.Add(FileName, Index);
                        break;
                }

                // Increment index
                Index++;
            }

            return true;
        }

        /// <summary>
        /// Loads file by index within specified library.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <param name="Index">Index of file.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool LoadFile(LibraryTypes Type, int Index)
        {
            // Arena2 must be valid, which implies files are indexed
            if (0 == GetFileCount(Type))
                return false;

            // Get file item and exit if already loaded (ImageFile is non-null)
            ImageFileItem Item = GetFileItem(Type, Index);
            if (null != Item.ImageFile)
            {
                MyFileIndex = Index;
                return true;
            }

            // Discard last file
            DiscardLastFile();

            // Compose full path to file
            string FilePath = Path.Combine(MyArena2Path, GetFileName(Type, Index));

            // Create file object
            Item.ImageFile = CreateFile(Type);
            if (null == Item.ImageFile)
                return false;

            // Load file
            if (!Item.ImageFile.Load(FilePath, Usage, true))
                return false;

            // Load pallette
            string PaletteName = Item.ImageFile.PaletteName;
            if (!string.IsNullOrEmpty(PaletteName))
            {
                string PalettePath = Path.Combine(MyArena2Path, PaletteName);
                Item.ImageFile.LoadPalette(PalettePath);
            }

            // Set file item
            SetFileItem(Type, Index, Item);

            // Store new file index
            MyFileIndex = Index;

            // Store new library and index for auto-discard later
            LastOpenLibraryType = LibraryType;
            LastOpenFileIndex = Index;

            return true;
        }

        /// <summary>
        /// Gets description of file by index within specified library.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <param name="Index">Index of file.</param>
        /// <returns>File description.</returns>
        private string GetFileDescription(LibraryTypes Type, int Index)
        {
            // File must be loaded to get description
            if (!LoadFile(Type, Index))
                return string.Empty;

            // Get file description
            return GetFileItem(Type, Index).ImageFile.Description;
        }

        /// <summary>
        /// Gets record count of file by index within specified library.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <param name="Index">Index of file.</param>
        /// <returns>File record count.</returns>
        private int GetFileRecordCount(LibraryTypes Type, int Index)
        {
            // File must be loaded
            if (!LoadFile(Type, Index))
                return 0;

            // Get file record count
            return GetFileItem(Type, Index).ImageFile.RecordCount;
        }

        /// <summary>
        /// Gets frame count of record by file index within specified library.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <param name="Index">Index of file.</param>
        /// <param name="Record">Record to obtain framecount from.</param>
        /// <returns>File record count.</returns>
        private int GetFileFrameCount(LibraryTypes Type, int Index, int Record)
        {
            // File must be loaded
            if (!LoadFile(Type, Index))
                return 0;

            // Get record frame count
            return GetFileItem(Type, Index).ImageFile.GetFrameCount(Record);
        }

        /// <summary>
        /// Discard a file object so it will be garbage collected.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <param name="Index">Index of file to discard.</param>
        private void DiscardFile(LibraryTypes Type, int Index)
        {
            // Discard by type
            ImageFileItem Item = GetFileItem(Type, Index);
            Item.ImageFile = null;
            SetFileItem(Type, Index, Item);
        }

        /// <summary>
        /// Discard last open file based on auto-discard property.
        /// </summary>
        private void DiscardLastFile()
        {
            // Discard based on auto-discard property
            if (AutoDiscard && LastOpenLibraryType != LibraryTypes.None && LastOpenFileIndex >= 0)
            {
                // Discard last file and clear
                DiscardFile(LastOpenLibraryType, LastOpenFileIndex);
                LastOpenLibraryType = LibraryTypes.None;
                LastOpenFileIndex = -1;
            }
        }

        #endregion

        #region Library List Management

        /// <summary>
        /// Gets image file object of index from specified library.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <param name="Index">Index of file.</param>
        /// <returns>DFImageFile object.</returns>
        private DFImageFile GetImageFile(LibraryTypes Type, int Index)
        {
            // File must be loaded
            if (!LoadFile(Type, Index))
                return null;

            // Get the item
            ImageFileItem Item = GetFileItem(Type, Index);

            return Item.ImageFile;
        }

        /// <summary>
        /// Create new file item lists and hashtables. Old lists and hashtables will be garbage collected.
        /// </summary>
        /// <param name="Type">Library type.</param> 
        private void CreateFileList(LibraryTypes Type)
        {
            // Create new lists to hold file items
            switch (Type)
            {
                case LibraryTypes.Texture:
                    TextureFileItems = new List<ImageFileItem>();
                    break;
                case LibraryTypes.Img:
                    ImgFileItems = new List<ImageFileItem>();
                    break;
                case LibraryTypes.Cif:
                    CifFileItems = new List<ImageFileItem>();
                    break;
                case LibraryTypes.Rci:
                    RciFileItems = new List<ImageFileItem>();
                    break;
                case LibraryTypes.Sky:
                    SkyFileItems = new List<ImageFileItem>();
                    break;
            }
            
            // Create new hashtable for fast index lookups
            switch (Type)
            {
                case LibraryTypes.Texture:
                    TextureHashtable = new Hashtable();
                    break;
                case LibraryTypes.Img:
                    ImgHashtable = new Hashtable();
                    break;
                case LibraryTypes.Cif:
                    CifHashtable = new Hashtable();
                    break;
                case LibraryTypes.Rci:
                    RciHashtable = new Hashtable();
                    break;
                case LibraryTypes.Sky:
                    SkyHashtable = new Hashtable();
                    break;
            }
        }

        /// <summary>
        /// Gets file item by index within specified library.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <param name="Index">Index of file.</param>
        /// <returns>ImageFileItem object.</returns>
        private ImageFileItem GetFileItem(LibraryTypes Type, int Index)
        {
            // Index must be within range
            if (Index < 0  || Index >= GetFileCount(Type))
                return new ImageFileItem();

            // Get path by index
            switch (Type)
            {
                case LibraryTypes.Texture:
                    return TextureFileItems[Index];
                case LibraryTypes.Img:
                    return ImgFileItems[Index];
                case LibraryTypes.Cif:
                    return CifFileItems[Index];
                case LibraryTypes.Rci:
                    return RciFileItems[Index];
                case LibraryTypes.Sky:
                    return SkyFileItems[Index];
                default:
                    return new ImageFileItem();
            }
        }

        /// <summary>
        /// Sets file item by index within specified library.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <param name="Index">Index of file.</param>
        /// <param name="Item">ImageFileItem to store.</param>
        private void SetFileItem(LibraryTypes Type, int Index, ImageFileItem Item)
        {
            // Index must be within range
            if (Index < 0 || Index >= GetFileCount(Type))
                return;

            // Get path by index
            switch (Type)
            {
                case LibraryTypes.Texture:
                    TextureFileItems[Index] = Item;
                    break;
                case LibraryTypes.Img:
                    ImgFileItems[Index] = Item;
                    break;
                case LibraryTypes.Cif:
                    CifFileItems[Index] = Item;
                    break;
                case LibraryTypes.Rci:
                    RciFileItems[Index] = Item;
                    break;
                case LibraryTypes.Sky:
                    SkyFileItems[Index] = Item;
                    break;
            }
        }

        /// <summary>
        /// Creates a new file resource by library type.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <returns>DFImageFile object.</returns>
        private DFImageFile CreateFile(LibraryTypes Type)
        {
            switch (Type)
            {
                case LibraryTypes.Texture:
                    return new TextureFile();
                case LibraryTypes.Img:
                    return new ImgFile();
                case LibraryTypes.Cif:
                case LibraryTypes.Rci:
                    return new CifRciFile();
                case LibraryTypes.Sky:
                    return new SkyFile();
                default:
                    return null;
            }
        }

        #endregion

        #region Enumeration Methods

        /// <summary>
        /// Gets first filename of current library. Sets file index to start.
        /// </summary>
        /// <returns>First filename in list.</returns>
        private string GetFirstFileName()
        {
            // LibraryType must be set
            if (LibraryType == LibraryTypes.None)
                return string.Empty;

            // Set index back to start
            MyFileIndex = 0;

            // Return first filename
            return GetFileName(MyLibraryType, MyFileIndex);
        }

        /// <summary>
        /// Gets last filename of current library. Sets file index to end.
        /// </summary>
        /// <returns>Last filename in list.</returns>
        private string GetLastFileName()
        {
            // LibraryType must be set
            if (LibraryType == LibraryTypes.None)
                return string.Empty;

            // Get count of files and exit if less than one
            int Count = GetFileCount(MyLibraryType);
            if (Count < 1)
                return string.Empty;

            // Set index to last file of library
            MyFileIndex = Count - 1;

            // Return last filename
            return GetFileName(MyLibraryType, MyFileIndex);
        }

        /// <summary>
        /// Gets next filename of current library. Increments file index.
        /// </summary>
        /// <returns>Next filename in list.</returns>
        private string GetNextFileName()
        {
            // LibraryType must be set
            if (LibraryType == LibraryTypes.None)
                return string.Empty;

            // Handle end of array
            if ((MyFileIndex + 1) == (GetFileCount(MyLibraryType)))
                return string.Empty;

            // Increment index
            MyFileIndex++;

            // Return filename
            return GetFileName(MyLibraryType, MyFileIndex);
        }

        /// <summary>
        /// Gets previous filename of current library. Decrements file index.
        /// </summary>
        /// <returns>Previous filename in list.</returns>
        private string GetPreviousFileName()
        {
            // LibraryType must be set
            if (LibraryType == LibraryTypes.None)
                return string.Empty;

            // Handle start of array
            if (MyFileIndex == 0)
                return string.Empty;

            // Decrement index
            MyFileIndex--;

            // Return filename
            return GetFileName(MyLibraryType, MyFileIndex);
        }

        /// <summary>
        /// Gets number of files in specified library.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <returns>File count.</returns>
        private int GetFileCount(LibraryTypes Type)
        {
            // Get count by type
            switch (Type)
            {
                case LibraryTypes.Texture:
                    if (null == TextureFileItems) return 0; else return TextureFileItems.Count;
                case LibraryTypes.Img:
                    if (null == ImgFileItems) return 0; else return ImgFileItems.Count;
                case LibraryTypes.Cif:
                    if (null == CifFileItems) return 0; else return CifFileItems.Count;
                case LibraryTypes.Rci:
                    if (null == RciFileItems) return 0; else return RciFileItems.Count;
                case LibraryTypes.Sky:
                    if (null == SkyFileItems) return 0; else return SkyFileItems.Count;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets absolute path to file by index within specified library.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <param name="Index">Index of file.</param>
        /// <returns>Name of file.</returns>
        private string GetFileName(LibraryTypes Type, int Index)
        {
            // Index must be within range
            if (Index < 0 || Index >= GetFileCount(Type))
                return string.Empty;

            // Get path by index
            switch (Type)
            {
                case LibraryTypes.Texture:
                    return TextureFileItems[Index].FileName;
                case LibraryTypes.Img:
                    return ImgFileItems[Index].FileName;
                case LibraryTypes.Cif:
                    return CifFileItems[Index].FileName;
                case LibraryTypes.Rci:
                    return RciFileItems[Index].FileName;
                case LibraryTypes.Sky:
                    return SkyFileItems[Index].FileName;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Gets array of file names for specified library.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <returns>Array of filenames.</returns>
        private string[] GetFileNames(LibraryTypes Type)
        {
            // Get by type
            switch (Type)
            {
                case LibraryTypes.Texture:
                    return TextureFileNames;
                case LibraryTypes.Img:
                    return ImgFileNames;
                case LibraryTypes.Cif:
                    return CifFileNames;
                case LibraryTypes.Rci:
                    return RciFileNames;
                case LibraryTypes.Sky:
                    return SkyFileNames;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets index of file in specified library.
        /// </summary>
        /// <param name="Type">Library type.</param>
        /// <param name="FileName">Name of file.</param>
        /// <returns>Index of file.</returns>
        private int GetFileIndex(LibraryTypes Type, string FileName)
        {
            // Check file exists
            switch (Type)
            {
                case LibraryTypes.Texture:
                    if (!TextureHashtable.ContainsKey(FileName)) return -1;
                    break;
                case LibraryTypes.Img:
                    if (!ImgHashtable.ContainsKey(FileName)) return -1;
                    break;
                case LibraryTypes.Cif:
                    if (!CifHashtable.ContainsKey(FileName)) return -1;
                    break;
                case LibraryTypes.Rci:
                    if (!RciHashtable.ContainsKey(FileName)) return -1;
                    break;
                case LibraryTypes.Sky:
                    if (!SkyHashtable.ContainsKey(FileName)) return -1;
                    break;
                default:
                    return -1;
            }

            // Get index based on type
            switch (Type)
            {
                case LibraryTypes.Texture:
                    return (int)TextureHashtable[FileName];
                case LibraryTypes.Img:
                    return (int)ImgHashtable[FileName];
                case LibraryTypes.Cif:
                    return (int)CifHashtable[FileName];
                case LibraryTypes.Rci:
                    return (int)RciHashtable[FileName];
                case LibraryTypes.Sky:
                    return (int)SkyHashtable[FileName];
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Determines library type from file name.
        /// </summary>
        /// <param name="FileName">Name of file.</param>
        /// <returns>Library type of this file.</returns>
        private LibraryTypes ParseLibraryType(string FileName)
        {
            FileName = FileName.ToUpper();
            if (FileName.StartsWith("TEXTURE."))
                return LibraryTypes.Texture;
            else if (FileName.EndsWith(".IMG"))
                return LibraryTypes.Img;
            else if (FileName.EndsWith(".CIF"))
                return LibraryTypes.Cif;
            else if (FileName.EndsWith(".RCI"))
                return LibraryTypes.Rci;
            else if (FileName.StartsWith("SKY"))
                return LibraryTypes.Sky;
            else
                return LibraryTypes.None;
        }

        #endregion
    }
}
