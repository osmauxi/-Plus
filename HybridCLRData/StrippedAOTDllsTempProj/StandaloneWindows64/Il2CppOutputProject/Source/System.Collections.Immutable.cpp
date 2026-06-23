#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>


template <typename R, typename T1>
struct VirtualFuncInvoker1
{
	typedef R (*Func)(void*, T1, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};

struct Dictionary_2_tAC32B254416DD510DC3E7E36B0706A6B031D7A53;
struct Dictionary_2_t954AEC357FE7190582B8255D4ACEF81F5E94A6A5;
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF;
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB;
struct ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263;
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129;
struct ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F;
struct Assembly_t;
struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA;
struct Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235;
struct CollectionBuilderAttribute_t54580B583B741DF2BEBC247A659A1B9D82E47AA2;
struct CultureInfo_t9BA817D41AD55AC8BD07480DD8AC22F8FFA378E0;
struct DoesNotReturnAttribute_t3598B7BBE88E89C0D9315952A9E497BFA5AAC753;
struct DoesNotReturnIfAttribute_t13E8A106B50DA12AA2B960D1CDDE616D3BEEACB9;
struct EmbeddedAttribute_t509CDEBC1DA670B6FAA87BD018F3C93804E6BC5E;
struct Hashtable_tEFC3B6496E6747787D8BB761B51F2AE3A8CFFE2D;
struct IDictionary_t6D03155AF1FA9083817AA5B6AD7DEEACC26AB220;
struct IResourceGroveler_tDEE701BD41E9E5D260606F79F75427B42C4CC0C0;
struct IsReadOnlyAttribute_t6F27DA2971106AA8A8010CC2A85B938621E07352;
struct MaybeNullWhenAttribute_tC05F437DB6EA00FF11A743B16C96D1F7982C9358;
struct MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553;
struct NonVersionableAttribute_tDE71082D419D7BCDB378F56EC6972E9B8B2600DE;
struct NotNullAttribute_tF521CEFFAF06EEF727844ECDDDB79C4F2DBEE4F0;
struct NotNullWhenAttribute_t7928691EA752981A4524F3737ADB1BB825EEC360;
struct NullableAttribute_t0A6C93DEC2D86CD9386617D6D9CACFDBAA6321A0;
struct NullableContextAttribute_tB8DBAF2CA8C952F26A27917D78DAEFAFE4E07132;
struct NullablePublicOnlyAttribute_t8E6C97291B52B88315355E3FEFCE5A8149CCCE91;
struct RefSafetyRulesAttribute_t5F7093546DFFF5023C78FF99DF02AE4E9624CE73;
struct ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB;
struct RuntimeAssembly_tA26A4DE82E77826DFC3D58AD976BCFC6BCA918AF;
struct SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6;
struct String_t;
struct Type_t;
struct Version_tE426DB5655D0F22920AE16A2AA9AB7781B8255A7;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
struct CultureNameResourceSetPair_t06C5772C09CA853E70E42C0E8BC57FE0AA2CB674;

IL2CPP_EXTERN_C RuntimeClass* AppContext_t0380D19FAC72CD59D46947D86DC1DAA3BCE638E0_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ImmutableArray_tED520C4BBA5449AF0B7A43F61250ED55233BF648_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* SecureObjectPool_t47E92FD075F6A0B33D038DF5D5B4B79B61A50C3C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Type_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral07E5C4DBC844DE7F5317C074564E52D67C4D5205;
IL2CPP_EXTERN_C String_t* _stringLiteral141C9904DF9DE9F151E0B8FCF5A8124B46177546;
IL2CPP_EXTERN_C String_t* _stringLiteral1BC0150CC915B8119BC6785445D0E1FCF16BF047;
IL2CPP_EXTERN_C String_t* _stringLiteral6E49800871410E6255741DE232E2006F4FA40D28;
IL2CPP_EXTERN_C String_t* _stringLiteral70079B6AAC8BF895F1E70E28153EE819801DF849;
IL2CPP_EXTERN_C String_t* _stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D;
IL2CPP_EXTERN_C String_t* _stringLiteral81B3823BBC3752A005ED4DC7AC4148D45DDF5ACC;
IL2CPP_EXTERN_C String_t* _stringLiteralA2F18350FA50F3568A798127F78B191252CA4CA3;
IL2CPP_EXTERN_C String_t* _stringLiteralB5528CA07A43AEA4EFA2F7B2DEF38E0A5D87ECD6;
IL2CPP_EXTERN_C const RuntimeMethod* Requires_Argument_mC0D17F52DC37FD36EA0C45C7464CD2965FFF6277_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Requires_FailArgumentNullException_mABEC2B38D4FF1AE27F6594BF5E93194F04EC8EB6_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Requires_FailRange_mDBFD5CC6EB7F02259AB1D488BACC2076DC1CF583_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeType* SR_tCFED80D0C6EF3202EA88CF770D21756283090813_0_0_0_var;
struct Exception_t_marshaled_com;
struct Exception_t_marshaled_pinvoke;

struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct U3CModuleU3E_t4F97576DFF29BFCE305CE461D8B86843F800ADA0 
{
};
struct AllocFreeConcurrentStack_tF356ECBDE0D1D4923914EFDDD52E81144E8C1B73  : public RuntimeObject
{
};
struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA  : public RuntimeObject
{
};
struct ImmutableArray_tED520C4BBA5449AF0B7A43F61250ED55233BF648  : public RuntimeObject
{
};
struct ImmutableDictionary_tADDDD22ED3AFC95882576BBB6CFDE2A3545BA29A  : public RuntimeObject
{
};
struct ImmutableExtensions_t12614C82E46A8D8A1F95544D54AE34A3DBC2B748  : public RuntimeObject
{
};
struct ImmutableHashSet_t4E7FEFA64495FDCE0D3434B87D5A059E4ADF8225  : public RuntimeObject
{
};
struct ImmutableList_t17F0BF227FD86D81157675C24EDB4E80508BD39F  : public RuntimeObject
{
};
struct ImmutableQueue_t165ECBED89BC0F526AC57FFDA32A772EFEEAE1D2  : public RuntimeObject
{
};
struct ImmutableSortedDictionary_t190AA91F6EBFDCB7CF7BC05FF2A474630159008C  : public RuntimeObject
{
};
struct ImmutableSortedSet_t1E50E2933B23B54111C220BD5F74CFCA2FAD9A90  : public RuntimeObject
{
};
struct ImmutableStack_tB4D5761888CB57A38DCD75B80F8C1907DBF2C844  : public RuntimeObject
{
};
struct MemberInfo_t  : public RuntimeObject
{
};
struct Requires_tF331399A847AC9FA61CE588EBB4E5B5F3E487251  : public RuntimeObject
{
};
struct SR_tCFED80D0C6EF3202EA88CF770D21756283090813  : public RuntimeObject
{
};
struct SR_t94F426715F6702F0295747008794B9AC9DF5757F  : public RuntimeObject
{
};
struct SecureObjectPool_t47E92FD075F6A0B33D038DF5D5B4B79B61A50C3C  : public RuntimeObject
{
};
struct String_t  : public RuntimeObject
{
	int32_t ____stringLength;
	Il2CppChar ____firstChar;
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct Byte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3 
{
	uint8_t ___m_value;
};
struct CollectionBuilderAttribute_t54580B583B741DF2BEBC247A659A1B9D82E47AA2  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	Type_t* ___U3CBuilderTypeU3Ek__BackingField;
	String_t* ___U3CMethodNameU3Ek__BackingField;
};
struct DoesNotReturnAttribute_t3598B7BBE88E89C0D9315952A9E497BFA5AAC753  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
};
struct DoesNotReturnIfAttribute_t13E8A106B50DA12AA2B960D1CDDE616D3BEEACB9  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	bool ___U3CParameterValueU3Ek__BackingField;
};
struct EmbeddedAttribute_t509CDEBC1DA670B6FAA87BD018F3C93804E6BC5E  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2  : public ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_pinvoke
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_com
{
};
struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	int32_t ___m_value;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct IsReadOnlyAttribute_t6F27DA2971106AA8A8010CC2A85B938621E07352  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
};
struct MaybeNullWhenAttribute_tC05F437DB6EA00FF11A743B16C96D1F7982C9358  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	bool ___U3CReturnValueU3Ek__BackingField;
};
struct NonVersionableAttribute_tDE71082D419D7BCDB378F56EC6972E9B8B2600DE  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
};
struct NotNullAttribute_tF521CEFFAF06EEF727844ECDDDB79C4F2DBEE4F0  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
};
struct NotNullWhenAttribute_t7928691EA752981A4524F3737ADB1BB825EEC360  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	bool ___U3CReturnValueU3Ek__BackingField;
};
struct NullableAttribute_t0A6C93DEC2D86CD9386617D6D9CACFDBAA6321A0  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___NullableFlags;
};
struct NullableContextAttribute_tB8DBAF2CA8C952F26A27917D78DAEFAFE4E07132  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	uint8_t ___Flag;
};
struct NullablePublicOnlyAttribute_t8E6C97291B52B88315355E3FEFCE5A8149CCCE91  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	bool ___IncludesInternals;
};
struct RefSafetyRulesAttribute_t5F7093546DFFF5023C78FF99DF02AE4E9624CE73  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	int32_t ___Version;
};
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};
struct Exception_t  : public RuntimeObject
{
	String_t* ____className;
	String_t* ____message;
	RuntimeObject* ____data;
	Exception_t* ____innerException;
	String_t* ____helpURL;
	RuntimeObject* ____stackTrace;
	String_t* ____stackTraceString;
	String_t* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	RuntimeObject* ____dynamicMethods;
	int32_t ____HResult;
	String_t* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_pinvoke
{
	char* ____className;
	char* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_pinvoke* ____innerException;
	char* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	char* ____stackTraceString;
	char* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	char* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_com
{
	Il2CppChar* ____className;
	Il2CppChar* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_com* ____innerException;
	Il2CppChar* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	Il2CppChar* ____stackTraceString;
	Il2CppChar* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	Il2CppChar* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B 
{
	intptr_t ___value;
};
struct UltimateResourceFallbackLocation_tFA91547D7BF4CEF1101A7C391ECB7B73EE073AB6 
{
	int32_t ___value__;
};
struct ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB  : public RuntimeObject
{
	String_t* ___BaseNameField;
	Hashtable_tEFC3B6496E6747787D8BB761B51F2AE3A8CFFE2D* ___ResourceSets;
	Dictionary_2_tAC32B254416DD510DC3E7E36B0706A6B031D7A53* ____resourceSets;
	String_t* ___moduleDir;
	Assembly_t* ___MainAssembly;
	Type_t* ____locationInfo;
	Type_t* ____userResourceSet;
	CultureInfo_t9BA817D41AD55AC8BD07480DD8AC22F8FFA378E0* ____neutralResourcesCulture;
	CultureNameResourceSetPair_t06C5772C09CA853E70E42C0E8BC57FE0AA2CB674* ____lastUsedResourceCache;
	bool ____ignoreCase;
	bool ___UseManifest;
	bool ___UseSatelliteAssem;
	int32_t ____fallbackLoc;
	Version_tE426DB5655D0F22920AE16A2AA9AB7781B8255A7* ____satelliteContractVersion;
	bool ____lookedForSatelliteContractVersion;
	Assembly_t* ____callingAssembly;
	RuntimeAssembly_tA26A4DE82E77826DFC3D58AD976BCFC6BCA918AF* ___m_callingAssembly;
	RuntimeObject* ___resourceGroveler;
};
struct SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295  : public Exception_t
{
};
struct Type_t  : public MemberInfo_t
{
	RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ____impl;
};
struct ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
	String_t* ____paramName;
};
struct MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129  : public ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263
{
};
struct ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F  : public ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263
{
	RuntimeObject* ____actualValue;
};
struct AllocFreeConcurrentStack_tF356ECBDE0D1D4923914EFDDD52E81144E8C1B73_ThreadStaticFields
{
	Dictionary_2_t954AEC357FE7190582B8255D4ACEF81F5E94A6A5* ___t_stacks;
};
struct ImmutableArray_tED520C4BBA5449AF0B7A43F61250ED55233BF648_StaticFields
{
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___TwoElementArray;
};
struct SR_t94F426715F6702F0295747008794B9AC9DF5757F_StaticFields
{
	bool ___s_usingResourceKeys;
	ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* ___s_resourceManager;
};
struct SecureObjectPool_t47E92FD075F6A0B33D038DF5D5B4B79B61A50C3C_StaticFields
{
	int32_t ___s_poolUserIdCounter;
};
struct String_t_StaticFields
{
	String_t* ___Empty;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB_StaticFields
{
	int32_t ___MagicNumber;
	int32_t ___HeaderVersionNumber;
	Type_t* ____minResourceSet;
	String_t* ___ResReaderTypeName;
	String_t* ___ResSetTypeName;
	String_t* ___MscorlibName;
	int32_t ___DEBUG;
};
struct Type_t_StaticFields
{
	Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235* ___s_defaultBinder;
	Il2CppChar ___Delimiter;
	TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* ___EmptyTypes;
	RuntimeObject* ___Missing;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterAttribute;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterName;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterNameIgnoreCase;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031  : public RuntimeArray
{
	ALIGN_FIELD (8) uint8_t m_Items[1];

	inline uint8_t GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline uint8_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, uint8_t value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline uint8_t GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline uint8_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, uint8_t value)
	{
		m_Items[index] = value;
	}
};
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918  : public RuntimeArray
{
	ALIGN_FIELD (8) RuntimeObject* m_Items[1];

	inline RuntimeObject* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline RuntimeObject** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, RuntimeObject* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline RuntimeObject* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline RuntimeObject** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, RuntimeObject* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};



IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2 (Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool SR_UsingResourceKeys_m07156D597F559D2AE590FBE698D7C6A12E256E00_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* SR_get_ResourceManager_mF511D7B963FE90206CEB167B349B6B7BDCBEFCC7 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Join_m1DE9E42021535E27E1623C55608AC7EF370AB3D7 (String_t* ___0_separator, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_values, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Format_mA8DBB4C2516B9723C5A41E6CB1E2FAF4BBE96DD8 (String_t* ___0_format, RuntimeObject* ___1_arg0, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57 (RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ___0_handle, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void ResourceManager__ctor_mC93D478F43E5089ACC407FDECF067A0F208A3784 (ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* __this, Type_t* ___0_resourceSource, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_GetResourceString_mB470DBFBB0411FE6451117F56AD7C40CEB1AA5FC (String_t* ___0_resourceKey, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AppContext_TryGetSwitch_mD2500DB32F941228B6964BC53CAA0EA7047AEB78 (String_t* ___0_switchName, bool* ___1_isEnabled, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Interlocked_Increment_m3C240C32E8D9544EC050B74D4F28EEB58F1F9309 (int32_t* ___0_location, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* __this, String_t* ___0_paramName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Requires_FailRange_mDBFD5CC6EB7F02259AB1D488BACC2076DC1CF583 (String_t* ___0_parameterName, String_t* ___1_message, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478 (String_t* ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentOutOfRangeException__ctor_mBC1D5DEEA1BA41DE77228CB27D6BAFEB6DCCBF4A (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* __this, String_t* ___0_paramName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentOutOfRangeException__ctor_mE5B2755F0BEA043CACF915D5CE140859EE58FA66 (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* __this, String_t* ___0_paramName, String_t* ___1_message, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentException__ctor_m34A925BA55EC4CE4253404E363B5F6A53EB51CA3 (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* __this, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EmbeddedAttribute__ctor_m6B22C8EB6C4686C3978A5F454FF1A69209DBB1FA (EmbeddedAttribute_t509CDEBC1DA670B6FAA87BD018F3C93804E6BC5E* __this, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void IsReadOnlyAttribute__ctor_m0C302A2B03CB17EC4FF218C19F8C9C46964A657A (IsReadOnlyAttribute_t6F27DA2971106AA8A8010CC2A85B938621E07352* __this, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NullableAttribute__ctor_m96114758F3A32BBA8EF82AB52264404D70B75F8B (NullableAttribute_t0A6C93DEC2D86CD9386617D6D9CACFDBAA6321A0* __this, uint8_t ___0_p, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_0 = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)SZArrayNew(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var, (uint32_t)1);
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_1 = L_0;
		uint8_t L_2 = ___0_p;
		NullCheck(L_1);
		(L_1)->SetAt(static_cast<il2cpp_array_size_t>(0), (uint8_t)L_2);
		__this->___NullableFlags = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___NullableFlags), (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NullableAttribute__ctor_mE322E7465CB430A9BCED8D5A68BAF052305C88C0 (NullableAttribute_t0A6C93DEC2D86CD9386617D6D9CACFDBAA6321A0* __this, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___0_p, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_0 = ___0_p;
		__this->___NullableFlags = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___NullableFlags), (void*)L_0);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NullableContextAttribute__ctor_m2FF4CCF823DC1FE064EC8B49A082986E6F6BFE23 (NullableContextAttribute_tB8DBAF2CA8C952F26A27917D78DAEFAFE4E07132* __this, uint8_t ___0_p, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		uint8_t L_0 = ___0_p;
		__this->___Flag = L_0;
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NullablePublicOnlyAttribute__ctor_m4391D75E9F52ED1F24D4B65D7B1EC5446D749695 (NullablePublicOnlyAttribute_t8E6C97291B52B88315355E3FEFCE5A8149CCCE91* __this, bool ___0_p, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		bool L_0 = ___0_p;
		__this->___IncludesInternals = L_0;
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RefSafetyRulesAttribute__ctor_m6BA65B983B276A533870EA56903D2D36B0E97B27 (RefSafetyRulesAttribute_t5F7093546DFFF5023C78FF99DF02AE4E9624CE73* __this, int32_t ___0_p, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		int32_t L_0 = ___0_p;
		__this->___Version = L_0;
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool SR_UsingResourceKeys_m07156D597F559D2AE590FBE698D7C6A12E256E00 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		bool L_0 = ((SR_t94F426715F6702F0295747008794B9AC9DF5757F_StaticFields*)il2cpp_codegen_static_fields_for(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var))->___s_usingResourceKeys;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_GetResourceString_mB470DBFBB0411FE6451117F56AD7C40CEB1AA5FC (String_t* ___0_resourceKey, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	String_t* V_0 = NULL;
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		bool L_0;
		L_0 = SR_UsingResourceKeys_m07156D597F559D2AE590FBE698D7C6A12E256E00_inline(NULL);
		if (!L_0)
		{
			goto IL_0009;
		}
	}
	{
		String_t* L_1 = ___0_resourceKey;
		return L_1;
	}

IL_0009:
	{
		V_0 = (String_t*)NULL;
	}
	try
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_2;
		L_2 = SR_get_ResourceManager_mF511D7B963FE90206CEB167B349B6B7BDCBEFCC7(NULL);
		String_t* L_3 = ___0_resourceKey;
		NullCheck(L_2);
		String_t* L_4;
		L_4 = VirtualFuncInvoker1< String_t*, String_t* >::Invoke(7, L_2, L_3);
		V_0 = L_4;
		goto IL_001c;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_0019;
		}
		throw e;
	}

CATCH_0019:
	{
		MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD* L_5 = ((MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD*)IL2CPP_GET_ACTIVE_EXCEPTION(MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD*));;
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		goto IL_001c;
	}

IL_001c:
	{
		String_t* L_6 = V_0;
		return L_6;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_Format_m15C2107D9AC3B0DBA15121DB9A582AB29DCAFDA4 (String_t* ___0_resourceFormat, RuntimeObject* ___1_p1, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		bool L_0;
		L_0 = SR_UsingResourceKeys_m07156D597F559D2AE590FBE698D7C6A12E256E00_inline(NULL);
		if (!L_0)
		{
			goto IL_0020;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)2);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = L_1;
		String_t* L_3 = ___0_resourceFormat;
		NullCheck(L_2);
		ArrayElementTypeCheck (L_2, L_3);
		(L_2)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_3);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = L_2;
		RuntimeObject* L_5 = ___1_p1;
		NullCheck(L_4);
		ArrayElementTypeCheck (L_4, L_5);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(1), (RuntimeObject*)L_5);
		String_t* L_6;
		L_6 = String_Join_m1DE9E42021535E27E1623C55608AC7EF370AB3D7(_stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D, L_4, NULL);
		return L_6;
	}

IL_0020:
	{
		String_t* L_7 = ___0_resourceFormat;
		RuntimeObject* L_8 = ___1_p1;
		String_t* L_9;
		L_9 = String_Format_mA8DBB4C2516B9723C5A41E6CB1E2FAF4BBE96DD8(L_7, L_8, NULL);
		return L_9;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* SR_get_ResourceManager_mF511D7B963FE90206CEB167B349B6B7BDCBEFCC7 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_tCFED80D0C6EF3202EA88CF770D21756283090813_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* G_B2_0 = NULL;
	ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* G_B1_0 = NULL;
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_0 = ((SR_t94F426715F6702F0295747008794B9AC9DF5757F_StaticFields*)il2cpp_codegen_static_fields_for(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var))->___s_resourceManager;
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_1 = L_0;
		if (L_1)
		{
			G_B2_0 = L_1;
			goto IL_001e;
		}
		G_B1_0 = L_1;
	}
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (SR_tCFED80D0C6EF3202EA88CF770D21756283090813_0_0_0_var) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_4 = (ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB*)il2cpp_codegen_object_new(ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB_il2cpp_TypeInfo_var);
		ResourceManager__ctor_mC93D478F43E5089ACC407FDECF067A0F208A3784(L_4, L_3, NULL);
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_5 = L_4;
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		((SR_t94F426715F6702F0295747008794B9AC9DF5757F_StaticFields*)il2cpp_codegen_static_fields_for(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var))->___s_resourceManager = L_5;
		Il2CppCodeGenWriteBarrier((void**)(&((SR_t94F426715F6702F0295747008794B9AC9DF5757F_StaticFields*)il2cpp_codegen_static_fields_for(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var))->___s_resourceManager), (void*)L_5);
		G_B2_0 = L_5;
	}

IL_001e:
	{
		return G_B2_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_Arg_KeyNotFoundWithKey_mFEA2BD79E749F50C4604903D6C74E92203149BE7 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral6E49800871410E6255741DE232E2006F4FA40D28);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = SR_GetResourceString_mB470DBFBB0411FE6451117F56AD7C40CEB1AA5FC(_stringLiteral6E49800871410E6255741DE232E2006F4FA40D28, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_ArrayInitializedStateNotEqual_m7754B4F94A58E9B290A319FE40C7585A57F018CA (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral70079B6AAC8BF895F1E70E28153EE819801DF849);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = SR_GetResourceString_mB470DBFBB0411FE6451117F56AD7C40CEB1AA5FC(_stringLiteral70079B6AAC8BF895F1E70E28153EE819801DF849, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_ArrayLengthsNotEqual_m19E26E4B981BA1E9CA76F2461CEC73D97CFD5843 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral1BC0150CC915B8119BC6785445D0E1FCF16BF047);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = SR_GetResourceString_mB470DBFBB0411FE6451117F56AD7C40CEB1AA5FC(_stringLiteral1BC0150CC915B8119BC6785445D0E1FCF16BF047, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_CollectionModifiedDuringEnumeration_m9EF158B7E33B9FBBEF8553A734E27D3C6A331544 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral141C9904DF9DE9F151E0B8FCF5A8124B46177546);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = SR_GetResourceString_mB470DBFBB0411FE6451117F56AD7C40CEB1AA5FC(_stringLiteral141C9904DF9DE9F151E0B8FCF5A8124B46177546, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_DuplicateKey_mAA6D35BD4D69F00D8F742656987A55B4D02C92C0 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral81B3823BBC3752A005ED4DC7AC4148D45DDF5ACC);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = SR_GetResourceString_mB470DBFBB0411FE6451117F56AD7C40CEB1AA5FC(_stringLiteral81B3823BBC3752A005ED4DC7AC4148D45DDF5ACC, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_InvalidEmptyOperation_m4CCFA31E0D7ADF78F18BD7145DDA8F80D29D5A0B (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral07E5C4DBC844DE7F5317C074564E52D67C4D5205);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = SR_GetResourceString_mB470DBFBB0411FE6451117F56AD7C40CEB1AA5FC(_stringLiteral07E5C4DBC844DE7F5317C074564E52D67C4D5205, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_InvalidOperationOnDefaultArray_mCA50D98B5E90C713753769F360F42CF8C4522801 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralA2F18350FA50F3568A798127F78B191252CA4CA3);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = SR_GetResourceString_mB470DBFBB0411FE6451117F56AD7C40CEB1AA5FC(_stringLiteralA2F18350FA50F3568A798127F78B191252CA4CA3, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SR__cctor_mF1FFEB9D46DE20DAB019522AD78BFB988F52FC79 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AppContext_t0380D19FAC72CD59D46947D86DC1DAA3BCE638E0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralB5528CA07A43AEA4EFA2F7B2DEF38E0A5D87ECD6);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	int32_t G_B3_0 = 0;
	{
		il2cpp_codegen_runtime_class_init_inline(AppContext_t0380D19FAC72CD59D46947D86DC1DAA3BCE638E0_il2cpp_TypeInfo_var);
		bool L_0;
		L_0 = AppContext_TryGetSwitch_mD2500DB32F941228B6964BC53CAA0EA7047AEB78(_stringLiteralB5528CA07A43AEA4EFA2F7B2DEF38E0A5D87ECD6, (&V_0), NULL);
		if (L_0)
		{
			goto IL_0011;
		}
	}
	{
		G_B3_0 = 0;
		goto IL_0012;
	}

IL_0011:
	{
		bool L_1 = V_0;
		G_B3_0 = ((int32_t)(L_1));
	}

IL_0012:
	{
		((SR_t94F426715F6702F0295747008794B9AC9DF5757F_StaticFields*)il2cpp_codegen_static_fields_for(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var))->___s_usingResourceKeys = (bool)G_B3_0;
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NotNullAttribute__ctor_m7CDC6F4289DC183D678E376DB6103E32FC0CEF82 (NotNullAttribute_tF521CEFFAF06EEF727844ECDDDB79C4F2DBEE4F0* __this, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void MaybeNullWhenAttribute__ctor_m2CFF21523A78CCF6871A6994125D60E9964EFD1D (MaybeNullWhenAttribute_tC05F437DB6EA00FF11A743B16C96D1F7982C9358* __this, bool ___0_returnValue, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		bool L_0 = ___0_returnValue;
		__this->___U3CReturnValueU3Ek__BackingField = L_0;
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NotNullWhenAttribute__ctor_m48414C2494020009389DF7ABC0E6EAEA0EE65258 (NotNullWhenAttribute_t7928691EA752981A4524F3737ADB1BB825EEC360* __this, bool ___0_returnValue, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		bool L_0 = ___0_returnValue;
		__this->___U3CReturnValueU3Ek__BackingField = L_0;
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DoesNotReturnAttribute__ctor_m4D2A0A636361F01C5FC01AF7E2D5CFF14E670DE0 (DoesNotReturnAttribute_t3598B7BBE88E89C0D9315952A9E497BFA5AAC753* __this, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DoesNotReturnIfAttribute__ctor_mF2F99916C12EF68FC6F8480B34623957DDD2FE30 (DoesNotReturnIfAttribute_t13E8A106B50DA12AA2B960D1CDDE616D3BEEACB9* __this, bool ___0_parameterValue, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		bool L_0 = ___0_parameterValue;
		__this->___U3CParameterValueU3Ek__BackingField = L_0;
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NonVersionableAttribute__ctor_m0BF9715034DBE2F59F007681E158E293305CBCB2 (NonVersionableAttribute_tDE71082D419D7BCDB378F56EC6972E9B8B2600DE* __this, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CollectionBuilderAttribute__ctor_mFCC38EC89410069CC92EF40FBF7F84F237F449B2 (CollectionBuilderAttribute_t54580B583B741DF2BEBC247A659A1B9D82E47AA2* __this, Type_t* ___0_builderType, String_t* ___1_methodName, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		Type_t* L_0 = ___0_builderType;
		__this->___U3CBuilderTypeU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CBuilderTypeU3Ek__BackingField), (void*)L_0);
		String_t* L_1 = ___1_methodName;
		__this->___U3CMethodNameU3Ek__BackingField = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CMethodNameU3Ek__BackingField), (void*)L_1);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ImmutableArray__cctor_m56DAE227080D99D49B775A6EBF6F0DAE4C7FF65E (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ImmutableArray_tED520C4BBA5449AF0B7A43F61250ED55233BF648_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_0 = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)SZArrayNew(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var, (uint32_t)2);
		((ImmutableArray_tED520C4BBA5449AF0B7A43F61250ED55233BF648_StaticFields*)il2cpp_codegen_static_fields_for(ImmutableArray_tED520C4BBA5449AF0B7A43F61250ED55233BF648_il2cpp_TypeInfo_var))->___TwoElementArray = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&((ImmutableArray_tED520C4BBA5449AF0B7A43F61250ED55233BF648_StaticFields*)il2cpp_codegen_static_fields_for(ImmutableArray_tED520C4BBA5449AF0B7A43F61250ED55233BF648_il2cpp_TypeInfo_var))->___TwoElementArray), (void*)L_0);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t SecureObjectPool_NewId_m937EA0D1220608DB6FDDBD0AF1FEA395F2D3DFDE (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SecureObjectPool_t47E92FD075F6A0B33D038DF5D5B4B79B61A50C3C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	int32_t V_0 = 0;

IL_0000:
	{
		int32_t L_0;
		L_0 = Interlocked_Increment_m3C240C32E8D9544EC050B74D4F28EEB58F1F9309((&((SecureObjectPool_t47E92FD075F6A0B33D038DF5D5B4B79B61A50C3C_StaticFields*)il2cpp_codegen_static_fields_for(SecureObjectPool_t47E92FD075F6A0B33D038DF5D5B4B79B61A50C3C_il2cpp_TypeInfo_var))->___s_poolUserIdCounter), NULL);
		V_0 = L_0;
		int32_t L_1 = V_0;
		if ((((int32_t)L_1) == ((int32_t)(-1))))
		{
			goto IL_0000;
		}
	}
	{
		int32_t L_2 = V_0;
		return L_2;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Requires_FailArgumentNullException_mABEC2B38D4FF1AE27F6594BF5E93194F04EC8EB6 (String_t* ___0_parameterName, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_parameterName;
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, L_0, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Requires_FailArgumentNullException_mABEC2B38D4FF1AE27F6594BF5E93194F04EC8EB6_RuntimeMethod_var)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Requires_Range_m1F4EA69EC6F6292F284590E64A7BC2B27160CB75 (bool ___0_condition, String_t* ___1_parameterName, String_t* ___2_message, const RuntimeMethod* method) 
{
	{
		bool L_0 = ___0_condition;
		if (L_0)
		{
			goto IL_000a;
		}
	}
	{
		String_t* L_1 = ___1_parameterName;
		String_t* L_2 = ___2_message;
		Requires_FailRange_mDBFD5CC6EB7F02259AB1D488BACC2076DC1CF583(L_1, L_2, NULL);
	}

IL_000a:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Requires_FailRange_mDBFD5CC6EB7F02259AB1D488BACC2076DC1CF583 (String_t* ___0_parameterName, String_t* ___1_message, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___1_message;
		bool L_1;
		L_1 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_0, NULL);
		if (!L_1)
		{
			goto IL_000f;
		}
	}
	{
		String_t* L_2 = ___0_parameterName;
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_3 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_mBC1D5DEEA1BA41DE77228CB27D6BAFEB6DCCBF4A(L_3, L_2, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_3, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Requires_FailRange_mDBFD5CC6EB7F02259AB1D488BACC2076DC1CF583_RuntimeMethod_var)));
	}

IL_000f:
	{
		String_t* L_4 = ___0_parameterName;
		String_t* L_5 = ___1_message;
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_6 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_mE5B2755F0BEA043CACF915D5CE140859EE58FA66(L_6, L_4, L_5, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_6, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Requires_FailRange_mDBFD5CC6EB7F02259AB1D488BACC2076DC1CF583_RuntimeMethod_var)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Requires_Argument_mC0D17F52DC37FD36EA0C45C7464CD2965FFF6277 (bool ___0_condition, const RuntimeMethod* method) 
{
	{
		bool L_0 = ___0_condition;
		if (L_0)
		{
			goto IL_0009;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_1 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m34A925BA55EC4CE4253404E363B5F6A53EB51CA3(L_1, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Requires_Argument_mC0D17F52DC37FD36EA0C45C7464CD2965FFF6277_RuntimeMethod_var)));
	}

IL_0009:
	{
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool SR_UsingResourceKeys_m07156D597F559D2AE590FBE698D7C6A12E256E00_inline (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var);
		bool L_0 = ((SR_t94F426715F6702F0295747008794B9AC9DF5757F_StaticFields*)il2cpp_codegen_static_fields_for(SR_t94F426715F6702F0295747008794B9AC9DF5757F_il2cpp_TypeInfo_var))->___s_usingResourceKeys;
		return L_0;
	}
}
