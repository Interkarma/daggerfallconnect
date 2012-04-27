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
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.Components;
using DeepEngine.World;
using DeepEngine.Utility;
using ProjectEditor.Documents;
#endregion

namespace ProjectEditor.Proxies
{

    /// <summary>
    /// Light proxy interface.
    /// </summary>
    internal interface ILightProxy : IEditorProxy { }

    /// <summary>
    /// Encapsulate a versatile light component.
    /// </summary>
    internal sealed class LightProxy : BaseComponentProxy, IBaseComponentProxy
    {

        #region Fields

        const string defaultName = "Light";
        const string commonCategoryName = "Common";
        const string pointCategoryName = "Point";
        const string directionalCategoryName = "Directional";

        LightComponent light;
        LightProxyTypes lightType;

        #endregion

        #region Custom Types

        /// <summary>
        /// Defines light types supported by the proxy.
        /// </summary>
        public enum LightProxyTypes
        {
            Ambient,
            Directional,
            Point,
        }

        #endregion

        #region Editor Properties

        /// <summary>
        /// Gets or sets light type.
        /// </summary>
        [Category(commonCategoryName), Description("Light type.")]
        public LightProxyTypes LightType
        {
            get { return lightType; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("LightType"));
                lightType = value;
                UpdateLight();
            }
        }

        /// <summary>
        /// Gets or sets light colour.
        /// </summary>
        [Category(commonCategoryName), Description("Colour of light.")]
        public System.Drawing.Color Color
        {
            get { return ColorHelper.FromXNA(light.Color); }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Color"));
                light.Color = ColorHelper.FromWinForms(value);
            }
        }

        /// <summary>
        /// Gets or sets light intensity.
        /// </summary>
        [Category(commonCategoryName), Description("Intensity of light.")]
        public float Intensity
        {
            get { return light.Intensity; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Intensity"));
                light.Intensity = value;
            }
        }

        /// <summary>
        /// Gets or sets directional light vector.
        /// </summary>
        [Category(directionalCategoryName), Description("Vector of directional light.")]
        public Vector3 Direction
        {
            get { return light.Direction; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Direction"));
                light.Direction = value;
            }
        }

        /// <summary>
        /// Gets or sets position of light.
        /// </summary>
        [Category(pointCategoryName), Description("Position of point light.")]
        public new Vector3 Position
        {
            get { return light.Position; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Position"));
                light.Position = value;
            }
        }

        /// <summary>
        /// Hide base rotation.
        /// </summary>
        [Browsable(false)]
        public new Vector3 Rotation
        {
            get { return base.Rotation; }
            set { base.Rotation = value; }
        }

        /// <summary>
        /// Hide base scale.
        /// </summary>
        [Browsable(false)]
        public new Vector3 Scale
        {
            get { return base.Scale; }
            set { base.Scale = value; }
        }

        /// <summary>
        /// Gets or sets radius of light.
        /// </summary>
        [Category(pointCategoryName), Description("Radius of point light.")]
        public float Radius
        {
            get { return light.PointRadius; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Radius"));
                light.PointRadius = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        /// <param name="entity">Parent entity</param>
        public LightProxy(SceneDocument document, EntityProxy entity)
            : base(document, entity)
        {
            base.Name = defaultName;
            this.light = new LightComponent(document.EditorScene.Core);
            this.light.PointRadius = 8f;
            this.light.Direction = Vector3.Down;
            this.lightType = LightProxyTypes.Ambient;
            UpdateLight();

            base.Entity.Components.Add(light);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates light with current settings.
        /// </summary>
        private void UpdateLight()
        {
            switch (lightType)
            {
                case LightProxyTypes.Ambient:
                    light.MakeAmbient(light.Color, light.Intensity);
                    break;
                case LightProxyTypes.Directional:
                    light.MakeDirectional(light.Direction, light.Color, light.Intensity);
                    break;
                case LightProxyTypes.Point:
                    light.MakePoint(light.Position, light.PointRadius, light.Color, light.Intensity);
                    break;
            }
        }

        #endregion

        #region BaseEditorProxy Overrides

        /// <summary>
        /// Removes this proxy from editor.
        /// </summary>
        public override void Remove()
        {
            // Remove from entity
            if (entity != null)
            {
                entity.Components.Remove(light);
            }
        }

        #endregion

    }

}
