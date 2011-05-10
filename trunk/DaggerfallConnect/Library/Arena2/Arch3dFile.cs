// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using DaggerfallConnect.Utility;
#endregion

namespace DaggerfallConnect.Arena2
{
    /// <summary>
    /// Connects to ARCH3D.BSA to enumerate and extract 3D mesh data.
    /// </summary>
    public class Arch3dFile
    {
        #region Class Variables

        // Buffer lengths used during decomposition
        private const int CornerBufferLength = 16;
        private const int UniqueTextureBufferLength = 32;
        private const int SubMeshBufferLength = 32;
        private const int PlaneBufferLength = 512;
        private const int PointBufferLength = 16;
        private const int IndexBufferLength = 16;
        private const int CalculatedUVBufferLength = 24;

        // Divisors for points and textures
        private const float PointDivisor = 256.0f;
        private const float TextureDivisor = 16.0f;

        // Buffer arrays used during decomposition
        private int[] CornerPointBuffer = new int[CornerBufferLength];
        private TextureIndex[] UniqueTextureBuffer = new TextureIndex[UniqueTextureBufferLength];
        private DFSubMeshBuffer[] SubMeshBuffer = new DFSubMeshBuffer[SubMeshBufferLength];
        private FaceUVTool.DFPurePoint[] CalculatedUVBuffer = new FaceUVTool.DFPurePoint[CalculatedUVBufferLength];

        /// <summary>
        /// Index lookup dictionary.
        /// </summary>
        private Dictionary<UInt32, int> RecordIndexLookup = new Dictionary<UInt32, int>();

        /// <summary>
        /// Auto-discard behaviour enabled or disabled.
        /// </summary>
        private bool AutoDiscardValue = true;

        /// <summary>
        /// The last record opened. Used by autoDiscard logic
        /// </summary>
        private int LastRecord = -1;

        /// <summary>
        /// The BsaFile representing ARCH3D.BSA
        /// </summary>
        private BsaFile BsaFile = new BsaFile();

        /// <summary>
        /// Array of decomposed mesh records.
        /// </summary>
        private MeshRecord[] Records;

        /// <summary>
        /// Object for calculating UV values of face points
        /// </summary>
        private FaceUVTool FaceUVTool = new FaceUVTool();

        #endregion

        #region Class Structures

        /// <summary>
        /// Possible mesh versions enumeration.
        /// </summary>
        internal enum MeshVersions
        {
            Unknown,
            Version25,
            Version26,
            Version27,
        }

        /// <summary>
        /// Represents ARCH3D.BSA file header.
        /// </summary>
        internal struct FileHeader
        {
            public long Position;
            public String Version;
            public Int32 PointCount;
            public Int32 PlaneCount;
            public UInt32 Unknown1;
            public UInt64 NullValue1;
            public Int32 PlaneDataOffset;
            public Int32 ObjectDataOffset;
            public Int32 ObjectDataCount;
            public UInt32 Unknown2;
            public UInt64 NullValue2;
            public Int32 PointListOffset;
            public Int32 NormalListOffset;
            public UInt32 Unknown3;
            public Int32 PlaneListOffset;
        }

        /*
        /// <summary>
        /// Header to unknown object data.
        /// </summary>
        internal struct ObjectDataHeader
        {
            public Int32[] Numbers;
            public Int16 SubRecordCount;
        }
        */

        /// <summary>
        /// A single mesh record.
        /// </summary>
        internal struct MeshRecord
        {
            public UInt32 ObjectId;
            public MeshVersions Version;
            public FileProxy MemoryFile;
            public FileHeader Header;
            public PureMesh PureMesh;
            public DFMesh DFMesh;
        }

        /// <summary>
        /// Native plane (face) header.
        /// </summary>
        internal struct PlaneHeader
        {
            public long Position;
            public Byte PlanePointCount;
            public Byte Unknown1;
            public UInt16 Texture;
            public UInt32 Unknown2;
        }

        /// <summary>
        /// A texture index.
        /// </summary>
        internal struct TextureIndex
        {
            public int Archive;
            public int Record;
        }

        /// <summary>
        /// Native mesh.
        /// </summary>
        internal struct PureMesh
        {
            public TextureIndex[] UniqueTextures;
            public PurePlane[] Planes;
        }

        /// <summary>
        /// A single native plane (face).
        /// </summary>
        internal struct PurePlane
        {
            public PlaneHeader Header;
            public TextureIndex TextureIndex;
            public byte[] PlaneData;
            public FaceUVTool.DFPurePoint[] Points;
        }

        /// <summary>
        /// A submesh. All planes of this submesh are grouped by unique texture index.
        /// </summary>
        private struct DFSubMeshBuffer
        {
            public int TextureArchive;
            public int TextureRecord;
            public int planeCount;
            public DFPlaneBuffer[] PlaneBuffer;
        }

        /// <summary>
        /// Buffer for storing plane data during decomposition.
        /// </summary>
        private struct DFPlaneBuffer
        {
            public int PointCount;
            public DFMesh.DFPoint[] PointBuffer;
            public DFMesh.UVGenerationMethods UVGenerationMethod;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Arch3dFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="FilePath">Absolute path to ARCH3D.BSA file.</param>
        /// <param name="Usage">Determines if the BSA file will read from disk or memory.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public Arch3dFile(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            Load(FilePath, Usage, ReadOnly);
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets mesh record array.
        /// </summary>
        internal MeshRecord[] MeshRecords
        {
            get { return Records; }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// If true then decomposed mesh records will be destroyed every time a different record is fetched.
        ///  If false then decomposed mesh records will be maintained until discardRecord() or discardAllRecords() is called.
        ///  Turning off auto-discard will speed up mesh retrieval times at the expense of RAM. For best results, disable
        ///  auto-discard and impose your own caching scheme using LoadRecord() and DiscardRecord() based on your application's
        ///  needs.
        /// </summary>
        public bool AutoDiscard
        {
            get { return AutoDiscardValue; }
            set { AutoDiscardValue = value; }
        }

        /// <summary>
        /// Number of BSA records in ARCH3D.BSA.
        /// </summary>
        public int Count
        {
            get {return BsaFile.Count;}
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load ARCH3D.BSA file.
        /// </summary>
        /// <param name="FilePath">Absolute path to ARCH3D.BSA file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            // Validate filename
            FilePath = FilePath.ToUpper();
            if (!FilePath.EndsWith("ARCH3D.BSA"))
                return false;

            // Load file
            if (!BsaFile.Load(FilePath, Usage, ReadOnly))
                return false;

            // Create records array
            Records = new MeshRecord[BsaFile.Count];

            return true;
        }

        /// <summary>
        /// Gets index of mesh record with specified id. Does not change the currently loaded record.
        ///  Uses a dictionary to map ID to index so this method will be faster on subsequent calls.
        /// </summary>
        /// <param name="Id">ID of mesh.</param>
        /// <returns>Index of found mesh, or -1 if not found.</returns>
        public int GetRecordIndex(uint Id)
        {
            // Return known value if already indexed
            if (RecordIndexLookup.ContainsKey(Id))
                return RecordIndexLookup[Id];

            // Otherwise find and store index by searching for id
            for (int i = 0; i < Count; i++)
            {
                if (BsaFile.GetRecordId(i) == Id)
                {
                    RecordIndexLookup.Add(Id, i);
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets ID of record from index.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>ID of mesh.</returns>
        public uint GetRecordId(int Record)
        {
            return BsaFile.GetRecordId(Record);
        }

        /// <summary>
        /// Load a mesh record into memory and decompose it for use.
        /// </summary>
        /// <param name="Record">Index of record to load.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool LoadRecord(int Record)
        {
            // Validate
            if (Record < 0 || Record >= BsaFile.Count)
                return false;

            // Exit if file has already been opened
            if (Records[Record].MemoryFile != null && Records[Record].PureMesh.Planes != null)
                return true;

            // Auto discard previous record
            if (AutoDiscardValue && LastRecord != -1)
                DiscardRecord(LastRecord);

            // Load record
            Records[Record].MemoryFile = BsaFile.GetRecordProxy(Record);
            if (Records[Record].MemoryFile == null)
                return false;

            // Read record
            if (!Read(Record))
            {
                DiscardRecord(Record);
                return false;
            }

            // Store in lookup dictionary
            if (!RecordIndexLookup.ContainsKey(Records[Record].ObjectId))
                RecordIndexLookup.Add(Records[Record].ObjectId, Record);

            // Set previous record
            LastRecord = Record;

            return true;
        }

        /// <summary>
        /// Discard a mesh record from memory.
        /// </summary>
        /// <param name="Record">Index of record to discard.</param>
        public void DiscardRecord(int Record)
        {
            // Validate
            if (Record < 0 || Record >= BsaFile.Count)
                return;

            // Discard mesh data stored in memory
            Records[Record].ObjectId = 0;
            Records[Record].Version = MeshVersions.Unknown;
            Records[Record].MemoryFile = null;
            Records[Record].PureMesh.Planes = null;
            Records[Record].DFMesh.ObjectId = 0;
            Records[Record].DFMesh.TotalVertices = 0;
            Records[Record].DFMesh.SubMeshes = null;
        }

        /// <summary>
        /// Discard all mesh records.
        /// </summary>
        public void DiscardAllRecords()
        {
            // Clear all records
            for (int record = 0; record < BsaFile.Count; record++)
            {
                DiscardRecord(record);
            }
        }

        /// <summary>
        /// Obtain a new preview rendering of a mesh. Uses GDI+ to draw wireframe representation on native mesh data.
        /// Mesh is automatically scaled and positioned as best possible to fill thumbail area.
        /// </summary>
        /// <param name="Record">Index of record to load.</param>
        /// <param name="Background">Colour of background field.</param>
        /// <param name="Wires">Colour of wires used to draw mesh into thumbnail.</param>
        /// <param name="Width">Width of thumbnail image.</param>
        /// <param name="Height">Height of thumbnail image.</param>
        /// <param name="Antialias">True to antialias wires.</param>
        /// <param name="RotateY">Amount to rotate around Y axis in degrees.</param>
        /// <returns>A new System.Drawing.Image.</returns>
        public Image GetPreview(int Record, Color Background, Color Wires, int Width, int Height, bool Antialias, float RotateY)
        {
            // Create and draw image
            Image image = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            GetPreview(Record, image, Background, Wires, Antialias, RotateY);
            return image;
        }

        /// <summary>
        /// Obtain a new preview rendering of a mesh. Uses GDI+ to draw wireframe representation on native mesh data.
        /// Mesh is automatically scaled and positioned as best possible to fill thumbail area.
        /// </summary>
        /// <param name="Record">Index of record to load.</param>
        /// <param name="Image">Existing image to update.</param>
        /// <param name="Background">Colour of background field.</param>
        /// <param name="Wires">Colour of wires used to draw mesh into thumbnail.</param>
        /// <param name="Antialias">True to antialias wires.</param>
        /// <param name="RotateY">Amount to rotate around Y axis in degrees.</param>
        /// <returns>A new System.Drawing.Image.</returns>
        public Image GetPreview(int Record, Image Image, Color Background, Color Wires, bool Antialias, float RotateY)
        {
            // Load the record
            if (!LoadRecord(Record))
                return Image;

            // Get width and height
            int width = Image.Width;
            int height = Image.Height;

            // Get graphics object
            Graphics gr = Graphics.FromImage(Image);
            gr.Clear(Background);

            // Set smoothing mode
            if (Antialias)
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Get mesh bounds
            Vector3 min = new Vector3(0, 0, 0);
            Vector3 max = new Vector3(0, 0, 0);
            int faceCount = Records[Record].PureMesh.Planes.Length;
            for (int face = 0; face < faceCount; face++)
            {
                int pointCount = Records[Record].PureMesh.Planes[face].Points.Length;
                for (int point = 0; point < pointCount; point++)
                {
                    float x = Records[Record].PureMesh.Planes[face].Points[point].x >> 8;
                    float y = Records[Record].PureMesh.Planes[face].Points[point].y >> 8;
                    float z = Records[Record].PureMesh.Planes[face].Points[point].z >> 8;
                    if (x < min.X) min.X = x;
                    if (x > max.X) max.X = x;
                    if (y < min.Y) min.Y = y;
                    if (y > max.Y) max.Y = y;
                    if (z < min.Z) min.Z = z;
                    if (z > max.Z) max.Z = z;
                }
            }

            // Render faces as 2d lines
            for (int face = 0; face < faceCount; face++)
            {
                int pointCount = Records[Record].PureMesh.Planes[face].Points.Length;
                for (int point = 0; point < pointCount; point++)
                {
                    // Get start and end points for line (wrap back to start if on last point)
                    Point p1, p2;
                    p1 = Get2dPoint(Records[Record].PureMesh.Planes[face].Points[point], min, max, width, height, RotateY);
                    if (point + 1 >= pointCount)
                        p2 = Get2dPoint(Records[Record].PureMesh.Planes[face].Points[0], min, max, width, height, RotateY);
                    else
                        p2 = Get2dPoint(Records[Record].PureMesh.Planes[face].Points[point + 1], min, max, width, height, RotateY);

                    // Draw the line
                    gr.DrawLine(new Pen(Wires), p1, p2);
                }
            }

            return Image;
        }

        /// <summary>
        /// Benchmark mesh decomposition time. Forces on auto-discard behaviour and discards all existing records before starting.
        /// </summary>
        /// <returns>Time to decompose Count meshes in milliseconds.</returns>
        public int Benchmark()
        {
            // Must be ready
            if (BsaFile.Count == 0)
                return -1;

            // Force on autoDiscard and discard all records
            AutoDiscardValue = true;
            DiscardAllRecords();

            // Get time in milliseconds for parsing all objects
            int recordCount = BsaFile.Count;
            long startTicks = DateTime.Now.Ticks;
            for (int i = 0; i < recordCount; i++)
            {
                LoadRecord(i);
            }
            long elapsedTicks = (DateTime.Now.Ticks - startTicks);

            return (int)elapsedTicks / 10000;
        }

        /// <summary>
        /// Get a DFMesh representation of a record.
        /// </summary>
        /// <param name="Record">Index of record to load.</param>
        /// <returns>DFMesh object.</returns>
        public DFMesh GetMesh(int Record)
        {
            // Load the record
            if (!LoadRecord(Record))
                return new DFMesh();

            return Records[Record].DFMesh;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Convert a 3d point to a 2d point for GetPreview() methods.
        /// </summary>
        /// <param name="Point">Source 3D point.</param>
        /// <param name="Min">Minimum vector positions (used to centre render in viewport).</param>
        /// <param name="Max">Maximum vector positions (used to centre render in viewport).</param>
        /// <param name="ViewWidth">Width of viewport.</param>
        /// <param name="ViewHeight">Height of viewport.</param>
        /// <param name="RotateY">Amount to rotate around Y axis in degrees.</param>
        /// <returns>2D point for plotting in viewport</returns>
        private Point Get2dPoint(FaceUVTool.DFPurePoint Point, Vector3 Min, Vector3 Max, int ViewWidth, int ViewHeight, float RotateY)
        {
            // Get fractional point
            DFMesh.DFPoint point3d;
            point3d.X = Point.x >> 8;
            point3d.Y = Point.y >> 8;
            point3d.Z = Point.z >> 8;

            // Rotate around Y axis
            const double PIOVER180 = Math.PI / 180.0;
            double degrees = RotateY;
            double cDegrees = degrees * PIOVER180;
            double cosDegrees = Math.Cos(cDegrees);
            double sinDegrees = Math.Sin(cDegrees);
            double xrot = (point3d.X * cosDegrees) + (point3d.Z * sinDegrees);
            double zrot = (point3d.X * -sinDegrees) + (point3d.Z * cosDegrees);
            point3d.X = (float)xrot;
            point3d.Z = (float)zrot;

            // Centre vector
            float transX = (float)(Min.X + ((Max.X - Min.X) / 2));
            float transY = (float)(Min.Y + ((Max.Y - Min.Y) / 2));
            float transZ = (float)(Min.Z + ((Max.Z - Min.Z) / 2));
            point3d.X -= transX;
            point3d.Y -= transY;
            point3d.Z -= transZ;

            // Scale vector (large objects become smaller, small objects grow larger)
            Vector3 size = new Vector3(Max.X - Min.X, Max.Y - Min.Y, Max.Z - Min.Z);
            float scale = 1000.0f / (float)((size.X + size.Y + size.Z) / 3);
            point3d.X *= scale;
            point3d.Y *= scale;
            point3d.Z *= scale;

            // Set camera position and zoom
            float zoom = (float)(ViewWidth / 0.5);
            float camx = 0;
            float camy = 0;
            float camz = (float)(Max.Z - 4500);

            // Translate point into 2d space
            float z = point3d.Z - camz;
            Point point2d = new Point();
            point2d.X = ViewWidth / 2 - (int)((camx - point3d.X) / z * zoom);
            point2d.Y = ViewHeight /2 - (int)((camy - point3d.Y) / z * zoom);

            return point2d;
        }

        /// <summary>
        /// Convert a string to member of meshVersions enumeration.
        /// </summary>
        /// <param name="Version">Version of mesh as string</param>
        /// <returns>Member of meshVersions enumeration</returns>
        private MeshVersions GetVersion(string Version)
        {
            if (Version == "v2.7")
                return MeshVersions.Version27;
            else if (Version == "v2.6")
                return MeshVersions.Version26;
            else if (Version == "v2.5")
                return MeshVersions.Version25;
            else
                return MeshVersions.Unknown;
        }

        #endregion

        #region Readers

        /// <summary>
        /// Read and decompose a mesh record.
        /// </summary>
        /// <param name="Record">The record index to read.</param>
        /// <returns>True if successful, otherwise false</returns>
        private bool Read(int Record)
        {
            try
            {
                // Read header
                BinaryReader reader = Records[Record].MemoryFile.GetReader();
                ReadHeader(reader, Record);

                // Store name and version
                Records[Record].ObjectId = BsaFile.GetRecordId(Record);
                Records[Record].Version = GetVersion(Records[Record].Header.Version);

                // Read mesh
                if (!ReadMesh(Record))
                    return false;

                // Decompose this mesh
                if (!DecomposeMesh(Record))
                    return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read ARCH3D.BSA file header.
        /// </summary>
        /// <param name="Reader">A binary reader to file.</param>
        /// <param name="Record">Destination record index.</param>
        private void ReadHeader(BinaryReader Reader, int Record)
        {
            Reader.BaseStream.Position = 0;
            Records[Record].Header.Position = 0;
            Records[Record].Header.Version = Records[Record].MemoryFile.ReadCString(Reader, 4);
            Records[Record].Header.PointCount = Reader.ReadInt32();
            Records[Record].Header.PlaneCount = Reader.ReadInt32();
            Records[Record].Header.Unknown1 = Reader.ReadUInt32();
            Records[Record].Header.NullValue1 = Reader.ReadUInt64();
            Records[Record].Header.PlaneDataOffset = Reader.ReadInt32();
            Records[Record].Header.ObjectDataOffset = Reader.ReadInt32();
            Records[Record].Header.ObjectDataCount = Reader.ReadInt32();
            Records[Record].Header.Unknown2 = Reader.ReadUInt32();
            Records[Record].Header.NullValue2 = Reader.ReadUInt64();
            Records[Record].Header.PointListOffset = Reader.ReadInt32();
            Records[Record].Header.NormalListOffset = Reader.ReadInt32();
            Records[Record].Header.Unknown3 = Reader.ReadUInt32();
            Records[Record].Header.PlaneListOffset = Reader.ReadInt32();
        }

        /// <summary>
        /// Read mesh data to record array.
        /// </summary>
        /// <param name="Record">Destination record index.</param>
        private bool ReadMesh(int Record)
        {
            // Create empty mesh
            Records[Record].PureMesh = new PureMesh();

            // Create plane array
            int faceCount = Records[Record].Header.PlaneCount;
            Records[Record].PureMesh.Planes = new PurePlane[faceCount];

            // Get reader for normal data
            long normalPosition = Records[Record].Header.NormalListOffset;
            BinaryReader normalReader = Records[Record].MemoryFile.GetReader(normalPosition);

            // Read native data into plane array
            int uniqueTextureCount = 0;
            MeshVersions version = Records[Record].Version;
            long position = Records[Record].Header.PlaneListOffset;
            BinaryReader reader = Records[Record].MemoryFile.GetReader(position);
            BinaryReader pointReader = Records[Record].MemoryFile.GetReader();
            BinaryReader planeDataReader = Records[Record].MemoryFile.GetReader();
            for (int plane = 0; plane < faceCount; plane++)
            {
                // Read plane header
                Records[Record].PureMesh.Planes[plane].Header.Position = reader.BaseStream.Position;
                Records[Record].PureMesh.Planes[plane].Header.PlanePointCount = reader.ReadByte();
                Records[Record].PureMesh.Planes[plane].Header.Unknown1 = reader.ReadByte();
                Records[Record].PureMesh.Planes[plane].Header.Texture = reader.ReadUInt16();
                Records[Record].PureMesh.Planes[plane].Header.Unknown2 = reader.ReadUInt32();

                // Read the normal data for this plane
                Int32 nx = normalReader.ReadInt32();
                Int32 ny = normalReader.ReadInt32();
                Int32 nz = normalReader.ReadInt32();

                // Build list of unique textures across all planes - this will be used later to create submesh buffers
                UInt16 textureBitfield = Records[Record].PureMesh.Planes[plane].Header.Texture;
                int textureArchive = textureBitfield >> 7;
                int textureRecord = textureBitfield & 0x7f;
                bool foundTexture = false;
                for (int i = 0; i < uniqueTextureCount; i++)
                {
                    if (UniqueTextureBuffer[i].Archive == textureArchive && UniqueTextureBuffer[i].Record == textureRecord)
                    {
                        foundTexture = true;
                        break;
                    }
                }
                if (!foundTexture)
                {
                    UniqueTextureBuffer[uniqueTextureCount].Archive = textureArchive;
                    UniqueTextureBuffer[uniqueTextureCount].Record = textureRecord;
                    uniqueTextureCount++;
                }
                
                // Store texture index for this plane
                Records[Record].PureMesh.Planes[plane].TextureIndex.Archive = textureArchive;
                Records[Record].PureMesh.Planes[plane].TextureIndex.Record = textureRecord;

                // Read plane points
                int pointCount = Records[Record].PureMesh.Planes[plane].Header.PlanePointCount;
                Records[Record].PureMesh.Planes[plane].Points = new FaceUVTool.DFPurePoint[pointCount];
                for (int point = 0; point < pointCount; point++)
                {
                    // Read planePoint data
                    int pointOffset = reader.ReadInt32();
                    Records[Record].PureMesh.Planes[plane].Points[point].u = reader.ReadInt16();
                    Records[Record].PureMesh.Planes[plane].Points[point].v = reader.ReadInt16();

                    // Get point position
                    long pointPosition = Records[Record].Header.PointListOffset;
                    switch (version)
                    {
                        case MeshVersions.Version27:
                        case MeshVersions.Version26:
                            pointPosition += pointOffset;
                            break;
                        case MeshVersions.Version25:
                            pointPosition += (pointOffset * 3);
                            break;
                    }

                    // Store native point values
                    pointReader.BaseStream.Position = pointPosition;
                    Records[Record].PureMesh.Planes[plane].Points[point].x = pointReader.ReadInt32();
                    Records[Record].PureMesh.Planes[plane].Points[point].y = pointReader.ReadInt32();
                    Records[Record].PureMesh.Planes[plane].Points[point].z = pointReader.ReadInt32();

                    // Store native normal values for each vertex
                    Records[Record].PureMesh.Planes[plane].Points[point].nx = nx;
                    Records[Record].PureMesh.Planes[plane].Points[point].ny = ny;
                    Records[Record].PureMesh.Planes[plane].Points[point].nz = nz;
                }

                // Read unknown plane data
                planeDataReader.BaseStream.Position = Records[Record].Header.PlaneDataOffset + plane * 24;
                Records[Record].PureMesh.Planes[plane].PlaneData = planeDataReader.ReadBytes(24);
            }

            // CURRENTLY UNUSED - Read start of object data
            //if (records[record].header.objectDataCount > 0)
            //{
            //    // Read first object data record only (format is not yet known enough to enumerate)
            //    // This data is only loaded temporarily to look at for now
            //    reader.BaseStream.Position = records[record].header.objectDataOffset;

            //    // Read number array
            //    objectDataHeader dataHeader = new objectDataHeader();
            //    dataHeader.numbers = new Int32[4];
            //    for (int i = 0; i < 4; i++)
            //    {
            //        dataHeader.numbers[i] = reader.ReadInt32();
            //    }

            //    // Read sub-record count
            //    dataHeader.subrecordCount = reader.ReadInt16();
            //}

            // Copy valid part of unique texture list into pureMesh data and create plane buffer for decomposition
            Records[Record].PureMesh.UniqueTextures = new TextureIndex[uniqueTextureCount];
            for (int i = 0; i < uniqueTextureCount; i++)
            {
                Records[Record].PureMesh.UniqueTextures[i] = UniqueTextureBuffer[i];
                SubMeshBuffer[i].TextureArchive = UniqueTextureBuffer[i].Archive;
                SubMeshBuffer[i].TextureRecord = UniqueTextureBuffer[i].Record;
                SubMeshBuffer[i].planeCount = 0;
                SubMeshBuffer[i].PlaneBuffer = new DFPlaneBuffer[PlaneBufferLength];
            }

            return true;
        }

        /// <summary>
        /// Decompose pure mesh into submeshes grouped by texture and containing a triangle-friendly point strip per plane.
        /// </summary>
        /// <param name="Record">Destination record index.</param>
        private bool DecomposeMesh(int Record)
        {
            // Create mesh and submesh records
            int uniquetextureCount = Records[Record].PureMesh.UniqueTextures.Length;
            Records[Record].DFMesh = new DFMesh();
            Records[Record].DFMesh.SubMeshes = new DFMesh.DFSubMesh[uniquetextureCount];
            Records[Record].DFMesh.ObjectId = Records[Record].ObjectId;

            // Decompose each plane of this mesh into a buffer
            int planeCount = Records[Record].PureMesh.Planes.Length;
            for (int plane = 0; plane < planeCount; plane++)
            {
                // Determine which submesh group this plane belongs to based on texture
                int subMeshIndex = GetSubMesh(ref Records[Record].PureMesh, Records[Record].PureMesh.Planes[plane].TextureIndex.Archive, Records[Record].PureMesh.Planes[plane].TextureIndex.Record);

                // Decompose plane into a strip based on number of points
                WritePlane(subMeshIndex, ref Records[Record].PureMesh.Planes[plane]);
            }

            // Copy valid mesh data from buffer into mesh record
            int totalTriangles = 0;
            for (int submesh = 0; submesh < uniquetextureCount; submesh++)
            {
                // Store texture information
                Records[Record].DFMesh.SubMeshes[submesh].TextureArchive = SubMeshBuffer[submesh].TextureArchive;
                Records[Record].DFMesh.SubMeshes[submesh].TextureRecord = SubMeshBuffer[submesh].TextureRecord;

                // Store plane data
                int bufferPlaneCount = SubMeshBuffer[submesh].planeCount;
                Records[Record].DFMesh.SubMeshes[submesh].Planes = new DFMesh.DFPlane[bufferPlaneCount];
                for (int plane = 0; plane < bufferPlaneCount; plane++)
                {
                    // Store point data for this plane
                    int bufferPointCount = SubMeshBuffer[submesh].PlaneBuffer[plane].PointCount;
                    Records[Record].DFMesh.TotalVertices += bufferPointCount;
                    Records[Record].DFMesh.SubMeshes[submesh].TotalTriangles += bufferPointCount - 2;
                    Records[Record].DFMesh.SubMeshes[submesh].Planes[plane].Points = new DFMesh.DFPoint[bufferPointCount];
                    for (int point = 0; point < bufferPointCount; point++)
                    {
                        Records[Record].DFMesh.SubMeshes[submesh].Planes[plane].Points[point] = SubMeshBuffer[submesh].PlaneBuffer[plane].PointBuffer[point];
                        Records[Record].DFMesh.SubMeshes[submesh].Planes[plane].UVGenerationMethod = SubMeshBuffer[submesh].PlaneBuffer[plane].UVGenerationMethod;
                    }
                }

                // Increment total triangle for this submesh
                totalTriangles += Records[Record].DFMesh.SubMeshes[submesh].TotalTriangles;
            }

            // Store total triangle across whole mesh
            Records[Record].DFMesh.TotalTriangles = totalTriangles;

            return true;
        }

        /// <summary>
        /// Write points of a plane.
        /// </summary>
        /// <param name="SubMeshIndex">Index of the submesh (texture group) to work with.</param>
        /// <param name="PlaneIn">Source plane.</param>
        private int WritePlane(int SubMeshIndex, ref PurePlane PlaneIn)
        {
            // Handle planes with greater than 3 points
            if (PlaneIn.Points.Length > 3)
                return WriteVariablePlane(SubMeshIndex, ref PlaneIn);

            // Add new point buffer to submesh buffer
            int planeIndex = SubMeshBuffer[SubMeshIndex].planeCount;
            SubMeshBuffer[SubMeshIndex].PlaneBuffer[planeIndex].PointBuffer = new DFMesh.DFPoint[PointBufferLength];
            SubMeshBuffer[SubMeshIndex].planeCount++;

            // Calculate UV coordinates for points 1, 2 (points 1 & 2 are deltas that are added to the previous point)
            PlaneIn.Points[1].u += PlaneIn.Points[0].u;
            PlaneIn.Points[1].v += PlaneIn.Points[0].v;
            PlaneIn.Points[2].u += PlaneIn.Points[1].u;
            PlaneIn.Points[2].v += PlaneIn.Points[1].v;

            // Write the 3 points
            WritePoint(ref PlaneIn.Points[0], ref SubMeshBuffer[SubMeshIndex].PlaneBuffer[planeIndex]);
            WritePoint(ref PlaneIn.Points[1], ref SubMeshBuffer[SubMeshIndex].PlaneBuffer[planeIndex]);
            WritePoint(ref PlaneIn.Points[2], ref SubMeshBuffer[SubMeshIndex].PlaneBuffer[planeIndex]);

            // Store UV generation method of this plane
            SubMeshBuffer[SubMeshIndex].PlaneBuffer[planeIndex].UVGenerationMethod = DFMesh.UVGenerationMethods.TriangleOnly;

            return planeIndex;
        }

        /// <summary>
        /// Write a N-point triangle fan to buffer by finding corners.
        /// </summary>
        /// <param name="SubMeshIndex">Index of the submesh (texture group) to work with.</param>
        /// <param name="PlaneIn">Source plane.</param>
        private int WriteVariablePlane(int SubMeshIndex, ref PurePlane PlaneIn)
        {
            // Add new point buffer to submesh buffer
            int planeIndex = SubMeshBuffer[SubMeshIndex].planeCount;
            SubMeshBuffer[SubMeshIndex].PlaneBuffer[planeIndex].PointBuffer = new DFMesh.DFPoint[PointBufferLength];
            SubMeshBuffer[SubMeshIndex].planeCount++;

            // Find corner points
            int cornerCount = GetCornerPoints(ref PlaneIn.Points);

            // Calculate UV coordinates of all points
            if (FaceUVTool.ComputeFaceUVCoordinates(ref PlaneIn.Points, ref CalculatedUVBuffer))
            {
                // Copy calculated UV coordinates
                for (int pt = 0; pt < PlaneIn.Points.Length; pt++)
                {
                    PlaneIn.Points[pt].u = CalculatedUVBuffer[pt].u;
                    PlaneIn.Points[pt].v = CalculatedUVBuffer[pt].v;
                }

                // Store UV generation method of this plane
                SubMeshBuffer[SubMeshIndex].PlaneBuffer[planeIndex].UVGenerationMethod = DFMesh.UVGenerationMethods.ModifedMatrixGenerator;
            }

            // Write first 3 points
            int cornerPos = 0;
            WritePoint(ref PlaneIn.Points[CornerPointBuffer[cornerPos++]], ref SubMeshBuffer[SubMeshIndex].PlaneBuffer[planeIndex]);
            WritePoint(ref PlaneIn.Points[CornerPointBuffer[cornerPos++]], ref SubMeshBuffer[SubMeshIndex].PlaneBuffer[planeIndex]);
            WritePoint(ref PlaneIn.Points[CornerPointBuffer[cornerPos++]], ref SubMeshBuffer[SubMeshIndex].PlaneBuffer[planeIndex]);

            // Write remaining points
            while (cornerPos < cornerCount)
            {
                WritePoint(ref PlaneIn.Points[CornerPointBuffer[cornerPos++]], ref SubMeshBuffer[SubMeshIndex].PlaneBuffer[planeIndex]);
            }

            return planeIndex;
        }

        /// <summary>
        /// Write a single point to buffer.
        /// </summary>
        /// <param name="SrcPoint">Source point.</param>
        /// <param name="DstPlane">Destination plane buffer.</param>
        /// <implementation>
        /// Vector coordinates are divided by 256.0f, and texture coordinates by 16.0f.
        /// </implementation>
        private int WritePoint(ref FaceUVTool.DFPurePoint SrcPoint, ref DFPlaneBuffer DstPlane)
        {
            // Copy point data
            int pointPos = DstPlane.PointCount;
            DstPlane.PointBuffer[pointPos].X = SrcPoint.x / PointDivisor;
            DstPlane.PointBuffer[pointPos].Y = SrcPoint.y / PointDivisor;
            DstPlane.PointBuffer[pointPos].Z = SrcPoint.z / PointDivisor;
            DstPlane.PointBuffer[pointPos].NX = SrcPoint.nx / PointDivisor;
            DstPlane.PointBuffer[pointPos].NY = SrcPoint.ny / PointDivisor;
            DstPlane.PointBuffer[pointPos].NZ = SrcPoint.nz / PointDivisor;
            DstPlane.PointBuffer[pointPos].U = SrcPoint.u / TextureDivisor;
            DstPlane.PointBuffer[pointPos].V = SrcPoint.v / TextureDivisor;
            DstPlane.PointCount++;

            return pointPos;
        }

        /// <summary>
        /// Find submesh this archive/record combo will belong.
        /// </summary>
        /// <param name="Mesh">Source mesh to search.</param>
        /// <param name="TextureArchive">Texture archive value to match.</param>
        /// <param name="TextureRecord">Texture index value to match.</param>
        /// <returns>Index of submesh matching this texture.</returns>
        private int GetSubMesh(ref PureMesh Mesh, int TextureArchive, int TextureRecord)
        {
            for (int i = 0; i < Mesh.UniqueTextures.Length; i++)
            {
                if (Mesh.UniqueTextures[i].Archive == TextureArchive && Mesh.UniqueTextures[i].Record == TextureRecord)
                    return i;
            }

            throw new Exception("GetSubMesh() index not found.");
        }

        /// <summary>
        /// Find corner points from a pure face - this reduces the number of points in the final strip.
        /// </summary>
        /// <param name="PointsIn">Source points to find corners of.</param>
        /// <returns>Number of corners found in this point array.</returns>
        private int GetCornerPoints(ref FaceUVTool.DFPurePoint[] PointsIn)
        {
            int cornerCount = 0;
            int maxCorners = CornerPointBuffer.Length;
            Vector3 v0, v1, v2, l0, l1;
            int pointCount = PointsIn.Length;
            for (int point = 0; point < pointCount; point++)
            {
                // Determine angle between this point and next two points
                int cornerIndex;
                if (point < pointCount - 2)
                {
                    v0 = new Vector3(PointsIn[point].x, PointsIn[point].y, PointsIn[point].z);
                    v1 = new Vector3(PointsIn[point + 1].x, PointsIn[point + 1].y, PointsIn[point + 1].z);
                    v2 = new Vector3(PointsIn[point + 2].x, PointsIn[point + 2].y, PointsIn[point + 2].z);
                    cornerIndex = point + 1;
                }
                else if (point < pointCount - 1)
                {
                    v0 = new Vector3(PointsIn[point].x, PointsIn[point].y, PointsIn[point].z);
                    v1 = new Vector3(PointsIn[point + 1].x, PointsIn[point + 1].y, PointsIn[point + 1].z);
                    v2 = new Vector3(PointsIn[0].x, PointsIn[0].y, PointsIn[0].z);
                    cornerIndex = point + 1;
                }
                else
                {
                    v0 = new Vector3(PointsIn[point].x, PointsIn[point].y, PointsIn[point].z);
                    v1 = new Vector3(PointsIn[0].x, PointsIn[0].y, PointsIn[0].z);
                    v2 = new Vector3(PointsIn[1].x, PointsIn[1].y, PointsIn[1].z);
                    cornerIndex = 0;
                }

                // Construct direction vectors
                l0 = v1 - v0;
                l1 = v2 - v0;

                // Check angle between direction vectors
                double angle = l0.Angle(l1);
                if (angle > 0.001f)
                {
                    // Write corner point to buffer
                    CornerPointBuffer[cornerCount++] = cornerIndex;
                }
            }

            return cornerCount;
        }

        #endregion
    }
}
