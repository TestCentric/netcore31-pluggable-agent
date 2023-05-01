#tool NuGet.CommandLine&version=6.0.0
#tool nuget:?package=GitVersion.CommandLine&version=5.6.3
#tool nuget:?package=GitReleaseManager&version=0.12.1

// Load the recipe
#load nuget:?package=TestCentric.Cake.Recipe&version=1.0.0-dev00054
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../TestCentric.Cake.Recipe/recipe/*.cake

var target = Argument("target", Argument("t", "Default"));

static readonly string GUI_RUNNER = "tools/testcentric.exe";

BuildSettings.Initialize
(
	context: Context,
	title: "NetCore31PluggableAgent",
	solutionFile: "netcore31-pluggable-agent.sln",
	unitTests: "netcore31-agent-launcher.tests.exe",
	guiVersion: "2.0.0-alpha8",
	githubOwner: "TestCentric",
	githubRepository: "netcore31-pluggable-agent"
);

ExpectedResult MockAssemblyResult => new ExpectedResult("Failed")
{
	Total = 36,
	Passed = 23,
	Failed = 5,
	Warnings = 1,
	Inconclusive = 1,
	Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[]
	{
		new ExpectedAssemblyResult("mock-assembly.dll", "NetCore31AgentLauncher")
	}
};

var packageTests = new PackageTest[] {
	// Tests of single assemblies targeting each runtime we support
	new PackageTest(
		1, "NetCore11PackageTest", "Run mock-assembly.dll targeting .NET Core 1.1",
		"tests/netcoreapp1.1/mock-assembly.dll", MockAssemblyResult),
	new PackageTest(
		1, "NetCore21PackageTest", "Run mock-assembly.dll targeting .NET Core 2.1",
		"tests/netcoreapp2.1/mock-assembly.dll", MockAssemblyResult),
	new PackageTest(
		1, "NetCore31PackageTest", "Run mock-assembly.dll targeting .NET Core 3.1",
		"tests/netcoreapp3.1/mock-assembly.dll", MockAssemblyResult),
	// AspNetCore Test
	new PackageTest(1, "AspNetCore31Test", "Run test using AspNetCore under .NET Core 3.1",
		"tests/netcoreapp3.1/aspnetcore-test.dll",
    new ExpectedResult("Passed")
    {
        Assemblies = new [] { new ExpectedAssemblyResult("aspnetcore-test.dll", "NetCore31AgentLauncher") }
    })
};

var nugetPackage = new NuGetPackage(
	id: "NUnit.Extension.NetCore31PluggableAgent",
	source: "nuget/NetCore31PluggableAgent.nuspec",
	basePath: BuildSettings.OutputDirectory,
	checks: new PackageCheck[] {
		HasFiles("LICENSE.txt", "CHANGES.txt"),
		HasDirectory("tools").WithFiles("netcore31-agent-launcher.dll", "nunit.engine.api.dll"),
		HasDirectory("tools/agent").WithFiles(
			"netcore31-pluggable-agent.dll", "netcore31-pluggable-agent.dll.config",
			"nunit.engine.api.dll", "testcentric.engine.core.dll",
			"testcentric.engine.metadata.dll", "testcentric.extensibility.dll") },
	testRunner: new GuiRunner("TestCentric.GuiRunner", "2.0.0-alpha8"),
	tests: packageTests );

var chocolateyPackage = new ChocolateyPackage(
		id: "nunit-extension-netcore31-pluggable-agent",
		source: "choco/netcore31-pluggable-agent.nuspec",
		basePath: BuildSettings.OutputDirectory,
		checks: new PackageCheck[] {
			HasDirectory("tools").WithFiles("netcore31-agent-launcher.dll", "nunit.engine.api.dll")
				.WithFiles("LICENSE.txt", "CHANGES.txt", "VERIFICATION.txt"),
			HasDirectory("tools/agent").WithFiles(
				"netcore31-pluggable-agent.dll", "netcore31-pluggable-agent.dll.config",
				"nunit.engine.api.dll", "testcentric.engine.core.dll",
				"testcentric.engine.metadata.dll", "testcentric.extensibility.dll") },
		testRunner: new GuiRunner("testcentric-gui", "2.0.0-alpha8"),
		tests: packageTests);

BuildSettings.Packages.AddRange(new PackageDefinition[] { nugetPackage, chocolateyPackage });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Appveyor")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package")
	.IsDependentOn("Publish")
	.IsDependentOn("CreateDraftRelease")
	.IsDependentOn("CreateProductionRelease");

Task("BuildTestAndPackage")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package");

//Task("Travis")
//	.IsDependentOn("Build")
//	.IsDependentOn("Test");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
