using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Weapon
{

    public sealed class ProjectileHitTargetAdapter : MonoBehaviour
    {
        public IProjectileHitTarget Target { get; private set; }
        public bool HasEntityId { get; private set; }
        public ulong EntityId { get; private set; }

        /// <summary>
        /// 绑定自己的身份
        /// </summary>
        public void BindIdentity(ulong entityId)
        {
            EntityId = entityId;
            HasEntityId = true;
        }
        /// <summary>
        /// 绑定可伤害组件
        /// </summary>
        public void Bind(IProjectileHitTarget target)
        {
            Target = target;
            if (target != null) BindIdentity(target.EntityId);
        }

        public void Unbind()
        {
            Target = null;
            HasEntityId = false;
        }
    }
}
