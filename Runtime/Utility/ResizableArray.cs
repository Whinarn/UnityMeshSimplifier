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

namespace UnityMeshSimplifier
{
    /// <summary>
    /// A resizable array with the goal of being quicker than List<T>.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    internal sealed class ResizableArray<T>
    {
        #region Fields
        private T[] items = null;
        private int length = 0;

        private static T[] emptyArr = new T[0];
        #endregion

        #region Properties
        /// <summary>
        /// Gets the length of this array.
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return length; }
        }

        /// <summary>
        /// Gets the internal data buffer for this array.
        /// </summary>
        public T[] Data
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return items; }
        }

        /// <summary>
        /// Gets or sets the element value at a specific index.
        /// </summary>
        /// <param name="index">The element index.</param>
        /// <returns>The element value.</returns>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return items[index]; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { items[index] = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new resizable array.
        /// </summary>
        /// <param name="capacity">The initial array capacity.</param>
        public ResizableArray(int capacity)
            : this(capacity, 0)
        {

        }

        /// <summary>
        /// Creates a new resizable array.
        /// </summary>
        /// <param name="capacity">The initial array capacity.</param>
        /// <param name="length">The initial length of the array.</param>
        public ResizableArray(int capacity, int length)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            else if (length < 0 || length > capacity)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (capacity > 0)
                items = new T[capacity];
            else
                items = emptyArr;

            this.length = length;
        }

        /// <summary>
        /// Creates a new resizable array.
        /// </summary>
        /// <param name="initialArray">The initial array.</param>
        public ResizableArray(T[] initialArray)
        {
            if (initialArray == null)
                throw new ArgumentNullException(nameof(initialArray));

            if (initialArray.Length > 0)
            {
                items = new T[initialArray.Length];
                length = initialArray.Length;
                Array.Copy(initialArray, 0, items, 0, initialArray.Length);
            }
            else
            {
                items = emptyArr;
                length = 0;
            }
        }
        #endregion

        #region Private Methods
        private void IncreaseCapacity(int capacity)
        {
            T[] newItems = new T[capacity];
            Array.Copy(items, 0, newItems, 0, System.Math.Min(length, capacity));
            items = newItems;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Clears this array.
        /// </summary>
        public void Clear()
        {
            Array.Clear(items, 0, length);
            length = 0;
        }

        /// <summary>
        /// Resizes this array.
        /// </summary>
        /// <param name="length">The new length.</param>
        /// <param name="trimExess">If exess memory should be trimmed.</param>
        public void Resize(int length, bool trimExess = false)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (length > items.Length)
            {
                IncreaseCapacity(length);
            }
            else if (length < this.length)
            {
                //Array.Clear(items, capacity, length - capacity);
            }

            this.length = length;

            if (trimExess)
            {
                TrimExcess();
            }
        }

        /// <summary>
        /// Trims any excess memory for this array.
        /// </summary>
        public void TrimExcess()
        {
            if (items.Length == length) // Nothing to do
                return;

            var newItems = new T[length];
            Array.Copy(items, 0, newItems, 0, length);
            items = newItems;
        }

        /// <summary>
        /// Adds a new item to the end of this array.
        /// </summary>
        /// <param name="item">The new item.</param>
        public void Add(T item)
        {
            if (length >= items.Length)
            {
                IncreaseCapacity(items.Length << 1);
            }

            items[length++] = item;
        }

        /// <summary>
        /// Returns a copy of the resizable array as an actually array.
        /// </summary>
        /// <returns>The array.</returns>
        public T[] ToArray()
        {
            var newItems = new T[length];
            Array.Copy(items, 0, newItems, 0, length);
            return newItems;
        }
        #endregion
    }
}