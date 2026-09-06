# Sprint 0 architecture guide

This guide explains the final implementation for Reagan Little. It is a learning
reference kept separate from the submission ZIP. The game code is unchanged by
submission packaging; only a project exclusion was added so the development
project does not accidentally compile the submission copy inside `Submission`.

## 1. The overall design

There are three main jobs: interpret input, update the player, and draw the result.
Keeping those jobs separate means a mouse click and a keyboard press can control
the same player without duplicating movement, speed, or animation rules.

```text
Program creates Game1
  Game1 creates and connects the objects
    KeyboardController ---- Move / Stop ---------+
    MouseController ------- MoveTo -------------+--> IPlayer -> Player
                                                         |
                                                         +--> ISprite -> AnimatedSprite
                                                                            |
                                                                            +--> SpriteSheet
    FloorMap -------------------------------> draws the checkerboard
    MousePointer -> SpriteSheet ------------> draws the cursor
    CreditsScreen --------------------------> draws the credits page

IsometricProjection converts tile coordinates <-> unzoomed drawing coordinates.
Game1's camera converts unzoomed drawing coordinates <-> window coordinates.
Builder runs during compilation to prepare assets, outside the running game.
```

The arrows represent calls or dependencies, not inheritance. The interfaces let
callers ask an object to do a job without requiring its particular implementation.

## 2. Startup and one game frame

`Program.cs` uses top-level C# statements. It creates a `Game1`, calls `Run`, and
uses `using` so the game is disposed when it exits. MonoGame supplies the loop;
there is no custom `while` loop in this project.

During startup, `Game1.LoadContent` loads the textures and font, constructs the
floor and character sprite, passes that sprite to the player, and connects both
controllers to that player. This is the one place that assembles the application.

Each update happens in a deliberate order:

1. If the window is active, the keyboard controller checks Tab and world controls.
   If it is inactive and the world is open, the player is stopped.
2. The mouse controller records its position and button state. It may request a
   new destination only when the game permits mouse movement.
3. If credits are closed, the player updates position and character animation.
   The pointer animation also advances if the window is active.
4. MonoGame's base update is called.

Drawing is separate. `Game1.Draw` clears the screen and draws either the credits
page or the world. The world draws floor first, character second, and pointer last.

**Why this order?** Keyboard input establishes priority before the mouse is
processed. Opening credits takes effect before player movement can advance in
that update. Drawing sees the result of the update instead of changing movement
while it renders. MonoGame may run updates without a matching draw, so all timing
and simulation belong in `Update`.

## 3. Every implemented class

### Game1 — assembly, update order, camera, and active page

**File:** `Game1.cs`. **Base class:** MonoGame `Game`.

`Game1` owns the graphics manager, sprite batch, floor, player/controller
references, mouse pointer, credits screen, and the small white pixel texture
used to draw rectangles. `creditsVisible` selects between the two pages.

Its constructor sets the 1280 by 720 window, content root, cursor visibility,
and control reminder in the window title. `LoadContent` creates objects only
after the graphics device and content services are available.

`GetCameraTransform` translates the player's projected position to zero, scales
the world by two, and translates it to the window center. The character's feet
therefore stay at the center even though its world position changes. The floor
appears to scroll in the opposite direction.

`DrawWorld` uses that camera for the floor and player, then starts a separate
batch without the camera for the mouse pointer. The credits screen also draws
without a world camera.

**Why:** one coordinator makes object ownership and update order easy to find.
Keeping setup in `Game1` avoids controllers secretly creating players or loading
assets. Sharing the camera calculation with mouse picking prevents the clicked
position from drifting away from what was actually drawn. Separating world and
screen drawing prevents zoom from moving the pointer or enlarging the credits.

### KeyboardController — turn keys into requests

**File:** `Controllers/KeyboardController.cs`. **Implements:** `IController`.

It holds an `IPlayer`, callbacks for quitting/toggling credits, a query for whether
credits are visible, the previous keyboard state, and whether movement keys were
held on the preceding update.

`Update()` polls the keyboard once and passes the result to
`Update(KeyboardState)`. The overload also allows a test to supply a known state.

Tab has first priority. A new Tab press calls `player.Stop()` and toggles credits;
other input is skipped for that update. While credits remain open, world controls
are skipped but the previous key state is still refreshed.

In the world, WASD/arrows produce a vector such as `(1, -1)`. Left Shift is passed
as the running flag. Escape invokes the quit callback only on a new key press.

`hadMovementInput` solves an important interaction: releasing keyboard movement
sends one zero-direction request to stop it. Later empty keyboard updates send
no movement request. If they sent `Move(Vector2.Zero)` forever, they would erase
the mouse destination every update.

`AllowsMouseMovement` is false while movement keys, Tab, or Escape are held.
Opposite keys still count as keyboard input, even when their resulting direction
is zero. That makes keyboard priority predictable.

**Why:** the controller describes intent; the player owns movement rules.
Previous/current state comparisons distinguish a press from a held key, preventing
Tab flicker and repeated quit commands. Small callbacks avoid giving this class
permission to modify every field in `Game1`.

### MouseController — interpret a click in world coordinates

**File:** `Controllers/MouseController.cs`. **Implements:** `IMouseController`.

It holds an `IPlayer`, functions that provide the current camera/window bounds/
input permission, the previous mouse state, and a readable `ScreenPosition`.

Every update records the pointer position. A click means the left button changed
from released to pressed. If the click is enabled and inside the window, the
controller inverts the current camera, transforms the window position into
unzoomed drawing coordinates, and calls `IsometricProjection.ToTile`. It passes
the resulting tile position to `player.MoveTo`.

Button history is updated even while clicks are disabled. This consumes clicks
made during credits or focus loss so they do not become delayed movement requests.

**Why:** saving a world position once makes the destination stay fixed as the
camera follows the player. Continuously aiming at the current window cursor
would chase a moving world destination. Query functions supply fresh camera and
permission values without coupling this class to the whole `Game1` object.

### Player — authoritative movement and character state

**File:** `Player/Player.cs`. **Implements:** `IPlayer`.

The player owns its tile position, normalized movement direction, running flag,
optional target tile position, and facing direction. It also holds the map's
maximum allowed tile position and an `ISprite` supplied by `Game1`.

- `Move(direction, isRunning)` cancels any click destination and selects manual
  movement. This is how keyboard takeover works.
- `MoveTo(tilePosition)` stores a destination clamped to the floor bounds.
- `Stop()` sends zero manual movement, clearing both running and any destination.
- `SetMovement` normalizes the vector and selects the nearest of eight facing
  directions using its angle. A zero vector preserves the previous facing.
- `Update` computes travel from speed and elapsed time, updates the position,
  then tells the sprite the facing, movement state, and animation speed.
- `Draw` delegates rendering to the sprite.

Walking is 80 unzoomed drawing pixels per second. Running multiplies this by two,
and click movement uses that same multiplier. At the 2x camera zoom, the displayed
rates are 160 and 320 pixels per second respectively.

For a destination, `Update` projects the remaining tile difference into drawing
coordinates. If the next step would reach or pass the destination, it assigns
the exact target and stops. Otherwise, it advances along the normalized vector.
The final position is kept within `[0, 31]` on both tile axes.

**Why:** one movement implementation keeps keyboard running and mouse movement
at the same speed. Normalization prevents diagonals from being faster. Elapsed
time prevents speed from depending on the number of frames. Limiting the last
step prevents overshooting and oscillating around the target. A nullable target
expresses two clear states: following a destination or using manual movement.

The path is a straight line because the floor has no obstacles. The character
can travel at arbitrary angles toward a click while selecting the nearest
available facing sprite. No obstacle-search algorithm or navigation mesh is used.

### IsometricProjection — pure coordinate conversion

**File:** `Isometric/IsometricProjection.cs`. **Static class** with no stored state.

Tiles use a 64 by 32 unzoomed footprint. `ToScreen(x, y)` computes:

```text
drawingX = (x - y) * 32
drawingY = (x + y) * 16
```

For example, tile `(1, 0)` projects to `(32, 16)`; tile `(0, 1)` projects to
`(-32, 16)`. Integer tile coordinates represent top-face centers.

`ToTile` reverses the equations. Both methods accept fractional positions, so
walking is smooth and a click can select a point between tile centers.

**Why:** rendering, movement, and picking must agree about the same geometry.
Two small functions keep the math in one place and are easy to check by round
tripping a point. The projection does not include zoom; the camera handles that
separately. This separates map geometry from how closely the viewer sees it.

### FloorMap — select and place the blocks

**File:** `Isometric/FloorMap.cs`.

`FloorMap` holds the tile texture, source rectangles for the light/dark blocks,
the draw origin, and grid dimensions. It exposes the center position to help
`Game1` place the player initially.

`Draw` loops through the 32 by 32 grid and selects a block with `(x + y) % 2`.
It projects each tile coordinate and submits that sprite to the supplied batch.

The sheet cells are 18 by 18 pixels, but adjacent top faces use a 16 by 8 pixel
pitch. Drawing at four-times scale gives the 64 by 32 projection footprint.
The `(9, 4)` origin anchors the top face instead of the bottom of the entire cube.

**Why:** using the sheet cell width as tile spacing would leave gaps from its
padding. Alternating parity generates the whole checkerboard without storing
1,024 tile records. The chosen traversal and equal-height floor allow simple
back-to-front drawing. Taller objects would need their own depth-ordering rules.
At this map size, culling and a map-editor dependency would add work without a
demonstrated need.

### SpriteSheet — describe rectangles within a texture

**File:** `Sprites/SpriteSheet.cs`.

It stores a texture, cell dimensions, and the offset where a group of frames
begins. `GetFrame(column, row)` returns a source rectangle:

```text
x = offsetX + column * cellWidth
y = offsetY + row * cellHeight
```

The character uses 16 by 24 cells starting at `(0, 216)`. The cursor uses 16 by
16 cells starting at `(0, 0)`.

**Why:** character and cursor animation share rectangle arithmetic without
sharing their very different playback rules. The class does not load textures,
choose animation states, or own the camera. Keeping those decisions elsewhere
lets the same rectangle helper serve both assets.

### AnimatedSprite — character pose timing and drawing

**File:** `Sprites/AnimatedSprite.cs`. **Implements:** `ISprite`.

This class holds a `SpriteSheet`, origin, scale, current direction, pose index,
and elapsed animation time. The player supplies direction, whether it actually
moved, and an animation-speed multiplier.

While moving, the pose sequence is `0, 1, 2, 1`. It uses the three supplied poses
and returns through the standing pose between alternating steps. Each sequence
entry lasts 0.12 seconds while walking and 0.06 seconds at the running multiplier.
Standing resets the timer and selects pose 1.

Drawing asks `SpriteSheet` for the direction column and pose row, then uses the
supplied bottom anchor `(8, 22)` and four-times scale. The two transparent rows
below the feet are excluded from the anchor, avoiding a floating character.

**Why:** player physics and input do not need to know rectangle coordinates or
animation clocks. Drawing does not advance the clock, so skipped draws do not
slow the animation. The constructor accepts sheet, origin, and scale for reuse,
although the current playback model still assumes this three-pose character layout.

### MousePointer — independent cursor animation

**File:** `Sprites/MousePointer.cs`.

It stores a `SpriteSheet` and elapsed time within a one-second loop. Drawing
selects one of six frames from that time and anchors it at the supplied mouse
position, using origin `(8, 8)` and scale two.

**Why:** the cursor animates continuously in the world, even when the character
stands still. It has no facing direction or walking state, so forcing it through
the character-oriented `ISprite.Update` signature would require meaningless
arguments. It reuses `SpriteSheet` while keeping its own small playback API.
Screen-space drawing also keeps its hotspot aligned with the actual click.

### CreditsScreen — display-only page

**File:** `Screens/CreditsScreen.cs`.

It receives a `SpriteFont` and the shared white pixel texture. `Draw` paints an
opaque background, title, return hint, and a bordered box near the bottom. The
box height comes from font line spacing, padding, and the three attribution lines.
Stretching the white pixel and tinting it creates the solid rectangles.

**Why:** this class only handles presentation. Keyboard handling belongs in the
controller; deciding whether to advance the world belongs in `Game1`. It neither
polls Tab nor moves the player. Two pages need only one visibility boolean, so a
scene hierarchy or menu framework would add unnecessary structure here.

### Builder — prepare content before the game runs

**File:** `Builder/Builder.cs`. **Base class:** MonoGame `ContentBuilder`.

This is a separate console project, not a runtime game object. Its entry point
runs the builder and returns an error status if content compilation fails.
`GetContentCollection` explicitly includes the tile sheet, character sheet,
six-frame cursor image, and credits font.

The texture processor preserves color, disables mipmaps and color-key removal,
and premultiplies alpha. The cursor and character receive short output names,
so runtime code loads `Cursors/BlueCircle` and `Characters/OrangeWalker` instead
of depending on long source-folder names.

**Why:** processing assets during the build produces XNB files that MonoGame's
content manager can load consistently. Keeping the builder separate prevents
its entry point and tooling from becoming game code. Explicit asset selection
also keeps the unused cursor-pack images out of the build.

## 4. The four interfaces and the direction enum

An interface is a contract, not a second implementation. These interfaces make
dependencies explicit and allow tests to use substitutes that record calls.

| Type | Contract | Why it exists |
| --- | --- | --- |
| `IPlayer` | Read tile position; request manual movement, a target, or stop; update and draw | Both controllers operate the same player without setting its private state. |
| `IController` | Update input | `Game1` can schedule a controller through a small common contract. |
| `IMouseController` | `IController` plus readable screen position | Rendering needs pointer position without access to mouse-controller internals. |
| `ISprite` | Update character animation and draw at a supplied position | The player delegates appearance while retaining movement responsibility. |

`Player/Direction.cs` defines `North, NorthEast, East, SouthEast, South,
SouthWest, West, NorthWest`. The numeric order matches the eight sheet columns.
This lets `AnimatedSprite` use the direction directly as a column index. The
tradeoff is that a differently arranged sheet would need an explicit remapping.

There is no `ICommand` in this version. Each action already fits a player method
or a short callback. A separate command class for every action would introduce
more files without a current requirement for undo, recording, or replay.

## 5. Three concrete examples of cooperation

### Holding W + D + Left Shift

The keyboard controller requests `Move((1, -1), true)`. `Player` normalizes the
vector to approximately `(0.707, -0.707)` and chooses the northeast facing.
For a 1/60-second update, running advances about 2.667 unzoomed drawing pixels.
The projection converts that displacement to tile coordinates. The sprite
receives the northeast direction and animation speed two. The camera follows
the new position, so the character stays centered while the floor scrolls.

### Clicking 320 window pixels to the right of the player's feet

At 2x zoom, that is 160 unzoomed drawing pixels to the right. The inverse camera
and `ToTile` convert it to a tile offset of `(2.5, -2.5)`. Starting at the map
center `(15.5, 15.5)`, the target is `(18, 13)`.

The player saves that target and runs at 160 unzoomed pixels per second. On an
uninterrupted path it arrives after approximately one second. Empty keyboard
updates leave the route intact. Moving the mouse elsewhere changes the pointer
but not the target. On the final step the player snaps only the remaining small
distance to the exact target, clears the route, and stands.

### Pressing Tab during that route

The keyboard controller sees a new Tab press, stops the player, and opens credits.
`Game1` then skips player and pointer animation updates. The mouse still records
button history but cannot request movement. Rendering selects `CreditsScreen`.
Holding Tab does not repeat the toggle. After releasing and pressing it again,
the world reappears; the cancelled route does not restart. A held mouse button
or Escape key from the credits page is not treated as a fresh press.

## 6. Build files, ownership, and deliberate limits

`sprint0_new.slnx` groups the game and content-builder projects.
`sprint0_new.csproj` references DesktopGL and the builder, excludes builder/example
C# files from game compilation, and imports `BuildContent.targets`.
`ReferenceOutputAssembly="false"` makes the builder a build dependency rather
than a library that the game calls at runtime.

`BuildContent.targets` runs the builder after project references are resolved and
before game compilation. It chooses matching Debug/Release paths, quotes paths
for folders containing spaces, and uses `MSBuild.NormalizePath` because that
helper works in both the installed Visual Studio and command-line build engines.
It does not run the content step for design-time builds used by the editor.

The supplied assets and build settings are necessary submission inputs. `bin`
contains generated outputs, and `obj` contains generated intermediate files.
They are rebuilt from source and are therefore absent from the submission ZIP,
as are Git metadata, editor state, temporary checks, and unused asset variants.
The package contains a readme and the supplied asset license/credit files.

Loaded textures and the font belong to MonoGame's content manager. `SpriteSheet`,
`FloorMap`, and the renderer classes borrow those references. `Game1` explicitly
disposes the `SpriteBatch` and one-pixel texture it created itself. `Program`'s
`using` statement ensures game disposal. This avoids having several classes try
to dispose the same graphics resource.

Names use descriptive camelCase private fields, PascalCase public members, and
named constants for fixed speeds, dimensions, and timing. Readonly references
make dependencies stable after construction. The `null!` fields in `Game1` reflect
MonoGame's lifecycle: the references are assigned in `LoadContent`, not in the
constructor. That relies on the normal startup sequence and successful loading.

This is intentionally a small movement demo. The final version contains no
jumping from the original plan. The floor has no obstacles, camera rotation,
or variable terrain heights. Credits use a layout designed for the fixed window.
Those facts explain why the implementation can remain small; adding those
features later would require revisiting the relevant responsibilities rather
than pretending the current code is a complete engine.

## 7. A useful reading order

Read `Program.cs`, then `Game1.LoadContent`, then `Game1.Update` to see how the
objects are connected and scheduled. Follow a `Move` request into `Player`,
then into `AnimatedSprite` and `SpriteSheet`. Next follow `MouseController` into
the inverse camera and projection. Finish with the three drawing paths:
`FloorMap`, `MousePointer`, and `CreditsScreen`. Read `Builder` last to connect
the asset names in `LoadContent` to the original files.

Reference documentation: [MonoGame's 2D guide](https://docs.monogame.net/articles/tutorials/building_2d_games/index.html),
[SpriteBatch](https://docs.monogame.net/api/Microsoft.Xna.Framework.Graphics.SpriteBatch.html),
and [content-builder projects](https://docs.monogame.net/articles/getting_started/content_pipeline/content_builder_project.html).
The explanations above are based on the implemented source in this submission.
