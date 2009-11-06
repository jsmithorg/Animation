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

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NCoverExplorer.NAntTasks.Types
{
	/// <summary>
	/// A module threshold is used by the console application (only currently) to set coverage thresholds
	/// at a more granular level than just at project level.
	/// </summary>
	[ElementName("exclusion")]
	public class ModuleThreshold : Element
	{
		#region Private Variables

		private string _moduleName;
		private float _satisfactoryCoverage;

		#endregion Private Variables

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleThreshold"/> class.
		/// </summary>
		public ModuleThreshold()
			: this(string.Empty, 100f)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleThreshold"/> class.
		/// </summary>
		/// <param name="moduleName">Name of the module.</param>
		/// <param name="satisfactoryCoverage">The coverage threshold.</param>
		public ModuleThreshold(string moduleName, float satisfactoryCoverage)
		{
			_moduleName = moduleName;
			_satisfactoryCoverage = satisfactoryCoverage;
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets or sets the name of the module.
		/// </summary>
		/// <value>The name of the module.</value>
		[TaskAttribute("moduleName")]
		public string ModuleName
		{
			get { return _moduleName; }
			set { _moduleName = value; }
		}

		/// <summary>
		/// Gets or sets the coverage threshold for this module.
		/// </summary>
		/// <value>The coverage threshold.</value>
		[TaskAttribute("satisfactoryCoverage")]
		public float SatisfactoryCoverage
		{
			get { return _satisfactoryCoverage; }
			set { _satisfactoryCoverage = value; }
		}

		#endregion Public Properties
	}
}
