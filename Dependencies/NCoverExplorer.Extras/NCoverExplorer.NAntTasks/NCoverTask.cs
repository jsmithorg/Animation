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

using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

using NCoverExplorer.Common;

namespace NCoverExplorer.NAntTasks
{
	/// <summary>
	/// NAnt task for automating NCover.Console.exe, with both NCover 1.3.3 and 1.5.x support. Note that this task
	/// will self register CoverLib.dll by default using the registry (does not require local admin).
	/// </summary>
	/// <example>
	/// This example shows the standard profiling using NCover for standard nunit tests with minimum arguments.
	/// Defaults are with no logging, profiling all assemblies, output filename of coverage.xml and this assumes that
	/// NCover and NUnit are in the path.
	///        <code>
	///            <![CDATA[
	///            <ncover  commandLineExe="${nunit.path}\nunit-console.exe" 
	///						commandLineArgs="${build.path}\myapp.tests.dll" >
	///            </ncover>
	///            ]]>
	///        </code>
	/// </example>
	/// <example>
	/// If you are using TypeMock, you may experience issues with the registration of coverlib.dll conflicting
	/// due to overwriting the registered profiler. You should add the "registerProfiler" attribute below and set it to false.
	///        <code>
	///            <![CDATA[
	///            <ncover  commandLineExe="${nunit.path}\nunit-console.exe" 
	///						commandLineArgs="${build.path}\myapp.tests.dll"
	///						registerProfiler="false" >
	///            </ncover>
	///            ]]>
	///        </code>
	/// </example>
	/// <example>
	/// This example shows the standard profiling using for standard nunit tests with minimal arguments.
	/// Defaults are with logging to coverage.log, profiling all assemblies, output filename of coverage.xml and this 
	/// example specifies a full path to NCover.Console.exe.
	///        <code>
	///            <![CDATA[
	///            <ncover  program="C:\Program Files\NCover\ncover.console.exe" 
	///						commandLineExe="${nunit.path}\nunit-console.exe" 
	///						commandLineArgs="${build.path}\myapp.tests.dll" >
	///            </ncover>
	///            ]]>
	///        </code>
	/// </example>
	/// <example>
	/// This example for NCover 1.5.8 shows profiling a process which is launched by another process.
	///        <code>
	///            <![CDATA[
	///            <ncover  program="C:\Program Files\NCover\ncover.console.exe" 
	///						commandLineExe="MyLauncher.exe" 
	///						profiledProcessModule="LaunchedProcess.exe" >
	///            </ncover>
	///            ]]>
	///        </code>
	/// </example>
	/// <example>
	/// This example shows using the assemblies attribute inline rather than having child elements, and
	/// also showing how child elements of commandLineArgs should be quoted if they contain spaces.
	///        <code>
	///            <![CDATA[
	///            <ncover  program="tools\ncover\ncover.console.exe" 
	///						commandLineExe="${nunit.path}\nunit-console.exe" 
	///						commandLineArgs="myapp.tests.dll /xml=&quot;c:\my results\test.xml&quot;"
	///						assemblyList="myapp.main;myapp.core"
	///						excludeAttributes="CoverageExcludeAttribute" >
	///            </ncover>
	///            ]]>
	///        </code>
	/// </example>
	/// <example>
	/// This example shows the standard profiling using NCover 1.5.x for a Windows application, specifying a coverage
	/// exclusion, verbose logging to a named file, specifically named log, output xml and html files. It also shows
	/// coverage exclusion attributes, overriding the NCover location to run from and a way of listing assemblies
	/// to be included in the profiled NCover results.
	///        <code>
	///            <![CDATA[
	///            <ncover  program="tools\ncover\ncover.console.exe" 
	///						commandLineExe="${nunit.path}\nunit-console.exe" 
	///						commandLineArgs="myapp.tests.dll"
	///						coverageFile="myapp.coverage.xml"
	///						logLevel="Verbose"
	///						logFile="myapp.coverage.log"
	///						workingDirectory="${build.path}"
	///						excludeAttributes="CoverageExcludeAttribute" >
	///                <assemblies basedir="${build.path}">
	///                    <include name="myapp.*.dll" />
	///                </assemblies>
	///            </ncover>
	///            ]]>
	///        </code>
	/// </example>
	[TaskName("ncover")]
	public class NCoverTask : NAnt.Core.Tasks.ExternalProgramBase
	{
		#region Private Variables / Constants

		private const string DefaultApplicationName = "NCover.Console.exe";

		private string _commandLineExe;
		private string _commandLineArgs;
		private string _coverageFile;
		private NCoverLogLevel _logLevel;
		private string _logFile;
		private string _workingDirectory;
		private FileSet _assemblyFiles;	
		private string _assemblyList;
		
		private string _excludeAttributes;
		private bool _profileIIS;
		private string _profileService;

		private bool _registerProfiler;
		private NCoverXmlFormat _xmlFormat;
		private string _profiledProcessModule;

		private string _settingsFile;
		private StringBuilder _programArguments;

		#endregion Private Variables / Constants

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="NCoverTask"/> class.
		/// </summary>
		public NCoverTask()
		{
			_commandLineExe = string.Empty;
			_commandLineArgs = string.Empty;
			_workingDirectory = string.Empty;
			_coverageFile = "coverage.xml";
			_logLevel = NCoverLogLevel.Normal;
			_logFile = "coverage.log";

			_assemblyFiles = new FileSet();
			_assemblyList = string.Empty;
			_settingsFile = string.Empty;

			_excludeAttributes = string.Empty;
			_profileIIS = false;
			_profileService = string.Empty;

			_registerProfiler = true;
			_xmlFormat = NCoverXmlFormat.Xml1;
			_profiledProcessModule = string.Empty;

			this.ExeName = DefaultApplicationName;
			_programArguments = new StringBuilder();
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// The name of the executable that should be used to launch the external program.
		/// </summary>
		/// <value>
		/// The name of the executable that should be used to launch the external
		/// program, or <see langword="null"/> if no name is specified.
		/// </value>
		/// <remarks>
		/// If available, the configured value in the NAnt configuration
		/// file will be used if no name is specified.
		/// </remarks>
		[TaskAttribute("program", Required=false)]
		[StringValidator(AllowEmpty=false)]
		public override string ExeName
		{
			get { return base.ExeName; }
			set { base.ExeName = value; }
		}

		/// <summary>
		/// The command line executable to be launched by NCover (such as nunit-console.exe).
		/// </summary>
		[TaskAttribute("commandLineExe")]
		[StringValidator(AllowEmpty=false)]
		public string CommandLineExe
		{
			get { return _commandLineExe; }
			set { _commandLineExe = value; }
		}

		/// <summary>
		/// The arguments to pass to the command line executable to be launched by NCover (such as nunit-console.exe).
		/// </summary>
		[TaskAttribute("commandLineArgs", Required=false)]
		public string CommandLineArgs
		{
			get { return _commandLineArgs; }
			set { _commandLineArgs = value; }
		}

		/// <summary>
		/// The filename for the output coverage.xml file (default).
		/// </summary>
		[TaskAttribute("coverageFile", Required=false)]
		public string CoverageFile
		{
			get { return _coverageFile; }
			set { _coverageFile = value; }
		}

		/// <summary>
		/// What level of NCover logging to provide. Values are "Quiet", "Normal" (default) and "Verbose".
		/// Note that due to a current bug in NCover 1.5.4 there is no different between Quiet and Normal
		/// since the //q argument (NoLog=true) will lock NCover. So logging is always on for NCover 1.5.4
		/// </summary>
		[TaskAttribute("logLevel")]
		public NCoverLogLevel LogLevel
		{
			get { return _logLevel; }
			set { _logLevel = value; }
		}

		/// <summary>
		/// Gets or sets the logfile name to write to if logLevel is set to anything other than "Quiet". The default
		/// is "coverage.log".
		/// </summary>
		[TaskAttribute("logFile", Required=false)]
		public string LogFile
		{
			get { return _logFile; }
			set { _logFile = value; }
		}

		/// <summary>
		/// Gets or sets the working directory for the command line executable.
		/// </summary>
		[TaskAttribute("workingDirectory", Required=false)]
		public string WorkingDirectory
		{
			get { return _workingDirectory; }
			set { _workingDirectory = value; }
		}

		/// <summary>
		/// If coverage exclusion attributes have been applied (NCover 1.5.4 onwards) specify the full namespace
		/// to the attribute including the "Attribute" suffix - e.g. "CoverageExcludeAttribute" if defined in no
		/// namespace. Separate multiple attributes with semi-colons.
		/// </summary>
		[TaskAttribute("excludeAttributes")]
		public string ExcludeAttributes
		{
			get { return _excludeAttributes; }
			set { _excludeAttributes = value; }
		}

		/// <summary>
		/// Determines whether to profile under IIS (//iis). Default value is <see langword="false" />.
		/// </summary>
		[TaskAttribute("profileIIS")]
		[BooleanValidator()]
		public bool ProfileIIS
		{
			get { return _profileIIS; }
			set { _profileIIS = value; }
		}

		/// <summary>
		/// The service name to profile if any (//svc). Default is none.
		/// </summary>
		[TaskAttribute("profileService")]
		public string ProfileService
		{
			get { return _profileService; }
			set { _profileService = value; }
		}

		/// <summary>
		/// Used to specify the assemblies to be profiled by specifically naming them. Values
		/// should be separated by semi-colons and not include suffixe or path (and case sensitive)
		/// as this is how they are identified by the CLR. e.g. "MyApp.Main;MyApp.Core".
		/// </summary>
		[TaskAttribute("assemblyList")]
		public string AssemblyList
		{
			get { return _assemblyList; }
			set { _assemblyList = value; }
		}

		/// <summary>
		/// Used to specify the assemblies to be profiled by choosing dynamically from a
		/// directory. To use see <see cref="FileSet" />.
		/// </summary>
		[BuildElement("assemblies")]
		public FileSet Assemblies
		{
			get { return _assemblyFiles; }
			set { _assemblyFiles = value; }
		}

		/// <summary>
		/// Determines whether to register NCover CoverLib.dll on each run. The default is <c>true</c>. You
		/// would set this to <c>false</c> if using TypeMock due to a conflict in registered profilers.
		/// If set to true, the NCover task uses a reference counting approach to minimise the chance
		/// of issues when simultaneous builds.
		/// </summary>
		[TaskAttribute("registerProfiler", Required=false)]
		[BooleanValidator()]
		public bool RegisterProfiler
		{
			get { return _registerProfiler; }
			set { _registerProfiler = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating the xml output format to write (new in NCover 1.5.7).
		/// Default value is "Xml1", alternate option is "Xml2" which nests method nodes within class
		/// nodes. Note however that "Xml2" is for future use and is not yet supported by NCoverExplorer
		/// as of version 1.3.6.
		/// </summary>
		[TaskAttribute("xmlFormat", Required=false)]
		public NCoverXmlFormat XmlFormat
		{
			get { return _xmlFormat; }
			set { _xmlFormat = value; }
		}

		/// <summary>
		/// Gets or sets the profiled process module name. Use this argument when the executable being
		/// launched is not the actual process you want to profile coverage for.
		/// </summary>
		[TaskAttribute("profiledProcessModule", Required=false)]
		public string ProfiledProcessModule
		{
			get { return _profiledProcessModule; }
			set { _profiledProcessModule = value; }
		}

		#endregion Properties

		#region Override implementation of ExternalProgramBase

		/// <summary>
		/// The command-line arguments for the external program.
		/// </summary>
		public override string ProgramArguments
		{
			get	{ return _programArguments.ToString(); }
		}

		/// <summary>
		/// Starts the process and handles errors.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Diagnostics.Process"/> that was started.
		/// </returns>
		protected override Process StartProcess()
		{
			if (!Path.IsPathRooted(ExeName))
			{
				ExeName = this.Project.GetFullPath(ExeName);
			}
			if (_registerProfiler)
			{
				NCoverUtilities.RegisterNCover(Path.GetDirectoryName(ExeName));
			}
			Process process = base.StartProcess();
			process.Exited += new System.EventHandler(_OnProcessExited);
			return process;
		}

		/// <summary>
		/// Performs logic before the external process is started.
		/// </summary>
		/// <param name="process">Process.</param>
		protected override void PrepareProcess(Process process) 
		{
			_settingsFile = Path.GetTempFileName() + ".ncoversettings";
			Log( Level.Verbose, "Creating settings file: " + _settingsFile );

			string[] assemblyFiles = _ConvertStringCollectionToArray(_assemblyFiles.FileNames);

			string commandSwitch = NCoverUtilities.BuildTempSettingsXmlFileForNCover(
				null, ExeName, _settingsFile, _commandLineExe, _commandLineArgs, _workingDirectory,
				_assemblyList, assemblyFiles, _coverageFile, _logLevel, _logFile, 
				_excludeAttributes, _profileIIS, _profileService, _xmlFormat, _profiledProcessModule);

			_programArguments.AppendFormat("{0} \"{1}\" ", commandSwitch, _settingsFile);

			Log(Level.Verbose, "Working directory: {0}", BaseDirectory);
			Log(Level.Verbose, "Arguments: {0}", ProgramArguments);
			if (IsLogEnabledFor(Level.Verbose))
			{
				// Dump out the contents of the settings file.
				string fileContents = NCoverUtilities.GetFileContents(_settingsFile);
				Log(Level.Verbose, "Contents of NCover settings file:");
				Log(Level.Verbose, fileContents);
			}

			base.PrepareProcess(process);
		}

		#endregion Override implementation of ExternalProgramBase

		#region Private Methods

		/// <summary>
		/// Convert the NAnt specific StringCollection to a string array for use by NCoverUtilities.
		/// </summary>
		/// <param name="files"></param>
		/// <returns></returns>
		private string[] _ConvertStringCollectionToArray(StringCollection files)
		{
			ArrayList fileNames = new ArrayList();
			foreach (string fileName in files)
			{
				fileNames.Add(fileName);
			}
			return (string[])fileNames.ToArray(typeof(string));
		}

		/// <summary>
		/// Removes generated settings file after process has run.
		/// </summary>
		private void _OnProcessExited(object sender, System.EventArgs e)
		{
			if ( File.Exists( _settingsFile ) )
			{
				Log( Level.Verbose, "Deleting settings file: " + _settingsFile );
				File.Delete( _settingsFile );
			}
			if (_registerProfiler)
			{
				NCoverUtilities.UnregisterNCover();
			}
		}

		#endregion Private Methods
	}
}
