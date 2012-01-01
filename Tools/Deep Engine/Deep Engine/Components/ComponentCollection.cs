// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// A list of components with events.
    /// </summary>
    public class ComponentCollection : IEnumerable
    {
        #region Fields

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
            // Add component
            components.Add(component);

            // Raise event
            RaiseComponentAddedEvent(component);
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
            public BaseComponent Component;
        }

        /// <summary>
        /// Raise event.
        /// </summary>
        protected virtual void RaiseComponentAddedEvent(BaseComponent component)
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
