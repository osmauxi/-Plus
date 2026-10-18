using System.Collections;
using ProjectGame.HotFix.Gameplay.Pooling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>池化怪物预制件的表现入口；只负责姿态、VAT 动画和命中碰撞体。</summary>
    [DisallowMultipleComponent]
    public sealed class MonsterViewHandle : MonoBehaviour, IMonsterViewHandle, IPoolable
    {
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int BaseColorHash = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorHash = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorHash = Shader.PropertyToID("_EmissionColor");
        private static readonly int HitFlashColorHash = Shader.PropertyToID("_HitFlashColor");
        private static readonly int HitFlashAmountHash = Shader.PropertyToID("_HitFlashAmount");

        [SerializeField] private MonsterVATAsset _vatAsset;
        [SerializeField] private Material _vatMaterial;
        [SerializeField, Min(0f)] private float _animationCrossFadeDuration = 0.12f;
        [SerializeField, Min(0f)] private float _movementThreshold = 0.05f;
        [SerializeField] private Collider[] _damageColliders;
        [SerializeField] private Renderer[] _flashRenderers;
        [SerializeField, Min(0.01f)] private float _hitFlashDuration = 0.08f;
        [SerializeField] private Color _hitFlashColor = Color.white;

        private Vector2 _previousPosition;
        private bool _hasPreviousPose;
        private MonsterVATPlayer _vatPlayer;
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
            MonsterVATPlayer player = new MonsterVATPlayer();
            if (player.Initialize(_vatAsset, _vatMaterial, transform, _animationCrossFadeDuration))
            {
                _vatPlayer = player;
                _flashRenderers = new Renderer[] { _vatPlayer.Renderer };
            }

            InitializeFlashColors();
        }

        private void Update()
        {
            _vatPlayer.Tick(Time.deltaTime);
        }

        public void OnRentFromPool()
        {
            Slot = -1;
            _hasPreviousPose = false;
            ResetHitFlash();
            _vatPlayer.ResetToIdle();

            for (int i = 0; i < _damageColliders.Length; i++)
                    _damageColliders[i].enabled = true;
        }

        public void OnReturnToPool()
        {
            Slot = -1;
            ResetHitFlash();
            _vatPlayer.ResetToIdle();
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
            float moveSpeed = _hasPreviousPose ? Vector2.Distance(_previousPosition, position) / Mathf.Max(Time.deltaTime, 0.0001f) : 0f;
             _vatPlayer.SetLocomotion(moveSpeed > _movementThreshold);
            _previousPosition = position;
            _hasPreviousPose = true;
        }

        public void PlayAttack()
        {
            _vatPlayer.PlayAttack();
        }

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

        private void InitializeFlashColors()
        {
            _baseColors = new Color[_flashRenderers.Length];
            _colors = new Color[_flashRenderers.Length];
            _emissionColors = new Color[_flashRenderers.Length];
            for (int i = 0; i < _flashRenderers.Length; i++)
            {
                Renderer renderer = _flashRenderers[i];
                Material material = renderer != null ? renderer.sharedMaterial : null;
                _baseColors[i] = material != null && material.HasProperty(BaseColorHash) ? material.GetColor(BaseColorHash) : Color.white;
                _colors[i] = material != null && material.HasProperty(ColorHash) ? material.GetColor(ColorHash) : Color.white;
                _emissionColors[i] = material != null && material.HasProperty(EmissionColorHash) ? material.GetColor(EmissionColorHash) : Color.black;
            }
        }

        private void ResetHitFlash()
        {
            StopAllCoroutines();
            _hitFlashRoutine = null;
            SetHitFlash(false);
        }

        private void SetHitFlash(bool active)
        {
            if (_propertyBlock == null || _flashRenderers == null)
                return;
            for (int i = 0; i < _flashRenderers.Length; i++)
            {
                Renderer renderer = _flashRenderers[i];
                if (renderer == null)
                    continue;
                _propertyBlock.Clear();
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(HitFlashColorHash, _hitFlashColor);
                _propertyBlock.SetFloat(HitFlashAmountHash, active ? 1f : 0f);
  
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}
