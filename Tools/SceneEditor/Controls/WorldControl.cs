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
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.Components;
using DeepEngine.World;
#endregion

namespace SceneEditor.Controls
{

    /// <summary>
    /// Control wrapper for Deep Engine.
    /// </summary>
    class WorldControl : GraphicsDeviceControl
    {

        #region Fields

        DeepCore core;

        string arena2Path = string.Empty;
        bool isReady = false;

        Color clearColor = Color.CornflowerBlue;

        Stopwatch stopwatch = Stopwatch.StartNew();
        TimeSpan startTime;
        TimeSpan lastTime;

        public event EventHandler InitializeCompleted;
        public event EventHandler OnTick;

        #endregion

        #region Properties

        /// <summary>
        /// Gets engine core.
        /// </summary>
        public DeepCore Core
        {
            get { return core; }
        }

        #endregion

        #region GraphicsDeviceService Overrides

        /// <summary>
        /// Initialise the control.
        /// </summary>
        protected override void Initialize()
        {
            // Read Arena2Path from settings
            arena2Path = Properties.Settings.Default.Arena2Path;

            // Get start times
            startTime = stopwatch.Elapsed;
            lastTime = stopwatch.Elapsed;
        }

        /// <summary>
        /// Draw control.
        /// </summary>
        protected override void Draw()
        {
            // Do nothing if not visible or status other than normal
            if (!Visible ||
                GraphicsDevice.GraphicsDeviceStatus != GraphicsDeviceStatus.Normal)
                return;

            // Handle control not ready
            if (!isReady)
            {
                // Keep trying to initialise view
                if (!InitialiseView())
                {
                    // Just clear the display until ready
                    GraphicsDevice.Clear(clearColor);
                    return;
                }
            }

            // Clear device
            core.Draw(true);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Attempts to initialise view based on Arena2Path
        /// </summary>
        /// <returns></returns>
        private bool InitialiseView()
        {
            // Exit if not created or Arena2 is not set
            if (!this.Created || string.IsNullOrEmpty(arena2Path))
                return false;

            // Create engine core
            core = new DeepCore(arena2Path, base.Services);
            core.Initialize();

            // Set options
            core.Renderer.ShowDebugBuffers = false;
            core.Renderer.FXAAEnabled = false;
            core.Renderer.BloomEnabled = false;

            // Hook idle event to run as fast as possible
            Application.Idle += TickWhileIdle;

            // Set ready flag
            isReady = true;

            // Raise event
            InitializeCompleted(this, null);

            return true;
        }

        /// <summary>
        /// Tick to update and draw in variable timestep.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void Tick(object sender, EventArgs e)
        {
            // Measure elapsed time
            TimeSpan currentTime = stopwatch.Elapsed;
            TimeSpan elapsedTime = currentTime - lastTime;

            // Update core
            core.Update(elapsedTime);

            // Apply input to camera when control has focus
            if (this.Focused)
            {
                core.Input.Apply(core.ActiveScene.Camera, true);
                core.ActiveScene.Camera.Update();
            }
            else
            {
                core.Input.Reset();
            }

            // Call tick event
            if (OnTick != null)
                OnTick(this, null);

            // Store update time
            lastTime = currentTime;

            // Redraw
            CustomRefresh();
        }

        /// <summary>
        /// Redraw form.
        /// </summary>
        public void CustomRefresh()
        {
            // Draw form directly rather than hand to event queue
            string beginDrawError = BeginDraw();
            if (string.IsNullOrEmpty(beginDrawError))
            {
                Draw();
                EndDraw();
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Resize event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Invalidate control
            this.Invalidate();
        }

        /// <summary>
        /// Mouse movement, delta, and time.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            // Update pointer ray
            if (isReady)
                core.Renderer.UpdatePointerRay(e.X, e.Y);
        }

        /// <summary>
        /// Mouse wheel movement.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
        }

        /// <summary>
        /// Mouse buttons down.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Set focus to this control
            this.Focus();

            // Store button state
            switch (e.Button)
            {
                case MouseButtons.Left:
                    break;
                case MouseButtons.Right:
                    break;
                case MouseButtons.Middle:
                    break;
            }
        }

        /// <summary>
        /// Mouse buttons up.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            // Store button state
            switch (e.Button)
            {
                case MouseButtons.Left:
                    break;
                case MouseButtons.Right:
                    break;
                case MouseButtons.Middle:
                    break;
            }
        }

        /// <summary>
        /// Mouse button pressed and release.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
        }

        /// <summary>
        /// Called when user double-clicks mouse.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
        }

        /// <summary>
        /// Mouse enters client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            // Set focus to this control
            this.Focus();
        }

        /// <summary>
        /// Mouse leaves client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Key down.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        /// <summary>
        /// Visible state of control has changed.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
        }

        #endregion

        #region Events

        /// <summary>
        /// Tick while idle.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        void TickWhileIdle(object sender, EventArgs e)
        {
            NativeMethods.Message message;
            while (!NativeMethods.PeekMessage(out message, IntPtr.Zero, 0, 0, 0))
            {
                Tick(sender, e);
            }
        }

        #endregion

        #region Native Methods

        /// <summary>
        /// Native methods.
        /// </summary>
        static class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Message
            {
                public IntPtr hWnd;
                public uint Msg;
                public IntPtr wParam;
                public IntPtr lParam;
                public uint Time;
                public System.Drawing.Point Point;
            }

            [DllImport("User32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool PeekMessage(out Message message, IntPtr hWnd, uint filterMin, uint filterMax, uint flags);
        }

        #endregion

    }

}
