const core = require('@actions/core');
const github = require('@actions/github');
const fs = require('fs').promises;

async function run() {
    
  try {

    core.info('Begin');

    // get inputs
    const token = core.getInput('token', { required: true });
    const name = core.getInput('name', { required: true });
    const path = core.getInput('path', { required: true });
    const version = core.getInput('version', { required: false });
    
    // get the REST api
    const octokit = github.getOctokit(token);
    const rest = octokit.rest;
    
    // get the context, the workflow, etc.
    const context = github.context;
    const workflow = context.workflow;
    const repository = context.payload.repository;
    
    // get the ref 
    const ref = getSha(context);
    if (!ref) {
      core.error(`Context: ${JSON.stringify(context, null, 2)}`);
      return process.exit(1);
    }
    
    // create an in-progress check run
    // TODO: could we have 1 run for both linux & windows?
    const created = await rest.checks.create({
        // TODO: ...context.repository syntax?
        owner: repository.owner.login,
        repo: repository.name,
        name: name,
        head_sha: ref,
        status: 'in_progress', // queued, in_progress, completed        
    });
    
    // gather values
    var failed = false;
    var summary = `${name}:`;
    
    try {
        const fpath = process.cwd() + '/' + path;
        const dirs = await fs.readdir(fpath, { withFileTypes: true});
        for (const dir of dirs) {
            if (!dir.isDirectory()) continue;
            const content = await fs.readFile(`${fpath}/${dir.name}/cover.json`, 'utf-8');
            const p = content.indexOf('{'); // trim weirdish leading chars
            const report = JSON.parse(content.substring(p));
            const target = dir.name.substr('cover-'.length);
            const percent = report.CoveragePercent;
            summary += `\n* ${target}: ${percent}%`;
        }
        
        var coverVersion = version;
        if (version.indexOf('-') > 0) {
            coverVersion = 'dev';
        }
        
        var coverUrl = `http://hazelcast.github.io/hazelcast-csharp-client/${coverVersion}/cover/index.html`;
        summary += '\n\nThe complete code coverage report has been uploaded as an artifact, ';
        summary += `and the latest report for this version is also [available online](${coverUrl}).`;
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
        owner: repository.owner.login,
        repo: repository.name,
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
   
    core.info('Completed.');
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
