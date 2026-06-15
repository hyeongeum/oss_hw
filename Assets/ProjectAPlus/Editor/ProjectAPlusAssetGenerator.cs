#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAPlus.Editor
{
    [InitializeOnLoad]
    public static class ProjectAPlusAssetGenerator
    {
        private static readonly string[] Folders =
        {
            "Assets/ProjectAPlus/Scripts", "Assets/ProjectAPlus/Scenes", "Assets/ProjectAPlus/Prefabs",
            "Assets/ProjectAPlus/Sprites", "Assets/ProjectAPlus/Sprites/Player", "Assets/ProjectAPlus/Sprites/Enemies",
            "Assets/ProjectAPlus/Sprites/Bosses", "Assets/ProjectAPlus/Sprites/Backgrounds", "Assets/ProjectAPlus/Sprites/UI",
            "Assets/ProjectAPlus/Sprites/Effects", "Assets/ProjectAPlus/Data", "Assets/ProjectAPlus/Audio", "Assets/ProjectAPlus/Resources",
            "Assets/ProjectAPlus/Resources/FreshPixelArt", "Assets/ProjectAPlus/Resources/FreshPixelArt/Backgrounds"
        };

        static ProjectAPlusAssetGenerator()
        {
            EditorApplication.delayCall += EnsureScenes;
            if (SessionState.GetBool("ProjectAPlusStageSmoke", false))
            {
                EditorApplication.update -= StageOneSmokeUpdate;
                EditorApplication.update += StageOneSmokeUpdate;
            }
            if (SessionState.GetBool("ProjectAPlusAllStagesSmoke", false))
            {
                EditorApplication.update -= AllStagesSmokeUpdate;
                EditorApplication.update += AllStagesSmokeUpdate;
            }
            if (SessionState.GetBool("ProjectAPlusCompleteFlowSmoke", false))
            {
                EditorApplication.update -= CompleteFlowSmokeUpdate;
                EditorApplication.update += CompleteFlowSmokeUpdate;
            }
            if (SessionState.GetBool("ProjectAPlusCaptureFreshPreview", false))
            {
                EditorApplication.update -= CaptureFreshPreviewUpdate;
                EditorApplication.update += CaptureFreshPreviewUpdate;
            }
            if (SessionState.GetBool("ProjectAPlusSubmissionSmoke", false))
            {
                EditorApplication.update -= SubmissionSmokeUpdate;
                EditorApplication.update += SubmissionSmokeUpdate;
            }
        }

        [MenuItem("Tools/Project A+/Open Boot Scene")]
        public static void OpenBootScene()
        {
            EnsureScenes();
            EditorSceneManager.OpenScene("Assets/ProjectAPlus/Scenes/BootScene.unity", OpenSceneMode.Single);
        }

        [MenuItem("Tools/Project A+/Generate All Assets")]
        public static void GenerateAllAssets()
        {
            EnsureFolders();
            ProvidedArtSheetImporter.Generate();
            GeneratePlayerSprites();
            GenerateEnemySprites();
            GenerateBossSprites();
            GenerateItemSprites();
            GenerateEffectSprites();
            ImportFreshPixelArt();
            GeneratePrefabs();
            WriteStageSummary();
            EnsureScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Project A+: all offline pixel assets and scenes generated.");
        }

        [MenuItem("Tools/Project A+/Validate Project")]
        public static void ValidateProject()
        {
            var stages = StageCatalog.CreateAll();
            bool valid = stages.Count == 10 && stages.FindAll(s => s.bossData != null).Count == 2 && stages[4].bossData != null && stages[9].bossData != null;
            string[] scripts = Directory.GetFiles("Assets/ProjectAPlus", "*.cs", SearchOption.AllDirectories);
            bool networkFree = true;
            string[] forbidden = { "Unity" + "WebRequest", "System" + ".Net", "Http" + "Client" };
            foreach (string file in scripts)
            {
                string text = File.ReadAllText(file);
                foreach (string token in forbidden) if (text.Contains(token)) networkFree = false;
            }
            Debug.Log("Project A+ validation - stages: " + valid + ", network-free: " + networkFree + ", scripts: " + scripts.Length);
        }

        [MenuItem("Tools/Project A+/Build Windows Game")]
        public static void BuildWindowsGame()
        {
            GenerateAllAssets();
            PlayerSettings.companyName = "Project A+ Team";
            PlayerSettings.productName = "Project A+";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            Directory.CreateDirectory("outputs/ProjectAPlus_Windows");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/ProjectAPlus/Scenes/BootScene.unity" },
                locationPathName = "outputs/ProjectAPlus_Windows/Project A+.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Project A+ Windows build failed: " + report.summary.result);
            Debug.Log("Project A+ Windows build ready: " + Path.GetFullPath(options.locationPathName));
        }

        public static void RunPlayModeSmokeTest()
        {
            OpenBootScene();
            EditorApplication.isPlaying = true;
        }

        public static void RunStageOneSmokeTest()
        {
            OpenBootScene();
            SessionState.SetBool("ProjectAPlusStageSmoke", true);
            EditorApplication.update -= StageOneSmokeUpdate;
            EditorApplication.update += StageOneSmokeUpdate;
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Tools/Project A+/Capture Fresh Stage Preview")]
        public static void CaptureFreshStagePreview()
        {
            CaptureStagePreview(1);
        }

        [MenuItem("Tools/Project A+/Capture Boss Stage Preview")]
        public static void CaptureBossStagePreview()
        {
            CaptureStagePreview(10);
        }

        private static void CaptureStagePreview(int stage)
        {
            OpenBootScene();
            SessionState.SetBool("ProjectAPlusCaptureFreshPreview", true);
            SessionState.SetInt("ProjectAPlusCaptureFreshPreviewPhase", 0);
            SessionState.SetInt("ProjectAPlusCaptureStage", Mathf.Clamp(stage, 1, 10));
            EditorApplication.update -= CaptureFreshPreviewUpdate;
            EditorApplication.update += CaptureFreshPreviewUpdate;
            EditorApplication.isPlaying = true;
        }

        private static double freshPreviewNext;

        private static void CaptureFreshPreviewUpdate()
        {
            if (!EditorApplication.isPlaying || GameManager.Instance == null) return;
            int phase = SessionState.GetInt("ProjectAPlusCaptureFreshPreviewPhase", 0);
            if (EditorApplication.timeSinceStartup < freshPreviewNext) return;
            if (phase == 0)
            {
                GameManager.Instance.LoadStage(SessionState.GetInt("ProjectAPlusCaptureStage", 1));
                SessionState.SetInt("ProjectAPlusCaptureFreshPreviewPhase", 1);
                freshPreviewNext = EditorApplication.timeSinceStartup + 2.5f;
                return;
            }
            if (phase == 1)
            {
                if (GameManager.Instance.Player != null)
                    GameManager.Instance.Player.transform.position = new Vector2(
                        SessionState.GetInt("ProjectAPlusCaptureStage", 1) == 10 ? 43f : 26f, 1.4f);
                SessionState.SetInt("ProjectAPlusCaptureFreshPreviewPhase", 2);
                freshPreviewNext = EditorApplication.timeSinceStartup + 1.25f;
                return;
            }
            if (phase == 2)
            {
                Directory.CreateDirectory("Logs");
                bool boss = SessionState.GetInt("ProjectAPlusCaptureStage", 1) == 10;
                CaptureMainCamera(Path.GetFullPath(boss ? "Logs/BossStagePreview.png" : "Logs/FreshStagePreview.png"));
                SessionState.SetInt("ProjectAPlusCaptureFreshPreviewPhase", 3);
                freshPreviewNext = EditorApplication.timeSinceStartup + 0.5f;
                return;
            }
            SessionState.SetBool("ProjectAPlusCaptureFreshPreview", false);
            EditorApplication.update -= CaptureFreshPreviewUpdate;
            Debug.Log("Project A+ fresh stage preview captured.");
            EditorApplication.Exit(0);
        }

        private static void CaptureMainCamera(string path)
        {
            Camera camera = Camera.main;
            if (camera == null) return;
            const int width = 1920;
            const int height = 1080;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            camera.targetTexture = target;
            RenderTexture.active = target;
            camera.Render();
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(texture);
        }

        private static double stageSmokeStarted;

        private static void StageOneSmokeUpdate()
        {
            if (!EditorApplication.isPlaying || GameManager.Instance == null) return;
            if (GameManager.Instance.Stage.Current == null)
            {
                GameManager.Instance.LoadStage(1);
                stageSmokeStarted = EditorApplication.timeSinceStartup;
                return;
            }
            if (EditorApplication.timeSinceStartup - stageSmokeStarted < 1.5f) return;
            int enemies = Object.FindObjectsOfType<EnemyController>().Length;
            int healthBars = Object.FindObjectsOfType<WorldHealthBar>().Length;
            bool passed = GameManager.Instance.Player != null && enemies >= 3 && healthBars >= enemies && GameObject.Find("Stage_1") != null;
            Debug.Log("Project A+ polished Stage 1 smoke: " + passed + ", enemies=" + enemies + ", healthBars=" + healthBars);
            SessionState.SetBool("ProjectAPlusStageSmoke", false);
            EditorApplication.update -= StageOneSmokeUpdate;
            EditorApplication.Exit(passed ? 0 : 1);
        }

        public static void RunAllStagesSmokeTest()
        {
            OpenBootScene();
            SessionState.SetBool("ProjectAPlusAllStagesSmoke", true);
            SessionState.SetInt("ProjectAPlusSmokeStage", 1);
            contactProbeStarted = false;
            contactProbeMental = 0;
            fallRecoveryProbeStarted = false;
            fallRecoveryProbeEnemy = null;
            jumpProbeStarted = false;
            jumpProbeStartY = 0f;
            jumpProbePeakY = 0f;
            allStagesSmokeLoaded = false;
            EditorApplication.update -= AllStagesSmokeUpdate;
            EditorApplication.update += AllStagesSmokeUpdate;
            EditorApplication.isPlaying = true;
        }

        private static double allStagesSmokeNext;
        private static bool allStagesSmokeLoaded;
        private static bool contactProbeStarted;
        private static int contactProbeMental;
        private static bool fallRecoveryProbeStarted;
        private static EnemyController fallRecoveryProbeEnemy;
        private static bool jumpProbeStarted;
        private static float jumpProbeStartY;
        private static float jumpProbePeakY;

        private static void AllStagesSmokeUpdate()
        {
            if (!EditorApplication.isPlaying || GameManager.Instance == null) return;
            int stage = SessionState.GetInt("ProjectAPlusSmokeStage", 1);
            if (jumpProbeStarted && stage == 1 && GameManager.Instance.Player != null)
                jumpProbePeakY = Mathf.Max(jumpProbePeakY, GameManager.Instance.Player.transform.position.y);
            if (!allStagesSmokeLoaded)
            {
                GameManager.Instance.LoadStage(stage);
                allStagesSmokeLoaded = true;
                allStagesSmokeNext = EditorApplication.timeSinceStartup + 0.35f;
                return;
            }
            if (EditorApplication.timeSinceStartup < allStagesSmokeNext) return;

            StageData data = GameManager.Instance.Stage.Current;
            int enemies = Object.FindObjectsOfType<EnemyController>().Length;
            if (stage == 1 && !contactProbeStarted && enemies > 0)
            {
                EnemyController contactEnemy = Object.FindObjectOfType<EnemyController>();
                contactProbeMental = GameManager.Instance.Player.mental;
                contactEnemy.transform.position = GameManager.Instance.Player.transform.position;
                Rigidbody2D contactBody = contactEnemy.GetComponent<Rigidbody2D>();
                if (contactBody != null) contactBody.velocity = Vector2.zero;
                contactProbeStarted = true;
                allStagesSmokeNext = EditorApplication.timeSinceStartup + 0.4f;
                return;
            }
            if (stage == 1 && contactProbeStarted && !jumpProbeStarted)
            {
                foreach (EnemyController enemy in Object.FindObjectsOfType<EnemyController>())
                    enemy.transform.position += Vector3.right * 8f;

                Transform playerTransform = GameManager.Instance.Player.transform;
                playerTransform.position = new Vector2(2f, 0.8f);
                Rigidbody2D playerBody = playerTransform.GetComponent<Rigidbody2D>();
                if (playerBody != null)
                {
                    playerBody.gravityScale = GameBalance.PlayerRiseGravityScale;
                    playerBody.velocity = new Vector2(0f, GameBalance.BaseJumpPower
                        + GameManager.Instance.Player.movementEfficiency * 0.18f);
                }
                jumpProbeStartY = playerTransform.position.y;
                jumpProbePeakY = jumpProbeStartY;
                jumpProbeStarted = true;
                allStagesSmokeNext = EditorApplication.timeSinceStartup + 0.85f;
                return;
            }
            if (stage == 2 && !fallRecoveryProbeStarted && enemies > 0)
            {
                fallRecoveryProbeEnemy = Object.FindObjectOfType<EnemyController>();
                fallRecoveryProbeEnemy.transform.position = new Vector2(fallRecoveryProbeEnemy.transform.position.x, -4f);
                Rigidbody2D fallBody = fallRecoveryProbeEnemy.GetComponent<Rigidbody2D>();
                if (fallBody != null) fallBody.velocity = Vector2.down * 4f;
                fallRecoveryProbeStarted = true;
                allStagesSmokeNext = EditorApplication.timeSinceStartup + 0.4f;
                return;
            }
            int bosses = Object.FindObjectsOfType<BossController>().Length;
            int hazards = Object.FindObjectsOfType<StageHazard>().Length;
            int movingPlatforms = Object.FindObjectsOfType<MovingStudyPlatform>().Length;
            int roomGates = Object.FindObjectsOfType<StageRoomGate>().Length;
            int hurtboxes = Object.FindObjectsOfType<Hurtbox>().Length;
            int contactDamage = Object.FindObjectsOfType<EnemyContactDamage>().Length;
            int terrainPieces = Object.FindObjectsOfType<StageTerrainPiece>().Length;
            int decorations = Object.FindObjectsOfType<StageDecoration>().Length;
            StageChamberMarker[] chamberMarkers = Object.FindObjectsOfType<StageChamberMarker>();
            int chambers = chamberMarkers.Length;
            var uniqueChamberPatterns = new HashSet<string>();
            foreach (StageChamberMarker marker in chamberMarkers)
                if (!string.IsNullOrEmpty(marker.Pattern)) uniqueChamberPatterns.Add(marker.Pattern);
            int oneWayPlatforms = Object.FindObjectsOfType<PlatformEffector2D>().Length;
            WalkableSurfaceMarker[] walkableSurfaces = Object.FindObjectsOfType<WalkableSurfaceMarker>();
            int mainFloors = 0;
            foreach (WalkableSurfaceMarker surface in walkableSurfaces)
                if (surface.IsMainFloor) mainFloors++;
            int reachableSurfaces;
            float highestRequiredRise;
            bool platformReachability = AuditPlatformReachability(walkableSurfaces, out reachableSurfaces, out highestRequiredRise);
            bool detailedBackground = Object.FindObjectOfType<CameraBackdrop>() != null;
            bool legacyArtRemoved = !Directory.Exists("Assets/ProjectAPlus/Resources/GeneratedBackgrounds")
                && !Directory.Exists("Assets/ProjectAPlus/Resources/ProductionSprites")
                && !Directory.Exists("Assets/ProjectAPlus/Resources/StableSprites");
            bool productionArt = Resources.Load<Sprite>("FreshPixelArt/Backgrounds/CampusLectureDungeon") != null
                && Resources.Load<Sprite>("FreshPixelArt/Backgrounds/MidnightDataLibrary") != null
                && Resources.Load<Sprite>("FreshPixelArt/Backgrounds/FinalExamArchive") != null
                && GameManager.Instance.Player != null
                && GameManager.Instance.Player.GetComponent<PlayerVisualAnimator>() != null
                && RuntimeArt.GetPlayer().name.StartsWith("idle_")
                && RuntimeArt.GetEnemy(EnemyType.SleepSlime).name.StartsWith("fresh_enemy_")
                && RuntimeArt.GetBoss(10).name.StartsWith("fresh_boss_");
            PlayerVisualAnimator playerAnimator = GameManager.Instance.Player != null ? GameManager.Instance.Player.GetComponent<PlayerVisualAnimator>() : null;
            StableSpriteVisual[] stableVisualComponents = Object.FindObjectsOfType<StableSpriteVisual>();
            bool stableVisuals = stableVisualComponents.Length >= enemies + bosses + 1;
            foreach (StableSpriteVisual visual in stableVisualComponents) stableVisuals &= visual.IsStable;
            CameraBackdrop cameraBackdrop = Object.FindObjectOfType<CameraBackdrop>();
            bool stablePlayerVisual = playerAnimator != null
                && Mathf.Abs(playerAnimator.CurrentVisualScale - 1f) < 0.001f
                && GameManager.Instance.Player.transform.localScale == Vector3.one
                && GameManager.Instance.Player.GetComponentInChildren<StableSpriteVisual>() != null;
            bool stableBackdrop = cameraBackdrop != null && cameraBackdrop.IsUniform
                && cameraBackdrop.GetComponent<SpriteRenderer>() != null
                && cameraBackdrop.GetComponent<SpriteRenderer>().sprite != null
                && (cameraBackdrop.GetComponent<SpriteRenderer>().sprite.name == "CampusLectureDungeon"
                    || cameraBackdrop.GetComponent<SpriteRenderer>().sprite.name == "MidnightDataLibrary"
                    || cameraBackdrop.GetComponent<SpriteRenderer>().sprite.name == "FinalExamArchive");
            bool isBossStage = stage == 5 || stage == 10;
            SpriteRenderer livePlayer = GameManager.Instance.Player != null ? GameManager.Instance.Player.GetComponentInChildren<SpriteRenderer>() : null;
            SpriteRenderer liveBoss = isBossStage && bosses > 0 ? Object.FindObjectOfType<BossController>().GetComponentInChildren<SpriteRenderer>() : null;
            bool liveEnemyUsesProductionArt = isBossStage;
            if (!isBossStage)
            {
                foreach (EnemyController liveEnemyController in Object.FindObjectsOfType<EnemyController>())
                {
                    SpriteRenderer liveEnemy = liveEnemyController.GetComponentInChildren<SpriteRenderer>();
                    if (liveEnemy != null && liveEnemy.sprite != null && liveEnemy.sprite.name.StartsWith("fresh_enemy_"))
                    {
                        liveEnemyUsesProductionArt = true;
                        break;
                    }
                }
            }
            bool productionArtLive = livePlayer != null && livePlayer.sprite != null && !livePlayer.sprite.name.StartsWith("player_student_dark_academia")
                && (isBossStage ? liveBoss != null && liveBoss.sprite != null && liveBoss.sprite.name.StartsWith("fresh_boss_")
                    : liveEnemyUsesProductionArt);
            Canvas liveCanvas = Object.FindObjectOfType<Canvas>();
            int activeImages = Object.FindObjectsOfType<Image>().Length;
            bool pixelUiFrame = false;
            bool pixelUiSlot = false;
            foreach (Image image in Object.FindObjectsOfType<Image>())
            {
                if (image.sprite == null) continue;
                if (image.sprite.name.StartsWith("fresh_ui_frame")) pixelUiFrame = true;
                if (image.sprite.name.StartsWith("fresh_ui_slot")) pixelUiSlot = true;
            }
            bool pixelCamera = Camera.main != null && Camera.main.GetComponent<PixelPerfectCamera>() != null;
            bool pixelCanvas = liveCanvas != null && liveCanvas.pixelPerfect && liveCanvas.GetComponent<PixelCanvasScale>() != null;
            bool pixelRendering = pixelCamera && pixelCanvas && pixelUiFrame && pixelUiSlot && activeImages >= 8;
            bool passThroughPhysics = Physics2D.GetIgnoreLayerCollision(CombatLayers.PlayerBody, CombatLayers.EnemyBody)
                && Physics2D.GetIgnoreLayerCollision(CombatLayers.EnemyBody, CombatLayers.EnemyBody)
                && Physics2D.GetIgnoreLayerCollision(CombatLayers.EnemyBody, CombatLayers.PlayerGate)
                && GameManager.Instance.Player.gameObject.layer == CombatLayers.PlayerBody;
            bool enemyScaleValid = true;
            EnemyScaleMarker[] enemyScaleMarkers = Object.FindObjectsOfType<EnemyScaleMarker>();
            foreach (EnemyScaleMarker marker in enemyScaleMarkers)
            {
                float expectedScale = marker.IsElite ? 1.5f : 1f;
                Vector2 expectedBody = new Vector2(0.9f, 1.35f) * expectedScale;
                BoxCollider2D bodyCollider = marker.GetComponent<BoxCollider2D>();
                Hurtbox markerHurtbox = marker.GetComponentInChildren<Hurtbox>();
                StableSpriteVisual stableVisual = marker.GetComponentInChildren<StableSpriteVisual>();
                enemyScaleValid &= Mathf.Abs(marker.UniformScale - expectedScale) < 0.001f
                    && Vector2.Distance(marker.BodySize, expectedBody) < 0.001f
                    && bodyCollider != null && Vector2.Distance(bodyCollider.size, expectedBody) < 0.001f
                    && markerHurtbox != null && markerHurtbox.GetComponent<BoxCollider2D>() != null
                    && Vector2.Distance(markerHurtbox.GetComponent<BoxCollider2D>().size, expectedBody) < 0.001f
                    && stableVisual != null && Mathf.Abs(stableVisual.UniformScale - expectedScale) < 0.001f;
            }
            bool bossCameraScroll = !isBossStage
                || Mathf.Abs(GameManager.Instance.Stage.CameraFocusX(10f) - GameManager.Instance.Stage.CameraFocusX(GameBalance.StageWidth - 10f)) > 20f;
            bool providedArtValid = stage != 1 || ProvidedArtSheetImporter.ValidateGeneratedArt();
            bool contactDamageTriggered = stage != 1 || (contactProbeStarted && GameManager.Instance.Player.mental < contactProbeMental);
            bool enemiesAboveFloor = true;
            bool[] occupiedChambers = new bool[4];
            foreach (EnemyController enemy in Object.FindObjectsOfType<EnemyController>())
            {
                enemiesAboveFloor &= enemy.transform.position.y >= -2f;
                float x = enemy.transform.position.x;
                int chamberIndex = x < 18f ? 0 : x < 34f ? 1 : x < 50f ? 2 : 3;
                occupiedChambers[chamberIndex] = true;
            }
            int occupiedChamberCount = 0;
            foreach (bool occupied in occupiedChambers) if (occupied) occupiedChamberCount++;
            bool gateProgressionValid = true;
            if (!isBossStage && stage != 1)
            {
                StageRoomGate[] gates = Object.FindObjectsOfType<StageRoomGate>();
                System.Array.Sort(gates, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
                foreach (StageRoomGate gate in gates)
                {
                    int availableKills = 0;
                    foreach (EnemyController enemy in Object.FindObjectsOfType<EnemyController>())
                        if (enemy.transform.position.x < gate.transform.position.x) availableKills++;
                    if (availableKills < gate.RequiredKills) gateProgressionValid = false;
                }
            }
            bool fallRecoveryPassed = stage != 2 || (fallRecoveryProbeStarted && fallRecoveryProbeEnemy != null
                && fallRecoveryProbeEnemy.transform.position.y >= -2f);
            float actualJumpRise = jumpProbePeakY - jumpProbeStartY;
            bool actualJumpPassed = stage != 1 || (jumpProbeStarted && actualJumpRise >= GameBalance.SafePlatformRise);
            bool valid = data != null && data.stageNumber == stage && GameManager.Instance.Player != null
                && GameObject.Find("Stage_" + stage) != null
                && detailedBackground
                && legacyArtRemoved
                && productionArt
                && productionArtLive
                && stablePlayerVisual
                && stableVisuals
                && stableBackdrop
                && pixelRendering
                && hazards == 0
                && mainFloors == 1
                && walkableSurfaces.Length >= oneWayPlatforms + 1
                && platformReachability
                && enemiesAboveFloor
                && fallRecoveryPassed
                && actualJumpPassed
                && (isBossStage ? bosses == 1 : enemies >= data.targetEnemyKillCount)
                && (isBossStage ? terrainPieces >= 5 && decorations >= 6 : terrainPieces >= 9 && decorations >= 18)
                && (isBossStage ? oneWayPlatforms >= 4 : oneWayPlatforms >= 5)
                && hurtboxes >= enemies + bosses + 1
                && contactDamage == enemies + bosses
                && passThroughPhysics
                && enemyScaleValid
                && bossCameraScroll
                && providedArtValid
                && contactDamageTriggered
                && (isBossStage ? chambers == 1 : chambers == 4)
                && (isBossStage || uniqueChamberPatterns.Count == 4)
                && (isBossStage || (stage == 1 ? occupiedChamberCount >= 2 : occupiedChamberCount == 4))
                && gateProgressionValid
                && (isBossStage || roomGates == 4);
            Debug.Log("Project A+ Stage " + stage + " runtime smoke: " + valid + ", enemies=" + enemies + ", bosses=" + bosses + ", hazards=" + hazards + ", chambers=" + chambers + ", uniquePatterns=" + uniqueChamberPatterns.Count + ", occupiedChambers=" + occupiedChamberCount + ", gateProgressionValid=" + gateProgressionValid + ", mainFloors=" + mainFloors + ", walkableSurfaces=" + walkableSurfaces.Length + ", reachableSurfaces=" + reachableSurfaces + ", platformReachability=" + platformReachability + ", highestRequiredRise=" + highestRequiredRise.ToString("0.00") + ", maxJumpRise=" + GameBalance.MaxJumpRise().ToString("0.00") + ", actualJumpRise=" + actualJumpRise.ToString("0.00") + ", actualJumpPassed=" + actualJumpPassed + ", enemiesAboveFloor=" + enemiesAboveFloor + ", fallRecoveryPassed=" + fallRecoveryPassed + ", gates=" + roomGates + ", hurtboxes=" + hurtboxes + ", contactDamage=" + contactDamage + ", passThroughPhysics=" + passThroughPhysics + ", enemyScaleValid=" + enemyScaleValid + ", enemyScaleMarkers=" + enemyScaleMarkers.Length + ", bossCameraScroll=" + bossCameraScroll + ", providedArtValid=" + providedArtValid + ", contactDamageTriggered=" + contactDamageTriggered + ", terrain=" + terrainPieces + ", decor=" + decorations + ", oneWay=" + oneWayPlatforms + ", moving=" + movingPlatforms + ", detailedBg=" + detailedBackground + ", legacyArtRemoved=" + legacyArtRemoved + ", productionArt=" + productionArt + ", productionArtLive=" + productionArtLive + ", stablePlayerVisual=" + stablePlayerVisual + ", stableVisuals=" + stableVisuals + ", stableVisualCount=" + stableVisualComponents.Length + ", stableBackdrop=" + stableBackdrop + ", visualScale=" + (playerAnimator != null ? playerAnimator.CurrentVisualScale : 0f) + ", pixelRendering=" + pixelRendering + ", pixelCamera=" + pixelCamera + ", pixelCanvas=" + pixelCanvas + ", pixelUiFrame=" + pixelUiFrame + ", pixelUiSlot=" + pixelUiSlot + ", activeImages=" + activeImages);
            if (!valid)
            {
                SessionState.SetBool("ProjectAPlusAllStagesSmoke", false);
                EditorApplication.update -= AllStagesSmokeUpdate;
                EditorApplication.Exit(1);
                return;
            }

            if (stage >= 10)
            {
                SessionState.SetBool("ProjectAPlusAllStagesSmoke", false);
                EditorApplication.update -= AllStagesSmokeUpdate;
                Debug.Log("Project A+ all 10 stages runtime smoke passed.");
                EditorApplication.Exit(0);
                return;
            }
            SessionState.SetInt("ProjectAPlusSmokeStage", stage + 1);
            allStagesSmokeLoaded = false;
        }

        private class ReachabilitySurface
        {
            public float left;
            public float right;
            public float top;
            public bool reachable;
        }

        private static bool AuditPlatformReachability(WalkableSurfaceMarker[] markers, out int reachableCount, out float highestRequiredRise)
        {
            var surfaces = new List<ReachabilitySurface>();
            foreach (WalkableSurfaceMarker marker in markers)
            {
                BoxCollider2D collider = marker.transform.parent != null ? marker.transform.parent.GetComponent<BoxCollider2D>() : null;
                if (collider == null || marker.transform.parent.GetComponent<MovingStudyPlatform>() != null) continue;
                Bounds bounds = collider.bounds;
                surfaces.Add(new ReachabilitySurface
                {
                    left = bounds.min.x,
                    right = bounds.max.x,
                    top = bounds.max.y,
                    reachable = marker.IsMainFloor
                });
            }

            highestRequiredRise = 0f;
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (ReachabilitySurface target in surfaces)
                {
                    if (target.reachable) continue;
                    foreach (ReachabilitySurface source in surfaces)
                    {
                        if (!source.reachable) continue;
                        float rise = target.top - source.top;
                        if (rise > GameBalance.SafePlatformRise) continue;
                        float gap = Mathf.Max(0f, Mathf.Max(target.left - source.right, source.left - target.right));
                        float allowedGap = rise <= 0f ? GameBalance.SafePlatformGap * 1.5f : GameBalance.SafePlatformGap;
                        if (gap > allowedGap) continue;
                        target.reachable = true;
                        highestRequiredRise = Mathf.Max(highestRequiredRise, Mathf.Max(0f, rise));
                        changed = true;
                        break;
                    }
                }
            }

            reachableCount = 0;
            foreach (ReachabilitySurface surface in surfaces) if (surface.reachable) reachableCount++;
            return surfaces.Count > 0 && reachableCount == surfaces.Count
                && GameBalance.MaxJumpRise() >= GameBalance.SafePlatformRise + 0.8f;
        }

        public static void RunCompleteFlowSmokeTest()
        {
            OpenBootScene();
            SessionState.SetBool("ProjectAPlusCompleteFlowSmoke", true);
            SessionState.SetInt("ProjectAPlusFlowStage", 1);
            SessionState.SetInt("ProjectAPlusFlowPhase", 0);
            EditorApplication.update -= CompleteFlowSmokeUpdate;
            EditorApplication.update += CompleteFlowSmokeUpdate;
            EditorApplication.isPlaying = true;
        }

        private static double completeFlowNext;

        private static void CompleteFlowSmokeUpdate()
        {
            if (!EditorApplication.isPlaying || GameManager.Instance == null) return;
            int stage = SessionState.GetInt("ProjectAPlusFlowStage", 1);
            int phase = SessionState.GetInt("ProjectAPlusFlowPhase", 0);
            if (EditorApplication.timeSinceStartup < completeFlowNext) return;

            if (phase == 0)
            {
                if (stage == 1) GameManager.Instance.Save.NewGame();
                GameManager.Instance.LoadStage(stage);
                SessionState.SetInt("ProjectAPlusFlowPhase", 1);
                completeFlowNext = EditorApplication.timeSinceStartup + 0.2f;
                return;
            }
            if (phase == 1)
            {
                GameManager.Instance.ClearStage();
                bool validState = stage == 10 ? GameManager.Instance.State == GameState.Ending : GameManager.Instance.State == GameState.Result;
                Debug.Log("Project A+ flow Stage " + stage + " clear transition: " + validState + ", state=" + GameManager.Instance.State);
                if (!validState) { FinishCompleteFlow(false); return; }
                if (stage == 10)
                {
                    SessionState.SetInt("ProjectAPlusFlowPhase", 2);
                    completeFlowNext = EditorApplication.timeSinceStartup + 13.8f;
                    return;
                }
                GameManager.Instance.GoToNextStage();
                validState = GameManager.Instance.State == GameState.Playing
                    && GameManager.Instance.Stage.Current != null
                    && GameManager.Instance.Stage.Current.stageNumber == stage + 1;
                if (!validState) { FinishCompleteFlow(false); return; }
                SessionState.SetInt("ProjectAPlusFlowStage", stage + 1);
                SessionState.SetInt("ProjectAPlusFlowPhase", 0);
                completeFlowNext = EditorApplication.timeSinceStartup + 0.1f;
                return;
            }
            bool finished = GameManager.Instance.State == GameState.FinalGrade;
            Debug.Log("Project A+ ending to final grade transition: " + finished + ", state=" + GameManager.Instance.State);
            FinishCompleteFlow(finished);
        }

        private static void FinishCompleteFlow(bool passed)
        {
            SessionState.SetBool("ProjectAPlusCompleteFlowSmoke", false);
            EditorApplication.update -= CompleteFlowSmokeUpdate;
            Debug.Log("Project A+ complete game flow smoke: " + passed);
            EditorApplication.Exit(passed ? 0 : 1);
        }

        [MenuItem("Tools/Project A+/Run Submission Readiness Smoke Test")]
        public static void RunSubmissionReadinessSmokeTest()
        {
            OpenBootScene();
            SessionState.SetBool("ProjectAPlusSubmissionSmoke", true);
            SessionState.SetInt("ProjectAPlusSubmissionSmokePhase", 0);
            EditorApplication.update -= SubmissionSmokeUpdate;
            EditorApplication.update += SubmissionSmokeUpdate;
            EditorApplication.isPlaying = true;
        }

        private static double submissionSmokeNext;
        private static int submissionBaseStudyPower;

        private static void SubmissionSmokeUpdate()
        {
            if (!EditorApplication.isPlaying || GameManager.Instance == null) return;
            if (EditorApplication.timeSinceStartup < submissionSmokeNext) return;
            int phase = SessionState.GetInt("ProjectAPlusSubmissionSmokePhase", 0);
            if (phase == 0)
            {
                GameManager.Instance.Save.NewGame();
                GameManager.Instance.LoadStage(3);
                submissionBaseStudyPower = GameManager.Instance.Player.studyPower;
                GameManager.Instance.Player.mental = 37;
                GameManager.Instance.Player.GetComponent<PlayerInventory>().UseSlot(0);
                GameManager.Instance.ShowTitle();
                SessionState.SetInt("ProjectAPlusSubmissionSmokePhase", 1);
                submissionSmokeNext = EditorApplication.timeSinceStartup + 0.25f;
                return;
            }
            if (phase == 1)
            {
                GameManager.Instance.ContinueGame();
                SessionState.SetInt("ProjectAPlusSubmissionSmokePhase", 2);
                submissionSmokeNext = EditorApplication.timeSinceStartup + 0.35f;
                return;
            }
            if (phase == 2)
            {
                PlayerInventory inventory = GameManager.Instance.Player.GetComponent<PlayerInventory>();
                InventoryEntry jelly = inventory.items.Find(item => item.itemId == "energy_jelly");
                bool resumeValid = GameManager.Instance.State == GameState.Playing
                    && GameManager.Instance.Stage.Current != null
                    && GameManager.Instance.Stage.Current.stageNumber == 3
                    && GameManager.Instance.Player.mental == 57
                    && jelly != null && jelly.count == 1;
                GameManager.Instance.Player.studyPower += 99;
                GameManager.Instance.RetryStage();
                SessionState.SetBool("ProjectAPlusSubmissionResumeValid", resumeValid);
                SessionState.SetInt("ProjectAPlusSubmissionSmokePhase", 3);
                submissionSmokeNext = EditorApplication.timeSinceStartup + 0.35f;
                return;
            }

            PlayerInventory retryInventory = GameManager.Instance.Player.GetComponent<PlayerInventory>();
            InventoryEntry retryJelly = retryInventory.items.Find(item => item.itemId == "energy_jelly");
            bool retryValid = GameManager.Instance.State == GameState.Playing
                && GameManager.Instance.Stage.Current != null
                && GameManager.Instance.Stage.Current.stageNumber == 3
                && GameManager.Instance.Player.studyPower == submissionBaseStudyPower
                && GameManager.Instance.Player.mental == GameManager.Instance.Player.maxMental
                && retryJelly != null && retryJelly.count == 1;
            GameManager.Instance.PauseGame();
            bool pauseValid = GameManager.Instance.State == GameState.Paused && Mathf.Approximately(Time.timeScale, 0f);
            GameManager.Instance.ResumeGame();
            pauseValid &= GameManager.Instance.State == GameState.Playing && Mathf.Approximately(Time.timeScale, 1f);
            bool passed = SessionState.GetBool("ProjectAPlusSubmissionResumeValid", false)
                && retryValid && pauseValid && ProjectValidator.ValidateCore().Count == 0
                && GameManager.Instance.Save.HasSave;
            Debug.Log("Project A+ submission readiness smoke: " + passed
                + ", resume=" + SessionState.GetBool("ProjectAPlusSubmissionResumeValid", false)
                + ", retryCheckpoint=" + retryValid + ", pauseResume=" + pauseValid
                + ", save=" + GameManager.Instance.Save.HasSave);
            SessionState.SetBool("ProjectAPlusSubmissionSmoke", false);
            EditorApplication.update -= SubmissionSmokeUpdate;
            EditorApplication.Exit(passed ? 0 : 1);
        }

        private static void EnsureFolders()
        {
            foreach (string path in Folders)
            {
                if (AssetDatabase.IsValidFolder(path)) continue;
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                string name = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static void GeneratePlayerSprites()
        {
            var names = new List<string> { "idle_0", "idle_1", "jump", "fall", "damaged", "dead", "dodge_0", "dodge_1" };
            for (int i = 0; i < 4; i++) names.Add("run_" + i);
            for (int i = 0; i < 3; i++) names.Add("attack_" + i);
            foreach (string name in names)
            {
                Sprite frame = RuntimeArt.GetPlayerFrame(name);
                SaveRuntimeSprite("Assets/ProjectAPlus/Sprites/Player/" + name + ".png", frame != null ? frame : RuntimeArt.GetPlayer(), 128, 80);
            }
        }

        private static void GenerateStablePlayerFrames()
        {
            string sourceFolder = "Assets/ProjectAPlus/Resources/ProductionSprites/Player";
            string outputFolder = "Assets/ProjectAPlus/Resources/StableSprites/Player";
            if (!Directory.Exists(sourceFolder)) return;
            Directory.CreateDirectory(outputFolder);
            string[] names =
            {
                "idle_0", "idle_1", "run_0", "run_1", "run_2", "run_3", "jump", "fall",
                "attack_0", "attack_1", "attack_2", "dodge_0", "dodge_1", "damaged", "dead"
            };
            foreach (string name in names)
            {
                string sourcePath = sourceFolder + "/" + name + ".png";
                if (!File.Exists(sourcePath)) continue;
                Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                source.LoadImage(File.ReadAllBytes(sourcePath), false);
                Color32[] sourcePixels = source.GetPixels32();
                int minX = source.width;
                int minY = source.height;
                int maxX = -1;
                int maxY = -1;
                for (int y = 0; y < source.height; y++)
                for (int x = 0; x < source.width; x++)
                {
                    if (sourcePixels[y * source.width + x].a <= 8) continue;
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
                if (maxX < minX || maxY < minY)
                {
                    Object.DestroyImmediate(source);
                    continue;
                }

                int targetHeight = name == "dead" ? 60 : name.StartsWith("dodge_") ? 82 : 100;
                int sourceWidth = maxX - minX + 1;
                int sourceHeight = maxY - minY + 1;
                float scale = Mathf.Min(targetHeight / (float)sourceHeight, 120f / sourceWidth);
                const int canvas = 128;
                const int footLine = 6;
                Color32[] outputPixels = new Color32[canvas * canvas];
                for (int y = footLine; y < canvas; y++)
                for (int x = 0; x < canvas; x++)
                {
                    int sourceX = Mathf.RoundToInt(64f + (x - 64f) / scale);
                    int sourceY = Mathf.RoundToInt(minY + (y - footLine) / scale);
                    if (sourceX < 0 || sourceX >= source.width || sourceY < minY || sourceY > maxY) continue;
                    outputPixels[y * canvas + x] = sourcePixels[sourceY * source.width + sourceX];
                }
                var output = new Texture2D(canvas, canvas, TextureFormat.RGBA32, false);
                output.filterMode = FilterMode.Point;
                output.wrapMode = TextureWrapMode.Clamp;
                output.SetPixels32(outputPixels);
                output.Apply();
                string outputPath = outputFolder + "/" + name + ".png";
                File.WriteAllBytes(outputPath, output.EncodeToPNG());
                Object.DestroyImmediate(source);
                Object.DestroyImmediate(output);
                ImportSprite(outputPath, 80);
            }
        }

        private static void GenerateEnemySprites()
        {
            string[] names = { "SleepSlime", "PhoneTemptation", "AssignmentMonster", "TeamProjectMonster", "PresentationLaserMonster", "DeadlineTimer", "AnxietyShadow" };
            EnemyType[] types = { EnemyType.SleepSlime, EnemyType.PhoneTemptation, EnemyType.Assignment, EnemyType.TeamProject, EnemyType.PresentationLaser, EnemyType.DeadlineTimer, EnemyType.AnxietyShadow };
            for (int i = 0; i < names.Length; i++) SaveRuntimeSprite("Assets/ProjectAPlus/Sprites/Enemies/" + names[i] + ".png", RuntimeArt.GetEnemy(types[i]), 48, 32);
        }

        private static void GenerateBossSprites()
        {
            SaveRuntimeSprite("Assets/ProjectAPlus/Sprites/Bosses/MidtermWatcher.png", RuntimeArt.GetBoss(5), 128, 32);
            SaveRuntimeSprite("Assets/ProjectAPlus/Sprites/Bosses/FinalExamJudge.png", RuntimeArt.GetBoss(10), 128, 32);
        }

        private static void GenerateItemSprites()
        {
            string[] ids = { "energy_jelly", "night_coffee", "organized_notes", "past_exam_book", "presentation_script", "focus_headphones", "final_notes" };
            string[] names = { "EnergyJelly", "NightCoffee", "OrganizedNotes", "PastExamBook", "PresentationScript", "FocusHeadphones", "FinalNotes" };
            for (int i = 0; i < ids.Length; i++) SaveRuntimeSprite("Assets/ProjectAPlus/Sprites/UI/" + names[i] + ".png", RuntimeArt.GetItem(ids[i]), 48, 32);
        }

        private static void GenerateEffectSprites()
        {
            SaveRuntimeSprite("Assets/ProjectAPlus/Sprites/Effects/Attack.png", RuntimeArt.GetSlash(), 64, 32);
            SaveRuntimeSprite("Assets/ProjectAPlus/Sprites/Effects/Hit.png", RuntimeArt.GetProjectile(new Color32(255, 94, 78, 255)), 32, 32);
            SaveRuntimeSprite("Assets/ProjectAPlus/Sprites/Effects/LevelUp.png", RuntimeArt.GetProjectile(new Color32(255, 220, 72, 255)), 32, 32);
            SaveRuntimeSprite("Assets/ProjectAPlus/Sprites/Effects/ItemGet.png", RuntimeArt.GetItem("energy_jelly"), 32, 32);
            SaveRuntimeSprite("Assets/ProjectAPlus/Sprites/Effects/BossWarning.png", RuntimeArt.GetSpike(), 64, 32);
            SaveRuntimeSprite("Assets/ProjectAPlus/Sprites/Effects/BossProjectile.png", RuntimeArt.GetProjectile(new Color32(233, 72, 91, 255)), 32, 32);
        }

        private static void SaveRuntimeSprite(string path, Sprite sprite, int targetSize, int pixelsPerUnit)
        {
            Texture2D source = sprite.texture;
            Texture2D readable = source;
            bool destroyReadable = false;
            if (!source.isReadable)
            {
                RenderTexture previous = RenderTexture.active;
                RenderTexture renderTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(source, renderTexture);
                RenderTexture.active = renderTexture;
                readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                readable.Apply();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(renderTexture);
                destroyReadable = true;
            }
            var output = new Texture2D(targetSize, targetSize, TextureFormat.RGBA32, false);
            output.filterMode = FilterMode.Point;
            for (int y = 0; y < targetSize; y++)
            for (int x = 0; x < targetSize; x++)
            {
                int sx = Mathf.Clamp(Mathf.FloorToInt(x / (float)targetSize * readable.width), 0, readable.width - 1);
                int sy = Mathf.Clamp(Mathf.FloorToInt(y / (float)targetSize * readable.height), 0, readable.height - 1);
                output.SetPixel(x, y, readable.GetPixel(sx, sy));
            }
            output.Apply();
            File.WriteAllBytes(path, output.EncodeToPNG());
            Object.DestroyImmediate(output);
            if (destroyReadable) Object.DestroyImmediate(readable);
            ImportSprite(path, pixelsPerUnit);
        }

        private static void GenerateNamedSprites(string folder, string[] names, int size, Color baseColor)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Color color = Color.Lerp(baseColor, new Color(0.98f, 0.45f, 0.28f), (i % 5) * 0.12f);
                SavePixelPng("Assets/ProjectAPlus/Sprites/" + folder + "/" + names[i] + ".png", size, size, color, i);
            }
        }

        private static void GenerateBackgrounds()
        {
            string[] names = { "MorningClassroom", "Classroom", "LunchClassroom", "TeamMeetingRoom", "ExamHall", "EveningClassroom", "PresentationRoom", "Library", "NightLibrary", "FinalExamHall", "CampusOutside" };
            for (int i = 0; i < names.Length; i++) SaveBackground("Assets/ProjectAPlus/Sprites/Backgrounds/" + names[i] + ".png", i);
        }

        private static void ImportFreshPixelArt()
        {
            const string folder = "Assets/ProjectAPlus/Resources/FreshPixelArt";
            if (!Directory.Exists(folder)) return;
            foreach (string file in Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories))
                ImportSprite(file.Replace('\\', '/'), 100);
        }

        private static void ImportProductionSprites()
        {
            const string folder = "Assets/ProjectAPlus/Resources/ProductionSprites";
            if (!Directory.Exists(folder)) return;
            foreach (string file in Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories))
                ImportSprite(file.Replace('\\', '/'), 80);
        }

        private static void GeneratePrefabs()
        {
            var platform = new GameObject("Platform");
            platform.AddComponent<SpriteRenderer>();
            platform.AddComponent<BoxCollider2D>();
            PrefabUtility.SaveAsPrefabAsset(platform, "Assets/ProjectAPlus/Prefabs/Platform.prefab");
            Object.DestroyImmediate(platform);

            var player = new GameObject("Player");
            player.AddComponent<SpriteRenderer>();
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<PlayerStatus>();
            player.AddComponent<PlayerGrowth>();
            player.AddComponent<PlayerInventory>();
            player.AddComponent<PlayerController>();
            player.AddComponent<PlayerCombat>();
            player.AddComponent<PlayerHitHandler>();
            PrefabUtility.SaveAsPrefabAsset(player, "Assets/ProjectAPlus/Prefabs/Player.prefab");
            Object.DestroyImmediate(player);

            var enemy = new GameObject("Enemy");
            enemy.AddComponent<SpriteRenderer>();
            enemy.AddComponent<Rigidbody2D>();
            enemy.AddComponent<BoxCollider2D>();
            enemy.AddComponent<EnemyStatus>();
            enemy.AddComponent<EnemyController>();
            PrefabUtility.SaveAsPrefabAsset(enemy, "Assets/ProjectAPlus/Prefabs/Enemy.prefab");
            Object.DestroyImmediate(enemy);
        }

        private static void SavePixelPng(string path, int width, int height, Color color, int variant)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                bool shape = x > 2 && x < width - 3 && y > 2 && y < height - 3;
                bool border = shape && (x < 5 || x > width - 6 || y < 5 || y > height - 6);
                bool mark = shape && ((x + y + variant * 3) % 13 == 0);
                texture.SetPixel(x, y, !shape ? clear : border ? color * 0.35f : mark ? Color.white : color);
            }
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            ImportSprite(path, width >= 96 ? 32 : 16);
        }

        private static void SaveBackground(string path, int variant)
        {
            const int width = 480;
            const int height = 270;
            Color sky = Color.HSVToRGB((0.52f + variant * 0.045f) % 1f, 0.35f, variant >= 8 ? 0.28f : 0.72f);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Color pixel = sky;
                if (y < 56) pixel = new Color(0.14f, 0.17f, 0.22f);
                else if (y < 105 && x % 86 < 66) pixel = new Color(0.48f, 0.36f, 0.27f);
                else if (y > 170 && x % 112 > 82) pixel = new Color(0.72f, 0.82f, 0.86f);
                texture.SetPixel(x, y, pixel);
            }
            Color ink = new Color32(31, 35, 47, 255);
            Color wood = new Color32(111, 78, 52, 255);
            Color light = variant >= 8 ? new Color32(86, 104, 150, 255) : new Color32(205, 225, 217, 255);
            for (int i = 0; i < 5; i++)
            {
                int x = 20 + i * 94;
                PaintRect(texture, x, 165, 68, 72, ink);
                PaintRect(texture, x + 5, 170, 58, 62, light);
                PaintRect(texture, x + 32, 170, 4, 62, ink);
                PaintRect(texture, x + 5, 199, 58, 4, ink);
            }
            PaintRect(texture, 52, 82, 175, 58, ink);
            PaintRect(texture, 58, 88, 163, 46, new Color32(52, 91, 79, 255));
            for (int i = 0; i < 7; i++) PaintRect(texture, 67 + i * 20, 105 + (i % 2) * 8, 14, 3, new Color32(224, 225, 188, 255));
            for (int i = 0; i < 8; i++)
            {
                int x = 12 + i * 62;
                PaintRect(texture, x, 43, 52, 10, wood);
                PaintRect(texture, x + 5, 12, 6, 31, ink);
                PaintRect(texture, x + 41, 12, 6, 31, ink);
                PaintRect(texture, x + 4, 54, 44, 8, new Color32(145, 103, 68, 255));
            }
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            ImportSprite(path, 16);
        }

        private static void PaintRect(Texture2D texture, int x, int y, int width, int height, Color color)
        {
            for (int py = y; py < y + height; py++)
            for (int px = x; px < x + width; px++)
                if (px >= 0 && py >= 0 && px < texture.width && py < texture.height) texture.SetPixel(px, py, color);
        }

        private static void ImportSprite(string path, int pixelsPerUnit)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.maxTextureSize = 4096;
            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(textureSettings);
            TextureImporterPlatformSettings standalone = importer.GetPlatformTextureSettings("Standalone");
            standalone.overridden = true;
            standalone.maxTextureSize = 4096;
            standalone.format = TextureImporterFormat.RGBA32;
            standalone.textureCompression = TextureImporterCompression.Uncompressed;
            standalone.crunchedCompression = false;
            importer.SetPlatformTextureSettings(standalone);
            importer.SaveAndReimport();
        }

        private static void WriteStageSummary()
        {
            var wrapper = new StageWrapper { stages = StageCatalog.CreateAll() };
            File.WriteAllText("Assets/ProjectAPlus/Data/StageCatalog.json", JsonUtility.ToJson(wrapper, true));
            AssetDatabase.ImportAsset("Assets/ProjectAPlus/Data/StageCatalog.json");
        }

        private static void EnsureScenes()
        {
            EnsureFolders();
            string[] names = { "BootScene", "TitleScene", "MainGameScene" };
            var buildScenes = new List<EditorBuildSettingsScene>();
            foreach (string name in names)
            {
                string path = "Assets/ProjectAPlus/Scenes/" + name + ".unity";
                if (!File.Exists(path))
                {
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    var note = new GameObject("Project A+ Runtime Bootstrap - scene intentionally procedural");
                    note.transform.position = Vector3.zero;
                    EditorSceneManager.SaveScene(scene, path);
                }
                buildScenes.Add(new EditorBuildSettingsScene(path, name == "BootScene"));
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        [System.Serializable]
        private class StageWrapper { public List<StageData> stages; }
    }
}
#endif
