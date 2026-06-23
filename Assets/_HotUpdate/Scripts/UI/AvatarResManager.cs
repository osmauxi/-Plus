using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using ProjectGame.HotFix.Netcode;
using ProjectGame.HotFix.Core.Network;

namespace ProjectGame.HotFix.Lobby
{
    /// <summary>
    /// 大厅3D模型管理器
    /// </summary>
    public class AvatarResManager : MonoBehaviour
    {
        [Header("人物展台锚点")]
        [SerializeField] private Transform[] _stationAnchors;

        //内部结构：记录每个展台当前正在使用的资源句柄和数据
        private class StationData
        {
            public ulong CurrentClientId = 0;
            public int CharacterId = 0;
            public int WeaponId = 0;

            public GameObject CharacterInstance;
            public GameObject WeaponInstance;

            public AsyncOperationHandle<GameObject> CharacterHandle;
            public AsyncOperationHandle<GameObject> WeaponHandle;

            public Transform WeaponSocket;
        }

        //维护4个展台的内部状态
        private StationData[] _stations;

        private void Awake()
        {
            //初始化展台数据
            _stations = new StationData[_stationAnchors.Length];
            for (int i = 0; i < _stations.Length; i++)
            {
                _stations[i] = new StationData();
            }
        }

        private void Start()
        {
            LobbyNetworkManager.Instance.OnLobbyDataChanged += Sync3DWorld;  
        }

        private void OnDestroy()
        {
            LobbyNetworkManager.Instance.OnLobbyDataChanged -= Sync3DWorld;
            

            //场景销毁时，彻底释放所有显存
            foreach (var station in _stations)
            {
                ReleaseStation(station);
            }
        }

        /// <summary>
        /// 检测网络数据变化对比并刷新物理世界
        /// </summary>
        private void Sync3DWorld()
        {
            var networkList = LobbyNetworkManager.Instance.LobbyPlayers;

            //遍历所有位置，有人就刷新，没人就清空
            for (int i = 0; i < _stationAnchors.Length; i++)
            {
                if (i < networkList.Count)
                {
                    UpdateStation(i, networkList[i]);
                }
                else
                {
                    if (_stations[i].CharacterInstance != null)
                    {
                        ReleaseStation(_stations[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 异步更新单个展台的模型
        /// </summary>
        private async void UpdateStation(int index, LobbyPlayerState state)
        {
            var station = _stations[index];

            //角色模型变更判定
            if (station.CharacterId != state.CharacterId || station.CurrentClientId != state.ClientId)
            {
                //记录目标ID，防并发狂点
                int targetCharId = state.CharacterId;

                //存在旧模型就先释放
                if (station.CharacterHandle.IsValid())
                    Addressables.ReleaseInstance(station.CharacterHandle);

                string charAddress = $"Character_{targetCharId}";
                //通过IO请求，拿到句柄，等它加载完才继续（Addressables内部有引用计数等缓存机制）
                station.CharacterHandle = Addressables.InstantiateAsync(charAddress, _stationAnchors[index]);

                //InstantiateAsync等原生异步API使用事件与回调实现类异步，无法await
                //ToUniTask方法能将这些异步事件转换为可await的UniTask，此句等效于协程的Yield return,但是性能表现更加优越。
                await station.CharacterHandle.ToUniTask();

                //防竞态校验：如果在加载的这半秒内，玩家又切了别的模型，当前结果作废
                if(targetCharId != LobbyNetworkManager.Instance.LobbyPlayers[index].CharacterId)
                {
                    Addressables.ReleaseInstance(station.CharacterHandle);
                    return;
                }

                //检查句柄状态，成功就记录实例和ID
                if(station.CharacterHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    station.CharacterInstance = station.CharacterHandle.Result;
                    station.CharacterId = targetCharId;
                    station.CurrentClientId = state.ClientId;

                    //寻找新模型的武器挂载点 (必须在预制体里约定好这个名字)
                    //Transform[] allTransforms = station.CharacterInstance.GetComponentsInChildren<Transform>();
                    //foreach (var t in allTransforms)
                    //{
                    //    if (t.name == "Weapon_Socket")
                    //    {
                    //        station.WeaponSocket = t;
                    //        break;
                    //    }
                    //}

                    //AvatarSockets sockets = station.CharacterInstance.GetComponent<AvatarSockets>();
                    //if (sockets != null)
                    //{
                    //    station.WeaponSocket = sockets.WeaponSocket;
                    //}

                    // 因为换了人，必须强制重新刷一次武器
                    station.WeaponId = 0;
                }
            }

            // ----------------------------------------------------
            // 阶段 2：武器模型变更判定 (必须等角色加载完才有手部节点)
            // ----------------------------------------------------
            if (station.WeaponId != state.WeaponId && station.CharacterInstance != null)
            {
                int targetWeaponId = state.WeaponId;

                // 释放旧武器
                if (station.WeaponHandle.IsValid())
                    Addressables.ReleaseInstance(station.WeaponHandle);

                string weaponAddress = $"Weapon_{targetWeaponId}";

                // 挂载在找到的手部节点上，如果没有就挂在脚下(兜底)
                Transform parentSocket = station.WeaponSocket != null ? station.WeaponSocket : station.CharacterInstance.transform;

                station.WeaponHandle = Addressables.InstantiateAsync(weaponAddress, parentSocket);
                await station.WeaponHandle.ToUniTask();

                if (targetWeaponId != LobbyNetworkManager.Instance.LobbyPlayers[index].WeaponId)
                {
                    Addressables.ReleaseInstance(station.WeaponHandle);
                    return;
                }

                if (station.WeaponHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    station.WeaponInstance = station.WeaponHandle.Result;
                    // 归零局部坐标，让武器完美贴合手部
                    station.WeaponInstance.transform.localPosition = Vector3.zero;
                    station.WeaponInstance.transform.localRotation = Quaternion.identity;
                    station.WeaponId = targetWeaponId;
                }
            }
        }

        /// <summary>
        /// 彻底清空展台
        /// </summary>
        private void ReleaseStation(StationData station)
        {
            if (station.WeaponHandle.IsValid())
                Addressables.ReleaseInstance(station.WeaponHandle);

            if (station.CharacterHandle.IsValid())
                Addressables.ReleaseInstance(station.CharacterHandle);

            station.CharacterId = 0;
            station.WeaponId = 0;
            station.CurrentClientId = 0;
            station.CharacterInstance = null;
            station.WeaponInstance = null;
            station.WeaponSocket = null;
        }
    }
}