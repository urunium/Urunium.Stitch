#tool "nuget:?package=ILRepack"
#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument("target", "BuildPackages");
var artifactsDir  = Directory("./artifacts/");
var rootAbsoluteDir = MakeAbsolute(Directory("./")).FullPath;


Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
    CleanDirectory("./tests/Urunium.Stitch.Tests/bin/Release/");
    CleanDirectory("./src/Urunium.Stitch/bin/Release/");
});

Task("RunTests").IsDependentOn("Clean").Does(() => {
    MSBuild("./Urunium.Stitch.sln",  
        configurator => configurator.SetConfiguration("Release")
    );
    var testAssemblies = GetFiles("./tests/Urunium.Stitch.Tests/bin/Release/Urunium.Stitch.Tests.dll");
    NUnit3(testAssemblies);
});

Task("Ilmerge")
    //.IsDependentOn("RunTests")
    .IsDependentOn("Clean")
    .Does(() => {

    MSBuild("./Urunium.Stitch.sln",  
        configurator => configurator.SetConfiguration("Release")
    );
    var assemblyPaths = new Cake.Core.IO.FilePath[]  { 
        "./src/Urunium.Stitch/bin/Release/dotless.Core.dll", 
        "./src/Urunium.Stitch/bin/Release/Newtonsoft.Json.dll", 
        "./src/Urunium.Stitch/bin/Release/System.IO.Abstractions.dll" 
    };
    ILRepack(
        "./src/Urunium.Stitch/bin/Release/Urunium.Stitch.dll", 
        "./src/Urunium.Stitch/bin/Release/Urunium.Stitch.dll", 
        assemblyPaths, 
        new ILRepackSettings { Internalize = true, TargetKind = TargetKind.Dll });
    DeleteFiles(assemblyPaths);
});

Task("BuildPackages")
    .IsDependentOn("Ilmerge")
    .Does(() =>
{
    var assemblyInfo = ParseAssemblyInfo("./src/Urunium.Stitch/Properties/AssemblyInfo.cs");
    Information("Version: {0}", assemblyInfo.AssemblyInformationalVersion);
    var version = string.Format("{0}", assemblyInfo.AssemblyInformationalVersion);
    var author = "Nripedra Nath Newa";
    var licenseUrl = XmlPeek("./Urunium.Stitch-Msbuild.nuspec", "/package/metadata/licenseUrl/text()");
    var projectUrl = XmlPeek("./Urunium.Stitch-Msbuild.nuspec", "/package/metadata/projectUrl/text()");
    var description = XmlPeek("./Urunium.Stitch-Msbuild.nuspec", "/package/metadata/description/text()");

    var buildAssemblies = GetFiles("./src/Urunium.Stitch/bin/Release/*.dll");
    System.Collections.Generic.List<NuSpecContent> nuspecContents = new  System.Collections.Generic.List<NuSpecContent>(
         new [] {
            new NuSpecContent {Source = rootAbsoluteDir + "/src/Urunium.Stitch/Msbuild/Urunium-Stitch.Msbuild.targets", Target = "build"},
            new NuSpecContent {Source = rootAbsoluteDir + "/LICENSE"},
        }
    );

    foreach(var buildAssembly in buildAssemblies)
    {
        nuspecContents.Add(new NuSpecContent {Source = buildAssembly.FullPath, Target = "build"});
    }

    var jsonConfigs = GetFiles("./src/Urunium.Stitch/urunium-stitch.config*.json");

    
    foreach(var jsonConfig in jsonConfigs)
    {
        nuspecContents.Add(new NuSpecContent {Source = jsonConfig.FullPath, Target = "Content"});
    }

    var msbuildNuGetPackSettings = new NuGetPackSettings
	{
        Authors = new []{ author },
        Owners = new []{ author },
        BasePath = "./src/Urunium.Stitch/bin/Release/",
        Description = description,
        Summary = "Webpack like packager built on dotnet.",
		OutputDirectory = rootAbsoluteDir + @"\artifacts\",
		IncludeReferencedProjects = true,
        Version                 = version,
        Files                   = nuspecContents.ToArray(),
		Properties = new Dictionary<string, string>
		{
			{ "Configuration", "Release" }
		}
	};

    var libNuGetPackSettings = new NuGetPackSettings
	{
        Authors = new []{ author },
        Owners = new []{ author },
        Description = msbuildNuGetPackSettings.Description,
        Summary = msbuildNuGetPackSettings.Summary,
        BasePath = msbuildNuGetPackSettings.BasePath,
        LicenseUrl = new Uri(licenseUrl),
		OutputDirectory = msbuildNuGetPackSettings.OutputDirectory,
        ProjectUrl = new Uri(projectUrl),
		IncludeReferencedProjects = true,
        Version                 = msbuildNuGetPackSettings.Version,
        Files                   = new [] {
            new NuSpecContent { 
                Source = rootAbsoluteDir + "/src/Urunium.Stitch/bin/Release/*.dll", 
                Target = "lib"
            },
        },
		Properties = new Dictionary<string, string>
		{
			{ "Configuration", "Release" }
		}
	};

    NuGetPack("./src/Urunium.Stitch/Urunium.Stitch.csproj", libNuGetPackSettings);
    NuGetPack("./Urunium.Stitch-Msbuild.nuspec", msbuildNuGetPackSettings);
    //
});

Task("Default")
    .IsDependentOn("BuildPackages")
  .Does(() =>
{
    
  Information("Finished building!");
});

RunTarget(target);