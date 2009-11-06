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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

using NCoverExplorer.Common;

namespace NCoverExplorer.MSBuildTasks
{
	/// <summary>
	/// MSBuild task for automating NCoverExplorer.Console.
    /// Using this task you can merge coverage files from NCover, produce xml coverage reports for use
    /// with CruiseControl.Net, produce html report files directly, fail automated builds if coverage
    /// thresholds are not met and apply a range of detail to the reports produced such as sorting, 
    /// filtering and coverage exclusions.
	/// </summary>
    /// <example>
    /// This example shows producing an xml coverage report at Module/Namespace/Class detail level for
    /// inclusion on a CC.Net build server. You would add a merge file task in the publishers section
    /// of your CC.Net project file to merge in this "CoverageSummary.xml" file so that it can be 
    /// transformed by the NCoverExplorer xsl stylesheets you have copied into the CC.Net folder. Here
    /// we have set a satisfactory coverage threshold at 80%.
    ///        <code>
    ///            <![CDATA[
    ///			   <ItemGroup>
    ///				   <CoverageFile Include="$(MSBuildProjectDirectory)\Coverage\Coverage.xml" />
    ///			   </ItemGroup>
    /// 
    ///            <NCoverExplorer ToolPath="Tools\NCoverExplorer\"
	///							   ProjectName="My Project"
    ///							   OutputDir="$(MSBuildProjectDirectory)\Coverage"
    ///							   CoverageFiles="@(CoverageFile)"
    ///							   SatisfactoryCoverage="80"
    ///                            ReportType="4"
    ///							   XmlReportName="CoverageSummary.xml"
    ///			   />
    ///            ]]>
    ///        </code>
    /// </example>
    /// <example>
    /// This example shows producing an html function coverage report, excluding the test assemblies. The
    /// assemblies excluded are being displayed at the bottom of the report. Note also that this time
    /// the ReportType is specified by its enum name rather than numeric value - they are interchangable.
    /// We have also "inlined" the "CoverageFiles" from the ItemGroup above to show this can be done.
    ///        <code>
    ///            <![CDATA[
    ///            <PropertyGroup> 
    ///                <CoverageExclusions> 
    ///                   <CoverageExclusion>
	///						  <ExclusionType>Assembly</ExclusionType>
	///						  <Pattern>*.Tests</Pattern> 
    ///                   </CoverageExclusion> 
    ///                </CoverageExclusions> 
    ///            </PropertyGroup>
    /// 
    ///            <NCoverExplorer ToolPath="Tools\NCoverExplorer\"
    ///                            ProjectName="My Project"
    ///							   OutputDir="$(MSBuildProjectDirectory)\Coverage"
    ///							   CoverageFiles="$(MSBuildProjectDirectory)\Coverage\Coverage.xml"
    ///							   SatisfactoryCoverage="80"
    ///                            ReportType="ModuleClassFunctionSummary"
    ///							   HtmlReportName="CoverageSummary.html"
    ///							   Exclusions="$(CoverageExclusions)"
    ///							   ShowExcluded="True"
    ///			   />
    ///            ]]>
    ///        </code>
    /// </example>
    /// <example>
    /// This example shows producing an html module class summary coverage report with exclusions as above.
    /// This time we have added applying specific sorting and filtering criteria. This report will show all
    /// classes that do not have 100% coverage, sorted within their namespaces by descending coverage %. We
    /// have also "inlined" the exclusions
    ///        <code>
    ///            <![CDATA[
    ///            <NCoverExplorer ToolPath="Tools\NCoverExplorer\"
    ///                            ProjectName="My Project"
    ///							   OutputDir="$(MSBuildProjectDirectory)\Coverage"
    ///							   CoverageFiles="$(MSBuildProjectDirectory)\Coverage\Coverage.xml"
    ///							   SatisfactoryCoverage="80"
    ///                            ReportType="ModuleClassSummary"
    ///							   HtmlReportName="CoverageSummary.html"
    ///							   Exclusions="Assembly=*.Tests;"
    ///							   ShowExcluded="True"
    ///							   Sort="CoveragePercentageDescending"
    ///							   Filter="HideFullyCovered"
    ///			   />
    ///            ]]>
    ///        </code>
    /// </example>
    /// <example>
    /// This example shows the merging capability to produce a consolidated merge file from multiple
    /// coverage test runs. The results are being stored in a single "MyApp.CoverageMerged.xml" file.
    /// Note that you could additionally apply coverage exclusions at this point. Merging files can
    /// be useful if your testing process requires multiple coverage runs and you want a single archive
    /// which consolidates the results.
    ///        <code>
    ///            <![CDATA[
    ///			   <ItemGroup>
    ///				   <CoverageFile Include="$(MSBuildProjectDirectory)\Coverage\MyApp*.Coverage.xml" />
    ///			   </ItemGroup>
    /// 
    ///            <NCoverExplorer ToolPath="Tools\NCoverExplorer\"
    ///                            OutputDir="$(MSBuildProjectDirectory)\Coverage"
    ///							   ReportType="None"
    ///							   CoverageFiles="@(CoverageFile)"
    ///							   MergeFileName="MyApp.CoverageMerged.xml"
    ///			   />
    ///            ]]>
    ///        </code>
    /// </example>
    /// <example>
    /// This example shows failing a build if the overall coverage % does not meet our threshold, without
    /// producing a coverage report.
    ///        <code>
    ///            <![CDATA[
    ///            <NCoverExplorer ToolPath="Tools\NCoverExplorer\"
    ///                            ProjectName="My Project"
    ///							   ReportType="None"
    ///							   CoverageFiles="$(MSBuildProjectDirectory)\Coverage\Coverage.xml"
    ///							   MinimumCoverage="80"
    ///							   FailMinimum="True"
    ///			   />
    ///            ]]>
    ///        </code>
    /// </example>
    /// <example>
    /// This example shows failing a build if either the overall coverage % does not meet our threshold, or
    /// if one of the individual module thresholds is not met. Note that the ModuleThresholds
    /// could have been "inlined" (just showing the MSBuild flexibility to place in a separate group).
    ///        <code>
    ///            <![CDATA[
    ///			   <ItemGroup>
    ///				   <ModuleThreshold Include="MyProject.1.dll=75" />
    ///				   <ModuleThreshold Include="MyProject.2.dll=85" />
    ///			   </ItemGroup>
    /// 
    ///            <NCoverExplorer ToolPath="Tools\NCoverExplorer\"
    ///                            ProjectName="My Project"
    ///							   ReportType="None"
    ///							   CoverageFiles="$(MSBuildProjectDirectory)\Coverage\Coverage.xml"
    ///							   MinimumCoverage="80"
    ///							   FailMinimum="True"
    ///							   ModuleThresholds="@(ModuleThreshold)"
    ///			   />
    ///            ]]>
    ///        </code>
    /// </example>
    /// <example>
    /// This example shows using virtually the whole range of attributes. Shown below is failing a build 
    /// if not reaching the overall or module level coverage thresholds. The results of merging multiple 
    /// NCover files together are stored as a separate file. We are producing xml and html Namespace per
    /// module summary reports (with the exclusions show in the footer). Note that the module thresholds 
    /// will also be used in the reports. The reports are sorted by name with no filter applied.
    /// We are excluding test assemblies and anything in a presentation layer namespace.
	///        <code>
	///            <![CDATA[
	///			   <ItemGroup>
    ///				   <CoverageFile Include="$(MSBuildProjectDirectory)\Test\Coverage\*.Coverage.xml" />
	///			   </ItemGroup>
    /// 
    ///            <PropertyGroup> 
    ///                <CoverageExclusions> 
    ///                   <CoverageExclusion>
	///						  <ExclusionType>Class</ExclusionType>
	///						  <Pattern>MyApp.SomeNamespace.SomeClass</Pattern> 
    ///                   </CoverageExclusion> 
    ///                   <CoverageExclusion>
	///						  <ExclusionType>Namespace</ExclusionType>
	///						  <Pattern>MyApp\.(\w*\.)?</Pattern> 
	///						  <Pattern>true</Pattern> 
    ///                   </CoverageExclusion> 
    ///                </CoverageExclusions> 
    ///            </PropertyGroup>
    /// 
    ///            <NCoverExplorer ToolPath="Tools\NCoverExplorer\"
    ///                            ProjectName="My Project" 
    ///							   ReportType="ModuleNamespaceSummary"
	///							   Sort="Name"
	///							   Filter="None"
	///							   OutputDir="$(MSBuildProjectDirectory)\Test\Coverage"
	///							   XmlReportName="CoverageSummary.xml"
	///							   HtmlReportName="CoverageSummary.html"
    ///							   MergeFileName="CoverageMerge.xml"
    ///							   ShowExcluded="True"
	///							   SatisfactoryCoverage="80"
    ///							   MinimumCoverage="80"
    ///							   FailMinimum="True"
	///							   CoverageFiles="@(CoverageFile)"
    ///							   Exclusions="$(CoverageExclusions)"
    ///							   ModuleThresholds="MyProject.1.dll=75;MyProject.2.dll=85"
	///			   />
	///            ]]>
	///        </code>
	/// </example>
	public class NCoverExplorer : ToolTask
	{
		#region Private Variables / Constants

		private const string DefaultApplicationName = "NCoverExplorer.Console.exe";

		private string _projectName;
		private string _outputDir;
		private string _configName;
		private string _xmlReportName;
		private string _htmlReportName;
        private string _mergeFileName;
        private bool _showExcluded;
		private bool _failMinimum;
        private bool _failCombinedMinimum;
        private float _satisfactoryCoverage;
        private float _minimumCoverage;
        private ITaskItem[] _coverageFiles;
		private string _coverageExclusions;
		private ITaskItem[] _moduleThresholds;
		private CoverageReportType _reportType;
		private TreeSortStyle _treeSortStyle;
		private TreeFilterStyle _treeFilterStyle;

		#endregion Private Variables / Constants

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="NCoverExplorer"/> class.
		/// </summary>
		public NCoverExplorer()
		{
			_configName = string.Empty;
			_satisfactoryCoverage = 100;
            _minimumCoverage = 100;
            _reportType = CoverageReportType.ModuleSummary;
			_treeSortStyle = TreeSortStyle.Name;
			_treeFilterStyle = TreeFilterStyle.None;
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// Gets or sets the output directory for the reports.
		/// </summary>
		/// <value>The output dir.</value>
		public string OutputDir
		{
			get { return _outputDir; }
			set { _outputDir = value; }
		}

        /// <summary>
        /// Whether to fail the task if the minimumCoverage threshold is not reached on any module. 
        /// NCoverExplorer console application will return exit code 3.
        /// </summary>
        public bool FailMinimum
        {
            get { return _failMinimum; }
            set { _failMinimum = value; }
        }

        /// <summary>
        /// Whether to fail the task if the minimumCoverage threshold is not reached on total coverage. 
        /// NCoverExplorer console application will return exit code 3.
        /// </summary>
        public bool FailCombinedMinimum
        {
            get { return _failCombinedMinimum; }
            set { _failCombinedMinimum = value; }
        }

        /// <summary>
        /// The minimum coverage percentage to be used with the FailMinimum and FailCombinedMinimum options.
        /// </summary>
        public float MinimumCoverage
        {
            get { return _minimumCoverage; }
            set { _minimumCoverage = value; }
        }

		/// <summary>
		/// Gets or sets the name of the temporary XML config file being generated for coverage.
		/// </summary>
		/// <value>The name of the XML config.</value>
		public string ConfigName
		{
			get { return _configName; }
			set { _configName = value; }
		}

		/// <summary>
		/// The satisfactory coverage percentage for display in the reports.
		/// </summary>
		public float SatisfactoryCoverage
		{
			get { return _satisfactoryCoverage; }
			set { _satisfactoryCoverage = value; }
		}

		/// <summary>
		/// The .config filename for containing any custom exclusions and parameters.
		/// </summary>
		public string ProjectName
		{
			get { return _projectName; }
			set { _projectName = value; }
		}

		/// <summary>
		/// The type of report to produce (use numeric value or string name). 
		/// 0 / None, 1 / ModuleSummary, 3 / ModuleNamespaceSummary, 
        /// 4 / ModuleClassSummary, 5 / ModuleClassFunctionSummary
		/// </summary>
		public string ReportType
		{
			get { return _reportType.ToString(); }
			set
			{
				_reportType = (CoverageReportType)Enum.Parse(typeof(CoverageReportType), value);
				if (!Enum.IsDefined(typeof(CoverageReportType), _reportType))
				{
					throw new ArgumentOutOfRangeException("value", value, "Not a valid report type.");
				}
			}
		}

		/// <summary>
		/// The sorting if any to apply (use numeric value or string name).
		/// 0 / Name, 1 / ClassLine, 
		/// 2 / CoveragePercentageAscending, 3 / CoveragePercentageDescending,
		/// 4 / UnvisitedSequencePointsAscending, 5 / UnvisitedSequencePointsDescending,
		/// 6 / VisitCountAscending, 7 / VisitCountDescending,
		/// 8 / FunctionCoverageAscending, 9 / FunctionCoverageDescending
		/// </summary>
		public string Sort
		{
			get { return _treeSortStyle.ToString(); }
			set
			{
				_treeSortStyle = (TreeSortStyle)Enum.Parse(typeof(TreeSortStyle), value);
				if (!Enum.IsDefined(typeof(TreeSortStyle), _treeSortStyle))
				{
					throw new ArgumentOutOfRangeException("value", value, "Not a valid sort style.");
				}
			}
		}

		/// <summary>
		/// The filtering if any to apply (use numeric value or string name).
		/// 0 / None, 1 / HideUnvisited, 2 / HideFullyCovered, 3 / HideThresholdCovered
		/// </summary>
		/// <value>The string or textual enum value.</value>
		public string Filter
		{
			get { return _treeFilterStyle.ToString(); }
			set
			{
				_treeFilterStyle = (TreeFilterStyle)Enum.Parse(typeof(TreeFilterStyle), value);
				if (!Enum.IsDefined(typeof(TreeFilterStyle), _treeFilterStyle))
				{
					throw new ArgumentOutOfRangeException("value", value, "Not a valid filter style.");
				}
			}
		}

		/// <summary>
		/// The filename for generating an xml report.
		/// </summary>
		public string XmlReportName
		{
			get { return _xmlReportName; }
			set { _xmlReportName = value; }
		}

		/// <summary>
		/// The filename for generating an html report.
		/// </summary>
		public string HtmlReportName
		{
			get { return _htmlReportName; }
			set { _htmlReportName = value; }
		}

        /// <summary>
        /// The filename for the merge of the coverage xml files.
        /// </summary>
        public string MergeFileName
        {
            get { return _mergeFileName; }
            set { _mergeFileName = value; }
        }

		/// <summary>
		/// Determines whether to include the coverage exclusions in the report. The default is 
		/// <see langword="true" />.
		/// </summary>
		public bool ShowExcluded
		{
			get { return _showExcluded; }
			set { _showExcluded = value; }
		}

		/// <summary>
		/// Used to select the coverage xml files to merge into the report.
		/// </summary>
		[Required]
		public ITaskItem[] CoverageFiles
		{
			get { return _coverageFiles; }
			set { _coverageFiles = value; }
		}

		/// <summary>
		/// Coverage exclusions to apply, in one of two formats:
        /// They can be semi-colon delimited "Type=Pattern" pairs, e.g. "Assembly=*.Tests;Class=My.*".
        /// Alternatively they can be defined in a property group as a &lt;CoverageExclusions&gt; section.
        /// See the examples for both formats. If you want to use regular expressions then you must
        /// use the &lt;PropertyGroup&gt; approach.
		/// </summary>
        /// <example>
        /// This example shows a range of coverage exclusions using the &lt;PropertyGroup&gt; approach.
        /// Note the optional use of wildcard characters in the pattern. You could set the exclusions
        /// up within the gui and then paste the xml directly from the NCoverExplorer.config file located
        /// in C:\Documents and Settings\user\Application Data\Gnoso\NCoverExplorer\
        ///        <code>
        ///            <![CDATA[
        ///            <PropertyGroup> 
        ///                <CoverageExclusions> 
        ///                   <CoverageExclusion>
        ///						  <ExclusionType>Assembly</ExclusionType>
        ///						  <Pattern>*.Tests</Pattern> 
        ///                   </CoverageExclusion> 
        ///                   <CoverageExclusion>
        ///						  <ExclusionType>Namespace</ExclusionType>
        ///						  <Pattern>MyNamespace.*</Pattern> 
        ///                   </CoverageExclusion> 
        ///                   <CoverageExclusion>
        ///						  <ExclusionType>Class</ExclusionType>
        ///						  <Pattern>MyNamespace.MyClass</Pattern> 
        ///                   </CoverageExclusion> 
        ///                   <CoverageExclusion>
        ///						  <ExclusionType>Method</ExclusionType>
        ///						  <Pattern>MyNamespace.MyClass.MyMethod</Pattern> 
        ///                   </CoverageExclusion> 
        ///                   <CoverageExclusion>
        ///						  <ExclusionType>Namespace</ExclusionType>
        ///						  <Pattern>MyApp\.(\w*\.)?</Pattern> 
        ///						  <IsRegex>true</IsRegex> 
        ///                   </CoverageExclusion> 
        ///                </CoverageExclusions> 
        ///            </PropertyGroup>
        /// 
        ///            <NCoverExplorer ...
        ///							   Exclusions="$(CoverageExclusions)"
        ///							   ...
        ///			   />
        ///            ]]>
        ///        </code>
        /// </example>
        /// <example>
        /// This example shows inlining of three of the same exclusions above. Note with this approach
        /// it is not possible to use regular expressions.
        ///        <code>
        ///            <![CDATA[
        ///            <NCoverExplorer ...
        ///							   Exclusions="Assembly=*.Tests;Namespace=MyNamespace.*;Class=MyNamespace.MyClass"
        ///							   ...
        ///			   />
        ///            ]]>
        ///        </code>
        /// </example>
		public string Exclusions
		{
			get { return _coverageExclusions; }
			set { _coverageExclusions = value; }
		}

		/// <summary>
		/// Module thresholds to apply, in format "AssemblyName=Percentage", e.g. "MyApp.Core=75"
		/// </summary>
		public ITaskItem[] ModuleThresholds
		{
			get { return _moduleThresholds; }
			set { _moduleThresholds = value; }
		}

		#endregion Properties

		#region Override ToolTask

		/// <summary>
        /// Validate the parameters supplied to this task.
        /// </summary>
        /// <returns><c>true</c> if parameters are valid, <c>false</c> otherwise.</returns>
        protected override bool ValidateParameters()
        {
            if (_coverageFiles.Length == 0)
            {
                Log.LogError("No matching coverage files found in the CoverageFiles attribute in your <NCoverExplorer> MSBuild task.");
                return false;
            }
            return base.ValidateParameters();
        }

		/// <summary>
		/// Executes the task.
		/// </summary>
		/// <returns><see langword="true"/> if the task ran successfully; otherwise <see langword="false"/>.</returns>
		public override bool Execute()
		{
			bool success = base.Execute();

			_CleanupConfigFile();
			
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
            string settingsFileName = _GetConfigFilename();
            StringBuilder builder = new StringBuilder();

            // Build the xml file containing settings which cannot be directly supplied to the command
            // line, such as coverage exclusions and module thresholds.
            if (_IsLegacyNCoverExplorerConsole(Path.Combine(ToolPath, ToolName)))
            {
                _BuildLegacyTempConfigXmlFile(settingsFileName);
                _BuildLegacyArguments(builder);
            }
            else
            {
                _BuildTempConfigXmlFile(settingsFileName);
            }
            builder.AppendFormat(" /c:\"{0}\"", _GetConfigFilename());

            // Dump out the contents of the settings file in diagnostics mode.
            string fileContents = NCoverUtilities.GetFileContents(settingsFileName);
            Log.LogMessage(MessageImportance.Low, Properties.Resources.NCoverExplorerConfigContent, fileContents);

            return builder.ToString();
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
		/// Determine the path to NCoverExplorer. Either the user can specify it in the arguments to the task,
		/// or we look in the registry, program files and finally just assume it is in the path.
		/// </summary>
		private void _CheckToolPath()
		{
			string ncoverExplorerPath = (ToolPath == null) ? String.Empty : ToolPath.Trim();
			if (string.IsNullOrEmpty(ncoverExplorerPath))
			{
				// Not specified in the task arguments so might be in trouble if the user has not
				// installed it using TD.Net (which will have registry entry).
				// Let's hope the user installed it to under Program Files if not installed with TestDriven.Net
				ncoverExplorerPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
				ncoverExplorerPath = Path.Combine(ncoverExplorerPath, @"NCoverExplorer");
				try
				{
                    RegistryKey buildKey = Registry.CurrentUser.OpenSubKey(@"Software\KiwiNova\NCoverExplorer");
                    if (buildKey == null || buildKey.GetValue("InstallDir") == null)
                        {
                        Log.LogCommandLine(MessageImportance.Low, "Enter HKLM");
                            // Try looking under HKLM instead
                        RegistryKey alternateBuildKey = Registry.LocalMachine.OpenSubKey(@"Software\KiwiNova\NCoverExplorer");
                                if (alternateBuildKey == null)
                                {
                                    Log.LogError(Properties.Resources.NCoverExplorerNotFound);
                                }
                                else
                                {
                                    ncoverExplorerPath = alternateBuildKey.GetValue("InstallDir").ToString();
                            alternateBuildKey.Close();
                            }
                        }
						else
						{
							ncoverExplorerPath = buildKey.GetValue("InstallDir").ToString();
                        buildKey.Close();
					}
				}
				catch (Exception ex)
				{
					Log.LogErrorFromException(ex);
				}
				if (!File.Exists(Path.Combine(ncoverExplorerPath, ToolName)))
				{
					// Really in trouble now - better hope the user has NCoverExplorer in their path.
					ncoverExplorerPath = string.Empty;
				}
				ToolPath = ncoverExplorerPath;
			}
            else if (!Path.IsPathRooted(ncoverExplorerPath))
            {
                ToolPath = Path.Combine(Path.GetDirectoryName(this.BuildEngine.ProjectFileOfTaskNode), ncoverExplorerPath);
            }
        }

        /// <summary>
        /// Return a temporary filename for the config file for executing NCoverExplorer.Console.
        /// </summary>
        /// <returns>Configuration filename.</returns>
        private string _GetConfigFilename()
        {
            if (_configName == null || _configName.Length == 0)
            {
                _configName = Path.GetTempFileName();
            }
            return _configName;
        }

        /// <summary>
        /// Legacy NCoverExplorer.Console is considered prior to 1.4.0 (as 1.4.0 was when the settings
        /// file format was changed).
        /// </summary>
        /// <param name="exeName">NCoverExplorer.Console.exe path.</param>
        /// <returns><c>true</c> if version is prior to 1.4.0</returns>
        private bool _IsLegacyNCoverExplorerConsole(string exeName)
        {
            if (!File.Exists(exeName))
                return false;

            string version = PathSearch.GetVersionForExecutable(exeName, false);
            if (version.Length > 0)
            {
                int versionNumber = int.Parse(version.Replace(".", ""));
                return versionNumber < 140;
            }
            return false;
        }

        /// <summary>
        /// Builds a temporary NCoverExplorer configuration file which we can pass in the command line.
        /// We require this as the command line itself does not directly support all the argument combinations.
        /// </summary>
        /// <param name="settingsFileName">Name of the settings file.</param>
        private void _BuildLegacyTempConfigXmlFile(string settingsFileName)
        {
            using (Stream fileStream = File.Create(settingsFileName))
            {
                XmlTextWriter xmlTextWriter = new XmlTextWriter(fileStream, Encoding.UTF8);
                xmlTextWriter.Indentation = 2;
                xmlTextWriter.Formatting = Formatting.Indented;

                xmlTextWriter.WriteStartDocument();
                xmlTextWriter.WriteStartElement("NCoverExplorer");

                if (!string.IsNullOrEmpty(_projectName))
                {
                    xmlTextWriter.WriteElementString("ProjectName", _projectName);
                }

                xmlTextWriter.WriteElementString("SatisfactoryCoverageThreshold", _satisfactoryCoverage.ToString());
                xmlTextWriter.WriteElementString("TreeSortStyle", _treeSortStyle.ToString());
                xmlTextWriter.WriteElementString("TreeFilterStyle", _treeFilterStyle.ToString());

                xmlTextWriter.WriteStartElement("CoverageExclusions");
                if (_coverageExclusions != null && _coverageExclusions.Length > 0)
                {
                    if (_coverageExclusions.StartsWith("<"))
                    {
                        // The coverage exclusions have been specified as xml children inside a property group.
                        _WriteCoverageExclusionsFromXmlToTempConfigFile(xmlTextWriter);
                    }
                    else
                    {
                        // We need to split by parsing the semi-colon delimited pairs of values.
                        _WriteCoverageExclusionsFromPairsToTempConfigFile(xmlTextWriter);
                    }
                }
                xmlTextWriter.WriteEndElement();

                xmlTextWriter.WriteStartElement("ModuleThresholds");
                if (_moduleThresholds != null)
                {
                    _WriteModuleThresholdsToTempConfigFile(xmlTextWriter);
                }
                xmlTextWriter.WriteEndElement();

                xmlTextWriter.WriteEndElement(); // NCoverExplorer
                xmlTextWriter.WriteEndDocument();
                xmlTextWriter.Flush();

                fileStream.Close();
            }
        }

		/// <summary>
		/// Builds a temporary NCoverExplorer configuration file which we can pass in the command line.
		/// We require this as the command line itself does not directly support all the argument combinations.
		/// </summary>
        /// <param name="settingsFileName">Name of the settings file.</param>
        private void _BuildTempConfigXmlFile(string settingsFileName)
		{
            using (Stream fileStream = File.Create(settingsFileName))
			{
				XmlTextWriter xmlTextWriter = new XmlTextWriter(fileStream, Encoding.UTF8);
				xmlTextWriter.Indentation = 2;
				xmlTextWriter.Formatting = Formatting.Indented;

				xmlTextWriter.WriteStartDocument();
                xmlTextWriter.WriteStartElement("ConsoleSetting");

                xmlTextWriter.WriteElementString("ProjectName", _projectName);
                xmlTextWriter.WriteElementString("SatisfactoryCoverageThreshold", _satisfactoryCoverage.ToString());
				xmlTextWriter.WriteElementString("TreeSortStyle", _treeSortStyle.ToString());
				xmlTextWriter.WriteElementString("TreeFilterStyle", _treeFilterStyle.ToString());

				xmlTextWriter.WriteStartElement("CoverageExclusions");
                if (_coverageExclusions != null && _coverageExclusions.Length > 0)
				{
                    if (_coverageExclusions.StartsWith("<"))
                    {
                        // The coverage exclusions have been specified as xml children inside a property group.
                        _WriteCoverageExclusionsFromXmlToTempConfigFile(xmlTextWriter);
                    }
                    else
                    {
                        // We need to split by parsing the semi-colon delimited pairs of values.
                        _WriteCoverageExclusionsFromPairsToTempConfigFile(xmlTextWriter);
                    }
				}
				xmlTextWriter.WriteEndElement();

				xmlTextWriter.WriteStartElement("ModuleThresholds");
				if (_moduleThresholds != null)
				{
                    _WriteModuleThresholdsToTempConfigFile(xmlTextWriter);
				}
				xmlTextWriter.WriteEndElement();

                // Append the file names of all the coverage.xml files we want to parse.
                xmlTextWriter.WriteStartElement("CoverageFileNames");
                foreach (ITaskItem fileName in _coverageFiles)
                {
                    xmlTextWriter.WriteElementString("CoverageFileName", fileName.ItemSpec);
                }
                xmlTextWriter.WriteEndElement(); // CoverageFileNames

                xmlTextWriter.WriteElementString("ReportType", _reportType.ToString());
                if (!string.IsNullOrEmpty(_htmlReportName))
                {
                    if (!Path.IsPathRooted(_htmlReportName) && !string.IsNullOrEmpty(_outputDir))
                    {
                        _htmlReportName = Path.Combine(_outputDir, _htmlReportName);
                    }
                    xmlTextWriter.WriteElementString("HtmlReportFileName", _htmlReportName);
                }
                if (!string.IsNullOrEmpty(_xmlReportName))
                {
                    if (!Path.IsPathRooted(_xmlReportName) && !string.IsNullOrEmpty(_outputDir))
                    {
                        _xmlReportName = Path.Combine(_outputDir, _xmlReportName);
                    }
                    xmlTextWriter.WriteElementString("XmlReportFileName", _xmlReportName);
                }
                if (!string.IsNullOrEmpty(_mergeFileName))
                {
                    if (!Path.IsPathRooted(_mergeFileName) && !string.IsNullOrEmpty(_outputDir))
                    {
                        _mergeFileName = Path.Combine(_outputDir, _mergeFileName);
                    }
                    xmlTextWriter.WriteElementString("MergeFileName", _mergeFileName);
                }
                xmlTextWriter.WriteElementString("ShowExcludedFooter", XmlConvert.ToString(_showExcluded));
                xmlTextWriter.WriteElementString("FailIfBelowMinimum", XmlConvert.ToString(_failMinimum));
                xmlTextWriter.WriteElementString("FailIfBelowCombinedMinimum", XmlConvert.ToString(_failCombinedMinimum));
                xmlTextWriter.WriteElementString("MinimumCoverage", _minimumCoverage.ToString());

                xmlTextWriter.WriteEndElement(); // ConsoleSetting
				xmlTextWriter.WriteEndDocument();
				xmlTextWriter.Flush();

				fileStream.Close();
			}
		}

        /// <summary>
        /// The coverage exclusions have been inlined as type=pattern semi-colon delimited pairs.
        /// Break apart and write to the temp config file.
        /// </summary>
        /// <param name="xmlTextWriter">Current xml output stream.</param>
        private void _WriteCoverageExclusionsFromPairsToTempConfigFile(XmlTextWriter xmlTextWriter)
        {
            string[] exclusionPairs = _coverageExclusions.Split(';');
            foreach (string coverageExclusion in exclusionPairs)
            {
                xmlTextWriter.WriteStartElement("CoverageExclusion");
                string[] exclusionElements = coverageExclusion.Split('=');
                if (exclusionElements.Length != 2)
                {
                    Log.LogError(Properties.Resources.NCoverExplorerInvalidExclusion, coverageExclusion);
                    throw new InvalidDataException(string.Format(Properties.Resources.NCoverExplorerInvalidExclusion,
                        coverageExclusion));
                }
                xmlTextWriter.WriteElementString("ExclusionType", exclusionElements[0].ToString());
                xmlTextWriter.WriteElementString("Pattern", exclusionElements[1].ToString());
                xmlTextWriter.WriteElementString("IsRegex", XmlConvert.ToString(false));
                xmlTextWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// The coverage exclusions have been inlined as type=pattern semi-colon delimited pairs.
        /// Break apart and write to the temp config file.
        /// </summary>
        /// <param name="xmlTextWriter">Current xml output stream.</param>
        private void _WriteCoverageExclusionsFromXmlToTempConfigFile(XmlTextWriter xmlTextWriter)
        {
            // Need to remove the namespace crap that MSBuild has inserted.
            string pattern = "\\s*xmlns\\s*=\\s*\"[^\"]*\"";
            _coverageExclusions = Regex.Replace(_coverageExclusions, pattern, "");
            xmlTextWriter.WriteRaw(_coverageExclusions);
        }

        /// <summary>
        /// Iterate through the module thresholds and write their values into the configuration file.
        /// </summary>
        /// <param name="xmlTextWriter">Current xml output stream.</param>
        private void _WriteModuleThresholdsToTempConfigFile(XmlTextWriter xmlTextWriter)
        {
            foreach (ITaskItem moduleThreshold in _moduleThresholds)
            {
                xmlTextWriter.WriteStartElement("ModuleThreshold");
                string[] thresholdElements = moduleThreshold.ItemSpec.Split('=');
                if (thresholdElements.Length != 2)
                {
                    Log.LogError(Properties.Resources.NCoverExplorerInvalidExclusion, moduleThreshold.ItemSpec);
                    throw new InvalidDataException(string.Format(Properties.Resources.NCoverExplorerInvalidExclusion,
                        moduleThreshold.ItemSpec));
                }
                xmlTextWriter.WriteAttributeString("ModuleName", thresholdElements[0].ToString());
                xmlTextWriter.WriteAttributeString("SatisfactoryCoverage", thresholdElements[1].ToString());
                xmlTextWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Build command line for passing to legacy NCoverExplorer.Console versions.
        /// </summary>
        private void _BuildLegacyArguments(StringBuilder builder)
        {
            builder.AppendFormat(" /r:{0}", _reportType.ToString());
            if (!string.IsNullOrEmpty(_htmlReportName))
            {
                if (!Path.IsPathRooted(_htmlReportName) && !string.IsNullOrEmpty(_outputDir))
                {
                    _htmlReportName = Path.Combine(_outputDir, _htmlReportName);
                }
                builder.AppendFormat(" /h:\"{0}\"", _htmlReportName);
            }
            if (!string.IsNullOrEmpty(_xmlReportName))
            {
                if (!Path.IsPathRooted(_xmlReportName) && !string.IsNullOrEmpty(_outputDir))
                {
                    _xmlReportName = Path.Combine(_outputDir, _xmlReportName);
                }
                builder.AppendFormat(" /x:\"{0}\"", _xmlReportName);
            }
            if (!string.IsNullOrEmpty(_mergeFileName))
            {
                if (!Path.IsPathRooted(_mergeFileName) && !string.IsNullOrEmpty(_outputDir))
                {
                    _mergeFileName = Path.Combine(_outputDir, _mergeFileName);
                }
                builder.AppendFormat(" /s:\"{0}\" ", _mergeFileName);
            }
            if (_showExcluded)
            {
                builder.Append(" /e");
            }
            if (_failMinimum)
            {
                builder.Append(" /f");
            }

            // Finally append the file names of all the coverage.xml files we want to parse.
            foreach (ITaskItem fileName in _coverageFiles)
            {
                builder.AppendFormat(" \"{0}\"", fileName.ItemSpec);
            }
        }

        /// <summary>
        /// Removes generated settings file after process has run.
        /// </summary>
        private void _CleanupConfigFile()
		{
			string filename = _GetConfigFilename();
			if (File.Exists(filename))
			{
				Log.LogMessage(MessageImportance.Low, Properties.Resources.NCoverExplorerDeletingConfig, filename);
				File.Delete(filename);
			}
		}

		#endregion Private Methods
	}
}
