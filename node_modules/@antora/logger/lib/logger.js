'use strict'

const { EventEmitter, once } = require('events')
const expandPath = require('@antora/expand-path-helper')
const ospath = require('path')
const { posix: path } = ospath
const {
  levels: { labels: levelLabels, values: levelValues },
  symbols: { serializersSym: $serializers, streamSym: $stream },
  pino,
} = require('pino')
const { default: pinoPretty, prettyFactory } = require('pino-pretty')
const SonicBoom = require('sonic-boom')

const closedLogger = { closed: true }
const finalizers = []
const INF = Infinity
const minLevel = levelLabels[Math.min.apply(null, Object.keys(levelLabels))]
const noopLogger = pino({ base: null, enabled: false, timestamp: false }, {})
const rootLoggerHolder = new Map()
const standardStreams = { 1: 1, 2: 2, stderr: 2, stdout: 1 }

function close () {
  const rootLogger = rootLoggerHolder.get() || closedLogger
  if (rootLogger.closed) return
  const dest = Object.assign(rootLogger, closedLogger)[$stream].stream || rootLogger[$stream]
  if (dest instanceof EventEmitter && typeof dest.end === 'function' && !dest.destroyed) {
    finalizers.push(once(dest, 'close').catch(() => undefined)) && dest.end()
  }
}

function configure ({ name, level = 'info', levelFormat, failureLevel = 'silent', format, destination } = {}, baseDir) {
  let logger
  if ((levelValues[level] || (level === 'all' ? (level = minLevel) : INF)) === INF) {
    if ((levelValues[failureLevel] || INF) === INF && (rootLoggerHolder.get() || {}).noop) return module.exports
    close()
    logger = Object.assign(Object.create(Object.getPrototypeOf(noopLogger)), noopLogger)
  } else {
    const prettyPrint = format === 'pretty'
    let colorize
    if (typeof (destination || (destination = {})).write !== 'function') {
      let dest
      const { file, bufferSize, ...destOpts } = destination
      if (bufferSize != null) destOpts.minLength = bufferSize
      if (file && !(dest = standardStreams[file])) {
        dest = expandPath(file, { dot: baseDir })
        colorize = false
      } else if (prettyPrint) {
        dest = dest || 2
      }
      destOpts.dest = dest || 1
      destination = new SonicBoom(Object.assign({ mkdir: true, sync: true }, destOpts))
    }
    const config = {
      name,
      base: {},
      level,
      formatters: { level: levelFormat === 'number' ? (_, level) => ({ level }) : (level) => ({ level }) },
      hooks: {
        // NOTE logMethod only called if log level is enabled
        logMethod (args, method) {
          const arg0 = args[0]
          if (arg0 instanceof Error) {
            reshapeErrorForLog(arg0, args[1], prettyPrint && this[$serializers].err).forEach((v, i) => (args[i] = v))
          } else if (arg0.constructor === Object && typeof arg0.file === 'object') {
            const { file, line, stack, ...obj } = arg0
            args[0] = Object.assign(obj, reshapeFileForLog(arg0)) // NOTE assume file key is a file.src object
          }
          method.apply(this, args)
        },
      },
    }
    close()
    prettyPrint
      ? ((logger = pino(config, createPrettyDestination(destination, colorize)))[$stream].stream = destination)
      : (logger = pino(config, destination))
    if (destination instanceof SonicBoom) moderateDestination(destination)
  }
  rootLoggerHolder.set(undefined, addFailOnExitHooks(logger, failureLevel))
  return module.exports
}

function get (name) {
  if (name === null) return rootLoggerHolder.get()
  return new Proxy(noopLogger, {
    resolveTarget () {
      if ((this.ownRootLogger || closedLogger).closed) {
        if ((this.ownRootLogger = rootLoggerHolder.get() || closedLogger).closed) {
          ;(this.ownRootLogger = configure({ format: 'pretty' }).get(null)).warn(
            'logger not configured; creating logger with default settings'
          )
        }
        this.target = undefined
      }
      return this.target || (this.target = name ? this.ownRootLogger.child({ name }) : this.ownRootLogger)
    },
    get (_, property) {
      return property === 'unwrap' ? () => this.resolveTarget() : this.resolveTarget()[property]
    },
    set (_, property, value) {
      this.resolveTarget()[property] = value
      return true
    },
  })
}

function finalize () {
  close()
  return Promise.all(finalizers.splice(0, finalizers.length)).then(() => (rootLoggerHolder.get() || {}).failOnExit)
}

function createPrettyDestination (destination, colorize) {
  if (colorize == null) {
    colorize =
      process.env.NO_COLOR == null &&
      (process.env.FORCE_COLOR != null || prettyFactory()({ msg: 'colorize' }).includes('\u001b['))
  }
  return pinoPretty({
    destination,
    colorize,
    customPrettifiers: {
      file: ({ path: path_, line }) => (line == null ? path_ : `${path_}:${line}`),
      stack: (stack, _, log) => {
        if (!Array.isArray(stack)) return JSON.stringify(stack, null, 2)
        let prevSource = log.source
        return stack
          .map(({ file: { path: path_, line }, source }) => {
            const file = `${path_}:${line}`
            const sameSource =
              prevSource &&
              source.url === prevSource.url &&
              source.refname === prevSource.refname &&
              source.startPath === prevSource.startPath
            prevSource = source
            if (sameSource) return `\n    file: ${file}`
            const { url, worktree, refname, startPath } = source
            source = worktree
              ? `${worktree} (refname: ${refname} <worktree>${startPath ? ', start path: ' + startPath : ''})`
              : `${url || '<unknown>'} (refname: ${refname}${startPath ? ', start path: ' + startPath : ''})`
            return `\n    file: ${file}\n    source: ${source}`
          })
          .join('')
      },
      source: ({ url, worktree, refname, startPath }) =>
        worktree
          ? `${worktree} (refname: ${refname} <worktree>${startPath ? ', start path: ' + startPath : ''})`
          : `${url || '<unknown>'} (refname: ${refname}${startPath ? ', start path: ' + startPath : ''})`,
    },
    ignore: 'hint',
    messageFormat: (log, msgKey) => {
      let hint, msg
      if (typeof (msg = log[msgKey]) !== 'string') return
      if ((hint = log.hint)) msg += '\n' + (colorize ? `\x1b[2m${hint}\x1b[22m` : hint)
      if (colorize) msg = msg.replace('\n', '\n\x1b[0m')
      return msg
    },
    translateTime: 'SYS:HH:MM:ss.l', // Q: do we really need ms? should we honor DATE_FORMAT env var?
  })
}

function moderateDestination (dest) {
  dest.flushSync = undefined // pino's fatal handler wants to call this, but we do our own flushing
  if (dest.fd === standardStreams[dest.fd] && !dest.listeners('error').find((l) => l.name === 'filterBrokenPipe')) {
    dest.on('error', function disconnectBrokenPipe (err) {
      if (err.code === 'EPIPE') return Object.assign(this, { destroyed: true, write: () => undefined })
      this.removeListener('error', disconnectBrokenPipe).emit('error', err)
    })
  }
}

function reshapeErrorForLog (err, msg, prettyPrint, serialize) {
  const { name, message } = err
  let stack
  if ({}.propertyIsEnumerable.call(err, 'name')) Object.defineProperty(err, 'name', { enumerable: false })
  if (msg === undefined) msg = message
  if (message && message === msg) err.message = undefined
  if ((stack = err.backtrace)) {
    stack = ['Error', ...stack.slice(1)].join('\n')
  } else if ((stack = err.stack || name) && err instanceof SyntaxError && stack.includes('\nSyntaxError: ')) {
    stack = `SyntaxError: ${message}\n    at ` + stack.split(/\n+SyntaxError: [^\n]+\n?/).join('\n')
  }
  if (message && (message === msg || !prettyPrint) && stack.startsWith(`${name}: ${message}`)) {
    stack = stack.replace(`${name}: ${message}`, name)
  }
  err.stack = (prettyPrint ? 'Cause: ' : '') + (stack === name ? `${name} (no stacktrace)` : stack)
  if (prettyPrint) {
    const { message: _discard, ...flatErr } = prettyPrint(err)
    err = Object.assign(flatErr, { type: 'Error' })
  }
  return [err, msg]
}

function reshapeFileForLog ({ file: { abspath, origin, path: vpath }, line, stack }) {
  if (origin) {
    const { url, refname, startPath, worktree } = origin
    const logObject = {
      file: { path: abspath || path.join(startPath, vpath), line },
      source: worktree
        ? { url, worktree, refname, startPath: startPath || undefined }
        : { url, refname, startPath: startPath || undefined },
    }
    if (stack) logObject.stack = stack.map(reshapeFileForLog)
    return logObject
  }
  return stack ? { file: { path: vpath, line }, stack: stack.map(reshapeFileForLog) } : { file: { path: vpath, line } }
}

function addFailOnExitHooks (logger, failureLevel = undefined) {
  let failureLevelVal
  if (failureLevel === undefined) {
    failureLevelVal = logger.failureLevelVal
  } else {
    logger.failureLevelVal = failureLevelVal = levelValues[failureLevel] || INF
    Object.defineProperty(logger, 'failureLevel', {
      enumerable: true,
      get () {
        return levelLabels[this.failureLevelVal]
      },
    })
    logger.setFailOnExit = setFailOnExit.bind(logger) // direct call to base logger
    logger.child = ((method) =>
      function (bindings) {
        return addFailOnExitHooks(method.call(this, bindings))
      })(logger.child)
  }
  Object.defineProperty(logger, 'noop', {
    enumerable: true,
    get () {
      return this.levelVal === INF && this.failureLevelVal === INF
    },
  })
  if (failureLevelVal !== INF) {
    for (const [levelName, levelVal] of Object.entries(levelValues)) {
      if (levelVal >= failureLevelVal) logger[levelName] = decorateWithSetFailOnExit(logger[levelName])
    }
  }
  return logger
}

function decorateWithSetFailOnExit (method) {
  return method.name === 'noop'
    ? callSetFailOnExit
    : function (...args) {
      this.setFailOnExit()
      method.apply(this, args)
    }
}

function callSetFailOnExit () {
  this.setFailOnExit()
}

function setFailOnExit () {
  this.failOnExit = true
}

module.exports = Object.assign(get, {
  close,
  closeLogger: close,
  configure,
  configureLogger: configure,
  finalize,
  finalizeLogger: finalize,
  get,
  getLogger: get,
})
