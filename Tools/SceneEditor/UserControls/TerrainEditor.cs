using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SceneEditor.UserControls
{
    public partial class TerrainEditor : UserControl
    {
        public TerrainEditor()
        {
            InitializeComponent();
        }

        public void PositionCrosshair(float u, float v)
        {
            int x = (int)((float)this.HeightMapImage.Width * u) - 2;
            int y = (int)((float)this.HeightMapImage.Height * v) - 2;
            CrosshairImage.Location = new Point(x, y);
        }
    }
}
