using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

//使用结构体作为事件数据载体，因为结构体是值类型，分配在栈上，用完直接删，不会有GC开销
//RPC方法不支持运行时泛型，写出来就要写明类型，这结构又是基于结构体的，100个事件就要写100个RPC，所以这里不做使用
public class NetEventCenter : NetworkBehaviour
{
    public static NetEventCenter Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    public override void OnNetworkSpawn()
    {
        //网络底层方法，RegisterNamedMessageHandler注册NetEvent消息处理器，
        //收到消息时直接传输原始的二进制数据流(FastBufferReader)，并调用HandleIncomingPacket处理
        //使用这个因为
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("NetEvent", HandleIncomingPacket);
        InitializeEventTypes();
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.CustomMessagingManager != null)
        {
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler("NetEvent");
        }
        _messageHandlers.Clear();
    }

    //泛型委托缓存机制，旨在解决泛型解析漏斗ReceiveInternal调用的装箱问题和反射调用的性能问题，反射调用会有较大的性能开销
    //我们用一个字典在游戏开始时缓存每个事件类型对应的委托，这样在处理收到的事件时，就可以直接从字典中获取对应的委托来调用，而不需要每次都通过反射来获取方法信息和创建委托。
    //Key是事件的唯一ID 
    //Value是一个处理函数：接收发送者ID和字节流读取器
    private Dictionary<ushort, Action<ulong, FastBufferReader>> _messageHandlers = new Dictionary<ushort, Action<ulong, FastBufferReader>>();

    //Type存结构体的类型，object存对应的委托(Action<T>)，因为结构体可以存多个值，所以只需要单泛型方法
    //不同委托因为委托名不同，获取出来是没法放一起的，所以用object存储，所有物体都是object的子类，在后续需要使用的时候再拆箱，强制类型转换进行使用
    private Dictionary<Type, object> _handlers = new Dictionary<Type, object>();
    //Type，object作为标准的泛型事件总线的存储结构，装箱拆箱的性能开销并不大，高级C#这一块

    //结构限制了即使无参数的方法也要有空结构体来表示，保底传值一个方法位置，我们对此进行优化
    //将每个方法分配ID，传值时保底只传输一个ulong ID值，让网络传输进一步加快，拿到ID再从这个字典里找就行了
    //字典找值是o(1)，找键是o（n），为了极致的处理速度，我们直接存两个字典，保证找键和值都是o（1）
    private Dictionary<Type, ushort> _typeToId = new Dictionary<Type, ushort>();
    private Dictionary<ushort, Type> _idToType = new Dictionary<ushort, Type>();
    private void InitializeEventTypes()
    {
        _typeToId.Clear();
        _idToType.Clear();

        //使用反射，扫描当前程序集里所有实现了 INetEvent 的结构体
        var allEventTypes = new List<Type>();
        //AppDomain：游戏运行时的整个内存空间。
        //GetAssemblies: 拿到所有 DLL（Unity引擎、系统库、自己的代码...都在这里面）
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var targetAssemblies = new HashSet<string>
        {
            "Assembly-CSharp",      //没有划分 Asmdef 的默认代码
            "CoreArchitecture",     //底层核心架构
            "GameplayExtensions"    //合作开发的沙盒程序集
        };

        foreach (var assembly in assemblies)
        {
            //可以创建Assembly Definition，可以指定分包(dll)，这样能把遍历过滤范围进一步放大，更精确
            //但是要折腾什么依赖什么的。
            string name = assembly.GetName().Name;
            if (!targetAssemblies.Contains(name))
                continue;
            //获取当前DLL的Type信息，这里就包含接口实现情况
            var types = assembly.GetTypes();
            foreach (var t in types)
            {
                //必须是值类型，且INetEvent类型的变量可以赋值给t，就是说t跟INetEvent有相关
                if (t.IsValueType && typeof(INetEvent).IsAssignableFrom(t))
                {
                    allEventTypes.Add(t);
                }
            }
        }

        //不同主机扫描顺序不一样，这里应该尽可能保证表的一致性，所以进行一次排序，这里根据首字母排序
        allEventTypes.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));

        //分配 ID
        for (ushort i = 0; i < allEventTypes.Count; i++)
        {
            var type = allEventTypes[i];

            _typeToId[type] = i;
            _idToType[i] = type;

            //委托缓存机制：在初始化时就利用反射创建好强类型委托
            var methodInfo = typeof(NetEventCenter).GetMethod(nameof(ReceiveInternal), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var genericMethod = methodInfo.MakeGenericMethod(type);

            //将泛型方法转换为具体的 Action 委托并缓存
            var action = (Action<ulong, FastBufferReader>)Delegate.CreateDelegate(typeof(Action<ulong, FastBufferReader>), this, genericMethod);
            _messageHandlers[i] = action;

            Debug.Log($"[NetEvent] 注册事件 ID: {i} -> {type.Name}");
        }
    }
    public void Subscribe<T>(Action<T, ulong> handler) where T : struct, INetEvent
    {
        //获取类型
        Type type = typeof(T);

        if (!_handlers.ContainsKey(type))
        {
            _handlers[type] = null;
        }
        //_handlers[type]取出所有委托，强制类型转换Action<T>，然后加上新的handler委托
        _handlers[type] = (Action<T, ulong>)_handlers[type] + handler;
    }
    //为什么(Action<T>)_handlers[type]取出委托之后可以进行加减法？这里是加减法的重载。
    //因为委托本质上是一个多播委托，也就是一个列表，可以通过+和-操作符来添加或移除方法引用，被触发时会依次调用列表中的所有方法

    //struct约束T必须是值类型，INetEvent约束T必须实现INetEvent接口
    public void Unsubscribe<T>(Action<T, ulong> handler) where T : struct, INetEvent
    {
        Type type = typeof(T);
        if (_handlers.ContainsKey(type))
        {
            _handlers[type] = (Action<T, ulong>)_handlers[type] - handler;
        }
    }

    //事件触发函数
    public void Send<T>(T data, params ulong[] clientIds) where T : struct, INetEvent
    {
        //如果是Server：直接广播给所有Client，并执行本地逻辑
        if (IsServer)
        {
            if (clientIds != null && clientIds.Length > 0)
            {
                //定向发送
                SendToSpecificClients(data, clientIds);
            }
            else
            {
                //广播给所有人
                SendToAllClients(data);
            }

            if(!IsClient)
                InvokeLocal(data, NetworkManager.ServerClientId);
        }
        //如果是Client：发给Server请求转发
        else if (IsClient)
        {
            if (clientIds != null && clientIds.Length > 0)
            {
                Debug.LogWarning("客户端调用 Send 时传入 clientIds 是无效的！客户端只能发给服务器。路由请在数据包内指定或由服务器决定。");
            }
            SendToServer(data);
            //客户端发送后，通常不立即执行本地，而是等服务器广播回来（保证时序一致）
            //或者如果你需要预测表现，可以在这里 InvokeLocal(data)
        }
    }

    #region 网络底层处理
        private void SendToServer<T>(T data) where T : struct, INetEvent
        {
            //创建一个“快速写入器”
            //1024:初始容量，申请一块 1024 字节的内存条来写数据。
            //Allocator.Temp:告诉 Unity 这块内存是临时的，用完这一帧立马销毁，0GC。
            using (var writer = new FastBufferWriter(1024, Allocator.Temp)) 
            {
                if (!_typeToId.TryGetValue(typeof(T), out ushort eventId))
                {
                    Debug.LogError($"未注册的事件类型: {typeof(T).Name}");
                    return;
                }
                //写入“信封标签”
                ////typeof(T).FullName输出此事件数据结构体的类型信息，也就是所在命名空间 + 结构体名
                //WriteValueSafe: 把字符串转换成二进制写入内存条。
                //也就是告诉接收方，这包数据是什么类型的事件，之后收到二进制数据流之后就按照这个类型来解析。
                writer.WriteValueSafe(eventId);

                //写入“信件内容”
                //因为结构体都实现了INetworkSerializable，直接调用NGO的扩展方法WriteNetworkSerializable就行了
                //这里会调用结构体T里的NetworkSerialize方法把实际数据转为二进制流。
                writer.WriteNetworkSerializable(data);

                //发送数据包
                //NetEvent:频道名，NetEventBus已经在OnNetworkSpawn注册了这个频道的处理器
                //NetworkManager.ServerClientId获取服务器ID，这个方法是发给服务器的。
                //writer:把写好的内存条交出去。
                //所以这里服务器会收到这包数据，然后调用HandleIncomingPacket处理。
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("NetEvent", NetworkManager.ServerClientId, writer);
            }
        }

        private void SendToAllClients<T>(T data) where T : struct, INetEvent
        {
            using (var writer = new FastBufferWriter(1024, Allocator.Temp)) 
            {
                if (!_typeToId.TryGetValue(typeof(T), out ushort eventId))
                {
                    Debug.LogError($"未注册的事件类型: {typeof(T).Name}");
                    return;
                }
                writer.WriteValueSafe(eventId);
                writer.WriteNetworkSerializable(data);

                //SendNamedMessageToAll: 这是一个群发指令。
                //NGO会把这份二进制数据复制N份，发给所有连接的客户端。
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("NetEvent", writer);
            }

        }

        //处理收到的包
        private void HandleIncomingPacket(ulong senderId, FastBufferReader reader)
        {
            //拆“信封标签”
            //第一个写入的内容是ushort的ID，所以第一个读出来的也是ID
            //读完后，reader的指针会向后移动，指向剩下的数据（结构体内容）
            reader.ReadValueSafe(out ushort eventId);

            //直接从缓存字典中获取委托并执行，0GC，无反射开销
            if (_messageHandlers.TryGetValue(eventId, out var handler))
            {
                handler.Invoke(senderId, reader);
                return;
            }
            else 
            {
                Debug.LogError($"收到未知事件或委托未缓存, ID: {eventId}");
            }
            //typeof(NetEventCenter).GetMethod:去NetEventBus类里找一个名字叫"ReceiveInternal"的方法。
            //BindingFlags:告诉它去哪里找（NonPublic = 私有方法也找，Instance = 实例方法）。
            //也就是拿到了ReceiveInternal方法，但是现在还没填泛型参数 T，是空的容器。
            //var method = typeof(NetEventCenter).GetMethod("ReceiveInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            //MakeGenericMethod:用反射给泛型方法补全类型参，让它从抽象的泛型模板变成可直接调用的具体方法
            //填入泛型参数，因为上方已经获取了T的实际类型。
            //等同于：private void ReceiveInternal<PlayerFireEvent>(...)
            //var genericMethod = method.MakeGenericMethod(eventType);
            //调用方法，传入参数
            //genericMethod.Invoke(this, new object[] { senderId, reader });
        }

        //泛型解析漏斗
        public void ReceiveInternal<T>(ulong senderId, FastBufferReader reader) where T : struct, INetEvent
        {
            T data = new T();
            //拆包
            //reader此时的指针正好指在结构体数据的开头（因为刚才FullName已经在HandleIncomingPacket中被读走了）。
            //调用结构体的NetworkSerialize方法进行反序列化，读出值写入data。
            reader.ReadNetworkSerializable(out data);

            //服务器最大，客户端转发到服务器，服务器视作请求，先进行审核
            //服务器转发到客户端，客户端视作命令，直接执行
            if (IsServer)
            {
                if (data.AutoBroadcast)
                {
                    SendToAllClients(data);
                }
                //执行服务器本地逻辑
                InvokeLocal(data, senderId);
            }
            //如果我是客户端，收到了服务器的消息 -> 执行本地逻辑
            else
            {
                InvokeLocal(data, NetworkManager.ServerClientId);
            }
        }

        private void SendToSpecificClients<T>(T data, ulong[] targetIds) where T : struct, INetEvent
        {
            using (var writer = new FastBufferWriter(1024, Allocator.Temp)) 
            {
                if (!_typeToId.TryGetValue(typeof(T), out ushort eventId))
                {
                    Debug.LogError($"未注册的事件类型: {typeof(T).Name}");
                    return;
                }

                writer.WriteValueSafe(eventId);
                writer.WriteNetworkSerializable(data);

                //SendNamedMessage可以接受一个ID列表
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    "NetEvent",
                    targetIds, //这里NGO就会自动处理列表的id进行定向转发
                    writer
                );
            }
        }

        private void InvokeLocal<T>(T data, ulong senderId) where T : struct, INetEvent
        {
            Type type = typeof(T);
            if (_handlers.TryGetValue(type, out var handlerObj) && handlerObj != null)
            {
                //拆箱并调用
                ((Action<T, ulong>)handlerObj).Invoke(data, senderId);
            }
        }
        #endregion

    public static class NetUtils
    {
        //这是一个通用的“过滤器”,直接写了最基础无检测的基础事件
        //如果返回 true，说明这是服务器发来的权威消息，直接执行表现逻辑
        //如果返回 false，说明这是客户端请求，自动尝试转发
        //使用方法，这样就直接省略了自己写无判定的触发方法
        //if (!NetUtils.Filter(evt,senderId,true))
        //{
        //    return;
        //}
        public static bool Filter<T>(T evt, ulong senderId, bool autoForward = false) where T : struct, INetEvent
        {
            //本方法是模拟ServerRPC到server后的ClientRPC过程
            //Filter的返回值旨在判断这次触发是服务器触发的权威方法还是客户端传到服务器的请求
            //是服务器的权威指令，直接True让走后面具体的事件逻辑
            if (senderId == NetworkManager.ServerClientId)
                return true;

            if (NetworkManager.Singleton.IsServer)
            {
                //是客户端发到服务器的请求，且有自动转发的话就让服务器触发一次事件，这次的事件就是权威命令了
                if (autoForward)
                {
                    Instance.Send(evt);
                }
                //因为是客户端的请求，所以怎么都是false不让走具体事件方法
                return false;
            }
            return false;
        }
    }
}