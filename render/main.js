/* eslint-disable no-unused-vars */
const { app, BrowserWindow, Menu } = require('electron')
const path = require('path')
const { spawn, exec, fork } = require('child_process')
const portscanner = require('portscanner')
const settings = require('electron-settings')
const defConfig = require('./config.json')


const isProd = process.env.ELECTRON_ENV !== 'dev'

console.log('mode:', isProd ? 'prod' : 'dev')

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let win
let config

process.on('uncaughtException', function (error) {
  console.log(error)
  setTimeout(() => {
    console.log('relaunch')
    // app.relaunch()
    app.exit(0)
  }, 5000)
})

app.on('ready', () => {
  global.platform = process.platform
  console.log(global.platform)
  config = settings.get('config')

  if (config == undefined) {
    const appData = app.getPath('appData')
    defConfig.appData = path.resolve(appData, 'digwex')
    defConfig.contentPath = path.resolve(appData, 'digwex/content')
    settings.set('config', defConfig)
    config = defConfig
  }

  console.log(config)

  global.platform = process.platform
  createWindow()

  if (isProd || true) {
    startCore()
  } else {
    global.port = 8000
  }
})

// Quit when all windows are closed.
app.on('window-all-closed', () => {
  // On macOS it is common for applications and their menu bar
  // to stay active until the user quits explicitly with Cmd + Q
  if (process.platform !== 'darwin') {
    app.quit()
  }
})

app.on('activate', () => {
  // On macOS it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (win === null) {
    createWindow()
  }
})

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and require them here.


function createWindow() {
  win = new BrowserWindow({
    x: config.position.x,
    y: config.position.y,
    width: config.position.width,
    height: config.position.height,
    frame: false,
    show: false,
    fullscreen: false && config.fullscreen,
    webPreferences: {
      nodeIntegration: true,
      webviewTag: true
    }
  })

  if (!isProd) {
    win.webContents.openDevTools()
  }

  // if (isProd)
  //   win.setAlwaysOnTop(true, 'screen-saver')
  global.__dirname = __dirname
  win.loadURL(path.join('file://', __dirname, './index.html'))

  win.webContents.session.clearCache()

  win.webContents.on('did-finish-load', () => {
    win.setTitle(app.name)
    win.show()
  })

  win.on('closed', () => {
    win = null
    app.quit()
  })

  let timerId

  win.on('minimize', () => {
    timerId = setTimeout(() => {
      win.restore()
    }, config.expanding * 1000)
  })

  win.on('restore', () => {
    clearTimeout(timerId)
  })
}

function startCore() {
  portscanner.findAPortNotInUse(8000, 9000, '127.0.0.1', function (error, port) {
    global.port = port
    process.stdout.write(`start core: ${port}\n`)

    let core
    if (process.platform == 'linux' || process.platform == 'darwin') {
      const pathCore = isProd ? `${__dirname}/core/./core` : `${__dirname}/../core/Digwex.Core/bin/x86/debug/netcoreapp3.1/./core`
      core = spawn(pathCore, [port])
    }
    if (process.platform == 'win32') {

      const pathCore = isProd ? `${__dirname}/core/core.exe` : `${__dirname}/../core/Digwex.Core/bin/debug/netcoreapp3.1/core.exe`
      core = spawn(pathCore, [port])

      // core = spawn(isProd ? `${ __dirname } / core / core.exe} ` : `${ __dirname } /../core / Digwex.Core / bin / x86 / debug / netcoreapp3.1 / core.exe`, [port])
    }

    core.stdout.on('data', (data) => {
      process.stdout.write(data)
    })

    core.on('exit', function (code) {
      console.log(`About to exit with code ${code} `)
      if (code != 0) {
        setTimeout(() => startCore(), 5000)
      }
    })
  })
}