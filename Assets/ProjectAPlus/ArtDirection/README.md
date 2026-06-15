# Project A+ Fresh Pixel Art Direction

Project A+ now uses a fully replaced original pixel-art set.

## Runtime Art

- `FreshPixelArt.cs` creates the player animation frames, enemies, bosses, items, effects, terrain tiles, props, gates, and UI frames.
- `Resources/FreshPixelArt/Backgrounds` contains three new original environment backdrops used by the title, cutscenes, and stages.
- Legacy `ProductionSprites`, `StableSprites`, `GeneratedBackgrounds`, and old master sheets were removed.

## Map Language

- Thick solid terrain masses form upper rooms, lower corridors, ceilings, and side structures.
- Gold top edges mark solid terrain.
- Cyan top edges mark one-way platforms.
- The continuous lower floor prevents accidental falling deaths.
- Background decoration never creates invisible collision.

## Backgrounds

- `CampusLectureDungeon.png`
- `MidnightDataLibrary.png`
- `FinalExamArchive.png`

The background images are original Project A+ assets generated with the built-in image-generation workflow. Runtime character and tile art is generated locally with `Texture2D.SetPixel()`.
