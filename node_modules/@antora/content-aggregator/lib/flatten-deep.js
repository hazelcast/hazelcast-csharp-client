'use strict'

function flattenDeep (array, accum = []) {
  const len = array.length
  for (let i = 0, it; i < len; i++) Array.isArray((it = array[i])) ? flattenDeep(it, accum) : accum.push(it)
  return accum
}

module.exports = flattenDeep
