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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

using NCoverExplorer.NAntTasks.Types;
using NCoverExplorer.Common;

namespace NCoverExplorer.NAntTasks
{
	/// <summary>
	/// NAnt task for automating NCoverExplorer.Console.
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
	///            <ncoverexplorer program="tools\ncoverexplorer\ncoverexplorer.console.exe" 
	///							   projectName="My Project"
	///							   outputDir="${build.reports}"
	///							   satisfactoryCoverage="80"
	///                            reportType="4"
	///							   xmlReportName="CoverageSummary.xml" >
	///                <fileset>
	///                    <include name="coverage.xml" />
	///                </fileset>
	///			   </ncoverexplorer>
	///            ]]>
	///        </code>
	/// </example>
	/// <example>
	/// This example shows producing an html function coverage report, excluding the test assemblies. The
	/// assemblies excluded are being displayed at the bottom of the report. Note also that this time
	/// the reportType is specified by its enum name rather than numeric value - they are interchangable.
	///        <code>
	///            <![CDATA[
	///            <ncoverexplorer program="tools\ncoverexplorer\ncoverexplorer.console.exe" 
	///							   projectName="My Project"
	///							   outputDir="${build.reports}"
	///							   satisfactoryCoverage="80"
	///                            reportType="ModuleClassFunctionSummary"
	///							   htmlReportName="CoverageSummary.html"
	///							   showExcluded="True" >
	///                <fileset>
	///                    <include name="coverage.xml" />
	///                </fileset>
	///                <exclusions>
	///                    <exclusion type="Assembly" pattern="*.Tests" />
	///                </exclusions>
	///			   </ncoverexplorer>
	///            ]]>
	///        </code>
	/// </example>
	/// <example>
	/// This example shows producing an html module class summary coverage report with exclusions as above.
	/// This time we have added applying specific sorting and filtering criteria. This report will show all
	/// classes that do not have 100% coverage, sorted within their namespaces by descending coverage %.
	///        <code>
	///            <![CDATA[
	///            <ncoverexplorer program="tools\ncoverexplorer\ncoverexplorer.console.exe" 
	///							   projectName="My Project"
	///							   outputDir="${build.reports}"
	///							   satisfactoryCoverage="80"
	///                            reportType="ModuleClassSummary"
	///							   htmlReportName="CoverageSummary.html"
	///							   showExcluded="True"
	///							   sort="CoveragePercentageDescending"
	///							   filter="HideFullyCovered" >
	///                <fileset>
	///                    <include name="coverage.xml" />
	///                </fileset>
	///                <exclusions>
	///                    <exclusion type="Assembly" pattern="*.Tests" />
	///                </exclusions>
	///			   </ncoverexplorer>
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
	///            <ncoverexplorer program="tools\ncoverexplorer\ncoverexplorer.console.exe" 
	///							   outputDir="${build.reports}"
	///							   reportType="None"
	///							   mergeFileName="MyApp.CoverageMerged.xml" >
	///                <fileset>
	///                    <include name="*.coverage.xml" />
	///                </fileset>
	///			   </ncoverexplorer>
	///            ]]>
	///        </code>
	/// </example>
	/// <example>
	/// This example shows failing a build if the overall coverage % does not meet our threshold, without
	/// producing a coverage report.
	///        <code>
	///            <![CDATA[
	///            <ncoverexplorer program="tools\ncoverexplorer\ncoverexplorer.console.exe" 
	///							   projectName="My Project"
	///							   reportType="None"
	///							   minimumCoverage="80"
	///							   failMinimum="True" >
	///                <fileset>
	///                    <include name="coverage.xml" />
	///                </fileset>
	///			   </ncoverexplorer>
	///            ]]>
	///        </code>
	/// </example>
	/// <example>
	/// This example shows failing a build if either the overall coverage % does not meet our threshold, or
	/// if one of the individual module thresholds is not met.
	///        <code>
	///            <![CDATA[
	///            <ncoverexplorer program="tools\ncoverexplorer\ncoverexplorer.console.exe" 
	///							   projectName="My Project"
	///							   reportType="None"
	///							   minimumCoverage="80"
	///							   failMinimum="True" >
	///                <fileset>
	///                    <include name="coverage.xml" />
	///                </fileset>
	///                <moduleThresholds>
	///                    <moduleThreshold moduleName="MyApp.1.dll" satisfactoryCoverage="75" />
	///                    <moduleThreshold moduleName="MyApp.2.dll" satisfactoryCoverage="85" />
	///                </moduleThresholds>
	///			   </ncoverexplorer>
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
	///            <ncoverexplorer program="tools\ncoverexplorer\ncoverexplorer.console.exe" 
	///							   projectName="My Project" 
	///							   reportType="ModuleNamespaceSummary"
	///							   sort="Name"
	///							   filter="None"
	///							   outputDir="${build.reports}"
	///							   xmlReportName="CoverageSummary.xml"
	///							   htmlReportName="CoverageSummary.html"
	///							   mergeFileName="CoverageMerge.xml"
	///							   showExcluded="True"
	///							   satisfactoryCoverage="80"
	///							   minimumCoverage="80"
	///							   failMinimum="True">
	///                <fileset>
	///                    <include name="coverage.xml" />
	///                </fileset>
	///                <exclusions>
	///                    <exclusion type="Assembly" pattern="*.Tests" />
	///                    <exclusion type="Namespace" pattern="MyApp.SomeNamespace" />
	///                    <exclusion type="Namespace" pattern="MyApp\.(\w*\.)?" isRegex="true" />
	///                    <exclusion type="Class" pattern="MyApp.SomeNamespace.SomeClass" />
	///                </exclusions>
	///                <moduleThresholds>
	///                    <moduleThreshold moduleName="MyApp.1.dll" satisfactoryCoverage="75" />
	///                    <moduleThreshold moduleName="MyApp.2.dll" satisfactoryCoverage="85" />
	///                </moduleThresholds>
	///			   </ncoverexplorer>
	///            ]]>
	///        </code>
	/// </example>
	[TaskName("ncoverexplorer")]
	public class NCoverExplorerTask : NAnt.Core.Tasks.ExternalProgramBase
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
		private CoverageReportType _reportType;
		private TreeSortStyle _treeSortStyle;
		private TreeFilterStyle _treeFilterStyle;
		private FileSet _coverageFiles;
		private CoverageExclusionCollection _coverageExclusions;
		private ModuleThresholdCollection _moduleThresholds;

		private StringBuilder _programArguments;

		#endregion Private Variables / Constants

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="NCoverExplorerTask"/> class.
		/// </summary>
		public NCoverExplorerTask()
		{
			_coverageFiles = new FileSet();
			_configName = string.Empty;
			_satisfactoryCoverage = 100;
			_minimumCoverage = 100;
			_reportType = CoverageReportType.ModuleSummary;
			_treeSortStyle = TreeSortStyle.Name;
			_treeFilterStyle = TreeFilterStyle.None;

			_programArguments = new StringBuilder();
			_coverageExclusions = new CoverageExclusionCollection();
			_moduleThresholds = new ModuleThresholdCollection();

			ExeName = DefaultApplicationName;
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// The name of the executable that should be used to launch the
		/// external program.
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
		/// Gets or sets the output directory for the reports.
		/// </summary>
		/// <value>The output dir.</value>
		[TaskAttribute("outputDir", Required=false)]
		[StringValidator(AllowEmpty=false)]
		public string OutputDir
		{
			get { return _outputDir; }
			set { _outputDir = value; }
		}

		/// <summary>
		/// Whether to fail the task if the minimumCoverage threshold is not reached on any module. 
		/// NCoverExplorer console application will return exit code 3.
		/// </summary>
		[TaskAttribute("failMinimum")]
		public bool FailMinimum
		{
			get { return _failMinimum; }
			set { _failMinimum = value; }
		}

		/// <summary>
		/// Whether to fail the task if the minimumCoverage threshold is not reached on total coverage. 
		/// NCoverExplorer console application will return exit code 3.
		/// </summary>
		[TaskAttribute("failCombinedMinimum")]
		public bool FailCombinedMinimum
		{
			get { return _failCombinedMinimum; }
			set { _failCombinedMinimum = value; }
		}

		/// <summary>
		/// The minimum coverage percentage to be used with the failMinimum and failCombinedMinimum options.
		/// </summary>
		[TaskAttribute("minimumCoverage")]
		public float MinimumCoverage
		{
			get { return _minimumCoverage; }
			set { _minimumCoverage = value; }
		}

		/// <summary>
		/// The satisfactory coverage percentage for display in the reports.
		/// </summary>
		[TaskAttribute("satisfactoryCoverage")]
		public float SatisfactoryCoverage
		{
			get { return _satisfactoryCoverage; }
			set { _satisfactoryCoverage = value; }
		}

		/// <summary>
		/// The project name to appear in the report.
		/// </summary>
		[TaskAttribute("projectName")]
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
		/// <value>The string or textual enum value.</value>
		[TaskAttribute("reportType")]
		public string ReportType
		{
			get { return _reportType.ToString(); }
			set 
			{ 
				_reportType = (CoverageReportType)Enum.Parse(typeof(CoverageReportType), value); 
				if (!Enum.IsDefined(typeof(CoverageReportType), _reportType))
				{
					throw new ArgumentOutOfRangeException("Not a valid report type.");
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
		/// <value>The string or textual enum value.</value>
		[TaskAttribute("sort")]
		public string Sort
		{
			get { return _treeSortStyle.ToString(); }
			set 
			{ 
				_treeSortStyle = (TreeSortStyle) Enum.Parse(typeof(TreeSortStyle), value); 
				if (!Enum.IsDefined(typeof(TreeSortStyle), _treeSortStyle))
				{
					throw new ArgumentOutOfRangeException("Not a valid sort style.");
				}
			}
		}

		/// <summary>
		/// The filtering if any to apply (use numeric value or string name).
		/// 0 / None, 1 / HideUnvisited, 2 / HideFullyCovered, 3 / HideThresholdCovered
		/// </summary>
		/// <value>The string or textual enum value.</value>
		[TaskAttribute("filter")]
		public string Filter
		{
			get { return _treeFilterStyle.ToString(); }
			set 
			{ 
				_treeFilterStyle = (TreeFilterStyle) Enum.Parse(typeof(TreeFilterStyle), value); 
				if (!Enum.IsDefined(typeof(TreeFilterStyle), _treeFilterStyle))
				{
					throw new ArgumentOutOfRangeException("Not a valid filter style.");
				}
			}
		}

		/// <summary>
		/// The filename for generating an xml report.
		/// </summary>
		[TaskAttribute("xmlReportName")]
		public string XmlReportName
		{
			get { return _xmlReportName; }
			set { _xmlReportName = value; }
		}

		/// <summary>
		/// The filename for generating an html report.
		/// </summary>
		[TaskAttribute("htmlReportName")]
		public string HtmlReportName
		{
			get { return _htmlReportName; }
			set { _htmlReportName = value; }
		}

		/// <summary>
		/// The filename for the merge of the coverage xml files.
		/// </summary>
		[TaskAttribute("mergeFileName")]
		public string MergeFileName
		{
			get { return _mergeFileName; }
			set { _mergeFileName = value; }
		}

		/// <summary>
		/// Determines whether to include the coverage exclusions in the report. The default is 
		/// <see langword="true" />.
		/// </summary>
		[TaskAttribute("showExcluded")]
		[BooleanValidator()]
		public bool ShowExcluded 
		{
			get { return _showExcluded; }
			set { _showExcluded = value; }
		}

		/// <summary>
		/// Coverage exclusions to apply.
		/// </summary>
		[BuildElementCollection("exclusions", "exclusion")]
		public CoverageExclusionCollection CoverageExclusions 
		{
			get { return _coverageExclusions; }
		}

		/// <summary>
		/// Coverage exclusions to apply.
		/// </summary>
		[BuildElementCollection("moduleThresholds", "moduleThreshold")]
		public ModuleThresholdCollection ModuleThresholds 
		{
			get { return _moduleThresholds; }
		}

		/// <summary>
		/// Used to select the coverage xml files to merge into the report. To use a <see cref="FileSet" />.
		/// </summary>
		[BuildElement("fileset")]
		public FileSet CoverageFiles 
		{
			get { return _coverageFiles; }
			set { _coverageFiles = value; }
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
			Process process = base.StartProcess();
			process.Exited += new System.EventHandler(_OnProcessExited);
			return process;
		}

		/// <summary>
		/// Performs logic before the external process is started.
		/// </summary>
		/// <param name="process">Process.</param>
		/// <exception cref="BuildException">Thrown if no matching coverage files.</exception>
		protected override void PrepareProcess(Process process) 
		{
			if (_coverageFiles.FileNames.Count == 0)
			{
				throw new BuildException("No matching coverage files found in the <fileset> specified in your <ncoverexplorer> NAnt task.");
			}
			string settingsFileName = _GetConfigFilename();

			if (_IsLegacyNCoverExplorerConsole(ExeName))
			{
				_BuildLegacyTempConfigXmlFile(settingsFileName);
				_BuildLegacyArguments();
			}
			else
			{
				_BuildTempConfigXmlFile(settingsFileName);
			}
			_programArguments.AppendFormat("/c:\"{0}\" ", settingsFileName);

			// Dump out the content of this file if verbose logging turned on.
			if (IsLogEnabledFor(Level.Verbose))
			{
				string fileContents = NCoverUtilities.GetFileContents(settingsFileName);
				Log(Level.Verbose, "Contents of NCoverExplorer settings file:");
				Log(Level.Verbose, fileContents);
			}
			Log(Level.Verbose, "Working directory: {0}", BaseDirectory);
			Log(Level.Verbose, "Arguments: {0}", ProgramArguments);

			base.PrepareProcess(process);
		}

		#endregion Override implementation of ExternalProgramBase

		#region Private Methods

		/// <summary>
		/// Removes generated config file after process has run.
		/// </summary>
		private void _OnProcessExited(object sender, System.EventArgs e)
		{
			string filename = _GetConfigFilename();
			if ( File.Exists( filename ) )
			{
				Log( Level.Verbose, "Deleting config file: " + filename );
				File.Delete( filename );
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
		/// Build the xml config file to pass to the NCoverExplorer.Console executable for legacy versions (prior to 1.4.0).
		/// </summary>
		private void _BuildLegacyTempConfigXmlFile(string settingsFileName)
		{
			using (Stream fileStream = File.Create(settingsFileName))
			{
				XmlTextWriter xmlTextWriter = new XmlTextWriter(fileStream, Encoding.UTF8);
				xmlTextWriter.Indentation = 2;
				xmlTextWriter.Formatting = Formatting.Indented;

				xmlTextWriter.WriteStartDocument();
				xmlTextWriter.WriteStartElement("NCoverExplorer");

				xmlTextWriter.WriteElementString("ProjectName", _projectName);
				xmlTextWriter.WriteElementString("SatisfactoryCoverageThreshold", XmlConvert.ToString(_satisfactoryCoverage));
				xmlTextWriter.WriteElementString("TreeSortStyle", _treeSortStyle.ToString());
				xmlTextWriter.WriteElementString("TreeFilterStyle", _treeFilterStyle.ToString());

				xmlTextWriter.WriteStartElement("CoverageExclusions");
				foreach (CoverageExclusion coverageExclusion in _coverageExclusions)
				{
					xmlTextWriter.WriteStartElement("CoverageExclusion");
					xmlTextWriter.WriteElementString("ExclusionType", coverageExclusion.ExclusionType);
					xmlTextWriter.WriteElementString("Pattern", coverageExclusion.Pattern);
					xmlTextWriter.WriteElementString("IsRegex", XmlConvert.ToString(coverageExclusion.IsRegex));
					xmlTextWriter.WriteElementString("Enabled", XmlConvert.ToString(coverageExclusion.Enabled));
					xmlTextWriter.WriteEndElement();
				}
				xmlTextWriter.WriteEndElement();

				xmlTextWriter.WriteStartElement("ModuleThresholds");
				foreach (ModuleThreshold moduleThreshold in _moduleThresholds)
				{
					xmlTextWriter.WriteStartElement("ModuleThreshold");
					xmlTextWriter.WriteAttributeString("ModuleName", moduleThreshold.ModuleName);
					xmlTextWriter.WriteAttributeString("SatisfactoryCoverage", XmlConvert.ToString(moduleThreshold.SatisfactoryCoverage));
					xmlTextWriter.WriteEndElement();
				}
				xmlTextWriter.WriteEndElement();

				xmlTextWriter.WriteEndElement(); // NCoverExplorer
				xmlTextWriter.WriteEndDocument();
				xmlTextWriter.Flush();

				fileStream.Close();
			}
		}

		/// <summary>
		/// Build the Xml config file to pass to the NCoverExplorer.Console executable.
		/// </summary>
		private void _BuildTempConfigXmlFile(string settingsFile)
		{
			using (Stream fileStream = File.Create(settingsFile))
			{
				XmlTextWriter xmlTextWriter = new XmlTextWriter(fileStream, Encoding.UTF8);
				xmlTextWriter.Indentation = 2;
				xmlTextWriter.Formatting = Formatting.Indented;

				xmlTextWriter.WriteStartDocument();
				xmlTextWriter.WriteStartElement("ConsoleSetting");

				xmlTextWriter.WriteElementString("ProjectName", _projectName);
				xmlTextWriter.WriteElementString("SatisfactoryCoverageThreshold", XmlConvert.ToString(_satisfactoryCoverage));
				xmlTextWriter.WriteElementString("TreeSortStyle", _treeSortStyle.ToString());
				xmlTextWriter.WriteElementString("TreeFilterStyle", _treeFilterStyle.ToString());
	
				xmlTextWriter.WriteStartElement("CoverageExclusions");
				foreach (CoverageExclusion coverageExclusion in _coverageExclusions)
				{
					xmlTextWriter.WriteStartElement("CoverageExclusion");
					xmlTextWriter.WriteElementString("ExclusionType", coverageExclusion.ExclusionType);
					xmlTextWriter.WriteElementString("Pattern", coverageExclusion.Pattern);
					xmlTextWriter.WriteElementString("IsRegex", XmlConvert.ToString(coverageExclusion.IsRegex));
					xmlTextWriter.WriteElementString("Enabled", XmlConvert.ToString(coverageExclusion.Enabled));
					xmlTextWriter.WriteEndElement();
				}
				xmlTextWriter.WriteEndElement();
					
				xmlTextWriter.WriteStartElement("ModuleThresholds");
				foreach (ModuleThreshold moduleThreshold in _moduleThresholds)
				{
					xmlTextWriter.WriteStartElement("ModuleThreshold");
					xmlTextWriter.WriteAttributeString("ModuleName", moduleThreshold.ModuleName);
					xmlTextWriter.WriteAttributeString("SatisfactoryCoverage", XmlConvert.ToString(moduleThreshold.SatisfactoryCoverage));
					xmlTextWriter.WriteEndElement();
				}
				xmlTextWriter.WriteEndElement();

				// Append the file names of all the coverage.xml files we want to parse.
				xmlTextWriter.WriteStartElement("CoverageFileNames");
				foreach (string fileName in _coverageFiles.FileNames)
				{
					xmlTextWriter.WriteElementString("CoverageFileName", fileName);
				}
				xmlTextWriter.WriteEndElement(); // CoverageFileNames

				xmlTextWriter.WriteElementString("ReportType", _reportType.ToString());
				if (!StringUtils.IsNullOrEmpty(_htmlReportName))
				{
					if (!Path.IsPathRooted(_htmlReportName) && !StringUtils.IsNullOrEmpty(_outputDir))
					{
						_htmlReportName = Path.Combine(_outputDir, _htmlReportName);
					}
					xmlTextWriter.WriteElementString("HtmlReportFileName", _htmlReportName);
				}
				if (!StringUtils.IsNullOrEmpty(_xmlReportName))
				{
					if (!Path.IsPathRooted(_xmlReportName) && !StringUtils.IsNullOrEmpty(_outputDir))
					{
						_xmlReportName = Path.Combine(_outputDir, _xmlReportName);
					}
					xmlTextWriter.WriteElementString("XmlReportFileName", _xmlReportName);
				}
				if (!StringUtils.IsNullOrEmpty(_mergeFileName))
				{
					if (!Path.IsPathRooted(_mergeFileName) && !StringUtils.IsNullOrEmpty(_outputDir))
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
		/// Build command line for passing to legacy NCoverExplorer.Console versions.
		/// </summary>
		private void _BuildLegacyArguments()
		{
			_programArguments.AppendFormat("/r:{0} ", _reportType);
			if (!StringUtils.IsNullOrEmpty(_htmlReportName))
			{
				if (!Path.IsPathRooted(_htmlReportName) && !StringUtils.IsNullOrEmpty(_outputDir))
				{
					_htmlReportName = Path.Combine(_outputDir, _htmlReportName);
				}
				_programArguments.AppendFormat("/h:\"{0}\" ", _htmlReportName);
			}
			if (!StringUtils.IsNullOrEmpty(_xmlReportName))
			{
				if (!Path.IsPathRooted(_xmlReportName) && !StringUtils.IsNullOrEmpty(_outputDir))
				{
					_xmlReportName = Path.Combine(_outputDir, _xmlReportName);
				}
				_programArguments.AppendFormat("/x:\"{0}\" ", _xmlReportName);
			}
			if (!StringUtils.IsNullOrEmpty(_mergeFileName))
			{
				if (!Path.IsPathRooted(_mergeFileName) && !StringUtils.IsNullOrEmpty(_outputDir))
				{
					_mergeFileName = Path.Combine(_outputDir, _mergeFileName);
				}
				_programArguments.AppendFormat("/s:\"{0}\" ", _mergeFileName);
			}
			if (_showExcluded)
			{
				_programArguments.Append("/e ");
			}
			if (_failMinimum)
			{
				_programArguments.Append("/f ");
			}

			foreach (string fileName in _coverageFiles.FileNames)
			{
				_programArguments.AppendFormat("\"{0}\" ", fileName);
			}
		}

		#endregion Private Methods
	}
}
