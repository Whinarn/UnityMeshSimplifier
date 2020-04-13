#region License
/*
MIT License

Copyright(c) 2017-2020 Mattias Edlund

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

namespace UnityMeshSimplifier.Internal
{
    internal struct Triangle : IEquatable<Triangle>
    {
        #region Fields
        public int index;

        public int v0;
        public int v1;
        public int v2;
        public int subMeshIndex;

        public int va0;
        public int va1;
        public int va2;

        public double err0;
        public double err1;
        public double err2;
        public double err3;

        public bool deleted;
        public bool dirty;
        public Vector3d n;
        #endregion

        #region Properties
        public int this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (index == 0 ? v0 : (index == 1 ? v1 : v2));
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                switch (index)
                {
                    case 0:
                        v0 = value;
                        break;
                    case 1:
                        v1 = value;
                        break;
                    case 2:
                        v2 = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
        #endregion

        #region Constructor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Triangle(int index, int v0, int v1, int v2, int subMeshIndex)
        {
            this.index = index;

            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.subMeshIndex = subMeshIndex;

            this.va0 = v0;
            this.va1 = v1;
            this.va2 = v2;

            err0 = err1 = err2 = err3 = 0;
            deleted = dirty = false;
            n = new Vector3d();
        }
        #endregion

        #region Public Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetAttributeIndices(int[] attributeIndices)
        {
            attributeIndices[0] = va0;
            attributeIndices[1] = va1;
            attributeIndices[2] = va2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAttributeIndex(int index, int value)
        {
            switch (index)
            {
                case 0:
                    va0 = value;
                    break;
                case 1:
                    va1 = value;
                    break;
                case 2:
                    va2 = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetErrors(double[] err)
        {
            err[0] = err0;
            err[1] = err1;
            err[2] = err2;
        }

        public override int GetHashCode()
        {
            return index;
        }

        public override bool Equals(object obj)
        {
            if (obj is Triangle)
            {
                var other = (Triangle)obj;
                return index == other.index;
            }

            return false;
        }

        public bool Equals(Triangle other)
        {
            return index == other.index;
        }
        #endregion
    }
}
