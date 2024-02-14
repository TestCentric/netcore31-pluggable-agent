// Load the recipe
#load nuget:?package=TestCentric.Cake.Recipe&version=1.1.0-dev00082
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../TestCentric.Cake.Recipe/recipe/*.cake

var target = Argument("target", Argument("t", "Default"));

BuildSettings.Initialize
(
	context: Context,
	title: "NetCore31PluggableAgent",
	solutionFile: "netcore31-pluggable-agent.sln",
	unitTests: "netcore31-agent-launcher.tests.exe",
	githubOwner: "TestCentric",
	githubRepository: "netcore31-pluggable-agent"
);

var MockAssemblyResult = new ExpectedResult("Failed")
{
	Total = 36, Passed = 23, Failed = 5, Warnings = 1, Inconclusive = 1, Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly.dll") }
};


var AspNetCoreResult = new ExpectedResult("Passed")
{
	Total = 2, Passed = 2, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
	Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("aspnetcore-test.dll") }
};

var WindowsFormsResult = new ExpectedResult("Passed")
{
	Total = 2, Passed = 2, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
	Assemblies = new ExpectedAssemblyResult[] {	new ExpectedAssemblyResult("windows-forms-test.dll") }
};

var	PackageTests = new List<PackageTest>();
PackageTests.Add(new PackageTest(
	1, "NetCore11PackageTest", "Run mock-assembly.dll targeting .NET Core 1.1",
	"tests/netcoreapp1.1/mock-assembly.dll", MockAssemblyResult));

PackageTests.Add(new PackageTest(
	1, "NetCore21PackageTest", "Run mock-assembly.dll targeting .NET Core 2.1",
	"tests/netcoreapp2.1/mock-assembly.dll", MockAssemblyResult));

PackageTests.Add(new PackageTest(
	1, "NetCore31PackageTest", "Run mock-assembly.dll targeting .NET Core 3.1",
	"tests/netcoreapp3.1/mock-assembly.dll --trace:Debug", MockAssemblyResult));

PackageTests.Add(new PackageTest(
	1, $"AspNetCore31Test", $"Run test using AspNetCore targeting .NET Core 3.1",
	$"tests/netcoreapp3.1/aspnetcore-test.dll", AspNetCoreResult));

static readonly FilePath[] AGENT_FILES = new FilePath[] {
		"agent/netcore31-agent.dll", "agent/netcore31-agent.pdb", "agent/netcore31-agent.dll.config",
		"agent/netcore31-agent.deps.json", $"agent/netcore31-agent.runtimeconfig.json",
		"agent/TestCentric.Agent.Core.dll",
		"agent/TestCentric.Engine.Api.dll", "agent/TestCentric.Extensibility.Api.dll",
		"agent/TestCentric.Extensibility.dll", "agent/TestCentric.Metadata.dll",
		"agent/TestCentric.InternalTrace.dll",
		"agent/Microsoft.Extensions.DependencyModel.dll"};

BuildSettings.Packages.Add(new NuGetPackage(
	"TestCentric.Extension.NetCore31PluggableAgent",
	title: ".NET Core 3.1 Pluggable Agent",
	description: "TestCentric engine extension for running tests under .NET Core 3.1",
	tags: new [] { "testcentric", "pluggable", "agent", "netcpreapp3.1" },
	packageContent: new PackageContent()
		.WithRootFiles("../../LICENSE.txt", "../../README.md", "../../testcentric.png")
		.WithDirectories(
			new DirectoryContent("tools").WithFiles(
				"netcore31-agent-launcher.dll", "netcore31-agent-launcher.pdb",
				"testcentric.extensibility.api.dll", "testcentric.engine.api.dll" ),
			new DirectoryContent("tools/agent").WithFiles(AGENT_FILES) ),
	testRunner: new AgentRunner(BuildSettings.NuGetTestDirectory + "TestCentric.Extension.NetCore31PluggableAgent." + BuildSettings.PackageVersion + "/tools/agent/netcore31-agent.dll"),
	tests: PackageTests) );
	
BuildSettings.Packages.Add(new ChocolateyPackage(
	"testcentric-extension-netcore31-pluggable-agent",
	title: ".NET 50 Pluggable Agent",
	description: "TestCentric engine extension for running tests under .NET Core 3.1",
	tags: new [] { "testcentric", "pluggable", "agent", "netcoreapp3.1" },
	packageContent: new PackageContent()
		.WithRootFiles("../../testcentric.png")
		.WithDirectories(
			new DirectoryContent("tools").WithFiles(
				"../../LICENSE.txt", "../../README.md", "../../VERIFICATION.txt",
				"netcore31-agent-launcher.dll", "netcore31-agent-launcher.pdb",
				"testcentric.extensibility.api.dll", "testcentric.engine.api.dll" ),
			new DirectoryContent("tools/agent").WithFiles(AGENT_FILES) ),
	testRunner: new AgentRunner(BuildSettings.ChocolateyTestDirectory + "testcentric-extension-netcore31-pluggable-agent." + BuildSettings.PackageVersion + "/tools/agent/netcore31-agent.dll"),
	tests: PackageTests) );

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

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(CommandLineOptions.Target.Value);
