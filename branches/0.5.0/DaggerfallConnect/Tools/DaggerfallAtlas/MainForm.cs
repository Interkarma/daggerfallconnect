using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DaggerfallAtlas
{
    public partial class MainForm : Form
    {
        private string arena2Path = "C:\\dosgames\\DAGGER\\ARENA2";

        public MainForm()
        {
            InitializeComponent();

            dfRegionFlow1.Arena2Path = arena2Path;
        }
    }
}
