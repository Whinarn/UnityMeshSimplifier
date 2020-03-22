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
    internal struct Vertex : IEquatable<Vertex>
    {
        public int index;
        public Vector3d p;
        public int tstart;
        public int tcount;
        public SymmetricMatrix q;
        public bool borderEdge;
        public bool uvSeamEdge;
        public bool uvFoldoverEdge;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vertex(int index, Vector3d p)
        {
            this.index = index;
            this.p = p;
            this.tstart = 0;
            this.tcount = 0;
            this.q = new SymmetricMatrix();
            this.borderEdge = true;
            this.uvSeamEdge = false;
            this.uvFoldoverEdge = false;
        }

        public override int GetHashCode()
        {
            return index;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vertex)
            {
                var other = (Vertex)obj;
                return index == other.index;
            }

            return false;
        }

        public bool Equals(Vertex other)
        {
            return index == other.index;
        }
    }
}
