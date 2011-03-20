// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/using System;

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

#endregion

namespace XNALibrary
{
    /// <summary>
    /// Helper class to load and store Daggerfall textures for XNA.
    ///  All textures are grouped into a texture atlas to reduce state changes.
    ///  Certain texture types will be grouped together. For example, region terrain tiles all go in a single atlas.
    /// </summary>
    public class TextureManager
    {
        #region Class Variables

        private GraphicsDevice graphicsDevice;
        private ImageFileReader imageFileReader;
        //private Dictionary<int, Texture> textureDictionary;

        // Atlases for reserved texture groups
        private Texture2D terrainTilesAtlas;
        private Texture2D regionalTexturesAtlas;

        #endregion

        #region Public Properties

        /// <summary>
        /// ImageFileReader.
        /// </summary>
        public ImageFileReader ImageFileReader
        {
            get { return imageFileReader; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="device">Graphics Device.</param>
        /// <param name="arena2Folder">Path to Arena2 folder.</param>
        public TextureManager(GraphicsDevice device, string arena2Folder)
        {
            graphicsDevice = device;
            imageFileReader = new ImageFileReader(arena2Folder);
            //textureDictionary = new Dictionary<int, Texture2D>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get unique key for specified texture.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="frame">Frame index.</param>
        /// <returns>Texture key.</returns>
        //public int GetTextureKey(int archive, int record, int frame)
        //{
        //    return (archive * 10000) + (record * 100) + frame;
        //}

        /// <summary>
        /// Loads texture based on index.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="frame">Frame index.</param>
        /// <returns>Texture key.</returns>
        //public int LoadTexture(int archive, int record, int frame)
        //{
        //    // Just return key if already in dictionary
        //    int textureKey = GetTextureKey(archive, record, frame);
        //    if (textureDictionary.ContainsKey(textureKey))
        //        return textureKey;

        //    // Get DF texture in ARGB format so we can just SetData the byte array into XNA
        //    DFImageFile textureFile = imageFileReader.LoadFile(TextureFile.IndexToFileName(archive));
        //    DFBitmap dfbitmap = textureFile.GetBitmapFormat(record, frame, 0, DFBitmap.Formats.ARGB);

        //    // Create XNA texture
        //    Texture2D texture = new Texture2D(graphicsDevice, dfbitmap.Width, dfbitmap.Height, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
        //    texture.SetData<byte>(dfbitmap.Data);

        //    // Store texture in dictionary
        //    textureDictionary.Add(textureKey, texture);

        //    return textureKey;
        //}

        /// <summary>
        /// Get texture based on key. The manager will return NULL if texture does not exist.
        /// </summary>
        /// <param name="textureKey"></param>
        /// <returns></returns>
        //public Texture2D GetTexture(int textureKey)
        //{
        //    if (!textureDictionary.ContainsKey(textureKey))
        //        return null;
        //    else
        //        return textureDictionary[textureKey];
        //}

        #endregion
    }
}
