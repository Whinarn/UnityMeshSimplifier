#region License
/*
MIT License

Copyright(c) 2017 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// A double precision 3D vector.
    /// </summary>
    public struct Vector3d : IEquatable<Vector3d>
    {
        #region Static Read-Only
        /// <summary>
        /// The zero vector.
        /// </summary>
        public static readonly Vector3d zero = new Vector3d(0, 0, 0);
        #endregion

        #region Consts
        /// <summary>
        /// The vector epsilon.
        /// </summary>
        public const double Epsilon = double.Epsilon;
        #endregion

        #region Fields
        /// <summary>
        /// The x component.
        /// </summary>
        public double x;
        /// <summary>
        /// The y component.
        /// </summary>
        public double y;
        /// <summary>
        /// The z component.
        /// </summary>
        public double z;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the magnitude of this vector.
        /// </summary>
        public double Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return System.Math.Sqrt(x * x + y * y + z * z); }
        }

        /// <summary>
        /// Gets the squared magnitude of this vector.
        /// </summary>
        public double MagnitudeSqr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (x * x + y * y + z * z); }
        }

        /// <summary>
        /// Gets a normalized vector from this vector.
        /// </summary>
        public Vector3d Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Vector3d result;
                Normalize(ref this, out result);
                return result;
            }
        }

        /// <summary>
        /// Gets or sets a specific component by index in this vector.
        /// </summary>
        /// <param name="index">The component index.</param>
        public double this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3d index!");
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3d index!");
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new vector with one value for all components.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3d(double value)
        {
            this.x = value;
            this.y = value;
            this.z = value;
        }

        /// <summary>
        /// Creates a new vector.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <param name="z">The z value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Creates a new vector from a single precision vector.
        /// </summary>
        /// <param name="vector">The single precision vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3d(Vector3 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
        }
        #endregion

        #region Operators
        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The resulting vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The resulting vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        /// <summary>
        /// Scales the vector uniformly.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="d">The scaling value.</param>
        /// <returns>The resulting vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator *(Vector3d a, double d)
        {
            return new Vector3d(a.x * d, a.y * d, a.z * d);
        }

        /// <summary>
        /// Scales the vector uniformly.
        /// </summary>
        /// <param name="d">The scaling vlaue.</param>
        /// <param name="a">The vector.</param>
        /// <returns>The resulting vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator *(double d, Vector3d a)
        {
            return new Vector3d(a.x * d, a.y * d, a.z * d);
        }

        /// <summary>
        /// Divides the vector with a float.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="d">The dividing float value.</param>
        /// <returns>The resulting vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator /(Vector3d a, double d)
        {
            return new Vector3d(a.x / d, a.y / d, a.z / d);
        }

        /// <summary>
        /// Subtracts the vector from a zero vector.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <returns>The resulting vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator -(Vector3d a)
        {
            return new Vector3d(-a.x, -a.y, -a.z);
        }

        /// <summary>
        /// Returns if two vectors equals eachother.
        /// </summary>
        /// <param name="lhs">The left hand side vector.</param>
        /// <param name="rhs">The right hand side vector.</param>
        /// <returns>If equals.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3d lhs, Vector3d rhs)
        {
            return (lhs - rhs).MagnitudeSqr < Epsilon;
        }

        /// <summary>
        /// Returns if two vectors don't equal eachother.
        /// </summary>
        /// <param name="lhs">The left hand side vector.</param>
        /// <param name="rhs">The right hand side vector.</param>
        /// <returns>If not equals.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3d lhs, Vector3d rhs)
        {
            return (lhs - rhs).MagnitudeSqr >= Epsilon;
        }

        /// <summary>
        /// Implicitly converts from a single-precision vector into a double-precision vector.
        /// </summary>
        /// <param name="v">The single-precision vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector3d(Vector3 v)
        {
            return new Vector3d(v.x, v.y, v.z);
        }

        /// <summary>
        /// Implicitly converts from a double-precision vector into a single-precision vector.
        /// </summary>
        /// <param name="v">The double-precision vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(Vector3d v)
        {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }
        #endregion

        #region Public Methods
        #region Instance
        /// <summary>
        /// Set x, y and z components of an existing vector.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <param name="z">The z value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Multiplies with another vector component-wise.
        /// </summary>
        /// <param name="scale">The vector to multiply with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(ref Vector3d scale)
        {
            x *= scale.x;
            y *= scale.y;
            z *= scale.z;
        }

        /// <summary>
        /// Normalizes this vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            double mag = this.Magnitude;
            if (mag > Epsilon)
            {
                x /= mag;
                y /= mag;
                z /= mag;
            }
            else
            {
                x = y = z = 0;
            }
        }

        /// <summary>
        /// Clamps this vector between a specific range.
        /// </summary>
        /// <param name="min">The minimum component value.</param>
        /// <param name="max">The maximum component value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clamp(double min, double max)
        {
            if (x < min) x = min;
            else if (x > max) x = max;

            if (y < min) y = min;
            else if (y > max) y = max;

            if (z < min) z = min;
            else if (z > max) z = max;
        }
        #endregion

        #region Object
        /// <summary>
        /// Returns a hash code for this vector.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
        }

        /// <summary>
        /// Returns if this vector is equal to another one.
        /// </summary>
        /// <param name="other">The other vector to compare to.</param>
        /// <returns>If equals.</returns>
        public override bool Equals(object other)
        {
            if (!(other is Vector3d))
            {
                return false;
            }
            Vector3d vector = (Vector3d)other;
            return (x == vector.x && y == vector.y && z == vector.z);
        }

        /// <summary>
        /// Returns if this vector is equal to another one.
        /// </summary>
        /// <param name="other">The other vector to compare to.</param>
        /// <returns>If equals.</returns>
        public bool Equals(Vector3d other)
        {
            return (x == other.x && y == other.y && z == other.z);
        }

        /// <summary>
        /// Returns a nicely formatted string for this vector.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1})", x, y, z);
        }

        /// <summary>
        /// Returns a nicely formatted string for this vector.
        /// </summary>
        /// <param name="format">The float format.</param>
        /// <returns>The string.</returns>
        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2})", x.ToString(format), y.ToString(format), z.ToString(format));
        }
        #endregion

        #region Static
        /// <summary>
        /// Dot Product of two vectors.
        /// </summary>
        /// <param name="lhs">The left hand side vector.</param>
        /// <param name="rhs">The right hand side vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(ref Vector3d lhs, ref Vector3d rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        /// <summary>
        /// Cross Product of two vectors.
        /// </summary>
        /// <param name="lhs">The left hand side vector.</param>
        /// <param name="rhs">The right hand side vector.</param>
        /// <param name="result">The resulting vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cross(ref Vector3d lhs, ref Vector3d rhs, out Vector3d result)
        {
            result = new Vector3d(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
        }

        /// <summary>
        /// Calculates the angle between two vectors.
        /// </summary>
        /// <param name="from">The from vector.</param>
        /// <param name="to">The to vector.</param>
        /// <returns>The angle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Angle(ref Vector3d from, ref Vector3d to)
        {
            Vector3d fromNormalized = from.Normalized;
            Vector3d toNormalized = to.Normalized;
            return System.Math.Acos(MathHelper.Clamp(Vector3d.Dot(ref fromNormalized, ref toNormalized), -1.0, 1.0)) * MathHelper.Rad2Degd;
        }

        /// <summary>
        /// Performs a linear interpolation between two vectors.
        /// </summary>
        /// <param name="a">The vector to interpolate from.</param>
        /// <param name="b">The vector to interpolate to.</param>
        /// <param name="t">The time fraction.</param>
        /// <param name="result">The resulting vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Vector3d a, ref Vector3d b, double t, out Vector3d result)
        {
            result = new Vector3d(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        /// <summary>
        /// Multiplies two vectors component-wise.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <param name="result">The resulting vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Scale(ref Vector3d a, ref Vector3d b, out Vector3d result)
        {
            result = new Vector3d(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        /// <summary>
        /// Normalizes a vector.
        /// </summary>
        /// <param name="value">The vector to normalize.</param>
        /// <param name="result">The resulting normalized vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Normalize(ref Vector3d value, out Vector3d result)
        {
            double mag = value.Magnitude;
            if (mag > Epsilon)
            {
                result = new Vector3d(value.x / mag, value.y / mag, value.z / mag);
            }
            else
            {
                result = Vector3d.zero;
            }
        }
        #endregion
        #endregion
    }
}