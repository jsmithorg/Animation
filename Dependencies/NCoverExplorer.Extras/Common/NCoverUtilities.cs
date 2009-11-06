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
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Microsoft.Win32;

namespace NCoverExplorer.Common
{
	/// <summary>
	/// Common utility functions for working with NCover.
	/// </summary>
	internal class NCoverUtilities
	{
		#region Private Constants

		/// <summary>
		/// Registry key for registering NCover manually - this will all become unnecessary in future versions of NCover (post 1.5.5) hopefully.
		/// </summary>
		private const string NCOVER_PROFILER_CLSID = "{6287B5F9-08A1-45e7-9498-B5B2E7B02995}";
		private const string NCOVER_CLSID_KEY_NAME = @"Software\Classes\CLSID\" + NCOVER_PROFILER_CLSID;

		private const string NCOVEREXPLORER_KEY_NAME = @"Software\KiwiNova\NCoverExplorer";
		private const string NCOVER_REFCOUNT = @"NCoverRefCount";

		#endregion Private Constants

		#region WinAPI

		[DllImport("Kernel32")]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

		// A delegate type to be used as the handler routine 
		// for SetConsoleCtrlHandler.
		public delegate bool HandlerRoutine(CtrlTypes CtrlType);

		// An enumerated type for the control messages
		// sent to the handler routine.
		public enum CtrlTypes 
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT, 
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}

		#endregion WinAPI

		#region Build Temp Settings.xml File For NCover

		/// <summary>
		/// Builds the temp settings XML file for NCover.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="ncoverPath">The ncover path.</param>
		/// <param name="settingsFile">The settings file.</param>
		/// <param name="commandLineExe">The command line exe.</param>
		/// <param name="commandLineArgs">The command line args.</param>
		/// <param name="workingDirectory">The working directory.</param>
		/// <param name="assemblyList">The assembly list.</param>
		/// <param name="assemblyFiles">The assembly files.</param>
		/// <param name="coverageFile">The coverage file.</param>
		/// <param name="logLevel">The log level.</param>
		/// <param name="logFile">The log file.</param>
		/// <param name="excludeAttributes">The exclude attributes.</param>
		/// <param name="profileIIS">If set to <c>true</c> profile IIS.</param>
		/// <param name="profileService">The profile service.</param>
		/// <param name="xmlFormat">The XML format to write out (new feature in 1.5.7).</param>
		/// <param name="profiledProcessModule">Name of the profiled process.</param>
		/// <returns>
		/// Command line switch necessary for passing as an argument.
		/// </returns>
		internal static string BuildTempSettingsXmlFileForNCover(
			string version,
			string ncoverPath,
			string settingsFile,
			string commandLineExe,
			string commandLineArgs,
			string workingDirectory,
			string assemblyList,
			string[] assemblyFiles,
			string coverageFile,
			NCoverLogLevel logLevel,
			string logFile,
			string excludeAttributes,
			bool profileIIS,
			string profileService,
			NCoverXmlFormat xmlFormat,
			string profiledProcessModule)
		{
			if (version == null || version.Length == 0)
			{
				version = _ReadNCoverConsoleVersion(ncoverPath);
			}
			int versionNumber = int.Parse(version.Replace(".", ""));

			if (versionNumber < 150)
			{
				_BuildTempSettingsXmlFileForNCover133(
					settingsFile, commandLineExe, commandLineArgs, workingDirectory,
					assemblyList, assemblyFiles, coverageFile, logLevel, logFile);
				return "/r";
			}
			else
			{
				_BuildTempSettingsXmlFileForNCover15(
					versionNumber, settingsFile, commandLineExe, commandLineArgs, workingDirectory,
					assemblyList, assemblyFiles, coverageFile, logLevel, logFile, excludeAttributes,
					profileIIS, profileService, xmlFormat, profiledProcessModule);
				return "//r";
			}
		}

		#endregion Build Temp Settings.xml File For NCover

		#region Create Command Line Arguments

		/// <summary>
		/// Creates the command line arguments.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="ncoverPath">The ncover path.</param>
		/// <param name="commandLineExe">The command line exe.</param>
		/// <param name="commandLineArgs">The command line args.</param>
		/// <param name="workingDirectory">The working directory.</param>
		/// <param name="assemblyList">The assembly list.</param>
		/// <param name="coverageFile">The coverage file.</param>
		/// <param name="logLevel">The log level.</param>
		/// <param name="logFile">The log file.</param>
		/// <param name="excludeAttributes">The exclude attributes.</param>
		/// <param name="profileIIS">if set to <c>true</c> [profile IIS].</param>
		/// <param name="profileService">The profile service.</param>
		/// <param name="includeFormatting">if set to <c>true</c> include formatting.</param>
		/// <param name="registerCoverLib">Whether to register CoverLib.dll.</param>
		/// <param name="commandLineFormatToken">The command line format token.</param>
		/// <returns></returns>
		public static string CreateCommandLineArguments(			
			string version,
			string ncoverPath,
			string commandLineExe,
			string commandLineArgs,
			string workingDirectory,
			string assemblyList,
			string coverageFile,
			NCoverLogLevel logLevel,
			string logFile,
			string excludeAttributes,
			bool profileIIS,
			string profileService,
			bool includeFormatting,
			bool registerCoverLib,
			string commandLineFormatToken)
		{
			string commandLine;
			if (version == null || version.Length == 0)
			{
				version = _ReadNCoverConsoleVersion(ncoverPath);
			}
			int versionNumber = int.Parse(version.Replace(".", ""));

			if (versionNumber < 150)
			{
				commandLine = _BuildCommandLineForNCover133(
					commandLineExe, commandLineArgs, workingDirectory,
					assemblyList,  coverageFile, logLevel, logFile, commandLineFormatToken);
			}
			else
			{
				commandLine = _BuildCommandLineForNCover15(
					versionNumber, commandLineExe, commandLineArgs, workingDirectory,
					assemblyList, coverageFile, logLevel, logFile, excludeAttributes,
					profileIIS, profileService, registerCoverLib, commandLineFormatToken);
			}
			if (!includeFormatting)
			{
				commandLine = commandLine.Replace(commandLineFormatToken, " ");
			}
			return commandLine;
		}

		#endregion Create Command Line Arguments

		#region Register / Unregister NCover

		/// <summary>
		/// Registers the NCover coverlib.dll by writing directly into the registry under HKCU.
		/// Keeps a reference count so only register if only NCover task currently running.
		/// </summary>
		public static void RegisterNCover(string ncoverPath)
		{
			ncoverPath = _GetNCoverPath(ncoverPath);
			
			int currentRefCount = 0;
			// Update the reference count for currently running NCover instances
			using (RegistryKey ncoverExplorerKey = Registry.CurrentUser.CreateSubKey(NCOVEREXPLORER_KEY_NAME))
			{
				if (ncoverExplorerKey.GetValue(NCOVER_REFCOUNT) != null)
				{
					currentRefCount = (int)ncoverExplorerKey.GetValue(NCOVER_REFCOUNT);
				}
				currentRefCount++;
				ncoverExplorerKey.SetValue(NCOVER_REFCOUNT, currentRefCount);
			}

			// We will always register CoverLib.dll to make sure the NCover path gets updated.
			_RegisterNCoverInHKCURegistry(ncoverPath);

			// Register for Ctrl+C style events so we can keep our refcount accurate.
			SetConsoleCtrlHandler(new HandlerRoutine(_ConsoleCtrlCheck), true);
		}

		/// <summary>
		/// Unregisters the NCover coverlib.dll
		/// Keeps a reference count so only unregister if last NCover task currently running.
		/// </summary>
		public static void UnregisterNCover()
		{
			// Will no longer be interested in ctrl+C style events.
			SetConsoleCtrlHandler(new HandlerRoutine(_ConsoleCtrlCheck), false);

			int currentRefCount = 0;
			// Update the reference count for currently running NCover instances
			using (RegistryKey ncoverExplorerKey = Registry.CurrentUser.CreateSubKey(NCOVEREXPLORER_KEY_NAME))
			{
				if (ncoverExplorerKey.GetValue(NCOVER_REFCOUNT) == null)
				{
					// Shouldn't happen - calling unregister before register?
				}
				else
				{
					currentRefCount = (int)ncoverExplorerKey.GetValue(NCOVER_REFCOUNT) - 1;
					// Make double sure things won't go wrong for future runs.
					currentRefCount = Math.Max(currentRefCount, 0);
				}
				ncoverExplorerKey.SetValue(NCOVER_REFCOUNT, currentRefCount);
			}

			if (currentRefCount == 0)
			{
				// All instances have stopped running so we can unregister CoverLib.dll to cleanup.
				RegistryKey classKey = Registry.CurrentUser.OpenSubKey(NCOVER_CLSID_KEY_NAME);
				if (classKey != null)
				{
					classKey.Close();
					Registry.CurrentUser.DeleteSubKeyTree(NCOVER_CLSID_KEY_NAME);
				}
			}
		}

		#endregion Register / Unregister NCover

		#region Private Methods

		#region Locate NCover Executable

		private static string _GetNCoverPath(string ncoverPath)
		{
			if (ncoverPath == null)
			{
				// Let's assume the default NCover location.
				ncoverPath = @"C:\Program Files\NCover";
			}
			if (!Directory.Exists(ncoverPath))
			{
				string[] ncoverLocations = PathSearch.Search("ncover.console.exe");
				if (ncoverLocations.Length > 0)
				{
					ncoverPath = Path.GetDirectoryName(ncoverLocations[0]);
				}
				else
				{
					throw new FileNotFoundException("Could not find NCover folder in your path.");
				}
			}
			return ncoverPath;
		}

		#endregion Locate NCover Executable

		#region Read NCover.Console Version

		/// <summary>
		/// Find path to NCover console and retrieve the version info.
		/// </summary>
		private static string _ReadNCoverConsoleVersion(string ncoverExePath)
		{
			bool isNCoverFound = false;
			if (ncoverExePath.Length == 0)
			{
				// No chance!
			}
			else if (!File.Exists(ncoverExePath))
			{
				string[] ncoverLocations = PathSearch.Search(ncoverExePath);
				if (ncoverLocations.Length > 0)
				{
					ncoverExePath = ncoverLocations[0];
					isNCoverFound = true;
				}
			}
			else
			{
				isNCoverFound = true;
			}
			if (isNCoverFound)
			{
				FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(ncoverExePath);
				// Only interested in the first part of the version, not the private part.
				return fileVersionInfo.FileMajorPart + "." + fileVersionInfo.FileMinorPart + "." + fileVersionInfo.FileBuildPart;
			}
			else
			{
				throw new FileNotFoundException("NCover.Console could not be found. Please specify a full path or add folder to your path.");
			}
		}

		#endregion Read NCover.Console Version

		#region Read NCover Settings File
		
		/// <summary>
		/// Reads the file contents and returns as a string.
		/// </summary>
		public static string GetFileContents(string fileName)
		{
			using (StreamReader streamReader = File.OpenText(fileName))
			{
				return streamReader.ReadToEnd();
			}
		}

		#endregion Read NCover Settings File

		#region Build Settings File for NCover 1.3.3

		/// <summary>
		/// Build the Xml .ncoversettings file to pass to the NCover.Console executable using NCover 1.3.3 syntax.
		/// </summary>
		private static void _BuildTempSettingsXmlFileForNCover133(
			string settingsFile,
			string commandLineExe,
			string commandLineArgs,
			string workingDirectory,
			string assemblyList,
			string[] assemblyFiles,
			string coverageFile,
			NCoverLogLevel logLevel,
			string logFile)
		{
			using (Stream fileStream = File.Create(settingsFile))
			{
				XmlTextWriter xmlTextWriter = new XmlTextWriter(fileStream, Encoding.UTF8);
				xmlTextWriter.Indentation = 2;
				xmlTextWriter.Formatting = Formatting.Indented;

				xmlTextWriter.WriteStartDocument();
				xmlTextWriter.WriteStartElement("ProfilerSettings");
				xmlTextWriter.WriteElementString("CommandLineExe", commandLineExe);
				xmlTextWriter.WriteElementString("CommandLineArgs", commandLineArgs.Replace("\"", ""));
				xmlTextWriter.WriteElementString("WorkingDirectory", workingDirectory);

				// Write assembly names as semi-colon delimited string.
				_WriteAssemblyNamesForNCover133(assemblyList, assemblyFiles, xmlTextWriter);

				xmlTextWriter.WriteElementString("CoverageFile", coverageFile);
				if (logLevel == NCoverLogLevel.Quiet)
				{
					xmlTextWriter.WriteElementString("LogFile", string.Empty);
					xmlTextWriter.WriteElementString("VerboseLog", "false");
					xmlTextWriter.WriteElementString("NoLog", "true");
				}
				else
				{
					xmlTextWriter.WriteElementString("LogFile", logFile);
					xmlTextWriter.WriteElementString("VerboseLog", (logLevel == NCoverLogLevel.Verbose).ToString().ToLower());
					xmlTextWriter.WriteElementString("NoLog", "false");
				}
				xmlTextWriter.WriteElementString("NoXslCopy", XmlConvert.ToString(true));

				xmlTextWriter.WriteEndElement(); // ProfilerSettings
				xmlTextWriter.WriteEndDocument();
				xmlTextWriter.Flush();

				fileStream.Close();
			}
		}

		/// <summary>
		/// Write assembly names as a semi-colon delimited unique assembly name list.
		/// Assembly names do not have extensions (how NCover requires them) to match how the
		/// CLR identifies them when being profiled.
		/// </summary>
		private static void _WriteAssemblyNamesForNCover133(string assemblyList, string[] fileNames,
			XmlTextWriter xmlTextWriter)
		{
			string assemblyNames = string.Empty;
			StringCollection uniqueNames = new StringCollection();

			if (assemblyList.Length > 0)
			{
				string[] listedNames = assemblyList.Split(';');
				foreach (string listedName in listedNames)
				{
					string assemblyName = listedName.Trim();
					if (assemblyName.Length > 0)
					{
						// Check to see if user screwed up and put an extension in - rip it off if found.
						if (assemblyName.ToLower().EndsWith(".dll") || assemblyName.ToLower().EndsWith(".exe"))
						{
							assemblyName = Path.GetFileNameWithoutExtension(assemblyName);
						}
						if (!uniqueNames.Contains(assemblyName))
						{
							uniqueNames.Add(assemblyName);
							assemblyNames += ";" + assemblyName;
						}
					}
				}
			}

			foreach (string fileName in fileNames)
			{
				string assemblyName = Path.GetFileNameWithoutExtension(fileName);
				if (!uniqueNames.Contains(assemblyName))
				{
					uniqueNames.Add(assemblyName);
					assemblyNames += ";" + assemblyName;
				}
			}

			if (assemblyNames.Length > 0)
			{
				assemblyNames = assemblyNames.Substring(1);
			}
			xmlTextWriter.WriteElementString("Assemblies", assemblyNames);
		}

		#endregion Build Settings File for NCover 1.3.3

		#region Build Settings File for NCover 1.5.x

		/// <summary>
		/// Build the Xml .ncoversettings file to pass to the NCover.Console executable using NCover 1.5 syntax.
		/// </summary>
		private static void _BuildTempSettingsXmlFileForNCover15(
			int versionNumber,
			string settingsFile,
			string commandLineExe,
			string commandLineArgs,
			string workingDirectory,
			string assemblyList,
			string[] assemblyFiles,
			string coverageFile,
			NCoverLogLevel logLevel,
			string logFile,
			string excludeAttributes,
			bool profileIIS,
			string profileService,
			NCoverXmlFormat xmlFormat,
			string profiledProcessModule)
		{
			using (Stream fileStream = File.Create(settingsFile))
			{
				XmlTextWriter xmlTextWriter = new XmlTextWriter(fileStream, Encoding.UTF8);
				xmlTextWriter.Indentation = 2;
				xmlTextWriter.Formatting = Formatting.Indented;

				xmlTextWriter.WriteStartDocument();
				xmlTextWriter.WriteStartElement("ProfilerSettings");
				xmlTextWriter.WriteElementString("CommandLineExe", commandLineExe);
				if (commandLineArgs != null && commandLineArgs.Length > 0)
				{
					xmlTextWriter.WriteElementString("CommandLineArgs", commandLineArgs);
				}
				if (workingDirectory != null && workingDirectory.Length >= 0)
				{
					xmlTextWriter.WriteElementString("WorkingDirectory", workingDirectory);
				}
				
				// For NCover 1.5.4 onwards we write each assembly file as a separate node.
				_WriteAssemblyNodes(assemblyList, assemblyFiles, xmlTextWriter);
				
				if (coverageFile != null && coverageFile.Length >= 0)
				{
					xmlTextWriter.WriteElementString("CoverageXml", coverageFile);
					if (versionNumber >= 157)
					{
						xmlTextWriter.WriteElementString("XmlFormat", xmlFormat.ToString());
					}
				}
				if (logLevel == NCoverLogLevel.Quiet)
				{
					if (versionNumber == 154)
					{
						// HACK: Setting NoLog to "true" results in NCover hanging in the NCover 1.5.4 release
						// For now we will just leave at false and always write a log file until Peter fixes it.
						xmlTextWriter.WriteElementString("LogFile", logFile);
						xmlTextWriter.WriteElementString("VerboseLog", "false");
						xmlTextWriter.WriteElementString("NoLog", "false");
					}
					else
					{
						// NCover 1.5.5 onwards has the fix.
						xmlTextWriter.WriteElementString("LogFile", string.Empty);
						xmlTextWriter.WriteElementString("VerboseLog", "false");
						xmlTextWriter.WriteElementString("NoLog", "true");
					}
				}
				else
				{
					xmlTextWriter.WriteElementString("LogFile", logFile);
					xmlTextWriter.WriteElementString("VerboseLog", (logLevel == NCoverLogLevel.Verbose).ToString().ToLower());
					xmlTextWriter.WriteElementString("NoLog", "false");
				}
				if (excludeAttributes != null && excludeAttributes.Length > 0)
				{
					string[] eachExcludeAttributes = excludeAttributes.Split(';');
					foreach (string excludeAttribute in eachExcludeAttributes)
					{
						xmlTextWriter.WriteElementString("ExclusionAttributes", excludeAttribute);
					}
				}
				if (versionNumber >= 158 && profiledProcessModule.Length > 0)
				{
					xmlTextWriter.WriteElementString("ProfileProcessModule", profiledProcessModule);
				}
				xmlTextWriter.WriteElementString("ProfileIIS", XmlConvert.ToString(profileIIS));
				if (profileService != null && profileService.Length > 0)
				{
					xmlTextWriter.WriteElementString("ProfileService", profileService);
				}
				xmlTextWriter.WriteElementString("DumpOnErrorNormal", "false");
				xmlTextWriter.WriteElementString("DumpOnErrorFull", "false");
				// Some NCover 1.5.5 specific features
				if (versionNumber >= 155)
				{
					xmlTextWriter.WriteElementString("CoverageBinary", "Coverage.bcv");
					xmlTextWriter.WriteElementString("AutoExclude", "true");
				}

				xmlTextWriter.WriteEndElement(); // ProfilerSettings
				xmlTextWriter.WriteEndDocument();
				xmlTextWriter.Flush();

				fileStream.Close();
			}
		}

		/// <summary>
		/// Writes assembly names as separate Assembly nodes in the settings file. Seems to be a 
		/// difference in how NCover 1.5.4 onwards handles from previous versions in the xml.
		/// </summary>
		private static void _WriteAssemblyNodes(string assemblyList, string[] assemblyFiles, XmlTextWriter xmlTextWriter)
		{
			StringCollection uniqueNames = new StringCollection();

			if (assemblyList.Length > 0)
			{
				string[] listedNames = assemblyList.Split(';');
				foreach (string listedName in listedNames)
				{
					string assemblyName = listedName.Trim();
					if (assemblyName.Length > 0)
					{
						// Check to see if user screwed up and put an extension in - rip it off if found.
						if (assemblyName.ToLower().EndsWith(".dll") || assemblyName.ToLower().EndsWith(".exe"))
						{
							assemblyName = Path.GetFileNameWithoutExtension(assemblyName);
						}
						if (!uniqueNames.Contains(assemblyName))
						{
							uniqueNames.Add(assemblyName);
							xmlTextWriter.WriteElementString("Assemblies", assemblyName);
						}
					}
				}
			}

			foreach (string fileName in assemblyFiles)
			{
				string assemblyName = Path.GetFileNameWithoutExtension(fileName);
				if (!uniqueNames.Contains(assemblyName))
				{
					xmlTextWriter.WriteElementString("Assemblies", assemblyName);
					uniqueNames.Add(assemblyName);
				}
			}

			// Do not write empty assemblies node if no assemblies specified.
		}

		#endregion Build Settings File for NCover 1.5.x

		#region Build Command Line for NCover 1.3.3

		/// <summary>
		/// Build a command line using NCover 1.3.3 syntax.
		/// </summary>
		private static string _BuildCommandLineForNCover133(
			string commandLineExe,
			string commandLineArgs,
			string workingDirectory,
			string assemblyList,
			string coverageFile,
			NCoverLogLevel logLevel,
			string logFile,
			string commandLineFormatToken)
		{
			StringBuilder programArguments = new StringBuilder();
			if (workingDirectory.Length > 0)
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("/w \"{0}\"", workingDirectory);
			}
			if (coverageFile.Length > 0 && coverageFile != "coverage.xml")
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("/x \"{0}\"", coverageFile);
			}
			if (logLevel == NCoverLogLevel.Verbose)
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("/v");
			}
			if (logFile.Length > 0 && logFile != "coverage.log")
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("/l \"{0}\"", logFile);
			}
			if (assemblyList.Length > 0)
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("/a {0}", assemblyList);
			}
			programArguments.Append(commandLineFormatToken);
			programArguments.AppendFormat("/c \"{0}\"", commandLineExe);
			if (commandLineArgs.Length > 0)
			{
				programArguments.Append(commandLineFormatToken);
				// Replace any quotes as we will add our own around the entire expression.
				programArguments.AppendFormat("\"{0}\"", commandLineArgs.Replace("\"", ""));
			}

			return programArguments.ToString();
		}

		#endregion Build Command Line for NCover 1.3.3

		#region Build Command Line for NCover 1.5.x

		/// <summary>
		/// Build a command line using NCover 1.5.x syntax.
		/// </summary>
		private static string _BuildCommandLineForNCover15(
			int versionNumber,
			string commandLineExe,
			string commandLineArgs,
			string workingDirectory,
			string assemblyList,
			string coverageFile,
			NCoverLogLevel logLevel,
			string logFile,
			string excludeAttributes,
			bool profileIIS,
			string profileService,
			bool registerCoverLib,
			string commandLineFormatToken)
		{
			StringBuilder programArguments = new StringBuilder();
			if (registerCoverLib && versionNumber >= 157)
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.Append("//reg");
			}
			if (workingDirectory.Length > 0)
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("//w \"{0}\"", workingDirectory);
			}
			if (coverageFile.Length > 0 && coverageFile != "coverage.xml")
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("//x \"{0}\"", coverageFile);
			}
			if (logLevel == NCoverLogLevel.Verbose)
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("//v");
			}
			if (logFile.Length > 0 && logFile != "coverage.log")
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("//l \"{0}\"", logFile);
			}
			if (assemblyList.Length > 0)
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("//a {0}", assemblyList);
			}
			if (excludeAttributes.Length > 0)
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("//ea {0}", excludeAttributes);
			}
			if (profileIIS)
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("//iis");
			}
			else if (profileService.Length > 0)
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("//svc \"{0}\"", profileService);
			}
			else
			{
				programArguments.Append(commandLineFormatToken);
				programArguments.AppendFormat("\"{0}\"", commandLineExe);
				if (commandLineArgs.Length > 0)
				{
					programArguments.Append(commandLineFormatToken);
					// No need to put quotes around arguments for NCover 1.5 as all are passed through.
					programArguments.AppendFormat("{0}", commandLineArgs);
				}
			}

			return programArguments.ToString();
		}

		#endregion Build Command Line for NCover 1.5.x

		#region NCover Registry Settings

		/// <summary>
		///Creates the necessary HKCU entries for the NCover coverlib.dll.
		/// </summary>
		/// <param name="ncoverPath">The ncover path.</param>
		private static void _RegisterNCoverInHKCURegistry(string ncoverPath)
		{
			using (RegistryKey clsidKey = Registry.CurrentUser.CreateSubKey(NCOVER_CLSID_KEY_NAME))
			{
				clsidKey.SetValue(null, "NCover Profiler Object");
				using (RegistryKey serverKey = clsidKey.CreateSubKey("InprocServer32"))
				{
					string coverLibPath = Path.Combine(ncoverPath, "CoverLib.dll");
					serverKey.SetValue(null, coverLibPath);
					serverKey.SetValue("ThreadingModel", "Both");
				}
			}
		}

		#endregion NCover Registry Settings

		#region Ctrl+C Event Handler To Unregister NCover

		/// <summary>
		/// Handles a control-C style event so we can cleanup our refcount.
		/// </summary>
		/// <param name="ctrlType">Type of control exit.</param>
		/// <returns>Whether to cancel the event.</returns>
		private static bool _ConsoleCtrlCheck(CtrlTypes ctrlType) 
		{
			UnregisterNCover();
			return false;
		}

		#endregion Ctrl+C Event Handler To Unregister NCover

		#endregion Private Methods
	}

}
