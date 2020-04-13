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

using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityMeshSimplifier.Internal
{
    internal class BlendShapeContainer
    {
        private readonly string shapeName;
        private readonly BlendShapeFrameContainer[] frames;

        public BlendShapeContainer(BlendShape blendShape)
        {
            shapeName = blendShape.ShapeName;
            frames = new BlendShapeFrameContainer[blendShape.Frames.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new BlendShapeFrameContainer(blendShape.Frames[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveVertexElement(int dst, int src)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i].MoveVertexElement(dst, src);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InterpolateVertexAttributes(int dst, int i0, int i1, int i2, ref Vector3 barycentricCoord)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i].InterpolateVertexAttributes(dst, i0, i1, i2, ref barycentricCoord);
            }
        }

        public void Resize(int length, bool trimExess = false)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i].Resize(length, trimExess);
            }
        }

        public BlendShape ToBlendShape()
        {
            var shapeFrames = new BlendShapeFrame[frames.Length];
            for (int i = 0; i < shapeFrames.Length; i++)
            {
                shapeFrames[i] = frames[i].ToBlendShapeFrame();
            }
            return new BlendShape(shapeName, shapeFrames);
        }
    }
}
