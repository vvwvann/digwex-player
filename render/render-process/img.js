class Img extends Image {
  img = null;
  _uri = null;
  hash = -1;
  id = -1;
  loaded = false
  isForeground = false
  _position = undefined


  constructor(img) {
    super()
    this.img = img;
    this.style.position = 'fixed'
  }

  equalsImages(pos) {
    return this._position != undefined && pos != undefined && pos[0] == this._position[0] &&
      pos[1] == this._position[1] &&
      pos[2] == this._position[2] &&
      pos[3] == this._position[3]
  }

  /**
   * @param {any} uri
   */
  set uri(uri) {
    this._uri = uri;
    this.src = uri;
  }

  set position(pos) {
    this.style.left = pos[0] + '%'
    this.style.top = pos[1] + '%'
    this.style.width = pos[2] + '%'
    this.style.height = pos[3] + '%'
    this._position = pos
  }

  get uri() {
    return this._uri;
  }

  _vis = false;

  set visible(ok) {
    this.style.zIndex = (ok ? '1000' : '0');
  }

  get visible() {
    //return this._vis; 
    return this.style.zIndex == '1000';
  }

  set zIndex(value) {
    this.style.zIndex = value
  }

  get opacity() {
    return this.style.opacity
  }

  set opacity(value) {
    this.style.opacity = value
  }

  /**
   * @param {any} ok
   */
  set preload(ok) {
    this.style.zIndex = 500;
  }
}

module.exports = Img