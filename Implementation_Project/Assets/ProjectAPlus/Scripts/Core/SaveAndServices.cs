using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ProjectAPlus
{
    public class SaveDataManager : MonoBehaviour
    {
        public SaveData Current { get; private set; }
        public string SavePath { get { return Path.Combine(Application.persistentDataPath, "project_aplus_save.json"); } }
        public string BackupPath { get { return SavePath + ".bak"; } }
        private string TempPath { get { return SavePath + ".tmp"; } }
        public bool HasSave { get { return File.Exists(SavePath) || File.Exists(BackupPath); } }

        public SaveData NewGame()
        {
            Current = new SaveData();
            Current.inventoryItems.Add(new InventoryEntry("energy_jelly", 2));
            Save();
            return Current;
        }

        public SaveData Load()
        {
            if (!HasSave)
            {
                Current = CreateDefault();
                return Current;
            }
            try
            {
                Current = ReadAndNormalize(File.Exists(SavePath) ? SavePath : BackupPath);
                return Current;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Project A+ primary save recovery: " + ex.Message);
                try
                {
                    Current = ReadAndNormalize(BackupPath);
                    Save();
                    return Current;
                }
                catch (Exception backupEx)
                {
                    Debug.LogWarning("Project A+ backup save recovery: " + backupEx.Message);
                    return NewGame();
                }
            }
        }

        public void Save()
        {
            if (Current == null) Current = new SaveData();
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                Current = NormalizeForLoad(Current);
                File.WriteAllText(TempPath, JsonUtility.ToJson(Current, true));
                if (File.Exists(SavePath))
                {
                    try { File.Replace(TempPath, SavePath, BackupPath, true); }
                    catch
                    {
                        File.Copy(SavePath, BackupPath, true);
                        File.Copy(TempPath, SavePath, true);
                        File.Delete(TempPath);
                    }
                }
                else File.Move(TempPath, SavePath);
            }
            catch (Exception ex) { Debug.LogWarning("Project A+ save failed: " + ex.Message); }
        }

        public string CreateSnapshot()
        {
            return JsonUtility.ToJson(NormalizeForLoad(Current ?? CreateDefault()));
        }

        public bool RestoreSnapshot(string json)
        {
            if (string.IsNullOrEmpty(json)) return false;
            try
            {
                Current = NormalizeForLoad(JsonUtility.FromJson<SaveData>(json));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Project A+ checkpoint recovery: " + ex.Message);
                return false;
            }
        }

        public static SaveData NormalizeForLoad(SaveData data)
        {
            if (data == null) throw new InvalidDataException("Save data is empty.");
            data.currentStage = Mathf.Clamp(data.currentStage, 1, 10);
            data.playerLevel = Mathf.Clamp(data.playerLevel, 1, 999);
            data.maxMental = Mathf.Clamp(data.maxMental, 1, 9999);
            data.mental = Mathf.Clamp(data.mental, 1, data.maxMental);
            data.studyPower = Mathf.Clamp(data.studyPower, 1, 9999);
            data.review = Mathf.Clamp(data.review, 0, 9999);
            data.attackEfficiency = Mathf.Clamp(data.attackEfficiency, 0, 999);
            data.movementEfficiency = Mathf.Clamp(data.movementEfficiency, 0, 999);
            data.exp = Mathf.Max(0, data.exp);
            data.growthPoint = Mathf.Max(0, data.growthPoint);
            data.score = Mathf.Max(0, data.score);
            if (!IsFinite(data.runElapsed) || data.runElapsed < 0f) data.runElapsed = 0f;
            if (!GradeManager.IsValidGrade(data.highestGrade)) data.highestGrade = "F";

            var mergedInventory = new Dictionary<string, int>();
            if (data.inventoryItems != null)
            {
                foreach (InventoryEntry entry in data.inventoryItems)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.itemId) || entry.count <= 0) continue;
                    int count;
                    mergedInventory.TryGetValue(entry.itemId, out count);
                    mergedInventory[entry.itemId] = Mathf.Clamp(count + entry.count, 1, 999);
                }
            }
            data.inventoryItems = new List<InventoryEntry>();
            foreach (KeyValuePair<string, int> entry in mergedInventory)
                data.inventoryItems.Add(new InventoryEntry(entry.Key, entry.Value));

            var cleared = new HashSet<int>();
            if (data.clearedStages != null)
                foreach (int stage in data.clearedStages) if (stage >= 1 && stage <= 10) cleared.Add(stage);
            data.clearedStages = new List<int>(cleared);

            var upgrades = new HashSet<string>();
            if (data.runUpgrades != null)
            {
                foreach (string upgrade in data.runUpgrades)
                {
                    RunUpgradeType parsed;
                    if (!string.IsNullOrEmpty(upgrade) && Enum.TryParse(upgrade, out parsed)) upgrades.Add(parsed.ToString());
                }
            }
            data.runUpgrades = new List<string>(upgrades);

            if (data.settings == null) data.settings = new SettingsData();
            if (!IsFinite(data.settings.bgmVolume)) data.settings.bgmVolume = 0.5f;
            if (!IsFinite(data.settings.sfxVolume)) data.settings.sfxVolume = 0.8f;
            data.settings.bgmVolume = Mathf.Clamp01(data.settings.bgmVolume);
            data.settings.sfxVolume = Mathf.Clamp01(data.settings.sfxVolume);
            data.settings.resolutionWidth = Mathf.Clamp(data.settings.resolutionWidth, 640, 7680);
            data.settings.resolutionHeight = Mathf.Clamp(data.settings.resolutionHeight, 360, 4320);
            return data;
        }

        private SaveData ReadAndNormalize(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) throw new FileNotFoundException("Save file was not found.", path);
            return NormalizeForLoad(JsonUtility.FromJson<SaveData>(File.ReadAllText(path)));
        }

        private static SaveData CreateDefault()
        {
            var data = new SaveData();
            data.inventoryItems.Add(new InventoryEntry("energy_jelly", 2));
            return data;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    public class SettingManager : MonoBehaviour
    {
        private readonly List<Vector2Int> resolutions = new List<Vector2Int>
        {
            new Vector2Int(1280, 720), new Vector2Int(1366, 768), new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080), new Vector2Int(2560, 1440), new Vector2Int(3840, 2160)
        };
        public SettingsData Data { get; private set; } = new SettingsData();
        public void Apply(SettingsData data)
        {
            Data = data ?? new SettingsData();
            EnsureResolutionOptions();
            if (Data.resolutionWidth < 640 || Data.resolutionHeight < 360)
            {
                Data.resolutionWidth = 1920;
                Data.resolutionHeight = 1080;
            }
            Screen.SetResolution(Data.resolutionWidth, Data.resolutionHeight, Data.fullscreen);
            if (AudioManager.Instance != null) AudioManager.Instance.ApplyVolumes(Data.bgmVolume, Data.sfxVolume);
        }
        public void SetBgm(float value) { Data.bgmVolume = value; ApplyAndSave(); }
        public void SetSfx(float value) { Data.sfxVolume = value; ApplyAndSave(); }
        public void ToggleFullscreen() { Data.fullscreen = !Data.fullscreen; ApplyAndSave(); }
        public void NextResolution() { ChangeResolution(1); }
        public void PreviousResolution() { ChangeResolution(-1); }
        public string ResolutionLabel() { return Data.resolutionWidth + " x " + Data.resolutionHeight; }
        public string DisplayModeLabel() { return Data.fullscreen ? "전체화면" : "창 모드"; }

        private void ChangeResolution(int direction)
        {
            EnsureResolutionOptions();
            int current = 0;
            int bestDistance = int.MaxValue;
            for (int i = 0; i < resolutions.Count; i++)
            {
                int distance = Mathf.Abs(resolutions[i].x - Data.resolutionWidth) + Mathf.Abs(resolutions[i].y - Data.resolutionHeight);
                if (distance < bestDistance) { current = i; bestDistance = distance; }
            }
            current = (current + direction + resolutions.Count) % resolutions.Count;
            Data.resolutionWidth = resolutions[current].x;
            Data.resolutionHeight = resolutions[current].y;
            ApplyAndSave();
        }

        private void EnsureResolutionOptions()
        {
            foreach (Resolution option in Screen.resolutions)
            {
                var candidate = new Vector2Int(option.width, option.height);
                if (candidate.x >= 960 && candidate.y >= 540 && !resolutions.Contains(candidate))
                    resolutions.Add(candidate);
            }
            resolutions.Sort((left, right) =>
            {
                int pixels = left.x * left.y - right.x * right.y;
                return pixels != 0 ? pixels : left.x - right.x;
            });
        }
        private void ApplyAndSave()
        {
            Apply(Data);
            if (GameManager.Instance != null && GameManager.Instance.Save != null)
            {
                GameManager.Instance.Save.Current.settings = Data;
                GameManager.Instance.Save.Save();
            }
        }
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        private AudioSource sfxSource;
        private AudioSource bgmSource;
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            Instance = this;
            sfxSource = gameObject.AddComponent<AudioSource>();
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            Create("attack", 520, 0.07f);
            Create("hit", 130, 0.11f);
            Create("jump", 360, 0.08f);
            Create("item", 660, 0.12f);
            Create("levelUp", 880, 0.24f);
            Create("bossWarning", 190, 0.15f);
            Create("clear", 740, 0.3f);
            Create("gameOver", 90, 0.4f);
        }

        public void Play(string id)
        {
            AudioClip clip;
            if (clips.TryGetValue(id, out clip)) sfxSource.PlayOneShot(clip);
        }
        public void ApplyVolumes(float bgm, float sfx) { bgmSource.volume = Mathf.Clamp01(bgm); sfxSource.volume = Mathf.Clamp01(sfx); }

        private void Create(string id, float frequency, float seconds)
        {
            int sampleRate = 22050;
            int length = Mathf.CeilToInt(sampleRate * seconds);
            var samples = new float[length];
            for (int i = 0; i < length; i++) samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * (1f - i / (float)length) * 0.22f;
            var clip = AudioClip.Create(id, length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            clips[id] = clip;
        }
    }

    public class GameStateManager : MonoBehaviour
    {
        public GameState Current { get; private set; }
        public void Set(GameState state) { Current = state; }
    }

    public class SceneLoader : MonoBehaviour
    {
        public void ReloadCurrentStage() { if (GameManager.Instance != null) GameManager.Instance.RetryStage(); }
    }
}
