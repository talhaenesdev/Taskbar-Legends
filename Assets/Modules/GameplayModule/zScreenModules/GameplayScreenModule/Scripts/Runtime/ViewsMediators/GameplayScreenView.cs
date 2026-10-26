using System;
using System.Collections;
using System.Collections.Generic;
using FlowIoC.BaseModule.Injectable.Components;
using FlowIoC.ScreenModule.ViewsMediators.Screen;
using Modules.GameplayModule.Shared.Data.ValueObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modules.GameplayModule.GameplayScreenModule.ViewsMediators
{
    /// <summary>
    /// The System window the battle runs in. It draws what it is handed and animates it - bars
    /// easing to their value, damage numbers floating off, a notice fading in and out - and decides
    /// nothing. Dragging anywhere on the window is reported as a drag of the window.
    /// </summary>
    [RequireComponent(typeof(ViewInjector))]
    public class GameplayScreenView : ScreenView, IBeginDragHandler, IDragHandler
    {
        private const int LOG_LINES = 3;
        private const float BAR_EASE_SPEED = 8f;
        private const float POPUP_SECONDS = 0.7f;
        private const float POPUP_RISE = 28f;
        private const float ANNOUNCEMENT_SECONDS = 2.4f;
        private const float HIT_FLASH_SECONDS = 0.12f;

        [Header("Hero")]
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _rankText;
        [SerializeField] private Text _rankTitleText;
        [SerializeField] private Image _heroHpFill;
        [SerializeField] private Text _heroHpText;
        [SerializeField] private Image _expFill;
        [SerializeField] private Text _expText;
        [SerializeField] private Image _heroBody;
        [SerializeField] private Color _heroTint = new(0.35f, 0.75f, 1f);

        [Header("Stats")]
        [SerializeField] private Text _attackText;
        [SerializeField] private Text _critText;
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _killsText;

        [Header("Enemy")]
        [SerializeField] private Text _enemyNameText;
        [SerializeField] private Image _enemyHpFill;
        [SerializeField] private Text _enemyHpText;
        [SerializeField] private Image _enemyBody;

        [Header("System")]
        [SerializeField] private Text _logText;
        [SerializeField] private CanvasGroup _announcementGroup;
        [SerializeField] private Text _announcementText;
        [SerializeField] private Text _popupTemplate;
        [SerializeField] private Color _criticalColor = new(1f, 0.82f, 0.25f);
        [SerializeField] private Color _enemyDamageColor = Color.white;
        [SerializeField] private Color _heroDamageColor = new(1f, 0.35f, 0.35f);

        public Action DragBegan;
        public Action Dragged;

        private readonly Queue<string> _logLines = new();
        private readonly Stack<Text> _popupPool = new();

        private float _heroHpTarget = 1f;
        private float _expTarget;
        private float _enemyHpTarget = 1f;

        private Color _enemyTint = Color.white;
        private Coroutine _announcement;

        public void OnBeginDrag(PointerEventData eventData) => DragBegan?.Invoke();
        public void OnDrag(PointerEventData eventData) => Dragged?.Invoke();

        public void ShowHero(HeroStateVO hero)
        {
            _levelText.text = $"Lv {hero.Level}";
            _rankText.text = hero.Rank.ToString();
            _rankTitleText.text = hero.RankTitle;

            _heroHpTarget = Ratio(hero.Hp, hero.MaxHp);
            _heroHpText.text = $"{hero.Hp} / {hero.MaxHp}";

            _expTarget = Ratio(hero.Exp, hero.ExpToNextLevel);
            _expText.text = $"{hero.Exp} / {hero.ExpToNextLevel}";

            _attackText.text = $"ATK  {hero.Attack}";
            _critText.text = $"CRIT {Mathf.RoundToInt(hero.CritChance * 100f)}%";
            _goldText.text = $"{hero.Gold:N0} G";
            _killsText.text = $"KILLS {hero.Kills:N0}";
        }

        public void ShowEnemy(EnemyStateVO enemy)
        {
            _enemyNameText.text = string.IsNullOrEmpty(enemy.Name) ? string.Empty : $"{enemy.Name}  Lv {enemy.Level}";
            _enemyHpText.text = $"{enemy.Hp} / {enemy.MaxHp}";

            float target = Ratio(enemy.Hp, enemy.MaxHp);

            // A fresh creature fills its bar at once rather than easing up from the last one's zero.
            if (target > _enemyHpTarget)
                _enemyHpFill.fillAmount = target;

            _enemyHpTarget = target;
            _enemyTint = enemy.Tint;
            _enemyBody.color = _enemyTint;
        }

        public void ShowEnemyHit(int damage, bool isCritical)
        {
            string label = isCritical ? $"{damage}!" : damage.ToString();
            SpawnPopup(_enemyBody.rectTransform, label, isCritical ? _criticalColor : _enemyDamageColor, isCritical ? 1.35f : 1f);
            StartCoroutine(Flash(_enemyBody, () => _enemyTint));
        }

        public void ShowHeroHit(int damage)
        {
            SpawnPopup(_heroBody.rectTransform, damage.ToString(), _heroDamageColor, 1f);
            StartCoroutine(Flash(_heroBody, () => _heroTint));
        }

        public void AddLog(string line)
        {
            _logLines.Enqueue(line);
            while (_logLines.Count > LOG_LINES)
                _logLines.Dequeue();

            _logText.text = string.Join("\n", _logLines);
        }

        public void Announce(string message)
        {
            _announcementText.text = $"[SYSTEM]  {message}";

            if (_announcement != null)
                StopCoroutine(_announcement);

            _announcement = StartCoroutine(PlayAnnouncement());
        }

        public override void BeforeScreenActivation()
        {
            base.BeforeScreenActivation();

            _logLines.Clear();
            _logText.text = string.Empty;
            _announcementGroup.alpha = 0f;
            _heroBody.color = _heroTint;
            _popupTemplate.gameObject.SetActive(false);
        }

        private void Update()
        {
            float step = Time.unscaledDeltaTime * BAR_EASE_SPEED;
            _heroHpFill.fillAmount = Mathf.Lerp(_heroHpFill.fillAmount, _heroHpTarget, step);
            _expFill.fillAmount = Mathf.Lerp(_expFill.fillAmount, _expTarget, step);
            _enemyHpFill.fillAmount = Mathf.Lerp(_enemyHpFill.fillAmount, _enemyHpTarget, step);
        }

        private static float Ratio(long value, long max) => max <= 0 ? 0f : Mathf.Clamp01((float) value / max);

        private void SpawnPopup(RectTransform anchor, string label, Color color, float scale)
        {
            Text popup = _popupPool.Count > 0 ? _popupPool.Pop() : Instantiate(_popupTemplate, _popupTemplate.transform.parent);
            popup.text = label;
            popup.color = color;
            popup.transform.localScale = Vector3.one * scale;
            popup.rectTransform.position = anchor.position + new Vector3(UnityEngine.Random.Range(-10f, 10f), 10f, 0f);
            popup.gameObject.SetActive(true);
            StartCoroutine(FloatPopup(popup));
        }

        private IEnumerator FloatPopup(Text popup)
        {
            Vector3 start = popup.rectTransform.localPosition;
            Color color = popup.color;

            for (float t = 0f; t < POPUP_SECONDS; t += Time.unscaledDeltaTime)
            {
                float k = t / POPUP_SECONDS;
                popup.rectTransform.localPosition = start + Vector3.up * (POPUP_RISE * k);
                popup.color = new Color(color.r, color.g, color.b, 1f - k * k);
                yield return null;
            }

            popup.gameObject.SetActive(false);
            _popupPool.Push(popup);
        }

        /// <summary>The rest colour is read when the flash ends, so a creature replaced mid-flash keeps its own tint.</summary>
        private IEnumerator Flash(Image body, Func<Color> restColor)
        {
            body.color = Color.white;
            body.rectTransform.localScale = Vector3.one * 0.9f;
            yield return new WaitForSecondsRealtime(HIT_FLASH_SECONDS);
            body.color = restColor();
            body.rectTransform.localScale = Vector3.one;
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
