using System;
using System.Collections;
using System.Collections.Generic;
using FlowIoC.BaseModule.Injectable.Components;
using FlowIoC.ScreenModule.ViewsMediators.Screen;
using Modules.GameplayModule.GameplayScreenModule.Entities;
using Modules.GameplayModule.Shared.Data.ValueObjects;
using Modules.GameplayModule.Shared.Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modules.GameplayModule.GameplayScreenModule.ViewsMediators
{
    /// <summary>
    /// The battle as a little side-on stage: the party on the left, the creature on the right, a
    /// dungeon scrolling past while they walk to the next one. It draws what it is handed and
    /// animates it - who swings, what flies, where the numbers pop - and decides nothing. Dragging
    /// anywhere on it is reported as a drag of the window.
    /// </summary>
    [RequireComponent(typeof(ViewInjector))]
    public class GameplayScreenView : ScreenView, IBeginDragHandler, IDragHandler
    {
        [Serializable]
        public class PartySlot
        {
            public PartyRole Role;
            public SpriteActor Actor;
            public RectTransform HpBar;
            public Image HpFill;
        }

        [Serializable]
        public class EnemyBody
        {
            public EnemyLook Look;
            public Sprite[] Idle;
            public Sprite[] Attack;
            public Sprite Hurt;
        }

        [Serializable]
        public class UpgradeCard
        {
            public Button Button;
            public Image Icon;
            public Image RoleIcon;
            public Text Title;
            public Text Description;
        }

        private const float FLOOR_Y = 58f;
        private const float FRONT_X = 290f;
        private const float MEMBER_SPACING = 58f;
        private const float ENEMY_HOME_X = 470f;
        private const float ENEMY_ENTER_X = 720f;
        private const float BODY_SIZE = 80f;
        private const float WALK_SECONDS = 1.2f;
        private const float DEATH_DELAY = 0.3f;
        private const float DEATH_FADE = 0.3f;
        private const float PROJECTILE_SECONDS = 0.22f;
        private const float BAR_EASE_SPEED = 10f;
        private const float POPUP_SECONDS = 0.75f;
        private const float POPUP_RISE = 34f;
        private const float ANNOUNCEMENT_SECONDS = 2.4f;

        [Header("Stage")]
        [SerializeField] private RawImage _sky;
        [SerializeField] private RawImage _ruins;
        [SerializeField] private RawImage _ground;
        [SerializeField] private RawImage _lava;
        [SerializeField] private float _skyDrift = 0.004f;
        [SerializeField] private float _skyWalkSpeed = 0.03f;
        [SerializeField] private float _ruinsWalkSpeed = 0.12f;
        [SerializeField] private float _groundWalkSpeed = 1.6f;
        [SerializeField] private float _lavaFlow = 0.4f;

        [Header("Party")]
        [SerializeField] private PartySlot[] _party;

        [Header("Enemy")]
        [SerializeField] private SpriteActor _enemy;
        [SerializeField] private RectTransform _enemyHud;
        [SerializeField] private Text _enemyNameText;
        [SerializeField] private Image _enemyHpFill;
        [SerializeField] private EnemyBody[] _enemyBodies;

        [Header("Effects")]
        [SerializeField] private RectTransform _effects;
        [SerializeField] private Image _arrow;
        [SerializeField] private Image _fireball;
        [SerializeField] private Sprite[] _fireballFrames;
        [SerializeField] private Image _slash;
        [SerializeField] private Sprite[] _slashFrames;
        [SerializeField] private Image _sparkTemplate;
        [SerializeField] private Sprite[] _sparkFrames;
        [SerializeField] private Image _healTemplate;
        [SerializeField] private Sprite[] _healFrames;
        [SerializeField] private Color _healColor = new(0.45f, 1f, 0.5f);
        [SerializeField] private Color _eliteTint = new(1f, 0.62f, 0.62f);
        [SerializeField] private Color _eliteNameColor = new(1f, 0.6f, 0.3f);
        [SerializeField] private Color _bossNameColor = new(1f, 0.3f, 0.3f);
        [SerializeField] private Text _popupTemplate;
        [SerializeField] private Color _criticalColor = new(1f, 0.82f, 0.25f);
        [SerializeField] private Color _enemyDamageColor = Color.white;
        [SerializeField] private Color _heroDamageColor = new(1f, 0.35f, 0.35f);

        [Header("HUD")]
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _rankText;
        [SerializeField] private Text _rankTitleText;
        [SerializeField] private Image _expFill;
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _killsText;
        [SerializeField] private Text _logText;
        [SerializeField] private Text _floorText;
        [SerializeField] private CanvasGroup _announcementGroup;
        [SerializeField] private Text _announcementText;

        [Header("Level-up choice")]
        [SerializeField] private GameObject _offerPanel;
        [SerializeField] private Image _offerTimerFill;
        [SerializeField] private UpgradeCard[] _cards;

        /// <summary>One per UpgradeKind, in the enum's order.</summary>
        [SerializeField] private Sprite[] _kindIcons;

        /// <summary>One per PartyRole, in the enum's order.</summary>
        [SerializeField] private Sprite[] _roleIcons;

        public Action DragBegan;
        public Action Dragged;

        /// <summary>A card was clicked: the offer's id, and the card's index.</summary>
        public Action<int, int> UpgradeChosen;

        private int _offerId;
        private float _offerSeconds;
        private float _offerLeft;
        private bool _isOfferUp;

        private readonly Stack<Text> _popupPool = new();
        private readonly Stack<Image> _sparkPool = new();
        private readonly Stack<Image> _healPool = new();
        private readonly HashSet<PartyRole> _shownRoles = new();
        private readonly Dictionary<PartyRole, PartySlot> _slots = new();
        private readonly Dictionary<PartyRole, float> _memberHpTargets = new();

        private int _enemySpawnId = -1;
        private float _enemyScale = 1f;
        private float _enemyHpTarget = 1f;
        private float _expTarget;
        private bool _isWalking;
        private Coroutine _transition;
        private Coroutine _announcement;

        public void OnBeginDrag(PointerEventData eventData) => DragBegan?.Invoke();
        public void OnDrag(PointerEventData eventData) => Dragged?.Invoke();

        public void ShowHero(HeroStateVO hero)
        {
            _levelText.text = $"Lv {hero.Level}";
            _rankText.text = hero.Rank.ToString();
            _rankTitleText.text = hero.RankTitle;
            _expTarget = Ratio(hero.Exp, hero.ExpToNextLevel);
            _goldText.text = $"{hero.Gold:N0} G";
            _killsText.text = $"KILLS {hero.Kills:N0}";
            _floorText.text = hero.FloorKills >= hero.FloorKillsToBoss
                ? $"FLOOR {hero.Floor}  ·  BOSS"
                : $"FLOOR {hero.Floor}  ·  {hero.FloorKills}/{hero.FloorKillsToBoss}";

            // Only the members who have joined stand on the stage, front to back from the right. A figure
            // already standing is left on, so its animation runs on through the update.
            _shownRoles.Clear();
            foreach (PartyMemberStateVO member in hero.Members)
                _shownRoles.Add(member.Role);

            foreach (PartySlot slot in _party)
            {
                if (!_shownRoles.Contains(slot.Role))
                    slot.Actor.gameObject.SetActive(false);
            }

            for (int i = 0; i < hero.Members.Count; i++)
            {
                PartyMemberStateVO member = hero.Members[i];
                if (!Slots.TryGetValue(member.Role, out PartySlot slot))
                    continue;

                float x = FRONT_X - i * MEMBER_SPACING;
                ((RectTransform) slot.Actor.transform).anchoredPosition = new Vector2(x, FLOOR_Y);
                slot.HpBar.anchoredPosition = new Vector2(x, slot.HpBar.anchoredPosition.y);
                if (!slot.Actor.gameObject.activeSelf)
                    slot.Actor.gameObject.SetActive(true);

                bool isDown = member.Hp <= 0;
                if (slot.Actor.IsDown != isDown)
                    slot.Actor.SetDown(isDown);

                slot.HpBar.gameObject.SetActive(!isDown);
                _memberHpTargets[member.Role] = Ratio(member.Hp, member.MaxHp);
            }

            foreach (PartySlot slot in _party)
            {
                if (!slot.Actor.gameObject.activeSelf)
                    slot.HpBar.gameObject.SetActive(false);
            }
        }

        public void ShowHeal(PartyRole healer, PartyRole target, int amount)
        {
            if (!Slots.TryGetValue(healer, out PartySlot healerSlot) || !Slots.TryGetValue(target, out PartySlot targetSlot))
                return;

            healerSlot.Actor.PlayAttack();
            Vector2 at = ((RectTransform) targetSlot.Actor.transform).anchoredPosition + new Vector2(0f, 40f);
            Image heal = _healPool.Count > 0 ? _healPool.Pop() : Instantiate(_healTemplate, _effects);
            StartCoroutine(PlayPooledFrames(heal, _healFrames, at, 0.09f, _healPool));
            SpawnPopup(at + new Vector2(0f, 22f), $"+{amount}", _healColor, 1f);
        }

        public void ShowUpgradeOffer(UpgradeOfferVO offer)
        {
            _offerId = offer.OfferId;
            _offerSeconds = Mathf.Max(0.1f, offer.AutoPickSeconds);
            _offerLeft = _offerSeconds;
            _isOfferUp = true;

            for (int i = 0; i < _cards.Length; i++)
            {
                UpgradeCard card = _cards[i];
                bool isUsed = i < offer.Options.Count;
                card.Button.gameObject.SetActive(isUsed);
                if (!isUsed) continue;

                UpgradeOptionVO option = offer.Options[i];
                card.Title.text = option.Title;
                card.Description.text = option.Description;
                card.Icon.sprite = _kindIcons[Mathf.Clamp((int) option.Kind, 0, _kindIcons.Length - 1)];

                bool showsRole = !option.AllRoles;
                card.RoleIcon.gameObject.SetActive(showsRole);
                if (showsRole)
                    card.RoleIcon.sprite = _roleIcons[Mathf.Clamp((int) option.Role, 0, _roleIcons.Length - 1)];
            }

            _offerPanel.SetActive(true);
        }

        public void HideUpgradeOffer(int offerId)
        {
            if (offerId != _offerId) return;

            _isOfferUp = false;
            _offerPanel.SetActive(false);
        }

        public void ShowEnemy(EnemyStateVO enemy)
        {
            _enemyHpTarget = Ratio(enemy.Hp, enemy.MaxHp);

            if (enemy.SpawnId == _enemySpawnId)
                return;

            bool hadEnemy = _enemySpawnId >= 0;
            _enemySpawnId = enemy.SpawnId;

            if (_transition != null) StopCoroutine(_transition);
            _transition = StartCoroutine(PlayNextEnemy(enemy, hadEnemy));
        }

        public void ShowEnemyHit(PartyRole role, int damage, bool isCritical)
        {
            if (!Slots.TryGetValue(role, out PartySlot slot))
                return;

            slot.Actor.PlayAttack();
            string label = isCritical ? $"{damage}!" : damage.ToString();
            Color color = isCritical ? _criticalColor : _enemyDamageColor;
            float scale = isCritical ? 1.4f : 1f;

            switch (role)
            {
                case PartyRole.Warrior:
                    slot.Actor.Lunge(ENEMY_HOME_X - ((RectTransform) slot.Actor.transform).anchoredPosition.x - 70f, 0.3f);
                    StartCoroutine(PlayMelee(label, color, scale));
                    break;
                case PartyRole.Archer:
                    StartCoroutine(PlayProjectile(_arrow, null, slot.Actor, label, color, scale));
                    break;
                default:
                    StartCoroutine(PlayProjectile(_fireball, _fireballFrames, slot.Actor, label, color, scale));
                    break;
            }
        }

        public void ShowHeroHit(PartyRole role, int damage)
        {
            if (!Slots.TryGetValue(role, out PartySlot slot))
                return;

            _enemy.PlayAttack();
            _enemy.Lunge(-50f, 0.3f);
            StartCoroutine(PlayHeroHurt(slot, damage));
        }

        public void AddLog(string line) => _logText.text = line;

        public void Announce(string message)
        {
            _announcementText.text = $"[SYSTEM]  {message}";
            if (_announcement != null) StopCoroutine(_announcement);
            _announcement = StartCoroutine(PlayAnnouncement());
        }

        public override void BeforeScreenActivation()
        {
            base.BeforeScreenActivation();

            _enemySpawnId = -1;
            _isWalking = false;
            _logText.text = string.Empty;
            _announcementGroup.alpha = 0f;
            _popupTemplate.gameObject.SetActive(false);
            _sparkTemplate.gameObject.SetActive(false);
            _arrow.gameObject.SetActive(false);
            _fireball.gameObject.SetActive(false);
            _slash.gameObject.SetActive(false);
            _enemy.gameObject.SetActive(false);
            _enemyHud.gameObject.SetActive(false);
            _healTemplate.gameObject.SetActive(false);
            _isOfferUp = false;
            _offerPanel.SetActive(false);

            foreach (PartySlot slot in _party)
                slot.Actor.SetDown(false);
        }

        private void OnEnable()
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                int index = i;
                _cards[i].Button.onClick.AddListener(() => UpgradeChosen?.Invoke(_offerId, index));
            }
        }

        private void OnDisable()
        {
            foreach (UpgradeCard card in _cards)
                card.Button.onClick.RemoveAllListeners();
        }

        private Dictionary<PartyRole, PartySlot> Slots
        {
            get
            {
                if (_slots.Count == 0)
                    foreach (PartySlot slot in _party)
                        _slots[slot.Role] = slot;
                return _slots;
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            Scroll(_sky, (_isWalking ? _skyWalkSpeed : 0f) + _skyDrift, dt);
            Scroll(_lava, _lavaFlow, dt);
            if (_isWalking)
            {
                Scroll(_ruins, _ruinsWalkSpeed, dt);
                Scroll(_ground, _groundWalkSpeed, dt);
            }

            if (_isOfferUp)
            {
                _offerLeft = Mathf.Max(0f, _offerLeft - dt);
                _offerTimerFill.fillAmount = _offerLeft / _offerSeconds;
            }

            float step = dt * BAR_EASE_SPEED;
            _expFill.fillAmount = Mathf.Lerp(_expFill.fillAmount, _expTarget, step);
            _enemyHpFill.fillAmount = Mathf.Lerp(_enemyHpFill.fillAmount, _enemyHpTarget, step);

            foreach (PartySlot slot in _party)
            {
                if (_memberHpTargets.TryGetValue(slot.Role, out float target))
                    slot.HpFill.fillAmount = Mathf.Lerp(slot.HpFill.fillAmount, target, step);
            }

            // The creature's name and bar ride above it wherever it is.
            var enemyRoot = (RectTransform) _enemy.transform;
            _enemyHud.anchoredPosition = new Vector2(enemyRoot.anchoredPosition.x, FLOOR_Y + BODY_SIZE * _enemyScale * 0.85f + 6f);
        }

        private static void Scroll(RawImage layer, float speed, float dt)
        {
            Rect uv = layer.uvRect;
            uv.x = Mathf.Repeat(uv.x + speed * dt, 1f);
            layer.uvRect = uv;
        }

        private static float Ratio(long value, long max) => max <= 0 ? 0f : Mathf.Clamp01((float) value / max);

        private Vector2 EnemyChest => ((RectTransform) _enemy.transform).anchoredPosition + new Vector2(0f, BODY_SIZE * _enemyScale * 0.4f);

        private IEnumerator PlayNextEnemy(EnemyStateVO next, bool hadEnemy)
        {
            var enemyRoot = (RectTransform) _enemy.transform;

            // The last blow lands, then the fallen creature fades where it stands.
            if (hadEnemy && _enemy.gameObject.activeSelf)
            {
                yield return new WaitForSecondsRealtime(DEATH_DELAY);
                _enemyHud.gameObject.SetActive(false);
                SpawnSpark(EnemyChest);
                for (float t = 0f; t < DEATH_FADE; t += Time.unscaledDeltaTime)
                {
                    _enemy.SetAlpha(1f - t / DEATH_FADE);
                    yield return null;
                }
            }

            EnemyBody body = Array.Find(_enemyBodies, b => b.Look == next.Look) ?? _enemyBodies[0];
            _enemy.SetFrames(body.Idle, body.Attack, body.Hurt);
            _enemy.SetTint(next.IsElite ? _eliteTint : Color.white);
            _enemy.SetAlpha(1f);
            _enemyNameText.color = next.IsBoss ? _bossNameColor : next.IsElite ? _eliteNameColor : Color.white;
            _enemyScale = Mathf.Max(0.5f, next.Scale);
            enemyRoot.localScale = new Vector3(-_enemyScale, _enemyScale, 1f);
            enemyRoot.anchoredPosition = new Vector2(ENEMY_ENTER_X, FLOOR_Y);
            _enemy.gameObject.SetActive(true);

            _enemyNameText.text = $"{next.Name}  Lv {next.Level}";
            _enemyHpFill.fillAmount = 1f;
            _enemyHud.gameObject.SetActive(true);

            // The party walks on while the dungeon scrolls by and the creature closes in.
            _isWalking = true;
            foreach (PartySlot slot in _party)
                if (!slot.Actor.IsDown) slot.Actor.PlayWalk();

            for (float t = 0f; t < WALK_SECONDS; t += Time.unscaledDeltaTime)
            {
                float k = 1f - (1f - t / WALK_SECONDS) * (1f - t / WALK_SECONDS);
                enemyRoot.anchoredPosition = new Vector2(Mathf.Lerp(ENEMY_ENTER_X, ENEMY_HOME_X, k), FLOOR_Y);
                yield return null;
            }

            enemyRoot.anchoredPosition = new Vector2(ENEMY_HOME_X, FLOOR_Y);
            _isWalking = false;
            foreach (PartySlot slot in _party)
                if (!slot.Actor.IsDown) slot.Actor.PlayIdle();

            _transition = null;
        }

        private IEnumerator PlayMelee(string label, Color color, float scale)
        {
            yield return new WaitForSecondsRealtime(0.14f);
            Vector2 at = EnemyChest;
            StartCoroutine(PlayFrames(_slash, _slashFrames, at, 0.05f));
            Impact(at, label, color, scale);
        }

        private IEnumerator PlayProjectile(Image projectile, Sprite[] frames, SpriteActor from, string label, Color color, float scale)
        {
            yield return new WaitForSecondsRealtime(0.1f);

            // Each kind has one projectile; a second shot before the first lands takes it over.
            Vector2 start = ((RectTransform) from.transform).anchoredPosition + new Vector2(30f, 38f);
            Vector2 end = EnemyChest;
            projectile.gameObject.SetActive(true);

            for (float t = 0f; t < PROJECTILE_SECONDS; t += Time.unscaledDeltaTime)
            {
                float k = t / PROJECTILE_SECONDS;
                projectile.rectTransform.anchoredPosition = Vector2.Lerp(start, end, k) + new Vector2(0f, Mathf.Sin(k * Mathf.PI) * 10f);
                if (frames != null && frames.Length > 0)
                    projectile.sprite = frames[Mathf.FloorToInt(t * 16f) % frames.Length];
                yield return null;
            }

            projectile.gameObject.SetActive(false);
            Impact(end, label, color, scale);
        }

        private void Impact(Vector2 at, string label, Color color, float scale)
        {
            _enemy.PlayHurt();
            SpawnSpark(at);
            SpawnPopup(at + new Vector2(0f, 20f), label, color, scale);
        }

        private IEnumerator PlayHeroHurt(PartySlot slot, int damage)
        {
            yield return new WaitForSecondsRealtime(0.15f);
            slot.Actor.PlayHurt();
            Vector2 at = ((RectTransform) slot.Actor.transform).anchoredPosition + new Vector2(0f, 46f);
            SpawnSpark(at);
            SpawnPopup(at + new Vector2(0f, 18f), damage.ToString(), _heroDamageColor, 1f);
        }

        private void SpawnSpark(Vector2 at)
        {
            Image spark = _sparkPool.Count > 0 ? _sparkPool.Pop() : Instantiate(_sparkTemplate, _effects);
            StartCoroutine(PlaySpark(spark, at));
        }

        private IEnumerator PlaySpark(Image spark, Vector2 at)
        {
            yield return PlayFrames(spark, _sparkFrames, at + new Vector2(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-8f, 8f)), 0.06f);
            _sparkPool.Push(spark);
        }

        private static IEnumerator PlayPooledFrames(Image image, Sprite[] frames, Vector2 at, float secondsPerFrame, Stack<Image> pool)
        {
            yield return PlayFrames(image, frames, at, secondsPerFrame);
            pool.Push(image);
        }

        private static IEnumerator PlayFrames(Image image, Sprite[] frames, Vector2 at, float secondsPerFrame)
        {
            image.rectTransform.anchoredPosition = at;
            image.gameObject.SetActive(true);
            foreach (Sprite frame in frames)
            {
                image.sprite = frame;
                yield return new WaitForSecondsRealtime(secondsPerFrame);
            }

            image.gameObject.SetActive(false);
        }

        private void SpawnPopup(Vector2 at, string label, Color color, float scale)
        {
            Text popup = _popupPool.Count > 0 ? _popupPool.Pop() : Instantiate(_popupTemplate, _effects);
            popup.text = label;
            popup.color = color;
            popup.transform.localScale = Vector3.one * scale;
            popup.rectTransform.anchoredPosition = at + new Vector2(UnityEngine.Random.Range(-10f, 10f), 0f);
            popup.gameObject.SetActive(true);
            StartCoroutine(FloatPopup(popup));
        }

        private IEnumerator FloatPopup(Text popup)
        {
            Vector2 start = popup.rectTransform.anchoredPosition;
            Color color = popup.color;

            for (float t = 0f; t < POPUP_SECONDS; t += Time.unscaledDeltaTime)
            {
                float k = t / POPUP_SECONDS;
                popup.rectTransform.anchoredPosition = start + Vector2.up * (POPUP_RISE * (1f - (1f - k) * (1f - k)));
                popup.color = new Color(color.r, color.g, color.b, 1f - k * k);
                yield return null;
            }

            popup.gameObject.SetActive(false);
            _popupPool.Push(popup);
        }

        private IEnumerator PlayAnnouncement()
        {
            const float fade = 0.25f;

            for (float t = 0f; t < fade; t += Time.unscaledDeltaTime)
            {
                _announcementGroup.alpha = t / fade;
                yield return null;
            }

            _announcementGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(ANNOUNCEMENT_SECONDS);

            for (float t = 0f; t < fade; t += Time.unscaledDeltaTime)
            {
                _announcementGroup.alpha = 1f - t / fade;
                yield return null;
            }

            _announcementGroup.alpha = 0f;
            _announcement = null;
        }

        protected override void PlayShowAnimation() => ShowCompleted?.Invoke(this);

        protected override void PlayHideAnimation() => HideCompleted?.Invoke(this);
    }
}
