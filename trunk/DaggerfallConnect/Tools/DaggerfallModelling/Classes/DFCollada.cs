// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using GWrapCollada;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace DaggerfallModelling.Classes
{
    /// <summary>
    /// Provides support for exporting Daggerfall models to Collada 1.4.1 format.
    ///  Uses GWrapCollada which wraps the static Collada DOM into a .NET assembly.
    /// </summary>
    public class DFCollada
    {
        #region Class Variables

        // Properties
        private UpVectors upVector = UpVectors.Y_UP;
        private ImageFormat imageFormat = System.Drawing.Imaging.ImageFormat.Png;
        private string modelOutputPath = string.Empty;
        private string imageOutputRelativePath = string.Empty;

        // Collada
        private _DAE dae = new _DAE();

        // Daggerfall Connect
        private string arena2Path;
        private Arch3dFile arch3dFile;
        private TextureFile textureFile;

        #endregion

        #region Class Structures

        /// <summary>
        /// Defines vectors to be used as "up" vectors.
        /// </summary>
        public enum UpVectors
        {
            X_UP,
            Y_UP,
            Z_UP,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets UpVector to use when exporting geometry.
        ///  This is achieved by setting a rotation on the geometry node.
        ///  Exporters should be aware of the up vector used by the destination platform
        ///  (e.g. Blender uses Z_UP, and XNA uses Y_UP).
        ///  Importers should likewise read up_vector under the asset tag and adjust to their platform.
        ///  It may also be required for imports to invert axes and swap winding order depending
        ///  on the target modelling program / engine.
        /// </summary>
        public UpVectors UpVector
        {
            get { return this.upVector; }
            set { this.upVector = value; }
        }

        /// <summary>
        /// Gets or sets System.Drawing.Imaging.ImageFormat to use when exporting textures.
        /// </summary>
        public System.Drawing.Imaging.ImageFormat ImageFormat
        {
            get { return this.imageFormat; }
            set { this.imageFormat = value; }
        }

        /// <summary>
        /// Gets or sets the output path for model exporting.
        /// </summary>
        public string ModelOutputPath
        {
            get { return this.modelOutputPath; }
            set { this.modelOutputPath = value; }
        }

        /// <summary>
        /// Gets or sets the output path for image exporting.
        ///  This must be _relative_ to model output path, not an
        ///  absolute path.
        /// </summary>
        public string ImageOutputRelativePath
        {
            get { return this.imageOutputRelativePath; }
            set { this.imageOutputRelativePath = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        public DFCollada(string arena2Path)
        {
            // Arena2
            this.arena2Path = arena2Path;

            // Textures
            this.textureFile = new TextureFile();
            this.textureFile.Palette = new DFPalette(
                Path.Combine(arena2Path, textureFile.PaletteName));

            // ARCH3D.BSA
            this.arch3dFile = new Arch3dFile(
                Path.Combine(arena2Path, "ARCH3D.BSA"),
                FileUsage.UseDisk,
                true);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Exports a DFMesh object to Collada format.
        /// </summary>
        /// <param name="id">ID of DFMesh to export.</param>
        /// <returns></returns>
        public bool DFMeshToCollada(uint id)
        {
            // Get source mesh
            int index = arch3dFile.GetRecordIndex(id);
            DFMesh dfMesh = arch3dFile.GetMesh(index);
            if (dfMesh.SubMeshes == null)
            {
                string error = string.Format("Model '{0}' not found", id);
                Console.WriteLine(error);
                throw new Exception(error);
            }

            // Test model output path
            if (!Directory.Exists(modelOutputPath))
            {
                string error = string.Format("Model output path '{0}' does not exist.", modelOutputPath);
                Console.WriteLine(error);
                throw new Exception(error);
            }

            // Test or create image output path
            string fullImageOutputPath = Path.Combine(modelOutputPath, imageOutputRelativePath);
            if (!Directory.Exists(fullImageOutputPath))
            {
                Directory.CreateDirectory(fullImageOutputPath);
            }

            // Create dae root
            string filePath = Path.Combine(modelOutputPath, dfMesh.ObjectId.ToString() + ".dae");
            _daeElement root = dae.add(filePath);

            // Build dae file
            AddAsset(ref root);
            AddGeometry(ref root, ref dfMesh);
            AddImages(ref root, ref dfMesh);
            AddEffects(ref root, ref dfMesh);
            AddMaterials(ref root, ref dfMesh);
            AddVisualScene(ref root, ref dfMesh);

            // Write dae file
            dae.writeAll();

            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds asset element.
        /// </summary>
        /// <param name="root">Root dae element.</param>
        private _daeElement AddAsset(ref _daeElement root)
        {
            // Get ISO 8601 UTC date time
            string xsDateTime = DateTime.Now.ToUniversalTime().ToString("o");

            // Create <asset>
            _daeElement asset = root.add("asset");
            asset.add("created").setCharData(xsDateTime);
            asset.add("modified").setCharData(xsDateTime);

            // Add up vector to <asset>
            switch (this.upVector)
            {
                case UpVectors.X_UP:
                    asset.add("up_axis").setCharData("X_UP");
                    break;
                case UpVectors.Y_UP:
                    asset.add("up_axis").setCharData("Y_UP");
                    break;
                case UpVectors.Z_UP:
                    asset.add("up_axis").setCharData("Z_UP");
                    break;
            }

            return asset;
        }

        /// <summary>
        /// Adds geometry elements.
        /// </summary>
        /// <param name="root">Root element.</param>
        /// <param name="dfMesh">DFMesh.</param>
        private void AddGeometry(ref _daeElement root, ref DFMesh dfMesh)
        {
            _daeElement geomLib = root.add("library_geometries");
            _daeElement geom = geomLib.add("geometry");

            string geomId = "model" + dfMesh.ObjectId.ToString();
            geom.setAttribute("id", geomId);
            _daeElement mesh = geom.add("mesh");

            // Get source vertex data
            List<float> posArray = new List<float>(dfMesh.TotalVertices * 3);
            List<float> normalArray = new List<float>(dfMesh.TotalVertices * 3);
            List<float> uvArray = new List<float>(dfMesh.TotalVertices * 2);
            foreach (var subMesh in dfMesh.SubMeshes)
            {
                // Get texture dimensions for this submesh
                string archivePath = Path.Combine(arena2Path, TextureFile.IndexToFileName(subMesh.TextureArchive));
                System.Drawing.Size sz = TextureFile.QuickSize(archivePath, subMesh.TextureRecord);

                // Collect vertex information for every plane in this submesh
                foreach (var plane in subMesh.Planes)
                {
                    foreach (var point in plane.Points)
                    {
                        // Add position data
                        posArray.Add(point.X);
                        posArray.Add(-point.Y);
                        posArray.Add(-point.Z);

                        // Add normal data
                        normalArray.Add(point.NX);
                        normalArray.Add(-point.NY);
                        normalArray.Add(-point.NZ);

                        // Add UV data
                        uvArray.Add(point.U / (float)sz.Width);
                        uvArray.Add(-(point.V / (float)sz.Height));
                    }
                }
            }

            // Add positions, normals, and texture coordinates
            AddSource(ref mesh, geomId + "-positions", "X Y Z", ref posArray);
            AddSource(ref mesh, geomId + "-normals", "X Y Z", ref normalArray);
            AddSource(ref mesh, geomId + "-uv", "S T", ref uvArray);

            // Add <vertices> element
            _daeElement vertices = mesh.add("vertices");
            vertices.setAttribute("id", geomId + "-vertices");
            _daeElement verticesInput = vertices.add("input");
            verticesInput.setAttribute("semantic", "POSITION");
            verticesInput.setAttribute("source", MakeUriRef(geomId + "-positions"));

            // Add triangle indices for each submesh
            uint vertexCount = 0;
            foreach (var subMesh in dfMesh.SubMeshes)
            {
                // Loop through all planes in this submesh
                List<uint> indexArray = new List<uint>(subMesh.TotalTriangles * (3 * 3));
                foreach (var plane in subMesh.Planes)
                {
                    // Every DFPlane is a triangle fan radiating from point 0
                    uint sharedPoint = vertexCount++;

                    // Index remaining points. There are (plane.Points.Length - 2) triangles in every plane
                    for (int tri = 0; tri < plane.Points.Length - 2; tri++)
                    {
                        // Position, Normal, UV index for shared point
                        indexArray.Add(sharedPoint);
                        indexArray.Add(sharedPoint);
                        indexArray.Add(sharedPoint);

                        // Position, Normal, UV index for vertexCount
                        indexArray.Add(vertexCount);
                        indexArray.Add(vertexCount);
                        indexArray.Add(vertexCount);

                        // Position, Normal, UV index for vertexCount + 1
                        indexArray.Add(vertexCount + 1);
                        indexArray.Add(vertexCount + 1);
                        indexArray.Add(vertexCount + 1);

                        // Increment vertexCount to next point in fan
                        vertexCount++;
                    }

                    // Increment vertexCount to start of next fan in vertex buffer
                    vertexCount++;
                }

                // Add <triangle>
                string materialName = MakeMaterialName(subMesh.TextureArchive, subMesh.TextureRecord);
                _daeElement triangles = mesh.add("triangles");
                triangles.setAttribute("count", (indexArray.Count / (3 * 3)).ToString());
                triangles.setAttribute("material", materialName + "-material");

                AddInput(ref triangles, "VERTEX", geomId + "-vertices", 0);
                AddInput(ref triangles, "NORMAL", geomId + "-normals", 1);
                AddInput(ref triangles, "TEXCOORD", geomId + "-uv", 2);

                // Add <p>
                _daeElement p = triangles.add("p");
                p.setCharData(UIntArrayToString(ref indexArray));
            }
        }

        /// <summary>
        /// Adds source data.
        /// </summary>
        /// <param name="mesh">Parent mesh element.</param>
        /// <param name="srcID">ID of new source.</param>
        /// <param name="paramNames">Parameters.</param>
        /// <param name="values">Values.</param>
        private void AddSource(ref _daeElement mesh, string srcID, string paramNames, ref List<float> values)
        {
            _daeElement src = mesh.add("source");
            src.setAttribute("id", srcID);

            _daeElement fa = src.add("float_array");
            fa.setAttribute("id", src.getAttribute("id") + "-array");
            fa.setAttribute("count", values.Count.ToString());
            fa.setCharData(FloatArrayToString(ref values));

            _daeElement acc = src.add("technique_common accessor");
            acc.setAttribute("source", MakeUriRef(fa.getAttribute("id")));

            string[] _params = paramNames.Split(' ');
            acc.setAttribute("stride", _params.Length.ToString());
            acc.setAttribute("count", (values.Count / _params.Length).ToString());

            foreach (string param in _params)
            {
                _daeElement p = acc.add("param");
                p.setAttribute("name", param);
                p.setAttribute("type", "float");
            }
        }

        /// <summary>
        /// Converts a list of floats to a string.
        /// </summary>
        /// <param name="values">Values.</param>
        /// <returns>String.</returns>
        private string FloatArrayToString(ref List<float> values)
        {
            string valueString = string.Empty;
            foreach (float value in values)
            {
                valueString += value.ToString() + " ";
            }
            return valueString;
        }

        /// <summary>
        /// Converts a list of uints to a string.
        /// </summary>
        /// <param name="values">Values.</param>
        /// <returns>String.</returns>
        private string UIntArrayToString(ref List<uint> values)
        {
            string valueString = string.Empty;
            foreach (uint value in values)
            {
                valueString += value.ToString() + " ";
            }
            return valueString;
        }

        /// <summary>
        /// Converts ID to URI reference.
        /// </summary>
        /// <param name="id">ID.</param>
        /// <returns>URI reference.</returns>
        private string MakeUriRef(string id)
        {
            return "#" + id;
        }

        /// <summary>
        /// Compose material name from archive and record indices.
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        private string MakeMaterialName(int archive, int record)
        {
            return string.Format(
                "{0}_{1}",
                TextureFile.IndexToFileName(archive),
                record);
        }

        /// <summary>
        /// Adds input.
        /// </summary>
        /// <param name="triangles">Triangles element.</param>
        /// <param name="semantic">Semantic.</param>
        /// <param name="srcID">Source ID.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>True if successful.</returns>
        private bool AddInput(ref _daeElement triangles, string semantic, string srcID, int offset)
        {
            _daeElement input = triangles.add("input");
            input.setAttribute("semantic", semantic);
            input.setAttribute("offset", offset.ToString());
            input.setAttribute("source", MakeUriRef(srcID));
            if (semantic == "TEXCOORD")
                input.setAttribute("set", "0");
            return true;
        }

        /// <summary>
        /// Exports texture images and add references to the dae file.
        /// </summary>
        /// <param name="root">Root element.</param>
        /// <param name="dfMesh">DFMesh.</param>
        /// <returns>True if successful.</returns>
        private bool AddImages(ref _daeElement root, ref DFMesh dfMesh)
        {
            // Create <library_images>
            _daeElement imageLib = root.add("library_images");

            // Export all textures.
            foreach (var subMesh in dfMesh.SubMeshes)
            {
                // Construct string
                string materialName = MakeMaterialName(subMesh.TextureArchive, subMesh.TextureRecord);
                string inputFilename = TextureFile.IndexToFileName(subMesh.TextureArchive);
                string outputFilename = materialName + "." + imageFormat.ToString().ToLower();

                // Export texture
                string outputPath = Path.Combine(modelOutputPath, imageOutputRelativePath);
                string outputFilePath = Path.Combine(outputPath, outputFilename);
                if (!File.Exists(outputFilePath))
                {
                    // Load texture file
                    textureFile.Load(
                        Path.Combine(arena2Path, inputFilename),
                        FileUsage.UseDisk,
                        true);

                    // Export image
                    Bitmap bm = textureFile.GetManagedBitmap(
                        subMesh.TextureRecord,
                        0,
                        false,
                        true);
                    bm.Save(outputFilePath, imageFormat);
                }

                // Add <image>
                string relativeOutputPath = Path.Combine(imageOutputRelativePath, outputFilename);
                relativeOutputPath = relativeOutputPath.Replace("\\", "/");
                _daeElement image = imageLib.add("image");
                image.setAttribute("id", materialName + "-image");
                image.add("init_from").setCharData(relativeOutputPath);
            }

            return true;
        }

        /// <summary>
        /// Adds visual effects for materials.
        /// </summary>
        /// <param name="root">Root element.</param>
        /// <param name="dfMesh">DFMesh.</param>
        /// <returns>True if successful.</returns>
        private bool AddEffects(ref _daeElement root, ref DFMesh dfMesh)
        {
            // Add <library_effects>
            _daeElement effectLib = root.add("library_effects");

            // Add one effect per submesh
            foreach (var subMesh in dfMesh.SubMeshes)
            {
                // Compose strings
                string materialName = MakeMaterialName(subMesh.TextureArchive, subMesh.TextureRecord);
                string effectName = materialName + "-effect";

                // Add <effect> and profile_COMMON
                _daeElement effect = effectLib.add("effect");
                effect.setAttribute("id", effectName);
                _daeElement profile = effect.add("profile_COMMON");

                // Add <surface>
                _daeElement newparam = profile.add("newparam");
                newparam.setAttribute("sid", "surface");
                _daeElement surface = newparam.add("surface");
                surface.setAttribute("type", "2D");
                surface.add("init_from").setCharData(materialName + "-image");

                // Add <sampler2D>
                newparam = profile.add("newparam");
                newparam.setAttribute("sid", "sampler");
                _daeElement sampler = newparam.add("sampler2D");
                sampler.add("source").setCharData("surface");
                sampler.add("minfilter").setCharData("LINEAR_MIPMAP_LINEAR");
                sampler.add("magfilter").setCharData("LINEAR");

                // Add <technique>
                _daeElement technique = profile.add("technique");
                technique.setAttribute("sid", "common");
                _daeElement texture = technique.add("phong diffuse texture");
                texture.setAttribute("texture", "sampler");
                texture.setAttribute("texcoord", "uv0");
            }

            return true;
        }

        /// <summary>
        /// Adds materials.
        /// </summary>
        /// <param name="root">Root element.</param>
        /// <param name="dfMesh">DFMesh.</param>
        /// <returns>True if successful.</returns>
        private bool AddMaterials(ref _daeElement root, ref DFMesh dfMesh)
        {
            // Add <library_materials>
            _daeElement materialLib = root.add("library_materials");

            // Add one material per submesh
            foreach (var subMesh in dfMesh.SubMeshes)
            {
                // Add <material>
                string materialName = MakeMaterialName(subMesh.TextureArchive, subMesh.TextureRecord);
                _daeElement material = materialLib.add("material");
                material.setAttribute("id", materialName + "-material");
                material.add("instance_effect").setAttribute("url", MakeUriRef(materialName + "-effect"));
            }

            return true;
        }

        private void AddVisualScene(ref _daeElement root, ref DFMesh dfMesh)
        {
            string geomId = "model" + dfMesh.ObjectId.ToString();
            string sceneName = geomId + "-scene";

            _daeElement visualSceneLib = root.add("library_visual_scenes");
            _daeElement visualScene = visualSceneLib.add("visual_scene");
            visualScene.setAttribute("id", sceneName);

            // Add <node>
            _daeElement node = visualScene.add("node");
            node.setAttribute("id", geomId + "-node");

            // Transform <node> based on up vector
            switch (this.upVector)
            {
                case UpVectors.X_UP:
                    node.add("rotate").setCharData("0 0 1 -90");
                    break;
                case UpVectors.Y_UP:
                    // No change required. This is how the geometry is in DFMesh.
                    break;
                case UpVectors.Z_UP:
                    node.add("rotate").setCharData("1 0 0 90");
                    break;
            }

            // Instantiate the <geometry>
            _daeElement instanceGeom = node.add("instance_geometry");
            instanceGeom.setAttribute("url", MakeUriRef(geomId));

            // Bind material parameters
            _daeElement techniqueCommon = instanceGeom.add("bind_material technique_common");
            foreach (var subMesh in dfMesh.SubMeshes)
            {
                string materialName = MakeMaterialName(subMesh.TextureArchive, subMesh.TextureRecord);
                _daeElement instanceMaterial = techniqueCommon.add("instance_material");
                instanceMaterial.setAttribute("symbol", materialName + "-material");
                instanceMaterial.setAttribute("target", MakeUriRef(materialName + "-material"));

                _daeElement bindVertexInput = instanceMaterial.add("bind_vertex_input");
                bindVertexInput.setAttribute("semantic", "uv0");
                bindVertexInput.setAttribute("input_semantic", "TEXCOORD");
                bindVertexInput.setAttribute("input_set", "0");
            }

            // Add a <scene>
            root.add("scene instance_visual_scene").setAttribute("url", MakeUriRef(sceneName));
        }

        #endregion
    }
}
