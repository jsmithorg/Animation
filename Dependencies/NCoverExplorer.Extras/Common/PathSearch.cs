using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace NCoverExplorer.Common
{
	/// <summary>
	/// Utility class to scan for an executable in all available paths.
	/// Based on CodeProject article at: http://www.codeproject.com/csharp/winwhich.asp
	/// </summary>
	internal sealed class PathSearch
	{
		#region Private Variables

		private static string[] _pathExtensions = new string[0];

		#endregion Private Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="PathSearch"/> class.
		/// </summary>
		private PathSearch()
		{
		}

		#endregion Constructor
		
		#region Public Methods

		/// <summary>
		/// Return the version information for the executable located at this path. If executable
		/// not found at this location (e.g. not a fully qualified path) does a path search to see
		/// if it can be found anywhere else.
		/// </summary>
		/// <param name="executablePath">Path to executable to find.</param>
		/// <param name="throwExceptionIfNotFound">Whether to throw exception if not found.</param>
		/// <returns>Version number in format Major.Minor.Build</returns>
		public static string GetVersionForExecutable(string executablePath, bool throwExceptionIfNotFound)
		{
			bool isExecutableFound = false;
			if (executablePath.Length == 0)
			{
				// No chance!
			}
			else if (!File.Exists(executablePath))
			{
				string[] executableLocations = PathSearch.Search(Path.GetFileName(executablePath));
				if (executableLocations.Length > 0)
				{
					executablePath = executableLocations[0];
					isExecutableFound = true;
				}
			}
			else
			{
				isExecutableFound = true;
			}
			if (isExecutableFound)
			{
				FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(executablePath);
				// Only interested in the first part of the version, not the private part.
				return fileVersionInfo.FileMajorPart + "." + fileVersionInfo.FileMinorPart + "." +
					fileVersionInfo.FileBuildPart;
			}
			else if (throwExceptionIfNotFound)
			{
				throw new FileNotFoundException(
					"Executable could not be found. Please specify a full path or add folder to your path.");
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Searches for the specified executable name in all available paths.
		/// </summary>
		/// <param name="executableName">Name of the executable.</param>
		/// <returns></returns>
		public static string[] Search(string executableName)
		{
			string pathString = "";
			ArrayList matchingPaths = new ArrayList();

			IDictionary  environmentVariables = Environment.GetEnvironmentVariables();
			foreach (DictionaryEntry de in environmentVariables)
			{
				if(de.Key.ToString().ToUpper().Equals("PATH"))
				{
					pathString = de.Value.ToString().Trim();
				}
				if (de.Key.ToString().ToUpper().Equals("PATHEXT"))
				{
					_pathExtensions = de.Value.ToString().Trim().Split(new char[]{';'});
				}
			}
			
			Regex regEx = new Regex(_GetRegExString(executableName), RegexOptions.IgnoreCase);
            
			string[] paths = _GetPathsSplit(pathString);
			foreach (string path in paths)
			{
				string trimmedPath = path.Trim();
				if (trimmedPath.Length == 0)
				{
					continue;
				}
				DirectoryInfo directoryInfo = new DirectoryInfo(trimmedPath);
				if (!directoryInfo.Exists)
				{
					continue;
				}
				foreach(FileInfo fileInfo in directoryInfo.GetFiles())
				{
					if (regEx.IsMatch(fileInfo.Name))
					{
						matchingPaths.Add(fileInfo.FullName);
					}
				}
			}
			return (string[])matchingPaths.ToArray(typeof(string));
		}

		#endregion Public Methods
		
		#region Private Methods

		//Insert the startup path at the beginning
		//Remove other instances of the startup path
		//Return an array of the paths.
		private static string[] _GetPathsSplit(string pathString)
		{
			string[] paths = pathString.Split(new char[]{Path.PathSeparator});
			string startupPath = System.Environment.CurrentDirectory.ToLower();

			ArrayList cleanPaths = new ArrayList();
			foreach (string oneString in paths)
			{
				if (!(oneString.ToLower().Equals(startupPath) || 
					oneString.Equals(".")))
				{
					cleanPaths.Add(oneString);
				}
			}

			cleanPaths.Insert(0, ".");
			return (string[])cleanPaths.ToArray(typeof(string));
		}

		/// <summary>
		/// Form the regular expression string for the matching file.
		/// </summary>
		/// <param name="executableName">The name of the executable</param>
		/// <returns>string that is the regex pattern.</returns>
		private static string _GetRegExString(string executableName)
		{
			executableName = executableName.ToLower(); 
			string regexString = "^" + executableName;
			bool execNameHasExtension = false;
			foreach (string oneExtension in _pathExtensions)
			{
				if (executableName.EndsWith(oneExtension.ToLower()))
				{
					execNameHasExtension = true;
					break;
				}
			}
			if (execNameHasExtension || _pathExtensions == null || _pathExtensions.Length == 0)
			{
				regexString += "$";
			}
			else
			{
				regexString += "(?:";
				foreach(string oneExtension in _pathExtensions)
				{
					regexString += "\\" + oneExtension + "|";
				}
				regexString += ")$";
			}
			return regexString;
		}

		#endregion Private Methods
	}
}
