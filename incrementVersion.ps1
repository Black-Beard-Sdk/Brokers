<#
.SYNOPSIS

Update version information in the .csproj file in preparation for building a nuget
package.
.DESCRIPTION

Discovers the package name and latest version. If that package exists and is newer
than the target that goes into it, do nothing; otherwise, increment the version
information in the .csproj file (without updating that .csproj file last modified
time).

The latest version gets picked from the maximum of the package/file/assembly
versions in the .csproj file and the version found on the nuget server.
.PARAMETER csproj


.EXAMPLE


#>
param (
    [Parameter(Mandatory=$true)][string]$dir,
    [Parameter(Mandatory=$true)][string]$toIncrement
)

function IncrementProjects()
{
	$keys = "AssemblyVersion", "FileVersion", "Version"
	[VersionIncrement]::UpdateFolders($dir, $keys, $toIncrement)

}

function InitializeType()
{

	$source = @"

	public class VersionIncrement
	{
	
        public static void UpdateFolders(string dir, string[] keys, string increment)
        {
			var _dir = new System.IO.DirectoryInfo(dir);
            _dir.Refresh();
            foreach (var file in _dir.GetFiles("*.csproj", System.IO.SearchOption.AllDirectories))
            {
                file.Refresh();
                System.Console.WriteLine("Updating " + file.Name);
                UpdateVersion(file.FullName, keys, increment);
                file.Refresh();

            }
            _dir.Refresh();
        }
		
	    public static void UpdateVersion(string csproj, string[] keys, string increment)
        {
            var txt = System.IO.File.ReadAllText(csproj);
            foreach (var key in keys)
                txt = Increment(txt, key, increment);

            System.IO.File.WriteAllText(csproj, txt);
        }

	
        public static string Increment(string txt, string key, string toChange)
        {

            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"<" + key + @">\d+(\.\d+)*</" + key + @">", System.Text.RegularExpressions.RegexOptions.Multiline);


            var match = reg.Match(txt);
            if (match.Success)
            {

                var index = match.Index;
                var value = match.Value.Substring(key.Length + 2);
                value = value.Substring(0, value.Length - (key.Length + 3));

                var version = new System.Version(value);
                var arr = value.Split('.').Length;

                switch (toChange.ToLower())
                {
                    case "major":
                        if (version.Revision > -1)
                            version = new System.Version(version.Major + 1, version.Minor, version.Build, version.Revision);
                        else
                            version = new System.Version(version.Major + 1, version.Minor, version.Build);
                        break;
                    case "minor":
                        if (version.Revision > -1)
                            version = new System.Version(version.Major, version.Minor + 1, version.Build, version.Revision);
                        else
                            version = new System.Version(version.Major, version.Minor + 1, version.Build);
                        break;
                    case "build":
                        if (version.Revision > -1)
                            version = new System.Version(version.Major, version.Minor, version.Build + 1, version.Revision);
                        else
                            version = new System.Version(version.Major, version.Minor, version.Build + 1);
                        break;
                    case "revision":
                        if (version.Revision > -1)
                            version = new System.Version(version.Major, version.Minor, version.Build, version.Revision + 1);
                        else
                            version = new System.Version(version.Major, version.Minor, version.Build, 1);
                        break;
                    default:
                        return txt;
                }

                System.Text.StringBuilder sb = new System.Text.StringBuilder(txt.Length + 5);
                var i = index + key.Length + 2;
                sb.Append(txt.Substring(0, i));

                sb.Append(version.ToString(arr));

				System.Console.WriteLine("   detected '" + key + "' incremented to " + version.ToString(arr));

                i += value.Length;
                sb.Append(txt.Substring(i, txt.Length - i));

                var txt2 = sb.ToString();

                return txt2;

            }
			else
			{
				System.Console.WriteLine("   not found " + key);
			}
            return txt;

        }
	} 
"@

    try
    {
	Add-Type -TypeDefinition $source
    }
    catch
    {

    }
}

InitializeType

IncrementProjects	
