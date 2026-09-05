using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using ProjectGame.HotFix.Gameplay.Network;

namespace ProjectGame.HotFix.Gameplay.Weapon.Network
{
    /// <summary>
    /// 只负责路由
    /// </summary>
    public sealed class WeaponReplication
    {
        private const int WriterInitialCapacity = 64;
        private const int WriterMaxCapacity = 256;

        private readonly GameplayNetworkRuntime _runtime;
        private readonly Dictionary<uint, IWeaponFireEndpoint> _fireEndpoints = new();

        private bool _initialized;

        public WeaponReplication(GameplayNetworkRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public void Initialize()
        {
            if (_initialized) 
                return;
            //注册信道，信道收到消息传给HandleFireCommand并触发
            if (_runtime.Transport.IsServer)
                _runtime.Transport.RegisterHandler(WeaponMessageNames.FireCommand, HandleFireCommand);

            _initialized = true;
        }

        public void Shutdown()
        {
            if(!_initialized) 
                return;

            if(_runtime.Transport.IsServer)
                _runtime.Transport.UnregisterHandler(WeaponMessageNames.FireCommand);

            _fireEndpoints.Clear();
            _initialized = false;
        }

        public void RegisterFireEndpoint(IWeaponFireEndpoint endpoint)
        {
            if(endpoint == null) 
                throw new ArgumentNullException(nameof(endpoint));

            _fireEndpoints[endpoint.OwnerEntityId] = endpoint;
        }

        public void UnregisterFireEndpoint(uint ownerEntityId)
        {
            _fireEndpoints.Remove(ownerEntityId);
        }

        public void SendFireCommand(in FireCommand command)
        {
            if(!_runtime.Transport.IsClient) 
                return;

            using FastBufferWriter writer = new FastBufferWriter(
                WriterInitialCapacity,
                Allocator.Temp,
                WriterMaxCapacity);

            writer.WriteNetworkSerializable(command);

            _runtime.Transport.SendToServer(
                WeaponMessageNames.FireCommand,
                writer,
                NetworkDeliveryClass.Command);
        }

        private void HandleFireCommand(ulong senderClientId, FastBufferReader reader)
        {
            if(!_runtime.Transport.IsServer) 
                return;

            try
            {
                reader.ReadNetworkSerializable(out FireCommand command);

                if(!_fireEndpoints.TryGetValue(command.OwnerEntityId, out IWeaponFireEndpoint endpoint))
                    return;

                //第一层安全检查：不能假冒其他 Client 的实体。
                if(endpoint.OwnerClientId != senderClientId)
                    return;

                endpoint.ReceiveFireCommand(senderClientId, command);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(WeaponReplication)}] FireCommand 解析失败：{exception.Message}");
            }
        }
    }
}