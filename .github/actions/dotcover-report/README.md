# DotCover Report

This action reports test coverage results to GitHub Action.

## Inputs

### `token`

**Required** The GitHub token.

### `name`

**Required** The name of the check run, e.g. "Test Coverage (windows-latest)".

### `path`

**Required** The path to the directory where the test coverage results have been produced.

### `version`

The client version e.g. `1.2.3` or `1.2.3-preview.44`.

## Outputs

(none)

## Example usage

This is how we use it in our own `build-release.yml` workflow:

```
uses: ./.github/actions/dotcover-report
with:
	name: Test Coverage (${{ matrix.os }})
	path: temp/tests/cover
	version: ${{ needs.analyze.outputs.version }}
	sha: ${{ github.sha }}
```