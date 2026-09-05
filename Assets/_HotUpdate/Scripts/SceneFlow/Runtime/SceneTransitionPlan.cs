using System.Collections.Generic;
using ProjectGame.HotFix.Network.Runtime;

namespace ProjectGame.HotFix.SceneFlow
{
    /// <summary>
    /// 单次场景切换的定义
    /// 包含参数为：
    /// 当前的场景掩码，试图加载的场景掩码，Loading文字，需要加载的场景列表，需要卸载的场景列表
    /// </summary>
    internal sealed class SceneTransitionPlan
    {
        //这里Mask用在网络场景中，快速获取对应场景NetworkPrefab信息
        //ScenesToLoad等则用在具体的场景加载中
        public NetworkSceneMask ExpectedSourceMask { get; }
        public NetworkSceneMask TargetMask { get; }
        public string LoadingMessage { get; }
        public IReadOnlyList<PhysicalSceneReference> ScenesToLoad { get; }
        public IReadOnlyList<PhysicalSceneReference> ScenesToUnload { get; }

        public SceneTransitionPlan(
            NetworkSceneMask expectedSourceMask,
            NetworkSceneMask targetMask,
            string loadingMessage,
            IReadOnlyList<PhysicalSceneReference> scenesToLoad,
            IReadOnlyList<PhysicalSceneReference> scenesToUnload)
        {
            ExpectedSourceMask = expectedSourceMask;
            TargetMask = targetMask;
            LoadingMessage = loadingMessage;
            ScenesToLoad = scenesToLoad;
            ScenesToUnload = scenesToUnload;
        }
    }
}
