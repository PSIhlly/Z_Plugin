# WangTile terrain batch

- Format: standard 4 x 6 sheet, 24 cells total.
- Sheet size: 96 x 144 px.
- Cell size: 24 x 24 px.
- Files: 9 materials x 3 variants = 27 PNG files.
- Topology: inherited pixel-for-pixel from `wang_reference_4x6.png`.
- QA: compatible left/right and top/bottom topology signatures are required to have identical rendered edge pixels.

## Unity import

Set Texture Type to Sprite (2D and UI), Sprite Mode to Multiple, Pixels Per Unit to 24,
Filter Mode to Point, Compression to None, and slice by a 24 x 24 cell grid with zero
offset and zero padding. Keep mipmaps disabled for a pixel-stable 2D tilemap.

`Output/` contains production PNGs, `Previews/` contains 4x grid overlays and a
contact sheet, and `qa_report.json` contains the machine validation results.
