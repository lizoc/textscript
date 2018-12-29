properties {
    [string]$Configuration = 'Debug'
    [string[]]$BuildTarget = @()
    [string]$HelpTopic = ''
    [string]$Subcommand = 'build'
}

printTask {
    param($taskName)

    if ($taskName -notin @('Help', 'Localize'))
    {
        say -NewLine -LineCount 3
        say ('+-' + ('-' * $taskName.Length) + '-+') -fg Blue
        say ('| {0} |' -f $taskName) -fg Blue
        say ('+-' + ('-' * $taskName.Length) + '-+') -fg Blue
    }
}

task default -depends Finish

task Localize {
    $psIgnoreAction = $(
        if ($PSVersionTable.PSVersion.Major -ge 3) { 'Ignore' } 
        else { 'SilentlyContinue' }
    )

    $noImportLocalizedDataCmdletErr = $null
    Get-Command Import-LocalizedData -ErrorVariable noImportLocalizedDataCmdletErr -ErrorAction $psIgnoreAction | Out-Null

    if ($noImportLocalizedDataCmdletErr[0] -eq $null)
    {
        Import-LocalizedData -BindingVariable BMLocalizedData -BaseDirectory $BuildEnv.BuildScriptDir -FileName 'Message.psd1'
    }
    else
    {
        die "Unable to find command 'Import-LocalizedData'. The Powershell application available may be incompatible."
    }

    $BuildEnv.BMLocalizedData = $BMLocalizedData
}

task Help -depends Localize -precondition { $Subcommand -eq 'Help' } {
    $sr = $BuildEnv.BMLocalizedData

    $availHelpTopics = dir (Join-Path $BuildEnv.BuildScriptDir -ChildPath "$($sr.LocaleName)/about_*.txt") -File | select -expand BaseName

    if ($HelpTopic)
    {
        
        $helpTopicFile = Join-Path $BuildEnv.BuildScriptDir -ChildPath "$($sr.LocaleName)/about_$HelpTopic.txt"
        if (-not (Test-Path $helpTopicFile -PathType Leaf))
        {
            say ($sr.HelpTopicNotFound -f $HelpTopic) -v 0
            $availHelpTopics | ForEach-Object {
                say ('- {0}' -f $_.Substring('about_'.Length))
            }
        }
        else
        {
            say ('{0}' -f $HelpTopic) -fg Magenta
            say ('{0}' -f ('=' * $HelpTopic.Length)) -fg Magenta
            $helpTopicContent = Get-Content -Path $helpTopicFile -Encoding UTF8
            say ($helpTopicContent -join [Environment]::NewLine)
        }

        exit 0
    }

    $syntax = @(
        'build /?|/help|-h|--help [help_topic]'
        'build configure'
        'build [debug|release]'
        'build debug|release [*|project1.name] [project2.name] [...]'
        'build clean [debug|release]'
        'build clean debug|release [*|project1.name] [project2.name] [...]'
        'build publish [path/to/file1.nupkg] [path/to/file1.nupkg] [...]'
    )

    $examples = @{}
    $exampleCounter = 1
    @(
    	'build'
    	'build release'
    	'build debug MyProject1 MyProject2'
    ) | ForEach-Object {
        $examples."$_" = @()
        for ($i = 1; $i -lt 100; $i++)
        {
            $exampleLineName = 'Example{0}_{1}' -f $exampleCounter, $i
            if (-not $sr."$exampleLineName")
            {
                break
            }
            else
            {
                $examples."$_" += $sr."$exampleLineName"
            }
        }
        $exampleCounter += 1
    }

    $author = $sr.Author
    $version = $sr.Version

    # -------------------------------------

    say $sr.Syntax -fg Magenta
    $syntax | ForEach-Object {
        say "    $_" -fg Green
    }

    say -NewLine -LineCount 2

    $exampleCounter = 1
    $examples.Keys | ForEach-Object {
        say ('{0} #{1}' -f $sr.Example, $exampleCounter) -fg Magenta
        say ('    ' + $_) -fg Green
        say -NewLine
        say ('    {0}' -f $sr.Description) -fg Cyan
        say ('    {0}' -f ('-' * $sr.Description.Length)) -fg Cyan
        $examples."$_" | ForEach-Object {
            if ($_) { say ('    ' + $_) }
            else { say -NewLine }
        }
        say -NewLine

        $exampleCounter += 1
    }

    say $sr.Remarks -fg Magenta
    say ('    {0} {1}' -f $sr.VersionLabel, $version)
    say ('    {0} {1}' -f $sr.AuthorLabel, $author)

    if ($availHelpTopics)
    {
        say -NewLine
        say $sr.HelpTopics -fg Magenta
        say '    build /? [help_topic]' -fg Green
        say -NewLine
        $availHelpTopics | ForEach-Object {
            say ('    - {0}' -f $_.Substring('about_'.Length))
        }
    }

    exit 0
}

task Precheck -depends Localize {
    $sr = $BuildEnv.BMLocalizedData

    say $sr.VerifyingBuildParams

    if ($Subcommand -in @('build', 'clean'))
    {
	    $supportedConfigs = @('Release', 'Debug')
	    assert ($Configuration -in $supportedConfigs) ($sr.InvalidConfigName -f ($supportedConfigs -join ', '))

	    if (-not $BuildTarget)
	    {
	    	$explicitUserConfirmInput = 'target all'

	        say $sr.AreYouSure -v 0 -fg Red
	        say $sr.BuildAllWarning -v 0 -fg Yellow
	        say -NewLine
	        say ($sr.BuildAllUserConfirm -f $explicitUserConfirmInput)
	        say -NewLine
	        for ($i = 0; $i -lt 3; $i++)
	        {
	            $confirmInput = Read-Host '>'
	            if ($confirmInput -ne $explicitUserConfirmInput)
	            {
	                if ($i -eq 2)
	                {
	                    assert $false $sr.BuildAllUserConfirmError
	                }
	                else
	                {
	                    say ($sr.BuildAllUserConfirmBadInput -f $explicitUserConfirmInput) -v 0 -fg Red
	                }
	            }
	            else
	            {
	                $BuildTarget = '*'
	                say $sr.BuildAllTips -fg Green
	                Read-Host $sr.PressEnterToContinue
	                break
	            }
	        }
	    }
    }

    if ($Subcommand -eq 'Publish')
    {
        $BuildEnv.UploadToNuget = $true
        say $sr.WillPushPackage -v 2
        assert ($BuildTarget.Count -ne 0) ($sr.SpecifyPathToNupkgFile)
    }
}

task Setup -depends Precheck {
    $sr = $BuildEnv.BMLocalizedData

    # basic assumption -- tools folder is 1 level above this script
    $BuildEnv.toolsDir = (Get-Item $BuildEnv.BuildScriptDir).Parent.FullName
    $BuildEnv.rootDir = Get-Item $BuildEnv.BuildScriptDir | select -expand Root | select -expand FullName
    $BuildEnv.repoDir = (Get-Item $BuildEnv.toolsDir).Parent.FullName
    $BuildEnv.repoName = Split-Path $BuildEnv.repoDir -Leaf

    # ~~~~~~~~~~~~~~~~~
    # import modules
    # ~~~~~~~~~~~~~~~~~
    say ($sr.ImportingModules)
    @(
        'JsonPlus/JsonPlus.psd1'
        'TextScript/Lizoc.PowerShell.TextScript.dll'
        'Robocopy.psm1'
    ) | ForEach-Object {
        say ('* {0}' -f $_) -v 2
        ipmo (Join-Path $BuildEnv.toolsDir -ChildPath $_) -Force
    }


    # ~~~~~~~~~~~~~~~~~
    # global config
    # ~~~~~~~~~~~~~~~~~
    # The default bsd content that real bsd files will override
    $defaultBsd = @(
        "build-configuration = '{0}'" -f $Configuration
        "repoName = '{0}'" -f $BuildEnv.repoName
        ''
        '# --- env ---'
        "hostOS = '{0}'" -f $env:OS
        "dirSeparator = '{0}'" -f [System.IO.Path]::DirectorySeparatorChar.ToString().Replace('\', '\\')
        ''
        '# --- paths ---'
        "toolsDir = '{0}'" -f $BuildEnv.toolsDir.Replace('\', '/')
        "rootDir = '{0}'" -f $BuildEnv.rootDir.Replace('\', '/')
        'repoDir = ${toolsDir}/..'
        'tempDir = ${repoDir}/temp'
        'workingDir = ${repoDir}/working'
        'workingSourceDir = ${workingDir}/src'
        'pkgDir = ${workingDir}/packages'
        'dotnetSdkDir = ${pkgDir}/dotnet-sdk'
        'sourceDir = ${repoDir}/src'
        'outputDir = ${workingDir}/bin'
        'outputObjDir = ${workingDir}/obj'
        'credDir = ${repoDir}/credentials'
        'releaseDir = ${repoDir}/releases'
        'docDir = ${repoDir}/docs'
        'globalConfigFile = ${sourceDir}/global.bsd'
        "includeDiscoveryDir = ['{0}', {1}, {2}]" -f $BuildEnv.BuildScriptDir.Replace('\', '/'), '${toolsDir}', '${sourceDir}'
        '# --- /paths ---'
        ''
        '# --- versioning ---'
        'versioning {'
        '  major = 0'
        '  minor = 1'
        '  revision = 0'
        '  suffix = beta'
        '  release-build = 0'
        '  debug-build = 0'
        '}'
        '# --- /versioning ---'
        ''
        'source {'
        '  ignore {'
        "    file = ['*.suo', '*.user', '*.lock.json']"
        "    folder = ['.vs', 'TestResults']"
        '  }'
        '}'
    )

    # source.ignore --
    #    "    file = ['*.suo', '*.user', '*.lock.json']"
    #    "    folder = ['bin', 'obj', 'TestResults', 'AppPackages', 'packages', '.vs', 'artifacts']"

    # override with tools/config.bsd (if available)
    $defaultConfigPath = Join-Path $BuildEnv.toolsDir -ChildPath 'config.bsd'
    if (Test-Path $defaultConfigPath -PathType Leaf)
    {
        say $sr.ImportingDefaultConfig
        $defaultBsd += Get-Content $defaultConfigPath -Encoding UTF8
    }
    else
    {
        say $sr.UsingStockDefaultConfig
    }

    # parse round 1
    $defaultConfig = ConvertFrom-JsonPlus ($defaultBsd -join [Environment]::NewLine)

    # if global config file found, parse round 2
    $globalBsd = $defaultBsd
    if (Test-Path $defaultConfig.globalConfigFile -PathType Leaf)
    {
        say $sr.ImportingGlobalConfig
        $globalBsd += (Get-Content $defaultConfig.globalConfigFile -Encoding UTF8)

        $globalConfig = ConvertFrom-JsonPlus ($globalBsd -join [Environment]::NewLine)
    }
    else
    {
        say $sr.GlobalConfigNotFound
        $globalConfig = $defaultConfig
    }

    $allGlobalConfigKeys = $globalConfig | Get-Member -MemberType NoteProperty | select -expand Name
    $ignoreConfigKeys = @('toolsDir', 'rootDir', 'repoDir', 'repoName')
    # make sure bsd doesn't contain reserved property names
    $allGlobalConfigKeys | where { $_ -notin $ignoreConfigKeys } | ForEach-Object {
        assert ($_ -ne 'Keys') ($sr.ConfigPropertyNameReserved -f 'Keys')
        assert ($_ -notin $BuildEnv.Keys) ($sr.ConfigPropertyNameReserved -f $_)
    }
    # now they are all safe
    # note that some special properties are not overridable by custom config: ignoreConfigKeys
    $allGlobalConfigKeys | where { $_ -notin $ignoreConfigKeys } | ForEach-Object {
        $BuildEnv."$_" = $globalConfig."$_"
    }

    $BuildEnv.globalConfigText = $globalBsd

    # ~~~~~~~~~~~~~~~~~
    # check prerequisite files
    # ~~~~~~~~~~~~~~~~~
    if ($Subcommand -eq 'Configure')
    {
	    if ($BuildEnv.templates)
	    {
	        $BuildEnv.templates | Get-Member -MemberType NoteProperty | select -expand Name | ForEach-Object {
	            assert ($BuildEnv.templates."$_".path) ($sr.BadConfigSchema -f "templates.$_.path")
	            assert (Test-Path $BuildEnv.templates."$_".path -PathType Leaf) ($sr.CriticalFileNotFound -f $BuildEnv.templates."$_".path)
	        }
	    }
	}

    # ~~~~~~~~~~~~~~~~~
    # set up working dir
    # ~~~~~~~~~~~~~~~~~
    if ($Subcommand -eq 'Configure')
    {
	    assert (Test-Path $BuildEnv.sourceDir -PathType Container) ($sr.SourceDirNotFound -f $BuildEnv.sourceDir)

	    if (-not (Test-Path $BuildEnv.workingSourceDir -PathType Container))
	    {
	        if (Test-Path $BuildEnv.workingSourceDir -PathType Leaf) 
	        {
	            say ($sr.RemovingUnexpectedFile -f $BuildEnv.workingSourceDir) -v 0
	            del $BuildEnv.workingSourceDir
	        }

	        say ($sr.CreatingWorkingFolder -f $BuildEnv.workingSourceDir)
	        md $BuildEnv.workingSourceDir -Force | Out-Null
	    }

	    say ($sr.CopyingSourceToWorking -f $BuildEnv.sourceDir, $BuildEnv.workingSourceDir)
	    $robocopyParams = @{
	        SourcePath = $BuildEnv.sourceDir
	        DestinationPath = $BuildEnv.workingSourceDir
	        Mirror = $true
	        ExcludeFolder = $BuildEnv.source.ignore.Folder
	        ExcludeFile = $BuildEnv.source.ignore.File
	    }

	    if (-not (Test-Path $robocopyParams.DestinationPath -PathType Container))
	    {
	        md $robocopyParams.DestinationPath | Out-Null
	    }
	    Invoke-Robocopy @robocopyParams
	}


    # ~~~~~~~~~~~~~~~~~
    # autoincrement version
    # ~~~~~~~~~~~~~~~~~
    $useBuildVerLog = $false
    if ((($Configuration -eq 'Debug') -and ($BuildEnv.versioning.'debug-build' -eq 'auto')) -or
        (($Configuration -ne 'Debug') -and ($BuildEnv.versioning.'release-build' -eq 'auto')))
    {
        $useBuildVerLog = $true
    }

    if ($useBuildVerLog)
    {
        $autoBuildNumber = 0
        $lastAutoBuildNumber = 0
        $verlogPath = Join-Path $BuildEnv.sourceDir -ChildPath 'autobuildver.log'

        if (Test-Path $verlogPath -PathType Leaf)
        {
            $verlogText = Get-Content -Path $verlogPath -Encoding UTF8 | select -First 1
            $verlogIsValid = [int]::TryParse($verlogText, [ref]$lastAutoBuildNumber)
            if ($verlogIsValid)
            {
	        if ($lastAutoBuildNumber -eq [int]::MaxValue)
	        {
	            say $sr.AutoBuildVersionOverflow -v 0
	            $autoBuildNumber = $lastAutoBuildNumber
	        }
	        else
	        {
	            $autoBuildNumber = [math]::Max($lastAutoBuildNumber + 1, 0)
	            $autoBuildNumber | Set-Content -Path $verlogPath -Encoding UTF8
	        }

	        $autoBuildNumber | Set-Content -Path $verlogPath -Encoding UTF8
            }
            else
            {
                say $sr.VersionLogCorrupted -v 0
                '0' | Set-Content -Path $verlogPath -Encoding UTF8
           }
        }
        else
        {
            if (Test-Path $verlogPath -PathType Container)
            {
                die ($sr.PathOccupiedByDirectory -f $verlogPath)
            }

            '0' | Set-Content -Path $verlogPath -Encoding UTF8
        }

        $BuildEnv.versioning | Add-Member -MemberType NoteProperty -Name 'build' -Value $autoBuildNumber
        $BuildEnv.globalConfigText += 'versioning.build = {0}' -f $autoBuildNumber

        say ($sr.AutoBuildVersion -f $lastAutoBuildNumber, $autoBuildNumber)
    }
    else
    {
        if ($Configuration -eq 'Debug')
        {
            $BuildEnv.versioning | Add-Member -MemberType NoteProperty -Name 'build' -Value $BuildEnv.versioning.'debug-build'
            $BuildEnv.globalConfigText += 'versioning.build = ${versioning.debug-build}'
        }
        else
        {
            $BuildEnv.versioning | Add-Member -MemberType NoteProperty -Name 'build' -Value $BuildEnv.versioning.'release-build'
            $BuildEnv.globalConfigText += 'versioning.build = ${versioning.release-build}'
        }
    }

    $fullVersion = '{0}.{1}.{2}.{3}' -f $BuildEnv.versioning.major, $BuildEnv.versioning.minor, $BuildEnv.versioning.build, $BuildEnv.versioning.revision
    if ($BuildEnv.versioning.suffix) 
    { 
        $fullVersion = $fullVersion + '-' + $BuildEnv.versioning.suffix
    }
    $BuildEnv.versioning | Add-Member -MemberType NoteProperty -Name 'full' -Value $fullVersion
    $BuildEnv.globalConfigText += "versioning.full = '{0}'" -f $fullVersion


    # ~~~~~~~~~~~~~~~~~
    # temp dir to hold downloads mostly...
    # ~~~~~~~~~~~~~~~~~
    if (-not (Test-Path $BuildEnv.tempDir -PathType Container))
    {
        if (Test-Path $BuildEnv.tempDir -PathType Leaf)
        {
            del $BuildEnv.tempDir -Force
        }
        md $BuildEnv.tempDir | Out-Null
    }


    # ~~~~~~~~~~~~~~~~~
    # download .net sdk
    # ~~~~~~~~~~~~~~~~~
    if (($Subcommand -eq 'Configure') -and 
    	(-not (Test-Path $BuildEnv.dotnetSdkDir -PathType Container)))
    {
        md $BuildEnv.dotnetSdkDir | Out-Null

        $dotnetSdkUrl = $BuildEnv.dotnetSdkUrl
        assert ($dotnetSdkUrl -ne '') ($sr.DotnetSdkUrlUndefined)

        $downloadSdkFile = $dotnetSdkUrl.Split('/')[-1]
        $downloadSdkPath = Join-Path $BuildEnv.tempDir -ChildPath $downloadSdkFile
        if (-not (Test-Path $downloadSdkPath -PathType Leaf))
        {
            say ($sr.DownloadingDotNetSdk -f $dotnetSdkUrl, $downloadSdkPath)
            Invoke-WebRequest -Uri $dotnetSdkUrl -OutFile $downloadSdkPath
        }
        else
        {
            say ($sr.FoundLocalDotNetSdkCache -f $downloadSdkPath)
        }

        say ($sr.InstallingDotNetSdk -f $BuildEnv.dotnetSdkDir)
        Expand-Archive -Path $downloadSdkPath -DestinationPath $BuildEnv.dotnetSdkDir
    }

    $sr = $BuildEnv.BMLocalizedData

    say $sr.TestingDotNetSdk
    envpath $BuildEnv.dotnetSdkDir
    if (-not $BuildEnv.enableDotNetSdkTelemetry)
    {
        $env:DOTNET_CLI_TELEMETRY_OPTOUT = 1
    }
    exec { dotnet --info }


    # ~~~~~~~~~~~~~~~~~
    # misc required folders
    # ~~~~~~~~~~~~~~~~~
    @(
        'outputDir', 'outputObjDir'
        'credDir', 'releaseDir', 'docDir', 'pkgDir'
    ) | ForEach-Object {
        if (Test-Path $BuildEnv."$_" -PathType Leaf)
        {
            say ($sr.RemovingUnexpectedFile -f $BuildEnv."$_")
            del $BuildEnv."$_" -Force
        }

        if (-not (Test-Path $BuildEnv."$_"))
        {
            say ($sr.CreatingDirectory -f $BuildEnv."$_")
            md $BuildEnv."$_" | Out-Null
        }
    }
}

task Discover -depends Setup {
    $sr = $BuildEnv.BMLocalizedData

    # all folders in working\source are projects
    say ($sr.DiscoveringAvailableProjects)
    $availProjects = dir $BuildEnv.workingSourceDir -Directory | select -expand Name | where { $_ -notin $BuildEnv.source.ignore.folder }

    # what project to build? resolve in the following order:
    # - user specified build target via commandline
    # - build all available
    $buildAll = $false
    $requestedBuildTargets = @()

    if ((-not $BuildTarget) -or ($BuildTarget -eq '*'))
    {
        $buildAll = $true
        $requestedBuildTargets = $availProjects | where { 
            ($_ -ne $null) -and 
            ($_ -ne '') -and 
            ($_ -notlike '*.Tests') 
        }
    }
    else
    {
        $requestedBuildTargets = $BuildTarget
    }

    $effectiveBuildTargets = @()

    $requestedBuildTargets | where { ($_ -ne $null) -and ($_ -ne '') } | ForEach-Object {
        if ($_ -like '*.Tests')
        {
            say $sr.DoNotSpecifyTestProject -v 0
        }
        else
        {
            assert ($_ -in $availProjects) ($sr.TargetProjectNotFound -f $_)

            $projectInfoPath = Join-Path $BuildEnv.workingSourceDir -ChildPath ('{0}/project.bsd' -f $_)
            assert (Test-Path $projectInfoPath -PathType Leaf) ($sr.ProjectFileNotFound -f $_)

            $testProjectPath = Join-Path $BuildEnv.workingSourceDir -ChildPath ('{0}.Tests' -f $_)
            $projectTestInfoPath = Join-Path $testProjectPath -ChildPath 'project.bsd'
            if (Test-Path $testProjectPath -PathType Container)
            {
                assert (Test-Path $projectTestInfoPath -PathType Leaf) ($sr.TestProjectFileNotFound -f $_)
            }

            say ('- {0}' -f $_)
            $effectiveBuildTargets += $_
        }
    }

    $BuildEnv.buildAll = $buildAll
    $BuildEnv.EffectiveBuildTarget = $effectiveBuildTargets
}

task Configure -depends Discover -precondition { $Subcommand -eq 'Configure' } {
    $sr = $BuildEnv.BMLocalizedData

    if ($BuildEnv.EffectiveBuildTarget.Count -eq 0)
    {
        say $sr.NothingToBuild -v 0
        return
    }

    $repoBasePath = [System.IO.Path]::GetFullPath($BuildEnv.repoDir)

    # -----------
    # working\source\global.json
    # -----------
    say $sr.CreatingGlobalJson
    exec { dotnet new globaljson -o $BuildEnv.workingSourceDir }


    # -----------
    # global level templates
    # -----------
    # we need to regen the templates every time we reconfigure, because (1) template may have changed, or (2) BuildEnv may have changed
    say ($sr.ProcessingGlobalTemplate)
    $BuildEnv.templates | Get-Member -MemberType NoteProperty | select -expand Name | ForEach-Object {
        if ($BuildEnv.templates."$_".global -eq $true)
        {
            $globalTmplPath = $BuildEnv.templates."$_".path
            $globalTmplOutputPath = Join-Path $BuildEnv."$($BuildEnv.templates."$_".outputBasePath)" -ChildPath $BuildEnv.templates."$_".outputPath

            $globalTmplPath = [System.IO.Path]::GetFullPath($globalTmplPath)
            $globalTmplOutputPath = [System.IO.Path]::GetFullPath($globalTmplOutputPath)

            $globalTmplSubPath = $(
                if ($globalTmplPath.StartsWith($repoBasePath)) { $globalTmplPath.Substring($repoBasePath.Length) }
                else { $globalTmplPath }
            )
            $globalTmplOutputSubPath = $(
                if ($globalTmplOutputPath.StartsWith($repoBasePath)) { $globalTmplOutputPath.Substring($repoBasePath.Length) }
                else { $globalTmplOutputPath }
            )
            say ($sr.GenerateFromTemplate -f $globalTmplSubPath, $globalTmplOutputSubPath)

            $convertFromTemplateParams = @{
                InputObject = $BuildEnv
                Template = ((Get-Content -Path $globalTmplPath -Encoding UTF8) -join [Environment]::NewLine)
            }
            ConvertFrom-Template @convertFromTemplateParams | Set-Content -Path $globalTmplOutputPath -Encoding UTF8
        }
    }

    # -----------
    # project level templates
    # -----------
    if (-not $BuildEnv.includeDiscoveryDir)
    {
        $BuildEnv.includeDiscoveryDir = $BuildEnv.BuildScriptDir.Replace('/', '\')
    }

    $allBuildTargets = @()
    for ($i = 0; $i -lt $BuildEnv.EffectiveBuildTarget.Count; $i++)
    {
    	$allBuildTargets += $BuildEnv.EffectiveBuildTarget[$i]
    	
    	$testProjectName = $BuildEnv.EffectiveBuildTarget[$i] + '.Tests'
    	$testProjectDir = Join-Path $BuildEnv.workingSourceDir -ChildPath $testProjectName
    	if (Test-Path $testProjectDir -PathType Container)
    	{
    		$allBuildTargets += $testProjectName
    	}
    	else
    	{
    		say ($sr.NoTestProject -f $BuildEnv.EffectiveBuildTarget[$i]) -v 0
    	}
    }

    $allMakeFiles = @()
    for ($i = 0; $i -lt $allBuildTargets.Count; $i++)
    {
        $projectDir = Join-Path $BuildEnv.workingSourceDir -ChildPath $allBuildTargets[$i]
        $projectName = $allBuildTargets[$i]

        $projectEffectiveBsd = @()

        # global config
        $projectEffectiveBsd += $BuildEnv.globalConfigText

        # project-level auto properties
        $projectDefaultBsd = @()
        $projectDefaultBsd += "projectName = '{0}'" -f $allBuildTargets[$i]
        $projectDefaultBsd += "projectDir = '{0}'" -f $projectDir.Replace('\', '\\')

        Push-Location
        cd $projectDir
        @(
            'toolsDir', 'repoDir', 'tempDir', 'workingDir', 'workingSourceDir'
            'pkgDir', 'dotnetSdkDir', 'sourceDir', 'outputDir', 'outputObjDir'
            'credDir', 'releaseDir', 'docDir'
        ) | ForEach-Object {
            $projectDefaultBsd += "{0}RelativeDir = '{1}'" -f $_.Substring(0, $_.Length - 'Dir'.Length), (Resolve-Path -Path $BuildEnv.$_ -Relative).Replace('\', '\\')
        }
        Pop-Location

        $projectEffectiveBsd += ($projectDefaultBsd -join [Environment]::NewLine)

        # bsd includes
        $projectInfoPath = Join-Path $projectDir -ChildPath 'project.bsd'
        $projectInfoContent = Get-Content -Path $projectInfoPath -Encoding UTF8
        $projectEffectiveBsd += ($projectInfoContent -join [Environment]::NewLine)

        # eval effective bsd
        $projectConfig = ConvertFrom-JsonPlus ($projectEffectiveBsd -join [Environment]::NewLine) -Include {
            $inclName = $args[0]

            if ($inclName -eq $null) 
            { 
                throw $sr.IncludePathIsNull
            }
            
            if ($inclName.Contains('/') -or $inclName.Contains('\'))
            {
                throw $sr.IncludePathContainsPathSeparator
            }

            $inclExists = $false
            foreach ($inclDir in $BuildEnv.includeDiscoveryDir)
            {
                $inclFilePath = Join-Path $inclDir -ChildPath ($inclName + '.bsd')
                if (Test-Path $inclFilePath -PathType Leaf)
                {
                    $inclExists = $true
                    Get-Content $inclFilePath -Raw
                    break
                }
            }

            if ($inclExists -eq $false)
            {
                throw ($sr.IncludeNameNotFound -f $inclName)
            }
        }

        $projectConfigHashtable = @{}
        $projectConfig | Get-Member -MemberType NoteProperty | select -expand Name | ForEach-Object {
            $projectConfigHashtable."$_" = $projectConfig."$_"
        }

        # used for beautifying messages
        $projectBasePath = $projectConfig.projectDir.Substring(0, $projectConfig.projectDir.Length - $projectConfig.projectDir.Split('/').Split('\')[-1].Length)
        $projectBasePath = [System.IO.Path]::GetFullPath($projectBasePath)
        
        # process all templates
        say ($sr.ProcessingTemplates -f $projectName)
        $projectConfig.files | Get-Member -MemberType NoteProperty | select -expand Name | ForEach-Object {
            if ($projectConfig.files."$_".type -eq 'template')
            {
                $tmplInfo = $projectConfig.templates."$_"
                assert ($tmplInfo -ne $null) ($sr.TemplateInfoNotFound -f $_)
                assert ($tmplInfo.path -ne $null) ($sr.TemplatePathNotSpecified)

                if ($tmplInfo.path.Contains('*'))
                {
                    $tmplPathList = dir $tmplInfo.path -Recurse:$(if ($tmplInfo.Recurse -eq $true) { $true } else { $false }) -File
                    say ($sr.ProcessingTemplateWildcard -f $tmplInfo.path, ($tmplInfo.recurse -eq $true))

                    $tmplPathList | where { $_ -ne $null } | ForEach-Object {
                        $tmplPath = $_.FullName
                        $tmplOutputPath = Join-Path $_.Directory.FullName -ChildPath $_.BaseName

                        assert (-not (Test-Path $tmplOutputPath -PathType Container)) ($sr.TemplateOutputPathAlreadyUsed -f $tmplOutputPath)

                        $tmplSubPath = $tmplPath.Substring($projectBasePath.Length)
                        $tmplOutputSubPath = $tmplOutputPath.Substring($projectBasePath.Length)

                        say ($sr.GenerateFromTemplate -f $tmplSubPath, $tmplOutputSubPath)
                        $convertFromTemplateParams = @{
                            InputObject = $projectConfigHashtable
                            Template = ((Get-Content -Path $tmplPath -Encoding UTF8) -join [Environment]::NewLine)
                        }
                        ConvertFrom-Template @convertFromTemplateParams | Set-Content -Path $tmplOutputPath -Encoding UTF8
                    }
                }
                else
                {
                    $tmplOutputPath = Join-Path $projectConfig."$($tmplInfo.outputBasePath)" -ChildPath $tmplInfo.outputPath
                    $tmplOutputPath = [System.IO.Path]::GetFullPath($tmplOutputPath)
                    $tmplOutputPathParent = Split-Path $tmplOutputPath -Parent
                    $tmplOutputSubPath = $tmplOutputPath.Substring($projectBasePath.Length)

                    $tmplSubPath = [System.IO.Path]::GetFullPath($tmplInfo.path).Substring($repoBasePath.Length)

                    assert (-not (Test-Path $tmplOutputPath -PathType Container)) ($sr.TemplateOutputPathAlreadyUsed -f $tmplOutputPath)

                    if (-not (Test-Path $tmplOutputPathParent))
                    {
                        md $tmplOutputPathParent | Out-Null
                    }
                    elseif (Test-Path $tmplOutputPathParent -PathType Leaf)
                    {
                        die ($sr.TemplateOutputPathAlreadyUsed -f $tmplOutputPathParent)
                    }

                    say ($sr.GenerateFromTemplate -f $tmplSubPath, $tmplOutputSubPath)
                    $convertFromTemplateParams = @{
                        InputObject = $projectConfigHashtable
                        Template = ((Get-Content -Path $tmplInfo.path -Encoding UTF8) -join [Environment]::NewLine)
                    }
                    ConvertFrom-Template @convertFromTemplateParams | Set-Content -Path $tmplOutputPath -Encoding UTF8
                }
            }
        }

        #add copyright headers
        if ($projectConfig.copyrightHeader -is [psobject])
        {
            $projectConfig.copyrightHeader | Get-Member -MemberType NoteProperty | select -expand Name | ForEach-Object {
                $fileSelectWildcard = $_
                $fileSelectRecurse = $projectConfig.copyrightHeader."$_".recurse
                $copyrightHeaderRegex = $projectConfig.copyrightHeader."$_".find 
                $copyrightHeaderTmpl = $projectConfig.copyrightHeader."$_".replace
                $fileSelectPath = Join-Path $projectConfig.projectDir -ChildPath $fileSelectWildcard

                say ($sr.EnforceCopyrightHeader -f $projectName, $fileSelectWildcard)

                $fileSelect = dir $fileSelectPath -Recurse:$fileSelectRecurse -File
                foreach ($fileInfo in $fileSelect)
                {
                    $fileSubPath = $fileInfo.FullName.Substring($projectBasePath.Length)

                    $fileContent = Get-Content -Path $fileInfo.FullName -Raw
                    $copyrightHeader = $copyrightHeaderTmpl.Replace('${filename}', $fileInfo.Name)
                    if ($fileContent -notmatch $copyrightHeaderRegex)
                    {
                        say ($sr.AddCopyrightHeader -f $fileSubPath)
                        $fileNewContent = $copyrightHeader + $fileContent
                        $fileNewContent | Set-Content -Path $fileInfo.FullName -NoNewline -Encoding UTF8
                    }
                    elseif (-not $fileContent.StartsWith($copyrightHeader)) 
                    {
                        say ($sr.ModifyCopyrightHeader -f $fileSubPath)
                        $fileNewContent = $fileContent -replace $copyrightHeaderRegex, $copyrightHeader
                        $fileNewContent | Set-Content -Path $fileInfo.FullName -NoNewline -Encoding UTF8
                    }
                }
            }
        }

        # make file
        $projectMakeFilePath = Join-Path $projectConfig.projectDir -ChildPath ($projectConfig.projectName + $projectConfig.compiler.makeFileExtension)
        $allMakeFiles += Resolve-Path $projectMakeFilePath | select -expand Path
    }

    # -----------
    # working\source\<repoName>.sln
    # -----------
    say $sr.CreatingSolutionFile
    exec { dotnet new sln -n $BuildEnv.repoName -o $BuildEnv.workingSourceDir }

	# add all projects to solution
	$escapeParser = '--%'
	$solutionFilePath = Resolve-Path (Join-Path $BuildEnv.workingSourceDir -ChildPath ('{0}.sln' -f $BuildEnv.repoName)) | select -expand Path
	$dotnetSlnAddProjects = ($allMakeFiles | ForEach-Object { '"{0}"' -f $_ }) -join ' '
    exec { dotnet sln $solutionFilePath add $escapeParser $dotnetSlnAddProjects }
}

task Build -depends Discover -precondition { $Subcommand -eq 'Build' } {
    # #todo
    # [ ] aot

    $sr = $BuildEnv.BMLocalizedData

    if ($BuildEnv.EffectiveBuildTarget.Count -eq 0)
    {
        say $sr.NothingToBuild -v 0
        return
    }

    for ($i = 0; $i -lt $BuildEnv.EffectiveBuildTarget.Count; $i++)
    {
        $projectName = $BuildEnv.EffectiveBuildTarget[$i]
        $projectDir = Join-Path $BuildEnv.workingSourceDir -ChildPath $projectName
        
        $testProjectName = $projectName + '.Tests'
        $testProjectDir = Join-Path $BuildEnv.workingSourceDir -ChildPath $testProjectName
        $testProjectExists = $false
        if (Test-Path (Join-Path $testProjectDir -ChildPath "project.bsd") -PathType Leaf)
        {
            $testProjectExists = $true
        }

        $buildConfiguration = 'Debug'
        if ($Configuration -ne 'Debug')
        {
            $buildConfiguration = 'Release'
        }

        say ($sr.BuildingForConfiguration -f $projectName, $buildConfiguration)

        Push-Location
        cd $projectDir
        exec { dotnet build -c $buildConfiguration }
        Pop-Location

        if ($testProjectExists)
        {
            say ($sr.TestingForConfiguration -f $projectName, $buildConfiguration)
            Push-Location
            cd $testProjectDir
            # xunit console output doesn't work without this
            exec { dotnet test --logger:"console;verbosity=detailed" }
            Pop-Location
        }
        else
        {
            say ($sr.NoTestProject -f $projectName)
        }

        if ($buildConfiguration -eq 'Release')
        {
            say ($sr.CreatingPackage)
            Push-Location
            cd $projectDir
            exec { dotnet pack --include-symbols -c Release -o $BuildEnv.releaseDir }
            Pop-Location
        }

        say -Divider
    }
}

task Clean -depends Discover -precondition { $Subcommand -eq 'Clean' } {
    $sr = $BuildEnv.BMLocalizedData

    if ($BuildEnv.EffectiveBuildTarget.Count -eq 0)
    {
        say $sr.NothingToClean -v 0
        return
    }

    $allBuildTargets = @()
    for ($i = 0; $i -lt $BuildEnv.EffectiveBuildTarget.Count; $i++)
    {
    	$allBuildTargets += $BuildEnv.EffectiveBuildTarget[$i]
    	
    	$testProjectName = $BuildEnv.EffectiveBuildTarget[$i] + '.Tests'
    	$testProjectDir = Join-Path $BuildEnv.workingSourceDir -ChildPath $testProjectName
    	if (Test-Path $testProjectDir -PathType Container)
    	{
    		$allBuildTargets += $testProjectName
    	}
    	else
    	{
    		say ($sr.NoTestProject -f $BuildEnv.EffectiveBuildTarget[$i]) -v 0
    	}
    }

    $allBuildTargets | ForEach-Object {
    	$projectDir = Join-Path $BuildEnv.workingSourceDir -ChildPath $_ 
		say ($sr.CleaningProject -f $projectDir)
		exec { dotnet clean $projectDir -c $Configuration }
	}
}

task Publish -depends Setup -precondition { $Subcommand -eq 'Publish' } {
    $sr = $BuildEnv.BMLocalizedData

    # precheck will have ensured there is at least 1 BuildTarget, which is the file path to nupkg file in this case.
    # we still need to make sure it exists.

    $publishServers = @()
    $BuildEnv.package.publish | Get-Member -MemberType NoteProperty | select -expand Name | ForEach-Object {
        $pkgServerName = $_
        $pkgServerInfo = $BuildEnv.package.publish."$_"

        if (($pkgServerInfo.apiSchema -eq 'oneget') -and ($pkgServerInfo.disabled -ne $true))
        {
            $pkgServerCredFilePath = Join-Path $BuildEnv.credDir -ChildPath ($pkgServerName + '.repokey')
            if (-not (Test-Path $pkgServerCredFilePath -PathType Leaf))
            {
                say ($sr.PublishingCredFileNotFound -f $pkgServerName, $pkgServerCredFilePath)
            }
            else
            {
                $pkgServerCred = Get-Content $pkgServerCredFilePath -Encoding UTF8

                if (($pkgServerCred.Count -lt 1) -or ($pkgServerCred[0] -eq ''))
                {
                    say ($sr.PublishingCredMissing -f $pkgServerName, $pkgServerCredFilePath)
                }
                else
                {
                    $pkgServerInfo | Add-Member -MemberType NoteProperty -Name name -Value $pkgServerName
                    $pkgServerInfo | Add-Member -MemberType NoteProperty -Name apiKey -Value @($pkgServerCred)[0]
                    if (($pkgServerCred.Count -gt 1) -and ($pkgServerCred[1] -ne ''))
                    {
                        $pkgServerInfo | Add-Member -MemberType NoteProperty -Name symbolsApiKey -Value $pkgServerCred[1]
                    }
                    $publishServers += $pkgServerInfo
                }
            }
        }
    }

    $pkgList = @()
    $BuildTarget | ForEach-Object {
        $pkgLiteralPath = $_
        if (-not [System.IO.Path]::IsPathRooted($pkgLiteralPath))
        {
            $pkgLiteralPath = Join-Path $BuildEnv.repoDir -ChildPath $pkgLiteralPath
        }
        
        assert (Test-Path $pkgLiteralPath -PathType Leaf) ($sr.NupkgFileNotFound -f $pkgLiteralPath)
        $pkgLiteralPath = Resolve-Path $pkgLiteralPath | select -expand Path

        $pkgList += $pkgLiteralPath
    }

    $pkgList | ForEach-Object {
        $pkgPath = $_
        $pkgFileBaseName = Get-Item $pkgPath | select -expand BaseName
        $symbolPkgFileBaseName = $pkgFileBaseName + '.symbols'
        $symbolPkgPath = Join-Path (Split-Path $pkgPath -Parent) -ChildPath ($symbolPkgFileBaseName + '.nupkg')
        $pushSymbolPkg = $true

        say ($sr.PushingPackage -f $pkgFileBaseName)

        if (-not (Test-Path $symbolPkgPath -PathType Leaf))
        {
            say ($sr.SymbolsPackageNotFound) -v 0
            $pushSymbolPkg = $false
        }

        $publishServers | ForEach-Object {
            $publishServer = $_
            if (($publishServer.symbols -eq $true) -and ($pushSymbolPkg -eq $true))
            {
                say ($sr.PublishPackageWithSymbols -f $publishServer.url)

                if ($publishServer.symbolsUrl -and $publishServer.symbolsApiKey)
                {
                    exec { dotnet nuget push $pkgPath --timeout $publishServer.timeout --api-key $publishServer.apiKey --source $publishServer.url --symbol-source $publishServer.symbolsUrl --symbol-api-key $publishServer.symbolsApiKey }
                }
                elseif ($publishServer.symbolsUrl)
                {
                    exec { dotnet nuget push $pkgPath --timeout $publishServer.timeout --api-key $publishServer.apiKey --source $publishServer.url --symbol-source $publishServer.symbolsUrl }
                }
                elseif ($publishServer.symbolsApiKey)
                {
                    exec { dotnet nuget push $pkgPath --timeout $publishServer.timeout --api-key $publishServer.apiKey --source $publishServer.url --symbol-api-key $publishServer.symbolsApiKey }
                }
                else
                {
                    exec { dotnet nuget push $pkgPath --timeout $publishServer.timeout --api-key $publishServer.apiKey --source $publishServer.url }
                }
            }
            else
            {
                say ($sr.PublishPackageWithoutSymbols -f $publishServer.url)

                exec { dotnet nuget push $pkgPath --timeout $publishServer.timeout --api-key $publishServer.apiKey --source $publishServer.url --no-symbols }
            }
        }
    }
}

task Finish -depends Help, Configure, Build, Clean, Publish {
    $sr = $BuildEnv.BMLocalizedData

    say $sr.Goodbye
    say -NewLine -LineCount 5
}