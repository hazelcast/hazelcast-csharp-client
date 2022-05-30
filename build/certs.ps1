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
# note:
# note all functions below are actually used at the moment, but we keep them for reference

# makes a new directory, clearing it if it already exists
function mkdirf ( $dir ) {
    if (test-path $dir) { rm -recurse -force $dir }
    mkdir $dir >$null 2>&1
}

# concatenate files
function concat ( $source_files, $dest_file ) {
    get-content -raw -asByteStream $source_files | set-content -asByteStream $dest_file
}

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

# initialize for a new certificate
function openssl-init-cert ( $cert_dir, $cert_name ) {
    Write-Output ""
    Write-Output "CERTS: initialize for certificate '$cert_name'"
    mkdirf "$cert_dir/$cert_name"
    mkdirf "$cert_dir/$cert_name/private"
}

# initializes for a new ca
function openssl-init-ca ( $cert_dir, $cert_name, $config_dir ) {
    Write-Output ""
    Write-Output "CERTS: initialize for CA '$cert_name'"
    mkdirf "$cert_dir/$cert_name"
    mkdirf "$cert_dir/$cert_name/certs"
    mkdirf "$cert_dir/$cert_name/db"
    mkdirf "$cert_dir/$cert_name/private"
    openssl rand -hex 16 > "$cert_dir/$cert_name/db/serial"
    set-content "$cert_dir/$cert_name/db/crlnumber" "1001" -noNewLine
    set-content "$cert_dir/$cert_name/db/index" "" -noNewLine
    cp "$config_dir/certs-$cert_name.conf" "$cert_dir/$cert_name/$cert_name.conf"
}

# requests a new certificate + its private key (from configuration, or minimal)
# REQ = PKCS#10 X.509 Certificate Signing Request (CSR) Management
# CSR = certificate signing request (contains the public key)
# -> <cert>.csr, <cert>.key
function openssl-req ( $cert_dir, $cert_name, $password, $config = $null, $days = -1 ) {
    $args = @(
        "req"
    )
    if (-not [string]::IsNullOrEmpty($config)) {
        $args += @(
            "-new",
            "-config", "$cert_dir/$config/$config.conf"
        )
        $details = " with configuration '$config'"
        $ext = "csr"
    }
    else {
        $args += @(
            "-x509", "-newkey", "rsa:4096", "-days", "$days",
            "-subj", "/C=US/O=Hazelcast/CN=$cert_name"
        )
        $details = " with validity $days days"
        $ext = "crt"
    }
    $args += @(
        "-out", "$cert_dir/$cert_name/$cert_name.$ext",
        "-keyout", "$cert_dir/$cert_name/private/$cert_name.key",
        "-passout", "pass:$password"
    )
    Write-Output ""
    Write-Output "CERTS: request certificate '$cert_name'$details"
    openssl $args
}

function keytool-genkey ( $cert_dir, $cert_name, $password, $days ) {
    Write-Output ""
    Write-Output "CERTS: genkey '$cert_name' validity $days days"
    keytool -genkey -keyalg RSA `
        -alias $cert_name `
        -validity $days `
        -dname "CN=foo.bar.com,OU=DRE,O=HZ,L=Foo,S=Bar,C=US" `
        -keystore "$cert_dir/$cert_name/$cert_name.keystore" `
        -keypass $password `
        -storepass $password
    keytool -export `
        -alias $cert_name `
        -keystore "$cert_dir/$cert_name/$cert_name.keystore" `
        -storepass $password `
        -file "$cert_dir/$cert_name/$cert_name.cer"
}

# self-signs a certificate
# CA = Certificate Authority (CA) Management
# -selfsign = self-sign the certificate
# -batch = no question asked
# -extensions = section of config file to be added when a certificate is issued
# CRT = cert as binary (or Base64)
# <cert>.csr -> <cert>.crt
function openssl-selfsign ( $cert_dir, $cert_name, $password, $config = $null, $ext = $null ) {
    $args = @(
        "ca", "-selfsign",
        "-passin", "pass:$password",
        "-in", "$cert_dir/$cert_name/$cert_name.csr",
        "-out", "$cert_dir/$cert_name/$cert_name.crt",
        "-batch"
    )
    if (-not [string]::IsNullOrEmpty($config)) {
        $args += @(
            "-config", "$cert_dir/$config/$config.conf",
            "-extensions", $ext
        )
        $details = " with configuration '$config`:$ext'"
    }
    else {
        $details = ""
    }
    Write-Output ""
    Write-Output "CERTS: self-sign certificate '$cert_name'$details"
    openssl $args
}

# signs a certificate
# <cert>.csr -> <cert>.crt
function openssl-sign ( $cert_dir, $cert_name, $password, $config, $ext ) {
    Write-Output ""
    Write-Output "CERTS: sign certificate '$cert_name' with configuration '$config`:$ext'"
    # https://www.openssl.org/docs/manmaster/man1/openssl-ca.html
    # no -name option = uses the default_ca option of the ca section of the configuration file
    openssl ca `
        -config "$cert_dir/$config/$config.conf" `
        -passin "pass:$password" `
        -in "$cert_dir/$cert_name/$cert_name.csr" `
        -out "$cert_dir/$cert_name/$cert_name.crt" `
        -batch `
        -extensions $ext
}

# generates a CRL for the certificate based on infos in the index file
# CRL = Certificate Revocation List
# <cert> -> <cert>.crl
function openssl-gencrl ( $cert_dir, $cert_name, $password, $config ) {
    Write-Output ""
    Write-Output "CERTS: generate certificate '$cert_name' revocation list with '$config'"
    openssl ca -gencrl `
        -config "$cert_dir/$config/$config.conf" `
        -passin "pass:$password" `
        -out "$cert_dir/$cert_name/$cert_name.crl"
}

# imports a PKCS #12 certificate store into a JKS store
function keytool-import-store ( $cert_dir, $dir, $cert_name, $cert_password, $alias, $store_name, $store_password ) {
    Write-Output "CERTS: import certificate store '$cert_name' (PKCS #12) into store '$store_name' (JKS)"
    keytool -importkeystore `
        -alias "$alias" `
        -srckeystore "$cert_dir/$dir/$cert_name.p12" `
        -srcstoretype pkcs12 `
        -srcstorepass $cert_password `
        -destkeystore "$cert_dir/$dir/$store_name.jks" `
        -deststoretype JKS `
        -deststorepass $store_password
}

# imports a certificate into a JKS store
function keytool-import-cert ( $cert_dir, $dir, $cert_name, $cert_password, $alias, $store_name, $store_password ) {
    Write-Output ""
    Write-Output "CERTS: import certificate '$cert_name' into store '$store_name' (JKS)"
    keytool -importcert -trustcacerts -noprompt -v `
        -alias "$alias" `
        -file "$cert_dir/$dir/$cert_name.pem" `
        -keypass $cert_password `
        -keystore "$cert_dir/$dir/$store_name.jks" `
        -storetype JKS `
        -storepass $store_password
}
function keytool-import-cert2 ( $cert_dir, $dir, $cert_name, $cert_password, $alias, $store_name, $store_password ) {
    Write-Output ""
    Write-Output "CERTS: import certificate '$cert_name' into store '$store_name' (JKS)"
    keytool -importcert -noprompt -v `
        -alias "$alias" `
        -file "$cert_dir/$dir/$cert_name.cer" `
        -keypass $cert_password `
        -keystore "$cert_dir/$dir/$store_name" `
        -storetype JKS `
        -storepass $store_password
}

function keytool-importcert ( $cert_dir, $cert_name, $store_dir, $store_name, $password ) {
    Write-Output ""
    Write-Output "CERTS: import certificate '$cert_name.cer' into store '$store_name' (JKS)"
    keytool -importcert -noprompt -v `
        -alias "cert_name" `
        -file "$cert_dir/$cert_name/$cert_name.cer" `
        -keystore "$cert_dir/$store_dir/$store_name" `
        -storetype JKS `
        -storepass $password -keypass $password
}

# imports a certificate into an existing keystore
# <cert>.crt -> <store>.jks
#function keytool-import-file ( $cert_dir, $dir, $cert_name, $cert_password, $store_name, $store_password ) {
#    keytool -import -noprompt -v `
#        -alias "$cert_name" `
#        -file "$cert_dir/$dir/$cert_name.crt" `
#        -keypass $cert_password `
#        -destkeystore "$cert_dir/$dir/$store_name.jks" `
#        -deststorepass $store_password
#    return $LASTEXITCODE
#}

# generates a PKCS #12 store for a certificate
# <cert>.crt, <cert>.key, [<chain>.pem] -> cert.p12
function openssl-pkcs12 ( $cert_dir, $dir, $cert_name, $password, $chain_name = $null ) {
    $args = @(
        "pkcs12", "-export",
        "-name", $cert_name,
        "-in", "$cert_dir/$dir/$cert_name.crt",
        "-inkey", "$cert_dir/$dir/$cert_name.key",
        "-passin", "pass:$password",
        "-out", "$cert_dir/$dir/$cert_name.p12",
        "-passout", "pass:$password"
    )
    if (-not [string]::IsNullOrEmpty($chain_name)) {
        $args += @(
            "-certfile",
            "$cert_dir/$dir/$chain_name.pem"
        )
        $details = " with chain '$chain_name'"
    }
    else {
        $details = ""
    }
    Write-Output ""
    Write-Output "CERTS: generates PKCS #12 store for certificate '$cert_name'$details"
    openssl $args
}

# encrypts a cert key using `PBE-SHA1-3DES` PKCS #8 v1.5 algorithm
# <cert>.key -> <cert>.enc.key
function openssl-pkcs8 ( $cert_dir, $dir, $cert_name, $password ) {
    Write-Output ""
    Write-Output "CERTS: encrypt certificate '$cert_name' key"
    openssl pkcs8 -topk8 -v1 PBE-SHA1-3DES `
        -in "$cert_dir/$dir/$cert_name.key" `
        -passin "pass:$password" `
        -out "$cert_dir/$dir/$cert_name.enc.key" `
        -passout "pass:$password"
}

# exports a certificate as a PFX file
# <cert>.crt, <cert>.key -> <cert>.pfx
function openssl-export-pfx ( $cert_dir, $cert_name, $password ) {
    Write-Output ""
    Write-Output "CERTS: export certificate '$cert_name' to PFX"
    openssl pkcs12 -export `
        -in "$cert_dir/$cert_name/$cert_name.crt" `
        -inkey "$cert_dir/$cert_name/private/$cert_name.key" `
        -passin "pass:$password" `
        -out "$cert_dir/$cert_name/$cert_name.pfx" `
        -passout "pass:$password"
}

function keytool-importkeystore-pfx ( $cert_dir, $cert_name, $cert_password ) {
    Write-Output ""
    Write-Output "CERTS: import '$cert_name.keystore' into '$cert_name.pfx'"
    keytool -importkeystore `
        -srcalias $cert_name `
        -srckeystore "$cert_dir/$cert_name/$cert_name.keystore" `
        -srcstorepass $password -srckeypass $password `
        -destalias $cert_name `
        -deststoretype PKCS12 `
        -destkeystore "$cert_dir/$cert_name/$cert_name.pfx" `
        -deststorepass $password -destkeypass $password
}

# ----------------------------------------------------------------
# GENERATE THE TEST CERTIFICATES
# ----------------------------------------------------------------

# see https://www.openssl.org/docs/manmaster/man1/openssl-ca.html
# see https://www.openssl.org/docs/manmaster/man5/config.html
#
# the root-ca and sub-ca are basically CA homes containing each CA's db etc
#
# each .conf file has a line like
# home = root-ca
# and then uses $home everywhere => need to fix home
# export environment variable from pwsh and do
# home = $ENV::CERT_DIR/root-ca

function gen-test-certs ( $cert_dir, $src_dir, $config_dir ) {

    Write-Output ""
    Write-Output "CERTS: -------- BEGIN --------"
    Write-Output ""
    Write-Output "CERTS: cert_dir   = $cert_dir"
    Write-Output "CERTS: src_dir    = $src_dir"
    Write-Output "CERTS: config_dir = $config_dir"

    $r = get-command openssl 2>&1
    Write-Output "CERTS: openssl    = $($r.Version) at $($r.Source)"
    $r = get-command keytool 2>&1
    Write-Output "CERTS: keytool    = $($r.Version) at $($r.Source)"

    $password = "123456"

    $env:CERT_DIR="$cert_dir" # used in the CA configuration files

    mkdirf $cert_dir

    Write-Output ""
    Write-Output "CERTS: -------- CREATE ROOT CA (SELF-SIGNED) --------"

    # root-ca:
    openssl-init-ca $cert_dir -cert_name "root-ca" -config_dir $config_dir

    # root-ca: -> root-ca.csr, root-ca.key
    openssl-req $cert_dir -cert_name "root-ca" -password $password -config "root-ca"
    if (failed) { return }

    # root-ca: root-ca.csr -> root-ca.crt
    openssl-selfsign $cert_dir -cert_name "root-ca" -password $password -config "root-ca" -ext "ca_ext"
    if (failed) { return }

    # root-ca: -> root-ca.crl
    openssl-gencrl $cert_dir -cert_name "root-ca" -password $password -config "root-ca"
    if (failed) { return }

    Write-Output ""
    Write-Output "CERTS: -------- CREATE SUB CA (SIGNED BY ROOT CA) --------"

    # sub-ca:
    openssl-init-ca $cert_dir -cert_name "sub-ca" -config_dir $config_dir

    # sub-ca: -> sub-ca.csr, sub-ca.key
    openssl-req $cert_dir -cert_name "sub-ca" -password $password -config "sub-ca"
    if (failed) { return }

    # sub-ca: sub-ca.csr -> sub-ca.crt
    openssl-sign $cert_dir -cert_name "sub-ca" -password $password -config "root-ca" -ext "sub_ca_ext"
    if (failed) { return }

    Write-Output ""
    Write-Output "CERTS: --------- CREATE CLUSTER1 CERT (SIGNED BY SUB CA) --------"

    # cluster1:
    openssl-init-cert $cert_dir -cert_name "cluster1"

    # cluster1: -> cluster1.crs, cluster1.key
    # beware! cluster1.hazelcast.meh MUST match the ca domain (see name_constraints in conf)
    # and the subjectAltName is required else we get RemoteCertificateNameMismatch errors
    openssl req -new -newkey rsa:2048 -days 3650 `
        -subj "/C=US/O=Hazelcast Test/CN=cluster1.hazelcast.meh" `
        -addext "subjectAltName = DNS:cluster1.hazelcast.meh" `
        -keyout "$cert_dir/cluster1/cluster1.key" -out "$cert_dir/cluster1/cluster1.csr" `
        -passout "pass:$password"
    if (failed) { return }

    # cluster1: cluster1.csr -> cluster1.crt
    openssl ca -batch `
        -config "$cert_dir/sub-ca/sub-ca.conf" `
        -out "$cert_dir/cluster1/cluster1.crt" `
        -passin "pass:$password" `
        -infiles "$cert_dir/cluster1/cluster1.csr"
    if (failed) { return }

    Write-Output ""
    Write-Output "CERTS: --------- CREATE KEYSTORE (CONTAINS CLUSTER1 PRIVATE + CHAIN AS KEYSTORE-CA TRUSTED) --------"

    # keystore:
    mkdirf "$cert_dir/keystore"
    concat "$cert_dir/sub-ca/sub-ca.crt", "$cert_dir/root-ca/root-ca.crt" "$cert_dir/keystore/chain.pem"

    openssl pkcs12 -export `
        -name "cluster1" `
        -inkey "$cert_dir/cluster1/cluster1.key" -in "$cert_dir/cluster1/cluster1.crt" `
        -passin "pass:$password" `
        -certfile "$cert_dir/keystore/chain.pem" `
        -out "$cert_dir/keystore/keystore.p12" `
        -passout "pass:$password"
    if (failed) { return }

    keytool -importkeystore `
        -srckeystore "$cert_dir/keystore/keystore.p12" -srcstoretype PKCS12 -srcstorepass $password `
        -destkeystore "$cert_dir/keystore/keystore.jks" -deststoretype JKS -deststorepass $password
    if (failed) { return }

    keytool -importcert -trustcacerts -noprompt `
        -alias "keystore-ca" `
        -file "$cert_dir/keystore/chain.pem" -keypass $password `
        -keystore "$cert_dir/keystore/keystore.jks" -storetype JKS -storepass $password
    if (failed) { return }

    # the keystore now contains 2 entries
    # - keystore-ca, trustedCertEntry
    # - member1, PrivateKeyEntry
    Write-Output ""
    Write-Output "CERTS: examine $cert_dir/keystore/keystore.jks"
    keytool -list -keystore "$cert_dir/keystore/keystore.jks" -storepass $password
    if (failed) { return }

    Write-Output ""
    Write-Output "CERTS: -------- CREATE CLIENT1 CERT --------"

    mkdirf "$cert_dir/client1"

    # client1: -> client1.keystore, client1.cer
    keytool-genkey $cert_dir -cert_name "client1" -password $password -days 3650
    if (failed) { return }

    # client1: client1.keystore -> client1.pfx
    keytool-importkeystore-pfx $cert_dir -cert_name "client1" -password $password
    if (failed) { return }

    Write-Output ""
    Write-Output "CERTS: -------- CREATE CLIENT2 CERT --------"

    mkdirf "$cert_dir/client2"

    # client2: -> client2.keystore, client2.cer
    keytool-genkey $cert_dir -cert_name "client2" -password $password -days 3650
    if (failed) { return }

    # client2: client2.keystore -> client2.pfx
    keytool-importkeystore-pfx $cert_dir -cert_name "client2" -password $password
    if (failed) { return }

    # copy both clients to the same place
    mkdirf "$cert_dir/clients"
    cp "$cert_dir/client1/client1.pfx" "$cert_dir/clients/client1.pfx"
    cp "$cert_dir/client2/client2.pfx" "$cert_dir/clients/client2.pfx"

    Write-Output ""
    Write-Output "CERTS: -------- CREATE SERVER1.TRUSTSTORE (CONTAINS CLIENT1 TRUSTED) --------"

    mkdirf "$cert_dir/server1"

    # server1: client1.cer -> server1.truststore
    keytool-importcert $cert_dir -cert_name "client1" -store_dir "server1" -store_name "server1.truststore" -password $password
    if (failed) { return }

    # the server1.truststore now contains 1 entry
    # - client1, trustedCertEntry
    Write-Output ""
    Write-Output "CERTS: examine $cert_dir/server1/server1.truststore"
    keytool -list -keystore "$cert_dir/server1/server1.truststore" -storepass $password
    if (failed) { return }

    Write-Output ""
    Write-Output "CERTS: -------- CREATE SERVER1.KEYSTORE (CONTAINS SERVER1 PRIVATE) --------"

    # server1: -> server1.keystore, server1.cer
    keytool-genkey $cert_dir -cert_name "server1" -password $password -days 3650
    if (failed) { return }

    # the server1.keystore now contains 1 entry
    # - server1, PrivateKeyEntry
    Write-Output ""
    Write-Output "CERTS: examine $cert_dir/server1/server1.keystore"
    keytool -list -keystore "$cert_dir/server1/server1.keystore" -storepass $password
    if (failed) { return }

    # not anymore - directly use the files from the temp directory - keep this for reference
    #Write-Output ""
    #Write-Output "CERTS: -------- COPY FILES --------"
    #
    #$res_dir = "$src_dir/Hazelcast.Net.Tests/Resources/Certificates/"
    #cp "$cert_dir/clients/client1.pfx" "$res_dir/client1.pfx"
    #cp "$cert_dir/clients/client2.pfx" "$res_dir/client2.pfx"
    #cp "$cert_dir/keystore/keystore.jks" "$res_dir/keystore.jks"
    #cp "$cert_dir/server1/server1.keystore" "$res_dir/server1.keystore"
    #cp "$cert_dir/server1/server1.truststore" "$res_dir/server1.truststore"

    Write-Output ""
    Write-Output "CERTS: -------- DONE --------"
    Write-Output ""

    $global:CERTSEXITCODE = 0
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