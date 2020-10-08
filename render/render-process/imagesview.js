const Img = require('./img')

class ImagesView extends HTMLElement {
	images = {}

	loadImages(images) {
		if (images == undefined) return

		let index = 2000
		let set = new Set()

		for (let i in images) {
			const item = images[i]
			const img = this.load(item.url, item.position)
			set.add(img)
			img.zIndex = index++
		}

		const removes = []

		for (let i in this.images) {
			const img = this.images[i]
			if (!set.has(img)) {
				img.remove()
				removes.push(i)
			}
		}

		for (const i in removes) {
			const j = removes[i]
			delete this.images[j]
		}
		console.log('after image remove: ', this.images)
	}

	preload(images) {

		if (images == undefined || images.length == 0) return

		for (let i in images) {
			let item = images[i]
			this.preloadImage(item.url, item.position)
		}
	}


	preloadImage(uri, pos) {
		let img = undefined
		let free = []

		for (let i in this.images) {
			const item = this.images[i]
			if (item.uri == uri && item.equalsImages(pos)) {
				browser = item
				break
			}
		}

		for (let i in this.images) {
			const item = this.images[i]
			if (item != img && item.opacity == 0) {
				free.push(item)
			}
		}

		if (img == undefined) {
			if (free.length > 0) {
				img = free.shift()
			}
			if (img == undefined) {
				this.add()
			}
			img.position = pos
			img.uri = uri
		}
		return img
	}

	load(uri, pos) {
		let img = undefined
		for (let i in this.images) {
			const item = this.images[i]
			if (item.uri == uri && item.equalsImages(pos)) {
				img = item
				break
			}
		}
		if (img == undefined) {
			console.log("NOT FOUND IMAGES!!!")
			img = this.add()
		}
		img.position = pos
		img.uri = uri
		img.opacity = 1
		return img
	}


	add() {
		const img = new Img()
		this.images[Object.keys(this.images).length] = img
		this.appendChild(img);

		img.transparent = false;
		img.opacity = 0;
		return img;
	}
}

customElements.define('images-view', ImagesView);