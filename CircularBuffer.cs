/*
* $HeadURL: svn://esvn/NEXT_PC_Apps/NB_Embedded_SPI/trunk/NB_EngineeringApp/Models/CircularBuffer.cs $
*
*  Created on: 13 Mar 2019
*      Author: Matt Jordan
*
*      Last Modified: $LastChangedDate: 2019-03-28 16:45:43 -0700 (Thu, 28 Mar 2019) $
*      Last Modified by: $LastChangedBy: robby.connor $
*      LastChangedRevision : $LastChangedRevision: 11692 $
*
*      This software is provided "as is".  NEXT Biometrics makes no warranty of any kind, either
*      express or implied, including without limitation any implied warranties of condition, uninterrupted
*      use, merchantability, or fitness for a particular purpose.
*
*      This document as well as the information or material contained is copyrighted.
*      Any use not explicitly permitted by copyright law requires prior consent of NEXT Biometrics.
*      This applies to any reproduction, revision, translation and storage.
*
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace NB_EngineeringApp.Models
{
    /// <summary>
    /// Represents a circular buffer of <typeparamref name="T"/>.
    /// </summary>
    public class CircularBuffer<T>
    {
        #region Fields

        /// <summary>
        /// // The buffer to fill.
        /// </summary>
        private T[] _buffer;

        /// <summary>
        /// The index at which to insert the next item.
        /// </summary>
        private int _head = 0;

        /// <summary>
        /// The current size (max capacity) of the buffer.
        /// </summary>
        public int Capacity { get; private set; }

        private int _currentAmount = 0;
        /// <summary>
        /// The number of items that have been successfully added to buffer or <see cref="Capacity"/> if the buffer is full.
        /// </summary>
        public int CurrentAmount
        {
            get => _currentAmount;
            private set
            { 
                if (value > Capacity)
                    _currentAmount = Capacity;
                else
                    _currentAmount = value;
            }
        }

        #endregion

        #region Initialization Fxns

        public CircularBuffer(int size)
        {
            if (size <= 0) throw new ArgumentException($"Size must be positive and non-zero.");

            _buffer = new T[size];
            Capacity = size;
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Adds <paramref name="obj"/> at the next available location in the circular buffer.
        /// </summary>
        public void Add(T obj)
        {
            if (_head < _buffer.Length)
                _buffer[_head] = obj;
            else
                throw new OverflowException($"Something went wrong when trying to add the object to the circular buffer\nIndex = {_head}, Size = {Capacity}");

            _head = (_head + 1) % Capacity;
            CurrentAmount++;
        }

        /// <summary>
        /// Retrieves the value at <paramref name="index"/> in the buffer.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="index"/> is not a valid index in the buffer.</exception>
        public T Get(int index)
        {
            if (index < 0 || index >= _buffer.Length) throw new ArgumentException("Index out-of-range");

            return _buffer[index];
        }

        /// <summary>
        /// Gets a collection of all added objects in the buffer.
        /// </summary>
        public IEnumerable<T> GetAll()
        {
            if (CurrentAmount == 0) return Enumerable.Empty<T>();

            return _buffer.Take(CurrentAmount);
        }

        /// <summary>
        /// Changes the size of the internal buffer, copying over all items that will fit into the new buffer while retaining order.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="newSize"/> is less than or equal to zero.</exception>
        public void Resize(int newSize)
        {
            if (newSize <= 0) throw new ArgumentException($"Size must be positive and non-zero.");

            if (newSize == Capacity) return;    // Don't waste my time

            T[] tempBuffer = new T[newSize];

            // Place _head at the appropriate starting location
            findStartingBufferIndexForResize(newSize);

            // Copy elements, stopping once we have copied all elements currently in the buffer (CurrentAmount)
            // or we have copied all elements that will fit in the new buffer (newSize)
            int i = 0;
            while (i < CurrentAmount && i < newSize)
            {
                tempBuffer[i] = _buffer[_head];

                i++;
                _head = (_head + 1) % Capacity;
            }

            // Place _head at the apprioriate ending location
            findEndingBufferIndexAfterResize(newSize);

            _buffer = tempBuffer;
            Capacity = newSize;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Find the correct starting location in the internal buffer to begin copying elements into the resized buffer.
        /// </summary>
        private void findStartingBufferIndexForResize(int newSize)
        {
            // The primary cases are:
            // 1. Resizing larger and not full -- move to _head to beginning
            // 2. Resizing larger and full -- do nothing (_head is already at the correct location)
            // 3. Resizing smaller and CurrentAmount will not fit in new buffer -- move _head to "oldest" element that will fit in new buffer 
            // 4. Resizing smaller and CurrentAmount will fit in new buffer -- move _head to beginning

            if (newSize < Capacity)
            {
                // If resizing smaller, move _head to the "oldest" element that should be copied to ensure order is maintained
                if (CurrentAmount > newSize)
                    _head = (_head + (CurrentAmount - newSize)) % CurrentAmount;
                else
                    _head = 0;
            }
            else if (CurrentAmount != Capacity)
            {
                // If resizing larger, but not full, start at the beginning 
                _head = 0;
            }
        }

        /// <summary>
        /// Ensure _head is ready for the next <see cref="Add(T)"/> operation after resizing 
        /// </summary>
        /// <param name="newSize"></param>
        private void findEndingBufferIndexAfterResize(int newSize)
        {
            if (Capacity < newSize && CurrentAmount == Capacity)
            {
                // If resizing to a larger size and the current buffer is "full", move the head index up to the next "empty" location
                _head = CurrentAmount;
            }
            else if (CurrentAmount >= newSize)
            {
                // If the current buffer has more items than will fit in the new buffer,
                // set the currentAmount to "full" and move the head index to the beginning
                CurrentAmount = newSize;
                // If the currentAmount == newSize, then we still need to reset the head to the beginning
                _head = 0;
            }
        }

        #endregion
    }
}
