/* eslint-disable no-unused-vars */
/* eslint-disable no-undef */
const settings = require('electron-settings')
const electron = require('electron')
const Player = require('./player.js')

const { BrowserWindow, remote } = electron
const { Menu, screen } = remote

const currentWindow = remote.getCurrentWindow()

class App {

  constructor() {
    this.activateSection = document.getElementById('activate')
    this.splashSection = document.getElementById('splash')
    this.playerSection = document.getElementById('player')


    // activate pages
    this.activatePin = document.getElementById('pin-code')
    // --------------

    this.player = new Player(this.playerSection)

    this.player.onplayed = () => {
      this.playerSection.style.display = 'block'
      this.splashSection.style.display = 'none'
    }

    this.player.onnext = () => {
      //socket.send('preload');
    }

    this.player.onend = () => {
      this.ws.send('end')
    }

    const template = [
      {
        label: 'Deactivate',
        accelerator: 'CmdOrCtrl+D',
        click: () => this.deactivate()
      },
      {
        label: 'Dev',
        accelerator: 'CmdOrCtrl+Shift+I',
        click: () => {
          currentWindow.webContents.openDevTools()
          console.log('dev')
        }
      },
      {
        label: 'Full',
        accelerator: 'F11',
        click: () => {
          if (currentWindow)
            currentWindow.setFullScreen(!currentWindow.isFullScreen())
          console.log('fullscreen')
        }
      }
    ]

    const menu = Menu.buildFromTemplate(template)

    Menu.setApplicationMenu(menu)

    // document.onkeyup = (e) => {
    //   if (e.code == 'KeyA') {
    //     console.log('Key A')
    //   } else if (e.ctrlKey) {
    //     if (e.code == 'KeyN') {
    //       console.log('settings windows')
    //       // openSettings();
    //     } else if (e.code == 'KeyD') {
    //       this.deactivate()
    //     }
    //   }
    // }
  }

  run() {
    const isActivate = settings.get('activate')

    if (isActivate) {
      this.defaultMode()
    } else {
      setTimeout(() => {
        this.deactivate()
      }, 200)
    }
  }

  defaultMode() {
    this.wait()
    let timerId = setInterval(() => {
      const port = remote.getGlobal('port')
      console.log(`port ${port}`)
      if (port != undefined) {
        clearInterval(timerId)
        this.connect()
      }
    }, 2000)
  }

  activate() {
    this.activateSection.style.display = ''
    let url = new URL(settings.get('config')['serverUrl'])
    let protocol = url.protocol == 'https:' ? 'wss:' : 'ws:'

    const ws = new WebSocket(`${protocol}//${url.hostname}/activate`)

    let activated = false

    ws.onopen = () => {
      console.log('activate socket open')
      const data = JSON.stringify({
        pin: settings.get('pin'),
        platform: 'windows',
      })

      console.log('activate send: ' + data)

      ws.send(data)
    }

    ws.onmessage = (e) => {
      let data = JSON.parse(e.data)
      console.log('activate msg: ', data)
      if (data != undefined && data.pin != undefined) {
        settings.set('pin', data.pin)
        this.activatePin.innerHTML = data.pin
      } else if (data != undefined && data.access_token != undefined) {
        settings.set('activate', data)
        activated = true
        this.activateSection.style.display = 'none'
        this.defaultMode()
      }
    }

    ws.onclose = (e) => {
      console.log('activate socket is closed')
      if (activated || e.reason != 'activated') return
      setTimeout(() => {
        this.activate()
      }, 2000)
    }

    ws.onerror = (err) => {
      console.log('activate socket encountered error: ', err.message, 'closing socket')
      ws.close()
    }
  }

  deactivate() {
    console.log('deactivate')
    settings.set('activate', null)
    settings.set('pin', '')
    this.player.stop()

    if (this.ws != undefined) {
      this.ws.close()
    }

    this.activatePin.innerHTML = '. . .'
    this.activateSection.style.display = ''
    this.playerSection.style.display = 'none'
    this.splashSection.style.display = 'none'
    this.player.stop()
    this.activate()
  }

  openSettings() {

    let win = new BrowserWindow({
      width: 800,
      height: 600,
      frame: false,
      show: false,
      webPreferences: {
        nodeIntegration: true
      }
    })

    win.once('ready-to-show', () => {
      win.show()
    })

    win.on('closed', () => {
      win = null
    })

    win.loadURL(`file://${__dirname}/../settings.html`)
  }

  connect() {
    const port = remote.getGlobal('port')

    var ws = new WebSocket(`ws://localhost:${port}`)

    ws.onopen = () => {
      this.ws = ws
      console.log('connected bind')

      const bound = screen.getPrimaryDisplay().workArea
      bound.x *= window.devicePixelRatio
      bound.y *= window.devicePixelRatio
      bound.width *= window.devicePixelRatio
      bound.height *= window.devicePixelRatio

      const data = settings.getAll()
      data.screen = bound
      ws.send(JSON.stringify(data))
    }

    ws.onmessage = (e) => {
      let data = JSON.parse(e.data)
      console.log('message:', data)
      if (data.media != null) {
        this.next(data.media, data.next)
      }
      if (data.next != null) {
        this.preload(data.next)
      }
      if (data.deactivate) {
        this.deactivate()
      }
      if (data.wait) {
        this.wait()
      }
      if (data.screenshot) {
        // this.sendScreenshot()
      }
      if (data.rebootId) {
        // this.relaunch(data.rebootId)
      }
    }

    ws.onclose = (e) => {
      console.log('socket is closed')
      if (settings.get('activate') == undefined) return
      this.wait()
      setTimeout(() => {
        this.connect()
      }, 1000)
    }

    ws.onerror = (err) => {
      console.log('socket encountered error: ', err.message, 'closing socket')
      ws.close()
    }
  }

  next(media, next) {
    this.player.play(media, next)
  }

  preload(media) {
    setTimeout(() => {
      this.player.preload(media)
    }, 2000)
  }

  wait() {
    this.splashSection.style.display = ''
    this.playerSection.style.display = 'none'
    this.player.stop()
  }
}

module.exports = App