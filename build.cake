#tool "nuget:?package=ILRepack"
#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument("target", "BuildPackages");

var artifactsDir  = Directory("./artifacts/");
var rootAbsoluteDir = MakeAbsolute(Directory("./")).FullPath;
var testReleaseDir = "./tests/Urunium.Stitch.Tests/bin/Release/";
var projectDir = "./src/Urunium.Stitch/";
var releaseDir = projectDir + "bin/Release/";
var assemblyInfoPath = projectDir + "Properties/AssemblyInfo.cs";
var msbuildTargetPath = rootAbsoluteDir + "/src/Urunium.Stitch/Msbuild/Urunium-Stitch.Msbuild.targets";
var solutionName = "Urunium.Stitch.sln";

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
    CleanDirectory(testReleaseDir);
    CleanDirectory(releaseDir);
});

Task("RunTests").IsDependentOn("Clean").Does(() => {
    MSBuild("./" + solutionName,  
        configurator => configurator.SetConfiguration("Release")
    );
    var testAssemblies = GetFiles(testReleaseDir + "Urunium.Stitch.Tests.dll");
    NUnit3(testAssemblies);
});

Task("Ilmerge")
    //.IsDependentOn("RunTests")
    .IsDependentOn("Clean")
    .Does(() => {

    MSBuild("./" + solutionName,  
        configurator => configurator.SetConfiguration("Release")
    );
    var assemblyPaths = new Cake.Core.IO.FilePath[]  { 
        releaseDir + "dotless.Core.dll", 
        releaseDir + "Newtonsoft.Json.dll", 
        releaseDir + "System.IO.Abstractions.dll" 
    };
    ILRepack(
        releaseDir + "Urunium.Stitch.dll", 
        releaseDir + "Urunium.Stitch.dll", 
        assemblyPaths, 
        new ILRepackSettings { Internalize = true, TargetKind = TargetKind.Dll });
    DeleteFiles(assemblyPaths);
});

Task("BuildPackages")
    .IsDependentOn("Ilmerge")
    .Does(() =>
{
    var assemblyInfo = ParseAssemblyInfo(assemblyInfoPath);
    Information("Version: {0}", assemblyInfo.AssemblyInformationalVersion);
    var version = string.Format("{0}", assemblyInfo.AssemblyInformationalVersion);
    var author = "Nripedra Nath Newa";
    var licenseUrl = XmlPeek("./Urunium.Stitch-Msbuild.nuspec", "/package/metadata/licenseUrl/text()");
    var projectUrl = XmlPeek("./Urunium.Stitch-Msbuild.nuspec", "/package/metadata/projectUrl/text()");
    var description = XmlPeek("./Urunium.Stitch-Msbuild.nuspec", "/package/metadata/description/text()");
    var summary = XmlPeek("./Urunium.Stitch-Msbuild.nuspec", "/package/metadata/summary/text()");

    var buildAssemblies = GetFiles(releaseDir + "*.dll");
    var jsonConfigs = GetFiles(projectDir + "urunium-stitch.config*.json");
    var msbuildNuspecContents = GetNuspecContents(new Dictionary<string, IEnumerable<string>> 
    { 
        { "build", buildAssemblies.Select(x => x.FullPath).Concat(new [] { msbuildTargetPath } ) },
        { "Content", jsonConfigs.Select(x => x.FullPath) },
        { "Root", new [] { rootAbsoluteDir + "/LICENSE" } }    
    });

    var msbuildNuspec = new NuGetPackSettings
	{
        Authors = new []{ author },
        Owners = new []{ author },
        BasePath = releaseDir,
		OutputDirectory = artifactsDir,
		IncludeReferencedProjects = true,
        Version = version,
        Files = msbuildNuspecContents.ToArray(),
		Properties = new Dictionary<string, string>
		{
			{ "Configuration", "Release" }
		}
	};

    var libNuspec = new NuGetPackSettings 
    {
        BasePath = msbuildNuspec.BasePath,
        OutputDirectory = msbuildNuspec.OutputDirectory,
        IncludeReferencedProjects = true,
        Properties = msbuildNuspec.Properties,
        Version = msbuildNuspec.Version
    };

    using(CreateTempFile(projectDir + "Urunium.Stitch.nuspec", 
        CreateNuspecXml("Urunium-Stitch", version, author, licenseUrl, projectUrl, description, summary)
    ))
    {
        // core package
        NuGetPack(projectDir + "Urunium.Stitch.csproj", libNuspec); 
        // Msbuild package
        NuGetPack("./Urunium.Stitch-Msbuild.nuspec", msbuildNuspec);
    }
});

Task("Default")
    .IsDependentOn("BuildPackages")
  .Does(() =>
{
    
  Information("Finished building!");
});

RunTarget(target);


//////////////////////////////////////////
//           HELPER Functions           //
//////////////////////////////////////////

System.Collections.Generic.List<NuSpecContent> GetNuspecContents(System.Collections.Generic.Dictionary<string, System.Collections.Generic.IEnumerable<string>> files)
{
    System.Collections.Generic.List<NuSpecContent> nuspecContents = new System.Collections.Generic.List<NuSpecContent>();
    foreach(var key in files.Keys)
    {
        foreach(var file in files[key]) 
        {
            if(key == "Root")
            {
                nuspecContents.Add(new NuSpecContent {Source = file });  
                continue;  
            }
            nuspecContents.Add(new NuSpecContent {Source = file, Target = key });
        }
    }
    return nuspecContents;
}

class DisposableFile : IDisposable
{
    string _path;
    public DisposableFile(string path, string content)
    {
        _path = path;
        System.IO.File.WriteAllText(path, content);
    }

    public void Dispose()
    {
        System.IO.File.Delete(_path);
    }
}

IDisposable CreateTempFile(string path, string content)
{
    //System.IO.File.WriteAllText(path, content);
    return new DisposableFile(path, content);
}

string CreateNuspecXml(string id, string version, string author, string licenseUrl, string projectUrl, string description, string summary)
{
    return @"<?xml version='1.0'?>
<package >
  <metadata>
    <id>" + id + @"</id>
    <version>" + version + @"</version>
    <title>Urunium Stitch</title>
    <authors>" + author + @"</authors>
    <owners>" + author + @"</owners>
    <licenseUrl>" + licenseUrl + @"</licenseUrl>
    <projectUrl>" + projectUrl + @"</projectUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <description>" + description + @"</description>
    <summary>" + summary + @"</summary>
    <releaseNotes>
      ## Version 0.1 ##
      Init
    </releaseNotes>
    <copyright>Copyright 2017</copyright>
    <tags>c# webpack stitch-it stitch js</tags>
  </metadata>
</package>";
}