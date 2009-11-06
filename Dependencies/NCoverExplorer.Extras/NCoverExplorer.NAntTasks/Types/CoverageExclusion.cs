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
	/// Represents a coverage exclusion for passing to NCoverExplorer.Console in the configuration file.
	/// </summary>
	[ElementName("exclusion")]
	public class CoverageExclusion : Element
	{
		#region Private Variables

		private string _exclusionType;
		private string _pattern;
		private bool _isRegex;
		private bool _enabled;

		#endregion Private Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="CoverageExclusion"/> class.
		/// </summary>
		public CoverageExclusion() 
			: this(null, null, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CoverageExclusion"/> class.
		/// </summary>
		public CoverageExclusion(string exclusionType, string pattern, bool isRegex)
		{
			_exclusionType = exclusionType;
			_pattern = pattern;
			_isRegex = isRegex;
			_enabled = true;
		}

		#endregion Constructor

		#region Public Properties

		/// <summary>
		/// The exclusion type of Assembly, Namespace or Class.
		/// </summary>
		[TaskAttribute("type")]
		public string ExclusionType 
		{
			get { return _exclusionType; }
			set { _exclusionType = value; }
		}

		/// <summary>
		/// The pattern to match.
		/// </summary>
		[TaskAttribute("pattern")]
		public string Pattern 
		{
			get { return _pattern; }
			set { _pattern = value; }
		}

		/// <summary>
		/// Indicates if the pattern is a regular expression.
		/// </summary>
		[TaskAttribute("isRegex")]
		[BooleanValidator()]
		public bool IsRegex 
		{
			get { return _isRegex; }
			set { _isRegex = value; }
		}

		/// <summary>
		/// Indicates if the exclusion is enabled.
		/// </summary>
		[TaskAttribute("enabled")]
		[BooleanValidator()]
		public bool Enabled 
		{
			get { return _enabled; }
			set { _enabled = value; }
		}

		#endregion Public Properties
	}
}
