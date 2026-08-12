using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectGame.HotFix.Config;
using ProjectGame.HotFix.Gameplay.Map.Generation;
using ProjectGame.HotFix.Gameplay.Runtime;
using UnityEngine;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 从 ConfigManager 的 RoomTemplate 配表建立房间模板运行时索引。
    /// </summary>
    public sealed class RoomTemplateCatalog : MonoBehaviour, IGameRuntimeService
    {
        private readonly List<RoomTemplateConfig> _templates = new();
        private readonly Dictionary<int, RoomTemplateConfig> _templatesById = new();

        public bool IsInitialized { get; private set; }

        public IReadOnlyList<RoomTemplateConfig> Templates => _templates;

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            cancellationToken.ThrowIfCancellationRequested();

            Dictionary<int, Config_RoomTemplate> table =
                ConfigManager.Instance.GetTable<Config_RoomTemplate>();

            if (table == null)
                throw new InvalidOperationException("未加载 RoomTemplate 配置表。");

            if (table.Count == 0)
                throw new InvalidOperationException("RoomTemplate 配置表没有任何房间模板。");

            _templates.Clear();
            _templatesById.Clear();

            try
            {
                var templateIds = new List<int>(table.Keys);
                templateIds.Sort();

                for (int index = 0; index < templateIds.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    int templateId = templateIds[index];
                    Config_RoomTemplate row = table[templateId];

                    if (row == null)
                        throw new InvalidOperationException($"RoomTemplate 配置为空，TemplateId={templateId}");

                    if (row.TemplateId != templateId)
                    {
                        throw new InvalidOperationException(
                            $"RoomTemplate 配置主键不一致：DictionaryKey={templateId}，TemplateId={row.TemplateId}");
                    }

                    var template = new RoomTemplateConfig(
                        row.TemplateId,
                        (RoomType)row.RoomType,
                        (MapStrategyMask)row.AllowedStrategyMask,
                        row.PoolId,
                        (ConnectorMask)row.SupportedConnectorMask,
                        row.AllowUnusedConnectors,
                        (QuarterTurnMask)row.AllowedRotations,
                        row.Priority,
                        row.Weight);

                    template.Validate();

                    if (!_templatesById.TryAdd(template.TemplateId, template))
                        throw new InvalidOperationException($"房间 TemplateId 重复：{template.TemplateId}");

                    _templates.Add(template);
                }

                IsInitialized = true;
                Debug.Log($"[{nameof(RoomTemplateCatalog)}] 已从 ConfigManager 初始化，模板数量={_templates.Count}");

                return UniTask.CompletedTask;
            }
            catch
            {
                _templates.Clear();
                _templatesById.Clear();
                throw;
            }
        }

        public bool TryGetTemplate(int templateId, out RoomTemplateConfig template)
        {
            return _templatesById.TryGetValue(templateId, out template);
        }

        public RoomTemplateConfig GetTemplate(int templateId)
        {
            if (!_templatesById.TryGetValue(templateId, out RoomTemplateConfig template))
                throw new KeyNotFoundException($"找不到房间模板：{templateId}");

            return template;
        }

        public UniTask ShutdownAsync(CancellationToken cancellationToken)
        {
            _templates.Clear();
            _templatesById.Clear();
            IsInitialized = false;
            return UniTask.CompletedTask;
        }
    }
}
