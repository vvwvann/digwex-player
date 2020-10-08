class ImageView extends HTMLElement {
  images = [];
  currIndex = 0;

  constructor() {
    super();

    this.images.push(document.createElement('img'));
    this.images.push(document.createElement('img'));

    this.appendChild(this.images[0]);
    this.appendChild(this.images[1]);
  }

  preload(media) {
    let index = this.currIndex;
    index++;
    index %= 2;
    this.currIndex = index;
    let image = this.images[index];
    image.src = media.main.url;
    //player.style.zIndex = '0';
    image.style.display = 'none';
  }

  play(media) {
    let index = this.currIndex;
    let image = this.images[index];
    let src = media.main.url;

    if (image.src != src) {
      image.src = src;
    }
    image.style.display = 'block';
    this.images[(index + 1) % 2].style.display = 'none';
  }

  /**
   * @param {boolean} ok
   */
  set visible(ok) {
    this.style.display = (ok == true ? 'block' : 'none');
  }
}

customElements.define('image-view', ImageView);