using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.GameplayModule.GameplayScreenModule.Entities
{
    /// <summary>
    /// A pixel-art figure on the stage: it flips through the frames of whatever it is doing -
    /// standing, walking, attacking - flashes when hit and lies down when felled. Where it stands
    /// is its RectTransform, which the view moves; a lunge moves only the body inside it.
    /// </summary>
    public class SpriteActor : MonoBehaviour
    {
        private enum Pose
        {
            Idle = 0,
            Walk = 1,
            Attack = 2,
        }

        [SerializeField] private Image _body;
        [SerializeField] private Sprite[] _idle;
        [SerializeField] private Sprite[] _walk;
        [SerializeField] private Sprite[] _attack;
        [SerializeField] private Sprite _hurt;
        [SerializeField] private Sprite _down;
        [SerializeField] private float _framesPerSecond = 6f;
        [SerializeField] private float _attackFramesPerSecond = 12f;

        private Pose _pose;
        private float _clock;
        private bool _isDown;
        private float _hurtUntil;
        private Coroutine _lunge;
        private Color _tint = Color.white;

        public RectTransform Body => _body.rectTransform;
        public bool IsDown => _isDown;

        /// <summary>Gives a figure that changes body - the creature slot - its frames. Walking reuses standing.</summary>
        public void SetFrames(Sprite[] idle, Sprite[] attack, Sprite hurt)
        {
            _idle = idle;
            _walk = idle;
            _attack = attack;
            _hurt = hurt;
            _down = null;
            _isDown = false;
            PlayIdle();
        }

        public void PlayIdle() => SetPose(Pose.Idle);
        public void PlayWalk() => SetPose(Pose.Walk);
        public void PlayAttack() => SetPose(Pose.Attack);

        public void PlayHurt() => _hurtUntil = Time.unscaledTime + 0.15f;

        public void SetDown(bool isDown)
        {
            _isDown = isDown;
            if (!isDown) PlayIdle();
            Draw();
        }

        /// <summary>Colours the whole body - a creature's elite glow. White is the sprite as drawn.</summary>
        public void SetTint(Color tint)
        {
            _tint = tint;
            _body.color = new Color(tint.r, tint.g, tint.b, _body.color.a);
        }

        public void SetAlpha(float alpha) => _body.color = new Color(_tint.r, _tint.g, _tint.b, alpha);

        /// <summary>The body darts by offset and back, over seconds - a melee strike or a creature's pounce.</summary>
        public void Lunge(float offset, float seconds)
        {
            if (_lunge != null) StopCoroutine(_lunge);
            _lunge = StartCoroutine(PlayLunge(offset, seconds));
        }

        private void OnDisable()
        {
            _lunge = null;
            _body.rectTransform.anchoredPosition = Vector2.zero;
        }

        private void SetPose(Pose pose)
        {
            _pose = pose;
            _clock = 0f;
            Draw();
        }

        private void Update()
        {
            _clock += Time.unscaledDeltaTime;
            Draw();
        }

        private void Draw()
        {
            if (_isDown && _down != null) { _body.sprite = _down; return; }
            if (Time.unscaledTime < _hurtUntil && _hurt != null) { _body.sprite = _hurt; return; }

            Sprite[] frames = _pose == Pose.Walk ? _walk : _pose == Pose.Attack ? _attack : _idle;
            if (frames == null || frames.Length == 0) return;

            if (_pose == Pose.Attack)
            {
                int index = Mathf.FloorToInt(_clock * _attackFramesPerSecond);
                if (index >= frames.Length) { SetPose(Pose.Idle); return; }
                _body.sprite = frames[index];
                return;
            }

            float fps = _pose == Pose.Walk ? _framesPerSecond * 1.6f : _framesPerSecond * 0.5f;
            _body.sprite = frames[Mathf.FloorToInt(_clock * fps) % frames.Length];
        }

        private IEnumerator PlayLunge(float offset, float seconds)
        {
            RectTransform body = _body.rectTransform;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                float k = t / seconds;
                body.anchoredPosition = new Vector2(Mathf.Sin(k * Mathf.PI) * offset, 0f);
                yield return null;
            }

            body.anchoredPosition = Vector2.zero;
            _lunge = null;
        }
    }
}
