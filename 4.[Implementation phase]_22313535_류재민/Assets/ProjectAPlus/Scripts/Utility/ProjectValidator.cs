using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectAPlus
{
    public static class ProjectValidator
    {
        public static List<string> ValidateCore()
        {
            var errors = new List<string>();
            if (DamageCalculator.CalculatePlayerDamageDeterministic(1, 0, 999, -3, false) < 0)
                errors.Add("DamageCalculator returned negative damage.");

            var stages = StageCatalog.CreateAll();
            if (stages.Count != 10) errors.Add("Stage catalog must contain exactly 10 stages.");
            int[] bosses = stages.Where(s => s.bossData != null).Select(s => s.stageNumber).ToArray();
            if (!bosses.SequenceEqual(new[] { 5, 10 })) errors.Add("Boss data must exist only at Stage 5 and Stage 10.");
            float targetRunMinutes = GameBalance.TargetRunMinutes;
            if (!Mathf.Approximately(targetRunMinutes, 30f)) errors.Add("Target run time must be 30 minutes.");
            if (GameBalance.MaxJumpRise() < GameBalance.SafePlatformRise + 0.8f)
                errors.Add("Player jump height does not have enough safety margin for platform layout.");
            float targetSeconds = stages.Sum(stage => stage.timeLimit > 0 ? stage.timeLimit : GameBalance.TargetRoomSeconds(stage.stageNumber));
            if (targetSeconds < 27f * 60f || targetSeconds > 33f * 60f) errors.Add("Combined room target time must stay near 30 minutes.");

            var save = new SaveData { currentStage = 7, studyPower = 42, attackEfficiency = 8, runElapsed = 321f };
            save.runUpgrades.Add(RunUpgradeType.Coffee.ToString());
            var loaded = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(save));
            if (loaded == null || loaded.currentStage != 7 || loaded.studyPower != 42 || loaded.attackEfficiency != 8
                || loaded.runElapsed != 321f || loaded.runUpgrades == null || loaded.runUpgrades.Count != 1)
                errors.Add("SaveData JSON round-trip failed.");

            var corrupt = new SaveData
            {
                currentStage = 99,
                mental = -10,
                maxMental = -1,
                highestGrade = "Z",
                runElapsed = float.NaN,
                settings = new SettingsData { bgmVolume = float.NaN, sfxVolume = 5f, resolutionWidth = 1, resolutionHeight = 1 }
            };
            corrupt.inventoryItems.Add(null);
            corrupt.inventoryItems.Add(new InventoryEntry("", -2));
            corrupt.inventoryItems.Add(new InventoryEntry("energy_jelly", 2));
            corrupt.inventoryItems.Add(new InventoryEntry("energy_jelly", 3));
            corrupt.clearedStages.AddRange(new[] { -1, 3, 3, 12 });
            corrupt = SaveDataManager.NormalizeForLoad(corrupt);
            if (corrupt.currentStage != 10 || corrupt.mental != 1 || corrupt.maxMental != 1 || corrupt.highestGrade != "F"
                || corrupt.runElapsed != 0f || corrupt.inventoryItems.Count != 1 || corrupt.inventoryItems[0].count != 5
                || corrupt.clearedStages.Count != 1 || corrupt.clearedStages[0] != 3
                || corrupt.settings.bgmVolume != 0.5f || corrupt.settings.sfxVolume != 1f
                || corrupt.settings.resolutionWidth != 640 || corrupt.settings.resolutionHeight != 360)
                errors.Add("Corrupt SaveData normalization failed.");
            if (GradeManager.BestGrade("A", "B+") != "A" || GradeManager.BestGrade("C", "A+") != "A+")
                errors.Add("Highest grade preservation failed.");

            var status = new SaveData { growthPoint = 0, studyPower = 14 };
            if (status.growthPoint < 0 || status.studyPower != 14) errors.Add("Default growth data is invalid.");
            return errors;
        }

        public static void LogResult()
        {
            List<string> errors = ValidateCore();
            if (errors.Count == 0) Debug.Log("Project A+ core validation passed.");
            else foreach (string error in errors) Debug.LogError("Project A+ validation: " + error);
        }
    }
}
