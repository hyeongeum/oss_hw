using UnityEngine;

namespace ProjectAPlus
{
    // Loads the selected, normalized sprites extracted from the user-provided sheets.
    public static class ProvidedArt
    {
        private const string Root = "ProvidedArt/";

        public static Sprite GetPlayerFrame(string frame)
        {
            return Resources.Load<Sprite>(Root + "Player/" + frame);
        }

        public static Sprite GetEnemy(EnemyType type)
        {
            return Resources.Load<Sprite>(Root + "Enemies/fresh_enemy_" + type);
        }

        public static Sprite GetBoss(int stage)
        {
            return Resources.Load<Sprite>(Root + "Bosses/fresh_boss_" + stage);
        }

        public static Sprite GetPlatform(string style)
        {
            bool oneWay = style.EndsWith("_oneway");
            string baseStyle = style.Replace("_oneway", "").Replace("_ground", "");
            if (baseStyle == "boundary") return null;
            string theme = baseStyle == "library" ? "library"
                : baseStyle == "meeting" ? "meeting"
                : baseStyle == "presentation" ? "presentation"
                : baseStyle == "exam" ? "exam"
                : baseStyle == "final" ? "final"
                : "classroom";
            return Resources.Load<Sprite>(Root + "Terrain/" + theme + (oneWay ? "_oneway" : "_ground"));
        }

        public static Sprite GetPlatformSupport(string style)
        {
            // Repeating the large sheet columns vertically creates visible seams.
            return null;
        }

        public static Sprite GetProp(string kind)
        {
            return Resources.Load<Sprite>(Root + "Props/" + kind);
        }
    }
}
