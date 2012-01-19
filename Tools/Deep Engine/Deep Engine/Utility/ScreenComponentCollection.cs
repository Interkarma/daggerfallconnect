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
using DeepEngine.UserInterface;
#endregion

namespace DeepEngine.Utility
{

    /// <summary>
    /// A list of screen components with events.
    /// </summary>
    public class ScreenComponentCollection : IEnumerable
    {
        #region Fields

        PanelScreenComponent panel;
        List<BaseScreenComponent> components;

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
        public BaseScreenComponent this[int index]
        {
            get { return components[index]; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="panel">Panel hosting this collection</param>
        public ScreenComponentCollection(PanelScreenComponent panel)
        {
            this.panel = panel;
            components = new List<BaseScreenComponent>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a component to collection.
        /// </summary>
        /// <param name="component">Component to add.</param>
        public void Add(BaseScreenComponent component)
        {
            // Add component
            components.Add(component);
            component.Parent = panel;

            // Raise event
            RaiseComponentAddedEvent(component, false);
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            components.Clear();
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

        /// <summary>
        /// This event is fired whenever a component is added, allowing the entity to overlay
        ///  any special handling required.
        /// </summary>
        public event ComponentAddedEventHandler ComponentAdded;
        public delegate void ComponentAddedEventHandler(object sender, ComponentAddedEventArgs e);

        /// <summary>
        /// Event arguments.
        /// </summary>
        public class ComponentAddedEventArgs : EventArgs
        {
            public object Component;
        }

        /// <summary>
        /// Raise event.
        /// </summary>
        protected virtual void RaiseComponentAddedEvent(BaseScreenComponent component, bool isStatic)
        {
            // Raise event
            if (null != ComponentAdded)
            {
                // Popuate event arguments
                ComponentAddedEventArgs e = new ComponentAddedEventArgs()
                {
                    Component = component,
                };

                // Raise event
                ComponentAdded(this, e);
            }
        }

        #endregion
    }

}
