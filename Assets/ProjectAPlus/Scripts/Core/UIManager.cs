using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAPlus
{
    public class UIManager : MonoBehaviour
    {
        private Canvas canvas;
        private Font font;
        private GameObject titlePanel;
        private GameObject hudPanel;
        private GameObject pausePanel;
        private GameObject abilityPanel;
        private GameObject resultPanel;
        private GameObject cutscenePanel;
        private GameObject gameOverPanel;
        private GameObject finalPanel;
        private GameObject settingsPanel;
        private Text hudLeft;
        private Text hudRight;
        private Text hudBottom;
        private Text bossText;
        private Text comboText;
        private Text abilityText;
        private Text cutsceneText;
        private Text toastText;
        private Text resolutionText;
        private Text displayModeText;
        private Image mentalFill;
        private Image expFill;
        private Image bossFill;
        private Image stageFill;
        private Image cutsceneCharacter;
        private Image cutsceneBackdrop;
        private Image qItemIcon;
        private Image eItemIcon;
        private CanvasGroup cutsceneCanvasGroup;
        private Button continueButton;
        private Button nextRoomButton;
        private int cutsceneIndex;
        private string[] cutsceneLines;
        private Action cutsceneComplete;
        private float nextCutsceneAdvance;

        public void Initialize()
        {
            font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial" }, 24);
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();
            var canvasGo = new GameObject("Project A+ UI");
            DontDestroyOnLoad(canvasGo);
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvas.pixelPerfect = true;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
            scaler.referencePixelsPerUnit = 16f;
            canvasGo.AddComponent<PixelCanvasScale>();
            canvasGo.AddComponent<GraphicRaycaster>();
            BuildTitle();
            BuildHud();
            BuildPause();
            BuildAbility();
            BuildResult();
            BuildCutscene();
            BuildGameOver();
            BuildFinal();
            BuildSettings();
            toastText = MakeText(canvas.transform, "", 21, TextAnchor.MiddleCenter, new Color32(255, 231, 172, 255), new Vector2(0.24f, 0.76f), new Vector2(0.76f, 0.82f));
            toastText.gameObject.SetActive(false);
            Debug.Log("Project A+ pixel UI ready: canvasPixelPerfect=" + canvas.pixelPerfect
                + ", integerScale=" + (canvas.GetComponent<PixelCanvasScale>() != null)
                + ", frame=" + RuntimeArt.GetUiFrame().name + ", button=" + RuntimeArt.GetUiButton().name);
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.State == GameState.Playing && GameManager.Instance.Player != null) RefreshHud();
            if ((GameManager.Instance.State == GameState.Opening || GameManager.Instance.State == GameState.Ending) && Input.GetKeyDown(KeyCode.Return)) AdvanceCutscene();
            if (cutscenePanel != null && cutscenePanel.activeSelf && Time.unscaledTime >= nextCutsceneAdvance) AdvanceCutscene();
            if (cutscenePanel != null && cutscenePanel.activeSelf) AnimateCutscene();
            if (settingsPanel != null && settingsPanel.activeSelf) RefreshSettings();
        }

        public void ShowTitle(bool hasSave)
        {
            HideAll();
            Time.timeScale = 1f;
            titlePanel.SetActive(true);
            continueButton.interactable = hasSave;
        }

        public void ShowHud()
        {
            HideAll();
            hudPanel.SetActive(true);
        }

        public void ShowPause(bool visible)
        {
            pausePanel.SetActive(visible);
            if (!visible) settingsPanel.SetActive(false);
        }

        public void ToggleAbility()
        {
            if (abilityPanel.activeSelf) HideAbility();
            else
            {
                abilityPanel.SetActive(true);
                RefreshAbility();
                if (GameManager.Instance.State == GameState.Playing) GameManager.Instance.PauseGame();
            }
        }

        public void HideAbility() { abilityPanel.SetActive(false); }

        public void ShowResult(StageResult result)
        {
            HideAll();
            resultPanel.SetActive(true);
            Text text = resultPanel.transform.Find("Body").GetComponent<Text>();
            text.text = "ROOM " + result.stage + " CLEAR!\n"
                + "클리어 시간  " + result.clearTime.ToString("0.0") + "초\n"
                + "남은 멘탈  " + result.remainingMental + " / " + result.maxMental + "\n"
                + "처치 " + result.kills + "   점수 " + result.score + "   GRADE " + result.grade + "\n"
                + "현재 런 시간  " + FormatRunTime(GameManager.Instance != null ? GameManager.Instance.RunElapsed : 0f);
            if (nextRoomButton != null) nextRoomButton.interactable = true;
        }

        public void ShowGameOver(string reason)
        {
            HideAll();
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.Find("Body").GetComponent<Text>().text = "F...\n\n" + reason + "\n잠깐 쉬고 다시 도전해 보세요.";
        }

        public void ShowFinalGrade(string grade, int score)
        {
            HideAll();
            finalPanel.SetActive(true);
            finalPanel.transform.Find("Body").GetComponent<Text>().text = "한 학기 종료\n\n최종 성적\n" + grade + "\n\n총점  " + score
                + "\n런 시간  " + FormatRunTime(GameManager.Instance != null ? GameManager.Instance.RunElapsed : 0f);
        }

        public void PlayCutscene(string[] lines, Action onComplete, bool ending)
        {
            HideAll();
            cutscenePanel.SetActive(true);
            cutsceneLines = lines;
            cutsceneComplete = onComplete;
            cutsceneIndex = 0;
            cutsceneText.text = lines.Length > 0 ? lines[0] : "";
            cutscenePanel.GetComponent<Image>().color = ending ? new Color(0.12f, 0.08f, 0.20f, 1f) : new Color(0.12f, 0.26f, 0.38f, 1f);
            Sprite sceneArt = Resources.Load<Sprite>(ending
                ? "FreshPixelArt/Backgrounds/FinalExamArchive"
                : "FreshPixelArt/Backgrounds/CampusLectureDungeon");
            if (sceneArt != null)
            {
                cutsceneBackdrop.sprite = sceneArt;
                cutsceneBackdrop.preserveAspect = true;
            }
            cutsceneCanvasGroup.alpha = 0f;
            cutsceneCharacter.color = ending ? new Color32(255, 202, 137, 255) : Color.white;
            nextCutsceneAdvance = Time.unscaledTime + 3.2f;
        }

        public void Toast(string message)
        {
            if (toastText == null) return;
            StopCoroutine("ToastRoutine");
            StartCoroutine(ToastRoutine(message));
        }

        private IEnumerator ToastRoutine(string message)
        {
            toastText.text = message;
            toastText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(2f);
            toastText.gameObject.SetActive(false);
        }

        private void AdvanceCutscene()
        {
            if (cutsceneLines == null) return;
            cutsceneIndex++;
            if (cutsceneIndex >= cutsceneLines.Length)
            {
                cutscenePanel.SetActive(false);
                var callback = cutsceneComplete;
                cutsceneComplete = null;
                cutsceneLines = null;
                if (callback != null) callback();
                return;
            }
            cutsceneText.text = cutsceneLines[cutsceneIndex];
            cutsceneCanvasGroup.alpha = 0f;
            nextCutsceneAdvance = Time.unscaledTime + 3.2f;
        }

        private void AnimateCutscene()
        {
            cutsceneCanvasGroup.alpha = Mathf.MoveTowards(cutsceneCanvasGroup.alpha, 1f, Time.unscaledDeltaTime * 1.8f);
            float wave = Mathf.Sin(Time.unscaledTime * 1.2f);
            cutsceneCharacter.rectTransform.anchoredPosition = new Vector2(wave * 12f, Mathf.Abs(wave) * 5f);
            cutsceneBackdrop.rectTransform.anchoredPosition = new Vector2(wave * -5f, 0f);
        }

        private void RefreshHud()
        {
            var player = GameManager.Instance.Player;
            var stage = GameManager.Instance.Stage;
            var inventory = player.GetComponent<PlayerInventory>();
            int nextExp = PlayerGrowth.ExpForNextLevel(player.level);
            hudLeft.text = "MENTAL  " + player.mental + " / " + player.maxMental
                + "\n공부량 " + player.studyPower + "  복습 " + player.review + "  공격 효율 " + player.attackEfficiency;
            string timer = stage.Current != null && stage.Current.timeLimit > 0 ? "\n남은 시간 " + Mathf.Max(0, stage.Current.timeLimit - stage.Elapsed).ToString("0.0") : "";
            string objective = stage.Current.objective != null && stage.Current.objective.Length > 28 ? stage.Current.objective.Substring(0, 28) + "..." : stage.Current.objective;
            hudRight.text = "ROOM " + stage.Current.stageNumber + "  " + stage.Current.stageName
                + "\n" + objective + "\n처치 " + stage.Kills + " / " + stage.Current.targetEnemyKillCount + "   SCORE " + player.score
                + "\nRUN " + FormatRunTime(GameManager.Instance.RunElapsed) + " / 30:00" + timer;
            InventoryEntry q = inventory.GetSlot(0);
            InventoryEntry e = inventory.GetSlot(1);
            hudBottom.text = "LV." + player.level + "   EXP " + player.exp + " / " + nextExp + "   GP " + player.growthPoint
                + "      Q " + ItemLabel(q) + "   E " + ItemLabel(e);
            SetItemIcon(qItemIcon, q);
            SetItemIcon(eItemIcon, e);
            BossController boss = FindObjectOfType<BossController>();
            bossText.gameObject.SetActive(boss != null);
            if (boss != null) bossText.text = boss.Data.bossName + "   HP " + Mathf.Max(0, boss.CurrentHp) + " / " + boss.Data.maxHp + "   PHASE " + boss.Phase;
            if (mentalFill != null) SetBar(mentalFill, player.mental / (float)Mathf.Max(1, player.maxMental));
            if (expFill != null) SetBar(expFill, player.exp / (float)Mathf.Max(1, nextExp));
            if (bossFill != null)
            {
                bossFill.transform.parent.gameObject.SetActive(boss != null);
                if (boss != null) SetBar(bossFill, boss.CurrentHp / (float)Mathf.Max(1, boss.Data.maxHp));
            }
            if (stageFill != null) SetBar(stageFill, player.transform.position.x / GameBalance.StageWidth);
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            int combo = combat != null ? combat.ComboCount : 0;
            comboText.gameObject.SetActive(combo > 1);
            if (combo > 1) comboText.text = combo + " HIT\nCOMBO";
        }

        private string ItemLabel(InventoryEntry entry)
        {
            if (entry == null) return "비어 있음";
            ItemData data;
            return GameManager.Instance.Items.TryGetValue(entry.itemId, out data) ? data.itemName + " x" + entry.count : entry.itemId;
        }

        private void SetItemIcon(Image image, InventoryEntry entry)
        {
            if (image == null) return;
            image.gameObject.SetActive(entry != null);
            if (entry != null) image.sprite = RuntimeArt.GetItem(entry.itemId);
        }

        private void RefreshAbility()
        {
            var p = GameManager.Instance.Player;
            if (p == null) return;
            abilityText.text = "성장 포인트: " + p.growthPoint + "\n\n멘탈 강화        " + p.maxMental
                + "\n공부량 강화      " + p.studyPower
                + "\n복습 강화        " + p.review
                + "\n공격 효율 강화   " + p.attackEfficiency
                + "\n이동 효율 강화   " + p.movementEfficiency;
        }

        private void Upgrade(UpgradeType type)
        {
            if (GameManager.Instance.Player == null || !GameManager.Instance.Player.TryUpgrade(type)) Toast("성장 포인트가 부족합니다.");
            RefreshAbility();
        }

        private void BuildTitle()
        {
            titlePanel = Panel("Title", new Color32(18, 25, 42, 255));
            Sprite campus = Resources.Load<Sprite>("FreshPixelArt/Backgrounds/CampusLectureDungeon");
            if (campus != null)
            {
                Image campusImage = AddRect(titlePanel.transform, new Color(0.48f, 0.58f, 0.68f, 1f), Vector2.zero, Vector2.one);
                campusImage.sprite = campus;
                campusImage.type = Image.Type.Simple;
                campusImage.preserveAspect = true;
            }
            AddRect(titlePanel.transform, new Color32(31, 22, 45, 118), new Vector2(0.03f, 0.05f), new Vector2(0.97f, 0.95f));
            AddRect(titlePanel.transform, new Color32(13, 12, 25, 138), new Vector2(0.045f, 0.07f), new Vector2(0.955f, 0.93f));
            AddRect(titlePanel.transform, new Color32(235, 174, 56, 255), new Vector2(0.12f, 0.72f), new Vector2(0.88f, 0.735f));
            Image hero = AddRect(titlePanel.transform, Color.white, new Vector2(0.08f, 0.18f), new Vector2(0.34f, 0.68f));
            hero.sprite = RuntimeArt.GetPlayer();
            hero.preserveAspect = true;
            hero.type = Image.Type.Simple;
            Image exam = AddRect(titlePanel.transform, new Color32(255, 144, 144, 255), new Vector2(0.69f, 0.17f), new Vector2(0.94f, 0.67f));
            exam.sprite = RuntimeArt.GetBoss(10);
            exam.preserveAspect = true;
            exam.type = Image.Type.Simple;
            Debug.Log("Project A+ title production art: player=" + (hero.sprite != null ? hero.sprite.name : "missing")
                + ", boss=" + (exam.sprite != null ? exam.sprite.name : "missing"));
            MakeText(titlePanel.transform, "PROJECT A+", 78, TextAnchor.MiddleCenter, new Color32(255, 210, 73, 255), new Vector2(0.15f, 0.72f), new Vector2(0.85f, 0.91f));
            MakeText(titlePanel.transform, "30분 한 학기를 돌파하는 로그라이트 액션", 24, TextAnchor.MiddleCenter, new Color32(190, 218, 226, 255), new Vector2(0.2f, 0.64f), new Vector2(0.8f, 0.72f));
            MakeText(titlePanel.transform, "10 ROOMS  /  TWO EXAMS  /  OFFLINE SINGLE PLAYER", 16, TextAnchor.MiddleCenter, new Color32(111, 147, 166, 255), new Vector2(0.2f, 0.59f), new Vector2(0.8f, 0.64f));
            Button(titlePanel.transform, "학기 시작", new Vector2(0.39f, 0.44f), new Vector2(0.61f, 0.525f), GameManager.Instance.StartGame);
            continueButton = Button(titlePanel.transform, "이어하기", new Vector2(0.39f, 0.335f), new Vector2(0.61f, 0.42f), GameManager.Instance.ContinueGame);
            Button(titlePanel.transform, "설정", new Vector2(0.39f, 0.23f), new Vector2(0.61f, 0.315f), delegate { settingsPanel.SetActive(true); });
            Button(titlePanel.transform, "종료", new Vector2(0.39f, 0.125f), new Vector2(0.61f, 0.21f), GameManager.Instance.QuitGame);
            MakeText(titlePanel.transform, "A/D 이동   SPACE 점프   J 공격   K 회피", 17, TextAnchor.MiddleCenter, new Color32(136, 158, 175, 255), new Vector2(0.18f, 0.06f), new Vector2(0.82f, 0.11f));
        }

        private void BuildHud()
        {
            hudPanel = Panel("HUD", new Color(0, 0, 0, 0));
            hudPanel.GetComponent<Image>().raycastTarget = false;
            AddRect(hudPanel.transform, new Color32(16, 20, 29, 205), new Vector2(0.012f, 0.865f), new Vector2(0.285f, 0.985f));
            AddRect(hudPanel.transform, new Color32(16, 20, 29, 205), new Vector2(0.70f, 0.875f), new Vector2(0.988f, 0.985f));
            AddRect(hudPanel.transform, new Color32(16, 20, 29, 205), new Vector2(0.27f, 0.008f), new Vector2(0.73f, 0.072f));
            hudLeft = MakeText(hudPanel.transform, "", 16, TextAnchor.UpperLeft, new Color32(244, 238, 218, 255), new Vector2(0.025f, 0.89f), new Vector2(0.272f, 0.972f));
            hudRight = MakeText(hudPanel.transform, "", 16, TextAnchor.UpperRight, new Color32(244, 238, 218, 255), new Vector2(0.715f, 0.895f), new Vector2(0.975f, 0.972f));
            hudBottom = MakeText(hudPanel.transform, "", 14, TextAnchor.MiddleCenter, new Color32(224, 218, 199, 255), new Vector2(0.292f, 0.016f), new Vector2(0.708f, 0.063f));
            bossText = MakeText(hudPanel.transform, "", 18, TextAnchor.MiddleCenter, new Color32(255, 190, 116, 255), new Vector2(0.31f, 0.935f), new Vector2(0.69f, 0.982f));
            comboText = MakeText(hudPanel.transform, "", 27, TextAnchor.MiddleLeft, new Color32(255, 207, 66, 255), new Vector2(0.025f, 0.49f), new Vector2(0.22f, 0.67f));
            CreateBar(hudPanel.transform, new Vector2(0.025f, 0.868f), new Vector2(0.272f, 0.882f), new Color32(72, 218, 128, 255), out mentalFill);
            CreateBar(hudPanel.transform, new Vector2(0.29f, 0.009f), new Vector2(0.71f, 0.019f), new Color32(73, 181, 236, 255), out expFill);
            CreateBar(hudPanel.transform, new Vector2(0.29f, 0.022f), new Vector2(0.71f, 0.032f), new Color32(242, 177, 55, 255), out stageFill);
            CreateBar(hudPanel.transform, new Vector2(0.32f, 0.92f), new Vector2(0.68f, 0.932f), new Color32(235, 70, 80, 255), out bossFill);
            qItemIcon = AddRect(hudPanel.transform, Color.white, new Vector2(0.74f, 0.015f), new Vector2(0.775f, 0.075f));
            qItemIcon.preserveAspect = true;
            qItemIcon.type = Image.Type.Simple;
            eItemIcon = AddRect(hudPanel.transform, Color.white, new Vector2(0.782f, 0.015f), new Vector2(0.817f, 0.075f));
            eItemIcon.preserveAspect = true;
            eItemIcon.type = Image.Type.Simple;
            bossFill.transform.parent.gameObject.SetActive(false);
        }

        private void BuildPause()
        {
            pausePanel = Panel("Pause", new Color(0, 0, 0, 0.78f));
            MakeText(pausePanel.transform, "잠깐 휴식", 48, TextAnchor.MiddleCenter, Color.white, new Vector2(0.3f, 0.67f), new Vector2(0.7f, 0.82f));
            Button(pausePanel.transform, "계속하기", new Vector2(0.4f, 0.50f), new Vector2(0.6f, 0.58f), GameManager.Instance.ResumeGame);
            Button(pausePanel.transform, "설정", new Vector2(0.4f, 0.39f), new Vector2(0.6f, 0.47f), delegate { settingsPanel.SetActive(true); });
            Button(pausePanel.transform, "타이틀로", new Vector2(0.4f, 0.28f), new Vector2(0.6f, 0.36f), GameManager.Instance.ShowTitle);
            Button(pausePanel.transform, "종료", new Vector2(0.4f, 0.17f), new Vector2(0.6f, 0.25f), GameManager.Instance.QuitGame);
        }

        private void BuildAbility()
        {
            abilityPanel = Panel("Ability", new Color(0.06f, 0.09f, 0.15f, 0.96f));
            MakeText(abilityPanel.transform, "ABILITY POINT", 42, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.24f), new Vector2(0.25f, 0.78f), new Vector2(0.75f, 0.91f));
            abilityText = MakeText(abilityPanel.transform, "", 25, TextAnchor.UpperLeft, Color.white, new Vector2(0.31f, 0.29f), new Vector2(0.65f, 0.75f));
            UpgradeButton(UpgradeType.Mental, 0.65f);
            UpgradeButton(UpgradeType.StudyPower, 0.56f);
            UpgradeButton(UpgradeType.Review, 0.47f);
            UpgradeButton(UpgradeType.AttackEfficiency, 0.38f);
            UpgradeButton(UpgradeType.MovementEfficiency, 0.29f);
            Button(abilityPanel.transform, "닫기", new Vector2(0.43f, 0.13f), new Vector2(0.57f, 0.21f), delegate { HideAbility(); GameManager.Instance.ResumeGame(); });
        }

        private void UpgradeButton(UpgradeType type, float y)
        {
            Button(abilityPanel.transform, "+", new Vector2(0.66f, y), new Vector2(0.71f, y + 0.06f), delegate { Upgrade(type); });
        }

        private void BuildResult()
        {
            resultPanel = Panel("Result", new Color(0.08f, 0.13f, 0.22f, 0.98f));
            MakeText(resultPanel.transform, "ROOM CLEAR", 40, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.24f), new Vector2(0.2f, 0.82f), new Vector2(0.8f, 0.94f));
            Text body = MakeText(resultPanel.transform, "", 24, TextAnchor.MiddleCenter, Color.white, new Vector2(0.2f, 0.32f), new Vector2(0.8f, 0.80f));
            body.name = "Body";
            MakeText(resultPanel.transform, "스테이지 보상은 자동으로 적용됩니다.", 19, TextAnchor.MiddleCenter, new Color32(208, 221, 227, 255), new Vector2(0.2f, 0.25f), new Vector2(0.8f, 0.31f));
            nextRoomButton = Button(resultPanel.transform, "다음 방", new Vector2(0.34f, 0.10f), new Vector2(0.49f, 0.18f), GameManager.Instance.GoToNextStage);
            Button(resultPanel.transform, "재도전", new Vector2(0.51f, 0.10f), new Vector2(0.66f, 0.18f), GameManager.Instance.RetryStage);
        }

        private static string FormatRunTime(float seconds)
        {
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }

        private void BuildCutscene()
        {
            cutscenePanel = Panel("Cutscene", new Color(0.12f, 0.26f, 0.38f, 1f));
            cutsceneCanvasGroup = cutscenePanel.AddComponent<CanvasGroup>();
            AddRect(cutscenePanel.transform, new Color32(12, 18, 31, 170), new Vector2(0.02f, 0.04f), new Vector2(0.98f, 0.96f));
            cutsceneBackdrop = AddRect(cutscenePanel.transform, new Color32(90, 137, 158, 255), new Vector2(0.42f, 0.18f), new Vector2(0.93f, 0.82f));
            cutsceneBackdrop.sprite = RuntimeArt.GetProp("window", new Color32(85, 151, 184, 255));
            cutsceneBackdrop.preserveAspect = true;
            cutsceneBackdrop.type = Image.Type.Simple;
            cutsceneCharacter = AddRect(cutscenePanel.transform, Color.white, new Vector2(0.08f, 0.18f), new Vector2(0.38f, 0.73f));
            cutsceneCharacter.sprite = RuntimeArt.GetPlayer();
            cutsceneCharacter.preserveAspect = true;
            cutsceneCharacter.type = Image.Type.Simple;
            MakeText(cutscenePanel.transform, "PROJECT A+  |  IN-GAME CUTSCENE", 20, TextAnchor.UpperLeft, new Color(1f, 0.84f, 0.24f), new Vector2(0.04f, 0.88f), new Vector2(0.96f, 0.96f));
            AddRect(cutscenePanel.transform, new Color32(13, 19, 32, 235), new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.25f));
            cutsceneText = MakeText(cutscenePanel.transform, "", 31, TextAnchor.MiddleCenter, Color.white, new Vector2(0.1f, 0.09f), new Vector2(0.9f, 0.24f));
            MakeText(cutscenePanel.transform, "ENTER: 다음 장면", 18, TextAnchor.LowerRight, Color.white, new Vector2(0.65f, 0.04f), new Vector2(0.96f, 0.12f));
        }

        private void BuildGameOver()
        {
            gameOverPanel = Panel("GameOver", new Color(0.18f, 0.05f, 0.08f, 0.98f));
            Text body = MakeText(gameOverPanel.transform, "", 35, TextAnchor.MiddleCenter, Color.white, new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.75f));
            body.name = "Body";
            Button(gameOverPanel.transform, "재도전", new Vector2(0.38f, 0.19f), new Vector2(0.49f, 0.27f), GameManager.Instance.RetryStage);
            Button(gameOverPanel.transform, "타이틀", new Vector2(0.52f, 0.19f), new Vector2(0.63f, 0.27f), GameManager.Instance.ShowTitle);
        }

        private void BuildFinal()
        {
            finalPanel = Panel("Final", new Color(0.08f, 0.15f, 0.25f, 1f));
            Text body = MakeText(finalPanel.transform, "", 48, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.24f), new Vector2(0.2f, 0.28f), new Vector2(0.8f, 0.82f));
            body.name = "Body";
            Button(finalPanel.transform, "타이틀로", new Vector2(0.42f, 0.14f), new Vector2(0.58f, 0.22f), GameManager.Instance.ShowTitle);
        }

        private void BuildSettings()
        {
            settingsPanel = Panel("Settings", new Color(0.05f, 0.07f, 0.12f, 0.98f));
            MakeText(settingsPanel.transform, "SETTING", 45, TextAnchor.MiddleCenter, Color.white, new Vector2(0.3f, 0.82f), new Vector2(0.7f, 0.94f));
            MakeText(settingsPanel.transform, "BGM 볼륨", 23, TextAnchor.MiddleLeft, Color.white, new Vector2(0.27f, 0.68f), new Vector2(0.43f, 0.75f));
            Slider(settingsPanel.transform, new Vector2(0.44f, 0.69f), new Vector2(0.72f, 0.74f), GameManager.Instance.Settings.Data.bgmVolume, GameManager.Instance.Settings.SetBgm);
            MakeText(settingsPanel.transform, "SFX 볼륨", 23, TextAnchor.MiddleLeft, Color.white, new Vector2(0.27f, 0.58f), new Vector2(0.43f, 0.65f));
            Slider(settingsPanel.transform, new Vector2(0.44f, 0.59f), new Vector2(0.72f, 0.64f), GameManager.Instance.Settings.Data.sfxVolume, GameManager.Instance.Settings.SetSfx);
            MakeText(settingsPanel.transform, "해상도", 23, TextAnchor.MiddleLeft, Color.white, new Vector2(0.27f, 0.47f), new Vector2(0.41f, 0.54f));
            Button(settingsPanel.transform, "<", new Vector2(0.42f, 0.47f), new Vector2(0.47f, 0.54f), GameManager.Instance.Settings.PreviousResolution);
            resolutionText = MakeText(settingsPanel.transform, "", 22, TextAnchor.MiddleCenter, new Color32(255, 214, 89, 255), new Vector2(0.48f, 0.47f), new Vector2(0.62f, 0.54f));
            Button(settingsPanel.transform, ">", new Vector2(0.63f, 0.47f), new Vector2(0.68f, 0.54f), GameManager.Instance.Settings.NextResolution);
            Button(settingsPanel.transform, "화면 모드 전환", new Vector2(0.38f, 0.36f), new Vector2(0.55f, 0.43f), GameManager.Instance.Settings.ToggleFullscreen);
            displayModeText = MakeText(settingsPanel.transform, "", 21, TextAnchor.MiddleLeft, Color.white, new Vector2(0.57f, 0.36f), new Vector2(0.72f, 0.43f));
            MakeText(settingsPanel.transform, "키: A/D 이동, SPACE 점프, J 공격, S+J 내려찍기, K 회피, Q/E 아이템", 18, TextAnchor.MiddleCenter, Color.white, new Vector2(0.12f, 0.22f), new Vector2(0.88f, 0.31f));
            Button(settingsPanel.transform, "닫기", new Vector2(0.43f, 0.1f), new Vector2(0.57f, 0.18f), delegate { settingsPanel.SetActive(false); });
            RefreshSettings();
        }

        private void RefreshSettings()
        {
            if (resolutionText != null) resolutionText.text = GameManager.Instance.Settings.ResolutionLabel();
            if (displayModeText != null) displayModeText.text = GameManager.Instance.Settings.DisplayModeLabel();
        }

        private void HideAll()
        {
            if (titlePanel != null) titlePanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (abilityPanel != null) abilityPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(false);
            if (cutscenePanel != null) cutscenePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (finalPanel != null) finalPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private GameObject Panel(string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(canvas.transform, false);
            var image = go.AddComponent<Image>();
            image.sprite = RuntimeArt.GetUiFrame();
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = color;
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.SetActive(false);
            return go;
        }

        private Text MakeText(Transform parent, string value, int size, TextAnchor anchor, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.alignByGeometry = true;
            text.fontStyle = FontStyle.Bold;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color32(8, 9, 15, 230);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return text;
        }

        private Button Button(Transform parent, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(label + " Button");
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.sprite = RuntimeArt.GetUiButton();
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = Color.white;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color32(255, 222, 133, 255);
            colors.pressedColor = new Color32(255, 157, 91, 255);
            colors.selectedColor = new Color32(255, 222, 133, 255);
            colors.disabledColor = new Color32(75, 75, 86, 180);
            colors.fadeDuration = 0f;
            button.colors = colors;
            button.onClick.AddListener(action);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            MakeText(go.transform, label, 22, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one);
            return button;
        }

        private Image AddRect(Transform parent, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject("Frame");
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.sprite = RuntimeArt.GetUiFrame();
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = color;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return image;
        }

        private void CreateBar(Transform parent, Vector2 min, Vector2 max, Color color, out Image fill)
        {
            Image background = AddRect(parent, new Color32(21, 26, 37, 245), min, max);
            background.sprite = RuntimeArt.GetUiSlot();
            background.type = Image.Type.Sliced;
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(background.transform, false);
            fill = fillGo.AddComponent<Image>();
            fill.color = color;
            fill.sprite = RuntimeArt.Solid("ui_bar_fill", Color.white);
            fill.type = Image.Type.Sliced;
            fill.raycastTarget = false;
            RectTransform rect = fill.rectTransform;
            rect.anchorMin = new Vector2(0.01f, 0.16f);
            rect.anchorMax = new Vector2(0.99f, 0.84f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private void SetBar(Image fill, float ratio)
        {
            RectTransform rect = fill.rectTransform;
            rect.anchorMax = new Vector2(Mathf.Lerp(0.01f, 0.99f, Mathf.Clamp01(ratio)), rect.anchorMax.y);
        }

        private void Slider(Transform parent, Vector2 min, Vector2 max, float value, UnityEngine.Events.UnityAction<float> action)
        {
            var go = new GameObject("Slider");
            go.transform.SetParent(parent, false);
            var background = go.AddComponent<Image>();
            background.sprite = RuntimeArt.GetUiSlot();
            background.type = Image.Type.Sliced;
            background.color = Color.white;
            var slider = go.AddComponent<Slider>();
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(go.transform, false);
            var fillImage = fillGo.AddComponent<Image>();
            fillImage.color = new Color(0.25f, 0.75f, 0.95f, 1);
            fillImage.sprite = RuntimeArt.Solid("ui_slider_fill", Color.white);
            fillImage.type = Image.Type.Sliced;
            RectTransform fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(4, 4);
            fillRect.offsetMax = new Vector2(-4, -4);
            var handleGo = new GameObject("Handle");
            handleGo.transform.SetParent(go.transform, false);
            var handleImage = handleGo.AddComponent<Image>();
            handleImage.sprite = RuntimeArt.GetUiButton();
            handleImage.type = Image.Type.Sliced;
            handleImage.color = new Color32(255, 209, 86, 255);
            RectTransform handleRect = handleGo.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 30);
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.minValue = 0; slider.maxValue = 1; slider.value = value;
            slider.onValueChanged.AddListener(action);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            DontDestroyOnLoad(go);
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
    }
}
