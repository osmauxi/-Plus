using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"AOT_Core.dll",
		"DOTween.dll",
		"MessagePack.dll",
		"System.Core.dll",
		"System.dll",
		"Unity.Addressables.dll",
		"Unity.Netcode.Runtime.dll",
		"Unity.ResourceManager.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// DelegateList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// DelegateList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// DelegateList<float>
	// EventInfo<GamePlayStartStruct>
	// MessagePack.Formatters.IMessagePackFormatter<object>
	// MessagePack.SequenceReader<byte>
	// Nerdbank.Streams.Sequence.SequenceSegment<byte>
	// Nerdbank.Streams.Sequence<byte>
	// System.Action<GamePlayStartStruct,ulong>
	// System.Action<GlobalLocalVFXPool.VFXRegistry>
	// System.Action<MapGenerator.VariantConfig>
	// System.Action<MonsterVFXController.PrebakedVFX>
	// System.Action<StatModConfig>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,object>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Action<byte>
	// System.Action<float,float>
	// System.Action<float>
	// System.Action<int,int>
	// System.Action<object,object>
	// System.Action<object>
	// System.Action<ulong>
	// System.ArraySegment.Enumerator<byte>
	// System.ArraySegment<byte>
	// System.Buffers.ArrayMemoryPool.ArrayMemoryPoolBuffer<byte>
	// System.Buffers.ArrayMemoryPool<byte>
	// System.Buffers.ArrayPool<byte>
	// System.Buffers.ArrayPool<int>
	// System.Buffers.ConfigurableArrayPool.Bucket<byte>
	// System.Buffers.ConfigurableArrayPool.Bucket<int>
	// System.Buffers.ConfigurableArrayPool<byte>
	// System.Buffers.ConfigurableArrayPool<int>
	// System.Buffers.IBufferWriter<byte>
	// System.Buffers.IMemoryOwner<byte>
	// System.Buffers.MemoryManager<byte>
	// System.Buffers.MemoryPool<byte>
	// System.Buffers.ReadOnlySequence.<>c<byte>
	// System.Buffers.ReadOnlySequence.Enumerator<byte>
	// System.Buffers.ReadOnlySequence<byte>
	// System.Buffers.ReadOnlySequenceSegment<byte>
	// System.Buffers.SpanAction<ushort,System.Buffers.ReadOnlySequence<ushort>>
	// System.Buffers.TlsOverPerCoreLockedStacksArrayPool.LockedStack<byte>
	// System.Buffers.TlsOverPerCoreLockedStacksArrayPool.LockedStack<int>
	// System.Buffers.TlsOverPerCoreLockedStacksArrayPool.PerCoreLockedStacks<byte>
	// System.Buffers.TlsOverPerCoreLockedStacksArrayPool.PerCoreLockedStacks<int>
	// System.Buffers.TlsOverPerCoreLockedStacksArrayPool<byte>
	// System.Buffers.TlsOverPerCoreLockedStacksArrayPool<int>
	// System.ByReference<byte>
	// System.Collections.Generic.ArraySortHelper<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.ArraySortHelper<MapGenerator.VariantConfig>
	// System.Collections.Generic.ArraySortHelper<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.ArraySortHelper<StatModConfig>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.ArraySortHelper<float>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.Comparer<MapGenerator.VariantConfig>
	// System.Collections.Generic.Comparer<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.Comparer<StatModConfig>
	// System.Collections.Generic.Comparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.Comparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.Comparer<UnityEngine.Vector3>
	// System.Collections.Generic.Comparer<float>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,ushort>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,ushort>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.KeyCollection<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,float>
	// System.Collections.Generic.Dictionary.KeyCollection<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,ushort>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,ushort>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.ValueCollection<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,float>
	// System.Collections.Generic.Dictionary.ValueCollection<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,ushort>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,object>
	// System.Collections.Generic.Dictionary<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,float>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary<object,ushort>
	// System.Collections.Generic.Dictionary<ulong,object>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector3>
	// System.Collections.Generic.EqualityComparer<float>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.EqualityComparer<ulong>
	// System.Collections.Generic.EqualityComparer<ushort>
	// System.Collections.Generic.HashSet.Enumerator<UnityEngine.Vector2Int>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet<UnityEngine.Vector2Int>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSetEqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.ICollection<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.ICollection<MapGenerator.VariantConfig>
	// System.Collections.Generic.ICollection<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.ICollection<StatModConfig>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.ICollection<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.ICollection<UnityEngine.Vector2Int>
	// System.Collections.Generic.ICollection<float>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.IComparer<MapGenerator.VariantConfig>
	// System.Collections.Generic.IComparer<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.IComparer<StatModConfig>
	// System.Collections.Generic.IComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.IComparer<float>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.IEnumerable<MapGenerator.VariantConfig>
	// System.Collections.Generic.IEnumerable<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.IEnumerable<StatModConfig>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IEnumerable<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEnumerable<float>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.IEnumerator<MapGenerator.VariantConfig>
	// System.Collections.Generic.IEnumerator<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.IEnumerator<StatModConfig>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IEnumerator<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEnumerator<float>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IEqualityComparer<ulong>
	// System.Collections.Generic.IList<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.IList<MapGenerator.VariantConfig>
	// System.Collections.Generic.IList<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.IList<StatModConfig>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.IList<float>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.IReadOnlyCollection<object>
	// System.Collections.Generic.IReadOnlyCollection<ulong>
	// System.Collections.Generic.IReadOnlyList<ulong>
	// System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<object,float>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.KeyValuePair<object,ushort>
	// System.Collections.Generic.KeyValuePair<ulong,object>
	// System.Collections.Generic.LinkedList.Enumerator<object>
	// System.Collections.Generic.LinkedList<object>
	// System.Collections.Generic.LinkedListNode<object>
	// System.Collections.Generic.List.Enumerator<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.List.Enumerator<MapGenerator.VariantConfig>
	// System.Collections.Generic.List.Enumerator<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.List.Enumerator<StatModConfig>
	// System.Collections.Generic.List.Enumerator<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.List.Enumerator<float>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.List<MapGenerator.VariantConfig>
	// System.Collections.Generic.List<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.List<StatModConfig>
	// System.Collections.Generic.List<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.List<float>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.ObjectComparer<MapGenerator.VariantConfig>
	// System.Collections.Generic.ObjectComparer<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.ObjectComparer<StatModConfig>
	// System.Collections.Generic.ObjectComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector3>
	// System.Collections.Generic.ObjectComparer<float>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector3>
	// System.Collections.Generic.ObjectEqualityComparer<float>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<ulong>
	// System.Collections.Generic.ObjectEqualityComparer<ushort>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.ObjectModel.ReadOnlyCollection<MapGenerator.VariantConfig>
	// System.Collections.ObjectModel.ReadOnlyCollection<MonsterVFXController.PrebakedVFX>
	// System.Collections.ObjectModel.ReadOnlyCollection<StatModConfig>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.ObjectModel.ReadOnlyCollection<float>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<GlobalLocalVFXPool.VFXRegistry>
	// System.Comparison<MapGenerator.VariantConfig>
	// System.Comparison<MonsterVFXController.PrebakedVFX>
	// System.Comparison<StatModConfig>
	// System.Comparison<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Comparison<float>
	// System.Comparison<object>
	// System.Func<System.Collections.Generic.KeyValuePair<object,float>,float>
	// System.Func<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Func<byte>
	// System.Func<object,byte>
	// System.Func<object,object>
	// System.Func<object>
	// System.IEquatable<UnityEngine.Vector2Int>
	// System.IEquatable<byte>
	// System.IEquatable<float>
	// System.Linq.Buffer<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Linq.Enumerable.Iterator<object>
	// System.Linq.Enumerable.WhereArrayIterator<object>
	// System.Linq.Enumerable.WhereEnumerableIterator<object>
	// System.Linq.Enumerable.WhereListIterator<object>
	// System.Linq.EnumerableSorter<System.Collections.Generic.KeyValuePair<object,float>,float>
	// System.Linq.EnumerableSorter<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Linq.OrderedEnumerable<System.Collections.Generic.KeyValuePair<object,float>,float>
	// System.Linq.OrderedEnumerable<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Memory<byte>
	// System.Nullable<System.Buffers.ReadOnlySequence<byte>>
	// System.Nullable<int>
	// System.Predicate<GlobalLocalVFXPool.VFXRegistry>
	// System.Predicate<MapGenerator.VariantConfig>
	// System.Predicate<MonsterVFXController.PrebakedVFX>
	// System.Predicate<StatModConfig>
	// System.Predicate<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Predicate<UnityEngine.Vector2Int>
	// System.Predicate<float>
	// System.Predicate<object>
	// System.ReadOnlyMemory<byte>
	// System.ReadOnlySpan<byte>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Span<byte>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskCompletionSource<object>
	// System.Threading.Tasks.TaskFactory<object>
	// System.ValueTuple<UnityEngine.Vector2Int,UnityEngine.Vector3,float,object>
	// Unity.Netcode.BufferSerializer<Unity.Netcode.BufferSerializerWriter>
	// Unity.Netcode.FallbackSerializer<UnityEngine.Vector2Int>
	// Unity.Netcode.FallbackSerializer<byte>
	// Unity.Netcode.FallbackSerializer<float>
	// Unity.Netcode.FallbackSerializer<int>
	// Unity.Netcode.INetworkVariableSerializer<UnityEngine.Vector2Int>
	// Unity.Netcode.INetworkVariableSerializer<byte>
	// Unity.Netcode.INetworkVariableSerializer<float>
	// Unity.Netcode.INetworkVariableSerializer<int>
	// Unity.Netcode.NetworkVariable.CheckExceedsDirtinessThresholdDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.NetworkVariable.CheckExceedsDirtinessThresholdDelegate<byte>
	// Unity.Netcode.NetworkVariable.CheckExceedsDirtinessThresholdDelegate<float>
	// Unity.Netcode.NetworkVariable.CheckExceedsDirtinessThresholdDelegate<int>
	// Unity.Netcode.NetworkVariable.OnValueChangedDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.NetworkVariable.OnValueChangedDelegate<byte>
	// Unity.Netcode.NetworkVariable.OnValueChangedDelegate<float>
	// Unity.Netcode.NetworkVariable.OnValueChangedDelegate<int>
	// Unity.Netcode.NetworkVariable<UnityEngine.Vector2Int>
	// Unity.Netcode.NetworkVariable<byte>
	// Unity.Netcode.NetworkVariable<float>
	// Unity.Netcode.NetworkVariable<int>
	// Unity.Netcode.NetworkVariableSerialization.EqualsDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.NetworkVariableSerialization.EqualsDelegate<byte>
	// Unity.Netcode.NetworkVariableSerialization.EqualsDelegate<float>
	// Unity.Netcode.NetworkVariableSerialization.EqualsDelegate<int>
	// Unity.Netcode.NetworkVariableSerialization<UnityEngine.Vector2Int>
	// Unity.Netcode.NetworkVariableSerialization<byte>
	// Unity.Netcode.NetworkVariableSerialization<float>
	// Unity.Netcode.NetworkVariableSerialization<int>
	// Unity.Netcode.UnmanagedTypeSerializer<UnityEngine.Vector2Int>
	// Unity.Netcode.UnmanagedTypeSerializer<byte>
	// Unity.Netcode.UnmanagedTypeSerializer<float>
	// Unity.Netcode.UnmanagedTypeSerializer<int>
	// Unity.Netcode.UserNetworkVariableSerialization.DuplicateValueDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.UserNetworkVariableSerialization.DuplicateValueDelegate<byte>
	// Unity.Netcode.UserNetworkVariableSerialization.DuplicateValueDelegate<float>
	// Unity.Netcode.UserNetworkVariableSerialization.DuplicateValueDelegate<int>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadDeltaDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadDeltaDelegate<byte>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadDeltaDelegate<float>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadDeltaDelegate<int>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadValueDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadValueDelegate<byte>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadValueDelegate<float>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadValueDelegate<int>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteDeltaDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteDeltaDelegate<byte>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteDeltaDelegate<float>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteDeltaDelegate<int>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteValueDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteValueDelegate<byte>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteValueDelegate<float>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteValueDelegate<int>
	// UnityEngine.AddressableAssets.AddressablesImpl.<>c__DisplayClass79_0<object>
	// UnityEngine.Events.UnityAction<GamePlayStartStruct>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass60_0<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass61_0<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.<>c<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>
	// UnityEngine.ResourceManagement.ChainOperationTypelessDepedency<object>
	// UnityEngine.ResourceManagement.ResourceManager.CompletedOperation<object>
	// UnityEngine.ResourceManagement.Util.GlobalLinkedListNodeCache<object>
	// UnityEngine.ResourceManagement.Util.LinkedListNodeCache<object>
	// }}

	public void RefMethods()
	{
		// object DG.Tweening.TweenSettingsExtensions.OnComplete<object>(object,DG.Tweening.TweenCallback)
		// object DG.Tweening.TweenSettingsExtensions.SetDelay<object>(object,float)
		// object DG.Tweening.TweenSettingsExtensions.SetEase<object>(object,DG.Tweening.Ease)
		// object DG.Tweening.TweenSettingsExtensions.SetLoops<object>(object,int,DG.Tweening.LoopType)
		// System.Void LocalEventCenter.AddEventListener<GamePlayStartStruct>(UnityEngine.Events.UnityAction<GamePlayStartStruct>)
		// System.Void LocalEventCenter.EventTrigger<GamePlayStartStruct>(GamePlayStartStruct)
		// System.Void LocalEventCenter.RemoveEventListener<GamePlayStartStruct>(UnityEngine.Events.UnityAction<GamePlayStartStruct>)
		// MessagePack.Formatters.IMessagePackFormatter<object> MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<object>(MessagePack.IFormatterResolver)
		// MessagePack.Formatters.IMessagePackFormatter<object> MessagePack.IFormatterResolver.GetFormatter<object>()
		// object MessagePack.MessagePackSerializer.Deserialize<object>(MessagePack.MessagePackReader&,MessagePack.MessagePackSerializerOptions)
		// object MessagePack.MessagePackSerializer.Deserialize<object>(System.ReadOnlyMemory<byte>,MessagePack.MessagePackSerializerOptions,System.Threading.CancellationToken)
		// System.Void NetEventCenter.InvokeLocal<GamePlayStartStruct>(GamePlayStartStruct,ulong)
		// System.Void NetEventCenter.Send<GamePlayStartStruct>(GamePlayStartStruct,ulong[])
		// System.Void NetEventCenter.SendToAllClients<GamePlayStartStruct>(GamePlayStartStruct)
		// System.Void NetEventCenter.SendToServer<GamePlayStartStruct>(GamePlayStartStruct)
		// System.Void NetEventCenter.SendToSpecificClients<GamePlayStartStruct>(GamePlayStartStruct,ulong[])
		// System.Void NetEventCenter.Subscribe<GamePlayStartStruct>(System.Action<GamePlayStartStruct,ulong>)
		// System.Void NetEventCenter.Unsubscribe<GamePlayStartStruct>(System.Action<GamePlayStartStruct,ulong>)
		// bool NetEventCenter.NetUtils.Filter<GamePlayStartStruct>(GamePlayStartStruct,ulong,bool)
		// ulong[] System.Array.Empty<ulong>()
		// System.Collections.Generic.KeyValuePair<object,float> System.Linq.Enumerable.First<System.Collections.Generic.KeyValuePair<object,float>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,float>>)
		// object System.Linq.Enumerable.FirstOrDefault<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<object,float>> System.Linq.Enumerable.OrderByDescending<System.Collections.Generic.KeyValuePair<object,float>,float>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,float>>,System.Func<System.Collections.Generic.KeyValuePair<object,float>,float>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ProjectGame.HotFix.HotFixEntry.<TestLoadExcelData>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ProjectGame.HotFix.HotFixEntry.<TestLoadExcelData>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<ProjectGame.HotFix.HotFixEntry.<TestLoadExcelData>d__2>(ProjectGame.HotFix.HotFixEntry.<TestLoadExcelData>d__2&)
		// object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
		// System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
		// System.Void* Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf<int>(int&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<byte>(byte&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<float>(float&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<int>(int&)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<byte>(byte&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<float>(float&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<int>(int&,Unity.Netcode.FastBufferWriter.ForEnums)
		// System.Void Unity.Netcode.FastBufferWriter.WriteNetworkSerializable<GamePlayStartStruct>(GamePlayStartStruct&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<byte>(byte&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<float>(float&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<int>(int&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<ushort>(ushort&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<byte>(byte&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<float>(float&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<int>(int&,Unity.Netcode.FastBufferWriter.ForEnums)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<ushort>(ushort&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.INetworkSerializable.NetworkSerialize<Unity.Netcode.BufferSerializerWriter>(Unity.Netcode.BufferSerializer<Unity.Netcode.BufferSerializerWriter>)
		// bool Unity.Netcode.NetworkVariableSerialization<UnityEngine.Vector2Int>.EqualityEquals<UnityEngine.Vector2Int>(UnityEngine.Vector2Int&,UnityEngine.Vector2Int&)
		// bool Unity.Netcode.NetworkVariableSerialization<byte>.EqualityEquals<byte>(byte&,byte&)
		// bool Unity.Netcode.NetworkVariableSerialization<float>.EqualityEquals<float>(float&,float&)
		// bool Unity.Netcode.NetworkVariableSerialization<int>.ValueEquals<int>(int&,int&)
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<UnityEngine.Vector2Int>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<byte>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<float>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedValueEquals<int>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<UnityEngine.Vector2Int>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<byte>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<float>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<int>()
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<object>(object)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.AddressablesImpl.LoadAssetAsync<object>(object)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.AddressablesImpl.LoadAssetWithChain<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,object)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.AddressablesImpl.TrackHandle<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>)
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.Component.GetComponentInChildren<object>()
		// object UnityEngine.Component.GetComponentInParent<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.Component.TryGetComponent<object>(object&)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>(bool)
		// object UnityEngine.GameObject.GetComponentInParent<object>()
		// object UnityEngine.GameObject.GetComponentInParent<object>(bool)
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>()
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.GameObject.TryGetComponent<object>(object&)
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Vector3,UnityEngine.Quaternion)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.Convert<object>()
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.CreateChainOperation<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,System.Func<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.CreateCompletedOperationInternal<object>(object,bool,System.Exception,bool)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.CreateCompletedOperationWithException<object>(object,System.Exception)
		// object UnityEngine.ResourceManagement.ResourceManager.CreateOperation<object>(System.Type,int,UnityEngine.ResourceManagement.Util.IOperationCacheKey,System.Action<UnityEngine.ResourceManagement.AsyncOperations.IAsyncOperation>)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.ProvideResource<object>(UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.StartOperation<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase<object>,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle)
	}
}