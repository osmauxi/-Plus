using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"Cinemachine.dll",
		"DOTween.dll",
		"MessagePack.dll",
		"System.Core.dll",
		"System.dll",
		"UniTask.Addressables.dll",
		"UniTask.dll",
		"Unity.Addressables.dll",
		"Unity.Collections.dll",
		"Unity.InputSystem.dll",
		"Unity.Netcode.Runtime.dll",
		"Unity.ResourceManager.dll",
		"UnityEngine.CoreModule.dll",
		"UnityEngine.JSONSerializeModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.AddressablesAsyncExtensions.AsyncOperationHandleConfiguredSource.<>c<object>
	// Cysharp.Threading.Tasks.AddressablesAsyncExtensions.AsyncOperationHandleConfiguredSource<object>
	// Cysharp.Threading.Tasks.AutoResetUniTaskCompletionSource.<>c<object>
	// Cysharp.Threading.Tasks.AutoResetUniTaskCompletionSource<object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<object,UnityEngine.SceneManagement.Scene>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<object,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<object,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<object,UnityEngine.SceneManagement.Scene>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<object,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<object,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.SceneManagement.Scene>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<object>
	// Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<UnityEngine.SceneManagement.Scene>
	// Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<byte>
	// Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<object>
	// Cysharp.Threading.Tasks.ITaskPoolNode<object>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.IUniTaskSource<UnityEngine.SceneManagement.Scene>
	// Cysharp.Threading.Tasks.IUniTaskSource<byte>
	// Cysharp.Threading.Tasks.IUniTaskSource<object>
	// Cysharp.Threading.Tasks.Internal.StatePool<Cysharp.Threading.Tasks.UniTask.Awaiter<object>>
	// Cysharp.Threading.Tasks.Internal.StateTuple<Cysharp.Threading.Tasks.UniTask.Awaiter<object>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.SceneManagement.Scene>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<byte>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<object>
	// Cysharp.Threading.Tasks.UniTask.CanceledResultSource<object>
	// Cysharp.Threading.Tasks.UniTask.ExceptionResultSource<object>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<UnityEngine.SceneManagement.Scene>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<byte>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<object>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<UnityEngine.SceneManagement.Scene>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<byte>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<object>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask<UnityEngine.SceneManagement.Scene>
	// Cysharp.Threading.Tasks.UniTask<byte>
	// Cysharp.Threading.Tasks.UniTask<object>
	// Cysharp.Threading.Tasks.UniTaskCompletionSource<object>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<Cysharp.Threading.Tasks.AsyncUnit>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<UnityEngine.SceneManagement.Scene>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<byte>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<object>
	// Cysharp.Threading.Tasks.UniTaskExtensions.<>c__19<object>
	// Cysharp.Threading.Tasks.UniTaskExtensions.AttachExternalCancellationSource.<RunTask>d__5<object>
	// Cysharp.Threading.Tasks.UniTaskExtensions.AttachExternalCancellationSource<object>
	// DG.Tweening.Core.DOGetter<UnityEngine.Color>
	// DG.Tweening.Core.DOGetter<float>
	// DG.Tweening.Core.DOSetter<UnityEngine.Color>
	// DG.Tweening.Core.DOSetter<float>
	// DelegateList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// DelegateList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// DelegateList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// DelegateList<float>
	// MessagePack.Formatters.IMessagePackFormatter<object>
	// MessagePack.SequenceReader<byte>
	// Nerdbank.Streams.Sequence.SequenceSegment<byte>
	// Nerdbank.Streams.Sequence<byte>
	// System.Action<Cysharp.Threading.Tasks.UniTask>
	// System.Action<GlobalLocalVFXPool.VFXRegistry>
	// System.Action<MapGenerator.VariantConfig>
	// System.Action<MonsterVFXController.PrebakedVFX>
	// System.Action<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Action<ProjectGame.HotFix.Core.Session.PlayerSessionData>
	// System.Action<ProjectGame.HotFix.Gameplay.Events.CameraAimTargetUpdatedEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Events.CameraEffectPlayRequestedEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Events.CameraEffectSetRequestedEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Events.GameStateChangedEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Events.GameplayCameraTargetReleasedEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Events.GameplayCameraTargetRequestedEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Events.GameplayWorldCameraChangedEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Events.NextLevelRequestedEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Action<ProjectGame.HotFix.Gameplay.Map.Flow.PlayerRoomChangedEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Map.Flow.RoomFlowSnapshotAppliedEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Map.Flow.RoomFogChangedEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Map.Flow.RoomStateChangedEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Action<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Action<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Action<ProjectGame.HotFix.Gameplay.Map.Generation.MapRuntimeBuiltEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Map.Generation.MapRuntimeClearingEvent>
	// System.Action<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Action<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Action<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Action<ProjectGame.HotFix.Gameplay.Monsters.MonsterRoomBeginData>
	// System.Action<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Action<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Action<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Action<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Action<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Action<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Action<ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>
	// System.Action<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Action<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Action<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Action<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Action<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Action<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Action<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Action<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Action<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Action<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeState>
	// System.Action<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Action<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Action<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Action<ProjectGame.HotFix.UI.Gameplay.GameplayUIRequest>
	// System.Action<ProjectGame.HotFix.UI.Gameplay.GameplayUITopChangedEvent>
	// System.Action<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Action<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Action<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Action<ProjectGame.HotFix.UI.Lobby.ShowOverviewMessageEvent>
	// System.Action<StatModConfig>
	// System.Action<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Action<System.ValueTuple<object,object>>
	// System.Action<Unity.Netcode.NetworkManager.ConnectionApprovalRequest,object>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,object>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Action<UnityEngine.Vector2Int>
	// System.Action<UnityEngine.Vector3>
	// System.Action<byte,byte>
	// System.Action<byte>
	// System.Action<float,float>
	// System.Action<float>
	// System.Action<int,byte>
	// System.Action<int,float>
	// System.Action<int,int>
	// System.Action<int,object>
	// System.Action<int>
	// System.Action<object,ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectOwnerStatSnapshot>
	// System.Action<object,byte>
	// System.Action<object,object>
	// System.Action<object,ulong>
	// System.Action<object>
	// System.Action<uint,ushort,byte>
	// System.Action<uint>
	// System.Action<ulong,Unity.Netcode.FastBufferReader>
	// System.Action<ulong,byte>
	// System.Action<ulong,ushort,object,int>
	// System.Action<ulong>
	// System.Action<ushort>
	// System.ArraySegment.Enumerator<byte>
	// System.ArraySegment.Enumerator<float>
	// System.ArraySegment.Enumerator<ushort>
	// System.ArraySegment<byte>
	// System.ArraySegment<float>
	// System.ArraySegment<ushort>
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
	// System.ByReference<float>
	// System.ByReference<ushort>
	// System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<object>
	// System.Collections.Concurrent.ConcurrentQueue.Segment<object>
	// System.Collections.Concurrent.ConcurrentQueue<object>
	// System.Collections.Generic.ArraySortHelper<Cysharp.Threading.Tasks.UniTask>
	// System.Collections.Generic.ArraySortHelper<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.ArraySortHelper<MapGenerator.VariantConfig>
	// System.Collections.Generic.ArraySortHelper<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Core.Session.PlayerSessionData>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.Generic.ArraySortHelper<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.Generic.ArraySortHelper<StatModConfig>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.ArraySortHelper<System.ValueTuple<object,object>>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector2Int>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector3>
	// System.Collections.Generic.ArraySortHelper<byte>
	// System.Collections.Generic.ArraySortHelper<float>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.ArraySortHelper<ulong>
	// System.Collections.Generic.ArraySortHelper<ushort>
	// System.Collections.Generic.Comparer<Cysharp.Threading.Tasks.UniTask>
	// System.Collections.Generic.Comparer<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.Comparer<MapGenerator.VariantConfig>
	// System.Collections.Generic.Comparer<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Core.Session.PlayerSessionData>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.Generic.Comparer<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.Generic.Comparer<StatModConfig>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.Comparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.Comparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.Comparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.Comparer<UnityEngine.SceneManagement.Scene>
	// System.Collections.Generic.Comparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.Comparer<UnityEngine.Vector3>
	// System.Collections.Generic.Comparer<byte>
	// System.Collections.Generic.Comparer<float>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Comparer<ulong>
	// System.Collections.Generic.Comparer<ushort>
	// System.Collections.Generic.Dictionary.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,float>
	// System.Collections.Generic.Dictionary.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,object>
	// System.Collections.Generic.Dictionary.Enumerator<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.Dictionary.Enumerator<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.Enumerator<byte,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.Dictionary.Enumerator<int,ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.Dictionary.Enumerator<int,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.Enumerator<int,byte>
	// System.Collections.Generic.Dictionary.Enumerator<int,float>
	// System.Collections.Generic.Dictionary.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,ushort>
	// System.Collections.Generic.Dictionary.Enumerator<object,ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.Dictionary.Enumerator<object,ProjectGame.HotFix.Gameplay.Network.NetworkMessageStats>
	// System.Collections.Generic.Dictionary.Enumerator<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Dictionary.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,ushort>
	// System.Collections.Generic.Dictionary.Enumerator<uint,object>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSyncTransport.EndpointEntry>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,UnityEngine.Vector3>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,byte>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,int>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.Enumerator<ushort,ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>
	// System.Collections.Generic.Dictionary.Enumerator<ushort,byte>
	// System.Collections.Generic.Dictionary.Enumerator<ushort,float>
	// System.Collections.Generic.Dictionary.Enumerator<ushort,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<byte,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,byte>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,ushort>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,ProjectGame.HotFix.Gameplay.Network.NetworkMessageStats>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,ushort>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<uint,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSyncTransport.EndpointEntry>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,UnityEngine.Vector3>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,byte>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ushort,ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ushort,byte>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ushort,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ushort,object>
	// System.Collections.Generic.Dictionary.KeyCollection<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,float>
	// System.Collections.Generic.Dictionary.KeyCollection<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,object>
	// System.Collections.Generic.Dictionary.KeyCollection<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.Dictionary.KeyCollection<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<byte,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.Dictionary.KeyCollection<int,ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.Dictionary.KeyCollection<int,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.KeyCollection<int,byte>
	// System.Collections.Generic.Dictionary.KeyCollection<int,float>
	// System.Collections.Generic.Dictionary.KeyCollection<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,ushort>
	// System.Collections.Generic.Dictionary.KeyCollection<object,ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.Dictionary.KeyCollection<object,ProjectGame.HotFix.Gameplay.Network.NetworkMessageStats>
	// System.Collections.Generic.Dictionary.KeyCollection<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Dictionary.KeyCollection<object,float>
	// System.Collections.Generic.Dictionary.KeyCollection<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,ushort>
	// System.Collections.Generic.Dictionary.KeyCollection<uint,object>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSyncTransport.EndpointEntry>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,UnityEngine.Vector3>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,byte>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,int>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,object>
	// System.Collections.Generic.Dictionary.KeyCollection<ushort,ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>
	// System.Collections.Generic.Dictionary.KeyCollection<ushort,byte>
	// System.Collections.Generic.Dictionary.KeyCollection<ushort,float>
	// System.Collections.Generic.Dictionary.KeyCollection<ushort,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<byte,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,byte>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,ushort>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,ProjectGame.HotFix.Gameplay.Network.NetworkMessageStats>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,ushort>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<uint,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSyncTransport.EndpointEntry>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,UnityEngine.Vector3>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,byte>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ushort,ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ushort,byte>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ushort,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ushort,object>
	// System.Collections.Generic.Dictionary.ValueCollection<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,float>
	// System.Collections.Generic.Dictionary.ValueCollection<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,object>
	// System.Collections.Generic.Dictionary.ValueCollection<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.Dictionary.ValueCollection<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<byte,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.Dictionary.ValueCollection<int,ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.Dictionary.ValueCollection<int,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.ValueCollection<int,byte>
	// System.Collections.Generic.Dictionary.ValueCollection<int,float>
	// System.Collections.Generic.Dictionary.ValueCollection<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,ushort>
	// System.Collections.Generic.Dictionary.ValueCollection<object,ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.Dictionary.ValueCollection<object,ProjectGame.HotFix.Gameplay.Network.NetworkMessageStats>
	// System.Collections.Generic.Dictionary.ValueCollection<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Dictionary.ValueCollection<object,float>
	// System.Collections.Generic.Dictionary.ValueCollection<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,ushort>
	// System.Collections.Generic.Dictionary.ValueCollection<uint,object>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSyncTransport.EndpointEntry>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,UnityEngine.Vector3>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,byte>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,int>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,object>
	// System.Collections.Generic.Dictionary.ValueCollection<ushort,ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>
	// System.Collections.Generic.Dictionary.ValueCollection<ushort,byte>
	// System.Collections.Generic.Dictionary.ValueCollection<ushort,float>
	// System.Collections.Generic.Dictionary.ValueCollection<ushort,object>
	// System.Collections.Generic.Dictionary<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,float>
	// System.Collections.Generic.Dictionary<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,object>
	// System.Collections.Generic.Dictionary<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.Dictionary<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary<byte,object>
	// System.Collections.Generic.Dictionary<int,ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.Dictionary<int,ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.Dictionary<int,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary<int,byte>
	// System.Collections.Generic.Dictionary<int,float>
	// System.Collections.Generic.Dictionary<int,int>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<int,ushort>
	// System.Collections.Generic.Dictionary<object,ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.Dictionary<object,ProjectGame.HotFix.Gameplay.Network.NetworkMessageStats>
	// System.Collections.Generic.Dictionary<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Dictionary<object,float>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary<object,ushort>
	// System.Collections.Generic.Dictionary<uint,object>
	// System.Collections.Generic.Dictionary<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>
	// System.Collections.Generic.Dictionary<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSyncTransport.EndpointEntry>
	// System.Collections.Generic.Dictionary<ulong,ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.Dictionary<ulong,UnityEngine.Vector3>
	// System.Collections.Generic.Dictionary<ulong,byte>
	// System.Collections.Generic.Dictionary<ulong,int>
	// System.Collections.Generic.Dictionary<ulong,object>
	// System.Collections.Generic.Dictionary<ushort,ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>
	// System.Collections.Generic.Dictionary<ushort,byte>
	// System.Collections.Generic.Dictionary<ushort,float>
	// System.Collections.Generic.Dictionary<ushort,object>
	// System.Collections.Generic.EqualityComparer<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.EqualityComparer<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.EqualityComparer<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.EqualityComparer<ProjectGame.HotFix.Gameplay.Network.NetworkMessageStats>
	// System.Collections.Generic.EqualityComparer<ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>
	// System.Collections.Generic.EqualityComparer<ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSyncTransport.EndpointEntry>
	// System.Collections.Generic.EqualityComparer<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.EqualityComparer<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.EqualityComparer<ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.EqualityComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.EqualityComparer<UnityEngine.SceneManagement.Scene>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector2>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector3>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<float>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.EqualityComparer<uint>
	// System.Collections.Generic.EqualityComparer<ulong>
	// System.Collections.Generic.EqualityComparer<ushort>
	// System.Collections.Generic.HashSet.Enumerator<UnityEngine.Vector2Int>
	// System.Collections.Generic.HashSet.Enumerator<byte>
	// System.Collections.Generic.HashSet.Enumerator<int>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet.Enumerator<ulong>
	// System.Collections.Generic.HashSet.Enumerator<ushort>
	// System.Collections.Generic.HashSet<UnityEngine.Vector2Int>
	// System.Collections.Generic.HashSet<byte>
	// System.Collections.Generic.HashSet<int>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSet<ulong>
	// System.Collections.Generic.HashSet<ushort>
	// System.Collections.Generic.HashSetEqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.HashSetEqualityComparer<byte>
	// System.Collections.Generic.HashSetEqualityComparer<int>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.HashSetEqualityComparer<ulong>
	// System.Collections.Generic.HashSetEqualityComparer<ushort>
	// System.Collections.Generic.ICollection<Cysharp.Threading.Tasks.UniTask>
	// System.Collections.Generic.ICollection<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.ICollection<MapGenerator.VariantConfig>
	// System.Collections.Generic.ICollection<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.Generic.ICollection<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.Generic.ICollection<StatModConfig>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<byte,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,UnityEngine.Vector2>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,ProjectGame.HotFix.Gameplay.Network.NetworkMessageStats>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSyncTransport.EndpointEntry>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,ProjectGame.HotFix.Gameplay.Weapon.ShotContext>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,UnityEngine.Vector3>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,byte>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ushort,ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ushort,byte>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ushort,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ushort,object>>
	// System.Collections.Generic.ICollection<System.ValueTuple<object,object>>
	// System.Collections.Generic.ICollection<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.ICollection<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.ICollection<UnityEngine.Vector2Int>
	// System.Collections.Generic.ICollection<UnityEngine.Vector3>
	// System.Collections.Generic.ICollection<byte>
	// System.Collections.Generic.ICollection<float>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.ICollection<ulong>
	// System.Collections.Generic.ICollection<ushort>
	// System.Collections.Generic.IComparer<Cysharp.Threading.Tasks.UniTask>
	// System.Collections.Generic.IComparer<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.IComparer<MapGenerator.VariantConfig>
	// System.Collections.Generic.IComparer<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Core.Session.PlayerSessionData>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.Generic.IComparer<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.Generic.IComparer<StatModConfig>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.IComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.IComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.IComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.IComparer<UnityEngine.Vector3>
	// System.Collections.Generic.IComparer<byte>
	// System.Collections.Generic.IComparer<float>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IComparer<ulong>
	// System.Collections.Generic.IComparer<ushort>
	// System.Collections.Generic.IDictionary<int,object>
	// System.Collections.Generic.IEnumerable<Cysharp.Threading.Tasks.UniTask>
	// System.Collections.Generic.IEnumerable<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.IEnumerable<MapGenerator.VariantConfig>
	// System.Collections.Generic.IEnumerable<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.Generic.IEnumerable<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.Generic.IEnumerable<StatModConfig>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<byte,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,UnityEngine.Vector2>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,ProjectGame.HotFix.Gameplay.Network.NetworkMessageStats>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSyncTransport.EndpointEntry>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,ProjectGame.HotFix.Gameplay.Weapon.ShotContext>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,UnityEngine.Vector3>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,byte>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ushort,ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ushort,byte>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ushort,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ushort,object>>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<object,object>>
	// System.Collections.Generic.IEnumerable<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.IEnumerable<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector3>
	// System.Collections.Generic.IEnumerable<byte>
	// System.Collections.Generic.IEnumerable<float>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerable<ulong>
	// System.Collections.Generic.IEnumerable<ushort>
	// System.Collections.Generic.IEnumerator<Cysharp.Threading.Tasks.UniTask>
	// System.Collections.Generic.IEnumerator<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.IEnumerator<MapGenerator.VariantConfig>
	// System.Collections.Generic.IEnumerator<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.Generic.IEnumerator<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.Generic.IEnumerator<StatModConfig>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<byte,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,UnityEngine.Vector2>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,ushort>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,ProjectGame.HotFix.Gameplay.Network.NetworkMessageStats>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSyncTransport.EndpointEntry>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,ProjectGame.HotFix.Gameplay.Weapon.ShotContext>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,UnityEngine.Vector3>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,byte>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ushort,ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ushort,byte>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ushort,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ushort,object>>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<object,object>>
	// System.Collections.Generic.IEnumerator<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.IEnumerator<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector3>
	// System.Collections.Generic.IEnumerator<byte>
	// System.Collections.Generic.IEnumerator<float>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEnumerator<ulong>
	// System.Collections.Generic.IEnumerator<ushort>
	// System.Collections.Generic.IEqualityComparer<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.IEqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEqualityComparer<byte>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IEqualityComparer<uint>
	// System.Collections.Generic.IEqualityComparer<ulong>
	// System.Collections.Generic.IEqualityComparer<ushort>
	// System.Collections.Generic.IList<Cysharp.Threading.Tasks.UniTask>
	// System.Collections.Generic.IList<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.IList<MapGenerator.VariantConfig>
	// System.Collections.Generic.IList<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Collections.Generic.IList<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.Generic.IList<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Collections.Generic.IList<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.Generic.IList<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Collections.Generic.IList<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.Generic.IList<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.Generic.IList<StatModConfig>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IList<System.ValueTuple<object,object>>
	// System.Collections.Generic.IList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.IList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.IList<UnityEngine.Vector2Int>
	// System.Collections.Generic.IList<UnityEngine.Vector3>
	// System.Collections.Generic.IList<byte>
	// System.Collections.Generic.IList<float>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.IList<ulong>
	// System.Collections.Generic.IList<ushort>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Core.Session.PlayerSessionData>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Monsters.MonsterPackedState>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.Settings.InputBindingDefinition>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.Generic.IReadOnlyCollection<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.Generic.IReadOnlyCollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IReadOnlyCollection<byte>
	// System.Collections.Generic.IReadOnlyCollection<int>
	// System.Collections.Generic.IReadOnlyCollection<object>
	// System.Collections.Generic.IReadOnlyCollection<ulong>
	// System.Collections.Generic.IReadOnlyCollection<ushort>
	// System.Collections.Generic.IReadOnlyDictionary<int,object>
	// System.Collections.Generic.IReadOnlyDictionary<int,ushort>
	// System.Collections.Generic.IReadOnlyDictionary<ulong,object>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Core.Session.PlayerSessionData>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Monsters.MonsterPackedState>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.Settings.InputBindingDefinition>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.Generic.IReadOnlyList<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.Generic.IReadOnlyList<byte>
	// System.Collections.Generic.IReadOnlyList<int>
	// System.Collections.Generic.IReadOnlyList<object>
	// System.Collections.Generic.IReadOnlyList<ulong>
	// System.Collections.Generic.IReadOnlyList<ushort>
	// System.Collections.Generic.KeyValuePair<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,float>
	// System.Collections.Generic.KeyValuePair<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey,object>
	// System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.KeyValuePair<byte,object>
	// System.Collections.Generic.KeyValuePair<int,ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.KeyValuePair<int,ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.KeyValuePair<int,UnityEngine.Vector2>
	// System.Collections.Generic.KeyValuePair<int,byte>
	// System.Collections.Generic.KeyValuePair<int,float>
	// System.Collections.Generic.KeyValuePair<int,int>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<int,ushort>
	// System.Collections.Generic.KeyValuePair<object,ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.KeyValuePair<object,ProjectGame.HotFix.Gameplay.Network.NetworkMessageStats>
	// System.Collections.Generic.KeyValuePair<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.KeyValuePair<object,float>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.KeyValuePair<object,ushort>
	// System.Collections.Generic.KeyValuePair<uint,object>
	// System.Collections.Generic.KeyValuePair<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>
	// System.Collections.Generic.KeyValuePair<ulong,ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSyncTransport.EndpointEntry>
	// System.Collections.Generic.KeyValuePair<ulong,ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.KeyValuePair<ulong,UnityEngine.Vector3>
	// System.Collections.Generic.KeyValuePair<ulong,byte>
	// System.Collections.Generic.KeyValuePair<ulong,int>
	// System.Collections.Generic.KeyValuePair<ulong,object>
	// System.Collections.Generic.KeyValuePair<ushort,ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>
	// System.Collections.Generic.KeyValuePair<ushort,byte>
	// System.Collections.Generic.KeyValuePair<ushort,float>
	// System.Collections.Generic.KeyValuePair<ushort,object>
	// System.Collections.Generic.LinkedList.Enumerator<object>
	// System.Collections.Generic.LinkedList<object>
	// System.Collections.Generic.LinkedListNode<object>
	// System.Collections.Generic.List.Enumerator<Cysharp.Threading.Tasks.UniTask>
	// System.Collections.Generic.List.Enumerator<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.List.Enumerator<MapGenerator.VariantConfig>
	// System.Collections.Generic.List.Enumerator<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.Generic.List.Enumerator<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.Generic.List.Enumerator<StatModConfig>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.List.Enumerator<System.ValueTuple<object,object>>
	// System.Collections.Generic.List.Enumerator<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.List.Enumerator<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector2Int>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector3>
	// System.Collections.Generic.List.Enumerator<byte>
	// System.Collections.Generic.List.Enumerator<float>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List.Enumerator<ulong>
	// System.Collections.Generic.List.Enumerator<ushort>
	// System.Collections.Generic.List<Cysharp.Threading.Tasks.UniTask>
	// System.Collections.Generic.List<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.List<MapGenerator.VariantConfig>
	// System.Collections.Generic.List<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.List<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Collections.Generic.List<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.Generic.List<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Collections.Generic.List<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.Generic.List<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Collections.Generic.List<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.Generic.List<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.Generic.List<StatModConfig>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.List<System.ValueTuple<object,object>>
	// System.Collections.Generic.List<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.List<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.List<UnityEngine.Vector2Int>
	// System.Collections.Generic.List<UnityEngine.Vector3>
	// System.Collections.Generic.List<byte>
	// System.Collections.Generic.List<float>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.List<ulong>
	// System.Collections.Generic.List<ushort>
	// System.Collections.Generic.ObjectComparer<Cysharp.Threading.Tasks.UniTask>
	// System.Collections.Generic.ObjectComparer<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.Generic.ObjectComparer<MapGenerator.VariantConfig>
	// System.Collections.Generic.ObjectComparer<MonsterVFXController.PrebakedVFX>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Core.Session.PlayerSessionData>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.Generic.ObjectComparer<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.Generic.ObjectComparer<StatModConfig>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.ObjectComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.ObjectComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.ObjectComparer<UnityEngine.SceneManagement.Scene>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector3>
	// System.Collections.Generic.ObjectComparer<byte>
	// System.Collections.Generic.ObjectComparer<float>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectComparer<ulong>
	// System.Collections.Generic.ObjectComparer<ushort>
	// System.Collections.Generic.ObjectEqualityComparer<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Collections.Generic.ObjectEqualityComparer<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.Generic.ObjectEqualityComparer<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.Generic.ObjectEqualityComparer<ProjectGame.HotFix.Gameplay.Network.NetworkMessageStats>
	// System.Collections.Generic.ObjectEqualityComparer<ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSimulationState>
	// System.Collections.Generic.ObjectEqualityComparer<ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSyncTransport.EndpointEntry>
	// System.Collections.Generic.ObjectEqualityComparer<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.Generic.ObjectEqualityComparer<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.Generic.ObjectEqualityComparer<ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.SceneManagement.Scene>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector2>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector3>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<float>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<uint>
	// System.Collections.Generic.ObjectEqualityComparer<ulong>
	// System.Collections.Generic.ObjectEqualityComparer<ushort>
	// System.Collections.Generic.Queue.Enumerator<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOfferAuthority.EffectRollRequest>
	// System.Collections.Generic.Queue.Enumerator<int>
	// System.Collections.Generic.Queue.Enumerator<object>
	// System.Collections.Generic.Queue<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOfferAuthority.EffectRollRequest>
	// System.Collections.Generic.Queue<int>
	// System.Collections.Generic.Queue<object>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<Cysharp.Threading.Tasks.UniTask>
	// System.Collections.ObjectModel.ReadOnlyCollection<GlobalLocalVFXPool.VFXRegistry>
	// System.Collections.ObjectModel.ReadOnlyCollection<MapGenerator.VariantConfig>
	// System.Collections.ObjectModel.ReadOnlyCollection<MonsterVFXController.PrebakedVFX>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Collections.ObjectModel.ReadOnlyCollection<StatModConfig>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.ValueTuple<object,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector2Int>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector3>
	// System.Collections.ObjectModel.ReadOnlyCollection<byte>
	// System.Collections.ObjectModel.ReadOnlyCollection<float>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<ulong>
	// System.Collections.ObjectModel.ReadOnlyCollection<ushort>
	// System.Comparison<Cysharp.Threading.Tasks.UniTask>
	// System.Comparison<GlobalLocalVFXPool.VFXRegistry>
	// System.Comparison<MapGenerator.VariantConfig>
	// System.Comparison<MonsterVFXController.PrebakedVFX>
	// System.Comparison<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Comparison<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Comparison<ProjectGame.HotFix.Core.Session.PlayerSessionData>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Comparison<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Comparison<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Comparison<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Comparison<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Comparison<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Comparison<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Comparison<StatModConfig>
	// System.Comparison<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Comparison<System.ValueTuple<object,object>>
	// System.Comparison<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Comparison<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Comparison<UnityEngine.Vector2Int>
	// System.Comparison<UnityEngine.Vector3>
	// System.Comparison<byte>
	// System.Comparison<float>
	// System.Comparison<int>
	// System.Comparison<object>
	// System.Comparison<ulong>
	// System.Comparison<ushort>
	// System.Func<ProjectGame.HotFix.Core.Session.PlayerSessionData,ProjectGame.HotFix.Core.Session.PlayerSessionData>
	// System.Func<System.Collections.Generic.KeyValuePair<object,float>,float>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,object>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,object>,object>
	// System.Func<System.Threading.CancellationToken,Cysharp.Threading.Tasks.UniTask>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Func<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Func<System.ValueTuple<byte,byte>>
	// System.Func<System.ValueTuple<byte,object>>
	// System.Func<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Func<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Func<UnityEngine.SceneManagement.Scene>
	// System.Func<byte>
	// System.Func<int>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Func<object,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Func<object,System.ValueTuple<byte,byte>>
	// System.Func<object,System.ValueTuple<byte,object>>
	// System.Func<object,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Func<object,UnityEngine.SceneManagement.Scene>
	// System.Func<object,byte>
	// System.Func<object,int>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.Func<ulong,byte>
	// System.Func<ulong,object>
	// System.IEquatable<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.IEquatable<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// System.IEquatable<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.IEquatable<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeState>
	// System.IEquatable<ProjectGame.HotFix.UI.Gameplay.HUD.HUDAmmoState>
	// System.IEquatable<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.IEquatable<UnityEngine.Vector2Int>
	// System.IEquatable<byte>
	// System.IEquatable<float>
	// System.IProgress<float>
	// System.Linq.Buffer<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Linq.Buffer<object>
	// System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Linq.Enumerable.Iterator<object>
	// System.Linq.Enumerable.Iterator<ulong>
	// System.Linq.Enumerable.WhereArrayIterator<object>
	// System.Linq.Enumerable.WhereEnumerableIterator<object>
	// System.Linq.Enumerable.WhereListIterator<object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<System.Collections.Generic.KeyValuePair<ulong,object>,object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<ulong,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<System.Collections.Generic.KeyValuePair<ulong,object>,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<ulong,object>
	// System.Linq.Enumerable.WhereSelectListIterator<System.Collections.Generic.KeyValuePair<ulong,object>,object>
	// System.Linq.Enumerable.WhereSelectListIterator<ulong,object>
	// System.Linq.EnumerableSorter<System.Collections.Generic.KeyValuePair<object,float>,float>
	// System.Linq.EnumerableSorter<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Linq.EnumerableSorter<object,int>
	// System.Linq.EnumerableSorter<object>
	// System.Linq.IOrderedEnumerable<object>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<object>
	// System.Linq.OrderedEnumerable<System.Collections.Generic.KeyValuePair<object,float>,float>
	// System.Linq.OrderedEnumerable<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Linq.OrderedEnumerable<object,int>
	// System.Linq.OrderedEnumerable<object>
	// System.Memory<byte>
	// System.Nullable<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// System.Nullable<ProjectGame.HotFix.Gameplay.Player.Sync.PlayerInputCommand>
	// System.Nullable<ProjectGame.HotFix.Gameplay.Weapon.WeaponStatSnapshot>
	// System.Nullable<System.Buffers.ReadOnlySequence<byte>>
	// System.Nullable<byte>
	// System.Nullable<float>
	// System.Nullable<int>
	// System.Nullable<uint>
	// System.Nullable<ulong>
	// System.Predicate<Cysharp.Threading.Tasks.UniTask>
	// System.Predicate<GlobalLocalVFXPool.VFXRegistry>
	// System.Predicate<MapGenerator.VariantConfig>
	// System.Predicate<MonsterVFXController.PrebakedVFX>
	// System.Predicate<ProjectGame.HotFix.Core.Events.LocalEventBus.EventStream.EventSubscriber<object>>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Input.InputManager.ContextRequest>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Map.Generation.GridGraphLayoutStrategy.FrontierEdge>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Map.Generation.MapRoomDefinition>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Map.RuntimeNavigationObstacleSystem.ObstacleState>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackProfile>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Monsters.MonsterRuntimeConfig>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnConfig>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnData>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Monsters.MonsterSpawnPlan>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Navigation.FlowFieldSource>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Navigation.GridPoint>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Spawning.SpawnPose>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollService.Candidate>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Weapon.Presentation.WeaponPresentationService.ProjectileKey>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Weapon.ProjectileImpact>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Weapon.ProjectileSpawn>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Weapon.ProjectileState>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Weapon.ShotContext>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeService.LightningNode>
	// System.Predicate<ProjectGame.HotFix.Gameplay.Weapon.WeaponSpecialVfxEvent>
	// System.Predicate<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>
	// System.Predicate<ProjectGame.HotFix.UI.Gameplay.EffectRoll.EffectRollCardModel>
	// System.Predicate<ProjectGame.HotFix.UI.Gameplay.GameplayUIVisibilityChangedEvent>
	// System.Predicate<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerBinding>
	// System.Predicate<ProjectGame.HotFix.UI.Gameplay.HUD.HUDPlayerState>
	// System.Predicate<StatModConfig>
	// System.Predicate<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Predicate<System.ValueTuple<object,object>>
	// System.Predicate<UnityEngine.InputSystem.InputBinding>
	// System.Predicate<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Predicate<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Predicate<UnityEngine.Vector2Int>
	// System.Predicate<UnityEngine.Vector3>
	// System.Predicate<byte>
	// System.Predicate<float>
	// System.Predicate<int>
	// System.Predicate<object>
	// System.Predicate<ulong>
	// System.Predicate<ushort>
	// System.ReadOnlyMemory<byte>
	// System.ReadOnlySpan.Enumerator<byte>
	// System.ReadOnlySpan.Enumerator<float>
	// System.ReadOnlySpan.Enumerator<ushort>
	// System.ReadOnlySpan<byte>
	// System.ReadOnlySpan<float>
	// System.ReadOnlySpan<ushort>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<UnityEngine.SceneManagement.Scene>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<byte>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<UnityEngine.SceneManagement.Scene>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<byte>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<UnityEngine.SceneManagement.Scene>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<byte>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<UnityEngine.SceneManagement.Scene>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<byte>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<UnityEngine.SceneManagement.Scene>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<byte>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.TaskAwaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Runtime.CompilerServices.TaskAwaiter<UnityEngine.SceneManagement.Scene>
	// System.Runtime.CompilerServices.TaskAwaiter<byte>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<UnityEngine.SceneManagement.Scene>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<byte>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<object>
	// System.Span.Enumerator<byte>
	// System.Span.Enumerator<float>
	// System.Span.Enumerator<ushort>
	// System.Span<byte>
	// System.Span<float>
	// System.Span<ushort>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<UnityEngine.SceneManagement.Scene>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<byte>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.Sources.IValueTaskSource<UnityEngine.SceneManagement.Scene>
	// System.Threading.Tasks.Sources.IValueTaskSource<byte>
	// System.Threading.Tasks.Sources.IValueTaskSource<object>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.Task<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.Task<UnityEngine.SceneManagement.Scene>
	// System.Threading.Tasks.Task<byte>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskCompletionSource<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.TaskCompletionSource<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<UnityEngine.SceneManagement.Scene>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<byte>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.TaskFactory<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.TaskFactory<UnityEngine.SceneManagement.Scene>
	// System.Threading.Tasks.TaskFactory<byte>
	// System.Threading.Tasks.TaskFactory<object>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<UnityEngine.SceneManagement.Scene>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<byte>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<object>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<UnityEngine.SceneManagement.Scene>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<byte>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<object>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.ValueTask<UnityEngine.SceneManagement.Scene>
	// System.Threading.Tasks.ValueTask<byte>
	// System.Threading.Tasks.ValueTask<object>
	// System.ValueTuple<UnityEngine.Vector2Int,UnityEngine.Vector3,float,object>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>>
	// System.ValueTuple<byte,System.ValueTuple<byte,byte>>
	// System.ValueTuple<byte,System.ValueTuple<byte,object>>
	// System.ValueTuple<byte,UnityEngine.SceneManagement.Scene>
	// System.ValueTuple<byte,byte>
	// System.ValueTuple<byte,object>
	// System.ValueTuple<object,object>
	// Unity.Collections.IIndexable<byte>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList.ParallelReader<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList.ParallelReader<Unity.Netcode.NetworkListEvent<ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList.ParallelReader<Unity.Netcode.NetworkListEvent<ushort>>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList.ParallelReader<ushort>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList.ParallelWriter<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList.ParallelWriter<Unity.Netcode.NetworkListEvent<ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList.ParallelWriter<Unity.Netcode.NetworkListEvent<ushort>>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList.ParallelWriter<ushort>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList.ReadOnly<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList.ReadOnly<Unity.Netcode.NetworkListEvent<ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList.ReadOnly<Unity.Netcode.NetworkListEvent<ushort>>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList.ReadOnly<ushort>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList<Unity.Netcode.NetworkListEvent<ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList<Unity.Netcode.NetworkListEvent<ushort>>
	// Unity.Collections.LowLevel.Unsafe.UnsafeList<ushort>
	// Unity.Collections.NativeArray.Enumerator<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Collections.NativeArray.Enumerator<Unity.Netcode.NetworkListEvent<ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// Unity.Collections.NativeArray.Enumerator<Unity.Netcode.NetworkListEvent<ushort>>
	// Unity.Collections.NativeArray.Enumerator<ushort>
	// Unity.Collections.NativeArray.ReadOnly.Enumerator<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Collections.NativeArray.ReadOnly.Enumerator<Unity.Netcode.NetworkListEvent<ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// Unity.Collections.NativeArray.ReadOnly.Enumerator<Unity.Netcode.NetworkListEvent<ushort>>
	// Unity.Collections.NativeArray.ReadOnly.Enumerator<ushort>
	// Unity.Collections.NativeArray.ReadOnly<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Collections.NativeArray.ReadOnly<Unity.Netcode.NetworkListEvent<ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// Unity.Collections.NativeArray.ReadOnly<Unity.Netcode.NetworkListEvent<ushort>>
	// Unity.Collections.NativeArray.ReadOnly<ushort>
	// Unity.Collections.NativeArray<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Collections.NativeArray<Unity.Netcode.NetworkListEvent<ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// Unity.Collections.NativeArray<Unity.Netcode.NetworkListEvent<ushort>>
	// Unity.Collections.NativeArray<ushort>
	// Unity.Collections.NativeList.ParallelWriter<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Collections.NativeList.ParallelWriter<Unity.Netcode.NetworkListEvent<ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// Unity.Collections.NativeList.ParallelWriter<Unity.Netcode.NetworkListEvent<ushort>>
	// Unity.Collections.NativeList.ParallelWriter<ushort>
	// Unity.Collections.NativeList<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Collections.NativeList<Unity.Netcode.NetworkListEvent<ProjectGame.HotFix.Core.Network.LobbyPlayerState>>
	// Unity.Collections.NativeList<Unity.Netcode.NetworkListEvent<ushort>>
	// Unity.Collections.NativeList<ushort>
	// Unity.Netcode.BufferSerializer<Unity.Netcode.BufferSerializerReader>
	// Unity.Netcode.BufferSerializer<Unity.Netcode.BufferSerializerWriter>
	// Unity.Netcode.BufferSerializer<object>
	// Unity.Netcode.FallbackSerializer<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Netcode.FallbackSerializer<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.FallbackSerializer<UnityEngine.Vector2Int>
	// Unity.Netcode.FallbackSerializer<byte>
	// Unity.Netcode.FallbackSerializer<float>
	// Unity.Netcode.FallbackSerializer<int>
	// Unity.Netcode.FallbackSerializer<ushort>
	// Unity.Netcode.INetworkVariableSerializer<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Netcode.INetworkVariableSerializer<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.INetworkVariableSerializer<UnityEngine.Vector2Int>
	// Unity.Netcode.INetworkVariableSerializer<byte>
	// Unity.Netcode.INetworkVariableSerializer<float>
	// Unity.Netcode.INetworkVariableSerializer<int>
	// Unity.Netcode.INetworkVariableSerializer<ushort>
	// Unity.Netcode.NetworkList.OnListChangedDelegate<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Netcode.NetworkList.OnListChangedDelegate<ushort>
	// Unity.Netcode.NetworkList<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Netcode.NetworkList<ushort>
	// Unity.Netcode.NetworkVariable.CheckExceedsDirtinessThresholdDelegate<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.NetworkVariable.CheckExceedsDirtinessThresholdDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.NetworkVariable.CheckExceedsDirtinessThresholdDelegate<byte>
	// Unity.Netcode.NetworkVariable.CheckExceedsDirtinessThresholdDelegate<float>
	// Unity.Netcode.NetworkVariable.CheckExceedsDirtinessThresholdDelegate<int>
	// Unity.Netcode.NetworkVariable.OnValueChangedDelegate<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.NetworkVariable.OnValueChangedDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.NetworkVariable.OnValueChangedDelegate<byte>
	// Unity.Netcode.NetworkVariable.OnValueChangedDelegate<float>
	// Unity.Netcode.NetworkVariable.OnValueChangedDelegate<int>
	// Unity.Netcode.NetworkVariable<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.NetworkVariable<UnityEngine.Vector2Int>
	// Unity.Netcode.NetworkVariable<byte>
	// Unity.Netcode.NetworkVariable<float>
	// Unity.Netcode.NetworkVariable<int>
	// Unity.Netcode.NetworkVariableSerialization.EqualsDelegate<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Netcode.NetworkVariableSerialization.EqualsDelegate<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.NetworkVariableSerialization.EqualsDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.NetworkVariableSerialization.EqualsDelegate<byte>
	// Unity.Netcode.NetworkVariableSerialization.EqualsDelegate<float>
	// Unity.Netcode.NetworkVariableSerialization.EqualsDelegate<int>
	// Unity.Netcode.NetworkVariableSerialization<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Netcode.NetworkVariableSerialization<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.NetworkVariableSerialization<UnityEngine.Vector2Int>
	// Unity.Netcode.NetworkVariableSerialization<byte>
	// Unity.Netcode.NetworkVariableSerialization<float>
	// Unity.Netcode.NetworkVariableSerialization<int>
	// Unity.Netcode.NetworkVariableSerialization<ushort>
	// Unity.Netcode.UnmanagedNetworkSerializableSerializer<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Netcode.UnmanagedNetworkSerializableSerializer<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.UnmanagedTypeSerializer<UnityEngine.Vector2Int>
	// Unity.Netcode.UnmanagedTypeSerializer<byte>
	// Unity.Netcode.UnmanagedTypeSerializer<float>
	// Unity.Netcode.UnmanagedTypeSerializer<int>
	// Unity.Netcode.UserNetworkVariableSerialization.DuplicateValueDelegate<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Netcode.UserNetworkVariableSerialization.DuplicateValueDelegate<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.UserNetworkVariableSerialization.DuplicateValueDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.UserNetworkVariableSerialization.DuplicateValueDelegate<byte>
	// Unity.Netcode.UserNetworkVariableSerialization.DuplicateValueDelegate<float>
	// Unity.Netcode.UserNetworkVariableSerialization.DuplicateValueDelegate<int>
	// Unity.Netcode.UserNetworkVariableSerialization.DuplicateValueDelegate<ushort>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadDeltaDelegate<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadDeltaDelegate<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadDeltaDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadDeltaDelegate<byte>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadDeltaDelegate<float>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadDeltaDelegate<int>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadDeltaDelegate<ushort>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadValueDelegate<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadValueDelegate<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadValueDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadValueDelegate<byte>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadValueDelegate<float>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadValueDelegate<int>
	// Unity.Netcode.UserNetworkVariableSerialization.ReadValueDelegate<ushort>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteDeltaDelegate<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteDeltaDelegate<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteDeltaDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteDeltaDelegate<byte>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteDeltaDelegate<float>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteDeltaDelegate<int>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteDeltaDelegate<ushort>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteValueDelegate<ProjectGame.HotFix.Core.Network.LobbyPlayerState>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteValueDelegate<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteValueDelegate<UnityEngine.Vector2Int>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteValueDelegate<byte>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteValueDelegate<float>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteValueDelegate<int>
	// Unity.Netcode.UserNetworkVariableSerialization.WriteValueDelegate<ushort>
	// UnityEngine.AddressableAssets.AddressablesImpl.<>c__DisplayClass79_0<object>
	// UnityEngine.AddressableAssets.AddressablesImpl.<>c__DisplayClass88_0<object>
	// UnityEngine.AddressableAssets.AddressablesImpl.<>c__DisplayClass91_0<object>
	// UnityEngine.Events.InvokableCall<byte>
	// UnityEngine.Events.InvokableCall<float>
	// UnityEngine.Events.InvokableCall<object>
	// UnityEngine.Events.UnityAction<GamePlayStartStruct>
	// UnityEngine.Events.UnityAction<byte>
	// UnityEngine.Events.UnityAction<float>
	// UnityEngine.Events.UnityAction<object,object,object>
	// UnityEngine.Events.UnityAction<object,object>
	// UnityEngine.Events.UnityAction<object>
	// UnityEngine.Events.UnityEvent<byte>
	// UnityEngine.Events.UnityEvent<float>
	// UnityEngine.Events.UnityEvent<object>
	// UnityEngine.InputSystem.InputBindingComposite<UnityEngine.Vector2>
	// UnityEngine.InputSystem.InputBindingComposite<float>
	// UnityEngine.InputSystem.InputControl<UnityEngine.Vector2>
	// UnityEngine.InputSystem.InputControl<float>
	// UnityEngine.InputSystem.InputProcessor<UnityEngine.Vector2>
	// UnityEngine.InputSystem.InputProcessor<float>
	// UnityEngine.InputSystem.Utilities.InlinedArray<object>
	// UnityEngine.InputSystem.Utilities.ReadOnlyArray.Enumerator<UnityEngine.InputSystem.InputBinding>
	// UnityEngine.InputSystem.Utilities.ReadOnlyArray<UnityEngine.InputSystem.InputBinding>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass60_0<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass60_0<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass61_0<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass61_0<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.<>c<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.<>c<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>
	// UnityEngine.ResourceManagement.ChainOperationTypelessDepedency<object>
	// UnityEngine.ResourceManagement.ResourceManager.<>c__DisplayClass100_0<object>
	// UnityEngine.ResourceManagement.ResourceManager.CompletedOperation<object>
	// UnityEngine.ResourceManagement.Util.GlobalLinkedListNodeCache<object>
	// UnityEngine.ResourceManagement.Util.LinkedListNodeCache<object>
	// }}

	public void RefMethods()
	{
		// object Cinemachine.CinemachineVirtualCamera.AddCinemachineComponent<object>()
		// object Cinemachine.CinemachineVirtualCamera.GetCinemachineComponent<object>()
		// Cysharp.Threading.Tasks.UniTask<object> Cysharp.Threading.Tasks.AddressablesAsyncExtensions.ToUniTask<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>,System.IProgress<float>,Cysharp.Threading.Tasks.PlayerLoopTiming,System.Threading.CancellationToken,bool,bool)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,object>(Cysharp.Threading.Tasks.UniTask.Awaiter&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.SceneManagement.Scene>,object>(Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.SceneManagement.Scene>&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,object>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,object>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,object>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.SceneManagement.Scene>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,object>(Cysharp.Threading.Tasks.UniTask.Awaiter&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,object>(Cysharp.Threading.Tasks.UniTask.Awaiter&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,object>(Cysharp.Threading.Tasks.UniTask.Awaiter&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,object>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<object>(object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.SceneManagement.Scene>.Start<object>(object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.Start<object>(object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<object>(object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,object>(Cysharp.Threading.Tasks.UniTask.Awaiter&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.SceneManagement.Scene>,object>(Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.SceneManagement.Scene>&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,object>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,object>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,object&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<object>(object&)
		// Cysharp.Threading.Tasks.Internal.StateTuple<Cysharp.Threading.Tasks.UniTask.Awaiter<object>> Cysharp.Threading.Tasks.Internal.StateTuple.Create<Cysharp.Threading.Tasks.UniTask.Awaiter<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>)
		// Cysharp.Threading.Tasks.UniTask<object> Cysharp.Threading.Tasks.UniTask.FromCanceled<object>(System.Threading.CancellationToken)
		// Cysharp.Threading.Tasks.UniTask<object> Cysharp.Threading.Tasks.UniTask.FromException<object>(System.Exception)
		// Cysharp.Threading.Tasks.UniTask<object> Cysharp.Threading.Tasks.UniTask.FromResult<object>(object)
		// Cysharp.Threading.Tasks.UniTask<object> Cysharp.Threading.Tasks.UniTaskExtensions.AttachExternalCancellation<object>(Cysharp.Threading.Tasks.UniTask<object>,System.Threading.CancellationToken)
		// System.Void Cysharp.Threading.Tasks.UniTaskExtensions.Forget<object>(Cysharp.Threading.Tasks.UniTask<object>)
		// object DG.Tweening.TweenSettingsExtensions.OnComplete<object>(object,DG.Tweening.TweenCallback)
		// object DG.Tweening.TweenSettingsExtensions.SetDelay<object>(object,float)
		// object DG.Tweening.TweenSettingsExtensions.SetEase<object>(object,DG.Tweening.Ease)
		// object DG.Tweening.TweenSettingsExtensions.SetLink<object>(object,UnityEngine.GameObject)
		// object DG.Tweening.TweenSettingsExtensions.SetLoops<object>(object,int,DG.Tweening.LoopType)
		// object DG.Tweening.TweenSettingsExtensions.SetUpdate<object>(object,bool)
		// MessagePack.Formatters.IMessagePackFormatter<object> MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<object>(MessagePack.IFormatterResolver)
		// MessagePack.Formatters.IMessagePackFormatter<object> MessagePack.IFormatterResolver.GetFormatter<object>()
		// object MessagePack.MessagePackSerializer.Deserialize<object>(MessagePack.MessagePackReader&,MessagePack.MessagePackSerializerOptions)
		// object MessagePack.MessagePackSerializer.Deserialize<object>(System.ReadOnlyMemory<byte>,MessagePack.MessagePackSerializerOptions,System.Threading.CancellationToken)
		// ProjectGame.HotFix.Core.Network.LobbyPlayerState System.Activator.CreateInstance<ProjectGame.HotFix.Core.Network.LobbyPlayerState>()
		// ProjectGame.HotFix.Gameplay.Map.View.MapBuildPlan System.Activator.CreateInstance<ProjectGame.HotFix.Gameplay.Map.View.MapBuildPlan>()
		// ProjectGame.HotFix.Gameplay.Player.Sync.PlayerInputCommand System.Activator.CreateInstance<ProjectGame.HotFix.Gameplay.Player.Sync.PlayerInputCommand>()
		// ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSnapshotPacket System.Activator.CreateInstance<ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSnapshotPacket>()
		// Unity.Collections.FixedString32Bytes System.Activator.CreateInstance<Unity.Collections.FixedString32Bytes>()
		// object System.Activator.CreateInstance<object>()
		// System.Collections.ObjectModel.ReadOnlyCollection<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption> System.Array.AsReadOnly<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>(ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption[])
		// ProjectGame.HotFix.Core.Session.PlayerSessionData[] System.Array.Empty<ProjectGame.HotFix.Core.Session.PlayerSessionData>()
		// ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition[] System.Array.Empty<ProjectGame.HotFix.Gameplay.Map.Generation.MapConnectionDefinition>()
		// ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition[] System.Array.Empty<ProjectGame.HotFix.Gameplay.Map.View.MapRoomBuildDefinition>()
		// ProjectGame.HotFix.Gameplay.Weapon.EffectSnapshot[] System.Array.Empty<ProjectGame.HotFix.Gameplay.Weapon.EffectSnapshot>()
		// ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption[] System.Array.Empty<ProjectGame.HotFix.Gameplay.Weapon.Effects.EffectRollOption>()
		// ProjectGame.HotFix.SceneFlow.PhysicalSceneReference[] System.Array.Empty<ProjectGame.HotFix.SceneFlow.PhysicalSceneReference>()
		// UnityEngine.ParticleSystem.Burst[] System.Array.Empty<UnityEngine.ParticleSystem.Burst>()
		// UnityEngine.Vector2[] System.Array.Empty<UnityEngine.Vector2>()
		// byte[] System.Array.Empty<byte>()
		// float[] System.Array.Empty<float>()
		// int[] System.Array.Empty<int>()
		// object[] System.Array.Empty<object>()
		// ulong[] System.Array.Empty<ulong>()
		// ushort[] System.Array.Empty<ushort>()
		// System.Void System.Array.Fill<int>(int[],int)
		// System.Void System.Array.Fill<int>(int[],int,int,int)
		// int System.Array.IndexOf<object>(object[],object)
		// int System.Array.IndexOfImpl<object>(object[],object,int,int)
		// System.Void System.Array.Resize<ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackData>(ProjectGame.HotFix.Gameplay.Monsters.MonsterAttackData[]&,int)
		// System.Void System.Array.Resize<ProjectGame.HotFix.Gameplay.Monsters.MonsterHealthData>(ProjectGame.HotFix.Gameplay.Monsters.MonsterHealthData[]&,int)
		// System.Void System.Array.Resize<ProjectGame.HotFix.Gameplay.Monsters.MonsterMetaData>(ProjectGame.HotFix.Gameplay.Monsters.MonsterMetaData[]&,int)
		// System.Void System.Array.Resize<ProjectGame.HotFix.Gameplay.Monsters.MonsterMotionData>(ProjectGame.HotFix.Gameplay.Monsters.MonsterMotionData[]&,int)
		// System.Void System.Array.Resize<ProjectGame.HotFix.Gameplay.Monsters.MonsterPresentationData>(ProjectGame.HotFix.Gameplay.Monsters.MonsterPresentationData[]&,int)
		// System.Void System.Array.Resize<ProjectGame.HotFix.Gameplay.Monsters.MonsterStatusData>(ProjectGame.HotFix.Gameplay.Monsters.MonsterStatusData[]&,int)
		// System.Void System.Array.Resize<ProjectGame.HotFix.Gameplay.Navigation.MultiSourceFlowField.HeapNode>(ProjectGame.HotFix.Gameplay.Navigation.MultiSourceFlowField.HeapNode[]&,int)
		// System.Void System.Array.Resize<UnityEngine.Vector2>(UnityEngine.Vector2[]&,int)
		// System.Void System.Array.Resize<byte>(byte[]&,int)
		// System.Void System.Array.Resize<float>(float[]&,int)
		// System.Void System.Array.Resize<int>(int[]&,int)
		// System.Void System.Array.Resize<object>(object[]&,int)
		// System.Void System.Array.Resize<ushort>(ushort[]&,int)
		// System.Void System.Array.Sort<ProjectGame.HotFix.Core.Network.LobbyPlayerState>(ProjectGame.HotFix.Core.Network.LobbyPlayerState[],System.Comparison<ProjectGame.HotFix.Core.Network.LobbyPlayerState>)
		// System.Void System.Array.Sort<ProjectGame.HotFix.Core.Session.PlayerSessionData>(ProjectGame.HotFix.Core.Session.PlayerSessionData[],System.Comparison<ProjectGame.HotFix.Core.Session.PlayerSessionData>)
		// System.Void System.Array.Sort<int>(int[])
		// System.Void System.Array.Sort<int>(int[],int,int,System.Collections.Generic.IComparer<int>)
		// System.Void System.Array.Sort<ulong>(ulong[])
		// System.Void System.Array.Sort<ulong>(ulong[],int,int,System.Collections.Generic.IComparer<ulong>)
		// int System.HashCode.Combine<ulong,uint>(ulong,uint)
		// bool System.Linq.Enumerable.All<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// bool System.Linq.Enumerable.Any<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.KeyValuePair<object,float> System.Linq.Enumerable.First<System.Collections.Generic.KeyValuePair<object,float>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,float>>)
		// object System.Linq.Enumerable.First<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderBy<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<object,float>> System.Linq.Enumerable.OrderByDescending<System.Collections.Generic.KeyValuePair<object,float>,float>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,float>>,System.Func<System.Collections.Generic.KeyValuePair<object,float>,float>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderByDescending<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<System.Collections.Generic.KeyValuePair<ulong,object>,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,object>>,System.Func<System.Collections.Generic.KeyValuePair<ulong,object>,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<ulong,object>(System.Collections.Generic.IEnumerable<ulong>,System.Func<ulong,object>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.ThenBy<object,int>(System.Linq.IOrderedEnumerable<object>,System.Func<object,int>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.ThenByDescending<object,int>(System.Linq.IOrderedEnumerable<object>,System.Func<object,int>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<ulong,object>>.Select<object>(System.Func<System.Collections.Generic.KeyValuePair<ulong,object>,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<ulong>.Select<object>(System.Func<ulong,object>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.IOrderedEnumerable<object>.CreateOrderedEnumerable<int>(System.Func<object,int>,System.Collections.Generic.IComparer<int>,bool)
		// System.Span<byte> System.MemoryExtensions.AsSpan<byte>(byte[],int,int)
		// object System.Reflection.CustomAttributeExtensions.GetCustomAttribute<object>(System.Reflection.MemberInfo)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,object>(Cysharp.Threading.Tasks.UniTask.Awaiter&,object&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,object>(System.Runtime.CompilerServices.TaskAwaiter<object>&,object&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<object>(object&)
		// object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
		// System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
		// System.Void* Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf<UnityEngine.Vector2>(UnityEngine.Vector2&)
		// System.Void* Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf<byte>(byte&)
		// System.Void* Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf<float>(float&)
		// System.Void* Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf<int>(int&)
		// int Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<UnityEngine.Vector2>()
		// int Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<float>()
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<ProjectGame.HotFix.Gameplay.Player.Stamina.PlayerStaminaState>(ProjectGame.HotFix.Gameplay.Player.Stamina.PlayerStaminaState&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<ProjectGame.HotFix.Gameplay.Player.State.PlayerActionRuntimeState>(ProjectGame.HotFix.Gameplay.Player.State.PlayerActionRuntimeState&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<ProjectGame.HotFix.Gameplay.Player.State.PlayerControlState>(ProjectGame.HotFix.Gameplay.Player.State.PlayerControlState&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeState>(ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeState&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<Unity.Collections.FixedString32Bytes>(Unity.Collections.FixedString32Bytes&,Unity.Netcode.FastBufferWriter.ForFixedStrings)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<Unity.Collections.FixedString64Bytes>(Unity.Collections.FixedString64Bytes&,Unity.Netcode.FastBufferWriter.ForFixedStrings)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<byte>(byte&,Unity.Netcode.FastBufferWriter.ForEnums)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<byte>(byte&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<float>(float&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<int>(int&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<uint>(uint&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<ulong>(ulong&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<ushort>(ushort&,Unity.Netcode.FastBufferWriter.ForEnums)
		// System.Void Unity.Netcode.BufferSerializer<object>.SerializeValue<ushort>(ushort&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferReader.ReadNetworkSerializable<ProjectGame.HotFix.Core.Network.LobbyPlayerState>(ProjectGame.HotFix.Core.Network.LobbyPlayerState&)
		// System.Void Unity.Netcode.FastBufferReader.ReadNetworkSerializable<ProjectGame.HotFix.Core.Network.LobbyPlayerState>(ProjectGame.HotFix.Core.Network.LobbyPlayerState[]&)
		// System.Void Unity.Netcode.FastBufferReader.ReadNetworkSerializable<ProjectGame.HotFix.Gameplay.Map.View.MapBuildPlan>(ProjectGame.HotFix.Gameplay.Map.View.MapBuildPlan&)
		// System.Void Unity.Netcode.FastBufferReader.ReadNetworkSerializable<ProjectGame.HotFix.Gameplay.Player.Sync.PlayerInputCommand>(ProjectGame.HotFix.Gameplay.Player.Sync.PlayerInputCommand&)
		// System.Void Unity.Netcode.FastBufferReader.ReadNetworkSerializable<ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSnapshotPacket>(ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSnapshotPacket&)
		// System.Void Unity.Netcode.FastBufferReader.ReadNetworkSerializable<object>(object&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<byte>(byte&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<byte>(byte[]&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<float>(float&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<int>(int&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<int>(int[]&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<uint>(uint&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<ulong>(ulong&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<ulong>(ulong[]&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<ushort>(ushort&)
		// System.Void Unity.Netcode.FastBufferReader.ReadUnmanagedSafe<ushort>(ushort[]&)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<ProjectGame.HotFix.Core.Network.LobbyPlayerState>(ProjectGame.HotFix.Core.Network.LobbyPlayerState[]&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<ProjectGame.HotFix.Gameplay.Map.View.MapBuildPlan>(ProjectGame.HotFix.Gameplay.Map.View.MapBuildPlan&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<Unity.Collections.FixedString32Bytes>(Unity.Collections.FixedString32Bytes&,Unity.Netcode.FastBufferWriter.ForFixedStrings)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<byte>(byte&,Unity.Netcode.FastBufferWriter.ForEnums)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<byte>(byte&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<byte>(byte[]&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<float>(float&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<int>(int&,Unity.Netcode.FastBufferWriter.ForEnums)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<int>(int&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<int>(int[]&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<object>(object&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<uint>(uint&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<ulong>(ulong&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<ulong>(ulong[]&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<ushort>(ushort&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferReader.ReadValueSafe<ushort>(ushort[]&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferWriter.WriteNetworkSerializable<ProjectGame.HotFix.Core.Network.LobbyPlayerState>(ProjectGame.HotFix.Core.Network.LobbyPlayerState&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteNetworkSerializable<ProjectGame.HotFix.Core.Network.LobbyPlayerState>(ProjectGame.HotFix.Core.Network.LobbyPlayerState[],int,int)
		// System.Void Unity.Netcode.FastBufferWriter.WriteNetworkSerializable<ProjectGame.HotFix.Gameplay.Map.View.MapBuildPlan>(ProjectGame.HotFix.Gameplay.Map.View.MapBuildPlan&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteNetworkSerializable<ProjectGame.HotFix.Gameplay.Player.Sync.PlayerInputCommand>(ProjectGame.HotFix.Gameplay.Player.Sync.PlayerInputCommand&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteNetworkSerializable<ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSnapshotPacket>(ProjectGame.HotFix.Gameplay.Player.Sync.PlayerSnapshotPacket&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteNetworkSerializable<object>(object&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanaged<int>(int&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<byte>(byte&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<byte>(byte[])
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<float>(float&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<int>(int&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<int>(int[])
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<uint>(uint&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<ulong>(ulong&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<ulong>(ulong[])
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<ushort>(ushort&)
		// System.Void Unity.Netcode.FastBufferWriter.WriteUnmanagedSafe<ushort>(ushort[])
		// System.Void Unity.Netcode.FastBufferWriter.WriteValue<Unity.Collections.FixedString32Bytes>(Unity.Collections.FixedString32Bytes&,Unity.Netcode.FastBufferWriter.ForFixedStrings)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<ProjectGame.HotFix.Core.Network.LobbyPlayerState>(ProjectGame.HotFix.Core.Network.LobbyPlayerState[],Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<ProjectGame.HotFix.Gameplay.Map.View.MapBuildPlan>(ProjectGame.HotFix.Gameplay.Map.View.MapBuildPlan&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<Unity.Collections.FixedString32Bytes>(Unity.Collections.FixedString32Bytes&,Unity.Netcode.FastBufferWriter.ForFixedStrings)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<byte>(byte&,Unity.Netcode.FastBufferWriter.ForEnums)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<byte>(byte&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<byte>(byte[],Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<float>(float&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<int>(int&,Unity.Netcode.FastBufferWriter.ForEnums)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<int>(int&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<int>(int[],Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<object>(object&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<uint>(uint&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<ulong>(ulong&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<ulong>(ulong[],Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<ushort>(ushort&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.FastBufferWriter.WriteValueSafe<ushort>(ushort[],Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.INetworkSerializable.NetworkSerialize<Unity.Netcode.BufferSerializerReader>(Unity.Netcode.BufferSerializer<Unity.Netcode.BufferSerializerReader>)
		// System.Void Unity.Netcode.INetworkSerializable.NetworkSerialize<Unity.Netcode.BufferSerializerWriter>(Unity.Netcode.BufferSerializer<Unity.Netcode.BufferSerializerWriter>)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<ProjectGame.HotFix.Gameplay.Player.Stamina.PlayerStaminaState>(ProjectGame.HotFix.Gameplay.Player.Stamina.PlayerStaminaState&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<ProjectGame.HotFix.Gameplay.Player.State.PlayerActionRuntimeState>(ProjectGame.HotFix.Gameplay.Player.State.PlayerActionRuntimeState&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<ProjectGame.HotFix.Gameplay.Player.State.PlayerControlState>(ProjectGame.HotFix.Gameplay.Player.State.PlayerControlState&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeState>(ProjectGame.HotFix.Gameplay.Weapon.WeaponRuntimeState&,Unity.Netcode.FastBufferWriter.ForNetworkSerializable)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<Unity.Collections.FixedString32Bytes>(Unity.Collections.FixedString32Bytes&,Unity.Netcode.FastBufferWriter.ForFixedStrings)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<Unity.Collections.FixedString64Bytes>(Unity.Collections.FixedString64Bytes&,Unity.Netcode.FastBufferWriter.ForFixedStrings)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<byte>(byte&,Unity.Netcode.FastBufferWriter.ForEnums)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<byte>(byte&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<float>(float&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<int>(int&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<uint>(uint&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<ulong>(ulong&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<ushort>(ushort&,Unity.Netcode.FastBufferWriter.ForEnums)
		// System.Void Unity.Netcode.IReaderWriter.SerializeValue<ushort>(ushort&,Unity.Netcode.FastBufferWriter.ForPrimitives)
		// bool Unity.Netcode.NetworkVariableSerialization<ProjectGame.HotFix.Core.Network.LobbyPlayerState>.EqualityEquals<ProjectGame.HotFix.Core.Network.LobbyPlayerState>(ProjectGame.HotFix.Core.Network.LobbyPlayerState&,ProjectGame.HotFix.Core.Network.LobbyPlayerState&)
		// bool Unity.Netcode.NetworkVariableSerialization<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>.EqualityEquals<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>(ProjectGame.HotFix.Gameplay.Player.PlayerHealthState&,ProjectGame.HotFix.Gameplay.Player.PlayerHealthState&)
		// bool Unity.Netcode.NetworkVariableSerialization<UnityEngine.Vector2Int>.EqualityEquals<UnityEngine.Vector2Int>(UnityEngine.Vector2Int&,UnityEngine.Vector2Int&)
		// bool Unity.Netcode.NetworkVariableSerialization<byte>.EqualityEquals<byte>(byte&,byte&)
		// bool Unity.Netcode.NetworkVariableSerialization<float>.EqualityEquals<float>(float&,float&)
		// bool Unity.Netcode.NetworkVariableSerialization<byte>.ValueEquals<byte>(byte&,byte&)
		// bool Unity.Netcode.NetworkVariableSerialization<int>.ValueEquals<int>(int&,int&)
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<ProjectGame.HotFix.Core.Network.LobbyPlayerState>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<UnityEngine.Vector2Int>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<byte>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<float>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedValueEquals<byte>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedValueEquals<int>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<UnityEngine.Vector2Int>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<byte>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<float>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<int>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedINetworkSerializable<ProjectGame.HotFix.Core.Network.LobbyPlayerState>()
		// System.Void Unity.Netcode.NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedINetworkSerializable<ProjectGame.HotFix.Gameplay.Player.PlayerHealthState>()
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<object>(object)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<System.Collections.Generic.IList<object>> UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<object>(object,System.Action<object>)
		// System.Void UnityEngine.AddressableAssets.Addressables.Release<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>)
		// System.Void UnityEngine.AddressableAssets.Addressables.Release<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.AddressablesImpl.LoadAssetAsync<object>(object)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.AddressablesImpl.LoadAssetWithChain<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,object)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<System.Collections.Generic.IList<object>> UnityEngine.AddressableAssets.AddressablesImpl.LoadAssetsAsync<object>(System.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>,System.Action<object>,bool)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<System.Collections.Generic.IList<object>> UnityEngine.AddressableAssets.AddressablesImpl.LoadAssetsAsync<object>(object,System.Action<object>,bool)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<System.Collections.Generic.IList<object>> UnityEngine.AddressableAssets.AddressablesImpl.LoadAssetsWithChain<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,System.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>,System.Action<object>,bool)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<System.Collections.Generic.IList<object>> UnityEngine.AddressableAssets.AddressablesImpl.LoadAssetsWithChain<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,object,System.Action<object>,bool)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.AddressablesImpl.TrackHandle<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.AssetReference.LoadAssetAsync<object>()
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.Component.GetComponentInChildren<object>()
		// object UnityEngine.Component.GetComponentInChildren<object>(bool)
		// object UnityEngine.Component.GetComponentInParent<object>()
		// object UnityEngine.Component.GetComponentInParent<object>(bool)
		// object[] UnityEngine.Component.GetComponents<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.Component.TryGetComponent<object>(object&)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>(bool)
		// object UnityEngine.GameObject.GetComponentInParent<object>()
		// object UnityEngine.GameObject.GetComponentInParent<object>(bool)
		// object[] UnityEngine.GameObject.GetComponents<object>()
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>()
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.GameObject.TryGetComponent<object>(object&)
		// UnityEngine.Vector2 UnityEngine.InputSystem.InputAction.ReadValue<UnityEngine.Vector2>()
		// float UnityEngine.InputSystem.InputAction.ReadValue<float>()
		// UnityEngine.Vector2 UnityEngine.InputSystem.InputActionState.ApplyProcessors<UnityEngine.Vector2>(int,UnityEngine.Vector2,UnityEngine.InputSystem.InputControl<UnityEngine.Vector2>)
		// float UnityEngine.InputSystem.InputActionState.ApplyProcessors<float>(int,float,UnityEngine.InputSystem.InputControl<float>)
		// UnityEngine.Vector2 UnityEngine.InputSystem.InputActionState.ReadValue<UnityEngine.Vector2>(int,int,bool)
		// float UnityEngine.InputSystem.InputActionState.ReadValue<float>(int,int,bool)
		// object UnityEngine.JsonUtility.FromJson<object>(string)
		// object UnityEngine.Object.FindObjectOfType<object>(bool)
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Vector3,UnityEngine.Quaternion)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.Convert<object>()
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.CreateChainOperation<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,System.Func<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.CreateChainOperation<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,System.Func<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>,bool)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.CreateCompletedOperation<object>(object,string)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.CreateCompletedOperationInternal<object>(object,bool,System.Exception,bool)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.CreateCompletedOperationWithException<object>(object,System.Exception)
		// object UnityEngine.ResourceManagement.ResourceManager.CreateOperation<object>(System.Type,int,UnityEngine.ResourceManagement.Util.IOperationCacheKey,System.Action<UnityEngine.ResourceManagement.AsyncOperations.IAsyncOperation>)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.ProvideResource<object>(UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<System.Collections.Generic.IList<object>> UnityEngine.ResourceManagement.ResourceManager.ProvideResources<object>(System.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>,bool,System.Action<object>)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.StartOperation<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase<object>,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle)
		// string string.Join<ulong>(string,System.Collections.Generic.IEnumerable<ulong>)
		// string string.JoinCore<ulong>(System.Char*,int,System.Collections.Generic.IEnumerable<ulong>)
	}
}