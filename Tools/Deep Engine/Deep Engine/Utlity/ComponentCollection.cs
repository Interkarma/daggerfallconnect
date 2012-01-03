// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Components;
#endregion

namespace DeepEngine.Utility
{

    /// <summary>
    /// A list of components with events.
    /// </summary>
    public class ComponentCollection : IEnumerable
    {
        #region Fields

        // Constant strings
        const string componentIsStaticError = "Cannot add or share a component that is already marked as static.";

        // Component list
        List<BaseComponent> components;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of components contained in collection.
        /// </summary>
        public int Count
        {
            get { return components.Count; }
        }

        /// <summary>
        /// Gets component at index.
        /// </summary>
        /// <param name="index">Index of component.</param>
        /// <returns>Reference to component.</returns>
        public BaseComponent this[int index]
        {
            get { return components[index]; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ComponentCollection()
        {
            components = new List<BaseComponent>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a component to collection.
        /// </summary>
        /// <param name="component">Component to add.</param>
        public void Add(BaseComponent component)
        {
            // Cannot add a static component
            if (component.IsStatic)
                throw new Exception(componentIsStaticError);

            // Add component
            components.Add(component);

            // Raise event
            RaiseComponentAddedEvent(component, false);
        }

        /// <summary>
        /// Adds a static component to collection.
        ///  A static component cannot move independently but allows parent
        ///  entity to make optimisation decisions, such as combining
        ///  static geometry to improve draw performance.
        ///  Always set component transform before adding static, as you cannot
        ///  change it afterwards.
        /// </summary>
        /// <param name="component">Component to add as static.</param>
        public void AddStatic(BaseComponent component)
        {
            // Cannot add a static component
            if (component.IsStatic)
                throw new Exception(componentIsStaticError);

            // Add component
            components.Add(component);

            // Flag as static
            component.MakeStatic();

            // Raise event
            RaiseComponentAddedEvent(component, true);
        }

        #endregion

        #region IEnumerable

        /// <summary>
        /// Gets IEnumerator for the component collection.
        /// </summary>
        /// <returns>IEnumerator object.</returns>
        public IEnumerator GetEnumerator()
        {
            return (components as IEnumerable).GetEnumerator();
        }

        #endregion

        #region ComponentAdded Event

        public delegate void ComponentAddedEventHandler(object sender, ComponentAddedEventArgs e);
        public event ComponentAddedEventHandler ComponentAdded;

        /// <summary>
        /// Event arguments.
        /// </summary>
        public class ComponentAddedEventArgs : EventArgs
        {
            public object Component;
            public bool IsStatic;
        }

        /// <summary>
        /// Raise event.
        /// </summary>
        protected virtual void RaiseComponentAddedEvent(BaseComponent component, bool isStatic)
        {
            // Raise event
            if (null != ComponentAdded)
            {
                // Popuate event arguments
                ComponentAddedEventArgs e = new ComponentAddedEventArgs()
                {
                    Component = component,
                    IsStatic = isStatic,
                };

                // Raise event
                ComponentAdded(this, e);
            }
        }

        #endregion
    }

}
