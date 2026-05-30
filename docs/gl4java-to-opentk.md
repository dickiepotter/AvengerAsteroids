# From GL4Java to OpenTK

Avenger Asteroids (2003) was a Java applet rendered with **GL4Java**, a binding
to OpenGL that has been dead for ~two decades. This port moves it to **OpenTK 4
on .NET 8**, keeping the orthographic, immediate-mode, alpha-blended rendering
intact and replacing only the dead plumbing.

## API name mapping

| Concept | GL4Java (2003) | OpenTK 4 |
| --- | --- | --- |
| GL handle | `openGl.glBegin(...)` (a passed `GLFunc`) | static `GL.Begin(...)` |
| Constants | loose `int` (`GL_QUADS`) | typed enums (`PrimitiveType.Quads`) |
| Verts / coords | `glVertex2d` / `glTexCoord2f` | `GL.Vertex2` / `GL.TexCoord2` |
| Blending | `glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA)` | `GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)` |
| Texture load | `PngTextureLoader` | `StbImageSharp` (`Core/TextureLoader.cs`) |
| Text | `glutBitmapString` | bitmap font (`Core/Font8x8` + `Core/Text.cs`) |
| Window / canvas | `GLAnimCanvas` (applet) | `GameWindow` |
| Animation loop | `GLAnimCanvas` redraw thread | `GameWindow` render loop |
| Input | AWT `KeyListener` | per-frame `KeyboardState` + `OnKeyDown` |

## What had to be re-implemented

- **Texture loading (`Core/TextureLoader.cs`).** `PngTextureLoader` is gone;
  StbImageSharp decodes the PNGs to RGBA. The sprite textures carry an alpha
  channel, so they're uploaded as RGBA and drawn with
  `SRC_ALPHA, ONE_MINUS_SRC_ALPHA` — which is what makes the ship/alien/meteor
  show on transparent backgrounds. The author's header note that "black boxes
  appear" at home was exactly this alpha path failing on the old binding; with a
  real RGBA upload it works.
- **HUD text (`Core/Font8x8.cs`, `Core/Text.cs`).** OpenTK has no GLUT, so the
  `glutBitmapString` calls are replaced by drawing a public-domain 8×8 bitmap font
  as small quads in world space.

## Default-state differences worth knowing

- **Context profile.** Immediate mode, the matrix stack, `glOrtho` and `GL_QUADS`
  were removed from core OpenGL 3.1+. OpenTK defaults to a Core profile that
  rejects them, so the window requests `ContextProfile.Compatability`.
- **Texture orientation.** `glTexImage2D` treats the first uploaded row as the
  texture's bottom, while PNG rows run top-first. The game's texture coordinates
  put `(0,0)` at the bottom-left, so the loader flips vertically on load —
  otherwise every sprite and backdrop is upside down.
- **Row alignment.** Unpack alignment is set to 1 so non-power-of-two images don't
  unpack into a skewed pattern.
- **Frame pacing.** The applet redrew off its own canvas thread; the OpenTK loop
  is capped at 60 fps so the per-frame movement speeds (tuned for the original's
  rate) stay consistent.
