class HtmlView extends HTMLElement {

  constructor() {
    super()
    this.views = []
  }

  addBrowser() {
    const view = document.createElement('webview');
    const browser = new Browser(view);
    browser.transparent = false;
    this.appendChild(view);
    browser.visible = false;
    this.views.push(browser);
    return browser;
  }

  preload(uri, data) {
    let browser;
    let free = [];

    this.views.forEach(function (v) {
      if (v.uri == uri) {
        browser = v;
      }
      else if (!v.visible) {
        free.push(v);
      }
    });

    if (browser == undefined) {
      if (free.length > 0)
        browser = free[0];
      if (browser == null)
        browser = this.addBrowser();
      browser.uri = uri;
    }
    browser.data = data;
    browser.preload = true;
  }

  play(uri, data) {
    let browser;

    this.views.forEach(function (v) {
      if (v.uri == uri) {
        browser = v;
      }
      else {
        v.visible = false;
      }
    });

    if (browser == undefined) {
      browser = this.addBrowser();
      browser.uri = uri;
    } else {
      console.log(`play from preload: ${browser.uri}`);
    }
    browser.data = data;
    browser.visible = true;
  }

  clear() {
    while (this.firstChild) {
      this.removeChild(this.firstChild)
    }
    this.views = []
  }

  updateData(id) {
  }

  stop() {
    this.visible = false
  }

  /**
   * @param {boolean} ok
  */
  set visible(ok) {
    this.style.display = (ok == true ? 'block' : 'none');
    if (!ok) {
      this.views.forEach(function (v) {
        if (v.visible)
          v.visible = false;
      });
    }
  }
}

customElements.define('html-view', HtmlView);

class Browser {
  view = null;
  _uri = null;
  _data
  hash = -1;
  id = -1;
  loaded = false
  isForeground = false
  _position = undefined


  constructor(view) {
    this.view = view;

    view.addEventListener('dom-ready', () => {
      view.setAudioMuted(true)
    });

    view.addEventListener('did-finish-load', () => {
      this.loaded = true;
      this.opacity = 1
      if (this.visible) {
        view.setAudioMuted(false)
      }
      console.log(`html loaded: ${this._uri}`);
      if (this._data != undefined) {
        //console.log(this._data.length);
        view.executeJavaScript('setData(' + this._data + ')')
      }
    });
  }

  equalsWidget(data, pos) {
    let ok = this._position == undefined && pos == undefined

    if (pos != undefined && this._position != undefined) {
      ok = pos[0] == this._position[0] &&
        pos[1] == this._position[1] &&
        pos[2] == this._position[2] &&
        pos[3] == this._position[3]
    }
    return ok && this._data == data
  }

  remove() {
    this.view.remove()
  }

  /**
   * @param {any} uri
   */
  set uri(uri) {
    if (this._uri != uri) {
      this._uri = uri;
      this.view.src = uri;
    }
  }

  /**
   * @param {any} data
   */
  set data(data) {
    // console.log('update data:', data)
    if (this.data != data) {
      this._data = data;
      if (this.loaded) {
        this.view.executeJavaScript('setData(' + data + ')')
      }
    }
  }

  set position(pos) {
    this.view.style.left = pos[0] + '%'
    this.view.style.top = pos[1] + '%'
    this.view.style.width = pos[2] + '%'
    this.view.style.height = pos[3] + '%'
    this._position = pos
  }

  get uri() {
    return this._uri;
  }

  _vis = false;

  set visible(ok) {
    //this._vis = ok;
    if (this.loaded)
      this.view.setAudioMuted(!ok)
    this.view.style.zIndex = (ok ? '1000' : '0');
  }

  get visible() {
    //return this._vis; 
    return this.view.style.zIndex == '1000';
  }

  set zIndex(value) {
    this.view.style.zIndex = value
  }

  get opacity() {
    return this.view.style.opacity
  }

  set opacity(value) {
    this.view.style.opacity = value
  }

  /**
   * @param {any} ok
   */
  set preload(ok) {
    this.view.style.zIndex = 500;
  }

}

module.exports = Browser