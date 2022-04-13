module.exports = /*async*/ ({github, context, core}) => {
   
    const restapi = github.rest
    var dryrun = context.payload.inputs.dryrun

    function firstOrDefault(items, predicate) {
        for (const item of items) {
            if (predicate(item)) {
                return item
            }
        }
        return null
    }

    async function validateRelease(version) {

        const tag = "v" + version
        console.log(`Validate version '${version}'.`)

        // git branch must exist
        const branchRefs = await restapi.git.listMatchingRefs({
            owner: context.repo.owner,
            repo: context.repo.repo,
            ref: `heads/release/${version}`
        })
        if (firstOrDefault(branchRefs.data, (x) => x.ref == `refs/heads/release/${version}`) == null) {
            core.setFailed(`Could not find branch 'release/${version}'.`)
            return    
        }
        console.log(`Found branch 'release/${version}'.`)

        // github milestone must exist and be open
        const milestone = await getMilestone(version)
        if (milestone == null) {
            core.setFailed(`Could not find milestone '${version}'.`)
            return
        }
        if (milestone.state != "open") {
            core.setFailed(`Milestone '${version}' is already closed.`)
            return
        }
        console.log(`Found open milestone '${version}'.`)

        // github release must exist and not be published yet
        const release = await getRelease(version)
        if (release === null) {
            core.setFailed(`Could not find a GitHub release for tag '${tag}'.`)
            return
        }
        if (!release.draft) {
            core.setFailed(`GitHub release for tag '${tag}' is already published.`)
            return
        }
        console.log(`Found yet-unpublished GitHub release for tag '${tag}'.`)

        // tag must not exist
        const tagRefs = await restapi.git.listMatchingRefs({
            owner: context.repo.owner,
            repo: context.repo.repo,
            ref: `tags/${tag}`
        })
        if (firstOrDefault(tagRefs.data, (x) => x.ref == `tags/${tag}`) != null) {
            core.setFailed(`Tag '${tag}' already exists.`)
            return
        }
        console.log(`Verified that tag '${tag}' does not exist yet.`)

        console.log('Release is valid.')
    }   

    async function publishRelease(version) {

        const tag = "v" + version
        console.log(`Publish GitHub release '${version}'.`)

        const release = await getRelease(version)

        await restapi.repos.updateRelease({
            owner: context.repo.owner,
            repo: context.repo.repo,
            release_id: release.id,
            draft: false
        })
    }

    async function closeMilestone(version) {

        const tag = "v" + version
        console.log(`Close milestone '${version}'.`)

        const milestone = await getMilestone(version)
        await restapi.issues.updateMilestone({
            owner: context.repo.owner,
            repo: context.repo.repo,
            milestone_number: milestone.number,
            state: "closed"
        })
    }

    async function getRelease(version) {

        // note: getReleaseByTag only returns published releases
        
        // note: we may eventually need to paginate
        // and then releases.headers should contain what we need?
        // https://octokit.github.io/rest.js/v18#pagination

        const tag = `v${version}`
        const releases = await restapi.repos.listReleases({
            owner: context.repo.owner,
            repo: context.repo.repo
        })
        const release = firstOrDefault(releases.data, (x) => x.tag_name == tag)
        return release
    }

    async function getMilestone(version) {

        const milestonesResponse = await restapi.issues.listMilestones({
            owner: context.repo.owner,
            repo: context.repo.repo
        })
        const milestones = milestonesResponse.data
        return firstOrDefault(milestones, (x) => x.title == version)
    }

    return {
        validateRelease: validateRelease,
        publishRelease: publishRelease,
        closeMilestone: closeMilestone
    }
}