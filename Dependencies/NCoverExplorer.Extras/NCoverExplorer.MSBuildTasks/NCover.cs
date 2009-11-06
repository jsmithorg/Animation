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
using System.IO;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NCoverExplorer.Common;

namespace NCoverExplorer.MSBuildTasks
{
	/// <summary>
    /// MSBuild task for automating NCover.Console.exe, with NCover 1.5.x support. Note that this task
	/// will self register CoverLib.dll by default using the registry (does not require local admin).
	/// </summary>
	/// <example>
	/// This example shows the standard profiling using NCover for standard nunit tests with minimal arguments.
	/// Defaults are with logging to coverage.log, profiling all assemblies, output filename of coverage.xml and this 
	/// example specifies a path to where to find ncover.console.exe.
	///        <code>
	///            <![CDATA[
    ///            <NCover  ToolPath="Tool\NCover\"
    ///                     CommandLineExe="$(nunit.path)\nunit-console.exe" 
	///						CommandLineArgs="$(MSBuildProjectDirectory)\myapp.tests.dll" />
	///            ]]>
	///        </code>
	/// </example>
    /// <example>
    /// If you are using TypeMock, you may experience issues with the registration of coverlib.dll conflicting
    /// due to overwriting the registered profiler. You should add the "registerProfiler" attribute below and set it to false.
    ///        <code>
    ///            <![CDATA[
    ///            <NCover  ToolPath="Tool\NCover\"
    ///                     CommandLineExe="$(nunit.path)\nunit-console.exe" 
    ///						CommandLineArgs="$(MSBuildProjectDirectory)\myapp.tests.dll"
    ///						RegisterProfiler="false" />
    ///            ]]>
    ///        </code>
    /// </example>
    /// <example>
    /// This example for NCover 1.5.8 shows profiling a process which is launched by another process.
    ///        <code>
	///            <![CDATA[
	/// 
	///            <NCover  ToolPath="Tool\NCover\"
    ///                     CommandLineExe="MyLauncher.exe" 
    ///						ProfiledProcessModule="LaunchedProcess.exe" />
	///            ]]>
	///        </code>
	/// </example>
    /// <example>
	/// This example shows using an assembly list as ; delimited names rather than using the ability
	/// of the NCover task to dynamically build from a list of files (shown in following example).
	///        <code>
	///            <![CDATA[
	/// 
	///            <NCover  ToolPath="Tool\NCover\"
	///                     CommandLineExe="$(nunit.path)\nunit-console.exe" 
	///						CommandLineArgs="$(MSBuildProjectDirectory)\myapp.tests.dll"
	///						AssemblyList="MyApp.Core;MyApp.Tests" />
	///            ]]>
	///        </code>
	/// </example>
	/// <example>
	/// This example shows the standard profiling using NCover 1.5.x for a Windows application, specifying a coverage
	/// exclusion, verbose logging to a named file, specifically named log and output xml files. It also shows
	/// coverage exclusion attributes, overriding the NCover location to run from and a way of listing assemblies
	/// to be included in the profiled NCover results.
	///        <code>
	///            <![CDATA[
	///			   <ItemGroup>
    ///				   <Assembly Include="$(MSBuildProjectDirectory)\MyApp.MyCode.dll" />
	///			   </ItemGroup>
    /// 
	///            <NCover  ToolPath="Tool\NCover\"
    ///                     CommandLineExe="$(nunit.path)\nunit-console.exe" 
	///						CommandLineArgs="myapp.tests.dll"
	///						CoverageFile="myapp.coverage.xml"
	///						LogLevel="Verbose"
	///						LogFile="myapp.coverage.log"
	///						WorkingDirectory="$(MSBuildProjectDirectory)"
	///						ExcludeAttributes="CoverageExcludeAttribute"
	///                     Assemblies="@(Assembly)" />
	///            ]]>
	///        </code>
	/// </example>
	public class NCover : ToolTask
	{
		#region Private Variables / Constants

		private const string DefaultApplicationName = "NCover.Console.exe";

		private string _commandLineExe;
		private string _commandLineArgs;
		private string _coverageFile;
		private NCoverLogLevel _logLevel;
		private string _logFile;
		private string _workingDirectory;
		private string _assemblyList;
		private ITaskItem[] _assemblyFiles;	
		
		private string _excludeAttributes;
		private bool _profileIIS;
		private string _profileService;

        private bool _registerProfiler;
        private NCoverXmlFormat _xmlFormat;
        private string _profiledProcessModule;

		private string _settingsFile;

		#endregion Private Variables / Constants

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="NCover"/> class.
		/// </summary>
        public NCover()
		{
			_commandLineExe = string.Empty;
			_commandLineArgs = string.Empty;
			_workingDirectory = string.Empty;
            _coverageFile = "coverage.xml";
			_logLevel = NCoverLogLevel.Normal;   // Since Quiet and Normal are same until Peter fixes bug.
			_logFile = "coverage.log";

			_assemblyList = string.Empty;
			_assemblyFiles = new ITaskItem[0];
			_settingsFile = string.Empty;

			_excludeAttributes = string.Empty;
			_profileIIS = false;
			_profileService = string.Empty;

            _registerProfiler = true;
            _xmlFormat = NCoverXmlFormat.Xml1;
            _profiledProcessModule = string.Empty;
        }

		#endregion Constructors

		#region Properties

		/// <summary>
		/// The command line executable to be launched by NCover (such as nunit-console.exe).
		/// </summary>
		public string CommandLineExe
		{
			get { return _commandLineExe; }
			set { _commandLineExe = value; }
		}

		/// <summary>
		/// The arguments to pass to the command line executable to be launched by NCover (such as nunit-console.exe).
		/// </summary>
		public string CommandLineArgs
		{
			get { return _commandLineArgs; }
			set { _commandLineArgs = value; }
		}

		/// <summary>
		/// The filename for the output coverage.xml file (default).
		/// </summary>
		public string CoverageFile
		{
			get { return _coverageFile; }
			set { _coverageFile = value; }
		}

		/// <summary>
        /// What level of NCover logging to provide. Values are "Normal" (default) and "Verbose".
        /// Due to a bug in NCover 1.5.4 "Quiet" will result in NCover stopping abnormally - hence has been
        /// defaulted to be "Normal" until the bug is fixed.
		/// </summary>
		public string LogLevel
		{
			get { return _logLevel.ToString(); }
			set 
			{ 
				_logLevel = (NCoverLogLevel)Enum.Parse(typeof(NCoverLogLevel), value); 
			}
		}

		/// <summary>
		/// Gets or sets the logfile name to write to if logLevel is set to anything other than "Quiet". The default
		/// is "coverage.log".
		/// </summary>
		public string LogFile
		{
			get { return _logFile; }
			set { _logFile = value; }
		}

		/// <summary>
		/// Gets or sets the working directory for the command line executable.
		/// </summary>
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
		public string ExcludeAttributes
		{
			get { return _excludeAttributes; }
			set { _excludeAttributes = value; }
		}

		/// <summary>
		/// Determines whether to profile under IIS. Default value is <see langword="false" />.
		/// </summary>
		public bool ProfileIIS
		{
			get { return _profileIIS; }
			set { _profileIIS = value; }
		}

		/// <summary>
		/// The service name if profiling a windows service.
		/// </summary>
		public string ProfileService
		{
			get { return _profileService; }
			set { _profileService = value; }
		}

		/// <summary>
		/// Alternative to specifying assembly names - you can instead list them as you would on the
		/// command line as a semi-colon delimited list without any suffixes or paths.
		/// </summary>
		public string AssemblyList
		{
			get { return _assemblyList; }
			set { _assemblyList = value; }
		}

		/// <summary>
		/// Used to specify the assemblies to be profiled. Alternative to the AssemblyList property,
		/// where instead you wat the list to be dynamically built using an itemgroup, for instance
		/// to match all assemblies against a wildcard. The NCover task will take care of stripping
		/// off the suffixes etc.
		/// </summary>
		public ITaskItem[] Assemblies
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
        public bool RegisterProfiler
        {
            get { return _registerProfiler; }
            set { _registerProfiler = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the xml output format to write (new in NCover 1.5.7).
        /// Default value is "Xml1", alternat option is "Xml2" which nests method nodes within class
        /// nodes. Note however that "Xml2" is for future use and is not yet supported by NCoverExplorer
        /// as of version 1.3.6.
        /// </summary>
        public string XmlFormat
        {
            get { return _xmlFormat.ToString(); }
            set { _xmlFormat = (NCoverXmlFormat)Enum.Parse(typeof(NCoverXmlFormat), value); }
        }

        /// <summary>
        /// Gets or sets the profiled process module name. Use this argument when the executable being
        /// launched is not the actual process you want to profile coverage for.
        /// </summary>
        public string ProfiledProcessModule
        {
            get { return _profiledProcessModule; }
            set { _profiledProcessModule = value; }
        }

		#endregion Properties

		#region Override ToolTask

		/// <summary>
		/// Executes the task.
		/// </summary>
		/// <returns><see langword="true"/> if the task ran successfully; otherwise <see langword="false"/>.</returns>
		public override bool Execute()
		{
            _CheckToolPath();
            if (_registerProfiler)
            {
                NCoverUtilities.RegisterNCover(ToolPath);
            }
			
            bool success = base.Execute();

            if (_registerProfiler)
            {
                NCoverUtilities.UnregisterNCover();
            }
			_CleanupSettingsFile();
			
			return success;
		}
        
		/// <summary>
		/// Returns a string value containing the command line arguments to pass directly to the executable file.
		/// </summary>
		/// <returns>
		/// A string value containing the command line arguments to pass directly to the executable file.
		/// </returns>
		protected override string GenerateCommandLineCommands()
		{
			_settingsFile = Path.GetTempFileName() + ".ncoversettings";
            Log.LogMessage(MessageImportance.Low, "Creating settings file: {0}", _settingsFile);

            StringBuilder builder = new StringBuilder();
            string[] assemblyFileNames = _ConvertTaskItemsToArray(_assemblyFiles);

			string commandSwitch = NCoverUtilities.BuildTempSettingsXmlFileForNCover(
					null, GenerateFullPathToTool(), _settingsFile, _commandLineExe, _commandLineArgs, _workingDirectory, 
					_assemblyList, assemblyFileNames, _coverageFile, _logLevel, _logFile, _excludeAttributes, 
					_profileIIS, _profileService, _xmlFormat, _profiledProcessModule);

			builder.AppendFormat("{0} \"{1}\" ", commandSwitch, _settingsFile);
            Log.LogMessage(MessageImportance.Low, "Arguments: {0}", builder.ToString());

            // Dump out the contents of the settings file in diagnostics mode.
            string fileContents = NCoverUtilities.GetFileContents(_settingsFile);
            Log.LogMessage(MessageImportance.Low, Properties.Resources.NCoverConfigContent, fileContents);

            return builder.ToString();
		}

		/// <summary>
		/// Returns the fully qualified path to the executable file.
		/// </summary>
		/// <returns>
		/// The fully qualified path to the executable file.
		/// </returns>
		protected override string GenerateFullPathToTool()
		{
			_CheckToolPath();
			return Path.Combine(ToolPath, ToolName);
		}

		/// <summary>
		/// Logs the starting point of the run to all registered loggers.
		/// </summary>
		/// <param name="message">A descriptive message to provide loggers, usually the command line and switches.</param>
		protected override void LogToolCommand(string message)
		{
			Log.LogCommandLine(MessageImportance.Low, message);
		}

		/// <summary>
		/// Gets the name of the executable file to run.
		/// </summary>
		/// <value></value>
		/// <returns>The name of the executable file to run.</returns>
		protected override string ToolName
		{
			get { return DefaultApplicationName; }
		}

		/// <summary>
		/// Gets the <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.
		/// </summary>
		/// <value></value>
		/// <returns>The <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.</returns>
		protected override MessageImportance StandardOutputLoggingImportance
		{
			get { return MessageImportance.Normal; }
		}

		#endregion Override ToolTask

		#region Private Methods
        
        /// <summary>
        /// Check that we have a valid path to NCover.
        /// </summary>
		private void _CheckToolPath()
		{
			string ncoverPath = (ToolPath == null) ? String.Empty : ToolPath.Trim();
			if (string.IsNullOrEmpty(ncoverPath))
			{
				// Not specified in the task arguments so might be in trouble if the user has not
				// installed it in the default NCover folder.
				ncoverPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
				ncoverPath = Path.Combine(ncoverPath, @"NCover");
                if (!Directory.Exists(ncoverPath))
	            {
					// One last try - does it exist somewhere in the path?
					string[] ncoverLocations = PathSearch.Search(ToolName);
					if (ncoverLocations.Length > 0)
					{
						ncoverPath = Path.GetDirectoryName(ncoverLocations[0]);
					}
					else
					{
						Log.LogError(Properties.Resources.NCoverNotFound);
					}
	            }
				ToolPath = ncoverPath;
			}
            else if (!Path.IsPathRooted(ncoverPath)) 
            { 
                ToolPath = Path.Combine(Path.GetDirectoryName(this.BuildEngine.ProjectFileOfTaskNode), ncoverPath);
            }
		}

		/// <summary>
		/// Removes generated settings file after process has run.
		/// </summary>
		private void _CleanupSettingsFile()
		{
			if ( File.Exists( _settingsFile ) )
			{
			    Log.LogMessage(MessageImportance.Low, "Deleting settings file: {0}", _settingsFile);
				File.Delete( _settingsFile );
			}
		}

		/// <summary>
		/// Convert the MSBuild specific ITaskItem[] to a string array for use by NCoverUtilities.
		/// </summary>
		/// <param name="files"></param>
		/// <returns></returns>
		private string[] _ConvertTaskItemsToArray(ITaskItem[] files)
		{
			ArrayList fileNames = new ArrayList();
			foreach (ITaskItem fileName in files)
			{
				fileNames.Add(fileName.ItemSpec);
			}
			return (string[])fileNames.ToArray(typeof(string));
		}

		#endregion Private Methods
	}
}
