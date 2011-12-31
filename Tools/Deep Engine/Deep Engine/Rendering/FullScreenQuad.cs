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
#endregion

namespace DeepEngine.Rendering
{

    /// <summary>
    /// Draws a full-screen quad for post-rendering effect such as combining deferred buffers.
    /// </summary>
    internal class FullScreenQuad
    {
        // Fields
        VertexBuffer vb;
        IndexBuffer ib;

        // Constructor
        public FullScreenQuad(GraphicsDevice graphicsDevice)
        {
            // Vertices
            VertexPositionTexture[] vertices =
                {
                    new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
                    new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                    new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
                    new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0))
                };

            // Create Vertex Buffer
            vb = new VertexBuffer(
                graphicsDevice,
                VertexPositionTexture.VertexDeclaration,
                vertices.Length,
                BufferUsage.None);
            vb.SetData<VertexPositionTexture>(vertices);

            // Indices
            ushort[] indices = { 0, 1, 2, 2, 3, 0 };

            // Create Index Buffer
            ib = new IndexBuffer(
                graphicsDevice,
                IndexElementSize.SixteenBits,
                indices.Length,
                BufferUsage.None);
            ib.SetData<ushort>(indices);
        }

        // Render quad
        public void Draw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.SetVertexBuffer(vb);
            graphicsDevice.Indices = ib;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }
    }

}
