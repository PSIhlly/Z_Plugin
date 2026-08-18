# High-resolution transparent WangTile batch

- Standard layout: 4 columns x 6 rows (24 cells).
- Sheet size: 384 x 576 px.
- Cell size: 96 x 96 px.
- Format: RGBA PNG with real alpha transparency.
- Materials: river, sea, lava, mud, road, cement, ice, sand, grass, rock.
- Effects per material: three inset variants, raised, feather, and noise.
- Total: 60 unique sheets.

The terrain surface and transition art are generated at the 96px cell scale from
high-resolution source swatches. This is not a nearest-neighbor enlargement of
the old 24px output. Alpha contours follow each material's luminance, grains,
cracks, and ridges, while visible edge colors are extended beneath transparent
pixels to avoid hard cutoffs and dark filtering fringes. All compatible Wang
edges are validated across RGBA pixels.

## Unity import

Use Sprite (2D and UI), Sprite Mode Multiple, Pixels Per Unit 96, Alpha Is
Transparency enabled, Max Size at least 1024, Compression None, and mipmaps off.
Slice as a 96 x 96 grid with zero offset and zero padding. Bilinear filtering is
recommended for this hand-painted HD set; use Point only for a deliberately
pixelated presentation.
