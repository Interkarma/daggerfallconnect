// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace DeepEngine.Rendering
{

    #region VertexPositionNormalTextureBump

    /// <summary>
    /// Position, Normal, TextureCoordinate, Tangent, Bitangent (binormal).
    /// </summary>
    public struct VertexPositionNormalTextureBump : IVertexType
    {
        Vector3 pos;
        Vector3 normal;
        Vector2 tex;
        Vector3 tan;
        Vector3 bitan;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(44, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0));

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        public VertexPositionNormalTextureBump(
            Vector3 Position,
            Vector3 Normal,
            Vector2 TextureCoordinate,
            Vector3 Tangent,
            Vector3 Binormal)
        {
            this.pos = Position;
            this.normal = Normal;
            this.tex = TextureCoordinate;
            this.tan = Tangent;
            this.bitan = Binormal;
        }

        public static bool operator !=(VertexPositionNormalTextureBump left, VertexPositionNormalTextureBump right)
        {
            return left.GetHashCode() != right.GetHashCode();
        }

        public static bool operator ==(VertexPositionNormalTextureBump left, VertexPositionNormalTextureBump right)
        {
            return left.GetHashCode() == right.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this == (VertexPositionNormalTextureBump)obj;
        }

        public Vector3 Position { get { return pos; } set { pos = value; } }
        public Vector3 Normal { get { return normal; } set { normal = value; } }
        public Vector2 TextureCoordinate { get { return tex; } set { tex = value; } }
        public Vector3 Tangent { get { return tan; } set { tan = value; } }
        public Vector3 Binormal { get { return bitan; } set { bitan = value; } }

        public static int SizeInBytes { get { return 56; } }

        public override int GetHashCode()
        {
            return pos.GetHashCode() | tex.GetHashCode() | normal.GetHashCode() | tan.GetHashCode() | bitan.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", pos.X, pos.Y, pos.Z);
        }
    }

    #endregion

}
