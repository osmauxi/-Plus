using ProjectGame.HotFix.Gameplay.Pooling;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    /// <summary>池化怪物碰撞代理；向房间级 VAT 渲染提供共享资源配置。</summary>
    [DisallowMultipleComponent]
    public sealed class MonsterViewHandle : MonoBehaviour, IMonsterViewHandle, IPoolable
    {
        [SerializeField] private MonsterVATAsset _vatAsset;
        [SerializeField] private Material _vatMaterial;
        [SerializeField, Min(0f)] private float _animationCrossFadeDuration = 0.12f;
        [SerializeField, Min(0f)] private float _movementThreshold = 0.05f;
        [SerializeField] private Collider[] _damageColliders;
        [SerializeField, Min(0.01f)] private float _hitFlashDuration = 0.08f;
        [SerializeField] private Color _hitFlashColor = Color.white;

        private MonsterVATRenderProfile _renderProfile;

        public GameObject Instance => gameObject;
        public int Slot { get; private set; } = -1;
        public MonsterVATRenderProfile RenderProfile => _renderProfile;

        public void BindSlot(int slot) => Slot = slot;

        private void Awake()
        {
            SkinnedMeshRenderer sourceRenderer = GetComponentInChildren<SkinnedMeshRenderer>(true);
            _renderProfile = new MonsterVATRenderProfile(
                _vatAsset,
                _vatMaterial,
                sourceRenderer.sharedMaterial,
                transform.localScale,
                transform.worldToLocalMatrix * sourceRenderer.transform.localToWorldMatrix,
                _animationCrossFadeDuration,
                _movementThreshold,
                _hitFlashDuration,
                _hitFlashColor,
                sourceRenderer.shadowCastingMode,
                sourceRenderer.receiveShadows,
                gameObject.layer);
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].enabled = false;
        }

        public void OnRentFromPool()
        {
            Slot = -1;
            for (int i = 0; i < _damageColliders.Length; i++)
                _damageColliders[i].enabled = true;
        }

        public void OnReturnToPool()
        {
            Slot = -1;
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
        }
    }
}
