// -----------------------------------------------------------------------
// <copyright file="FastStack.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lizoc.TextScript
{
    /// <summary>
    /// Lightweight stack object for reference types
    /// </summary>
    /// <typeparam name="T">Type of the object</typeparam>
    internal struct FastStack<T>
    {
        private T[] _array; // Storage for stack elements. Do not rename (binary serialization)
        private int _size; // Number of items in the stack. Do not rename (binary serialization)
        private const int DefaultCapacity = 4;

        // Create a stack with a specific initial capacity.  The initial capacity
        // must be a non-negative number.
        public FastStack(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), RS.ExpectPositiveInteger);

            _array = new T[capacity];
            _size = 0;
        }

        public int Count => _size;

        public T[] Items => _array;

        // Removes all Objects from the Stack.
        public void Clear()
        {
            Array.Clear(_array, 0, _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            _size = 0;
        }

        [MethodImpl(MethodImplOptionsHelper.AggressiveInlining)]
        public T Peek()
        {
            if (_size == 0)
                ThrowForEmptyStack();

            return _array[_size - 1];
        }

        // Pops an item from the top of the stack.  If the stack is empty, Pop
        // throws an InvalidOperationException.
        [MethodImpl(MethodImplOptionsHelper.AggressiveInlining)]
        public T Pop()
        {
            if (_size == 0)
                ThrowForEmptyStack();

            T item = _array[--_size];
            _array[_size] = default(T);     // Free memory quicker.
            return item;
        }

        // Pushes an item to the top of the stack.
        [MethodImpl(MethodImplOptionsHelper.AggressiveInlining)]
        public void Push(T item)
        {
            if (_size == _array.Length)
                Array.Resize(ref _array, (_array.Length == 0) ? DefaultCapacity : 2 * _array.Length);

            _array[_size++] = item;
        }

        private void ThrowForEmptyStack()
        {
#if DEBUG
            Debug.Assert(_size == 0);
#endif
            throw new InvalidOperationException(RS.StackEmptyError);
        }
    }
}