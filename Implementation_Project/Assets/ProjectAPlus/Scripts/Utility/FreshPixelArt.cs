using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAPlus
{
    // Original Project A+ pixel set. No legacy production-sheet assets are used.
    public static class FreshPixelArt
    {
        public static bool Enabled { get { return true; } }

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        private static readonly Color Clear = new Color(0f, 0f, 0f, 0f);
        private static readonly Color Ink = new Color32(11, 17, 32, 255);
        private static readonly Color Deep = new Color32(22, 32, 55, 255);
        private static readonly Color Paper = new Color32(236, 225, 188, 255);
        private static readonly Color Gold = new Color32(255, 190, 66, 255);
        private static readonly Color Cyan = new Color32(67, 224, 220, 255);
        private static readonly Color Red = new Color32(239, 69, 88, 255);

        public static Sprite GetPlayerFrame(string frame)
        {
            Sprite provided = ProvidedArt.GetPlayerFrame(frame);
            if (provided != null) return provided;
            return Create("fresh_player_" + frame, 128, 128, 80f, texture =>
            {
                int stride = frame == "run_1" || frame == "run_3" ? 5 : frame.StartsWith("run_") ? -3 : 0;
                int lift = frame == "jump" ? 7 : frame == "fall" ? 3 : 0;
                int crouch = frame.StartsWith("dodge_") ? -18 : 0;
                int bodyY = 35 + lift + crouch;
                Color skin = new Color32(244, 183, 139, 255);
                Color hair = new Color32(25, 28, 45, 255);
                Color coat = new Color32(39, 70, 104, 255);
                Color coatLight = new Color32(68, 124, 151, 255);
                Color scarf = new Color32(227, 70, 79, 255);
                Color shoe = new Color32(19, 24, 38, 255);
                Color note = new Color32(241, 226, 178, 255);

                if (frame == "dead")
                {
                    Outline(texture, 28, 14, 72, 25, Ink, coat);
                    Outline(texture, 83, 18, 27, 27, Ink, skin);
                    Rect(texture, 89, 39, 18, 8, hair);
                    Rect(texture, 36, 9, 22, 9, shoe);
                    Rect(texture, 67, 9, 22, 9, shoe);
                    Rect(texture, 96, 26, 5, 3, Ink);
                    return;
                }

                int leftFoot = 46 - stride;
                int rightFoot = 69 + stride;
                Outline(texture, leftFoot, 6 + lift, 18, 34 + crouch / 2, Ink, coat);
                Outline(texture, rightFoot, 6 + lift, 18, 34 + crouch / 2, Ink, coatLight);
                Rect(texture, leftFoot - 4, 6 + lift, 24, 8, shoe);
                Rect(texture, rightFoot - 1, 6 + lift, 24, 8, shoe);

                Outline(texture, 39, bodyY, 52, 46, Ink, coat);
                Rect(texture, 44, bodyY + 6, 8, 34, coatLight);
                Rect(texture, 62, bodyY + 4, 8, 38, scarf);
                Rect(texture, 70, bodyY + 4, 4, 34, Paper);
                Rect(texture, 50, bodyY + 17, 32, 4, Deep);
                Rect(texture, 48, bodyY + 3, 11, 5, Gold);

                int armOffset = frame == "attack_1" || frame == "attack_2" ? 22 : frame.StartsWith("dodge_") ? 8 : 0;
                Outline(texture, 23 - armOffset / 2, bodyY + 10, 22 + armOffset, 18, Ink, coatLight);
                Outline(texture, 84, bodyY + 12, 20 + armOffset, 17, Ink, coat);
                Rect(texture, 25 - armOffset / 2, bodyY + 14, 8, 8, skin);
                Rect(texture, 96 + armOffset, bodyY + 15, 8, 8, skin);

                Outline(texture, 42, bodyY + 39, 46, 39, Ink, skin);
                Rect(texture, 43, bodyY + 65, 45, 14, hair);
                Rect(texture, 39, bodyY + 57, 12, 19, hair);
                Rect(texture, 80, bodyY + 57, 13, 19, hair);
                Rect(texture, 48, bodyY + 70, 12, 11, hair);
                Rect(texture, 67, bodyY + 72, 14, 9, hair);
                Rect(texture, 52, bodyY + 53, 5, 5, Ink);
                Rect(texture, 73, bodyY + 53, 5, 5, Ink);
                Rect(texture, 53, bodyY + 55, 2, 2, Cyan);
                Rect(texture, 74, bodyY + 55, 2, 2, Cyan);
                Rect(texture, 59, bodyY + 44, 13, 3, new Color32(201, 126, 111, 255));

                int bookX = frame.StartsWith("attack_") ? 91 + armOffset : 94;
                Outline(texture, bookX, bodyY + 19, 27, 31, Ink, note);
                Rect(texture, bookX + 5, bodyY + 25, 18, 3, new Color32(82, 133, 162, 255));
                Rect(texture, bookX + 5, bodyY + 34, 13, 3, Red);
                Rect(texture, bookX + 3, bodyY + 20, 4, 27, Gold);

                if (frame.StartsWith("attack_"))
                {
                    int slash = frame == "attack_0" ? 9 : frame == "attack_1" ? 18 : 28;
                    Arc(texture, 105, bodyY + 42, 24 + slash, Gold);
                    Arc(texture, 105, bodyY + 42, 20 + slash, new Color32(255, 248, 207, 255));
                }
                if (frame == "damaged")
                {
                    Rect(texture, 31, bodyY + 65, 8, 8, Red);
                    Rect(texture, 23, bodyY + 71, 5, 5, Gold);
                    Rect(texture, 91, bodyY + 71, 5, 5, Red);
                }
            }, frame);
        }

        public static Sprite GetEnemy(EnemyType type)
        {
            Sprite provided = ProvidedArt.GetEnemy(type);
            if (provided != null) return provided;
            return Create("fresh_enemy_" + type, 48, 48, 32f, texture =>
            {
                Color main = EnemyColor(type);
                Color shade = Darken(main, 0.55f);
                if (type == EnemyType.PhoneTemptation)
                {
                    Outline(texture, 12, 5, 24, 38, Ink, shade);
                    Rect(texture, 16, 12, 16, 23, main);
                    Rect(texture, 18, 16, 12, 15, new Color32(50, 224, 213, 255));
                    Rect(texture, 21, 20, 3, 3, Ink);
                    Rect(texture, 27, 20, 3, 3, Ink);
                    Rect(texture, 21, 7, 7, 3, Gold);
                }
                else if (type == EnemyType.Assignment)
                {
                    Outline(texture, 7, 5, 35, 31, Ink, Paper);
                    for (int y = 12; y < 31; y += 7) Rect(texture, 12, y, 23, 3, y == 26 ? Red : new Color32(75, 130, 157, 255));
                    Rect(texture, 10, 36, 25, 7, shade);
                    Rect(texture, 15, 17, 4, 4, Ink);
                    Rect(texture, 30, 17, 4, 4, Ink);
                    Rect(texture, 2, 10, 9, 7, main);
                    Rect(texture, 38, 10, 9, 7, main);
                }
                else if (type == EnemyType.DeadlineTimer)
                {
                    Circle(texture, 24, 23, 20, Ink);
                    Circle(texture, 24, 23, 16, Paper);
                    Rect(texture, 18, 42, 12, 5, Ink);
                    Line(texture, 24, 23, 24, 35, Red);
                    Line(texture, 24, 23, 34, 17, Red);
                    Rect(texture, 15, 27, 4, 4, Ink);
                    Rect(texture, 30, 27, 4, 4, Ink);
                }
                else if (type == EnemyType.PresentationLaser)
                {
                    Outline(texture, 5, 10, 38, 27, Ink, shade);
                    Rect(texture, 10, 15, 28, 16, main);
                    Rect(texture, 14, 20, 18, 6, new Color32(227, 241, 220, 255));
                    Rect(texture, 33, 20, 14, 7, Red);
                    Rect(texture, 15, 5, 6, 7, Deep);
                    Rect(texture, 29, 5, 6, 7, Deep);
                }
                else if (type == EnemyType.TeamProject)
                {
                    Circle(texture, 17, 24, 14, Ink);
                    Circle(texture, 32, 24, 14, Ink);
                    Circle(texture, 17, 24, 10, main);
                    Circle(texture, 32, 24, 10, shade);
                    Rect(texture, 20, 8, 9, 8, Gold);
                    Rect(texture, 13, 25, 4, 4, Ink);
                    Rect(texture, 31, 25, 4, 4, Ink);
                }
                else if (type == EnemyType.AnxietyShadow || type == EnemyType.ThoughtCloud)
                {
                    Circle(texture, 24, 24, 19, Ink);
                    Rect(texture, 7, 7, 34, 21, Ink);
                    Rect(texture, 13, 25, 6, 6, new Color32(213, 97, 255, 255));
                    Rect(texture, 30, 25, 6, 6, new Color32(213, 97, 255, 255));
                    Rect(texture, 2, 3, 12, 9, shade);
                    Rect(texture, 35, 3, 11, 9, shade);
                }
                else
                {
                    Circle(texture, 24, 18, 20, Ink);
                    Rect(texture, 5, 7, 38, 16, Ink);
                    Circle(texture, 24, 19, 16, main);
                    Rect(texture, 10, 8, 28, 14, main);
                    Rect(texture, 13, 23, 6, 7, Paper);
                    Rect(texture, 30, 23, 6, 7, Paper);
                    Rect(texture, 15, 25, 3, 3, Ink);
                    Rect(texture, 31, 25, 3, 3, Ink);
                    Rect(texture, 14, 7, 20, 5, shade);
                }
                Rect(texture, 8, 5, 8, 3, Deep);
                Rect(texture, 32, 5, 8, 3, Deep);
            });
        }

        public static Sprite GetBoss(int stage)
        {
            Sprite provided = ProvidedArt.GetBoss(stage);
            if (provided != null) return provided;
            return Create("fresh_boss_" + stage, 128, 128, 32f, texture =>
            {
                Color accent = stage == 10 ? new Color32(226, 50, 91, 255) : new Color32(252, 159, 55, 255);
                Color metal = stage == 10 ? new Color32(47, 38, 72, 255) : new Color32(55, 72, 91, 255);
                Outline(texture, 25, 19, 78, 79, Ink, metal);
                Outline(texture, 34, 32, 60, 54, Ink, Paper);
                Rect(texture, 41, 69, 46, 7, accent);
                Rect(texture, 43, 57, 34, 4, new Color32(69, 125, 158, 255));
                Rect(texture, 43, 47, 42, 4, new Color32(69, 125, 158, 255));
                Rect(texture, 45, 36, 28, 4, accent);
                Outline(texture, 35, 101, 58, 19, Ink, accent);
                Rect(texture, 48, 117, 31, 8, Ink);
                Circle(texture, 28, 83, 21, Ink);
                Circle(texture, 28, 83, 16, accent);
                Circle(texture, 28, 83, 11, Paper);
                Line(texture, 28, 83, 28, 94, Ink);
                Line(texture, 28, 83, 38, 77, Ink);
                Rect(texture, 45, 78, 12, 8, Ink);
                Rect(texture, 74, 78, 12, 8, Ink);
                Rect(texture, 49, 81, 5, 3, accent);
                Rect(texture, 78, 81, 5, 3, accent);
                Outline(texture, 3, 30, 27, 18, Ink, accent);
                Outline(texture, 98, 30, 27, 18, Ink, accent);
                Rect(texture, 8, 14, 24, 9, Deep);
                Rect(texture, 96, 14, 24, 9, Deep);
                for (int i = 0; i < 5; i++)
                {
                    Rect(texture, 8 + i * 24, 106 + (i % 2) * 8, 6, 6, Gold);
                    Rect(texture, 12 + i * 24, 112 + (i % 2) * 8, 3, 3, accent);
                }
            });
        }

        public static Sprite GetItem(string id)
        {
            return Create("fresh_item_" + id, 32, 32, 32f, texture =>
            {
                Color accent = id.Contains("coffee") ? new Color32(181, 93, 55, 255)
                    : id.Contains("headphone") ? Cyan
                    : id.Contains("final") ? new Color32(181, 90, 228, 255)
                    : id.Contains("exam") ? Red : Gold;
                if (id.Contains("coffee"))
                {
                    Outline(texture, 6, 5, 18, 21, Ink, accent);
                    Outline(texture, 22, 9, 8, 12, Ink, Paper);
                    Rect(texture, 10, 25, 10, 3, Paper);
                    Line(texture, 10, 28, 9, 31, Cyan);
                    Line(texture, 17, 28, 18, 31, Cyan);
                }
                else if (id.Contains("headphone"))
                {
                    Circle(texture, 16, 16, 13, Ink);
                    Circle(texture, 16, 16, 8, Clear);
                    Rect(texture, 3, 8, 8, 15, accent);
                    Rect(texture, 21, 8, 8, 15, accent);
                }
                else
                {
                    Outline(texture, 6, 4, 21, 25, Ink, Paper);
                    Rect(texture, 10, 22, 13, 4, accent);
                    Rect(texture, 10, 15, 11, 3, new Color32(72, 128, 158, 255));
                    Rect(texture, 10, 9, 14, 3, Red);
                    Rect(texture, 7, 5, 4, 23, accent);
                }
            });
        }

        public static Sprite GetPlatform(string style, Color color)
        {
            Sprite provided = ProvidedArt.GetPlatform(style);
            if (provided != null) return provided;
            bool oneWay = style.EndsWith("_oneway");
            string baseStyle = style.Replace("_oneway", "").Replace("_ground", "");
            return Create("fresh_platform_" + style + color, 32, 32, 32f, texture =>
            {
                Color dark = Darken(color, 0.42f);
                Color mid = Lighten(color, 1.12f);
                Color top = oneWay ? Cyan : Gold;
                Rect(texture, 0, 0, 32, 32, dark);
                Rect(texture, 0, oneWay ? 17 : 4, 32, oneWay ? 15 : 28, color);
                Rect(texture, 0, 27, 32, 5, top);
                Rect(texture, 0, 24, 32, 3, Ink);
                Rect(texture, 2, 22, 28, 2, mid);
                for (int y = 5; y < 21; y += 8)
                {
                    int offset = (y / 8) % 2 == 0 ? 0 : 7;
                    for (int x = -offset; x < 32; x += 14)
                    {
                        Rect(texture, x, y, 12, 5, color);
                        Rect(texture, x, y, 12, 2, mid);
                        Rect(texture, x + 10, y, 2, 5, Ink);
                    }
                }
                if (baseStyle == "library")
                    for (int x = 3; x < 30; x += 6) Rect(texture, x, 7, 4, 13, x % 12 == 3 ? Red : new Color32(70, 137, 167, 255));
                if (baseStyle == "presentation" || baseStyle == "final") Rect(texture, 4, 13, 24, 4, Red);
                if (baseStyle == "meeting") Rect(texture, 8, 8, 16, 5, Gold);
            });
        }

        public static Sprite GetPlatformSupport(string style, Color color)
        {
            Sprite provided = ProvidedArt.GetPlatformSupport(style);
            if (provided != null) return provided;
            return Create("fresh_support_" + style + color, 16, 32, 32f, texture =>
            {
                Rect(texture, 2, 0, 12, 32, Ink);
                Rect(texture, 5, 0, 6, 32, color);
                for (int y = 3; y < 30; y += 8)
                {
                    Line(texture, 3, y, 12, y + 6, Gold);
                    Line(texture, 12, y, 3, y + 6, Gold);
                }
            });
        }

        public static Sprite GetProp(string kind, Color color)
        {
            Sprite provided = ProvidedArt.GetProp(kind);
            if (provided != null) return provided;
            return Create("fresh_prop_" + kind + color, 48, 48, 32f, texture =>
            {
                Color shade = Darken(color, 0.48f);
                if (kind == "books")
                {
                    Outline(texture, 4, 4, 40, 40, Ink, shade);
                    for (int shelf = 0; shelf < 3; shelf++)
                    {
                        Rect(texture, 7, 10 + shelf * 11, 34, 3, Gold);
                        for (int x = 8; x < 39; x += 6) Rect(texture, x, 13 + shelf * 11, 4, 8, x % 12 == 2 ? Red : Cyan);
                    }
                }
                else if (kind == "lamp")
                {
                    Rect(texture, 21, 5, 6, 24, Ink);
                    Outline(texture, 10, 26, 28, 17, Ink, shade);
                    Rect(texture, 15, 29, 18, 10, Gold);
                    Rect(texture, 20, 33, 8, 6, Paper);
                }
                else if (kind == "desk")
                {
                    Outline(texture, 4, 15, 40, 16, Ink, color);
                    Rect(texture, 8, 5, 7, 15, shade);
                    Rect(texture, 33, 5, 7, 15, shade);
                    Rect(texture, 2, 29, 44, 5, Gold);
                    Rect(texture, 12, 34, 20, 5, Paper);
                }
                else if (kind == "banner")
                {
                    Outline(texture, 12, 4, 24, 40, Ink, color);
                    Rect(texture, 16, 34, 16, 5, Gold);
                    Line(texture, 18, 12, 30, 31, Paper);
                    Line(texture, 30, 12, 18, 31, Paper);
                }
                else if (kind == "arch")
                {
                    Rect(texture, 3, 3, 10, 36, Ink);
                    Rect(texture, 35, 3, 10, 36, Ink);
                    Circle(texture, 24, 32, 21, Ink);
                    Circle(texture, 24, 30, 14, Clear);
                    Rect(texture, 8, 6, 5, 29, color);
                    Rect(texture, 35, 6, 5, 29, color);
                }
                else if (kind == "launch")
                {
                    Outline(texture, 4, 9, 40, 22, Ink, shade);
                    Rect(texture, 7, 23, 34, 6, Cyan);
                    for (int x = 9; x < 39; x += 8) Line(texture, x, 13, x + 5, 23, Gold);
                }
                else
                {
                    Outline(texture, 4, 5, 40, 37, Ink, color);
                    Rect(texture, 8, 9, 32, 26, shade);
                    Rect(texture, 21, 8, 5, 29, Gold);
                    Rect(texture, 8, 21, 32, 5, Gold);
                }
            });
        }

        public static Sprite GetGate()
        {
            return Create("fresh_gate", 24, 48, 32f, texture =>
            {
                Rect(texture, 1, 0, 5, 48, Ink);
                Rect(texture, 18, 0, 5, 48, Ink);
                Rect(texture, 5, 2, 14, 44, new Color32(61, 41, 103, 220));
                for (int y = 4; y < 45; y += 8) Rect(texture, 7, y, 10, 4, Cyan);
                Rect(texture, 0, 43, 24, 5, Gold);
            });
        }

        public static Sprite GetSlash()
        {
            return Create("fresh_slash", 64, 48, 32f, texture =>
            {
                for (int i = 0; i < 36; i++)
                {
                    int y = 7 + Mathf.RoundToInt(Mathf.Sin(i / 35f * Mathf.PI) * 26f);
                    Rect(texture, 8 + i, y, 8, 4, Gold);
                    Rect(texture, 12 + i, y + 2, 6, 3, Paper);
                }
            });
        }

        public static Sprite GetProjectile(Color color)
        {
            return Create("fresh_projectile_" + color, 16, 16, 32f, texture =>
            {
                Circle(texture, 8, 8, 7, Ink);
                Circle(texture, 8, 8, 5, color);
                Circle(texture, 7, 10, 2, Paper);
            });
        }

        public static Sprite GetHitSpark(bool critical)
        {
            return Create(critical ? "fresh_hit_critical" : "fresh_hit", 32, 32, 32f, texture =>
            {
                Color glow = critical ? Red : Gold;
                for (int i = 3; i < 15; i++)
                {
                    Rect(texture, 15, 16 + i, 3, 2, glow);
                    Rect(texture, 15, 14 - i, 3, 2, glow);
                    Rect(texture, 16 + i, 15, 2, 3, glow);
                    Rect(texture, 14 - i, 15, 2, 3, glow);
                }
                Circle(texture, 16, 16, 5, Paper);
            });
        }

        public static Sprite GetSpike()
        {
            return Create("fresh_warning", 32, 16, 32f, texture =>
            {
                Rect(texture, 0, 0, 32, 4, Ink);
                for (int x = 0; x < 32; x += 8)
                    for (int y = 0; y < 12; y++) Rect(texture, x + 4 - y / 3, 4 + y, 1 + y / 2, 1, y > 8 ? Gold : Red);
            });
        }

        public static Sprite GetUiFrame() { return NineSlice("fresh_ui_frame", Gold, new Color32(56, 94, 119, 255), new Color32(10, 18, 33, 245)); }
        public static Sprite GetUiButton() { return NineSlice("fresh_ui_button", Cyan, new Color32(42, 81, 108, 255), new Color32(15, 27, 45, 255)); }
        public static Sprite GetUiSlot() { return NineSlice("fresh_ui_slot", new Color32(183, 101, 69, 255), new Color32(64, 56, 86, 255), new Color32(9, 16, 29, 255)); }

        private static Color EnemyColor(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.SleepSlime: return new Color32(97, 112, 190, 255);
                case EnemyType.PhoneTemptation: return new Color32(51, 207, 195, 255);
                case EnemyType.Assignment: return new Color32(205, 177, 113, 255);
                case EnemyType.TeamProject: return new Color32(170, 85, 177, 255);
                case EnemyType.PresentationLaser: return new Color32(224, 71, 86, 255);
                case EnemyType.DeadlineTimer: return new Color32(241, 151, 52, 255);
                default: return new Color32(74, 55, 112, 255);
            }
        }

        private static Sprite Create(string key, int width, int height, float ppu, Action<Texture2D> paint, string spriteName = null)
        {
            Sprite cached;
            if (Cache.TryGetValue(key, out cached)) return cached;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(new Color[width * height]);
            paint(texture);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);
            sprite.name = string.IsNullOrEmpty(spriteName) ? key : spriteName;
            Cache[key] = sprite;
            return sprite;
        }

        private static Sprite NineSlice(string key, Color edge, Color bevel, Color center)
        {
            Sprite cached;
            if (Cache.TryGetValue(key, out cached)) return cached;
            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            Rect(texture, 0, 0, 16, 16, center);
            Rect(texture, 0, 0, 16, 2, Ink);
            Rect(texture, 0, 14, 16, 2, Ink);
            Rect(texture, 0, 0, 2, 16, Ink);
            Rect(texture, 14, 0, 2, 16, Ink);
            Rect(texture, 2, 2, 12, 2, edge);
            Rect(texture, 2, 12, 12, 2, edge);
            Rect(texture, 2, 2, 2, 12, edge);
            Rect(texture, 12, 2, 2, 12, edge);
            Rect(texture, 4, 4, 8, 2, bevel);
            Rect(texture, 4, 10, 8, 2, bevel);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect, new Vector4(5, 5, 5, 5));
            sprite.name = key;
            Cache[key] = sprite;
            return sprite;
        }

        private static void Rect(Texture2D texture, int x, int y, int width, int height, Color color)
        {
            for (int py = y; py < y + height; py++)
            for (int px = x; px < x + width; px++)
                if (px >= 0 && py >= 0 && px < texture.width && py < texture.height) texture.SetPixel(px, py, color);
        }

        private static void Outline(Texture2D texture, int x, int y, int width, int height, Color outline, Color fill)
        {
            Rect(texture, x, y, width, height, outline);
            Rect(texture, x + 3, y + 3, width - 6, height - 6, fill);
        }

        private static void Circle(Texture2D texture, int cx, int cy, int radius, Color color)
        {
            for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
                if (x * x + y * y <= radius * radius) Rect(texture, cx + x, cy + y, 1, 1, color);
        }

        private static void Arc(Texture2D texture, int cx, int cy, int radius, Color color)
        {
            for (int i = -38; i <= 42; i++)
            {
                float radians = i * Mathf.Deg2Rad;
                int x = cx + Mathf.RoundToInt(Mathf.Cos(radians) * radius);
                int y = cy + Mathf.RoundToInt(Mathf.Sin(radians) * radius);
                Rect(texture, x, y, 4, 4, color);
            }
        }

        private static void Line(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;
            while (true)
            {
                Rect(texture, x0, y0, 2, 2, color);
                if (x0 == x1 && y0 == y1) break;
                int twice = 2 * error;
                if (twice >= dy) { error += dy; x0 += sx; }
                if (twice <= dx) { error += dx; y0 += sy; }
            }
        }

        private static Color Darken(Color color, float amount) { return new Color(color.r * amount, color.g * amount, color.b * amount, color.a); }
        private static Color Lighten(Color color, float amount) { return new Color(Mathf.Clamp01(color.r * amount), Mathf.Clamp01(color.g * amount), Mathf.Clamp01(color.b * amount), color.a); }
    }
}
