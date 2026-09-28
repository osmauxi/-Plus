using System.Collections;
using System.Collections.Generic;
using ProjectGame.HotFix.Gameplay.Pooling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>池化怪物预制件的表现入口；只负责姿态、动画和命中碰撞体。</summary>
    [DisallowMultipleComponent]
    public sealed class MonsterViewHandle : MonoBehaviour, IMonsterViewHandle, IPoolable
    {
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int BaseColorHash = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorHash = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorHash = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Animator _animator;
        [SerializeField] private Collider[] _damageColliders;
        [SerializeField] private Renderer[] _flashRenderers;
        [SerializeField, Min(0.01f)] private float _hitFlashDuration = 0.08f;
        [SerializeField] private Color _hitFlashColor = Color.white;

        private Vector2 _previousPosition;
        private bool _hasPreviousPose;
        private MaterialPropertyBlock _propertyBlock;
        private Color[] _baseColors;
        private Color[] _colors;
        private Color[] _emissionColors;
        private Coroutine _hitFlashRoutine;

        public GameObject Instance => gameObject;
        public int Slot { get; private set; } = -1;

        public void BindSlot(int slot) => Slot = slot;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _baseColors = new Color[_flashRenderers.Length];
            _colors = new Color[_flashRenderers.Length];
            _emissionColors = new Color[_flashRenderers.Length];
            for (int i = 0; i < _flashRenderers.Length; i++)
            {
                Material material = _flashRenderers[i].sharedMaterial;
                _baseColors[i] = material.GetColor(BaseColorHash);
                _colors[i] = material.GetColor(ColorHash);
                _emissionColors[i] = material.GetColor(EmissionColorHash);
            }
        }

        public void OnRentFromPool()
        {
            Slot = -1;
            _hasPreviousPose = false;
            ResetHitFlash();
            _animator.Rebind();
            for (int i = 0; i < _damageColliders.Length; i++)
                _damageColliders[i].enabled = true;
        }

        public void OnReturnToPool()
        {
            Slot = -1;
            ResetHitFlash();
            _animator.ResetTrigger(AttackHash);
            _animator.SetFloat(MoveSpeedHash, 0f);
            for (int i = 0; i < _damageColliders.Length; i++)
                _damageColliders[i].enabled = false;
        }

        public void SetPose(Vector2 position, float yaw)
        {
            Transform viewTransform = transform;
            Vector3 worldPosition = viewTransform.position;
            worldPosition.x = position.x;
            worldPosition.z = position.y;
            viewTransform.SetPositionAndRotation(worldPosition, Quaternion.Euler(0f, yaw, 0f));

            float moveSpeed = _hasPreviousPose
                ? Vector2.Distance(_previousPosition, position) / Mathf.Max(Time.deltaTime, 0.0001f)
                : 0f;
            _animator.SetFloat(MoveSpeedHash, moveSpeed);
            _previousPosition = position;
            _hasPreviousPose = true;
        }

        public void PlayAttack() => _animator.SetTrigger(AttackHash);

        public void PlayHit()
        {
            if (_hitFlashRoutine != null)
                StopCoroutine(_hitFlashRoutine);
            _hitFlashRoutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            SetHitFlash(true);
            yield return new WaitForSecondsRealtime(_hitFlashDuration);
            SetHitFlash(false);
            _hitFlashRoutine = null;
        }

        private void ResetHitFlash()
        {
            if (_hitFlashRoutine != null)
                StopCoroutine(_hitFlashRoutine);
            _hitFlashRoutine = null;
            SetHitFlash(false);
        }

        private void SetHitFlash(bool active)
        {
            for (int i = 0; i < _flashRenderers.Length; i++)
            {
                Renderer renderer = _flashRenderers[i];
                _propertyBlock.Clear();
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(BaseColorHash, active ? _hitFlashColor : _baseColors[i]);
                _propertyBlock.SetColor(ColorHash, active ? _hitFlashColor : _colors[i]);
                _propertyBlock.SetColor(EmissionColorHash,
                    active ? _hitFlashColor : _emissionColors[i]);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}
