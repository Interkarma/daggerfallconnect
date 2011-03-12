using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

namespace Tutorial_XNASeries_1
{

    #region DFMeshLoader

    /// <summary>
    /// Simple class to handle loading and drawing a Daggerfall mesh in XNA.
    /// </summary>
    public class DFMeshHelper
    {
        private DFMesh dfMesh;
        private ImageFileReader imageFileReader = new ImageFileReader();
        private Arch3dFile arch3dFile = new Arch3dFile();
        private GraphicsDevice graphicsDevice;

        public submesh[] submeshes;
        public VertexPositionNormalTexture[] vertices;
        public Dictionary<int, Texture2D> textures = new Dictionary<int, Texture2D>();

        /// <summary>Stores local submesh data.</summary>
        public struct submesh
        {
            public int textureKey;
            public short[] indices;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="device">Your GraphicsDevice.</param>
        /// <param name="arena2Path">Absolute path to your Arena2 folder.</param>
        public DFMeshHelper(GraphicsDevice device, string arena2Path)
        {
            graphicsDevice = device;
            imageFileReader.Arena2Path = arena2Path;
            arch3dFile.Load(Path.Combine(arena2Path, "ARCH3D.BSA"), FileUsage.UseDisk, true);
        }

        /// <summary>
        /// Loads a DF mesh.
        /// </summary>
        /// <param name="index">Index of mesh to load.</param>
        public void LoadMesh(int index)
        {
            // Load mesh
            dfMesh = arch3dFile.GetMesh(index);

            // Load mesh data. These three methods can be combined to improve loading & conversion efficiency.
            // They have been split out here to better demonstrate how each part works.
            LoadTextures();
            LoadVertices();
            LoadIndices();
        }

        /// <summary>
        /// Draws the loaded mesh using the BasicEffect specifed.
        /// </summary>
        /// <param name="effect"></param>
        public void DrawMesh(ref BasicEffect effect)
        {
            foreach (var submesh in submeshes)
            {
                effect.Texture = textures[submesh.textureKey];

                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();

                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length,
                    submesh.indices, 0, submesh.indices.Length / 3);

                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }

        private void LoadTextures()
        {
            // Loop through all submeshes
            foreach (DFMesh.DFSubMesh subMesh in dfMesh.SubMeshes)
            {
                // Get DF texture in ARGB format so we can just SetData the byte array into XNA
                DFImageFile textureFile = imageFileReader.LoadFile(TextureFile.IndexToFileName(subMesh.TextureArchive));
                DFBitmap dfbitmap = textureFile.GetBitmapFormat(subMesh.TextureRecord, 0, 0, DFBitmap.Formats.ARGB);

                // Create XNA texture
                Texture2D texture = new Texture2D(graphicsDevice, dfbitmap.Width, dfbitmap.Height, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
                texture.SetData<byte>(dfbitmap.Data);

                // Store texture in dictionary
                int textureKey = subMesh.TextureArchive * 100 + subMesh.TextureRecord;
                if (!textures.ContainsKey(textureKey))
                    textures.Add(textureKey, texture);
            }
        }

        private void LoadVertices()
        {
            // Allocate vertex buffer
            vertices = new VertexPositionNormalTexture[dfMesh.TotalVertices];

            // Loop through all submeshes
            int vertexCount = 0;
            foreach (DFMesh.DFSubMesh subMesh in dfMesh.SubMeshes)
            {
                // Get texture dimensions. This is required to normalise Daggerfall's texture coordinates
                int textureKey = subMesh.TextureArchive * 100 + subMesh.TextureRecord;
                float textureWidth = textures[textureKey].Width;
                float textureHeight = textures[textureKey].Height;

                // Loop through all planes in this submesh
                foreach (DFMesh.DFPlane plane in subMesh.Planes)
                {
                    // Copy each point in this plane to vertex buffer
                    foreach (DFMesh.DFPoint point in plane.Points)
                    {
                        // Daggerfall uses a different axis layout than XNA.
                        // The X and Y axes should be inverted so the model is displayed correctly.
                        // This also requires a change to winding order in LoadIndices().
                        vertices[vertexCount].Position = new Vector3(-point.X, -point.Y, point.Z);
                        vertices[vertexCount].Normal = new Vector3(-point.NX, -point.NY, point.NZ);
                        vertices[vertexCount].TextureCoordinate = new Vector2(point.U / textureWidth, point.V / textureHeight);
                        vertexCount++;
                    }
                }
            }
        }

        private void LoadIndices()
        {
            // Allocate local submesh buffer
            submeshes = new submesh[dfMesh.SubMeshes.Length];

            // Loop through all submeshes
            int subMeshCount = 0;
            int vertexCount = 0;
            foreach (DFMesh.DFSubMesh subMesh in dfMesh.SubMeshes)
            {
                // Allocate index buffer
                submeshes[subMeshCount].indices = new short[subMesh.TotalTriangles * 3];

                // Store texture key for this submesh
                submeshes[subMeshCount].textureKey = subMesh.TextureArchive * 100 + subMesh.TextureRecord;

                // Loop through all planes in this submesh
                int indexCount = 0;
                foreach (DFMesh.DFPlane plane in subMesh.Planes)
                {
                    // Every DFPlane is a triangle fan radiating from point 0
                    short sharedPoint = (short)vertexCount++;

                    // Index remaining points. There are plane.Points.Length - 2 triangles in every plane
                    for (int tri = 0; tri < plane.Points.Length - 2; tri++)
                    {
                        // Store 3 points of current triangle.
                        // The second two indices are swapped so the winding order is correct after
                        // inverting X and Y axes in LoadVertices().
                        submeshes[subMeshCount].indices[indexCount++] = sharedPoint;
                        submeshes[subMeshCount].indices[indexCount++] = (short)(vertexCount + 1);
                        submeshes[subMeshCount].indices[indexCount++] = (short)(vertexCount);

                        // Increment vertexCount to next point in fan
                        vertexCount++;
                    }

                    // Increment vertexCount to start of next fan in vertex buffer
                    vertexCount++;
                }

                // Increment submesh count
                subMeshCount++;
            }
        }

    }

    #endregion

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GraphicsDevice device;
        Matrix cameraMatrix;
        Matrix projectionMatrix;
        BasicEffect effect;
        VertexDeclaration vertexDeclaration;
        float angle = 0f;

        DFMeshHelper meshHelper;
        string MyArena2Path = "C:\\dosgames\\DAGGER\\ARENA2";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Window.Title = "Tutorial_XNASeries_1";

            base.Initialize();
            InitializeWorld();
        }

        public void InitializeWorld()
        {
            cameraMatrix = Matrix.CreateLookAt(new Vector3(0, 350, 1500), new Vector3(0, 350, 0), new Vector3(0, 1, 0));
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)Window.ClientBounds.Width / (float)Window.ClientBounds.Height, 1.0f, 2000.0f);
            vertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);

            effect = new BasicEffect(GraphicsDevice, null);
            effect.World = Matrix.Identity;
            effect.View = cameraMatrix;
            effect.Projection = projectionMatrix;
            effect.TextureEnabled = true;
            effect.PreferPerPixelLighting = true;
            effect.EnableDefaultLighting();
            effect.SpecularColor = new Vector3(0.3f, 0.3f, 0.3f);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            device = graphics.GraphicsDevice;

            // Initialise mesh loader and load default mesh
            meshHelper = new DFMeshHelper(device, MyArena2Path);
            meshHelper.LoadMesh(5557);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            angle += 0.3f;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            device.VertexDeclaration = vertexDeclaration;

            effect.World = Matrix.CreateRotationY(MathHelper.ToRadians(angle));

            meshHelper.DrawMesh(ref effect);

            base.Draw(gameTime);
        }
    }
}
