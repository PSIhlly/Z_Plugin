param(
    [string]$Source = (Join-Path $PSScriptRoot '0.single-layer-source.png'),
    [string]$Output = (Join-Path $PSScriptRoot '0.png'),
    [string]$Preview = (Join-Path $PSScriptRoot '0.single-layer-preview.png'),
    [switch]$PreviewOnly
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

public static class FenceWangTilePacker
{
    const int Quarter = 256;
    const int Tile = Quarter * 2;

    static Bitmap NewBitmap(int width, int height)
    {
        return new Bitmap(width, height, PixelFormat.Format32bppArgb);
    }

    static Graphics GraphicsFor(Bitmap image)
    {
        Graphics graphics = Graphics.FromImage(image);
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        return graphics;
    }

    static Rectangle VisibleBounds(Bitmap image, Rectangle region)
    {
        int left = region.Right, right = region.Left, top = region.Bottom, bottom = region.Top;
        for (int y = region.Top; y < region.Bottom; y++)
            for (int x = region.Left; x < region.Right; x++)
                if (image.GetPixel(x, y).A > 32)
                {
                    left = Math.Min(left, x);
                    right = Math.Max(right, x + 1);
                    top = Math.Min(top, y);
                    bottom = Math.Max(bottom, y + 1);
                }
        if (left >= right || top >= bottom)
            throw new InvalidOperationException("Expected a visible railing in the source region.");
        return Rectangle.FromLTRB(left, top, right, bottom);
    }

    static void ClearRect(Bitmap image, Rectangle rect)
    {
        using (Graphics graphics = GraphicsFor(image))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            using (Brush transparent = new SolidBrush(Color.Transparent))
                graphics.FillRectangle(transparent, rect);
        }
    }

    static Bitmap BuildTile(Bitmap horizontal, Bitmap vertical, Bitmap post, int directions)
    {
        Bitmap result = NewBitmap(Tile, Tile);
        using (Graphics graphics = GraphicsFor(result))
        {
            if ((directions & 1) != 0)
                graphics.DrawImage(vertical, new Rectangle(0, 0, Tile, Quarter),
                    new Rectangle(0, 0, Tile, Quarter), GraphicsUnit.Pixel);
            if ((directions & 2) != 0)
                graphics.DrawImage(horizontal, new Rectangle(Quarter, 0, Quarter, Tile),
                    new Rectangle(Quarter, 0, Quarter, Tile), GraphicsUnit.Pixel);
            if ((directions & 4) != 0)
                graphics.DrawImage(vertical, new Rectangle(0, Quarter, Tile, Quarter),
                    new Rectangle(0, Quarter, Tile, Quarter), GraphicsUnit.Pixel);
            if ((directions & 8) != 0)
                graphics.DrawImage(horizontal, new Rectangle(0, 0, Quarter, Tile),
                    new Rectangle(0, 0, Quarter, Tile), GraphicsUnit.Pixel);
            graphics.DrawImageUnscaled(post, 0, 0);
        }
        return result;
    }

    static void PartCell(int part, out int column, out int row)
    {
        column = part <= 8 ? 2 + (part - 5) % 2 : (part - 9) % 4;
        row = part <= 8 ? (part - 5) / 2 : 2 + (part - 9) / 4;
    }

    static int SelectPart(bool horizontal, bool vertical, bool diagonal,
        int all, int corner, int neither, int verticalOnly, int horizontalOnly)
    {
        if (horizontal && vertical) return diagonal ? all : corner;
        if (!horizontal && !vertical) return neither;
        return vertical ? verticalOnly : horizontalOnly;
    }

    static bool Bit(int mask, int bit) { return (mask & (1 << bit)) != 0; }

    // Mirrors TileHelper.GetAutoTileSprites, including its diagonal masks.
    static Bitmap Assemble(Bitmap sheet, int mask)
    {
        int[] parts = {
            SelectPart(Bit(mask, 3), Bit(mask, 1), Bit(mask, 0), 19, 5, 9, 17, 11),
            SelectPart(Bit(mask, 4), Bit(mask, 1), Bit(mask, 2), 18, 6, 12, 20, 10),
            SelectPart(Bit(mask, 3), Bit(mask, 6), Bit(mask, 5), 15, 7, 21, 13, 23),
            SelectPart(Bit(mask, 4), Bit(mask, 6), Bit(mask, 7), 14, 8, 24, 16, 22)
        };
        Bitmap result = NewBitmap(Tile, Tile);
        using (Graphics graphics = GraphicsFor(result))
            for (int q = 0; q < 4; q++)
            {
                int column, row;
                PartCell(parts[q], out column, out row);
                graphics.DrawImage(sheet,
                    new Rectangle((q % 2) * Quarter, (q / 2) * Quarter, Quarter, Quarter),
                    new Rectangle(column * Quarter, row * Quarter, Quarter, Quarter), GraphicsUnit.Pixel);
            }
        return result;
    }

    static void DrawLayout(Graphics graphics, Bitmap sheet, int originX, int originY, bool[,] cells)
    {
        int[] dx = {-1, 0, 1, -1, 1, -1, 0, 1};
        int[] dy = {-1, -1, -1, 0, 0, 1, 1, 1};
        for (int y = 0; y < cells.GetLength(0); y++)
            for (int x = 0; x < cells.GetLength(1); x++)
                if (cells[y, x])
                {
                    int mask = 0;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        int nx = x + dx[bit], ny = y + dy[bit];
                        if (nx >= 0 && ny >= 0 && nx < cells.GetLength(1) && ny < cells.GetLength(0)
                            && cells[ny, nx]) mask |= 1 << bit;
                    }
                    using (Bitmap tile = Assemble(sheet, mask))
                        graphics.DrawImage(tile, new Rectangle(originX + x * 128, originY + y * 128, 128, 128));
                }
    }

    public static void Preview(string sourcePath, string outputPath)
    {
        using (Bitmap sheet = new Bitmap(sourcePath))
        using (Bitmap result = NewBitmap(1152, 1152))
        using (Graphics graphics = GraphicsFor(result))
        using (Font font = new Font("Microsoft YaHei", 18))
        {
            graphics.Clear(Color.FromArgb(47, 52, 57));
            graphics.DrawString("Horizontal / vertical / closed loop (actual 8-neighbour masks)", font, Brushes.White, 24, 16);
            bool[,] line = new bool[1, 8];
            for (int x = 0; x < 8; x++) line[0, x] = true;
            DrawLayout(graphics, sheet, 64, 64, line);
            bool[,] vertical = new bool[6, 1];
            for (int y = 0; y < 6; y++) vertical[y, 0] = true;
            DrawLayout(graphics, sheet, 32, 288, vertical);
            bool[,] ring = new bool[6, 7];
            for (int y = 0; y < 6; y++)
                for (int x = 0; x < 7; x++)
                    ring[y, x] = x == 0 || y == 0 || x == 6 || y == 5;
            DrawLayout(graphics, sheet, 224, 288, ring);
            result.Save(outputPath, ImageFormat.Png);
        }
    }

    public static void Build(string sourcePath, string outputPath)
    {
        using (Bitmap source = new Bitmap(sourcePath))
        {
            Rectangle postBounds = VisibleBounds(source, new Rectangle(source.Width / 2 - 1, 0, 3, source.Height));
            Rectangle sideBounds = VisibleBounds(source, new Rectangle(0, 0, source.Width / 8, source.Height));
            Rectangle capBounds = VisibleBounds(source, new Rectangle(source.Width / 3, postBounds.Top,
                source.Width / 3, Math.Max(1, sideBounds.Top - postBounds.Top - 2)));
            Rectangle sourcePost = Rectangle.FromLTRB(capBounds.Left, postBounds.Top, capBounds.Right, postBounds.Bottom);
            // Keep one rail roughly as tall as the original full quarter-piece
            // fence; removing the duplicate row must not halve its apparent size.
            int postWidth = Quarter * 9 / 32;
            int postHeight = Quarter * 7 / 8;
            float heightScale = (float)postHeight / sourcePost.Height;
            Rectangle finalPost = new Rectangle((Tile - postWidth) / 2, (Tile - postHeight) / 2, postWidth, postHeight);

            using (Bitmap horizontal = NewBitmap(Tile, Tile))
            using (Bitmap vertical = NewBitmap(Tile, Tile))
            using (Bitmap post = NewBitmap(Tile, Tile))
            using (Bitmap sheet = NewBitmap(Quarter * 4, Quarter * 6))
            {
                using (Graphics graphics = GraphicsFor(horizontal))
                    graphics.DrawImage(source, new Rectangle(0,
                        (int)Math.Round(Tile / 2f - (sourcePost.Top + sourcePost.Height / 2f) * heightScale),
                        Tile, (int)Math.Round(source.Height * heightScale)));
                ClearRect(horizontal, new Rectangle(finalPost.Left, 0, finalPost.Width, Tile));
                using (Graphics graphics = GraphicsFor(post))
                    graphics.DrawImage(source, finalPost, sourcePost, GraphicsUnit.Pixel);
                using (Bitmap rotated = (Bitmap)horizontal.Clone())
                {
                    rotated.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    Rectangle railBounds = VisibleBounds(rotated, new Rectangle(0, 0, Tile, Tile));
                    using (Graphics graphics = GraphicsFor(vertical))
                        graphics.DrawImage(rotated, new Rectangle(finalPost.Left, 0, finalPost.Width, Tile),
                            new Rectangle(railBounds.Left, 0, railBounds.Width, Tile), GraphicsUnit.Pixel);
                }
                ClearRect(vertical, new Rectangle(0, finalPost.Top, Tile, finalPost.Height));

                // Each piece is cut from a single centre-line junction. The inner
                // pieces match the corners too; this is a line asset, never a fill.
                int[] parts = {5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24};
                int[] quadrants = {0,1,2,3,0,1,0,1,2,3,2,3,0,1,0,1,2,3,2,3};
                int[] directions = {9,3,12,6,0,2,8,0,4,6,12,4,1,3,9,1,0,2,8,0};
                using (Graphics graphics = GraphicsFor(sheet))
                    for (int i = 0; i < parts.Length; i++)
                    {
                        int column, row;
                        PartCell(parts[i], out column, out row);
                        using (Bitmap tile = BuildTile(horizontal, vertical, post, directions[i]))
                            graphics.DrawImage(tile, new Rectangle(column * Quarter, row * Quarter, Quarter, Quarter),
                                new Rectangle((quadrants[i] % 2) * Quarter, (quadrants[i] / 2) * Quarter, Quarter, Quarter),
                                GraphicsUnit.Pixel);
                    }
                sheet.Save(outputPath, ImageFormat.Png);
                Console.WriteLine("Packed 1024x1536 sheet; central post: {0}x{1} pixels.", postWidth, postHeight);
            }
        }
    }
}
'@

$resolvedSource = (Resolve-Path -LiteralPath $Source).Path
if ($PreviewOnly) {
    [FenceWangTilePacker]::Preview($resolvedSource, [IO.Path]::GetFullPath($Preview))
} else {
    [FenceWangTilePacker]::Build($resolvedSource, [IO.Path]::GetFullPath($Output))
    [FenceWangTilePacker]::Preview([IO.Path]::GetFullPath($Output), [IO.Path]::GetFullPath($Preview))
}
