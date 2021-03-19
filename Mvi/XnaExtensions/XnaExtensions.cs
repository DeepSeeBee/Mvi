using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.Enumerables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CharlyBeck.Mvi.XnaExtensions
{
    using CColoredVector3Dbl = Tuple<CVector3Dbl, CVector3Dbl>;
    using CTranslateAndRotate = Tuple<CVector3Dbl, CVector3Dbl>;

    public static class MatrixExtensions
    {
        public static Matrix ToRotateMatrixXyz(this Vector3 xyz)
            => Matrix.CreateRotationX(xyz.X) * Matrix.CreateRotationY(xyz.Y) * Matrix.CreateRotationZ(xyz.Z);
        public static Matrix ToRotateMatrixZyx(this Vector3 xyz)
            => Matrix.CreateRotationZ(xyz.Z) * Matrix.CreateRotationY(xyz.Y) * Matrix.CreateRotationX(xyz.X);
        public static Matrix ToTranslateMatrix(this Vector3 v)
            => Matrix.CreateTranslation(v);

        public static Vector3 Rotate(this Matrix m, Vector3 v) // https://stackoverflow.com/questions/12731704/vector3-matrix-multiplication
            => new Vector3((v.X * m[0, 0] + v.Y * m[0, 1] + v.Z * m[0, 2] + m[0, 3]),
                           (v.X * m[1, 0] + v.Y * m[1, 1] + v.Z * m[1, 2] + m[1, 3]),
                           (v.X * m[2, 0] + v.Y * m[2, 1] + v.Z * m[2, 2] + m[2, 3]));

        public static Vector3 RotateXyz(this Vector3 v, Vector3 aXyzAngle)
            => aXyzAngle.ToRotateMatrixXyz().Rotate(v);
        // => v.RotateX(aXyzAngle.X).RotateY(aXyzAngle.Y).RotateZ(aXyzAngle.Z);
        public static Vector3 RotateZyx(this Vector3 v, Vector3 aXyzAngle)
            => aXyzAngle.ToRotateMatrixZyx().Rotate(v);
        public static Vector3 RotateZ(this Vector3 aRotated, float aRadians)
            => Matrix.CreateRotationZ(aRadians).Rotate(aRotated);

        public static Vector3 RotateZ(this Vector3 aRotated, Vector3 aCenter, float aRadians)
            => (aRotated - aCenter).RotateZ(aRadians) + aCenter;

        public static Vector3 RotateY(this Vector3 aRotated, float aRadians)
            => Matrix.CreateRotationY(aRadians).Rotate(aRotated);

        public static Vector3 RotateY(this Vector3 aRotated, Vector3 aCenter, float aRadians)
            => (aRotated - aCenter).RotateY(aRadians) + aCenter;

        public static Vector3 RotateX(this Vector3 aRotated, float aRadians)
            => Matrix.CreateRotationX(aRadians).Rotate(aRotated);

        public static Vector3 RotateX(this Vector3 aRotated, Vector3 aCenter, float aRadians)
            => (aRotated - aCenter).RotateX(aRadians) + aCenter;        

        public static float GetLength(this Vector3 aPoint) // Not tested, https://www.engineeringtoolbox.com/distance-relationship-between-two-points-d_1854.html
            => (float)Math.Sqrt((aPoint.X * aPoint.X) + (aPoint.Y * aPoint.Y) + (aPoint.Z * aPoint.Z));

       public static Vector3 MakeLongerDelta(this Vector3 aVector, float aLength) // Not tested, https://www.freemathhelp.com/forum/threads/extend-length-of-line-in-3d-space-find-new-end-point.125160/
            => (new Vector3(aLength) / new Vector3(aVector.GetLength())) * aVector;

        public static float GetDistance(this Vector3 v1, Vector3 v2)
            => Vector3.Subtract(Vector3.Max(v1, v2), Vector3.Min(v1, v2)).GetLength();

        public static float Dot(this Vector3 lhs, Vector3 rhs)
            => lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z + rhs.Z;
        public static Vector3 MakeLonger(this Vector3 aVector, float aLength)
            => aVector + aVector.MakeLongerDelta(aLength);

        public static float GetRadiansXy(this Vector3 v)
            => (float)v.ToVector3Dbl().GetRadiansXy();

        public static float GetRadiansXz(this Vector3 v)
            => (float)v.ToVector3Dbl().GetRadiansXz();

        public static float GetRadiansYz(this Vector3 v)
            => (float)v.ToVector3Dbl().GetRadiansYz();

        public static float ToDegrees(this float aRadians)
            => MathHelper.ToDegrees(aRadians);
        public static float ToRadians(this float aDegrees)
            => MathHelper.ToRadians(aDegrees);

        public static bool CircaEquals(this float lhs, float rhs, float aTolerance = 0.1f)
            => (Math.Max(lhs, rhs) - Math.Min(lhs, rhs)) <= aTolerance;

        public static IEnumerable<float> ToFloats(this Vector3 v)
        {
            yield return v.X;
            yield return v.Y;
            yield return v.Z;
        }
        public static IEnumerable<float> ToFloats(this Matrix m)
        {
            for (var r = 0; r < 4; ++r)
                for (var c = 0; c < 4; ++c)
                    yield return m[r, c];
        }

        public static T[][] GetSizedGroups<T>(this T[] aItems, int aLength)
            => (aItems.Length % aLength) == 0
            ? (from aIdx in Enumerable.Range(0, aItems.Length / aLength)
               select (from aIdx2 in Enumerable.Range(aIdx * aLength, aLength) select aItems[aIdx2]).ToArray()).ToArray()
            : throw new ArgumentException();

        public static Vector4[] ToVector4s(this float[] aFloats)
            => (from aItem in aFloats.GetSizedGroups(4) select new Vector4(aItem[0], aItem[1], aItem[2], aItem[3])).ToArray();

        public static Vector3[] ToVector3s(this float[] aFloats)
            => (from aItem in aFloats.GetSizedGroups(3) select new Vector3(aItem[0], aItem[1], aItem[2])).ToArray();

        public static Matrix[] ToMatrixs(this Vector4[] aVectors)
            => (from aVector in aVectors.GetSizedGroups(4) select new Matrix(aVector[0], aVector[1], aVector[2], aVector[3])).ToArray();

        public static Matrix[] ToMatrixs(this float[] aFloats)
            => (from aItem in aFloats.ToVector4s() select aItem).ToArray().ToMatrixs();
        public static void Write(this Stream s, Vector3 v)
            => s.Write(v.ToFloats().ToArray());
        public static void Write(this Stream s, float[] aFloats)
        {
            var aWriter = new BinaryWriter(s);
            foreach (var aFloat in aFloats)
                aWriter.Write((double)aFloat);
        }
        public static float[] ReadFloats(this Stream s, int aFloatCount)
        {
            var aReader = new BinaryReader(s);
            var aFloats = new float[aFloatCount];
            for (var aIdx = 0; aIdx < aFloatCount; ++aIdx)
                aFloats[aIdx] = (float)aReader.ReadDouble();
            return aFloats;
        }
        public static void Write(this Stream s, Matrix m)
            => s.Write(m.ToFloats().ToArray());
        public static Matrix ReadMatrix(this Stream s)
            => s.ReadFloats(16).ToMatrixs().Single();
        public static Vector3 ReadVector3(this Stream s)
            => s.ReadFloats(3).ToVector3s().Single();

        public static Matrix ToMatrix(IEnumerable<CTranslateAndRotate> aTranslateAndRotates)
        {
            throw new NotImplementedException();
        }
    }

    public static class CWorldPosExtensions
    {

        public static CVector3Dbl ToVector3Dbl(this Vector3 v)
            => new CVector3Dbl(v.X, v.Y, v.Z);

        //public static Vector3 ToVector3(this CVector3Dbl w)
        //    => new Vector3((float)w.x, (float)w.y, (float)w.z);
    }



    public static class CExtensions
    {
        public static Color ToColor(this Vector3 v)
            => new Color(v);

        public static IEnumerable<VertexPositionColor> ToVertexPositionColor(this IEnumerable<Vector3> aVector3s, Color aColor)
            => from aVector3 in aVector3s select new VertexPositionColor(aVector3, aColor);
        public static Vector3 ToVector3(this CVector3Dbl aVector)
            => new Vector3((float)aVector.x, (float)aVector.y, (float)aVector.z);
        public static CVector3Dbl InvertX(this CVector3Dbl v)
            => new CVector3Dbl(-v.x, v.y, v.z);
        public static CVector3Dbl InvertXz(this CVector3Dbl v)
        => new CVector3Dbl(-v.x, v.y, -v.z);
        public static CVector3Dbl InvertY(this CVector3Dbl v)
            => new CVector3Dbl(v.x, -v.y, v.z);
        public static CVector3Dbl InvertZ(this CVector3Dbl v)
            => new CVector3Dbl(v.x, v.y, -v.z);
        public static CVector3Dbl Invert(this CVector3Dbl v)
            => new CVector3Dbl(-v.x, -v.y, -v.z);
        public static IEnumerable<Vector3> ToVector3s(this IEnumerable<CVector3Dbl> aCoordinates)
            => from aCoordinate in aCoordinates select ToVector3(aCoordinate);
        public static IEnumerable<VertexPosition> ToVertexPosition(this IEnumerable<Vector3> aVectors)
            => from aVector in aVectors select new VertexPosition(aVector);
        public static IEnumerable<VertexPosition> ToVertexPosition(this IEnumerable<CVector3Dbl> aCoordinates)
            => aCoordinates.ToVector3s().ToVertexPosition();
        public static IEnumerable<VertexPositionColor> ToVertexPositionColor(this IEnumerable<CColoredVector3Dbl> aItems)
            => from aItem in aItems select new VertexPositionColor(ToVector3(aItem.Item1), aItem.Item2.ToColor());

        public static Color ToColor(this CVector3Dbl v)
            => new Color(v.ToVector3());
        public static VertexBuffer ToVertexBuffer<T>(this IEnumerable<T> aItems, GraphicsDevice aGraphicDevice, BufferUsage aBufferUsage = BufferUsage.WriteOnly) where T : struct
        {
            var aItemArray = aItems.ToArray();
            var aVertexBuffer = new VertexBuffer(aGraphicDevice, typeof(T), aItemArray.Length, aBufferUsage);
            aVertexBuffer.SetData<T>(aItemArray);
            return aVertexBuffer;
        }
        public static VertexBuffer ToVertexPositionBuffer(this IEnumerable<CVector3Dbl> aItems, GraphicsDevice aGraphicDevice, BufferUsage aBufferUsage = BufferUsage.WriteOnly)
            => aItems.ToVertexPosition().ToVertexBuffer(aGraphicDevice, aBufferUsage);

        public static void DrawLineList(this VertexBuffer aVertexBuffer, GraphicsDevice aGraphicsDevice)
        {
            //aGraphicsDevice.SetVertexBuffer(aVertexBuffer);
            //var aCount = aVertexBuffer.VertexCount / 2;
            //for (var aIdx = 0; aIdx < aCount; ++aIdx)
            //{
            //    aGraphicsDevice.DrawPrimitives(PrimitiveType.LineList, aIdx * 2, 1);
            //}
            aGraphicsDevice.SetVertexBuffer(aVertexBuffer);
            aGraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, aVertexBuffer.VertexCount);
        }
        public static void DrawLineStrip(this VertexBuffer aVertexBuffer, GraphicsDevice aGraphicsDevice)
        {
            aGraphicsDevice.SetVertexBuffer(aVertexBuffer);
            var aCount = aVertexBuffer.VertexCount;
            //for (var aIdx = 0; aIdx < aCount; ++aIdx)
            //{
            aGraphicsDevice.DrawPrimitives(PrimitiveType.LineStrip, 0, aCount);
            //}
        }
        public static void DrawTriangleList(this VertexBuffer aVertexBuffer, GraphicsDevice aGraphicsDevice)
        {
            //aGraphicsDevice.SetVertexBuffer(aVertexBuffer);
            //var aCount = aVertexBuffer.VertexCount / 3;
            //for (var aIdx = 0; aIdx < aCount; ++aIdx)
            //{
            //    aGraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, aIdx * 3, 1);
            //}
            aGraphicsDevice.SetVertexBuffer(aVertexBuffer);
            aGraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, aVertexBuffer.VertexCount);
        }
        public static void DrawTriangleStrip(this VertexBuffer aVertexBuffer, GraphicsDevice aGraphicsDevice)
        {
            aGraphicsDevice.SetVertexBuffer(aVertexBuffer);
            var aCount = aVertexBuffer.VertexCount / 2;
            //for (var aIdx = 0; aIdx < aCount; ++aIdx)
            //{
                aGraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, aCount);
           // }
        }

        public static bool IsKeyDownExclusive(this KeyboardState aKeyboardState, Keys aKeyDown, params Keys[] aNotDowns)
        {
            if (aKeyboardState.IsKeyDown(aKeyDown))
            {
                foreach (var aNotDown in aNotDowns)
                    if (aKeyboardState.IsKeyDown(aNotDown))
                        return false;
                return true;
            }
            return false;
        }
        public static Color SetAlpha(this Color aColor, float aAlpha)
            => new Color(aColor, aAlpha);
    }



}

namespace CharlyBeck.Mvi.XnaExtensions
{
    public struct CBasicEffectMemento
    {
        public CBasicEffectMemento(BasicEffect aBasicEffect)
        {
            this.World = aBasicEffect.World;
            this.View = aBasicEffect.View;
            this.Projection = aBasicEffect.Projection;
            this.Alpha = aBasicEffect.Alpha;
        }

        private readonly Matrix World;
        private readonly Matrix View;
        private readonly Matrix Projection;
        private readonly float Alpha;

        public void Restore(BasicEffect aBasicEffect)
        {

        }
    }

}