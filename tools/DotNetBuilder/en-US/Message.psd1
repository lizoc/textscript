# Localized	28/10/2018 9:40 PM (GMT)	303:5.89.0411	Message.psd1
# DotNetBuilder BMLocalizedData.en-US

ConvertFrom-StringData @'

# ---- [ Localized Data ] ---------------------------------------------

LocaleName = en-US
AuthorLabel = Created by
VersionLabel = Version
Author = Build Team @Lizoc
Version = 3.15.4212.0
Syntax = SYNTAX
Example = EXAMPLE
Description = DESCRIPTION
Remarks = REMARKS
HelpTopics = OTHER HELP TOPICS
HelpTopicNotFound = [!] The help topic "{0}" is unavailable. Here are the topics we have:
Example1_1 = Build all projects in this repo. Configuration by default is "debug".
Example2_1 = Build all projects in this repo using the "release" configuration. You can also specify "debug" or "publish":
Example2_2 = * Debug   -> No optimization. Strong signing is not enabled.
Example2_3 = * Release -> Optimization. Strong signing where applicable.
Example2_4 = * Publish -> Same as release, plus pushing to NuGet.
Example3_1 = Build selective projects in this repo. Project names are folders under the "/source" folder that do not end with ".Tests".\n
Example3_2 = Project names are case sensitive. An error occurs if the project folder was not found.
VerifyingBuildParams = [i] Verifying build parameters...
InvalidConfigName = Invalid configuration. The following build configurations are supported: {0}
WillPushPackage = * Output packages will be uploaded to NuGet server if successfully compiled.
AreYouSure = [WARNING] ARE YOU SURE?
BuildAllWarning = You are targeting all the projects in this repository. This may take a long time if there are lots of projects.
BuildAllUserConfirm = To continue, type in "{0}" and press "ENTER".
BuildAllUserConfirmError = Unable to continue because the user did not confirm that the build jobs intends to target all projects.
BuildAllUserConfirmBadInput = [!] You must enter "{0}" to continue (or CTRL^C to abort)
BuildAllTips = TIPS: To perform a full build in a non-interactive environment, do this: build [publish] debug|release *
PressEnterToContinue = Press "ENTER" to continue...
ImportingModules = [i] Importing add-on modules
RemovingUnexpectedFile = [!] Trying to remove unexpected file: {0}\
CreatingDirectory = [i] Creating folder: {0}
CreatingWorkingFolder = [i] Creating working folder: {0}
CopyingSourceToWorking = [i] Copying source code to working folder: {0} -> {1}
ConfigPropertyNameReserved = The configuration property name is reserved: {0}
ImportingDefaultConfig = [i] Importing default configuration
UsingStockDefaultConfig = [i] Using generated default configuration
ImportingGlobalConfig = [i] Importing global configuration file
GlobalConfigNotFound = [i] The custom global configuration file was not found
AutoBuildVersionOverflow = [!] Automatic build version cannot increment anymore because it has reached the maximum supported value.
VersionLogCorrupted = [!] Recreating the version log file because it is corrupted.
AutoBuildVersion = [i] Setting automatic build version from {0} -> {1}
DiscoveringAvailableProjects = [i] Getting a list of projects available in this repository...
DoNotSpecifyTestProject = [!] Do not specify projects that ends with ".Tests" because they are test projects. Test projects are managed automatically.
TargetProjectNotFound = Unable to find the requested target project (project names are case-sensitive): {0}
SourceDirNotFound = The source folder was not found: {0}
DotnetSdkUrlUndefined = The .NET Core SDK URL is not defined. Check "https://github.com/dotnet/core" for the download URL of the latest SDK and update file "global.bsd".
DownloadingDotNetSdk = [i] Downloading .NET Core SDK from "{0}" -> "{1}"
FoundLocalDotNetSdkCache = [i] Using local cached copy of .NET Core SDK: {0}
InstallingDotNetSdk = [i] Installing .NET Core SDK to "{0}"
TestingDotNetSdk = [i] Verifying that the .NET Core SDK is available...
CriticalFileNotFound = Unable to continue because a required file was not found: {0}
BadConfigSchema = The effective configuration file does not conform to the requisite schema. The defective object path is "{0}".
Goodbye = My work here is done. Goodbye!
CreatingGlobalJson = [i] Creating global.json
ProcessingGlobalTemplate = [i] Processing global templates
CreatingSolutionFile = [i] Creating solution file
NothingToBuild = [!] There is nothing to build!
ProjectFileNotFound = Unable to build a project because its project file was missing: {0}
TestProjectFileNotFound = Unable to build a project because its test project file was missing: {0}
ProjectIncludeFileNotFound = Unable to build a project because an include file was missing: {0}
TemplateInfoNotFound = Unable to build a project because a required template was undefined: {0}
TemplatePathNotSpecified = A template definition did not specify a required property: path
ProcessingTemplates = [i] Generating additional code in project '{0}' using text templates
GenerateFromTemplate = --> gen {1} <- {0}
ProcessingTemplateWildcard = --> wildcard search for {0} (recurse = {1})
TemplateOutputPathAlreadyUsed = Unable to build a project because the output path for a template is unavailable: {0}
EnforceCopyrightHeader = [i] Enforce copyright header to {0}/{1}
AddCopyrightHeader = --> add {0}
ModifyCopyrightHeader = --> mod {0}
NoTestProject = [!] The project "{0}" does not have an accompanying unit test project.
CreatingPackage = Creating package...
NothingToClean = There is no project to clean.
CleaningProject = Cleaning project output: {0}
BuildingForConfiguration = Building project "{0}" under "{1}" configuration...
TestingForConfiguration = Running unit test for project "{0}" under "{1}" configuration...
SpecifyPathToNupkgFile = You need to specify the full path of .nupkg file(s) to publish.
NupkgFileNotFound = [!] Unable to publish to server because the package file is missing: {0}
PublishingCredFileNotFound = [!] Unable to publish to server because the credential file "{1}" is missing: {0}
PublishingCredMissing = [!] Unable to publish to server because the credential file "{1}" does not contain any credential information: {0}
PushingPackage = Pushing package: {0}
SymbolsPackageNotFound = [!] Symbols package not found!
PublishPackageWithSymbols = Target server with symbols: {0}
PublishPackageWithoutSymbols = Target server (symbols disabled): {0}
IncludePathIsNull = Failed to process an include directive because the path is null or an empty string.
IncludePathContainsPathSeparator = An include directive in BSD cannot contain path separator character.
IncludeNameNotFound = An include directive for "{0}" was not found.
PathOccupiedByDirectory = Unable to create file "{0}" because the path was occupied by a directory.

# ---- [ /Localized Data ] --------------------------------------------
'@
