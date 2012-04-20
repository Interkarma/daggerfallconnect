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
using DeepEngine.Components;
using DeepEngine.World;
using SceneEditor.Proxies;
using SceneEditor.UserControls;
#endregion

namespace SceneEditor.Documents
{
    
    /// <summary>
    /// Wraps a scene into a document for the editor.
    /// </summary>
    public class SceneDocument
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
        /// Defines supported undo actions.
        /// </summary>
        private enum UndoTypes
        {
            None,
            Property,
            TerrainDeformation,
            TerrainPaint,
        }

        /// <summary>
        /// Defines a property for undo/redo operations.
        /// </summary>
        private struct UndoInfo
        {
            public UndoTypes Type;
            public BaseEditorProxy Proxy;
            public PropertyInfo PropertyInfo;
            public TerrainDeformUndoInfo TerrainDeformInfo;
            public object Value;
        }

        /// <summary>
        /// Defines undo for terrain deformation undo/redo operations.
        /// </summary>
        public struct TerrainDeformUndoInfo
        {
            /// <summary>Terrain editor to process undo.</summary>
            public TerrainEditor TerrainEditor;
            /// <summary>The terrain component to receive undo.</summary>
            public QuadTerrainComponent TerrainComponent;
            /// <summary>The height map to receive undo.</summary>
            public float[] HeightMap;
            /// <summary>Top-Left corner of undo.</summary>
            public Point Position;
            /// <summary>Dimension of undo area from top-left corner.</summary>
            public int Dimension;
            /// <summary>The buffer containing undo information.</summary>
            public float[] UndoBuffer;
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

            // Set camera view distance
            editorScene.Camera.FarPlane = 5000f;

            // Create large initial undo/redo stacks
            undoStack = new Stack<UndoInfo>(100);
            redoStack = new Stack<UndoInfo>(100);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Pushes property undo information onto the undo stack.
        /// </summary>
        /// <param name="propertyInfo">Property information.</param>
        public void PushUndo(BaseEditorProxy proxy, PropertyInfo propertyInfo)
        {
            // Do nothing if locked
            if (lockUndoRedo)
                return;

            // Making a change invalidates redo.
            // This is to prevent inconsistent redo operations.
            if (redoStack.Count > 0)
                redoStack.Clear();

            // Set undo information
            UndoInfo undoInfo = new UndoInfo
            {
                Type = UndoTypes.Property,
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
        /// Pushes terrain deformation undo information onto the undo stack.
        /// </summary>
        /// <param name="info">Terrain undo information.</param>
        public void PushUndo(TerrainDeformUndoInfo info)
        {
            // Do nothing if locked
            if (lockUndoRedo)
                return;

            // Making a change invalidates redo.
            // This is to prevent inconsistent redo operations.
            if (redoStack.Count > 0)
                redoStack.Clear();

            // Set undo information
            UndoInfo undoInfo = new UndoInfo
            {
                Type = UndoTypes.TerrainDeformation,
                TerrainDeformInfo = info,
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
            // Do nothing if locked or nothing to pop
            if (lockUndoRedo || undoStack.Count == 0)
                return;

            // Pop from undo stack
            UndoInfo undoInfo = undoStack.Pop();

            // Pop based on type
            if (undoInfo.Type == UndoTypes.Property)
                UndoProperty(undoInfo, redoStack);
            else if (undoInfo.Type == UndoTypes.TerrainDeformation)
                UndoDeformation(undoInfo, redoStack);
            else if (undoInfo.Type == UndoTypes.TerrainPaint)
            {
            }
        }

        /// <summary>
        /// Pops property off redo stack and restores value.
        ///  Property is then pushed onto undo stack.
        /// </summary>
        public void PopRedo()
        {
            // Do nothing if locked or nothing to pop
            if (lockUndoRedo || redoStack.Count == 0)
                return;

            // Pop from redo stack
            UndoInfo undoInfo = redoStack.Pop();

            // Pop based on type
            if (undoInfo.Type == UndoTypes.Property)
                UndoProperty(undoInfo, undoStack);
            else if (undoInfo.Type == UndoTypes.TerrainDeformation)
                UndoDeformation(undoInfo, undoStack);
            else if (undoInfo.Type == UndoTypes.TerrainPaint)
            {
            }
        }

        #endregion

        #region Pop Methods

        /// <summary>
        /// Undo a property and push onto another stack.
        /// </summary>
        /// <param name="undoInfo">Undo info.</param>
        private void UndoProperty(UndoInfo undoInfo, Stack<UndoInfo> pushTo)
        {
            // Gets current property value
            object liveValue = undoInfo.PropertyInfo.GetValue(undoInfo.Proxy, null);

            // Restore previous property value
            lockUndoRedo = true;
            undoInfo.PropertyInfo.SetValue(undoInfo.Proxy, undoInfo.Value, null);
            lockUndoRedo = false;

            // Set undo to last property value
            undoInfo.Value = liveValue;

            // Push onto redo stack
            pushTo.Push(undoInfo);
        }

        /// <summary>
        /// Undo a deformation and push onto another stack.
        /// </summary>
        /// <param name="undoInfo">Undo info.</param>
        private void UndoDeformation(UndoInfo undoInfo, Stack<UndoInfo> pushTo)
        {
            // Get terrain editor reference
            TerrainEditor terrainEditor = undoInfo.TerrainDeformInfo.TerrainEditor;

            // Get current terrain deformation
            TerrainDeformUndoInfo? liveData = terrainEditor.GetHeightMapUndo(
                undoInfo.TerrainDeformInfo.TerrainComponent,
                undoInfo.TerrainDeformInfo.Position.X,
                undoInfo.TerrainDeformInfo.Position.Y,
                undoInfo.TerrainDeformInfo.Dimension);

            // Perform undo operation
            terrainEditor.RestoreHeightMapUndo(undoInfo.TerrainDeformInfo);

            // Push last value on redo stack if valid
            if (liveData != null)
            {
                // Push onto stack
                undoInfo.TerrainDeformInfo = liveData.Value;
                pushTo.Push(undoInfo);
            }
        }

        #endregion

    }

}
