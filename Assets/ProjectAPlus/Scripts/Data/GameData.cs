using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAPlus
{
    public enum GameState { Title, Opening, Playing, Paused, Result, Ending, GameOver, FinalGrade }
    public enum EnemyType { SleepSlime, PhoneTemptation, Assignment, TeamProject, PresentationLaser, DeadlineTimer, AnxietyShadow, ThoughtCloud, Generic }
    public enum ItemType { Heal, AttackEfficiencyBuff, StudyPowerBuff, BossDamageBuff, PresentationGuard, Stealth, FinalHeal }
    public enum UpgradeType { Mental, StudyPower, Review, AttackEfficiency, MovementEfficiency }
    public enum RunUpgradeType { Coffee, LectureNotes, SummarySheet, Highlighter, AllNighter }

    [Serializable]
    public class EnemySpawnData
    {
        public EnemyType type;
        public float x;
        public float y;
        public bool elite;

        public EnemySpawnData(EnemyType type, float x, float y, bool elite = false)
        {
            this.type = type;
            this.x = x;
            this.y = y;
            this.elite = elite;
        }
    }

    [Serializable]
    public class BossData
    {
        public string bossName;
        public int maxHp;
        public int attackPower;
    }

    [Serializable]
    public class StageData
    {
        public int stageNumber;
        public string stageName;
        public string backgroundType;
        public string clearCondition;
        public string objective;
        public int targetEnemyKillCount;
        public float timeLimit;
        public int baseRewardExp;
        public int baseRewardScore;
        public int growthPointReward;
        public string rewardItemId;
        public int nextStageNumber;
        public BossData bossData;
        public List<EnemySpawnData> enemySpawnList = new List<EnemySpawnData>();
        public bool IsBossStage { get { return bossData != null; } }
    }

    [Serializable]
    public class EnemyData
    {
        public string enemyId;
        public string enemyName;
        public int maxHp;
        public int attackPower;
        public int defense;
        public float moveSpeed;
        public float attackRange;
        public float detectRange;
        public int expReward;
        public int scoreReward;
        public float itemDropChance;
        public EnemyType enemyType;
    }

    [Serializable]
    public class ItemData
    {
        public string itemId;
        public string itemName;
        public string description;
        public ItemType itemType;
        public float value;
        public float duration;
        public Sprite icon;
    }

    [Serializable]
    public class RunUpgradeData
    {
        public RunUpgradeType type;
        public string upgradeName;
        public string description;
        public string iconItemId;

        public RunUpgradeData(RunUpgradeType type, string name, string description, string iconItemId)
        {
            this.type = type;
            upgradeName = name;
            this.description = description;
            this.iconItemId = iconItemId;
        }
    }

    [Serializable]
    public class InventoryEntry
    {
        public string itemId;
        public int count;

        public InventoryEntry(string id, int amount)
        {
            itemId = id;
            count = amount;
        }
    }

    [Serializable]
    public class SettingsData
    {
        public float bgmVolume = 0.5f;
        public float sfxVolume = 0.8f;
        public bool fullscreen = false;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
    }

    [Serializable]
    public class SaveData
    {
        public int currentStage = 1;
        public int playerLevel = 1;
        public int mental = 100;
        public int maxMental = 100;
        public int studyPower = 14;
        public int review = 2;
        public int attackEfficiency = 1;
        public int movementEfficiency = 1;
        public int exp;
        public int growthPoint;
        public int score;
        public string highestGrade = "F";
        public List<InventoryEntry> inventoryItems = new List<InventoryEntry>();
        public List<int> clearedStages = new List<int>();
        public List<string> runUpgrades = new List<string>();
        public float runElapsed;
        public SettingsData settings = new SettingsData();
    }

    public struct StageResult
    {
        public int stage;
        public float clearTime;
        public int remainingMental;
        public int maxMental;
        public int kills;
        public int damageTaken;
        public int itemsUsed;
        public int score;
        public int exp;
        public string grade;
    }

    public static class GameBalance
    {
        public const float BaseMoveSpeed = 6f;
        public const float BaseJumpPower = 13.2f;
        public const float PlayerRiseGravityScale = 2.45f;
        public const float PlayerFallGravityScale = 3.1f;
        public const float SafePlatformRise = 2.1f;
        public const float SafePlatformGap = 3.6f;
        public const float BaseAttackCooldown = 0.52f;
        public const float BaseDodgeCooldown = 1.2f;
        public const float StageWidth = 68f;
        public const int LevelExpBase = 100;
        public const float TargetRunMinutes = 30f;

        public static float MaxJumpRise(int movementEfficiency = 1)
        {
            float velocity = BaseJumpPower + Mathf.Max(0, movementEfficiency) * 0.18f;
            return velocity * velocity / (2f * Mathf.Abs(Physics2D.gravity.y) * PlayerRiseGravityScale);
        }

        public static float TargetRoomSeconds(int stage)
        {
            if (stage == 5 || stage == 10) return 270f;
            if (stage == 1) return 150f;
            return 165f;
        }
    }

    public static class StageCatalog
    {
        public static List<StageData> CreateAll()
        {
            return new List<StageData>
            {
                Make(1, "첫 수업 - 튜토리얼", "아침 강의실", "졸음 슬라임 2마리를 처치하고 강의실 끝에 도착", 2, 0, 50, 500, 1, "energy_jelly",
                    S(EnemyType.SleepSlime, 10), S(EnemyType.SleepSlime, 20), S(EnemyType.DeadlineTimer, 29)),
                Make(2, "강의 집중", "일반 강의실", "적 5마리를 처치하고 목표 지점 도착", 5, 0, 80, 1000, 1, "",
                    S(EnemyType.PhoneTemptation, 7), S(EnemyType.ThoughtCloud, 13), S(EnemyType.SleepSlime, 20), S(EnemyType.PhoneTemptation, 27), S(EnemyType.SleepSlime, 33)),
                Make(3, "과제 폭탄", "점심 강의실", "과제 몬스터 2마리를 포함해 적을 처치", 4, 0, 120, 1400, 1, "organized_notes",
                    S(EnemyType.Assignment, 11, true), S(EnemyType.PhoneTemptation, 18), S(EnemyType.DeadlineTimer, 25), S(EnemyType.Assignment, 32, true)),
                Make(4, "팀플 지옥", "팀플 회의실", "팀플 웨이브 3개를 정리", 6, 0, 150, 1900, 2, "night_coffee",
                    S(EnemyType.TeamProject, 7), S(EnemyType.TeamProject, 12), S(EnemyType.PresentationLaser, 19), S(EnemyType.TeamProject, 25), S(EnemyType.PresentationLaser, 31), S(EnemyType.TeamProject, 36)),
                Boss(5, "중간고사", "중간고사 시험장", "중간고사 감시자를 처치", 250, 3000, 3, "past_exam_book", "중간고사 감시자", 560, 15),
                Make(6, "다시 시작되는 학기", "저녁 강의실", "누적 피로와 과제를 뚫고 목표 지점 도착", 5, 0, 180, 2200, 1, "",
                    S(EnemyType.AnxietyShadow, 8), S(EnemyType.DeadlineTimer, 14), S(EnemyType.Assignment, 21, true), S(EnemyType.AnxietyShadow, 28), S(EnemyType.Assignment, 35, true)),
                Make(7, "발표 공포", "발표실", "발표 공격을 회피하며 적 처치", 5, 0, 220, 2600, 2, "presentation_script",
                    S(EnemyType.PresentationLaser, 8), S(EnemyType.AnxietyShadow, 15), S(EnemyType.PresentationLaser, 22), S(EnemyType.ThoughtCloud, 29), S(EnemyType.PresentationLaser, 35)),
                Make(8, "레포트 마감", "도서관", "제한 시간 안에 마감 지점 도착", 5, 180, 250, 3000, 2, "focus_headphones",
                    S(EnemyType.DeadlineTimer, 7), S(EnemyType.Assignment, 13), S(EnemyType.AnxietyShadow, 20), S(EnemyType.DeadlineTimer, 27), S(EnemyType.Assignment, 34, true)),
                Make(9, "기말 직전 밤샘", "밤 도서관", "졸음 군단 웨이브를 정리", 7, 0, 300, 3600, 3, "final_notes",
                    S(EnemyType.SleepSlime, 6), S(EnemyType.SleepSlime, 11), S(EnemyType.AnxietyShadow, 16), S(EnemyType.SleepSlime, 22), S(EnemyType.PhoneTemptation, 27), S(EnemyType.AnxietyShadow, 33), S(EnemyType.SleepSlime, 37)),
                Boss(10, "기말고사", "최종 시험장", "기말고사 심판관을 처치", 500, 6000, 4, "", "기말고사 심판관", 1180, 20)
            };
        }

        public static Dictionary<string, ItemData> CreateItems()
        {
            var items = new Dictionary<string, ItemData>();
            Add(items, "energy_jelly", "에너지 젤리", "멘탈 20 회복", ItemType.Heal, 20, 0);
            Add(items, "night_coffee", "밤샘 커피", "10초간 공격 효율 증가", ItemType.AttackEfficiencyBuff, 2, 10);
            Add(items, "organized_notes", "정리된 필기", "12초간 공부량 증가", ItemType.StudyPowerBuff, 8, 12);
            Add(items, "past_exam_book", "기출문제집", "보스에게 주는 피해 증가", ItemType.BossDamageBuff, 0.35f, 20);
            Add(items, "presentation_script", "발표 대본", "발표 공격 피해 감소", ItemType.PresentationGuard, 0.5f, 25);
            Add(items, "focus_headphones", "집중 이어폰", "일시적으로 이동 효율 증가", ItemType.Stealth, 2, 20);
            Add(items, "final_notes", "최종 정리 노트", "멘탈을 크게 회복", ItemType.FinalHeal, 50, 0);
            return items;
        }

        public static EnemyData CreateEnemy(EnemyType type, int stage, bool elite)
        {
            int hp = 36 + stage * 9 + (elite ? 45 : 0);
            int power = 7 + stage + (elite ? 3 : 0);
            return new EnemyData
            {
                enemyId = type.ToString(),
                enemyName = EnemyName(type),
                maxHp = hp,
                attackPower = power,
                defense = elite ? 2 + stage / 4 : stage / 5,
                moveSpeed = type == EnemyType.DeadlineTimer ? 3.2f : 1.7f + stage * 0.06f,
                attackRange = type == EnemyType.PresentationLaser ? 8f : 1.1f,
                detectRange = 8f,
                expReward = 12 + stage * 3,
                scoreReward = 100 + stage * 20,
                itemDropChance = stage == 9 ? 0.28f : 0.1f,
                enemyType = type
            };
        }

        private static string EnemyName(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.SleepSlime: return "졸음 슬라임";
                case EnemyType.PhoneTemptation: return "스마트폰 유혹";
                case EnemyType.Assignment: return "과제 몬스터";
                case EnemyType.TeamProject: return "무임승차 팀원";
                case EnemyType.PresentationLaser: return "발표자료 버그";
                case EnemyType.DeadlineTimer: return "마감 타이머";
                case EnemyType.AnxietyShadow: return "불안감 그림자";
                case EnemyType.ThoughtCloud: return "딴생각 구름";
                default: return "학업 스트레스";
            }
        }

        private static EnemySpawnData S(EnemyType type, float x, bool elite = false) { return new EnemySpawnData(type, x, 1.2f, elite); }
        private static StageData Make(int number, string name, string background, string objective, int kills, float limit, int exp, int score, int points, string item, params EnemySpawnData[] spawns)
        {
            return new StageData { stageNumber = number, stageName = name, backgroundType = background, clearCondition = objective, objective = objective, targetEnemyKillCount = kills, timeLimit = limit, baseRewardExp = exp, baseRewardScore = score, growthPointReward = points, rewardItemId = item, nextStageNumber = number + 1, enemySpawnList = new List<EnemySpawnData>(spawns) };
        }
        private static StageData Boss(int number, string name, string background, string objective, int exp, int score, int points, string item, string bossName, int hp, int power)
        {
            return new StageData { stageNumber = number, stageName = name, backgroundType = background, clearCondition = objective, objective = objective, targetEnemyKillCount = 1, baseRewardExp = exp, baseRewardScore = score, growthPointReward = points, rewardItemId = item, nextStageNumber = number + 1, bossData = new BossData { bossName = bossName, maxHp = hp, attackPower = power } };
        }
        private static void Add(Dictionary<string, ItemData> items, string id, string name, string description, ItemType type, float value, float duration)
        {
            items[id] = new ItemData { itemId = id, itemName = name, description = description, itemType = type, value = value, duration = duration };
        }
    }

    public static class RunUpgradeCatalog
    {
        public static List<RunUpgradeData> CreateAll()
        {
            return new List<RunUpgradeData>
            {
                new RunUpgradeData(RunUpgradeType.Coffee, "커피", "공격 효율 +1, 이동 효율 +1, 멘탈 10 회복", "night_coffee"),
                new RunUpgradeData(RunUpgradeType.LectureNotes, "강의노트", "공부력 +5", "organized_notes"),
                new RunUpgradeData(RunUpgradeType.SummarySheet, "요약본", "복습 +4, 멘탈 15 회복", "summary_sheet"),
                new RunUpgradeData(RunUpgradeType.Highlighter, "형광펜", "공부력 +3, 복습 +2", "highlighter"),
                new RunUpgradeData(RunUpgradeType.AllNighter, "밤샘 버프", "공부력 +8, 공격 효율 +1, 최대 멘탈 -10", "all_nighter")
            };
        }

        public static RunUpgradeData Find(RunUpgradeType type)
        {
            return CreateAll().Find(option => option.type == type);
        }

        public static List<RunUpgradeData> PickThree(int roomNumber, int ownedCount)
        {
            List<RunUpgradeData> all = CreateAll();
            var result = new List<RunUpgradeData>();
            int start = Mathf.Abs(roomNumber * 2 + ownedCount) % all.Count;
            for (int i = 0; i < 3; i++) result.Add(all[(start + i * 2) % all.Count]);
            return result;
        }
    }
}
