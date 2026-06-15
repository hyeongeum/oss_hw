#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectAPlus.Editor
{
    public static class ProvidedArtSheetImporter
    {
        private const string Source = "Assets/ProjectAPlus/ArtSource/ProvidedSheets/";
        private const string Output = "Assets/ProjectAPlus/Resources/ProvidedArt/";

        private struct Slice
        {
            public string sheet;
            public string output;
            public RectInt rect;
            public int width;
            public int height;
            public int ppu;
            public bool bottomAlign;

            public Slice(string source, string path, int x, int y, int w, int h, int targetW, int targetH, int pixelsPerUnit, bool alignBottom = true)
            {
                sheet = source;
                output = path;
                rect = new RectInt(x, y, w, h);
                width = targetW;
                height = targetH;
                ppu = pixelsPerUnit;
                bottomAlign = alignBottom;
            }
        }

        [MenuItem("Tools/Project A+/Import Provided Art Sheets")]
        public static void Generate()
        {
            Directory.CreateDirectory(Output + "Player");
            Directory.CreateDirectory(Output + "Enemies");
            Directory.CreateDirectory(Output + "Bosses");
            Directory.CreateDirectory(Output + "Terrain");
            Directory.CreateDirectory(Output + "Props");

            var slices = new List<Slice>
            {
                // Player sheet: fixed 128x128 canvases prevent animation scale changes.
                new Slice("PlayerSheet.png", "Player/idle_0.png", 214, 18, 110, 125, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/idle_1.png", 382, 18, 110, 125, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/run_0.png", 210, 142, 120, 145, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/run_1.png", 375, 142, 120, 145, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/run_2.png", 540, 142, 120, 145, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/run_3.png", 705, 142, 120, 145, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/jump.png", 545, 286, 130, 145, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/fall.png", 382, 425, 130, 145, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/dodge_0.png", 380, 562, 135, 145, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/dodge_1.png", 545, 562, 135, 145, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/attack_0.png", 195, 725, 190, 145, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/attack_1.png", 405, 725, 220, 145, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/attack_2.png", 630, 725, 225, 145, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/damaged.png", 200, 868, 145, 180, 128, 128, 80),
                new Slice("PlayerSheet.png", "Player/dead.png", 575, 868, 155, 180, 128, 128, 80),

                // Normal enemies: one readable idle pose per archetype.
                // Coordinates calibrated to the actual sheet rows so crops do not bleed into
                // neighboring frames or row-label text.
                new Slice("EnemySheet.png", "Enemies/fresh_enemy_SleepSlime.png", 220, 120, 104, 110, 96, 96, 64),
                new Slice("EnemySheet.png", "Enemies/fresh_enemy_Assignment.png", 214, 288, 108, 122, 96, 96, 64),
                new Slice("EnemySheet.png", "Enemies/fresh_enemy_AnxietyShadow.png", 216, 466, 98, 100, 96, 96, 64),
                new Slice("EnemySheet.png", "Enemies/fresh_enemy_ThoughtCloud.png", 216, 466, 98, 100, 96, 96, 64),
                new Slice("EnemySheet.png", "Enemies/fresh_enemy_PhoneTemptation.png", 228, 619, 80, 107, 96, 96, 64),
                new Slice("EnemySheet.png", "Enemies/fresh_enemy_DeadlineTimer.png", 229, 777, 90, 99, 96, 96, 64),
                new Slice("EnemySheet.png", "Enemies/fresh_enemy_TeamProject.png", 211, 926, 104, 122, 96, 96, 64),
                new Slice("MidBossSheet.png", "Enemies/fresh_enemy_PresentationLaser.png", 172, 487, 135, 160, 96, 96, 64),
                new Slice("EnemySheet.png", "Enemies/fresh_enemy_Generic.png", 229, 777, 90, 99, 96, 96, 64),

                // Bosses.
                new Slice("MidBossSheet.png", "Bosses/fresh_boss_5.png", 20, 105, 185, 225, 256, 256, 72),
                new Slice("BossSheet.png", "Bosses/fresh_boss_10.png", 14, 205, 330, 345, 256, 256, 72),

                // Platform strips. These are stretched to a clean, repeatable full rectangle.
                new Slice("TerrainSheet.png", "Terrain/classroom_ground.png", 945, 292, 230, 68, 128, 64, 64, false),
                new Slice("TerrainSheet.png", "Terrain/classroom_oneway.png", 945, 36, 230, 70, 128, 64, 64, false),
                new Slice("TerrainSheet.png", "Terrain/meeting_ground.png", 945, 375, 230, 70, 128, 64, 64, false),
                new Slice("TerrainSheet.png", "Terrain/meeting_oneway.png", 945, 620, 230, 70, 128, 64, 64, false),
                new Slice("TerrainSheet.png", "Terrain/presentation_ground.png", 945, 485, 230, 70, 128, 64, 64, false),
                new Slice("TerrainSheet.png", "Terrain/presentation_oneway.png", 945, 553, 230, 55, 128, 64, 64, false),
                new Slice("TerrainSheet.png", "Terrain/library_ground.png", 945, 810, 170, 125, 128, 64, 64, false),
                new Slice("TerrainSheet.png", "Terrain/library_oneway.png", 1205, 810, 160, 125, 128, 64, 64, false),
                new Slice("BuildingSheet.png", "Terrain/exam_ground.png", 970, 150, 165, 83, 128, 64, 64, false),
                new Slice("BuildingSheet.png", "Terrain/exam_oneway.png", 970, 410, 185, 72, 128, 64, 64, false),
                new Slice("BuildingSheet.png", "Terrain/final_ground.png", 1310, 150, 180, 83, 128, 64, 64, false),
                new Slice("BuildingSheet.png", "Terrain/final_oneway.png", 1210, 410, 190, 72, 128, 64, 64, false),
                new Slice("BuildingSheet.png", "Terrain/boundary_ground.png", 970, 238, 70, 165, 64, 128, 64, false),
                new Slice("BuildingSheet.png", "Terrain/support_stone.png", 1030, 238, 70, 165, 64, 128, 64, false),
                new Slice("BuildingSheet.png", "Terrain/support_dark.png", 1410, 238, 70, 165, 64, 128, 64, false),

                // Campus props used by stage decoration.
                new Slice("PropSheet.png", "Props/desk.png", 30, 45, 130, 105, 128, 96, 64),
                new Slice("PropSheet.png", "Props/lamp.png", 1060, 35, 100, 185, 96, 128, 64),
                new Slice("PropSheet.png", "Props/books.png", 275, 145, 135, 120, 96, 96, 64),
                new Slice("BuildingSheet.png", "Props/banner.png", 15, 625, 225, 115, 96, 128, 64),
                new Slice("BuildingSheet.png", "Props/arch.png", 770, 575, 250, 155, 160, 128, 64),
                new Slice("PropSheet.png", "Props/launch.png", 1420, 650, 180, 90, 128, 64, 64),
                new Slice("PropSheet.png", "Props/window.png", 865, 70, 220, 180, 128, 128, 64),
                new Slice("BuildingSheet.png", "Props/board.png", 20, 560, 350, 80, 160, 64, 64, false)
            };

            foreach (Slice slice in slices) Export(slice);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            bool valid = ValidateGeneratedArt();
            Debug.Log("Project A+: provided entity and terrain sheets imported. validation=" + valid);
        }

        private static void Export(Slice slice)
        {
            string sourcePath = Source + slice.sheet;
            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning("Missing provided art source: " + sourcePath);
                return;
            }

            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            source.LoadImage(File.ReadAllBytes(sourcePath), false);
            Color32[] crop = CropFromTop(source, slice.rect);
            // Border-based removal only: flood-fill the background that is connected to the
            // crop edges and clear it. Interior light/white pixels (exam paper, quiz sheet,
            // "DUE" box, highlights) are NOT touched because they are not reachable from the border.
            RemoveConnectedCheckerboard(crop, slice.rect.width, slice.rect.height);
            int safePadding = SafePadding(slice.output);
            Color32[] normalized = Normalize(crop, slice.rect.width, slice.rect.height, slice.width, slice.height, slice.bottomAlign, safePadding);

            var output = new Texture2D(slice.width, slice.height, TextureFormat.RGBA32, false);
            output.SetPixels32(normalized);
            output.filterMode = FilterMode.Point;
            output.Apply();
            string outputPath = Output + slice.output;
            File.WriteAllBytes(outputPath, output.EncodeToPNG());
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(output);
            ImportSprite(outputPath, slice.ppu, IsTerrain(slice.output));
        }

        private static Color32[] CropFromTop(Texture2D source, RectInt rect)
        {
            int width = Mathf.Clamp(rect.width, 1, source.width - Mathf.Clamp(rect.x, 0, source.width - 1));
            int height = Mathf.Clamp(rect.height, 1, source.height - Mathf.Clamp(rect.y, 0, source.height - 1));
            int startX = Mathf.Clamp(rect.x, 0, source.width - width);
            int startY = source.height - Mathf.Clamp(rect.y + height, height, source.height);
            Color32[] allPixels = source.GetPixels32();
            var crop = new Color32[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                crop[x + y * width] = allPixels[(startX + x) + (startY + y) * source.width];
            return crop;
        }

        private static void RemoveConnectedCheckerboard(Color32[] pixels, int width, int height)
        {
            var queue = new Queue<int>();
            var visited = new bool[pixels.Length];
            for (int x = 0; x < width; x++)
            {
                EnqueueBackground(x, 0, width, height, pixels, visited, queue);
                EnqueueBackground(x, height - 1, width, height, pixels, visited, queue);
            }
            for (int y = 0; y < height; y++)
            {
                EnqueueBackground(0, y, width, height, pixels, visited, queue);
                EnqueueBackground(width - 1, y, width, height, pixels, visited, queue);
            }

            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                int x = index % width;
                int y = index / width;
                Color32 pixel = pixels[index];
                pixel.a = 0;
                pixels[index] = pixel;
                EnqueueBackground(x - 1, y, width, height, pixels, visited, queue);
                EnqueueBackground(x + 1, y, width, height, pixels, visited, queue);
                EnqueueBackground(x, y - 1, width, height, pixels, visited, queue);
                EnqueueBackground(x, y + 1, width, height, pixels, visited, queue);
            }
        }

        private static void EnqueueBackground(int x, int y, int width, int height, Color32[] pixels, bool[] visited, Queue<int> queue)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return;
            int index = x + y * width;
            if (visited[index]) return;
            visited[index] = true;
            if (!IsCheckerboard(pixels[index])) return;
            queue.Enqueue(index);
        }

        private static bool IsCheckerboard(Color32 color)
        {
            int maximum = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            int minimum = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
            return minimum >= 226 && maximum - minimum <= 18;
        }

        private static Color32[] Normalize(Color32[] source, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, bool bottomAlign, int safePadding)
        {
            BoundsInt bounds = VisibleBounds(source, sourceWidth, sourceHeight);
            int contentWidth = Mathf.Max(1, bounds.size.x);
            int contentHeight = Mathf.Max(1, bounds.size.y);
            int padding = Mathf.Clamp(safePadding, 0, Mathf.Min(targetWidth, targetHeight) / 3);
            float scale = Mathf.Min((targetWidth - padding * 2f) / contentWidth, (targetHeight - padding * 2f) / contentHeight);
            int scaledWidth = Mathf.Max(1, Mathf.RoundToInt(contentWidth * scale));
            int scaledHeight = Mathf.Max(1, Mathf.RoundToInt(contentHeight * scale));
            int offsetX = (targetWidth - scaledWidth) / 2;
            int offsetY = bottomAlign ? padding : (targetHeight - scaledHeight) / 2;
            var target = new Color32[targetWidth * targetHeight];

            for (int y = 0; y < scaledHeight; y++)
            for (int x = 0; x < scaledWidth; x++)
            {
                int sourceX = bounds.min.x + Mathf.Clamp(Mathf.FloorToInt(x / scale), 0, contentWidth - 1);
                int sourceY = bounds.min.y + Mathf.Clamp(Mathf.FloorToInt(y / scale), 0, contentHeight - 1);
                target[(offsetX + x) + (offsetY + y) * targetWidth] = source[sourceX + sourceY * sourceWidth];
            }
            return target;
        }

        private static BoundsInt VisibleBounds(Color32[] pixels, int width, int height)
        {
            int minX = width;
            int minY = height;
            int maxX = 0;
            int maxY = 0;
            bool found = false;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (pixels[x + y * width].a < 8) continue;
                found = true;
                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
            return found ? new BoundsInt(minX, minY, 0, maxX - minX + 1, maxY - minY + 1, 1) : new BoundsInt(0, 0, 0, width, height, 1);
        }

        private static int SafePadding(string output)
        {
            if (IsTerrain(output)) return 0;
            if (output.StartsWith("Bosses/")) return 24;
            if (output.StartsWith("Player/")) return 12;
            if (output.StartsWith("Enemies/")) return 10;
            return 6;
        }

        private static bool IsTerrain(string output)
        {
            return output.StartsWith("Terrain/");
        }

        public static bool ValidateGeneratedArt()
        {
            bool valid = true;
            int checkedCount = 0;
            foreach (string file in Directory.GetFiles(Output, "*.png", SearchOption.AllDirectories))
            {
                string path = file.Replace('\\', '/');
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                TextureImporterPlatformSettings standalone = importer != null
                    ? importer.GetPlatformTextureSettings("Standalone")
                    : new TextureImporterPlatformSettings();
                bool importValid = importer != null
                    && importer.filterMode == FilterMode.Point
                    && !importer.mipmapEnabled
                    && importer.textureCompression == TextureImporterCompression.Uncompressed
                    && standalone.overridden
                    && standalone.format == TextureImporterFormat.RGBA32
                    && standalone.textureCompression == TextureImporterCompression.Uncompressed;
                bool borderValid = path.Contains("/Terrain/") || HasClearBorder(file);
                if (!importValid || !borderValid)
                {
                    valid = false;
                    Debug.LogError("Project A+ art validation failed: " + path + ", import=" + importValid + ", clearBorder=" + borderValid);
                }
                checkedCount++;
            }
            Debug.Log("Project A+ provided art audit: " + valid + ", checked=" + checkedCount);
            return valid && checkedCount > 0;
        }

        private static bool HasClearBorder(string path)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(path), false);
            Color32[] pixels = texture.GetPixels32();
            int width = texture.width;
            int height = texture.height;
            bool clear = true;
            for (int x = 0; x < width && clear; x++)
                clear = pixels[x].a < 8 && pixels[x + (height - 1) * width].a < 8;
            for (int y = 0; y < height && clear; y++)
                clear = pixels[y * width].a < 8 && pixels[(width - 1) + y * width].a < 8;
            Object.DestroyImmediate(texture);
            return clear;
        }

        private static void ImportSprite(string path, int pixelsPerUnit, bool repeat)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.maxTextureSize = 4096;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteExtrude = 0;
            importer.SetTextureSettings(settings);
            TextureImporterPlatformSettings standalone = importer.GetPlatformTextureSettings("Standalone");
            standalone.overridden = true;
            standalone.maxTextureSize = 4096;
            standalone.format = TextureImporterFormat.RGBA32;
            standalone.textureCompression = TextureImporterCompression.Uncompressed;
            standalone.crunchedCompression = false;
            importer.SetPlatformTextureSettings(standalone);
            importer.SaveAndReimport();
        }
    }
}
#endif
