require('./imageview')
require('./htmlview').default
require('./seamless-player')
require('./widgetsview')
require('./imagesview')


class Player {

  plIndex = 0;
  videoView = null;
  htmlView = null;
  imageView = null;
  widgets
  mainContent
  images

  playedEv = () => { };
  endEv = () => { };
  nextEv = () => { };

  constructor(elem) {
    let child = elem.children;
    this.mainContent = child[0];
    this.widgets = child[1];
    this.images = child[2];
    child = child[0].children
    this.videoView = child[0];
    this.imageView = child[1];
    this.htmlView = child[2];

    const cl = this;

    this.videoView.onplay = function () {
      cl.playedEv();
    };

    this.videoView.onended = function () {
      //cl.next();
      //check
      cl.endEv();
    };
  }

  play(media, next) {
    //let media = this.playlist[this.plIndex];
    let timeout = true;
    if (media.main.Type == 'html' || media.main.type == 'url') {
      this.htmlView.play(media.main.url, media.main.js);
      this.visibleUpdate(false, true, false);
      this.playedEv();
    } else if (media.main.type == 'video') {
      timeout = false;
      this.videoView.play(media);
      this.visibleUpdate(true, false, false);
      this.clearHtml(next)
    } else if (media.main.type == 'image') {
      this.imageView.play(media);
      this.visibleUpdate(false, false, true);
      this.playedEv();
      this.clearHtml(next)
    }

    if (media.main.position) {
      this.mainContent.style.left = media.main.position[0] + '%'
      this.mainContent.style.top = media.main.position[1] + '%'
      this.mainContent.style.width = media.main.position[2] + '%'
      this.mainContent.style.height = media.main.position[3] + '%'
    } else {
      this.mainContent.style.left = '0%'
      this.mainContent.style.top = '0%'
      this.mainContent.style.width = '100%'
      this.mainContent.style.height = '100%'
    }

    let imageWidgets = []
    let htmlWidgets = []
    let wids = media.widgets

    for (let i in wids) {
      if (wids[i].type == 'image') {
        imageWidgets.push(wids[i])
      }
      else if (wids[i].type == 'html' || wids[i].type == 'url') {
        htmlWidgets.push(wids[i])
      }
    }

    this.widgets.loadWidgets(htmlWidgets)
    this.images.loadImages(imageWidgets)

    if (timeout == true) {
      setTimeout(() => {
        console.log('WTF: ' + media.totalSecond);
        this.endEv();
      }, 1000 * media.totalSecond);
    }
  }

  stop() {
    this.videoView.stop()
    this.htmlView.stop()
  }

  clearHtml(next) {
    if (
      next == undefined ||
      next.main == undefined ||
      next.main.type == 'html' ||
      next.main.type == 'url') return
    console.log(`clear curr ${next.main.type}`)
    this.htmlView.clear()
  }

  preload(media) {
    //let index = this.plIndex;
    //index++;
    //index %= this.playlist.length;
    //this.plIndex = index;
    //let media = this.playlist[index];
    if (media.main.type == 'html' || media.main.type == 'url') {
      this.htmlView.preload(media.main.url, media.main.js);
    } else if (media.main.type == 'video') {
      this.videoView.preload(media);
    } else if (media.main.type == 'image') {
      this.imageView.preload(media);
    }
  }

  visibleUpdate(video, html, image) {
    if (html) {
      this.htmlView.visible = true;
      this.videoView.visible = false;
      this.imageView.visible = false;
    }
    if (image) {
      this.imageView.visible = true;
      this.htmlView.visible = false;
      this.videoView.visible = false;
    }
    if (video) {
      this.videoView.visible = true;
      this.imageView.visible = false;
      this.htmlView.visible = false;
    }

  }

  /**
   * @param {() => void} callback
  */
  set onplayed(callback) {
    this.playedEv = callback;
  }

  /**
   * @param {() => void} callback
  */
  set onnext(callback) {
    this.nextEv = callback;
  }

  /**
   * @param {() => void} callback
  */
  set onend(callback) {
    this.endEv = callback;
  }
}

module.exports = Player