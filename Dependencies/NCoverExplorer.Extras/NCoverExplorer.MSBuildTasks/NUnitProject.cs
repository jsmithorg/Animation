using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NCoverExplorer.MSBuildTasks
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
    /// <UsingTask TaskName="NCoverExplorer.MSBuildTasks.NUnitProject" 
    ///            AssemblyFile="NCoverExplorer.MSBuildTasks.dll"/>
    /// <PropertyGroup>
    ///    <OutputPath>$(MSBuildProjectDirectory)\Build</OutputPath>
    /// </PropertyGroup>
    ///
    ///	<ItemGroup>
    ///	   <Assembly Include="$(OutputPath)\MyApp.Tests.dll" />
    ///	</ItemGroup>
    /// 
    /// <NUnitProject   Project="$(OutputPath)\MyApp.nunit" 
    ///                 Assemblies="@(Assembly)" />
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
    ///	<ItemGroup>
    ///	   <Assembly Include="$(OutputPath)\MyApp.*.Tests.dll" />
    ///	</ItemGroup>
    /// 
    /// <NUnitProject   Project="$(OutputPath)\MyApp.nunit" 
    ///                 AppConfig="$(OutputPath)\MyApp.exe.config"
    ///                 Assemblies="@(Assembly)" />
    ///     ]]>
    ///   </code>
    /// </example>
    public class NUnitProject : Task
    {
        #region Private Variables

        private string _project;
        private string _appConfig;
        private string _appBase;
        private ITaskItem[] _assemblies;

        #endregion Private Variables

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NUnitProject()
        {
            _appConfig = string.Empty;
            _appBase = string.Empty;
        }

        #endregion Constructor

        #region Public Properties

        /// <summary>
        /// The nunit project file to create.
        /// </summary>
        [Required]
        public string Project
        {
            get { return _project; }
            set { _project = value; }
        }

        /// <summary>
        /// Optional path to an App.Config file to be specified in the .nunit project file.
        /// </summary>
        public string AppConfig
        {
            get { return _appConfig; }
            set { _appConfig = value; }
        }

        /// <summary>
        /// Optional path to the nunit app base, when included full paths to each assembly 
        /// (relative to the appbase) are included.
        /// </summary>
        public string AppBase
        {
            get { return _appBase; }
            set { _appBase = value; }
        }

        /// <summary>
        /// Used to select the test assemblies to be included in the .nunit project. 
        /// </summary>
        [Required]
        public ITaskItem[] Assemblies
        {
            get { return _assemblies; }
            set { _assemblies = value; }
        }

        #endregion Public Properties

        #region Override Task

        /// <summary>
        /// Build the contents of the .nunit file using the test assemblies matching this pattern.
        /// </summary>
        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.Low, "Creating {0} for test assemblies.", _project);

            _CreateNUnitProjectFile(_project, this.Assemblies, this.AppConfig, this.AppBase);

            Log.LogMessage(MessageImportance.Low, "{0} created", _project);
            return true;
        }

        #endregion Override Task

        #region Private Instance Methods

        /// <summary>
        /// Create a .nunit project file listing the test assemblies.
        /// </summary>
        /// <param name="nunitProjectFileName">Full filename of the .nunit file.</param>
        /// <param name="testFileSet">Fileset containing the test assemblies.</param>
        /// <param name="appConfig">Optional path to App.Config file to include in project.</param>
        /// <param name="appBase">Optional path to the nunit app base, when included full paths to each assembly (relative to the appbase) are included</param>
        private void _CreateNUnitProjectFile(string nunitProjectFileName, ITaskItem[] testFileSet, string appConfig, string appBase)
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
                writer.WriteAttributeString("activeconfig", "Debug");
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
                writer.WriteAttributeString("binpathtype", "Auto");

                // Now iterate through each of the test assemblies.
                foreach (ITaskItem pathName in testFileSet)
                {
                    FileInfo testAssemblyInfo = new FileInfo(pathName.ItemSpec);
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
                writer.WriteAttributeString("binpathtype", "Auto");
                writer.WriteEndElement();

                //</NUnitProject>
                writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
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
