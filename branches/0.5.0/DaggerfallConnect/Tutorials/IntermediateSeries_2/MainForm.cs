using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

namespace IntermediateSeries_Tutorial2
{
    public partial class MainForm : Form
    {
        // Specify Arena2 path of local Daggerfall installation
        string MyArena2Path = "C:\\dosgames\\DAGGER\\ARENA2";

        // Instantiate ImageFileReader
        ImageFileReader MyImageFileReader = new ImageFileReader();

        // Instantiate ARCH3D.BSA file manager
        private Arch3dFile arch3dFile = new Arch3dFile();

        // Set default mesh index
        private const int defaultMeshIndex = 5557;

        // Create bitmap as render target for wireframe mesh
        Bitmap renderTarget;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set MyImageFileReader Arena2 path
            MyImageFileReader.Arena2Path = MyArena2Path;

            // Set library type to textures
            MyImageFileReader.LibraryType = LibraryTypes.Texture;

            // Load ARCH3D.BSA
            arch3dFile.Load(Path.Combine(MyArena2Path, "ARCH3D.BSA"),
                FileUsage.UseDisk,
                true);

            // Create render target
            renderTarget = new Bitmap(RenderPictureBox.Width, RenderPictureBox.Height);
            RenderPictureBox.Image = renderTarget;

            // Set the slider value to default
            MeshIndexTrackBar.Value = defaultMeshIndex;

            // Show default mesh
            showMesh();
        }

        private void showMesh()
        {
            // Update render bitmap and refresh picture box
            arch3dFile.GetPreview(
                MeshIndexTrackBar.Value,    // Index of mesh
                renderTarget,               // Bitmap to render to
                Color.Black,                // Background colour
                Color.White,                // Wires colour
                true,                       // Antialias wires
                0);                         // Amount to rotate around Y axis

            // Update picture box render
            RenderPictureBox.Image = renderTarget;

            // Show textures associated with this mesh
            ShowMeshTextures();
        }

        private void ShowMeshTextures()
        {
            // Clear texture flow panel
            TexturesPanel.Controls.Clear();

            // Get the mesh data
            DFMesh mesh = arch3dFile.GetMesh(MeshIndexTrackBar.Value);

            // Loop through all submeshes
            foreach (DFMesh.DFSubMesh sm in mesh.SubMeshes)
            {
                // Load texture file
                string FileName = TextureFile.IndexToFileName(sm.TextureArchive);
                MyImageFileReader.LoadFile(FileName);

                // Get texture file
                DFImageFile textureFile = MyImageFileReader.ImageFile;

                // Get managed bitmap
                Bitmap bm = textureFile.GetManagedBitmap(sm.TextureRecord, 0, true, false);

                // Create a new picture box
                PictureBox pb = new PictureBox();
                pb.Width = bm.Width;
                pb.Height = bm.Height;
                pb.Image = bm;

                // Add picture box to flow panel
                TexturesPanel.Controls.Add(pb);
            }
        }

        private void MeshIndexTrackBar_Scroll(object sender, EventArgs e)
        {
            // Show the selected mesh index
            showMesh();
        }
    }
}
