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
using DeepEngine.World;
using DeepEngine.Primitives;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// A drawable geometric primitive.
    /// </summary>
    public class GeometricPrimitiveComponent : DrawableComponent
    {

        #region Fields

        // Primitive
        GeometricPrimitive primitive;
        GeometricPrimitiveType type;
        Color color = Color.White;

        // Effect
        Effect renderPrimitiveEffect;

        #endregion

        #region Structures

        /// <summary>
        /// Defines supported geometry primitives.
        /// </summary>
        public enum GeometricPrimitiveType
        {
            Cube,
            Cylinder,
            Sphere,
            Teapot,
            Torus,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets geometry primitive type.
        /// </summary>
        public GeometricPrimitiveType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets or sets the diffuse colour used to render this primitive.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="entity">Entity this component is attached to.</param>
        public GeometricPrimitiveComponent(BaseEntity entity)
            :base(entity)
        {
            // Load effect
            renderPrimitiveEffect = entity.Scene.Core.ContentManager.Load<Effect>("Effects/RenderPrimitive");

            // Create cube
            MakeCube(1f);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Makes primitive a cube.
        /// </summary>
        /// <param name="size">Size of cube.</param>
        public void MakeCube(float size)
        {
            // Create primitive
            primitive = new CubePrimitive(GraphicsDevice, size);
            type = GeometricPrimitiveType.Cube;
            BoundingSphere = primitive.BoundingSphere;
        }

        /// <summary>
        /// Makes primitive a cylinder.
        /// </summary>
        /// <param name="height">Height of cylinder.</param>
        /// <param name="diameter">Diameter of cylinder.</param>
        /// <param name="tessellation">Tessellation of cylinder.</param>
        public void MakeCylinder(float height, float diameter, int tessellation)
        {
            // Create primitive
            primitive = new CylinderPrimitive(GraphicsDevice, height, diameter, tessellation);
            type = GeometricPrimitiveType.Cylinder;
            BoundingSphere = primitive.BoundingSphere;
        }

        /// <summary>
        /// Makes primitive a sphere.
        /// </summary>
        /// <param name="radius">Radius of sphere.</param>
        /// <param name="tessellation">Tessellation of sphere.</param>
        public void MakeSphere(float radius, int tessellation)
        {
            // Create primitive
            primitive = new SpherePrimitive(GraphicsDevice, radius * 2, tessellation);
            type = GeometricPrimitiveType.Cube;
            BoundingSphere = primitive.BoundingSphere;
        }

        /// <summary>
        /// Makes primitive a teaport.
        /// </summary>
        /// <param name="size">Size of teapot.</param>
        /// <param name="tessellation">Tessellation of teapot.</param>
        public void MakeTeapot(float size, int tessellation)
        {
            // Create primitive
            primitive = new TeapotPrimitive(GraphicsDevice, size, tessellation);
            type = GeometricPrimitiveType.Teapot;
            BoundingSphere = primitive.BoundingSphere;
        }

        /// <summary>
        /// Makes primitive a torus.
        /// </summary>
        /// <param name="diameter">Diameter of torus.</param>
        /// <param name="radius">Radius of torus.</param>
        /// <param name="tessellation">Tessellation of torus.</param>
        public void MakeTorus(float radius, float thickness, int tessellation)
        {
            // Create primitive
            primitive = new TorusPrimitive(GraphicsDevice, radius * 2, thickness, tessellation);
            type = GeometricPrimitiveType.Torus;
            BoundingSphere = primitive.BoundingSphere;
        }

        /// <summary>
        /// Makes primitive into specified type using default values.
        /// </summary>
        /// <param name="type">Type of primitive to use.</param>
        public void MakePrimitive(GeometricPrimitiveType type)
        {
            // Create default primitive
            switch (type)
            {
                case GeometricPrimitiveType.Cube:
                    primitive = new CubePrimitive(GraphicsDevice);
                    break;
                case GeometricPrimitiveType.Cylinder:
                    primitive = new CylinderPrimitive(GraphicsDevice);
                    break;
                case GeometricPrimitiveType.Sphere:
                    primitive = new SpherePrimitive(GraphicsDevice);
                    break;
                case GeometricPrimitiveType.Teapot:
                    primitive = new TeapotPrimitive(GraphicsDevice);
                    break;
                case GeometricPrimitiveType.Torus:
                    primitive = new TorusPrimitive(GraphicsDevice);
                    break;
                default:
                    return;
            }

            this.type = type;
            this.BoundingSphere = primitive.BoundingSphere;
        }

        #endregion

        #region DrawableComponent Overrides

        /// <summary>
        /// Draws component.
        /// </summary>
        public override void Draw()
        {
            // Update effect
            renderPrimitiveEffect.Parameters["View"].SetValue(entity.Scene.DeprecatedCamera.View);
            renderPrimitiveEffect.Parameters["Projection"].SetValue(entity.Scene.DeprecatedCamera.Projection);
            renderPrimitiveEffect.Parameters["World"].SetValue(entity.Matrix);
            renderPrimitiveEffect.Parameters["DiffuseColor"].SetValue(color.ToVector3());

            // Draw primitive
            primitive.Draw(renderPrimitiveEffect);
        }

        #endregion

    }

}
