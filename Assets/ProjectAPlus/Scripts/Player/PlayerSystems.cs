using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAPlus
{
    public class PlayerStatus : MonoBehaviour
    {
        public int mental = 100;
        public int maxMental = 100;
        public int studyPower = 14;
        public int review = 2;
        public int attackEfficiency = 1;
        public int movementEfficiency = 1;
        public int exp;
        public int level = 1;
        public int growthPoint;
        public int score;
        public bool IsDead { get { return mental <= 0; } }

        public void ApplySave(SaveData data)
        {
            mental = Mathf.Clamp(data.mental, 1, data.maxMental);
            maxMental = Mathf.Max(1, data.maxMental);
            studyPower = Mathf.Max(1, data.studyPower);
            review = Mathf.Max(0, data.review);
            attackEfficiency = Mathf.Max(0, data.attackEfficiency);
            movementEfficiency = Mathf.Max(0, data.movementEfficiency);
            exp = Mathf.Max(0, data.exp);
            level = Mathf.Max(1, data.playerLevel);
            growthPoint = Mathf.Max(0, data.growthPoint);
            score = Mathf.Max(0, data.score);
        }

        public void WriteSave(SaveData data)
        {
            data.mental = mental;
            data.maxMental = maxMental;
            data.studyPower = studyPower;
            data.review = review;
            data.attackEfficiency = attackEfficiency;
            data.movementEfficiency = movementEfficiency;
            data.exp = exp;
            data.playerLevel = level;
            data.growthPoint = growthPoint;
            data.score = score;
        }

        public bool TryUpgrade(UpgradeType type)
        {
            if (growthPoint <= 0) return false;
            growthPoint--;
            switch (type)
            {
                case UpgradeType.Mental: maxMental += 15; mental = Mathf.Min(maxMental, mental + 15); break;
                case UpgradeType.StudyPower: studyPower += 4; break;
                case UpgradeType.Review: review += 2; break;
                case UpgradeType.AttackEfficiency: attackEfficiency += 1; break;
                case UpgradeType.MovementEfficiency: movementEfficiency += 1; break;
            }
            if (GameManager.Instance != null) GameManager.Instance.SaveProgress();
            return true;
        }

        public void ApplyRunUpgrade(RunUpgradeType type)
        {
            switch (type)
            {
                case RunUpgradeType.Coffee:
                    attackEfficiency += 1;
                    movementEfficiency += 1;
                    Heal(10);
                    break;
                case RunUpgradeType.LectureNotes:
                    studyPower += 5;
                    break;
                case RunUpgradeType.SummarySheet:
                    review += 4;
                    Heal(15);
                    break;
                case RunUpgradeType.Highlighter:
                    studyPower += 3;
                    review += 2;
                    break;
                case RunUpgradeType.AllNighter:
                    studyPower += 8;
                    attackEfficiency += 1;
                    maxMental = Mathf.Max(50, maxMental - 10);
                    mental = Mathf.Min(mental, maxMental);
                    break;
            }
        }

        public void Heal(int amount) { mental = Mathf.Clamp(mental + Mathf.Max(0, amount), 0, maxMental); }
    }

    public class PlayerGrowth : MonoBehaviour
    {
        private PlayerStatus status;
        private void Awake() { status = GetComponent<PlayerStatus>(); }

        public void AddExp(int amount)
        {
            if (status == null) return;
            status.exp += Mathf.Max(0, amount);
            while (status.exp >= ExpForNextLevel(status.level))
            {
                status.exp -= ExpForNextLevel(status.level);
                status.level++;
                status.growthPoint++;
                status.Heal(15);
                if (AudioManager.Instance != null) AudioManager.Instance.Play("levelUp");
                if (GameManager.Instance != null) GameManager.Instance.UI.Toast("LEVEL UP! 성장 포인트 +1");
            }
        }

        public static int ExpForNextLevel(int level) { return GameBalance.LevelExpBase + Mathf.Max(0, level - 1) * 45; }
    }

    public class PlayerInventory : MonoBehaviour
    {
        public List<InventoryEntry> items = new List<InventoryEntry>();
        public int ItemsUsed { get; private set; }
        public float StudyBuff { get; private set; }
        public float EfficiencyBuff { get; private set; }
        public float BossDamageBuff { get; private set; }
        public float PresentationGuard { get; private set; }
        private float coffeeUntil;

        public void ApplySave(List<InventoryEntry> saved)
        {
            items = new List<InventoryEntry>();
            if (saved == null) return;
            foreach (var entry in saved) items.Add(new InventoryEntry(entry.itemId, entry.count));
        }

        public List<InventoryEntry> CopyForSave()
        {
            var result = new List<InventoryEntry>();
            foreach (var item in items) if (item.count > 0) result.Add(new InventoryEntry(item.itemId, item.count));
            return result;
        }

        public void AddItem(string id, int count = 1)
        {
            if (string.IsNullOrEmpty(id) || count <= 0) return;
            InventoryEntry found = items.Find(i => i.itemId == id);
            if (found == null) items.Add(new InventoryEntry(id, count)); else found.count += count;
            if (GameManager.Instance != null && GameManager.Instance.Player == GetComponent<PlayerStatus>())
                GameManager.Instance.SaveProgress();
        }

        public InventoryEntry GetSlot(int slot)
        {
            var available = items.FindAll(i => i.count > 0);
            return slot >= 0 && slot < available.Count ? available[slot] : null;
        }

        public void UseSlot(int slot)
        {
            InventoryEntry entry = GetSlot(slot);
            if (entry == null || GameManager.Instance == null)
            {
                if (GameManager.Instance != null) GameManager.Instance.UI.Toast("해당 아이템 슬롯이 비어 있습니다.");
                return;
            }
            ItemData data;
            if (!GameManager.Instance.Items.TryGetValue(entry.itemId, out data)) return;
            var status = GetComponent<PlayerStatus>();
            if (data.itemType == ItemType.AttackEfficiencyBuff && Time.time < coffeeUntil)
            {
                GameManager.Instance.UI.Toast("카페인을 너무 많이 섭취했습니다!");
                return;
            }
            entry.count--;
            ItemsUsed++;
            StopCoroutine("ClearBuffsLater");
            switch (data.itemType)
            {
                case ItemType.Heal:
                case ItemType.FinalHeal: status.Heal(Mathf.RoundToInt(data.value)); break;
                case ItemType.AttackEfficiencyBuff: EfficiencyBuff = data.value; coffeeUntil = Time.time + data.duration; StartCoroutine(ClearBuffsLater(data.duration, ItemType.AttackEfficiencyBuff)); break;
                case ItemType.StudyPowerBuff: StudyBuff = data.value; StartCoroutine(ClearBuffsLater(data.duration, ItemType.StudyPowerBuff)); break;
                case ItemType.BossDamageBuff: BossDamageBuff = data.value; StartCoroutine(ClearBuffsLater(data.duration, ItemType.BossDamageBuff)); break;
                case ItemType.PresentationGuard: PresentationGuard = data.value; StartCoroutine(ClearBuffsLater(data.duration, ItemType.PresentationGuard)); break;
                case ItemType.Stealth: EfficiencyBuff = data.value; StartCoroutine(ClearBuffsLater(data.duration, ItemType.Stealth)); break;
            }
            if (AudioManager.Instance != null) AudioManager.Instance.Play("item");
            GameManager.Instance.UI.Toast(data.itemName + " 사용!");
            GameManager.Instance.SaveProgress();
        }

        private IEnumerator ClearBuffsLater(float seconds, ItemType type)
        {
            yield return new WaitForSeconds(seconds);
            if (type == ItemType.AttackEfficiencyBuff || type == ItemType.Stealth) EfficiencyBuff = 0;
            if (type == ItemType.StudyPowerBuff) StudyBuff = 0;
            if (type == ItemType.BossDamageBuff) BossDamageBuff = 0;
            if (type == ItemType.PresentationGuard) PresentationGuard = 0;
        }
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D body;
        private BoxCollider2D box;
        private PlayerStatus status;
        private PlayerInventory inventory;
        private SpriteRenderer sprite;
        private bool grounded;
        private bool dodging;
        private float dodgeReadyTime;
        private float groundedUntil;
        private float jumpQueuedUntil;
        public int Facing { get; private set; } = 1;
        public bool IsGrounded { get { return grounded; } }
        public bool IsDodging { get { return dodging; } }
        public bool CanAct { get { return !dodging && status != null && !status.IsDead && GameManager.Instance != null && GameManager.Instance.State == GameState.Playing; } }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
            status = GetComponent<PlayerStatus>();
            inventory = GetComponent<PlayerInventory>();
            sprite = GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            if (!CanAct) return;
            if (transform.position.y < -8f)
            {
                GameManager.Instance.GameOver("강의실 밖으로 떨어졌습니다.");
                return;
            }
            float axis = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) axis -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) axis += 1f;
            if (Mathf.Abs(axis) > 0.01f) Facing = axis > 0 ? 1 : -1;
            float speed = GameBalance.BaseMoveSpeed + status.movementEfficiency * 0.28f + inventory.EfficiencyBuff * 0.15f;
            body.velocity = new Vector2(axis * speed, body.velocity.y);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) jumpQueuedUntil = Time.time + 0.12f;
            if (jumpQueuedUntil >= Time.time && (grounded || groundedUntil >= Time.time))
            {
                body.velocity = new Vector2(body.velocity.x, GameBalance.BaseJumpPower + status.movementEfficiency * 0.18f);
                grounded = false;
                groundedUntil = 0f;
                jumpQueuedUntil = 0f;
                if (AudioManager.Instance != null) AudioManager.Instance.Play("jump");
            }
            if ((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow)) && body.velocity.y > 3f)
                body.velocity = new Vector2(body.velocity.x, body.velocity.y * 0.55f);
            if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.LeftShift)) TryDodge();
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) StartCoroutine(DropThrough());
            if (Input.GetKeyDown(KeyCode.Q)) inventory.UseSlot(0);
            if (Input.GetKeyDown(KeyCode.E)) inventory.UseSlot(1);
            if (sprite != null) sprite.flipX = Facing < 0;
        }

        private void FixedUpdate()
        {
            if (body == null) return;
            bool holdingJump = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            body.gravityScale = body.velocity.y > 0.1f && holdingJump
                ? GameBalance.PlayerRiseGravityScale
                : GameBalance.PlayerFallGravityScale;
        }

        private void TryDodge()
        {
            if (Time.time < dodgeReadyTime) return;
            dodgeReadyTime = Time.time + Mathf.Max(0.45f, GameBalance.BaseDodgeCooldown - status.movementEfficiency * 0.05f);
            StartCoroutine(DodgeRoutine());
        }

        private IEnumerator DodgeRoutine()
        {
            dodging = true;
            GetComponent<PlayerHitHandler>().SetInvulnerable(0.28f);
            float until = Time.time + 0.2f;
            float afterimageAt = 0f;
            while (Time.time < until)
            {
                body.velocity = new Vector2(Facing * (11f + status.movementEfficiency * 0.35f), body.velocity.y * 0.25f);
                if (Time.time >= afterimageAt)
                {
                    afterimageAt = Time.time + 0.045f;
                    CombatFx.Afterimage(sprite);
                }
                yield return null;
            }
            dodging = false;
        }

        private IEnumerator DropThrough()
        {
            if (transform.position.y < 1.2f) yield break;
            box.enabled = false;
            body.velocity = new Vector2(body.velocity.x, -4f);
            yield return new WaitForSeconds(0.16f);
            box.enabled = true;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            for (int i = 0; i < collision.contactCount; i++) if (collision.GetContact(i).normal.y > 0.45f) { grounded = true; groundedUntil = Time.time + 0.1f; }
        }
        private void OnCollisionExit2D(Collision2D collision) { if (body.velocity.y > 0.1f) { grounded = false; groundedUntil = Time.time + 0.1f; } }
    }

    public class PlayerCombat : MonoBehaviour
    {
        private PlayerController controller;
        private PlayerStatus status;
        private PlayerInventory inventory;
        private float nextAttack;
        private float chainExpires;
        private float comboExpires;
        private int chainStep;
        public int ComboCount { get; private set; }
        public int VisualAttackStep { get; private set; }
        public float VisualAttackUntil { get; private set; }

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            status = GetComponent<PlayerStatus>();
            inventory = GetComponent<PlayerInventory>();
        }

        private void Update()
        {
            if (controller == null || !controller.CanAct) return;
            if (ComboCount > 0 && Time.time > comboExpires) ComboCount = 0;
            if ((Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)) && Time.time >= nextAttack) Attack();
        }

        private void Attack()
        {
            if (Time.time > chainExpires) chainStep = 0;
            chainStep = chainStep % 3 + 1;
            VisualAttackStep = chainStep;
            VisualAttackUntil = Time.time + (chainStep == 3 ? 0.22f : 0.16f);
            chainExpires = Time.time + 0.62f;
            bool downStrike = !controller.IsGrounded && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
            float efficiency = status.attackEfficiency + inventory.EfficiencyBuff;
            nextAttack = Time.time + Mathf.Max(0.16f, GameBalance.BaseAttackCooldown - efficiency * 0.035f);
            float reach = chainStep == 3 ? 1.2f : 0.92f;
            Rigidbody2D attackBody = GetComponent<Rigidbody2D>();
            if (attackBody != null && controller.IsGrounded)
                attackBody.velocity = new Vector2(controller.Facing * (chainStep == 3 ? 4.2f : 2.4f), attackBody.velocity.y);
            Vector2 center = downStrike ? (Vector2)transform.position + Vector2.down * 1.05f : (Vector2)transform.position + Vector2.right * controller.Facing * reach;
            Vector2 attackSize = downStrike ? new Vector2(1.7f, 2.3f) : chainStep == 3 ? new Vector2(2.45f, 1.8f) : new Vector2(1.9f, 1.4f);
            var hits = Physics2D.OverlapBoxAll(center, attackSize, 0);
            bool critical;
            int damage = DamageCalculator.CalculatePlayerDamage(status.studyPower + Mathf.RoundToInt(inventory.StudyBuff), status.review, 0, Random.Range(-2, 3), out critical);
            damage = Mathf.RoundToInt(damage * (1f + (chainStep - 1) * 0.16f));
            if (downStrike) damage = Mathf.RoundToInt(damage * 1.3f);
            bool landed = false;
            var damaged = new HashSet<Hurtbox>();
            foreach (var hit in hits)
            {
                var hurtbox = hit.GetComponent<Hurtbox>();
                if (hurtbox == null || hurtbox.team != CombatTeam.Enemy || !damaged.Add(hurtbox)) continue;
                int finalDamage = FindObjectOfType<BossController>() != null ? Mathf.RoundToInt(damage * (1f + inventory.BossDamageBuff)) : damage;
                hurtbox.DamageEnemy(finalDamage, controller.Facing);
                CombatFx.HitSpark(hit.ClosestPoint(center), critical || chainStep == 3, controller.Facing);
                landed = true;
                ComboCount++;
            }
            if (downStrike)
            {
                Rigidbody2D body = GetComponent<Rigidbody2D>();
                if (body != null) body.velocity = new Vector2(body.velocity.x, -9f);
            }
            StartCoroutine(AttackFlash(center, critical, chainStep, downStrike));
            if (chainStep == 3) CombatFx.Afterimage(GetComponentInChildren<SpriteRenderer>());
            if (landed)
            {
                comboExpires = Time.time + 2f;
                CombatFx.HitStop(chainStep == 3 ? 0.075f : 0.045f);
                CameraFollow.Shake(chainStep == 3 || critical ? 0.2f : 0.1f, 0.14f);
            }
            if (AudioManager.Instance != null) AudioManager.Instance.Play("attack");
        }

        public void ResetCombo() { ComboCount = 0; chainStep = 0; }

        private IEnumerator AttackFlash(Vector2 position, bool critical, int step, bool downStrike)
        {
            var go = new GameObject("AttackEffect");
            go.transform.position = position;
            float scale = step == 3 ? 1.55f : critical ? 1.35f : 1.05f + step * 0.08f;
            go.transform.localScale = new Vector3(controller.Facing * scale, scale, 1);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeArt.GetSlash();
            sr.color = step == 3 ? new Color32(255, 130, 73, 255) : Color.white;
            sr.sortingOrder = 8;
            float angle = step == 1 ? -24f : step == 2 ? 18f : -4f;
            go.transform.rotation = Quaternion.Euler(0, 0, downStrike ? 90f : controller.Facing > 0 ? angle : -angle);
            yield return new WaitForSeconds(step == 3 ? 0.15f : 0.11f);
            Destroy(go);
        }
    }

    public class PlayerHitHandler : MonoBehaviour
    {
        private PlayerStatus status;
        private Rigidbody2D body;
        private SpriteRenderer sprite;
        private float invulnerableUntil;
        public int DamageTaken { get; private set; }
        public float HitVisualUntil { get; private set; }

        private void Awake() { status = GetComponent<PlayerStatus>(); body = GetComponent<Rigidbody2D>(); sprite = GetComponentInChildren<SpriteRenderer>(); }
        public void SetInvulnerable(float seconds) { invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds); }

        public void TakeDamage(int amount, Vector2 source, bool presentation = false)
        {
            if (status == null || status.IsDead || Time.time < invulnerableUntil) return;
            var inventory = GetComponent<PlayerInventory>();
            if (presentation && inventory != null) amount = Mathf.RoundToInt(amount * (1f - inventory.PresentationGuard));
            amount = Mathf.Max(1, amount);
            status.mental = Mathf.Max(0, status.mental - amount);
            var combat = GetComponent<PlayerCombat>();
            if (combat != null) combat.ResetCombo();
            DamageTaken += amount;
            HitVisualUntil = Time.time + 0.22f;
            invulnerableUntil = Time.time + 0.75f;
            if (body != null) body.velocity = new Vector2(transform.position.x < source.x ? -5f : 5f, 5f);
            CombatFx.Burst(transform.position, new Color32(255, 102, 91, 255), 9);
            CameraFollow.Shake(0.22f, 0.2f);
            StartCoroutine(Flash());
            if (AudioManager.Instance != null) AudioManager.Instance.Play("hit");
            if (status.IsDead && GameManager.Instance != null) GameManager.Instance.GameOver();
        }

        private IEnumerator Flash()
        {
            if (sprite == null) yield break;
            for (int i = 0; i < 5; i++)
            {
                sprite.enabled = !sprite.enabled;
                yield return new WaitForSeconds(0.08f);
            }
            sprite.enabled = true;
        }
    }
}
