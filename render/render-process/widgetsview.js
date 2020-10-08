const Browser = require('./htmlview')

class WidgetsView extends HTMLElement {

  constructor() {
    super()
    this.onkeydown = function (evt) {
      evt.preventDefault()
      console.log('keydown: ' + evt.keyCode)
    }
    this.views = {}
  }

  loadWidgets(widgets) {
    if (widgets == undefined)
      return

    let index = 2000
    let set = new Set()

    for (let i in widgets) {
      const item = widgets[i]
      const browser = this.load(item.url, item.data, item.position)
      set.add(browser)
      browser.zIndex = index++
    }

    const removes = []

    for (let i in this.views) {
      const browser = this.views[i]
      if (!set.has(browser)) {
        removes.push(i)
      }
    }

    this.removeBrowsers(removes)
  }

  removeBrowsers(removes) {
    for (const i in removes) {
      const j = removes[i]
      this.views[j].remove()
      delete this.views[j]
    }

    console.log('remove: ', this.views)
  }

  load(uri, data, pos) {
    let browser = undefined
    for (let i in this.views) {
      const item = this.views[i]
      if (item.uri == uri && item.equalsWidget(data, pos)) {
        browser = item
        break
      }
    }
    if (browser == undefined) {
      console.log('NOT FOUND WIDGETS!!!')
      browser = this.addBrowser()
    }
    browser.position = pos
    browser.data = data
    browser.uri = uri
    return browser
  }

  preload(widgets) {
    if (widgets == undefined || widgets.length == 0) return
    let set = new Set()

    for (let i in widgets) {
      let item = widgets[i]
      let browser = this.preloadUrl(item.url, undefined, item.position)
      set.add(browser)
    }

    const removes = []

    for (let i in this.views) {
      const browser = this.views[i]
      if (!set.has(browser) && browser.opacity == 0) {
        removes.push(i)
      }
    }

    this.removeBrowsers(removes)
  }

  preloadUrl(uri, data, pos) {
    let browser = undefined
    let free = []

    for (let i in this.views) {
      const item = this.views[i]
      if (item.uri == uri && item.equalsWidget(data, pos)) {
        browser = item
        break
      }
    }

    for (let i in this.views) {
      const item = this.views[i]
      if (item != browser && item.opacity == 0) {
        free.push(item)
      }
    }

    if (browser == undefined) {
      if (free.length > 0) {
        browser = free.shift()
      }
      if (browser == undefined) {
        this.addBrowser()
      }
      browser.position = pos
      browser.data = data
      browser.uri = uri
    }
    else {
      browser.data = data
    }
    return browser
  }

  clear() {
    for (let i in this.views) {
      this.views[i].remove()
    }

    this.views = {}
  }

  addBrowser() {
    const view = document.createElement('webview')
    const browser = new Browser(view)
    view.style.position = 'fixed'
    browser.transparent = false
    browser.opacity = 0
    this.appendChild(view)
    this.views[Object.keys(this.views).length] = browser
    return browser
  }

  updateData(data) {
    for (let i in this.views) {
      this.views[i].data = data
    }
  }
}

customElements.define('widgets-view', WidgetsView);