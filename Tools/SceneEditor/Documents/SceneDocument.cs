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
using System.IO;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.World;
using SceneEditor.Proxies;
#endregion

namespace SceneEditor.Documents
{
    
    /// <summary>
    /// Wraps a scene into a document for the editor.
    ///  Supports unlimited undo/redo for non-indexed properties.
    /// </summary>
    internal class SceneDocument
    {

        #region Fields

        DeepCore core;
        Scene editorScene;
        bool isSaved = false;
        bool lockUndoRedo = false;

        Stack<UndoInfo> undoStack;
        Stack<UndoInfo> redoStack;

        public event EventHandler OnPushUndo;

        #endregion

        #region Structures

        /// <summary>
        /// Defines a property for undo/redo operations.
        /// </summary>
        private struct UndoInfo
        {
            public BaseEditorProxy Proxy;
            public PropertyInfo PropertyInfo;
            public object Value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets flag stating if scene has been saved since last modification.
        /// </summary>
        [Browsable(false)]
        public bool IsSaved
        {
            get { return isSaved; }
        }

        /// <summary>
        /// Gets editor scene.
        /// </summary>
        [Browsable(false)]
        public Scene EditorScene
        {
            get { return editorScene; }
        }

        /// <summary>
        /// Flag to prevent changes from being pushed to the undo and redo stacks.
        /// </summary>
        public bool LockUndoRedo
        {
            get { return lockUndoRedo; }
            set { lockUndoRedo = value; }
        }

        /// <summary>
        /// Gets number of undo operations on stack.
        /// </summary>
        [Browsable(false)]
        public int UndoCount
        {
            get { return undoStack.Count; }
        }

        /// <summary>
        /// Gets number of redo operations on stack.
        /// </summary>
        [Browsable(false)]
        public int RedoCount
        {
            get { return redoStack.Count; }
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

            // Create large initial undo/redo stacks
            undoStack = new Stack<UndoInfo>(1000);
            redoStack = new Stack<UndoInfo>(1000);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Pushes current property information onto the undo stack.
        /// </summary>
        /// <param name="propertyInfo">Property information.</param>
        public void PushUndo(BaseEditorProxy proxy, PropertyInfo propertyInfo)
        {
            // Do nothing if locked
            if (lockUndoRedo)
                return;

            // Set undo information
            UndoInfo undoInfo = new UndoInfo
            {
                Proxy = proxy,
                PropertyInfo = propertyInfo,
                Value = propertyInfo.GetValue(proxy, null),
            };

            // Push onto undo stack
            undoStack.Push(undoInfo);

            // Raise event
            if (OnPushUndo != null)
                OnPushUndo(this, null);
        }

        /// <summary>
        /// Pops property off undo stack and restores value.
        ///  Property is then pushed onto redo stack.
        /// </summary>
        public void PopUndo()
        {
            // Do nothing if locked
            if (lockUndoRedo)
                return;

            // Pop
            if (undoStack.Count > 0)
            {
                // Pop from undo stack
                UndoInfo undoInfo = undoStack.Pop();

                // Gets current live value
                object liveValue = undoInfo.PropertyInfo.GetValue(undoInfo.Proxy, null);

                // Restore previous property value
                lockUndoRedo = true;
                undoInfo.PropertyInfo.SetValue(undoInfo.Proxy, undoInfo.Value, null);
                lockUndoRedo = false;

                // Update to last live value
                undoInfo.Value = liveValue;

                // Push onto redo stack
                redoStack.Push(undoInfo);
            }
        }

        /// <summary>
        /// Pops property off redo stack and restores value.
        ///  Property is then pushed onto undo stack.
        /// </summary>
        public void PopRedo()
        {
            // Do nothing if locked
            if (lockUndoRedo)
                return;

            // Pop
            if (redoStack.Count > 0)
            {
                // Pop from redo stack
                UndoInfo undoInfo = redoStack.Pop();

                // Gets current live value
                object liveValue = undoInfo.PropertyInfo.GetValue(undoInfo.Proxy, null);

                // Restore previous property value
                lockUndoRedo = true;
                undoInfo.PropertyInfo.SetValue(undoInfo.Proxy, undoInfo.Value, null);
                lockUndoRedo = false;

                // Update to last live value
                undoInfo.Value = liveValue;

                // Push onto undo stack
                undoStack.Push(undoInfo);
            }
        }

        #endregion

    }

}
