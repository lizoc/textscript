@echo off
rem Starts the build process from console.
rem Run from cmd.exe:
rem     build /?

rem no need to use forward slashes because we're in windows!

rem shifting will modify the batch file dir variable, so we need to assign first.
set SCRIPTDIR=%~dp0

rem help
rem build /?|/help|-h|--help [help_topic]
if '%1'=='/?' goto cmd_help
if '%1'=='/help' goto cmd_help
if '%1'=='-h' goto cmd_help
if '%1'=='--help' goto cmd_help

rem configure
rem build configure
if '%1'=='configure' goto cmd_configure

rem publish
rem build publish [path/to/file1.nupkg] [path/to/file2.nupkg] [...]
if '%1'=='publish' goto cmd_publish

rem clean
rem build clean debug|release [*|project1.name] [project2.name] [...]
if '%1'=='clean' goto cmd_clean

rem defaults to build
rem build debug|release [*|project1.name] [project2.name] [...]
goto cmd_build

rem ###############################################################################

:cmd_publish
rem build publish [path/to/file1.nupkg] [path/to/file2.nupkg] [...]
set SUBCOMMAND=%1
set CONFIGURATION=Release
shift
set PARAMSARG=%1
if "%PARAMSARG%"=="" goto publish_target_unspecified
shift
goto loop_params_arg

:publish_target_unspecified
rem build publish
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%SCRIPTDIR%\tools\Builder\Builder.ps1' '%SCRIPTDIR%\tools\DotNetBuilder\DotNetBuilder.ps1' -properties @{ Subcommand = 'Publish' }; if ($BuildEnv.BuildSuccess -eq $false) { exit 1 } else { exit 0 }"
exit /B %errorlevel%

:cmd_clean
rem build clean debug|release [*|project1.name] [project2.name] [...]
set SUBCOMMAND=%1
shift
set CONFIGURATION=%1
if "%CONFIGURATION%"=="" goto clean_default_configuration
shift
set PARAMSARG=%1
if "%PARAMSARG%"=="" goto clean_target_unspecified
shift
goto loop_params_arg

:clean_default_configuration
rem build clean
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%SCRIPTDIR%\tools\Builder\Builder.ps1' '%SCRIPTDIR%\tools\DotNetBuilder\DotNetBuilder.ps1' -properties @{ Subcommand = 'Clean' }; if ($BuildEnv.BuildSuccess -eq $false) { exit 1 } else { exit 0 }"
exit /B %errorlevel%

:clean_target_unspecified
rem build clean debug|release
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%SCRIPTDIR%\tools\Builder\Builder.ps1' '%SCRIPTDIR%\tools\DotNetBuilder\DotNetBuilder.ps1' -properties @{ Subcommand = 'Clean'; Configuration = '%CONFIGURATION%' }; if ($BuildEnv.BuildSuccess -eq $false) { exit 1 } else { exit 0 }"
exit /B %errorlevel%

:cmd_build
rem build debug|release [*|project1.name] [project2.name] [...]
set SUBCOMMAND=build
set CONFIGURATION=%1
if "%CONFIGURATION%"=="" goto build_default_configuration
shift
set PARAMSARG=%1
if "%PARAMSARG%"=="" goto build_target_unspecified
shift
goto loop_params_arg

:build_default_configuration
rem build
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%SCRIPTDIR%\tools\Builder\Builder.ps1' '%SCRIPTDIR%\tools\DotNetBuilder\DotNetBuilder.ps1'; if ($BuildEnv.BuildSuccess -eq $false) { exit 1 } else { exit 0 }"
exit /B %errorlevel%

:build_target_unspecified
rem build debug|release
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%SCRIPTDIR%\tools\Builder\Builder.ps1' '%SCRIPTDIR%\tools\DotNetBuilder\DotNetBuilder.ps1' -properties @{ Configuration = '%CONFIGURATION%' }; if ($BuildEnv.BuildSuccess -eq $false) { exit 1 } else { exit 0 }"
exit /B %errorlevel%

:cmd_configure
rem build configure
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%SCRIPTDIR%\tools\Builder\Builder.ps1' '%SCRIPTDIR%\tools\DotNetBuilder\DotNetBuilder.ps1' -properties @{ Subcommand = 'Configure' }; if ($BuildEnv.BuildSuccess -eq $false) { exit 1 } else { exit 0 }"
exit /B %errorlevel%

:cmd_help
rem build /?|/help|-h|--help [help_topic]
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%SCRIPTDIR%\tools\Builder\Builder.ps1' '%SCRIPTDIR%\tools\DotNetBuilder\DotNetBuilder.ps1' -properties @{ Subcommand = 'Help'; HelpTopic = '%2' }"
exit /B %errorlevel%

:loop_params_arg
if "%1"=="" goto loop_end
set PARAMSARG=%PARAMSARG% %1
shift
goto loop_params_arg

:loop_end
rem build clean debug|release *|project1.name [project2.name] [...]
rem build debug|release *|project1.name [project2.name] [...]
rem build publish [path/to/file1.nupkg] [path/to/file2.nupkg] [...]
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%SCRIPTDIR%\tools\Builder\Builder.ps1' '%SCRIPTDIR%\tools\DotNetBuilder\DotNetBuilder.ps1' -properties @{ Subcommand = '%SUBCOMMAND%'; Configuration = '%CONFIGURATION%'; BuildTarget = ('%PARAMSARG%'.Split(' ') | where { $_ -ne '' } | ForEach-Object { $_.Trim() }) }; if ($BuildEnv.BuildSuccess -eq $false) { exit 1 } else { exit 0 }"
exit /B %errorlevel%
