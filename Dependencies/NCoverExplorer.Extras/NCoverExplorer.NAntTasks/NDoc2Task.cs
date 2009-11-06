#region Copyright © 2007 Grant Drake. All rights reserved.
/*
Copyright © 2007 Grant Drake. All rights reserved.

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

// Adapted from the original <ndoc> NAnt task written by:
// Gerry Shaw (gerry_shaw@yahoo.com)
// Ian MacLean (ian_maclean@another.com)
// Giuseppe Greco (giuseppe.greco@agamura.com)

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

using NCoverExplorer.Common;

namespace NCoverExplorer.NAntTasks
{
	/// <summary>
	/// Runs NDoc2 Alpha to create documentation for .NET 2.0 assemblies. Unlike the original &lt;ndoc&gt; task, this one 
	/// simply runs the NDoc.Console.exe with the configuration file created.
	/// </summary>
	/// <remarks>
	///   <para>
	///   The easiest way to create the correct contents for this task is to use the NDocGui and save the resulting
	///   configuration file. Then just paste in the &lt;documenter&gt; and other sections you want into your NAnt script as is.
	///   See the <see href="http://www.kynosarges.de/NDoc.html">NDoc2 Alpha page</see> for more 
	///   information about NDoc Alpha.
	///   </para>
	/// </remarks>
	/// <example>
	///   <para>
	///   Document two assemblies using the MSDN-CHM documenter. The namespaces are documented in <c>NamespaceSummary.xml</c>. 
	///   This is assuming that the NDocConsole.exe is in your path. If not, use the "program" attribute on the ndoc2
	///   node to specify the path to NDocConsole.exe.
	///   </para>
	///   <code>
	///     <![CDATA[
	/// <ndoc2>
	///     <assemblies basedir="${build.dir}">
	///         <include name="MyApp.exe" />
	///         <include name="MyApp.dll" />
	///     </assemblies>
	///     <summaries basedir="${build.dir}">
	///         <include name="NamespaceSummary.xml" />
	///     </summaries>
	///     <documenters>
	///         <documenter name="MSDN-CHM">
	///             <property name="IncludeFavorites" value="False" />
	///             <property name="OutputDirectory" value="doc\MSDN" />
	///             <property name="HtmlHelpName" value="MyApp" />
	///             <property name="Title" value="MyApp Class Library" />
	///             <property name="Version" value="" />
	///				<property name="UseAssemblyShadowCache" value="False" />
	///				<property name="DocumentExplicitInterfaceImplementations" value="True" />
	///				<property name="AssemblyVersionInfo" value="AssemblyFileVersion" />
	///				<property name="CopyrightText" value="© 2007 KiwiNova" />
	///          </documenter>
	///     </documenters> 
	/// </ndoc2>
	///     ]]>
	///   </code>
	///   <para>Content of <c>NamespaceSummary.xml</c> :</para>
	///   <code>
	///     <![CDATA[
	/// <namespaces>
	///     <namespace name="Foo.Bar">
	///         The <b>Foo.Bar</b> namespace reinvents the wheel.
	///     </namespace>
	///     <namespace name="Foo.Bar.Tests">
	///         The <b>Foo.Bar.Tests</b> namespace ensures that the Foo.Bar namespace reinvents the wheel correctly.
	///     </namespace>
	/// </namespaces>
	///     ]]>
	///   </code>
	/// </example>
	[TaskName("ndoc2")]
	public class NDoc2Task : NAnt.Core.Tasks.ExternalProgramBase
	{
		#region Private Variables / Constants

		private const string DefaultApplicationName = "NDocConsole.exe";
		private const string DefaultConfigName = "NAnt.NDoc2.ndoc";

		private XmlNodeList _docNodes;
		private FileSet _assemblies;
		private FileSet _summaries;
		private RawXml _documenters;
		private DirSet _referencePaths;
		private string _configName;

		private StringBuilder _programArguments;

		#endregion Private Variables / Constants

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="NDoc2Task"/> class.
		/// </summary>
		public NDoc2Task()
		{
			_assemblies = new FileSet();
			_summaries = new FileSet();
			_referencePaths = new DirSet();

			_programArguments = new StringBuilder();

			_configName = DefaultConfigName;
			ExeName = DefaultApplicationName;
		}

		#endregion Constructors

		#region Public Properties

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
		/// The set of assemblies to document.
		/// </summary>
		[BuildElement("assemblies", Required=true)]
		public FileSet Assemblies 
		{
			get { return _assemblies; }
			set { _assemblies = value; }
		}

		/// <summary>
		/// The set of namespace summary files.
		/// </summary>
		[BuildElement("summaries")]
		public FileSet Summaries 
		{
			get { return _summaries; }
			set { _summaries = value; }
		}

		/// <summary>
		/// Specifies the formats in which the documentation should be generated.
		/// </summary>
		[BuildElement("documenters", Required=true)]
		public RawXml Documenters 
		{
			get { return _documenters; }
			set { _documenters = value; }
		}

		/// <summary>
		/// Collection of additional directories to search for referenced 
		/// assemblies.
		/// </summary>
		[BuildElement("referencepaths")]
		public DirSet ReferencePaths 
		{
			get { return _referencePaths; }
			set { _referencePaths = value; }
		}

		#endregion Public Properties

		#region Override implementation of ExternalProgramBase

		/// <summary>
		/// Initializes the taks and verifies the parameters.
		/// </summary>
		/// <param name="taskNode"><see cref="XmlNode" /> containing the XML fragment used to define this task instance.</param>
		protected override void InitializeTask(XmlNode taskNode) 
		{
			// expand and store clone of the xml node
			_docNodes = Documenters.Xml.Clone().SelectNodes("nant:documenter", NamespaceManager);
			_ExpandPropertiesInNodes(_docNodes);
		}

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
		protected override void PrepareProcess(Process process) 
		{
			// ensure base directory is set, even if fileset was not initialized
			// from XML
			if (Assemblies.BaseDirectory == null) 
			{
				Assemblies.BaseDirectory = new DirectoryInfo(Project.BaseDirectory);
			}
			if (Summaries.BaseDirectory == null) 
			{
				Summaries.BaseDirectory = new DirectoryInfo(Project.BaseDirectory);
			}
			if (ReferencePaths.BaseDirectory == null) 
			{
				ReferencePaths.BaseDirectory = new DirectoryInfo(Project.BaseDirectory);
			}

			// Make sure there is at least one included assembly.  This can't
			// be done in the InitializeTask() method because the files might
			// not have been built at startup time.
			if (Assemblies.FileNames.Count == 0) 
			{
				throw new BuildException("There must be at least one included assembly.", Location);
			}
			_BuildTempConfigXmlFile();
			_BuildArguments();

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
		/// Concatenates variables for complete filename.
		/// </summary>
		/// <returns>Configuration filename.</returns>
		private string _GetConfigFilename()
		{
			return Path.Combine(Path.GetTempPath(), _configName);
		}

		/// <summary>
		/// Performs macro expansion for the given nodes.
		/// </summary>
		/// <param name="nodes"><see cref="XmlNodeList" /> for which expansion should be performed.</param>
		private void _ExpandPropertiesInNodes(XmlNodeList nodes) 
		{
			foreach (XmlNode node in nodes) 
			{
				// do not process comment nodes, or entities and other internal element types.
				if (node.NodeType == XmlNodeType.Element) 
				{
					_ExpandPropertiesInNodes(node.ChildNodes);
					foreach (XmlAttribute attr in node.Attributes) 
					{
						// use "this" keyword as workaround for Mono bug #71992
						attr.Value = this.Project.ExpandProperties(attr.Value, Location);
					}

					// convert output directory to full path relative to project base directory
					XmlNode outputDirProperty = node.SelectSingleNode("property[@name='OutputDirectory']");
					if (outputDirProperty != null) 
					{
						XmlAttribute valueAttribute = (XmlAttribute) outputDirProperty.Attributes.GetNamedItem("value");
						if (valueAttribute != null) 
						{
							// use "this" keyword as workaround for Mono bug #71992
							valueAttribute.Value = this.Project.GetFullPath(valueAttribute.Value);
						}
					}
				}
			}
		}

		/// <summary>
		/// Builds the arguments to pass to the exe.
		/// </summary>
		private void _BuildArguments() 
		{
			_programArguments.AppendFormat(" -project=\"{0}\"", _GetConfigFilename());
			if (this.Verbose)
			{
				_programArguments.Append(" -verbose");
			}
		}

		/// <summary>
		/// Build the Xml config file to pass to the NDoc2 executable.
		/// </summary>
		private void _BuildTempConfigXmlFile()
		{
			string projectFileName = _GetConfigFilename();
			Log(Level.Verbose, "Writing project settings to '{0}'.", projectFileName);

			using (Stream fileStream = File.Create(projectFileName))
			{
				XmlTextWriter writer = new XmlTextWriter(fileStream, Encoding.UTF8);
				writer.Indentation = 2;
				writer.Formatting = Formatting.Indented;

				writer.WriteStartDocument();
				writer.WriteStartElement("project");
				writer.WriteAttributeString("SchemaVersion", "2.0");

				// write assemblies section
				writer.WriteStartElement("assemblies");
				foreach (string assemblyPath in Assemblies.FileNames) 
				{
					string docPath = Path.ChangeExtension(assemblyPath, ".xml");
					writer.WriteStartElement("assembly");
					writer.WriteAttributeString("location", assemblyPath);
					if (File.Exists(docPath)) 
					{
						writer.WriteAttributeString("documentation", docPath);
					}
					writer.WriteEndElement();
				}
				writer.WriteEndElement();

				// write summaries section
				StringBuilder sb = new StringBuilder();
				foreach (string summaryPath in Summaries.FileNames) 
				{
					// write out the namespace summary nodes
					try 
					{
						XmlTextReader tr = new XmlTextReader(summaryPath);
						tr.MoveToContent();   // skip XmlDeclaration  and Processing Instructions                                               
						sb.Append(tr.ReadOuterXml());
						tr.Close();
					} 
					catch (IOException ex) 
					{
						throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
							"Failed to read ndoc namespace summary file '{0}'.", summaryPath), Location, ex);
					}
				}
				writer.WriteRaw(sb.ToString());

				// write out the documenters section
				writer.WriteStartElement("documenters");
				foreach (XmlNode node in _docNodes) 
				{
					//skip non-nant namespace elements and special elements like comments, pis, text, etc.
					if (!(node.NodeType == XmlNodeType.Element) || !node.NamespaceURI.Equals(NamespaceManager.LookupNamespace("nant"))) 
					{
						continue;
					}
					writer.WriteRaw(node.OuterXml);
				}
				writer.WriteEndElement();

				// end project element
				writer.WriteEndElement();
				writer.Flush();

				fileStream.Close();
			}
			// Dump out the content of this file if verbose logging turned on.
			if (IsLogEnabledFor(Level.Verbose))
			{
				string fileContents = NCoverUtilities.GetFileContents(_GetConfigFilename());
				Log(Level.Verbose, "Contents of NDoc settings file:");
				Log(Level.Verbose, fileContents);
			}
		}

		#endregion Private Instance Methods
	}
}
