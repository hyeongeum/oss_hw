using System.Collections;
using UnityEngine;

namespace ProjectAPlus
{
    public class EnemyStatus : MonoBehaviour
    {
        public EnemyData Data { get; private set; }
        public int CurrentHp { get; private set; }
        public void Initialize(EnemyData data) { Data = data; CurrentHp = data != null ? data.maxHp : 0; }
        public void SetCurrentHp(int hp) { CurrentHp = Mathf.Max(0, hp); }
    }

    public class EnemySpawner : MonoBehaviour
    {
        public void Spawn(EnemySpawnData spawn)
        {
            if (GameManager.Instance != null && GameManager.Instance.Stage != null) GameManager.Instance.Stage.SpawnEnemy(spawn, false);
        }
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class EnemyController : MonoBehaviour
    {
        public EnemyData Data { get; private set; }
        public int CurrentHp { get; private set; }
        protected Rigidbody2D body;
        protected Transform player;
        protected SpriteRenderer sprite;
        protected float nextAttack;
        protected bool dead;
        protected bool telegraphing;
        private Vector2 lastSafePosition;

        public virtual void Initialize(EnemyData data)
        {
            Data = data;
            CurrentHp = data.maxHp;
            var status = GetComponent<EnemyStatus>();
            if (status == null) status = gameObject.AddComponent<EnemyStatus>();
            status.Initialize(data);
            name = data.enemyName;
            body = GetComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.gravityScale = 2.5f;
            player = GameManager.Instance != null && GameManager.Instance.Player != null ? GameManager.Instance.Player.transform : null;
            sprite = GetComponentInChildren<SpriteRenderer>();
            lastSafePosition = transform.position;
        }

        protected virtual void Update()
        {
            if (dead || player == null || GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            UpdateSafePosition();
            float distance = Mathf.Abs(player.position.x - transform.position.x);
            if (distance < Data.detectRange)
            {
                float direction = Mathf.Sign(player.position.x - transform.position.x);
                MoveSafely(direction * Data.moveSpeed);
                if (sprite != null) sprite.flipX = direction < 0;
            }
            if (distance < 1.2f && Time.time >= nextAttack && !telegraphing) StartCoroutine(TelegraphedAttack(false));
            RecoverFromFall();
        }

        protected void MoveSafely(float horizontalSpeed, float lookAhead = 0.75f)
        {
            if (body == null) return;
            float direction = Mathf.Sign(horizontalSpeed);
            bool canMove = Mathf.Abs(horizontalSpeed) < 0.01f || HasTerrainBelow(
                (Vector2)transform.position + new Vector2(direction * lookAhead, 0.2f), 1.8f);
            body.velocity = new Vector2(canMove ? horizontalSpeed : 0f, body.velocity.y);
        }

        protected void UpdateSafePosition()
        {
            if (HasTerrainBelow((Vector2)transform.position + Vector2.up * 0.15f, 1.6f) && body.velocity.y <= 0.3f)
                lastSafePosition = new Vector2(Mathf.Clamp(transform.position.x, 1.5f, GameBalance.StageWidth - 1.5f), transform.position.y);
        }

        private bool HasTerrainBelow(Vector2 origin, float distance)
        {
            foreach (RaycastHit2D hit in Physics2D.RaycastAll(origin, Vector2.down, distance))
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                if (hit.collider.GetComponent<StageTerrainPiece>() != null) return true;
            }
            return false;
        }

        protected void RecoverFromFall()
        {
            if (transform.position.y >= -2f) return;
            transform.position = lastSafePosition;
            body.velocity = Vector2.zero;
        }

        protected IEnumerator TelegraphedAttack(bool presentation)
        {
            telegraphing = true;
            nextAttack = Time.time + 1.25f;
            var warning = new GameObject("Attack Warning");
            warning.transform.SetParent(transform, false);
            warning.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            warning.transform.localScale = Vector3.one * 0.32f;
            var sr = warning.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeArt.GetProjectile(new Color32(255, 77, 65, 255));
            sr.sortingOrder = 15;
            yield return new WaitForSeconds(0.28f);
            if (!dead && player != null) AttackPlayer(presentation);
            Destroy(warning);
            telegraphing = false;
        }

        protected void AttackPlayer(bool presentation)
        {
            nextAttack = Mathf.Max(nextAttack, Time.time + 1.1f);
            Vector2 direction = player != null ? ((Vector2)player.position - (Vector2)transform.position).normalized : Vector2.right;
            Vector2 center = (Vector2)transform.position + direction * 0.72f;
            foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, new Vector2(1.35f, 1.1f), 0f))
            {
                Hurtbox hurtbox = hit.GetComponent<Hurtbox>();
                if (hurtbox != null && hurtbox.team == CombatTeam.Player) hurtbox.DamagePlayer(Data.attackPower, transform.position, presentation);
            }
        }

        public virtual void TakeDamage(int damage, int knockbackDirection)
        {
            if (dead) return;
            CurrentHp -= Mathf.Max(0, damage - Data.defense);
            var status = GetComponent<EnemyStatus>();
            if (status != null) status.SetCurrentHp(CurrentHp);
            if (body != null) body.velocity = new Vector2(knockbackDirection * 4f, 3f);
            CombatFx.Burst(transform.position + Vector3.up * 0.25f, new Color32(255, 225, 98, 255), 5);
            if (sprite != null) StartCoroutine(HitFlash());
            if (CurrentHp <= 0) Die();
        }

        private IEnumerator HitFlash()
        {
            sprite.color = new Color32(255, 119, 119, 255);
            yield return new WaitForSeconds(0.07f);
            sprite.color = Color.white;
        }

        protected virtual void Die()
        {
            if (dead) return;
            dead = true;
            if (GameManager.Instance != null && GameManager.Instance.Stage != null)
            {
                GameManager.Instance.Stage.RegisterKill(Data);
                if (Random.value < Data.itemDropChance) GameManager.Instance.Player.GetComponent<PlayerInventory>().AddItem("energy_jelly");
            }
            CombatFx.Burst(transform.position, RuntimeArt.EnemyColor(Data.enemyType), 12);
            CameraFollow.Shake(0.12f, 0.14f);
            Destroy(gameObject);
        }
    }

    public class AssignmentEnemy : EnemyController
    {
        private float chargeAt;
        public override void Initialize(EnemyData data) { base.Initialize(data); chargeAt = Time.time + 2f; }
        protected override void Update()
        {
            base.Update();
            if (dead || player == null || Time.time < chargeAt) return;
            chargeAt = Time.time + Random.Range(2.2f, 3.6f);
            MoveSafely(Mathf.Sign(player.position.x - transform.position.x) * 9f, 1.25f);
        }
    }

    public class RangedEnemy : EnemyController
    {
        private float shootAt;
        public override void Initialize(EnemyData data) { base.Initialize(data); shootAt = Time.time + 1f; }
        protected override void Update()
        {
            if (dead || player == null || GameManager.Instance.State != GameState.Playing) return;
            UpdateSafePosition();
            float distance = Mathf.Abs(player.position.x - transform.position.x);
            MoveSafely(distance < 4f ? -Mathf.Sign(player.position.x - transform.position.x) * Data.moveSpeed : 0);
            if (distance < Data.detectRange && Time.time >= shootAt)
            {
                shootAt = Time.time + 1.8f;
                Projectile.Spawn(transform.position + Vector3.up * 0.2f, (player.position - transform.position).normalized, Data.attackPower, true, new Color(1f, 0.35f, 0.35f));
            }
            RecoverFromFall();
        }
    }

    public class DebuffEnemy : EnemyController
    {
        protected override void Update()
        {
            base.Update();
            if (!dead && player != null && Vector2.Distance(player.position, transform.position) < 2.2f && Time.time >= nextAttack)
            {
                AttackPlayer(false);
                if (GameManager.Instance != null) GameManager.Instance.UI.Toast("딴생각 때문에 공격 효율이 흔들립니다!");
            }
        }
    }

    public class Projectile : MonoBehaviour
    {
        private Vector2 direction;
        private int damage;
        private bool presentation;
        private float expire;

        public static Projectile Spawn(Vector2 position, Vector2 direction, int damage, bool presentation, Color color)
        {
            var go = new GameObject("StressProjectile");
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.42f, 0.42f, 1);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeArt.GetProjectile(color);
            sr.sortingOrder = 7;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            var projectile = go.AddComponent<Projectile>();
            projectile.direction = direction.normalized;
            projectile.damage = damage;
            projectile.presentation = presentation;
            projectile.expire = Time.time + 5f;
            return projectile;
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * 7f * Time.deltaTime);
            transform.Rotate(0f, 0f, 240f * Time.deltaTime);
            if (Time.time >= expire) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var hurtbox = other.GetComponent<Hurtbox>();
            if (hurtbox == null || hurtbox.team != CombatTeam.Player) return;
            hurtbox.DamagePlayer(damage, transform.position, presentation);
            Destroy(gameObject);
        }
    }
}
