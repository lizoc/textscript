function Invoke-Robocopy
{
    [CmdletBinding(DefaultParameterSetName = '__AllParameterSets')]
    Param(
        [Parameter(Mandatory = $true, Position = 1, ValueFromPipeline = $true)]
        [string]$SourcePath,

        [Parameter(Mandatory = $true, Position = 2)]
        [string]$DestinationPath,

        [Parameter(Mandatory = $true, ParameterSetName = 'MirrorSet')]
        [switch]$Mirror,

        [Parameter(Mandatory = $false)]
        [string[]]$ExcludeFolder,

        [Parameter(Mandatory = $false)]
        [string[]]$ExcludeFile,

        [Parameter(Mandatory = $false)]
        [switch]$Silent
    )

    # np  - no progress
    # ns  - don't file size
    # nfl - don't show file name
    # ndl - don't show dir name
    # njs - no job summary
    # njh - no job header

    # caveats: the current parsing implementation does not work with /njs
    $cpOptions = @('/NP', '/NS', '/NC', '/NFL', '/NDL', '/NJS')

    # mir - mirror
    if ($PSCmdlet.ParameterSetName -eq 'MirrorSet')
    {
        $cpOptions += '/MIR'
    }

    # /xd dirs [dirs] - exclude dirs matching given name/path
    if ($ExcludeFolder)
    {
        $cpOptions += '/XD ' + (($ExcludeFolder | ForEach-Object { '"{0}"' -f $_ }) -join ' ')
    }

    # /xf file [file] - exclude files matching given name/paths/wildcard
    if ($ExcludeFile)
    {
        $cpOptions += '/XF ' + (($ExcludeFile | ForEach-Object { '"{0}"' -f $_ }) -join ' ')
    }

    # run it!
    # https://stackoverflow.com/questions/18923315/using-in-powershell
    $sourceLiteralPath = Resolve-Path $SourcePath | select -expand Path
    $destLiteralPath = Resolve-Path $DestinationPath | select -expand Path

    # strangely, if you want to quote your paths, robocopy requires "double quote", not 'single quote'
    $copyArgs = ('"{0}" "{1}" ' -f $sourceLiteralPath, $destLiteralPath) + ($cpOptions -join ' ')
    Write-Verbose "Executing external program: robocopy $copyArgs"
    $copyResult = robocopy '--%' $copyArgs
    $copyResult = $copyResult | Out-String
    Write-Verbose "Execution result: $copyResult"

    # Silent -- just return as is
    if ($Silent) { return }


    # Attempt to parse output!

    $copyResultObj = @{}
    $parseNextLine = $false
    $parseNextField = ''
    $parseBreakPattern = ''

    $copyResultList = $copyResult.Split([Environment]::NewLine)
    for ($i = 0; $i -lt $copyResultList.Count; $i++)
    {
        if ($copyResultList[$i] -eq $null) { continue }
        if ($copyResultList[$i] -eq '') { continue }

        if ($parseNextLine -eq $true)
        {
            if ($copyResultList[$i] -clike $parseBreakPattern)
            {
                $parseNextLine = $false
                $parseNextField = ''
                $parseBreakPattern = ''
            }
            else
            {
                $copyResultObj."$parseNextField" += $copyResultList[$i].Trim()
                continue
            }
        }

        if ($copyResultList[$i] -like '*----*') { continue }
        if ($copyResultList[$i] -clike '*ROBOCOPY*') { continue }
        
        if ($copyResultList[$i].Trim() -clike 'Started : *') 
        { 
            $parseValue =  $copyResultList[$i].Substring($copyResultList[$i].IndexOf('Started : ') + 'Started : '.Length)
            $copyResultObj.StartTime = [datetime]::Parse($parseValue)
        }
        elseif ($copyResultList[$i].Trim() -clike 'Source : *') 
        { 
            $parseValue =  $copyResultList[$i].Substring($copyResultList[$i].IndexOf('Source : ') + 'Source : '.Length)
            $copyResultObj.Source = $parseValue
        }
        elseif ($copyResultList[$i].Trim() -clike 'Dest : *') 
        { 
            $parseValue =  $copyResultList[$i].Substring($copyResultList[$i].IndexOf('Dest : ') + 'Dest : '.Length)
            $copyResultObj.Destination = $parseValue
        }
        elseif ($copyResultList[$i].Trim() -clike 'Files : *') 
        { 
            $parseValue =  $copyResultList[$i].Substring($copyResultList[$i].IndexOf('Files : ') + 'Files : '.Length)
            $copyResultObj.Files = $parseValue
        }
        elseif ($copyResultList[$i].Trim() -clike 'Options : *') 
        { 
            $parseValue =  $copyResultList[$i].Substring($copyResultList[$i].IndexOf('Options : ') + 'Options : '.Length)
            $copyResultObj.Options = $parseValue
        }
        elseif ($copyResultList[$i].Trim() -clike 'Exc Files : *') 
        { 
            $parseValue =  $copyResultList[$i].Substring($copyResultList[$i].IndexOf('Exc Files : ') + 'Exc Files : '.Length)
            $copyResultObj.ExcludeFiles = @($parseValue)
            $parseNextLine = $true
            $parseNextField = 'ExcludeFiles'
            $parseBreakPattern = '* : *'
        }
        elseif ($copyResultList[$i].Trim() -clike 'Exc Dirs : *') 
        { 
            $parseValue =  $copyResultList[$i].Substring($copyResultList[$i].IndexOf('Exc Dirs : ') + 'Exc Dirs : '.Length)
            $copyResultObj.ExcludeFolders = @($parseValue)
            $parseNextLine = $true
            $parseNextField = 'ExcludeFolders'
            $parseBreakPattern = '* : *'
        }
    }

    [pscustomobject]$copyResultObj
}


# -----------
# Export
# -----------
Export-ModuleMember -Function @(
    'Invoke-Robocopy'
)
