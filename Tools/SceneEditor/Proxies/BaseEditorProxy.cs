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
using System.Windows.Forms;
using System.ComponentModel;
using SceneEditor.Documents;
#endregion

namespace SceneEditor.Proxies
{

    /// <summary>
    /// Base proxy interface.
    /// </summary>
    internal interface IBaseEditorProxy : IEditorProxy { }

    /// <summary>
    /// Defines base requirements of editor proxies.
    /// </summary>
    public abstract class BaseEditorProxy : IBaseEditorProxy
    {

        #region Fields

        const string categoryName = "Base";

        protected string name;
        protected TreeNode treeNode;
        protected SceneDocument document;

        #endregion

        #region Properties

        /// <summary>
        /// Gets scene document this proxy belongs to.
        /// </summary>
        [Browsable(false)]
        public SceneDocument SceneDocument
        {
            get { return document; }
        }

        /// <summary>
        /// Gets or sets TreeNode linked to this proxy.
        /// </summary>
        [Browsable(false)]
        public TreeNode TreeNode
        {
            get { return treeNode; }
            set { treeNode = value; }
        }

        #endregion

        #region Editor Properties

        /// <summary>
        /// Gets or sets proxy name.
        /// </summary>
        [Category(categoryName), Description("Name of object.")]
        public string Name
        {
            get { return name; }
            set { SetName(value, true); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        public BaseEditorProxy(SceneDocument document)
        {
            // Save references
            this.document = document;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set name of object.
        /// </summary>
        /// <param name="name">New name.</param>
        /// <param name="pushUndo">True to push change onto undo stack.</param>
        public void SetName(string name, bool pushUndo)
        {
            // Push undo
            if (pushUndo)
                SceneDocument.PushUndo(this, this.GetType().GetProperty("Name"));

            // Set name
            this.name = name;
            if (treeNode != null)
                treeNode.Text = name;
        }

        #endregion

    }

}
