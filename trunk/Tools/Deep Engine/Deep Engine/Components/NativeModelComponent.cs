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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.Daggerfall;
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// A drawable Daggerfall native model. Can be attached at runtime independently of content pipeline.
    /// </summary>
    public class NativeModelComponent : DrawableComponent
    {

        #region Fields

        // Model data
        ModelManager.ModelData modelData;

        // Effects
        Effect renderGeometryEffect;

        #endregion

        #region Properties

        /// <summary>
        /// Gets reference to loaded model.
        /// </summary>
        public ModelManager.ModelData ModelData
        {
            get { return modelData; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="id">Model ID to load.</param>
        public NativeModelComponent(DeepCore core, uint id)
            : base(core)
        {
            // Load effect
            renderGeometryEffect = core.ContentManager.Load<Effect>("Effects/RenderGeometry");

            // Load model
            LoadModel(id);
        }

        #endregion

        #region DrawableComponents Overrides

        /// <summary>
        /// Draws component.
        /// </summary>
        /// <param name="caller">Entity calling the draw operation.</param>
        public override void Draw(BaseEntity caller)
        {
            // Do nothing if disabled
            if (!enabled)
                return;

            // Calculate world matrix
            Matrix worldMatrix = caller.Matrix * matrix;

            // Setup effect
            renderGeometryEffect.Parameters["World"].SetValue(worldMatrix);
            renderGeometryEffect.Parameters["View"].SetValue(core.ActiveScene.DeprecatedCamera.View);
            renderGeometryEffect.Parameters["Projection"].SetValue(core.ActiveScene.DeprecatedCamera.Projection);

            // Set buffers
            core.GraphicsDevice.SetVertexBuffer(modelData.VertexBuffer);
            core.GraphicsDevice.Indices = modelData.IndexBuffer;

            // Render each submesh
            Texture2D diffuseTexture = null;
            foreach (var sm in modelData.SubMeshes)
            {
                // Set texture
                diffuseTexture = core.MaterialManager.GetTexture(sm.TextureKey);
                renderGeometryEffect.Parameters["Texture"].SetValue(diffuseTexture);

                // Render geometry
                foreach (EffectPass pass in renderGeometryEffect.CurrentTechnique.Passes)
                {
                    // Apply effect pass
                    pass.Apply();

                    // Draw indexed primitives
                    core.GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        modelData.VertexBuffer.VertexCount,
                        sm.StartIndex,
                        sm.PrimitiveCount);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads a new model to display with this component.
        ///  Replaces any previously loaded model.
        /// </summary>
        /// <param name="id">Model ID to load.</param>
        /// <returns>True if successful.</returns>
        public bool LoadModel(uint id)
        {
            try
            {
                // Load model and textures
                modelData = core.ModelManager.GetModelData(id);
                for (int i = 0; i < modelData.SubMeshes.Length; i++)
                {
                    // Set flags
                    MaterialManager.TextureCreateFlags flags =
                        MaterialManager.TextureCreateFlags.ApplyClimate |
                        MaterialManager.TextureCreateFlags.MipMaps |
                        MaterialManager.TextureCreateFlags.PowerOfTwo;

                    // Set extended alpha flags
                    flags |= MaterialManager.TextureCreateFlags.ExtendedAlpha;

                    // Load texture
                    modelData.SubMeshes[i].TextureKey = core.MaterialManager.LoadTexture(
                        modelData.DFMesh.SubMeshes[i].TextureArchive,
                        modelData.DFMesh.SubMeshes[i].TextureRecord,
                        flags);
                }

                // Set component bounding sphere
                this.BoundingSphere = modelData.BoundingSphere;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                modelData = null;
                return false;
            }

            return true;
        }

        #endregion

    }

}
