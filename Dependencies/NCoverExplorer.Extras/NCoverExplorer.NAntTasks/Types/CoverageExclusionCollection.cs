#region Copyright © 2006 Grant Drake. All rights reserved.
/*
Copyright © 2006 Grant Drake. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.Collections;

namespace NCoverExplorer.NAntTasks.Types
{
	/// <summary>
	/// Contains a collection of <see cref="CoverageExclusion"/> elements.
	/// </summary>
	[Serializable()]
	public class CoverageExclusionCollection : CollectionBase 
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CoverageExclusionCollection"/> class.
		/// </summary>
		public CoverageExclusionCollection() 
		{
		}
        
		/// <summary>
		/// Initializes a new instance of the <see cref="CoverageExclusionCollection"/> class
		/// with the specified <see cref="CoverageExclusionCollection"/> instance.
		/// </summary>
		public CoverageExclusionCollection(CoverageExclusionCollection value) 
		{
			AddRange(value);
		}
        
		/// <summary>
		/// Initializes a new instance of the <see cref="CoverageExclusionCollection"/> class
		/// with the specified array of <see cref="CoverageExclusion"/> instances.
		/// </summary>
		public CoverageExclusionCollection(CoverageExclusion[] value) 
		{
			AddRange(value);
		}

		#endregion Constructors
        
		#region Public Properties

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		[System.Runtime.CompilerServices.IndexerName("Item")]
		public CoverageExclusion this[int index] 
		{
			get {return ((CoverageExclusion)(base.List[index]));}
			set {base.List[index] = value;}
		}

		#endregion Public Properties

		#region Public Methods
        
		/// <summary>
		/// Adds a <see cref="CoverageExclusion"/> to the end of the collection.
		/// </summary>
		/// <param name="item">The <see cref="CoverageExclusion"/> to be added to the end of the collection.</param> 
		/// <returns>The position into which the new element was inserted.</returns>
		public int Add(CoverageExclusion item) 
		{
			return base.List.Add(item);
		}

		/// <summary>
		/// Adds the elements of a <see cref="CoverageExclusion"/> array to the end of the collection.
		/// </summary>
		/// <param name="items">The array of <see cref="CoverageExclusion"/> elements to be added to the end of the collection.</param> 
		public void AddRange(CoverageExclusion[] items) 
		{
			for (int i = 0; (i < items.Length); i = (i + 1)) 
			{
				Add(items[i]);
			}
		}

		/// <summary>
		/// Adds the elements of a <see cref="CoverageExclusionCollection"/> to the end of the collection.
		/// </summary>
		/// <param name="items">The <see cref="CoverageExclusionCollection"/> to be added to the end of the collection.</param> 
		public void AddRange(CoverageExclusionCollection items) 
		{
			for (int i = 0; (i < items.Count); i = (i + 1)) 
			{
				Add(items[i]);
			}
		}
        
		/// <summary>
		/// Determines whether a <see cref="CoverageExclusion"/> is in the collection.
		/// </summary>
		/// <param name="item">The <see cref="CoverageExclusion"/> to locate in the collection.</param> 
		/// <returns>
		/// <see langword="true" /> if <paramref name="item"/> is found in the 
		/// collection; otherwise, <see langword="false" />.
		/// </returns>
		public bool Contains(CoverageExclusion item) 
		{
			return base.List.Contains(item);
		}
        
		/// <summary>
		/// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.        
		/// </summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have zero-based indexing.</param> 
		/// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public void CopyTo(CoverageExclusion[] array, int index) 
		{
			base.List.CopyTo(array, index);
		}
        
		/// <summary>
		/// Retrieves the index of a specified <see cref="CoverageExclusion"/> object in the collection.
		/// </summary>
		/// <param name="item">The <see cref="CoverageExclusion"/> object for which the index is returned.</param> 
		/// <returns>
		/// The index of the specified <see cref="CoverageExclusion"/>. If the <see cref="CoverageExclusion"/> is not currently a member of the collection, it returns -1.
		/// </returns>
		public int IndexOf(CoverageExclusion item) 
		{
			return base.List.IndexOf(item);
		}
        
		/// <summary>
		/// Inserts a <see cref="CoverageExclusion"/> into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The <see cref="CoverageExclusion"/> to insert.</param>
		public void Insert(int index, CoverageExclusion item) 
		{
			base.List.Insert(index, item);
		}
        
		/// <summary>
		/// Returns an enumerator that can iterate through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="CoverageExclusionEnumerator"/> for the entire collection.
		/// </returns>
		public new CoverageExclusionEnumerator GetEnumerator() 
		{
			return new CoverageExclusionEnumerator(this);
		}
        
		/// <summary>
		/// Removes a member from the collection.
		/// </summary>
		/// <param name="item">The <see cref="CoverageExclusion"/> to remove from the collection.</param>
		public void Remove(CoverageExclusion item) 
		{
			base.List.Remove(item);
		}
        
		#endregion Public Methods
	}

	/// <summary>
	/// Enumerates the <see cref="CoverageExclusion"/> elements of a <see cref="CoverageExclusionCollection"/>.
	/// </summary>
	public class CoverageExclusionEnumerator : IEnumerator 
	{
		#region Internal Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CoverageExclusionEnumerator"/> class
		/// with the specified <see cref="CoverageExclusionCollection"/>.
		/// </summary>
		/// <param name="arguments">The collection that should be enumerated.</param>
		internal CoverageExclusionEnumerator(CoverageExclusionCollection arguments) 
		{
			IEnumerable temp = arguments;
			_baseEnumerator = temp.GetEnumerator();
		}

		#endregion Internal Instance Constructors

		#region Implementation of IEnumerator
            
		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		/// <returns>
		/// The current element in the collection.
		/// </returns>
		public CoverageExclusion Current 
		{
			get { return (CoverageExclusion) _baseEnumerator.Current; }
		}

		object IEnumerator.Current 
		{
			get { return _baseEnumerator.Current; }
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// <see langword="true" /> if the enumerator was successfully advanced 
		/// to the next element; <see langword="false" /> if the enumerator has 
		/// passed the end of the collection.
		/// </returns>
		public bool MoveNext() 
		{
			return _baseEnumerator.MoveNext();
		}

		bool IEnumerator.MoveNext() 
		{
			return _baseEnumerator.MoveNext();
		}
            
		/// <summary>
		/// Sets the enumerator to its initial position, which is before the 
		/// first element in the collection.
		/// </summary>
		public void Reset() 
		{
			_baseEnumerator.Reset();
		}
            
		void IEnumerator.Reset() 
		{
			_baseEnumerator.Reset();
		}

		#endregion Implementation of IEnumerator

		#region Private Instance Fields
    
		private IEnumerator _baseEnumerator;

		#endregion Private Instance Fields
	}
}
