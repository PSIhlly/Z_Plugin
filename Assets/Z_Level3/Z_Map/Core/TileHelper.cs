using System;
using System.Collections.Generic;
using UnityEngine;
using Z_Texture;

namespace Z_Map
{
    /// <summary>
    /// Creates the 256 neighbour-mask sprites for a standard RPG Maker autotile sheet.
    /// The sheet is a 4 by 6 grid of quarter-tile pieces; pieces 1 to 4 are unused.
    /// </summary>
    public static class TileHelper
    {
        // Bit layout, read left-to-right and top-to-bottom around the centre tile:
        // 0 1 2
        // 3   4
        // 5 6 7
        private const int TopLeftBit = 0;
        private const int TopBit = 1;
        private const int TopRightBit = 2;
        private const int LeftBit = 3;
        private const int RightBit = 4;
        private const int BottomLeftBit = 5;
        private const int BottomBit = 6;
        private const int BottomRightBit = 7;

        /// <summary>
        /// Builds sprites for every 8-neighbour mask. A set bit means that neighbour
        /// uses the same tile texture as the centre. Equivalent masks share one of
        /// the 47 generated sprites.
        /// </summary>
        public static Dictionary<int, Sprite> GetAutoTileSprites(Texture2D texture)
        {
            Validate(texture);

            int partSize = texture.width / 4;
            Dictionary<int, Texture2D> parts = CreateParts(texture, partSize);
            Dictionary<string, Sprite> spriteByParts = new Dictionary<string, Sprite>();
            Dictionary<int, Sprite> spritesByMask = new Dictionary<int, Sprite>(256);

            try
            {
                for (int mask = 0; mask < 256; mask++)
                {
                    int topLeft = GetPartId(
                        IsSame(mask, LeftBit), IsSame(mask, TopBit), IsSame(mask, TopLeftBit),
                        19, 5, 9, 17, 11);
                    int topRight = GetPartId(
                        IsSame(mask, RightBit), IsSame(mask, TopBit), IsSame(mask, TopRightBit),
                        18, 6, 12, 20, 10);
                    int bottomLeft = GetPartId(
                        IsSame(mask, LeftBit), IsSame(mask, BottomBit), IsSame(mask, BottomLeftBit),
                        15, 7, 21, 13, 23);
                    int bottomRight = GetPartId(
                        IsSame(mask, RightBit), IsSame(mask, BottomBit), IsSame(mask, BottomRightBit),
                        14, 8, 24, 16, 22);

                    string partKey = $"{topLeft},{topRight},{bottomLeft},{bottomRight}";
                    if (!spriteByParts.TryGetValue(partKey, out Sprite sprite))
                    {
                        // TextureCombine lays out its input from bottom-left. The source
                        // parts are therefore passed in BL, BR, TL, TR order.
                        Texture2D tileTexture = TextureCombine.FillTexture2DsToTexture2D(
                            new[] { parts[bottomLeft], parts[bottomRight], parts[topLeft], parts[topRight] },
                            2,
                            2);
                        tileTexture.name = $"{texture.name}_AutoTile_{partKey}";
                        tileTexture.filterMode = texture.filterMode;
                        tileTexture.wrapMode = TextureWrapMode.Clamp;

                        // One full generated tile occupies one world unit, independent
                        // of the source sheet's concrete pixel dimensions.
                        sprite = Sprite.Create(
                            tileTexture,
                            new Rect(0, 0, tileTexture.width, tileTexture.height),
                            new Vector2(0.5f, 0.5f),
                            tileTexture.width);
                        sprite.name = tileTexture.name;
                        spriteByParts.Add(partKey, sprite);
                    }

                    spritesByMask.Add(mask, sprite);
                }
            }
            finally
            {
                foreach (Texture2D part in parts.Values)
                    UnityEngine.Object.Destroy(part);
            }

            return spritesByMask;
        }

        private static bool IsSame(int mask, int bit)
        {
            return (mask & (1 << bit)) != 0;
        }

        private static int GetPartId(
            bool horizontalSame,
            bool verticalSame,
            bool diagonalSame,
            int allSame,
            int diagonalDifferent,
            int neitherSame,
            int verticalOnly,
            int horizontalOnly)
        {
            if (horizontalSame && verticalSame)
                return diagonalSame ? allSame : diagonalDifferent;
            if (!horizontalSame && !verticalSame)
                return neitherSame;
            return verticalSame ? verticalOnly : horizontalOnly;
        }

        private static Dictionary<int, Texture2D> CreateParts(Texture2D texture, int partSize)
        {
            Dictionary<int, Texture2D> parts = new Dictionary<int, Texture2D>(20);
            for (int partId = 5; partId <= 24; partId++)
            {
                int rowFromTop;
                int column;
                if (partId <= 8)
                {
                    rowFromTop = (partId - 5) / 2;
                    column = 2 + (partId - 5) % 2;
                }
                else
                {
                    rowFromTop = 2 + (partId - 9) / 4;
                    column = (partId - 9) % 4;
                }

                Texture2D part = new Texture2D(partSize, partSize, TextureFormat.RGBA32, false)
                {
                    name = $"{texture.name}_AutoTilePart_{partId}",
                    filterMode = texture.filterMode,
                    wrapMode = TextureWrapMode.Clamp
                };
                part.SetPixels(texture.GetPixels(
                    column * partSize,
                    texture.height - (rowFromTop + 1) * partSize,
                    partSize,
                    partSize));
                part.Apply();
                parts.Add(partId, part);
            }

            return parts;
        }

        private static void Validate(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            if (!texture.isReadable)
                throw new ArgumentException("The RPG Maker autotile texture must have Read/Write Enabled.", nameof(texture));
            if (texture.width % 4 != 0 || texture.height % 6 != 0)
                throw new ArgumentException("The RPG Maker autotile texture must be divisible into a 4 by 6 grid.", nameof(texture));
            if (texture.width / 4 != texture.height / 6)
                throw new ArgumentException("Every RPG Maker autotile source cell must be square.", nameof(texture));
        }
    }
}
