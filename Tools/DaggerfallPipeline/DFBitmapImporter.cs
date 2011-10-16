// Project:         DaggerfallPipeline
// Description:     Load Daggerfall content using the XNA Content Pipeline
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace DaggerfallPipeline
{
    [ContentImporter(".dfb", DisplayName = "Daggerfall - Bitmap Importer", DefaultProcessor = "TextureProcessor")]
    public class DFBitmapImporter : ContentImporter<Texture2DContent>
    {
        #region Variables

        private const string Arena2PathTxt = "Arena2Path.txt";
        private string arena2Path;

        #endregion

        public override Texture2DContent Import(string filename, ContentImporterContext context)
        {
            // Load Arena2Path.txt
            arena2Path = File.ReadAllText(
                Path.Combine(Path.GetDirectoryName(filename), Arena2PathTxt));

            // Read input text
            string input = File.ReadAllText(filename);

            // Remove new lines
            input = input.Replace('\n', ' ').Trim();
            input = input.Replace('\r', ' ').Trim();

            // Get source information
            string[] lines = input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string textureFilename = lines[0].Trim();
            int record = Convert.ToInt32(lines[1].Trim());
            int frame = Convert.ToInt32(lines[2].Trim());

            // Get bitmap in RGBA format
            ImageFileReader fileReader = new ImageFileReader(arena2Path);
            DFImageFile imageFile = fileReader.LoadFile(textureFilename);
            DFBitmap dfBitmap = imageFile.GetBitmapFormat(record, frame, 0, DFBitmap.Formats.RGBA);

            // Set bitmap data
            BitmapContent bitmapContent = new PixelBitmapContent<Color>(dfBitmap.Width, dfBitmap.Height);
            bitmapContent.SetPixelData(dfBitmap.Data);
            Texture2DContent tc = new Texture2DContent();
            tc.Faces[0] = bitmapContent;

            return tc;
        }
    }
}
