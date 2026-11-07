using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Monsters
{
    [Serializable]
    public sealed class MonsterVATRenderProfileCatalogEntry
    {
        public string LocalPoolId;
        public MonsterVATRenderProfileAsset Profile;
    }

    /// <summary>
    /// 维护一个本地池ID到MonsterVATRenderProfileAsset的映射表，用于在运行时快速查找对应的渲染配置
    /// </summary>
    [CreateAssetMenu(menuName = "Monster/VAT Render Profile Catalog")]
    public sealed class MonsterVATRenderProfileCatalog : ScriptableObject
    {
        [SerializeField] private MonsterVATRenderProfileCatalogEntry[] _entries =
            Array.Empty<MonsterVATRenderProfileCatalogEntry>();

        public MonsterVATRenderProfile Resolve(string localPoolId)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                MonsterVATRenderProfileCatalogEntry entry = _entries[i];
                if (entry.LocalPoolId != localPoolId)
                    continue;
                if (entry.Profile == null)
                    throw new InvalidOperationException(
                        $"Monster VAT Profile Catalog 条目缺少 Profile：{localPoolId}");
                return entry.Profile.ToRuntimeProfile();
            }

            throw new KeyNotFoundException(
                $"Monster VAT Profile Catalog 未注册 LocalPoolId={localPoolId}。");
        }

        public void SetProfile(string localPoolId, MonsterVATRenderProfileAsset profile)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].LocalPoolId != localPoolId)
                    continue;
                _entries[i].Profile = profile;
                return;
            }

            Array.Resize(ref _entries, _entries.Length + 1);
            _entries[^1] = new MonsterVATRenderProfileCatalogEntry
            {
                LocalPoolId = localPoolId,
                Profile = profile,
            };
        }
    }
}
