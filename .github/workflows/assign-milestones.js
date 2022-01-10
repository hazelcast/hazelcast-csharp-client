// assign a milestone to issues or PRs added to a project, with a name corresponding
// to the name of the project - create the milestone if it does not exist
//
// adding a *second* project to the issue will update the milestone accordingly
// removing that project will reset the milestone entirely (not use the other project)
// and, no idea what happens if one very quickly adds and removes an issue
//
// an issue or PR actually becomes a card (or, a card is actually created...) when
// it's move to a project column for the first time - being assigned to the project
// and showing in project's backlog is not enough.

module.exports = async ({github, context, core}) => {
   
    const restapi = github.rest

    function last1(s) {
        const pos = s.lastIndexOf('/')
        return s.substring(pos + 1)
    }

    function last2(s) {
        const pos1 = s.lastIndexOf('/')
        const pos2 = s.lastIndexOf('/', pos1-1)
        return [ s.substring(pos2 + 1, pos1), s.substring(pos1 + 1) ]
    }

    function firstOrDefault(items, predicate) {
        for (const item of items) {
            if (predicate(item)) {
                return item
            }
        }
        return null
    }

    // insanely enough, due to the total lack of documentation, this
    // is the best way one can figure out what exactly we are getting
    console.log(github)
    console.log(context)
    console.log(context.payload)

    //console.log(`event: ${github.event_name}/${github.event.action}`)

    // get and validate the event name
    const eventName = context.eventName
    if (eventName != 'project_card') {
        return
    }

    // get and validate the event action
    const eventAction = context.payload.action
    if (eventAction != 'created' && eventAction != 'deleted') {
        return
    }

    console.log(`event: ${eventName}/${eventAction}`)

    const columnId = context.payload.project_card.column_id
    const columnResponse = await restapi.projects.getColumn({
        column_id: columnId
    })
    const column = columnResponse.data
    console.log(column)

    const projectId = last1(column.project_url)
    const projectResponse = await restapi.projects.get({
        project_id: projectId
    });
    const project = projectResponse.data
    console.log(project)
    const projectName = project.name

    // -- when a card is deleted it cannot be fetched!
    //const cardId = context.payload.project_card.id
    //const cardResponse = await restapi.projects.getCard({
    //    card_id: cardId
    //})
    //const card = cardResponse.data
    //console.log(card)

    //const [ itemType, itemNumber ] = last2(card.content_url)
    const [ itemType, itemNumber ] = last2(context.payload.project_card.content_url)
    const isIssue = itemType == 'issues'
    const isPull = itemType == 'pulls'
    if (!isIssue && !isPull) {
        core.setFailed(`Unsupported item type ${itemType}`)
        return
    }
    const itemApi = isIssue ? restapi.issues : restapi.pulls

    var request = {
        owner: context.repo.owner,
        repo: context.repo.repo
    }
    request[isIssue ? 'issue_number' : 'pull_number'] = itemNumber
    console.log(request)

    const itemResponse = await itemApi.get(request)
    const item = itemResponse.data
    console.log(item)
    const itemId = item.id
    const itemMilestone = item.milestone
    console.log(`item: ${itemType}/${itemId} milestone: ${itemMilestone == null ? '<none>' : itemMilestone}`)

    if (eventAction == 'created') {
        console.log('add or update milestone of issue/pull of created card')

        const milestonesResponse = await restapi.issues.listMilestones({
            owner: context.repo.owner,
            repo: context.repo.repo
        })
        const milestones = milestonesResponse.data
        console.log(milestones)
        var milestone = firstOrDefault(milestones, (x) => x.title == projectName)

        if (!milestone) {
            console.log(`create milestone '${projectName}'`)
            const milestoneResponse = await restapi.issues.createMilestone({
                owner: context.repo.owner,
                repo: context.repo.repo,
                title: projectName
            })
            milestone = milestoneResponse.data
        }

        const milestoneNumber = milestone.number

        if (!item.milestone || item.milestone.number != milestoneNumber)
        {
            request = {
                owner: context.repo.owner,
                repo: context.repo.repo,
                milestone: milestoneNumber
            }
            request[isIssue ? 'issue_number' : 'pull_number'] = itemNumber
            await itemApi.update(request)
        }
    }
    else if (eventAction == 'deleted') {
        if (itemMilestone) {
            console.log('remove milestone from issue/pull of deleted card')
            request = {
                owner: context.repo.owner,
                repo: context.repo.repo
            }
            request[isIssue ? 'issue_number' : 'pull_number'] = itemNumber
            request.milestone = null
            await itemApi.update(request)
        }
        else {
            console.log('deleted card had no milestone')
        }
    }
    else {
        console.log(`nothing to do for evet action ${eventAction}`)
    }
}

