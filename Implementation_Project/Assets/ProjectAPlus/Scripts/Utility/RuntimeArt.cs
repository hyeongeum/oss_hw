using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAPlus
{
    // Small procedural pixel-art library used by the runtime-built game.
    public static class RuntimeArt
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        private static readonly Color Ink = new Color32(15, 13, 24, 255);
        private static readonly Color Paper = new Color32(224, 214, 184, 255);
        private static readonly Color Highlight = new Color32(255, 236, 195, 255);

        public static Sprite GetPlayer()
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetPlayerFrame("idle_0");
            Sprite production = GetPlayerFrame("idle_0");
            if (production != null) return production;
            return Create("player_student_dark_academia_v5", 32, 32, texture =>
            {
                Color skin = new Color32(255, 205, 159, 255);
                Color skinShade = new Color32(220, 151, 121, 255);
                Color hair = new Color32(24, 21, 32, 255);
                Color hairLight = new Color32(73, 57, 68, 255);
                Color jacket = new Color32(54, 49, 67, 255);
                Color jacketShade = new Color32(31, 27, 43, 255);
                Color shirt = new Color32(132, 43, 55, 255);
                Color bag = new Color32(123, 79, 47, 255);
                Rect(texture, 7, 2, 7, 4, Ink);
                Rect(texture, 19, 2, 7, 4, Ink);
                Rect(texture, 8, 4, 5, 2, new Color32(80, 99, 125, 255));
                Rect(texture, 20, 4, 5, 2, new Color32(80, 99, 125, 255));
                Rect(texture, 8, 6, 7, 7, Ink);
                Rect(texture, 18, 6, 7, 7, Ink);
                Rect(texture, 9, 7, 5, 6, new Color32(42, 60, 87, 255));
                Rect(texture, 19, 7, 5, 6, new Color32(42, 60, 87, 255));
                OutlineRect(texture, 7, 11, 19, 11, Ink, jacket);
                Rect(texture, 8, 12, 5, 8, jacketShade);
                Rect(texture, 14, 13, 6, 8, shirt);
                Rect(texture, 16, 13, 2, 8, new Color32(218, 225, 216, 255));
                Rect(texture, 5, 12, 4, 9, Ink);
                Rect(texture, 6, 13, 3, 7, jacketShade);
                Rect(texture, 25, 12, 4, 9, Ink);
                Rect(texture, 25, 13, 3, 7, jacket);
                Rect(texture, 27, 11, 3, 3, skin);
                OutlineRect(texture, 23, 9, 7, 11, Ink, bag);
                Rect(texture, 25, 11, 3, 7, new Color32(204, 117, 45, 255));
                Rect(texture, 24, 17, 5, 2, new Color32(255, 207, 79, 255));
                OutlineRect(texture, 9, 20, 16, 10, Ink, skin);
                Rect(texture, 10, 20, 3, 8, skinShade);
                Rect(texture, 8, 26, 18, 5, hair);
                Rect(texture, 8, 23, 3, 5, hair);
                Rect(texture, 23, 23, 3, 5, hair);
                Rect(texture, 11, 28, 9, 2, hairLight);
                Rect(texture, 13, 24, 2, 2, Ink);
                Rect(texture, 20, 24, 2, 2, Ink);
                Rect(texture, 14, 25, 1, 1, Highlight);
                Rect(texture, 20, 25, 1, 1, Highlight);
                Rect(texture, 16, 21, 5, 2, skinShade);
                Rect(texture, 6, 13, 2, 2, Highlight);
                Line(texture, 28, 14, 31, 8, Ink);
                Line(texture, 29, 14, 31, 9, new Color32(225, 72, 69, 255));
                Rect(texture, 30, 7, 2, 2, Highlight);
                OutlineRect(texture, 2, 13, 5, 7, Ink, Paper);
                Rect(texture, 3, 17, 3, 1, new Color32(68, 126, 167, 255));
                Rect(texture, 3, 15, 2, 1, new Color32(68, 126, 167, 255));
            }, 21.333f);
        }

        public static Sprite GetEnemy(EnemyType type)
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetEnemy(type);
            string productionName = type == EnemyType.SleepSlime ? "SleepSlime" :
                type == EnemyType.PhoneTemptation ? "PhoneTemptation" :
                type == EnemyType.Assignment ? "AssignmentMonster" :
                type == EnemyType.TeamProject ? "TeamProjectMonster" :
                type == EnemyType.PresentationLaser ? "PresentationLaserMonster" :
                type == EnemyType.DeadlineTimer ? "DeadlineTimer" :
                type == EnemyType.AnxietyShadow || type == EnemyType.ThoughtCloud ? "AnxietyShadow" :
                "SleepSlime";
            Sprite production = LoadProduction("Enemies/" + productionName);
            if (production != null) return production;
            string key = "enemy_dark_academia_" + type + "_v3";
            return Create(key, 24, 24, texture =>
            {
                Color main = EnemyColor(type);
                Color shade = Darken(main, 0.52f);
                if (type == EnemyType.PhoneTemptation)
                {
                    OutlineRect(texture, 7, 3, 10, 18, Ink, main);
                    Rect(texture, 9, 7, 6, 9, new Color32(92, 211, 224, 255));
                    Rect(texture, 11, 4, 2, 1, Highlight);
                    Rect(texture, 10, 17, 1, 1, Ink);
                    Rect(texture, 14, 17, 1, 1, Ink);
                }
                else if (type == EnemyType.Assignment)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        OutlineRect(texture, 4 + i * 2, 4 + i * 4, 16, 7, Ink, Paper);
                        Rect(texture, 7 + i * 2, 7 + i * 4, 8, 1, new Color32(94, 126, 152, 255));
                    }
                    Rect(texture, 10, 18, 1, 1, Ink);
                    Rect(texture, 15, 18, 1, 1, Ink);
                }
                else if (type == EnemyType.DeadlineTimer)
                {
                    Circle(texture, 12, 12, 9, Ink);
                    Circle(texture, 12, 12, 7, Paper);
                    Rect(texture, 10, 21, 4, 2, Ink);
                    Line(texture, 12, 12, 12, 17, new Color32(226, 68, 64, 255));
                    Line(texture, 12, 12, 16, 10, new Color32(226, 68, 64, 255));
                    Rect(texture, 8, 14, 2, 2, Ink);
                    Rect(texture, 15, 14, 2, 2, Ink);
                }
                else if (type == EnemyType.PresentationLaser)
                {
                    OutlineRect(texture, 4, 7, 16, 11, Ink, main);
                    Rect(texture, 7, 10, 10, 5, new Color32(248, 241, 210, 255));
                    Rect(texture, 9, 11, 6, 1, new Color32(220, 70, 70, 255));
                    Rect(texture, 18, 11, 5, 3, new Color32(255, 77, 65, 255));
                    Rect(texture, 7, 5, 3, 2, shade);
                    Rect(texture, 14, 5, 3, 2, shade);
                }
                else if (type == EnemyType.TeamProject)
                {
                    Circle(texture, 8, 13, 6, Ink);
                    Circle(texture, 16, 13, 6, Ink);
                    Circle(texture, 8, 13, 4, main);
                    Circle(texture, 16, 13, 4, main);
                    Rect(texture, 10, 7, 4, 3, shade);
                    Rect(texture, 7, 14, 1, 1, Highlight);
                    Rect(texture, 16, 14, 1, 1, Highlight);
                }
                else if (type == EnemyType.AnxietyShadow)
                {
                    Circle(texture, 12, 14, 8, Ink);
                    Rect(texture, 5, 4, 14, 11, Ink);
                    Rect(texture, 8, 14, 2, 2, new Color32(210, 103, 255, 255));
                    Rect(texture, 15, 14, 2, 2, new Color32(210, 103, 255, 255));
                    Rect(texture, 3, 2, 5, 4, shade);
                    Rect(texture, 16, 2, 5, 4, shade);
                }
                else
                {
                    Circle(texture, 12, 10, 10, Ink);
                    Rect(texture, 3, 5, 18, 7, Ink);
                    Circle(texture, 12, 11, 8, main);
                    Rect(texture, 6, 5, 12, 5, main);
                    Rect(texture, 8, 13, 2, 2, Highlight);
                    Rect(texture, 15, 13, 2, 2, Highlight);
                    Rect(texture, 9, 13, 1, 1, Ink);
                    Rect(texture, 15, 13, 1, 1, Ink);
                    Rect(texture, 8, 7, 8, 2, shade);
                }
                Rect(texture, 5, 5, 3, 1, Lighten(main, 1.35f));
                Rect(texture, 18, 6, 2, 1, shade);
            });
        }

        public static Sprite GetBoss(int stage)
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetBoss(stage);
            Sprite production = LoadProduction(stage == 10 ? "Bosses/FinalExamJudge" : "Bosses/MidtermWatcher");
            if (production != null) return production;
            return Create("boss_exam_dark_academia_" + stage + "_v5", 64, 64, texture =>
            {
                Color red = stage == 10 ? new Color32(176, 45, 87, 255) : new Color32(205, 67, 62, 255);
                Color gold = new Color32(242, 183, 62, 255);
                Color body = stage == 10 ? new Color32(37, 30, 49, 255) : Paper;
                Color paperShade = stage == 10 ? new Color32(63, 47, 70, 255) : new Color32(218, 207, 174, 255);
                OutlineRect(texture, 9, 7, 46, 47, Ink, body);
                Rect(texture, 11, 9, 6, 42, paperShade);
                Rect(texture, 48, 9, 5, 42, new Color32(232, 220, 183, 255));
                Rect(texture, 15, 48, 34, 4, red);
                Rect(texture, 15, 42, 24, 3, new Color32(94, 126, 152, 255));
                Rect(texture, 15, 37, 31, 2, new Color32(94, 126, 152, 255));
                Rect(texture, 15, 32, 20, 2, new Color32(94, 126, 152, 255));
                Rect(texture, 42, 41, 7, 5, new Color32(95, 185, 120, 255));
                Line(texture, 43, 43, 45, 41, Highlight);
                Line(texture, 45, 41, 49, 46, Highlight);
                Rect(texture, 16, 18, 11, 7, Ink);
                Rect(texture, 38, 18, 11, 7, Ink);
                Rect(texture, 19, 20, 5, 3, red);
                Rect(texture, 41, 20, 5, 3, red);
                Rect(texture, 24, 12, 17, 4, red);
                Rect(texture, 28, 13, 9, 2, new Color32(112, 29, 53, 255));
                Circle(texture, 50, 50, 11, Ink);
                Circle(texture, 50, 50, 8, gold);
                Circle(texture, 50, 50, 5, new Color32(255, 220, 102, 255));
                Line(texture, 50, 50, 50, 56, Ink);
                Line(texture, 50, 50, 56, 47, Ink);
                Rect(texture, 3, 10, 7, 34, red);
                Rect(texture, 55, 10, 7, 34, red);
                Rect(texture, 4, 14, 2, 26, new Color32(245, 105, 84, 255));
                Rect(texture, 58, 14, 2, 26, new Color32(245, 105, 84, 255));
                Line(texture, 5, 9, 1, 2, gold);
                Line(texture, 59, 9, 63, 2, gold);
                Rect(texture, 2, 1, 4, 4, Highlight);
                Rect(texture, 59, 1, 4, 4, Highlight);
                Line(texture, 10, 53, 5, 61, Ink);
                Line(texture, 54, 53, 59, 61, Ink);
                Rect(texture, 5, 59, 5, 3, red);
                Rect(texture, 55, 59, 5, 3, red);
            }, 21.333f);
        }

        public static Sprite GetItem(string itemId)
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetItem(itemId);
            Sprite production = LoadProduction("Items/" + itemId);
            if (production != null) return production;
            return Create("item_" + itemId + "_v3", 24, 24, texture =>
            {
                Color gold = new Color32(242, 184, 61, 255);
                if (itemId == "energy_jelly")
                {
                    OutlineRect(texture, 5, 4, 14, 15, Ink, new Color32(91, 220, 156, 255));
                    Rect(texture, 7, 17, 10, 3, Highlight);
                    Rect(texture, 9, 9, 6, 3, new Color32(181, 255, 214, 255));
                }
                else if (itemId == "night_coffee")
                {
                    OutlineRect(texture, 5, 4, 12, 15, Ink, new Color32(139, 81, 54, 255));
                    OutlineRect(texture, 15, 7, 5, 8, Ink, new Color32(232, 207, 165, 255));
                    Rect(texture, 7, 17, 8, 2, new Color32(245, 229, 201, 255));
                    Line(texture, 8, 20, 8, 23, Highlight);
                    Line(texture, 13, 20, 13, 23, Highlight);
                }
                else if (itemId == "focus_headphones")
                {
                    Circle(texture, 12, 12, 9, Ink);
                    Circle(texture, 12, 12, 6, new Color(0, 0, 0, 0));
                    Rect(texture, 3, 7, 5, 9, new Color32(68, 160, 208, 255));
                    Rect(texture, 16, 7, 5, 9, new Color32(68, 160, 208, 255));
                }
                else if (itemId == "highlighter")
                {
                    Line(texture, 5, 5, 18, 18, Ink);
                    Line(texture, 6, 5, 19, 18, new Color32(255, 222, 61, 255));
                    Rect(texture, 16, 16, 5, 4, new Color32(255, 244, 144, 255));
                    Rect(texture, 3, 3, 5, 3, new Color32(72, 52, 73, 255));
                }
                else if (itemId == "all_nighter")
                {
                    Circle(texture, 12, 12, 10, Ink);
                    Circle(texture, 12, 12, 7, new Color32(53, 42, 82, 255));
                    Circle(texture, 15, 15, 6, new Color32(255, 222, 102, 255));
                    Circle(texture, 12, 17, 6, new Color32(53, 42, 82, 255));
                    Rect(texture, 5, 5, 2, 2, new Color32(190, 221, 255, 255));
                    Rect(texture, 19, 8, 2, 2, new Color32(190, 221, 255, 255));
                }
                else if (itemId == "summary_sheet")
                {
                    OutlineRect(texture, 4, 3, 16, 18, Ink, Paper);
                    Rect(texture, 7, 17, 10, 2, new Color32(94, 178, 129, 255));
                    Rect(texture, 7, 13, 8, 1, new Color32(82, 127, 158, 255));
                    Rect(texture, 7, 10, 10, 1, new Color32(82, 127, 158, 255));
                    Rect(texture, 7, 7, 6, 1, new Color32(226, 88, 81, 255));
                }
                else
                {
                    OutlineRect(texture, 5, 3, 15, 18, Ink, Paper);
                    Rect(texture, 8, 17, 9, 2, gold);
                    Rect(texture, 8, 13, 7, 1, new Color32(82, 127, 158, 255));
                    Rect(texture, 8, 10, 9, 1, new Color32(82, 127, 158, 255));
                    if (itemId == "past_exam_book") Rect(texture, 16, 5, 3, 14, new Color32(211, 73, 72, 255));
                    if (itemId == "final_notes") Rect(texture, 7, 4, 11, 3, new Color32(171, 84, 208, 255));
                }
            });
        }

        public static Sprite GetSlash()
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetSlash();
            Sprite production = LoadProduction("Effects/Slash");
            if (production != null) return production;
            return Create("slash_v2", 32, 24, texture =>
            {
                Color glow = new Color32(255, 214, 73, 215);
                Color core = new Color32(255, 251, 218, 255);
                for (int i = 0; i < 17; i++)
                {
                    Rect(texture, 4 + i, 3 + i / 2, 3, 3, glow);
                    Rect(texture, 7 + i, 4 + i / 2, 2, 2, core);
                }
                Rect(texture, 22, 14, 5, 2, glow);
                Rect(texture, 25, 18, 3, 2, glow);
            });
        }

        public static Sprite GetHitSpark(bool critical)
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetHitSpark(critical);
            return Create(critical ? "critical_hit_spark_v2" : "hit_spark_v2", 32, 32, texture =>
            {
                Color core = new Color32(255, 252, 220, 255);
                Color glow = critical ? new Color32(255, 153, 45, 255) : new Color32(255, 218, 91, 255);
                for (int i = 0; i < 10; i++)
                {
                    int width = Mathf.Max(1, 6 - i / 2);
                    Rect(texture, 16 - width / 2, 16 + i, width, 1, i < 4 ? core : glow);
                    Rect(texture, 16 - width / 2, 15 - i, width, 1, i < 4 ? core : glow);
                    Rect(texture, 16 + i, 16 - width / 2, 1, width, i < 4 ? core : glow);
                    Rect(texture, 15 - i, 16 - width / 2, 1, width, i < 4 ? core : glow);
                }
                Line(texture, 7, 7, 25, 25, glow);
                Line(texture, 7, 25, 25, 7, glow);
                Circle(texture, 16, 16, critical ? 5 : 3, core);
            }, 32f);
        }

        public static Sprite GetProjectile(Color color)
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetProjectile(color);
            return Create("projectile_v2_" + color, 12, 12, texture =>
            {
                Circle(texture, 6, 6, 5, Darken(color, 0.45f));
                Circle(texture, 6, 6, 3, color);
                Rect(texture, 5, 7, 2, 2, Highlight);
            });
        }

        public static Sprite GetPlatform(Color color)
        {
            return GetPlatform("stone", color);
        }

        public static Sprite GetPlatform(string style, Color color)
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetPlatform(style, color);
            bool oneWay = style.EndsWith("_oneway");
            string baseStyle = style.Replace("_oneway", "").Replace("_ground", "");
            return Create("platform_" + style + "_v5_" + color, 16, 16, texture =>
            {
                Color top = Lighten(color, 1.35f);
                Color shade = Darken(color, 0.58f);
                if (oneWay)
                {
                    Rect(texture, 0, 9, 16, 7, shade);
                    Rect(texture, 0, 12, 16, 4, color);
                    Rect(texture, 0, 14, 16, 2, top);
                    Rect(texture, 2, 10, 12, 1, Darken(color, 0.78f));
                }
                else
                {
                    Rect(texture, 0, 0, 16, 16, color);
                    Rect(texture, 0, 13, 16, 3, top);
                    Rect(texture, 0, 0, 16, 3, shade);
                    Rect(texture, 0, 5, 16, 1, Darken(color, 0.76f));
                    Rect(texture, 0, 10, 16, 1, Lighten(color, 1.08f));
                }
                if (baseStyle == "library")
                {
                    Rect(texture, 1, 4, 14, 2, shade);
                    Rect(texture, 2, 7, 3, 6, new Color32(119, 66, 54, 255));
                    Rect(texture, 6, 7, 2, 6, new Color32(52, 89, 112, 255));
                    Rect(texture, 9, 7, 3, 6, new Color32(132, 91, 45, 255));
                    Rect(texture, 13, 7, 2, 6, new Color32(78, 57, 94, 255));
                }
                else if (baseStyle == "meeting")
                {
                    Rect(texture, 0, 7, 16, 2, shade);
                    Rect(texture, 3, 2, 2, 11, top);
                    Rect(texture, 11, 2, 2, 11, top);
                    Rect(texture, 1, 10, 14, 1, Lighten(color, 1.15f));
                }
                else if (baseStyle == "presentation")
                {
                    Rect(texture, 0, 10, 16, 2, new Color32(157, 49, 60, 255));
                    Rect(texture, 2, 4, 3, 4, shade);
                    Rect(texture, 11, 4, 3, 4, shade);
                    Rect(texture, 7, 5, 2, 2, new Color32(233, 179, 63, 255));
                }
                else if (baseStyle == "exam" || baseStyle == "final")
                {
                    Rect(texture, 1, 5, 14, 2, shade);
                    Rect(texture, 1, 10, 14, 2, shade);
                    Rect(texture, 4, 3, 2, 10, top);
                    Rect(texture, 11, 3, 2, 10, top);
                    Rect(texture, 7, 7, 3, 3, baseStyle == "final" ? new Color32(163, 48, 83, 255) : new Color32(202, 151, 61, 255));
                }
                else
                {
                    Rect(texture, 2, 6, 3, 2, shade);
                    Rect(texture, 10, 9, 4, 2, shade);
                    Rect(texture, 6, 3, 2, 2, top);
                    Rect(texture, 0, 8, 16, 1, Darken(color, 0.72f));
                }
            });
        }

        public static Sprite GetPlatformSupport(string style, Color color)
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetPlatformSupport(style, color);
            return Create("platform_support_" + style + "_v2_" + color, 8, 16, texture =>
            {
                Color shade = Darken(color, 0.4f);
                Color edge = Lighten(color, 1.18f);
                Rect(texture, 1, 0, 6, 16, shade);
                Rect(texture, 2, 0, 3, 16, color);
                Rect(texture, 5, 0, 1, 16, edge);
                Rect(texture, 0, 13, 8, 3, shade);
                Rect(texture, 0, 0, 8, 2, Ink);
                for (int y = 4; y < 13; y += 5) Rect(texture, 1, y, 6, 1, edge);
            });
        }

        public static Sprite GetSpike()
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetSpike();
            return Create("stress_spike_v3", 32, 16, texture =>
            {
                Color red = new Color32(217, 62, 71, 255);
                Color glow = new Color32(255, 156, 75, 255);
                Rect(texture, 0, 0, 32, 3, Ink);
                for (int i = 0; i < 4; i++)
                {
                    int x = i * 8;
                    for (int h = 0; h < 12; h++)
                    {
                        int half = Mathf.Max(0, (11 - h) / 2);
                        Rect(texture, x + 4 - half, 3 + h, half * 2 + 1, 1, h > 8 ? glow : red);
                    }
                }
            });
        }

        public static Sprite GetGate()
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetGate();
            return Create("academic_room_gate_dark_v2", 16, 32, texture =>
            {
                Color blue = new Color32(92, 38, 109, 225);
                Color glow = new Color32(227, 73, 101, 245);
                Rect(texture, 0, 0, 16, 32, new Color(0, 0, 0, 0));
                Rect(texture, 1, 0, 3, 32, Ink);
                Rect(texture, 12, 0, 3, 32, Ink);
                Rect(texture, 4, 1, 8, 30, blue);
                for (int y = 2; y < 31; y += 5) Rect(texture, 5, y, 6, 2, glow);
                Rect(texture, 0, 28, 16, 4, Ink);
                Rect(texture, 0, 0, 16, 4, Ink);
            });
        }

        public static Sprite GetProp(string kind, Color color)
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetProp(kind, color);
            return Create("prop_" + kind + "_" + color, 32, 32, texture =>
            {
                Color shade = Darken(color, 0.55f);
                if (kind == "window")
                {
                    OutlineRect(texture, 2, 3, 28, 26, Ink, color);
                    Rect(texture, 5, 6, 22, 19, Lighten(color, 1.35f));
                    Rect(texture, 15, 5, 2, 22, Ink);
                    Rect(texture, 4, 15, 24, 2, Ink);
                }
                else if (kind == "board")
                {
                    OutlineRect(texture, 1, 7, 30, 20, Ink, color);
                    Rect(texture, 5, 21, 16, 2, Lighten(color, 1.45f));
                    Rect(texture, 8, 16, 19, 2, Lighten(color, 1.3f));
                    Rect(texture, 3, 5, 26, 2, shade);
                }
                else if (kind == "books")
                {
                    Rect(texture, 2, 3, 28, 4, shade);
                    for (int i = 0; i < 7; i++)
                    {
                        Color book = i % 3 == 0 ? new Color32(207, 76, 76, 255) : i % 3 == 1 ? new Color32(65, 151, 190, 255) : new Color32(229, 174, 65, 255);
                        Rect(texture, 4 + i * 4, 7, 3, 17 + i % 2 * 3, book);
                        Rect(texture, 4 + i * 4, 8, 3, 2, Lighten(book, 1.25f));
                    }
                }
                else if (kind == "column")
                {
                    Rect(texture, 9, 2, 14, 28, Ink);
                    Rect(texture, 11, 4, 10, 24, color);
                    Rect(texture, 6, 2, 20, 4, shade);
                    Rect(texture, 6, 27, 20, 4, shade);
                    Rect(texture, 13, 6, 3, 19, Lighten(color, 1.22f));
                }
                else if (kind == "arch")
                {
                    Rect(texture, 2, 2, 5, 24, Ink);
                    Rect(texture, 25, 2, 5, 24, Ink);
                    Rect(texture, 5, 22, 22, 8, Ink);
                    Circle(texture, 16, 21, 11, Ink);
                    Circle(texture, 16, 20, 7, new Color(0, 0, 0, 0));
                    Rect(texture, 7, 3, 3, 20, color);
                    Rect(texture, 22, 3, 3, 20, color);
                }
                else if (kind == "lamp")
                {
                    Rect(texture, 14, 18, 4, 12, Ink);
                    Rect(texture, 8, 8, 16, 12, Ink);
                    Rect(texture, 11, 10, 10, 8, new Color32(242, 178, 70, 255));
                    Rect(texture, 13, 12, 6, 5, Highlight);
                    Rect(texture, 11, 5, 10, 3, shade);
                }
                else if (kind == "banner")
                {
                    Rect(texture, 8, 2, 16, 28, Ink);
                    Rect(texture, 10, 4, 12, 23, color);
                    Rect(texture, 10, 4, 12, 3, Lighten(color, 1.28f));
                    Line(texture, 12, 20, 20, 10, Highlight);
                    Line(texture, 20, 20, 12, 10, Highlight);
                }
                else if (kind == "desk")
                {
                    Rect(texture, 3, 12, 26, 8, Ink);
                    Rect(texture, 5, 14, 22, 4, color);
                    Rect(texture, 6, 3, 4, 10, shade);
                    Rect(texture, 22, 3, 4, 10, shade);
                    Rect(texture, 2, 19, 28, 3, Lighten(color, 1.2f));
                }
                else if (kind == "warning")
                {
                    Rect(texture, 14, 2, 4, 18, Ink);
                    Rect(texture, 12, 2, 8, 3, shade);
                    OutlineRect(texture, 4, 17, 24, 12, Ink, new Color32(226, 167, 59, 255));
                    Rect(texture, 14, 20, 4, 5, Ink);
                    Rect(texture, 14, 18, 4, 2, new Color32(244, 232, 191, 255));
                }
                else if (kind == "launch")
                {
                    OutlineRect(texture, 2, 7, 28, 13, Ink, shade);
                    Rect(texture, 4, 14, 24, 4, Lighten(color, 1.35f));
                    Rect(texture, 6, 9, 20, 4, color);
                    for (int i = 0; i < 4; i++) Line(texture, 7 + i * 5, 8, 10 + i * 5, 14, new Color32(255, 220, 104, 255));
                    Rect(texture, 8, 20, 16, 3, new Color32(255, 238, 170, 255));
                }
                else
                {
                    OutlineRect(texture, 3, 4, 26, 22, Ink, color);
                    Rect(texture, 6, 20, 20, 3, Lighten(color, 1.25f));
                    Rect(texture, 8, 8, 3, 12, shade);
                    Rect(texture, 21, 8, 3, 12, shade);
                }
            });
        }

        public static Sprite Get(string key, Color color, int size = 16)
        {
            return Create(key + "_fallback_" + color + "_" + size, size, size, texture =>
            {
                OutlineRect(texture, 0, 0, size, size, Darken(color, 0.35f), color);
                Rect(texture, size / 3, size * 2 / 3, 1, 1, Highlight);
                Rect(texture, size * 2 / 3, size * 2 / 3, 1, 1, Highlight);
            });
        }

        public static Sprite Solid(string key, Color color)
        {
            return Create(key + color, 2, 2, texture => Rect(texture, 0, 0, 2, 2, color), 2);
        }

        public static Sprite GetUiFrame()
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetUiFrame();
            return CreateNineSlice("ui_pixel_frame_v3", new Color32(151, 105, 62, 255), new Color32(67, 49, 59, 255), new Color32(14, 18, 28, 255));
        }

        public static Sprite GetUiButton()
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetUiButton();
            return CreateNineSlice("ui_pixel_button_v3", new Color32(182, 126, 66, 255), new Color32(61, 68, 83, 255), new Color32(22, 29, 42, 255));
        }

        public static Sprite GetUiSlot()
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetUiSlot();
            return CreateNineSlice("ui_pixel_slot_v3", new Color32(123, 91, 56, 255), new Color32(55, 47, 58, 255), new Color32(12, 16, 25, 255));
        }

        public static Color StageColor(int stage)
        {
            Color[] colors = {
                new Color32(106, 161, 186, 255), new Color32(95, 145, 153, 255), new Color32(202, 139, 91, 255),
                new Color32(117, 91, 137, 255), new Color32(67, 72, 91, 255), new Color32(52, 83, 108, 255),
                new Color32(112, 66, 104, 255), new Color32(70, 103, 83, 255), new Color32(31, 42, 69, 255),
                new Color32(43, 24, 48, 255)
            };
            return colors[Mathf.Clamp(stage - 1, 0, colors.Length - 1)];
        }

        public static Color EnemyColor(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.SleepSlime: return new Color32(89, 72, 119, 255);
                case EnemyType.PhoneTemptation: return new Color32(35, 40, 59, 255);
                case EnemyType.Assignment: return new Color32(196, 181, 146, 255);
                case EnemyType.TeamProject: return new Color32(126, 63, 128, 255);
                case EnemyType.PresentationLaser: return new Color32(161, 47, 57, 255);
                case EnemyType.DeadlineTimer: return new Color32(178, 112, 44, 255);
                case EnemyType.AnxietyShadow: return new Color32(49, 31, 70, 255);
                default: return new Color32(75, 91, 113, 255);
            }
        }

        public static Sprite GetPlayerFrame(string frame)
        {
            if (FreshPixelArt.Enabled) return FreshPixelArt.GetPlayerFrame(frame);
            Sprite stable = LoadStable("Player/" + frame);
            return stable != null ? stable : LoadProduction("Player/" + frame);
        }

        private static Sprite LoadStable(string relativePath)
        {
            return null;
        }

        private static Sprite LoadProduction(string relativePath)
        {
            return null;
        }

        private static Sprite Create(string key, int width, int height, Action<Texture2D> paint, float pixelsPerUnit = 16f)
        {
            Sprite sprite;
            if (Cache.TryGetValue(key, out sprite)) return sprite;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(new Color[width * height]);
            paint(texture);
            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = key;
            Cache[key] = sprite;
            return sprite;
        }

        private static Sprite CreateNineSlice(string key, Color edge, Color bevel, Color center)
        {
            Sprite sprite;
            if (Cache.TryGetValue(key, out sprite)) return sprite;
            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            Rect(texture, 0, 0, size, size, center);
            Rect(texture, 0, 0, size, 2, Ink);
            Rect(texture, 0, size - 2, size, 2, Ink);
            Rect(texture, 0, 0, 2, size, Ink);
            Rect(texture, size - 2, 0, 2, size, Ink);
            Rect(texture, 2, 2, size - 4, 2, edge);
            Rect(texture, 2, size - 4, size - 4, 2, edge);
            Rect(texture, 2, 2, 2, size - 4, edge);
            Rect(texture, size - 4, 2, 2, size - 4, edge);
            Rect(texture, 4, 4, size - 8, 2, bevel);
            Rect(texture, 4, size - 6, size - 8, 2, bevel);
            Rect(texture, 4, 4, 2, size - 8, bevel);
            Rect(texture, size - 6, 4, 2, size - 8, bevel);
            Rect(texture, 0, 0, 4, 4, new Color(0, 0, 0, 0));
            Rect(texture, size - 4, 0, 4, 4, new Color(0, 0, 0, 0));
            Rect(texture, 0, size - 4, 4, 4, new Color(0, 0, 0, 0));
            Rect(texture, size - 4, size - 4, 4, 4, new Color(0, 0, 0, 0));
            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect, new Vector4(6, 6, 6, 6));
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

        private static void OutlineRect(Texture2D texture, int x, int y, int width, int height, Color outline, Color fill)
        {
            Rect(texture, x, y, width, height, outline);
            Rect(texture, x + 2, y + 2, width - 4, height - 4, fill);
        }

        private static void Circle(Texture2D texture, int cx, int cy, int radius, Color color)
        {
            for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
                if (x * x + y * y <= radius * radius) Rect(texture, cx + x, cy + y, 1, 1, color);
        }

        private static void Line(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;
            while (true)
            {
                Rect(texture, x0, y0, 1, 1, color);
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
