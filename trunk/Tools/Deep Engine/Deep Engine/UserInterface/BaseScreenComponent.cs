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
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.World;
#endregion

namespace DeepEngine.UserInterface
{

    /// <summary>
    /// Provides features common to all screen components.
    /// </summary>
    public abstract class BaseScreenComponent :
        IDisposable
    {

        #region Fields

        protected DeepCore core;
        protected bool enabled;
        protected object tag;
        protected PanelScreenComponent parent;
        protected Vector2 position;
        protected Vector2 size;

        private Rectangle rectangle;

        #endregion

        #region Properties

        /// <summary>
        /// Gets engine core.
        /// </summary>
        public DeepCore Core
        {
            get { return core; }
        }

        /// <summary>
        /// Gets or sets enabled flag.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets or sets custom tag.
        /// </summary>
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        /// <summary>
        /// Gets or sets position on screen.
        /// </summary>
        public virtual Vector2 Position
        {
            get { return position; }
            set { position = value;}
        }

        /// <summary>
        /// Gets or sets size of component.
        /// </summary>
        public virtual Vector2 Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        /// Gets parent panel.
        /// </summary>
        public PanelScreenComponent Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        /// <summary>
        /// Gets screen area occupied by component.
        /// </summary>
        public Rectangle Rectangle
        {
            get
            {
                rectangle.X = (int)position.X;
                rectangle.Y = (int)position.Y;
                rectangle.Width = (int)size.X;
                rectangle.Height = (int)size.Y;
                return rectangle;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public BaseScreenComponent(DeepCore core, Vector2 position, Vector2 size)
        {
            // Store values
            this.core = core;
            this.enabled = true;
            this.tag = null;
            this.position = position;
            this.size = size;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Called when screen component should update itself.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update.</param>
        public abstract void Update(TimeSpan elapsedTime);

        /// <summary>
        /// Called when screen component should draw itself.
        ///  Must be called between SpriteBatch Begin() & End() methods.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw with.</param>
        internal abstract void Draw(SpriteBatch spriteBatch);

        #endregion

        #region IDisposable

        /// <summary>
        /// Called when component is to be disposed.
        ///  Override if special handling needed
        ///  to dispose of component resources.
        /// </summary>
        public virtual void Dispose()
        {
        }

        #endregion

    }

}
