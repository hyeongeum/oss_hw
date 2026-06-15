using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAPlus
{
    [DefaultExecutionOrder(10000)]
    public class PixelPerfectCamera : MonoBehaviour
    {
        public const float PixelsPerUnit = 80f;
        private Camera targetCamera;
        private Material pixelMaterial;
        private float nextRendererRefresh;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            QualitySettings.antiAliasing = 0;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            if (targetCamera != null)
            {
                targetCamera.allowMSAA = false;
                targetCamera.allowHDR = false;
            }
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                pixelMaterial = new Material(shader);
                pixelMaterial.name = "Project A+ Pixel Snap Material";
                pixelMaterial.EnableKeyword("PIXELSNAP_ON");
                if (pixelMaterial.HasProperty("PixelSnap")) pixelMaterial.SetFloat("PixelSnap", 1f);
            }
        }

        private void LateUpdate()
        {
            float grid = 1f / PixelsPerUnit;
            Vector3 position = transform.position;
            transform.position = new Vector3(Mathf.Round(position.x / grid) * grid, Mathf.Round(position.y / grid) * grid, position.z);
            if (Time.unscaledTime < nextRendererRefresh) return;
            nextRendererRefresh = Time.unscaledTime + 0.75f;
            if (pixelMaterial == null) return;
            foreach (SpriteRenderer renderer in FindObjectsOfType<SpriteRenderer>())
                if (renderer.sharedMaterial != pixelMaterial) renderer.sharedMaterial = pixelMaterial;
        }
    }

    public class PixelCanvasScale : MonoBehaviour
    {
        private CanvasScaler scaler;
        private int lastWidth;
        private int lastHeight;

        private void Awake()
        {
            scaler = GetComponent<CanvasScaler>();
            Refresh();
        }

        private void Update()
        {
            if (lastWidth != Screen.width || lastHeight != Screen.height) Refresh();
        }

        private void Refresh()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            if (scaler == null) return;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            float widthScale = Screen.width / 1920f;
            float heightScale = Screen.height / 1080f;
            scaler.scaleFactor = Mathf.Max(1f, Mathf.Floor(Mathf.Min(widthScale, heightScale)));
            scaler.referencePixelsPerUnit = 16f;
        }
    }

    public static class StableVisual
    {
        public const float CharacterPixelsPerUnit = 80f;

        public static SpriteRenderer AttachSprite(GameObject owner, Sprite sprite, float uniformScale, int sortingOrder, string visualName, Vector2 offset)
        {
            var visual = new GameObject(visualName);
            visual.transform.SetParent(owner.transform, false);
            visual.transform.localPosition = offset;
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            visual.AddComponent<StableSpriteVisual>().Configure(uniformScale);
            return renderer;
        }

        public static SpriteRenderer FitSpriteInBox(GameObject owner, Sprite sprite, Vector2 box, int sortingOrder, string visualName)
        {
            float scale = 1f;
            if (sprite != null && sprite.bounds.size.x > 0.001f && sprite.bounds.size.y > 0.001f)
                scale = Mathf.Min(box.x / sprite.bounds.size.x, box.y / sprite.bounds.size.y);
            return AttachSprite(owner, sprite, Mathf.Max(0.01f, scale), sortingOrder, visualName, Vector2.zero);
        }

        public static bool IsUniform(Vector3 scale)
        {
            return Mathf.Abs(Mathf.Abs(scale.x) - Mathf.Abs(scale.y)) < 0.001f;
        }
    }

    public class StableSpriteVisual : MonoBehaviour
    {
        private Vector3 stableScale = Vector3.one;
        private bool configured;
        public float UniformScale { get { return Mathf.Abs(stableScale.x); } }
        public bool IsStable { get { return configured && StableVisual.IsUniform(transform.localScale) && Vector3.Distance(transform.localScale, stableScale) < 0.001f; } }

        public void Configure(float uniformScale)
        {
            float scale = Mathf.Max(0.125f, Mathf.Round(Mathf.Abs(uniformScale) * 8f) / 8f);
            stableScale = new Vector3(scale, scale, 1f);
            transform.localScale = stableScale;
            configured = true;
        }

        private void LateUpdate()
        {
            if (configured && transform.localScale != stableScale) transform.localScale = stableScale;
        }
    }

    public enum CombatTeam { Player, Enemy }

    public static class CombatLayers
    {
        public const int PlayerBody = 8;
        public const int EnemyBody = 9;
        public const int PlayerGate = 10;
        public const int CombatHitbox = 11;
        private static bool configured;

        public static void Configure()
        {
            if (configured) return;
            configured = true;
            Physics2D.IgnoreLayerCollision(PlayerBody, EnemyBody, true);
            Physics2D.IgnoreLayerCollision(EnemyBody, EnemyBody, true);
            Physics2D.IgnoreLayerCollision(EnemyBody, PlayerGate, true);
        }
    }

    public class Hurtbox : MonoBehaviour
    {
        public CombatTeam team;
        private EnemyController enemy;
        private BossController boss;
        private PlayerHitHandler player;

        public void Initialize(CombatTeam combatTeam, Component owner)
        {
            team = combatTeam;
            enemy = owner as EnemyController;
            boss = owner as BossController;
            player = owner as PlayerHitHandler;
        }

        public void DamageEnemy(int damage, int direction)
        {
            if (team != CombatTeam.Enemy) return;
            if (enemy != null) enemy.TakeDamage(damage, direction);
            else if (boss != null) boss.TakeDamage(damage);
        }

        public void DamagePlayer(int damage, Vector2 source, bool presentation)
        {
            if (team == CombatTeam.Player && player != null) player.TakeDamage(damage, source, presentation);
        }
    }

    public static class CombatGeometry
    {
        public static Hurtbox AttachHurtbox(GameObject owner, CombatTeam team, Vector2 size, Vector2 offset, Component receiver)
        {
            var go = new GameObject(team + " Hurtbox");
            go.transform.SetParent(owner.transform, false);
            go.transform.localPosition = offset;
            go.layer = CombatLayers.CombatHitbox;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size;
            var hurtbox = go.AddComponent<Hurtbox>();
            hurtbox.Initialize(team, receiver);
            return hurtbox;
        }

        public static void AttachContactDamage(Hurtbox source, int damage, float cooldown = 0.8f)
        {
            if (source == null) return;
            Transform owner = source.transform.parent;
            if (owner == null) return;
            var contact = owner.gameObject.AddComponent<EnemyContactDamage>();
            contact.Initialize(damage, cooldown, owner);
        }
    }

    public class EnemyContactDamage : MonoBehaviour
    {
        private int damage;
        private float cooldown;
        private float nextDamageAt;
        private Transform source;

        public void Initialize(int amount, float interval, Transform damageSource)
        {
            damage = Mathf.Max(1, amount);
            cooldown = Mathf.Max(0.1f, interval);
            source = damageSource;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time < nextDamageAt || GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            Hurtbox hurtbox = other.GetComponent<Hurtbox>();
            if (hurtbox == null || hurtbox.team != CombatTeam.Player) return;
            nextDamageAt = Time.time + cooldown;
            hurtbox.DamagePlayer(damage, source != null ? (Vector2)source.position : (Vector2)transform.position, false);
        }
    }

    public class PixelBob : MonoBehaviour
    {
        public float height = 0.055f;
        public float speed = 3f;
        public float squash = 0.035f;
        private Vector3 baseScale;
        private float offset;

        private void Awake()
        {
            baseScale = transform.localScale;
            offset = Random.Range(0f, 6f);
        }

        private void LateUpdate()
        {
            // Fractional squash makes point-filtered pixel art shimmer and tear.
            if (transform.localScale != baseScale) transform.localScale = baseScale;
        }
    }

    public class PlayerVisualAnimator : MonoBehaviour
    {
        private struct FrameProfile
        {
            public int height;
            public int bottomPixel;
            public FrameProfile(int visibleHeight, int visibleBottomPixel)
            {
                height = visibleHeight;
                bottomPixel = visibleBottomPixel;
            }
        }

        private static readonly Dictionary<string, FrameProfile> Profiles = new Dictionary<string, FrameProfile>
        {
            { "idle_0", new FrameProfile(100, 122) }, { "idle_1", new FrameProfile(100, 122) },
            { "run_0", new FrameProfile(100, 122) }, { "run_1", new FrameProfile(100, 122) },
            { "run_2", new FrameProfile(100, 122) }, { "run_3", new FrameProfile(100, 122) },
            { "jump", new FrameProfile(100, 122) }, { "fall", new FrameProfile(100, 122) },
            { "attack_0", new FrameProfile(100, 122) }, { "attack_1", new FrameProfile(100, 122) },
            { "attack_2", new FrameProfile(100, 122) }, { "dodge_0", new FrameProfile(82, 122) },
            { "dodge_1", new FrameProfile(82, 122) }, { "damaged", new FrameProfile(100, 122) },
            { "dead", new FrameProfile(60, 122) }
        };

        private SpriteRenderer sprite;
        private Rigidbody2D body;
        private PlayerController controller;
        private PlayerCombat combat;
        private PlayerHitHandler hit;
        private PlayerStatus status;
        private Sprite[] idle;
        private Sprite[] run;
        private Sprite[] attack;
        private Sprite[] dodge;
        private Sprite jump;
        private Sprite fall;
        private Sprite damaged;
        private Sprite dead;
        private const float PixelsPerUnit = StableVisual.CharacterPixelsPerUnit;
        private const float VisualBottom = -0.68f;
        private float currentVisualHeight;
        private float currentVisualScale = 1f;
        public float CurrentVisualHeight { get { return currentVisualHeight; } }
        public float CurrentVisualScale { get { return currentVisualScale; } }

        private void Awake()
        {
            sprite = GetComponentInChildren<SpriteRenderer>();
            body = GetComponent<Rigidbody2D>();
            controller = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            hit = GetComponent<PlayerHitHandler>();
            status = GetComponent<PlayerStatus>();
            idle = LoadFrames("idle_", 2);
            run = LoadFrames("run_", 4);
            attack = LoadFrames("attack_", 3);
            dodge = LoadFrames("dodge_", 2);
            jump = RuntimeArt.GetPlayerFrame("jump");
            fall = RuntimeArt.GetPlayerFrame("fall");
            damaged = RuntimeArt.GetPlayerFrame("damaged");
            dead = RuntimeArt.GetPlayerFrame("dead");
            StableSpriteVisual stable = sprite != null ? sprite.GetComponent<StableSpriteVisual>() : null;
            if (stable == null && sprite != null) stable = sprite.gameObject.AddComponent<StableSpriteVisual>();
            if (stable != null) stable.Configure(1f);
        }

        private void LateUpdate()
        {
            if (sprite == null || controller == null || body == null) return;
            Sprite next;
            if (status != null && status.IsDead) next = dead;
            else if (hit != null && Time.time < hit.HitVisualUntil) next = damaged;
            else if (combat != null && Time.time < combat.VisualAttackUntil) next = Frame(attack, combat.VisualAttackStep - 1);
            else if (controller.IsDodging) next = Frame(dodge, Mathf.FloorToInt(Time.time * 18f));
            else if (!controller.IsGrounded) next = body.velocity.y >= 0f ? jump : fall;
            else if (Mathf.Abs(body.velocity.x) > 0.2f) next = Frame(run, Mathf.FloorToInt(Time.time * 10f));
            else next = Frame(idle, Mathf.FloorToInt(Time.time * 2.5f));
            if (next != null) ApplyFrame(next);
        }

        private void ApplyFrame(Sprite next)
        {
            sprite.sprite = next;
            FrameProfile profile;
            if (!Profiles.TryGetValue(next.name, out profile)) profile = new FrameProfile(100, 116);
            const float scale = 1f;
            currentVisualHeight = profile.height / PixelsPerUnit;
            currentVisualScale = scale;
            float sourceBottom = (64f - profile.bottomPixel) / PixelsPerUnit;
            float motionX = 0f;
            if (combat != null && Time.time < combat.VisualAttackUntil) motionX = controller.Facing * (combat.VisualAttackStep == 3 ? 0.13f : 0.07f);
            else if (controller.IsDodging) motionX = controller.Facing * 0.08f;
            float grid = 1f / PixelsPerUnit;
            motionX = Mathf.Round(motionX / grid) * grid;
            float anchoredY = Mathf.Round((VisualBottom - sourceBottom) / grid) * grid;
            sprite.transform.localScale = Vector3.one;
            sprite.transform.localPosition = new Vector3(motionX, anchoredY, 0f);
        }

        private static Sprite[] LoadFrames(string prefix, int count)
        {
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++) frames[i] = RuntimeArt.GetPlayerFrame(prefix + i);
            return frames;
        }

        private static Sprite Frame(Sprite[] frames, int index)
        {
            return frames == null || frames.Length == 0 ? null : frames[Mathf.Abs(index) % frames.Length];
        }
    }

    public class WorldHealthBar : MonoBehaviour
    {
        private Transform fill;
        private SpriteRenderer back;
        private SpriteRenderer fillSprite;
        private EnemyController enemy;
        private BossController boss;
        private int maxHp;
        private int lastHp;
        private float visibleUntil;

        public static void Attach(GameObject target, int maxHp, bool boss)
        {
            var root = new GameObject("HealthBar");
            root.transform.SetParent(target.transform, false);
            Vector3 parentScale = target.transform.lossyScale;
            root.transform.localPosition = new Vector3(0, (boss ? 1.15f : 0.88f) / Mathf.Max(0.01f, Mathf.Abs(parentScale.y)), 0);
            root.transform.localScale = new Vector3((boss ? 1.15f : 0.72f) / Mathf.Max(0.01f, Mathf.Abs(parentScale.x)), (boss ? 0.09f : 0.065f) / Mathf.Max(0.01f, Mathf.Abs(parentScale.y)), 1);
            var back = root.AddComponent<SpriteRenderer>();
            back.sprite = RuntimeArt.Solid("health_back", new Color32(29, 31, 42, 245));
            back.sortingOrder = 18;
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(root.transform, false);
            fillGo.transform.localPosition = new Vector3(-0.48f, 0, -0.1f);
            fillGo.transform.localScale = new Vector3(0.94f, 0.62f, 1);
            var fill = fillGo.AddComponent<SpriteRenderer>();
            fill.sprite = RuntimeArt.Solid("health_fill", boss ? new Color32(233, 68, 79, 255) : new Color32(85, 221, 126, 255));
            fill.sortingOrder = 19;
            var bar = root.AddComponent<WorldHealthBar>();
            bar.fill = fillGo.transform;
            bar.back = back;
            bar.fillSprite = fill;
            bar.maxHp = Mathf.Max(1, maxHp);
            bar.lastHp = bar.maxHp;
            bar.enemy = target.GetComponent<EnemyController>();
            bar.boss = target.GetComponent<BossController>();
            bar.SetVisible(false);
        }

        private void LateUpdate()
        {
            int current = enemy != null ? enemy.CurrentHp : boss != null ? boss.CurrentHp : 0;
            if (current < lastHp) visibleUntil = Time.time + (boss != null ? 4f : 2.2f);
            lastHp = current;
            SetVisible(current > 0 && (boss != null || current < maxHp) && Time.time < visibleUntil);
            float ratio = Mathf.Clamp01(current / (float)maxHp);
            fill.localScale = new Vector3(0.94f * ratio, fill.localScale.y, 1);
            fill.localPosition = new Vector3(-0.48f + 0.47f * ratio, 0, -0.1f);
        }

        private void SetVisible(bool visible)
        {
            if (back != null) back.enabled = visible;
            if (fillSprite != null) fillSprite.enabled = visible;
        }
    }

    public static class CombatFx
    {
        public static void Burst(Vector2 position, Color color, int count = 7)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("PixelBurst");
                go.transform.position = position;
                go.transform.localScale = Vector3.one * Random.Range(0.08f, 0.2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = RuntimeArt.Solid("pixel_burst", color);
                sr.sortingOrder = 20;
                var particle = go.AddComponent<PixelBurst>();
                particle.velocity = Random.insideUnitCircle.normalized * Random.Range(2f, 6f) + Vector2.up * 2f;
            }
        }

        public static void Afterimage(SpriteRenderer source)
        {
            if (source == null || source.sprite == null) return;
            var go = new GameObject("Afterimage");
            go.transform.position = source.transform.position;
            go.transform.localScale = source.transform.lossyScale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = source.sprite;
            sr.flipX = source.flipX;
            sr.color = new Color(0.28f, 0.85f, 1f, 0.42f);
            sr.sortingOrder = source.sortingOrder - 1;
            go.AddComponent<FadeAndDestroy>().duration = 0.22f;
        }

        public static void HitSpark(Vector2 position, bool critical, int direction)
        {
            Burst(position, critical ? new Color32(255, 207, 73, 255) : new Color32(255, 244, 202, 255), critical ? 10 : 6);
            var go = new GameObject(critical ? "Critical Hit Spark" : "Hit Spark");
            go.transform.position = position;
            float scale = critical ? 1.12f : 0.78f;
            go.transform.localScale = new Vector3(direction * scale, scale, 1f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-18f, 18f));
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeArt.GetHitSpark(critical);
            sr.sortingOrder = 24;
            sr.color = critical ? new Color32(255, 183, 62, 255) : Color.white;
            go.AddComponent<FadeAndDestroy>().duration = critical ? 0.2f : 0.13f;
        }

        public static void HitStop(float seconds)
        {
            if (GameManager.Instance != null) GameManager.Instance.StartCoroutine(HitStopRoutine(seconds));
        }

        private static IEnumerator HitStopRoutine(float seconds)
        {
            float old = Time.timeScale;
            Time.timeScale = 0.04f;
            yield return new WaitForSecondsRealtime(seconds);
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Playing) Time.timeScale = old;
        }
    }

    public class PixelBurst : MonoBehaviour
    {
        public Vector2 velocity;
        private float life = 0.38f;
        private SpriteRenderer sprite;
        private void Awake() { sprite = GetComponent<SpriteRenderer>(); }
        private void Update()
        {
            life -= Time.unscaledDeltaTime;
            velocity += Vector2.down * 12f * Time.unscaledDeltaTime;
            transform.position += (Vector3)(velocity * Time.unscaledDeltaTime);
            if (sprite != null) sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, Mathf.Clamp01(life * 3f));
            if (life <= 0) Destroy(gameObject);
        }
    }

    public class FadeAndDestroy : MonoBehaviour
    {
        public float duration = 0.25f;
        private float left;
        private SpriteRenderer sprite;
        private void Awake() { left = duration; sprite = GetComponent<SpriteRenderer>(); }
        private void Update()
        {
            left -= Time.unscaledDeltaTime;
            if (sprite != null) sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, Mathf.Clamp01(left / duration));
            if (left <= 0) Destroy(gameObject);
        }
    }
}
