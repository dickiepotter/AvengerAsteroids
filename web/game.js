/*
 * Avenger Asteroids — web port.
 *
 * A faithful re-implementation of the 2003 GL4Java game (by Richard Potter) on an
 * HTML5 canvas. The world is the original [-100, 100] square; sprites are textured
 * quads with alpha transparency, drawn with the browser's image/text APIs instead
 * of OpenGL. Game logic (movement, collisions, splitting, the alien, levels,
 * shields) mirrors the desktop C#/OpenTK port.
 */
(() => {
  'use strict';

  // ---- World / view -------------------------------------------------------
  const canvas = document.getElementById('game');
  const ctx = canvas.getContext('2d');
  const W = canvas.width, H = canvas.height;

  const wLeft = -100, wRight = 100, wBottom = -100, wTop = 100;
  const scale = W / (wRight - wLeft);
  const cx = W / 2, cy = H / 2;

  // World -> screen (y is up in the world, down on the canvas).
  const sx = x => cx + x * scale;
  const sy = y => cy - y * scale;

  // Rotate a local point by a (world, counter-clockwise) heading, place it at an
  // origin, and return screen coordinates. Used for the vector shapes.
  function pt(localX, localY, ox, oy, headingDeg) {
    const r = headingDeg * Math.PI / 180;
    const wx = ox + (localX * Math.cos(r) - localY * Math.sin(r));
    const wy = oy + (localX * Math.sin(r) + localY * Math.cos(r));
    return [sx(wx), sy(wy)];
  }

  // ---- Assets -------------------------------------------------------------
  const TEX_FILES = [
    'backdrop-Sun.png', 'backdrop-Galaxy.png', 'backdrop-Moon.png',
    'meteor.png', 'ship.png', 'alien.png', 'flame.png',
  ];
  const textures = [];

  function loadAssets() {
    return Promise.all(TEX_FILES.map((f, i) => new Promise(res => {
      const img = new Image();
      img.onload = () => res();
      img.src = 'assets/' + f;
      textures[i] = img;
    })));
  }

  // ---- Sprite base --------------------------------------------------------
  class Sprite {
    constructor(game) {
      this.game = game;
      this.manoeuvrability = 10;
      this.acceleration = 0.2;
      this.textureNumber = -1;
      this.dim = [[-8, 8], [8, 8], [8, -8], [-8, -8]];
      this.heading = 0;
      this.positionX = 0; this.positionY = 0;
      this.velocityX = 0; this.velocityY = 0;
      this.maxBullets = 1;
      this.collisionRadius = 8;
    }

    // Half-width/height from the corner dimensions.
    get w() { return this.dim[1][0] - this.dim[0][0]; }
    get h() { return this.dim[0][1] - this.dim[3][1]; }

    drawShape() { /* invisible for textured sprites; bullets override */ }

    renderTexture() {
      const img = textures[this.textureNumber];
      ctx.save();
      ctx.translate(sx(this.positionX), sy(this.positionY));
      ctx.rotate(-this.heading * Math.PI / 180);
      ctx.drawImage(img, -this.w / 2 * scale, -this.h / 2 * scale, this.w * scale, this.h * scale);
      ctx.restore();
    }

    draw() {
      this.positionX += this.velocityX;
      this.positionY += this.velocityY;
      this.wrapAround();

      this.drawShape();
      if (this.textureNumber >= 0) this.renderTexture();
    }

    wrapAround() {
      if (this.positionX < wLeft) this.positionX = wRight;
      if (this.positionX > wRight) this.positionX = wLeft;
      if (this.positionY < wBottom) this.positionY = wTop;
      if (this.positionY > wTop) this.positionY = wBottom;
    }

    randomPosition() {
      this.positionX = Math.random() * 200;
      if (this.positionX > 100) this.positionX = Math.random() * -100;
      this.positionY = Math.random() * 200;
      if (this.positionY > 100) this.positionY = Math.random() * -100;
    }

    collisionDetection(other) {
      const dx = this.positionX - other.positionX;
      const dy = this.positionY - other.positionY;
      if (Math.sqrt(dx * dx + dy * dy) < this.collisionRadius + other.collisionRadius) {
        this.collisionEffect(other);
        other.collisionEffect(this);
      }
    }

    collisionEffect() { this.drawExplode(); }

    drawExplode() { this.textureNumber = 6; this.renderTexture(); }

    reset() {
      this.heading = 0;
      this.positionX = 0; this.positionY = 0;
      this.velocityX = 0; this.velocityY = 0;
    }
  }

  // ---- Ship ---------------------------------------------------------------
  const MAX_SPEED = 5;

  class Ship extends Sprite {
    constructor(game) {
      super(game);
      this.dim = [[-6, 6], [6, 6], [6, -6], [-6, -6]];
      this.maxBullets = 4;
      this.shots = new Bullets(game, this, false);
      this.textureNumber = 4;
      this.emergencyBrakes = 2;
      this.showThrust = false;
      this.xHeadingSwitch = false;
      this.yHeadingSwitch = false;
      this.shields = 100;
      this.invulnerable = 60; // start-of-life grace (see desktop port)
    }

    rotate(rleft) {
      const t = this.heading;
      this.heading += rleft ? -this.manoeuvrability : this.manoeuvrability;
      if (this.heading > 360) this.heading = 0;
      if (this.heading < 0) this.heading = 360;
      const h = this.heading;
      if (((t >= 90 && t < 270) && (h >= 270 && h < 360 || h < 90 && h >= 0)) ||
          ((h >= 90 && h < 270) && (t >= 270 && t < 360 || t < 90 && t >= 0))) this.xHeadingSwitch = true;
      if (((t >= 0 && t < 180) && (h >= 180 && h < 360)) ||
          ((h >= 0 && h < 180) && (t >= 180 && t < 360))) this.yHeadingSwitch = true;
    }

    emergencyStop() {
      if (this.emergencyBrakes > 0 && (this.velocityX !== 0 || this.velocityY !== 0)) {
        this.velocityX = 0; this.velocityY = 0;
        this.emergencyBrakes--;
      }
    }

    fire() { this.shots.createBullet(); }

    thrust() {
      const r = this.heading * Math.PI / 180;
      if (this.velocityX < MAX_SPEED && this.velocityX > -MAX_SPEED || this.xHeadingSwitch) {
        this.velocityX += this.acceleration * Math.sin(-r);
        this.showThrust = true;
        if (this.velocityX < MAX_SPEED && this.velocityX > -MAX_SPEED) this.xHeadingSwitch = false;
      }
      if (this.velocityY < MAX_SPEED && this.velocityY > -MAX_SPEED || this.yHeadingSwitch) {
        this.velocityY += this.acceleration * Math.cos(-r);
        this.showThrust = true;
        if (this.velocityY < MAX_SPEED && this.velocityY > -MAX_SPEED) this.yHeadingSwitch = false;
      }
    }

    collisionEffect(other) {
      if (other instanceof Bullet && !other.baddieBullet) return;
      if (this.invulnerable > 0) return;
      this.shields -= 5;
      this.invulnerable = 45;
    }

    destroyed() {
      if (this.shields <= 0) {
        this.velocityX = 0; this.velocityY = 0; this.acceleration = 0;
        this.drawExplode();
        return true;
      }
      return false;
    }

    shieldColor() {
      if (this.shields <= 30) return '#ff3030';
      if (this.shields <= 60) return '#3060ff';
      return '#30d030';
    }

    drawShields() {
      ctx.save();
      ctx.strokeStyle = this.shieldColor();
      ctx.lineWidth = Math.max(1, Math.min(6, this.shields / 10));
      ctx.beginPath();
      ctx.arc(sx(this.positionX), sy(this.positionY), this.collisionRadius * scale, 0, Math.PI * 2);
      ctx.stroke();
      ctx.restore();
    }

    drawThrust() {
      const p0 = pt(0, this.dim[2][1] * 2, this.positionX, this.positionY, this.heading);
      const p1 = pt(this.dim[2][0] / 3, this.dim[2][1], this.positionX, this.positionY, this.heading);
      const p2 = pt(this.dim[3][0] / 3, this.dim[3][1], this.positionX, this.positionY, this.heading);
      const grad = ctx.createLinearGradient(p1[0], p1[1], p0[0], p0[1]);
      grad.addColorStop(0, '#cc0000');
      grad.addColorStop(1, '#ffff00');
      ctx.fillStyle = grad;
      ctx.beginPath();
      ctx.moveTo(p0[0], p0[1]);
      ctx.lineTo(p1[0], p1[1]);
      ctx.lineTo(p2[0], p2[1]);
      ctx.closePath();
      ctx.fill();
      this.showThrust = false;
    }

    draw() {
      if (this.invulnerable > 0) this.invulnerable--;
      const blinkHide = this.invulnerable > 0 && Math.floor(this.invulnerable / 4) % 2 === 0;
      const saved = this.textureNumber;
      if (blinkHide) this.textureNumber = -1;
      super.draw();
      this.textureNumber = saved;

      this.drawShields();
      if (this.showThrust) this.drawThrust();
    }
  }

  // ---- Asteroid -----------------------------------------------------------
  class Asteroid extends Sprite {
    constructor(game, sub) {
      super(game);
      this.spent = false;
      this.subAsteroid = sub;
      if (sub === 1) { this.dim = [[-6, 6], [6, 6], [6, -6], [-6, -6]]; this.collisionRadius = 6; }
      if (sub === 2) { this.dim = [[-4, 4], [4, 4], [4, -4], [-4, -4]]; this.collisionRadius = 5; }
      this.acceleration = 2;
      this.textureNumber = 3;
      this.randomPosition();
      this.heading = Math.floor(Math.random() * 360);
      const r = this.heading * Math.PI / 180;
      this.velocityY += this.acceleration * Math.cos(-r);
      this.velocityX += this.acceleration * Math.sin(-r);
    }

    draw() { super.draw(); this.heading += 5; }

    collisionEffect(other) {
      if (other instanceof Ship || other instanceof Bullet) {
        if (other instanceof Bullet && other.baddieBullet) return;
        this.drawExplode();
        this.game.level.score();
        this.spent = true;
      } else {
        this.heading += this.heading < 180 ? 150 : -150;
      }
    }
  }

  // ---- Alien --------------------------------------------------------------
  class Alien extends Sprite {
    constructor(game) {
      super(game);
      this.dim = [[-6, 6], [6, 6], [6, -6], [-6, -6]];
      this.collisionRadius = 6;
      this.maxBullets = 1;
      this.randomPosition();
      this.acceleration = 2;
      this.textureNumber = 5;
      this.heading = Math.floor(Math.random() * 360);
      const r = this.heading * Math.PI / 180;
      this.velocityY += this.acceleration * Math.cos(-r);
      this.velocityX += this.acceleration * Math.sin(-r);
      this.shots = new Bullets(game, this, true);
      this.fireDelay = 25;
      this.spent = false;
    }

    collisionEffect(other) {
      if (other instanceof Ship || other instanceof Bullet) {
        if (other instanceof Bullet && other.baddieBullet) return;
        this.drawExplode();
        this.game.level.score();
        this.game.level.score();
        this.spent = true;
      } else {
        this.heading += this.heading < 180 ? 150 : -150;
      }
    }

    draw() {
      super.draw();
      if (this.fireDelay < 0) { this.shots.createBullet(); this.fireDelay = 20; }
      this.fireDelay--;
    }
  }

  // ---- Bullet -------------------------------------------------------------
  class Bullet extends Sprite {
    constructor(game, bad, owner) {
      super(game);
      this.dim = [[-1, 2], [1, 2], [1, -1], [-1, -1]];
      this.baddieBullet = bad;
      this.spent = false;
      this.distance = 0;
      this.acceleration = 6;
      this.maxTravelDistance = wRight * 1.2;
      if (bad) {
        let dx = game.baddie.positionX - game.goodie.positionX;
        let dy = game.baddie.positionY - game.goodie.positionY;
        const hyp = Math.sqrt(dx * dx + dy * dy);
        dx /= hyp; dy /= hyp;
        this.velocityX += this.acceleration * -dx;
        this.velocityY += this.acceleration * -dy;
      } else {
        this.heading = owner.heading;
        const r = this.heading * Math.PI / 180;
        this.velocityY += this.acceleration * Math.cos(-r);
        this.velocityX += this.acceleration * Math.sin(-r);
      }
      this.positionX = owner.positionX;
      this.positionY = owner.positionY;
    }

    outOfRange() { this.distance += this.acceleration; if (this.distance > this.maxTravelDistance) this.spent = true; }

    drawShape() {
      const c = [
        pt(this.dim[0][0], this.dim[0][1], this.positionX, this.positionY, this.heading),
        pt(this.dim[1][0], this.dim[1][1], this.positionX, this.positionY, this.heading),
        pt(this.dim[2][0], this.dim[2][1], this.positionX, this.positionY, this.heading),
        pt(this.dim[3][0], this.dim[3][1], this.positionX, this.positionY, this.heading),
      ];
      const grad = ctx.createLinearGradient(c[2][0], c[2][1], c[0][0], c[0][1]);
      grad.addColorStop(0, '#e6e600');
      grad.addColorStop(1, '#990000');
      ctx.fillStyle = grad;
      ctx.beginPath();
      ctx.moveTo(c[0][0], c[0][1]);
      for (let i = 1; i < 4; i++) ctx.lineTo(c[i][0], c[i][1]);
      ctx.closePath();
      ctx.fill();
    }

    collisionEffect(other) {
      if (other instanceof Ship && !this.baddieBullet) return;
      if (other instanceof Alien && this.baddieBullet) return;
      this.spent = true;
    }
  }

  // ---- Bullet / asteroid collections --------------------------------------
  class Bullets {
    constructor(game, owner, bad) {
      this.game = game; this.owner = owner; this.baddieBullet = bad;
      this.list = new Array(owner.maxBullets).fill(null);
    }
    createBullet() {
      for (let i = 0; i < this.list.length; i++) {
        if (this.list[i] === null) { this.list[i] = new Bullet(this.game, this.baddieBullet, this.owner); return; }
      }
    }
    checkSpent() {
      for (let i = 0; i < this.list.length; i++) {
        if (this.list[i]) { this.list[i].outOfRange(); if (this.list[i].spent) this.list[i] = null; }
      }
    }
    draw() { this.checkSpent(); for (const b of this.list) if (b) b.draw(); }
    collisionDetection() {
      for (const shot of this.list) {
        if (!shot) continue;
        shot.collisionDetection(this.game.goodie);
        if (this.game.baddie) shot.collisionDetection(this.game.baddie);
        for (const rock of this.game.rocks.list) if (rock) shot.collisionDetection(rock);
      }
    }
  }

  class Asteroids {
    constructor(game) { this.game = game; this.list = []; }
    createAsteroids() {
      this.list = [];
      for (let i = 0; i < this.game.level.currentLevel; i++) this.list.push(new Asteroid(this.game, 0));
    }
    checkSpent() {
      for (let i = 0; i < this.list.length; i++) {
        const a = this.list[i];
        if (a && a.spent) {
          if (a.subAsteroid < 2) this.createSubAsteroids(a.positionX, a.positionY, a.subAsteroid + 1);
          this.list[i] = null;
        }
      }
    }
    createSubAsteroids(x, y, sub) {
      for (let i = 0; i < 4; i++) {
        const a = new Asteroid(this.game, sub);
        a.positionX = x; a.positionY = y;
        this.list.push(a);
      }
    }
    draw() { this.checkSpent(); for (const a of this.list) if (a) a.draw(); }
    collisionDetection() {
      for (let i = 0; i < this.list.length; i++) {
        const rock = this.list[i];
        if (!rock) continue;
        rock.collisionDetection(this.game.goodie);
        if (this.game.baddie) rock.collisionDetection(this.game.baddie);
        for (let j = 0; j < this.list.length; j++) if (j !== i && this.list[j]) rock.collisionDetection(this.list[j]);
      }
    }
    empty() { return this.list.every(a => a === null); }
  }

  // ---- Level --------------------------------------------------------------
  class Level {
    constructor(game) { this.game = game; this.currentLevel = 1; this.backgroundNumber = 0; this.totalScore = 0; this.endGameDelay = 40; }
    setLevel(n) {
      this.currentLevel = n;
      this.backgroundNumber++;
      if (this.backgroundNumber > 2) this.backgroundNumber = 0;
      this.game.setup();
    }
    levelChange() { if (this.game.rocks.empty()) this.setLevel(this.currentLevel + 1); }
    score() { this.totalScore += this.currentLevel; }
    gameOver() {
      if (this.game.goodie.destroyed()) {
        this.game.controlEnabled = false;
        this.game.goodie.heading += 15;
        if (this.endGameDelay < 0) {
          this.backgroundNumber = 4; // end-game backdrop (ship texture; faithful quirk)
          this.currentLevel = -1;
        }
        this.endGameDelay--;
      }
    }
  }

  // ---- HUD (canvas text) --------------------------------------------------
  class Hud {
    constructor(game) { this.game = game; }

    text(s, x, y, size, color, align = 'left') {
      ctx.fillStyle = color;
      ctx.textAlign = align;
      ctx.textBaseline = 'alphabetic';
      ctx.font = `${size}px system-ui, sans-serif`;
      ctx.fillText(s, sx(x), sy(y));
    }

    playDetails() {
      const g = this.game;
      this.text('Level ' + g.level.currentLevel, wLeft + 3, wTop - 8, 16, '#fff');
      this.text('Score: ' + g.level.totalScore, wLeft + 3, wTop - 18, 16, '#fff');
      this.text('Shields', wRight - 70, wTop - 10, 16, '#fff', 'right');
      this.drawShieldBar();
      this.text('Emergency brakes ' + g.goodie.emergencyBrakes, wLeft + 3, wBottom + 6, 14, '#fff');
      this.text('RP', wRight - 3, wBottom + 6, 12, '#fff', 'right');
    }

    gameOver() {
      const g = this.game;
      this.text('You scored ' + g.level.totalScore, wLeft + 15, wTop - 30, 18, '#fff');
      this.text('GAME OVER', wLeft + 15, wTop - 60, 30, '#ccb019');
      this.text('Created by Richard Potter', wLeft + 15, wBottom + 60, 18, '#ff8080');
      this.text('With thanks to NASA for many of the pictures', wLeft + 15, wBottom + 30, 13, '#808080');
    }

    drawShieldBar() {
      const g = this.game;
      const x0 = sx(wRight - g.goodie.shields), x1 = sx(wRight);
      const y0 = sy(wTop), y1 = sy(wTop - 6);
      ctx.fillStyle = g.goodie.shieldColor();
      ctx.fillRect(x0, y0, x1 - x0, y1 - y0);
    }

    debugDetails(s) {
      this.text(`Position (${s.positionX | 0} , ${s.positionY | 0})`, wLeft + 3, wBottom + 4, 12, '#fff');
      this.text(`Velocity (${s.velocityX | 0} , ${s.velocityY | 0})`, wLeft + 3, wBottom + 10, 12, '#fff');
      this.text(`Heading ${s.heading} deg`, wLeft + 3, wBottom + 16, 12, '#fff');
      if (s instanceof Ship) this.text(`Shields ${s.shields}`, wLeft + 3, wBottom + 22, 12, '#fff');
    }
  }

  // ---- Game ---------------------------------------------------------------
  class Game {
    constructor() {
      this.level = new Level(this);
      this.controlEnabled = true;
      this.debugGoodie = false;
      this.goodie = new Ship(this);
      this.setup();
    }

    setup() {
      this.goodie.reset();
      this.hud = new Hud(this);
      this.rocks = new Asteroids(this);
      this.baddie = new Alien(this);
      this.rocks.createAsteroids();
    }

    renderBackground() {
      const img = textures[this.level.backgroundNumber];
      if (img) ctx.drawImage(img, 0, 0, W, H);
    }

    display() {
      ctx.clearRect(0, 0, W, H);
      this.renderBackground();

      if (this.level.currentLevel >= 0) {
        if (this.debugGoodie) this.hud.debugDetails(this.goodie); else this.hud.playDetails();
        this.rocks.draw();
        if (this.baddie) {
          this.baddie.collisionDetection(this.goodie);
          if (this.baddie.spent) this.baddie = null;
        }
        if (this.baddie) {
          this.baddie.draw();
          this.baddie.shots.draw();
          this.baddie.shots.collisionDetection();
        }
        this.goodie.draw();
        this.goodie.shots.draw();
        this.rocks.collisionDetection();
        this.goodie.shots.collisionDetection();
        this.level.levelChange();
        this.level.gameOver();
      } else {
        this.hud.gameOver();
      }
    }
  }

  // ---- Input --------------------------------------------------------------
  const held = {};
  function setupInput(game) {
    const map = { ArrowUp: 1, ArrowLeft: 1, ArrowRight: 1 };
    window.addEventListener('keydown', e => {
      if (e.key in map || e.code === 'Space' || ['KeyC', 'KeyB', 'F1'].includes(e.code)) e.preventDefault();
      if (!game.controlEnabled) return;
      if (e.repeat) return;
      switch (e.code) {
        case 'Space': game.goodie.fire(); break;
        case 'KeyC': game.goodie.randomPosition(); break;
        case 'KeyB': game.goodie.emergencyStop(); break;
        case 'F1': game.debugGoodie = !game.debugGoodie; break;
      }
      held[e.code] = true;
    });
    window.addEventListener('keyup', e => { held[e.code] = false; });
  }

  function pollHeld(game) {
    if (!game.controlEnabled) return;
    if (held['ArrowUp']) game.goodie.thrust();
    if (held['ArrowLeft']) game.goodie.rotate(false);
    if (held['ArrowRight']) game.goodie.rotate(true);
  }

  // ---- Fixed-timestep loop -----------------------------------------------
  const FPS = 25;            // matches the desktop pace
  const STEP = 1000 / FPS;

  loadAssets().then(() => {
    const game = new Game();
    setupInput(game);

    let last = performance.now();
    let acc = 0;
    function loop(now) {
      acc += now - last;
      last = now;
      if (acc >= STEP) {
        if (acc > STEP * 5) acc = STEP; // don't spiral after a tab switch
        acc -= STEP;
        pollHeld(game);
        game.display();
      }
      requestAnimationFrame(loop);
    }
    requestAnimationFrame(loop);
  });
})();
