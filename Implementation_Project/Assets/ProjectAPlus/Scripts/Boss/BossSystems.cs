using System.Collections;
using UnityEngine;

namespace ProjectAPlus
{
    public class BossController : MonoBehaviour
    {
        public BossData Data { get; private set; }
        public int CurrentHp { get; private set; }
        public int Phase { get; private set; } = 1;
        private bool dead;
        private BossPatternManager patterns;
        private SpriteRenderer sprite;

        public void Initialize(BossData data, int stage)
        {
            Data = data;
            CurrentHp = data.maxHp;
            name = data.bossName;
            sprite = GetComponentInChildren<SpriteRenderer>();
            patterns = gameObject.AddComponent<BossPatternManager>();
            patterns.Initialize(this, stage);
        }

        public void TakeDamage(int damage)
        {
            if (dead) return;
            CurrentHp -= Mathf.Max(0, damage);
            float ratio = CurrentHp / (float)Data.maxHp;
            Phase = ratio <= 0.2f ? 4 : ratio <= 0.4f ? 3 : ratio <= 0.7f ? 2 : 1;
            if (sprite != null) StartCoroutine(HitFlash());
            CombatFx.Burst(transform.position + Vector3.up * 0.4f, new Color32(255, 210, 76, 255), 8);
            CameraFollow.Shake(0.13f, 0.14f);
            if (CurrentHp <= 0) Die();
        }

        private IEnumerator HitFlash()
        {
            sprite.color = new Color32(255, 105, 105, 255);
            yield return new WaitForSeconds(0.07f);
            sprite.color = Color.white;
        }

        private void Die()
        {
            if (dead) return;
            dead = true;
            if (GameManager.Instance != null) GameManager.Instance.Stage.RegisterBossKill();
            CombatFx.Burst(transform.position, new Color32(255, 197, 69, 255), 30);
            CameraFollow.Shake(0.35f, 0.55f);
            Destroy(gameObject, 0.1f);
        }
    }

    public class BossPatternManager : MonoBehaviour
    {
        private BossController boss;
        private Transform player;
        private int stage;
        private float nextPattern;

        public void Initialize(BossController controller, int stageNumber)
        {
            boss = controller;
            stage = stageNumber;
            player = GameManager.Instance.Player.transform;
            nextPattern = Time.time + 1.5f;
        }

        private void Update()
        {
            if (boss == null || player == null || GameManager.Instance.State != GameState.Playing || Time.time < nextPattern) return;
            nextPattern = Time.time + Mathf.Max(1.0f, 2.5f - boss.Phase * 0.28f);
            int patternCount = stage == 10 ? 5 : 4;
            int selection = Random.Range(0, patternCount);
            if (selection == 0) StartCoroutine(FanProjectiles());
            else if (selection == 1) StartCoroutine(GroundWarnings());
            else if (selection == 2) StartCoroutine(FallingQuiz());
            else if (selection == 3) StartCoroutine(SidePressure());
            else StartCoroutine(SummonAnxiety());
        }

        private IEnumerator FanProjectiles()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.Play("bossWarning");
            int count = 4 + boss.Phase * 2;
            for (int wave = 0; wave < (boss.Phase >= 3 ? 2 : 1); wave++)
            {
                for (int i = 0; i < count; i++)
                {
                    float angle = Mathf.Lerp(200f, 340f, i / Mathf.Max(1f, count - 1f)) * Mathf.Deg2Rad;
                    Projectile.Spawn(transform.position, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), boss.Data.attackPower, false, new Color(1f, 0.9f, 0.25f));
                }
                yield return new WaitForSeconds(0.35f);
            }
        }

        private IEnumerator GroundWarnings()
        {
            for (int i = 0; i < 2 + boss.Phase; i++)
            {
                WarningArea.Spawn(new Vector2(Random.Range(4f, GameBalance.StageWidth - 4f), 0.25f), new Vector2(3.2f, 0.35f), boss.Data.attackPower + 3, 0.9f);
                yield return new WaitForSeconds(0.18f);
            }
        }

        private IEnumerator FallingQuiz()
        {
            for (int i = 0; i < 2 + boss.Phase; i++)
            {
                Vector2 target = new Vector2(player.position.x + Random.Range(-1.5f, 1.5f), 0.5f);
                WarningArea.Spawn(target, new Vector2(1.2f, 6f), boss.Data.attackPower, 0.55f);
                yield return new WaitForSeconds(0.28f);
            }
        }

        private IEnumerator SidePressure()
        {
            Vector2 from = new Vector2(Random.value > 0.5f ? 1f : GameBalance.StageWidth - 1f, Random.Range(1.2f, 4.5f));
            Vector2 direction = ((Vector2)player.position - from).normalized;
            for (int i = 0; i < 3 + boss.Phase; i++)
            {
                Projectile.Spawn(from, direction + Random.insideUnitCircle * 0.08f, boss.Data.attackPower, false, new Color(0.95f, 0.38f, 0.32f));
                yield return new WaitForSeconds(0.22f);
            }
        }

        private IEnumerator SummonAnxiety()
        {
            if (stage != 10) yield break;
            for (int i = 0; i < Mathf.Min(3, boss.Phase); i++)
            {
                GameManager.Instance.Stage.SpawnEnemy(new EnemySpawnData(EnemyType.AnxietyShadow, transform.position.x + Random.Range(-3f, 3f), 1.2f), true);
                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    public class WarningArea : MonoBehaviour
    {
        private int damage;
        private float triggerAt;
        private float destroyAt;
        private SpriteRenderer sprite;

        public static WarningArea Spawn(Vector2 position, Vector2 size, int damage, float delay)
        {
            var go = new GameObject("WarningArea");
            go.transform.position = position;
            go.transform.localScale = size;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeArt.Solid("warningArea", new Color(1f, 0.16f, 0.12f, 0.42f));
            sr.sortingOrder = 6;
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            var warning = go.AddComponent<WarningArea>();
            warning.damage = damage;
            warning.triggerAt = Time.time + delay;
            warning.destroyAt = warning.triggerAt + 0.22f;
            warning.sprite = sr;
            if (AudioManager.Instance != null) AudioManager.Instance.Play("bossWarning");
            return warning;
        }

        private void Update()
        {
            if (Time.time < triggerAt)
            {
                sprite.color = new Color(1f, 0.16f, 0.12f, 0.25f + Mathf.PingPong(Time.time * 4f, 0.35f));
                return;
            }
            sprite.color = new Color(1f, 0.92f, 0.18f, 0.85f);
            if (Time.time >= destroyAt) Destroy(gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time < triggerAt) return;
            var player = other.GetComponent<PlayerHitHandler>();
            if (player != null) player.TakeDamage(damage, transform.position);
        }
    }

    public class BossProjectile : Projectile { }
}
