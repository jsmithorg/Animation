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
	/// Strongly typed collection of <see cref="ModuleThreshold"/> objects.
	/// </summary>
	[Serializable]
	public class ModuleThresholdCollection : CollectionBase
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleThresholdCollection"/> class.
		/// </summary>
		public ModuleThresholdCollection()
		{
		}

		#endregion Constructor

		#region IList Interface Strongly Typed Members
		
		/// <summary>
		/// Add a <see cref="ModuleThreshold"/> object to the collection.
		/// </summary>
		/// <param name="item">The item to add.</param>
		public void Add(ModuleThreshold item)
		{
			this.List.Add(item);
		}

		/// <summary>
		/// Gets or sets the <see cref="ModuleThreshold"/> object at this ordinal index.
		/// </summary>
		public ModuleThreshold this[int index]
		{
			get { return this.List[index] as ModuleThreshold; }
			set { this.List[index] = value; }
		}

		/// <summary>
		/// Gets or sets the <see cref="ModuleThreshold"/> object with this module name.
		/// </summary>
		public ModuleThreshold this[string moduleName]
		{
			get 
			{ 
				foreach (ModuleThreshold moduleThreshold in this.List)
				{
					if (moduleThreshold.ModuleName == moduleName)
					{
						return moduleThreshold;
					}
				}
				return null; 
			}
		}

		/// <summary>
		/// The remove method that takes a <see cref="ModuleThreshold"/> object.
		/// </summary>
		/// <param name="value">The item to remove.</param>
		public void Remove(ModuleThreshold value)
		{
			this.List.Remove(value);
		}

		/// <summary>
		/// Insert object at this position.
		/// </summary>
		/// <param name="index">Position to insert at.</param>
		/// <param name="value">Object to insert.</param>
		public void Insert(int index, ModuleThreshold value)
		{
			this.List.Insert(index, value);
		}

		/// <summary>
		/// Returns index position of this object.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public int IndexOf(ModuleThreshold value)
		{
			return this.List.IndexOf(value);
		}

		/// <summary>
		/// Returns whether this collection contains this object.
		/// </summary>
		/// <param name="value">Object to find.</param>
		/// <returns>
		/// 	<c>true</c> if contains the specified value; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(ModuleThreshold value)
		{
			return this.List.Contains(value);
		}

		/// <summary>
		/// Returns whether this collection contains this object.
		/// </summary>
		/// <param name="moduleName">Name of the module.</param>
		/// <returns>
		/// 	<c>true</c> if contains the specified value; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(string moduleName)
		{
			foreach (ModuleThreshold moduleThreshold in this.List)
			{
				if (moduleThreshold.ModuleName == moduleName)
				{
					return true;
				}
			}
			return false;
		}

		#endregion IList Interface Strongly Typed Members
	}

	/// <summary>
	/// Enumerates the <see cref="ModuleThreshold"/> elements of a <see cref="ModuleThresholdCollection"/>.
	/// </summary>
	public class ModuleThresholdEnumerator : IEnumerator 
	{
		#region Internal Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleThresholdEnumerator"/> class
		/// with the specified <see cref="ModuleThresholdCollection"/>.
		/// </summary>
		/// <param name="arguments">The collection that should be enumerated.</param>
		internal ModuleThresholdEnumerator(ModuleThresholdCollection arguments) 
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
		public ModuleThreshold Current 
		{
			get { return (ModuleThreshold) _baseEnumerator.Current; }
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
