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
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Extends basic renderer with deferred shading.
    /// </summary>
    public class DeferredRenderer : Renderer
    {

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="textureManager"></param>
        public DeferredRenderer(TextureManager textureManager)
            : base(textureManager)
        {
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Render active scene.
        /// </summary>
        public new void Draw()
        {
            // Clear batches from previous frame
            ClearBatches();

            // Batch visible elements
            BatchNode(scene.Root, true);

            // Draw background
            DrawBackground();

            // Draw visible geometry
            DrawBatches();

            // Draw billboard batches
            if (HasOptionsFlags(RendererOptions.Flats))
            {
                billboardManager.Draw(camera);
            }
        }

        #endregion

    }

}
