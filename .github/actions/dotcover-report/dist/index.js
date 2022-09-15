/******/ (() => { // webpackBootstrap
/******/ 	var __webpack_modules__ = ({

/***/ 337:
/***/ ((module) => {

module.exports = eval("require")("@actions/core");


/***/ }),

/***/ 504:
/***/ ((module) => {

module.exports = eval("require")("@actions/github");


/***/ }),

/***/ 747:
/***/ ((module) => {

"use strict";
module.exports = require("fs");;

/***/ })

/******/ 	});
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __nccwpck_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		var cachedModule = __webpack_module_cache__[moduleId];
/******/ 		if (cachedModule !== undefined) {
/******/ 			return cachedModule.exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
/******/ 			// no module.id needed
/******/ 			// no module.loaded needed
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		var threw = true;
/******/ 		try {
/******/ 			__webpack_modules__[moduleId](module, module.exports, __nccwpck_require__);
/******/ 			threw = false;
/******/ 		} finally {
/******/ 			if(threw) delete __webpack_module_cache__[moduleId];
/******/ 		}
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/************************************************************************/
/******/ 	/* webpack/runtime/compat */
/******/ 	
/******/ 	if (typeof __nccwpck_require__ !== 'undefined') __nccwpck_require__.ab = __dirname + "/";/************************************************************************/
var __webpack_exports__ = {};
// This entry need to be wrapped in an IIFE because it need to be isolated against other modules in the chunk.
(() => {
const core = __nccwpck_require__(337);
const github = __nccwpck_require__(504);
const fs = __nccwpck_require__(747).promises;

async function run() {

  try {

    core.info('Begin');

    // get inputs
    const token = core.getInput('token', { required: true });
    const name = core.getInput('name', { required: true });
    const path = core.getInput('path', { required: true });
    const sha = core.getInput('sha', { required: true });
    const version = core.getInput('version', { required: false });

    // get the REST api
    const octokit = github.getOctokit(token);
    const rest = octokit.rest;

    // get context
    const context = github.context;
    //const repository = context.payload.repository;
    //const owner = repository.owner.login
    //const repo = repository.name
    const { owner, repo } = context.repo;

    // get the ref
    // TODO: ref vs sha ?!
    const ref = sha;
    //const ref = getSha(context);
    //if (!ref) {
    //  core.error(`Context: ${JSON.stringify(context, null, 2)}`);
    //  return process.exit(1);
    //}

    // create an in-progress check run
    const created = await rest.checks.create({
        owner: owner,
        repo: repo,
        name: name,
        head_sha: ref,
        status: 'in_progress', // queued, in_progress, completed
    });

    // gather values
    var failed = false;
    var summary = `${name}:`;

    async function readOsFiles(os) {
        const fpath = process.cwd() + '/' + path + '/' + os;
        const files = await fs.readdir(fpath, { withFileTypes: true });
        for (const file of files) {
            if (file.isDirectory() || !file.name.endsWith('.json')) continue;

            const target = file.name.substr('cover-'.length, file.name.length - 'cover-.json'.length);

            const content = await fs.readFile(`${fpath}/${file.name}`, 'utf-8');
            const p = content.indexOf('{'); // trim weirdish leading chars (BOM)
            const report = JSON.parse(content.substring(p));
            const percent = report.CoveragePercent;

            summary += `\n* ${os} / ${target}: ${percent}%`;
        }
    }

    try {
        await readOsFiles('windows');
        await readOsFiles('ubuntu');

        summary += '\n\nThe detailed code coverage report has been uploaded as an artifact.';

        // build-branch and report-pr pass 'sha', version remains ''
        // build-release passes 'sha' and 'version'

        if (version !== '') {
            var coverVersion = version;
            if (version.indexOf('-') > 0) { coverVersion = 'dev'; }

            var docUrl = `http://hazelcast.github.io/hazelcast-csharp-client/${coverVersion}/cover/index.html`;
            summary += `\n\nThe report for this version has been published as part of the [documentation](${docUrl}).`;
        }

        if (sha !== '') {
            var covUrl = `https://codecov.io/gh/hazelcast/hazelcast-csharp-client/commit/${sha}/`;
            summary += `\n\nThe report for this commit has been published on [codecov](${covUrl}).`;
        }
    }
    catch (error) {
        summary = `Failed: ${error.message}`;
        failed = true;
    }

    // create failure annotation in case we failed
    var annotations = [];
    if (failed) {
        annotations = [{
            path: ".github", // required - GitHub uses .github when unknown
            start_line: 1,   // required - GitHub uses 1 when unknown
            end_line: 1,     // same
            annotation_level: "failure", // notice, warning or failure
            title: `Error in ${name}`,
            message: `Error in ${name}`
            // raw_details (string)
        }];
    }

    // update the check run
    const r_update = await rest.checks.update({
        owner: owner,
        repo: repo,
        check_run_id: created.data.id,
        status: 'completed',
        conclusion: failed ? 'failure' : 'success', // success, failure, neutral, cancelled, timed_out, action_required, skipped
        output: {
            title: `Test Coverage`,
            summary: summary,
            //text: '...details...',
            annotations
        }
    });

    core.info(`\n\n${summary}\n\n`);

    core.info(`Completed (run ${created.data.id} ref ${ref})`);
  }
  catch (error) {
    core.setFailed(error.message);
  }
}

const getSha = (context) => {
  if (context.eventName === "pull_request") {
    return context.payload.pull_request.head.sha || context.payload.after;
  } else {
    return context.sha;
  }
};

run();

})();

module.exports = __webpack_exports__;
/******/ })()
;