// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.World;
#endregion

namespace SceneEditor.Documents
{
    
    /// <summary>
    /// Wraps a scene into a document for the editor.
    /// </summary>
    class SceneDocument
    {

        #region Fields

        DeepCore core;
        Scene editorScene;
        Scene gameScene;
        System.Drawing.Color clearColor = System.Drawing.Color.CornflowerBlue;

        bool gameMode = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets flag to put scene document into Game Mode.
        ///  While in Game Mode, scene objects receive updates normally.
        ///  Everything remains at starting positions when returning to Editor Mode.
        /// </summary>
        [Browsable(false)]
        public bool GameMode
        {
            get { return gameMode; }
            set { gameMode = value; }
        }

        /// <summary>
        /// Gets or sets clear colour.
        /// </summary>
        [Description("Colour to clear scene in draw operations.")]
        public System.Drawing.Color ClearColor
        {
            get { return clearColor; }
            set
            {
                clearColor = value;
                core.Renderer.ClearColor = new Color(clearColor.R, clearColor.G, clearColor.B);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public SceneDocument(DeepCore core)
        {
            // Save references
            this.core = core;

            // Create scene objects
            editorScene = new Scene(core);
            gameScene = new Scene(core);

            // Set clear colour on engine
            ClearColor = clearColor;
        }

        #endregion

    }

}
