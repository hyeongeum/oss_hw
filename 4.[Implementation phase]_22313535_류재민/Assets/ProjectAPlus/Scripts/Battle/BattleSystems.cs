using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAPlus
{
    public static class DamageCalculator
    {
        public static int CalculatePlayerDamage(int studyPower, int review, int defense, float randomVariance, out bool critical)
        {
            critical = UnityEngine.Random.value < Mathf.Clamp01(review * 0.025f);
            int variance = Mathf.RoundToInt(Mathf.Clamp(randomVariance, -3f, 3f));
            int result = studyPower + Mathf.RoundToInt(review * 0.6f) + variance - Mathf.Max(0, defense);
            if (critical) result = Mathf.RoundToInt(result * 1.55f);
            return Mathf.Max(0, result);
        }

        public static int CalculatePlayerDamageDeterministic(int studyPower, int review, int defense, int variance, bool critical)
        {
            int result = studyPower + Mathf.RoundToInt(review * 0.6f) + Mathf.Clamp(variance, -3, 3) - Mathf.Max(0, defense);
            if (critical) result = Mathf.RoundToInt(result * 1.55f);
            return Mathf.Max(0, result);
        }
    }

    public static class GradeManager
    {
        private static readonly string[] Grades = { "F", "C", "C+", "B", "B+", "A", "A+" };

        public static StageResult Calculate(StageData stage, PlayerStatus status, float clearTime, int kills, int damageTaken, int itemsUsed)
        {
            float mentalRatio = status.maxMental <= 0 ? 0 : status.mental / (float)status.maxMental;
            int score = stage.baseRewardScore + Mathf.RoundToInt(mentalRatio * 900f) + kills * 80 - damageTaken * 5 - itemsUsed * 70;
            float target = stage.timeLimit > 0 ? stage.timeLimit : GameBalance.TargetRoomSeconds(stage.stageNumber);
            if (clearTime < target) score += Mathf.RoundToInt((target - clearTime) * 10f);
            score = Mathf.Max(0, score);
            return new StageResult { stage = stage.stageNumber, clearTime = clearTime, remainingMental = status.mental, maxMental = status.maxMental, kills = kills, damageTaken = damageTaken, itemsUsed = itemsUsed, score = score, exp = stage.baseRewardExp, grade = GradeFromScore(score) };
        }

        public static string GradeFromScore(int score)
        {
            if (score >= 6500) return "A+";
            if (score >= 5200) return "A";
            if (score >= 4200) return "B+";
            if (score >= 3300) return "B";
            if (score >= 2500) return "C+";
            if (score >= 1700) return "C";
            return "F";
        }

        public static string FinalGrade(int totalScore) { return GradeFromScore(totalScore / 3); }

        public static bool IsValidGrade(string grade)
        {
            return Array.IndexOf(Grades, grade) >= 0;
        }

        public static string BestGrade(string current, string candidate)
        {
            int currentRank = Array.IndexOf(Grades, current);
            int candidateRank = Array.IndexOf(Grades, candidate);
            if (currentRank < 0) currentRank = 0;
            if (candidateRank < 0) candidateRank = 0;
            return Grades[Mathf.Max(currentRank, candidateRank)];
        }
    }

    public class RewardManager : MonoBehaviour
    {
        public void RewardEnemy(EnemyData data)
        {
            if (GameManager.Instance == null || GameManager.Instance.Player == null) return;
            GameManager.Instance.Player.GetComponent<PlayerGrowth>().AddExp(data.expReward);
            GameManager.Instance.Player.score += data.scoreReward;
        }
    }

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }
        private void Awake() { Instance = this; }
    }

    public class ItemManager : MonoBehaviour
    {
        public ItemData Get(string itemId)
        {
            if (GameManager.Instance == null) return null;
            ItemData item;
            return GameManager.Instance.Items.TryGetValue(itemId, out item) ? item : null;
        }
    }
}
