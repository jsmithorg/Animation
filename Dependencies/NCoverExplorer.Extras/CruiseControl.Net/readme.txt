To make use of the NCoverExplorer stylesheets do the following:

(1) Copy the NCoverExplorer.xsl and NCoverExplorerSummary.xsl stylesheets into the following folder:
	C:\Program Files\Cruise Control.Net\webDashboard\xsl

(2) Edit or replace this file:
	C:\Program Files\Cruise Control.Net\webDashboard\dashboard.config

    The example dashboard.config in this folder shows how to replace the NCover stylesheets with
    the NCoverExplorer alternatives.

(3) Add a NAnt or MSBuild task running NCoverExplorer.Console.exe which will process the
    coverage.xml file(s) to produce an xml report output.

(4) In your CruiseControl.Net project, merge the resulting CoverageReport.xml into the buildlog.

For a blog entry showing an example, refer to:
http://www.kiwidude.com/blog/2006/04/ncoverexplorer-v133.html

