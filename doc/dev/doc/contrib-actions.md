# GitHub Actions

The Hazelcast .NET Client relies on GitHub Actions for CI. Workflows live in YAML files in `.github/workflows` and are described below.

## Build Branch

This workflow is defined in `build-branch.yml` and triggers on every push to a branch (except `release/*` branches). It runs one single job with the following steps:
* Install some utilities (e.g. PowerShell...) latest versions, for Linux (bash script)
* Install .NET (`actions/setup-dotnet`)
* Install JDK (`actions/setup-java`)
* Checkout our code (`actions/checkout`)
* Build and test (`hz.ps1`)
* Report the test results to the workflow log (`dorny/test-reporter`)
* Report the test coverage to the workflow log (`dotcover-report`)
* Upload the test coverage reports as an artifact, (`actions/upload-artifact`)
* Publish the test coverage report to [Codecov.io](https://app.codecov.io/gh/hazelcast/hazelcast-csharp-client) (`codecov/codecov-action`)

## Build PR

This workflow is defined in `build-pr.yml` and triggers on every push to a PR. It runs one single job with the following steps:
* Install some utilities (e.g. PowerShell...) latest versions, for Linux (bash script)
* Install .NET (`actions/setup-dotnet`)
* Install JDK (`actions/setup-java`)
* Checkout our code (`actions/checkout`)
* Build and test (`hz.ps1`)
* Upload the test results as an artifact (`actions/upload-artifact`)
* Upload the test coverage reports as an artifact (`actions/upload-artifact`)

Note that this workflow does not *report* nor *publish* anything, as it executes against a user's PR and thus has no permission to write to our repository. Therefore, a second workflow, defined in `report-pr.yml`, triggers after every run of `build-pr.yml`, and runs one single job with the following steps:
* Prepare the environment (bash script)
* Download the test results and coverage reports artifacts (`actions/github-script` + custom script)
* Unzip the downloaded artifacts, (bash script)
* Report the test results to the workflow log (`dorny/test-reporter`)
* Report the test coverage to the workflow log (`dotcover-report`)
* Publish the test coverage report to [Codecov.io](https://app.codecov.io/gh/hazelcast/hazelcast-csharp-client) (`codecov/codecov-action`)

In addition, this worklow is registered as a required status check for all protected branches (`master`, `4.0.z`...). These branches require that both the *Build PR* for Linux and Windows checks have passed before merging any PR.

## Build Release

This workflow is defined in `build-release.yml` and triggers on every push to a `release/*` branch, and every push of a `v*` tag. It runs a combination of four jobs:

### Analyze

The *Analyze* job runs the following steps:
* Checkout our code (`actions/checkout`)
* Analyze the situation (bash script) and determine whether it has been triggered by a branch or a tag, and a few other things

### Build

The *Build* job runs if the *Analyze* job has validated the situation. It runs the following steps:
* Install some utilities (e.g. PowerShell...) latest versions, for Linux (bash script)
* Install .NET (`actions/setup-dotnet`)
* Install JDK (`actions/setup-java`)
* Checkout our code (`actions/checkout`)
* Verify that the branch or tag version matches the code version (`hz.ps1`)
* Obtains the assemblies signature key from GitHub Secrets (bash script)
* Build signed assemblies and test (`hz.ps1`)
* Report the test results to the workflow log (`dorny/test-reporter`)
* Report the test coverage to the workflow log (`dotcover-report`)
* Upload the test coverage reports as an artifact (`actions/upload-artifact`)
* Publish the test coverage report to [Codecov.io](https://app.codecov.io/gh/hazelcast/hazelcast-csharp-client) (`codecov/codecov-action`)
* Pack the NuGet packages (`hz.ps1`)
* Upload the NuGet packages as an artifact (`actions/upload-artifact`)
* Publish the examples (`hz.ps1`)
* Uploads the examples as an artifact (`actions/upload-artifact`)
* Creates a documentation patch (`hz.ps1` + git commands)
* Uploads the documentation patch as an artifact (`actions/upload-artifact`)

### Publish

The *Publish* job runs if the *Build* job was successful, and is in fact *two* jobs, that run different steps, depending on whether the workflow handles a branch build (preparing for a release) or a tag build (releasing). These steps are detailed below: actions in **bold** impact publicly visible resources such as NuGet or the documentation.

For *branch builds*, the job runs the following steps:
* Download the test coverage reports and doc patch artifacts (`actions/github-script` + custom script)
* Checkout the documentation (i.e. the `gh-pages` branch), apply the patch, and **push documentation back to GitHub** (`actions/checkout` + bash script)
* Publish the test coverage report to [Codecov.io](https://app.codecov.io/gh/hazelcast/hazelcast-csharp-client) (`codecov/codecov-action`)

As this is a branch build and not an official release, only the "dev" part of the documentation is updated.

For *tag builds*, the job runs the following steps:
* Checkout our code (`actions/checkout`)
* Checkout devops extensions for the `hz.ps1` script from the [DevOps](https://github.com/hazelcast/devops) private repository, into the `build/devops` directory (`actions/checkout`)
* Download the NuGet packages and doc patch artifacts (`actions/github-script` + custom script)
* **Upload the NuGet packages to NuGet** (`hz.ps1` script devops extensions, and API key provided by GitHub Secrets)
* Checkout the documentation (i.e. the `gh-pages` branch), apply the patch, and **push documentation back to GitHub** (`actions/checkout` + bash script)
* **Delete the release branch** (bash script)


## Notes

In order to ensure that the actions only run on our repository, and not on forks, each worfklow's job contains
```
if: github.repository == 'hazelcast/hazelcast-csharp-client'
```

For building and testing, our workflows use a strategy to ensure we test both on Linux and Windows:
```
strategy:
  matrix:
    os: [ ubuntu-latest, windows-latest ]
```

## Actions

### dotcover-report

The `dotcover-report` custom action lives in the `.github/actions/dotcover-report` directory. It is implemented as a Node script, and accepts the following inputs:
* `token` is the GitHub token
* `name` is the name of the action, re-used when creating the check run
* `path` is the path to the coverage reports
* `version` is the client version

The action scans the `path` for JSON coverage reports (one per target, e.g. `net462`, `netcoreapp3.1`...) and retrieves the global coverage percentage for each target. It then attaches a new check run to the commit SHA, containing these percentages, so that they become visible directly in GitHub.

The `.github/actions.txt` file contains more details about that action.