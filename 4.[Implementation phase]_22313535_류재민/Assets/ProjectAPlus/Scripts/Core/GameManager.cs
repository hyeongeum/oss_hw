using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAPlus
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState State { get; private set; }
        public SaveDataManager Save { get; private set; }
        public SettingManager Settings { get; private set; }
        public StageManager Stage { get; private set; }
        public UIManager UI { get; private set; }
        public PlayerStatus Player { get; private set; }
        public Dictionary<string, ItemData> Items { get; private set; }
        public List<StageData> Stages { get; private set; }
        public float RunElapsed { get; private set; }
        private GameStateManager stateManager;
        private int stageToLoad;
        private string stageCheckpointJson;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (FindObjectOfType<GameManager>() != null) return;
            var root = new GameObject("Project A+ Runtime");
            root.AddComponent<GameManager>();
        }

        private void Awake()
        {
            if (!Application.isPlaying) return;
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CombatLayers.Configure();
            EnsureCamera();
            stateManager = gameObject.AddComponent<GameStateManager>();
            gameObject.AddComponent<BattleManager>();
            gameObject.AddComponent<AudioManager>();
            Save = gameObject.AddComponent<SaveDataManager>();
            Settings = gameObject.AddComponent<SettingManager>();
            Stage = gameObject.AddComponent<StageManager>();
            UI = gameObject.AddComponent<UIManager>();
            Items = StageCatalog.CreateItems();
            Stages = StageCatalog.CreateAll();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectValidator.LogResult();
#endif
            Save.Load();
            Settings.Apply(Save.Current.settings);
            UI.Initialize();
            ShowTitle();
            Debug.Log("Project A+ title ready at " + Screen.width + "x" + Screen.height + ".");
        }

        private void Update()
        {
            if (State == GameState.Playing) RunElapsed += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (State == GameState.Playing) PauseGame();
                else if (State == GameState.Paused) ResumeGame();
            }
            if ((Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I)) && (State == GameState.Playing || State == GameState.Paused))
                UI.ToggleAbility();
        }

        private void EnsureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                camera = go.AddComponent<Camera>();
            }
            if (camera.GetComponent<AudioListener>() == null) camera.gameObject.AddComponent<AudioListener>();
            DontDestroyOnLoad(camera.gameObject);
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.transform.position = new Vector3(8.5f, 4.3f, -10f);
            if (camera.GetComponent<PixelPerfectCamera>() == null) camera.gameObject.AddComponent<PixelPerfectCamera>();
        }

        public void StartGame()
        {
            Save.NewGame();
            RunElapsed = 0f;
            stageCheckpointJson = null;
            stageToLoad = 1;
            SetState(GameState.Opening);
            UI.PlayCutscene(new[]
            {
                "새 학기가 시작되었다.",
                "이번 목표는 단 하나, A+.",
                "10개의 방과 두 번의 시험을 버티며 한 학기를 완주하자."
            }, delegate { LoadStage(stageToLoad); }, false);
        }

        public void ContinueGame()
        {
            Save.Load();
            RunElapsed = Mathf.Max(0f, Save.Current.runElapsed);
            LoadStage(Mathf.Clamp(Save.Current.currentStage, 1, 10));
        }

        public void LoadStage(int number)
        {
            LoadStageInternal(number, true);
        }

        private void LoadStageInternal(int number, bool establishCheckpoint)
        {
            Time.timeScale = 1f;
            number = Mathf.Clamp(number, 1, 10);
            Save.Current.currentStage = number;
            SetState(GameState.Playing);
            UI.ShowHud();
            Stage.Load(Stages[number - 1]);
            SaveProgress();
            if (establishCheckpoint) stageCheckpointJson = Save.CreateSnapshot();
            UI.Toast("Stage " + number + "  " + Stages[number - 1].stageName);
        }

        public void CreatePlayer(Vector2 position)
        {
            if (Player != null) Destroy(Player.gameObject);
            var go = new GameObject("Player");
            go.layer = CombatLayers.PlayerBody;
            go.transform.position = position;
            StableVisual.AttachSprite(go, RuntimeArt.GetPlayer(), 1f, 10, "Player Visual", Vector2.zero);
            var body = go.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.gravityScale = GameBalance.PlayerRiseGravityScale;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.72f, 1.35f);
            collider.offset = new Vector2(0f, -0.03f);
            Player = go.AddComponent<PlayerStatus>();
            go.AddComponent<PlayerGrowth>();
            go.AddComponent<PlayerInventory>();
            go.AddComponent<PlayerController>();
            go.AddComponent<PlayerCombat>();
            var hitHandler = go.AddComponent<PlayerHitHandler>();
            go.AddComponent<PlayerVisualAnimator>();
            CombatGeometry.AttachHurtbox(go, CombatTeam.Player, new Vector2(0.72f, 1.35f), new Vector2(0f, -0.03f), hitHandler);
            Player.ApplySave(Save.Current);
            go.GetComponent<PlayerInventory>().ApplySave(Save.Current.inventoryItems);
        }

        public void PauseGame()
        {
            if (State != GameState.Playing) return;
            SetState(GameState.Paused);
            Time.timeScale = 0f;
            UI.ShowPause(true);
        }

        public void ResumeGame()
        {
            if (State != GameState.Paused) return;
            SetState(GameState.Playing);
            Time.timeScale = 1f;
            UI.ShowPause(false);
            UI.HideAbility();
        }

        public void GameOver(string reason = "멘탈이 바닥났습니다.")
        {
            if (State == GameState.GameOver) return;
            SetState(GameState.GameOver);
            Time.timeScale = 0f;
            if (AudioManager.Instance != null) AudioManager.Instance.Play("gameOver");
            UI.ShowGameOver(reason);
        }

        public void RetryStage()
        {
            Time.timeScale = 1f;
            if (Save.Current != null)
            {
                if (!string.IsNullOrEmpty(stageCheckpointJson)) Save.RestoreSnapshot(stageCheckpointJson);
                Save.Current.mental = Save.Current.maxMental;
                RunElapsed = Mathf.Max(0f, Save.Current.runElapsed);
                LoadStageInternal(Stage.Current != null ? Stage.Current.stageNumber : Save.Current.currentStage, false);
            }
        }

        public void ClearStage()
        {
            if (Player == null || Stage.Current == null) return;
            var hit = Player.GetComponent<PlayerHitHandler>();
            var inventory = Player.GetComponent<PlayerInventory>();
            StageResult result = GradeManager.Calculate(Stage.Current, Player, Stage.Elapsed, Stage.Kills, hit.DamageTaken, inventory.ItemsUsed);
            Player.GetComponent<PlayerGrowth>().AddExp(Stage.Current.baseRewardExp);
            Player.growthPoint += Stage.Current.growthPointReward;
            Player.score += result.score;
            ItemData rewardData = null;
            if (!string.IsNullOrEmpty(Stage.Current.rewardItemId))
            {
                inventory.AddItem(Stage.Current.rewardItemId);
                if (Items.TryGetValue(Stage.Current.rewardItemId, out rewardData)) UI.Toast("ITEM GET! " + rewardData.itemName);
            }
            if (!Save.Current.clearedStages.Contains(Stage.Current.stageNumber)) Save.Current.clearedStages.Add(Stage.Current.stageNumber);
            Save.Current.currentStage = Mathf.Min(10, Stage.Current.nextStageNumber);
            SaveProgress();
            if (AudioManager.Instance != null) AudioManager.Instance.Play("clear");
            if (Stage.Current.stageNumber == 10)
            {
                SetState(GameState.Ending);
                UI.PlayCutscene(new[]
                {
                    "모든 시험이 끝났다.",
                    "수많은 과제와 밤샘을 넘어...",
                    "너는 이번 학기를 끝까지 버텨냈다.",
                    "최종 성적 발표"
                }, ShowFinalGrade, true);
            }
            else
            {
                SetState(GameState.Result);
                UI.ShowResult(result);
            }
        }

        public void GoToNextStage()
        {
            if (Stage.Current == null) return;
            LoadStage(Mathf.Min(10, Stage.Current.nextStageNumber));
        }

        private void ShowFinalGrade()
        {
            SetState(GameState.FinalGrade);
            string grade = GradeManager.FinalGrade(Player != null ? Player.score : Save.Current.score);
            Save.Current.highestGrade = GradeManager.BestGrade(Save.Current.highestGrade, grade);
            SaveProgress();
            UI.ShowFinalGrade(grade, Save.Current.score);
        }

        public void SaveProgress()
        {
            if (Save == null || Save.Current == null) return;
            if (Player != null)
            {
                Player.WriteSave(Save.Current);
                Save.Current.inventoryItems = Player.GetComponent<PlayerInventory>().CopyForSave();
                if (State == GameState.GameOver && Save.Current.mental <= 0) Save.Current.mental = Save.Current.maxMental;
            }
            Save.Current.settings = Settings.Data;
            Save.Current.runElapsed = RunElapsed;
            Save.Save();
        }

        public void ShowTitle()
        {
            if (State != GameState.Title) SaveProgress();
            Time.timeScale = 1f;
            SetState(GameState.Title);
            UI.ShowTitle(Save != null && Save.HasSave);
        }

        public void QuitGame()
        {
            SaveProgress();
            Application.Quit();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveProgress();
        }

        private void OnApplicationQuit()
        {
            SaveProgress();
        }

        private void SetState(GameState state)
        {
            State = state;
            if (stateManager != null) stateManager.Set(state);
        }
    }
}
