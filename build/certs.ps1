if ($CERTSLIB) { return }

# GENERATE CERTIFICATES FOR SSL TESTS
#
# sources:
# hazelcast-enterprise repository
# - ./hazelcast-enterprise/src/test/resources/com/hazelcast/nio/ssl/create_demo_ca_key_material.sh
# - ./hazelcast-enterprise/src/test/resources/com/hazelcast/nio/ssl/README.md
# hazelcast-python-client repository
# - ./tests/integration/backward_compatible/ssl_tests/README.md
#
# note: all functions below are actually used at the moment, but we keep them for reference
# note: most of the script has been transfered to https://github.com/hazelcast/private-test-artifacts

# test for failure
# because... pwsh cannot both write to output and return a value
function failed () {
    if ($LASTEXITCODE) {
        $global:CERTSEXITCODE = 1
        return $true
    }
    else {
        $global:CERTSEXITCODE = 0
        return $false
    }
}

# ----------------------------------------------------------------
# INSTALL OR REMOVE THE TEST ROOT CERTIFICATE
# ----------------------------------------------------------------

# (Windows only)
# installs or removes a certificate from the local machine root
# without prompting the user (install-certificate *does* prompt)
# must be Administrator - but Current User *does* prompt
# root cert: ./temp/certs/root-ca/root-ca.crt
function install-root-ca-windows ( $filename, $add = $true ) {
    Write-Output ""
    Write-Output "CERTS: -------- INSTALL/REMOVE ROOT-CA --------"

    try {
        $cert = new-object System.Security.Cryptography.X509Certificates.X509Certificate2($filename)
    }
    catch {
        write-output $_
        $global:CERTSEXITCODE = 1
        return
    }

    $store = get-item Cert:\LocalMachine\Root

    try {
        $store.Open("ReadWrite")
    }
    catch {
        write-output "Failed on open Cert:\LocalMachine\Root - this script must run as Administrator"
        write-output $_
        $global:CERTSEXITCODE = 1
        return
    }

    try {
        $existing = ($store.Certificates | where { $_.Subject -eq $cert.Subject -and $_.Thumbprint -ne $cert.Thumbprint })

        if ($existing.Count -ne 0) {
            write-output "CERTS: found one or more rogue root-ca cert already in Cert:\LocalMachine\Root, removing"
            $existing | foreach-object { 
                $store.Remove($_) 
                write-output "CERTS: removed cert '$($_.Subject)' ($($_.Thumbprint)) from Cert:\LocalMachine\Root"
            }
        }

        $existing = ($store.Certificates | where { $_.Subject -eq $cert.Subject })

        if ($add) {
            if ($existing.Count -eq 0) {
                $store.Add($cert)
                write-output "CERTS: added cert '$($cert.Subject)' ($($cert.Thumbprint)) to Cert:\LocalMachine\Root"
            }
            else {
                write-output "CERTS: found cert '$($cert.Subject)' ($($cert.Thumbprint)) already in Cert:\LocalMachine\Root"
            }
        }
        else {
            if ($existing.Count -eq 1) {
                $store.Remove($cert)
                write-output "CERTS: removed cert '$($cert.Subject)' ($($cert.Thumbprint)) from Cert:\LocalMachine\Root"
            }
            else {
                write-output "CERTS: cert '$($cert.Subject)' ($($cert.Thumbprint)) not in Cert:\LocalMachine\Root"
            }
        }
    }
    catch {
        write-output $_
        $global:CERTSEXITCODE = 1
        return
    }

    $store.Close()
    $global:CERTSEXITCODE = 0
}

# (Linux only)
# same, but for Linux
function install-root-ca-linux ( $filename, $add = $true ) {
    try {
        $fileonly = [IO.Path]::GetFileName($filename)
        if ($add) {
            if (-not (test-path "/usr/local/share/ca-certificates/$fileonly")) {
                sudo cp $filename /usr/local/share/ca-certificates/
                sudo update-ca-certificates
                write-output "CERTS: added cert '$fileonly' to /usr/local/share/ca-certificates"
            }
            else {
                write-output "CERTS: found cert '$fileonly' already in /usr/local/share/ca-certificates"
            }
        }
        else {
            if (test-path "/usr/local/share/ca-certificates/$fileonly") {
                sudo rm /usr/local/share/ca-certificates/$fileonly
                sudo update-ca-certificates --fresh
                write-output "CERTS: removed cert '$fileonly' from /usr/local/share/ca-certificates"
            }
            else {
                write-output "CERTS: cert '$fileonly' not in /usr/local/share/ca-certificates"
            }
        }
        $global:CERTSEXITCODE = 0
    }
    catch {
        Write-Output "Failed: Linux support is experimental, must be root, ubuntu or debian."
        Write-Output $_
        $global:CERTSEXITCODE = 1
    }
}

function install-root-ca ( $filename, $add = $true ) {
    if ($isWindows) {
        install-root-ca-windows $filename $add
    }
    elseif ($isLinux) {
        install-root-ca-linux $filename $add
    }
    else {
        Write-Output "Unsupported platform."
        $global:CERTSEXITCODE = 1
    }
}

function remove-root-ca ( $filename ) {
    install-root-ca $filename $false
}

$CERTSLIB=$true