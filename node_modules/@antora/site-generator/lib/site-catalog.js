'use strict'

const $files = Symbol('files')

class SiteCatalog {
  constructor () {
    this[$files] = []
  }

  addFile (file) {
    this[$files].push(file)
  }

  addFiles (files) {
    this[$files].push(...files)
  }

  getFiles () {
    return this[$files].slice()
  }
}

/**
 * @deprecated superceded by getFiles(); scheduled to be removed in Antora 4
 */
SiteCatalog.prototype.getAll = SiteCatalog.prototype.getFiles

module.exports = SiteCatalog
