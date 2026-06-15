using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAPlus
{
    public class StageManager : MonoBehaviour
    {
        public StageData Current { get; private set; }
        public int Kills { get; private set; }
        public float Elapsed { get; private set; }
        public bool GoalReached { get; private set; }
        public bool Cleared { get; private set; }
        private GameObject stageRoot;
        private RewardManager rewards;
        private readonly List<StageRoomGate> roomGates = new List<StageRoomGate>();
        private readonly List<float> gatePositions = new List<float>();
        private readonly List<Vector2> enemyAnchors = new List<Vector2>();
        private int enemyAnchorCursor;
        private readonly int[] chamberEnemyCounts = new int[4];

        private enum ChamberPattern
        {
            Fork,
            Crown,
            ZigZag,
            TwinDeck,
            Well,
            Cross
        }

        private void Awake() { rewards = gameObject.AddComponent<RewardManager>(); }

        private void Update()
        {
            if (Current == null || Cleared || GameManager.Instance.State != GameState.Playing) return;
            Elapsed += Time.deltaTime;
            if (Current.timeLimit > 0 && Elapsed >= Current.timeLimit) GameManager.Instance.GameOver("마감 시간을 넘겼습니다.");
            if (!Current.IsBossStage && GoalReached && Kills >= Current.targetEnemyKillCount) Clear();
        }

        public void Load(StageData data)
        {
            Current = data;
            Kills = 0;
            Elapsed = 0;
            GoalReached = false;
            Cleared = false;
            roomGates.Clear();
            gatePositions.Clear();
            enemyAnchors.Clear();
            enemyAnchorCursor = 0;
            for (int i = 0; i < chamberEnemyCounts.Length; i++) chamberEnemyCounts[i] = 0;
            if (stageRoot != null) Destroy(stageRoot);
            stageRoot = new GameObject("Stage_" + data.stageNumber);
            BuildEnvironment();
            GameManager.Instance.CreatePlayer(new Vector2(2f, 1.4f));
            if (data.IsBossStage) SpawnBoss();
            else
            {
                foreach (var spawn in data.enemySpawnList) SpawnEnemy(spawn, false);
                SpawnGoal();
                BuildRoomGates();
            }
            if (data.stageNumber == 1)
            {
                SpawnWorldItem("energy_jelly", new Vector2(16f, 4.25f));
                stageRoot.AddComponent<StageTutorialGuide>();
            }
            CameraFollow follow = Camera.main.GetComponent<CameraFollow>();
            if (follow == null) follow = Camera.main.gameObject.AddComponent<CameraFollow>();
            follow.target = GameManager.Instance.Player.transform;
        }

        private void BuildEnvironment()
        {
            Color stageColor = RuntimeArt.StageColor(Current.stageNumber);
            Camera.main.backgroundColor = new Color(stageColor.r * 0.38f, stageColor.g * 0.38f, stageColor.b * 0.42f);
            Decor(new Vector2(GameBalance.StageWidth * 0.5f, 4.6f), new Vector2(GameBalance.StageWidth + 4f, 11f), stageColor, -20);
            BuildGeneratedBackdrop();
            BuildStageProps(stageColor);
            if (Current.IsBossStage)
            {
                BuildBossArena(stageColor);
                return;
            }
            BuildNormalTerrain(stageColor);
        }

        private void BuildNormalTerrain(Color stageColor)
        {
            string terrainStyle = Current.stageNumber >= 8 ? "library" :
                Current.stageNumber == 4 ? "meeting" :
                Current.stageNumber == 7 ? "presentation" : "classroom";
            Color floor = Current.stageNumber >= 8 ? new Color32(40, 35, 48, 255) :
                Current.stageNumber == 7 ? new Color32(48, 30, 43, 255) :
                new Color32(35, 34, 46, 255);
            Color upper = Current.stageNumber >= 8 ? new Color32(81, 58, 42, 255) :
                Current.stageNumber == 7 ? new Color32(86, 43, 53, 255) :
                new Color32(76, 57, 45, 255);

            Platform(new Vector2(GameBalance.StageWidth * 0.5f, -0.65f), new Vector2(GameBalance.StageWidth, 1.3f), floor, terrainStyle, false);
            ArenaWall(0f);
            ArenaWall(GameBalance.StageWidth);
            gatePositions.AddRange(new[] { 18f, 34f, 50f, 64f });
            // Single terrain authority: the chamber dungeon lays out every platform per chamber.
            // (A second full-width platform layer used to be stacked here, which caused tiles to
            // overlap and duplicate. The chamber layout already covers the whole stage.)
            BuildChamberDungeon(upper, terrainStyle);
            BuildTraversalFeatures(terrainStyle);
            BuildForegroundDetails(stageColor, terrainStyle);
        }

        private void BuildChamberDungeon(Color upper, string style)
        {
            Color block = new Color(
                Mathf.Clamp01(upper.r * 0.82f),
                Mathf.Clamp01(upper.g * 0.82f),
                Mathf.Clamp01(upper.b * 0.9f),
                1f);
            ChamberPattern[][] layouts =
            {
                new[] { ChamberPattern.Fork, ChamberPattern.Crown, ChamberPattern.ZigZag, ChamberPattern.TwinDeck },
                new[] { ChamberPattern.Crown, ChamberPattern.Well, ChamberPattern.Cross, ChamberPattern.Fork },
                new[] { ChamberPattern.ZigZag, ChamberPattern.TwinDeck, ChamberPattern.Crown, ChamberPattern.Well },
                new[] { ChamberPattern.TwinDeck, ChamberPattern.Cross, ChamberPattern.Fork, ChamberPattern.Crown },
                new[] { ChamberPattern.Well, ChamberPattern.ZigZag, ChamberPattern.TwinDeck, ChamberPattern.Cross },
                new[] { ChamberPattern.Cross, ChamberPattern.Fork, ChamberPattern.Well, ChamberPattern.ZigZag }
            };
            ChamberPattern[] selected = layouts[(Current.stageNumber - 1) % layouts.Length];
            for (int chamber = 0; chamber < 4; chamber++)
            {
                float left = chamber == 0 ? 1.2f : gatePositions[chamber - 1] + 0.8f;
                float right = gatePositions[chamber] - 0.8f;
                bool mirror = ((Current.stageNumber + chamber) & 1) == 0;
                BuildCombatChamber(chamber, left, right, selected[chamber], mirror, block, style);
            }
            BuildChamberDecor(style);
        }

        private void BuildCombatChamber(int index, float left, float right, ChamberPattern pattern, bool mirror, Color color, string style)
        {
            float center = (left + right) * 0.5f;
            float span = right - left;
            float X(float normalized) { return mirror ? right - span * normalized : left + span * normalized; }
            void L(float normalized, float surface, float width, float depth = 0.65f)
            {
                ReachableLedge(X(normalized), surface, width, depth, color, style);
            }

            switch (pattern)
            {
                case ChamberPattern.Fork:
                    L(0.24f, 1.45f, 5.2f);
                    L(0.76f, 1.45f, 5.2f);
                    L(0.5f, 2.9f, 6.2f);
                    L(0.5f, 4.35f, 4.8f);
                    break;
                case ChamberPattern.Crown:
                    L(0.5f, 1.4f, 5.2f);
                    L(0.25f, 2.8f, 4.5f);
                    L(0.75f, 2.8f, 4.5f);
                    L(0.5f, 4.2f, 5.2f);
                    break;
                case ChamberPattern.ZigZag:
                    L(0.18f, 1.35f, 4.2f);
                    L(0.42f, 2.7f, 4.2f);
                    L(0.66f, 4.05f, 4.2f);
                    L(0.84f, 2.7f, 3.5f);
                    break;
                case ChamberPattern.TwinDeck:
                    L(0.28f, 1.4f, 5.4f);
                    L(0.72f, 1.4f, 5.4f);
                    L(0.5f, 2.85f, 7.2f);
                    L(0.24f, 4.3f, 4.2f);
                    L(0.76f, 4.3f, 4.2f);
                    break;
                case ChamberPattern.Well:
                    L(0.5f, 1.35f, 4.2f);
                    L(0.23f, 2.75f, 4.8f);
                    L(0.77f, 2.75f, 4.8f);
                    L(0.5f, 4.15f, 5.8f);
                    break;
                default:
                    L(0.2f, 1.4f, 4.2f);
                    L(0.8f, 1.4f, 4.2f);
                    L(0.5f, 2.8f, 7.5f);
                    L(0.2f, 4.2f, 4.2f);
                    L(0.8f, 4.2f, 4.2f);
                    break;
            }

            // Ordered anchors guarantee enough enemies exist before each locked gate.
            enemyAnchors.Add(new Vector2(center, 1.15f));
            enemyAnchors.Add(new Vector2(X(0.25f), 2.3f));
            enemyAnchors.Add(new Vector2(X(0.72f), 1.15f));

            var marker = new GameObject("Combat Chamber " + (index + 1) + " - " + pattern);
            marker.transform.SetParent(stageRoot.transform);
            marker.transform.position = new Vector2(center, 4f);
            marker.AddComponent<StageChamberMarker>().Initialize(index + 1, pattern.ToString());
            VisualProp("arch", new Vector2(right, 2.4f), new Vector2(2.1f, 3.9f), -2, RuntimeArt.StageColor(Current.stageNumber));
        }

        private void ReachableLedge(float x, float surfaceY, float width, float depth, Color color, string style)
        {
            Platform(new Vector2(x, surfaceY - depth * 0.5f), new Vector2(width, depth), color, style, true);
        }

        private void ArchitectureMass(Vector2 position, Vector2 size, Color color, string style)
        {
            var go = new GameObject("Non-Colliding Architecture " + style);
            go.transform.SetParent(stageRoot.transform);
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.GetPlatform(style + "_ground", color);
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = size;
            renderer.sortingOrder = -1;
            go.AddComponent<StageDecoration>();
        }

        private void BuildChamberDecor(string style)
        {
            Color tint = RuntimeArt.StageColor(Current.stageNumber);
            string feature = style == "library" ? "books" : style == "presentation" ? "banner" : "desk";
            for (int chamber = 0; chamber < 4; chamber++)
            {
                float center = chamber == 0 ? 9f : 26f + (chamber - 1) * 16f;
                VisualProp(feature, new Vector2(center, 0.45f), new Vector2(1.1f, 0.85f), -2, tint);
                VisualProp("lamp", new Vector2(center - 5.5f, 6.6f), new Vector2(0.45f, 0.75f), -2, tint);
                VisualProp("lamp", new Vector2(center + 5.5f, 6.6f), new Vector2(0.45f, 0.75f), -2, tint);
            }
        }

        private void BuildTraversalFeatures(string style)
        {
            if (Current.stageNumber == 2 || Current.stageNumber == 3)
            {
                LaunchPad(new Vector2(24.5f, 0.18f), style);
                LaunchPad(new Vector2(54.5f, 0.18f), style);
            }
            else if (Current.stageNumber == 6 || Current.stageNumber == 9)
            {
                LaunchPad(new Vector2(16.5f, 0.18f), style);
                LaunchPad(new Vector2(45f, 0.18f), style);
            }
        }

        private void BuildBossArena(Color stageColor)
        {
            string style = Current.stageNumber == 10 ? "final" : "exam";
            Color floor = Current.stageNumber == 10 ? new Color32(39, 24, 45, 255) : new Color32(43, 39, 48, 255);
            Color structure = Current.stageNumber == 10 ? new Color32(71, 34, 59, 255) : new Color32(62, 58, 72, 255);
            Platform(new Vector2(GameBalance.StageWidth * 0.5f, -0.65f), new Vector2(GameBalance.StageWidth, 1.3f), floor, style, false);
            ReachableLedge(8f, 1.45f, 6f, 0.75f, structure, style);
            ReachableLedge(60f, 1.45f, 6f, 0.75f, structure, style);
            ReachableLedge(15f, 2.9f, 6f, 0.75f, structure, style);
            ReachableLedge(53f, 2.9f, 6f, 0.75f, structure, style);
            ReachableLedge(22f, 4.35f, 6f, 0.75f, structure, style);
            ReachableLedge(46f, 4.35f, 6f, 0.75f, structure, style);
            ReachableLedge(34f, 4.35f, 12f, 0.75f, structure, style);
            ReachableLedge(28f, 1.45f, 5f, 0.65f, structure, style);
            ReachableLedge(40f, 1.45f, 5f, 0.65f, structure, style);
            ArenaWall(0f);
            ArenaWall(GameBalance.StageWidth);
            var marker = new GameObject("Boss Combat Chamber");
            marker.transform.SetParent(stageRoot.transform);
            marker.transform.position = new Vector2(GameBalance.StageWidth * 0.5f, 3f);
            marker.AddComponent<StageChamberMarker>().Initialize(1, "BossArena");
            for (int i = 0; i < 4; i++) VisualProp("banner", new Vector2(9f + i * 16.5f, 6.8f), new Vector2(0.85f, 1.65f), -1, stageColor);
            VisualProp("arch", new Vector2(34f, 2.8f), new Vector2(5.5f, 4.2f), -2, stageColor);
        }

        private void ArenaWall(float x)
        {
            var go = new GameObject("Arena Boundary");
            go.transform.SetParent(stageRoot.transform);
            go.transform.position = new Vector2(x, 3f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.GetPlatform("boundary", new Color32(44, 47, 63, 255));
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(0.7f, 8f);
            renderer.sortingOrder = 2;
            renderer.enabled = false;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.7f, 8f);
            go.AddComponent<StageTerrainPiece>();
        }

        private void BuildGeneratedBackdrop()
        {
            string name = Current.stageNumber == 8 || Current.stageNumber == 9 ? "MidnightDataLibrary" :
                Current.stageNumber == 5 || Current.stageNumber == 10 ? "FinalExamArchive" :
                "CampusLectureDungeon";
            Sprite sprite = Resources.Load<Sprite>("FreshPixelArt/Backgrounds/" + name);
            if (sprite == null) return;
            var go = new GameObject("Detailed Runtime Background - " + name);
            go.transform.SetParent(stageRoot.transform);
            go.transform.position = new Vector3(8.5f, 4.3f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -19;
            go.AddComponent<CameraBackdrop>();
        }

        private void BuildStageProps(Color stageColor)
        {
            string prop = Current.stageNumber == 8 || Current.stageNumber == 9 ? "books" :
                Current.stageNumber == 5 || Current.stageNumber == 10 ? "banner" : "desk";
            for (int i = 0; i < 9; i++)
            {
                var go = new GameObject("Background " + prop);
                go.transform.SetParent(stageRoot.transform);
                go.transform.position = new Vector3(4f + i * 7.5f, 0.35f, 0);
                Sprite propSprite = RuntimeArt.GetProp(prop, new Color(stageColor.r * 0.72f, stageColor.g * 0.72f, stageColor.b * 0.78f));
                SpriteRenderer sr = StableVisual.AttachSprite(go, propSprite, 0.8f, -4, "Background Prop Visual", Vector2.zero);
                sr.color = new Color(0.55f, 0.58f, 0.66f, 0.18f);
                go.AddComponent<StageDecoration>();
            }
        }

        private void BuildForegroundDetails(Color stageColor, string style)
        {
            for (int i = 0; i < 5; i++)
            {
                float x = 7f + i * 14f;
                VisualProp("lamp", new Vector2(x, 1.15f), new Vector2(0.55f, 0.75f), -3, stageColor);
            }
            if (style == "library")
            {
                for (int i = 0; i < 6; i++) VisualProp("books", new Vector2(4f + i * 11.5f, 0.45f), new Vector2(1.1f, 1.1f), -3, stageColor);
            }
            else if (style == "presentation")
            {
                for (int i = 0; i < 5; i++) VisualProp("banner", new Vector2(7f + i * 14f, 1.05f), new Vector2(0.65f, 1.1f), -3, stageColor);
            }
            else if (style == "meeting")
            {
                for (int i = 0; i < 7; i++) VisualProp("desk", new Vector2(4.5f + i * 10f, 0.25f), new Vector2(1.05f, 0.75f), -3, stageColor);
            }
            else
            {
                for (int i = 0; i < 7; i++) VisualProp("desk", new Vector2(4.5f + i * 10f, 0.22f), new Vector2(0.95f, 0.7f), -3, stageColor);
            }
        }

        private void VisualProp(string kind, Vector2 position, Vector2 scale, int sortingOrder, Color stageColor)
        {
            var go = new GameObject("Visual " + kind);
            go.transform.SetParent(stageRoot.transform);
            go.transform.position = position;
            Sprite propSprite = RuntimeArt.GetProp(kind, new Color(stageColor.r * 0.7f, stageColor.g * 0.7f, stageColor.b * 0.75f));
            SpriteRenderer sr = StableVisual.FitSpriteInBox(go, propSprite, scale, sortingOrder, "Prop Visual");
            sr.color = sortingOrder >= 0 ? Color.white : new Color(0.58f, 0.61f, 0.68f, 0.32f);
            go.AddComponent<StageDecoration>();
        }

        private void Decor(Vector2 position, Vector2 scale, Color color, int sortingOrder)
        {
            var go = new GameObject("Decoration");
            go.transform.SetParent(stageRoot.transform);
            go.transform.position = position;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeArt.Solid("decor" + color, color);
            sr.sortingOrder = sortingOrder;
        }

        private void Platform(Vector2 position, Vector2 scale, Color color, string style = "stone", bool oneWay = false)
        {
            var go = new GameObject((oneWay ? "One Way " : "") + style + " Platform");
            go.transform.SetParent(stageRoot.transform);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeArt.GetPlatform(style + (oneWay ? "_oneway" : "_ground"), color);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = scale;
            sr.sortingOrder = 1;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = scale;
            if (oneWay)
            {
                collider.usedByEffector = true;
                var effector = go.AddComponent<PlatformEffector2D>();
                effector.useOneWay = true;
                effector.surfaceArc = 160f;
            }
            AddWalkableSurface(go, scale, oneWay, !oneWay && scale.x >= GameBalance.StageWidth - 0.1f);
            go.AddComponent<StageTerrainPiece>();
        }

        private void AddWalkableSurface(GameObject platform, Vector2 scale, bool oneWay, bool mainFloor)
        {
            // Logical marker only; the translucent surface highlight is intentionally not rendered.
            var surface = new GameObject(oneWay ? "One Way Walkable Surface" : "Solid Walkable Surface");
            surface.transform.SetParent(platform.transform, false);
            surface.transform.localPosition = new Vector3(0f, scale.y * 0.5f - 0.055f, 0f);
            surface.AddComponent<WalkableSurfaceMarker>().Initialize(oneWay, mainFloor);
        }

        private void LaunchPad(Vector2 position, string style)
        {
            var go = new GameObject("Study Launch Pad");
            go.transform.SetParent(stageRoot.transform);
            go.transform.position = position;
            StableVisual.FitSpriteInBox(go, RuntimeArt.GetProp("launch", RuntimeArt.StageColor(Current.stageNumber)), new Vector2(0.9f, 0.42f), 4, "Launch Pad Visual");
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.9f, 0.45f);
            go.AddComponent<StudyLaunchPad>();
            go.AddComponent<StageTerrainPiece>();
        }

        public void SpawnEnemy(EnemySpawnData spawn, bool summoned)
        {
            EnemyData data = StageCatalog.CreateEnemy(spawn.type, Current.stageNumber, spawn.elite);
            var go = new GameObject(data.enemyName);
            go.layer = CombatLayers.EnemyBody;
            go.transform.SetParent(stageRoot.transform);
            Vector2 anchor = SelectEnemyAnchor(spawn, summoned);
            go.transform.position = anchor;
            float enemyScale = spawn.elite ? 1.5f : 1f;
            Vector2 bodySize = new Vector2(0.9f, 1.35f) * enemyScale;
            StableVisual.AttachSprite(go, RuntimeArt.GetEnemy(spawn.type), enemyScale, 5, "Enemy Visual", Vector2.zero);
            var bodyCollider = go.AddComponent<BoxCollider2D>();
            bodyCollider.size = bodySize;
            go.AddComponent<Rigidbody2D>();
            EnemyController enemy;
            if (spawn.type == EnemyType.Assignment) enemy = go.AddComponent<AssignmentEnemy>();
            else if (spawn.type == EnemyType.PresentationLaser) enemy = go.AddComponent<RangedEnemy>();
            else if (spawn.type == EnemyType.PhoneTemptation || spawn.type == EnemyType.ThoughtCloud) enemy = go.AddComponent<DebuffEnemy>();
            else enemy = go.AddComponent<EnemyController>();
            enemy.Initialize(data);
            Hurtbox hurtbox = CombatGeometry.AttachHurtbox(go, CombatTeam.Enemy, bodySize, Vector2.zero, enemy);
            CombatGeometry.AttachContactDamage(hurtbox, Mathf.Max(1, Mathf.RoundToInt(data.attackPower * 0.65f)));
            go.AddComponent<PixelBob>();
            WorldHealthBar.Attach(go, data.maxHp, false);
            var marker = go.AddComponent<EnemyScaleMarker>();
            marker.Initialize(spawn.elite, enemyScale, bodySize);
            if (spawn.elite)
            {
                Transform healthBar = go.transform.Find("HealthBar");
                if (healthBar != null) healthBar.localPosition = new Vector3(0f, 1.3f, 0f);
            }
        }

        private float SafeGroundX(float x)
        {
            return Mathf.Clamp(x, 3f, GameBalance.StageWidth - 4f);
        }

        private Vector2 SelectEnemyAnchor(EnemySpawnData spawn, bool summoned)
        {
            if (summoned || enemyAnchors.Count < 12)
                return new Vector2(SafeGroundX(Mathf.Lerp(4f, GameBalance.StageWidth - 6f, Mathf.InverseLerp(2f, 40f, spawn.x))), spawn.y);

            int target = Mathf.Max(1, Current.targetEnemyKillCount);
            int orderedIndex = enemyAnchorCursor++;
            int chamber = orderedIndex >= target ? 0 : Mathf.Clamp(Mathf.FloorToInt(orderedIndex * 4f / target), 0, 3);
            int local = chamberEnemyCounts[chamber]++ % 3;
            return enemyAnchors[chamber * 3 + local];
        }

        private void SpawnBoss()
        {
            var go = new GameObject(Current.bossData.bossName);
            go.layer = CombatLayers.EnemyBody;
            go.transform.SetParent(stageRoot.transform);
            go.transform.position = new Vector2(GameBalance.StageWidth * 0.7f, 1.8f);
            StableVisual.FitSpriteInBox(go, RuntimeArt.GetBoss(Current.stageNumber),
                Current.stageNumber == 10 ? new Vector2(3.35f, 3.65f) : new Vector2(3.1f, 3.4f),
                5, "Boss Visual");
            var bossCollider = go.AddComponent<BoxCollider2D>();
            bossCollider.size = Current.stageNumber == 10 ? new Vector2(2.8f, 3.8f) : new Vector2(2.6f, 3.5f);
            bossCollider.offset = new Vector2(0f, -0.25f);
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            var boss = go.AddComponent<BossController>();
            boss.Initialize(Current.bossData, Current.stageNumber);
            Hurtbox hurtbox = CombatGeometry.AttachHurtbox(go, CombatTeam.Enemy,
                Current.stageNumber == 10 ? new Vector2(2.8f, 3.8f) : new Vector2(2.6f, 3.5f),
                new Vector2(0f, -0.25f), boss);
            CombatGeometry.AttachContactDamage(hurtbox, Mathf.Max(1, Mathf.RoundToInt(Current.bossData.attackPower * 0.55f)), 0.9f);
            var bob = go.AddComponent<PixelBob>();
            bob.height = 0.02f;
            bob.squash = 0.012f;
            WorldHealthBar.Attach(go, Current.bossData.maxHp, true);
        }

        private void SpawnGoal()
        {
            var go = new GameObject("Goal");
            go.transform.SetParent(stageRoot.transform);
            go.transform.position = new Vector2(GameBalance.StageWidth - 2f, 1.2f);
            StableVisual.FitSpriteInBox(go, RuntimeArt.GetProp("board", new Color(0.25f, 0.88f, 0.55f, 0.9f)), new Vector2(1.2f, 2.8f), 3, "Goal Visual");
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.2f, 2.8f);
            go.AddComponent<StageGoal>();
        }

        private void SpawnWorldItem(string itemId, Vector2 position)
        {
            var go = new GameObject("Item Pickup " + itemId);
            go.transform.SetParent(stageRoot.transform);
            go.transform.position = position;
            StableVisual.AttachSprite(go, RuntimeArt.GetItem(itemId), 0.75f, 9, "Item Visual", Vector2.zero);
            var collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.55f;
            var pickup = go.AddComponent<WorldItemPickup>();
            pickup.itemId = itemId;
            go.AddComponent<PixelBob>();
        }

        private void BuildRoomGates()
        {
            if (gatePositions.Count == 0) gatePositions.AddRange(new[] { 18f, 34f, 50f, 64f });
            for (int i = 0; i < gatePositions.Count; i++)
            {
                int required = Mathf.Max(1, Mathf.CeilToInt(Current.targetEnemyKillCount * ((i + 1) / 4f)));
                var go = new GameObject("Room Gate " + (i + 1));
                go.layer = CombatLayers.PlayerGate;
                go.transform.SetParent(stageRoot.transform);
                go.transform.position = new Vector2(gatePositions[i], 4.1f);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.GetGate();
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.size = new Vector2(0.42f, 7.2f);
                renderer.color = new Color(0.55f, 0.85f, 1f, 0.38f);
                renderer.sortingOrder = 3;
                var collider = go.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(0.65f, 9.2f);
                var gate = go.AddComponent<StageRoomGate>();
                gate.Initialize(required, renderer, collider);
                roomGates.Add(gate);
            }
        }

        public void RegisterKill(EnemyData data)
        {
            Kills++;
            rewards.RewardEnemy(data);
            foreach (StageRoomGate gate in roomGates) gate.Refresh(Kills);
            if (!Current.IsBossStage && GoalReached && Kills >= Current.targetEnemyKillCount) Clear();
        }

        public void RegisterBossKill() { Kills = 1; Clear(); }

        public float CameraFocusX(float playerX)
        {
            if (Current == null || Current.IsBossStage) return playerX;
            float left = 0f;
            foreach (float gate in gatePositions)
            {
                if (playerX < gate) return (left + gate) * 0.5f;
                left = gate;
            }
            return (left + GameBalance.StageWidth) * 0.5f;
        }

        public void ReachGoal()
        {
            GoalReached = true;
            if (Kills >= Current.targetEnemyKillCount) Clear();
            else GameManager.Instance.UI.Toast("목표 도착! 적을 " + (Current.targetEnemyKillCount - Kills) + "마리 더 처치하세요.");
        }

        private void Clear()
        {
            if (Cleared) return;
            Cleared = true;
            GameManager.Instance.ClearStage();
        }
    }

    public class StageGoal : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerStatus>() != null && GameManager.Instance != null) GameManager.Instance.Stage.ReachGoal();
        }
    }

    public class WorldItemPickup : MonoBehaviour
    {
        public string itemId;
        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory == null || GameManager.Instance == null) return;
            inventory.AddItem(itemId);
            ItemData data;
            string label = GameManager.Instance.Items.TryGetValue(itemId, out data) ? data.itemName : itemId;
            GameManager.Instance.UI.Toast("ITEM GET! " + label);
            if (AudioManager.Instance != null) AudioManager.Instance.Play("item");
            CombatFx.Burst(transform.position, new Color32(255, 220, 80, 255), 12);
            Destroy(gameObject);
        }
    }

    public class StageTutorialGuide : MonoBehaviour
    {
        private IEnumerator Start()
        {
            string[] guides =
            {
                "A / D 또는 방향키로 이동하세요.",
                "SPACE로 점프하고, S로 높은 플랫폼에서 내려가세요.",
                "J 또는 마우스 왼쪽 버튼으로 공격하세요.",
                "K 또는 LEFT SHIFT로 공격을 회피하세요.",
                "TAB 또는 I에서 성장 포인트를 투자할 수 있습니다.",
                "졸음 슬라임 2마리를 처치하고 오른쪽 목표 지점에 도착하세요."
            };
            yield return new WaitForSeconds(0.8f);
            foreach (string guide in guides)
            {
                if (GameManager.Instance == null || GameManager.Instance.Stage.Current == null || GameManager.Instance.Stage.Current.stageNumber != 1) yield break;
                GameManager.Instance.UI.Toast(guide);
                yield return new WaitForSeconds(2.4f);
            }
        }
    }

    public class StageHazard : MonoBehaviour
    {
        public int damage = 10;
        private void OnTriggerStay2D(Collider2D other)
        {
            PlayerHitHandler hit = other.GetComponent<PlayerHitHandler>();
            if (hit != null) hit.TakeDamage(damage, transform.position);
        }
    }

    public class StageTerrainPiece : MonoBehaviour { }

    public class WalkableSurfaceMarker : MonoBehaviour
    {
        public bool IsOneWay { get; private set; }
        public bool IsMainFloor { get; private set; }

        public void Initialize(bool oneWay, bool mainFloor)
        {
            IsOneWay = oneWay;
            IsMainFloor = mainFloor;
        }
    }

    public class StageDecoration : MonoBehaviour { }

    public class EnemyScaleMarker : MonoBehaviour
    {
        public bool IsElite { get; private set; }
        public float UniformScale { get; private set; }
        public Vector2 BodySize { get; private set; }

        public void Initialize(bool elite, float uniformScale, Vector2 bodySize)
        {
            IsElite = elite;
            UniformScale = uniformScale;
            BodySize = bodySize;
        }
    }

    public class StageChamberMarker : MonoBehaviour
    {
        public int Index { get; private set; }
        public string Pattern { get; private set; }

        public void Initialize(int index, string pattern)
        {
            Index = index;
            Pattern = pattern;
        }
    }

    public class StudyLaunchPad : MonoBehaviour
    {
        private float readyAt;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Time.time < readyAt) return;
            Rigidbody2D body = other.GetComponent<Rigidbody2D>();
            PlayerStatus player = other.GetComponent<PlayerStatus>();
            if (body == null || player == null) return;
            readyAt = Time.time + 0.35f;
            body.velocity = new Vector2(body.velocity.x, 11.5f + player.movementEfficiency * 0.12f);
            CombatFx.Burst(transform.position + Vector3.up * 0.2f, new Color32(255, 203, 83, 255), 8);
            if (AudioManager.Instance != null) AudioManager.Instance.Play("jump");
        }
    }

    public class MovingStudyPlatform : MonoBehaviour
    {
        public Vector2 travel = new Vector2(5f, 0f);
        public float speed = 1.5f;
        private Vector2 origin;
        private Rigidbody2D body;
        private void Awake() { origin = transform.position; body = GetComponent<Rigidbody2D>(); }
        private void FixedUpdate()
        {
            float wave = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
            body.MovePosition(origin + travel * wave);
        }
    }

    public class StageRoomGate : MonoBehaviour
    {
        public int RequiredKills { get; private set; }
        public bool IsOpen { get; private set; }
        private SpriteRenderer sprite;
        private BoxCollider2D blocker;

        public void Initialize(int requiredKills, SpriteRenderer renderer, BoxCollider2D collider)
        {
            RequiredKills = requiredKills;
            sprite = renderer;
            blocker = collider;
        }

        public void Refresh(int kills)
        {
            if (IsOpen || kills < RequiredKills) return;
            IsOpen = true;
            if (blocker != null) blocker.enabled = false;
            StartCoroutine(OpenRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.Play("clear");
            for (int i = 0; i < 8; i++)
            {
                if (sprite != null) sprite.color = new Color(0.4f, 0.9f, 1f, 1f - i / 8f);
                transform.position += Vector3.up * 0.06f;
                yield return new WaitForSeconds(0.035f);
            }
            gameObject.SetActive(false);
        }
    }

    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        private static CameraFollow instance;
        private float shakeTime;
        private float shakeStrength;

        private void Awake() { instance = this; }

        public static void Shake(float strength, float duration)
        {
            if (instance == null) return;
            instance.shakeStrength = Mathf.Max(instance.shakeStrength, strength);
            instance.shakeTime = Mathf.Max(instance.shakeTime, duration);
        }

        private void LateUpdate()
        {
            if (target == null) return;
            float halfWidth = Camera.main != null ? Mathf.Min(GameBalance.StageWidth * 0.5f, Camera.main.orthographicSize * Camera.main.aspect) : 8.5f;
            float focus = GameManager.Instance != null && GameManager.Instance.Stage != null
                ? GameManager.Instance.Stage.CameraFocusX(target.position.x)
                : target.position.x;
            float x = Mathf.Clamp(focus, halfWidth, GameBalance.StageWidth - halfWidth);
            Vector3 desired = new Vector3(x, 4.3f, -10f);
            if (shakeTime > 0f)
            {
                shakeTime -= Time.unscaledDeltaTime;
                desired += (Vector3)Random.insideUnitCircle * shakeStrength;
                shakeStrength = Mathf.Lerp(shakeStrength, 0f, 7f * Time.unscaledDeltaTime);
            }
            transform.position = Vector3.Lerp(transform.position, desired, 9f * Time.unscaledDeltaTime);
        }
    }

    public class CameraBackdrop : MonoBehaviour
    {
        public float CurrentUniformScale { get; private set; } = 1f;
        public bool IsUniform { get { return StableVisual.IsUniform(transform.localScale); } }

        private void LateUpdate()
        {
            if (Camera.main == null) return;
            transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0f);
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null) return;
            float requiredHeight = Camera.main.orthographicSize * 2f * 1.15f;
            float requiredWidth = requiredHeight * Camera.main.aspect;
            float scale = Mathf.Max(requiredWidth / renderer.sprite.bounds.size.x, requiredHeight / renderer.sprite.bounds.size.y);
            CurrentUniformScale = scale;
            transform.localScale = new Vector3(CurrentUniformScale, CurrentUniformScale, 1f);
        }
    }
}
