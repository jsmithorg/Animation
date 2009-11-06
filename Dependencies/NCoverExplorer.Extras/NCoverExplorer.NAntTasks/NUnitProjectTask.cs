using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NCoverExplorer.NAntTasks
{
	/// <summary>
	/// Create a .nunit project file for all the test assemblies matching the specified pattern.
	/// This should be created in the bin folder where your test assemblies are located so that 
	/// the assemblies are within the AppDomain path.
	/// The .nunit file can then be used by NUnit or NCover based tasks.
	/// </summary>
	/// <example>
	///   <para>
	///   Create a .nunit project file in output bin folder for a specified test assembly.
	///   </para>
	///   <code>
	///     <![CDATA[
	/// <nunitproject project="C:\MyApp\bin\MyApp.nunit">
	///     <fileset basedir="C:\MyApp\bin">
	///         <include name="MyApp.Tests.dll" />
	///     </fileset>
	/// </nunitproject>
	///     ]]>
	///   </code>
	/// </example>
	/// <example>
	///   <para>
	///   Create a .nunit project file in output bin folder with an associated App.Config file for
	///   all test assemblies matching a pattern.
	///   </para>
	///   <code>
	///     <![CDATA[
	/// <nunitproject project="C:\MyApp\bin\MyApp.nunit" appConfig="C:\MyApp\bin\App.config">
	///     <fileset basedir="C:\MyApp\bin">
	///         <include name="MyApp.*.Tests.dll" />
	///     </fileset>
	/// </nunitproject>
	///     ]]>
	///   </code>
	/// </example>
	[TaskName("nunitproject")]
	public class NUnitProjectTask : Task 
	{
		#region Private Variables

		private FileInfo _project;
		private string _appConfig;
		private string _appBase;
		private FileSet _assemblyFiles = new FileSet();

		#endregion Private Variables

		#region Constructor

		/// <summary>
		/// Default constructor.
		/// </summary>
		public NUnitProjectTask()
		{
			_appConfig = string.Empty;
			_appBase = string.Empty;
		}

		#endregion Constructor

		#region Public Properties

		/// <summary>
		/// The nunit project file to create.
		/// </summary>
		[TaskAttribute("project")]
		public FileInfo ProjectFile 
		{
			get { return _project; }
			set { _project = value; }
		}

		/// <summary>
		/// Optional path to an App.Config file to be specified in the .nunit project file.
		/// </summary>
		[TaskAttribute("appConfig")]
		public string AppConfig
		{
			get { return _appConfig; }
			set { _appConfig = value; }
		}

		/// <summary>
		/// Optional path to the nunit app base, when included full paths to each assembly 
		/// (relative to the appbase) are included.
		/// </summary>
		[TaskAttribute("appBase", Required=false)]
		public string AppBase
		{
			get { return _appBase; }
			set { _appBase = value; }
		}

		/// <summary>
		/// Used to select the test assemblies to be included in the .nunit project. To use see <see cref="FileSet" />.
		/// </summary>
		[BuildElement("fileset")]
		public FileSet TestingFileSet 
		{
			get { return _assemblyFiles; }
			set { _assemblyFiles = value; }
		}

		#endregion Public Properties

		#region Override implementation of Task

		/// <summary>
		/// Checks whether the task is initialized with valid attributes.
		/// </summary>
		/// <param name="taskNode">The <see cref="XmlNode" /> used to initialize the task.</param>
		protected override void InitializeTask(XmlNode taskNode) 
		{
			if (this.ProjectFile == null)
			{
				throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
					"The .nunit project file must be specified."), Location);
			}
			if (TestingFileSet == null || TestingFileSet.Includes.Count == 0)
			{
				throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
					"The test assembly fileset must be specified."), Location);
			}
		}

		/// <summary>
		/// Build the contents of the .nunit file using the test assemblies matching this pattern.
		/// </summary>
		protected override void ExecuteTask()  
		{
			Log(Level.Verbose, "Creating {0} for test assemblies.", this.ProjectFile.FullName);

			_CreateNUnitProjectFile(this.ProjectFile.FullName, this.TestingFileSet, this.AppConfig, this.AppBase);
			
			Log(Level.Info, "{0} created.", this.ProjectFile.FullName);
		}	

		#endregion Override implementation of Task

		#region Private Instance Methods

		/// <summary>
		/// Create a .nunit project file listing the test assemblies.
		/// </summary>
		/// <param name="nunitProjectFileName">Full filename of the .nunit file.</param>
		/// <param name="testFileSet">Fileset containing the test assemblies.</param>
		/// <param name="appConfig">Optional path to App.Config file to include in project.</param>
		/// <param name="appBase">Optional path to the nunit app base, when included full paths to each assembly (relative to the appbase) are included</param>
		private void _CreateNUnitProjectFile(string nunitProjectFileName, FileSet testFileSet, string appConfig, string appBase)
		{
			XmlTextWriter writer = null;
			try
			{
				// Delete any existing .nunit project file
				if (File.Exists(nunitProjectFileName))
				{
					File.Delete(nunitProjectFileName);
				}

				// Determine if we're going to use appbase
				bool useAppBase = (appBase != null && appBase.Length > 0);

				// Get the directory info for the app base
				DirectoryInfo appBaseDirectory = null;
				if (useAppBase)
				{
					appBaseDirectory = new DirectoryInfo(appBase);
				}

				// Create a new .nunit project file (which has xml format).
				writer = new XmlTextWriter(nunitProjectFileName, Encoding.Default);
				writer.Formatting = Formatting.Indented;

				//<NUnitProject>
				writer.WriteStartElement("NUnitProject");

				//	<Settings activeconfig="Debug" appbase="C:\Projects\TestProject" />
				writer.WriteStartElement("Settings");
				writer.WriteAttributeString("activeConfig", "Debug");
				if (useAppBase)
				{
					writer.WriteAttributeString("appbase", appBase);
				}
				writer.WriteEndElement();

				//  <Config name="Debug" configfile="App.config" binpathtype="Auto">
				writer.WriteStartElement("Config");
				writer.WriteAttributeString("name", "Debug");
				if (appConfig != null && appConfig.Length > 0)
				{
					writer.WriteAttributeString("configfile", appConfig);
				}
				writer.WriteAttributeString("binpathtype","Auto");

				// Now iterate through each of the test assemblies.
				foreach (string pathName in testFileSet.FileNames)
				{
					FileInfo testAssemblyInfo = new FileInfo(pathName);
					if (testAssemblyInfo.Exists)
					{
						//    <assembly path="Bin\MyApplication.Testing.Business.dll" />
						writer.WriteStartElement("assembly");
                       if (useAppBase)
                        {
                            writer.WriteAttributeString("path", testAssemblyInfo.FullName.Substring(appBaseDirectory.FullName.Length + 1));
                        }
                        else
                        {
                            writer.WriteAttributeString("path", testAssemblyInfo.Name);
                        }
						writer.WriteEndElement();
					}
				}

				//  </Config>
				writer.WriteEndElement();

				//  <Config name="Release" binpathtype="Auto" />
				writer.WriteStartElement("Config");
				writer.WriteAttributeString("name", "Release");
				writer.WriteAttributeString("binpathtype","Auto");
				writer.WriteEndElement();

				//</NUnitProject>
				writer.WriteEndElement();
			}
			catch (Exception ex)
			{
				Log(Level.Error, ex.Message);
				string errorMessage = string.Format(CultureInfo.InvariantCulture, 
				    "An error occurred while creating file: {0}", nunitProjectFileName);

				throw new BuildException(errorMessage, Location, ex);
			}
			finally
			{
				if (writer != null)
				{
					writer.Flush();
					writer.Close();
				}
			}
		}

		#endregion Private Instance Methods
	}
}
