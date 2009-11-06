
namespace NCoverExplorer.Common
{
	#region NCoverLogLevel Enum

	/// <summary>
	/// Logging levels to use within NCover task.
	/// </summary>
	public enum NCoverLogLevel
	{
		/// <summary>No logging.</summary>
		Quiet = 0,
		/// <summary>Writes standard log output (Default).</summary>
		Normal = 1,
		/// <summary>Writes verbose log output.</summary>
		Verbose = 2
	}

	#endregion NCoverLogLevel Enum

	#region NCoverXmlFormat Enum

	/// <summary>
	/// New element option introduced in NCover 1.5.7 for use with //x2 argument.
	/// </summary>
	public enum NCoverXmlFormat 
	{ 
		/// <summary>
		/// Legacy xml format that is the default.
		/// </summary>
		Xml1, 
		/// <summary>
		/// New xml format introduced in NCover 1.5.7 that nests method nodes with class nodes.
		/// </summary>
		Xml2 
	};

	#endregion NCoverXmlFormat Enum

	#region TreeSortStyle

	/// <summary>
	/// Sort order for displaying the coverage results in the tree.
	/// </summary>
	public enum TreeSortStyle
	{
		/// <summary>Sort by name (default). (0)</summary>
		Name = 0,
		/// <summary>Sort by name( down to class level) then by line within the class. (1)</summary>
		ClassLine = 1,
		/// <summary>Sort by coverage percentage ascending. (2)</summary>
		CoveragePercentageAscending = 2,
		/// <summary>Sort by coverage percentage descending. (3)</summary>
		CoveragePercentageDescending = 3,
		/// <summary>Sort by unvisited lines ascending. (4)</summary>
		UnvisitedSequencePointsAscending = 4,
		/// <summary>Sort by unvisited lines descending. (5)</summary>
		UnvisitedSequencePointsDescending = 5,
		/// <summary>Sort by visit count ascending. (6)</summary>
		VisitCountAscending = 6,
		/// <summary>Sort by visit count descending. (7)</summary>
		VisitCountDescending = 7,
		/// <summary>Sort by function coverage ascending. (8)</summary>
		FunctionCoverageAscending = 8,
		/// <summary>Sort by function coverage descending. (9)</summary>
		FunctionCoverageDescending = 9
	}

	#endregion TreeSortStyle

	#region TreeFilterStyle

	/// <summary>
	/// Filter styles that can be applied to the results. Filtered nodes are not excluded from the coverage
	/// statistics.
	/// </summary>
	public enum TreeFilterStyle
	{
		/// <summary>No filter applied. (0)</summary>
		None = 0,
		/// <summary>Hide unvisited nodes. (1)</summary>
		HideUnvisited = 1,
		/// <summary>Hide 100% fully covered nodes. (2)</summary>
		HideFullyCovered = 2,
		/// <summary>Hide nodes exceeding coverage threshold. (3)</summary>
		HideThresholdCovered = 3
	}

	#endregion TreeFilterStyle

	#region CoverageReportType

	/// <summary>
	/// Potential report types.
	/// </summary>
	public enum CoverageReportType
	{
		/// <summary>None. (0)</summary>
		None = 0,
		/// <summary>Modules summary only. (1)</summary>
		ModuleSummary = 1,
		// Previous report type of NamespaceSummary = 2 has been deprecated
		/// <summary>Modules summary followed with a namespaces by module summary. (3)</summary>
		ModuleNamespaceSummary = 3,
		/// <summary>Modules summary followed with a classes by namespace summary. (4)</summary>
		ModuleClassSummary = 4,
		/// <summary>Modules summary followed with a classes by namespace summary showing function coverage. (5)</summary>
		ModuleClassFunctionSummary = 5
	}

	#endregion CoverageReportType
}
