class SeamlessPlayer extends HTMLElement {

  players = [];
  currIndex = 0;

  onplayEv = function () { };
  onendedEv = function () { };
  onerrorEv = function () { };

  constructor() {
    super();

    this.players.push(document.createElement('video'));
    this.players.push(document.createElement('video'));

    this.appendChild(this.players[0]);
    this.appendChild(this.players[1]);

    this.players[0].preload = 'auto';
    this.players[1].preload = 'auto';
    this.players[0].autobuffer = true;
    this.players[1].autobuffer = true;

    const cl = this;

    this.players[0].onerror = function () {
      cl.onerrorEv();
    };

    this.players[1].onerror = function () {
      cl.onerrorEv();
    };

    this.players[0].onplay = function () {
      cl.onplayEv();
    };

    this.players[1].onplay = function () {
      cl.onplayEv();
    };

    this.players[0].onended = function () {
      console.log('ended: ' + 0);
      cl.onendedEv();
    };

    this.players[1].onended = function () {
      console.log('ended: ' + 1);
      cl.onendedEv();
    };
  }

  preload(media) {
    let index = this.currIndex;
    index++;
    index %= 2;
    this.currIndex = index;
    console.log('preload video: ' + index);
    let player = this.players[index];
    player.src = media.main.url;
    //player.style.zIndex = '0';
    player.style.display = 'none';
  }

  play(media) {
    let index = this.currIndex;
    console.log('play video: ' + index);
    let player = this.players[index];
    let src = media.main.url;
    console.log(player.src, src);

    if (player.src != src) {
      player.src = src;
    }
    player.play();
    player.style.display = 'block';
    this.players[(index + 1) % 2].style.display = 'none';
  }

  stop() {
    this.players[0].pause()
    this.players[1].pause()
  }

  /**
   * @param {() => void} callback
   */
  set onplay(callback) {
    this.onplayEv = callback;
  }

  /**
   * @param {() => void} callback
   */
  set onended(callback) {
    this.onendedEv = callback;
  }

  /**
   * @param {() => void} callback
   */
  set onerror(callback) {
    this.onerrorEv = callback;
  }

  /**
   * @param {boolean} ok
   */
  set visible(ok) {
    this.style.display = (ok == true ? 'block' : 'none');
  }
}

customElements.define('seamless-player', SeamlessPlayer);
