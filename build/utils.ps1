# utilities for hz.ps1

# die - PowerShell display of errors is a pain
function Die ( $message ) {
    [Console]::Error.WriteLine()
    [Console]::ForegroundColor = 'red'
    [Console]::Error.WriteLine($message)
    [Console]::ResetColor()
    [Console]::Error.WriteLine()
    Exit 1
}

# this is for troubleshooting
function Write-Object ( $indent, $object ) {

    if ($object -eq $null) {
        Write-Output "$indent`$null"
    }
    elseif ($object -is [array]) {
        Write-Output "$indent$($object.GetType().Name) (Count=$($object.Count)):"
        $object | foreach-object {
            Write-Object "$indent  " $_
        }
    }
    elseif ($object -is [Collections.HashTable]) {
        Write-Output "$indent$($object.GetType().Name) (Count=$($object.Count)):"
        $object.Keys | foreach-object {
            $key = $_
            $value = $object[$_]
            Write-Output "$indent  [$key]"
            Write-Object "$indent    " $value
        }
    }
    else {
        Write-Output "$indent$($object.GetType().Name): $object"
    }
}

function PowerShell-IsAtLeast ( $version ) {
    return ($script:powershellVersion -ge [System.Version]::Parse($version))
}

function Validate-Platform {
    # determine PowerShellVersion (see also $psVersionTable)
    # PowerShell 2.0 is integrated since Windows 7 and Server 2008 R2
    #            3.0                             8            2012
    #            4.0                             8.1          2012 R2
    #            5.0                             10
    #            5.1                             10AU         2016
    # and then, we have PowerShell 6.0+ aka 'Core' which is not integrated
    # and there are some annoying differences, so we are requiring 6.2+
    $psVersion=(get-host | select-object Version).Version
    $minVersion=[System.Version]::Parse("6.2.0")
    if (-not (Powershell-IsAtLeast($minVersion.ToString()))) {
        Write-Output ""
        Write-Output "This script requires at least version $($minVersion.Major).$($minVersion.Minor) of PowerShell, but you seem to be running version $($psVersion.Major).$($psVersion.Minor)."
        Write-Output "We recommend you install the most recent stable version available for download at:"
        Write-Output "https://github.com/PowerShell/PowerShell/releases"
        Write-Output "Please note that this version will need to be invoked with 'pwsh' not 'powershell'."
        Die "Unsupported PowerShell version: $($psVersion.Major).$($psVersion.Minor)"
    }
    $script:powershellVersion = $psVersion

    # determine platform
    $platform = "windows"

    # use built-in flags
    if ($isLinux) { $platform = "linux" }
    if ($isWindows) { $platform = "windows" }
    if ($isMacOS) { $platform = "macOS" }

    $script:platform = $platform
}

# write usage
function Write-Usage ( $params, $actions ) {
    Write-Output ""
    Write-Output "usage hz.[ps1|sh] [<options>] [<commands>] [<commargs>] [--- <rawargs>]"
    Write-Output ""
    Write-Output "  <options>    arguments for commands (see available options below)."
    Write-Output "  <commands>   CSV list of commands (see available commands below) to be executed by the script."
    Write-Output "               Note that not all commands can be combined together."
    Write-Output "  <commargs>   additional arguments for commands that support them."
    Write-Output "  <rawargs>    arguments that are not interpreted by the script, but passed raw to commands that"
    Write-Output "               support them, such as 'run-example'."
    Write-Output ""
    Write-Output "  Commands:"
    Write-Output ""

    $actions | `
        foreach-object {
            $action = $_
            if (-not $action.internal) {
                $name = "    $($action.name)"
                $infos = $action.desc
                if ($action.alias -ne $null) {
                    $alias = [string]::Join(", ", ($action.alias.Replace(" ", "").Split(',')))
                    $infos = "$infos (alias: $alias)"
                }
                if ($action.note -ne $null) {
                    $infos = "$infos`n$($action.note)"
                }
                @{ name = $name; infos = $infos }
            }
        } |`
        foreach-object { new-object PSObject -property $_ } | `
        format-table -autosize -property name,infos -hideTableHeaders -wrap

    Write-Output ""
    Write-Output "  Options:"

    $params | `
        foreach-object {
            $param = $_
            $name = "    -$($param.name)"
            if ($param.parm -ne $null) {
                $name = "$name $($param.parm)"
            }
            $infos = $param.desc
            if ($param.alias -ne $null) {
                $alias = [string]::Join(", ", ($param.alias.Replace(" ", "").Split(',') | foreach-object { "-$_" }))
                $infos = "$infos (alias: $alias)"
            }
            if ($param.note -ne $null) {
                $infos = "$infos`n$($param.note)"
            }
            @{ name = $name; infos = $infos }
        } | `
        foreach-object { new-object PSObject -property $_ } | `
        format-table -autosize -property name,infos -hideTableHeaders -wrap
}

function Get-Action ( $actions, $name ) {
    return $actions | where-object { $_.name -eq $name } | select-object -first 1
}

# parse commangs
function Parse-Commands ( $commands, $actions ) {

    # default?
    if ($commands.Count -eq 0) {
        $actions[0].run = $true
        return
    }

    $uniq = $null
    $count = 0
    $err = $null

    $actions | foreach-object { $_.run = $false }

    $actionx = @{}
    $actions | foreach-object {
        $a = $_
        $actionx[$a.name] = $a
        if ($a.alias -is [string]) {
          $a.alias.Split(',', [StringSplitOptions]::RemoveEmptyEntries) | foreach-object {
            $actionx[$_.Trim()] = $a
          }
        }
    }

    # else handle Commands
    $commands | foreach-object {

        if ($err -is [string]) { return }

        $action = $actionx[$_]
        if ($action -eq $null) {
            $err = "unknown command `'$_`'"
            return
        }

        if ($action.uniq) {

            if ($count -ne 0) {

                $err = "Command '$($action.name)' cannot be mixed with other commands."
                return
            }
            $uniq = $action.name
        }
        elseif ($uniq -ne $null) {

            $err = "Command '$uniq' cannot be mixed with other commands."
            return
        }

        $action.run = $true
        $count += 1
    }

    return $err
}

# parse arguments: (args, params) -> options
# because pwsh args are bonkers
function Parse-Args ( $argx, $params ) {

  # create default options
  $options = @{}
  $params | foreach-object {
    $options[$_.name] = $_.default
  }

  # add default commands and commargs
  $options.commands = [string[]] @()
  $options.commargs = [object[]] @()

  # create params hashtable
  $paramx = @{}
  $params | foreach-object {
    $p = $_
    $paramx["-$($p.name)"] = $p
    if ($p.alias -is [string]) {
      $p.alias.Split(',', [StringSplitOptions]::RemoveEmptyEntries) | foreach-object {
        $paramx["-$($_.Trim())"] = $p
      }
    }
  }

  $param = $null
  $canBeCommand = $true
  $justRaw = $false
  $comma = $true

  # handle arguments
  $argx | foreach-object {

    # if $options is an error string, skip all
    if ($options -is [string]) { return }

    $arg = $_

    # value of a valid param
    if ($param -ne $null) {
      if (-not ($arg -is $param.type)) {
        $narg = $arg -as $param.type
        if ($narg -eq $null) {
          $options = "invalid value `'$arg`' of type $($arg.GetType().Name) for parameter $($param.name) of type $($param.type.Name)"
          return # continue foreach-object
        }
        $arg = $narg
      }
      $options[$param.name] = $arg
      $param = $null
      return # continue foreach-object
    }

    # enter raw block?
    # when invoking ./hz.ps1 from pwsh, use --- to isolate commargs else we think they are hz.ps1 params
    # when invoking ./hs.sh from bash, use --- for the same reason, and it's converted to --% (pwsh's own thing)
    if ($arg -eq "--%" -or $arg -eq "---") {
      $justRaw = $true
      $canBeCommand = $false
      return # continue foreach-object
    }

    # -xxx in non-raw block, should be a valid parameter
    if (-not $justRaw -and $arg -is [string] -and $arg.StartsWith('-')) {
      $param = $paramx[$arg]
      if ($param -eq $null) {
        $options = "unknown parameter `'$arg`'"
        return # continue foreach-object
      }
      if ($param.handled) {
        $options = "parameter `'$($param.name)`' cannot be specified multiple times"
        return # continue foreach-object
      }
      $param.handled = $true
      if ($param.type -eq [switch]) {
        $options[$param.name] = $true
        $param = $null
      }
      return # continue foreach-object
    }

    # array can be commands
    if ($arg -is [array]) {
      if ($canBeCommand -and $options.commands.Count -eq 0) {
        $arg | foreach-object {

          # if $options is an error string, skip all
          if ($options -is [string]) { return }

          if ($_ -is [string] -and $_ -match "^[a-z][a-z0-9-]*$") {
            $options.commands += $_
          }
          else {
            $options = "invalid command name `'$_`'"
            return # continue foreach-object
          }
        }
        $comma = $false
        return # continue foreach-object
      }
    }

    # finaly, simple string can be more commands, or commargs
    if ($canBeCommand -and $arg -is [string] -and ($comma -or $arg.StartsWith(','))) {
      $arg.Split(',', [StringSplitOptions]::RemoveEmptyEntries) | foreach-object {

        # if $options is an error string, skip all
        if ($options -is [string]) { return }

        $trimmed = $_.Trim()
        if ($trimmed -ne "" -and $trimmed -match "^[a-z][a-z0-9-]*$") {
          $options.commands += $trimmed
        }
        else {
          $options = "invalid command name `'$trimmed`'"
          return # continue foreach-object
        }
      }
      $comma = $_.EndsWith(',')
      return # continue foreach-object
    }

    # just commargs
    # will not rebuild arrays there as pwsh would do
    $options.commargs += $arg
    $canBeCommand = $false
  }

  return $options
}

# clear source file
function clrSrc($file) {
    $txt = get-content $file -raw
    $txt = $txt.Replace("`r`n", "`n"); # crlf to lf
    $txt = $txt.Replace("`r", "`n"); # including single cr
    $txt = [System.Text.RegularExpressions.Regex]::Replace($txt, "[ `t]+$", "", [System.Text.RegularExpressions.RegexOptions]::Multiline); # trailing spaces
    $txt = $txt.TrimEnd("`n") + "`n"; # end file with one single lf
    $txt = $txt.Replace("`n", [System.Environment]::NewLine); # lf to actual system newline (as expected by Git)
    set-content $file $txt -noNewLine
}

function Get-TopologicalSort {
  param(
      [Parameter(Mandatory = $true, Position = 0)]
      [hashtable] $edgeList
  )

  # Make sure we can use HashSet
  Add-Type -AssemblyName System.Core

  # Clone it so as to not alter original
  #$currentEdgeList = [hashtable] (Get-ClonedObject $edgeList)
  $currentEdgeList = $edgeList

  # algorithm from http://en.wikipedia.org/wiki/Topological_sorting#Algorithms
  $topologicallySortedElements = New-Object System.Collections.ArrayList
  $setOfAllNodesWithNoIncomingEdges = New-Object System.Collections.Queue

  $fasterEdgeList = @{}

  # Keep track of all nodes in case they put it in as an edge destination but not source
  $allNodes = New-Object -TypeName System.Collections.Generic.HashSet[object] -ArgumentList (,[object[]] $currentEdgeList.Keys)

  foreach($currentNode in $currentEdgeList.Keys) {
      $currentDestinationNodes = [array] $currentEdgeList[$currentNode]
      if($currentDestinationNodes.Length -eq 0) {
          $setOfAllNodesWithNoIncomingEdges.Enqueue($currentNode)
      }

      foreach($currentDestinationNode in $currentDestinationNodes) {
          if(!$allNodes.Contains($currentDestinationNode)) {
              [void] $allNodes.Add($currentDestinationNode)
          }
      }

      # Take this time to convert them to a HashSet for faster operation
      $currentDestinationNodes = New-Object -TypeName System.Collections.Generic.HashSet[object] -ArgumentList (,[object[]] $currentDestinationNodes )
      [void] $fasterEdgeList.Add($currentNode, $currentDestinationNodes)
  }

  # Now let's reconcile by adding empty dependencies for source nodes they didn't tell us about
  foreach($currentNode in $allNodes) {
      if(!$currentEdgeList.ContainsKey($currentNode)) {
          [void] $currentEdgeList.Add($currentNode, (New-Object -TypeName System.Collections.Generic.HashSet[object]))
          $setOfAllNodesWithNoIncomingEdges.Enqueue($currentNode)
      }
  }

  $currentEdgeList = $fasterEdgeList

  while($setOfAllNodesWithNoIncomingEdges.Count -gt 0) {
      $currentNode = $setOfAllNodesWithNoIncomingEdges.Dequeue()
      [void] $currentEdgeList.Remove($currentNode)
      [void] $topologicallySortedElements.Add($currentNode)

      foreach($currentEdgeSourceNode in $currentEdgeList.Keys) {
          $currentNodeDestinations = $currentEdgeList[$currentEdgeSourceNode]
          if($currentNodeDestinations.Contains($currentNode)) {
              [void] $currentNodeDestinations.Remove($currentNode)

              if($currentNodeDestinations.Count -eq 0) {
                  [void] $setOfAllNodesWithNoIncomingEdges.Enqueue($currentEdgeSourceNode)
              }
          }
      }
  }

  if($currentEdgeList.Count -gt 0) {
      throw "Graph has at least one cycle!"
  }

  return $topologicallySortedElements
}

function Get-HazelcastRemote () {
    $remote = git remote -v | select-string 'https://github.com/hazelcast/hazelcast-csharp-client[\. ]' | select -first 1
    if ($remote -eq $null) { return $null }
    $remote = $remote.ToString().Split()[0]
    return $remote
}

function invoke-web-request($url, $dest) {
    $args = @{ Uri = $url }
    if (![System.String]::IsNullOrWhiteSpace($dest)) {
        $args.OutFile = $dest
        $args.PassThru = $true
    }

    $pp = $progressPreference
    $progressPreference = 'SilentlyContinue'

    if (PowerShell-IsAtLeast("7.0.0")) {
        $args.SkipHttpErrorCheck = $true
    }

    # on repository.hazelcast.com the default user-agent (mozilla) returns html content
    $args.UserAgent = "hz"

    try {
        $r = invoke-webRequest @args
        if ($null -ne $r) {
            if ($r.StatusCode -ne 200) {
                Write-Output "--> $($r.StatusCode) $($r.StatusDescription)"
            }
            return $r
        }
        return @{ StatusCode = 999; StatusDescription = "Error" }
    }
    catch [System.Net.WebException] {
        return @{ StatusCode = 999; StatusDescription = "Error" }
    }
    finally {
        $progressPreference = $pp
    }
}

function get-commarg ( $name ) {
    $commarg = $null
    $options.commargs | foreach-object {
        if ($_.StartsWith("$name=")) {
            $commarg = $_.SubString("$name=".Length).Trim("`"").Trim("`'").Trim()
        }
    }
    return $commarg
}

function get-project-name ( $path ) {
    $p = $path.LastIndexOf('\')
    $n = $path.SubString($p+1)
    $n = $n.SubString(0, $n.Length - '.csproj'.Length)
    return $n;
}

function read-file ( $filename ) {
    $text = get-content -raw -path $filename
    $text = $text.Replace("`r", "").TrimEnd("`n")
    return $text
}

function write-file ( $filename, $text ) {
    $text = $text.Replace("`n", [Environment]::Newline)
    set-content -noNewLine -encoding utf8 $filename $text
}

function test-command
{
    param ($command)

    $oldPreference = $ErrorActionPreference
    $ErrorActionPreference = "stop"

    try { if (Get-Command $command) { $true } }
    catch { $false }
    finally { $ErrorActionPreference=$oldPreference }
}