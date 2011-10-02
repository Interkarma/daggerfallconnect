// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{
    /// <summary>
    /// Helper class to load and store Daggerfall textures for XNA.
    ///  Provides some loading and pre-processing options. See TextureCreateFlags for more details.
    /// </summary>
    public class TextureManager
    {

        #region Class Variables

        // XNA
        SpriteBatch spriteBatch = null;

        // Class
        private string arena2Path;
        private GraphicsDevice graphicsDevice;
        private TextureFile textureFile;

        // Texture dictionaries
        private const int atlasTextureKey = -1000000;
        private Dictionary<int, Texture2D> generalTextureDict;
        private Dictionary<int, Texture2D> winterTextureDict;

        // Climate and weather
        DFLocation.ClimateBaseType climateType = DFLocation.ClimateBaseType.None;
        DFLocation.ClimateWeather climateWeather = DFLocation.ClimateWeather.Normal;

        // Constants
        private const int formatWidth = 4;
        private const int tileSide = 64;
        private const int groundTextureWidth = 1024;
        private const int groundTextureHeight = 1024;
        private const int groundTextureStride = groundTextureWidth * formatWidth;
        const string formatError = "DFBitmap not RGBA.";

        #endregion

        #region Class Structures

        /// <summary>
        /// Flags to modify how texture is created.
        /// </summary>
        [Flags]
        public enum TextureCreateFlags
        {
            /// <summary>
            /// No flags set.
            /// </summary>
            None = 0,

            /// <summary>
            /// Attempts to process texture into current Climate and
            ///  Weather. TextureManager will atempt to ignore invalid
            ///  climate swaps.
            /// </summary>
            ApplyClimate = 1,

            /// <summary>
            /// Ensures texture will be POW2 by extending right and
            ///  bottom dimensions to nearest POW2 boundary. Texture
            ///  will remain at top-left of total area. It is the
            ///  responsibility of the caller to ensure model UVs are
            ///  modified to correctly address valid space in the
            ///  POW2 texture.
            ///  </summary>
            PowerOfTwo = 2,

            /// <summary>
            /// Blends colour into neighbouring alpha pixels to help
            ///  create softer borders. Most useful when loading
            ///  billboard textures. Also extends image dimensions
            ///  by a couple of pixels around each edge so images do
            ///  not appear to be cut off at the edges.
            /// </summary>
            Dilate = 4,

            /// <summary>
            /// Pre-multiplies alpha with colour channels. It is the
            ///  responsibility of the caller to ensure renderer blend
            ///  states are set to use pre-multiplied textures.
            /// </summary>
            PreMultiplyAlpha = 8,

            /// <summary>
            /// Creates a chain of mipmaps for this texture.
            /// </summary>
            MipMaps = 16,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets GraphicsDevice set at construction.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }

        /// <summary>
        /// Gets Arena2 path set at construction.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
        }

        /// <summary>
        /// Gets or sets current climate type for swaps.
        /// </summary>
        public DFLocation.ClimateBaseType ClimateType
        {
            get { return climateType; }
            set { SetClimate(value, climateWeather); }
        }

        /// <summary>
        /// Gets or sets current weather for swaps.
        /// </summary>
        public DFLocation.ClimateWeather Weather
        {
            get { return climateWeather; }
            set { SetClimate(climateType, value); }
        }

        /// <summary>
        /// Gets reserved key for terrain textures.
        /// </summary>
        public static int TerrainAtlasKey
        {
            get { return atlasTextureKey; }
        }

        /// <summary>
        /// Gets TextureFile object.
        /// </summary>
        public TextureFile TextureFile
        {
            get { return textureFile; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="device">Graphics Device.</param>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        public TextureManager(GraphicsDevice device, string arena2Path)
        {
            // Setup
            graphicsDevice = device;
            this.arena2Path = arena2Path;
            textureFile = new TextureFile();
            textureFile.Palette.Load(Path.Combine(arena2Path, textureFile.PaletteName));
            spriteBatch = new SpriteBatch(graphicsDevice);

            // Create dictionaries
            generalTextureDict = new Dictionary<int, Texture2D>();
            winterTextureDict = new Dictionary<int, Texture2D>();

            // Set default climate
            SetClimate(climateType, climateWeather);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads a texture from archive and record index. Animated textures are
        ///  not supported at this time and only first frame will be loaded.
        ///  When using climate swaps the key returned will be stable. This means
        ///  you can store this key with your mesh/scene and TextureManager will
        ///  return the correct climate and weather variant for that key when you
        ///  call GetTexture(key) during rendering.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        /// <returns>Texture key.</returns>
        public int LoadTexture(int archive, int record, TextureCreateFlags flags)
        {
            // Load based on flags
            if (TextureCreateFlags.ApplyClimate == (flags & TextureCreateFlags.ApplyClimate) &&
                climateType != DFLocation.ClimateBaseType.None)
            {
                return LoadTextureWithClimate(archive, record, flags);
            }
            else
            {
                return LoadTextureNoClimate(archive, record, flags);
            }
        }

        /// <summary>
        /// Gets texture by key.
        ///  Use static TerrainAtlasKey property for terrain key.
        ///  Manager will return NULL if texture does not exist.
        /// </summary>
        /// <param name="key">Texture key.</param>
        /// <returns>Texture2D.</returns>
        public Texture2D GetTexture(int key)
        {
            // Try to return winter texture when required
            if (this.climateWeather == DFLocation.ClimateWeather.Winter)
            {
                if (winterTextureDict.ContainsKey(key))
                    return winterTextureDict[key];
            }

            // Otherwise return general texture
            if (!generalTextureDict.ContainsKey(key))
                return null;
            else
                return generalTextureDict[key];
        }

        /// <summary>
        /// Remove cached texture based on key.
        ///  Cannot remove terrain atlas by key,
        ///  use ClearAtlases() method instead.
        /// </summary>
        /// <param name="key">Texture key.</param>
        public void RemoveTexture(int key)
        {
            // Remove general texture og key
            if (generalTextureDict.ContainsKey(key))
                generalTextureDict.Remove(key);

            // Remove winter texture of key
            if (winterTextureDict.ContainsKey(key))
                winterTextureDict.Remove(key);
        }

        /// <summary>
        /// Clear all cached textures.
        /// </summary>
        public void ClearTextures()
        {
            // Remove general and winter dictionaries
            generalTextureDict.Clear();
            winterTextureDict.Clear();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets unique key for a non-climate-aware texture.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <returns>Texture key.</returns>
        private int GetTextureKey(int archive, int record)
        {
            return (archive * 1000) + record;
        }

        /// <summary>
        /// Gets unique key for a climate-aware texture.
        /// </summary>
        /// <param name="climateType">Climate type.</param>
        /// <param name="climateSet">Climate set.</param>
        /// <param name="record">Record index.</param>
        /// <returns>Texture key.</returns>
        private int GetTextureKey(DFLocation.ClimateBaseType climateType, DFLocation.ClimateTextureSet climateSet, int record)
        {
            return 1000000 + ((int)climateType * 100000) + ((int)climateSet * 1000) + record;
        }

        /// <summary>
        /// Gets unique key for atlas sub-textures.
        /// </summary>
        /// <param name="set">Climate set.</param>
        /// <param name="weather">Climate weather.</param>
        /// <param name="record">Record index.</param>
        /// <returns>Climate texture key.</returns>
        private int GetAtlasTextureKey(DFLocation.ClimateTextureSet set, DFLocation.ClimateWeather weather, int record)
        {
            return (int)set * 10000 + (int)weather * 100 + record;
        }

        /// <summary>
        /// Load a texture without climate processing.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        /// <returns>Texture key.</returns>
        private int LoadTextureNoClimate(int archive, int record, TextureCreateFlags flags)
        {
            int key = GetTextureKey(archive, record);
            if (generalTextureDict.ContainsKey(key))
                return key;
            else
                CreateTexture(key, archive, record, false, flags);

            return key;
        }

        /// <summary>
        /// Load a texture with climate processing.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        /// <returns>Texture key.</returns>
        private int LoadTextureWithClimate(int archive, int record, TextureCreateFlags flags)
        {
            // Get the base set this archive belongs to regardless of climate
            bool supportsWinter, supportsRain;
            DFLocation.ClimateTextureSet climateSet = GetClimateSet(archive, out supportsWinter, out supportsRain);

            // Load non-climate-aware textures without climate processing
            if (DFLocation.ClimateTextureSet.None == climateSet)
            {
                return LoadTextureNoClimate(archive, record, flags);
            }

            // Handle missing Swamp textures
            if (climateType == DFLocation.ClimateBaseType.Swamp)
            {
                switch (climateSet)
                {
                    case DFLocation.ClimateTextureSet.Interior_TempleInt:
                    case DFLocation.ClimateTextureSet.Interior_MarbleFloors:
                        return LoadTextureNoClimate(archive, record, flags);
                }
            }

            // Check if key already exists
            int key = GetTextureKey(climateType, climateSet, record);
            if (this.climateWeather == DFLocation.ClimateWeather.Winter)
            {
                if (winterTextureDict.ContainsKey(key))
                    return key;
            }
            else
            {
                if (generalTextureDict.ContainsKey(key))
                    return key;
            }

            // Handle specific climate sets with missing winter textures
            if (climateType == DFLocation.ClimateBaseType.Desert ||
                climateType == DFLocation.ClimateBaseType.Swamp)
            {
                switch (climateSet)
                {
                    case DFLocation.ClimateTextureSet.Exterior_Castle:
                    case DFLocation.ClimateTextureSet.Exterior_MagesGuild:
                        supportsWinter = false;
                        break;
                }
            }

            // Load climate-aware texture
            int newArchive = (int)climateType + (int)climateSet;
            CreateTexture(key, newArchive, record, supportsWinter, flags);
            return key;
        }

        /// <summary>
        /// Creates texture for the specified archive and record, then
        ///  adds to dictionary against key. Will attempt to load a
        ///  winter variant if specified.
        /// </summary>
        /// <param name="key">Key to associate with texture.</param>
        /// <param name="archive">Archive index to load.</param>
        /// <param name="record">Record index to load.</param>
        /// <param name="supportsWinter">True to load winter set (archive+1).</param>
        /// <param name="flags">TextureCreateFlags.</param>
        private void CreateTexture(int key, int archive, int record, bool supportsWinter, TextureCreateFlags flags)
        {
            // Get normal texture in RGBA format
            textureFile.Load(Path.Combine(arena2Path, TextureFile.IndexToFileName(archive)), FileUsage.UseDisk, true);
            DFBitmap normalBitmap = textureFile.GetBitmapFormat(record, 0, 0, DFBitmap.Formats.RGBA);

            // Perform optional image processing
            ProcessDFBitmap(ref normalBitmap, flags);

            // Create XNA texture
            generalTextureDict.Add(key, CreateTexture2D(ref normalBitmap, flags));

            // Load winter texture
            if (supportsWinter)
            {
                // Get winter texture in RGBA format
                textureFile.Load(Path.Combine(arena2Path, TextureFile.IndexToFileName(archive + 1)), FileUsage.UseDisk, true);
                DFBitmap winterBitmap = textureFile.GetBitmapFormat(record, 0, 0, DFBitmap.Formats.RGBA);

                // Perform optional image processing
                ProcessDFBitmap(ref normalBitmap, flags);
                
                // Create XNA texture
                winterTextureDict.Add(key, CreateTexture2D(ref winterBitmap, flags));
            }
        }

        /// <summary>
        /// Creates Texture2D from DFBitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap source. Must be in ARGB format.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        /// <returns>Texture2D.</returns>
        private Texture2D CreateTexture2D(ref DFBitmap dfBitmap, TextureCreateFlags flags)
        {
            // Get dimensions of new texture
            int width, height;
            if (TextureCreateFlags.PowerOfTwo == (flags & TextureCreateFlags.PowerOfTwo))
            {
                width = (PowerOfTwo.IsPowerOfTwo(dfBitmap.Width)) ? dfBitmap.Width : PowerOfTwo.NextPowerOfTwo(dfBitmap.Width);
                height = (PowerOfTwo.IsPowerOfTwo(dfBitmap.Height)) ? dfBitmap.Height : PowerOfTwo.NextPowerOfTwo(dfBitmap.Height);
            }
            else
            {
                width = dfBitmap.Width;
                height = dfBitmap.Height;
            }

            // Create new texture
            Texture2D texture = new Texture2D(
                    graphicsDevice,
                    width,
                    height,
                    flags.HasFlag(TextureCreateFlags.MipMaps),
                    SurfaceFormat.Color);

            // Set texture data
            for (int level = 0; level < texture.LevelCount; level++)
            {
                // Level 0 is always full size
                if (level == 0)
                {
                    texture.SetData<byte>(
                            0,
                            new Microsoft.Xna.Framework.Rectangle(0, 0, dfBitmap.Width, dfBitmap.Height),
                            dfBitmap.Data,
                            0,
                            dfBitmap.Width * dfBitmap.Height * formatWidth);
                    continue;
                }

                // Create mipmap
                DFBitmap newBitmap = CreateMipMap(ref dfBitmap, level);
                texture.SetData<byte>(
                            level,
                            new Microsoft.Xna.Framework.Rectangle(0, 0, newBitmap.Width, newBitmap.Height),
                            newBitmap.Data,
                            0,
                            newBitmap.Width * newBitmap.Height * formatWidth);
            }

            return texture;
        }

        #endregion

        #region Image Processing

        /// <summary>
        /// Performs optional processing to DFBitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        private void ProcessDFBitmap(ref DFBitmap dfBitmap, TextureCreateFlags flags)
        {
            // Dilate
            if (flags.HasFlag(TextureCreateFlags.Dilate))
                DilateDFBitmap(ref dfBitmap);

            // Pre-multiply alpha
            if (flags.HasFlag(TextureCreateFlags.PreMultiplyAlpha))
                PreMultiplyAlphaDFBitmap(ref dfBitmap);
        }

        /// <summary>
        /// Pre-multiply bitmap alpha.
        /// </summary>
        /// <param name="dfBitmap">FBitmap.</param>
        private void PreMultiplyAlphaDFBitmap(ref DFBitmap dfBitmap)
        {
            // Format must be RGBA
            if (dfBitmap.Format != DFBitmap.Formats.RGBA)
                throw new Exception(formatError);

            // Pre-multiply alpha for each pixel
            int pos;
            float multiplier;
            for (int y = 0; y < dfBitmap.Height; y++)
            {
                pos = y * dfBitmap.Stride;
                for (int x = 0; x < dfBitmap.Width; x++)
                {
                    multiplier = dfBitmap.Data[pos + 3] / 256f;
                    dfBitmap.Data[pos] = (byte)(dfBitmap.Data[pos] * multiplier);
                    dfBitmap.Data[pos + 1] = (byte)(dfBitmap.Data[pos + 1] * multiplier);
                    dfBitmap.Data[pos + 2] = (byte)(dfBitmap.Data[pos + 2] * multiplier);
                    pos += formatWidth;
                }
            }
        }

        /// <summary>
        /// Rotates a DFBitmap 90 degrees counter-clockwise.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        private void RotateDFBitmap(ref DFBitmap dfBitmap)
        {
            // Format must be RGBA
            if (dfBitmap.Format != DFBitmap.Formats.RGBA)
                throw new Exception(formatError);

            // Create destination bitmap to receive rotated image
            DFBitmap dstBitmap = new DFBitmap();
            dstBitmap.Format = dfBitmap.Format;
            dstBitmap.Width = dfBitmap.Width;
            dstBitmap.Height = dfBitmap.Height;
            dstBitmap.Stride = dfBitmap.Stride;
            dstBitmap.Data = new byte[dfBitmap.Data.Length];

            // Rotate image
            int srcPos = 0;
            int dstPos = 0;
            for (int x = 0; x < dstBitmap.Width; x++)
            {
                for (int y = dstBitmap.Height - 1; y >=0 ; y--)
                {
                    // Get source pixel
                    byte r = dfBitmap.Data[srcPos++];
                    byte g = dfBitmap.Data[srcPos++];
                    byte b = dfBitmap.Data[srcPos++];
                    byte a = dfBitmap.Data[srcPos++];

                    // Write destination pixel
                    dstPos = y * dstBitmap.Stride + x * formatWidth;
                    dstBitmap.Data[dstPos++] = r;
                    dstBitmap.Data[dstPos++] = g;
                    dstBitmap.Data[dstPos++] = b;
                    dstBitmap.Data[dstPos++] = a;
                }
            }

            // Assign processed image
            dfBitmap = dstBitmap;
        }

        /// <summary>
        /// Flips a DFBitmap horizontally and vertically.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        private void FlipDFBitmap(ref DFBitmap dfBitmap)
        {
            // Format must be RGBA
            if (dfBitmap.Format != DFBitmap.Formats.RGBA)
                throw new Exception(formatError);

            // Create destination bitmap to receive flipped image
            DFBitmap dstBitmap = new DFBitmap();
            dstBitmap.Format = dfBitmap.Format;
            dstBitmap.Width = dfBitmap.Width;
            dstBitmap.Height = dfBitmap.Height;
            dstBitmap.Stride = dfBitmap.Stride;
            dstBitmap.Data = new byte[dfBitmap.Data.Length];

            // Flip image
            int srcPos = 0;
            int dstPos = dstBitmap.Data.Length - 1;
            for (int i = 0; i < dfBitmap.Width * dfBitmap.Height; i++)
            {
                // Get source pixel
                byte r = dfBitmap.Data[srcPos++];
                byte g = dfBitmap.Data[srcPos++];
                byte b = dfBitmap.Data[srcPos++];
                byte a = dfBitmap.Data[srcPos++];

                // Write destination pixel
                dstBitmap.Data[dstPos--] = a;
                dstBitmap.Data[dstPos--] = b;
                dstBitmap.Data[dstPos--] = g;
                dstBitmap.Data[dstPos--] = r;
            }

            // Assign processed image
            dfBitmap = dstBitmap;
        }

        /// <summary>
        /// Dilate bitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        private void DilateDFBitmap(ref DFBitmap dfBitmap)
        {
            // Format must be RGBA
            if (dfBitmap.Format != DFBitmap.Formats.RGBA)
                throw new Exception(formatError);

            // Constants
            const int paddingPixels = 1;

            // Create larger bitmap to receive dilated image
            DFBitmap dstBitmap = new DFBitmap();
            dstBitmap.Format = dfBitmap.Format;
            dstBitmap.Width = dfBitmap.Width + (paddingPixels * 2);
            dstBitmap.Height = dfBitmap.Height + (paddingPixels * 2);
            dstBitmap.Stride = dstBitmap.Width * formatWidth;
            dstBitmap.Data = new byte[dstBitmap.Stride * dstBitmap.Height];

            // Process image
            int srcPos, dstPos;
            for (int y = 0; y < dfBitmap.Height; y++)
            {
                srcPos = y * dfBitmap.Stride;
                dstPos = (y * dstBitmap.Stride) + (paddingPixels * dstBitmap.Stride) + (paddingPixels * formatWidth);
                for (int x = 0; x < dfBitmap.Width; x++)
                {
                    // Copy pixel to top, bottom, left, right
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos - dstBitmap.Stride);
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos + dstBitmap.Stride);
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos - formatWidth);
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos + formatWidth);

                    // Copy pixel to four corners
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos - dstBitmap.Stride - formatWidth);
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos - dstBitmap.Stride + formatWidth);
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos + dstBitmap.Stride - formatWidth);
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos + dstBitmap.Stride + formatWidth);

                    // Copy central colour pixel
                    dstBitmap.Data[dstPos] = dfBitmap.Data[srcPos];
                    dstBitmap.Data[dstPos + 1] = dfBitmap.Data[srcPos + 1];
                    dstBitmap.Data[dstPos + 2] = dfBitmap.Data[srcPos + 2];
                    dstBitmap.Data[dstPos + 3] = dfBitmap.Data[srcPos + 3];

                    srcPos += formatWidth;
                    dstPos += formatWidth;
                }
            }

            // Assign processed image
            dfBitmap = dstBitmap;
        }

        /// <summary>
        /// Tests a pixel for transparancy and clones colour information.
        /// </summary>
        /// <param name="srcBuffer">Source buffer.</param>
        /// <param name="dstBuffer">Destination buffer.</param>
        /// <param name="srcPos">Source position.</param>
        /// <param name="dstPos">Destination position.</param>
        private void TestCopyPixel(ref byte[] srcBuffer, ref byte[] dstBuffer, int srcPos, int dstPos)
        {
            // Do nothing if source pixel is transparant
            if (srcBuffer[srcPos + 3] == 0)
                return;

            // Copy source colour if destination pixel is transparant
            if (dstBuffer[dstPos + 3] == 0)
            {
                dstBuffer[dstPos] = srcBuffer[srcPos];
                dstBuffer[dstPos + 1] = srcBuffer[srcPos + 1];
                dstBuffer[dstPos + 2] = srcBuffer[srcPos + 2];
                dstBuffer[dstPos + 3] = 0x04;
            }
        }

        /// <summary>
        /// Creates a new mipmap from the source bitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap source.</param>
        /// <param name="level">MipMap level.</param>
        /// <returns>DFBitmap mipmap.</returns>
        private DFBitmap CreateMipMap(ref DFBitmap dfBitmap, int level)
        {
            // Format must be RGBA
            if (dfBitmap.Format != DFBitmap.Formats.RGBA)
                throw new Exception(formatError);

            // Get start size
            int width = dfBitmap.Width;
            int height = dfBitmap.Height;

            // Get new size for level
            int newWidth = PowerOfTwo.MipMapSize(dfBitmap.Width, level);
            int newHeight = PowerOfTwo.MipMapSize(dfBitmap.Height, level);

            // Create new bitmap
            DFBitmap newBitmap = new DFBitmap();
            newBitmap.Format = DFBitmap.Formats.RGBA;
            newBitmap.Width = newWidth;
            newBitmap.Height = newHeight;
            newBitmap.Stride = newWidth * formatWidth;
            newBitmap.Data = new byte[newBitmap.Stride * newBitmap.Height];

            // Scale factors
            double xFactor = (double)width / newWidth;
            double yFactor = (double)height / newHeight;

            // Coordinates of source points and coefficients
            double ox, oy, dx, dy, k1, k2;
            int ox1, oy1, ox2, oy2;

            // Destination pixel values
            double r, g, b, a;

            // Width and height decreased by 1
            int ymax = height - 1;
            int xmax = width - 1;

            // Bicubic resize
            for (int y = 0; y < newHeight; y++)
            {
                // Y coordinates
                oy = (double)y * yFactor - 0.5f;
                oy1 = (int)oy;
                dy = oy - (double)oy1;

                for (int x = 0; x < newWidth; x++)
                {
                    // X coordinates
                    ox = (double)x * xFactor - 0.5f;
                    ox1 = (int)ox;
                    dx = ox - (double)ox1;

                    // Initial pixel value
                    r = g = b = a = 0;

                    for (int n = -1; n < 3; n++)
                    {
                        // Get Y coefficient
                        k1 = BiCubicKernel(dy - (double)n);

                        oy2 = oy1 + n;
                        if (oy2 < 0)
                            oy2 = 0;
                        if (oy2 > ymax)
                            oy2 = ymax;

                        for (int m = -1; m < 3; m++)
                        {
                            // Get X coefficient
                            k2 = k1 * BiCubicKernel((double)m - dx);

                            ox2 = ox1 + m;
                            if (ox2 < 0)
                                ox2 = 0;
                            if (ox2 > xmax)
                                ox2 = xmax;

                            // Get pixel of original image
                            int srcPos = (ox2 * formatWidth) + (dfBitmap.Stride * oy2);
                            r += k2 * dfBitmap.Data[srcPos++];
                            g += k2 * dfBitmap.Data[srcPos++];
                            b += k2 * dfBitmap.Data[srcPos++];
                            a += k2 * dfBitmap.Data[srcPos];
                        }
                    }

                    // Set destination pixel
                    int dstPos = (x * formatWidth) + (newBitmap.Stride * y);
                    newBitmap.Data[dstPos++] = (byte)r;
                    newBitmap.Data[dstPos++] = (byte)g;
                    newBitmap.Data[dstPos++] = (byte)b;
                    newBitmap.Data[dstPos] = (byte)a;
                }
            }

            return newBitmap;
        }

        /// <summary>
        /// BiCubic calculations.
        /// </summary>
        /// <param name="x">Value.</param>
        /// <returns>Double.</returns>
        private double BiCubicKernel(double x)
        {
            if (x > 2.0)
                return 0.0;

            double a, b, c, d;
            double xm1 = x - 1.0;
            double xp1 = x + 1.0;
            double xp2 = x + 2.0;

            a = (xp2 <= 0.0) ? 0.0 : xp2 * xp2 * xp2;
            b = (xp1 <= 0.0) ? 0.0 : xp1 * xp1 * xp1;
            c = (x <= 0.0) ? 0.0 : x * x * x;
            d = (xm1 <= 0.0) ? 0.0 : xm1 * xm1 * xm1;

            return (0.16666666666666666667 * (a - (4.0 * b) + (6.0 * c) - (4.0 * d)));
        }

        #endregion

        #region Climate Swaps

        /// <summary>
        /// Sets climate and weather type to use for texture swaps.
        /// </summary>
        /// <param name="climate">Climate type.</param>
        /// <param name="weather">Weather type.</param>
        private void SetClimate(DFLocation.ClimateBaseType climate, DFLocation.ClimateWeather weather)
        {
            // Store new climate settings
            this.climateType = climate;
            this.climateWeather = weather;
        }

        /// <summary>
        /// Gets the base climate set the specified archive belongs to.
        /// </summary>
        /// <param name="archive">Archive from which to derive set.</param>
        /// <param name="supportsWinter">True if there is a winter version of this set.</param>
        /// <param name="supportsRain">True if there is a rain version of this set.</param>
        /// <returns>Derived ClimateSet.</returns>
        private DFLocation.ClimateTextureSet GetClimateSet(int archive, out bool supportsWinter, out bool supportsRain)
        {
            // Get climate set
            supportsWinter = false;
            supportsRain = false;
            DFLocation.ClimateTextureSet set = (DFLocation.ClimateTextureSet)(archive - (archive / 100) * 100);
            switch (set)
            {
                //
                // Terrain sets
                //
                case DFLocation.ClimateTextureSet.Exterior_Terrain:
                    supportsWinter = true;
                    supportsRain = true;
                    break;
                //
                // Exterior sets
                //
                case DFLocation.ClimateTextureSet.Exterior_Castle:
                case DFLocation.ClimateTextureSet.Exterior_CityA:
                case DFLocation.ClimateTextureSet.Exterior_CityB:
                case DFLocation.ClimateTextureSet.Exterior_CityWalls:
                case DFLocation.ClimateTextureSet.Exterior_Farm:
                case DFLocation.ClimateTextureSet.Exterior_Fences:
                case DFLocation.ClimateTextureSet.Exterior_MagesGuild:
                case DFLocation.ClimateTextureSet.Exterior_Manor:
                case DFLocation.ClimateTextureSet.Exterior_MerchantHomes:
                case DFLocation.ClimateTextureSet.Exterior_Roofs:
                case DFLocation.ClimateTextureSet.Exterior_Ruins:
                case DFLocation.ClimateTextureSet.Exterior_TavernExteriors:
                case DFLocation.ClimateTextureSet.Exterior_TempleExteriors:
                case DFLocation.ClimateTextureSet.Exterior_Village:
                    supportsWinter = true;
                    break;
                //
                // Interior sets
                //
                case DFLocation.ClimateTextureSet.Interior_Caves:
                case DFLocation.ClimateTextureSet.Interior_CityInt:
                case DFLocation.ClimateTextureSet.Interior_CryptA:
                case DFLocation.ClimateTextureSet.Interior_CryptB:
                case DFLocation.ClimateTextureSet.Interior_Doors:
                case DFLocation.ClimateTextureSet.Interior_DungeonsA:
                case DFLocation.ClimateTextureSet.Interior_DungeonsB:
                case DFLocation.ClimateTextureSet.Interior_DungeonsC:
                case DFLocation.ClimateTextureSet.Interior_DungeonsNEWCs:
                case DFLocation.ClimateTextureSet.Interior_FarmInt:
                case DFLocation.ClimateTextureSet.Interior_MagesGuildInt:
                case DFLocation.ClimateTextureSet.Interior_ManorInt:
                case DFLocation.ClimateTextureSet.Interior_MarbleFloors:
                case DFLocation.ClimateTextureSet.Interior_MerchantHomesInt:
                case DFLocation.ClimateTextureSet.Interior_Mines:
                case DFLocation.ClimateTextureSet.Interior_Paintings:
                case DFLocation.ClimateTextureSet.Interior_PalaceInt:
                case DFLocation.ClimateTextureSet.Interior_Sewer:
                case DFLocation.ClimateTextureSet.Interior_TavernInt:
                case DFLocation.ClimateTextureSet.Interior_TempleInt:
                case DFLocation.ClimateTextureSet.Interior_VillageInt:
                    break;
                //
                // All other results
                //
                default:
                    return DFLocation.ClimateTextureSet.None;
            }

            // Confirmed valid set
            return set;
        }

        #endregion

        #region Ground Textures

        /// <summary>
        /// Builds ground plane texture for a single block.
        /// </summary>
        /// <param name="block">DFBlock.</param>
        /// <param name="groundArchive">Ground texture archive index.</param>
        public Texture2D CreateBlockGroundTexture(ref DFBlock block, int groundArchive)
        {
            // Create texture
            RenderTarget2D texture = new RenderTarget2D(
                graphicsDevice,
                groundTextureWidth,
                groundTextureHeight,
                true,
                SurfaceFormat.Color,
                DepthFormat.Depth16);

            // Begin rendering
            graphicsDevice.SetRenderTarget(texture);
            spriteBatch.Begin();

            // Draw tiles
            const int tileCount = 16;
            for (int x = 0; x < tileCount; x++)
            {
                for (int y = tileCount - 1; y >= 0; y--)
                {
                    // Get tile texture
                    DFBlock.RmbGroundTiles tile =
                        block.RmbBlock.FldHeader.GroundData.GroundTiles[x, y];
                    int textureKey = LoadTexture(
                        groundArchive,
                        (tile.TextureRecord < 56) ? tile.TextureRecord : 2,
                        TextureCreateFlags.None);
                    Texture2D tileTexture = GetTexture(textureKey);

                    // Set desination rectangle
                    Rectangle destinationRectangle = new Rectangle(
                        x * tileSide,
                        y * tileSide,
                        tileSide,
                        tileSide);

                    // Set sprite effects
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    if (tile.IsFlipped)
                    {
                        spriteEffects =
                            SpriteEffects.FlipHorizontally |
                            SpriteEffects.FlipVertically;
                    }

                    // Set rotation
                    Vector2 origin = Vector2.Zero;
                    float rotation = 0f;
                    if (tile.IsRotated)
                    {
                        rotation = MathHelper.ToRadians(-90f);
                        origin.X = 64f;
                        origin.Y = 0f;
                    }

                    // Render texture to target
                    spriteBatch.Draw(
                        tileTexture,
                        destinationRectangle,
                        null,
                        Color.White,
                        rotation,
                        origin,
                        spriteEffects,
                        0);
                }
            }

            // End rendering
            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);

            // TEST: Save texture for review
            //string filename = string.Format("D:\\Test\\{0}.png", block.Name);
            //FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
            //texture.SaveAsPng(fs, texture.Width, texture.Height);
            //fs.Close();

            return texture;
        }

        #endregion

    }

}
