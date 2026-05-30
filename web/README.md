# Avenger Asteroids — web version

A browser port of the game, the way the original ran (it was a Java applet
embedded in a web page). This version is plain **HTML5 + Canvas 2D + JavaScript**,
with no build step and no dependencies — the game logic mirrors the desktop
C#/OpenTK port.

## Play it

Because it loads image assets, it needs to be served over HTTP (opening
`index.html` directly as a `file://` can be blocked by the browser):

```bash
cd web
python -m http.server 8000
# then open http://localhost:8000/
```

Or host the `web/` folder anywhere static — e.g. **GitHub Pages** — and it just
works.

### Controls

`↑` thrust · `←` / `→` turn · `Space` fire · `C` teleport · `B` brake · `F1` debug

## How it maps to the desktop port

The classes match the desktop version (`Sprite`, `Ship`, `Alien`, `Asteroid`,
`Bullet`, `Level`, `Hud`, `Game`); only the rendering layer differs:

| Desktop (OpenTK) | Web (Canvas) |
| --- | --- |
| textured GL quads | `ctx.drawImage` (PNG alpha is native) |
| `glBegin` vector shapes | canvas paths (bullets, thrust, shields) |
| bitmap-font HUD | `ctx.fillText` |
| 25 fps update cap | fixed 25 fps timestep via `requestAnimationFrame` |

The same gameplay fixes apply: ship invulnerability (so the asteroid-split
cascade can't instakill you) and the gentler 25 fps pace.
