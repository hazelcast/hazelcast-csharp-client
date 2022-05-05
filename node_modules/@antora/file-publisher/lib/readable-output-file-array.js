'use strict'

const { Readable } = require('stream')
const Vinyl = require('vinyl')

class File extends Vinyl {
  get relative () {
    return this.path
  }
}

class ReadableOutputFileArray extends Readable {
  constructor (array) {
    super({ objectMode: true })
    this._array = array.slice().reverse()
  }

  _read (size) {
    const array = this._array
    while (size--) {
      const next = array.pop()
      if (next === undefined) break
      this.push(toOutputFile(next))
    }
    if (size > -1) this.push(null)
  }
}

function toOutputFile (file) {
  // Q: do we also need to clone contents and stat?
  return new File({ contents: file.contents, path: file.out.path, stat: file.stat })
}

module.exports = ReadableOutputFileArray
