#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>


template <typename R>
struct VirtualFuncInvoker0
{
	typedef R (*Func)(void*, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, invokeData.method);
	}
};
template <typename T1>
struct InvokerActionInvoker1;
template <typename T1>
struct InvokerActionInvoker1<T1*>
{
	static inline void Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1* p1)
	{
		void* params[1] = { p1 };
		method->invoker_method(methodPtr, method, obj, params, params[0]);
	}
};
template <typename R, typename T1, typename T2, typename T3>
struct InvokerFuncInvoker3;
template <typename R, typename T1, typename T2, typename T3>
struct InvokerFuncInvoker3<R, T1*, T2*, T3>
{
	static inline R Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1* p1, T2* p2, T3 p3)
	{
		R ret;
		void* params[3] = { p1, p2, &p3 };
		method->invoker_method(methodPtr, method, obj, params, &ret);
		return ret;
	}
};

struct HashSet_1_tEFC6605F7DE53F71946C33FD371E53C3100F2178;
struct IEnumerable_1_tCF90FB4C3D9D17223FD2D30FE8F86C198C051414;
struct IEnumerable_1_tCE758D940790D6D0D56B457E522C195F8C413AF2;
struct IEnumerable_1_t71A46277DBD73BD4009B2B20885D2B7057593A1A;
struct IEnumerable_1_t7E13FCC61B2DB26146CA569CB146A0200C4618D1;
struct IEnumerable_1_tF95C9E01A913DD50575531C8305932628663D9E9;
struct IEnumerable_1_tA6BA499EB9C53CE7ACA0898B4769F01B7A640048;
struct IEnumerable_1_tF43F17367427A8D452C71EFB7D142B35C5852E2D;
struct IEnumerable_1_t29E7244AE33B71FA0981E50D5BC73B7938F35C66;
struct IEnumerable_1_t6CD4A7DFC9E97930A27279D255D6BBF9E413F881;
struct IEnumerator_1_t48DE2C04DB4D057A312C6011901E265D6CAFE759;
struct IEnumerator_1_tD6A90A7446DA8E6CF865EDFBBF18C1200BB6D452;
struct IEnumerator_1_t239F6ACD0FC026E7FA70965FDE161517CD367AED;
struct IEnumerator_1_t9EBA015A0670545A091B6BE5A51191A69DD0F5EB;
struct IEnumerator_1_t43D2E4BA9246755F293DFA74F001FB1A70A648FD;
struct IEnumerator_1_t934D66F891883B7459C97B6E4E88904B9D9ED453;
struct IEnumerator_1_t69592F7A01FD92D90373FE1B774330D5D5EFC8A6;
struct IEnumerator_1_t75CB2681E18F7F2791528FA2CA60361FDB5DA08D;
struct IEnumerator_1_t48B09B24A80E91B22872AE26AEBD52E96746D13F;
struct SparseArray_1_t8D77ECC0E534C3B1A8C7403D9BDAC38694EAF242;
struct SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC;
struct SparselyPopulatedArray_1_t127D3C1977D038D143CA5A6CDD74A71117B5A614;
struct StackFormatter_1_tDFD80D25A4FEF1FD6AF71727AE2DB15A591D35C4;
struct StackItem_t653DA96524E2146197E46E4AB44EB360B1531DEB;
struct Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67;
struct Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6;
struct Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9;
struct Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E;
struct Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5;
struct Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA;
struct Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6;
struct Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A;
struct Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA;
struct RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186;
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C;
struct Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F;
struct Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D;
struct IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832;
struct Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7;
struct RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993;
struct SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913;
struct SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C;
struct StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF;
struct TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A;
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB;
struct UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83;
struct UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA;
struct UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299;
struct Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C;
struct __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC;
struct jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F;
struct MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5;
struct ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263;
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129;
struct ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F;
struct Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235;
struct IDictionary_t6D03155AF1FA9083817AA5B6AD7DEEACC26AB220;
struct IEnumerator_t7B609C2FFA6EB5167D9C62A0C32A21DE2F666DAA;
struct IFormatterResolver_t5B1CEFC5BBDC456659A223091F4E9614C7CEF1AB;
struct InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB;
struct MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553;
struct MessagePackSecurity_tDB02E2D77A7D902F39EDE4B53285607666ABC7DB;
struct MessagePackSerializerOptions_t6356E2266D4FA5A0334004939D1C37D105B23356;
struct NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A;
struct Regex_tE773142C2BE45C5D362B0F815AFF831707A51772;
struct SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6;
struct SequencePool_tDDF5759DAF0456103A2A673863B6524E96D83CFF;
struct String_t;
struct Type_t;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;

IL2CPP_EXTERN_C RuntimeClass* ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* RuntimeObject_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Type_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral0DB46164953228904843938099AF66650313FEE5;
IL2CPP_EXTERN_C String_t* _stringLiteral38E3DBC7FC353425EF3A98DC8DAC6689AF5FD1BE;
IL2CPP_EXTERN_C String_t* _stringLiteral469F05BE9BB4C7903C353D0EB9F6384C84A48B25;
IL2CPP_EXTERN_C String_t* _stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4;
IL2CPP_EXTERN_C String_t* _stringLiteral6195D7DA68D16D4985AD1A1B4FD2841A43CDDE70;
IL2CPP_EXTERN_C String_t* _stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D;
IL2CPP_EXTERN_C String_t* _stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8;
IL2CPP_EXTERN_C String_t* _stringLiteral95F0EE30865D503A05F1D329BC3FED0946B65C24;
IL2CPP_EXTERN_C String_t* _stringLiteral967D403A541A1026A83D548E5AD5CA800AD4EFB5;
IL2CPP_EXTERN_C String_t* _stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED;
IL2CPP_EXTERN_C String_t* _stringLiteralBD0381A992FDF4F7DA60E5D83689FE7FF6309CB8;
IL2CPP_EXTERN_C String_t* _stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4;
IL2CPP_EXTERN_C String_t* _stringLiteralC37D78082ACFC8DEE7B32D9351C6E433A074FEC7;
IL2CPP_EXTERN_C String_t* _stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8;
IL2CPP_EXTERN_C const RuntimeMethod* Nullable_1__ctor_m141FA88563AC0B5179132FB929EABD02C47FF703_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeType* Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var;
struct Exception_t_marshaled_com;
struct Exception_t_marshaled_pinvoke;

struct RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186;
struct Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C;
struct Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F;
struct Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D;
struct Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7;
struct RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993;
struct SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913;
struct SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C;
struct TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A;
struct UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83;
struct UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA;
struct UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299;
struct Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C;
struct __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC;
struct jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F;
struct MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct CollectionFormatterBase_4_t64CAD9344C5BA7B735D0B4FB4BD405FA6159AB78  : public RuntimeObject
{
};
struct EmptyArray_1_tD44919E2B605D341F68CD611C3D99481A875DFDF  : public RuntimeObject
{
};
struct EmptyArray_1_tE700FA647008891EF64C31436B092B253493667F  : public RuntimeObject
{
};
struct EmptyArray_1_t0A27D963887A48FA040C718B868C2455F9AD84FA  : public RuntimeObject
{
};
struct EmptyArray_1_t63074FE3C78EACBE204FE82D30DB508A9EB6268A  : public RuntimeObject
{
};
struct EmptyArray_1_t685951D47CCFA8F90AC66EE22811A68A045FB065  : public RuntimeObject
{
};
struct EmptyArray_1_tDF0DD7256B115243AA6BD5558417387A734240EE  : public RuntimeObject
{
};
struct EmptyArray_1_t6D66030EDC48119B7B485C10DBB9F8FB67366E47  : public RuntimeObject
{
};
struct EmptyArray_1_t08704EABC4CA3368B0E6C088FF83A3BDE70484C8  : public RuntimeObject
{
};
struct EmptyArray_1_tB3950DD0CFA703643EB93EDD4FF714B5A085FF8F  : public RuntimeObject
{
};
struct EmptyArray_1_t2984B8F74E4B1E6C047125D296C6C06779CA328D  : public RuntimeObject
{
};
struct EmptyArray_1_t21B6219CD4389EFB0E40E69BEC0AE2E54C1AA17A  : public RuntimeObject
{
};
struct EmptyArray_1_tA0524EFBAA750B3D1DCF60FE6F2B25DE798675AD  : public RuntimeObject
{
};
struct EmptyArray_1_t77BFDB090CFC6AE661834F0BD4ED43833F4F079D  : public RuntimeObject
{
};
struct EmptyArray_1_tA1B9390A54D6283BFF40643DC2A697126A16E63B  : public RuntimeObject
{
};
struct EmptyArray_1_tF91FBA61857F9D60B55FD121DEADC9788D1FE016  : public RuntimeObject
{
};
struct EmptyArray_1_tB610FBC2B87561A97224E274FC1699BCE50B2C60  : public RuntimeObject
{
};
struct EmptyArray_1_tA32003E1DE04BE9767FD72E5DC4FEA8D1B834759  : public RuntimeObject
{
};
struct SparseArray_1_t8D77ECC0E534C3B1A8C7403D9BDAC38694EAF242  : public RuntimeObject
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___m_array;
};
struct SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC  : public RuntimeObject
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ____elements;
	int32_t ____freeCount;
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* ____next;
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* ____prev;
};
struct SparselyPopulatedArray_1_t127D3C1977D038D143CA5A6CDD74A71117B5A614  : public RuntimeObject
{
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* ____head;
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* ____tail;
};
struct StackDebugView_1_t44367B01AFAD9690E05D510A1EBE184344E4BC56  : public RuntimeObject
{
};
struct StackItem_t653DA96524E2146197E46E4AB44EB360B1531DEB  : public RuntimeObject
{
	int32_t ___p;
	int32_t ___r;
};
struct Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67  : public RuntimeObject
{
	RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* ____array;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6  : public RuntimeObject
{
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ____array;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9  : public RuntimeObject
{
	Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* ____array;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E  : public RuntimeObject
{
	Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* ____array;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5  : public RuntimeObject
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ____array;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA  : public RuntimeObject
{
	RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* ____array;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6  : public RuntimeObject
{
	TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* ____array;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A  : public RuntimeObject
{
	__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ____array;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA  : public RuntimeObject
{
	MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* ____array;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct MemberInfo_t  : public RuntimeObject
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
struct Enumerator_t1ED2EFBA8997D05D3B6586A9FF9D467F8D5482F0 
{
	Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* ____stack;
	int32_t ____version;
	int32_t ____index;
	int32_t ____currentElement;
};
struct Enumerator_t852186DADC50D976C4BD8FE59C506354ED48B974 
{
	Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* ____stack;
	int32_t ____version;
	int32_t ____index;
	RuntimeObject* ____currentElement;
};
typedef Il2CppFullySharedGenericStruct Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41;
struct Nullable_1_t78F453FADB4A9F50F267A4E349019C34410D1A01 
{
	bool ___hasValue;
	bool ___value;
};
struct Nullable_1_tCF32C56A2641879C053C86F273C0C6EC1B40BC28 
{
	bool ___hasValue;
	int32_t ___value;
};
struct RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 
{
	RuntimeObject* ___Value;
};
struct SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D 
{
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* ____source;
	int32_t ____index;
};
struct StackFormatter_1_tDFD80D25A4FEF1FD6AF71727AE2DB15A591D35C4  : public CollectionFormatterBase_4_t64CAD9344C5BA7B735D0B4FB4BD405FA6159AB78
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
struct Char_t521A6F19B456D956AF452D926C32709DC03D6B17 
{
	Il2CppChar ___m_value;
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
struct Int64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3 
{
	int64_t ___m_value;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 
{
	float ___m00;
	float ___m10;
	float ___m20;
	float ___m30;
	float ___m01;
	float ___m11;
	float ___m21;
	float ___m31;
	float ___m02;
	float ___m12;
	float ___m22;
	float ___m32;
	float ___m03;
	float ___m13;
	float ___m23;
	float ___m33;
};
struct Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 
{
	float ___x;
	float ___y;
	float ___z;
	float ___w;
};
struct Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D 
{
	float ___m_XMin;
	float ___m_YMin;
	float ___m_Width;
	float ___m_Height;
};
struct SByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5 
{
	int8_t ___m_value;
};
struct Single_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C 
{
	float ___m_value;
};
struct TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 
{
	int32_t ___m_Index;
};
struct UInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455 
{
	uint16_t ___m_value;
};
struct UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B 
{
	uint32_t ___m_value;
};
struct UInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF 
{
	uint64_t ___m_value;
};
struct Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 
{
	float ___x;
	float ___y;
	float ___z;
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
struct MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 
{
	int32_t ___valueIndex;
	int32_t ___matchedVariableCount;
};
struct ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 
{
	intptr_t ____value;
};
struct ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 
{
	intptr_t ____value;
};
struct ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 
{
	intptr_t ____value;
};
struct ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD 
{
	intptr_t ____value;
};
struct ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 
{
	intptr_t ____value;
};
struct ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A 
{
	intptr_t ____value;
};
struct ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE 
{
	intptr_t ____value;
};
struct ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 
{
	intptr_t ____value;
};
struct ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 
{
	intptr_t ____value;
};
struct ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 
{
	intptr_t ____value;
};
struct ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 
{
	intptr_t ____value;
};
struct ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC 
{
	intptr_t ____value;
};
struct Enumerator_t6C87A4CFDE2206491F41A95637BE2F6F5222FABC 
{
	Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* ____stack;
	int32_t ____version;
	int32_t ____index;
	RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 ____currentElement;
};
struct Enumerator_t1999C0BADD971016559CDAF90DA9DC67FAEF5B8A 
{
	Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* ____stack;
	int32_t ____version;
	int32_t ____index;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ____currentElement;
};
struct Enumerator_t0757D472C179E7C385905619753AC7417A77B40A 
{
	Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* ____stack;
	int32_t ____version;
	int32_t ____index;
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ____currentElement;
};
struct Enumerator_tE489CD82CEA07F70C1967F4E911F125AA65FD336 
{
	Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* ____stack;
	int32_t ____version;
	int32_t ____index;
	TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 ____currentElement;
};
struct Enumerator_t6B0EB72F34C4614A45F424103A9D11C74439E854 
{
	Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* ____stack;
	int32_t ____version;
	int32_t ____index;
	MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 ____currentElement;
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
struct Int32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C 
{
	int32_t ___value__;
};
struct MessagePackCompression_t82BBC99CB3DD2EB27C402070930E756C3AE2D20B 
{
	int32_t ___value__;
};
struct RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B 
{
	intptr_t ___value;
};
struct jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			bool ___z;
		};
		#pragma pack(pop, tp)
		struct
		{
			bool ___z_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int8_t ___b;
		};
		#pragma pack(pop, tp)
		struct
		{
			int8_t ___b_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			Il2CppChar ___c;
		};
		#pragma pack(pop, tp)
		struct
		{
			Il2CppChar ___c_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int16_t ___s;
		};
		#pragma pack(pop, tp)
		struct
		{
			int16_t ___s_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___i;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___i_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int64_t ___j;
		};
		#pragma pack(pop, tp)
		struct
		{
			int64_t ___j_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			float ___f;
		};
		#pragma pack(pop, tp)
		struct
		{
			float ___f_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			double ___d;
		};
		#pragma pack(pop, tp)
		struct
		{
			double ___d_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			intptr_t ___l;
		};
		#pragma pack(pop, tp)
		struct
		{
			intptr_t ___l_forAlignmentOnly;
		};
	};
};
struct jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_marshaled_pinvoke
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___z;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___z_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int8_t ___b;
		};
		#pragma pack(pop, tp)
		struct
		{
			int8_t ___b_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			uint8_t ___c;
		};
		#pragma pack(pop, tp)
		struct
		{
			uint8_t ___c_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int16_t ___s;
		};
		#pragma pack(pop, tp)
		struct
		{
			int16_t ___s_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___i;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___i_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int64_t ___j;
		};
		#pragma pack(pop, tp)
		struct
		{
			int64_t ___j_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			float ___f;
		};
		#pragma pack(pop, tp)
		struct
		{
			float ___f_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			double ___d;
		};
		#pragma pack(pop, tp)
		struct
		{
			double ___d_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			intptr_t ___l;
		};
		#pragma pack(pop, tp)
		struct
		{
			intptr_t ___l_forAlignmentOnly;
		};
	};
};
struct jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_marshaled_com
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___z;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___z_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int8_t ___b;
		};
		#pragma pack(pop, tp)
		struct
		{
			int8_t ___b_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			uint8_t ___c;
		};
		#pragma pack(pop, tp)
		struct
		{
			uint8_t ___c_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int16_t ___s;
		};
		#pragma pack(pop, tp)
		struct
		{
			int16_t ___s_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___i;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___i_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int64_t ___j;
		};
		#pragma pack(pop, tp)
		struct
		{
			int64_t ___j_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			float ___f;
		};
		#pragma pack(pop, tp)
		struct
		{
			float ___f_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			double ___d;
		};
		#pragma pack(pop, tp)
		struct
		{
			double ___d_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			intptr_t ___l;
		};
		#pragma pack(pop, tp)
		struct
		{
			intptr_t ___l_forAlignmentOnly;
		};
	};
};
struct RawData_t37CAF2D3F74B7723974ED7CEEE9B297D8FA64ED0  : public RuntimeObject
{
	intptr_t ___Bounds;
	intptr_t ___Count;
	uint8_t ___Data;
};
struct RawData_t37CAF2D3F74B7723974ED7CEEE9B297D8FA64ED0_marshaled_pinvoke
{
	intptr_t ___Bounds;
	intptr_t ___Count;
	uint8_t ___Data;
};
struct RawData_t37CAF2D3F74B7723974ED7CEEE9B297D8FA64ED0_marshaled_com
{
	intptr_t ___Bounds;
	intptr_t ___Count;
	uint8_t ___Data;
};
struct Enumerator_t0D7F1F6081F1B6DDB263573004129C443F04F41C 
{
	Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* ____stack;
	int32_t ____version;
	int32_t ____index;
	int32_t ____currentElement;
};
struct ReadOnlySpan_1_t6190994DF094ABDFA6908C2C3FB347457E8E4282 
{
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 ____pointer;
	int32_t ____length;
};
struct ReadOnlySpan_1_t6CE9C0CA1262A820428D86548CAE80352AAA12AC 
{
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 ____pointer;
	int32_t ____length;
};
struct ReadOnlySpan_1_t40ECE3A478A7988D6572F814C5099ACFDE1FDF95 
{
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 ____pointer;
	int32_t ____length;
};
struct ReadOnlySpan_1_tDE8983102D42568E6127FA7BE5CE63380CD7A820 
{
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD ____pointer;
	int32_t ____length;
};
struct ReadOnlySpan_1_tB90CE592448A63B07B56F79364563BD361CF8CDC 
{
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 ____pointer;
	int32_t ____length;
};
struct ReadOnlySpan_1_t9C2C8EDE84088EDC61AADD4CA3C2CDC72D135E3D 
{
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A ____pointer;
	int32_t ____length;
};
struct ReadOnlySpan_1_tA2EFC117098BD2B38ADBF809AA976D9F3C13654F 
{
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE ____pointer;
	int32_t ____length;
};
struct ReadOnlySpan_1_t57F4BBC957039E8E904443D25F3A78AE60DC94B4 
{
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 ____pointer;
	int32_t ____length;
};
struct ReadOnlySpan_1_tD579EC24AE7548EFE16428C9DC0EE0423C0FDA0F 
{
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 ____pointer;
	int32_t ____length;
};
struct ReadOnlySpan_1_t8C2C24B6B10C9EAC8D8A8C92BED569A12615C2AE 
{
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 ____pointer;
	int32_t ____length;
};
struct ReadOnlySpan_1_tC416A5627E04F69CA2947A2A13F0A1DF096CABAC 
{
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 ____pointer;
	int32_t ____length;
};
struct ReadOnlySpan_1_tEC1E786ADECA91AF68E558734868FBB77E05D964 
{
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC ____pointer;
	int32_t ____length;
};
struct Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 
{
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 ____pointer;
	int32_t ____length;
};
struct Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 
{
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 ____pointer;
	int32_t ____length;
};
struct Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 
{
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 ____pointer;
	int32_t ____length;
};
struct Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 
{
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD ____pointer;
	int32_t ____length;
};
struct Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 
{
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 ____pointer;
	int32_t ____length;
};
struct Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C 
{
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A ____pointer;
	int32_t ____length;
};
struct Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D 
{
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE ____pointer;
	int32_t ____length;
};
struct Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA 
{
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 ____pointer;
	int32_t ____length;
};
struct Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 
{
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 ____pointer;
	int32_t ____length;
};
struct Span_1_t460D4E78469192619B0075C15897A6987393AAF9 
{
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 ____pointer;
	int32_t ____length;
};
struct Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 
{
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 ____pointer;
	int32_t ____length;
};
struct Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 
{
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC ____pointer;
	int32_t ____length;
};
struct MessagePackSerializerOptions_t6356E2266D4FA5A0334004939D1C37D105B23356  : public RuntimeObject
{
	RuntimeObject* ___U3CResolverU3Ek__BackingField;
	int32_t ___U3CCompressionU3Ek__BackingField;
	int32_t ___U3CCompressionMinLengthU3Ek__BackingField;
	int32_t ___U3CSuggestedContiguousMemorySizeU3Ek__BackingField;
	Nullable_1_t78F453FADB4A9F50F267A4E349019C34410D1A01 ___U3COldSpecU3Ek__BackingField;
	bool ___U3COmitAssemblyVersionU3Ek__BackingField;
	bool ___U3CAllowAssemblyVersionMismatchU3Ek__BackingField;
	MessagePackSecurity_tDB02E2D77A7D902F39EDE4B53285607666ABC7DB* ___U3CSecurityU3Ek__BackingField;
	SequencePool_tDDF5759DAF0456103A2A673863B6524E96D83CFF* ___U3CSequencePoolU3Ek__BackingField;
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
struct ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129  : public ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263
{
};
struct ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F  : public ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263
{
	RuntimeObject* ____actualValue;
};
struct EmptyArray_1_tD44919E2B605D341F68CD611C3D99481A875DFDF_StaticFields
{
	RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* ___Value;
};
struct EmptyArray_1_tE700FA647008891EF64C31436B092B253493667F_StaticFields
{
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___Value;
};
struct EmptyArray_1_t0A27D963887A48FA040C718B868C2455F9AD84FA_StaticFields
{
	Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* ___Value;
};
struct EmptyArray_1_t63074FE3C78EACBE204FE82D30DB508A9EB6268A_StaticFields
{
	Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* ___Value;
};
struct EmptyArray_1_t685951D47CCFA8F90AC66EE22811A68A045FB065_StaticFields
{
	Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* ___Value;
};
struct EmptyArray_1_tDF0DD7256B115243AA6BD5558417387A734240EE_StaticFields
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___Value;
};
struct EmptyArray_1_t6D66030EDC48119B7B485C10DBB9F8FB67366E47_StaticFields
{
	QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* ___Value;
};
struct EmptyArray_1_t08704EABC4CA3368B0E6C088FF83A3BDE70484C8_StaticFields
{
	RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* ___Value;
};
struct EmptyArray_1_tB3950DD0CFA703643EB93EDD4FF714B5A085FF8F_StaticFields
{
	SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* ___Value;
};
struct EmptyArray_1_t2984B8F74E4B1E6C047125D296C6C06779CA328D_StaticFields
{
	SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* ___Value;
};
struct EmptyArray_1_t21B6219CD4389EFB0E40E69BEC0AE2E54C1AA17A_StaticFields
{
	TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* ___Value;
};
struct EmptyArray_1_tA0524EFBAA750B3D1DCF60FE6F2B25DE798675AD_StaticFields
{
	UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* ___Value;
};
struct EmptyArray_1_t77BFDB090CFC6AE661834F0BD4ED43833F4F079D_StaticFields
{
	UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* ___Value;
};
struct EmptyArray_1_tA1B9390A54D6283BFF40643DC2A697126A16E63B_StaticFields
{
	UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* ___Value;
};
struct EmptyArray_1_tF91FBA61857F9D60B55FD121DEADC9788D1FE016_StaticFields
{
	Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* ___Value;
};
struct EmptyArray_1_tB610FBC2B87561A97224E274FC1699BCE50B2C60_StaticFields
{
	jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* ___Value;
};
struct EmptyArray_1_tA32003E1DE04BE9767FD72E5DC4FEA8D1B834759_StaticFields
{
	MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* ___Value;
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
struct Char_t521A6F19B456D956AF452D926C32709DC03D6B17_StaticFields
{
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___s_categoryForLatin1;
};
struct IntPtr_t_StaticFields
{
	intptr_t ___Zero;
};
struct Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_StaticFields
{
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___zeroMatrix;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___identityMatrix;
};
struct Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_StaticFields
{
	Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 ___identityQuaternion;
};
struct TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_StaticFields
{
	TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 ___invalid;
};
struct Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_StaticFields
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___zeroVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___oneVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___upVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___downVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___leftVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___rightVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___forwardVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___backVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___positiveInfinityVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___negativeInfinityVector;
};
struct MessagePackSerializerOptions_t6356E2266D4FA5A0334004939D1C37D105B23356_StaticFields
{
	Regex_tE773142C2BE45C5D362B0F815AFF831707A51772* ___AssemblyNameVersionSelectorRegex;
	HashSet_1_tEFC6605F7DE53F71946C33FD371E53C3100F2178* ___DisallowedTypes;
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
struct Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C  : public RuntimeArray
{
	ALIGN_FIELD (8) int32_t m_Items[1];

	inline int32_t GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline int32_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, int32_t value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline int32_t GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline int32_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, int32_t value)
	{
		m_Items[index] = value;
	}
};
struct Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D  : public RuntimeArray
{
	ALIGN_FIELD (8) int64_t m_Items[1];

	inline int64_t GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline int64_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, int64_t value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline int64_t GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline int64_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, int64_t value)
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
struct QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7  : public RuntimeArray
{
	ALIGN_FIELD (8) Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 m_Items[1];

	inline Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 value)
	{
		m_Items[index] = value;
	}
};
struct SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913  : public RuntimeArray
{
	ALIGN_FIELD (8) int8_t m_Items[1];

	inline int8_t GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline int8_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, int8_t value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline int8_t GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline int8_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, int8_t value)
	{
		m_Items[index] = value;
	}
};
struct SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C  : public RuntimeArray
{
	ALIGN_FIELD (8) float m_Items[1];

	inline float GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline float* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, float value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline float GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline float* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, float value)
	{
		m_Items[index] = value;
	}
};
struct UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83  : public RuntimeArray
{
	ALIGN_FIELD (8) uint16_t m_Items[1];

	inline uint16_t GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline uint16_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, uint16_t value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline uint16_t GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline uint16_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, uint16_t value)
	{
		m_Items[index] = value;
	}
};
struct UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA  : public RuntimeArray
{
	ALIGN_FIELD (8) uint32_t m_Items[1];

	inline uint32_t GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline uint32_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, uint32_t value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline uint32_t GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline uint32_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, uint32_t value)
	{
		m_Items[index] = value;
	}
};
struct UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299  : public RuntimeArray
{
	ALIGN_FIELD (8) uint64_t m_Items[1];

	inline uint64_t GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline uint64_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, uint64_t value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline uint64_t GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline uint64_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, uint64_t value)
	{
		m_Items[index] = value;
	}
};
struct Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C  : public RuntimeArray
{
	ALIGN_FIELD (8) Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 m_Items[1];

	inline Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 value)
	{
		m_Items[index] = value;
	}
};
struct __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC  : public RuntimeArray
{
	ALIGN_FIELD (8) uint8_t m_Items[1];

	inline uint8_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + il2cpp_array_calc_byte_offset(this, index);
	}
	inline uint8_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + il2cpp_array_calc_byte_offset(this, index);
	}
};
struct jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F  : public RuntimeArray
{
	ALIGN_FIELD (8) jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 m_Items[1];

	inline jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 value)
	{
		m_Items[index] = value;
	}
};
struct RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186  : public RuntimeArray
{
	ALIGN_FIELD (8) RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 m_Items[1];

	inline RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___Value), (void*)NULL);
	}
	inline RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___Value), (void*)NULL);
	}
};
struct Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F  : public RuntimeArray
{
	ALIGN_FIELD (8) int32_t m_Items[1];

	inline int32_t GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline int32_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, int32_t value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline int32_t GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline int32_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, int32_t value)
	{
		m_Items[index] = value;
	}
};
struct Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D  : public RuntimeArray
{
	ALIGN_FIELD (8) Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 m_Items[1];

	inline Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 value)
	{
		m_Items[index] = value;
	}
};
struct RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993  : public RuntimeArray
{
	ALIGN_FIELD (8) Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D m_Items[1];

	inline Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D value)
	{
		m_Items[index] = value;
	}
};
struct TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A  : public RuntimeArray
{
	ALIGN_FIELD (8) TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 m_Items[1];

	inline TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 value)
	{
		m_Items[index] = value;
	}
};
struct MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5  : public RuntimeArray
{
	ALIGN_FIELD (8) MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 m_Items[1];

	inline MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 value)
	{
		m_Items[index] = value;
	}
};


IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m87AB3C694F2E4802F14D006F21C020816045285F_gshared_inline (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Buffer_Memmove_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1CD5B4A82FDDB0C96C8ABC21339D0339688CEEAB_gshared (int32_t* ___0_destination, int32_t* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mA0D85386F3D3AAF59FC429C4A2A9E7CD6B7DCF2A_gshared_inline (ReadOnlySpan_1_t6190994DF094ABDFA6908C2C3FB347457E8E4282* __this, int32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m89B8042F831A4ACF35D15B29B8141AE29CFFDF84_gshared_inline (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, int32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* Array_Empty_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4D53E0E0F90F37AD5DBFD2DC75E52406F90C7ABC_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m176441CFA181B7C6097611CC13C24C5ED7F14CFF_gshared_inline (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___0_array, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m69C26EE24C8AB486BBD48D1BBB32574FB0B5CD91_gshared_inline (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Buffer_Memmove_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mEEF2A60C3462458756768283DF2A7C3591A6A6E4_gshared (int64_t* ___0_destination, int64_t* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_m3960AA4B8AD179BE83BBC4B4A67B3FB75BD6365A_gshared_inline (ReadOnlySpan_1_t6CE9C0CA1262A820428D86548CAE80352AAA12AC* __this, int64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m180D886432C6474539833ED47A77D2740E7FCD8A_gshared_inline (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, int64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* Array_Empty_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m4569050419CDB52F3B7303ED823142E9C0F12A6C_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m177A8208F7F1C1028420E224BF257E30597C717B_gshared_inline (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* ___0_array, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m0B5336E05EEAAE122DF68A3A82C2D4359A2BE33D_gshared_inline (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Buffer_Memmove_TisRuntimeObject_mD13EEA1538F26A567D15A7615D8851A427DFD15A_gshared (RuntimeObject** ___0_destination, RuntimeObject** ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mB44468A232EBDBBAC3914B3664064CE336BAF744_gshared_inline (ReadOnlySpan_1_t40ECE3A478A7988D6572F814C5099ACFDE1FDF95* __this, RuntimeObject** ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m143009D8D6D0129C1D86B783427FADE8A0E61936_gshared_inline (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, RuntimeObject** ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m8DB171982E2F26182BA531AC18EEAA27D52763EE_gshared_inline (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___0_array, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mFB84148EFC50BBB0940E474682C1649251F02576_gshared_inline (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Buffer_Memmove_TisQuaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_m0CD197E2DFBC77DF284D8F5CAD832F7CEAEBC8FA_gshared (Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* ___0_destination, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mA7A30FB7B805A8FB05709CD7C7D8C29E3F3C60CA_gshared_inline (ReadOnlySpan_1_tDE8983102D42568E6127FA7BE5CE63380CD7A820* __this, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mEB88C92980CFEB47188FEFC02935CCC0C1E57C7F_gshared_inline (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* Array_Empty_TisQuaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_mCE17AEB484914CF98BCAA9B85E19A2A027EE46C6_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mC41D37A4EC24675FE88E07FC2AD929915407785C_gshared_inline (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* ___0_array, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mF7BD3765E86D3ED55FB40E64A3D0F2D90EC0F908_gshared_inline (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Buffer_Memmove_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m8921CF9FB1C61F7FA656ADE11B64F27943551250_gshared (int8_t* ___0_destination, int8_t* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_m783D660E0BF03935E536537C3B32F79A7BD0FB42_gshared_inline (ReadOnlySpan_1_tB90CE592448A63B07B56F79364563BD361CF8CDC* __this, int8_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mA974AFDCD35D895EAD5E356146AD6C8682F2738A_gshared_inline (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, int8_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* Array_Empty_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mE1E6DB6377A6DCA206C1AB31B3964386D486F94F_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m08998C4090851C62879905634F774B4D4A1B3221_gshared_inline (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* ___0_array, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mED11128A130F01BC9C5F690BFFAEF38B2283751B_gshared_inline (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Buffer_Memmove_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m8E65A20A53C662400685CE50AE11C2A80FBC6D7C_gshared (float* ___0_destination, float* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mA93F2958FDB8B48F00F57C878275CF739DE6273B_gshared_inline (ReadOnlySpan_1_t9C2C8EDE84088EDC61AADD4CA3C2CDC72D135E3D* __this, float* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m1B5237AB31EC7CA6AB6981E961491177242F4A55_gshared_inline (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, float* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* Array_Empty_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m44F781E90531F7FCDB12BC4290CD4394A887FC06_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mA5EF3ABEDD5776174AAEF4D976B58B424F43B275_gshared_inline (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* ___0_array, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mD173AF2E3688317C8AB9621F7626A2A34DE8F56B_gshared_inline (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Buffer_Memmove_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mE48DFFFA4D52B03F4ACA304FD485E78F4BFF0E42_gshared (uint16_t* ___0_destination, uint16_t* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mCBA8EFCAA8102765E34B993A8177EE752D80890F_gshared_inline (ReadOnlySpan_1_tA2EFC117098BD2B38ADBF809AA976D9F3C13654F* __this, uint16_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m458FB27AA0EB3527A73C9D6305D452A062D2ABC4_gshared_inline (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, uint16_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* Array_Empty_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mA9FE4AE132DC76B02E8B39B5052CBA643CFF7220_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mC892A665B48BA9CD149DA76F26EA3607C7859792_gshared_inline (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* ___0_array, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mC3EBBD1CE9C5025EB30AFDE84FCCCFB3FE794EC5_gshared_inline (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Buffer_Memmove_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mEC6A6EF02BD38F45F23336F48D35B9DC2BC187FD_gshared (uint32_t* ___0_destination, uint32_t* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mFEB9E8BCBC125E065C80C12FC6037D87DC6FA2FC_gshared_inline (ReadOnlySpan_1_t57F4BBC957039E8E904443D25F3A78AE60DC94B4* __this, uint32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m44A796B80A3B7B5E228B0865F02AC548FA1E7567_gshared_inline (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, uint32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* Array_Empty_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_m8B0170B3C9368D9BDB90A666E2DF5549423C160C_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m1161A3B3850C22A54C838C62FB009355039C28ED_gshared_inline (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* ___0_array, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m62F446B5508B032A22CD1BA21D998626E6B8A5CC_gshared_inline (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Buffer_Memmove_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m4482A7BD91B51B22F471D785B1D22FAE110CD908_gshared (uint64_t* ___0_destination, uint64_t* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mC03446F4F48AEBD20B80591766ACE9CE79048BCE_gshared_inline (ReadOnlySpan_1_tD579EC24AE7548EFE16428C9DC0EE0423C0FDA0F* __this, uint64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m3C68BA129DD44A3486C6B57A99412947A16855F6_gshared_inline (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, uint64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* Array_Empty_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m696D3DE2D89DD613C8C0EB15BE627EABEF780255_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mC83E2FCB4929477ACCB736B25B9E2F89FF2A07CF_gshared_inline (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* ___0_array, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m556440A32F0A1EB65C95120CEC693B9AC4DFD52B_gshared_inline (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Buffer_Memmove_TisVector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_m5E3924B178120F38353E13C8B4545C90C49D6CED_gshared (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___0_destination, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_m682BD3D57C99591BB5B14A415A7F8F4D5B14241F_gshared_inline (ReadOnlySpan_1_t8C2C24B6B10C9EAC8D8A8C92BED569A12615C2AE* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m150E0E8C32CF01F22FFC539541D9F6EA05DB6967_gshared_inline (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* Array_Empty_TisVector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_mFE64AD477B9A8397B2CCC3FD2565DCA5B2F0A1F6_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mCC34AC618271C9F9C196DA22BA5DB8C6D8AD477D_gshared_inline (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* ___0_array, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mFA6EE52BCF39100AE30C79E73F0F972182D0CA2A_gshared_inline (ReadOnlySpan_1_tC416A5627E04F69CA2947A2A13F0A1DF096CABAC* __this, Il2CppFullySharedGenericAny* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mBA868F06359701D9950DEB1B10F52F848E9FF6DA_gshared_inline (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, Il2CppFullySharedGenericAny* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m94A95CF4DF158FDF992CC13DA185B637335D84C6_gshared_inline (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_array, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_gshared_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Buffer_Memmove_Tisjvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_m48D2E426B92CBE28782CF29BD703DF063CFF422D_gshared (jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* ___0_destination, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_m655719167577EE7B24CAAF5A9D0995E0259B6A84_gshared_inline (ReadOnlySpan_1_tEC1E786ADECA91AF68E558734868FBB77E05D964* __this, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m1DA3ED274C046791E48A52F415066C7C86B031D1_gshared_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* Array_Empty_Tisjvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_mD8B08917D1DC7B424036208C74F0A7A6EC83DAAE_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m13E8571165DB8DB1A1909B44B65D4F220890AC8F_gshared_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SparselyPopulatedArrayAddInfo_1__ctor_m323E378EC0EE0C48A24AA0E14E40D7B020BB0458_gshared (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* __this, SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* ___0_source, int32_t ___1_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* SparselyPopulatedArrayAddInfo_1_get_Source_mBA043CE79666AA955D0FE4335ED3B82802E75C86_gshared_inline (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t SparselyPopulatedArrayAddInfo_1_get_Index_mCBBF3F010A994612AC94DA417AB8D04A2C92B45A_gshared_inline (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SparselyPopulatedArrayFragment_1__ctor_m849B5F4632EC0E042F4CF4F23102F9D74CE14294_gshared (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* __this, int32_t ___0_size, SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* ___1_prev, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SparselyPopulatedArrayFragment_1__ctor_m1BAEA22A2C2FD0C50376CEBFC7F3A024EE3C302E_gshared (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* __this, int32_t ___0_size, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t SparselyPopulatedArrayFragment_1_get_Length_m2F77B48EDD934ED6586E559645752EB229677D27_gshared (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Nullable_1__ctor_m141FA88563AC0B5179132FB929EABD02C47FF703_gshared (Nullable_1_tCF32C56A2641879C053C86F273C0C6EC1B40BC28* __this, int32_t ___0_value, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* Array_Empty_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_m82361CB24EBE3B6C36CB18376DF3A1C7F6EE7228_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* EnumerableHelpers_ToArray_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_mE9205A140F2D330F1BD67CA68ECE924B2D0B0DA3_gshared (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_LastIndexOf_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_m101739DBA13BD2E5477EAF85123BA08D6DD13500_gshared (RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* ___0_array, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator__ctor_m1CA9573AF6376E9DE5D01340BC438666D124CB13_gshared (Enumerator_t6C87A4CFDE2206491F41A95637BE2F6F5222FABC* __this, Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* ___0_stack, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Array_Resize_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_mA42860625ED6BBDD2ACFC27AD9ABDE2680695217_gshared (RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m4C37721B5BEFB577136A37C100C0D1898A078EEE_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_m5677326E0EDDCB6AD5639B14C6BE13B2A75B57CB_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 ___0_item, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* EnumerableHelpers_ToArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEFD5911DDBDB2B3C1BCDAEE1DDF9EA8DCD3C5A22_gshared (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_LastIndexOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m603821E13AA067379EA5D21B577261CBB0BEF8E6_gshared (Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___0_array, int32_t ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator__ctor_m3D5138341817152CEF519175368AA02FBB797C96_gshared (Enumerator_t1ED2EFBA8997D05D3B6586A9FF9D467F8D5482F0* __this, Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* ___0_stack, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Array_Resize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m6BAA7BD6F22421B894347B1476C37052FAC6C916_gshared (Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_mA0C247058163BBAB7D8BFE60D542FFE434A3D834_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_m2E32959EAD54D6F02C9FF531A2AFDEF3EC04FEB6_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, int32_t ___0_item, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* Array_Empty_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m94E12BB613D748D2EEB9E1ABD961630D2F970385_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* EnumerableHelpers_ToArray_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_mEF42832E5D11AAAFF6F615CA9A8242D03F71539E_gshared (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_LastIndexOf_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m92EE93D981B3058A8384DCA90FFC1502242652F7_gshared (Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* ___0_array, int32_t ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator__ctor_m89062508E459D03177AC1ECE62116EFC56A6FB61_gshared (Enumerator_t0D7F1F6081F1B6DDB263573004129C443F04F41C* __this, Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* ___0_stack, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Array_Resize_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m540BB33A026A346B7B0033684BE811ED519B1E33_gshared (Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m652E1B38200FB5443D4A6688E81A1589AE0D10DC_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_mBA4873FC90B54D47FCDFAF825F1E462FA6F28560_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, int32_t ___0_item, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* Array_Empty_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m36408D2B54D6A6519FAF5E5AD03F49ECECA09F2E_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* EnumerableHelpers_ToArray_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m7FE7D9EE8E27D99A69409883CE6BE8EFE9795FD5_gshared (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_LastIndexOf_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_mFC821A119D3BD51B91A59F09A28A8FDC1C32F6FC_gshared (Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* ___0_array, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator__ctor_mB2E659404649FBEA780D53C3FA4E957998A349FB_gshared (Enumerator_t1999C0BADD971016559CDAF90DA9DC67FAEF5B8A* __this, Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* ___0_stack, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Array_Resize_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m45E2DE0DBF4E0547B569F9496A9D773B058C30CA_gshared (Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m8A1BE6CEDDAD8DB304FC2E141DA5635DB3B88338_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_m08841831A121C2488026F8131C34F8C15C250CC1_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___0_item, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* EnumerableHelpers_ToArray_TisRuntimeObject_m4BA4DBD38B4DA66F8DEB40393971473B983BE06A_gshared (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_LastIndexOf_TisRuntimeObject_m60B0AA749643DFCD60274810604B33A8A2CF0A40_gshared (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___0_array, RuntimeObject* ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator__ctor_m9F25E94BA06F0C82E9CE1E5A204D5C65DEA9DEAB_gshared (Enumerator_t852186DADC50D976C4BD8FE59C506354ED48B974* __this, Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* ___0_stack, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Array_Resize_TisRuntimeObject_mE8D92C287251BAF8256D85E5829F749359EC334E_gshared (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m44B37D35FD9357C7012A60D95B04AB96C0463EC7_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_mEC6D96B634757F6C1B0839B2B591944DF48C2B9B_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, RuntimeObject* ___0_item, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* Array_Empty_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_m85A5CFE26D5D127B03744735ED2B5FC30FCB1D59_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* EnumerableHelpers_ToArray_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_mB8308980C50019080799C439861F5EF700799906_gshared (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_LastIndexOf_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_mCEA7CE77FA45B1CB38444D71C46BC24E06A0ECA4_gshared (RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* ___0_array, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator__ctor_mF9CC77EDB16F67702CB0692CE3FB4BCA5EACC97E_gshared (Enumerator_t0757D472C179E7C385905619753AC7417A77B40A* __this, Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* ___0_stack, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Array_Resize_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_m2EC92F2F8F9EEAB25DF52C72C3C856D1ECA0C2A5_gshared (RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m9BB67CADD6555D0CECCD2652588673184823D4E6_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_m856B55C10DA17E80AD34FF3B276B07A80685EBFE_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___0_item, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* Array_Empty_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m3AE07A4CEAA2B2E2C2E03472BBF9F79BC799F948_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* EnumerableHelpers_ToArray_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m9C3FA1318E048265162E9AFA86DCE8A7619D2A70_gshared (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_LastIndexOf_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m5291A70BF372F7D73477976310C09A5B3BE49C90_gshared (TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* ___0_array, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator__ctor_mF766BF2C1792506A6941458B5EA1940528EAABA7_gshared (Enumerator_tE489CD82CEA07F70C1967F4E911F125AA65FD336* __this, Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* ___0_stack, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Array_Resize_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m980E6BCF46D049ECA55F63095E52D73F5C656788_gshared (TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_mB0004CD20AD9DFADDF410E929CF49B0FAFE7B78C_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_m067591A26AE7959E4430DD0FC3844D46AEAC3E90_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 ___0_item, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator__ctor_mD65EF492693E70B41695A031A49F72C7EFA82FCE_gshared (Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41* __this, Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* ___0_stack, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* Array_Empty_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m5BEAA64B8E1A4C758EDEA7D45C3177D7D8A6ADCF_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* EnumerableHelpers_ToArray_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m0933456EDFC7CBE2136969B0FD06280E7F0808E7_gshared (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_LastIndexOf_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_mFFFE5789A9BEF9E2ED789FB50EC9F52BB8954B28_gshared (MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* ___0_array, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator__ctor_m14E3BCC9341885A56233EF29D700857A00BBDFC7_gshared (Enumerator_t6B0EB72F34C4614A45F424103A9D11C74439E854* __this, Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* ___0_stack, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Array_Resize_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m4E7CB8B6701DF21BD6CA13920546C7869B4F6678_gshared (MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m89DEAB4CFF2C08C84D7B4988FD3A588F243CC50C_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_m046E8452EBCC5731C97F814735F12D1CC6714309_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 ___0_item, const RuntimeMethod* method) ;

IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint8_t* Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline (RuntimeArray* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SpanHelpers_ClearWithoutReferences_m65DB2925AE7A5FF88BB3EA1BF90513C9ADF0653D (uint8_t* ___0_b, uint64_t ___1_byteLength, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A (uint8_t* ___0_startAddress, uint8_t ___1_value, uint32_t ___2_byteCount, const RuntimeMethod* method) ;
inline int32_t Span_1_get_Length_m87AB3C694F2E4802F14D006F21C020816045285F_inline (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316*, const RuntimeMethod*))Span_1_get_Length_m87AB3C694F2E4802F14D006F21C020816045285F_gshared_inline)(__this, method);
}
inline void Buffer_Memmove_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1CD5B4A82FDDB0C96C8ABC21339D0339688CEEAB (int32_t* ___0_destination, int32_t* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method)
{
	((  void (*) (int32_t*, int32_t*, uint64_t, const RuntimeMethod*))Buffer_Memmove_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1CD5B4A82FDDB0C96C8ABC21339D0339688CEEAB_gshared)(___0_destination, ___1_source, ___2_elementCount, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D (const RuntimeMethod* method) ;
inline void ReadOnlySpan_1__ctor_mA0D85386F3D3AAF59FC429C4A2A9E7CD6B7DCF2A_inline (ReadOnlySpan_1_t6190994DF094ABDFA6908C2C3FB347457E8E4282* __this, int32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlySpan_1_t6190994DF094ABDFA6908C2C3FB347457E8E4282*, int32_t*, int32_t, const RuntimeMethod*))ReadOnlySpan_1__ctor_mA0D85386F3D3AAF59FC429C4A2A9E7CD6B7DCF2A_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57 (RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ___0_handle, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC (Type_t* ___0_left, Type_t* ___1_right, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2 (String_t* __this, Il2CppChar* ___0_value, int32_t ___1_startIndex, int32_t ___2_length, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987 (String_t* ___0_format, RuntimeObject* ___1_arg0, RuntimeObject* ___2_arg1, const RuntimeMethod* method) ;
inline void Span_1__ctor_m89B8042F831A4ACF35D15B29B8141AE29CFFDF84_inline (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, int32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316*, int32_t*, int32_t, const RuntimeMethod*))Span_1__ctor_m89B8042F831A4ACF35D15B29B8141AE29CFFDF84_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* Array_Empty_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4D53E0E0F90F37AD5DBFD2DC75E52406F90C7ABC_inline (const RuntimeMethod* method)
{
	return ((  Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* (*) (const RuntimeMethod*))Array_Empty_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4D53E0E0F90F37AD5DBFD2DC75E52406F90C7ABC_gshared_inline)(method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* __this, String_t* ___0_message, const RuntimeMethod* method) ;
inline void Span_1__ctor_m176441CFA181B7C6097611CC13C24C5ED7F14CFF_inline (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___0_array, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316*, Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C*, const RuntimeMethod*))Span_1__ctor_m176441CFA181B7C6097611CC13C24C5ED7F14CFF_gshared_inline)(__this, ___0_array, method);
}
inline int32_t Span_1_get_Length_m69C26EE24C8AB486BBD48D1BBB32574FB0B5CD91_inline (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955*, const RuntimeMethod*))Span_1_get_Length_m69C26EE24C8AB486BBD48D1BBB32574FB0B5CD91_gshared_inline)(__this, method);
}
inline void Buffer_Memmove_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mEEF2A60C3462458756768283DF2A7C3591A6A6E4 (int64_t* ___0_destination, int64_t* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method)
{
	((  void (*) (int64_t*, int64_t*, uint64_t, const RuntimeMethod*))Buffer_Memmove_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mEEF2A60C3462458756768283DF2A7C3591A6A6E4_gshared)(___0_destination, ___1_source, ___2_elementCount, method);
}
inline void ReadOnlySpan_1__ctor_m3960AA4B8AD179BE83BBC4B4A67B3FB75BD6365A_inline (ReadOnlySpan_1_t6CE9C0CA1262A820428D86548CAE80352AAA12AC* __this, int64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlySpan_1_t6CE9C0CA1262A820428D86548CAE80352AAA12AC*, int64_t*, int32_t, const RuntimeMethod*))ReadOnlySpan_1__ctor_m3960AA4B8AD179BE83BBC4B4A67B3FB75BD6365A_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline void Span_1__ctor_m180D886432C6474539833ED47A77D2740E7FCD8A_inline (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, int64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955*, int64_t*, int32_t, const RuntimeMethod*))Span_1__ctor_m180D886432C6474539833ED47A77D2740E7FCD8A_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* Array_Empty_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m4569050419CDB52F3B7303ED823142E9C0F12A6C_inline (const RuntimeMethod* method)
{
	return ((  Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* (*) (const RuntimeMethod*))Array_Empty_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m4569050419CDB52F3B7303ED823142E9C0F12A6C_gshared_inline)(method);
}
inline void Span_1__ctor_m177A8208F7F1C1028420E224BF257E30597C717B_inline (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* ___0_array, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955*, Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D*, const RuntimeMethod*))Span_1__ctor_m177A8208F7F1C1028420E224BF257E30597C717B_gshared_inline)(__this, ___0_array, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3 (RuntimeObject* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Type_op_Inequality_m83209C7BB3C05DFBEA3B6199B0BEFE8037301172 (Type_t* ___0_left, Type_t* ___1_right, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ThrowHelper_ThrowArrayTypeMismatchException_m781AD7A903FEA43FAE3137977E6BC5F9BAEBC590 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ThrowHelper_ThrowInvalidTypeWithPointersNotSupported_m5707DE408588F6EAC3FC7D10F9520308CF8C8CCF (Type_t* ___0_targetType, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t IntPtr_get_Size_m1FAAA59DA73D7E32BB1AB55DD92A90AFE3251DBE (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SpanHelpers_ClearWithReferences_m9641D8B6DC3AE81B4B0734BBA0E477EF131CD430 (intptr_t* ___0_ip, uint64_t ___1_pointerSizeLength, const RuntimeMethod* method) ;
inline int32_t Span_1_get_Length_m0B5336E05EEAAE122DF68A3A82C2D4359A2BE33D_inline (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2*, const RuntimeMethod*))Span_1_get_Length_m0B5336E05EEAAE122DF68A3A82C2D4359A2BE33D_gshared_inline)(__this, method);
}
inline void Buffer_Memmove_TisRuntimeObject_mD13EEA1538F26A567D15A7615D8851A427DFD15A (RuntimeObject** ___0_destination, RuntimeObject** ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method)
{
	((  void (*) (RuntimeObject**, RuntimeObject**, uint64_t, const RuntimeMethod*))Buffer_Memmove_TisRuntimeObject_mD13EEA1538F26A567D15A7615D8851A427DFD15A_gshared)(___0_destination, ___1_source, ___2_elementCount, method);
}
inline void ReadOnlySpan_1__ctor_mB44468A232EBDBBAC3914B3664064CE336BAF744_inline (ReadOnlySpan_1_t40ECE3A478A7988D6572F814C5099ACFDE1FDF95* __this, RuntimeObject** ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlySpan_1_t40ECE3A478A7988D6572F814C5099ACFDE1FDF95*, RuntimeObject**, int32_t, const RuntimeMethod*))ReadOnlySpan_1__ctor_mB44468A232EBDBBAC3914B3664064CE336BAF744_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline void Span_1__ctor_m143009D8D6D0129C1D86B783427FADE8A0E61936_inline (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, RuntimeObject** ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2*, RuntimeObject**, int32_t, const RuntimeMethod*))Span_1__ctor_m143009D8D6D0129C1D86B783427FADE8A0E61936_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_inline (const RuntimeMethod* method)
{
	return ((  ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* (*) (const RuntimeMethod*))Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_gshared_inline)(method);
}
inline void Span_1__ctor_m8DB171982E2F26182BA531AC18EEAA27D52763EE_inline (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___0_array, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))Span_1__ctor_m8DB171982E2F26182BA531AC18EEAA27D52763EE_gshared_inline)(__this, ___0_array, method);
}
inline int32_t Span_1_get_Length_mFB84148EFC50BBB0940E474682C1649251F02576_inline (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6*, const RuntimeMethod*))Span_1_get_Length_mFB84148EFC50BBB0940E474682C1649251F02576_gshared_inline)(__this, method);
}
inline void Buffer_Memmove_TisQuaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_m0CD197E2DFBC77DF284D8F5CAD832F7CEAEBC8FA (Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* ___0_destination, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method)
{
	((  void (*) (Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*, uint64_t, const RuntimeMethod*))Buffer_Memmove_TisQuaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_m0CD197E2DFBC77DF284D8F5CAD832F7CEAEBC8FA_gshared)(___0_destination, ___1_source, ___2_elementCount, method);
}
inline void ReadOnlySpan_1__ctor_mA7A30FB7B805A8FB05709CD7C7D8C29E3F3C60CA_inline (ReadOnlySpan_1_tDE8983102D42568E6127FA7BE5CE63380CD7A820* __this, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlySpan_1_tDE8983102D42568E6127FA7BE5CE63380CD7A820*, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*, int32_t, const RuntimeMethod*))ReadOnlySpan_1__ctor_mA7A30FB7B805A8FB05709CD7C7D8C29E3F3C60CA_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline void Span_1__ctor_mEB88C92980CFEB47188FEFC02935CCC0C1E57C7F_inline (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6*, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*, int32_t, const RuntimeMethod*))Span_1__ctor_mEB88C92980CFEB47188FEFC02935CCC0C1E57C7F_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* Array_Empty_TisQuaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_mCE17AEB484914CF98BCAA9B85E19A2A027EE46C6_inline (const RuntimeMethod* method)
{
	return ((  QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* (*) (const RuntimeMethod*))Array_Empty_TisQuaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_mCE17AEB484914CF98BCAA9B85E19A2A027EE46C6_gshared_inline)(method);
}
inline void Span_1__ctor_mC41D37A4EC24675FE88E07FC2AD929915407785C_inline (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* ___0_array, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6*, QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7*, const RuntimeMethod*))Span_1__ctor_mC41D37A4EC24675FE88E07FC2AD929915407785C_gshared_inline)(__this, ___0_array, method);
}
inline int32_t Span_1_get_Length_mF7BD3765E86D3ED55FB40E64A3D0F2D90EC0F908_inline (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03*, const RuntimeMethod*))Span_1_get_Length_mF7BD3765E86D3ED55FB40E64A3D0F2D90EC0F908_gshared_inline)(__this, method);
}
inline void Buffer_Memmove_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m8921CF9FB1C61F7FA656ADE11B64F27943551250 (int8_t* ___0_destination, int8_t* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method)
{
	((  void (*) (int8_t*, int8_t*, uint64_t, const RuntimeMethod*))Buffer_Memmove_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m8921CF9FB1C61F7FA656ADE11B64F27943551250_gshared)(___0_destination, ___1_source, ___2_elementCount, method);
}
inline void ReadOnlySpan_1__ctor_m783D660E0BF03935E536537C3B32F79A7BD0FB42_inline (ReadOnlySpan_1_tB90CE592448A63B07B56F79364563BD361CF8CDC* __this, int8_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlySpan_1_tB90CE592448A63B07B56F79364563BD361CF8CDC*, int8_t*, int32_t, const RuntimeMethod*))ReadOnlySpan_1__ctor_m783D660E0BF03935E536537C3B32F79A7BD0FB42_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline void Span_1__ctor_mA974AFDCD35D895EAD5E356146AD6C8682F2738A_inline (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, int8_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03*, int8_t*, int32_t, const RuntimeMethod*))Span_1__ctor_mA974AFDCD35D895EAD5E356146AD6C8682F2738A_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* Array_Empty_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mE1E6DB6377A6DCA206C1AB31B3964386D486F94F_inline (const RuntimeMethod* method)
{
	return ((  SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* (*) (const RuntimeMethod*))Array_Empty_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mE1E6DB6377A6DCA206C1AB31B3964386D486F94F_gshared_inline)(method);
}
inline void Span_1__ctor_m08998C4090851C62879905634F774B4D4A1B3221_inline (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* ___0_array, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03*, SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913*, const RuntimeMethod*))Span_1__ctor_m08998C4090851C62879905634F774B4D4A1B3221_gshared_inline)(__this, ___0_array, method);
}
inline int32_t Span_1_get_Length_mED11128A130F01BC9C5F690BFFAEF38B2283751B_inline (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C*, const RuntimeMethod*))Span_1_get_Length_mED11128A130F01BC9C5F690BFFAEF38B2283751B_gshared_inline)(__this, method);
}
inline void Buffer_Memmove_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m8E65A20A53C662400685CE50AE11C2A80FBC6D7C (float* ___0_destination, float* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method)
{
	((  void (*) (float*, float*, uint64_t, const RuntimeMethod*))Buffer_Memmove_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m8E65A20A53C662400685CE50AE11C2A80FBC6D7C_gshared)(___0_destination, ___1_source, ___2_elementCount, method);
}
inline void ReadOnlySpan_1__ctor_mA93F2958FDB8B48F00F57C878275CF739DE6273B_inline (ReadOnlySpan_1_t9C2C8EDE84088EDC61AADD4CA3C2CDC72D135E3D* __this, float* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlySpan_1_t9C2C8EDE84088EDC61AADD4CA3C2CDC72D135E3D*, float*, int32_t, const RuntimeMethod*))ReadOnlySpan_1__ctor_mA93F2958FDB8B48F00F57C878275CF739DE6273B_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline void Span_1__ctor_m1B5237AB31EC7CA6AB6981E961491177242F4A55_inline (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, float* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C*, float*, int32_t, const RuntimeMethod*))Span_1__ctor_m1B5237AB31EC7CA6AB6981E961491177242F4A55_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* Array_Empty_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m44F781E90531F7FCDB12BC4290CD4394A887FC06_inline (const RuntimeMethod* method)
{
	return ((  SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* (*) (const RuntimeMethod*))Array_Empty_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m44F781E90531F7FCDB12BC4290CD4394A887FC06_gshared_inline)(method);
}
inline void Span_1__ctor_mA5EF3ABEDD5776174AAEF4D976B58B424F43B275_inline (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* ___0_array, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C*, SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C*, const RuntimeMethod*))Span_1__ctor_mA5EF3ABEDD5776174AAEF4D976B58B424F43B275_gshared_inline)(__this, ___0_array, method);
}
inline int32_t Span_1_get_Length_mD173AF2E3688317C8AB9621F7626A2A34DE8F56B_inline (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D*, const RuntimeMethod*))Span_1_get_Length_mD173AF2E3688317C8AB9621F7626A2A34DE8F56B_gshared_inline)(__this, method);
}
inline void Buffer_Memmove_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mE48DFFFA4D52B03F4ACA304FD485E78F4BFF0E42 (uint16_t* ___0_destination, uint16_t* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method)
{
	((  void (*) (uint16_t*, uint16_t*, uint64_t, const RuntimeMethod*))Buffer_Memmove_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mE48DFFFA4D52B03F4ACA304FD485E78F4BFF0E42_gshared)(___0_destination, ___1_source, ___2_elementCount, method);
}
inline void ReadOnlySpan_1__ctor_mCBA8EFCAA8102765E34B993A8177EE752D80890F_inline (ReadOnlySpan_1_tA2EFC117098BD2B38ADBF809AA976D9F3C13654F* __this, uint16_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlySpan_1_tA2EFC117098BD2B38ADBF809AA976D9F3C13654F*, uint16_t*, int32_t, const RuntimeMethod*))ReadOnlySpan_1__ctor_mCBA8EFCAA8102765E34B993A8177EE752D80890F_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline void Span_1__ctor_m458FB27AA0EB3527A73C9D6305D452A062D2ABC4_inline (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, uint16_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D*, uint16_t*, int32_t, const RuntimeMethod*))Span_1__ctor_m458FB27AA0EB3527A73C9D6305D452A062D2ABC4_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* Array_Empty_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mA9FE4AE132DC76B02E8B39B5052CBA643CFF7220_inline (const RuntimeMethod* method)
{
	return ((  UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* (*) (const RuntimeMethod*))Array_Empty_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mA9FE4AE132DC76B02E8B39B5052CBA643CFF7220_gshared_inline)(method);
}
inline void Span_1__ctor_mC892A665B48BA9CD149DA76F26EA3607C7859792_inline (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* ___0_array, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D*, UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83*, const RuntimeMethod*))Span_1__ctor_mC892A665B48BA9CD149DA76F26EA3607C7859792_gshared_inline)(__this, ___0_array, method);
}
inline int32_t Span_1_get_Length_mC3EBBD1CE9C5025EB30AFDE84FCCCFB3FE794EC5_inline (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA*, const RuntimeMethod*))Span_1_get_Length_mC3EBBD1CE9C5025EB30AFDE84FCCCFB3FE794EC5_gshared_inline)(__this, method);
}
inline void Buffer_Memmove_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mEC6A6EF02BD38F45F23336F48D35B9DC2BC187FD (uint32_t* ___0_destination, uint32_t* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method)
{
	((  void (*) (uint32_t*, uint32_t*, uint64_t, const RuntimeMethod*))Buffer_Memmove_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mEC6A6EF02BD38F45F23336F48D35B9DC2BC187FD_gshared)(___0_destination, ___1_source, ___2_elementCount, method);
}
inline void ReadOnlySpan_1__ctor_mFEB9E8BCBC125E065C80C12FC6037D87DC6FA2FC_inline (ReadOnlySpan_1_t57F4BBC957039E8E904443D25F3A78AE60DC94B4* __this, uint32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlySpan_1_t57F4BBC957039E8E904443D25F3A78AE60DC94B4*, uint32_t*, int32_t, const RuntimeMethod*))ReadOnlySpan_1__ctor_mFEB9E8BCBC125E065C80C12FC6037D87DC6FA2FC_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline void Span_1__ctor_m44A796B80A3B7B5E228B0865F02AC548FA1E7567_inline (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, uint32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA*, uint32_t*, int32_t, const RuntimeMethod*))Span_1__ctor_m44A796B80A3B7B5E228B0865F02AC548FA1E7567_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* Array_Empty_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_m8B0170B3C9368D9BDB90A666E2DF5549423C160C_inline (const RuntimeMethod* method)
{
	return ((  UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* (*) (const RuntimeMethod*))Array_Empty_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_m8B0170B3C9368D9BDB90A666E2DF5549423C160C_gshared_inline)(method);
}
inline void Span_1__ctor_m1161A3B3850C22A54C838C62FB009355039C28ED_inline (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* ___0_array, const RuntimeMethod* method)
{
	((  void (*) (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA*, UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA*, const RuntimeMethod*))Span_1__ctor_m1161A3B3850C22A54C838C62FB009355039C28ED_gshared_inline)(__this, ___0_array, method);
}
inline int32_t Span_1_get_Length_m62F446B5508B032A22CD1BA21D998626E6B8A5CC_inline (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3*, const RuntimeMethod*))Span_1_get_Length_m62F446B5508B032A22CD1BA21D998626E6B8A5CC_gshared_inline)(__this, method);
}
inline void Buffer_Memmove_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m4482A7BD91B51B22F471D785B1D22FAE110CD908 (uint64_t* ___0_destination, uint64_t* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method)
{
	((  void (*) (uint64_t*, uint64_t*, uint64_t, const RuntimeMethod*))Buffer_Memmove_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m4482A7BD91B51B22F471D785B1D22FAE110CD908_gshared)(___0_destination, ___1_source, ___2_elementCount, method);
}
inline void ReadOnlySpan_1__ctor_mC03446F4F48AEBD20B80591766ACE9CE79048BCE_inline (ReadOnlySpan_1_tD579EC24AE7548EFE16428C9DC0EE0423C0FDA0F* __this, uint64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlySpan_1_tD579EC24AE7548EFE16428C9DC0EE0423C0FDA0F*, uint64_t*, int32_t, const RuntimeMethod*))ReadOnlySpan_1__ctor_mC03446F4F48AEBD20B80591766ACE9CE79048BCE_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline void Span_1__ctor_m3C68BA129DD44A3486C6B57A99412947A16855F6_inline (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, uint64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3*, uint64_t*, int32_t, const RuntimeMethod*))Span_1__ctor_m3C68BA129DD44A3486C6B57A99412947A16855F6_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* Array_Empty_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m696D3DE2D89DD613C8C0EB15BE627EABEF780255_inline (const RuntimeMethod* method)
{
	return ((  UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* (*) (const RuntimeMethod*))Array_Empty_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m696D3DE2D89DD613C8C0EB15BE627EABEF780255_gshared_inline)(method);
}
inline void Span_1__ctor_mC83E2FCB4929477ACCB736B25B9E2F89FF2A07CF_inline (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* ___0_array, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3*, UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299*, const RuntimeMethod*))Span_1__ctor_mC83E2FCB4929477ACCB736B25B9E2F89FF2A07CF_gshared_inline)(__this, ___0_array, method);
}
inline int32_t Span_1_get_Length_m556440A32F0A1EB65C95120CEC693B9AC4DFD52B_inline (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Span_1_t460D4E78469192619B0075C15897A6987393AAF9*, const RuntimeMethod*))Span_1_get_Length_m556440A32F0A1EB65C95120CEC693B9AC4DFD52B_gshared_inline)(__this, method);
}
inline void Buffer_Memmove_TisVector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_m5E3924B178120F38353E13C8B4545C90C49D6CED (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___0_destination, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method)
{
	((  void (*) (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*, uint64_t, const RuntimeMethod*))Buffer_Memmove_TisVector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_m5E3924B178120F38353E13C8B4545C90C49D6CED_gshared)(___0_destination, ___1_source, ___2_elementCount, method);
}
inline void ReadOnlySpan_1__ctor_m682BD3D57C99591BB5B14A415A7F8F4D5B14241F_inline (ReadOnlySpan_1_t8C2C24B6B10C9EAC8D8A8C92BED569A12615C2AE* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlySpan_1_t8C2C24B6B10C9EAC8D8A8C92BED569A12615C2AE*, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*, int32_t, const RuntimeMethod*))ReadOnlySpan_1__ctor_m682BD3D57C99591BB5B14A415A7F8F4D5B14241F_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline void Span_1__ctor_m150E0E8C32CF01F22FFC539541D9F6EA05DB6967_inline (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t460D4E78469192619B0075C15897A6987393AAF9*, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*, int32_t, const RuntimeMethod*))Span_1__ctor_m150E0E8C32CF01F22FFC539541D9F6EA05DB6967_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* Array_Empty_TisVector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_mFE64AD477B9A8397B2CCC3FD2565DCA5B2F0A1F6_inline (const RuntimeMethod* method)
{
	return ((  Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* (*) (const RuntimeMethod*))Array_Empty_TisVector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_mFE64AD477B9A8397B2CCC3FD2565DCA5B2F0A1F6_gshared_inline)(method);
}
inline void Span_1__ctor_mCC34AC618271C9F9C196DA22BA5DB8C6D8AD477D_inline (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* ___0_array, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t460D4E78469192619B0075C15897A6987393AAF9*, Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C*, const RuntimeMethod*))Span_1__ctor_mCC34AC618271C9F9C196DA22BA5DB8C6D8AD477D_gshared_inline)(__this, ___0_array, method);
}
inline void ReadOnlySpan_1__ctor_mFA6EE52BCF39100AE30C79E73F0F972182D0CA2A_inline (ReadOnlySpan_1_tC416A5627E04F69CA2947A2A13F0A1DF096CABAC* __this, Il2CppFullySharedGenericAny* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlySpan_1_tC416A5627E04F69CA2947A2A13F0A1DF096CABAC*, Il2CppFullySharedGenericAny*, int32_t, const RuntimeMethod*))ReadOnlySpan_1__ctor_mFA6EE52BCF39100AE30C79E73F0F972182D0CA2A_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline void Span_1__ctor_mBA868F06359701D9950DEB1B10F52F848E9FF6DA_inline (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, Il2CppFullySharedGenericAny* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54*, Il2CppFullySharedGenericAny*, int32_t, const RuntimeMethod*))Span_1__ctor_mBA868F06359701D9950DEB1B10F52F848E9FF6DA_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline void Span_1__ctor_m94A95CF4DF158FDF992CC13DA185B637335D84C6_inline (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_array, const RuntimeMethod* method)
{
	((  void (*) (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54*, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*, const RuntimeMethod*))Span_1__ctor_m94A95CF4DF158FDF992CC13DA185B637335D84C6_gshared_inline)(__this, ___0_array, method);
}
inline int32_t Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7*, const RuntimeMethod*))Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_gshared_inline)(__this, method);
}
inline void Buffer_Memmove_Tisjvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_m48D2E426B92CBE28782CF29BD703DF063CFF422D (jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* ___0_destination, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* ___1_source, uint64_t ___2_elementCount, const RuntimeMethod* method)
{
	((  void (*) (jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*, uint64_t, const RuntimeMethod*))Buffer_Memmove_Tisjvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_m48D2E426B92CBE28782CF29BD703DF063CFF422D_gshared)(___0_destination, ___1_source, ___2_elementCount, method);
}
inline void ReadOnlySpan_1__ctor_m655719167577EE7B24CAAF5A9D0995E0259B6A84_inline (ReadOnlySpan_1_tEC1E786ADECA91AF68E558734868FBB77E05D964* __this, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlySpan_1_tEC1E786ADECA91AF68E558734868FBB77E05D964*, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*, int32_t, const RuntimeMethod*))ReadOnlySpan_1__ctor_m655719167577EE7B24CAAF5A9D0995E0259B6A84_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline void Span_1__ctor_m1DA3ED274C046791E48A52F415066C7C86B031D1_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7*, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*, int32_t, const RuntimeMethod*))Span_1__ctor_m1DA3ED274C046791E48A52F415066C7C86B031D1_gshared_inline)(__this, ___0_ptr, ___1_length, method);
}
inline jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* Array_Empty_Tisjvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_mD8B08917D1DC7B424036208C74F0A7A6EC83DAAE_inline (const RuntimeMethod* method)
{
	return ((  jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* (*) (const RuntimeMethod*))Array_Empty_Tisjvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_mD8B08917D1DC7B424036208C74F0A7A6EC83DAAE_gshared_inline)(method);
}
inline void Span_1__ctor_m13E8571165DB8DB1A1909B44B65D4F220890AC8F_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* ___0_array, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7*, jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F*, const RuntimeMethod*))Span_1__ctor_m13E8571165DB8DB1A1909B44B65D4F220890AC8F_gshared_inline)(__this, ___0_array, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2 (RuntimeObject* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Monitor_Exit_m05B2CF037E2214B3208198C282490A2A475653FA (RuntimeObject* ___0_obj, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Monitor_Enter_m3CDB589DA1300B513D55FDCFB52B63E879794149 (RuntimeObject* ___0_obj, bool* ___1_lockTaken, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Array_Copy_m4233828B4E6288B6D815F539AAA38575DE627900 (RuntimeArray* ___0_sourceArray, RuntimeArray* ___1_destinationArray, int32_t ___2_length, const RuntimeMethod* method) ;
inline void SparselyPopulatedArrayAddInfo_1__ctor_m323E378EC0EE0C48A24AA0E14E40D7B020BB0458 (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* __this, SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	((  void (*) (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D*, SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC*, int32_t, const RuntimeMethod*))SparselyPopulatedArrayAddInfo_1__ctor_m323E378EC0EE0C48A24AA0E14E40D7B020BB0458_gshared)(__this, ___0_source, ___1_index, method);
}
inline SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* SparselyPopulatedArrayAddInfo_1_get_Source_mBA043CE79666AA955D0FE4335ED3B82802E75C86_inline (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* __this, const RuntimeMethod* method)
{
	return ((  SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* (*) (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D*, const RuntimeMethod*))SparselyPopulatedArrayAddInfo_1_get_Source_mBA043CE79666AA955D0FE4335ED3B82802E75C86_gshared_inline)(__this, method);
}
inline int32_t SparselyPopulatedArrayAddInfo_1_get_Index_mCBBF3F010A994612AC94DA417AB8D04A2C92B45A_inline (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D*, const RuntimeMethod*))SparselyPopulatedArrayAddInfo_1_get_Index_mCBBF3F010A994612AC94DA417AB8D04A2C92B45A_gshared_inline)(__this, method);
}
inline void SparselyPopulatedArrayFragment_1__ctor_m849B5F4632EC0E042F4CF4F23102F9D74CE14294 (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* __this, int32_t ___0_size, SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* ___1_prev, const RuntimeMethod* method)
{
	((  void (*) (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC*, int32_t, SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC*, const RuntimeMethod*))SparselyPopulatedArrayFragment_1__ctor_m849B5F4632EC0E042F4CF4F23102F9D74CE14294_gshared)(__this, ___0_size, ___1_prev, method);
}
inline void SparselyPopulatedArrayFragment_1__ctor_m1BAEA22A2C2FD0C50376CEBFC7F3A024EE3C302E (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* __this, int32_t ___0_size, const RuntimeMethod* method)
{
	((  void (*) (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC*, int32_t, const RuntimeMethod*))SparselyPopulatedArrayFragment_1__ctor_m1BAEA22A2C2FD0C50376CEBFC7F3A024EE3C302E_gshared)(__this, ___0_size, method);
}
inline int32_t SparselyPopulatedArrayFragment_1_get_Length_m2F77B48EDD934ED6586E559645752EB229677D27 (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC*, const RuntimeMethod*))SparselyPopulatedArrayFragment_1_get_Length_m2F77B48EDD934ED6586E559645752EB229677D27_gshared)(__this, method);
}
inline void Nullable_1__ctor_m141FA88563AC0B5179132FB929EABD02C47FF703 (Nullable_1_tCF32C56A2641879C053C86F273C0C6EC1B40BC28* __this, int32_t ___0_value, const RuntimeMethod* method)
{
	((  void (*) (Nullable_1_tCF32C56A2641879C053C86F273C0C6EC1B40BC28*, int32_t, const RuntimeMethod*))Nullable_1__ctor_m141FA88563AC0B5179132FB929EABD02C47FF703_gshared)(__this, ___0_value, method);
}
inline RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* Array_Empty_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_m82361CB24EBE3B6C36CB18376DF3A1C7F6EE7228_inline (const RuntimeMethod* method)
{
	return ((  RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* (*) (const RuntimeMethod*))Array_Empty_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_m82361CB24EBE3B6C36CB18376DF3A1C7F6EE7228_gshared_inline)(method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85 (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* __this, String_t* ___0_paramName, RuntimeObject* ___1_actualValue, String_t* ___2_message, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* __this, String_t* ___0_paramName, const RuntimeMethod* method) ;
inline RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* EnumerableHelpers_ToArray_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_mE9205A140F2D330F1BD67CA68ECE924B2D0B0DA3 (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method)
{
	return ((  RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* (*) (RuntimeObject*, int32_t*, const RuntimeMethod*))EnumerableHelpers_ToArray_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_mE9205A140F2D330F1BD67CA68ECE924B2D0B0DA3_gshared)(___0_source, ___1_length, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Array_Clear_m50BAA3751899858B097D3FF2ED31F284703FE5CB (RuntimeArray* ___0_array, int32_t ___1_index, int32_t ___2_length, const RuntimeMethod* method) ;
inline int32_t Array_LastIndexOf_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_m101739DBA13BD2E5477EAF85123BA08D6DD13500 (RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* ___0_array, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method)
{
	return ((  int32_t (*) (RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186*, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654, int32_t, const RuntimeMethod*))Array_LastIndexOf_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_m101739DBA13BD2E5477EAF85123BA08D6DD13500_gshared)(___0_array, ___1_value, ___2_startIndex, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465 (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* __this, String_t* ___0_message, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_get_Rank_m9383A200A2ECC89ECA44FE5F812ECFB874449C5F (RuntimeArray* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62 (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* __this, String_t* ___0_message, String_t* ___1_paramName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_GetLowerBound_m4FB0601E2E8A6304A42E3FC400576DF7B0F084BC (RuntimeArray* __this, int32_t ___0_dimension, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57 (RuntimeArray* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Array_Copy_mB4904E17BD92E320613A3251C0205E0786B3BF41 (RuntimeArray* ___0_sourceArray, int32_t ___1_sourceIndex, RuntimeArray* ___2_destinationArray, int32_t ___3_destinationIndex, int32_t ___4_length, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Array_Reverse_mE788006243D34C654D7DDEF13E2D9E7B129AF8AD (RuntimeArray* ___0_array, int32_t ___1_index, int32_t ___2_length, const RuntimeMethod* method) ;
inline void Enumerator__ctor_m1CA9573AF6376E9DE5D01340BC438666D124CB13 (Enumerator_t6C87A4CFDE2206491F41A95637BE2F6F5222FABC* __this, Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* ___0_stack, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_t6C87A4CFDE2206491F41A95637BE2F6F5222FABC*, Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67*, const RuntimeMethod*))Enumerator__ctor_m1CA9573AF6376E9DE5D01340BC438666D124CB13_gshared)(__this, ___0_stack, method);
}
inline void Array_Resize_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_mA42860625ED6BBDD2ACFC27AD9ABDE2680695217 (RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method)
{
	((  void (*) (RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186**, int32_t, const RuntimeMethod*))Array_Resize_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_mA42860625ED6BBDD2ACFC27AD9ABDE2680695217_gshared)(___0_array, ___1_newSize, method);
}
inline void Stack_1_ThrowForEmptyStack_m4C37721B5BEFB577136A37C100C0D1898A078EEE (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67*, const RuntimeMethod*))Stack_1_ThrowForEmptyStack_m4C37721B5BEFB577136A37C100C0D1898A078EEE_gshared)(__this, method);
}
inline void Stack_1_PushWithResize_m5677326E0EDDCB6AD5639B14C6BE13B2A75B57CB (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 ___0_item, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67*, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654, const RuntimeMethod*))Stack_1_PushWithResize_m5677326E0EDDCB6AD5639B14C6BE13B2A75B57CB_gshared)(__this, ___0_item, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void InvalidOperationException__ctor_mE4CB6F4712AB6D99A2358FBAE2E052B3EE976162 (InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB* __this, String_t* ___0_message, const RuntimeMethod* method) ;
inline Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* EnumerableHelpers_ToArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEFD5911DDBDB2B3C1BCDAEE1DDF9EA8DCD3C5A22 (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method)
{
	return ((  Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* (*) (RuntimeObject*, int32_t*, const RuntimeMethod*))EnumerableHelpers_ToArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEFD5911DDBDB2B3C1BCDAEE1DDF9EA8DCD3C5A22_gshared)(___0_source, ___1_length, method);
}
inline int32_t Array_LastIndexOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m603821E13AA067379EA5D21B577261CBB0BEF8E6 (Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___0_array, int32_t ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C*, int32_t, int32_t, const RuntimeMethod*))Array_LastIndexOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m603821E13AA067379EA5D21B577261CBB0BEF8E6_gshared)(___0_array, ___1_value, ___2_startIndex, method);
}
inline void Enumerator__ctor_m3D5138341817152CEF519175368AA02FBB797C96 (Enumerator_t1ED2EFBA8997D05D3B6586A9FF9D467F8D5482F0* __this, Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* ___0_stack, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_t1ED2EFBA8997D05D3B6586A9FF9D467F8D5482F0*, Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6*, const RuntimeMethod*))Enumerator__ctor_m3D5138341817152CEF519175368AA02FBB797C96_gshared)(__this, ___0_stack, method);
}
inline void Array_Resize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m6BAA7BD6F22421B894347B1476C37052FAC6C916 (Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method)
{
	((  void (*) (Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C**, int32_t, const RuntimeMethod*))Array_Resize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m6BAA7BD6F22421B894347B1476C37052FAC6C916_gshared)(___0_array, ___1_newSize, method);
}
inline void Stack_1_ThrowForEmptyStack_mA0C247058163BBAB7D8BFE60D542FFE434A3D834 (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6*, const RuntimeMethod*))Stack_1_ThrowForEmptyStack_mA0C247058163BBAB7D8BFE60D542FFE434A3D834_gshared)(__this, method);
}
inline void Stack_1_PushWithResize_m2E32959EAD54D6F02C9FF531A2AFDEF3EC04FEB6 (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, int32_t ___0_item, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6*, int32_t, const RuntimeMethod*))Stack_1_PushWithResize_m2E32959EAD54D6F02C9FF531A2AFDEF3EC04FEB6_gshared)(__this, ___0_item, method);
}
inline Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* Array_Empty_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m94E12BB613D748D2EEB9E1ABD961630D2F970385_inline (const RuntimeMethod* method)
{
	return ((  Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* (*) (const RuntimeMethod*))Array_Empty_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m94E12BB613D748D2EEB9E1ABD961630D2F970385_gshared_inline)(method);
}
inline Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* EnumerableHelpers_ToArray_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_mEF42832E5D11AAAFF6F615CA9A8242D03F71539E (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method)
{
	return ((  Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* (*) (RuntimeObject*, int32_t*, const RuntimeMethod*))EnumerableHelpers_ToArray_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_mEF42832E5D11AAAFF6F615CA9A8242D03F71539E_gshared)(___0_source, ___1_length, method);
}
inline int32_t Array_LastIndexOf_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m92EE93D981B3058A8384DCA90FFC1502242652F7 (Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* ___0_array, int32_t ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F*, int32_t, int32_t, const RuntimeMethod*))Array_LastIndexOf_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m92EE93D981B3058A8384DCA90FFC1502242652F7_gshared)(___0_array, ___1_value, ___2_startIndex, method);
}
inline void Enumerator__ctor_m89062508E459D03177AC1ECE62116EFC56A6FB61 (Enumerator_t0D7F1F6081F1B6DDB263573004129C443F04F41C* __this, Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* ___0_stack, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_t0D7F1F6081F1B6DDB263573004129C443F04F41C*, Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9*, const RuntimeMethod*))Enumerator__ctor_m89062508E459D03177AC1ECE62116EFC56A6FB61_gshared)(__this, ___0_stack, method);
}
inline void Array_Resize_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m540BB33A026A346B7B0033684BE811ED519B1E33 (Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method)
{
	((  void (*) (Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F**, int32_t, const RuntimeMethod*))Array_Resize_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m540BB33A026A346B7B0033684BE811ED519B1E33_gshared)(___0_array, ___1_newSize, method);
}
inline void Stack_1_ThrowForEmptyStack_m652E1B38200FB5443D4A6688E81A1589AE0D10DC (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9*, const RuntimeMethod*))Stack_1_ThrowForEmptyStack_m652E1B38200FB5443D4A6688E81A1589AE0D10DC_gshared)(__this, method);
}
inline void Stack_1_PushWithResize_mBA4873FC90B54D47FCDFAF825F1E462FA6F28560 (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, int32_t ___0_item, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9*, int32_t, const RuntimeMethod*))Stack_1_PushWithResize_mBA4873FC90B54D47FCDFAF825F1E462FA6F28560_gshared)(__this, ___0_item, method);
}
inline Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* Array_Empty_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m36408D2B54D6A6519FAF5E5AD03F49ECECA09F2E_inline (const RuntimeMethod* method)
{
	return ((  Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* (*) (const RuntimeMethod*))Array_Empty_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m36408D2B54D6A6519FAF5E5AD03F49ECECA09F2E_gshared_inline)(method);
}
inline Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* EnumerableHelpers_ToArray_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m7FE7D9EE8E27D99A69409883CE6BE8EFE9795FD5 (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method)
{
	return ((  Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* (*) (RuntimeObject*, int32_t*, const RuntimeMethod*))EnumerableHelpers_ToArray_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m7FE7D9EE8E27D99A69409883CE6BE8EFE9795FD5_gshared)(___0_source, ___1_length, method);
}
inline int32_t Array_LastIndexOf_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_mFC821A119D3BD51B91A59F09A28A8FDC1C32F6FC (Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* ___0_array, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D*, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6, int32_t, const RuntimeMethod*))Array_LastIndexOf_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_mFC821A119D3BD51B91A59F09A28A8FDC1C32F6FC_gshared)(___0_array, ___1_value, ___2_startIndex, method);
}
inline void Enumerator__ctor_mB2E659404649FBEA780D53C3FA4E957998A349FB (Enumerator_t1999C0BADD971016559CDAF90DA9DC67FAEF5B8A* __this, Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* ___0_stack, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_t1999C0BADD971016559CDAF90DA9DC67FAEF5B8A*, Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E*, const RuntimeMethod*))Enumerator__ctor_mB2E659404649FBEA780D53C3FA4E957998A349FB_gshared)(__this, ___0_stack, method);
}
inline void Array_Resize_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m45E2DE0DBF4E0547B569F9496A9D773B058C30CA (Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method)
{
	((  void (*) (Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D**, int32_t, const RuntimeMethod*))Array_Resize_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m45E2DE0DBF4E0547B569F9496A9D773B058C30CA_gshared)(___0_array, ___1_newSize, method);
}
inline void Stack_1_ThrowForEmptyStack_m8A1BE6CEDDAD8DB304FC2E141DA5635DB3B88338 (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E*, const RuntimeMethod*))Stack_1_ThrowForEmptyStack_m8A1BE6CEDDAD8DB304FC2E141DA5635DB3B88338_gshared)(__this, method);
}
inline void Stack_1_PushWithResize_m08841831A121C2488026F8131C34F8C15C250CC1 (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___0_item, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E*, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6, const RuntimeMethod*))Stack_1_PushWithResize_m08841831A121C2488026F8131C34F8C15C250CC1_gshared)(__this, ___0_item, method);
}
inline ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* EnumerableHelpers_ToArray_TisRuntimeObject_m4BA4DBD38B4DA66F8DEB40393971473B983BE06A (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method)
{
	return ((  ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* (*) (RuntimeObject*, int32_t*, const RuntimeMethod*))EnumerableHelpers_ToArray_TisRuntimeObject_m4BA4DBD38B4DA66F8DEB40393971473B983BE06A_gshared)(___0_source, ___1_length, method);
}
inline int32_t Array_LastIndexOf_TisRuntimeObject_m60B0AA749643DFCD60274810604B33A8A2CF0A40 (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___0_array, RuntimeObject* ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method)
{
	return ((  int32_t (*) (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, RuntimeObject*, int32_t, const RuntimeMethod*))Array_LastIndexOf_TisRuntimeObject_m60B0AA749643DFCD60274810604B33A8A2CF0A40_gshared)(___0_array, ___1_value, ___2_startIndex, method);
}
inline void Enumerator__ctor_m9F25E94BA06F0C82E9CE1E5A204D5C65DEA9DEAB (Enumerator_t852186DADC50D976C4BD8FE59C506354ED48B974* __this, Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* ___0_stack, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_t852186DADC50D976C4BD8FE59C506354ED48B974*, Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5*, const RuntimeMethod*))Enumerator__ctor_m9F25E94BA06F0C82E9CE1E5A204D5C65DEA9DEAB_gshared)(__this, ___0_stack, method);
}
inline void Array_Resize_TisRuntimeObject_mE8D92C287251BAF8256D85E5829F749359EC334E (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method)
{
	((  void (*) (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918**, int32_t, const RuntimeMethod*))Array_Resize_TisRuntimeObject_mE8D92C287251BAF8256D85E5829F749359EC334E_gshared)(___0_array, ___1_newSize, method);
}
inline void Stack_1_ThrowForEmptyStack_m44B37D35FD9357C7012A60D95B04AB96C0463EC7 (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5*, const RuntimeMethod*))Stack_1_ThrowForEmptyStack_m44B37D35FD9357C7012A60D95B04AB96C0463EC7_gshared)(__this, method);
}
inline void Stack_1_PushWithResize_mEC6D96B634757F6C1B0839B2B591944DF48C2B9B (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, RuntimeObject* ___0_item, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5*, RuntimeObject*, const RuntimeMethod*))Stack_1_PushWithResize_mEC6D96B634757F6C1B0839B2B591944DF48C2B9B_gshared)(__this, ___0_item, method);
}
inline RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* Array_Empty_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_m85A5CFE26D5D127B03744735ED2B5FC30FCB1D59_inline (const RuntimeMethod* method)
{
	return ((  RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* (*) (const RuntimeMethod*))Array_Empty_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_m85A5CFE26D5D127B03744735ED2B5FC30FCB1D59_gshared_inline)(method);
}
inline RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* EnumerableHelpers_ToArray_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_mB8308980C50019080799C439861F5EF700799906 (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method)
{
	return ((  RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* (*) (RuntimeObject*, int32_t*, const RuntimeMethod*))EnumerableHelpers_ToArray_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_mB8308980C50019080799C439861F5EF700799906_gshared)(___0_source, ___1_length, method);
}
inline int32_t Array_LastIndexOf_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_mCEA7CE77FA45B1CB38444D71C46BC24E06A0ECA4 (RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* ___0_array, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method)
{
	return ((  int32_t (*) (RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993*, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D, int32_t, const RuntimeMethod*))Array_LastIndexOf_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_mCEA7CE77FA45B1CB38444D71C46BC24E06A0ECA4_gshared)(___0_array, ___1_value, ___2_startIndex, method);
}
inline void Enumerator__ctor_mF9CC77EDB16F67702CB0692CE3FB4BCA5EACC97E (Enumerator_t0757D472C179E7C385905619753AC7417A77B40A* __this, Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* ___0_stack, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_t0757D472C179E7C385905619753AC7417A77B40A*, Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA*, const RuntimeMethod*))Enumerator__ctor_mF9CC77EDB16F67702CB0692CE3FB4BCA5EACC97E_gshared)(__this, ___0_stack, method);
}
inline void Array_Resize_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_m2EC92F2F8F9EEAB25DF52C72C3C856D1ECA0C2A5 (RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method)
{
	((  void (*) (RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993**, int32_t, const RuntimeMethod*))Array_Resize_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_m2EC92F2F8F9EEAB25DF52C72C3C856D1ECA0C2A5_gshared)(___0_array, ___1_newSize, method);
}
inline void Stack_1_ThrowForEmptyStack_m9BB67CADD6555D0CECCD2652588673184823D4E6 (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA*, const RuntimeMethod*))Stack_1_ThrowForEmptyStack_m9BB67CADD6555D0CECCD2652588673184823D4E6_gshared)(__this, method);
}
inline void Stack_1_PushWithResize_m856B55C10DA17E80AD34FF3B276B07A80685EBFE (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___0_item, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA*, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D, const RuntimeMethod*))Stack_1_PushWithResize_m856B55C10DA17E80AD34FF3B276B07A80685EBFE_gshared)(__this, ___0_item, method);
}
inline TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* Array_Empty_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m3AE07A4CEAA2B2E2C2E03472BBF9F79BC799F948_inline (const RuntimeMethod* method)
{
	return ((  TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* (*) (const RuntimeMethod*))Array_Empty_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m3AE07A4CEAA2B2E2C2E03472BBF9F79BC799F948_gshared_inline)(method);
}
inline TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* EnumerableHelpers_ToArray_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m9C3FA1318E048265162E9AFA86DCE8A7619D2A70 (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method)
{
	return ((  TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* (*) (RuntimeObject*, int32_t*, const RuntimeMethod*))EnumerableHelpers_ToArray_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m9C3FA1318E048265162E9AFA86DCE8A7619D2A70_gshared)(___0_source, ___1_length, method);
}
inline int32_t Array_LastIndexOf_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m5291A70BF372F7D73477976310C09A5B3BE49C90 (TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* ___0_array, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method)
{
	return ((  int32_t (*) (TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A*, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58, int32_t, const RuntimeMethod*))Array_LastIndexOf_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m5291A70BF372F7D73477976310C09A5B3BE49C90_gshared)(___0_array, ___1_value, ___2_startIndex, method);
}
inline void Enumerator__ctor_mF766BF2C1792506A6941458B5EA1940528EAABA7 (Enumerator_tE489CD82CEA07F70C1967F4E911F125AA65FD336* __this, Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* ___0_stack, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_tE489CD82CEA07F70C1967F4E911F125AA65FD336*, Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6*, const RuntimeMethod*))Enumerator__ctor_mF766BF2C1792506A6941458B5EA1940528EAABA7_gshared)(__this, ___0_stack, method);
}
inline void Array_Resize_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m980E6BCF46D049ECA55F63095E52D73F5C656788 (TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method)
{
	((  void (*) (TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A**, int32_t, const RuntimeMethod*))Array_Resize_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m980E6BCF46D049ECA55F63095E52D73F5C656788_gshared)(___0_array, ___1_newSize, method);
}
inline void Stack_1_ThrowForEmptyStack_mB0004CD20AD9DFADDF410E929CF49B0FAFE7B78C (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6*, const RuntimeMethod*))Stack_1_ThrowForEmptyStack_mB0004CD20AD9DFADDF410E929CF49B0FAFE7B78C_gshared)(__this, method);
}
inline void Stack_1_PushWithResize_m067591A26AE7959E4430DD0FC3844D46AEAC3E90 (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 ___0_item, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6*, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58, const RuntimeMethod*))Stack_1_PushWithResize_m067591A26AE7959E4430DD0FC3844D46AEAC3E90_gshared)(__this, ___0_item, method);
}
inline void Enumerator__ctor_mD65EF492693E70B41695A031A49F72C7EFA82FCE (Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41* __this, Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* ___0_stack, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41*, Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A*, const RuntimeMethod*))Enumerator__ctor_mD65EF492693E70B41695A031A49F72C7EFA82FCE_gshared)(__this, ___0_stack, method);
}
inline MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* Array_Empty_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m5BEAA64B8E1A4C758EDEA7D45C3177D7D8A6ADCF_inline (const RuntimeMethod* method)
{
	return ((  MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* (*) (const RuntimeMethod*))Array_Empty_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m5BEAA64B8E1A4C758EDEA7D45C3177D7D8A6ADCF_gshared_inline)(method);
}
inline MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* EnumerableHelpers_ToArray_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m0933456EDFC7CBE2136969B0FD06280E7F0808E7 (RuntimeObject* ___0_source, int32_t* ___1_length, const RuntimeMethod* method)
{
	return ((  MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* (*) (RuntimeObject*, int32_t*, const RuntimeMethod*))EnumerableHelpers_ToArray_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m0933456EDFC7CBE2136969B0FD06280E7F0808E7_gshared)(___0_source, ___1_length, method);
}
inline int32_t Array_LastIndexOf_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_mFFFE5789A9BEF9E2ED789FB50EC9F52BB8954B28 (MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* ___0_array, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 ___1_value, int32_t ___2_startIndex, const RuntimeMethod* method)
{
	return ((  int32_t (*) (MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5*, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253, int32_t, const RuntimeMethod*))Array_LastIndexOf_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_mFFFE5789A9BEF9E2ED789FB50EC9F52BB8954B28_gshared)(___0_array, ___1_value, ___2_startIndex, method);
}
inline void Enumerator__ctor_m14E3BCC9341885A56233EF29D700857A00BBDFC7 (Enumerator_t6B0EB72F34C4614A45F424103A9D11C74439E854* __this, Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* ___0_stack, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_t6B0EB72F34C4614A45F424103A9D11C74439E854*, Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA*, const RuntimeMethod*))Enumerator__ctor_m14E3BCC9341885A56233EF29D700857A00BBDFC7_gshared)(__this, ___0_stack, method);
}
inline void Array_Resize_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m4E7CB8B6701DF21BD6CA13920546C7869B4F6678 (MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5** ___0_array, int32_t ___1_newSize, const RuntimeMethod* method)
{
	((  void (*) (MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5**, int32_t, const RuntimeMethod*))Array_Resize_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m4E7CB8B6701DF21BD6CA13920546C7869B4F6678_gshared)(___0_array, ___1_newSize, method);
}
inline void Stack_1_ThrowForEmptyStack_m89DEAB4CFF2C08C84D7B4988FD3A588F243CC50C (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA*, const RuntimeMethod*))Stack_1_ThrowForEmptyStack_m89DEAB4CFF2C08C84D7B4988FD3A588F243CC50C_gshared)(__this, method);
}
inline void Stack_1_PushWithResize_m046E8452EBCC5731C97F814735F12D1CC6714309 (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 ___0_item, const RuntimeMethod* method)
{
	((  void (*) (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA*, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253, const RuntimeMethod*))Stack_1_PushWithResize_m046E8452EBCC5731C97F814735F12D1CC6714309_gshared)(__this, ___0_item, method);
}
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m176441CFA181B7C6097611CC13C24C5ED7F14CFF_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___0_array, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int32_t));
		goto IL_0037;
	}

IL_0037:
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		int32_t* L_4;
		L_4 = il2cpp_unsafe_as_ref<int32_t>(L_3);
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mE5D19FF7B2CED496CE41333FF842F490D1F14C03_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___0_array, int32_t ___1_start, int32_t ___2_length, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_1 = ___1_start;
		if (L_1)
		{
			goto IL_0009;
		}
	}
	{
		int32_t L_2 = ___2_length;
		if (!L_2)
		{
			goto IL_000e;
		}
	}

IL_0009:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316));
		return;
	}

IL_0016:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int32_t));
		goto IL_0042;
	}

IL_0042:
	{
		int32_t L_4 = ___1_start;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_5 = ___0_array;
		NullCheck(L_5);
		if ((!(((uint32_t)L_4) <= ((uint32_t)((int32_t)(((RuntimeArray*)L_5)->max_length))))))
		{
			goto IL_0050;
		}
	}
	{
		int32_t L_6 = ___2_length;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_7 = ___0_array;
		NullCheck(L_7);
		int32_t L_8 = ___1_start;
		if ((!(((uint32_t)L_6) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_7)->max_length)), L_8))))))
		{
			goto IL_0055;
		}
	}

IL_0050:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0055:
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_9 = ___0_array;
		NullCheck((RuntimeArray*)L_9);
		uint8_t* L_10;
		L_10 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_9, NULL);
		int32_t* L_11;
		L_11 = il2cpp_unsafe_as_ref<int32_t>(L_10);
		int32_t L_12 = ___1_start;
		int32_t* L_13;
		L_13 = il2cpp_unsafe_add<int32_t,int32_t>(L_11, L_12);
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_14;
		memset((&L_14), 0, sizeof(L_14));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_14), L_13);
		__this->____pointer = L_14;
		int32_t L_15 = ___2_length;
		__this->____length = L_15;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m31EE4A5510B5C504DB26DB281BC7D4179B859F2B_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_0 = ___1_length;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_1 = ___0_pointer;
		int32_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<int32_t>((uint8_t*)L_1);
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_3;
		memset((&L_3), 0, sizeof(L_3));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_3), L_2);
		__this->____pointer = L_3;
		int32_t L_4 = ___1_length;
		__this->____length = L_4;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m89B8042F831A4ACF35D15B29B8141AE29CFFDF84_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, int32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		int32_t* L_0 = ___0_ptr;
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t* Span_1_get_Item_m9272911ACF4FC0A82F6053A0DE22CEBC8C10D4E0_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_index;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) >= ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5(NULL);
	}

IL_000e:
	{
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_2 = __this->____pointer;
		V_0 = L_2;
		int32_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_index;
		int32_t* L_5;
		L_5 = il2cpp_unsafe_add<int32_t,int32_t>(L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t* Span_1_GetPinnableReference_mF920821F83971F1D7D3E554CAD596D5902754811_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, const RuntimeMethod* method) 
{
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_0010;
		}
	}
	{
		int32_t* L_1;
		L_1 = il2cpp_unsafe_as_ref<int32_t>((void*)((uintptr_t)0));
		return L_1;
	}

IL_0010:
	{
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_2 = __this->____pointer;
		V_0 = L_2;
		int32_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_0));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Clear_m36EEDEB219123208E625AC1446BC03AB5A21A001_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, const RuntimeMethod* method) 
{
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		goto IL_0034;
	}

IL_0034:
	{
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_0 = __this->____pointer;
		V_0 = L_0;
		int32_t* L_1;
		L_1 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_0));
		uint8_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint8_t>(L_1);
		int32_t L_3 = __this->____length;
		int32_t L_4;
		L_4 = il2cpp_unsafe_sizeof<int32_t>();
		SpanHelpers_ClearWithoutReferences_m65DB2925AE7A5FF88BB3EA1BF90513C9ADF0653D(L_2, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_3), ((int64_t)L_4))), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Fill_m0911D9EBB79D74E3F1442C095DEDB346CBE87340_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, int32_t ___0_value, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	int32_t V_1 = 0;
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 V_2;
	memset((&V_2), 0, sizeof(V_2));
	uint64_t V_3 = 0;
	int32_t* V_4 = NULL;
	uint64_t V_5 = 0;
	uint64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = il2cpp_unsafe_sizeof<int32_t>();
		if ((!(((uint32_t)L_0) == ((uint32_t)1))))
		{
			goto IL_0037;
		}
	}
	{
		int32_t L_1 = __this->____length;
		V_0 = (uint32_t)L_1;
		uint32_t L_2 = V_0;
		if (L_2)
		{
			goto IL_0013;
		}
	}
	{
		return;
	}

IL_0013:
	{
		int32_t L_3 = ___0_value;
		V_1 = L_3;
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_4 = __this->____pointer;
		V_2 = L_4;
		int32_t* L_5;
		L_5 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_2));
		uint8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint8_t>(L_5);
		uint8_t* L_7;
		L_7 = il2cpp_unsafe_as_ref<uint8_t>((&V_1));
		int32_t L_8 = *((uint8_t*)L_7);
		uint32_t L_9 = V_0;
		Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A(L_6, (uint8_t)L_8, L_9, NULL);
		return;
	}

IL_0037:
	{
		int32_t L_10 = __this->____length;
		V_3 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_10));
		uint64_t L_11 = V_3;
		if (L_11)
		{
			goto IL_0043;
		}
	}
	{
		return;
	}

IL_0043:
	{
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_12 = __this->____pointer;
		V_2 = L_12;
		int32_t* L_13;
		L_13 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_2));
		V_4 = L_13;
		int32_t L_14;
		L_14 = il2cpp_unsafe_sizeof<int32_t>();
		V_5 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_14));
		V_6 = (uint64_t)((int64_t)0);
		goto IL_0110;
	}

IL_0064:
	{
		int32_t* L_15 = V_4;
		uint64_t L_16 = V_6;
		uint64_t L_17 = V_5;
		int32_t* L_18;
		L_18 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_15, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_16, (int64_t)L_17)));
		int32_t L_19 = ___0_value;
		*(int32_t*)L_18 = L_19;
		int32_t* L_20 = V_4;
		uint64_t L_21 = V_6;
		uint64_t L_22 = V_5;
		int32_t* L_23;
		L_23 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_20, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_21, ((int64_t)1))), (int64_t)L_22)));
		int32_t L_24 = ___0_value;
		*(int32_t*)L_23 = L_24;
		int32_t* L_25 = V_4;
		uint64_t L_26 = V_6;
		uint64_t L_27 = V_5;
		int32_t* L_28;
		L_28 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_25, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_26, ((int64_t)2))), (int64_t)L_27)));
		int32_t L_29 = ___0_value;
		*(int32_t*)L_28 = L_29;
		int32_t* L_30 = V_4;
		uint64_t L_31 = V_6;
		uint64_t L_32 = V_5;
		int32_t* L_33;
		L_33 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_30, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_31, ((int64_t)3))), (int64_t)L_32)));
		int32_t L_34 = ___0_value;
		*(int32_t*)L_33 = L_34;
		int32_t* L_35 = V_4;
		uint64_t L_36 = V_6;
		uint64_t L_37 = V_5;
		int32_t* L_38;
		L_38 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_35, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_36, ((int64_t)4))), (int64_t)L_37)));
		int32_t L_39 = ___0_value;
		*(int32_t*)L_38 = L_39;
		int32_t* L_40 = V_4;
		uint64_t L_41 = V_6;
		uint64_t L_42 = V_5;
		int32_t* L_43;
		L_43 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_40, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_41, ((int64_t)5))), (int64_t)L_42)));
		int32_t L_44 = ___0_value;
		*(int32_t*)L_43 = L_44;
		int32_t* L_45 = V_4;
		uint64_t L_46 = V_6;
		uint64_t L_47 = V_5;
		int32_t* L_48;
		L_48 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_45, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_46, ((int64_t)6))), (int64_t)L_47)));
		int32_t L_49 = ___0_value;
		*(int32_t*)L_48 = L_49;
		int32_t* L_50 = V_4;
		uint64_t L_51 = V_6;
		uint64_t L_52 = V_5;
		int32_t* L_53;
		L_53 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_50, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_51, ((int64_t)7))), (int64_t)L_52)));
		int32_t L_54 = ___0_value;
		*(int32_t*)L_53 = L_54;
		uint64_t L_55 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_55, ((int64_t)8)));
	}

IL_0110:
	{
		uint64_t L_56 = V_6;
		uint64_t L_57 = V_3;
		if ((!(((uint64_t)L_56) >= ((uint64_t)((int64_t)((int64_t)L_57&((int64_t)((int32_t)-8))))))))
		{
			goto IL_0064;
		}
	}
	{
		uint64_t L_58 = V_6;
		uint64_t L_59 = V_3;
		if ((!(((uint64_t)L_58) < ((uint64_t)((int64_t)((int64_t)L_59&((int64_t)((int32_t)-4))))))))
		{
			goto IL_0198;
		}
	}
	{
		int32_t* L_60 = V_4;
		uint64_t L_61 = V_6;
		uint64_t L_62 = V_5;
		int32_t* L_63;
		L_63 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_60, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_61, (int64_t)L_62)));
		int32_t L_64 = ___0_value;
		*(int32_t*)L_63 = L_64;
		int32_t* L_65 = V_4;
		uint64_t L_66 = V_6;
		uint64_t L_67 = V_5;
		int32_t* L_68;
		L_68 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_65, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_66, ((int64_t)1))), (int64_t)L_67)));
		int32_t L_69 = ___0_value;
		*(int32_t*)L_68 = L_69;
		int32_t* L_70 = V_4;
		uint64_t L_71 = V_6;
		uint64_t L_72 = V_5;
		int32_t* L_73;
		L_73 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_70, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_71, ((int64_t)2))), (int64_t)L_72)));
		int32_t L_74 = ___0_value;
		*(int32_t*)L_73 = L_74;
		int32_t* L_75 = V_4;
		uint64_t L_76 = V_6;
		uint64_t L_77 = V_5;
		int32_t* L_78;
		L_78 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_75, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_76, ((int64_t)3))), (int64_t)L_77)));
		int32_t L_79 = ___0_value;
		*(int32_t*)L_78 = L_79;
		uint64_t L_80 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_80, ((int64_t)4)));
		goto IL_0198;
	}

IL_017f:
	{
		int32_t* L_81 = V_4;
		uint64_t L_82 = V_6;
		uint64_t L_83 = V_5;
		int32_t* L_84;
		L_84 = il2cpp_unsafe_add_byte_offset<int32_t,uint64_t>(L_81, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_82, (int64_t)L_83)));
		int32_t L_85 = ___0_value;
		*(int32_t*)L_84 = L_85;
		uint64_t L_86 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_86, ((int64_t)1)));
	}

IL_0198:
	{
		uint64_t L_87 = V_6;
		uint64_t L_88 = V_3;
		if ((!(((uint64_t)L_87) >= ((uint64_t)L_88))))
		{
			goto IL_017f;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_CopyTo_m197E47790117E2C925FE1A8E051A19AB9CF4260B_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 ___0_destination, const RuntimeMethod* method) 
{
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_m87AB3C694F2E4802F14D006F21C020816045285F_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0038;
		}
	}
	{
		Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 L_2 = ___0_destination;
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_3 = L_2.____pointer;
		V_0 = L_3;
		int32_t* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_0));
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_5 = __this->____pointer;
		V_0 = L_5;
		int32_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_0));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1CD5B4A82FDDB0C96C8ABC21339D0339688CEEAB(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return;
	}

IL_0038:
	{
		ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D(NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_TryCopyTo_m33CBE4497D24B50852F8C5C0924DFF38724969BD_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 ___0_destination, const RuntimeMethod* method) 
{
	bool V_0 = false;
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		V_0 = (bool)0;
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_m87AB3C694F2E4802F14D006F21C020816045285F_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_003b;
		}
	}
	{
		Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 L_2 = ___0_destination;
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_3 = L_2.____pointer;
		V_1 = L_3;
		int32_t* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_1));
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_5 = __this->____pointer;
		V_1 = L_5;
		int32_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_1));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1CD5B4A82FDDB0C96C8ABC21339D0339688CEEAB(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		V_0 = (bool)1;
	}

IL_003b:
	{
		bool L_8 = V_0;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlySpan_1_t6190994DF094ABDFA6908C2C3FB347457E8E4282 Span_1_op_Implicit_m2740023916201D5EB04C52CEB9FB3E175E79FE7A_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 ___0_span, const RuntimeMethod* method) 
{
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 L_0 = ___0_span;
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_1 = L_0.____pointer;
		V_0 = L_1;
		int32_t* L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_0));
		Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 L_3 = ___0_span;
		int32_t L_4 = L_3.____length;
		ReadOnlySpan_1_t6190994DF094ABDFA6908C2C3FB347457E8E4282 L_5;
		memset((&L_5), 0, sizeof(L_5));
		ReadOnlySpan_1__ctor_mA0D85386F3D3AAF59FC429C4A2A9E7CD6B7DCF2A_inline((&L_5), L_2, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 17));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Span_1_ToString_m71CB64722D92C563993B18D00317C1A3929D259B_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0DB46164953228904843938099AF66650313FEE5);
		s_Il2CppMethodInitialized = true;
	}
	Il2CppChar* V_0 = NULL;
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var) };
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		bool L_4;
		L_4 = Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_003e;
		}
	}
	{
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_5 = __this->____pointer;
		V_1 = L_5;
		int32_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_1));
		Il2CppChar* L_7;
		L_7 = il2cpp_unsafe_as_ref<Il2CppChar>(L_6);
		V_0 = L_7;
		Il2CppChar* L_8 = V_0;
		int32_t L_9 = __this->____length;
		String_t* L_10;
		L_10 = String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2(NULL, (Il2CppChar*)((uintptr_t)L_8), 0, L_9, NULL);
		return L_10;
	}

IL_003e:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck((MemberInfo_t*)L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(12, (MemberInfo_t*)L_12);
		int32_t L_14 = __this->____length;
		int32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral0DB46164953228904843938099AF66650313FEE5, (RuntimeObject*)L_13, L_16, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 Span_1_Slice_mEE3E0DF3B0F4D4D2A6CE3587C2919CD859EF4973_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, int32_t ___0_start, const RuntimeMethod* method) 
{
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) > ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_2 = __this->____pointer;
		V_0 = L_2;
		int32_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_start;
		int32_t* L_5;
		L_5 = il2cpp_unsafe_add<int32_t,int32_t>(L_3, L_4);
		int32_t L_6 = __this->____length;
		int32_t L_7 = ___0_start;
		Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 L_8;
		memset((&L_8), 0, sizeof(L_8));
		Span_1__ctor_m89B8042F831A4ACF35D15B29B8141AE29CFFDF84_inline((&L_8), L_5, ((int32_t)il2cpp_codegen_subtract(L_6, L_7)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 Span_1_Slice_m7586DA899BDF88591C3546C39E571CE889D6C098_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, int32_t ___0_start, int32_t ___1_length, const RuntimeMethod* method) 
{
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0014;
		}
	}
	{
		int32_t L_2 = ___1_length;
		int32_t L_3 = __this->____length;
		int32_t L_4 = ___0_start;
		if ((!(((uint32_t)L_2) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(L_3, L_4))))))
		{
			goto IL_0019;
		}
	}

IL_0014:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0019:
	{
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_5 = __this->____pointer;
		V_0 = L_5;
		int32_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_0));
		int32_t L_7 = ___0_start;
		int32_t* L_8;
		L_8 = il2cpp_unsafe_add<int32_t,int32_t>(L_6, L_7);
		int32_t L_9 = ___1_length;
		Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_m89B8042F831A4ACF35D15B29B8141AE29CFFDF84_inline((&L_10), L_8, L_9, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_10;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* Span_1_ToArray_m45051661AD085CCC9DDBA0E5926090B360668450_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, const RuntimeMethod* method) 
{
	ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_1;
		L_1 = Array_Empty_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4D53E0E0F90F37AD5DBFD2DC75E52406F90C7ABC_inline(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____length;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_3 = (Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C*)(Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C*)SZArrayNew(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 20), (uint32_t)L_2);
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_4 = L_3;
		NullCheck((RuntimeArray*)L_4);
		uint8_t* L_5;
		L_5 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_4, NULL);
		int32_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<int32_t>(L_5);
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_7 = __this->____pointer;
		V_0 = L_7;
		int32_t* L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(int32_t, (Il2CppByReference*)(&V_0));
		int32_t L_9 = __this->____length;
		Buffer_Memmove_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1CD5B4A82FDDB0C96C8ABC21339D0339688CEEAB(L_6, L_8, (uint64_t)((int64_t)L_9), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m87AB3C694F2E4802F14D006F21C020816045285F_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_Equals_m1756B3F9D59F21477044E6EE24B20B51BB216F31_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, RuntimeObject* ___0_obj, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_GetHashCode_mBB9141DEAC1EA44851C84E0A12B1A3136460B0D4_gshared (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 Span_1_op_Implicit_m75103E0CA16D9EEB5414F2FA9611149122CF23CC_gshared (Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___0_array, const RuntimeMethod* method) 
{
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_0 = ___0_array;
		Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316 L_1;
		memset((&L_1), 0, sizeof(L_1));
		Span_1__ctor_m176441CFA181B7C6097611CC13C24C5ED7F14CFF_inline((&L_1), L_0, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 21));
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m177A8208F7F1C1028420E224BF257E30597C717B_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* ___0_array, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	{
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int64_t));
		goto IL_0037;
	}

IL_0037:
	{
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		int64_t* L_4;
		L_4 = il2cpp_unsafe_as_ref<int64_t>(L_3);
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m63E0AC4D46B359D853FBD38A15D2C6D19ACC99DB_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* ___0_array, int32_t ___1_start, int32_t ___2_length, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	{
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_1 = ___1_start;
		if (L_1)
		{
			goto IL_0009;
		}
	}
	{
		int32_t L_2 = ___2_length;
		if (!L_2)
		{
			goto IL_000e;
		}
	}

IL_0009:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955));
		return;
	}

IL_0016:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int64_t));
		goto IL_0042;
	}

IL_0042:
	{
		int32_t L_4 = ___1_start;
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_5 = ___0_array;
		NullCheck(L_5);
		if ((!(((uint32_t)L_4) <= ((uint32_t)((int32_t)(((RuntimeArray*)L_5)->max_length))))))
		{
			goto IL_0050;
		}
	}
	{
		int32_t L_6 = ___2_length;
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_7 = ___0_array;
		NullCheck(L_7);
		int32_t L_8 = ___1_start;
		if ((!(((uint32_t)L_6) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_7)->max_length)), L_8))))))
		{
			goto IL_0055;
		}
	}

IL_0050:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0055:
	{
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_9 = ___0_array;
		NullCheck((RuntimeArray*)L_9);
		uint8_t* L_10;
		L_10 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_9, NULL);
		int64_t* L_11;
		L_11 = il2cpp_unsafe_as_ref<int64_t>(L_10);
		int32_t L_12 = ___1_start;
		int64_t* L_13;
		L_13 = il2cpp_unsafe_add<int64_t,int32_t>(L_11, L_12);
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_14;
		memset((&L_14), 0, sizeof(L_14));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_14), L_13);
		__this->____pointer = L_14;
		int32_t L_15 = ___2_length;
		__this->____length = L_15;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m57107B0971C215948970E813F419EC87CC0AB443_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_0 = ___1_length;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_1 = ___0_pointer;
		int64_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<int64_t>((uint8_t*)L_1);
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_3;
		memset((&L_3), 0, sizeof(L_3));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_3), L_2);
		__this->____pointer = L_3;
		int32_t L_4 = ___1_length;
		__this->____length = L_4;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m180D886432C6474539833ED47A77D2740E7FCD8A_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, int64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		int64_t* L_0 = ___0_ptr;
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t* Span_1_get_Item_m1D3E337979E0D03E72B7F9290DAF1ABAE018E913_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_index;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) >= ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5(NULL);
	}

IL_000e:
	{
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_2 = __this->____pointer;
		V_0 = L_2;
		int64_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_index;
		int64_t* L_5;
		L_5 = il2cpp_unsafe_add<int64_t,int32_t>(L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t* Span_1_GetPinnableReference_mF59782789C9CFAF154BCACF6AB27D8CFB84EDB44_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, const RuntimeMethod* method) 
{
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_0010;
		}
	}
	{
		int64_t* L_1;
		L_1 = il2cpp_unsafe_as_ref<int64_t>((void*)((uintptr_t)0));
		return L_1;
	}

IL_0010:
	{
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_2 = __this->____pointer;
		V_0 = L_2;
		int64_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_0));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Clear_m6864C2E74199530DEBBE8672C774EF8B41840091_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, const RuntimeMethod* method) 
{
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		goto IL_0034;
	}

IL_0034:
	{
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_0 = __this->____pointer;
		V_0 = L_0;
		int64_t* L_1;
		L_1 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_0));
		uint8_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint8_t>(L_1);
		int32_t L_3 = __this->____length;
		int32_t L_4;
		L_4 = il2cpp_unsafe_sizeof<int64_t>();
		SpanHelpers_ClearWithoutReferences_m65DB2925AE7A5FF88BB3EA1BF90513C9ADF0653D(L_2, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_3), ((int64_t)L_4))), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Fill_m76D13625B09398D11591FF340F0B94F788B32D2A_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, int64_t ___0_value, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	int64_t V_1 = 0;
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 V_2;
	memset((&V_2), 0, sizeof(V_2));
	uint64_t V_3 = 0;
	int64_t* V_4 = NULL;
	uint64_t V_5 = 0;
	uint64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = il2cpp_unsafe_sizeof<int64_t>();
		if ((!(((uint32_t)L_0) == ((uint32_t)1))))
		{
			goto IL_0037;
		}
	}
	{
		int32_t L_1 = __this->____length;
		V_0 = (uint32_t)L_1;
		uint32_t L_2 = V_0;
		if (L_2)
		{
			goto IL_0013;
		}
	}
	{
		return;
	}

IL_0013:
	{
		int64_t L_3 = ___0_value;
		V_1 = L_3;
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_4 = __this->____pointer;
		V_2 = L_4;
		int64_t* L_5;
		L_5 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_2));
		uint8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint8_t>(L_5);
		uint8_t* L_7;
		L_7 = il2cpp_unsafe_as_ref<uint8_t>((&V_1));
		int32_t L_8 = *((uint8_t*)L_7);
		uint32_t L_9 = V_0;
		Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A(L_6, (uint8_t)L_8, L_9, NULL);
		return;
	}

IL_0037:
	{
		int32_t L_10 = __this->____length;
		V_3 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_10));
		uint64_t L_11 = V_3;
		if (L_11)
		{
			goto IL_0043;
		}
	}
	{
		return;
	}

IL_0043:
	{
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_12 = __this->____pointer;
		V_2 = L_12;
		int64_t* L_13;
		L_13 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_2));
		V_4 = L_13;
		int32_t L_14;
		L_14 = il2cpp_unsafe_sizeof<int64_t>();
		V_5 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_14));
		V_6 = (uint64_t)((int64_t)0);
		goto IL_0110;
	}

IL_0064:
	{
		int64_t* L_15 = V_4;
		uint64_t L_16 = V_6;
		uint64_t L_17 = V_5;
		int64_t* L_18;
		L_18 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_15, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_16, (int64_t)L_17)));
		int64_t L_19 = ___0_value;
		*(int64_t*)L_18 = L_19;
		int64_t* L_20 = V_4;
		uint64_t L_21 = V_6;
		uint64_t L_22 = V_5;
		int64_t* L_23;
		L_23 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_20, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_21, ((int64_t)1))), (int64_t)L_22)));
		int64_t L_24 = ___0_value;
		*(int64_t*)L_23 = L_24;
		int64_t* L_25 = V_4;
		uint64_t L_26 = V_6;
		uint64_t L_27 = V_5;
		int64_t* L_28;
		L_28 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_25, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_26, ((int64_t)2))), (int64_t)L_27)));
		int64_t L_29 = ___0_value;
		*(int64_t*)L_28 = L_29;
		int64_t* L_30 = V_4;
		uint64_t L_31 = V_6;
		uint64_t L_32 = V_5;
		int64_t* L_33;
		L_33 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_30, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_31, ((int64_t)3))), (int64_t)L_32)));
		int64_t L_34 = ___0_value;
		*(int64_t*)L_33 = L_34;
		int64_t* L_35 = V_4;
		uint64_t L_36 = V_6;
		uint64_t L_37 = V_5;
		int64_t* L_38;
		L_38 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_35, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_36, ((int64_t)4))), (int64_t)L_37)));
		int64_t L_39 = ___0_value;
		*(int64_t*)L_38 = L_39;
		int64_t* L_40 = V_4;
		uint64_t L_41 = V_6;
		uint64_t L_42 = V_5;
		int64_t* L_43;
		L_43 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_40, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_41, ((int64_t)5))), (int64_t)L_42)));
		int64_t L_44 = ___0_value;
		*(int64_t*)L_43 = L_44;
		int64_t* L_45 = V_4;
		uint64_t L_46 = V_6;
		uint64_t L_47 = V_5;
		int64_t* L_48;
		L_48 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_45, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_46, ((int64_t)6))), (int64_t)L_47)));
		int64_t L_49 = ___0_value;
		*(int64_t*)L_48 = L_49;
		int64_t* L_50 = V_4;
		uint64_t L_51 = V_6;
		uint64_t L_52 = V_5;
		int64_t* L_53;
		L_53 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_50, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_51, ((int64_t)7))), (int64_t)L_52)));
		int64_t L_54 = ___0_value;
		*(int64_t*)L_53 = L_54;
		uint64_t L_55 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_55, ((int64_t)8)));
	}

IL_0110:
	{
		uint64_t L_56 = V_6;
		uint64_t L_57 = V_3;
		if ((!(((uint64_t)L_56) >= ((uint64_t)((int64_t)((int64_t)L_57&((int64_t)((int32_t)-8))))))))
		{
			goto IL_0064;
		}
	}
	{
		uint64_t L_58 = V_6;
		uint64_t L_59 = V_3;
		if ((!(((uint64_t)L_58) < ((uint64_t)((int64_t)((int64_t)L_59&((int64_t)((int32_t)-4))))))))
		{
			goto IL_0198;
		}
	}
	{
		int64_t* L_60 = V_4;
		uint64_t L_61 = V_6;
		uint64_t L_62 = V_5;
		int64_t* L_63;
		L_63 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_60, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_61, (int64_t)L_62)));
		int64_t L_64 = ___0_value;
		*(int64_t*)L_63 = L_64;
		int64_t* L_65 = V_4;
		uint64_t L_66 = V_6;
		uint64_t L_67 = V_5;
		int64_t* L_68;
		L_68 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_65, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_66, ((int64_t)1))), (int64_t)L_67)));
		int64_t L_69 = ___0_value;
		*(int64_t*)L_68 = L_69;
		int64_t* L_70 = V_4;
		uint64_t L_71 = V_6;
		uint64_t L_72 = V_5;
		int64_t* L_73;
		L_73 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_70, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_71, ((int64_t)2))), (int64_t)L_72)));
		int64_t L_74 = ___0_value;
		*(int64_t*)L_73 = L_74;
		int64_t* L_75 = V_4;
		uint64_t L_76 = V_6;
		uint64_t L_77 = V_5;
		int64_t* L_78;
		L_78 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_75, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_76, ((int64_t)3))), (int64_t)L_77)));
		int64_t L_79 = ___0_value;
		*(int64_t*)L_78 = L_79;
		uint64_t L_80 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_80, ((int64_t)4)));
		goto IL_0198;
	}

IL_017f:
	{
		int64_t* L_81 = V_4;
		uint64_t L_82 = V_6;
		uint64_t L_83 = V_5;
		int64_t* L_84;
		L_84 = il2cpp_unsafe_add_byte_offset<int64_t,uint64_t>(L_81, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_82, (int64_t)L_83)));
		int64_t L_85 = ___0_value;
		*(int64_t*)L_84 = L_85;
		uint64_t L_86 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_86, ((int64_t)1)));
	}

IL_0198:
	{
		uint64_t L_87 = V_6;
		uint64_t L_88 = V_3;
		if ((!(((uint64_t)L_87) >= ((uint64_t)L_88))))
		{
			goto IL_017f;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_CopyTo_m13F279BEC9B9BB1E2D4D06C1C93F44AECA0EEBD4_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 ___0_destination, const RuntimeMethod* method) 
{
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_m69C26EE24C8AB486BBD48D1BBB32574FB0B5CD91_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0038;
		}
	}
	{
		Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 L_2 = ___0_destination;
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_3 = L_2.____pointer;
		V_0 = L_3;
		int64_t* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_0));
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_5 = __this->____pointer;
		V_0 = L_5;
		int64_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_0));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mEEF2A60C3462458756768283DF2A7C3591A6A6E4(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return;
	}

IL_0038:
	{
		ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D(NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_TryCopyTo_mB819E6D9660E53A8964BEDACE758DBF652C060B0_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 ___0_destination, const RuntimeMethod* method) 
{
	bool V_0 = false;
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		V_0 = (bool)0;
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_m69C26EE24C8AB486BBD48D1BBB32574FB0B5CD91_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_003b;
		}
	}
	{
		Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 L_2 = ___0_destination;
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_3 = L_2.____pointer;
		V_1 = L_3;
		int64_t* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_1));
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_5 = __this->____pointer;
		V_1 = L_5;
		int64_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_1));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mEEF2A60C3462458756768283DF2A7C3591A6A6E4(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		V_0 = (bool)1;
	}

IL_003b:
	{
		bool L_8 = V_0;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlySpan_1_t6CE9C0CA1262A820428D86548CAE80352AAA12AC Span_1_op_Implicit_m4BB3A14D34CF739A36349066EDF43FA836F70DCE_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 ___0_span, const RuntimeMethod* method) 
{
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 L_0 = ___0_span;
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_1 = L_0.____pointer;
		V_0 = L_1;
		int64_t* L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_0));
		Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 L_3 = ___0_span;
		int32_t L_4 = L_3.____length;
		ReadOnlySpan_1_t6CE9C0CA1262A820428D86548CAE80352AAA12AC L_5;
		memset((&L_5), 0, sizeof(L_5));
		ReadOnlySpan_1__ctor_m3960AA4B8AD179BE83BBC4B4A67B3FB75BD6365A_inline((&L_5), L_2, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 17));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Span_1_ToString_m00EA5D946C52A9C065C59F3E5AE77FB049710EE2_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0DB46164953228904843938099AF66650313FEE5);
		s_Il2CppMethodInitialized = true;
	}
	Il2CppChar* V_0 = NULL;
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var) };
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		bool L_4;
		L_4 = Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_003e;
		}
	}
	{
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_5 = __this->____pointer;
		V_1 = L_5;
		int64_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_1));
		Il2CppChar* L_7;
		L_7 = il2cpp_unsafe_as_ref<Il2CppChar>(L_6);
		V_0 = L_7;
		Il2CppChar* L_8 = V_0;
		int32_t L_9 = __this->____length;
		String_t* L_10;
		L_10 = String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2(NULL, (Il2CppChar*)((uintptr_t)L_8), 0, L_9, NULL);
		return L_10;
	}

IL_003e:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck((MemberInfo_t*)L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(12, (MemberInfo_t*)L_12);
		int32_t L_14 = __this->____length;
		int32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral0DB46164953228904843938099AF66650313FEE5, (RuntimeObject*)L_13, L_16, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 Span_1_Slice_m1D3852AEAC35396DF476C94E7C9FE3B63480A63B_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, int32_t ___0_start, const RuntimeMethod* method) 
{
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) > ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_2 = __this->____pointer;
		V_0 = L_2;
		int64_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_start;
		int64_t* L_5;
		L_5 = il2cpp_unsafe_add<int64_t,int32_t>(L_3, L_4);
		int32_t L_6 = __this->____length;
		int32_t L_7 = ___0_start;
		Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 L_8;
		memset((&L_8), 0, sizeof(L_8));
		Span_1__ctor_m180D886432C6474539833ED47A77D2740E7FCD8A_inline((&L_8), L_5, ((int32_t)il2cpp_codegen_subtract(L_6, L_7)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 Span_1_Slice_m99112BD452022D83AFFBA2F394D3F7FC4143FE7D_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, int32_t ___0_start, int32_t ___1_length, const RuntimeMethod* method) 
{
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0014;
		}
	}
	{
		int32_t L_2 = ___1_length;
		int32_t L_3 = __this->____length;
		int32_t L_4 = ___0_start;
		if ((!(((uint32_t)L_2) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(L_3, L_4))))))
		{
			goto IL_0019;
		}
	}

IL_0014:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0019:
	{
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_5 = __this->____pointer;
		V_0 = L_5;
		int64_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_0));
		int32_t L_7 = ___0_start;
		int64_t* L_8;
		L_8 = il2cpp_unsafe_add<int64_t,int32_t>(L_6, L_7);
		int32_t L_9 = ___1_length;
		Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_m180D886432C6474539833ED47A77D2740E7FCD8A_inline((&L_10), L_8, L_9, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_10;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* Span_1_ToArray_m48970EF6F3DDB984D842A90662327A9D0A999E0E_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, const RuntimeMethod* method) 
{
	ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_1;
		L_1 = Array_Empty_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m4569050419CDB52F3B7303ED823142E9C0F12A6C_inline(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____length;
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_3 = (Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D*)(Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D*)SZArrayNew(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 20), (uint32_t)L_2);
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_4 = L_3;
		NullCheck((RuntimeArray*)L_4);
		uint8_t* L_5;
		L_5 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_4, NULL);
		int64_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<int64_t>(L_5);
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_7 = __this->____pointer;
		V_0 = L_7;
		int64_t* L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(int64_t, (Il2CppByReference*)(&V_0));
		int32_t L_9 = __this->____length;
		Buffer_Memmove_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mEEF2A60C3462458756768283DF2A7C3591A6A6E4(L_6, L_8, (uint64_t)((int64_t)L_9), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m69C26EE24C8AB486BBD48D1BBB32574FB0B5CD91_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_Equals_mCC4EC36ABDD3020E07A70D7B2BD57CDCC7EE1EE3_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, RuntimeObject* ___0_obj, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_GetHashCode_m732077720FEC16C4611EB21EE86797436F47663A_gshared (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 Span_1_op_Implicit_m715E874D8E8E8B7AA91823924610C5854E563216_gshared (Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* ___0_array, const RuntimeMethod* method) 
{
	{
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_0 = ___0_array;
		Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955 L_1;
		memset((&L_1), 0, sizeof(L_1));
		Span_1__ctor_m177A8208F7F1C1028420E224BF257E30597C717B_inline((&L_1), L_0, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 21));
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m8DB171982E2F26182BA531AC18EEAA27D52763EE_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___0_array, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	RuntimeObject* V_0 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(RuntimeObject*));
		RuntimeObject* L_1 = V_0;
		if (L_1)
		{
			goto IL_0037;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___0_array;
		NullCheck((RuntimeObject*)L_2);
		Type_t* L_3;
		L_3 = Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3((RuntimeObject*)L_2, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_4 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 3)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_5;
		L_5 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_4, NULL);
		bool L_6;
		L_6 = Type_op_Inequality_m83209C7BB3C05DFBEA3B6199B0BEFE8037301172(L_3, L_5, NULL);
		if (!L_6)
		{
			goto IL_0037;
		}
	}
	{
		ThrowHelper_ThrowArrayTypeMismatchException_m781AD7A903FEA43FAE3137977E6BC5F9BAEBC590(NULL);
	}

IL_0037:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_7 = ___0_array;
		NullCheck((RuntimeArray*)L_7);
		uint8_t* L_8;
		L_8 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_7, NULL);
		RuntimeObject** L_9;
		L_9 = il2cpp_unsafe_as_ref<RuntimeObject*>(L_8);
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_10;
		memset((&L_10), 0, sizeof(L_10));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_10), L_9);
		__this->____pointer = L_10;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_11 = ___0_array;
		NullCheck(L_11);
		__this->____length = ((int32_t)(((RuntimeArray*)L_11)->max_length));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mB699405722E96B949DE251894243517B81EDBBCD_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___0_array, int32_t ___1_start, int32_t ___2_length, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	RuntimeObject* V_0 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_1 = ___1_start;
		if (L_1)
		{
			goto IL_0009;
		}
	}
	{
		int32_t L_2 = ___2_length;
		if (!L_2)
		{
			goto IL_000e;
		}
	}

IL_0009:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2));
		return;
	}

IL_0016:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(RuntimeObject*));
		RuntimeObject* L_3 = V_0;
		if (L_3)
		{
			goto IL_0042;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = ___0_array;
		NullCheck((RuntimeObject*)L_4);
		Type_t* L_5;
		L_5 = Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3((RuntimeObject*)L_4, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_6 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 3)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_7;
		L_7 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_6, NULL);
		bool L_8;
		L_8 = Type_op_Inequality_m83209C7BB3C05DFBEA3B6199B0BEFE8037301172(L_5, L_7, NULL);
		if (!L_8)
		{
			goto IL_0042;
		}
	}
	{
		ThrowHelper_ThrowArrayTypeMismatchException_m781AD7A903FEA43FAE3137977E6BC5F9BAEBC590(NULL);
	}

IL_0042:
	{
		int32_t L_9 = ___1_start;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_10 = ___0_array;
		NullCheck(L_10);
		if ((!(((uint32_t)L_9) <= ((uint32_t)((int32_t)(((RuntimeArray*)L_10)->max_length))))))
		{
			goto IL_0050;
		}
	}
	{
		int32_t L_11 = ___2_length;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_12 = ___0_array;
		NullCheck(L_12);
		int32_t L_13 = ___1_start;
		if ((!(((uint32_t)L_11) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_12)->max_length)), L_13))))))
		{
			goto IL_0055;
		}
	}

IL_0050:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0055:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___0_array;
		NullCheck((RuntimeArray*)L_14);
		uint8_t* L_15;
		L_15 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_14, NULL);
		RuntimeObject** L_16;
		L_16 = il2cpp_unsafe_as_ref<RuntimeObject*>(L_15);
		int32_t L_17 = ___1_start;
		RuntimeObject** L_18;
		L_18 = il2cpp_unsafe_add<RuntimeObject*,int32_t>(L_16, L_17);
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_19;
		memset((&L_19), 0, sizeof(L_19));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_19), L_18);
		__this->____pointer = L_19;
		int32_t L_20 = ___2_length;
		__this->____length = L_20;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mD196947072A3F29E56F6B2320DBDE20828631A59_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
	}
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		ThrowHelper_ThrowInvalidTypeWithPointersNotSupported_m5707DE408588F6EAC3FC7D10F9520308CF8C8CCF(L_1, NULL);
	}

IL_0016:
	{
		int32_t L_2 = ___1_length;
		if ((((int32_t)L_2) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_3 = ___0_pointer;
		RuntimeObject** L_4;
		L_4 = il2cpp_unsafe_as_ref<RuntimeObject*>((uint8_t*)L_3);
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		int32_t L_6 = ___1_length;
		__this->____length = L_6;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m143009D8D6D0129C1D86B783427FADE8A0E61936_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, RuntimeObject** ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		RuntimeObject** L_0 = ___0_ptr;
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject** Span_1_get_Item_mED06F596E018FA53DB0B7F3BF0EB2A1CF5366945_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_index;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) >= ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5(NULL);
	}

IL_000e:
	{
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_2 = __this->____pointer;
		V_0 = L_2;
		RuntimeObject** L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_index;
		RuntimeObject** L_5;
		L_5 = il2cpp_unsafe_add<RuntimeObject*,int32_t>(L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject** Span_1_GetPinnableReference_m29C17737D2C5AADC90597FFA93A62E9D3743C3E2_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, const RuntimeMethod* method) 
{
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_0010;
		}
	}
	{
		RuntimeObject** L_1;
		L_1 = il2cpp_unsafe_as_ref<RuntimeObject*>((void*)((uintptr_t)0));
		return L_1;
	}

IL_0010:
	{
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_2 = __this->____pointer;
		V_0 = L_2;
		RuntimeObject** L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_0));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Clear_mE8440147924A10C96926501B80472DFE2576383A_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, const RuntimeMethod* method) 
{
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
	}
	{
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_0 = __this->____pointer;
		V_0 = L_0;
		RuntimeObject** L_1;
		L_1 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_0));
		intptr_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<intptr_t>(L_1);
		int32_t L_3 = __this->____length;
		int32_t L_4;
		L_4 = il2cpp_unsafe_sizeof<RuntimeObject*>();
		int32_t L_5;
		L_5 = IntPtr_get_Size_m1FAAA59DA73D7E32BB1AB55DD92A90AFE3251DBE(NULL);
		SpanHelpers_ClearWithReferences_m9641D8B6DC3AE81B4B0734BBA0E477EF131CD430(L_2, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_3), ((int64_t)((int32_t)(L_4/L_5))))), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Fill_m076FEFFBEE980474B0B4DDD1EB7ED878EF4B130B_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, RuntimeObject* ___0_value, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	RuntimeObject* V_1 = NULL;
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 V_2;
	memset((&V_2), 0, sizeof(V_2));
	uint64_t V_3 = 0;
	RuntimeObject** V_4 = NULL;
	uint64_t V_5 = 0;
	uint64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = il2cpp_unsafe_sizeof<RuntimeObject*>();
		if ((!(((uint32_t)L_0) == ((uint32_t)1))))
		{
			goto IL_0037;
		}
	}
	{
		int32_t L_1 = __this->____length;
		V_0 = (uint32_t)L_1;
		uint32_t L_2 = V_0;
		if (L_2)
		{
			goto IL_0013;
		}
	}
	{
		return;
	}

IL_0013:
	{
		RuntimeObject* L_3 = ___0_value;
		V_1 = L_3;
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_4 = __this->____pointer;
		V_2 = L_4;
		RuntimeObject** L_5;
		L_5 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_2));
		uint8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint8_t>(L_5);
		uint8_t* L_7;
		L_7 = il2cpp_unsafe_as_ref<uint8_t>((&V_1));
		int32_t L_8 = *((uint8_t*)L_7);
		uint32_t L_9 = V_0;
		Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A(L_6, (uint8_t)L_8, L_9, NULL);
		return;
	}

IL_0037:
	{
		int32_t L_10 = __this->____length;
		V_3 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_10));
		uint64_t L_11 = V_3;
		if (L_11)
		{
			goto IL_0043;
		}
	}
	{
		return;
	}

IL_0043:
	{
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_12 = __this->____pointer;
		V_2 = L_12;
		RuntimeObject** L_13;
		L_13 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_2));
		V_4 = L_13;
		int32_t L_14;
		L_14 = il2cpp_unsafe_sizeof<RuntimeObject*>();
		V_5 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_14));
		V_6 = (uint64_t)((int64_t)0);
		goto IL_0110;
	}

IL_0064:
	{
		RuntimeObject** L_15 = V_4;
		uint64_t L_16 = V_6;
		uint64_t L_17 = V_5;
		RuntimeObject** L_18;
		L_18 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_15, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_16, (int64_t)L_17)));
		RuntimeObject* L_19 = ___0_value;
		*(RuntimeObject**)L_18 = L_19;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_18, (void*)L_19);
		RuntimeObject** L_20 = V_4;
		uint64_t L_21 = V_6;
		uint64_t L_22 = V_5;
		RuntimeObject** L_23;
		L_23 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_20, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_21, ((int64_t)1))), (int64_t)L_22)));
		RuntimeObject* L_24 = ___0_value;
		*(RuntimeObject**)L_23 = L_24;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_23, (void*)L_24);
		RuntimeObject** L_25 = V_4;
		uint64_t L_26 = V_6;
		uint64_t L_27 = V_5;
		RuntimeObject** L_28;
		L_28 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_25, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_26, ((int64_t)2))), (int64_t)L_27)));
		RuntimeObject* L_29 = ___0_value;
		*(RuntimeObject**)L_28 = L_29;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_28, (void*)L_29);
		RuntimeObject** L_30 = V_4;
		uint64_t L_31 = V_6;
		uint64_t L_32 = V_5;
		RuntimeObject** L_33;
		L_33 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_30, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_31, ((int64_t)3))), (int64_t)L_32)));
		RuntimeObject* L_34 = ___0_value;
		*(RuntimeObject**)L_33 = L_34;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_33, (void*)L_34);
		RuntimeObject** L_35 = V_4;
		uint64_t L_36 = V_6;
		uint64_t L_37 = V_5;
		RuntimeObject** L_38;
		L_38 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_35, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_36, ((int64_t)4))), (int64_t)L_37)));
		RuntimeObject* L_39 = ___0_value;
		*(RuntimeObject**)L_38 = L_39;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_38, (void*)L_39);
		RuntimeObject** L_40 = V_4;
		uint64_t L_41 = V_6;
		uint64_t L_42 = V_5;
		RuntimeObject** L_43;
		L_43 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_40, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_41, ((int64_t)5))), (int64_t)L_42)));
		RuntimeObject* L_44 = ___0_value;
		*(RuntimeObject**)L_43 = L_44;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_43, (void*)L_44);
		RuntimeObject** L_45 = V_4;
		uint64_t L_46 = V_6;
		uint64_t L_47 = V_5;
		RuntimeObject** L_48;
		L_48 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_45, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_46, ((int64_t)6))), (int64_t)L_47)));
		RuntimeObject* L_49 = ___0_value;
		*(RuntimeObject**)L_48 = L_49;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_48, (void*)L_49);
		RuntimeObject** L_50 = V_4;
		uint64_t L_51 = V_6;
		uint64_t L_52 = V_5;
		RuntimeObject** L_53;
		L_53 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_50, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_51, ((int64_t)7))), (int64_t)L_52)));
		RuntimeObject* L_54 = ___0_value;
		*(RuntimeObject**)L_53 = L_54;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_53, (void*)L_54);
		uint64_t L_55 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_55, ((int64_t)8)));
	}

IL_0110:
	{
		uint64_t L_56 = V_6;
		uint64_t L_57 = V_3;
		if ((!(((uint64_t)L_56) >= ((uint64_t)((int64_t)((int64_t)L_57&((int64_t)((int32_t)-8))))))))
		{
			goto IL_0064;
		}
	}
	{
		uint64_t L_58 = V_6;
		uint64_t L_59 = V_3;
		if ((!(((uint64_t)L_58) < ((uint64_t)((int64_t)((int64_t)L_59&((int64_t)((int32_t)-4))))))))
		{
			goto IL_0198;
		}
	}
	{
		RuntimeObject** L_60 = V_4;
		uint64_t L_61 = V_6;
		uint64_t L_62 = V_5;
		RuntimeObject** L_63;
		L_63 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_60, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_61, (int64_t)L_62)));
		RuntimeObject* L_64 = ___0_value;
		*(RuntimeObject**)L_63 = L_64;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_63, (void*)L_64);
		RuntimeObject** L_65 = V_4;
		uint64_t L_66 = V_6;
		uint64_t L_67 = V_5;
		RuntimeObject** L_68;
		L_68 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_65, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_66, ((int64_t)1))), (int64_t)L_67)));
		RuntimeObject* L_69 = ___0_value;
		*(RuntimeObject**)L_68 = L_69;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_68, (void*)L_69);
		RuntimeObject** L_70 = V_4;
		uint64_t L_71 = V_6;
		uint64_t L_72 = V_5;
		RuntimeObject** L_73;
		L_73 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_70, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_71, ((int64_t)2))), (int64_t)L_72)));
		RuntimeObject* L_74 = ___0_value;
		*(RuntimeObject**)L_73 = L_74;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_73, (void*)L_74);
		RuntimeObject** L_75 = V_4;
		uint64_t L_76 = V_6;
		uint64_t L_77 = V_5;
		RuntimeObject** L_78;
		L_78 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_75, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_76, ((int64_t)3))), (int64_t)L_77)));
		RuntimeObject* L_79 = ___0_value;
		*(RuntimeObject**)L_78 = L_79;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_78, (void*)L_79);
		uint64_t L_80 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_80, ((int64_t)4)));
		goto IL_0198;
	}

IL_017f:
	{
		RuntimeObject** L_81 = V_4;
		uint64_t L_82 = V_6;
		uint64_t L_83 = V_5;
		RuntimeObject** L_84;
		L_84 = il2cpp_unsafe_add_byte_offset<RuntimeObject*,uint64_t>(L_81, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_82, (int64_t)L_83)));
		RuntimeObject* L_85 = ___0_value;
		*(RuntimeObject**)L_84 = L_85;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_84, (void*)L_85);
		uint64_t L_86 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_86, ((int64_t)1)));
	}

IL_0198:
	{
		uint64_t L_87 = V_6;
		uint64_t L_88 = V_3;
		if ((!(((uint64_t)L_87) >= ((uint64_t)L_88))))
		{
			goto IL_017f;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_CopyTo_mF4CD91CD5D3EBBD434B5F9CC3F4F91A37AB5984D_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 ___0_destination, const RuntimeMethod* method) 
{
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_m0B5336E05EEAAE122DF68A3A82C2D4359A2BE33D_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0038;
		}
	}
	{
		Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 L_2 = ___0_destination;
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_3 = L_2.____pointer;
		V_0 = L_3;
		RuntimeObject** L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_0));
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_5 = __this->____pointer;
		V_0 = L_5;
		RuntimeObject** L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_0));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisRuntimeObject_mD13EEA1538F26A567D15A7615D8851A427DFD15A(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return;
	}

IL_0038:
	{
		ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D(NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_TryCopyTo_m807B35740814FA9FAD0A8734F45019B6BC35C8A9_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 ___0_destination, const RuntimeMethod* method) 
{
	bool V_0 = false;
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		V_0 = (bool)0;
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_m0B5336E05EEAAE122DF68A3A82C2D4359A2BE33D_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_003b;
		}
	}
	{
		Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 L_2 = ___0_destination;
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_3 = L_2.____pointer;
		V_1 = L_3;
		RuntimeObject** L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_1));
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_5 = __this->____pointer;
		V_1 = L_5;
		RuntimeObject** L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_1));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisRuntimeObject_mD13EEA1538F26A567D15A7615D8851A427DFD15A(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		V_0 = (bool)1;
	}

IL_003b:
	{
		bool L_8 = V_0;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlySpan_1_t40ECE3A478A7988D6572F814C5099ACFDE1FDF95 Span_1_op_Implicit_mD53EB4D25C23E1572636C01DFEAF1A9EF657DE7D_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 ___0_span, const RuntimeMethod* method) 
{
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 L_0 = ___0_span;
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_1 = L_0.____pointer;
		V_0 = L_1;
		RuntimeObject** L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_0));
		Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 L_3 = ___0_span;
		int32_t L_4 = L_3.____length;
		ReadOnlySpan_1_t40ECE3A478A7988D6572F814C5099ACFDE1FDF95 L_5;
		memset((&L_5), 0, sizeof(L_5));
		ReadOnlySpan_1__ctor_mB44468A232EBDBBAC3914B3664064CE336BAF744_inline((&L_5), L_2, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 17));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Span_1_ToString_m3C0BC7C800AE3D95D69AA8D0639E23C8A0EB3942_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0DB46164953228904843938099AF66650313FEE5);
		s_Il2CppMethodInitialized = true;
	}
	Il2CppChar* V_0 = NULL;
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var) };
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		bool L_4;
		L_4 = Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_003e;
		}
	}
	{
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_5 = __this->____pointer;
		V_1 = L_5;
		RuntimeObject** L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_1));
		Il2CppChar* L_7;
		L_7 = il2cpp_unsafe_as_ref<Il2CppChar>(L_6);
		V_0 = L_7;
		Il2CppChar* L_8 = V_0;
		int32_t L_9 = __this->____length;
		String_t* L_10;
		L_10 = String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2(NULL, (Il2CppChar*)((uintptr_t)L_8), 0, L_9, NULL);
		return L_10;
	}

IL_003e:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck((MemberInfo_t*)L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(12, (MemberInfo_t*)L_12);
		int32_t L_14 = __this->____length;
		int32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral0DB46164953228904843938099AF66650313FEE5, (RuntimeObject*)L_13, L_16, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 Span_1_Slice_mFB084B3E2F7E74E9C6D0249A25D7AF1DAFA37B00_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, int32_t ___0_start, const RuntimeMethod* method) 
{
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) > ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_2 = __this->____pointer;
		V_0 = L_2;
		RuntimeObject** L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_start;
		RuntimeObject** L_5;
		L_5 = il2cpp_unsafe_add<RuntimeObject*,int32_t>(L_3, L_4);
		int32_t L_6 = __this->____length;
		int32_t L_7 = ___0_start;
		Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 L_8;
		memset((&L_8), 0, sizeof(L_8));
		Span_1__ctor_m143009D8D6D0129C1D86B783427FADE8A0E61936_inline((&L_8), L_5, ((int32_t)il2cpp_codegen_subtract(L_6, L_7)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 Span_1_Slice_mD62E87173D23164C641037162F7DF229A4DD772D_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, int32_t ___0_start, int32_t ___1_length, const RuntimeMethod* method) 
{
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0014;
		}
	}
	{
		int32_t L_2 = ___1_length;
		int32_t L_3 = __this->____length;
		int32_t L_4 = ___0_start;
		if ((!(((uint32_t)L_2) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(L_3, L_4))))))
		{
			goto IL_0019;
		}
	}

IL_0014:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0019:
	{
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_5 = __this->____pointer;
		V_0 = L_5;
		RuntimeObject** L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_0));
		int32_t L_7 = ___0_start;
		RuntimeObject** L_8;
		L_8 = il2cpp_unsafe_add<RuntimeObject*,int32_t>(L_6, L_7);
		int32_t L_9 = ___1_length;
		Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_m143009D8D6D0129C1D86B783427FADE8A0E61936_inline((&L_10), L_8, L_9, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_10;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* Span_1_ToArray_m048501CDA9FB43335CC9A0E27F7C13968F5AA999_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, const RuntimeMethod* method) 
{
	ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1;
		L_1 = Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_inline(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____length;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 20), (uint32_t)L_2);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = L_3;
		NullCheck((RuntimeArray*)L_4);
		uint8_t* L_5;
		L_5 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_4, NULL);
		RuntimeObject** L_6;
		L_6 = il2cpp_unsafe_as_ref<RuntimeObject*>(L_5);
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_7 = __this->____pointer;
		V_0 = L_7;
		RuntimeObject** L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(RuntimeObject*, (Il2CppByReference*)(&V_0));
		int32_t L_9 = __this->____length;
		Buffer_Memmove_TisRuntimeObject_mD13EEA1538F26A567D15A7615D8851A427DFD15A(L_6, L_8, (uint64_t)((int64_t)L_9), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m0B5336E05EEAAE122DF68A3A82C2D4359A2BE33D_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_Equals_m69706E4CE23FA4909D56BDCC0C622582CEB8A7F1_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, RuntimeObject* ___0_obj, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_GetHashCode_mBC410F47BFAE941EDB0C375AC807E0C32E7CB3FF_gshared (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 Span_1_op_Implicit_m880847D96585827EA2A199D7E8F531A2CAF33D8F_gshared (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___0_array, const RuntimeMethod* method) 
{
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___0_array;
		Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2 L_1;
		memset((&L_1), 0, sizeof(L_1));
		Span_1__ctor_m8DB171982E2F26182BA531AC18EEAA27D52763EE_inline((&L_1), L_0, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 21));
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mC41D37A4EC24675FE88E07FC2AD929915407785C_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* ___0_array, const RuntimeMethod* method) 
{
	Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974));
		goto IL_0037;
	}

IL_0037:
	{
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_4;
		L_4 = il2cpp_unsafe_as_ref<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974>(L_3);
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m821D1955B569BA1E9BB3B29FEA7CC17D66F42594_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* ___0_array, int32_t ___1_start, int32_t ___2_length, const RuntimeMethod* method) 
{
	Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_1 = ___1_start;
		if (L_1)
		{
			goto IL_0009;
		}
	}
	{
		int32_t L_2 = ___2_length;
		if (!L_2)
		{
			goto IL_000e;
		}
	}

IL_0009:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6));
		return;
	}

IL_0016:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974));
		goto IL_0042;
	}

IL_0042:
	{
		int32_t L_4 = ___1_start;
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_5 = ___0_array;
		NullCheck(L_5);
		if ((!(((uint32_t)L_4) <= ((uint32_t)((int32_t)(((RuntimeArray*)L_5)->max_length))))))
		{
			goto IL_0050;
		}
	}
	{
		int32_t L_6 = ___2_length;
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_7 = ___0_array;
		NullCheck(L_7);
		int32_t L_8 = ___1_start;
		if ((!(((uint32_t)L_6) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_7)->max_length)), L_8))))))
		{
			goto IL_0055;
		}
	}

IL_0050:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0055:
	{
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_9 = ___0_array;
		NullCheck((RuntimeArray*)L_9);
		uint8_t* L_10;
		L_10 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_9, NULL);
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_11;
		L_11 = il2cpp_unsafe_as_ref<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974>(L_10);
		int32_t L_12 = ___1_start;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_13;
		L_13 = il2cpp_unsafe_add<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,int32_t>(L_11, L_12);
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_14;
		memset((&L_14), 0, sizeof(L_14));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_14), L_13);
		__this->____pointer = L_14;
		int32_t L_15 = ___2_length;
		__this->____length = L_15;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mA6B0A6ED7314F9301B3FC7317CFED015A7DA262B_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_0 = ___1_length;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_1 = ___0_pointer;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_2;
		L_2 = il2cpp_unsafe_as_ref<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974>((uint8_t*)L_1);
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_3;
		memset((&L_3), 0, sizeof(L_3));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_3), L_2);
		__this->____pointer = L_3;
		int32_t L_4 = ___1_length;
		__this->____length = L_4;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mEB88C92980CFEB47188FEFC02935CCC0C1E57C7F_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_0 = ___0_ptr;
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* Span_1_get_Item_mB7071AA8E33DE4B13D2209840375228A5D52602C_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_index;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) >= ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5(NULL);
	}

IL_000e:
	{
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_2 = __this->____pointer;
		V_0 = L_2;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_index;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_5;
		L_5 = il2cpp_unsafe_add<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,int32_t>(L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* Span_1_GetPinnableReference_mF0A90F1E14D2D9075750CB8B57E0856E89ABB703_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, const RuntimeMethod* method) 
{
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_0010;
		}
	}
	{
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_1;
		L_1 = il2cpp_unsafe_as_ref<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974>((void*)((uintptr_t)0));
		return L_1;
	}

IL_0010:
	{
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_2 = __this->____pointer;
		V_0 = L_2;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_0));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Clear_m5120E1C2F78503B6FE831E8A69AA06459E241F0C_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, const RuntimeMethod* method) 
{
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		goto IL_0034;
	}

IL_0034:
	{
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_0 = __this->____pointer;
		V_0 = L_0;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_1;
		L_1 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_0));
		uint8_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint8_t>(L_1);
		int32_t L_3 = __this->____length;
		int32_t L_4;
		L_4 = il2cpp_unsafe_sizeof<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974>();
		SpanHelpers_ClearWithoutReferences_m65DB2925AE7A5FF88BB3EA1BF90513C9ADF0653D(L_2, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_3), ((int64_t)L_4))), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Fill_mA5A9176D924B9FBDB11B8E951E65559742231354_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 ___0_value, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 V_1;
	memset((&V_1), 0, sizeof(V_1));
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD V_2;
	memset((&V_2), 0, sizeof(V_2));
	uint64_t V_3 = 0;
	Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* V_4 = NULL;
	uint64_t V_5 = 0;
	uint64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = il2cpp_unsafe_sizeof<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974>();
		if ((!(((uint32_t)L_0) == ((uint32_t)1))))
		{
			goto IL_0037;
		}
	}
	{
		int32_t L_1 = __this->____length;
		V_0 = (uint32_t)L_1;
		uint32_t L_2 = V_0;
		if (L_2)
		{
			goto IL_0013;
		}
	}
	{
		return;
	}

IL_0013:
	{
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_3 = ___0_value;
		V_1 = L_3;
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_4 = __this->____pointer;
		V_2 = L_4;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_5;
		L_5 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_2));
		uint8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint8_t>(L_5);
		uint8_t* L_7;
		L_7 = il2cpp_unsafe_as_ref<uint8_t>((&V_1));
		int32_t L_8 = *((uint8_t*)L_7);
		uint32_t L_9 = V_0;
		Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A(L_6, (uint8_t)L_8, L_9, NULL);
		return;
	}

IL_0037:
	{
		int32_t L_10 = __this->____length;
		V_3 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_10));
		uint64_t L_11 = V_3;
		if (L_11)
		{
			goto IL_0043;
		}
	}
	{
		return;
	}

IL_0043:
	{
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_12 = __this->____pointer;
		V_2 = L_12;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_13;
		L_13 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_2));
		V_4 = L_13;
		int32_t L_14;
		L_14 = il2cpp_unsafe_sizeof<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974>();
		V_5 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_14));
		V_6 = (uint64_t)((int64_t)0);
		goto IL_0110;
	}

IL_0064:
	{
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_15 = V_4;
		uint64_t L_16 = V_6;
		uint64_t L_17 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_18;
		L_18 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_15, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_16, (int64_t)L_17)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_19 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_18 = L_19;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_20 = V_4;
		uint64_t L_21 = V_6;
		uint64_t L_22 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_23;
		L_23 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_20, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_21, ((int64_t)1))), (int64_t)L_22)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_24 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_23 = L_24;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_25 = V_4;
		uint64_t L_26 = V_6;
		uint64_t L_27 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_28;
		L_28 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_25, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_26, ((int64_t)2))), (int64_t)L_27)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_29 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_28 = L_29;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_30 = V_4;
		uint64_t L_31 = V_6;
		uint64_t L_32 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_33;
		L_33 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_30, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_31, ((int64_t)3))), (int64_t)L_32)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_34 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_33 = L_34;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_35 = V_4;
		uint64_t L_36 = V_6;
		uint64_t L_37 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_38;
		L_38 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_35, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_36, ((int64_t)4))), (int64_t)L_37)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_39 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_38 = L_39;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_40 = V_4;
		uint64_t L_41 = V_6;
		uint64_t L_42 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_43;
		L_43 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_40, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_41, ((int64_t)5))), (int64_t)L_42)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_44 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_43 = L_44;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_45 = V_4;
		uint64_t L_46 = V_6;
		uint64_t L_47 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_48;
		L_48 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_45, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_46, ((int64_t)6))), (int64_t)L_47)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_49 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_48 = L_49;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_50 = V_4;
		uint64_t L_51 = V_6;
		uint64_t L_52 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_53;
		L_53 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_50, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_51, ((int64_t)7))), (int64_t)L_52)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_54 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_53 = L_54;
		uint64_t L_55 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_55, ((int64_t)8)));
	}

IL_0110:
	{
		uint64_t L_56 = V_6;
		uint64_t L_57 = V_3;
		if ((!(((uint64_t)L_56) >= ((uint64_t)((int64_t)((int64_t)L_57&((int64_t)((int32_t)-8))))))))
		{
			goto IL_0064;
		}
	}
	{
		uint64_t L_58 = V_6;
		uint64_t L_59 = V_3;
		if ((!(((uint64_t)L_58) < ((uint64_t)((int64_t)((int64_t)L_59&((int64_t)((int32_t)-4))))))))
		{
			goto IL_0198;
		}
	}
	{
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_60 = V_4;
		uint64_t L_61 = V_6;
		uint64_t L_62 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_63;
		L_63 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_60, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_61, (int64_t)L_62)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_64 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_63 = L_64;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_65 = V_4;
		uint64_t L_66 = V_6;
		uint64_t L_67 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_68;
		L_68 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_65, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_66, ((int64_t)1))), (int64_t)L_67)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_69 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_68 = L_69;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_70 = V_4;
		uint64_t L_71 = V_6;
		uint64_t L_72 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_73;
		L_73 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_70, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_71, ((int64_t)2))), (int64_t)L_72)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_74 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_73 = L_74;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_75 = V_4;
		uint64_t L_76 = V_6;
		uint64_t L_77 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_78;
		L_78 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_75, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_76, ((int64_t)3))), (int64_t)L_77)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_79 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_78 = L_79;
		uint64_t L_80 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_80, ((int64_t)4)));
		goto IL_0198;
	}

IL_017f:
	{
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_81 = V_4;
		uint64_t L_82 = V_6;
		uint64_t L_83 = V_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_84;
		L_84 = il2cpp_unsafe_add_byte_offset<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,uint64_t>(L_81, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_82, (int64_t)L_83)));
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_85 = ___0_value;
		*(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974*)L_84 = L_85;
		uint64_t L_86 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_86, ((int64_t)1)));
	}

IL_0198:
	{
		uint64_t L_87 = V_6;
		uint64_t L_88 = V_3;
		if ((!(((uint64_t)L_87) >= ((uint64_t)L_88))))
		{
			goto IL_017f;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_CopyTo_mBC7F3CB19BF2E8AC96A8773EFB90DC9F89AE0C72_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 ___0_destination, const RuntimeMethod* method) 
{
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_mFB84148EFC50BBB0940E474682C1649251F02576_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0038;
		}
	}
	{
		Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 L_2 = ___0_destination;
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_3 = L_2.____pointer;
		V_0 = L_3;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_0));
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_5 = __this->____pointer;
		V_0 = L_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_0));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisQuaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_m0CD197E2DFBC77DF284D8F5CAD832F7CEAEBC8FA(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return;
	}

IL_0038:
	{
		ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D(NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_TryCopyTo_m25D19A09E1378604F3592F7ED4E2DE094D9BC440_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 ___0_destination, const RuntimeMethod* method) 
{
	bool V_0 = false;
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		V_0 = (bool)0;
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_mFB84148EFC50BBB0940E474682C1649251F02576_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_003b;
		}
	}
	{
		Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 L_2 = ___0_destination;
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_3 = L_2.____pointer;
		V_1 = L_3;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_1));
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_5 = __this->____pointer;
		V_1 = L_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_1));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisQuaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_m0CD197E2DFBC77DF284D8F5CAD832F7CEAEBC8FA(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		V_0 = (bool)1;
	}

IL_003b:
	{
		bool L_8 = V_0;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlySpan_1_tDE8983102D42568E6127FA7BE5CE63380CD7A820 Span_1_op_Implicit_mE21E2981C696DB02CB0E18049B031D817317D547_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 ___0_span, const RuntimeMethod* method) 
{
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 L_0 = ___0_span;
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_1 = L_0.____pointer;
		V_0 = L_1;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_0));
		Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 L_3 = ___0_span;
		int32_t L_4 = L_3.____length;
		ReadOnlySpan_1_tDE8983102D42568E6127FA7BE5CE63380CD7A820 L_5;
		memset((&L_5), 0, sizeof(L_5));
		ReadOnlySpan_1__ctor_mA7A30FB7B805A8FB05709CD7C7D8C29E3F3C60CA_inline((&L_5), L_2, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 17));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Span_1_ToString_mAF3D8E1E5EA313C7E290DB9D2BB7965F37CAC636_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0DB46164953228904843938099AF66650313FEE5);
		s_Il2CppMethodInitialized = true;
	}
	Il2CppChar* V_0 = NULL;
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var) };
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		bool L_4;
		L_4 = Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_003e;
		}
	}
	{
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_5 = __this->____pointer;
		V_1 = L_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_1));
		Il2CppChar* L_7;
		L_7 = il2cpp_unsafe_as_ref<Il2CppChar>(L_6);
		V_0 = L_7;
		Il2CppChar* L_8 = V_0;
		int32_t L_9 = __this->____length;
		String_t* L_10;
		L_10 = String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2(NULL, (Il2CppChar*)((uintptr_t)L_8), 0, L_9, NULL);
		return L_10;
	}

IL_003e:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck((MemberInfo_t*)L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(12, (MemberInfo_t*)L_12);
		int32_t L_14 = __this->____length;
		int32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral0DB46164953228904843938099AF66650313FEE5, (RuntimeObject*)L_13, L_16, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 Span_1_Slice_mB913DB356E234CA0CE4661552D59F4E25DD6064F_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, int32_t ___0_start, const RuntimeMethod* method) 
{
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) > ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_2 = __this->____pointer;
		V_0 = L_2;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_start;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_5;
		L_5 = il2cpp_unsafe_add<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,int32_t>(L_3, L_4);
		int32_t L_6 = __this->____length;
		int32_t L_7 = ___0_start;
		Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 L_8;
		memset((&L_8), 0, sizeof(L_8));
		Span_1__ctor_mEB88C92980CFEB47188FEFC02935CCC0C1E57C7F_inline((&L_8), L_5, ((int32_t)il2cpp_codegen_subtract(L_6, L_7)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 Span_1_Slice_m5C925CE26B575DD918DAA01AA3068C77FEFFD75C_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, int32_t ___0_start, int32_t ___1_length, const RuntimeMethod* method) 
{
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0014;
		}
	}
	{
		int32_t L_2 = ___1_length;
		int32_t L_3 = __this->____length;
		int32_t L_4 = ___0_start;
		if ((!(((uint32_t)L_2) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(L_3, L_4))))))
		{
			goto IL_0019;
		}
	}

IL_0014:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0019:
	{
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_5 = __this->____pointer;
		V_0 = L_5;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_0));
		int32_t L_7 = ___0_start;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_8;
		L_8 = il2cpp_unsafe_add<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974,int32_t>(L_6, L_7);
		int32_t L_9 = ___1_length;
		Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mEB88C92980CFEB47188FEFC02935CCC0C1E57C7F_inline((&L_10), L_8, L_9, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_10;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* Span_1_ToArray_mA7A7BA36A340E11528D00699E69C6462A0699FEC_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, const RuntimeMethod* method) 
{
	ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_1;
		L_1 = Array_Empty_TisQuaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_mCE17AEB484914CF98BCAA9B85E19A2A027EE46C6_inline(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____length;
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_3 = (QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7*)(QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7*)SZArrayNew(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 20), (uint32_t)L_2);
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_4 = L_3;
		NullCheck((RuntimeArray*)L_4);
		uint8_t* L_5;
		L_5 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_4, NULL);
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_6;
		L_6 = il2cpp_unsafe_as_ref<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974>(L_5);
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_7 = __this->____pointer;
		V_0 = L_7;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974, (Il2CppByReference*)(&V_0));
		int32_t L_9 = __this->____length;
		Buffer_Memmove_TisQuaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_m0CD197E2DFBC77DF284D8F5CAD832F7CEAEBC8FA(L_6, L_8, (uint64_t)((int64_t)L_9), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mFB84148EFC50BBB0940E474682C1649251F02576_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_Equals_m8072BD76486FAF990AA853EDF3DD7D6DAD3A73E1_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, RuntimeObject* ___0_obj, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_GetHashCode_m6591A32DB538CC39A12E9DBD8F4D4ADE893B8905_gshared (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 Span_1_op_Implicit_m3E30E82D4B94AA655E6A43528AEDADFE08D35455_gshared (QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* ___0_array, const RuntimeMethod* method) 
{
	{
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_0 = ___0_array;
		Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6 L_1;
		memset((&L_1), 0, sizeof(L_1));
		Span_1__ctor_mC41D37A4EC24675FE88E07FC2AD929915407785C_inline((&L_1), L_0, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 21));
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m08998C4090851C62879905634F774B4D4A1B3221_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* ___0_array, const RuntimeMethod* method) 
{
	int8_t V_0 = 0x0;
	{
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int8_t));
		goto IL_0037;
	}

IL_0037:
	{
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		int8_t* L_4;
		L_4 = il2cpp_unsafe_as_ref<int8_t>(L_3);
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m984DAF4FBC5E9A17BE7275FB73036C1F1E324801_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* ___0_array, int32_t ___1_start, int32_t ___2_length, const RuntimeMethod* method) 
{
	int8_t V_0 = 0x0;
	{
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_1 = ___1_start;
		if (L_1)
		{
			goto IL_0009;
		}
	}
	{
		int32_t L_2 = ___2_length;
		if (!L_2)
		{
			goto IL_000e;
		}
	}

IL_0009:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03));
		return;
	}

IL_0016:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int8_t));
		goto IL_0042;
	}

IL_0042:
	{
		int32_t L_4 = ___1_start;
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_5 = ___0_array;
		NullCheck(L_5);
		if ((!(((uint32_t)L_4) <= ((uint32_t)((int32_t)(((RuntimeArray*)L_5)->max_length))))))
		{
			goto IL_0050;
		}
	}
	{
		int32_t L_6 = ___2_length;
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_7 = ___0_array;
		NullCheck(L_7);
		int32_t L_8 = ___1_start;
		if ((!(((uint32_t)L_6) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_7)->max_length)), L_8))))))
		{
			goto IL_0055;
		}
	}

IL_0050:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0055:
	{
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_9 = ___0_array;
		NullCheck((RuntimeArray*)L_9);
		uint8_t* L_10;
		L_10 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_9, NULL);
		int8_t* L_11;
		L_11 = il2cpp_unsafe_as_ref<int8_t>(L_10);
		int32_t L_12 = ___1_start;
		int8_t* L_13;
		L_13 = il2cpp_unsafe_add<int8_t,int32_t>(L_11, L_12);
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_14;
		memset((&L_14), 0, sizeof(L_14));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_14), L_13);
		__this->____pointer = L_14;
		int32_t L_15 = ___2_length;
		__this->____length = L_15;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m9ECB9EFC03BF98C691F4ADEDDAD9A251D0452846_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_0 = ___1_length;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_1 = ___0_pointer;
		int8_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<int8_t>((uint8_t*)L_1);
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_3;
		memset((&L_3), 0, sizeof(L_3));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_3), L_2);
		__this->____pointer = L_3;
		int32_t L_4 = ___1_length;
		__this->____length = L_4;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mA974AFDCD35D895EAD5E356146AD6C8682F2738A_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, int8_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		int8_t* L_0 = ___0_ptr;
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t* Span_1_get_Item_m44CB387F5DBCBD564F6E9512EA08331E10CBDDC5_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_index;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) >= ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5(NULL);
	}

IL_000e:
	{
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_2 = __this->____pointer;
		V_0 = L_2;
		int8_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_index;
		int8_t* L_5;
		L_5 = il2cpp_unsafe_add<int8_t,int32_t>(L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t* Span_1_GetPinnableReference_m28D56E5B5E0BCDB327E5A8FCF14909B14D6EB969_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, const RuntimeMethod* method) 
{
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_0010;
		}
	}
	{
		int8_t* L_1;
		L_1 = il2cpp_unsafe_as_ref<int8_t>((void*)((uintptr_t)0));
		return L_1;
	}

IL_0010:
	{
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_2 = __this->____pointer;
		V_0 = L_2;
		int8_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_0));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Clear_mBBB3F805C6637F2399B8B350C2E0B41DFB99DD8E_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, const RuntimeMethod* method) 
{
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		goto IL_0034;
	}

IL_0034:
	{
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_0 = __this->____pointer;
		V_0 = L_0;
		int8_t* L_1;
		L_1 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_0));
		uint8_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint8_t>(L_1);
		int32_t L_3 = __this->____length;
		int32_t L_4;
		L_4 = il2cpp_unsafe_sizeof<int8_t>();
		SpanHelpers_ClearWithoutReferences_m65DB2925AE7A5FF88BB3EA1BF90513C9ADF0653D(L_2, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_3), ((int64_t)L_4))), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Fill_m73B38BB1A57451ED7EF46534C2A05B01C8B4F5D2_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, int8_t ___0_value, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	int8_t V_1 = 0x0;
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 V_2;
	memset((&V_2), 0, sizeof(V_2));
	uint64_t V_3 = 0;
	int8_t* V_4 = NULL;
	uint64_t V_5 = 0;
	uint64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = il2cpp_unsafe_sizeof<int8_t>();
		if ((!(((uint32_t)L_0) == ((uint32_t)1))))
		{
			goto IL_0037;
		}
	}
	{
		int32_t L_1 = __this->____length;
		V_0 = (uint32_t)L_1;
		uint32_t L_2 = V_0;
		if (L_2)
		{
			goto IL_0013;
		}
	}
	{
		return;
	}

IL_0013:
	{
		int8_t L_3 = ___0_value;
		V_1 = L_3;
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_4 = __this->____pointer;
		V_2 = L_4;
		int8_t* L_5;
		L_5 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_2));
		uint8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint8_t>(L_5);
		uint8_t* L_7;
		L_7 = il2cpp_unsafe_as_ref<uint8_t>((&V_1));
		int32_t L_8 = *((uint8_t*)L_7);
		uint32_t L_9 = V_0;
		Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A(L_6, (uint8_t)L_8, L_9, NULL);
		return;
	}

IL_0037:
	{
		int32_t L_10 = __this->____length;
		V_3 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_10));
		uint64_t L_11 = V_3;
		if (L_11)
		{
			goto IL_0043;
		}
	}
	{
		return;
	}

IL_0043:
	{
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_12 = __this->____pointer;
		V_2 = L_12;
		int8_t* L_13;
		L_13 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_2));
		V_4 = L_13;
		int32_t L_14;
		L_14 = il2cpp_unsafe_sizeof<int8_t>();
		V_5 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_14));
		V_6 = (uint64_t)((int64_t)0);
		goto IL_0110;
	}

IL_0064:
	{
		int8_t* L_15 = V_4;
		uint64_t L_16 = V_6;
		uint64_t L_17 = V_5;
		int8_t* L_18;
		L_18 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_15, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_16, (int64_t)L_17)));
		int8_t L_19 = ___0_value;
		*(int8_t*)L_18 = L_19;
		int8_t* L_20 = V_4;
		uint64_t L_21 = V_6;
		uint64_t L_22 = V_5;
		int8_t* L_23;
		L_23 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_20, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_21, ((int64_t)1))), (int64_t)L_22)));
		int8_t L_24 = ___0_value;
		*(int8_t*)L_23 = L_24;
		int8_t* L_25 = V_4;
		uint64_t L_26 = V_6;
		uint64_t L_27 = V_5;
		int8_t* L_28;
		L_28 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_25, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_26, ((int64_t)2))), (int64_t)L_27)));
		int8_t L_29 = ___0_value;
		*(int8_t*)L_28 = L_29;
		int8_t* L_30 = V_4;
		uint64_t L_31 = V_6;
		uint64_t L_32 = V_5;
		int8_t* L_33;
		L_33 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_30, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_31, ((int64_t)3))), (int64_t)L_32)));
		int8_t L_34 = ___0_value;
		*(int8_t*)L_33 = L_34;
		int8_t* L_35 = V_4;
		uint64_t L_36 = V_6;
		uint64_t L_37 = V_5;
		int8_t* L_38;
		L_38 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_35, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_36, ((int64_t)4))), (int64_t)L_37)));
		int8_t L_39 = ___0_value;
		*(int8_t*)L_38 = L_39;
		int8_t* L_40 = V_4;
		uint64_t L_41 = V_6;
		uint64_t L_42 = V_5;
		int8_t* L_43;
		L_43 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_40, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_41, ((int64_t)5))), (int64_t)L_42)));
		int8_t L_44 = ___0_value;
		*(int8_t*)L_43 = L_44;
		int8_t* L_45 = V_4;
		uint64_t L_46 = V_6;
		uint64_t L_47 = V_5;
		int8_t* L_48;
		L_48 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_45, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_46, ((int64_t)6))), (int64_t)L_47)));
		int8_t L_49 = ___0_value;
		*(int8_t*)L_48 = L_49;
		int8_t* L_50 = V_4;
		uint64_t L_51 = V_6;
		uint64_t L_52 = V_5;
		int8_t* L_53;
		L_53 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_50, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_51, ((int64_t)7))), (int64_t)L_52)));
		int8_t L_54 = ___0_value;
		*(int8_t*)L_53 = L_54;
		uint64_t L_55 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_55, ((int64_t)8)));
	}

IL_0110:
	{
		uint64_t L_56 = V_6;
		uint64_t L_57 = V_3;
		if ((!(((uint64_t)L_56) >= ((uint64_t)((int64_t)((int64_t)L_57&((int64_t)((int32_t)-8))))))))
		{
			goto IL_0064;
		}
	}
	{
		uint64_t L_58 = V_6;
		uint64_t L_59 = V_3;
		if ((!(((uint64_t)L_58) < ((uint64_t)((int64_t)((int64_t)L_59&((int64_t)((int32_t)-4))))))))
		{
			goto IL_0198;
		}
	}
	{
		int8_t* L_60 = V_4;
		uint64_t L_61 = V_6;
		uint64_t L_62 = V_5;
		int8_t* L_63;
		L_63 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_60, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_61, (int64_t)L_62)));
		int8_t L_64 = ___0_value;
		*(int8_t*)L_63 = L_64;
		int8_t* L_65 = V_4;
		uint64_t L_66 = V_6;
		uint64_t L_67 = V_5;
		int8_t* L_68;
		L_68 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_65, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_66, ((int64_t)1))), (int64_t)L_67)));
		int8_t L_69 = ___0_value;
		*(int8_t*)L_68 = L_69;
		int8_t* L_70 = V_4;
		uint64_t L_71 = V_6;
		uint64_t L_72 = V_5;
		int8_t* L_73;
		L_73 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_70, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_71, ((int64_t)2))), (int64_t)L_72)));
		int8_t L_74 = ___0_value;
		*(int8_t*)L_73 = L_74;
		int8_t* L_75 = V_4;
		uint64_t L_76 = V_6;
		uint64_t L_77 = V_5;
		int8_t* L_78;
		L_78 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_75, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_76, ((int64_t)3))), (int64_t)L_77)));
		int8_t L_79 = ___0_value;
		*(int8_t*)L_78 = L_79;
		uint64_t L_80 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_80, ((int64_t)4)));
		goto IL_0198;
	}

IL_017f:
	{
		int8_t* L_81 = V_4;
		uint64_t L_82 = V_6;
		uint64_t L_83 = V_5;
		int8_t* L_84;
		L_84 = il2cpp_unsafe_add_byte_offset<int8_t,uint64_t>(L_81, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_82, (int64_t)L_83)));
		int8_t L_85 = ___0_value;
		*(int8_t*)L_84 = L_85;
		uint64_t L_86 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_86, ((int64_t)1)));
	}

IL_0198:
	{
		uint64_t L_87 = V_6;
		uint64_t L_88 = V_3;
		if ((!(((uint64_t)L_87) >= ((uint64_t)L_88))))
		{
			goto IL_017f;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_CopyTo_m5E8637B3A596494338327E1B1D99BFE6D6530A53_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 ___0_destination, const RuntimeMethod* method) 
{
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_mF7BD3765E86D3ED55FB40E64A3D0F2D90EC0F908_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0038;
		}
	}
	{
		Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 L_2 = ___0_destination;
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_3 = L_2.____pointer;
		V_0 = L_3;
		int8_t* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_0));
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_5 = __this->____pointer;
		V_0 = L_5;
		int8_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_0));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m8921CF9FB1C61F7FA656ADE11B64F27943551250(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return;
	}

IL_0038:
	{
		ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D(NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_TryCopyTo_m4555E6AA44ACC87286706FD135172FEDB15A8547_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 ___0_destination, const RuntimeMethod* method) 
{
	bool V_0 = false;
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		V_0 = (bool)0;
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_mF7BD3765E86D3ED55FB40E64A3D0F2D90EC0F908_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_003b;
		}
	}
	{
		Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 L_2 = ___0_destination;
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_3 = L_2.____pointer;
		V_1 = L_3;
		int8_t* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_1));
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_5 = __this->____pointer;
		V_1 = L_5;
		int8_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_1));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m8921CF9FB1C61F7FA656ADE11B64F27943551250(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		V_0 = (bool)1;
	}

IL_003b:
	{
		bool L_8 = V_0;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlySpan_1_tB90CE592448A63B07B56F79364563BD361CF8CDC Span_1_op_Implicit_mEB5386CD324B698077FDFADB776603C569099F8C_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 ___0_span, const RuntimeMethod* method) 
{
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 L_0 = ___0_span;
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_1 = L_0.____pointer;
		V_0 = L_1;
		int8_t* L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_0));
		Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 L_3 = ___0_span;
		int32_t L_4 = L_3.____length;
		ReadOnlySpan_1_tB90CE592448A63B07B56F79364563BD361CF8CDC L_5;
		memset((&L_5), 0, sizeof(L_5));
		ReadOnlySpan_1__ctor_m783D660E0BF03935E536537C3B32F79A7BD0FB42_inline((&L_5), L_2, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 17));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Span_1_ToString_mC4300808DBC547D98DCB1CAA434BDA430659517B_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0DB46164953228904843938099AF66650313FEE5);
		s_Il2CppMethodInitialized = true;
	}
	Il2CppChar* V_0 = NULL;
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var) };
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		bool L_4;
		L_4 = Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_003e;
		}
	}
	{
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_5 = __this->____pointer;
		V_1 = L_5;
		int8_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_1));
		Il2CppChar* L_7;
		L_7 = il2cpp_unsafe_as_ref<Il2CppChar>(L_6);
		V_0 = L_7;
		Il2CppChar* L_8 = V_0;
		int32_t L_9 = __this->____length;
		String_t* L_10;
		L_10 = String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2(NULL, (Il2CppChar*)((uintptr_t)L_8), 0, L_9, NULL);
		return L_10;
	}

IL_003e:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck((MemberInfo_t*)L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(12, (MemberInfo_t*)L_12);
		int32_t L_14 = __this->____length;
		int32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral0DB46164953228904843938099AF66650313FEE5, (RuntimeObject*)L_13, L_16, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 Span_1_Slice_m0C36CA415A4A00F0432A7C9AEB10EA4BD2C94587_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, int32_t ___0_start, const RuntimeMethod* method) 
{
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) > ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_2 = __this->____pointer;
		V_0 = L_2;
		int8_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_start;
		int8_t* L_5;
		L_5 = il2cpp_unsafe_add<int8_t,int32_t>(L_3, L_4);
		int32_t L_6 = __this->____length;
		int32_t L_7 = ___0_start;
		Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 L_8;
		memset((&L_8), 0, sizeof(L_8));
		Span_1__ctor_mA974AFDCD35D895EAD5E356146AD6C8682F2738A_inline((&L_8), L_5, ((int32_t)il2cpp_codegen_subtract(L_6, L_7)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 Span_1_Slice_m7976AB7947E93CC67C17943C5CBE6D34B79291E2_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, int32_t ___0_start, int32_t ___1_length, const RuntimeMethod* method) 
{
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0014;
		}
	}
	{
		int32_t L_2 = ___1_length;
		int32_t L_3 = __this->____length;
		int32_t L_4 = ___0_start;
		if ((!(((uint32_t)L_2) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(L_3, L_4))))))
		{
			goto IL_0019;
		}
	}

IL_0014:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0019:
	{
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_5 = __this->____pointer;
		V_0 = L_5;
		int8_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_0));
		int32_t L_7 = ___0_start;
		int8_t* L_8;
		L_8 = il2cpp_unsafe_add<int8_t,int32_t>(L_6, L_7);
		int32_t L_9 = ___1_length;
		Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA974AFDCD35D895EAD5E356146AD6C8682F2738A_inline((&L_10), L_8, L_9, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_10;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* Span_1_ToArray_m3B383490D4FD606BE2411806B496ACA2CAA3AE2D_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, const RuntimeMethod* method) 
{
	ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_1;
		L_1 = Array_Empty_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mE1E6DB6377A6DCA206C1AB31B3964386D486F94F_inline(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____length;
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_3 = (SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913*)(SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913*)SZArrayNew(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 20), (uint32_t)L_2);
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_4 = L_3;
		NullCheck((RuntimeArray*)L_4);
		uint8_t* L_5;
		L_5 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_4, NULL);
		int8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<int8_t>(L_5);
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_7 = __this->____pointer;
		V_0 = L_7;
		int8_t* L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(int8_t, (Il2CppByReference*)(&V_0));
		int32_t L_9 = __this->____length;
		Buffer_Memmove_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m8921CF9FB1C61F7FA656ADE11B64F27943551250(L_6, L_8, (uint64_t)((int64_t)L_9), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mF7BD3765E86D3ED55FB40E64A3D0F2D90EC0F908_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_Equals_mC42120D62951B62C99E4811AEF4EB7C564C4267C_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, RuntimeObject* ___0_obj, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_GetHashCode_m12483FCF56DDC5FA8FEF7F8CAC2AB7F664AE2A03_gshared (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 Span_1_op_Implicit_mC73CC6DA4419750B1C83CAC45135E41F5907E8CB_gshared (SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* ___0_array, const RuntimeMethod* method) 
{
	{
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_0 = ___0_array;
		Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03 L_1;
		memset((&L_1), 0, sizeof(L_1));
		Span_1__ctor_m08998C4090851C62879905634F774B4D4A1B3221_inline((&L_1), L_0, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 21));
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mA5EF3ABEDD5776174AAEF4D976B58B424F43B275_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* ___0_array, const RuntimeMethod* method) 
{
	float V_0 = 0.0f;
	{
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(float));
		goto IL_0037;
	}

IL_0037:
	{
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		float* L_4;
		L_4 = il2cpp_unsafe_as_ref<float>(L_3);
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mC2731788C7CB616BDD9477D53265DA06DC788E1F_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* ___0_array, int32_t ___1_start, int32_t ___2_length, const RuntimeMethod* method) 
{
	float V_0 = 0.0f;
	{
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_1 = ___1_start;
		if (L_1)
		{
			goto IL_0009;
		}
	}
	{
		int32_t L_2 = ___2_length;
		if (!L_2)
		{
			goto IL_000e;
		}
	}

IL_0009:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C));
		return;
	}

IL_0016:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(float));
		goto IL_0042;
	}

IL_0042:
	{
		int32_t L_4 = ___1_start;
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_5 = ___0_array;
		NullCheck(L_5);
		if ((!(((uint32_t)L_4) <= ((uint32_t)((int32_t)(((RuntimeArray*)L_5)->max_length))))))
		{
			goto IL_0050;
		}
	}
	{
		int32_t L_6 = ___2_length;
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_7 = ___0_array;
		NullCheck(L_7);
		int32_t L_8 = ___1_start;
		if ((!(((uint32_t)L_6) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_7)->max_length)), L_8))))))
		{
			goto IL_0055;
		}
	}

IL_0050:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0055:
	{
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_9 = ___0_array;
		NullCheck((RuntimeArray*)L_9);
		uint8_t* L_10;
		L_10 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_9, NULL);
		float* L_11;
		L_11 = il2cpp_unsafe_as_ref<float>(L_10);
		int32_t L_12 = ___1_start;
		float* L_13;
		L_13 = il2cpp_unsafe_add<float,int32_t>(L_11, L_12);
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_14;
		memset((&L_14), 0, sizeof(L_14));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_14), L_13);
		__this->____pointer = L_14;
		int32_t L_15 = ___2_length;
		__this->____length = L_15;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m7DF6F0480C6904A216270964E3639CEA4F419C40_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_0 = ___1_length;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_1 = ___0_pointer;
		float* L_2;
		L_2 = il2cpp_unsafe_as_ref<float>((uint8_t*)L_1);
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_3;
		memset((&L_3), 0, sizeof(L_3));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_3), L_2);
		__this->____pointer = L_3;
		int32_t L_4 = ___1_length;
		__this->____length = L_4;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m1B5237AB31EC7CA6AB6981E961491177242F4A55_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, float* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		float* L_0 = ___0_ptr;
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float* Span_1_get_Item_mF6350A5455E12C7AFF2E3C3452232B73A5343BEE_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_index;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) >= ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5(NULL);
	}

IL_000e:
	{
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_2 = __this->____pointer;
		V_0 = L_2;
		float* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_index;
		float* L_5;
		L_5 = il2cpp_unsafe_add<float,int32_t>(L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float* Span_1_GetPinnableReference_mB3EA4E5E6B70A51EC51C563EBB60BA9A8DE4D1A0_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, const RuntimeMethod* method) 
{
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_0010;
		}
	}
	{
		float* L_1;
		L_1 = il2cpp_unsafe_as_ref<float>((void*)((uintptr_t)0));
		return L_1;
	}

IL_0010:
	{
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_2 = __this->____pointer;
		V_0 = L_2;
		float* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_0));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Clear_m521D995DEFD6B372D62A2D4ED58CB615A656E0E2_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, const RuntimeMethod* method) 
{
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		goto IL_0034;
	}

IL_0034:
	{
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_0 = __this->____pointer;
		V_0 = L_0;
		float* L_1;
		L_1 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_0));
		uint8_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint8_t>(L_1);
		int32_t L_3 = __this->____length;
		int32_t L_4;
		L_4 = il2cpp_unsafe_sizeof<float>();
		SpanHelpers_ClearWithoutReferences_m65DB2925AE7A5FF88BB3EA1BF90513C9ADF0653D(L_2, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_3), ((int64_t)L_4))), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Fill_mE5F739A0ED3DE473A64BBCEB9D204A86179900BB_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, float ___0_value, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	float V_1 = 0.0f;
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A V_2;
	memset((&V_2), 0, sizeof(V_2));
	uint64_t V_3 = 0;
	float* V_4 = NULL;
	uint64_t V_5 = 0;
	uint64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = il2cpp_unsafe_sizeof<float>();
		if ((!(((uint32_t)L_0) == ((uint32_t)1))))
		{
			goto IL_0037;
		}
	}
	{
		int32_t L_1 = __this->____length;
		V_0 = (uint32_t)L_1;
		uint32_t L_2 = V_0;
		if (L_2)
		{
			goto IL_0013;
		}
	}
	{
		return;
	}

IL_0013:
	{
		float L_3 = ___0_value;
		V_1 = L_3;
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_4 = __this->____pointer;
		V_2 = L_4;
		float* L_5;
		L_5 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_2));
		uint8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint8_t>(L_5);
		uint8_t* L_7;
		L_7 = il2cpp_unsafe_as_ref<uint8_t>((&V_1));
		int32_t L_8 = *((uint8_t*)L_7);
		uint32_t L_9 = V_0;
		Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A(L_6, (uint8_t)L_8, L_9, NULL);
		return;
	}

IL_0037:
	{
		int32_t L_10 = __this->____length;
		V_3 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_10));
		uint64_t L_11 = V_3;
		if (L_11)
		{
			goto IL_0043;
		}
	}
	{
		return;
	}

IL_0043:
	{
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_12 = __this->____pointer;
		V_2 = L_12;
		float* L_13;
		L_13 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_2));
		V_4 = L_13;
		int32_t L_14;
		L_14 = il2cpp_unsafe_sizeof<float>();
		V_5 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_14));
		V_6 = (uint64_t)((int64_t)0);
		goto IL_0110;
	}

IL_0064:
	{
		float* L_15 = V_4;
		uint64_t L_16 = V_6;
		uint64_t L_17 = V_5;
		float* L_18;
		L_18 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_15, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_16, (int64_t)L_17)));
		float L_19 = ___0_value;
		*(float*)L_18 = L_19;
		float* L_20 = V_4;
		uint64_t L_21 = V_6;
		uint64_t L_22 = V_5;
		float* L_23;
		L_23 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_20, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_21, ((int64_t)1))), (int64_t)L_22)));
		float L_24 = ___0_value;
		*(float*)L_23 = L_24;
		float* L_25 = V_4;
		uint64_t L_26 = V_6;
		uint64_t L_27 = V_5;
		float* L_28;
		L_28 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_25, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_26, ((int64_t)2))), (int64_t)L_27)));
		float L_29 = ___0_value;
		*(float*)L_28 = L_29;
		float* L_30 = V_4;
		uint64_t L_31 = V_6;
		uint64_t L_32 = V_5;
		float* L_33;
		L_33 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_30, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_31, ((int64_t)3))), (int64_t)L_32)));
		float L_34 = ___0_value;
		*(float*)L_33 = L_34;
		float* L_35 = V_4;
		uint64_t L_36 = V_6;
		uint64_t L_37 = V_5;
		float* L_38;
		L_38 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_35, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_36, ((int64_t)4))), (int64_t)L_37)));
		float L_39 = ___0_value;
		*(float*)L_38 = L_39;
		float* L_40 = V_4;
		uint64_t L_41 = V_6;
		uint64_t L_42 = V_5;
		float* L_43;
		L_43 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_40, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_41, ((int64_t)5))), (int64_t)L_42)));
		float L_44 = ___0_value;
		*(float*)L_43 = L_44;
		float* L_45 = V_4;
		uint64_t L_46 = V_6;
		uint64_t L_47 = V_5;
		float* L_48;
		L_48 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_45, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_46, ((int64_t)6))), (int64_t)L_47)));
		float L_49 = ___0_value;
		*(float*)L_48 = L_49;
		float* L_50 = V_4;
		uint64_t L_51 = V_6;
		uint64_t L_52 = V_5;
		float* L_53;
		L_53 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_50, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_51, ((int64_t)7))), (int64_t)L_52)));
		float L_54 = ___0_value;
		*(float*)L_53 = L_54;
		uint64_t L_55 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_55, ((int64_t)8)));
	}

IL_0110:
	{
		uint64_t L_56 = V_6;
		uint64_t L_57 = V_3;
		if ((!(((uint64_t)L_56) >= ((uint64_t)((int64_t)((int64_t)L_57&((int64_t)((int32_t)-8))))))))
		{
			goto IL_0064;
		}
	}
	{
		uint64_t L_58 = V_6;
		uint64_t L_59 = V_3;
		if ((!(((uint64_t)L_58) < ((uint64_t)((int64_t)((int64_t)L_59&((int64_t)((int32_t)-4))))))))
		{
			goto IL_0198;
		}
	}
	{
		float* L_60 = V_4;
		uint64_t L_61 = V_6;
		uint64_t L_62 = V_5;
		float* L_63;
		L_63 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_60, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_61, (int64_t)L_62)));
		float L_64 = ___0_value;
		*(float*)L_63 = L_64;
		float* L_65 = V_4;
		uint64_t L_66 = V_6;
		uint64_t L_67 = V_5;
		float* L_68;
		L_68 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_65, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_66, ((int64_t)1))), (int64_t)L_67)));
		float L_69 = ___0_value;
		*(float*)L_68 = L_69;
		float* L_70 = V_4;
		uint64_t L_71 = V_6;
		uint64_t L_72 = V_5;
		float* L_73;
		L_73 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_70, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_71, ((int64_t)2))), (int64_t)L_72)));
		float L_74 = ___0_value;
		*(float*)L_73 = L_74;
		float* L_75 = V_4;
		uint64_t L_76 = V_6;
		uint64_t L_77 = V_5;
		float* L_78;
		L_78 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_75, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_76, ((int64_t)3))), (int64_t)L_77)));
		float L_79 = ___0_value;
		*(float*)L_78 = L_79;
		uint64_t L_80 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_80, ((int64_t)4)));
		goto IL_0198;
	}

IL_017f:
	{
		float* L_81 = V_4;
		uint64_t L_82 = V_6;
		uint64_t L_83 = V_5;
		float* L_84;
		L_84 = il2cpp_unsafe_add_byte_offset<float,uint64_t>(L_81, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_82, (int64_t)L_83)));
		float L_85 = ___0_value;
		*(float*)L_84 = L_85;
		uint64_t L_86 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_86, ((int64_t)1)));
	}

IL_0198:
	{
		uint64_t L_87 = V_6;
		uint64_t L_88 = V_3;
		if ((!(((uint64_t)L_87) >= ((uint64_t)L_88))))
		{
			goto IL_017f;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_CopyTo_m622FEA536F071E2F732664FEE4C3F49F60720853_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C ___0_destination, const RuntimeMethod* method) 
{
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_mED11128A130F01BC9C5F690BFFAEF38B2283751B_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0038;
		}
	}
	{
		Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C L_2 = ___0_destination;
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_3 = L_2.____pointer;
		V_0 = L_3;
		float* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_0));
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_5 = __this->____pointer;
		V_0 = L_5;
		float* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_0));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m8E65A20A53C662400685CE50AE11C2A80FBC6D7C(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return;
	}

IL_0038:
	{
		ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D(NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_TryCopyTo_mA8236BB567B98CE0F6BB0EED9CF1E27D53611B28_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C ___0_destination, const RuntimeMethod* method) 
{
	bool V_0 = false;
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		V_0 = (bool)0;
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_mED11128A130F01BC9C5F690BFFAEF38B2283751B_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_003b;
		}
	}
	{
		Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C L_2 = ___0_destination;
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_3 = L_2.____pointer;
		V_1 = L_3;
		float* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_1));
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_5 = __this->____pointer;
		V_1 = L_5;
		float* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_1));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m8E65A20A53C662400685CE50AE11C2A80FBC6D7C(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		V_0 = (bool)1;
	}

IL_003b:
	{
		bool L_8 = V_0;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlySpan_1_t9C2C8EDE84088EDC61AADD4CA3C2CDC72D135E3D Span_1_op_Implicit_mB0102DF5CB96E5097ED5DD9E4EF70462B91ECA92_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C ___0_span, const RuntimeMethod* method) 
{
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C L_0 = ___0_span;
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_1 = L_0.____pointer;
		V_0 = L_1;
		float* L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_0));
		Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C L_3 = ___0_span;
		int32_t L_4 = L_3.____length;
		ReadOnlySpan_1_t9C2C8EDE84088EDC61AADD4CA3C2CDC72D135E3D L_5;
		memset((&L_5), 0, sizeof(L_5));
		ReadOnlySpan_1__ctor_mA93F2958FDB8B48F00F57C878275CF739DE6273B_inline((&L_5), L_2, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 17));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Span_1_ToString_mD58A3F7E334C1E37B8F2E2266DE89A2E67A68574_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0DB46164953228904843938099AF66650313FEE5);
		s_Il2CppMethodInitialized = true;
	}
	Il2CppChar* V_0 = NULL;
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var) };
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		bool L_4;
		L_4 = Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_003e;
		}
	}
	{
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_5 = __this->____pointer;
		V_1 = L_5;
		float* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_1));
		Il2CppChar* L_7;
		L_7 = il2cpp_unsafe_as_ref<Il2CppChar>(L_6);
		V_0 = L_7;
		Il2CppChar* L_8 = V_0;
		int32_t L_9 = __this->____length;
		String_t* L_10;
		L_10 = String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2(NULL, (Il2CppChar*)((uintptr_t)L_8), 0, L_9, NULL);
		return L_10;
	}

IL_003e:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck((MemberInfo_t*)L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(12, (MemberInfo_t*)L_12);
		int32_t L_14 = __this->____length;
		int32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral0DB46164953228904843938099AF66650313FEE5, (RuntimeObject*)L_13, L_16, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C Span_1_Slice_mD2575575D9ACF2D548DE2DA6DB1FFB36CD923508_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, int32_t ___0_start, const RuntimeMethod* method) 
{
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) > ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_2 = __this->____pointer;
		V_0 = L_2;
		float* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_start;
		float* L_5;
		L_5 = il2cpp_unsafe_add<float,int32_t>(L_3, L_4);
		int32_t L_6 = __this->____length;
		int32_t L_7 = ___0_start;
		Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C L_8;
		memset((&L_8), 0, sizeof(L_8));
		Span_1__ctor_m1B5237AB31EC7CA6AB6981E961491177242F4A55_inline((&L_8), L_5, ((int32_t)il2cpp_codegen_subtract(L_6, L_7)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C Span_1_Slice_mBA215A512B958A2D696F1FC47073FA9453553B8E_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, int32_t ___0_start, int32_t ___1_length, const RuntimeMethod* method) 
{
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0014;
		}
	}
	{
		int32_t L_2 = ___1_length;
		int32_t L_3 = __this->____length;
		int32_t L_4 = ___0_start;
		if ((!(((uint32_t)L_2) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(L_3, L_4))))))
		{
			goto IL_0019;
		}
	}

IL_0014:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0019:
	{
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_5 = __this->____pointer;
		V_0 = L_5;
		float* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_0));
		int32_t L_7 = ___0_start;
		float* L_8;
		L_8 = il2cpp_unsafe_add<float,int32_t>(L_6, L_7);
		int32_t L_9 = ___1_length;
		Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_m1B5237AB31EC7CA6AB6981E961491177242F4A55_inline((&L_10), L_8, L_9, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_10;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* Span_1_ToArray_mAAE4DC737A5BF9A5A29F336EDA790C8AFADA2CDE_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, const RuntimeMethod* method) 
{
	ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_1;
		L_1 = Array_Empty_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m44F781E90531F7FCDB12BC4290CD4394A887FC06_inline(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____length;
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_3 = (SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C*)(SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C*)SZArrayNew(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 20), (uint32_t)L_2);
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_4 = L_3;
		NullCheck((RuntimeArray*)L_4);
		uint8_t* L_5;
		L_5 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_4, NULL);
		float* L_6;
		L_6 = il2cpp_unsafe_as_ref<float>(L_5);
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_7 = __this->____pointer;
		V_0 = L_7;
		float* L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(float, (Il2CppByReference*)(&V_0));
		int32_t L_9 = __this->____length;
		Buffer_Memmove_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m8E65A20A53C662400685CE50AE11C2A80FBC6D7C(L_6, L_8, (uint64_t)((int64_t)L_9), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mED11128A130F01BC9C5F690BFFAEF38B2283751B_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_Equals_m7EE91A3706295633E217636FE45CBB5DD8A76404_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, RuntimeObject* ___0_obj, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_GetHashCode_mF2A430BCD36FB8331FD4CE77443388F381319EC1_gshared (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C Span_1_op_Implicit_m1CCE9A46E23410F893315674231D60515AB767A8_gshared (SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* ___0_array, const RuntimeMethod* method) 
{
	{
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_0 = ___0_array;
		Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C L_1;
		memset((&L_1), 0, sizeof(L_1));
		Span_1__ctor_mA5EF3ABEDD5776174AAEF4D976B58B424F43B275_inline((&L_1), L_0, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 21));
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mC892A665B48BA9CD149DA76F26EA3607C7859792_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* ___0_array, const RuntimeMethod* method) 
{
	uint16_t V_0 = 0;
	{
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(uint16_t));
		goto IL_0037;
	}

IL_0037:
	{
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		uint16_t* L_4;
		L_4 = il2cpp_unsafe_as_ref<uint16_t>(L_3);
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m88D9BE6D0BF5FDFDF1EC95538768786944AA873A_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* ___0_array, int32_t ___1_start, int32_t ___2_length, const RuntimeMethod* method) 
{
	uint16_t V_0 = 0;
	{
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_1 = ___1_start;
		if (L_1)
		{
			goto IL_0009;
		}
	}
	{
		int32_t L_2 = ___2_length;
		if (!L_2)
		{
			goto IL_000e;
		}
	}

IL_0009:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D));
		return;
	}

IL_0016:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(uint16_t));
		goto IL_0042;
	}

IL_0042:
	{
		int32_t L_4 = ___1_start;
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_5 = ___0_array;
		NullCheck(L_5);
		if ((!(((uint32_t)L_4) <= ((uint32_t)((int32_t)(((RuntimeArray*)L_5)->max_length))))))
		{
			goto IL_0050;
		}
	}
	{
		int32_t L_6 = ___2_length;
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_7 = ___0_array;
		NullCheck(L_7);
		int32_t L_8 = ___1_start;
		if ((!(((uint32_t)L_6) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_7)->max_length)), L_8))))))
		{
			goto IL_0055;
		}
	}

IL_0050:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0055:
	{
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_9 = ___0_array;
		NullCheck((RuntimeArray*)L_9);
		uint8_t* L_10;
		L_10 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_9, NULL);
		uint16_t* L_11;
		L_11 = il2cpp_unsafe_as_ref<uint16_t>(L_10);
		int32_t L_12 = ___1_start;
		uint16_t* L_13;
		L_13 = il2cpp_unsafe_add<uint16_t,int32_t>(L_11, L_12);
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_14;
		memset((&L_14), 0, sizeof(L_14));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_14), L_13);
		__this->____pointer = L_14;
		int32_t L_15 = ___2_length;
		__this->____length = L_15;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mB886029FDB28A19EF15C463DD88A08470033D192_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_0 = ___1_length;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_1 = ___0_pointer;
		uint16_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint16_t>((uint8_t*)L_1);
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_3;
		memset((&L_3), 0, sizeof(L_3));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_3), L_2);
		__this->____pointer = L_3;
		int32_t L_4 = ___1_length;
		__this->____length = L_4;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m458FB27AA0EB3527A73C9D6305D452A062D2ABC4_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, uint16_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		uint16_t* L_0 = ___0_ptr;
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint16_t* Span_1_get_Item_m51DF8F9B68EB998FCFF5DE6A753DEC3D3BE61D30_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_index;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) >= ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5(NULL);
	}

IL_000e:
	{
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_2 = __this->____pointer;
		V_0 = L_2;
		uint16_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_index;
		uint16_t* L_5;
		L_5 = il2cpp_unsafe_add<uint16_t,int32_t>(L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint16_t* Span_1_GetPinnableReference_m2084184F17A5461CA1BD4D2364E1423E83775299_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, const RuntimeMethod* method) 
{
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_0010;
		}
	}
	{
		uint16_t* L_1;
		L_1 = il2cpp_unsafe_as_ref<uint16_t>((void*)((uintptr_t)0));
		return L_1;
	}

IL_0010:
	{
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_2 = __this->____pointer;
		V_0 = L_2;
		uint16_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_0));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Clear_m0935AE3C29451F430E10E1C162F4B2767137CC57_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, const RuntimeMethod* method) 
{
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		goto IL_0034;
	}

IL_0034:
	{
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_0 = __this->____pointer;
		V_0 = L_0;
		uint16_t* L_1;
		L_1 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_0));
		uint8_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint8_t>(L_1);
		int32_t L_3 = __this->____length;
		int32_t L_4;
		L_4 = il2cpp_unsafe_sizeof<uint16_t>();
		SpanHelpers_ClearWithoutReferences_m65DB2925AE7A5FF88BB3EA1BF90513C9ADF0653D(L_2, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_3), ((int64_t)L_4))), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Fill_m1739BAAE7DCB30488B41A98BE7F70F2C3E2A683B_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, uint16_t ___0_value, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	uint16_t V_1 = 0;
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE V_2;
	memset((&V_2), 0, sizeof(V_2));
	uint64_t V_3 = 0;
	uint16_t* V_4 = NULL;
	uint64_t V_5 = 0;
	uint64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = il2cpp_unsafe_sizeof<uint16_t>();
		if ((!(((uint32_t)L_0) == ((uint32_t)1))))
		{
			goto IL_0037;
		}
	}
	{
		int32_t L_1 = __this->____length;
		V_0 = (uint32_t)L_1;
		uint32_t L_2 = V_0;
		if (L_2)
		{
			goto IL_0013;
		}
	}
	{
		return;
	}

IL_0013:
	{
		uint16_t L_3 = ___0_value;
		V_1 = L_3;
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_4 = __this->____pointer;
		V_2 = L_4;
		uint16_t* L_5;
		L_5 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_2));
		uint8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint8_t>(L_5);
		uint8_t* L_7;
		L_7 = il2cpp_unsafe_as_ref<uint8_t>((&V_1));
		int32_t L_8 = *((uint8_t*)L_7);
		uint32_t L_9 = V_0;
		Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A(L_6, (uint8_t)L_8, L_9, NULL);
		return;
	}

IL_0037:
	{
		int32_t L_10 = __this->____length;
		V_3 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_10));
		uint64_t L_11 = V_3;
		if (L_11)
		{
			goto IL_0043;
		}
	}
	{
		return;
	}

IL_0043:
	{
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_12 = __this->____pointer;
		V_2 = L_12;
		uint16_t* L_13;
		L_13 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_2));
		V_4 = L_13;
		int32_t L_14;
		L_14 = il2cpp_unsafe_sizeof<uint16_t>();
		V_5 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_14));
		V_6 = (uint64_t)((int64_t)0);
		goto IL_0110;
	}

IL_0064:
	{
		uint16_t* L_15 = V_4;
		uint64_t L_16 = V_6;
		uint64_t L_17 = V_5;
		uint16_t* L_18;
		L_18 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_15, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_16, (int64_t)L_17)));
		uint16_t L_19 = ___0_value;
		*(uint16_t*)L_18 = L_19;
		uint16_t* L_20 = V_4;
		uint64_t L_21 = V_6;
		uint64_t L_22 = V_5;
		uint16_t* L_23;
		L_23 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_20, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_21, ((int64_t)1))), (int64_t)L_22)));
		uint16_t L_24 = ___0_value;
		*(uint16_t*)L_23 = L_24;
		uint16_t* L_25 = V_4;
		uint64_t L_26 = V_6;
		uint64_t L_27 = V_5;
		uint16_t* L_28;
		L_28 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_25, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_26, ((int64_t)2))), (int64_t)L_27)));
		uint16_t L_29 = ___0_value;
		*(uint16_t*)L_28 = L_29;
		uint16_t* L_30 = V_4;
		uint64_t L_31 = V_6;
		uint64_t L_32 = V_5;
		uint16_t* L_33;
		L_33 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_30, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_31, ((int64_t)3))), (int64_t)L_32)));
		uint16_t L_34 = ___0_value;
		*(uint16_t*)L_33 = L_34;
		uint16_t* L_35 = V_4;
		uint64_t L_36 = V_6;
		uint64_t L_37 = V_5;
		uint16_t* L_38;
		L_38 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_35, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_36, ((int64_t)4))), (int64_t)L_37)));
		uint16_t L_39 = ___0_value;
		*(uint16_t*)L_38 = L_39;
		uint16_t* L_40 = V_4;
		uint64_t L_41 = V_6;
		uint64_t L_42 = V_5;
		uint16_t* L_43;
		L_43 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_40, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_41, ((int64_t)5))), (int64_t)L_42)));
		uint16_t L_44 = ___0_value;
		*(uint16_t*)L_43 = L_44;
		uint16_t* L_45 = V_4;
		uint64_t L_46 = V_6;
		uint64_t L_47 = V_5;
		uint16_t* L_48;
		L_48 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_45, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_46, ((int64_t)6))), (int64_t)L_47)));
		uint16_t L_49 = ___0_value;
		*(uint16_t*)L_48 = L_49;
		uint16_t* L_50 = V_4;
		uint64_t L_51 = V_6;
		uint64_t L_52 = V_5;
		uint16_t* L_53;
		L_53 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_50, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_51, ((int64_t)7))), (int64_t)L_52)));
		uint16_t L_54 = ___0_value;
		*(uint16_t*)L_53 = L_54;
		uint64_t L_55 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_55, ((int64_t)8)));
	}

IL_0110:
	{
		uint64_t L_56 = V_6;
		uint64_t L_57 = V_3;
		if ((!(((uint64_t)L_56) >= ((uint64_t)((int64_t)((int64_t)L_57&((int64_t)((int32_t)-8))))))))
		{
			goto IL_0064;
		}
	}
	{
		uint64_t L_58 = V_6;
		uint64_t L_59 = V_3;
		if ((!(((uint64_t)L_58) < ((uint64_t)((int64_t)((int64_t)L_59&((int64_t)((int32_t)-4))))))))
		{
			goto IL_0198;
		}
	}
	{
		uint16_t* L_60 = V_4;
		uint64_t L_61 = V_6;
		uint64_t L_62 = V_5;
		uint16_t* L_63;
		L_63 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_60, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_61, (int64_t)L_62)));
		uint16_t L_64 = ___0_value;
		*(uint16_t*)L_63 = L_64;
		uint16_t* L_65 = V_4;
		uint64_t L_66 = V_6;
		uint64_t L_67 = V_5;
		uint16_t* L_68;
		L_68 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_65, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_66, ((int64_t)1))), (int64_t)L_67)));
		uint16_t L_69 = ___0_value;
		*(uint16_t*)L_68 = L_69;
		uint16_t* L_70 = V_4;
		uint64_t L_71 = V_6;
		uint64_t L_72 = V_5;
		uint16_t* L_73;
		L_73 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_70, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_71, ((int64_t)2))), (int64_t)L_72)));
		uint16_t L_74 = ___0_value;
		*(uint16_t*)L_73 = L_74;
		uint16_t* L_75 = V_4;
		uint64_t L_76 = V_6;
		uint64_t L_77 = V_5;
		uint16_t* L_78;
		L_78 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_75, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_76, ((int64_t)3))), (int64_t)L_77)));
		uint16_t L_79 = ___0_value;
		*(uint16_t*)L_78 = L_79;
		uint64_t L_80 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_80, ((int64_t)4)));
		goto IL_0198;
	}

IL_017f:
	{
		uint16_t* L_81 = V_4;
		uint64_t L_82 = V_6;
		uint64_t L_83 = V_5;
		uint16_t* L_84;
		L_84 = il2cpp_unsafe_add_byte_offset<uint16_t,uint64_t>(L_81, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_82, (int64_t)L_83)));
		uint16_t L_85 = ___0_value;
		*(uint16_t*)L_84 = L_85;
		uint64_t L_86 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_86, ((int64_t)1)));
	}

IL_0198:
	{
		uint64_t L_87 = V_6;
		uint64_t L_88 = V_3;
		if ((!(((uint64_t)L_87) >= ((uint64_t)L_88))))
		{
			goto IL_017f;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_CopyTo_m1E3344EA531D3CEBB2D498C960EA1E11D3042E89_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D ___0_destination, const RuntimeMethod* method) 
{
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_mD173AF2E3688317C8AB9621F7626A2A34DE8F56B_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0038;
		}
	}
	{
		Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D L_2 = ___0_destination;
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_3 = L_2.____pointer;
		V_0 = L_3;
		uint16_t* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_0));
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_5 = __this->____pointer;
		V_0 = L_5;
		uint16_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_0));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mE48DFFFA4D52B03F4ACA304FD485E78F4BFF0E42(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return;
	}

IL_0038:
	{
		ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D(NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_TryCopyTo_m8B92037E39DBEC33F5EFF019B694C6F8422F6254_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D ___0_destination, const RuntimeMethod* method) 
{
	bool V_0 = false;
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		V_0 = (bool)0;
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_mD173AF2E3688317C8AB9621F7626A2A34DE8F56B_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_003b;
		}
	}
	{
		Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D L_2 = ___0_destination;
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_3 = L_2.____pointer;
		V_1 = L_3;
		uint16_t* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_1));
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_5 = __this->____pointer;
		V_1 = L_5;
		uint16_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_1));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mE48DFFFA4D52B03F4ACA304FD485E78F4BFF0E42(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		V_0 = (bool)1;
	}

IL_003b:
	{
		bool L_8 = V_0;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlySpan_1_tA2EFC117098BD2B38ADBF809AA976D9F3C13654F Span_1_op_Implicit_m2BCA68E89516F4E0AD7CF9A9513466D4837140F8_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D ___0_span, const RuntimeMethod* method) 
{
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D L_0 = ___0_span;
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_1 = L_0.____pointer;
		V_0 = L_1;
		uint16_t* L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_0));
		Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D L_3 = ___0_span;
		int32_t L_4 = L_3.____length;
		ReadOnlySpan_1_tA2EFC117098BD2B38ADBF809AA976D9F3C13654F L_5;
		memset((&L_5), 0, sizeof(L_5));
		ReadOnlySpan_1__ctor_mCBA8EFCAA8102765E34B993A8177EE752D80890F_inline((&L_5), L_2, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 17));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Span_1_ToString_mC92A31A501B7BC12A11981C1C3D653971D37E35C_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0DB46164953228904843938099AF66650313FEE5);
		s_Il2CppMethodInitialized = true;
	}
	Il2CppChar* V_0 = NULL;
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var) };
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		bool L_4;
		L_4 = Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_003e;
		}
	}
	{
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_5 = __this->____pointer;
		V_1 = L_5;
		uint16_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_1));
		Il2CppChar* L_7;
		L_7 = il2cpp_unsafe_as_ref<Il2CppChar>(L_6);
		V_0 = L_7;
		Il2CppChar* L_8 = V_0;
		int32_t L_9 = __this->____length;
		String_t* L_10;
		L_10 = String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2(NULL, (Il2CppChar*)((uintptr_t)L_8), 0, L_9, NULL);
		return L_10;
	}

IL_003e:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck((MemberInfo_t*)L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(12, (MemberInfo_t*)L_12);
		int32_t L_14 = __this->____length;
		int32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral0DB46164953228904843938099AF66650313FEE5, (RuntimeObject*)L_13, L_16, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D Span_1_Slice_m60CA4425F69A57B604820588F7299CE6056B9BF7_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, int32_t ___0_start, const RuntimeMethod* method) 
{
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) > ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_2 = __this->____pointer;
		V_0 = L_2;
		uint16_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_start;
		uint16_t* L_5;
		L_5 = il2cpp_unsafe_add<uint16_t,int32_t>(L_3, L_4);
		int32_t L_6 = __this->____length;
		int32_t L_7 = ___0_start;
		Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D L_8;
		memset((&L_8), 0, sizeof(L_8));
		Span_1__ctor_m458FB27AA0EB3527A73C9D6305D452A062D2ABC4_inline((&L_8), L_5, ((int32_t)il2cpp_codegen_subtract(L_6, L_7)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D Span_1_Slice_mD475695D1F124D1A5F0514CB93BF8B2D12FFF09A_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, int32_t ___0_start, int32_t ___1_length, const RuntimeMethod* method) 
{
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0014;
		}
	}
	{
		int32_t L_2 = ___1_length;
		int32_t L_3 = __this->____length;
		int32_t L_4 = ___0_start;
		if ((!(((uint32_t)L_2) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(L_3, L_4))))))
		{
			goto IL_0019;
		}
	}

IL_0014:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0019:
	{
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_5 = __this->____pointer;
		V_0 = L_5;
		uint16_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_0));
		int32_t L_7 = ___0_start;
		uint16_t* L_8;
		L_8 = il2cpp_unsafe_add<uint16_t,int32_t>(L_6, L_7);
		int32_t L_9 = ___1_length;
		Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_m458FB27AA0EB3527A73C9D6305D452A062D2ABC4_inline((&L_10), L_8, L_9, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_10;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* Span_1_ToArray_m69B5996786351756E80F75F1F46A6D8D14817044_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, const RuntimeMethod* method) 
{
	ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_1;
		L_1 = Array_Empty_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mA9FE4AE132DC76B02E8B39B5052CBA643CFF7220_inline(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____length;
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_3 = (UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83*)(UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83*)SZArrayNew(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 20), (uint32_t)L_2);
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_4 = L_3;
		NullCheck((RuntimeArray*)L_4);
		uint8_t* L_5;
		L_5 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_4, NULL);
		uint16_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint16_t>(L_5);
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_7 = __this->____pointer;
		V_0 = L_7;
		uint16_t* L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(uint16_t, (Il2CppByReference*)(&V_0));
		int32_t L_9 = __this->____length;
		Buffer_Memmove_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mE48DFFFA4D52B03F4ACA304FD485E78F4BFF0E42(L_6, L_8, (uint64_t)((int64_t)L_9), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mD173AF2E3688317C8AB9621F7626A2A34DE8F56B_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_Equals_m658BC08F24940E68B344C2623996A8BAA8506DFF_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, RuntimeObject* ___0_obj, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_GetHashCode_m0DD2A2BE777631909AB6BC8EB9C8C50A65227EF8_gshared (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D Span_1_op_Implicit_mDFEC7007D8B0366E7FB8FA350AC4D3F3EAFB4EA7_gshared (UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* ___0_array, const RuntimeMethod* method) 
{
	{
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_0 = ___0_array;
		Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D L_1;
		memset((&L_1), 0, sizeof(L_1));
		Span_1__ctor_mC892A665B48BA9CD149DA76F26EA3607C7859792_inline((&L_1), L_0, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 21));
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m1161A3B3850C22A54C838C62FB009355039C28ED_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* ___0_array, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	{
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(uint32_t));
		goto IL_0037;
	}

IL_0037:
	{
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		uint32_t* L_4;
		L_4 = il2cpp_unsafe_as_ref<uint32_t>(L_3);
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m660EEF593C35EC36D687474C6F23E166CD9F31D9_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* ___0_array, int32_t ___1_start, int32_t ___2_length, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	{
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_1 = ___1_start;
		if (L_1)
		{
			goto IL_0009;
		}
	}
	{
		int32_t L_2 = ___2_length;
		if (!L_2)
		{
			goto IL_000e;
		}
	}

IL_0009:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA));
		return;
	}

IL_0016:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(uint32_t));
		goto IL_0042;
	}

IL_0042:
	{
		int32_t L_4 = ___1_start;
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_5 = ___0_array;
		NullCheck(L_5);
		if ((!(((uint32_t)L_4) <= ((uint32_t)((int32_t)(((RuntimeArray*)L_5)->max_length))))))
		{
			goto IL_0050;
		}
	}
	{
		int32_t L_6 = ___2_length;
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_7 = ___0_array;
		NullCheck(L_7);
		int32_t L_8 = ___1_start;
		if ((!(((uint32_t)L_6) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_7)->max_length)), L_8))))))
		{
			goto IL_0055;
		}
	}

IL_0050:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0055:
	{
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_9 = ___0_array;
		NullCheck((RuntimeArray*)L_9);
		uint8_t* L_10;
		L_10 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_9, NULL);
		uint32_t* L_11;
		L_11 = il2cpp_unsafe_as_ref<uint32_t>(L_10);
		int32_t L_12 = ___1_start;
		uint32_t* L_13;
		L_13 = il2cpp_unsafe_add<uint32_t,int32_t>(L_11, L_12);
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_14;
		memset((&L_14), 0, sizeof(L_14));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_14), L_13);
		__this->____pointer = L_14;
		int32_t L_15 = ___2_length;
		__this->____length = L_15;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m999E2C05EC97317809898828AF892B8C79ACC7C1_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_0 = ___1_length;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_1 = ___0_pointer;
		uint32_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint32_t>((uint8_t*)L_1);
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_3;
		memset((&L_3), 0, sizeof(L_3));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_3), L_2);
		__this->____pointer = L_3;
		int32_t L_4 = ___1_length;
		__this->____length = L_4;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m44A796B80A3B7B5E228B0865F02AC548FA1E7567_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, uint32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		uint32_t* L_0 = ___0_ptr;
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint32_t* Span_1_get_Item_m02A8F30DBE1911D7E5357E864D231529455D1963_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_index;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) >= ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5(NULL);
	}

IL_000e:
	{
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_2 = __this->____pointer;
		V_0 = L_2;
		uint32_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_index;
		uint32_t* L_5;
		L_5 = il2cpp_unsafe_add<uint32_t,int32_t>(L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint32_t* Span_1_GetPinnableReference_m296F8EBB04F54E3973579C184284BEFAA947B759_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, const RuntimeMethod* method) 
{
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_0010;
		}
	}
	{
		uint32_t* L_1;
		L_1 = il2cpp_unsafe_as_ref<uint32_t>((void*)((uintptr_t)0));
		return L_1;
	}

IL_0010:
	{
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_2 = __this->____pointer;
		V_0 = L_2;
		uint32_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_0));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Clear_m2B78C0269CD984BA3EDE84E92E7D0405F534E396_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, const RuntimeMethod* method) 
{
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		goto IL_0034;
	}

IL_0034:
	{
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_0 = __this->____pointer;
		V_0 = L_0;
		uint32_t* L_1;
		L_1 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_0));
		uint8_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint8_t>(L_1);
		int32_t L_3 = __this->____length;
		int32_t L_4;
		L_4 = il2cpp_unsafe_sizeof<uint32_t>();
		SpanHelpers_ClearWithoutReferences_m65DB2925AE7A5FF88BB3EA1BF90513C9ADF0653D(L_2, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_3), ((int64_t)L_4))), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Fill_m8EC6942077811D5BDD7FE56421443067FD9B57B8_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, uint32_t ___0_value, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	uint32_t V_1 = 0;
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 V_2;
	memset((&V_2), 0, sizeof(V_2));
	uint64_t V_3 = 0;
	uint32_t* V_4 = NULL;
	uint64_t V_5 = 0;
	uint64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = il2cpp_unsafe_sizeof<uint32_t>();
		if ((!(((uint32_t)L_0) == ((uint32_t)1))))
		{
			goto IL_0037;
		}
	}
	{
		int32_t L_1 = __this->____length;
		V_0 = (uint32_t)L_1;
		uint32_t L_2 = V_0;
		if (L_2)
		{
			goto IL_0013;
		}
	}
	{
		return;
	}

IL_0013:
	{
		uint32_t L_3 = ___0_value;
		V_1 = L_3;
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_4 = __this->____pointer;
		V_2 = L_4;
		uint32_t* L_5;
		L_5 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_2));
		uint8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint8_t>(L_5);
		uint8_t* L_7;
		L_7 = il2cpp_unsafe_as_ref<uint8_t>((&V_1));
		int32_t L_8 = *((uint8_t*)L_7);
		uint32_t L_9 = V_0;
		Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A(L_6, (uint8_t)L_8, L_9, NULL);
		return;
	}

IL_0037:
	{
		int32_t L_10 = __this->____length;
		V_3 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_10));
		uint64_t L_11 = V_3;
		if (L_11)
		{
			goto IL_0043;
		}
	}
	{
		return;
	}

IL_0043:
	{
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_12 = __this->____pointer;
		V_2 = L_12;
		uint32_t* L_13;
		L_13 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_2));
		V_4 = L_13;
		int32_t L_14;
		L_14 = il2cpp_unsafe_sizeof<uint32_t>();
		V_5 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_14));
		V_6 = (uint64_t)((int64_t)0);
		goto IL_0110;
	}

IL_0064:
	{
		uint32_t* L_15 = V_4;
		uint64_t L_16 = V_6;
		uint64_t L_17 = V_5;
		uint32_t* L_18;
		L_18 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_15, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_16, (int64_t)L_17)));
		uint32_t L_19 = ___0_value;
		*(uint32_t*)L_18 = L_19;
		uint32_t* L_20 = V_4;
		uint64_t L_21 = V_6;
		uint64_t L_22 = V_5;
		uint32_t* L_23;
		L_23 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_20, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_21, ((int64_t)1))), (int64_t)L_22)));
		uint32_t L_24 = ___0_value;
		*(uint32_t*)L_23 = L_24;
		uint32_t* L_25 = V_4;
		uint64_t L_26 = V_6;
		uint64_t L_27 = V_5;
		uint32_t* L_28;
		L_28 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_25, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_26, ((int64_t)2))), (int64_t)L_27)));
		uint32_t L_29 = ___0_value;
		*(uint32_t*)L_28 = L_29;
		uint32_t* L_30 = V_4;
		uint64_t L_31 = V_6;
		uint64_t L_32 = V_5;
		uint32_t* L_33;
		L_33 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_30, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_31, ((int64_t)3))), (int64_t)L_32)));
		uint32_t L_34 = ___0_value;
		*(uint32_t*)L_33 = L_34;
		uint32_t* L_35 = V_4;
		uint64_t L_36 = V_6;
		uint64_t L_37 = V_5;
		uint32_t* L_38;
		L_38 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_35, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_36, ((int64_t)4))), (int64_t)L_37)));
		uint32_t L_39 = ___0_value;
		*(uint32_t*)L_38 = L_39;
		uint32_t* L_40 = V_4;
		uint64_t L_41 = V_6;
		uint64_t L_42 = V_5;
		uint32_t* L_43;
		L_43 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_40, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_41, ((int64_t)5))), (int64_t)L_42)));
		uint32_t L_44 = ___0_value;
		*(uint32_t*)L_43 = L_44;
		uint32_t* L_45 = V_4;
		uint64_t L_46 = V_6;
		uint64_t L_47 = V_5;
		uint32_t* L_48;
		L_48 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_45, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_46, ((int64_t)6))), (int64_t)L_47)));
		uint32_t L_49 = ___0_value;
		*(uint32_t*)L_48 = L_49;
		uint32_t* L_50 = V_4;
		uint64_t L_51 = V_6;
		uint64_t L_52 = V_5;
		uint32_t* L_53;
		L_53 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_50, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_51, ((int64_t)7))), (int64_t)L_52)));
		uint32_t L_54 = ___0_value;
		*(uint32_t*)L_53 = L_54;
		uint64_t L_55 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_55, ((int64_t)8)));
	}

IL_0110:
	{
		uint64_t L_56 = V_6;
		uint64_t L_57 = V_3;
		if ((!(((uint64_t)L_56) >= ((uint64_t)((int64_t)((int64_t)L_57&((int64_t)((int32_t)-8))))))))
		{
			goto IL_0064;
		}
	}
	{
		uint64_t L_58 = V_6;
		uint64_t L_59 = V_3;
		if ((!(((uint64_t)L_58) < ((uint64_t)((int64_t)((int64_t)L_59&((int64_t)((int32_t)-4))))))))
		{
			goto IL_0198;
		}
	}
	{
		uint32_t* L_60 = V_4;
		uint64_t L_61 = V_6;
		uint64_t L_62 = V_5;
		uint32_t* L_63;
		L_63 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_60, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_61, (int64_t)L_62)));
		uint32_t L_64 = ___0_value;
		*(uint32_t*)L_63 = L_64;
		uint32_t* L_65 = V_4;
		uint64_t L_66 = V_6;
		uint64_t L_67 = V_5;
		uint32_t* L_68;
		L_68 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_65, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_66, ((int64_t)1))), (int64_t)L_67)));
		uint32_t L_69 = ___0_value;
		*(uint32_t*)L_68 = L_69;
		uint32_t* L_70 = V_4;
		uint64_t L_71 = V_6;
		uint64_t L_72 = V_5;
		uint32_t* L_73;
		L_73 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_70, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_71, ((int64_t)2))), (int64_t)L_72)));
		uint32_t L_74 = ___0_value;
		*(uint32_t*)L_73 = L_74;
		uint32_t* L_75 = V_4;
		uint64_t L_76 = V_6;
		uint64_t L_77 = V_5;
		uint32_t* L_78;
		L_78 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_75, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_76, ((int64_t)3))), (int64_t)L_77)));
		uint32_t L_79 = ___0_value;
		*(uint32_t*)L_78 = L_79;
		uint64_t L_80 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_80, ((int64_t)4)));
		goto IL_0198;
	}

IL_017f:
	{
		uint32_t* L_81 = V_4;
		uint64_t L_82 = V_6;
		uint64_t L_83 = V_5;
		uint32_t* L_84;
		L_84 = il2cpp_unsafe_add_byte_offset<uint32_t,uint64_t>(L_81, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_82, (int64_t)L_83)));
		uint32_t L_85 = ___0_value;
		*(uint32_t*)L_84 = L_85;
		uint64_t L_86 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_86, ((int64_t)1)));
	}

IL_0198:
	{
		uint64_t L_87 = V_6;
		uint64_t L_88 = V_3;
		if ((!(((uint64_t)L_87) >= ((uint64_t)L_88))))
		{
			goto IL_017f;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_CopyTo_m45ED9076CBEDB2D4CA30A83E16D9BEE75626A9FF_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA ___0_destination, const RuntimeMethod* method) 
{
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_mC3EBBD1CE9C5025EB30AFDE84FCCCFB3FE794EC5_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0038;
		}
	}
	{
		Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA L_2 = ___0_destination;
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_3 = L_2.____pointer;
		V_0 = L_3;
		uint32_t* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_0));
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_5 = __this->____pointer;
		V_0 = L_5;
		uint32_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_0));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mEC6A6EF02BD38F45F23336F48D35B9DC2BC187FD(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return;
	}

IL_0038:
	{
		ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D(NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_TryCopyTo_mB44CEE930589FCECFE9020025FFB505DD707B2D5_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA ___0_destination, const RuntimeMethod* method) 
{
	bool V_0 = false;
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		V_0 = (bool)0;
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_mC3EBBD1CE9C5025EB30AFDE84FCCCFB3FE794EC5_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_003b;
		}
	}
	{
		Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA L_2 = ___0_destination;
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_3 = L_2.____pointer;
		V_1 = L_3;
		uint32_t* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_1));
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_5 = __this->____pointer;
		V_1 = L_5;
		uint32_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_1));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mEC6A6EF02BD38F45F23336F48D35B9DC2BC187FD(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		V_0 = (bool)1;
	}

IL_003b:
	{
		bool L_8 = V_0;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlySpan_1_t57F4BBC957039E8E904443D25F3A78AE60DC94B4 Span_1_op_Implicit_m030D7D0A16134F1819235F6051864FF9A776A1F6_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA ___0_span, const RuntimeMethod* method) 
{
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA L_0 = ___0_span;
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_1 = L_0.____pointer;
		V_0 = L_1;
		uint32_t* L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_0));
		Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA L_3 = ___0_span;
		int32_t L_4 = L_3.____length;
		ReadOnlySpan_1_t57F4BBC957039E8E904443D25F3A78AE60DC94B4 L_5;
		memset((&L_5), 0, sizeof(L_5));
		ReadOnlySpan_1__ctor_mFEB9E8BCBC125E065C80C12FC6037D87DC6FA2FC_inline((&L_5), L_2, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 17));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Span_1_ToString_mD3E4D84FCE98C375E6C9F2162A57B2395B398873_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0DB46164953228904843938099AF66650313FEE5);
		s_Il2CppMethodInitialized = true;
	}
	Il2CppChar* V_0 = NULL;
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var) };
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		bool L_4;
		L_4 = Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_003e;
		}
	}
	{
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_5 = __this->____pointer;
		V_1 = L_5;
		uint32_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_1));
		Il2CppChar* L_7;
		L_7 = il2cpp_unsafe_as_ref<Il2CppChar>(L_6);
		V_0 = L_7;
		Il2CppChar* L_8 = V_0;
		int32_t L_9 = __this->____length;
		String_t* L_10;
		L_10 = String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2(NULL, (Il2CppChar*)((uintptr_t)L_8), 0, L_9, NULL);
		return L_10;
	}

IL_003e:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck((MemberInfo_t*)L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(12, (MemberInfo_t*)L_12);
		int32_t L_14 = __this->____length;
		int32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral0DB46164953228904843938099AF66650313FEE5, (RuntimeObject*)L_13, L_16, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA Span_1_Slice_m0F9C99478BF7174C4DDEA1889C51F3FA1B7A0234_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, int32_t ___0_start, const RuntimeMethod* method) 
{
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) > ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_2 = __this->____pointer;
		V_0 = L_2;
		uint32_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_start;
		uint32_t* L_5;
		L_5 = il2cpp_unsafe_add<uint32_t,int32_t>(L_3, L_4);
		int32_t L_6 = __this->____length;
		int32_t L_7 = ___0_start;
		Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA L_8;
		memset((&L_8), 0, sizeof(L_8));
		Span_1__ctor_m44A796B80A3B7B5E228B0865F02AC548FA1E7567_inline((&L_8), L_5, ((int32_t)il2cpp_codegen_subtract(L_6, L_7)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA Span_1_Slice_mD2B8E011F70C9E2504AF31237A6738E6BDB321A5_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, int32_t ___0_start, int32_t ___1_length, const RuntimeMethod* method) 
{
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0014;
		}
	}
	{
		int32_t L_2 = ___1_length;
		int32_t L_3 = __this->____length;
		int32_t L_4 = ___0_start;
		if ((!(((uint32_t)L_2) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(L_3, L_4))))))
		{
			goto IL_0019;
		}
	}

IL_0014:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0019:
	{
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_5 = __this->____pointer;
		V_0 = L_5;
		uint32_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_0));
		int32_t L_7 = ___0_start;
		uint32_t* L_8;
		L_8 = il2cpp_unsafe_add<uint32_t,int32_t>(L_6, L_7);
		int32_t L_9 = ___1_length;
		Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_m44A796B80A3B7B5E228B0865F02AC548FA1E7567_inline((&L_10), L_8, L_9, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_10;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* Span_1_ToArray_m69464A7BA0B38D8637E326F94C6FBBB031DF39C4_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, const RuntimeMethod* method) 
{
	ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_1;
		L_1 = Array_Empty_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_m8B0170B3C9368D9BDB90A666E2DF5549423C160C_inline(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____length;
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_3 = (UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA*)(UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA*)SZArrayNew(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 20), (uint32_t)L_2);
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_4 = L_3;
		NullCheck((RuntimeArray*)L_4);
		uint8_t* L_5;
		L_5 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_4, NULL);
		uint32_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint32_t>(L_5);
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_7 = __this->____pointer;
		V_0 = L_7;
		uint32_t* L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(uint32_t, (Il2CppByReference*)(&V_0));
		int32_t L_9 = __this->____length;
		Buffer_Memmove_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_mEC6A6EF02BD38F45F23336F48D35B9DC2BC187FD(L_6, L_8, (uint64_t)((int64_t)L_9), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mC3EBBD1CE9C5025EB30AFDE84FCCCFB3FE794EC5_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_Equals_mBCA1DE3F35219C89B8834EC233C51D4CF12DF5A8_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, RuntimeObject* ___0_obj, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_GetHashCode_m8ADDE3CC62F09D09699842E5024D67145223201D_gshared (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA Span_1_op_Implicit_m5689B5F42218BA135D8CF5E828BF56EFB7FF7FBD_gshared (UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* ___0_array, const RuntimeMethod* method) 
{
	{
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_0 = ___0_array;
		Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA L_1;
		memset((&L_1), 0, sizeof(L_1));
		Span_1__ctor_m1161A3B3850C22A54C838C62FB009355039C28ED_inline((&L_1), L_0, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 21));
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mC83E2FCB4929477ACCB736B25B9E2F89FF2A07CF_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* ___0_array, const RuntimeMethod* method) 
{
	uint64_t V_0 = 0;
	{
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(uint64_t));
		goto IL_0037;
	}

IL_0037:
	{
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		uint64_t* L_4;
		L_4 = il2cpp_unsafe_as_ref<uint64_t>(L_3);
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mACE9723FAD4FABB5DB3492127924E7DED70D581F_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* ___0_array, int32_t ___1_start, int32_t ___2_length, const RuntimeMethod* method) 
{
	uint64_t V_0 = 0;
	{
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_1 = ___1_start;
		if (L_1)
		{
			goto IL_0009;
		}
	}
	{
		int32_t L_2 = ___2_length;
		if (!L_2)
		{
			goto IL_000e;
		}
	}

IL_0009:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3));
		return;
	}

IL_0016:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(uint64_t));
		goto IL_0042;
	}

IL_0042:
	{
		int32_t L_4 = ___1_start;
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_5 = ___0_array;
		NullCheck(L_5);
		if ((!(((uint32_t)L_4) <= ((uint32_t)((int32_t)(((RuntimeArray*)L_5)->max_length))))))
		{
			goto IL_0050;
		}
	}
	{
		int32_t L_6 = ___2_length;
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_7 = ___0_array;
		NullCheck(L_7);
		int32_t L_8 = ___1_start;
		if ((!(((uint32_t)L_6) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_7)->max_length)), L_8))))))
		{
			goto IL_0055;
		}
	}

IL_0050:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0055:
	{
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_9 = ___0_array;
		NullCheck((RuntimeArray*)L_9);
		uint8_t* L_10;
		L_10 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_9, NULL);
		uint64_t* L_11;
		L_11 = il2cpp_unsafe_as_ref<uint64_t>(L_10);
		int32_t L_12 = ___1_start;
		uint64_t* L_13;
		L_13 = il2cpp_unsafe_add<uint64_t,int32_t>(L_11, L_12);
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_14;
		memset((&L_14), 0, sizeof(L_14));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_14), L_13);
		__this->____pointer = L_14;
		int32_t L_15 = ___2_length;
		__this->____length = L_15;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m88AFBBE67F3E951762332D952D09441CB09F29B8_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_0 = ___1_length;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_1 = ___0_pointer;
		uint64_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint64_t>((uint8_t*)L_1);
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_3;
		memset((&L_3), 0, sizeof(L_3));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_3), L_2);
		__this->____pointer = L_3;
		int32_t L_4 = ___1_length;
		__this->____length = L_4;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m3C68BA129DD44A3486C6B57A99412947A16855F6_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, uint64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		uint64_t* L_0 = ___0_ptr;
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint64_t* Span_1_get_Item_m30051484137413775875E0518C56A0F7562A62AF_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_index;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) >= ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5(NULL);
	}

IL_000e:
	{
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_2 = __this->____pointer;
		V_0 = L_2;
		uint64_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_index;
		uint64_t* L_5;
		L_5 = il2cpp_unsafe_add<uint64_t,int32_t>(L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint64_t* Span_1_GetPinnableReference_m8E9CCA07F04C35E18E9D852F9221816617DFF043_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, const RuntimeMethod* method) 
{
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_0010;
		}
	}
	{
		uint64_t* L_1;
		L_1 = il2cpp_unsafe_as_ref<uint64_t>((void*)((uintptr_t)0));
		return L_1;
	}

IL_0010:
	{
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_2 = __this->____pointer;
		V_0 = L_2;
		uint64_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_0));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Clear_mE1DD2BC27D0EE4070148EC36656CB584143712DD_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, const RuntimeMethod* method) 
{
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		goto IL_0034;
	}

IL_0034:
	{
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_0 = __this->____pointer;
		V_0 = L_0;
		uint64_t* L_1;
		L_1 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_0));
		uint8_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint8_t>(L_1);
		int32_t L_3 = __this->____length;
		int32_t L_4;
		L_4 = il2cpp_unsafe_sizeof<uint64_t>();
		SpanHelpers_ClearWithoutReferences_m65DB2925AE7A5FF88BB3EA1BF90513C9ADF0653D(L_2, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_3), ((int64_t)L_4))), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Fill_mDA5F8A3B92B36549BDE83CCF74C6B56A77B077AF_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, uint64_t ___0_value, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	uint64_t V_1 = 0;
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 V_2;
	memset((&V_2), 0, sizeof(V_2));
	uint64_t V_3 = 0;
	uint64_t* V_4 = NULL;
	uint64_t V_5 = 0;
	uint64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = il2cpp_unsafe_sizeof<uint64_t>();
		if ((!(((uint32_t)L_0) == ((uint32_t)1))))
		{
			goto IL_0037;
		}
	}
	{
		int32_t L_1 = __this->____length;
		V_0 = (uint32_t)L_1;
		uint32_t L_2 = V_0;
		if (L_2)
		{
			goto IL_0013;
		}
	}
	{
		return;
	}

IL_0013:
	{
		uint64_t L_3 = ___0_value;
		V_1 = L_3;
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_4 = __this->____pointer;
		V_2 = L_4;
		uint64_t* L_5;
		L_5 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_2));
		uint8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint8_t>(L_5);
		uint8_t* L_7;
		L_7 = il2cpp_unsafe_as_ref<uint8_t>((&V_1));
		int32_t L_8 = *((uint8_t*)L_7);
		uint32_t L_9 = V_0;
		Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A(L_6, (uint8_t)L_8, L_9, NULL);
		return;
	}

IL_0037:
	{
		int32_t L_10 = __this->____length;
		V_3 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_10));
		uint64_t L_11 = V_3;
		if (L_11)
		{
			goto IL_0043;
		}
	}
	{
		return;
	}

IL_0043:
	{
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_12 = __this->____pointer;
		V_2 = L_12;
		uint64_t* L_13;
		L_13 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_2));
		V_4 = L_13;
		int32_t L_14;
		L_14 = il2cpp_unsafe_sizeof<uint64_t>();
		V_5 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_14));
		V_6 = (uint64_t)((int64_t)0);
		goto IL_0110;
	}

IL_0064:
	{
		uint64_t* L_15 = V_4;
		uint64_t L_16 = V_6;
		uint64_t L_17 = V_5;
		uint64_t* L_18;
		L_18 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_15, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_16, (int64_t)L_17)));
		uint64_t L_19 = ___0_value;
		*(uint64_t*)L_18 = L_19;
		uint64_t* L_20 = V_4;
		uint64_t L_21 = V_6;
		uint64_t L_22 = V_5;
		uint64_t* L_23;
		L_23 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_20, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_21, ((int64_t)1))), (int64_t)L_22)));
		uint64_t L_24 = ___0_value;
		*(uint64_t*)L_23 = L_24;
		uint64_t* L_25 = V_4;
		uint64_t L_26 = V_6;
		uint64_t L_27 = V_5;
		uint64_t* L_28;
		L_28 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_25, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_26, ((int64_t)2))), (int64_t)L_27)));
		uint64_t L_29 = ___0_value;
		*(uint64_t*)L_28 = L_29;
		uint64_t* L_30 = V_4;
		uint64_t L_31 = V_6;
		uint64_t L_32 = V_5;
		uint64_t* L_33;
		L_33 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_30, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_31, ((int64_t)3))), (int64_t)L_32)));
		uint64_t L_34 = ___0_value;
		*(uint64_t*)L_33 = L_34;
		uint64_t* L_35 = V_4;
		uint64_t L_36 = V_6;
		uint64_t L_37 = V_5;
		uint64_t* L_38;
		L_38 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_35, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_36, ((int64_t)4))), (int64_t)L_37)));
		uint64_t L_39 = ___0_value;
		*(uint64_t*)L_38 = L_39;
		uint64_t* L_40 = V_4;
		uint64_t L_41 = V_6;
		uint64_t L_42 = V_5;
		uint64_t* L_43;
		L_43 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_40, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_41, ((int64_t)5))), (int64_t)L_42)));
		uint64_t L_44 = ___0_value;
		*(uint64_t*)L_43 = L_44;
		uint64_t* L_45 = V_4;
		uint64_t L_46 = V_6;
		uint64_t L_47 = V_5;
		uint64_t* L_48;
		L_48 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_45, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_46, ((int64_t)6))), (int64_t)L_47)));
		uint64_t L_49 = ___0_value;
		*(uint64_t*)L_48 = L_49;
		uint64_t* L_50 = V_4;
		uint64_t L_51 = V_6;
		uint64_t L_52 = V_5;
		uint64_t* L_53;
		L_53 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_50, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_51, ((int64_t)7))), (int64_t)L_52)));
		uint64_t L_54 = ___0_value;
		*(uint64_t*)L_53 = L_54;
		uint64_t L_55 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_55, ((int64_t)8)));
	}

IL_0110:
	{
		uint64_t L_56 = V_6;
		uint64_t L_57 = V_3;
		if ((!(((uint64_t)L_56) >= ((uint64_t)((int64_t)((int64_t)L_57&((int64_t)((int32_t)-8))))))))
		{
			goto IL_0064;
		}
	}
	{
		uint64_t L_58 = V_6;
		uint64_t L_59 = V_3;
		if ((!(((uint64_t)L_58) < ((uint64_t)((int64_t)((int64_t)L_59&((int64_t)((int32_t)-4))))))))
		{
			goto IL_0198;
		}
	}
	{
		uint64_t* L_60 = V_4;
		uint64_t L_61 = V_6;
		uint64_t L_62 = V_5;
		uint64_t* L_63;
		L_63 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_60, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_61, (int64_t)L_62)));
		uint64_t L_64 = ___0_value;
		*(uint64_t*)L_63 = L_64;
		uint64_t* L_65 = V_4;
		uint64_t L_66 = V_6;
		uint64_t L_67 = V_5;
		uint64_t* L_68;
		L_68 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_65, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_66, ((int64_t)1))), (int64_t)L_67)));
		uint64_t L_69 = ___0_value;
		*(uint64_t*)L_68 = L_69;
		uint64_t* L_70 = V_4;
		uint64_t L_71 = V_6;
		uint64_t L_72 = V_5;
		uint64_t* L_73;
		L_73 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_70, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_71, ((int64_t)2))), (int64_t)L_72)));
		uint64_t L_74 = ___0_value;
		*(uint64_t*)L_73 = L_74;
		uint64_t* L_75 = V_4;
		uint64_t L_76 = V_6;
		uint64_t L_77 = V_5;
		uint64_t* L_78;
		L_78 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_75, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_76, ((int64_t)3))), (int64_t)L_77)));
		uint64_t L_79 = ___0_value;
		*(uint64_t*)L_78 = L_79;
		uint64_t L_80 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_80, ((int64_t)4)));
		goto IL_0198;
	}

IL_017f:
	{
		uint64_t* L_81 = V_4;
		uint64_t L_82 = V_6;
		uint64_t L_83 = V_5;
		uint64_t* L_84;
		L_84 = il2cpp_unsafe_add_byte_offset<uint64_t,uint64_t>(L_81, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_82, (int64_t)L_83)));
		uint64_t L_85 = ___0_value;
		*(uint64_t*)L_84 = L_85;
		uint64_t L_86 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_86, ((int64_t)1)));
	}

IL_0198:
	{
		uint64_t L_87 = V_6;
		uint64_t L_88 = V_3;
		if ((!(((uint64_t)L_87) >= ((uint64_t)L_88))))
		{
			goto IL_017f;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_CopyTo_m4F011B6E94BE67BF00E6EEF0F52FC07C6143590D_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 ___0_destination, const RuntimeMethod* method) 
{
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_m62F446B5508B032A22CD1BA21D998626E6B8A5CC_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0038;
		}
	}
	{
		Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 L_2 = ___0_destination;
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_3 = L_2.____pointer;
		V_0 = L_3;
		uint64_t* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_0));
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_5 = __this->____pointer;
		V_0 = L_5;
		uint64_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_0));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m4482A7BD91B51B22F471D785B1D22FAE110CD908(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return;
	}

IL_0038:
	{
		ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D(NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_TryCopyTo_mAB8D99AE7F7B967B3A5381D38CA35D489ED76345_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 ___0_destination, const RuntimeMethod* method) 
{
	bool V_0 = false;
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		V_0 = (bool)0;
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_m62F446B5508B032A22CD1BA21D998626E6B8A5CC_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_003b;
		}
	}
	{
		Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 L_2 = ___0_destination;
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_3 = L_2.____pointer;
		V_1 = L_3;
		uint64_t* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_1));
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_5 = __this->____pointer;
		V_1 = L_5;
		uint64_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_1));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m4482A7BD91B51B22F471D785B1D22FAE110CD908(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		V_0 = (bool)1;
	}

IL_003b:
	{
		bool L_8 = V_0;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlySpan_1_tD579EC24AE7548EFE16428C9DC0EE0423C0FDA0F Span_1_op_Implicit_mF533047803AD7D0C594E51DBD4C261DA5D5E370D_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 ___0_span, const RuntimeMethod* method) 
{
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 L_0 = ___0_span;
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_1 = L_0.____pointer;
		V_0 = L_1;
		uint64_t* L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_0));
		Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 L_3 = ___0_span;
		int32_t L_4 = L_3.____length;
		ReadOnlySpan_1_tD579EC24AE7548EFE16428C9DC0EE0423C0FDA0F L_5;
		memset((&L_5), 0, sizeof(L_5));
		ReadOnlySpan_1__ctor_mC03446F4F48AEBD20B80591766ACE9CE79048BCE_inline((&L_5), L_2, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 17));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Span_1_ToString_m6E70F419212ED46A47A2900761157E051D0A88D7_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0DB46164953228904843938099AF66650313FEE5);
		s_Il2CppMethodInitialized = true;
	}
	Il2CppChar* V_0 = NULL;
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var) };
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		bool L_4;
		L_4 = Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_003e;
		}
	}
	{
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_5 = __this->____pointer;
		V_1 = L_5;
		uint64_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_1));
		Il2CppChar* L_7;
		L_7 = il2cpp_unsafe_as_ref<Il2CppChar>(L_6);
		V_0 = L_7;
		Il2CppChar* L_8 = V_0;
		int32_t L_9 = __this->____length;
		String_t* L_10;
		L_10 = String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2(NULL, (Il2CppChar*)((uintptr_t)L_8), 0, L_9, NULL);
		return L_10;
	}

IL_003e:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck((MemberInfo_t*)L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(12, (MemberInfo_t*)L_12);
		int32_t L_14 = __this->____length;
		int32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral0DB46164953228904843938099AF66650313FEE5, (RuntimeObject*)L_13, L_16, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 Span_1_Slice_m0B26EE6F75B6659A26656BAF1AE818DD4FF1C376_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, int32_t ___0_start, const RuntimeMethod* method) 
{
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) > ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_2 = __this->____pointer;
		V_0 = L_2;
		uint64_t* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_start;
		uint64_t* L_5;
		L_5 = il2cpp_unsafe_add<uint64_t,int32_t>(L_3, L_4);
		int32_t L_6 = __this->____length;
		int32_t L_7 = ___0_start;
		Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 L_8;
		memset((&L_8), 0, sizeof(L_8));
		Span_1__ctor_m3C68BA129DD44A3486C6B57A99412947A16855F6_inline((&L_8), L_5, ((int32_t)il2cpp_codegen_subtract(L_6, L_7)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 Span_1_Slice_m66C540ED824CAA1711823B4BF222E5F30BC81573_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, int32_t ___0_start, int32_t ___1_length, const RuntimeMethod* method) 
{
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0014;
		}
	}
	{
		int32_t L_2 = ___1_length;
		int32_t L_3 = __this->____length;
		int32_t L_4 = ___0_start;
		if ((!(((uint32_t)L_2) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(L_3, L_4))))))
		{
			goto IL_0019;
		}
	}

IL_0014:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0019:
	{
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_5 = __this->____pointer;
		V_0 = L_5;
		uint64_t* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_0));
		int32_t L_7 = ___0_start;
		uint64_t* L_8;
		L_8 = il2cpp_unsafe_add<uint64_t,int32_t>(L_6, L_7);
		int32_t L_9 = ___1_length;
		Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_m3C68BA129DD44A3486C6B57A99412947A16855F6_inline((&L_10), L_8, L_9, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_10;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* Span_1_ToArray_mE0817125C69F368855E8B03DE0BA9F37FA32377E_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, const RuntimeMethod* method) 
{
	ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_1;
		L_1 = Array_Empty_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m696D3DE2D89DD613C8C0EB15BE627EABEF780255_inline(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____length;
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_3 = (UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299*)(UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299*)SZArrayNew(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 20), (uint32_t)L_2);
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_4 = L_3;
		NullCheck((RuntimeArray*)L_4);
		uint8_t* L_5;
		L_5 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_4, NULL);
		uint64_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint64_t>(L_5);
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_7 = __this->____pointer;
		V_0 = L_7;
		uint64_t* L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(uint64_t, (Il2CppByReference*)(&V_0));
		int32_t L_9 = __this->____length;
		Buffer_Memmove_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m4482A7BD91B51B22F471D785B1D22FAE110CD908(L_6, L_8, (uint64_t)((int64_t)L_9), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m62F446B5508B032A22CD1BA21D998626E6B8A5CC_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_Equals_m96D35BF7B64CE4FB8444E07FC1FC616FFD98825F_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, RuntimeObject* ___0_obj, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_GetHashCode_m315CB2A6699FD8608FEA6FFEC966216869215960_gshared (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 Span_1_op_Implicit_m921D52D34E89E0735644236C2846D3DC7A41B774_gshared (UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* ___0_array, const RuntimeMethod* method) 
{
	{
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_0 = ___0_array;
		Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3 L_1;
		memset((&L_1), 0, sizeof(L_1));
		Span_1__ctor_mC83E2FCB4929477ACCB736B25B9E2F89FF2A07CF_inline((&L_1), L_0, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 21));
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mCC34AC618271C9F9C196DA22BA5DB8C6D8AD477D_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* ___0_array, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t460D4E78469192619B0075C15897A6987393AAF9));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2));
		goto IL_0037;
	}

IL_0037:
	{
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_4;
		L_4 = il2cpp_unsafe_as_ref<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2>(L_3);
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m8185F6B96EDF20580766D2EB6ACB212C7B2AF23C_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* ___0_array, int32_t ___1_start, int32_t ___2_length, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_1 = ___1_start;
		if (L_1)
		{
			goto IL_0009;
		}
	}
	{
		int32_t L_2 = ___2_length;
		if (!L_2)
		{
			goto IL_000e;
		}
	}

IL_0009:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t460D4E78469192619B0075C15897A6987393AAF9));
		return;
	}

IL_0016:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2));
		goto IL_0042;
	}

IL_0042:
	{
		int32_t L_4 = ___1_start;
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_5 = ___0_array;
		NullCheck(L_5);
		if ((!(((uint32_t)L_4) <= ((uint32_t)((int32_t)(((RuntimeArray*)L_5)->max_length))))))
		{
			goto IL_0050;
		}
	}
	{
		int32_t L_6 = ___2_length;
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_7 = ___0_array;
		NullCheck(L_7);
		int32_t L_8 = ___1_start;
		if ((!(((uint32_t)L_6) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_7)->max_length)), L_8))))))
		{
			goto IL_0055;
		}
	}

IL_0050:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0055:
	{
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_9 = ___0_array;
		NullCheck((RuntimeArray*)L_9);
		uint8_t* L_10;
		L_10 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_9, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_11;
		L_11 = il2cpp_unsafe_as_ref<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2>(L_10);
		int32_t L_12 = ___1_start;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_13;
		L_13 = il2cpp_unsafe_add<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,int32_t>(L_11, L_12);
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_14;
		memset((&L_14), 0, sizeof(L_14));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_14), L_13);
		__this->____pointer = L_14;
		int32_t L_15 = ___2_length;
		__this->____length = L_15;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mCCDE317B7EF9509DB97AFE5968CF15240BB7DE03_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_0 = ___1_length;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_1 = ___0_pointer;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_2;
		L_2 = il2cpp_unsafe_as_ref<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2>((uint8_t*)L_1);
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_3;
		memset((&L_3), 0, sizeof(L_3));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_3), L_2);
		__this->____pointer = L_3;
		int32_t L_4 = ___1_length;
		__this->____length = L_4;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m150E0E8C32CF01F22FFC539541D9F6EA05DB6967_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_0 = ___0_ptr;
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* Span_1_get_Item_m3314C5CC3CAE6E7D226566CE0DC46BC4C79DAC8F_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_index;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) >= ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5(NULL);
	}

IL_000e:
	{
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_2 = __this->____pointer;
		V_0 = L_2;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_index;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_5;
		L_5 = il2cpp_unsafe_add<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,int32_t>(L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* Span_1_GetPinnableReference_m6592220ED6E9E21535C79DAD2B5C7411440CE238_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, const RuntimeMethod* method) 
{
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_0010;
		}
	}
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_1;
		L_1 = il2cpp_unsafe_as_ref<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2>((void*)((uintptr_t)0));
		return L_1;
	}

IL_0010:
	{
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_2 = __this->____pointer;
		V_0 = L_2;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_0));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Clear_m6E7824DAEA34E2FE4D5FCE78B65604233DC05050_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, const RuntimeMethod* method) 
{
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		goto IL_0034;
	}

IL_0034:
	{
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_0 = __this->____pointer;
		V_0 = L_0;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_1;
		L_1 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_0));
		uint8_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint8_t>(L_1);
		int32_t L_3 = __this->____length;
		int32_t L_4;
		L_4 = il2cpp_unsafe_sizeof<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2>();
		SpanHelpers_ClearWithoutReferences_m65DB2925AE7A5FF88BB3EA1BF90513C9ADF0653D(L_2, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_3), ((int64_t)L_4))), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Fill_m617F7A3B9E7E3B0C5C66871FBB8BC05D5190FD48_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_value, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_1;
	memset((&V_1), 0, sizeof(V_1));
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 V_2;
	memset((&V_2), 0, sizeof(V_2));
	uint64_t V_3 = 0;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* V_4 = NULL;
	uint64_t V_5 = 0;
	uint64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = il2cpp_unsafe_sizeof<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2>();
		if ((!(((uint32_t)L_0) == ((uint32_t)1))))
		{
			goto IL_0037;
		}
	}
	{
		int32_t L_1 = __this->____length;
		V_0 = (uint32_t)L_1;
		uint32_t L_2 = V_0;
		if (L_2)
		{
			goto IL_0013;
		}
	}
	{
		return;
	}

IL_0013:
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_3 = ___0_value;
		V_1 = L_3;
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_4 = __this->____pointer;
		V_2 = L_4;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_5;
		L_5 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_2));
		uint8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint8_t>(L_5);
		uint8_t* L_7;
		L_7 = il2cpp_unsafe_as_ref<uint8_t>((&V_1));
		int32_t L_8 = *((uint8_t*)L_7);
		uint32_t L_9 = V_0;
		Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A(L_6, (uint8_t)L_8, L_9, NULL);
		return;
	}

IL_0037:
	{
		int32_t L_10 = __this->____length;
		V_3 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_10));
		uint64_t L_11 = V_3;
		if (L_11)
		{
			goto IL_0043;
		}
	}
	{
		return;
	}

IL_0043:
	{
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_12 = __this->____pointer;
		V_2 = L_12;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_13;
		L_13 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_2));
		V_4 = L_13;
		int32_t L_14;
		L_14 = il2cpp_unsafe_sizeof<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2>();
		V_5 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_14));
		V_6 = (uint64_t)((int64_t)0);
		goto IL_0110;
	}

IL_0064:
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_15 = V_4;
		uint64_t L_16 = V_6;
		uint64_t L_17 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_18;
		L_18 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_15, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_16, (int64_t)L_17)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_19 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_18 = L_19;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_20 = V_4;
		uint64_t L_21 = V_6;
		uint64_t L_22 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_23;
		L_23 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_20, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_21, ((int64_t)1))), (int64_t)L_22)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_24 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_23 = L_24;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_25 = V_4;
		uint64_t L_26 = V_6;
		uint64_t L_27 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_28;
		L_28 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_25, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_26, ((int64_t)2))), (int64_t)L_27)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_29 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_28 = L_29;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_30 = V_4;
		uint64_t L_31 = V_6;
		uint64_t L_32 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_33;
		L_33 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_30, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_31, ((int64_t)3))), (int64_t)L_32)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_34 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_33 = L_34;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_35 = V_4;
		uint64_t L_36 = V_6;
		uint64_t L_37 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_38;
		L_38 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_35, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_36, ((int64_t)4))), (int64_t)L_37)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_39 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_38 = L_39;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_40 = V_4;
		uint64_t L_41 = V_6;
		uint64_t L_42 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_43;
		L_43 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_40, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_41, ((int64_t)5))), (int64_t)L_42)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_44 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_43 = L_44;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_45 = V_4;
		uint64_t L_46 = V_6;
		uint64_t L_47 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_48;
		L_48 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_45, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_46, ((int64_t)6))), (int64_t)L_47)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_49 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_48 = L_49;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_50 = V_4;
		uint64_t L_51 = V_6;
		uint64_t L_52 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_53;
		L_53 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_50, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_51, ((int64_t)7))), (int64_t)L_52)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_54 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_53 = L_54;
		uint64_t L_55 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_55, ((int64_t)8)));
	}

IL_0110:
	{
		uint64_t L_56 = V_6;
		uint64_t L_57 = V_3;
		if ((!(((uint64_t)L_56) >= ((uint64_t)((int64_t)((int64_t)L_57&((int64_t)((int32_t)-8))))))))
		{
			goto IL_0064;
		}
	}
	{
		uint64_t L_58 = V_6;
		uint64_t L_59 = V_3;
		if ((!(((uint64_t)L_58) < ((uint64_t)((int64_t)((int64_t)L_59&((int64_t)((int32_t)-4))))))))
		{
			goto IL_0198;
		}
	}
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_60 = V_4;
		uint64_t L_61 = V_6;
		uint64_t L_62 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_63;
		L_63 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_60, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_61, (int64_t)L_62)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_64 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_63 = L_64;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_65 = V_4;
		uint64_t L_66 = V_6;
		uint64_t L_67 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_68;
		L_68 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_65, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_66, ((int64_t)1))), (int64_t)L_67)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_69 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_68 = L_69;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_70 = V_4;
		uint64_t L_71 = V_6;
		uint64_t L_72 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_73;
		L_73 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_70, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_71, ((int64_t)2))), (int64_t)L_72)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_74 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_73 = L_74;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_75 = V_4;
		uint64_t L_76 = V_6;
		uint64_t L_77 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_78;
		L_78 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_75, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_76, ((int64_t)3))), (int64_t)L_77)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_79 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_78 = L_79;
		uint64_t L_80 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_80, ((int64_t)4)));
		goto IL_0198;
	}

IL_017f:
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_81 = V_4;
		uint64_t L_82 = V_6;
		uint64_t L_83 = V_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_84;
		L_84 = il2cpp_unsafe_add_byte_offset<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,uint64_t>(L_81, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_82, (int64_t)L_83)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_85 = ___0_value;
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_84 = L_85;
		uint64_t L_86 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_86, ((int64_t)1)));
	}

IL_0198:
	{
		uint64_t L_87 = V_6;
		uint64_t L_88 = V_3;
		if ((!(((uint64_t)L_87) >= ((uint64_t)L_88))))
		{
			goto IL_017f;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_CopyTo_m0F08E3E374BC97DFF1704281DD48947FD3C36583_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, Span_1_t460D4E78469192619B0075C15897A6987393AAF9 ___0_destination, const RuntimeMethod* method) 
{
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_m556440A32F0A1EB65C95120CEC693B9AC4DFD52B_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0038;
		}
	}
	{
		Span_1_t460D4E78469192619B0075C15897A6987393AAF9 L_2 = ___0_destination;
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_3 = L_2.____pointer;
		V_0 = L_3;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_0));
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_5 = __this->____pointer;
		V_0 = L_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_0));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisVector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_m5E3924B178120F38353E13C8B4545C90C49D6CED(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return;
	}

IL_0038:
	{
		ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D(NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_TryCopyTo_mE22F2A768A1050E6BFB3CD8B4AEC7B819C991C36_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, Span_1_t460D4E78469192619B0075C15897A6987393AAF9 ___0_destination, const RuntimeMethod* method) 
{
	bool V_0 = false;
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		V_0 = (bool)0;
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_m556440A32F0A1EB65C95120CEC693B9AC4DFD52B_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_003b;
		}
	}
	{
		Span_1_t460D4E78469192619B0075C15897A6987393AAF9 L_2 = ___0_destination;
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_3 = L_2.____pointer;
		V_1 = L_3;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_1));
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_5 = __this->____pointer;
		V_1 = L_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_1));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_TisVector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_m5E3924B178120F38353E13C8B4545C90C49D6CED(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		V_0 = (bool)1;
	}

IL_003b:
	{
		bool L_8 = V_0;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlySpan_1_t8C2C24B6B10C9EAC8D8A8C92BED569A12615C2AE Span_1_op_Implicit_m5D47150C8F313F781084C0B7AB26B085FA5BFA1D_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9 ___0_span, const RuntimeMethod* method) 
{
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Span_1_t460D4E78469192619B0075C15897A6987393AAF9 L_0 = ___0_span;
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_1 = L_0.____pointer;
		V_0 = L_1;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_0));
		Span_1_t460D4E78469192619B0075C15897A6987393AAF9 L_3 = ___0_span;
		int32_t L_4 = L_3.____length;
		ReadOnlySpan_1_t8C2C24B6B10C9EAC8D8A8C92BED569A12615C2AE L_5;
		memset((&L_5), 0, sizeof(L_5));
		ReadOnlySpan_1__ctor_m682BD3D57C99591BB5B14A415A7F8F4D5B14241F_inline((&L_5), L_2, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 17));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Span_1_ToString_mF3BF6D62DF3F7F4ADB18BC40C16BA8E73DABBBB9_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0DB46164953228904843938099AF66650313FEE5);
		s_Il2CppMethodInitialized = true;
	}
	Il2CppChar* V_0 = NULL;
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var) };
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		bool L_4;
		L_4 = Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_003e;
		}
	}
	{
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_5 = __this->____pointer;
		V_1 = L_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_1));
		Il2CppChar* L_7;
		L_7 = il2cpp_unsafe_as_ref<Il2CppChar>(L_6);
		V_0 = L_7;
		Il2CppChar* L_8 = V_0;
		int32_t L_9 = __this->____length;
		String_t* L_10;
		L_10 = String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2(NULL, (Il2CppChar*)((uintptr_t)L_8), 0, L_9, NULL);
		return L_10;
	}

IL_003e:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck((MemberInfo_t*)L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(12, (MemberInfo_t*)L_12);
		int32_t L_14 = __this->____length;
		int32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral0DB46164953228904843938099AF66650313FEE5, (RuntimeObject*)L_13, L_16, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t460D4E78469192619B0075C15897A6987393AAF9 Span_1_Slice_mC6EDEB2937854A25EE8250D473E51F3B43A568D5_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, int32_t ___0_start, const RuntimeMethod* method) 
{
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) > ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_2 = __this->____pointer;
		V_0 = L_2;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_start;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_5;
		L_5 = il2cpp_unsafe_add<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,int32_t>(L_3, L_4);
		int32_t L_6 = __this->____length;
		int32_t L_7 = ___0_start;
		Span_1_t460D4E78469192619B0075C15897A6987393AAF9 L_8;
		memset((&L_8), 0, sizeof(L_8));
		Span_1__ctor_m150E0E8C32CF01F22FFC539541D9F6EA05DB6967_inline((&L_8), L_5, ((int32_t)il2cpp_codegen_subtract(L_6, L_7)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t460D4E78469192619B0075C15897A6987393AAF9 Span_1_Slice_mEC63F7379D6B0247C3FAD97651111BEB91D13893_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, int32_t ___0_start, int32_t ___1_length, const RuntimeMethod* method) 
{
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0014;
		}
	}
	{
		int32_t L_2 = ___1_length;
		int32_t L_3 = __this->____length;
		int32_t L_4 = ___0_start;
		if ((!(((uint32_t)L_2) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(L_3, L_4))))))
		{
			goto IL_0019;
		}
	}

IL_0014:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0019:
	{
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_5 = __this->____pointer;
		V_0 = L_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_0));
		int32_t L_7 = ___0_start;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_8;
		L_8 = il2cpp_unsafe_add<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2,int32_t>(L_6, L_7);
		int32_t L_9 = ___1_length;
		Span_1_t460D4E78469192619B0075C15897A6987393AAF9 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_m150E0E8C32CF01F22FFC539541D9F6EA05DB6967_inline((&L_10), L_8, L_9, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_10;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* Span_1_ToArray_m2E6BEE60BD48A117C372CF405F27139869922A06_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, const RuntimeMethod* method) 
{
	ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_1;
		L_1 = Array_Empty_TisVector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_mFE64AD477B9A8397B2CCC3FD2565DCA5B2F0A1F6_inline(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____length;
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_3 = (Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C*)(Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C*)SZArrayNew(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 20), (uint32_t)L_2);
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_4 = L_3;
		NullCheck((RuntimeArray*)L_4);
		uint8_t* L_5;
		L_5 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_4, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_6;
		L_6 = il2cpp_unsafe_as_ref<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2>(L_5);
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_7 = __this->____pointer;
		V_0 = L_7;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, (Il2CppByReference*)(&V_0));
		int32_t L_9 = __this->____length;
		Buffer_Memmove_TisVector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_m5E3924B178120F38353E13C8B4545C90C49D6CED(L_6, L_8, (uint64_t)((int64_t)L_9), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m556440A32F0A1EB65C95120CEC693B9AC4DFD52B_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_Equals_m16593C3984238ABD5B94D35774829B067CC1DBD0_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, RuntimeObject* ___0_obj, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_GetHashCode_m2159E9A9023A510BCD9320B5D5DDB55AC7389D3A_gshared (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t460D4E78469192619B0075C15897A6987393AAF9 Span_1_op_Implicit_mA24A1765CD7932C77A7F028A1BECC4BFEF99B500_gshared (Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* ___0_array, const RuntimeMethod* method) 
{
	{
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_0 = ___0_array;
		Span_1_t460D4E78469192619B0075C15897A6987393AAF9 L_1;
		memset((&L_1), 0, sizeof(L_1));
		Span_1__ctor_mCC34AC618271C9F9C196DA22BA5DB8C6D8AD477D_inline((&L_1), L_0, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 21));
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m94A95CF4DF158FDF992CC13DA185B637335D84C6_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_array, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	const uint32_t SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	Il2CppFullySharedGenericAny V_0 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	memset(V_0, 0, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_0, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy(L_1, V_0, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		bool L_2 = il2cpp_codegen_would_box_to_non_null(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2), L_1);
		if (L_2)
		{
			goto IL_0037;
		}
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = ___0_array;
		NullCheck((RuntimeObject*)L_3);
		Type_t* L_4;
		L_4 = Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3((RuntimeObject*)L_3, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_5 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 3)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_6;
		L_6 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_5, NULL);
		bool L_7;
		L_7 = Type_op_Inequality_m83209C7BB3C05DFBEA3B6199B0BEFE8037301172(L_4, L_6, NULL);
		if (!L_7)
		{
			goto IL_0037;
		}
	}
	{
		ThrowHelper_ThrowArrayTypeMismatchException_m781AD7A903FEA43FAE3137977E6BC5F9BAEBC590(NULL);
	}

IL_0037:
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_8 = ___0_array;
		NullCheck((RuntimeArray*)L_8);
		uint8_t* L_9;
		L_9 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_8, NULL);
		Il2CppFullySharedGenericAny* L_10;
		L_10 = il2cpp_unsafe_as_ref<Il2CppFullySharedGenericAny>(L_9);
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_11;
		memset((&L_11), 0, sizeof(L_11));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_11), L_10);
		__this->____pointer = L_11;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_12 = ___0_array;
		NullCheck(L_12);
		__this->____length = ((int32_t)(((RuntimeArray*)L_12)->max_length));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m663A61429C38D76851892CB8A3E875E44548618D_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_array, int32_t ___1_start, int32_t ___2_length, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	const uint32_t SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_3 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	Il2CppFullySharedGenericAny V_0 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	memset(V_0, 0, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_1 = ___1_start;
		if (L_1)
		{
			goto IL_0009;
		}
	}
	{
		int32_t L_2 = ___2_length;
		if (!L_2)
		{
			goto IL_000e;
		}
	}

IL_0009:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54));
		return;
	}

IL_0016:
	{
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_0, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy(L_3, V_0, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		bool L_4 = il2cpp_codegen_would_box_to_non_null(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2), L_3);
		if (L_4)
		{
			goto IL_0042;
		}
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_5 = ___0_array;
		NullCheck((RuntimeObject*)L_5);
		Type_t* L_6;
		L_6 = Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3((RuntimeObject*)L_5, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_7 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 3)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_8;
		L_8 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_7, NULL);
		bool L_9;
		L_9 = Type_op_Inequality_m83209C7BB3C05DFBEA3B6199B0BEFE8037301172(L_6, L_8, NULL);
		if (!L_9)
		{
			goto IL_0042;
		}
	}
	{
		ThrowHelper_ThrowArrayTypeMismatchException_m781AD7A903FEA43FAE3137977E6BC5F9BAEBC590(NULL);
	}

IL_0042:
	{
		int32_t L_10 = ___1_start;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_11 = ___0_array;
		NullCheck(L_11);
		if ((!(((uint32_t)L_10) <= ((uint32_t)((int32_t)(((RuntimeArray*)L_11)->max_length))))))
		{
			goto IL_0050;
		}
	}
	{
		int32_t L_12 = ___2_length;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_13 = ___0_array;
		NullCheck(L_13);
		int32_t L_14 = ___1_start;
		if ((!(((uint32_t)L_12) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_13)->max_length)), L_14))))))
		{
			goto IL_0055;
		}
	}

IL_0050:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0055:
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_15 = ___0_array;
		NullCheck((RuntimeArray*)L_15);
		uint8_t* L_16;
		L_16 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_15, NULL);
		Il2CppFullySharedGenericAny* L_17;
		L_17 = il2cpp_unsafe_as_ref<Il2CppFullySharedGenericAny>(L_16);
		int32_t L_18 = ___1_start;
		Il2CppFullySharedGenericAny* L_19;
		L_19 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, int32_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 7)))(L_17, L_18, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 7));
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_20;
		memset((&L_20), 0, sizeof(L_20));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_20), L_19);
		__this->____pointer = L_20;
		int32_t L_21 = ___2_length;
		__this->____length = L_21;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m5599DAEC88C08C9797F461E977BF22E14E3C3008_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		bool L_0;
		L_0 = il2cpp_codegen_is_reference_or_contains_references(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 8));
		if (!L_0)
		{
			goto IL_0016;
		}
	}
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_1 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_2;
		L_2 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_1, NULL);
		ThrowHelper_ThrowInvalidTypeWithPointersNotSupported_m5707DE408588F6EAC3FC7D10F9520308CF8C8CCF(L_2, NULL);
	}

IL_0016:
	{
		int32_t L_3 = ___1_length;
		if ((((int32_t)L_3) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_4 = ___0_pointer;
		Il2CppFullySharedGenericAny* L_5;
		L_5 = il2cpp_unsafe_as_ref<Il2CppFullySharedGenericAny>((uint8_t*)L_4);
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_6;
		memset((&L_6), 0, sizeof(L_6));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_6), L_5);
		__this->____pointer = L_6;
		int32_t L_7 = ___1_length;
		__this->____length = L_7;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mBA868F06359701D9950DEB1B10F52F848E9FF6DA_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, Il2CppFullySharedGenericAny* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_ptr;
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppFullySharedGenericAny* Span_1_get_Item_m9C593C1A8E070D42D9DC7DB6C73CECDFB5626B81_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_index;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) >= ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5(NULL);
	}

IL_000e:
	{
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_2 = __this->____pointer;
		V_0 = L_2;
		Il2CppFullySharedGenericAny* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_index;
		Il2CppFullySharedGenericAny* L_5;
		L_5 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, int32_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 7)))(L_3, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 7));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppFullySharedGenericAny* Span_1_GetPinnableReference_mBC5955DDAAEA56F142B5C441DB6FBD96F2AB6ADB_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, const RuntimeMethod* method) 
{
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_0010;
		}
	}
	{
		Il2CppFullySharedGenericAny* L_1;
		L_1 = il2cpp_unsafe_as_ref<Il2CppFullySharedGenericAny>((void*)((uintptr_t)0));
		return L_1;
	}

IL_0010:
	{
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_2 = __this->____pointer;
		V_0 = L_2;
		Il2CppFullySharedGenericAny* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_0));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Clear_m11A4E1CD4E1718E30A0C2DA5934B04EDE635A447_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, const RuntimeMethod* method) 
{
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		bool L_0;
		L_0 = il2cpp_codegen_is_reference_or_contains_references(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 8));
		if (!L_0)
		{
			goto IL_0034;
		}
	}
	{
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_1 = __this->____pointer;
		V_0 = L_1;
		Il2CppFullySharedGenericAny* L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_0));
		intptr_t* L_3;
		L_3 = il2cpp_unsafe_as_ref<intptr_t>(L_2);
		int32_t L_4 = __this->____length;
		int32_t L_5;
		L_5 = ((  int32_t (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 11)))(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 11));
		int32_t L_6;
		L_6 = IntPtr_get_Size_m1FAAA59DA73D7E32BB1AB55DD92A90AFE3251DBE(NULL);
		SpanHelpers_ClearWithReferences_m9641D8B6DC3AE81B4B0734BBA0E477EF131CD430(L_3, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_4), ((int64_t)((int32_t)(L_5/L_6))))), NULL);
		return;
	}

IL_0034:
	{
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_7 = __this->____pointer;
		V_0 = L_7;
		Il2CppFullySharedGenericAny* L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_0));
		uint8_t* L_9;
		L_9 = il2cpp_unsafe_as_ref<uint8_t>(L_8);
		int32_t L_10 = __this->____length;
		int32_t L_11;
		L_11 = ((  int32_t (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 11)))(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 11));
		SpanHelpers_ClearWithoutReferences_m65DB2925AE7A5FF88BB3EA1BF90513C9ADF0653D(L_9, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_10), ((int64_t)L_11))), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Fill_m6EAE11FECC435B38881A2D1F4D4575D178BCE162_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, Il2CppFullySharedGenericAny ___0_value, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_3 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	const Il2CppFullySharedGenericAny L_19 = L_3;
	const Il2CppFullySharedGenericAny L_64 = L_3;
	const Il2CppFullySharedGenericAny L_85 = L_3;
	const Il2CppFullySharedGenericAny L_24 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	const Il2CppFullySharedGenericAny L_69 = L_24;
	const Il2CppFullySharedGenericAny L_29 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	const Il2CppFullySharedGenericAny L_74 = L_29;
	const Il2CppFullySharedGenericAny L_34 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	const Il2CppFullySharedGenericAny L_79 = L_34;
	const Il2CppFullySharedGenericAny L_39 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	const Il2CppFullySharedGenericAny L_44 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	const Il2CppFullySharedGenericAny L_49 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	const Il2CppFullySharedGenericAny L_54 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	uint32_t V_0 = 0;
	Il2CppFullySharedGenericAny V_1 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	memset(V_1, 0, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 V_2;
	memset((&V_2), 0, sizeof(V_2));
	uint64_t V_3 = 0;
	Il2CppFullySharedGenericAny* V_4 = NULL;
	uint64_t V_5 = 0;
	uint64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = ((  int32_t (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 11)))(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 11));
		if ((!(((uint32_t)L_0) == ((uint32_t)1))))
		{
			goto IL_0037;
		}
	}
	{
		int32_t L_1 = __this->____length;
		V_0 = (uint32_t)L_1;
		uint32_t L_2 = V_0;
		if (L_2)
		{
			goto IL_0013;
		}
	}
	{
		return;
	}

IL_0013:
	{
		il2cpp_codegen_memcpy(L_3, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy(V_1, L_3, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_4 = __this->____pointer;
		V_2 = L_4;
		Il2CppFullySharedGenericAny* L_5;
		L_5 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_2));
		uint8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint8_t>(L_5);
		uint8_t* L_7;
		L_7 = il2cpp_unsafe_as_ref<uint8_t>((Il2CppFullySharedGenericAny*)V_1);
		int32_t L_8 = *((uint8_t*)L_7);
		uint32_t L_9 = V_0;
		Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A(L_6, (uint8_t)L_8, L_9, NULL);
		return;
	}

IL_0037:
	{
		int32_t L_10 = __this->____length;
		V_3 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_10));
		uint64_t L_11 = V_3;
		if (L_11)
		{
			goto IL_0043;
		}
	}
	{
		return;
	}

IL_0043:
	{
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_12 = __this->____pointer;
		V_2 = L_12;
		Il2CppFullySharedGenericAny* L_13;
		L_13 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_2));
		V_4 = L_13;
		int32_t L_14;
		L_14 = ((  int32_t (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 11)))(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 11));
		V_5 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_14));
		V_6 = (uint64_t)((int64_t)0);
		goto IL_0110;
	}

IL_0064:
	{
		Il2CppFullySharedGenericAny* L_15 = V_4;
		uint64_t L_16 = V_6;
		uint64_t L_17 = V_5;
		Il2CppFullySharedGenericAny* L_18;
		L_18 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_15, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_16, (int64_t)L_17)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_19, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_18, L_19, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_18, (void*)L_19);
		Il2CppFullySharedGenericAny* L_20 = V_4;
		uint64_t L_21 = V_6;
		uint64_t L_22 = V_5;
		Il2CppFullySharedGenericAny* L_23;
		L_23 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_20, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_21, ((int64_t)1))), (int64_t)L_22)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_24, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_23, L_24, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_23, (void*)L_24);
		Il2CppFullySharedGenericAny* L_25 = V_4;
		uint64_t L_26 = V_6;
		uint64_t L_27 = V_5;
		Il2CppFullySharedGenericAny* L_28;
		L_28 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_25, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_26, ((int64_t)2))), (int64_t)L_27)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_29, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_28, L_29, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_28, (void*)L_29);
		Il2CppFullySharedGenericAny* L_30 = V_4;
		uint64_t L_31 = V_6;
		uint64_t L_32 = V_5;
		Il2CppFullySharedGenericAny* L_33;
		L_33 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_30, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_31, ((int64_t)3))), (int64_t)L_32)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_34, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_33, L_34, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_33, (void*)L_34);
		Il2CppFullySharedGenericAny* L_35 = V_4;
		uint64_t L_36 = V_6;
		uint64_t L_37 = V_5;
		Il2CppFullySharedGenericAny* L_38;
		L_38 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_35, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_36, ((int64_t)4))), (int64_t)L_37)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_39, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_38, L_39, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_38, (void*)L_39);
		Il2CppFullySharedGenericAny* L_40 = V_4;
		uint64_t L_41 = V_6;
		uint64_t L_42 = V_5;
		Il2CppFullySharedGenericAny* L_43;
		L_43 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_40, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_41, ((int64_t)5))), (int64_t)L_42)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_44, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_43, L_44, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_43, (void*)L_44);
		Il2CppFullySharedGenericAny* L_45 = V_4;
		uint64_t L_46 = V_6;
		uint64_t L_47 = V_5;
		Il2CppFullySharedGenericAny* L_48;
		L_48 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_45, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_46, ((int64_t)6))), (int64_t)L_47)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_49, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_48, L_49, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_48, (void*)L_49);
		Il2CppFullySharedGenericAny* L_50 = V_4;
		uint64_t L_51 = V_6;
		uint64_t L_52 = V_5;
		Il2CppFullySharedGenericAny* L_53;
		L_53 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_50, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_51, ((int64_t)7))), (int64_t)L_52)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_54, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_53, L_54, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_53, (void*)L_54);
		uint64_t L_55 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_55, ((int64_t)8)));
	}

IL_0110:
	{
		uint64_t L_56 = V_6;
		uint64_t L_57 = V_3;
		if ((!(((uint64_t)L_56) >= ((uint64_t)((int64_t)((int64_t)L_57&((int64_t)((int32_t)-8))))))))
		{
			goto IL_0064;
		}
	}
	{
		uint64_t L_58 = V_6;
		uint64_t L_59 = V_3;
		if ((!(((uint64_t)L_58) < ((uint64_t)((int64_t)((int64_t)L_59&((int64_t)((int32_t)-4))))))))
		{
			goto IL_0198;
		}
	}
	{
		Il2CppFullySharedGenericAny* L_60 = V_4;
		uint64_t L_61 = V_6;
		uint64_t L_62 = V_5;
		Il2CppFullySharedGenericAny* L_63;
		L_63 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_60, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_61, (int64_t)L_62)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_64, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_63, L_64, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_63, (void*)L_64);
		Il2CppFullySharedGenericAny* L_65 = V_4;
		uint64_t L_66 = V_6;
		uint64_t L_67 = V_5;
		Il2CppFullySharedGenericAny* L_68;
		L_68 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_65, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_66, ((int64_t)1))), (int64_t)L_67)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_69, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_68, L_69, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_68, (void*)L_69);
		Il2CppFullySharedGenericAny* L_70 = V_4;
		uint64_t L_71 = V_6;
		uint64_t L_72 = V_5;
		Il2CppFullySharedGenericAny* L_73;
		L_73 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_70, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_71, ((int64_t)2))), (int64_t)L_72)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_74, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_73, L_74, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_73, (void*)L_74);
		Il2CppFullySharedGenericAny* L_75 = V_4;
		uint64_t L_76 = V_6;
		uint64_t L_77 = V_5;
		Il2CppFullySharedGenericAny* L_78;
		L_78 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_75, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_76, ((int64_t)3))), (int64_t)L_77)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_79, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_78, L_79, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_78, (void*)L_79);
		uint64_t L_80 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_80, ((int64_t)4)));
		goto IL_0198;
	}

IL_017f:
	{
		Il2CppFullySharedGenericAny* L_81 = V_4;
		uint64_t L_82 = V_6;
		uint64_t L_83 = V_5;
		Il2CppFullySharedGenericAny* L_84;
		L_84 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12)))(L_81, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_82, (int64_t)L_83)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 12));
		il2cpp_codegen_memcpy(L_85, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2)) ? ___0_value : &___0_value), SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_84, L_85, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 2), (void**)(Il2CppFullySharedGenericAny*)L_84, (void*)L_85);
		uint64_t L_86 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_86, ((int64_t)1)));
	}

IL_0198:
	{
		uint64_t L_87 = V_6;
		uint64_t L_88 = V_3;
		if ((!(((uint64_t)L_87) >= ((uint64_t)L_88))))
		{
			goto IL_017f;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_CopyTo_m87850B36DC83BF310EC6136E6D14DC5634F96F05_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 ___0_destination, const RuntimeMethod* method) 
{
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = ((  int32_t (*) (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13)))((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0038;
		}
	}
	{
		Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 L_2 = ___0_destination;
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_3 = L_2.____pointer;
		V_0 = L_3;
		Il2CppFullySharedGenericAny* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_0));
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_5 = __this->____pointer;
		V_0 = L_5;
		Il2CppFullySharedGenericAny* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_0));
		int32_t L_7 = __this->____length;
		((  void (*) (Il2CppFullySharedGenericAny*, Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15)))(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return;
	}

IL_0038:
	{
		ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D(NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_TryCopyTo_m464C81DB101113B73DCD3F43C757D672CD893B37_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 ___0_destination, const RuntimeMethod* method) 
{
	bool V_0 = false;
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		V_0 = (bool)0;
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = ((  int32_t (*) (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13)))((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_003b;
		}
	}
	{
		Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 L_2 = ___0_destination;
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_3 = L_2.____pointer;
		V_1 = L_3;
		Il2CppFullySharedGenericAny* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_1));
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_5 = __this->____pointer;
		V_1 = L_5;
		Il2CppFullySharedGenericAny* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_1));
		int32_t L_7 = __this->____length;
		((  void (*) (Il2CppFullySharedGenericAny*, Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15)))(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		V_0 = (bool)1;
	}

IL_003b:
	{
		bool L_8 = V_0;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlySpan_1_tC416A5627E04F69CA2947A2A13F0A1DF096CABAC Span_1_op_Implicit_m704A5B9FD25EEA23756181A8EB40B875387A6C01_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 ___0_span, const RuntimeMethod* method) 
{
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 L_0 = ___0_span;
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_1 = L_0.____pointer;
		V_0 = L_1;
		Il2CppFullySharedGenericAny* L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_0));
		Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 L_3 = ___0_span;
		int32_t L_4 = L_3.____length;
		ReadOnlySpan_1_tC416A5627E04F69CA2947A2A13F0A1DF096CABAC L_5;
		memset((&L_5), 0, sizeof(L_5));
		ReadOnlySpan_1__ctor_mFA6EE52BCF39100AE30C79E73F0F972182D0CA2A_inline((&L_5), L_2, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 17));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Span_1_ToString_m51B73F86825C26B44AF2E5C9152D807780EB84ED_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0DB46164953228904843938099AF66650313FEE5);
		s_Il2CppMethodInitialized = true;
	}
	Il2CppChar* V_0 = NULL;
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var) };
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		bool L_4;
		L_4 = Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_003e;
		}
	}
	{
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_5 = __this->____pointer;
		V_1 = L_5;
		Il2CppFullySharedGenericAny* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_1));
		Il2CppChar* L_7;
		L_7 = il2cpp_unsafe_as_ref<Il2CppChar>(L_6);
		V_0 = L_7;
		Il2CppChar* L_8 = V_0;
		int32_t L_9 = __this->____length;
		String_t* L_10;
		L_10 = String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2(NULL, (Il2CppChar*)((uintptr_t)L_8), 0, L_9, NULL);
		return L_10;
	}

IL_003e:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck((MemberInfo_t*)L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(12, (MemberInfo_t*)L_12);
		int32_t L_14 = __this->____length;
		int32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral0DB46164953228904843938099AF66650313FEE5, (RuntimeObject*)L_13, L_16, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 Span_1_Slice_mC857EC48EAC26C4D9A5C6302BA08A7796020C8E1_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, int32_t ___0_start, const RuntimeMethod* method) 
{
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) > ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_2 = __this->____pointer;
		V_0 = L_2;
		Il2CppFullySharedGenericAny* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_start;
		Il2CppFullySharedGenericAny* L_5;
		L_5 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, int32_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 7)))(L_3, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 7));
		int32_t L_6 = __this->____length;
		int32_t L_7 = ___0_start;
		Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 L_8;
		memset((&L_8), 0, sizeof(L_8));
		Span_1__ctor_mBA868F06359701D9950DEB1B10F52F848E9FF6DA_inline((&L_8), L_5, ((int32_t)il2cpp_codegen_subtract(L_6, L_7)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 Span_1_Slice_m4D5C2B295B93702EF492EC0660798DE3BFC3FFDA_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, int32_t ___0_start, int32_t ___1_length, const RuntimeMethod* method) 
{
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0014;
		}
	}
	{
		int32_t L_2 = ___1_length;
		int32_t L_3 = __this->____length;
		int32_t L_4 = ___0_start;
		if ((!(((uint32_t)L_2) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(L_3, L_4))))))
		{
			goto IL_0019;
		}
	}

IL_0014:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0019:
	{
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_5 = __this->____pointer;
		V_0 = L_5;
		Il2CppFullySharedGenericAny* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_0));
		int32_t L_7 = ___0_start;
		Il2CppFullySharedGenericAny* L_8;
		L_8 = ((  Il2CppFullySharedGenericAny* (*) (Il2CppFullySharedGenericAny*, int32_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 7)))(L_6, L_7, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 7));
		int32_t L_9 = ___1_length;
		Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mBA868F06359701D9950DEB1B10F52F848E9FF6DA_inline((&L_10), L_8, L_9, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_10;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* Span_1_ToArray_mBB0A9E11BBAA9FDE1D0C045FBA14F4CD3E84773E_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, const RuntimeMethod* method) 
{
	ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_1;
		L_1 = ((  __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19)))(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____length;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*)(__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*)SZArrayNew(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 20), (uint32_t)L_2);
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_4 = L_3;
		NullCheck((RuntimeArray*)L_4);
		uint8_t* L_5;
		L_5 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_4, NULL);
		Il2CppFullySharedGenericAny* L_6;
		L_6 = il2cpp_unsafe_as_ref<Il2CppFullySharedGenericAny>(L_5);
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_7 = __this->____pointer;
		V_0 = L_7;
		Il2CppFullySharedGenericAny* L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(Il2CppFullySharedGenericAny, (Il2CppByReference*)(&V_0));
		int32_t L_9 = __this->____length;
		((  void (*) (Il2CppFullySharedGenericAny*, Il2CppFullySharedGenericAny*, uint64_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15)))(L_6, L_8, (uint64_t)((int64_t)L_9), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m4CED98A19744D579382FDCEDEF16DBC11586BE1C_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_Equals_m40491EA378B979FB91CD7BC368CA95BE931D13F0_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, RuntimeObject* ___0_obj, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_GetHashCode_m3061054FFC5FFBF234FA34F5319A12C2E9241B3F_gshared (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 Span_1_op_Implicit_mEAE085B1C884F71E5CF01E09CD6F285B497BBDE8_gshared (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_array, const RuntimeMethod* method) 
{
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0 = ___0_array;
		Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54 L_1;
		memset((&L_1), 0, sizeof(L_1));
		Span_1__ctor_m94A95CF4DF158FDF992CC13DA185B637335D84C6_inline((&L_1), L_0, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 21));
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m13E8571165DB8DB1A1909B44B65D4F220890AC8F_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* ___0_array, const RuntimeMethod* method) 
{
	jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225));
		goto IL_0037;
	}

IL_0037:
	{
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_4;
		L_4 = il2cpp_unsafe_as_ref<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225>(L_3);
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m6124823D76DBCB84B895045F4072C5D8BAD54418_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* ___0_array, int32_t ___1_start, int32_t ___2_length, const RuntimeMethod* method) 
{
	jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_1 = ___1_start;
		if (L_1)
		{
			goto IL_0009;
		}
	}
	{
		int32_t L_2 = ___2_length;
		if (!L_2)
		{
			goto IL_000e;
		}
	}

IL_0009:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		return;
	}

IL_0016:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225));
		goto IL_0042;
	}

IL_0042:
	{
		int32_t L_4 = ___1_start;
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_5 = ___0_array;
		NullCheck(L_5);
		if ((!(((uint32_t)L_4) <= ((uint32_t)((int32_t)(((RuntimeArray*)L_5)->max_length))))))
		{
			goto IL_0050;
		}
	}
	{
		int32_t L_6 = ___2_length;
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_7 = ___0_array;
		NullCheck(L_7);
		int32_t L_8 = ___1_start;
		if ((!(((uint32_t)L_6) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_7)->max_length)), L_8))))))
		{
			goto IL_0055;
		}
	}

IL_0050:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0055:
	{
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_9 = ___0_array;
		NullCheck((RuntimeArray*)L_9);
		uint8_t* L_10;
		L_10 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_9, NULL);
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_11;
		L_11 = il2cpp_unsafe_as_ref<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225>(L_10);
		int32_t L_12 = ___1_start;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_13;
		L_13 = il2cpp_unsafe_add<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,int32_t>(L_11, L_12);
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_14;
		memset((&L_14), 0, sizeof(L_14));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_14), L_13);
		__this->____pointer = L_14;
		int32_t L_15 = ___2_length;
		__this->____length = L_15;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_0 = ___1_length;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_1 = ___0_pointer;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_2;
		L_2 = il2cpp_unsafe_as_ref<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225>((uint8_t*)L_1);
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_3;
		memset((&L_3), 0, sizeof(L_3));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_3), L_2);
		__this->____pointer = L_3;
		int32_t L_4 = ___1_length;
		__this->____length = L_4;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1__ctor_m1DA3ED274C046791E48A52F415066C7C86B031D1_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_0 = ___0_ptr;
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* Span_1_get_Item_mB5F50B0D20B3D8E944F96112B8B4DAC7D17650EA_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_index;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) >= ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowIndexOutOfRangeException_m86F753A24E2765A35546BA6352A7E4F0BB8A66B5(NULL);
	}

IL_000e:
	{
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_2 = __this->____pointer;
		V_0 = L_2;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_index;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_5;
		L_5 = il2cpp_unsafe_add<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,int32_t>(L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* Span_1_GetPinnableReference_m56356CEAA8E67F718DC61AF6B7C4A08BB6CB7049_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, const RuntimeMethod* method) 
{
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_0010;
		}
	}
	{
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_1;
		L_1 = il2cpp_unsafe_as_ref<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225>((void*)((uintptr_t)0));
		return L_1;
	}

IL_0010:
	{
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_2 = __this->____pointer;
		V_0 = L_2;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_0));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Clear_m91ABCA1B4BF1AC38DEC21718D6A7A946A42A62EF_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, const RuntimeMethod* method) 
{
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		goto IL_0034;
	}

IL_0034:
	{
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_0 = __this->____pointer;
		V_0 = L_0;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_1;
		L_1 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_0));
		uint8_t* L_2;
		L_2 = il2cpp_unsafe_as_ref<uint8_t>(L_1);
		int32_t L_3 = __this->____length;
		int32_t L_4;
		L_4 = il2cpp_unsafe_sizeof<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225>();
		SpanHelpers_ClearWithoutReferences_m65DB2925AE7A5FF88BB3EA1BF90513C9ADF0653D(L_2, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)L_3), ((int64_t)L_4))), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_Fill_m5F214A54E6119724209D9A6022CF24DDB9C02AF6_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 ___0_value, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 V_1;
	memset((&V_1), 0, sizeof(V_1));
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC V_2;
	memset((&V_2), 0, sizeof(V_2));
	uint64_t V_3 = 0;
	jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* V_4 = NULL;
	uint64_t V_5 = 0;
	uint64_t V_6 = 0;
	{
		int32_t L_0;
		L_0 = il2cpp_unsafe_sizeof<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225>();
		if ((!(((uint32_t)L_0) == ((uint32_t)1))))
		{
			goto IL_0037;
		}
	}
	{
		int32_t L_1 = __this->____length;
		V_0 = (uint32_t)L_1;
		uint32_t L_2 = V_0;
		if (L_2)
		{
			goto IL_0013;
		}
	}
	{
		return;
	}

IL_0013:
	{
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_3 = ___0_value;
		V_1 = L_3;
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_4 = __this->____pointer;
		V_2 = L_4;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_5;
		L_5 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_2));
		uint8_t* L_6;
		L_6 = il2cpp_unsafe_as_ref<uint8_t>(L_5);
		uint8_t* L_7;
		L_7 = il2cpp_unsafe_as_ref<uint8_t>((&V_1));
		int32_t L_8 = *((uint8_t*)L_7);
		uint32_t L_9 = V_0;
		Unsafe_InitBlockUnaligned_m6F2353EB9ABC9320E61629FAEE23948C80BFF03A(L_6, (uint8_t)L_8, L_9, NULL);
		return;
	}

IL_0037:
	{
		int32_t L_10 = __this->____length;
		V_3 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_10));
		uint64_t L_11 = V_3;
		if (L_11)
		{
			goto IL_0043;
		}
	}
	{
		return;
	}

IL_0043:
	{
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_12 = __this->____pointer;
		V_2 = L_12;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_13;
		L_13 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_2));
		V_4 = L_13;
		int32_t L_14;
		L_14 = il2cpp_unsafe_sizeof<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225>();
		V_5 = (uint64_t)((int64_t)(uint64_t)((uint32_t)L_14));
		V_6 = (uint64_t)((int64_t)0);
		goto IL_0110;
	}

IL_0064:
	{
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_15 = V_4;
		uint64_t L_16 = V_6;
		uint64_t L_17 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_18;
		L_18 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_15, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_16, (int64_t)L_17)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_19 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_18 = L_19;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_20 = V_4;
		uint64_t L_21 = V_6;
		uint64_t L_22 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_23;
		L_23 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_20, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_21, ((int64_t)1))), (int64_t)L_22)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_24 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_23 = L_24;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_25 = V_4;
		uint64_t L_26 = V_6;
		uint64_t L_27 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_28;
		L_28 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_25, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_26, ((int64_t)2))), (int64_t)L_27)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_29 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_28 = L_29;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_30 = V_4;
		uint64_t L_31 = V_6;
		uint64_t L_32 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_33;
		L_33 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_30, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_31, ((int64_t)3))), (int64_t)L_32)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_34 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_33 = L_34;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_35 = V_4;
		uint64_t L_36 = V_6;
		uint64_t L_37 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_38;
		L_38 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_35, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_36, ((int64_t)4))), (int64_t)L_37)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_39 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_38 = L_39;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_40 = V_4;
		uint64_t L_41 = V_6;
		uint64_t L_42 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_43;
		L_43 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_40, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_41, ((int64_t)5))), (int64_t)L_42)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_44 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_43 = L_44;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_45 = V_4;
		uint64_t L_46 = V_6;
		uint64_t L_47 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_48;
		L_48 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_45, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_46, ((int64_t)6))), (int64_t)L_47)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_49 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_48 = L_49;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_50 = V_4;
		uint64_t L_51 = V_6;
		uint64_t L_52 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_53;
		L_53 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_50, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_51, ((int64_t)7))), (int64_t)L_52)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_54 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_53 = L_54;
		uint64_t L_55 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_55, ((int64_t)8)));
	}

IL_0110:
	{
		uint64_t L_56 = V_6;
		uint64_t L_57 = V_3;
		if ((!(((uint64_t)L_56) >= ((uint64_t)((int64_t)((int64_t)L_57&((int64_t)((int32_t)-8))))))))
		{
			goto IL_0064;
		}
	}
	{
		uint64_t L_58 = V_6;
		uint64_t L_59 = V_3;
		if ((!(((uint64_t)L_58) < ((uint64_t)((int64_t)((int64_t)L_59&((int64_t)((int32_t)-4))))))))
		{
			goto IL_0198;
		}
	}
	{
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_60 = V_4;
		uint64_t L_61 = V_6;
		uint64_t L_62 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_63;
		L_63 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_60, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_61, (int64_t)L_62)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_64 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_63 = L_64;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_65 = V_4;
		uint64_t L_66 = V_6;
		uint64_t L_67 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_68;
		L_68 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_65, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_66, ((int64_t)1))), (int64_t)L_67)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_69 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_68 = L_69;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_70 = V_4;
		uint64_t L_71 = V_6;
		uint64_t L_72 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_73;
		L_73 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_70, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_71, ((int64_t)2))), (int64_t)L_72)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_74 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_73 = L_74;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_75 = V_4;
		uint64_t L_76 = V_6;
		uint64_t L_77 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_78;
		L_78 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_75, (uint64_t)((int64_t)il2cpp_codegen_multiply(((int64_t)il2cpp_codegen_add((int64_t)L_76, ((int64_t)3))), (int64_t)L_77)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_79 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_78 = L_79;
		uint64_t L_80 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_80, ((int64_t)4)));
		goto IL_0198;
	}

IL_017f:
	{
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_81 = V_4;
		uint64_t L_82 = V_6;
		uint64_t L_83 = V_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_84;
		L_84 = il2cpp_unsafe_add_byte_offset<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,uint64_t>(L_81, (uint64_t)((int64_t)il2cpp_codegen_multiply((int64_t)L_82, (int64_t)L_83)));
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 L_85 = ___0_value;
		*(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225*)L_84 = L_85;
		uint64_t L_86 = V_6;
		V_6 = (uint64_t)((int64_t)il2cpp_codegen_add((int64_t)L_86, ((int64_t)1)));
	}

IL_0198:
	{
		uint64_t L_87 = V_6;
		uint64_t L_88 = V_3;
		if ((!(((uint64_t)L_87) >= ((uint64_t)L_88))))
		{
			goto IL_017f;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Span_1_CopyTo_m4A21D34A3385E3970F0A5816F70F7333A06BE56B_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___0_destination, const RuntimeMethod* method) 
{
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0038;
		}
	}
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_2 = ___0_destination;
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_3 = L_2.____pointer;
		V_0 = L_3;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_0));
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_5 = __this->____pointer;
		V_0 = L_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_0));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_Tisjvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_m48D2E426B92CBE28782CF29BD703DF063CFF422D(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return;
	}

IL_0038:
	{
		ThrowHelper_ThrowArgumentException_DestinationTooShort_m6468934A3BBB67DBC5BAEF7A64D91BD5BBBB3D4D(NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_TryCopyTo_m99A85634C362E5EB0DA6C2D017EBB178D32307F9_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___0_destination, const RuntimeMethod* method) 
{
	bool V_0 = false;
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		V_0 = (bool)0;
		int32_t L_0 = __this->____length;
		int32_t L_1;
		L_1 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&___0_destination), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 13));
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_003b;
		}
	}
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_2 = ___0_destination;
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_3 = L_2.____pointer;
		V_1 = L_3;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_4;
		L_4 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_1));
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_5 = __this->____pointer;
		V_1 = L_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_1));
		int32_t L_7 = __this->____length;
		Buffer_Memmove_Tisjvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_m48D2E426B92CBE28782CF29BD703DF063CFF422D(L_4, L_6, (uint64_t)((int64_t)L_7), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		V_0 = (bool)1;
	}

IL_003b:
	{
		bool L_8 = V_0;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlySpan_1_tEC1E786ADECA91AF68E558734868FBB77E05D964 Span_1_op_Implicit_m17AF33F5674C02E3D7679E066E81DB5609B84BAB_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___0_span, const RuntimeMethod* method) 
{
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_0 = ___0_span;
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_1 = L_0.____pointer;
		V_0 = L_1;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_2;
		L_2 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_0));
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_3 = ___0_span;
		int32_t L_4 = L_3.____length;
		ReadOnlySpan_1_tEC1E786ADECA91AF68E558734868FBB77E05D964 L_5;
		memset((&L_5), 0, sizeof(L_5));
		ReadOnlySpan_1__ctor_m655719167577EE7B24CAAF5A9D0995E0259B6A84_inline((&L_5), L_2, L_4, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 17));
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Span_1_ToString_m981EF00CBB9E2175CE158EE6AF832867FCAB1BAE_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0DB46164953228904843938099AF66650313FEE5);
		s_Il2CppMethodInitialized = true;
	}
	Il2CppChar* V_0 = NULL;
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_2 = { reinterpret_cast<intptr_t> (Char_t521A6F19B456D956AF452D926C32709DC03D6B17_0_0_0_var) };
		Type_t* L_3;
		L_3 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_2, NULL);
		bool L_4;
		L_4 = Type_op_Equality_m99930A0E44E420A685FABA60E60BA1CC5FA0EBDC(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_003e;
		}
	}
	{
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_5 = __this->____pointer;
		V_1 = L_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_1));
		Il2CppChar* L_7;
		L_7 = il2cpp_unsafe_as_ref<Il2CppChar>(L_6);
		V_0 = L_7;
		Il2CppChar* L_8 = V_0;
		int32_t L_9 = __this->____length;
		String_t* L_10;
		L_10 = String_CreateString_m3F8794FEB452558B8A68C65E1F0B603B3D94E0E2(NULL, (Il2CppChar*)((uintptr_t)L_8), 0, L_9, NULL);
		return L_10;
	}

IL_003e:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 9)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck((MemberInfo_t*)L_12);
		String_t* L_13;
		L_13 = VirtualFuncInvoker0< String_t* >::Invoke(12, (MemberInfo_t*)L_12);
		int32_t L_14 = __this->____length;
		int32_t L_15 = L_14;
		RuntimeObject* L_16 = Box(Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral0DB46164953228904843938099AF66650313FEE5, (RuntimeObject*)L_13, L_16, NULL);
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 Span_1_Slice_m2BA372A81D74723E61E19017C77886B880B5BF29_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, int32_t ___0_start, const RuntimeMethod* method) 
{
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) > ((uint32_t)L_1))))
		{
			goto IL_000e;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_000e:
	{
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_2 = __this->____pointer;
		V_0 = L_2;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_3;
		L_3 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_0));
		int32_t L_4 = ___0_start;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_5;
		L_5 = il2cpp_unsafe_add<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,int32_t>(L_3, L_4);
		int32_t L_6 = __this->____length;
		int32_t L_7 = ___0_start;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_8;
		memset((&L_8), 0, sizeof(L_8));
		Span_1__ctor_m1DA3ED274C046791E48A52F415066C7C86B031D1_inline((&L_8), L_5, ((int32_t)il2cpp_codegen_subtract(L_6, L_7)), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 Span_1_Slice_mC2BFF8AEEE5C308DBAE92526A79182BA3523561E_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, int32_t ___0_start, int32_t ___1_length, const RuntimeMethod* method) 
{
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_start;
		int32_t L_1 = __this->____length;
		if ((!(((uint32_t)L_0) <= ((uint32_t)L_1))))
		{
			goto IL_0014;
		}
	}
	{
		int32_t L_2 = ___1_length;
		int32_t L_3 = __this->____length;
		int32_t L_4 = ___0_start;
		if ((!(((uint32_t)L_2) > ((uint32_t)((int32_t)il2cpp_codegen_subtract(L_3, L_4))))))
		{
			goto IL_0019;
		}
	}

IL_0014:
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_0019:
	{
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_5 = __this->____pointer;
		V_0 = L_5;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_6;
		L_6 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_0));
		int32_t L_7 = ___0_start;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_8;
		L_8 = il2cpp_unsafe_add<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225,int32_t>(L_6, L_7);
		int32_t L_9 = ___1_length;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_m1DA3ED274C046791E48A52F415066C7C86B031D1_inline((&L_10), L_8, L_9, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 18));
		return L_10;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* Span_1_ToArray_mB0599718A25BB507D329B31366909FD456AFCEA6_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, const RuntimeMethod* method) 
{
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = __this->____length;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_1;
		L_1 = Array_Empty_Tisjvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_mD8B08917D1DC7B424036208C74F0A7A6EC83DAAE_inline(il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 19));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____length;
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_3 = (jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F*)(jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F*)SZArrayNew(il2cpp_rgctx_data(InitializedTypeInfo(method->klass)->rgctx_data, 20), (uint32_t)L_2);
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_4 = L_3;
		NullCheck((RuntimeArray*)L_4);
		uint8_t* L_5;
		L_5 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_4, NULL);
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_6;
		L_6 = il2cpp_unsafe_as_ref<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225>(L_5);
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_7 = __this->____pointer;
		V_0 = L_7;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_8;
		L_8 = IL2CPP_BY_REFERENCE_GET_VALUE(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225, (Il2CppByReference*)(&V_0));
		int32_t L_9 = __this->____length;
		Buffer_Memmove_Tisjvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_m48D2E426B92CBE28782CF29BD703DF063CFF422D(L_6, L_8, (uint64_t)((int64_t)L_9), il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 15));
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Span_1_Equals_m9F3E0520692B8B831C6629192F893B72EA9D95D2_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, RuntimeObject* ___0_obj, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral69508A540AFD085A745316DD7D6345B1C8CC662D)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Span_1_GetHashCode_mF597BBC608D68A57C8937FFFA10D01B1617B0349_gshared (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, const RuntimeMethod* method) 
{
	{
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_0 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECE618215BAC99C6FD12D8A273CC2118945EDCC8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 Span_1_op_Implicit_m2562566D0096520B0F0B4642C72BCFA5460FFE30_gshared (jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* ___0_array, const RuntimeMethod* method) 
{
	{
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_0 = ___0_array;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_1;
		memset((&L_1), 0, sizeof(L_1));
		Span_1__ctor_m13E8571165DB8DB1A1909B44B65D4F220890AC8F_inline((&L_1), L_0, il2cpp_rgctx_method(InitializedTypeInfo(method->klass)->rgctx_data, 21));
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SparseArray_1__ctor_m835EE9E3B6A134B4BAB7901843AAF2E8FA962B8E_gshared (SparseArray_1_t8D77ECC0E534C3B1A8C7403D9BDAC38694EAF242* __this, int32_t ___0_initialSize, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		int32_t L_0 = ___0_initialSize;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 0), (uint32_t)L_0);
		il2cpp_codegen_memory_barrier();
		__this->___m_array = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_array), (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* SparseArray_1_get_Current_m0FCEEF0118EEDEB25CD8A7FB6E38D1BBC12F86AF_gshared (SparseArray_1_t8D77ECC0E534C3B1A8C7403D9BDAC38694EAF242* __this, const RuntimeMethod* method) 
{
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = __this->___m_array;
		il2cpp_codegen_memory_barrier();
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t SparseArray_1_Add_mFA82FEC4F7D90A91283709B10F5151F2A7C2ADF0_gshared (SparseArray_1_t8D77ECC0E534C3B1A8C7403D9BDAC38694EAF242* __this, RuntimeObject* ___0_e, const RuntimeMethod* method) 
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* V_0 = NULL;
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* V_1 = NULL;
	bool V_2 = false;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* V_5 = NULL;

IL_0000:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = __this->___m_array;
		il2cpp_codegen_memory_barrier();
		V_0 = L_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = V_0;
		V_1 = L_1;
		V_2 = (bool)0;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_008e:
			{
				{
					bool L_2 = V_2;
					if (!L_2)
					{
						goto IL_0097;
					}
				}
				{
					ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = V_1;
					Monitor_Exit_m05B2CF037E2214B3208198C282490A2A475653FA((RuntimeObject*)L_3, NULL);
				}

IL_0097:
				{
					return;
				}
			}
		});
		try
		{
			{
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = V_1;
				Monitor_Enter_m3CDB589DA1300B513D55FDCFB52B63E879794149((RuntimeObject*)L_4, (&V_2), NULL);
				V_3 = 0;
				goto IL_0083_1;
			}

IL_0019_1:
			{
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_5 = V_0;
				int32_t L_6 = V_3;
				NullCheck(L_5);
				int32_t L_7 = L_6;
				RuntimeObject* L_8 = (L_5)->GetAt(static_cast<il2cpp_array_size_t>(L_7));
				if (L_8)
				{
					goto IL_0039_1;
				}
			}
			{
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_9 = V_0;
				int32_t L_10 = V_3;
				NullCheck(L_9);
				RuntimeObject* L_11 = ___0_e;
				VolatileWrite((RuntimeObject**)((L_9)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_10))), (RuntimeObject*)L_11);
				int32_t L_12 = V_3;
				V_4 = L_12;
				goto IL_0098;
			}

IL_0039_1:
			{
				int32_t L_13 = V_3;
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = V_0;
				NullCheck(L_14);
				if ((!(((uint32_t)L_13) == ((uint32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_14)->max_length)), 1))))))
				{
					goto IL_007f_1;
				}
			}
			{
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_15 = V_0;
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_16 = __this->___m_array;
				il2cpp_codegen_memory_barrier();
				if ((!(((RuntimeObject*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)L_15) == ((RuntimeObject*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)L_16))))
				{
					goto IL_007f_1;
				}
			}
			{
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_17 = V_0;
				NullCheck(L_17);
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_18 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 0), (uint32_t)((int32_t)il2cpp_codegen_multiply(((int32_t)(((RuntimeArray*)L_17)->max_length)), 2)));
				V_5 = L_18;
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_19 = V_0;
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_20 = V_5;
				int32_t L_21 = V_3;
				Array_Copy_m4233828B4E6288B6D815F539AAA38575DE627900((RuntimeArray*)L_19, (RuntimeArray*)L_20, ((int32_t)il2cpp_codegen_add(L_21, 1)), NULL);
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_22 = V_5;
				int32_t L_23 = V_3;
				RuntimeObject* L_24 = ___0_e;
				NullCheck(L_22);
				(L_22)->SetAt(static_cast<il2cpp_array_size_t>(((int32_t)il2cpp_codegen_add(L_23, 1))), (RuntimeObject*)L_24);
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_25 = V_5;
				il2cpp_codegen_memory_barrier();
				__this->___m_array = L_25;
				Il2CppCodeGenWriteBarrier((void**)(&__this->___m_array), (void*)L_25);
				int32_t L_26 = V_3;
				V_4 = ((int32_t)il2cpp_codegen_add(L_26, 1));
				goto IL_0098;
			}

IL_007f_1:
			{
				int32_t L_27 = V_3;
				V_3 = ((int32_t)il2cpp_codegen_add(L_27, 1));
			}

IL_0083_1:
			{
				int32_t L_28 = V_3;
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_29 = V_0;
				NullCheck(L_29);
				if ((((int32_t)L_28) < ((int32_t)((int32_t)(((RuntimeArray*)L_29)->max_length)))))
				{
					goto IL_0019_1;
				}
			}
			{
				goto IL_0000;
			}
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0098:
	{
		int32_t L_30 = V_4;
		return L_30;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SparseArray_1_Remove_mC48EB2673EB8C6ABDA639D24E83B75A2F4189EB9_gshared (SparseArray_1_t8D77ECC0E534C3B1A8C7403D9BDAC38694EAF242* __this, RuntimeObject* ___0_e, const RuntimeMethod* method) 
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* V_0 = NULL;
	bool V_1 = false;
	int32_t V_2 = 0;
	RuntimeObject* V_3 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = __this->___m_array;
		il2cpp_codegen_memory_barrier();
		V_0 = L_0;
		V_1 = (bool)0;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0063:
			{
				{
					bool L_1 = V_1;
					if (!L_1)
					{
						goto IL_006c;
					}
				}
				{
					ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = V_0;
					Monitor_Exit_m05B2CF037E2214B3208198C282490A2A475653FA((RuntimeObject*)L_2, NULL);
				}

IL_006c:
				{
					return;
				}
			}
		});
		try
		{
			{
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = V_0;
				Monitor_Enter_m3CDB589DA1300B513D55FDCFB52B63E879794149((RuntimeObject*)L_3, (&V_1), NULL);
				V_2 = 0;
				goto IL_0054_1;
			}

IL_0017_1:
			{
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = __this->___m_array;
				il2cpp_codegen_memory_barrier();
				int32_t L_5 = V_2;
				NullCheck(L_4);
				int32_t L_6 = L_5;
				RuntimeObject* L_7 = (L_4)->GetAt(static_cast<il2cpp_array_size_t>(L_6));
				RuntimeObject* L_8 = ___0_e;
				if ((!(((RuntimeObject*)(RuntimeObject*)L_7) == ((RuntimeObject*)(RuntimeObject*)L_8))))
				{
					goto IL_0050_1;
				}
			}
			{
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_9 = __this->___m_array;
				il2cpp_codegen_memory_barrier();
				int32_t L_10 = V_2;
				NullCheck(L_9);
				il2cpp_codegen_initobj((&V_3), sizeof(RuntimeObject*));
				RuntimeObject* L_11 = V_3;
				VolatileWrite((RuntimeObject**)((L_9)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_10))), (RuntimeObject*)L_11);
				goto IL_006d;
			}

IL_0050_1:
			{
				int32_t L_12 = V_2;
				V_2 = ((int32_t)il2cpp_codegen_add(L_12, 1));
			}

IL_0054_1:
			{
				int32_t L_13 = V_2;
				ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = __this->___m_array;
				il2cpp_codegen_memory_barrier();
				NullCheck(L_14);
				if ((((int32_t)L_13) < ((int32_t)((int32_t)(((RuntimeArray*)L_14)->max_length)))))
				{
					goto IL_0017_1;
				}
			}
			{
				goto IL_006d;
			}
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_006d:
	{
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SparselyPopulatedArrayAddInfo_1__ctor_m323E378EC0EE0C48A24AA0E14E40D7B020BB0458_gshared (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* __this, SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* ___0_source, int32_t ___1_index, const RuntimeMethod* method) 
{
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_0 = ___0_source;
		__this->____source = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____source), (void*)L_0);
		int32_t L_1 = ___1_index;
		__this->____index = L_1;
		return;
	}
}
IL2CPP_EXTERN_C  void SparselyPopulatedArrayAddInfo_1__ctor_m323E378EC0EE0C48A24AA0E14E40D7B020BB0458_AdjustorThunk (RuntimeObject* __this, SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* ___0_source, int32_t ___1_index, const RuntimeMethod* method)
{
	SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D*>(__this + _offset);
	SparselyPopulatedArrayAddInfo_1__ctor_m323E378EC0EE0C48A24AA0E14E40D7B020BB0458(_thisAdjusted, ___0_source, ___1_index, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* SparselyPopulatedArrayAddInfo_1_get_Source_mBA043CE79666AA955D0FE4335ED3B82802E75C86_gshared (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* __this, const RuntimeMethod* method) 
{
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_0 = __this->____source;
		return L_0;
	}
}
IL2CPP_EXTERN_C  SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* SparselyPopulatedArrayAddInfo_1_get_Source_mBA043CE79666AA955D0FE4335ED3B82802E75C86_AdjustorThunk (RuntimeObject* __this, const RuntimeMethod* method)
{
	SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D*>(__this + _offset);
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* _returnValue;
	_returnValue = SparselyPopulatedArrayAddInfo_1_get_Source_mBA043CE79666AA955D0FE4335ED3B82802E75C86_inline(_thisAdjusted, method);
	return _returnValue;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t SparselyPopulatedArrayAddInfo_1_get_Index_mCBBF3F010A994612AC94DA417AB8D04A2C92B45A_gshared (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____index;
		return L_0;
	}
}
IL2CPP_EXTERN_C  int32_t SparselyPopulatedArrayAddInfo_1_get_Index_mCBBF3F010A994612AC94DA417AB8D04A2C92B45A_AdjustorThunk (RuntimeObject* __this, const RuntimeMethod* method)
{
	SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D*>(__this + _offset);
	int32_t _returnValue;
	_returnValue = SparselyPopulatedArrayAddInfo_1_get_Index_mCBBF3F010A994612AC94DA417AB8D04A2C92B45A_inline(_thisAdjusted, method);
	return _returnValue;
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SparselyPopulatedArrayFragment_1__ctor_m1BAEA22A2C2FD0C50376CEBFC7F3A024EE3C302E_gshared (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* __this, int32_t ___0_size, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_size;
		SparselyPopulatedArrayFragment_1__ctor_m849B5F4632EC0E042F4CF4F23102F9D74CE14294(__this, L_0, (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC*)NULL, il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SparselyPopulatedArrayFragment_1__ctor_m849B5F4632EC0E042F4CF4F23102F9D74CE14294_gshared (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* __this, int32_t ___0_size, SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* ___1_prev, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		int32_t L_0 = ___0_size;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 2), (uint32_t)L_0);
		__this->____elements = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____elements), (void*)L_1);
		int32_t L_2 = ___0_size;
		il2cpp_codegen_memory_barrier();
		__this->____freeCount = L_2;
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_3 = ___1_prev;
		il2cpp_codegen_memory_barrier();
		__this->____prev = L_3;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____prev), (void*)L_3);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* SparselyPopulatedArrayFragment_1_get_Item_mEF1B53A93D46F6F69F18517CF5472F67BCE45C38_gshared (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* __this, int32_t ___0_index, const RuntimeMethod* method) 
{
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = __this->____elements;
		int32_t L_1 = ___0_index;
		NullCheck(L_0);
		RuntimeObject* L_2;
		L_2 = VolatileRead(((L_0)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_1))));
		return L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t SparselyPopulatedArrayFragment_1_get_Length_m2F77B48EDD934ED6586E559645752EB229677D27_gshared (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* __this, const RuntimeMethod* method) 
{
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = __this->____elements;
		NullCheck(L_0);
		return ((int32_t)(((RuntimeArray*)L_0)->max_length));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* SparselyPopulatedArrayFragment_1_get_Prev_m549822EBEA5C7E59A7EAC7B83D621B72E822A8FA_gshared (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* __this, const RuntimeMethod* method) 
{
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_0 = __this->____prev;
		il2cpp_codegen_memory_barrier();
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* SparselyPopulatedArrayFragment_1_SafeAtomicRemove_m41CC9DA2BF22A6BD80CE1F09B5F56031C6EE67FC_gshared (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* __this, int32_t ___0_index, RuntimeObject* ___1_expectedElement, const RuntimeMethod* method) 
{
	RuntimeObject* V_0 = NULL;
	RuntimeObject* G_B2_0 = NULL;
	RuntimeObject* G_B1_0 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = __this->____elements;
		int32_t L_1 = ___0_index;
		NullCheck(L_0);
		il2cpp_codegen_initobj((&V_0), sizeof(RuntimeObject*));
		RuntimeObject* L_2 = V_0;
		RuntimeObject* L_3 = ___1_expectedElement;
		RuntimeObject* L_4;
		L_4 = InterlockedCompareExchangeImpl<RuntimeObject*>(((L_0)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_1))), L_2, L_3);
		RuntimeObject* L_5 = L_4;
		if (!L_5)
		{
			G_B2_0 = L_5;
			goto IL_0035;
		}
		G_B1_0 = L_5;
	}
	{
		int32_t L_6 = __this->____freeCount;
		il2cpp_codegen_memory_barrier();
		il2cpp_codegen_memory_barrier();
		__this->____freeCount = ((int32_t)il2cpp_codegen_add(L_6, 1));
		G_B2_0 = G_B1_0;
	}

IL_0035:
	{
		return G_B2_0;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SparselyPopulatedArray_1__ctor_m13D75BA18ED19BF0AE6E4AB201C66718D56D0643_gshared (SparselyPopulatedArray_1_t127D3C1977D038D143CA5A6CDD74A71117B5A614* __this, int32_t ___0_initialSize, const RuntimeMethod* method) 
{
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* V_0 = NULL;
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		int32_t L_0 = ___0_initialSize;
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_1 = (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC*)il2cpp_codegen_object_new(il2cpp_rgctx_data(method->klass->rgctx_data, 0));
		SparselyPopulatedArrayFragment_1__ctor_m1BAEA22A2C2FD0C50376CEBFC7F3A024EE3C302E(L_1, L_0, il2cpp_rgctx_method(method->klass->rgctx_data, 1));
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_2 = L_1;
		V_0 = L_2;
		il2cpp_codegen_memory_barrier();
		__this->____tail = L_2;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____tail), (void*)L_2);
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_3 = V_0;
		__this->____head = L_3;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____head), (void*)L_3);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* SparselyPopulatedArray_1_get_Tail_m28175FF31A3507C7DDE945D58B2ADEA01D07B64C_gshared (SparselyPopulatedArray_1_t127D3C1977D038D143CA5A6CDD74A71117B5A614* __this, const RuntimeMethod* method) 
{
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_0 = __this->____tail;
		il2cpp_codegen_memory_barrier();
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D SparselyPopulatedArray_1_Add_mE99CA2479CB966676495F0908849E6ADE8C79CDB_gshared (SparselyPopulatedArray_1_t127D3C1977D038D143CA5A6CDD74A71117B5A614* __this, RuntimeObject* ___0_element, const RuntimeMethod* method) 
{
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* V_0 = NULL;
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* V_1 = NULL;
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* V_2 = NULL;
	int32_t V_3 = 0;
	int32_t V_4 = 0;
	int32_t V_5 = 0;
	int32_t V_6 = 0;
	RuntimeObject* V_7 = NULL;
	int32_t V_8 = 0;
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* G_B15_0 = NULL;
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* G_B14_0 = NULL;
	int32_t G_B16_0 = 0;
	SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* G_B16_1 = NULL;
	int32_t G_B24_0 = 0;

IL_0000:
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_0 = __this->____tail;
		il2cpp_codegen_memory_barrier();
		V_0 = L_0;
		goto IL_001d;
	}

IL_000b:
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_1 = V_0;
		NullCheck(L_1);
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_2 = L_1->____next;
		il2cpp_codegen_memory_barrier();
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_3 = L_2;
		V_0 = L_3;
		il2cpp_codegen_memory_barrier();
		__this->____tail = L_3;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____tail), (void*)L_3);
	}

IL_001d:
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_4 = V_0;
		NullCheck(L_4);
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_5 = L_4->____next;
		il2cpp_codegen_memory_barrier();
		if (L_5)
		{
			goto IL_000b;
		}
	}
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_6 = V_0;
		V_1 = L_6;
		goto IL_0115;
	}

IL_002e:
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_7 = V_1;
		NullCheck(L_7);
		int32_t L_8 = L_7->____freeCount;
		il2cpp_codegen_memory_barrier();
		if ((((int32_t)L_8) >= ((int32_t)1)))
		{
			goto IL_004b;
		}
	}
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_9 = V_1;
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_10 = L_9;
		NullCheck(L_10);
		int32_t L_11 = L_10->____freeCount;
		il2cpp_codegen_memory_barrier();
		NullCheck(L_10);
		il2cpp_codegen_memory_barrier();
		L_10->____freeCount = ((int32_t)il2cpp_codegen_subtract(L_11, 1));
	}

IL_004b:
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_12 = V_1;
		NullCheck(L_12);
		int32_t L_13 = L_12->____freeCount;
		il2cpp_codegen_memory_barrier();
		if ((((int32_t)L_13) > ((int32_t)0)))
		{
			goto IL_0065;
		}
	}
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_14 = V_1;
		NullCheck(L_14);
		int32_t L_15 = L_14->____freeCount;
		il2cpp_codegen_memory_barrier();
		if ((((int32_t)L_15) >= ((int32_t)((int32_t)-10))))
		{
			goto IL_010c;
		}
	}

IL_0065:
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_16 = V_1;
		NullCheck(L_16);
		int32_t L_17;
		L_17 = SparselyPopulatedArrayFragment_1_get_Length_m2F77B48EDD934ED6586E559645752EB229677D27(L_16, il2cpp_rgctx_method(method->klass->rgctx_data, 4));
		V_3 = L_17;
		int32_t L_18 = V_3;
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_19 = V_1;
		NullCheck(L_19);
		int32_t L_20 = L_19->____freeCount;
		il2cpp_codegen_memory_barrier();
		int32_t L_21 = V_3;
		V_4 = ((int32_t)(((int32_t)il2cpp_codegen_subtract(L_18, L_20))%L_21));
		int32_t L_22 = V_4;
		if ((((int32_t)L_22) >= ((int32_t)0)))
		{
			goto IL_0094;
		}
	}
	{
		V_4 = 0;
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_23 = V_1;
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_24 = L_23;
		NullCheck(L_24);
		int32_t L_25 = L_24->____freeCount;
		il2cpp_codegen_memory_barrier();
		NullCheck(L_24);
		il2cpp_codegen_memory_barrier();
		L_24->____freeCount = ((int32_t)il2cpp_codegen_subtract(L_25, 1));
	}

IL_0094:
	{
		V_5 = 0;
		goto IL_0107;
	}

IL_0099:
	{
		int32_t L_26 = V_4;
		int32_t L_27 = V_5;
		int32_t L_28 = V_3;
		V_6 = ((int32_t)(((int32_t)il2cpp_codegen_add(L_26, L_27))%L_28));
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_29 = V_1;
		NullCheck(L_29);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_30 = L_29->____elements;
		int32_t L_31 = V_6;
		NullCheck(L_30);
		int32_t L_32 = L_31;
		RuntimeObject* L_33 = (L_30)->GetAt(static_cast<il2cpp_array_size_t>(L_32));
		if (L_33)
		{
			goto IL_0101;
		}
	}
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_34 = V_1;
		NullCheck(L_34);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_35 = L_34->____elements;
		int32_t L_36 = V_6;
		NullCheck(L_35);
		RuntimeObject* L_37 = ___0_element;
		il2cpp_codegen_initobj((&V_7), sizeof(RuntimeObject*));
		RuntimeObject* L_38 = V_7;
		RuntimeObject* L_39;
		L_39 = InterlockedCompareExchangeImpl<RuntimeObject*>(((L_35)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_36))), L_37, L_38);
		if (L_39)
		{
			goto IL_0101;
		}
	}
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_40 = V_1;
		NullCheck(L_40);
		int32_t L_41 = L_40->____freeCount;
		il2cpp_codegen_memory_barrier();
		V_8 = ((int32_t)il2cpp_codegen_subtract(L_41, 1));
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_42 = V_1;
		int32_t L_43 = V_8;
		if ((((int32_t)L_43) > ((int32_t)0)))
		{
			G_B15_0 = L_42;
			goto IL_00ef;
		}
		G_B14_0 = L_42;
	}
	{
		G_B16_0 = 0;
		G_B16_1 = G_B14_0;
		goto IL_00f1;
	}

IL_00ef:
	{
		int32_t L_44 = V_8;
		G_B16_0 = L_44;
		G_B16_1 = G_B15_0;
	}

IL_00f1:
	{
		NullCheck(G_B16_1);
		il2cpp_codegen_memory_barrier();
		G_B16_1->____freeCount = G_B16_0;
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_45 = V_1;
		int32_t L_46 = V_6;
		SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D L_47;
		memset((&L_47), 0, sizeof(L_47));
		SparselyPopulatedArrayAddInfo_1__ctor_m323E378EC0EE0C48A24AA0E14E40D7B020BB0458((&L_47), L_45, L_46, il2cpp_rgctx_method(method->klass->rgctx_data, 9));
		return L_47;
	}

IL_0101:
	{
		int32_t L_48 = V_5;
		V_5 = ((int32_t)il2cpp_codegen_add(L_48, 1));
	}

IL_0107:
	{
		int32_t L_49 = V_5;
		int32_t L_50 = V_3;
		if ((((int32_t)L_49) < ((int32_t)L_50)))
		{
			goto IL_0099;
		}
	}

IL_010c:
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_51 = V_1;
		NullCheck(L_51);
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_52 = L_51->____prev;
		il2cpp_codegen_memory_barrier();
		V_1 = L_52;
	}

IL_0115:
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_53 = V_1;
		if (L_53)
		{
			goto IL_002e;
		}
	}
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_54 = V_0;
		NullCheck(L_54);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_55 = L_54->____elements;
		NullCheck(L_55);
		if ((((int32_t)((int32_t)(((RuntimeArray*)L_55)->max_length))) == ((int32_t)((int32_t)4096))))
		{
			goto IL_0136;
		}
	}
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_56 = V_0;
		NullCheck(L_56);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_57 = L_56->____elements;
		NullCheck(L_57);
		G_B24_0 = ((int32_t)il2cpp_codegen_multiply(((int32_t)(((RuntimeArray*)L_57)->max_length)), 2));
		goto IL_013b;
	}

IL_0136:
	{
		G_B24_0 = ((int32_t)4096);
	}

IL_013b:
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_58 = V_0;
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_59 = (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC*)il2cpp_codegen_object_new(il2cpp_rgctx_data(method->klass->rgctx_data, 0));
		SparselyPopulatedArrayFragment_1__ctor_m849B5F4632EC0E042F4CF4F23102F9D74CE14294(L_59, G_B24_0, L_58, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		V_2 = L_59;
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_60 = V_0;
		NullCheck(L_60);
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC** L_61 = (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC**)(&L_60->____next);
		il2cpp_codegen_memory_barrier();
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_62 = V_2;
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_63;
		L_63 = InterlockedCompareExchangeImpl<SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC*>(L_61, L_62, (SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC*)NULL);
		if (L_63)
		{
			goto IL_0000;
		}
	}
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_64 = V_2;
		il2cpp_codegen_memory_barrier();
		__this->____tail = L_64;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____tail), (void*)L_64);
		goto IL_0000;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Nullable_1_tCF32C56A2641879C053C86F273C0C6EC1B40BC28 StackFormatter_1_GetCount_mFF15B148AA3F20088CBD36B15FD82DA0D162B3C6_gshared (StackFormatter_1_tDFD80D25A4FEF1FD6AF71727AE2DB15A591D35C4* __this, Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* ___0_sequence, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Nullable_1__ctor_m141FA88563AC0B5179132FB929EABD02C47FF703_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* L_0 = ___0_sequence;
		NullCheck(L_0);
		int32_t L_1;
		L_1 = ((  int32_t (*) (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 1)))(L_0, il2cpp_rgctx_method(method->klass->rgctx_data, 1));
		Nullable_1_tCF32C56A2641879C053C86F273C0C6EC1B40BC28 L_2;
		memset((&L_2), 0, sizeof(L_2));
		Nullable_1__ctor_m141FA88563AC0B5179132FB929EABD02C47FF703((&L_2), L_1, Nullable_1__ctor_m141FA88563AC0B5179132FB929EABD02C47FF703_RuntimeMethod_var);
		return L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StackFormatter_1_Add_m3AACA783DB0DBC9E22ECA1426C742E5399719774_gshared (StackFormatter_1_tDFD80D25A4FEF1FD6AF71727AE2DB15A591D35C4* __this, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_collection, int32_t ___1_index, Il2CppFullySharedGenericAny ___2_value, MessagePackSerializerOptions_t6356E2266D4FA5A0334004939D1C37D105B23356* ___3_options, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_T_t7B6D4141203F2B90D3827CC76CA15ED4D0088A95 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 3));
	const Il2CppFullySharedGenericAny L_3 = alloca(SizeOf_T_t7B6D4141203F2B90D3827CC76CA15ED4D0088A95);
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0 = ___0_collection;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_1 = ___0_collection;
		NullCheck(L_1);
		if (((int64_t)((int32_t)(((RuntimeArray*)L_1)->max_length)) - (int64_t)1 < (int64_t)kIl2CppInt32Min) || ((int64_t)((int32_t)(((RuntimeArray*)L_1)->max_length)) - (int64_t)1 > (int64_t)kIl2CppInt32Max))
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		int32_t L_2 = ___1_index;
		if (((int64_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_1)->max_length)), 1)) - (int64_t)L_2 < (int64_t)kIl2CppInt32Min) || ((int64_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_1)->max_length)), 1)) - (int64_t)L_2 > (int64_t)kIl2CppInt32Max))
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		il2cpp_codegen_memcpy(L_3, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 3)) ? ___2_value : &___2_value), SizeOf_T_t7B6D4141203F2B90D3827CC76CA15ED4D0088A95);
		NullCheck(L_0);
		il2cpp_codegen_memcpy((L_0)->GetAddressAt(static_cast<il2cpp_array_size_t>(((int32_t)il2cpp_codegen_subtract(((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_1)->max_length)), 1)), L_2)))), L_3, SizeOf_T_t7B6D4141203F2B90D3827CC76CA15ED4D0088A95);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (void**)(L_0)->GetAddressAt(static_cast<il2cpp_array_size_t>(((int32_t)il2cpp_codegen_subtract(((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_1)->max_length)), 1)), L_2)))), (void*)L_3);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* StackFormatter_1_Create_m77BAC45BAEADA28E2ADD7C36EE89B84EB00AE32A_gshared (StackFormatter_1_tDFD80D25A4FEF1FD6AF71727AE2DB15A591D35C4* __this, int32_t ___0_count, MessagePackSerializerOptions_t6356E2266D4FA5A0334004939D1C37D105B23356* ___1_options, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_count;
		if (!L_0)
		{
			goto IL_000a;
		}
	}
	{
		int32_t L_1 = ___0_count;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_2 = (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*)(__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 4), (uint32_t)L_1);
		return L_2;
	}

IL_000a:
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3;
		L_3 = ((  __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 5)))(il2cpp_rgctx_method(method->klass->rgctx_data, 5));
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StackFormatter_1_GetSourceEnumerator_mBEDD7D244B508E20763FEB466254332959A5D994_gshared (StackFormatter_1_tDFD80D25A4FEF1FD6AF71727AE2DB15A591D35C4* __this, Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* ___0_source, Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41* il2cppRetVal, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_Enumerator_t04E8B278BAC906D0749519D1BE347B4B3C932020 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7));
	const Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41 L_1 = alloca(SizeOf_Enumerator_t04E8B278BAC906D0749519D1BE347B4B3C932020);
	{
		Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* L_0 = ___0_source;
		NullCheck(L_0);
		InvokerActionInvoker1< Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 6)), il2cpp_rgctx_method(method->klass->rgctx_data, 6), L_0, (Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41*)L_1);
		il2cpp_codegen_memcpy(il2cppRetVal, L_1, SizeOf_Enumerator_t04E8B278BAC906D0749519D1BE347B4B3C932020);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* StackFormatter_1_Complete_mCE3753BC2B26618BF4CACB3B0B4D28F182013E31_gshared (StackFormatter_1_tDFD80D25A4FEF1FD6AF71727AE2DB15A591D35C4* __this, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_intermediateCollection, const RuntimeMethod* method) 
{
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0 = ___0_intermediateCollection;
		Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* L_1 = (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A*)il2cpp_codegen_object_new(il2cpp_rgctx_data(method->klass->rgctx_data, 0));
		((  void (*) (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A*, RuntimeObject*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 8)))(L_1, (RuntimeObject*)L_0, il2cpp_rgctx_method(method->klass->rgctx_data, 8));
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StackFormatter_1__ctor_m7A802AD30430B90C286FAEB2A611E4762B6FC685_gshared (StackFormatter_1_tDFD80D25A4FEF1FD6AF71727AE2DB15A591D35C4* __this, const RuntimeMethod* method) 
{
	{
		((  void (*) (CollectionFormatterBase_4_t64CAD9344C5BA7B735D0B4FB4BD405FA6159AB78*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 10)))((CollectionFormatterBase_4_t64CAD9344C5BA7B735D0B4FB4BD405FA6159AB78*)__this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StackItem__ctor_mD824B8B1E5C80942625942B46A707730C703FFCF_gshared (StackItem_t653DA96524E2146197E46E4AB44EB360B1531DEB* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m055611BBF5D7F2430087C6EABCB9BC26D63A5A86_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_0;
		L_0 = Array_Empty_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_m82361CB24EBE3B6C36CB18376DF3A1C7F6EE7228_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		__this->____array = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m46DD76975EFBD5B93BC11E70D6913FD309B1B20F_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, int32_t ___0_capacity, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		int32_t L_0 = ___0_capacity;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_0020;
		}
	}
	{
		int32_t L_1 = ___0_capacity;
		int32_t L_2 = L_1;
		RuntimeObject* L_3 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_2);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_4 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC37D78082ACFC8DEE7B32D9351C6E433A074FEC7)), L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral38E3DBC7FC353425EF3A98DC8DAC6689AF5FD1BE)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0020:
	{
		int32_t L_5 = ___0_capacity;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_6 = (RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186*)(RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_5);
		__this->____array = L_6;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_6);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mE5A454B43CEB6E63BBB24990F54465957AAEA48A_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, RuntimeObject* ___0_collection, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		RuntimeObject* L_0 = ___0_collection;
		if (L_0)
		{
			goto IL_0014;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral469F05BE9BB4C7903C353D0EB9F6384C84A48B25)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_0014:
	{
		RuntimeObject* L_2 = ___0_collection;
		int32_t* L_3 = (int32_t*)(&__this->____size);
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_4;
		L_4 = EnumerableHelpers_ToArray_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_mE9205A140F2D330F1BD67CA68ECE924B2D0B0DA3(L_2, L_3, il2cpp_rgctx_method(method->klass->rgctx_data, 5));
		__this->____array = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_4);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_get_Count_mEA253BA6F030F96B99200FB32493FAC08D780726_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_System_Collections_ICollection_get_IsSynchronized_m562B1222BD6EDFB866A4F0AB1466C180DA1BA13D_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_ICollection_get_SyncRoot_mEF51EC741AC77308E3ECA0722FFEB6820EB68B51_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = __this->____syncRoot;
		if (L_0)
		{
			goto IL_001a;
		}
	}
	{
		RuntimeObject** L_1 = (RuntimeObject**)(&__this->____syncRoot);
		RuntimeObject* L_2 = (RuntimeObject*)il2cpp_codegen_object_new(RuntimeObject_il2cpp_TypeInfo_var);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(L_2, NULL);
		RuntimeObject* L_3;
		L_3 = InterlockedCompareExchangeImpl<RuntimeObject*>(L_1, L_2, NULL);
	}

IL_001a:
	{
		RuntimeObject* L_4 = __this->____syncRoot;
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Clear_mD9954C05CDB5BB25C8616B6D18AC6BEDF2EBE553_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	{
	}
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_0 = __this->____array;
		int32_t L_1 = __this->____size;
		Array_Clear_m50BAA3751899858B097D3FF2ED31F284703FE5CB((RuntimeArray*)L_0, 0, L_1, NULL);
	}

IL_0019:
	{
		__this->____size = 0;
		int32_t L_2 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_2, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_Contains_m9F747D7B5E7C65971D54D98280213A9FB831F1FB_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 ___0_item, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		if (!L_0)
		{
			goto IL_0023;
		}
	}
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_1 = __this->____array;
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 L_2 = ___0_item;
		int32_t L_3 = __this->____size;
		int32_t L_4;
		L_4 = Array_LastIndexOf_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_m101739DBA13BD2E5477EAF85123BA08D6DD13500(L_1, L_2, ((int32_t)il2cpp_codegen_subtract(L_3, 1)), il2cpp_rgctx_method(method->klass->rgctx_data, 8));
		return (bool)((((int32_t)((((int32_t)L_4) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
	}

IL_0023:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_CopyTo_m6F08B1032876E2B9CFA4AADC5BBC8AFF977C080A_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		int32_t L_2 = ___1_arrayIndex;
		if ((((int32_t)L_2) < ((int32_t)0)))
		{
			goto IL_0018;
		}
	}
	{
		int32_t L_3 = ___1_arrayIndex;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_4 = ___0_array;
		NullCheck(L_4);
		if ((((int32_t)L_3) <= ((int32_t)((int32_t)(((RuntimeArray*)L_4)->max_length)))))
		{
			goto IL_002e;
		}
	}

IL_0018:
	{
		int32_t L_5 = ___1_arrayIndex;
		int32_t L_6 = L_5;
		RuntimeObject* L_7 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_6);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_8 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_8, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_8, method);
	}

IL_002e:
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_9 = ___0_array;
		NullCheck(L_9);
		int32_t L_10 = ___1_arrayIndex;
		int32_t L_11 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_9)->max_length)), L_10))) >= ((int32_t)L_11)))
		{
			goto IL_0046;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_12 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_12, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_12, method);
	}

IL_0046:
	{
		V_0 = 0;
		int32_t L_13 = ___1_arrayIndex;
		int32_t L_14 = __this->____size;
		V_1 = ((int32_t)il2cpp_codegen_add(L_13, L_14));
		goto IL_006e;
	}

IL_0053:
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_15 = ___0_array;
		int32_t L_16 = V_1;
		int32_t L_17 = ((int32_t)il2cpp_codegen_subtract(L_16, 1));
		V_1 = L_17;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_18 = __this->____array;
		int32_t L_19 = V_0;
		int32_t L_20 = L_19;
		V_0 = ((int32_t)il2cpp_codegen_add(L_20, 1));
		NullCheck(L_18);
		int32_t L_21 = L_20;
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 L_22 = (L_18)->GetAt(static_cast<il2cpp_array_size_t>(L_21));
		NullCheck(L_15);
		(L_15)->SetAt(static_cast<il2cpp_array_size_t>(L_17), (RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654)L_22);
	}

IL_006e:
	{
		int32_t L_23 = V_0;
		int32_t L_24 = __this->____size;
		if ((((int32_t)L_23) < ((int32_t)L_24)))
		{
			goto IL_0053;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_System_Collections_ICollection_CopyTo_mD658E09FABEF2037B712739C876E8815443BBD4B_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, RuntimeArray* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		RuntimeArray* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		RuntimeArray* L_2 = ___0_array;
		NullCheck(L_2);
		int32_t L_3;
		L_3 = Array_get_Rank_m9383A200A2ECC89ECA44FE5F812ECFB874449C5F(L_2, NULL);
		if ((((int32_t)L_3) == ((int32_t)1)))
		{
			goto IL_0027;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_4 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral967D403A541A1026A83D548E5AD5CA800AD4EFB5)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0027:
	{
		RuntimeArray* L_5 = ___0_array;
		NullCheck(L_5);
		int32_t L_6;
		L_6 = Array_GetLowerBound_m4FB0601E2E8A6304A42E3FC400576DF7B0F084BC(L_5, 0, NULL);
		if (!L_6)
		{
			goto IL_0040;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_7 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral6195D7DA68D16D4985AD1A1B4FD2841A43CDDE70)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_7, method);
	}

IL_0040:
	{
		int32_t L_8 = ___1_arrayIndex;
		if ((((int32_t)L_8) < ((int32_t)0)))
		{
			goto IL_004d;
		}
	}
	{
		int32_t L_9 = ___1_arrayIndex;
		RuntimeArray* L_10 = ___0_array;
		NullCheck(L_10);
		int32_t L_11;
		L_11 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_10, NULL);
		if ((((int32_t)L_9) <= ((int32_t)L_11)))
		{
			goto IL_0063;
		}
	}

IL_004d:
	{
		int32_t L_12 = ___1_arrayIndex;
		int32_t L_13 = L_12;
		RuntimeObject* L_14 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_13);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_15 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_15, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_14, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_15, method);
	}

IL_0063:
	{
		RuntimeArray* L_16 = ___0_array;
		NullCheck(L_16);
		int32_t L_17;
		L_17 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_16, NULL);
		int32_t L_18 = ___1_arrayIndex;
		int32_t L_19 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(L_17, L_18))) >= ((int32_t)L_19)))
		{
			goto IL_007e;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_20 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_20, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_20, method);
	}

IL_007e:
	{
	}
	try
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_21 = __this->____array;
		RuntimeArray* L_22 = ___0_array;
		int32_t L_23 = ___1_arrayIndex;
		int32_t L_24 = __this->____size;
		Array_Copy_mB4904E17BD92E320613A3251C0205E0786B3BF41((RuntimeArray*)L_21, 0, L_22, L_23, L_24, NULL);
		RuntimeArray* L_25 = ___0_array;
		int32_t L_26 = ___1_arrayIndex;
		int32_t L_27 = __this->____size;
		Array_Reverse_mE788006243D34C654D7DDEF13E2D9E7B129AF8AD(L_25, L_26, L_27, NULL);
		goto IL_00b3;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_00a2;
		}
		throw e;
	}

CATCH_00a2:
	{
		ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1* L_28 = ((ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*)IL2CPP_GET_ACTIVE_EXCEPTION(ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*));;
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_29 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_29, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralBD0381A992FDF4F7DA60E5D83689FE7FF6309CB8)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_29, method);
	}

IL_00b3:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Enumerator_t6C87A4CFDE2206491F41A95637BE2F6F5222FABC Stack_1_GetEnumerator_m5375225AC86A902981364E6244F8A2C210303D71_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t6C87A4CFDE2206491F41A95637BE2F6F5222FABC L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m1CA9573AF6376E9DE5D01340BC438666D124CB13((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_Generic_IEnumerableU3CTU3E_GetEnumerator_mE76A638AE31095126D08BBC0FCF3D01A17951FCA_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t6C87A4CFDE2206491F41A95637BE2F6F5222FABC L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m1CA9573AF6376E9DE5D01340BC438666D124CB13((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t6C87A4CFDE2206491F41A95637BE2F6F5222FABC L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_IEnumerable_GetEnumerator_m5AFE9FC7C057FCF583FD3F757E5A1A32424465A3_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t6C87A4CFDE2206491F41A95637BE2F6F5222FABC L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m1CA9573AF6376E9DE5D01340BC438666D124CB13((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t6C87A4CFDE2206491F41A95637BE2F6F5222FABC L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_TrimExcess_m5862A48D57C9DFD2D28EABB2EEA92FF7C6904E30_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_0 = __this->____array;
		NullCheck(L_0);
		V_0 = il2cpp_codegen_cast_double_to_int<int32_t>(((double)il2cpp_codegen_multiply(((double)((int32_t)(((RuntimeArray*)L_0)->max_length))), (0.90000000000000002))));
		int32_t L_1 = __this->____size;
		int32_t L_2 = V_0;
		if ((((int32_t)L_1) >= ((int32_t)L_2)))
		{
			goto IL_003d;
		}
	}
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186** L_3 = (RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186**)(&__this->____array);
		int32_t L_4 = __this->____size;
		Array_Resize_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_mA42860625ED6BBDD2ACFC27AD9ABDE2680695217(L_3, L_4, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_003d:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 Stack_1_Peek_m13B3571DB4F7AD55FA9D94D0C11303D5998C8C5A_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_m4C37721B5BEFB577136A37C100C0D1898A078EEE(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_4 = V_1;
		int32_t L_5 = V_0;
		NullCheck(L_4);
		int32_t L_6 = L_5;
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 L_7 = (L_4)->GetAt(static_cast<il2cpp_array_size_t>(L_6));
		return L_7;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPeek_mED1C604DD138231820845ADFD62A084D9D653845_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654));
		return (bool)0;
	}

IL_001f:
	{
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654* L_5 = ___0_result;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		*(RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654*)L_5 = L_9;
		Il2CppCodeGenWriteBarrier((void**)&(((RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654*)L_5)->___Value), (void*)NULL);
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 Stack_1_Pop_m95D85369A808CE1F53B10DCF794455ED8CDA3469_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* V_1 = NULL;
	RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 V_2;
	memset((&V_2), 0, sizeof(V_2));
	RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 G_B4_0;
	memset((&G_B4_0), 0, sizeof(G_B4_0));
	RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 G_B3_0;
	memset((&G_B3_0), 0, sizeof(G_B3_0));
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_m4C37721B5BEFB577136A37C100C0D1898A078EEE(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		int32_t L_4 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_4, 1));
		int32_t L_5 = V_0;
		__this->____size = L_5;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		G_B3_0 = L_9;
	}
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_10 = V_1;
		int32_t L_11 = V_0;
		il2cpp_codegen_initobj((&V_2), sizeof(RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654));
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 L_12 = V_2;
		NullCheck(L_10);
		(L_10)->SetAt(static_cast<il2cpp_array_size_t>(L_11), (RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654)L_12);
		G_B4_0 = G_B3_0;
	}

IL_004f:
	{
		return G_B4_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPop_m85FF50DA12359209E56E41974DE961C3388B1BAB_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* V_1 = NULL;
	RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 V_2;
	memset((&V_2), 0, sizeof(V_2));
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654));
		return (bool)0;
	}

IL_001f:
	{
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
		int32_t L_6 = V_0;
		__this->____size = L_6;
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654* L_7 = ___0_result;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_8 = V_1;
		int32_t L_9 = V_0;
		NullCheck(L_8);
		int32_t L_10 = L_9;
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 L_11 = (L_8)->GetAt(static_cast<il2cpp_array_size_t>(L_10));
		*(RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654*)L_7 = L_11;
		Il2CppCodeGenWriteBarrier((void**)&(((RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654*)L_7)->___Value), (void*)NULL);
	}
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_12 = V_1;
		int32_t L_13 = V_0;
		il2cpp_codegen_initobj((&V_2), sizeof(RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654));
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 L_14 = V_2;
		NullCheck(L_12);
		(L_12)->SetAt(static_cast<il2cpp_array_size_t>(L_13), (RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654)L_14);
	}

IL_0058:
	{
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Push_m4126754E3719961D12646C784767E5B44E0708AD_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 ___0_item, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = L_0;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) < ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_4 = V_1;
		int32_t L_5 = V_0;
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 L_6 = ___0_item;
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654)L_6);
		int32_t L_7 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_7, 1));
		int32_t L_8 = V_0;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_8, 1));
		return;
	}

IL_0034:
	{
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 L_9 = ___0_item;
		Stack_1_PushWithResize_m5677326E0EDDCB6AD5639B14C6BE13B2A75B57CB(__this, L_9, il2cpp_rgctx_method(method->klass->rgctx_data, 16));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_m5677326E0EDDCB6AD5639B14C6BE13B2A75B57CB_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 ___0_item, const RuntimeMethod* method) 
{
	RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186** G_B2_0 = NULL;
	RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186** G_B1_0 = NULL;
	int32_t G_B3_0 = 0;
	RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186** G_B3_1 = NULL;
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186** L_0 = (RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186**)(&__this->____array);
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_1 = __this->____array;
		NullCheck(L_1);
		if (!(((RuntimeArray*)L_1)->max_length))
		{
			G_B2_0 = L_0;
			goto IL_001b;
		}
		G_B1_0 = L_0;
	}
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_2 = __this->____array;
		NullCheck(L_2);
		G_B3_0 = ((int32_t)il2cpp_codegen_multiply(2, ((int32_t)(((RuntimeArray*)L_2)->max_length))));
		G_B3_1 = G_B1_0;
		goto IL_001c;
	}

IL_001b:
	{
		G_B3_0 = 4;
		G_B3_1 = G_B2_0;
	}

IL_001c:
	{
		Array_Resize_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_mA42860625ED6BBDD2ACFC27AD9ABDE2680695217(G_B3_1, G_B3_0, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_3 = __this->____array;
		int32_t L_4 = __this->____size;
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 L_5 = ___0_item;
		NullCheck(L_3);
		(L_3)->SetAt(static_cast<il2cpp_array_size_t>(L_4), (RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654)L_5);
		int32_t L_6 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_6, 1));
		int32_t L_7 = __this->____size;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_7, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* Stack_1_ToArray_m5E7A418232192CA0C0D4EF59AEE160786C2D8A91_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = __this->____size;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_1;
		L_1 = Array_Empty_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_m82361CB24EBE3B6C36CB18376DF3A1C7F6EE7228_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____size;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_3 = (RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186*)(RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_2);
		V_0 = L_3;
		V_1 = 0;
		goto IL_003e;
	}

IL_001e:
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_4 = V_0;
		int32_t L_5 = V_1;
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_6 = __this->____array;
		int32_t L_7 = __this->____size;
		int32_t L_8 = V_1;
		NullCheck(L_6);
		int32_t L_9 = ((int32_t)il2cpp_codegen_subtract(((int32_t)il2cpp_codegen_subtract(L_7, L_8)), 1));
		RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654 L_10 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_9));
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (RefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654)L_10);
		int32_t L_11 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_11, 1));
	}

IL_003e:
	{
		int32_t L_12 = V_1;
		int32_t L_13 = __this->____size;
		if ((((int32_t)L_12) < ((int32_t)L_13)))
		{
			goto IL_001e;
		}
	}
	{
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_14 = V_0;
		return L_14;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m4C37721B5BEFB577136A37C100C0D1898A078EEE_gshared (Stack_1_tA3F4A536EC8EE3E2546DD6381768C08B6EBAFE67* __this, const RuntimeMethod* method) 
{
	{
		InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB* L_0 = (InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB_il2cpp_TypeInfo_var)));
		InvalidOperationException__ctor_mE4CB6F4712AB6D99A2358FBAE2E052B3EE976162(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral95F0EE30865D503A05F1D329BC3FED0946B65C24)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mF63AE96E8925749CDACE05B89002A389DDD748D1_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_0;
		L_0 = Array_Empty_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4D53E0E0F90F37AD5DBFD2DC75E52406F90C7ABC_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		__this->____array = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m141F1BE46354DA6AD95BD4791941D8BB59594DCA_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, int32_t ___0_capacity, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		int32_t L_0 = ___0_capacity;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_0020;
		}
	}
	{
		int32_t L_1 = ___0_capacity;
		int32_t L_2 = L_1;
		RuntimeObject* L_3 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_2);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_4 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC37D78082ACFC8DEE7B32D9351C6E433A074FEC7)), L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral38E3DBC7FC353425EF3A98DC8DAC6689AF5FD1BE)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0020:
	{
		int32_t L_5 = ___0_capacity;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_6 = (Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C*)(Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_5);
		__this->____array = L_6;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_6);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m214909CE46866ED73425C022CB87D3E96E110ED3_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, RuntimeObject* ___0_collection, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		RuntimeObject* L_0 = ___0_collection;
		if (L_0)
		{
			goto IL_0014;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral469F05BE9BB4C7903C353D0EB9F6384C84A48B25)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_0014:
	{
		RuntimeObject* L_2 = ___0_collection;
		int32_t* L_3 = (int32_t*)(&__this->____size);
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_4;
		L_4 = EnumerableHelpers_ToArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEFD5911DDBDB2B3C1BCDAEE1DDF9EA8DCD3C5A22(L_2, L_3, il2cpp_rgctx_method(method->klass->rgctx_data, 5));
		__this->____array = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_4);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_get_Count_m367DD1E571E10E5D1B0D13434E35DC7FC31FA886_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_System_Collections_ICollection_get_IsSynchronized_mE296FAAA22A3EB0AC336CF00A25F421F6B67A818_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_ICollection_get_SyncRoot_mA506A2FE234FF6181FB2AF00DB329E33CFF628B2_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = __this->____syncRoot;
		if (L_0)
		{
			goto IL_001a;
		}
	}
	{
		RuntimeObject** L_1 = (RuntimeObject**)(&__this->____syncRoot);
		RuntimeObject* L_2 = (RuntimeObject*)il2cpp_codegen_object_new(RuntimeObject_il2cpp_TypeInfo_var);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(L_2, NULL);
		RuntimeObject* L_3;
		L_3 = InterlockedCompareExchangeImpl<RuntimeObject*>(L_1, L_2, NULL);
	}

IL_001a:
	{
		RuntimeObject* L_4 = __this->____syncRoot;
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Clear_mEE1C6E0AF654AE01D41D12DAF62217D4FE3930E0_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	{
		goto IL_0019;
	}

IL_0019:
	{
		__this->____size = 0;
		int32_t L_0 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_0, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_Contains_m582B32C425FCABE2412EAFD287EE75C402ECF671_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, int32_t ___0_item, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		if (!L_0)
		{
			goto IL_0023;
		}
	}
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_1 = __this->____array;
		int32_t L_2 = ___0_item;
		int32_t L_3 = __this->____size;
		int32_t L_4;
		L_4 = Array_LastIndexOf_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m603821E13AA067379EA5D21B577261CBB0BEF8E6(L_1, L_2, ((int32_t)il2cpp_codegen_subtract(L_3, 1)), il2cpp_rgctx_method(method->klass->rgctx_data, 8));
		return (bool)((((int32_t)((((int32_t)L_4) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
	}

IL_0023:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_CopyTo_m317FABC09482F70E4432AF0D3BADA4815030F2ED_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		int32_t L_2 = ___1_arrayIndex;
		if ((((int32_t)L_2) < ((int32_t)0)))
		{
			goto IL_0018;
		}
	}
	{
		int32_t L_3 = ___1_arrayIndex;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_4 = ___0_array;
		NullCheck(L_4);
		if ((((int32_t)L_3) <= ((int32_t)((int32_t)(((RuntimeArray*)L_4)->max_length)))))
		{
			goto IL_002e;
		}
	}

IL_0018:
	{
		int32_t L_5 = ___1_arrayIndex;
		int32_t L_6 = L_5;
		RuntimeObject* L_7 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_6);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_8 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_8, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_8, method);
	}

IL_002e:
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_9 = ___0_array;
		NullCheck(L_9);
		int32_t L_10 = ___1_arrayIndex;
		int32_t L_11 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_9)->max_length)), L_10))) >= ((int32_t)L_11)))
		{
			goto IL_0046;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_12 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_12, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_12, method);
	}

IL_0046:
	{
		V_0 = 0;
		int32_t L_13 = ___1_arrayIndex;
		int32_t L_14 = __this->____size;
		V_1 = ((int32_t)il2cpp_codegen_add(L_13, L_14));
		goto IL_006e;
	}

IL_0053:
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_15 = ___0_array;
		int32_t L_16 = V_1;
		int32_t L_17 = ((int32_t)il2cpp_codegen_subtract(L_16, 1));
		V_1 = L_17;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_18 = __this->____array;
		int32_t L_19 = V_0;
		int32_t L_20 = L_19;
		V_0 = ((int32_t)il2cpp_codegen_add(L_20, 1));
		NullCheck(L_18);
		int32_t L_21 = L_20;
		int32_t L_22 = (L_18)->GetAt(static_cast<il2cpp_array_size_t>(L_21));
		NullCheck(L_15);
		(L_15)->SetAt(static_cast<il2cpp_array_size_t>(L_17), (int32_t)L_22);
	}

IL_006e:
	{
		int32_t L_23 = V_0;
		int32_t L_24 = __this->____size;
		if ((((int32_t)L_23) < ((int32_t)L_24)))
		{
			goto IL_0053;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_System_Collections_ICollection_CopyTo_m43C5D88AC2BA4A7B9657B005A68F0EE5930F395F_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, RuntimeArray* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		RuntimeArray* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		RuntimeArray* L_2 = ___0_array;
		NullCheck(L_2);
		int32_t L_3;
		L_3 = Array_get_Rank_m9383A200A2ECC89ECA44FE5F812ECFB874449C5F(L_2, NULL);
		if ((((int32_t)L_3) == ((int32_t)1)))
		{
			goto IL_0027;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_4 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral967D403A541A1026A83D548E5AD5CA800AD4EFB5)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0027:
	{
		RuntimeArray* L_5 = ___0_array;
		NullCheck(L_5);
		int32_t L_6;
		L_6 = Array_GetLowerBound_m4FB0601E2E8A6304A42E3FC400576DF7B0F084BC(L_5, 0, NULL);
		if (!L_6)
		{
			goto IL_0040;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_7 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral6195D7DA68D16D4985AD1A1B4FD2841A43CDDE70)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_7, method);
	}

IL_0040:
	{
		int32_t L_8 = ___1_arrayIndex;
		if ((((int32_t)L_8) < ((int32_t)0)))
		{
			goto IL_004d;
		}
	}
	{
		int32_t L_9 = ___1_arrayIndex;
		RuntimeArray* L_10 = ___0_array;
		NullCheck(L_10);
		int32_t L_11;
		L_11 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_10, NULL);
		if ((((int32_t)L_9) <= ((int32_t)L_11)))
		{
			goto IL_0063;
		}
	}

IL_004d:
	{
		int32_t L_12 = ___1_arrayIndex;
		int32_t L_13 = L_12;
		RuntimeObject* L_14 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_13);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_15 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_15, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_14, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_15, method);
	}

IL_0063:
	{
		RuntimeArray* L_16 = ___0_array;
		NullCheck(L_16);
		int32_t L_17;
		L_17 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_16, NULL);
		int32_t L_18 = ___1_arrayIndex;
		int32_t L_19 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(L_17, L_18))) >= ((int32_t)L_19)))
		{
			goto IL_007e;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_20 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_20, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_20, method);
	}

IL_007e:
	{
	}
	try
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_21 = __this->____array;
		RuntimeArray* L_22 = ___0_array;
		int32_t L_23 = ___1_arrayIndex;
		int32_t L_24 = __this->____size;
		Array_Copy_mB4904E17BD92E320613A3251C0205E0786B3BF41((RuntimeArray*)L_21, 0, L_22, L_23, L_24, NULL);
		RuntimeArray* L_25 = ___0_array;
		int32_t L_26 = ___1_arrayIndex;
		int32_t L_27 = __this->____size;
		Array_Reverse_mE788006243D34C654D7DDEF13E2D9E7B129AF8AD(L_25, L_26, L_27, NULL);
		goto IL_00b3;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_00a2;
		}
		throw e;
	}

CATCH_00a2:
	{
		ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1* L_28 = ((ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*)IL2CPP_GET_ACTIVE_EXCEPTION(ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*));;
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_29 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_29, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralBD0381A992FDF4F7DA60E5D83689FE7FF6309CB8)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_29, method);
	}

IL_00b3:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Enumerator_t1ED2EFBA8997D05D3B6586A9FF9D467F8D5482F0 Stack_1_GetEnumerator_mDC30F9E4DD7D12559CD9C33E6C412F630A4F9BE3_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t1ED2EFBA8997D05D3B6586A9FF9D467F8D5482F0 L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m3D5138341817152CEF519175368AA02FBB797C96((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_Generic_IEnumerableU3CTU3E_GetEnumerator_m9B28A06DBE13C361182F64834E2480DB46457B8E_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t1ED2EFBA8997D05D3B6586A9FF9D467F8D5482F0 L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m3D5138341817152CEF519175368AA02FBB797C96((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t1ED2EFBA8997D05D3B6586A9FF9D467F8D5482F0 L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_IEnumerable_GetEnumerator_mC9903FC129ABAFBAE6FDC2428B611C2CBF5E6F2D_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t1ED2EFBA8997D05D3B6586A9FF9D467F8D5482F0 L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m3D5138341817152CEF519175368AA02FBB797C96((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t1ED2EFBA8997D05D3B6586A9FF9D467F8D5482F0 L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_TrimExcess_mF95088F56DFF0C079F318253704FA92A25AF039B_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_0 = __this->____array;
		NullCheck(L_0);
		V_0 = il2cpp_codegen_cast_double_to_int<int32_t>(((double)il2cpp_codegen_multiply(((double)((int32_t)(((RuntimeArray*)L_0)->max_length))), (0.90000000000000002))));
		int32_t L_1 = __this->____size;
		int32_t L_2 = V_0;
		if ((((int32_t)L_1) >= ((int32_t)L_2)))
		{
			goto IL_003d;
		}
	}
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C** L_3 = (Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C**)(&__this->____array);
		int32_t L_4 = __this->____size;
		Array_Resize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m6BAA7BD6F22421B894347B1476C37052FAC6C916(L_3, L_4, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_003d:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_Peek_m919AA48BFC239B260BB6A0639B8E027B60CB8B66_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_mA0C247058163BBAB7D8BFE60D542FFE434A3D834(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_4 = V_1;
		int32_t L_5 = V_0;
		NullCheck(L_4);
		int32_t L_6 = L_5;
		int32_t L_7 = (L_4)->GetAt(static_cast<il2cpp_array_size_t>(L_6));
		return L_7;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPeek_m6CEE924ABCE4C7280AE4A26E80BCAC6FE74EB30E_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, int32_t* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		int32_t* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(int32_t));
		return (bool)0;
	}

IL_001f:
	{
		int32_t* L_5 = ___0_result;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		int32_t L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		*(int32_t*)L_5 = L_9;
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_Pop_m59DFD2B5EC8D9044532E0AD0BDB20DB33BA76748_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t G_B4_0 = 0;
	int32_t G_B3_0 = 0;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_mA0C247058163BBAB7D8BFE60D542FFE434A3D834(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		int32_t L_4 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_4, 1));
		int32_t L_5 = V_0;
		__this->____size = L_5;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		int32_t L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		G_B4_0 = L_9;
		goto IL_004f;
	}

IL_004f:
	{
		return G_B4_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPop_m452A31920DC54B5BF252AEA1A2397634090C1D9B_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, int32_t* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* V_1 = NULL;
	int32_t V_2 = 0;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		int32_t* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(int32_t));
		return (bool)0;
	}

IL_001f:
	{
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
		int32_t L_6 = V_0;
		__this->____size = L_6;
		int32_t* L_7 = ___0_result;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_8 = V_1;
		int32_t L_9 = V_0;
		NullCheck(L_8);
		int32_t L_10 = L_9;
		int32_t L_11 = (L_8)->GetAt(static_cast<il2cpp_array_size_t>(L_10));
		*(int32_t*)L_7 = L_11;
		goto IL_0058;
	}

IL_0058:
	{
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Push_mF7CC12CF73D9D4B66FFA2E2D264270212CAB3EDA_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, int32_t ___0_item, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = L_0;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) < ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_4 = V_1;
		int32_t L_5 = V_0;
		int32_t L_6 = ___0_item;
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (int32_t)L_6);
		int32_t L_7 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_7, 1));
		int32_t L_8 = V_0;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_8, 1));
		return;
	}

IL_0034:
	{
		int32_t L_9 = ___0_item;
		Stack_1_PushWithResize_m2E32959EAD54D6F02C9FF531A2AFDEF3EC04FEB6(__this, L_9, il2cpp_rgctx_method(method->klass->rgctx_data, 16));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_m2E32959EAD54D6F02C9FF531A2AFDEF3EC04FEB6_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, int32_t ___0_item, const RuntimeMethod* method) 
{
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C** G_B2_0 = NULL;
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C** G_B1_0 = NULL;
	int32_t G_B3_0 = 0;
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C** G_B3_1 = NULL;
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C** L_0 = (Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C**)(&__this->____array);
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_1 = __this->____array;
		NullCheck(L_1);
		if (!(((RuntimeArray*)L_1)->max_length))
		{
			G_B2_0 = L_0;
			goto IL_001b;
		}
		G_B1_0 = L_0;
	}
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_2 = __this->____array;
		NullCheck(L_2);
		G_B3_0 = ((int32_t)il2cpp_codegen_multiply(2, ((int32_t)(((RuntimeArray*)L_2)->max_length))));
		G_B3_1 = G_B1_0;
		goto IL_001c;
	}

IL_001b:
	{
		G_B3_0 = 4;
		G_B3_1 = G_B2_0;
	}

IL_001c:
	{
		Array_Resize_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m6BAA7BD6F22421B894347B1476C37052FAC6C916(G_B3_1, G_B3_0, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_3 = __this->____array;
		int32_t L_4 = __this->____size;
		int32_t L_5 = ___0_item;
		NullCheck(L_3);
		(L_3)->SetAt(static_cast<il2cpp_array_size_t>(L_4), (int32_t)L_5);
		int32_t L_6 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_6, 1));
		int32_t L_7 = __this->____size;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_7, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* Stack_1_ToArray_m66CF092BE9D2A70AA67FD5F4BC2CA8A84088B5A7_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = __this->____size;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_1;
		L_1 = Array_Empty_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4D53E0E0F90F37AD5DBFD2DC75E52406F90C7ABC_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____size;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_3 = (Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C*)(Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_2);
		V_0 = L_3;
		V_1 = 0;
		goto IL_003e;
	}

IL_001e:
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_4 = V_0;
		int32_t L_5 = V_1;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_6 = __this->____array;
		int32_t L_7 = __this->____size;
		int32_t L_8 = V_1;
		NullCheck(L_6);
		int32_t L_9 = ((int32_t)il2cpp_codegen_subtract(((int32_t)il2cpp_codegen_subtract(L_7, L_8)), 1));
		int32_t L_10 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_9));
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (int32_t)L_10);
		int32_t L_11 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_11, 1));
	}

IL_003e:
	{
		int32_t L_12 = V_1;
		int32_t L_13 = __this->____size;
		if ((((int32_t)L_12) < ((int32_t)L_13)))
		{
			goto IL_001e;
		}
	}
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_14 = V_0;
		return L_14;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_mA0C247058163BBAB7D8BFE60D542FFE434A3D834_gshared (Stack_1_t3197E0F5EA36E611B259A88751D31FC2396FE4B6* __this, const RuntimeMethod* method) 
{
	{
		InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB* L_0 = (InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB_il2cpp_TypeInfo_var)));
		InvalidOperationException__ctor_mE4CB6F4712AB6D99A2358FBAE2E052B3EE976162(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral95F0EE30865D503A05F1D329BC3FED0946B65C24)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mD05342C1BB452792EAB7B189EEA951942662B556_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_0;
		L_0 = Array_Empty_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m94E12BB613D748D2EEB9E1ABD961630D2F970385_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		__this->____array = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mABE5114368C378A79F5762E756A3D3A7BAA0E3B9_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, int32_t ___0_capacity, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		int32_t L_0 = ___0_capacity;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_0020;
		}
	}
	{
		int32_t L_1 = ___0_capacity;
		int32_t L_2 = L_1;
		RuntimeObject* L_3 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_2);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_4 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC37D78082ACFC8DEE7B32D9351C6E433A074FEC7)), L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral38E3DBC7FC353425EF3A98DC8DAC6689AF5FD1BE)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0020:
	{
		int32_t L_5 = ___0_capacity;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_6 = (Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F*)(Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_5);
		__this->____array = L_6;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_6);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mEDCF9912E7F9A3E6B93F13449C212DE667CD0D8B_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, RuntimeObject* ___0_collection, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		RuntimeObject* L_0 = ___0_collection;
		if (L_0)
		{
			goto IL_0014;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral469F05BE9BB4C7903C353D0EB9F6384C84A48B25)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_0014:
	{
		RuntimeObject* L_2 = ___0_collection;
		int32_t* L_3 = (int32_t*)(&__this->____size);
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_4;
		L_4 = EnumerableHelpers_ToArray_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_mEF42832E5D11AAAFF6F615CA9A8242D03F71539E(L_2, L_3, il2cpp_rgctx_method(method->klass->rgctx_data, 5));
		__this->____array = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_4);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_get_Count_m4792730A13F3249ABCC0E87BE58C4CEFAA2593AB_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_System_Collections_ICollection_get_IsSynchronized_m5669A05A4FD8B7CCB4CD8C85C2A6DE062991D5C3_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_ICollection_get_SyncRoot_mB3BDA03E5159B66D486FB4E1586CE35997217FD6_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = __this->____syncRoot;
		if (L_0)
		{
			goto IL_001a;
		}
	}
	{
		RuntimeObject** L_1 = (RuntimeObject**)(&__this->____syncRoot);
		RuntimeObject* L_2 = (RuntimeObject*)il2cpp_codegen_object_new(RuntimeObject_il2cpp_TypeInfo_var);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(L_2, NULL);
		RuntimeObject* L_3;
		L_3 = InterlockedCompareExchangeImpl<RuntimeObject*>(L_1, L_2, NULL);
	}

IL_001a:
	{
		RuntimeObject* L_4 = __this->____syncRoot;
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Clear_mAF063B67F8243B0B5C7A855C8A67BFCE618A77B2_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	{
		goto IL_0019;
	}

IL_0019:
	{
		__this->____size = 0;
		int32_t L_0 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_0, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_Contains_m6BBC3821516EBCC69444130978DA5FBB01D47D84_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, int32_t ___0_item, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		if (!L_0)
		{
			goto IL_0023;
		}
	}
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_1 = __this->____array;
		int32_t L_2 = ___0_item;
		int32_t L_3 = __this->____size;
		int32_t L_4;
		L_4 = Array_LastIndexOf_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m92EE93D981B3058A8384DCA90FFC1502242652F7(L_1, L_2, ((int32_t)il2cpp_codegen_subtract(L_3, 1)), il2cpp_rgctx_method(method->klass->rgctx_data, 8));
		return (bool)((((int32_t)((((int32_t)L_4) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
	}

IL_0023:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_CopyTo_m770261E7131410DFD69F2C73768B97D68F0180AA_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		int32_t L_2 = ___1_arrayIndex;
		if ((((int32_t)L_2) < ((int32_t)0)))
		{
			goto IL_0018;
		}
	}
	{
		int32_t L_3 = ___1_arrayIndex;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_4 = ___0_array;
		NullCheck(L_4);
		if ((((int32_t)L_3) <= ((int32_t)((int32_t)(((RuntimeArray*)L_4)->max_length)))))
		{
			goto IL_002e;
		}
	}

IL_0018:
	{
		int32_t L_5 = ___1_arrayIndex;
		int32_t L_6 = L_5;
		RuntimeObject* L_7 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_6);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_8 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_8, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_8, method);
	}

IL_002e:
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_9 = ___0_array;
		NullCheck(L_9);
		int32_t L_10 = ___1_arrayIndex;
		int32_t L_11 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_9)->max_length)), L_10))) >= ((int32_t)L_11)))
		{
			goto IL_0046;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_12 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_12, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_12, method);
	}

IL_0046:
	{
		V_0 = 0;
		int32_t L_13 = ___1_arrayIndex;
		int32_t L_14 = __this->____size;
		V_1 = ((int32_t)il2cpp_codegen_add(L_13, L_14));
		goto IL_006e;
	}

IL_0053:
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_15 = ___0_array;
		int32_t L_16 = V_1;
		int32_t L_17 = ((int32_t)il2cpp_codegen_subtract(L_16, 1));
		V_1 = L_17;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_18 = __this->____array;
		int32_t L_19 = V_0;
		int32_t L_20 = L_19;
		V_0 = ((int32_t)il2cpp_codegen_add(L_20, 1));
		NullCheck(L_18);
		int32_t L_21 = L_20;
		int32_t L_22 = (L_18)->GetAt(static_cast<il2cpp_array_size_t>(L_21));
		NullCheck(L_15);
		(L_15)->SetAt(static_cast<il2cpp_array_size_t>(L_17), (int32_t)L_22);
	}

IL_006e:
	{
		int32_t L_23 = V_0;
		int32_t L_24 = __this->____size;
		if ((((int32_t)L_23) < ((int32_t)L_24)))
		{
			goto IL_0053;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_System_Collections_ICollection_CopyTo_m4A3AD652EAB28362C9657E2E57B70D7AB605E075_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, RuntimeArray* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		RuntimeArray* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		RuntimeArray* L_2 = ___0_array;
		NullCheck(L_2);
		int32_t L_3;
		L_3 = Array_get_Rank_m9383A200A2ECC89ECA44FE5F812ECFB874449C5F(L_2, NULL);
		if ((((int32_t)L_3) == ((int32_t)1)))
		{
			goto IL_0027;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_4 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral967D403A541A1026A83D548E5AD5CA800AD4EFB5)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0027:
	{
		RuntimeArray* L_5 = ___0_array;
		NullCheck(L_5);
		int32_t L_6;
		L_6 = Array_GetLowerBound_m4FB0601E2E8A6304A42E3FC400576DF7B0F084BC(L_5, 0, NULL);
		if (!L_6)
		{
			goto IL_0040;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_7 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral6195D7DA68D16D4985AD1A1B4FD2841A43CDDE70)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_7, method);
	}

IL_0040:
	{
		int32_t L_8 = ___1_arrayIndex;
		if ((((int32_t)L_8) < ((int32_t)0)))
		{
			goto IL_004d;
		}
	}
	{
		int32_t L_9 = ___1_arrayIndex;
		RuntimeArray* L_10 = ___0_array;
		NullCheck(L_10);
		int32_t L_11;
		L_11 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_10, NULL);
		if ((((int32_t)L_9) <= ((int32_t)L_11)))
		{
			goto IL_0063;
		}
	}

IL_004d:
	{
		int32_t L_12 = ___1_arrayIndex;
		int32_t L_13 = L_12;
		RuntimeObject* L_14 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_13);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_15 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_15, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_14, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_15, method);
	}

IL_0063:
	{
		RuntimeArray* L_16 = ___0_array;
		NullCheck(L_16);
		int32_t L_17;
		L_17 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_16, NULL);
		int32_t L_18 = ___1_arrayIndex;
		int32_t L_19 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(L_17, L_18))) >= ((int32_t)L_19)))
		{
			goto IL_007e;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_20 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_20, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_20, method);
	}

IL_007e:
	{
	}
	try
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_21 = __this->____array;
		RuntimeArray* L_22 = ___0_array;
		int32_t L_23 = ___1_arrayIndex;
		int32_t L_24 = __this->____size;
		Array_Copy_mB4904E17BD92E320613A3251C0205E0786B3BF41((RuntimeArray*)L_21, 0, L_22, L_23, L_24, NULL);
		RuntimeArray* L_25 = ___0_array;
		int32_t L_26 = ___1_arrayIndex;
		int32_t L_27 = __this->____size;
		Array_Reverse_mE788006243D34C654D7DDEF13E2D9E7B129AF8AD(L_25, L_26, L_27, NULL);
		goto IL_00b3;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_00a2;
		}
		throw e;
	}

CATCH_00a2:
	{
		ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1* L_28 = ((ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*)IL2CPP_GET_ACTIVE_EXCEPTION(ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*));;
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_29 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_29, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralBD0381A992FDF4F7DA60E5D83689FE7FF6309CB8)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_29, method);
	}

IL_00b3:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Enumerator_t0D7F1F6081F1B6DDB263573004129C443F04F41C Stack_1_GetEnumerator_mDB63D8CFE5E6ABAEAAB522D0B379A1856CC2434B_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t0D7F1F6081F1B6DDB263573004129C443F04F41C L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m89062508E459D03177AC1ECE62116EFC56A6FB61((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_Generic_IEnumerableU3CTU3E_GetEnumerator_m85DDC6ED4CF02AB415FB39E590E78CBCC427B0C7_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t0D7F1F6081F1B6DDB263573004129C443F04F41C L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m89062508E459D03177AC1ECE62116EFC56A6FB61((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t0D7F1F6081F1B6DDB263573004129C443F04F41C L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_IEnumerable_GetEnumerator_m5E943A8F427C868897F545EDBB1D696EF1DA3E92_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t0D7F1F6081F1B6DDB263573004129C443F04F41C L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m89062508E459D03177AC1ECE62116EFC56A6FB61((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t0D7F1F6081F1B6DDB263573004129C443F04F41C L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_TrimExcess_m76DB8C2BCB2EE051FB723AAFCDF37BFCEF47828E_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_0 = __this->____array;
		NullCheck(L_0);
		V_0 = il2cpp_codegen_cast_double_to_int<int32_t>(((double)il2cpp_codegen_multiply(((double)((int32_t)(((RuntimeArray*)L_0)->max_length))), (0.90000000000000002))));
		int32_t L_1 = __this->____size;
		int32_t L_2 = V_0;
		if ((((int32_t)L_1) >= ((int32_t)L_2)))
		{
			goto IL_003d;
		}
	}
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F** L_3 = (Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F**)(&__this->____array);
		int32_t L_4 = __this->____size;
		Array_Resize_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m540BB33A026A346B7B0033684BE811ED519B1E33(L_3, L_4, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_003d:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_Peek_mCBB15AA52FA8864E2B9065918EE83B3B246CB54F_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_m652E1B38200FB5443D4A6688E81A1589AE0D10DC(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_4 = V_1;
		int32_t L_5 = V_0;
		NullCheck(L_4);
		int32_t L_6 = L_5;
		int32_t L_7 = (L_4)->GetAt(static_cast<il2cpp_array_size_t>(L_6));
		return L_7;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPeek_mA27BEA35544EF743D4419EE9A4AA5D3F95C685C0_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, int32_t* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		int32_t* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(int32_t));
		return (bool)0;
	}

IL_001f:
	{
		int32_t* L_5 = ___0_result;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		int32_t L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		*(int32_t*)L_5 = L_9;
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_Pop_mF65EFCC543A2FA4060EBB2D1F791A2C95BF0C2B6_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* V_1 = NULL;
	int32_t V_2 = 0;
	int32_t G_B4_0 = 0;
	int32_t G_B3_0 = 0;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_m652E1B38200FB5443D4A6688E81A1589AE0D10DC(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		int32_t L_4 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_4, 1));
		int32_t L_5 = V_0;
		__this->____size = L_5;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		int32_t L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		G_B4_0 = L_9;
		goto IL_004f;
	}

IL_004f:
	{
		return G_B4_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPop_m16EB6172C4035BFDE28761D3A4DEB75126454AB8_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, int32_t* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* V_1 = NULL;
	int32_t V_2 = 0;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		int32_t* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(int32_t));
		return (bool)0;
	}

IL_001f:
	{
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
		int32_t L_6 = V_0;
		__this->____size = L_6;
		int32_t* L_7 = ___0_result;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_8 = V_1;
		int32_t L_9 = V_0;
		NullCheck(L_8);
		int32_t L_10 = L_9;
		int32_t L_11 = (L_8)->GetAt(static_cast<il2cpp_array_size_t>(L_10));
		*(int32_t*)L_7 = L_11;
		goto IL_0058;
	}

IL_0058:
	{
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Push_m2A7A3A8D713263E2DE6846D71351F2951D0D3459_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, int32_t ___0_item, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = L_0;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) < ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_4 = V_1;
		int32_t L_5 = V_0;
		int32_t L_6 = ___0_item;
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (int32_t)L_6);
		int32_t L_7 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_7, 1));
		int32_t L_8 = V_0;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_8, 1));
		return;
	}

IL_0034:
	{
		int32_t L_9 = ___0_item;
		Stack_1_PushWithResize_mBA4873FC90B54D47FCDFAF825F1E462FA6F28560(__this, L_9, il2cpp_rgctx_method(method->klass->rgctx_data, 16));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_mBA4873FC90B54D47FCDFAF825F1E462FA6F28560_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, int32_t ___0_item, const RuntimeMethod* method) 
{
	Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F** G_B2_0 = NULL;
	Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F** G_B1_0 = NULL;
	int32_t G_B3_0 = 0;
	Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F** G_B3_1 = NULL;
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F** L_0 = (Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F**)(&__this->____array);
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_1 = __this->____array;
		NullCheck(L_1);
		if (!(((RuntimeArray*)L_1)->max_length))
		{
			G_B2_0 = L_0;
			goto IL_001b;
		}
		G_B1_0 = L_0;
	}
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_2 = __this->____array;
		NullCheck(L_2);
		G_B3_0 = ((int32_t)il2cpp_codegen_multiply(2, ((int32_t)(((RuntimeArray*)L_2)->max_length))));
		G_B3_1 = G_B1_0;
		goto IL_001c;
	}

IL_001b:
	{
		G_B3_0 = 4;
		G_B3_1 = G_B2_0;
	}

IL_001c:
	{
		Array_Resize_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m540BB33A026A346B7B0033684BE811ED519B1E33(G_B3_1, G_B3_0, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_3 = __this->____array;
		int32_t L_4 = __this->____size;
		int32_t L_5 = ___0_item;
		NullCheck(L_3);
		(L_3)->SetAt(static_cast<il2cpp_array_size_t>(L_4), (int32_t)L_5);
		int32_t L_6 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_6, 1));
		int32_t L_7 = __this->____size;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_7, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* Stack_1_ToArray_m38D62507A8284290DFCEE3279635B1487A71EBC8_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = __this->____size;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_1;
		L_1 = Array_Empty_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m94E12BB613D748D2EEB9E1ABD961630D2F970385_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____size;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_3 = (Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F*)(Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_2);
		V_0 = L_3;
		V_1 = 0;
		goto IL_003e;
	}

IL_001e:
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_4 = V_0;
		int32_t L_5 = V_1;
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_6 = __this->____array;
		int32_t L_7 = __this->____size;
		int32_t L_8 = V_1;
		NullCheck(L_6);
		int32_t L_9 = ((int32_t)il2cpp_codegen_subtract(((int32_t)il2cpp_codegen_subtract(L_7, L_8)), 1));
		int32_t L_10 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_9));
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (int32_t)L_10);
		int32_t L_11 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_11, 1));
	}

IL_003e:
	{
		int32_t L_12 = V_1;
		int32_t L_13 = __this->____size;
		if ((((int32_t)L_12) < ((int32_t)L_13)))
		{
			goto IL_001e;
		}
	}
	{
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_14 = V_0;
		return L_14;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m652E1B38200FB5443D4A6688E81A1589AE0D10DC_gshared (Stack_1_t509AEAED71EF48FB8551C238C1BA01882CC654B9* __this, const RuntimeMethod* method) 
{
	{
		InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB* L_0 = (InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB_il2cpp_TypeInfo_var)));
		InvalidOperationException__ctor_mE4CB6F4712AB6D99A2358FBAE2E052B3EE976162(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral95F0EE30865D503A05F1D329BC3FED0946B65C24)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mE8BC3D8693065B793B1A0CA19DA48B1F049CB940_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_0;
		L_0 = Array_Empty_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m36408D2B54D6A6519FAF5E5AD03F49ECECA09F2E_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		__this->____array = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mEA234C0CB558E2C38137325D097689FD7EBFFFF2_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, int32_t ___0_capacity, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		int32_t L_0 = ___0_capacity;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_0020;
		}
	}
	{
		int32_t L_1 = ___0_capacity;
		int32_t L_2 = L_1;
		RuntimeObject* L_3 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_2);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_4 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC37D78082ACFC8DEE7B32D9351C6E433A074FEC7)), L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral38E3DBC7FC353425EF3A98DC8DAC6689AF5FD1BE)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0020:
	{
		int32_t L_5 = ___0_capacity;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_6 = (Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D*)(Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_5);
		__this->____array = L_6;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_6);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mF3F6A4A85EA28E2873ED705440F262AC76C8BE0B_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, RuntimeObject* ___0_collection, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		RuntimeObject* L_0 = ___0_collection;
		if (L_0)
		{
			goto IL_0014;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral469F05BE9BB4C7903C353D0EB9F6384C84A48B25)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_0014:
	{
		RuntimeObject* L_2 = ___0_collection;
		int32_t* L_3 = (int32_t*)(&__this->____size);
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_4;
		L_4 = EnumerableHelpers_ToArray_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m7FE7D9EE8E27D99A69409883CE6BE8EFE9795FD5(L_2, L_3, il2cpp_rgctx_method(method->klass->rgctx_data, 5));
		__this->____array = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_4);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_get_Count_mE0CA6020FCAC821CECC725F9BDD93EEDFE97A872_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_System_Collections_ICollection_get_IsSynchronized_mAC5B5D1F0715D3F5EDCAAFD1DB41E98DC2A04E9C_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_ICollection_get_SyncRoot_mC5BE9250C67B11D27BC5D7F3AC54C729612BBFF8_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = __this->____syncRoot;
		if (L_0)
		{
			goto IL_001a;
		}
	}
	{
		RuntimeObject** L_1 = (RuntimeObject**)(&__this->____syncRoot);
		RuntimeObject* L_2 = (RuntimeObject*)il2cpp_codegen_object_new(RuntimeObject_il2cpp_TypeInfo_var);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(L_2, NULL);
		RuntimeObject* L_3;
		L_3 = InterlockedCompareExchangeImpl<RuntimeObject*>(L_1, L_2, NULL);
	}

IL_001a:
	{
		RuntimeObject* L_4 = __this->____syncRoot;
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Clear_mE760938D1ADEAF722C519115D86462EF392D5F8E_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	{
		goto IL_0019;
	}

IL_0019:
	{
		__this->____size = 0;
		int32_t L_0 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_0, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_Contains_m5EE0B038B11D0498E8627CCD869DC641026D4D1B_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___0_item, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		if (!L_0)
		{
			goto IL_0023;
		}
	}
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_1 = __this->____array;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_2 = ___0_item;
		int32_t L_3 = __this->____size;
		int32_t L_4;
		L_4 = Array_LastIndexOf_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_mFC821A119D3BD51B91A59F09A28A8FDC1C32F6FC(L_1, L_2, ((int32_t)il2cpp_codegen_subtract(L_3, 1)), il2cpp_rgctx_method(method->klass->rgctx_data, 8));
		return (bool)((((int32_t)((((int32_t)L_4) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
	}

IL_0023:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_CopyTo_m4EF6D5F7E4807B3E21C1E1D8D877FA582BB96229_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		int32_t L_2 = ___1_arrayIndex;
		if ((((int32_t)L_2) < ((int32_t)0)))
		{
			goto IL_0018;
		}
	}
	{
		int32_t L_3 = ___1_arrayIndex;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_4 = ___0_array;
		NullCheck(L_4);
		if ((((int32_t)L_3) <= ((int32_t)((int32_t)(((RuntimeArray*)L_4)->max_length)))))
		{
			goto IL_002e;
		}
	}

IL_0018:
	{
		int32_t L_5 = ___1_arrayIndex;
		int32_t L_6 = L_5;
		RuntimeObject* L_7 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_6);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_8 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_8, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_8, method);
	}

IL_002e:
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_9 = ___0_array;
		NullCheck(L_9);
		int32_t L_10 = ___1_arrayIndex;
		int32_t L_11 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_9)->max_length)), L_10))) >= ((int32_t)L_11)))
		{
			goto IL_0046;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_12 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_12, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_12, method);
	}

IL_0046:
	{
		V_0 = 0;
		int32_t L_13 = ___1_arrayIndex;
		int32_t L_14 = __this->____size;
		V_1 = ((int32_t)il2cpp_codegen_add(L_13, L_14));
		goto IL_006e;
	}

IL_0053:
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_15 = ___0_array;
		int32_t L_16 = V_1;
		int32_t L_17 = ((int32_t)il2cpp_codegen_subtract(L_16, 1));
		V_1 = L_17;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_18 = __this->____array;
		int32_t L_19 = V_0;
		int32_t L_20 = L_19;
		V_0 = ((int32_t)il2cpp_codegen_add(L_20, 1));
		NullCheck(L_18);
		int32_t L_21 = L_20;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_22 = (L_18)->GetAt(static_cast<il2cpp_array_size_t>(L_21));
		NullCheck(L_15);
		(L_15)->SetAt(static_cast<il2cpp_array_size_t>(L_17), (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6)L_22);
	}

IL_006e:
	{
		int32_t L_23 = V_0;
		int32_t L_24 = __this->____size;
		if ((((int32_t)L_23) < ((int32_t)L_24)))
		{
			goto IL_0053;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_System_Collections_ICollection_CopyTo_mF378128FED4B9119EE9AD3C0CD02C2BA04B244AC_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, RuntimeArray* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		RuntimeArray* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		RuntimeArray* L_2 = ___0_array;
		NullCheck(L_2);
		int32_t L_3;
		L_3 = Array_get_Rank_m9383A200A2ECC89ECA44FE5F812ECFB874449C5F(L_2, NULL);
		if ((((int32_t)L_3) == ((int32_t)1)))
		{
			goto IL_0027;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_4 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral967D403A541A1026A83D548E5AD5CA800AD4EFB5)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0027:
	{
		RuntimeArray* L_5 = ___0_array;
		NullCheck(L_5);
		int32_t L_6;
		L_6 = Array_GetLowerBound_m4FB0601E2E8A6304A42E3FC400576DF7B0F084BC(L_5, 0, NULL);
		if (!L_6)
		{
			goto IL_0040;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_7 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral6195D7DA68D16D4985AD1A1B4FD2841A43CDDE70)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_7, method);
	}

IL_0040:
	{
		int32_t L_8 = ___1_arrayIndex;
		if ((((int32_t)L_8) < ((int32_t)0)))
		{
			goto IL_004d;
		}
	}
	{
		int32_t L_9 = ___1_arrayIndex;
		RuntimeArray* L_10 = ___0_array;
		NullCheck(L_10);
		int32_t L_11;
		L_11 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_10, NULL);
		if ((((int32_t)L_9) <= ((int32_t)L_11)))
		{
			goto IL_0063;
		}
	}

IL_004d:
	{
		int32_t L_12 = ___1_arrayIndex;
		int32_t L_13 = L_12;
		RuntimeObject* L_14 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_13);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_15 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_15, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_14, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_15, method);
	}

IL_0063:
	{
		RuntimeArray* L_16 = ___0_array;
		NullCheck(L_16);
		int32_t L_17;
		L_17 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_16, NULL);
		int32_t L_18 = ___1_arrayIndex;
		int32_t L_19 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(L_17, L_18))) >= ((int32_t)L_19)))
		{
			goto IL_007e;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_20 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_20, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_20, method);
	}

IL_007e:
	{
	}
	try
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_21 = __this->____array;
		RuntimeArray* L_22 = ___0_array;
		int32_t L_23 = ___1_arrayIndex;
		int32_t L_24 = __this->____size;
		Array_Copy_mB4904E17BD92E320613A3251C0205E0786B3BF41((RuntimeArray*)L_21, 0, L_22, L_23, L_24, NULL);
		RuntimeArray* L_25 = ___0_array;
		int32_t L_26 = ___1_arrayIndex;
		int32_t L_27 = __this->____size;
		Array_Reverse_mE788006243D34C654D7DDEF13E2D9E7B129AF8AD(L_25, L_26, L_27, NULL);
		goto IL_00b3;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_00a2;
		}
		throw e;
	}

CATCH_00a2:
	{
		ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1* L_28 = ((ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*)IL2CPP_GET_ACTIVE_EXCEPTION(ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*));;
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_29 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_29, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralBD0381A992FDF4F7DA60E5D83689FE7FF6309CB8)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_29, method);
	}

IL_00b3:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Enumerator_t1999C0BADD971016559CDAF90DA9DC67FAEF5B8A Stack_1_GetEnumerator_m02240169A5AC764BD521BF173361FA927C136FB7_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t1999C0BADD971016559CDAF90DA9DC67FAEF5B8A L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_mB2E659404649FBEA780D53C3FA4E957998A349FB((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_Generic_IEnumerableU3CTU3E_GetEnumerator_m90327FF68B7506C945BA4506E55DF116B296B4FC_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t1999C0BADD971016559CDAF90DA9DC67FAEF5B8A L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_mB2E659404649FBEA780D53C3FA4E957998A349FB((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t1999C0BADD971016559CDAF90DA9DC67FAEF5B8A L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_IEnumerable_GetEnumerator_m2D77C0C75B6A42F516D2AFB5B44A703EC0B41F6F_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t1999C0BADD971016559CDAF90DA9DC67FAEF5B8A L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_mB2E659404649FBEA780D53C3FA4E957998A349FB((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t1999C0BADD971016559CDAF90DA9DC67FAEF5B8A L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_TrimExcess_mB940150782F38C3E66ABF9D28792F8A1F60FF7B6_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_0 = __this->____array;
		NullCheck(L_0);
		V_0 = il2cpp_codegen_cast_double_to_int<int32_t>(((double)il2cpp_codegen_multiply(((double)((int32_t)(((RuntimeArray*)L_0)->max_length))), (0.90000000000000002))));
		int32_t L_1 = __this->____size;
		int32_t L_2 = V_0;
		if ((((int32_t)L_1) >= ((int32_t)L_2)))
		{
			goto IL_003d;
		}
	}
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D** L_3 = (Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D**)(&__this->____array);
		int32_t L_4 = __this->____size;
		Array_Resize_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m45E2DE0DBF4E0547B569F9496A9D773B058C30CA(L_3, L_4, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_003d:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 Stack_1_Peek_m4C71DF7964D5892AE0F074981907CC29E354C498_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_m8A1BE6CEDDAD8DB304FC2E141DA5635DB3B88338(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_4 = V_1;
		int32_t L_5 = V_0;
		NullCheck(L_4);
		int32_t L_6 = L_5;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_7 = (L_4)->GetAt(static_cast<il2cpp_array_size_t>(L_6));
		return L_7;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPeek_m0C48E59870DD588F21B1E3878D487121E17B5F14_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6));
		return (bool)0;
	}

IL_001f:
	{
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* L_5 = ___0_result;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		*(Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6*)L_5 = L_9;
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 Stack_1_Pop_m682D32993E1CB092C642F495A93915B11F401A7F_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* V_1 = NULL;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 V_2;
	memset((&V_2), 0, sizeof(V_2));
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 G_B4_0;
	memset((&G_B4_0), 0, sizeof(G_B4_0));
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 G_B3_0;
	memset((&G_B3_0), 0, sizeof(G_B3_0));
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_m8A1BE6CEDDAD8DB304FC2E141DA5635DB3B88338(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		int32_t L_4 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_4, 1));
		int32_t L_5 = V_0;
		__this->____size = L_5;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		G_B4_0 = L_9;
		goto IL_004f;
	}

IL_004f:
	{
		return G_B4_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPop_m5A0398B2FE8793B31D3B00B60F5190D16FA04E18_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* V_1 = NULL;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 V_2;
	memset((&V_2), 0, sizeof(V_2));
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6));
		return (bool)0;
	}

IL_001f:
	{
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
		int32_t L_6 = V_0;
		__this->____size = L_6;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* L_7 = ___0_result;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_8 = V_1;
		int32_t L_9 = V_0;
		NullCheck(L_8);
		int32_t L_10 = L_9;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_11 = (L_8)->GetAt(static_cast<il2cpp_array_size_t>(L_10));
		*(Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6*)L_7 = L_11;
		goto IL_0058;
	}

IL_0058:
	{
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Push_m4D734379CC958AED5ED7FF36A9467EDCE6AFDF56_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___0_item, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = L_0;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) < ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_4 = V_1;
		int32_t L_5 = V_0;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_6 = ___0_item;
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6)L_6);
		int32_t L_7 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_7, 1));
		int32_t L_8 = V_0;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_8, 1));
		return;
	}

IL_0034:
	{
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_9 = ___0_item;
		Stack_1_PushWithResize_m08841831A121C2488026F8131C34F8C15C250CC1(__this, L_9, il2cpp_rgctx_method(method->klass->rgctx_data, 16));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_m08841831A121C2488026F8131C34F8C15C250CC1_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___0_item, const RuntimeMethod* method) 
{
	Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D** G_B2_0 = NULL;
	Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D** G_B1_0 = NULL;
	int32_t G_B3_0 = 0;
	Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D** G_B3_1 = NULL;
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D** L_0 = (Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D**)(&__this->____array);
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_1 = __this->____array;
		NullCheck(L_1);
		if (!(((RuntimeArray*)L_1)->max_length))
		{
			G_B2_0 = L_0;
			goto IL_001b;
		}
		G_B1_0 = L_0;
	}
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_2 = __this->____array;
		NullCheck(L_2);
		G_B3_0 = ((int32_t)il2cpp_codegen_multiply(2, ((int32_t)(((RuntimeArray*)L_2)->max_length))));
		G_B3_1 = G_B1_0;
		goto IL_001c;
	}

IL_001b:
	{
		G_B3_0 = 4;
		G_B3_1 = G_B2_0;
	}

IL_001c:
	{
		Array_Resize_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m45E2DE0DBF4E0547B569F9496A9D773B058C30CA(G_B3_1, G_B3_0, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_3 = __this->____array;
		int32_t L_4 = __this->____size;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_5 = ___0_item;
		NullCheck(L_3);
		(L_3)->SetAt(static_cast<il2cpp_array_size_t>(L_4), (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6)L_5);
		int32_t L_6 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_6, 1));
		int32_t L_7 = __this->____size;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_7, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* Stack_1_ToArray_m5C2695E48EE7B1A26EC9F7CD753081F409F2468F_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = __this->____size;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_1;
		L_1 = Array_Empty_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m36408D2B54D6A6519FAF5E5AD03F49ECECA09F2E_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____size;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_3 = (Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D*)(Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_2);
		V_0 = L_3;
		V_1 = 0;
		goto IL_003e;
	}

IL_001e:
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_4 = V_0;
		int32_t L_5 = V_1;
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_6 = __this->____array;
		int32_t L_7 = __this->____size;
		int32_t L_8 = V_1;
		NullCheck(L_6);
		int32_t L_9 = ((int32_t)il2cpp_codegen_subtract(((int32_t)il2cpp_codegen_subtract(L_7, L_8)), 1));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_10 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_9));
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6)L_10);
		int32_t L_11 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_11, 1));
	}

IL_003e:
	{
		int32_t L_12 = V_1;
		int32_t L_13 = __this->____size;
		if ((((int32_t)L_12) < ((int32_t)L_13)))
		{
			goto IL_001e;
		}
	}
	{
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_14 = V_0;
		return L_14;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m8A1BE6CEDDAD8DB304FC2E141DA5635DB3B88338_gshared (Stack_1_t8A661B4E11F715EF60608DE57485D9956E00C72E* __this, const RuntimeMethod* method) 
{
	{
		InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB* L_0 = (InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB_il2cpp_TypeInfo_var)));
		InvalidOperationException__ctor_mE4CB6F4712AB6D99A2358FBAE2E052B3EE976162(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral95F0EE30865D503A05F1D329BC3FED0946B65C24)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m70E8EDA96A608CE9BAB7FC8313B233AADA573BD4_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0;
		L_0 = Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		__this->____array = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m25F8C6095172E75DEE8A43E857889659DFC4DCE9_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, int32_t ___0_capacity, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		int32_t L_0 = ___0_capacity;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_0020;
		}
	}
	{
		int32_t L_1 = ___0_capacity;
		int32_t L_2 = L_1;
		RuntimeObject* L_3 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_2);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_4 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC37D78082ACFC8DEE7B32D9351C6E433A074FEC7)), L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral38E3DBC7FC353425EF3A98DC8DAC6689AF5FD1BE)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0020:
	{
		int32_t L_5 = ___0_capacity;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_5);
		__this->____array = L_6;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_6);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mA803B72D0217A6AF4F4C963A239374370AABBA49_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, RuntimeObject* ___0_collection, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		RuntimeObject* L_0 = ___0_collection;
		if (L_0)
		{
			goto IL_0014;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral469F05BE9BB4C7903C353D0EB9F6384C84A48B25)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_0014:
	{
		RuntimeObject* L_2 = ___0_collection;
		int32_t* L_3 = (int32_t*)(&__this->____size);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4;
		L_4 = EnumerableHelpers_ToArray_TisRuntimeObject_m4BA4DBD38B4DA66F8DEB40393971473B983BE06A(L_2, L_3, il2cpp_rgctx_method(method->klass->rgctx_data, 5));
		__this->____array = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_4);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_get_Count_mD08AE71D49787D30DDD9D484BCD323D646744D2E_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_System_Collections_ICollection_get_IsSynchronized_mC8C675C38F78EC0A9434F851D663057BE1A51CC9_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_ICollection_get_SyncRoot_mCA7DEBD17371E49CE88F409F21DDDA6486FACCFF_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = __this->____syncRoot;
		if (L_0)
		{
			goto IL_001a;
		}
	}
	{
		RuntimeObject** L_1 = (RuntimeObject**)(&__this->____syncRoot);
		RuntimeObject* L_2 = (RuntimeObject*)il2cpp_codegen_object_new(RuntimeObject_il2cpp_TypeInfo_var);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(L_2, NULL);
		RuntimeObject* L_3;
		L_3 = InterlockedCompareExchangeImpl<RuntimeObject*>(L_1, L_2, NULL);
	}

IL_001a:
	{
		RuntimeObject* L_4 = __this->____syncRoot;
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Clear_mD550E89582979ECB0D6E6D68F0237FC14708BE85_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	{
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = __this->____array;
		int32_t L_1 = __this->____size;
		Array_Clear_m50BAA3751899858B097D3FF2ED31F284703FE5CB((RuntimeArray*)L_0, 0, L_1, NULL);
	}

IL_0019:
	{
		__this->____size = 0;
		int32_t L_2 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_2, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_Contains_mDA7FA93F340E77121271246B7003BF77B08DDCF2_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, RuntimeObject* ___0_item, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		if (!L_0)
		{
			goto IL_0023;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = __this->____array;
		RuntimeObject* L_2 = ___0_item;
		int32_t L_3 = __this->____size;
		int32_t L_4;
		L_4 = Array_LastIndexOf_TisRuntimeObject_m60B0AA749643DFCD60274810604B33A8A2CF0A40(L_1, L_2, ((int32_t)il2cpp_codegen_subtract(L_3, 1)), il2cpp_rgctx_method(method->klass->rgctx_data, 8));
		return (bool)((((int32_t)((((int32_t)L_4) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
	}

IL_0023:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_CopyTo_m08586ECEE421F0A4CD3A71E13CD0E37DA5485470_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		int32_t L_2 = ___1_arrayIndex;
		if ((((int32_t)L_2) < ((int32_t)0)))
		{
			goto IL_0018;
		}
	}
	{
		int32_t L_3 = ___1_arrayIndex;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = ___0_array;
		NullCheck(L_4);
		if ((((int32_t)L_3) <= ((int32_t)((int32_t)(((RuntimeArray*)L_4)->max_length)))))
		{
			goto IL_002e;
		}
	}

IL_0018:
	{
		int32_t L_5 = ___1_arrayIndex;
		int32_t L_6 = L_5;
		RuntimeObject* L_7 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_6);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_8 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_8, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_8, method);
	}

IL_002e:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_9 = ___0_array;
		NullCheck(L_9);
		int32_t L_10 = ___1_arrayIndex;
		int32_t L_11 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_9)->max_length)), L_10))) >= ((int32_t)L_11)))
		{
			goto IL_0046;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_12 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_12, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_12, method);
	}

IL_0046:
	{
		V_0 = 0;
		int32_t L_13 = ___1_arrayIndex;
		int32_t L_14 = __this->____size;
		V_1 = ((int32_t)il2cpp_codegen_add(L_13, L_14));
		goto IL_006e;
	}

IL_0053:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_15 = ___0_array;
		int32_t L_16 = V_1;
		int32_t L_17 = ((int32_t)il2cpp_codegen_subtract(L_16, 1));
		V_1 = L_17;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_18 = __this->____array;
		int32_t L_19 = V_0;
		int32_t L_20 = L_19;
		V_0 = ((int32_t)il2cpp_codegen_add(L_20, 1));
		NullCheck(L_18);
		int32_t L_21 = L_20;
		RuntimeObject* L_22 = (L_18)->GetAt(static_cast<il2cpp_array_size_t>(L_21));
		NullCheck(L_15);
		(L_15)->SetAt(static_cast<il2cpp_array_size_t>(L_17), (RuntimeObject*)L_22);
	}

IL_006e:
	{
		int32_t L_23 = V_0;
		int32_t L_24 = __this->____size;
		if ((((int32_t)L_23) < ((int32_t)L_24)))
		{
			goto IL_0053;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_System_Collections_ICollection_CopyTo_m20FF8EC87636B953D59AA7A6810DA984A5D136E6_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, RuntimeArray* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		RuntimeArray* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		RuntimeArray* L_2 = ___0_array;
		NullCheck(L_2);
		int32_t L_3;
		L_3 = Array_get_Rank_m9383A200A2ECC89ECA44FE5F812ECFB874449C5F(L_2, NULL);
		if ((((int32_t)L_3) == ((int32_t)1)))
		{
			goto IL_0027;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_4 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral967D403A541A1026A83D548E5AD5CA800AD4EFB5)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0027:
	{
		RuntimeArray* L_5 = ___0_array;
		NullCheck(L_5);
		int32_t L_6;
		L_6 = Array_GetLowerBound_m4FB0601E2E8A6304A42E3FC400576DF7B0F084BC(L_5, 0, NULL);
		if (!L_6)
		{
			goto IL_0040;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_7 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral6195D7DA68D16D4985AD1A1B4FD2841A43CDDE70)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_7, method);
	}

IL_0040:
	{
		int32_t L_8 = ___1_arrayIndex;
		if ((((int32_t)L_8) < ((int32_t)0)))
		{
			goto IL_004d;
		}
	}
	{
		int32_t L_9 = ___1_arrayIndex;
		RuntimeArray* L_10 = ___0_array;
		NullCheck(L_10);
		int32_t L_11;
		L_11 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_10, NULL);
		if ((((int32_t)L_9) <= ((int32_t)L_11)))
		{
			goto IL_0063;
		}
	}

IL_004d:
	{
		int32_t L_12 = ___1_arrayIndex;
		int32_t L_13 = L_12;
		RuntimeObject* L_14 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_13);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_15 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_15, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_14, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_15, method);
	}

IL_0063:
	{
		RuntimeArray* L_16 = ___0_array;
		NullCheck(L_16);
		int32_t L_17;
		L_17 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_16, NULL);
		int32_t L_18 = ___1_arrayIndex;
		int32_t L_19 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(L_17, L_18))) >= ((int32_t)L_19)))
		{
			goto IL_007e;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_20 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_20, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_20, method);
	}

IL_007e:
	{
	}
	try
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_21 = __this->____array;
		RuntimeArray* L_22 = ___0_array;
		int32_t L_23 = ___1_arrayIndex;
		int32_t L_24 = __this->____size;
		Array_Copy_mB4904E17BD92E320613A3251C0205E0786B3BF41((RuntimeArray*)L_21, 0, L_22, L_23, L_24, NULL);
		RuntimeArray* L_25 = ___0_array;
		int32_t L_26 = ___1_arrayIndex;
		int32_t L_27 = __this->____size;
		Array_Reverse_mE788006243D34C654D7DDEF13E2D9E7B129AF8AD(L_25, L_26, L_27, NULL);
		goto IL_00b3;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_00a2;
		}
		throw e;
	}

CATCH_00a2:
	{
		ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1* L_28 = ((ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*)IL2CPP_GET_ACTIVE_EXCEPTION(ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*));;
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_29 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_29, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralBD0381A992FDF4F7DA60E5D83689FE7FF6309CB8)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_29, method);
	}

IL_00b3:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Enumerator_t852186DADC50D976C4BD8FE59C506354ED48B974 Stack_1_GetEnumerator_m0CCBAC4DEE59DAE0D095E9ABAE44E17B5002382C_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t852186DADC50D976C4BD8FE59C506354ED48B974 L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m9F25E94BA06F0C82E9CE1E5A204D5C65DEA9DEAB((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_Generic_IEnumerableU3CTU3E_GetEnumerator_m7E1DB0BA4510B052067303F5439D47FC54F76815_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t852186DADC50D976C4BD8FE59C506354ED48B974 L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m9F25E94BA06F0C82E9CE1E5A204D5C65DEA9DEAB((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t852186DADC50D976C4BD8FE59C506354ED48B974 L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_IEnumerable_GetEnumerator_mB13E70A7273352E65D0A8D7D574425EA412808C3_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t852186DADC50D976C4BD8FE59C506354ED48B974 L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m9F25E94BA06F0C82E9CE1E5A204D5C65DEA9DEAB((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t852186DADC50D976C4BD8FE59C506354ED48B974 L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_TrimExcess_m62298C7C54F462CA5FAEB67F574092C9C4B56852_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = __this->____array;
		NullCheck(L_0);
		V_0 = il2cpp_codegen_cast_double_to_int<int32_t>(((double)il2cpp_codegen_multiply(((double)((int32_t)(((RuntimeArray*)L_0)->max_length))), (0.90000000000000002))));
		int32_t L_1 = __this->____size;
		int32_t L_2 = V_0;
		if ((((int32_t)L_1) >= ((int32_t)L_2)))
		{
			goto IL_003d;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918** L_3 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918**)(&__this->____array);
		int32_t L_4 = __this->____size;
		Array_Resize_TisRuntimeObject_mE8D92C287251BAF8256D85E5829F749359EC334E(L_3, L_4, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_003d:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_Peek_mF0ECF6A61726B66E6D9B33D8C4DEAA47E586E6E4_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_m44B37D35FD9357C7012A60D95B04AB96C0463EC7(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = V_1;
		int32_t L_5 = V_0;
		NullCheck(L_4);
		int32_t L_6 = L_5;
		RuntimeObject* L_7 = (L_4)->GetAt(static_cast<il2cpp_array_size_t>(L_6));
		return L_7;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPeek_mE6B8D67DFB0053C48D2FAA02DC2B451502D02B79_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, RuntimeObject** ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		RuntimeObject** L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(RuntimeObject*));
		return (bool)0;
	}

IL_001f:
	{
		RuntimeObject** L_5 = ___0_result;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		RuntimeObject* L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		*(RuntimeObject**)L_5 = L_9;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_5, (void*)L_9);
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_Pop_m2AFF69249659372F07EE25817DBCAFE74E1CF778_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* V_1 = NULL;
	RuntimeObject* V_2 = NULL;
	RuntimeObject* G_B4_0 = NULL;
	RuntimeObject* G_B3_0 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_m44B37D35FD9357C7012A60D95B04AB96C0463EC7(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		int32_t L_4 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_4, 1));
		int32_t L_5 = V_0;
		__this->____size = L_5;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		RuntimeObject* L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		G_B3_0 = L_9;
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_10 = V_1;
		int32_t L_11 = V_0;
		il2cpp_codegen_initobj((&V_2), sizeof(RuntimeObject*));
		RuntimeObject* L_12 = V_2;
		NullCheck(L_10);
		(L_10)->SetAt(static_cast<il2cpp_array_size_t>(L_11), (RuntimeObject*)L_12);
		G_B4_0 = G_B3_0;
	}

IL_004f:
	{
		return G_B4_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPop_m963EB60D733BD73C9293F3856623FB359DCEF4A8_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, RuntimeObject** ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* V_1 = NULL;
	RuntimeObject* V_2 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		RuntimeObject** L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(RuntimeObject*));
		return (bool)0;
	}

IL_001f:
	{
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
		int32_t L_6 = V_0;
		__this->____size = L_6;
		RuntimeObject** L_7 = ___0_result;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_8 = V_1;
		int32_t L_9 = V_0;
		NullCheck(L_8);
		int32_t L_10 = L_9;
		RuntimeObject* L_11 = (L_8)->GetAt(static_cast<il2cpp_array_size_t>(L_10));
		*(RuntimeObject**)L_7 = L_11;
		Il2CppCodeGenWriteBarrier((void**)(RuntimeObject**)L_7, (void*)L_11);
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_12 = V_1;
		int32_t L_13 = V_0;
		il2cpp_codegen_initobj((&V_2), sizeof(RuntimeObject*));
		RuntimeObject* L_14 = V_2;
		NullCheck(L_12);
		(L_12)->SetAt(static_cast<il2cpp_array_size_t>(L_13), (RuntimeObject*)L_14);
	}

IL_0058:
	{
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Push_m709DD11BC1291A905814182CF9A367DE7399A778_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, RuntimeObject* ___0_item, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = L_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) < ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = V_1;
		int32_t L_5 = V_0;
		RuntimeObject* L_6 = ___0_item;
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (RuntimeObject*)L_6);
		int32_t L_7 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_7, 1));
		int32_t L_8 = V_0;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_8, 1));
		return;
	}

IL_0034:
	{
		RuntimeObject* L_9 = ___0_item;
		Stack_1_PushWithResize_mEC6D96B634757F6C1B0839B2B591944DF48C2B9B(__this, L_9, il2cpp_rgctx_method(method->klass->rgctx_data, 16));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_mEC6D96B634757F6C1B0839B2B591944DF48C2B9B_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, RuntimeObject* ___0_item, const RuntimeMethod* method) 
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918** G_B2_0 = NULL;
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918** G_B1_0 = NULL;
	int32_t G_B3_0 = 0;
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918** G_B3_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918** L_0 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918**)(&__this->____array);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = __this->____array;
		NullCheck(L_1);
		if (!(((RuntimeArray*)L_1)->max_length))
		{
			G_B2_0 = L_0;
			goto IL_001b;
		}
		G_B1_0 = L_0;
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = __this->____array;
		NullCheck(L_2);
		G_B3_0 = ((int32_t)il2cpp_codegen_multiply(2, ((int32_t)(((RuntimeArray*)L_2)->max_length))));
		G_B3_1 = G_B1_0;
		goto IL_001c;
	}

IL_001b:
	{
		G_B3_0 = 4;
		G_B3_1 = G_B2_0;
	}

IL_001c:
	{
		Array_Resize_TisRuntimeObject_mE8D92C287251BAF8256D85E5829F749359EC334E(G_B3_1, G_B3_0, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = __this->____array;
		int32_t L_4 = __this->____size;
		RuntimeObject* L_5 = ___0_item;
		NullCheck(L_3);
		(L_3)->SetAt(static_cast<il2cpp_array_size_t>(L_4), (RuntimeObject*)L_5);
		int32_t L_6 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_6, 1));
		int32_t L_7 = __this->____size;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_7, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* Stack_1_ToArray_m56A0E92066552DA8307949BCCC575556ECE44764_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = __this->____size;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1;
		L_1 = Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____size;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_2);
		V_0 = L_3;
		V_1 = 0;
		goto IL_003e;
	}

IL_001e:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = V_0;
		int32_t L_5 = V_1;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = __this->____array;
		int32_t L_7 = __this->____size;
		int32_t L_8 = V_1;
		NullCheck(L_6);
		int32_t L_9 = ((int32_t)il2cpp_codegen_subtract(((int32_t)il2cpp_codegen_subtract(L_7, L_8)), 1));
		RuntimeObject* L_10 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_9));
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (RuntimeObject*)L_10);
		int32_t L_11 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_11, 1));
	}

IL_003e:
	{
		int32_t L_12 = V_1;
		int32_t L_13 = __this->____size;
		if ((((int32_t)L_12) < ((int32_t)L_13)))
		{
			goto IL_001e;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = V_0;
		return L_14;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m44B37D35FD9357C7012A60D95B04AB96C0463EC7_gshared (Stack_1_tAD790A47551563636908E21E4F08C54C0C323EB5* __this, const RuntimeMethod* method) 
{
	{
		InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB* L_0 = (InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB_il2cpp_TypeInfo_var)));
		InvalidOperationException__ctor_mE4CB6F4712AB6D99A2358FBAE2E052B3EE976162(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral95F0EE30865D503A05F1D329BC3FED0946B65C24)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mE8025F482A1E24A3D57137158FD84D1DE34436CA_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_0;
		L_0 = Array_Empty_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_m85A5CFE26D5D127B03744735ED2B5FC30FCB1D59_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		__this->____array = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mFF546E1DE2E2F989C809B6F382BDF89D49AD1BB3_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, int32_t ___0_capacity, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		int32_t L_0 = ___0_capacity;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_0020;
		}
	}
	{
		int32_t L_1 = ___0_capacity;
		int32_t L_2 = L_1;
		RuntimeObject* L_3 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_2);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_4 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC37D78082ACFC8DEE7B32D9351C6E433A074FEC7)), L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral38E3DBC7FC353425EF3A98DC8DAC6689AF5FD1BE)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0020:
	{
		int32_t L_5 = ___0_capacity;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_6 = (RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993*)(RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_5);
		__this->____array = L_6;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_6);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mA4243B0DDF4679591D67ED4F56896190C1257824_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, RuntimeObject* ___0_collection, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		RuntimeObject* L_0 = ___0_collection;
		if (L_0)
		{
			goto IL_0014;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral469F05BE9BB4C7903C353D0EB9F6384C84A48B25)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_0014:
	{
		RuntimeObject* L_2 = ___0_collection;
		int32_t* L_3 = (int32_t*)(&__this->____size);
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_4;
		L_4 = EnumerableHelpers_ToArray_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_mB8308980C50019080799C439861F5EF700799906(L_2, L_3, il2cpp_rgctx_method(method->klass->rgctx_data, 5));
		__this->____array = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_4);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_get_Count_m6352E8A1C75FAF97C0127210A3241D92172561AD_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_System_Collections_ICollection_get_IsSynchronized_m37971AC3BF416AC49D0B33790CE37E3EB4291B18_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_ICollection_get_SyncRoot_m67AA71A87E100FD3E26C9D60E12884C477BBB4F1_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = __this->____syncRoot;
		if (L_0)
		{
			goto IL_001a;
		}
	}
	{
		RuntimeObject** L_1 = (RuntimeObject**)(&__this->____syncRoot);
		RuntimeObject* L_2 = (RuntimeObject*)il2cpp_codegen_object_new(RuntimeObject_il2cpp_TypeInfo_var);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(L_2, NULL);
		RuntimeObject* L_3;
		L_3 = InterlockedCompareExchangeImpl<RuntimeObject*>(L_1, L_2, NULL);
	}

IL_001a:
	{
		RuntimeObject* L_4 = __this->____syncRoot;
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Clear_mD2D3759CB5BA49536707C59755691DDAA6C21960_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	{
		goto IL_0019;
	}

IL_0019:
	{
		__this->____size = 0;
		int32_t L_0 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_0, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_Contains_m5A90B4FBA534A75F3F7FCE612F7D0E9DFC580A69_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___0_item, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		if (!L_0)
		{
			goto IL_0023;
		}
	}
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_1 = __this->____array;
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D L_2 = ___0_item;
		int32_t L_3 = __this->____size;
		int32_t L_4;
		L_4 = Array_LastIndexOf_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_mCEA7CE77FA45B1CB38444D71C46BC24E06A0ECA4(L_1, L_2, ((int32_t)il2cpp_codegen_subtract(L_3, 1)), il2cpp_rgctx_method(method->klass->rgctx_data, 8));
		return (bool)((((int32_t)((((int32_t)L_4) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
	}

IL_0023:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_CopyTo_mD18DBEDAD309E5DFAB965C01F30155253E435A71_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		int32_t L_2 = ___1_arrayIndex;
		if ((((int32_t)L_2) < ((int32_t)0)))
		{
			goto IL_0018;
		}
	}
	{
		int32_t L_3 = ___1_arrayIndex;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_4 = ___0_array;
		NullCheck(L_4);
		if ((((int32_t)L_3) <= ((int32_t)((int32_t)(((RuntimeArray*)L_4)->max_length)))))
		{
			goto IL_002e;
		}
	}

IL_0018:
	{
		int32_t L_5 = ___1_arrayIndex;
		int32_t L_6 = L_5;
		RuntimeObject* L_7 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_6);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_8 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_8, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_8, method);
	}

IL_002e:
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_9 = ___0_array;
		NullCheck(L_9);
		int32_t L_10 = ___1_arrayIndex;
		int32_t L_11 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_9)->max_length)), L_10))) >= ((int32_t)L_11)))
		{
			goto IL_0046;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_12 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_12, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_12, method);
	}

IL_0046:
	{
		V_0 = 0;
		int32_t L_13 = ___1_arrayIndex;
		int32_t L_14 = __this->____size;
		V_1 = ((int32_t)il2cpp_codegen_add(L_13, L_14));
		goto IL_006e;
	}

IL_0053:
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_15 = ___0_array;
		int32_t L_16 = V_1;
		int32_t L_17 = ((int32_t)il2cpp_codegen_subtract(L_16, 1));
		V_1 = L_17;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_18 = __this->____array;
		int32_t L_19 = V_0;
		int32_t L_20 = L_19;
		V_0 = ((int32_t)il2cpp_codegen_add(L_20, 1));
		NullCheck(L_18);
		int32_t L_21 = L_20;
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D L_22 = (L_18)->GetAt(static_cast<il2cpp_array_size_t>(L_21));
		NullCheck(L_15);
		(L_15)->SetAt(static_cast<il2cpp_array_size_t>(L_17), (Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D)L_22);
	}

IL_006e:
	{
		int32_t L_23 = V_0;
		int32_t L_24 = __this->____size;
		if ((((int32_t)L_23) < ((int32_t)L_24)))
		{
			goto IL_0053;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_System_Collections_ICollection_CopyTo_m589A8571C3366E1A6A8EF4F891252F12F431BB6E_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, RuntimeArray* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		RuntimeArray* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		RuntimeArray* L_2 = ___0_array;
		NullCheck(L_2);
		int32_t L_3;
		L_3 = Array_get_Rank_m9383A200A2ECC89ECA44FE5F812ECFB874449C5F(L_2, NULL);
		if ((((int32_t)L_3) == ((int32_t)1)))
		{
			goto IL_0027;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_4 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral967D403A541A1026A83D548E5AD5CA800AD4EFB5)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0027:
	{
		RuntimeArray* L_5 = ___0_array;
		NullCheck(L_5);
		int32_t L_6;
		L_6 = Array_GetLowerBound_m4FB0601E2E8A6304A42E3FC400576DF7B0F084BC(L_5, 0, NULL);
		if (!L_6)
		{
			goto IL_0040;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_7 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral6195D7DA68D16D4985AD1A1B4FD2841A43CDDE70)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_7, method);
	}

IL_0040:
	{
		int32_t L_8 = ___1_arrayIndex;
		if ((((int32_t)L_8) < ((int32_t)0)))
		{
			goto IL_004d;
		}
	}
	{
		int32_t L_9 = ___1_arrayIndex;
		RuntimeArray* L_10 = ___0_array;
		NullCheck(L_10);
		int32_t L_11;
		L_11 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_10, NULL);
		if ((((int32_t)L_9) <= ((int32_t)L_11)))
		{
			goto IL_0063;
		}
	}

IL_004d:
	{
		int32_t L_12 = ___1_arrayIndex;
		int32_t L_13 = L_12;
		RuntimeObject* L_14 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_13);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_15 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_15, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_14, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_15, method);
	}

IL_0063:
	{
		RuntimeArray* L_16 = ___0_array;
		NullCheck(L_16);
		int32_t L_17;
		L_17 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_16, NULL);
		int32_t L_18 = ___1_arrayIndex;
		int32_t L_19 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(L_17, L_18))) >= ((int32_t)L_19)))
		{
			goto IL_007e;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_20 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_20, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_20, method);
	}

IL_007e:
	{
	}
	try
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_21 = __this->____array;
		RuntimeArray* L_22 = ___0_array;
		int32_t L_23 = ___1_arrayIndex;
		int32_t L_24 = __this->____size;
		Array_Copy_mB4904E17BD92E320613A3251C0205E0786B3BF41((RuntimeArray*)L_21, 0, L_22, L_23, L_24, NULL);
		RuntimeArray* L_25 = ___0_array;
		int32_t L_26 = ___1_arrayIndex;
		int32_t L_27 = __this->____size;
		Array_Reverse_mE788006243D34C654D7DDEF13E2D9E7B129AF8AD(L_25, L_26, L_27, NULL);
		goto IL_00b3;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_00a2;
		}
		throw e;
	}

CATCH_00a2:
	{
		ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1* L_28 = ((ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*)IL2CPP_GET_ACTIVE_EXCEPTION(ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*));;
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_29 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_29, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralBD0381A992FDF4F7DA60E5D83689FE7FF6309CB8)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_29, method);
	}

IL_00b3:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Enumerator_t0757D472C179E7C385905619753AC7417A77B40A Stack_1_GetEnumerator_m9ECE8B11D246B323E2A277C12779D2457FF28B95_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t0757D472C179E7C385905619753AC7417A77B40A L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_mF9CC77EDB16F67702CB0692CE3FB4BCA5EACC97E((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_Generic_IEnumerableU3CTU3E_GetEnumerator_m235708CFE73EA3B5FAFDF5E55C46B1E14AD51A9A_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t0757D472C179E7C385905619753AC7417A77B40A L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_mF9CC77EDB16F67702CB0692CE3FB4BCA5EACC97E((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t0757D472C179E7C385905619753AC7417A77B40A L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_IEnumerable_GetEnumerator_mA76D46F5397A86465AF4139789EBFEBF6F630786_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t0757D472C179E7C385905619753AC7417A77B40A L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_mF9CC77EDB16F67702CB0692CE3FB4BCA5EACC97E((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t0757D472C179E7C385905619753AC7417A77B40A L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_TrimExcess_m782E8BF467016E8FDD531F07C1D9A1A2CE571A6D_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_0 = __this->____array;
		NullCheck(L_0);
		V_0 = il2cpp_codegen_cast_double_to_int<int32_t>(((double)il2cpp_codegen_multiply(((double)((int32_t)(((RuntimeArray*)L_0)->max_length))), (0.90000000000000002))));
		int32_t L_1 = __this->____size;
		int32_t L_2 = V_0;
		if ((((int32_t)L_1) >= ((int32_t)L_2)))
		{
			goto IL_003d;
		}
	}
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993** L_3 = (RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993**)(&__this->____array);
		int32_t L_4 = __this->____size;
		Array_Resize_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_m2EC92F2F8F9EEAB25DF52C72C3C856D1ECA0C2A5(L_3, L_4, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_003d:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D Stack_1_Peek_mB2B555E4E7CE625B69E74DA48FC6E95B192A62C2_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_m9BB67CADD6555D0CECCD2652588673184823D4E6(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_4 = V_1;
		int32_t L_5 = V_0;
		NullCheck(L_4);
		int32_t L_6 = L_5;
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D L_7 = (L_4)->GetAt(static_cast<il2cpp_array_size_t>(L_6));
		return L_7;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPeek_m8299A4204F8102E389FD39E72FBB54942C831EC4_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D));
		return (bool)0;
	}

IL_001f:
	{
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D* L_5 = ___0_result;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		*(Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D*)L_5 = L_9;
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D Stack_1_Pop_mBABC10C3A66167B8C5202740BEED900AB0FF3516_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* V_1 = NULL;
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D V_2;
	memset((&V_2), 0, sizeof(V_2));
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D G_B4_0;
	memset((&G_B4_0), 0, sizeof(G_B4_0));
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D G_B3_0;
	memset((&G_B3_0), 0, sizeof(G_B3_0));
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_m9BB67CADD6555D0CECCD2652588673184823D4E6(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		int32_t L_4 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_4, 1));
		int32_t L_5 = V_0;
		__this->____size = L_5;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		G_B4_0 = L_9;
		goto IL_004f;
	}

IL_004f:
	{
		return G_B4_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPop_mAE0AAD4B6AC95E20C8AC3747F20A52CF840D211E_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* V_1 = NULL;
	Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D V_2;
	memset((&V_2), 0, sizeof(V_2));
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D));
		return (bool)0;
	}

IL_001f:
	{
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
		int32_t L_6 = V_0;
		__this->____size = L_6;
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D* L_7 = ___0_result;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_8 = V_1;
		int32_t L_9 = V_0;
		NullCheck(L_8);
		int32_t L_10 = L_9;
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D L_11 = (L_8)->GetAt(static_cast<il2cpp_array_size_t>(L_10));
		*(Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D*)L_7 = L_11;
		goto IL_0058;
	}

IL_0058:
	{
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Push_mE3EEAE2C77DEA8C3F932C6074FAFCFDA80E6D86F_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___0_item, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = L_0;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) < ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_4 = V_1;
		int32_t L_5 = V_0;
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D L_6 = ___0_item;
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D)L_6);
		int32_t L_7 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_7, 1));
		int32_t L_8 = V_0;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_8, 1));
		return;
	}

IL_0034:
	{
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D L_9 = ___0_item;
		Stack_1_PushWithResize_m856B55C10DA17E80AD34FF3B276B07A80685EBFE(__this, L_9, il2cpp_rgctx_method(method->klass->rgctx_data, 16));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_m856B55C10DA17E80AD34FF3B276B07A80685EBFE_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D ___0_item, const RuntimeMethod* method) 
{
	RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993** G_B2_0 = NULL;
	RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993** G_B1_0 = NULL;
	int32_t G_B3_0 = 0;
	RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993** G_B3_1 = NULL;
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993** L_0 = (RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993**)(&__this->____array);
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_1 = __this->____array;
		NullCheck(L_1);
		if (!(((RuntimeArray*)L_1)->max_length))
		{
			G_B2_0 = L_0;
			goto IL_001b;
		}
		G_B1_0 = L_0;
	}
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_2 = __this->____array;
		NullCheck(L_2);
		G_B3_0 = ((int32_t)il2cpp_codegen_multiply(2, ((int32_t)(((RuntimeArray*)L_2)->max_length))));
		G_B3_1 = G_B1_0;
		goto IL_001c;
	}

IL_001b:
	{
		G_B3_0 = 4;
		G_B3_1 = G_B2_0;
	}

IL_001c:
	{
		Array_Resize_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_m2EC92F2F8F9EEAB25DF52C72C3C856D1ECA0C2A5(G_B3_1, G_B3_0, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_3 = __this->____array;
		int32_t L_4 = __this->____size;
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D L_5 = ___0_item;
		NullCheck(L_3);
		(L_3)->SetAt(static_cast<il2cpp_array_size_t>(L_4), (Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D)L_5);
		int32_t L_6 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_6, 1));
		int32_t L_7 = __this->____size;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_7, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* Stack_1_ToArray_mC215FC428117F271A35AF6EE3A5598D5B52586F5_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = __this->____size;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_1;
		L_1 = Array_Empty_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_m85A5CFE26D5D127B03744735ED2B5FC30FCB1D59_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____size;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_3 = (RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993*)(RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_2);
		V_0 = L_3;
		V_1 = 0;
		goto IL_003e;
	}

IL_001e:
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_4 = V_0;
		int32_t L_5 = V_1;
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_6 = __this->____array;
		int32_t L_7 = __this->____size;
		int32_t L_8 = V_1;
		NullCheck(L_6);
		int32_t L_9 = ((int32_t)il2cpp_codegen_subtract(((int32_t)il2cpp_codegen_subtract(L_7, L_8)), 1));
		Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D L_10 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_9));
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (Rect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D)L_10);
		int32_t L_11 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_11, 1));
	}

IL_003e:
	{
		int32_t L_12 = V_1;
		int32_t L_13 = __this->____size;
		if ((((int32_t)L_12) < ((int32_t)L_13)))
		{
			goto IL_001e;
		}
	}
	{
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_14 = V_0;
		return L_14;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m9BB67CADD6555D0CECCD2652588673184823D4E6_gshared (Stack_1_tEEC1F6968B6388E4800894946805617A2E7EDFDA* __this, const RuntimeMethod* method) 
{
	{
		InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB* L_0 = (InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB_il2cpp_TypeInfo_var)));
		InvalidOperationException__ctor_mE4CB6F4712AB6D99A2358FBAE2E052B3EE976162(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral95F0EE30865D503A05F1D329BC3FED0946B65C24)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m991D1F32313E86427FCF566730B675C621B16EBE_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_0;
		L_0 = Array_Empty_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m3AE07A4CEAA2B2E2C2E03472BBF9F79BC799F948_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		__this->____array = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m01E0E3AEA36B56123B096B18EEAD5E14ADBD2D32_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, int32_t ___0_capacity, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		int32_t L_0 = ___0_capacity;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_0020;
		}
	}
	{
		int32_t L_1 = ___0_capacity;
		int32_t L_2 = L_1;
		RuntimeObject* L_3 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_2);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_4 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC37D78082ACFC8DEE7B32D9351C6E433A074FEC7)), L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral38E3DBC7FC353425EF3A98DC8DAC6689AF5FD1BE)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0020:
	{
		int32_t L_5 = ___0_capacity;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_6 = (TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A*)(TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_5);
		__this->____array = L_6;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_6);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m6EF78814DD942FEA46664B7D046E02004827CBC8_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, RuntimeObject* ___0_collection, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		RuntimeObject* L_0 = ___0_collection;
		if (L_0)
		{
			goto IL_0014;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral469F05BE9BB4C7903C353D0EB9F6384C84A48B25)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_0014:
	{
		RuntimeObject* L_2 = ___0_collection;
		int32_t* L_3 = (int32_t*)(&__this->____size);
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_4;
		L_4 = EnumerableHelpers_ToArray_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m9C3FA1318E048265162E9AFA86DCE8A7619D2A70(L_2, L_3, il2cpp_rgctx_method(method->klass->rgctx_data, 5));
		__this->____array = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_4);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_get_Count_m6A7423122DB56788E859B23516004EB0E3D81F24_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_System_Collections_ICollection_get_IsSynchronized_m158F2D015FA1317715473EF182E8B3DC98350B21_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_ICollection_get_SyncRoot_mCFDD0C1A2A7203E22AFC5C2059A9ABFBD4D4F697_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = __this->____syncRoot;
		if (L_0)
		{
			goto IL_001a;
		}
	}
	{
		RuntimeObject** L_1 = (RuntimeObject**)(&__this->____syncRoot);
		RuntimeObject* L_2 = (RuntimeObject*)il2cpp_codegen_object_new(RuntimeObject_il2cpp_TypeInfo_var);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(L_2, NULL);
		RuntimeObject* L_3;
		L_3 = InterlockedCompareExchangeImpl<RuntimeObject*>(L_1, L_2, NULL);
	}

IL_001a:
	{
		RuntimeObject* L_4 = __this->____syncRoot;
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Clear_m7EA7A397FED8408124F3A9C4908C45BC281DDEAC_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	{
		goto IL_0019;
	}

IL_0019:
	{
		__this->____size = 0;
		int32_t L_0 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_0, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_Contains_m7D45199080703E1EA648AF927A94D9C0CC6BA424_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 ___0_item, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		if (!L_0)
		{
			goto IL_0023;
		}
	}
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_1 = __this->____array;
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 L_2 = ___0_item;
		int32_t L_3 = __this->____size;
		int32_t L_4;
		L_4 = Array_LastIndexOf_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m5291A70BF372F7D73477976310C09A5B3BE49C90(L_1, L_2, ((int32_t)il2cpp_codegen_subtract(L_3, 1)), il2cpp_rgctx_method(method->klass->rgctx_data, 8));
		return (bool)((((int32_t)((((int32_t)L_4) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
	}

IL_0023:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_CopyTo_m075D3EA13F4314402CFEDFC52AB3BB5E99091111_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		int32_t L_2 = ___1_arrayIndex;
		if ((((int32_t)L_2) < ((int32_t)0)))
		{
			goto IL_0018;
		}
	}
	{
		int32_t L_3 = ___1_arrayIndex;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_4 = ___0_array;
		NullCheck(L_4);
		if ((((int32_t)L_3) <= ((int32_t)((int32_t)(((RuntimeArray*)L_4)->max_length)))))
		{
			goto IL_002e;
		}
	}

IL_0018:
	{
		int32_t L_5 = ___1_arrayIndex;
		int32_t L_6 = L_5;
		RuntimeObject* L_7 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_6);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_8 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_8, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_8, method);
	}

IL_002e:
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_9 = ___0_array;
		NullCheck(L_9);
		int32_t L_10 = ___1_arrayIndex;
		int32_t L_11 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_9)->max_length)), L_10))) >= ((int32_t)L_11)))
		{
			goto IL_0046;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_12 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_12, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_12, method);
	}

IL_0046:
	{
		V_0 = 0;
		int32_t L_13 = ___1_arrayIndex;
		int32_t L_14 = __this->____size;
		V_1 = ((int32_t)il2cpp_codegen_add(L_13, L_14));
		goto IL_006e;
	}

IL_0053:
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_15 = ___0_array;
		int32_t L_16 = V_1;
		int32_t L_17 = ((int32_t)il2cpp_codegen_subtract(L_16, 1));
		V_1 = L_17;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_18 = __this->____array;
		int32_t L_19 = V_0;
		int32_t L_20 = L_19;
		V_0 = ((int32_t)il2cpp_codegen_add(L_20, 1));
		NullCheck(L_18);
		int32_t L_21 = L_20;
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 L_22 = (L_18)->GetAt(static_cast<il2cpp_array_size_t>(L_21));
		NullCheck(L_15);
		(L_15)->SetAt(static_cast<il2cpp_array_size_t>(L_17), (TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58)L_22);
	}

IL_006e:
	{
		int32_t L_23 = V_0;
		int32_t L_24 = __this->____size;
		if ((((int32_t)L_23) < ((int32_t)L_24)))
		{
			goto IL_0053;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_System_Collections_ICollection_CopyTo_mE9995ABC06E5FF2CE921C696F1C154434A2D301C_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, RuntimeArray* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		RuntimeArray* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		RuntimeArray* L_2 = ___0_array;
		NullCheck(L_2);
		int32_t L_3;
		L_3 = Array_get_Rank_m9383A200A2ECC89ECA44FE5F812ECFB874449C5F(L_2, NULL);
		if ((((int32_t)L_3) == ((int32_t)1)))
		{
			goto IL_0027;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_4 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral967D403A541A1026A83D548E5AD5CA800AD4EFB5)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0027:
	{
		RuntimeArray* L_5 = ___0_array;
		NullCheck(L_5);
		int32_t L_6;
		L_6 = Array_GetLowerBound_m4FB0601E2E8A6304A42E3FC400576DF7B0F084BC(L_5, 0, NULL);
		if (!L_6)
		{
			goto IL_0040;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_7 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral6195D7DA68D16D4985AD1A1B4FD2841A43CDDE70)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_7, method);
	}

IL_0040:
	{
		int32_t L_8 = ___1_arrayIndex;
		if ((((int32_t)L_8) < ((int32_t)0)))
		{
			goto IL_004d;
		}
	}
	{
		int32_t L_9 = ___1_arrayIndex;
		RuntimeArray* L_10 = ___0_array;
		NullCheck(L_10);
		int32_t L_11;
		L_11 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_10, NULL);
		if ((((int32_t)L_9) <= ((int32_t)L_11)))
		{
			goto IL_0063;
		}
	}

IL_004d:
	{
		int32_t L_12 = ___1_arrayIndex;
		int32_t L_13 = L_12;
		RuntimeObject* L_14 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_13);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_15 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_15, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_14, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_15, method);
	}

IL_0063:
	{
		RuntimeArray* L_16 = ___0_array;
		NullCheck(L_16);
		int32_t L_17;
		L_17 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_16, NULL);
		int32_t L_18 = ___1_arrayIndex;
		int32_t L_19 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(L_17, L_18))) >= ((int32_t)L_19)))
		{
			goto IL_007e;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_20 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_20, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_20, method);
	}

IL_007e:
	{
	}
	try
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_21 = __this->____array;
		RuntimeArray* L_22 = ___0_array;
		int32_t L_23 = ___1_arrayIndex;
		int32_t L_24 = __this->____size;
		Array_Copy_mB4904E17BD92E320613A3251C0205E0786B3BF41((RuntimeArray*)L_21, 0, L_22, L_23, L_24, NULL);
		RuntimeArray* L_25 = ___0_array;
		int32_t L_26 = ___1_arrayIndex;
		int32_t L_27 = __this->____size;
		Array_Reverse_mE788006243D34C654D7DDEF13E2D9E7B129AF8AD(L_25, L_26, L_27, NULL);
		goto IL_00b3;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_00a2;
		}
		throw e;
	}

CATCH_00a2:
	{
		ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1* L_28 = ((ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*)IL2CPP_GET_ACTIVE_EXCEPTION(ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*));;
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_29 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_29, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralBD0381A992FDF4F7DA60E5D83689FE7FF6309CB8)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_29, method);
	}

IL_00b3:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Enumerator_tE489CD82CEA07F70C1967F4E911F125AA65FD336 Stack_1_GetEnumerator_mBFA852F5309ACB3BE81AE3F1BD6C6314D2826F76_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_tE489CD82CEA07F70C1967F4E911F125AA65FD336 L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_mF766BF2C1792506A6941458B5EA1940528EAABA7((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_Generic_IEnumerableU3CTU3E_GetEnumerator_m105ADEA0BC43042D14EEEE163CD4507D2D48FD02_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_tE489CD82CEA07F70C1967F4E911F125AA65FD336 L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_mF766BF2C1792506A6941458B5EA1940528EAABA7((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_tE489CD82CEA07F70C1967F4E911F125AA65FD336 L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_IEnumerable_GetEnumerator_mC30B5062A8C59C707ABA68D4A6F584E654E362A7_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_tE489CD82CEA07F70C1967F4E911F125AA65FD336 L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_mF766BF2C1792506A6941458B5EA1940528EAABA7((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_tE489CD82CEA07F70C1967F4E911F125AA65FD336 L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_TrimExcess_m163A19EA55781F928943AA392012D8B53E31EE66_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_0 = __this->____array;
		NullCheck(L_0);
		V_0 = il2cpp_codegen_cast_double_to_int<int32_t>(((double)il2cpp_codegen_multiply(((double)((int32_t)(((RuntimeArray*)L_0)->max_length))), (0.90000000000000002))));
		int32_t L_1 = __this->____size;
		int32_t L_2 = V_0;
		if ((((int32_t)L_1) >= ((int32_t)L_2)))
		{
			goto IL_003d;
		}
	}
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A** L_3 = (TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A**)(&__this->____array);
		int32_t L_4 = __this->____size;
		Array_Resize_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m980E6BCF46D049ECA55F63095E52D73F5C656788(L_3, L_4, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_003d:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 Stack_1_Peek_mEEA697E5B5416609C6CFE11A9F849A68CA2690D7_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_mB0004CD20AD9DFADDF410E929CF49B0FAFE7B78C(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_4 = V_1;
		int32_t L_5 = V_0;
		NullCheck(L_4);
		int32_t L_6 = L_5;
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 L_7 = (L_4)->GetAt(static_cast<il2cpp_array_size_t>(L_6));
		return L_7;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPeek_m481A62BC65E48E5BC966906DA4FF7066EF14D827_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58));
		return (bool)0;
	}

IL_001f:
	{
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58* L_5 = ___0_result;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		*(TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58*)L_5 = L_9;
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 Stack_1_Pop_mD8A2569C08BA8307962EEBB916FD6E1729CDF465_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* V_1 = NULL;
	TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 V_2;
	memset((&V_2), 0, sizeof(V_2));
	TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 G_B4_0;
	memset((&G_B4_0), 0, sizeof(G_B4_0));
	TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 G_B3_0;
	memset((&G_B3_0), 0, sizeof(G_B3_0));
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_mB0004CD20AD9DFADDF410E929CF49B0FAFE7B78C(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		int32_t L_4 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_4, 1));
		int32_t L_5 = V_0;
		__this->____size = L_5;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		G_B4_0 = L_9;
		goto IL_004f;
	}

IL_004f:
	{
		return G_B4_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPop_mA37B5362701BB51704327757C4765C776F3C7F7D_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* V_1 = NULL;
	TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 V_2;
	memset((&V_2), 0, sizeof(V_2));
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58));
		return (bool)0;
	}

IL_001f:
	{
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
		int32_t L_6 = V_0;
		__this->____size = L_6;
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58* L_7 = ___0_result;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_8 = V_1;
		int32_t L_9 = V_0;
		NullCheck(L_8);
		int32_t L_10 = L_9;
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 L_11 = (L_8)->GetAt(static_cast<il2cpp_array_size_t>(L_10));
		*(TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58*)L_7 = L_11;
		goto IL_0058;
	}

IL_0058:
	{
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Push_m2ACE225399F5ADD408CF5B3214D0840CF7CB6922_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 ___0_item, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = L_0;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) < ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_4 = V_1;
		int32_t L_5 = V_0;
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 L_6 = ___0_item;
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58)L_6);
		int32_t L_7 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_7, 1));
		int32_t L_8 = V_0;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_8, 1));
		return;
	}

IL_0034:
	{
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 L_9 = ___0_item;
		Stack_1_PushWithResize_m067591A26AE7959E4430DD0FC3844D46AEAC3E90(__this, L_9, il2cpp_rgctx_method(method->klass->rgctx_data, 16));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_m067591A26AE7959E4430DD0FC3844D46AEAC3E90_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 ___0_item, const RuntimeMethod* method) 
{
	TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A** G_B2_0 = NULL;
	TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A** G_B1_0 = NULL;
	int32_t G_B3_0 = 0;
	TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A** G_B3_1 = NULL;
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A** L_0 = (TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A**)(&__this->____array);
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_1 = __this->____array;
		NullCheck(L_1);
		if (!(((RuntimeArray*)L_1)->max_length))
		{
			G_B2_0 = L_0;
			goto IL_001b;
		}
		G_B1_0 = L_0;
	}
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_2 = __this->____array;
		NullCheck(L_2);
		G_B3_0 = ((int32_t)il2cpp_codegen_multiply(2, ((int32_t)(((RuntimeArray*)L_2)->max_length))));
		G_B3_1 = G_B1_0;
		goto IL_001c;
	}

IL_001b:
	{
		G_B3_0 = 4;
		G_B3_1 = G_B2_0;
	}

IL_001c:
	{
		Array_Resize_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m980E6BCF46D049ECA55F63095E52D73F5C656788(G_B3_1, G_B3_0, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_3 = __this->____array;
		int32_t L_4 = __this->____size;
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 L_5 = ___0_item;
		NullCheck(L_3);
		(L_3)->SetAt(static_cast<il2cpp_array_size_t>(L_4), (TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58)L_5);
		int32_t L_6 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_6, 1));
		int32_t L_7 = __this->____size;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_7, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* Stack_1_ToArray_mE7A5B423FA2DC3F3577533069E907A663296D1D4_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = __this->____size;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_1;
		L_1 = Array_Empty_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m3AE07A4CEAA2B2E2C2E03472BBF9F79BC799F948_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____size;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_3 = (TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A*)(TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_2);
		V_0 = L_3;
		V_1 = 0;
		goto IL_003e;
	}

IL_001e:
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_4 = V_0;
		int32_t L_5 = V_1;
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_6 = __this->____array;
		int32_t L_7 = __this->____size;
		int32_t L_8 = V_1;
		NullCheck(L_6);
		int32_t L_9 = ((int32_t)il2cpp_codegen_subtract(((int32_t)il2cpp_codegen_subtract(L_7, L_8)), 1));
		TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58 L_10 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_9));
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (TextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58)L_10);
		int32_t L_11 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_11, 1));
	}

IL_003e:
	{
		int32_t L_12 = V_1;
		int32_t L_13 = __this->____size;
		if ((((int32_t)L_12) < ((int32_t)L_13)))
		{
			goto IL_001e;
		}
	}
	{
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_14 = V_0;
		return L_14;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_mB0004CD20AD9DFADDF410E929CF49B0FAFE7B78C_gshared (Stack_1_t3B750F239246A65B0BACFB807CBA1961CA8DE0A6* __this, const RuntimeMethod* method) 
{
	{
		InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB* L_0 = (InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB_il2cpp_TypeInfo_var)));
		InvalidOperationException__ctor_mE4CB6F4712AB6D99A2358FBAE2E052B3EE976162(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral95F0EE30865D503A05F1D329BC3FED0946B65C24)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m5C333061E1285AC5B22F556173FBE157EF1A37B7_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0;
		L_0 = ((  __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 0)))(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		__this->____array = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mFDFA9E1123DD1C9E5970223E605B8C973B39721A_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, int32_t ___0_capacity, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		int32_t L_0 = ___0_capacity;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_0020;
		}
	}
	{
		int32_t L_1 = ___0_capacity;
		int32_t L_2 = L_1;
		RuntimeObject* L_3 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_2);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_4 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC37D78082ACFC8DEE7B32D9351C6E433A074FEC7)), L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral38E3DBC7FC353425EF3A98DC8DAC6689AF5FD1BE)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0020:
	{
		int32_t L_5 = ___0_capacity;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_6 = (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*)(__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_5);
		__this->____array = L_6;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_6);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m9A65726EFB4841D3E17FBC7B07073E39A66FECD7_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, RuntimeObject* ___0_collection, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		RuntimeObject* L_0 = ___0_collection;
		if (L_0)
		{
			goto IL_0014;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral469F05BE9BB4C7903C353D0EB9F6384C84A48B25)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_0014:
	{
		RuntimeObject* L_2 = ___0_collection;
		int32_t* L_3 = (int32_t*)(&__this->____size);
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_4;
		L_4 = ((  __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* (*) (RuntimeObject*, int32_t*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 5)))(L_2, L_3, il2cpp_rgctx_method(method->klass->rgctx_data, 5));
		__this->____array = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_4);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_get_Count_mCAD00A69587F884F88C36DE7423C3C329166144E_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_System_Collections_ICollection_get_IsSynchronized_m084F2313417250EF4A79BEE4D6B5C9A3D01B0301_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, const RuntimeMethod* method) 
{
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_ICollection_get_SyncRoot_m18B990E9A22AC2A51EEF9B2A6A96F1F38E765554_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = __this->____syncRoot;
		if (L_0)
		{
			goto IL_001a;
		}
	}
	{
		RuntimeObject** L_1 = (RuntimeObject**)(&__this->____syncRoot);
		RuntimeObject* L_2 = (RuntimeObject*)il2cpp_codegen_object_new(RuntimeObject_il2cpp_TypeInfo_var);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(L_2, NULL);
		RuntimeObject* L_3;
		L_3 = InterlockedCompareExchangeImpl<RuntimeObject*>(L_1, L_2, NULL);
	}

IL_001a:
	{
		RuntimeObject* L_4 = __this->____syncRoot;
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Clear_m7853EC4DED588552A96316078B84CEC9208D84FD_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, const RuntimeMethod* method) 
{
	{
		bool L_0;
		L_0 = il2cpp_codegen_is_reference_or_contains_references(il2cpp_rgctx_method(method->klass->rgctx_data, 6));
		if (!L_0)
		{
			goto IL_0019;
		}
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_1 = __this->____array;
		int32_t L_2 = __this->____size;
		Array_Clear_m50BAA3751899858B097D3FF2ED31F284703FE5CB((RuntimeArray*)L_1, 0, L_2, NULL);
	}

IL_0019:
	{
		__this->____size = 0;
		int32_t L_3 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_3, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_Contains_m7C570B2DD3589F6E3E678A388A4DBD81F167AA53_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, Il2CppFullySharedGenericAny ___0_item, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7));
	const Il2CppFullySharedGenericAny L_2 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	{
		int32_t L_0 = __this->____size;
		if (!L_0)
		{
			goto IL_0023;
		}
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_1 = __this->____array;
		il2cpp_codegen_memcpy(L_2, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7)) ? ___0_item : &___0_item), SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		int32_t L_3 = __this->____size;
		int32_t L_4;
		L_4 = InvokerFuncInvoker3< int32_t, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*, Il2CppFullySharedGenericAny, int32_t >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 8)), il2cpp_rgctx_method(method->klass->rgctx_data, 8), NULL, L_1, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7)) ? L_2: *(void**)L_2), ((int32_t)il2cpp_codegen_subtract(L_3, 1)));
		return (bool)((((int32_t)((((int32_t)L_4) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
	}

IL_0023:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_CopyTo_m3DA819F21880EA78C8679D52CFDD6F7677A95D63_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7));
	const Il2CppFullySharedGenericAny L_22 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		int32_t L_2 = ___1_arrayIndex;
		if ((((int32_t)L_2) < ((int32_t)0)))
		{
			goto IL_0018;
		}
	}
	{
		int32_t L_3 = ___1_arrayIndex;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_4 = ___0_array;
		NullCheck(L_4);
		if ((((int32_t)L_3) <= ((int32_t)((int32_t)(((RuntimeArray*)L_4)->max_length)))))
		{
			goto IL_002e;
		}
	}

IL_0018:
	{
		int32_t L_5 = ___1_arrayIndex;
		int32_t L_6 = L_5;
		RuntimeObject* L_7 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_6);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_8 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_8, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_8, method);
	}

IL_002e:
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_9 = ___0_array;
		NullCheck(L_9);
		int32_t L_10 = ___1_arrayIndex;
		int32_t L_11 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_9)->max_length)), L_10))) >= ((int32_t)L_11)))
		{
			goto IL_0046;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_12 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_12, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_12, method);
	}

IL_0046:
	{
		V_0 = 0;
		int32_t L_13 = ___1_arrayIndex;
		int32_t L_14 = __this->____size;
		V_1 = ((int32_t)il2cpp_codegen_add(L_13, L_14));
		goto IL_006e;
	}

IL_0053:
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_15 = ___0_array;
		int32_t L_16 = V_1;
		int32_t L_17 = ((int32_t)il2cpp_codegen_subtract(L_16, 1));
		V_1 = L_17;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_18 = __this->____array;
		int32_t L_19 = V_0;
		int32_t L_20 = L_19;
		V_0 = ((int32_t)il2cpp_codegen_add(L_20, 1));
		NullCheck(L_18);
		int32_t L_21 = L_20;
		il2cpp_codegen_memcpy(L_22, (L_18)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_21)), SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		NullCheck(L_15);
		il2cpp_codegen_memcpy((L_15)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_17)), L_22, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->klass->rgctx_data, 7), (void**)(L_15)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_17)), (void*)L_22);
	}

IL_006e:
	{
		int32_t L_23 = V_0;
		int32_t L_24 = __this->____size;
		if ((((int32_t)L_23) < ((int32_t)L_24)))
		{
			goto IL_0053;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_System_Collections_ICollection_CopyTo_m159A15659D97C7AAA88168A81B98CD7C0140F1F4_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, RuntimeArray* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		RuntimeArray* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		RuntimeArray* L_2 = ___0_array;
		NullCheck(L_2);
		int32_t L_3;
		L_3 = Array_get_Rank_m9383A200A2ECC89ECA44FE5F812ECFB874449C5F(L_2, NULL);
		if ((((int32_t)L_3) == ((int32_t)1)))
		{
			goto IL_0027;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_4 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral967D403A541A1026A83D548E5AD5CA800AD4EFB5)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0027:
	{
		RuntimeArray* L_5 = ___0_array;
		NullCheck(L_5);
		int32_t L_6;
		L_6 = Array_GetLowerBound_m4FB0601E2E8A6304A42E3FC400576DF7B0F084BC(L_5, 0, NULL);
		if (!L_6)
		{
			goto IL_0040;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_7 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral6195D7DA68D16D4985AD1A1B4FD2841A43CDDE70)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_7, method);
	}

IL_0040:
	{
		int32_t L_8 = ___1_arrayIndex;
		if ((((int32_t)L_8) < ((int32_t)0)))
		{
			goto IL_004d;
		}
	}
	{
		int32_t L_9 = ___1_arrayIndex;
		RuntimeArray* L_10 = ___0_array;
		NullCheck(L_10);
		int32_t L_11;
		L_11 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_10, NULL);
		if ((((int32_t)L_9) <= ((int32_t)L_11)))
		{
			goto IL_0063;
		}
	}

IL_004d:
	{
		int32_t L_12 = ___1_arrayIndex;
		int32_t L_13 = L_12;
		RuntimeObject* L_14 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_13);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_15 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_15, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_14, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_15, method);
	}

IL_0063:
	{
		RuntimeArray* L_16 = ___0_array;
		NullCheck(L_16);
		int32_t L_17;
		L_17 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_16, NULL);
		int32_t L_18 = ___1_arrayIndex;
		int32_t L_19 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(L_17, L_18))) >= ((int32_t)L_19)))
		{
			goto IL_007e;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_20 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_20, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_20, method);
	}

IL_007e:
	{
	}
	try
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_21 = __this->____array;
		RuntimeArray* L_22 = ___0_array;
		int32_t L_23 = ___1_arrayIndex;
		int32_t L_24 = __this->____size;
		Array_Copy_mB4904E17BD92E320613A3251C0205E0786B3BF41((RuntimeArray*)L_21, 0, L_22, L_23, L_24, NULL);
		RuntimeArray* L_25 = ___0_array;
		int32_t L_26 = ___1_arrayIndex;
		int32_t L_27 = __this->____size;
		Array_Reverse_mE788006243D34C654D7DDEF13E2D9E7B129AF8AD(L_25, L_26, L_27, NULL);
		goto IL_00b3;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_00a2;
		}
		throw e;
	}

CATCH_00a2:
	{
		ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1* L_28 = ((ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*)IL2CPP_GET_ACTIVE_EXCEPTION(ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*));;
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_29 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_29, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralBD0381A992FDF4F7DA60E5D83689FE7FF6309CB8)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_29, method);
	}

IL_00b3:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_GetEnumerator_m407739F51246B6C7E996868F09750497FF67EDE7_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41* il2cppRetVal, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_Enumerator_t198FF3485E55915F2A85031CEE0E247740169CD6 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9));
	const Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41 L_0 = alloca(SizeOf_Enumerator_t198FF3485E55915F2A85031CEE0E247740169CD6);
	{
		memset(L_0, 0, SizeOf_Enumerator_t198FF3485E55915F2A85031CEE0E247740169CD6);
		Enumerator__ctor_mD65EF492693E70B41695A031A49F72C7EFA82FCE((Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41*)L_0, __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		il2cpp_codegen_memcpy(il2cppRetVal, L_0, SizeOf_Enumerator_t198FF3485E55915F2A85031CEE0E247740169CD6);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_Generic_IEnumerableU3CTU3E_GetEnumerator_m165EB962DDEDC9EF1558FDB60FA18ACC8A96F9E0_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_Enumerator_t198FF3485E55915F2A85031CEE0E247740169CD6 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9));
	const Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41 L_0 = alloca(SizeOf_Enumerator_t198FF3485E55915F2A85031CEE0E247740169CD6);
	{
		memset(L_0, 0, SizeOf_Enumerator_t198FF3485E55915F2A85031CEE0E247740169CD6);
		Enumerator__ctor_mD65EF492693E70B41695A031A49F72C7EFA82FCE((Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41*)L_0, __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		RuntimeObject* L_1 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), L_0);
		return (RuntimeObject*)L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_IEnumerable_GetEnumerator_m11E86131F4C73FD9931AEAE577F779FE6C81F6CF_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_Enumerator_t198FF3485E55915F2A85031CEE0E247740169CD6 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9));
	const Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41 L_0 = alloca(SizeOf_Enumerator_t198FF3485E55915F2A85031CEE0E247740169CD6);
	{
		memset(L_0, 0, SizeOf_Enumerator_t198FF3485E55915F2A85031CEE0E247740169CD6);
		Enumerator__ctor_mD65EF492693E70B41695A031A49F72C7EFA82FCE((Enumerator_t9C40FA80DC7A4C63469E514386FAB9AE1039DF41*)L_0, __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		RuntimeObject* L_1 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), L_0);
		return (RuntimeObject*)L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_TrimExcess_m22CB1F5D5B042406FBD6DE85DAC60DE6867DBC8E_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0 = __this->____array;
		NullCheck(L_0);
		V_0 = il2cpp_codegen_cast_double_to_int<int32_t>(((double)il2cpp_codegen_multiply(((double)((int32_t)(((RuntimeArray*)L_0)->max_length))), (0.90000000000000002))));
		int32_t L_1 = __this->____size;
		int32_t L_2 = V_0;
		if ((((int32_t)L_1) >= ((int32_t)L_2)))
		{
			goto IL_003d;
		}
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC** L_3 = (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC**)(&__this->____array);
		int32_t L_4 = __this->____size;
		((  void (*) (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC**, int32_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 12)))(L_3, L_4, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_003d:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Peek_m4B2292270D894BE7419111BEF9727F6FE56CE9FE_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7));
	const Il2CppFullySharedGenericAny L_7 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	int32_t V_0 = 0;
	__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		((  void (*) (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 14)))(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_4 = V_1;
		int32_t L_5 = V_0;
		NullCheck(L_4);
		int32_t L_6 = L_5;
		il2cpp_codegen_memcpy(L_7, (L_4)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_6)), SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		il2cpp_codegen_memcpy(il2cppRetVal, L_7, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPeek_m0160C699F9F87C4107A34DC279E999FB7ED4DB1A_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, Il2CppFullySharedGenericAny* ___0_result, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7));
	const Il2CppFullySharedGenericAny L_9 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	int32_t V_0 = 0;
	__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		Il2CppFullySharedGenericAny* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		return (bool)0;
	}

IL_001f:
	{
		Il2CppFullySharedGenericAny* L_5 = ___0_result;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		il2cpp_codegen_memcpy(L_9, (L_6)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_8)), SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_5, L_9, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->klass->rgctx_data, 7), (void**)(Il2CppFullySharedGenericAny*)L_5, (void*)L_9);
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Pop_m8E5FF1B4CFD9ADF4D8A7C4CFF4713C83E163A34A_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7));
	const Il2CppFullySharedGenericAny L_9 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	const Il2CppFullySharedGenericAny L_13 = L_9;
	int32_t V_0 = 0;
	__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* V_1 = NULL;
	Il2CppFullySharedGenericAny V_2 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	memset(V_2, 0, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	Il2CppFullySharedGenericAny G_B4_0 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	memset(G_B4_0, 0, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	Il2CppFullySharedGenericAny G_B3_0 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	memset(G_B3_0, 0, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		((  void (*) (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 14)))(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		int32_t L_4 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_4, 1));
		int32_t L_5 = V_0;
		__this->____size = L_5;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		il2cpp_codegen_memcpy(L_9, (L_6)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_8)), SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		bool L_10;
		L_10 = il2cpp_codegen_is_reference_or_contains_references(il2cpp_rgctx_method(method->klass->rgctx_data, 6));
		if (!L_10)
		{
			il2cpp_codegen_memcpy(G_B4_0, L_9, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
			goto IL_004f;
		}
		il2cpp_codegen_memcpy(G_B3_0, L_9, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_11 = V_1;
		int32_t L_12 = V_0;
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_2, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		il2cpp_codegen_memcpy(L_13, V_2, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		NullCheck(L_11);
		il2cpp_codegen_memcpy((L_11)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_12)), L_13, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->klass->rgctx_data, 7), (void**)(L_11)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_12)), (void*)L_13);
		il2cpp_codegen_memcpy(G_B4_0, G_B3_0, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	}

IL_004f:
	{
		il2cpp_codegen_memcpy(il2cppRetVal, G_B4_0, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPop_m7B8D9D3140C584C905EE9AC45C33C02756FADD51_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, Il2CppFullySharedGenericAny* ___0_result, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7));
	const Il2CppFullySharedGenericAny L_11 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	const Il2CppFullySharedGenericAny L_15 = L_11;
	int32_t V_0 = 0;
	__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* V_1 = NULL;
	Il2CppFullySharedGenericAny V_2 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	memset(V_2, 0, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		Il2CppFullySharedGenericAny* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		return (bool)0;
	}

IL_001f:
	{
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
		int32_t L_6 = V_0;
		__this->____size = L_6;
		Il2CppFullySharedGenericAny* L_7 = ___0_result;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_8 = V_1;
		int32_t L_9 = V_0;
		NullCheck(L_8);
		int32_t L_10 = L_9;
		il2cpp_codegen_memcpy(L_11, (L_8)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_10)), SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		il2cpp_codegen_memcpy((Il2CppFullySharedGenericAny*)L_7, L_11, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->klass->rgctx_data, 7), (void**)(Il2CppFullySharedGenericAny*)L_7, (void*)L_11);
		bool L_12;
		L_12 = il2cpp_codegen_is_reference_or_contains_references(il2cpp_rgctx_method(method->klass->rgctx_data, 6));
		if (!L_12)
		{
			goto IL_0058;
		}
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_13 = V_1;
		int32_t L_14 = V_0;
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_2, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		il2cpp_codegen_memcpy(L_15, V_2, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		NullCheck(L_13);
		il2cpp_codegen_memcpy((L_13)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_14)), L_15, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->klass->rgctx_data, 7), (void**)(L_13)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_14)), (void*)L_15);
	}

IL_0058:
	{
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Push_m072EF82431D7D7E164213D822010141A4C00050D_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, Il2CppFullySharedGenericAny ___0_item, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7));
	const Il2CppFullySharedGenericAny L_6 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	const Il2CppFullySharedGenericAny L_9 = L_6;
	int32_t V_0 = 0;
	__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = L_0;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) < ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_4 = V_1;
		int32_t L_5 = V_0;
		il2cpp_codegen_memcpy(L_6, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7)) ? ___0_item : &___0_item), SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		NullCheck(L_4);
		il2cpp_codegen_memcpy((L_4)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_5)), L_6, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->klass->rgctx_data, 7), (void**)(L_4)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_5)), (void*)L_6);
		int32_t L_7 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_7, 1));
		int32_t L_8 = V_0;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_8, 1));
		return;
	}

IL_0034:
	{
		il2cpp_codegen_memcpy(L_9, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7)) ? ___0_item : &___0_item), SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		InvokerActionInvoker1< Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 16)), il2cpp_rgctx_method(method->klass->rgctx_data, 16), __this, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7)) ? L_9: *(void**)L_9));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_mEBAADD1F1CA3A4CA06A56B93D2B52196A25F7411_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, Il2CppFullySharedGenericAny ___0_item, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7));
	const Il2CppFullySharedGenericAny L_5 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC** G_B2_0 = NULL;
	__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC** G_B1_0 = NULL;
	int32_t G_B3_0 = 0;
	__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC** G_B3_1 = NULL;
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC** L_0 = (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC**)(&__this->____array);
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_1 = __this->____array;
		NullCheck(L_1);
		if (!(((RuntimeArray*)L_1)->max_length))
		{
			G_B2_0 = L_0;
			goto IL_001b;
		}
		G_B1_0 = L_0;
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_2 = __this->____array;
		NullCheck(L_2);
		G_B3_0 = ((int32_t)il2cpp_codegen_multiply(2, ((int32_t)(((RuntimeArray*)L_2)->max_length))));
		G_B3_1 = G_B1_0;
		goto IL_001c;
	}

IL_001b:
	{
		G_B3_0 = 4;
		G_B3_1 = G_B2_0;
	}

IL_001c:
	{
		((  void (*) (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC**, int32_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 12)))(G_B3_1, G_B3_0, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = __this->____array;
		int32_t L_4 = __this->____size;
		il2cpp_codegen_memcpy(L_5, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7)) ? ___0_item : &___0_item), SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		NullCheck(L_3);
		il2cpp_codegen_memcpy((L_3)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_4)), L_5, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->klass->rgctx_data, 7), (void**)(L_3)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_4)), (void*)L_5);
		int32_t L_6 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_6, 1));
		int32_t L_7 = __this->____size;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_7, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* Stack_1_ToArray_mCDCCF0FB12C632AA1B2EBC59302AA2D7574E8B69_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, const RuntimeMethod* method) 
{
	const uint32_t SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 7));
	const Il2CppFullySharedGenericAny L_10 = alloca(SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
	__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = __this->____size;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_1;
		L_1 = ((  __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* (*) (const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->klass->rgctx_data, 0)))(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____size;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*)(__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_2);
		V_0 = L_3;
		V_1 = 0;
		goto IL_003e;
	}

IL_001e:
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_4 = V_0;
		int32_t L_5 = V_1;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_6 = __this->____array;
		int32_t L_7 = __this->____size;
		int32_t L_8 = V_1;
		NullCheck(L_6);
		int32_t L_9 = ((int32_t)il2cpp_codegen_subtract(((int32_t)il2cpp_codegen_subtract(L_7, L_8)), 1));
		il2cpp_codegen_memcpy(L_10, (L_6)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_9)), SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		NullCheck(L_4);
		il2cpp_codegen_memcpy((L_4)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_5)), L_10, SizeOf_T_t96A73EA6CF7DEE94DD5C5BA4D6F3BB998BAB8976);
		Il2CppCodeGenWriteBarrierForClass(il2cpp_rgctx_data(method->klass->rgctx_data, 7), (void**)(L_4)->GetAddressAt(static_cast<il2cpp_array_size_t>(L_5)), (void*)L_10);
		int32_t L_11 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_11, 1));
	}

IL_003e:
	{
		int32_t L_12 = V_1;
		int32_t L_13 = __this->____size;
		if ((((int32_t)L_12) < ((int32_t)L_13)))
		{
			goto IL_001e;
		}
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_14 = V_0;
		return L_14;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m5E2B7F746B04A08A5570995E8E3266FAF43264CC_gshared (Stack_1_tF3E5E7101E929741300A1CF7C159A6ED9B61621A* __this, const RuntimeMethod* method) 
{
	{
		InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB* L_0 = (InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB_il2cpp_TypeInfo_var)));
		InvalidOperationException__ctor_mE4CB6F4712AB6D99A2358FBAE2E052B3EE976162(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral95F0EE30865D503A05F1D329BC3FED0946B65C24)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m14AC177FC4E3DF8CD7C5026E5B98B999842682CE_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_0;
		L_0 = Array_Empty_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m5BEAA64B8E1A4C758EDEA7D45C3177D7D8A6ADCF_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		__this->____array = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_m49337485BC3FE9EC1678C54AC792AF6AFC91FC85_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, int32_t ___0_capacity, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		int32_t L_0 = ___0_capacity;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_0020;
		}
	}
	{
		int32_t L_1 = ___0_capacity;
		int32_t L_2 = L_1;
		RuntimeObject* L_3 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_2);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_4 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC37D78082ACFC8DEE7B32D9351C6E433A074FEC7)), L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral38E3DBC7FC353425EF3A98DC8DAC6689AF5FD1BE)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0020:
	{
		int32_t L_5 = ___0_capacity;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_6 = (MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5*)(MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_5);
		__this->____array = L_6;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_6);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1__ctor_mB4BA90BDB045AAA77B2D6D638919582359A6C21E_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, RuntimeObject* ___0_collection, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2((RuntimeObject*)__this, NULL);
		RuntimeObject* L_0 = ___0_collection;
		if (L_0)
		{
			goto IL_0014;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral469F05BE9BB4C7903C353D0EB9F6384C84A48B25)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_0014:
	{
		RuntimeObject* L_2 = ___0_collection;
		int32_t* L_3 = (int32_t*)(&__this->____size);
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_4;
		L_4 = EnumerableHelpers_ToArray_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m0933456EDFC7CBE2136969B0FD06280E7F0808E7(L_2, L_3, il2cpp_rgctx_method(method->klass->rgctx_data, 5));
		__this->____array = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____array), (void*)L_4);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Stack_1_get_Count_m9327D44FD6E8182FEF61EDCE801261C69CEC6341_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_System_Collections_ICollection_get_IsSynchronized_mB2465842854C21C27D6A4364654F419C2B2F0DD7_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_ICollection_get_SyncRoot_m76547563EE36D89D2846F2DEE731E88B12484591_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&RuntimeObject_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = __this->____syncRoot;
		if (L_0)
		{
			goto IL_001a;
		}
	}
	{
		RuntimeObject** L_1 = (RuntimeObject**)(&__this->____syncRoot);
		RuntimeObject* L_2 = (RuntimeObject*)il2cpp_codegen_object_new(RuntimeObject_il2cpp_TypeInfo_var);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(L_2, NULL);
		RuntimeObject* L_3;
		L_3 = InterlockedCompareExchangeImpl<RuntimeObject*>(L_1, L_2, NULL);
	}

IL_001a:
	{
		RuntimeObject* L_4 = __this->____syncRoot;
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Clear_mF241E1F4339BB39D483A733024684FE09382752B_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	{
		goto IL_0019;
	}

IL_0019:
	{
		__this->____size = 0;
		int32_t L_0 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_0, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_Contains_mBB10FA20CFE8819DD06E71C00DB336E5FF010CF1_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 ___0_item, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		if (!L_0)
		{
			goto IL_0023;
		}
	}
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_1 = __this->____array;
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 L_2 = ___0_item;
		int32_t L_3 = __this->____size;
		int32_t L_4;
		L_4 = Array_LastIndexOf_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_mFFFE5789A9BEF9E2ED789FB50EC9F52BB8954B28(L_1, L_2, ((int32_t)il2cpp_codegen_subtract(L_3, 1)), il2cpp_rgctx_method(method->klass->rgctx_data, 8));
		return (bool)((((int32_t)((((int32_t)L_4) == ((int32_t)(-1)))? 1 : 0)) == ((int32_t)0))? 1 : 0);
	}

IL_0023:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_CopyTo_mF48B5E6C96936CDD859625501DA7AACE0BE86E43_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		int32_t L_2 = ___1_arrayIndex;
		if ((((int32_t)L_2) < ((int32_t)0)))
		{
			goto IL_0018;
		}
	}
	{
		int32_t L_3 = ___1_arrayIndex;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_4 = ___0_array;
		NullCheck(L_4);
		if ((((int32_t)L_3) <= ((int32_t)((int32_t)(((RuntimeArray*)L_4)->max_length)))))
		{
			goto IL_002e;
		}
	}

IL_0018:
	{
		int32_t L_5 = ___1_arrayIndex;
		int32_t L_6 = L_5;
		RuntimeObject* L_7 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_6);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_8 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_8, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_8, method);
	}

IL_002e:
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_9 = ___0_array;
		NullCheck(L_9);
		int32_t L_10 = ___1_arrayIndex;
		int32_t L_11 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(((int32_t)(((RuntimeArray*)L_9)->max_length)), L_10))) >= ((int32_t)L_11)))
		{
			goto IL_0046;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_12 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_12, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_12, method);
	}

IL_0046:
	{
		V_0 = 0;
		int32_t L_13 = ___1_arrayIndex;
		int32_t L_14 = __this->____size;
		V_1 = ((int32_t)il2cpp_codegen_add(L_13, L_14));
		goto IL_006e;
	}

IL_0053:
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_15 = ___0_array;
		int32_t L_16 = V_1;
		int32_t L_17 = ((int32_t)il2cpp_codegen_subtract(L_16, 1));
		V_1 = L_17;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_18 = __this->____array;
		int32_t L_19 = V_0;
		int32_t L_20 = L_19;
		V_0 = ((int32_t)il2cpp_codegen_add(L_20, 1));
		NullCheck(L_18);
		int32_t L_21 = L_20;
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 L_22 = (L_18)->GetAt(static_cast<il2cpp_array_size_t>(L_21));
		NullCheck(L_15);
		(L_15)->SetAt(static_cast<il2cpp_array_size_t>(L_17), (MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253)L_22);
	}

IL_006e:
	{
		int32_t L_23 = V_0;
		int32_t L_24 = __this->____size;
		if ((((int32_t)L_23) < ((int32_t)L_24)))
		{
			goto IL_0053;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_System_Collections_ICollection_CopyTo_m46A2895350AE3379216479DFB6FAE50C68FEBDE7_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, RuntimeArray* ___0_array, int32_t ___1_arrayIndex, const RuntimeMethod* method) 
{
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		RuntimeArray* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		RuntimeArray* L_2 = ___0_array;
		NullCheck(L_2);
		int32_t L_3;
		L_3 = Array_get_Rank_m9383A200A2ECC89ECA44FE5F812ECFB874449C5F(L_2, NULL);
		if ((((int32_t)L_3) == ((int32_t)1)))
		{
			goto IL_0027;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_4 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_4, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral967D403A541A1026A83D548E5AD5CA800AD4EFB5)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, method);
	}

IL_0027:
	{
		RuntimeArray* L_5 = ___0_array;
		NullCheck(L_5);
		int32_t L_6;
		L_6 = Array_GetLowerBound_m4FB0601E2E8A6304A42E3FC400576DF7B0F084BC(L_5, 0, NULL);
		if (!L_6)
		{
			goto IL_0040;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_7 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_7, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral6195D7DA68D16D4985AD1A1B4FD2841A43CDDE70)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_7, method);
	}

IL_0040:
	{
		int32_t L_8 = ___1_arrayIndex;
		if ((((int32_t)L_8) < ((int32_t)0)))
		{
			goto IL_004d;
		}
	}
	{
		int32_t L_9 = ___1_arrayIndex;
		RuntimeArray* L_10 = ___0_array;
		NullCheck(L_10);
		int32_t L_11;
		L_11 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_10, NULL);
		if ((((int32_t)L_9) <= ((int32_t)L_11)))
		{
			goto IL_0063;
		}
	}

IL_004d:
	{
		int32_t L_12 = ___1_arrayIndex;
		int32_t L_13 = L_12;
		RuntimeObject* L_14 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_13);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_15 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_15, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralC00660333703C551EA80371B54D0ADCEB74C33B4)), L_14, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral569FEAE6AEE421BCD8D24F22865E84F808C2A1E4)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_15, method);
	}

IL_0063:
	{
		RuntimeArray* L_16 = ___0_array;
		NullCheck(L_16);
		int32_t L_17;
		L_17 = Array_get_Length_m361285FB7CF44045DC369834D1CD01F72F94EF57(L_16, NULL);
		int32_t L_18 = ___1_arrayIndex;
		int32_t L_19 = __this->____size;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(L_17, L_18))) >= ((int32_t)L_19)))
		{
			goto IL_007e;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_20 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_20, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_20, method);
	}

IL_007e:
	{
	}
	try
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_21 = __this->____array;
		RuntimeArray* L_22 = ___0_array;
		int32_t L_23 = ___1_arrayIndex;
		int32_t L_24 = __this->____size;
		Array_Copy_mB4904E17BD92E320613A3251C0205E0786B3BF41((RuntimeArray*)L_21, 0, L_22, L_23, L_24, NULL);
		RuntimeArray* L_25 = ___0_array;
		int32_t L_26 = ___1_arrayIndex;
		int32_t L_27 = __this->____size;
		Array_Reverse_mE788006243D34C654D7DDEF13E2D9E7B129AF8AD(L_25, L_26, L_27, NULL);
		goto IL_00b3;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_00a2;
		}
		throw e;
	}

CATCH_00a2:
	{
		ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1* L_28 = ((ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*)IL2CPP_GET_ACTIVE_EXCEPTION(ArrayTypeMismatchException_t95F1723A5A166E62D3FBEF9734DEFBF61594F8F1*));;
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_29 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_29, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralBD0381A992FDF4F7DA60E5D83689FE7FF6309CB8)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_29, method);
	}

IL_00b3:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Enumerator_t6B0EB72F34C4614A45F424103A9D11C74439E854 Stack_1_GetEnumerator_m77D9FF1BBFCCC4E8DD470B92AA4322B635C6424B_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t6B0EB72F34C4614A45F424103A9D11C74439E854 L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m14E3BCC9341885A56233EF29D700857A00BBDFC7((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_Generic_IEnumerableU3CTU3E_GetEnumerator_m2DED6FE56C05D93E7936BB02E43607A51E6903EF_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t6B0EB72F34C4614A45F424103A9D11C74439E854 L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m14E3BCC9341885A56233EF29D700857A00BBDFC7((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t6B0EB72F34C4614A45F424103A9D11C74439E854 L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Stack_1_System_Collections_IEnumerable_GetEnumerator_mBEA56CDA62AE7C10641D0E25709E9FEFE4EAA7F2_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	{
		Enumerator_t6B0EB72F34C4614A45F424103A9D11C74439E854 L_0;
		memset((&L_0), 0, sizeof(L_0));
		Enumerator__ctor_m14E3BCC9341885A56233EF29D700857A00BBDFC7((&L_0), __this, il2cpp_rgctx_method(method->klass->rgctx_data, 10));
		Enumerator_t6B0EB72F34C4614A45F424103A9D11C74439E854 L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_rgctx_data_no_init(method->klass->rgctx_data, 9), &L_1);
		return (RuntimeObject*)L_2;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_TrimExcess_m88E7E7688510C957E81BD50B2786384FB4682141_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_0 = __this->____array;
		NullCheck(L_0);
		V_0 = il2cpp_codegen_cast_double_to_int<int32_t>(((double)il2cpp_codegen_multiply(((double)((int32_t)(((RuntimeArray*)L_0)->max_length))), (0.90000000000000002))));
		int32_t L_1 = __this->____size;
		int32_t L_2 = V_0;
		if ((((int32_t)L_1) >= ((int32_t)L_2)))
		{
			goto IL_003d;
		}
	}
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5** L_3 = (MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5**)(&__this->____array);
		int32_t L_4 = __this->____size;
		Array_Resize_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m4E7CB8B6701DF21BD6CA13920546C7869B4F6678(L_3, L_4, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_003d:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 Stack_1_Peek_m4C20B9B8CF100B86A00FC95481714C366DD4E5C7_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_m89DEAB4CFF2C08C84D7B4988FD3A588F243CC50C(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_4 = V_1;
		int32_t L_5 = V_0;
		NullCheck(L_4);
		int32_t L_6 = L_5;
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 L_7 = (L_4)->GetAt(static_cast<il2cpp_array_size_t>(L_6));
		return L_7;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPeek_mAFD8A1173C8C11F514D985A7F7DF90014C146403_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253));
		return (bool)0;
	}

IL_001f:
	{
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253* L_5 = ___0_result;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		*(MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253*)L_5 = L_9;
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 Stack_1_Pop_mC298BD508BB0B8BA70438D9A3ABDF83D59D1A4A8_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* V_1 = NULL;
	MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 V_2;
	memset((&V_2), 0, sizeof(V_2));
	MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 G_B4_0;
	memset((&G_B4_0), 0, sizeof(G_B4_0));
	MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 G_B3_0;
	memset((&G_B3_0), 0, sizeof(G_B3_0));
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001c;
		}
	}
	{
		Stack_1_ThrowForEmptyStack_m89DEAB4CFF2C08C84D7B4988FD3A588F243CC50C(__this, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
	}

IL_001c:
	{
		int32_t L_4 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_4, 1));
		int32_t L_5 = V_0;
		__this->____size = L_5;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_6 = V_1;
		int32_t L_7 = V_0;
		NullCheck(L_6);
		int32_t L_8 = L_7;
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 L_9 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_8));
		G_B4_0 = L_9;
		goto IL_004f;
	}

IL_004f:
	{
		return G_B4_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Stack_1_TryPop_m7105553BAE2CF3626DB835E3A655EF4CC90E6ED6_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253* ___0_result, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* V_1 = NULL;
	MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 V_2;
	memset((&V_2), 0, sizeof(V_2));
	{
		int32_t L_0 = __this->____size;
		V_0 = ((int32_t)il2cpp_codegen_subtract(L_0, 1));
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) >= ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_001f;
		}
	}
	{
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253* L_4 = ___0_result;
		il2cpp_codegen_initobj(L_4, sizeof(MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253));
		return (bool)0;
	}

IL_001f:
	{
		int32_t L_5 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_5, 1));
		int32_t L_6 = V_0;
		__this->____size = L_6;
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253* L_7 = ___0_result;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_8 = V_1;
		int32_t L_9 = V_0;
		NullCheck(L_8);
		int32_t L_10 = L_9;
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 L_11 = (L_8)->GetAt(static_cast<il2cpp_array_size_t>(L_10));
		*(MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253*)L_7 = L_11;
		goto IL_0058;
	}

IL_0058:
	{
		return (bool)1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_Push_mBAF79A1FA0D8C0E9E3658328E92404F6D4703861_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 ___0_item, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* V_1 = NULL;
	{
		int32_t L_0 = __this->____size;
		V_0 = L_0;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_1 = __this->____array;
		V_1 = L_1;
		int32_t L_2 = V_0;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_3 = V_1;
		NullCheck(L_3);
		if ((!(((uint32_t)L_2) < ((uint32_t)((int32_t)(((RuntimeArray*)L_3)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_4 = V_1;
		int32_t L_5 = V_0;
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 L_6 = ___0_item;
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253)L_6);
		int32_t L_7 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_7, 1));
		int32_t L_8 = V_0;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_8, 1));
		return;
	}

IL_0034:
	{
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 L_9 = ___0_item;
		Stack_1_PushWithResize_m046E8452EBCC5731C97F814735F12D1CC6714309(__this, L_9, il2cpp_rgctx_method(method->klass->rgctx_data, 16));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void Stack_1_PushWithResize_m046E8452EBCC5731C97F814735F12D1CC6714309_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 ___0_item, const RuntimeMethod* method) 
{
	MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5** G_B2_0 = NULL;
	MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5** G_B1_0 = NULL;
	int32_t G_B3_0 = 0;
	MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5** G_B3_1 = NULL;
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5** L_0 = (MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5**)(&__this->____array);
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_1 = __this->____array;
		NullCheck(L_1);
		if (!(((RuntimeArray*)L_1)->max_length))
		{
			G_B2_0 = L_0;
			goto IL_001b;
		}
		G_B1_0 = L_0;
	}
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_2 = __this->____array;
		NullCheck(L_2);
		G_B3_0 = ((int32_t)il2cpp_codegen_multiply(2, ((int32_t)(((RuntimeArray*)L_2)->max_length))));
		G_B3_1 = G_B1_0;
		goto IL_001c;
	}

IL_001b:
	{
		G_B3_0 = 4;
		G_B3_1 = G_B2_0;
	}

IL_001c:
	{
		Array_Resize_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m4E7CB8B6701DF21BD6CA13920546C7869B4F6678(G_B3_1, G_B3_0, il2cpp_rgctx_method(method->klass->rgctx_data, 12));
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_3 = __this->____array;
		int32_t L_4 = __this->____size;
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 L_5 = ___0_item;
		NullCheck(L_3);
		(L_3)->SetAt(static_cast<il2cpp_array_size_t>(L_4), (MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253)L_5);
		int32_t L_6 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_6, 1));
		int32_t L_7 = __this->____size;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_7, 1));
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* Stack_1_ToArray_mABCEBC5DC9B72300F73C825FEA492A33D38C553C_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = __this->____size;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_1;
		L_1 = Array_Empty_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m5BEAA64B8E1A4C758EDEA7D45C3177D7D8A6ADCF_inline(il2cpp_rgctx_method(method->klass->rgctx_data, 0));
		return L_1;
	}

IL_000e:
	{
		int32_t L_2 = __this->____size;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_3 = (MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5*)(MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5*)SZArrayNew(il2cpp_rgctx_data(method->klass->rgctx_data, 3), (uint32_t)L_2);
		V_0 = L_3;
		V_1 = 0;
		goto IL_003e;
	}

IL_001e:
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_4 = V_0;
		int32_t L_5 = V_1;
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_6 = __this->____array;
		int32_t L_7 = __this->____size;
		int32_t L_8 = V_1;
		NullCheck(L_6);
		int32_t L_9 = ((int32_t)il2cpp_codegen_subtract(((int32_t)il2cpp_codegen_subtract(L_7, L_8)), 1));
		MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253 L_10 = (L_6)->GetAt(static_cast<il2cpp_array_size_t>(L_9));
		NullCheck(L_4);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(L_5), (MatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253)L_10);
		int32_t L_11 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_11, 1));
	}

IL_003e:
	{
		int32_t L_12 = V_1;
		int32_t L_13 = __this->____size;
		if ((((int32_t)L_12) < ((int32_t)L_13)))
		{
			goto IL_001e;
		}
	}
	{
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_14 = V_0;
		return L_14;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stack_1_ThrowForEmptyStack_m89DEAB4CFF2C08C84D7B4988FD3A588F243CC50C_gshared (Stack_1_tB568ED1852EE70A3EECA2CD66F2AB41DDEC262FA* __this, const RuntimeMethod* method) 
{
	{
		InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB* L_0 = (InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB_il2cpp_TypeInfo_var)));
		InvalidOperationException__ctor_mE4CB6F4712AB6D99A2358FBAE2E052B3EE976162(L_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral95F0EE30865D503A05F1D329BC3FED0946B65C24)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, method);
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint8_t* Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline (RuntimeArray* __this, const RuntimeMethod* method) 
{
	{
		RawData_t37CAF2D3F74B7723974ED7CEEE9B297D8FA64ED0* L_0;
		L_0 = il2cpp_unsafe_as<RawData_t37CAF2D3F74B7723974ED7CEEE9B297D8FA64ED0*>(__this);
		NullCheck(L_0);
		uint8_t* L_1 = (uint8_t*)(&L_0->___Data);
		return L_1;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m87AB3C694F2E4802F14D006F21C020816045285F_gshared_inline (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mA0D85386F3D3AAF59FC429C4A2A9E7CD6B7DCF2A_gshared_inline (ReadOnlySpan_1_t6190994DF094ABDFA6908C2C3FB347457E8E4282* __this, int32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		int32_t* L_0 = ___0_ptr;
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m89B8042F831A4ACF35D15B29B8141AE29CFFDF84_gshared_inline (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, int32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		int32_t* L_0 = ___0_ptr;
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* Array_Empty_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4D53E0E0F90F37AD5DBFD2DC75E52406F90C7ABC_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_0 = ((EmptyArray_1_tE700FA647008891EF64C31436B092B253493667F_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m176441CFA181B7C6097611CC13C24C5ED7F14CFF_gshared_inline (Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316* __this, Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___0_array, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t3C5DB525B005B1AC5A1F3BDD528900C5C7C7D316));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int32_t));
		goto IL_0037;
	}

IL_0037:
	{
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		int32_t* L_4;
		L_4 = il2cpp_unsafe_as_ref<int32_t>(L_3);
		ByReference_1_tDDF129F0BC02430629D5CD253C681112F166BAD4 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m69C26EE24C8AB486BBD48D1BBB32574FB0B5CD91_gshared_inline (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_m3960AA4B8AD179BE83BBC4B4A67B3FB75BD6365A_gshared_inline (ReadOnlySpan_1_t6CE9C0CA1262A820428D86548CAE80352AAA12AC* __this, int64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		int64_t* L_0 = ___0_ptr;
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m180D886432C6474539833ED47A77D2740E7FCD8A_gshared_inline (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, int64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		int64_t* L_0 = ___0_ptr;
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* Array_Empty_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m4569050419CDB52F3B7303ED823142E9C0F12A6C_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_0 = ((EmptyArray_1_t63074FE3C78EACBE204FE82D30DB508A9EB6268A_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m177A8208F7F1C1028420E224BF257E30597C717B_gshared_inline (Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955* __this, Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* ___0_array, const RuntimeMethod* method) 
{
	int64_t V_0 = 0;
	{
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t51050A3216B664417A9CDCC78BD6ED5C1081F955));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int64_t));
		goto IL_0037;
	}

IL_0037:
	{
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		int64_t* L_4;
		L_4 = il2cpp_unsafe_as_ref<int64_t>(L_3);
		ByReference_1_t6A55347AE8EB06C276344D40457E427873BFD1D0 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		Int64U5BU5D_tAEDFCBDB5414E2A140A6F34C0538BF97FCF67A1D* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m0B5336E05EEAAE122DF68A3A82C2D4359A2BE33D_gshared_inline (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mB44468A232EBDBBAC3914B3664064CE336BAF744_gshared_inline (ReadOnlySpan_1_t40ECE3A478A7988D6572F814C5099ACFDE1FDF95* __this, RuntimeObject** ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		RuntimeObject** L_0 = ___0_ptr;
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m143009D8D6D0129C1D86B783427FADE8A0E61936_gshared_inline (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, RuntimeObject** ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		RuntimeObject** L_0 = ___0_ptr;
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ((EmptyArray_1_tDF0DD7256B115243AA6BD5558417387A734240EE_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m8DB171982E2F26182BA531AC18EEAA27D52763EE_gshared_inline (Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2* __this, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___0_array, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	RuntimeObject* V_0 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t3F436092261253E8F2AF9867CA253C3B370766C2));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(RuntimeObject*));
		RuntimeObject* L_1 = V_0;
		if (L_1)
		{
			goto IL_0037;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___0_array;
		NullCheck((RuntimeObject*)L_2);
		Type_t* L_3;
		L_3 = Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3((RuntimeObject*)L_2, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_4 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 3)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_5;
		L_5 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_4, NULL);
		bool L_6;
		L_6 = Type_op_Inequality_m83209C7BB3C05DFBEA3B6199B0BEFE8037301172(L_3, L_5, NULL);
		if (!L_6)
		{
			goto IL_0037;
		}
	}
	{
		ThrowHelper_ThrowArrayTypeMismatchException_m781AD7A903FEA43FAE3137977E6BC5F9BAEBC590(NULL);
	}

IL_0037:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_7 = ___0_array;
		NullCheck((RuntimeArray*)L_7);
		uint8_t* L_8;
		L_8 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_7, NULL);
		RuntimeObject** L_9;
		L_9 = il2cpp_unsafe_as_ref<RuntimeObject*>(L_8);
		ByReference_1_t98B79BFB40A2CA0814BC183B09B4339A5EBF8524 L_10;
		memset((&L_10), 0, sizeof(L_10));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_10), L_9);
		__this->____pointer = L_10;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_11 = ___0_array;
		NullCheck(L_11);
		__this->____length = ((int32_t)(((RuntimeArray*)L_11)->max_length));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mFB84148EFC50BBB0940E474682C1649251F02576_gshared_inline (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mA7A30FB7B805A8FB05709CD7C7D8C29E3F3C60CA_gshared_inline (ReadOnlySpan_1_tDE8983102D42568E6127FA7BE5CE63380CD7A820* __this, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_0 = ___0_ptr;
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mEB88C92980CFEB47188FEFC02935CCC0C1E57C7F_gshared_inline (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_0 = ___0_ptr;
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* Array_Empty_TisQuaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_mCE17AEB484914CF98BCAA9B85E19A2A027EE46C6_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_0 = ((EmptyArray_1_t6D66030EDC48119B7B485C10DBB9F8FB67366E47_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mC41D37A4EC24675FE88E07FC2AD929915407785C_gshared_inline (Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6* __this, QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* ___0_array, const RuntimeMethod* method) 
{
	Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t391B794BE458E4BEE6117AD64D9AC077EDC512C6));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974));
		goto IL_0037;
	}

IL_0037:
	{
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* L_4;
		L_4 = il2cpp_unsafe_as_ref<Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974>(L_3);
		ByReference_1_tE86CE265923F6BE3290C056F8FADF455DF34F6AD L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		QuaternionU5BU5D_t3C088AFB0F3D2763228C9CAB227021C5DC462AF7* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mF7BD3765E86D3ED55FB40E64A3D0F2D90EC0F908_gshared_inline (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_m783D660E0BF03935E536537C3B32F79A7BD0FB42_gshared_inline (ReadOnlySpan_1_tB90CE592448A63B07B56F79364563BD361CF8CDC* __this, int8_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		int8_t* L_0 = ___0_ptr;
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mA974AFDCD35D895EAD5E356146AD6C8682F2738A_gshared_inline (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, int8_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		int8_t* L_0 = ___0_ptr;
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* Array_Empty_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mE1E6DB6377A6DCA206C1AB31B3964386D486F94F_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_0 = ((EmptyArray_1_tB3950DD0CFA703643EB93EDD4FF714B5A085FF8F_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m08998C4090851C62879905634F774B4D4A1B3221_gshared_inline (Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03* __this, SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* ___0_array, const RuntimeMethod* method) 
{
	int8_t V_0 = 0x0;
	{
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t53F7EC6DD1FE372380AC5F8146A9DA9F38A73E03));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int8_t));
		goto IL_0037;
	}

IL_0037:
	{
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		int8_t* L_4;
		L_4 = il2cpp_unsafe_as_ref<int8_t>(L_3);
		ByReference_1_t2D54E89BEF42DA6397FC70A30249F029F8C7FF62 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		SByteU5BU5D_t88116DA68378C3333DB73E7D36C1A06AFAA91913* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mED11128A130F01BC9C5F690BFFAEF38B2283751B_gshared_inline (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mA93F2958FDB8B48F00F57C878275CF739DE6273B_gshared_inline (ReadOnlySpan_1_t9C2C8EDE84088EDC61AADD4CA3C2CDC72D135E3D* __this, float* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		float* L_0 = ___0_ptr;
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m1B5237AB31EC7CA6AB6981E961491177242F4A55_gshared_inline (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, float* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		float* L_0 = ___0_ptr;
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* Array_Empty_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m44F781E90531F7FCDB12BC4290CD4394A887FC06_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_0 = ((EmptyArray_1_t2984B8F74E4B1E6C047125D296C6C06779CA328D_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mA5EF3ABEDD5776174AAEF4D976B58B424F43B275_gshared_inline (Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C* __this, SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* ___0_array, const RuntimeMethod* method) 
{
	float V_0 = 0.0f;
	{
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t7578EBC2679C3216D7A6FA0DAECAF2256A33CF4C));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(float));
		goto IL_0037;
	}

IL_0037:
	{
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		float* L_4;
		L_4 = il2cpp_unsafe_as_ref<float>(L_3);
		ByReference_1_t187A583E432E494CF3EE45BF80D58DB8309BF70A L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		SingleU5BU5D_t89DEFE97BCEDB5857010E79ECE0F52CF6E93B87C* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mD173AF2E3688317C8AB9621F7626A2A34DE8F56B_gshared_inline (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mCBA8EFCAA8102765E34B993A8177EE752D80890F_gshared_inline (ReadOnlySpan_1_tA2EFC117098BD2B38ADBF809AA976D9F3C13654F* __this, uint16_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		uint16_t* L_0 = ___0_ptr;
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m458FB27AA0EB3527A73C9D6305D452A062D2ABC4_gshared_inline (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, uint16_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		uint16_t* L_0 = ___0_ptr;
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* Array_Empty_TisUInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455_mA9FE4AE132DC76B02E8B39B5052CBA643CFF7220_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_0 = ((EmptyArray_1_tA0524EFBAA750B3D1DCF60FE6F2B25DE798675AD_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mC892A665B48BA9CD149DA76F26EA3607C7859792_gshared_inline (Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D* __this, UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* ___0_array, const RuntimeMethod* method) 
{
	uint16_t V_0 = 0;
	{
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t3C28155FFD2FA88D962FCE88A14C370626303A8D));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(uint16_t));
		goto IL_0037;
	}

IL_0037:
	{
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		uint16_t* L_4;
		L_4 = il2cpp_unsafe_as_ref<uint16_t>(L_3);
		ByReference_1_t946C8F453CAF957A5339893AAA7FFF61CC68CECE L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		UInt16U5BU5D_tEB7C42D811D999D2AA815BADC3FCCDD9C67B3F83* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_mC3EBBD1CE9C5025EB30AFDE84FCCCFB3FE794EC5_gshared_inline (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mFEB9E8BCBC125E065C80C12FC6037D87DC6FA2FC_gshared_inline (ReadOnlySpan_1_t57F4BBC957039E8E904443D25F3A78AE60DC94B4* __this, uint32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		uint32_t* L_0 = ___0_ptr;
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m44A796B80A3B7B5E228B0865F02AC548FA1E7567_gshared_inline (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, uint32_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		uint32_t* L_0 = ___0_ptr;
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* Array_Empty_TisUInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B_m8B0170B3C9368D9BDB90A666E2DF5549423C160C_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_0 = ((EmptyArray_1_t77BFDB090CFC6AE661834F0BD4ED43833F4F079D_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m1161A3B3850C22A54C838C62FB009355039C28ED_gshared_inline (Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA* __this, UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* ___0_array, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	{
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_tC47CA3FDABAC657528EC838C3FEB05AA0D4480EA));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(uint32_t));
		goto IL_0037;
	}

IL_0037:
	{
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		uint32_t* L_4;
		L_4 = il2cpp_unsafe_as_ref<uint32_t>(L_3);
		ByReference_1_tFE9AF4BD221B916FA525C43965FD23DB6BE5AC45 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		UInt32U5BU5D_t02FBD658AD156A17574ECE6106CF1FBFCC9807FA* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m62F446B5508B032A22CD1BA21D998626E6B8A5CC_gshared_inline (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mC03446F4F48AEBD20B80591766ACE9CE79048BCE_gshared_inline (ReadOnlySpan_1_tD579EC24AE7548EFE16428C9DC0EE0423C0FDA0F* __this, uint64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		uint64_t* L_0 = ___0_ptr;
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m3C68BA129DD44A3486C6B57A99412947A16855F6_gshared_inline (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, uint64_t* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		uint64_t* L_0 = ___0_ptr;
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* Array_Empty_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m696D3DE2D89DD613C8C0EB15BE627EABEF780255_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_0 = ((EmptyArray_1_tA1B9390A54D6283BFF40643DC2A697126A16E63B_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mC83E2FCB4929477ACCB736B25B9E2F89FF2A07CF_gshared_inline (Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3* __this, UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* ___0_array, const RuntimeMethod* method) 
{
	uint64_t V_0 = 0;
	{
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t2EECFBFD7BF99470FF5E3884902CE388B17BBDD3));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(uint64_t));
		goto IL_0037;
	}

IL_0037:
	{
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		uint64_t* L_4;
		L_4 = il2cpp_unsafe_as_ref<uint64_t>(L_3);
		ByReference_1_tC177AF8388672C5485D5AAD1C913963A8C7B4548 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m556440A32F0A1EB65C95120CEC693B9AC4DFD52B_gshared_inline (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_m682BD3D57C99591BB5B14A415A7F8F4D5B14241F_gshared_inline (ReadOnlySpan_1_t8C2C24B6B10C9EAC8D8A8C92BED569A12615C2AE* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_0 = ___0_ptr;
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m150E0E8C32CF01F22FFC539541D9F6EA05DB6967_gshared_inline (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_0 = ___0_ptr;
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* Array_Empty_TisVector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_mFE64AD477B9A8397B2CCC3FD2565DCA5B2F0A1F6_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_0 = ((EmptyArray_1_tF91FBA61857F9D60B55FD121DEADC9788D1FE016_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mCC34AC618271C9F9C196DA22BA5DB8C6D8AD477D_gshared_inline (Span_1_t460D4E78469192619B0075C15897A6987393AAF9* __this, Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* ___0_array, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t460D4E78469192619B0075C15897A6987393AAF9));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2));
		goto IL_0037;
	}

IL_0037:
	{
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_4;
		L_4 = il2cpp_unsafe_as_ref<Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2>(L_3);
		ByReference_1_t81E3F5607EE2CA97897E6361C9CAE9862DC3DED6 L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_mFA6EE52BCF39100AE30C79E73F0F972182D0CA2A_gshared_inline (ReadOnlySpan_1_tC416A5627E04F69CA2947A2A13F0A1DF096CABAC* __this, Il2CppFullySharedGenericAny* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_ptr;
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mBA868F06359701D9950DEB1B10F52F848E9FF6DA_gshared_inline (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, Il2CppFullySharedGenericAny* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		Il2CppFullySharedGenericAny* L_0 = ___0_ptr;
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m94A95CF4DF158FDF992CC13DA185B637335D84C6_gshared_inline (Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54* __this, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_array, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	const uint32_t SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	Il2CppFullySharedGenericAny V_0 = alloca(SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	memset(V_0, 0, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_tDEB40BEFA77B5E4BB49B058CD3050EEA4DD36C54));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_0, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		il2cpp_codegen_memcpy(L_1, V_0, SizeOf_T_tE7A3A53452487AB9BCB918AC1C5A795CF215388A);
		bool L_2 = il2cpp_codegen_would_box_to_non_null(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 2), L_1);
		if (L_2)
		{
			goto IL_0037;
		}
	}
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = ___0_array;
		NullCheck((RuntimeObject*)L_3);
		Type_t* L_4;
		L_4 = Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3((RuntimeObject*)L_3, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_5 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(InitializedTypeInfo(method->klass)->rgctx_data, 3)) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_6;
		L_6 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_5, NULL);
		bool L_7;
		L_7 = Type_op_Inequality_m83209C7BB3C05DFBEA3B6199B0BEFE8037301172(L_4, L_6, NULL);
		if (!L_7)
		{
			goto IL_0037;
		}
	}
	{
		ThrowHelper_ThrowArrayTypeMismatchException_m781AD7A903FEA43FAE3137977E6BC5F9BAEBC590(NULL);
	}

IL_0037:
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_8 = ___0_array;
		NullCheck((RuntimeArray*)L_8);
		uint8_t* L_9;
		L_9 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_8, NULL);
		Il2CppFullySharedGenericAny* L_10;
		L_10 = il2cpp_unsafe_as_ref<Il2CppFullySharedGenericAny>(L_9);
		ByReference_1_t607C1F3BC28B0E21B969461CDB0720FB01A82141 L_11;
		memset((&L_11), 0, sizeof(L_11));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_11), L_10);
		__this->____pointer = L_11;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_12 = ___0_array;
		NullCheck(L_12);
		__this->____length = ((int32_t)(((RuntimeArray*)L_12)->max_length));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_gshared_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void ReadOnlySpan_1__ctor_m655719167577EE7B24CAAF5A9D0995E0259B6A84_gshared_inline (ReadOnlySpan_1_tEC1E786ADECA91AF68E558734868FBB77E05D964* __this, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_0 = ___0_ptr;
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m1DA3ED274C046791E48A52F415066C7C86B031D1_gshared_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* ___0_ptr, int32_t ___1_length, const RuntimeMethod* method) 
{
	{
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_0 = ___0_ptr;
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_1;
		memset((&L_1), 0, sizeof(L_1));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_1), L_0);
		__this->____pointer = L_1;
		int32_t L_2 = ___1_length;
		__this->____length = L_2;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* Array_Empty_Tisjvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_mD8B08917D1DC7B424036208C74F0A7A6EC83DAAE_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_0 = ((EmptyArray_1_tB610FBC2B87561A97224E274FC1699BCE50B2C60_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_m13E8571165DB8DB1A1909B44B65D4F220890AC8F_gshared_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* ___0_array, const RuntimeMethod* method) 
{
	jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000b;
		}
	}
	{
		il2cpp_codegen_initobj(__this, sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		return;
	}

IL_000b:
	{
		il2cpp_codegen_initobj((&V_0), sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225));
		goto IL_0037;
	}

IL_0037:
	{
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_2 = ___0_array;
		NullCheck((RuntimeArray*)L_2);
		uint8_t* L_3;
		L_3 = Array_GetRawSzArrayData_m2F8F5B2A381AEF971F12866D9C0A6C4FBA59F6BB_inline((RuntimeArray*)L_2, NULL);
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_4;
		L_4 = il2cpp_unsafe_as_ref<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225>(L_3);
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_5;
		memset((&L_5), 0, sizeof(L_5));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_5), L_4);
		__this->____pointer = L_5;
		jvalueU5BU5D_t2232DC04C2D2643358141038962889D92D3B5E6F* L_6 = ___0_array;
		NullCheck(L_6);
		__this->____length = ((int32_t)(((RuntimeArray*)L_6)->max_length));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* SparselyPopulatedArrayAddInfo_1_get_Source_mBA043CE79666AA955D0FE4335ED3B82802E75C86_gshared_inline (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* __this, const RuntimeMethod* method) 
{
	{
		SparselyPopulatedArrayFragment_1_t2D5B093B992E7F6EEEBD059E1065DFEDDBE906FC* L_0 = __this->____source;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t SparselyPopulatedArrayAddInfo_1_get_Index_mCBBF3F010A994612AC94DA417AB8D04A2C92B45A_gshared_inline (SparselyPopulatedArrayAddInfo_1_tC49C525D3AFE80CB49D53650F40C9A49D4CDD19D* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____index;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* Array_Empty_TisRefAsValueType_1_t4782DEE731ECCA9680D45996CB400BD7EA1A0654_m82361CB24EBE3B6C36CB18376DF3A1C7F6EE7228_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		RefAsValueType_1U5BU5D_t0FA919E635B60BCB363285A8066D34DF6E552186* L_0 = ((EmptyArray_1_tD44919E2B605D341F68CD611C3D99481A875DFDF_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* Array_Empty_TisInt32Enum_tCBAC8BA2BFF3A845FA599F303093BBBA374B6F0C_m94E12BB613D748D2EEB9E1ABD961630D2F970385_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		Int32EnumU5BU5D_t87B7DB802810C38016332669039EF42C487A081F* L_0 = ((EmptyArray_1_t0A27D963887A48FA040C718B868C2455F9AD84FA_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* Array_Empty_TisMatrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_m36408D2B54D6A6519FAF5E5AD03F49ECECA09F2E_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		Matrix4x4U5BU5D_t9C51C93425FABC022B506D2DB3A5FA70F9752C4D* L_0 = ((EmptyArray_1_t685951D47CCFA8F90AC66EE22811A68A045FB065_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* Array_Empty_TisRect_tA04E0F8A1830E767F40FB27ECD8D309303571F0D_m85A5CFE26D5D127B03744735ED2B5FC30FCB1D59_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		RectU5BU5D_t83297CB2E61BDF9D27DCB1A3E5C78EBCE9F7C993* L_0 = ((EmptyArray_1_t08704EABC4CA3368B0E6C088FF83A3BDE70484C8_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* Array_Empty_TisTextureId_tFF4B4AAE53408AB10B0B89CCA5F7B50CF2535E58_m3AE07A4CEAA2B2E2C2E03472BBF9F79BC799F948_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		TextureIdU5BU5D_t03041DBB5C72B7E6F5F694A36DC5FEA75432188A* L_0 = ((EmptyArray_1_t21B6219CD4389EFB0E40E69BEC0AE2E54C1AA17A_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* Array_Empty_TisMatchContext_t04110FFA271D89A23BC1918BE657634A7DC06253_m5BEAA64B8E1A4C758EDEA7D45C3177D7D8A6ADCF_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		MatchContextU5BU5D_t49C4E5DA5C1F9B06B211BE6F94AC6BD4D0ABCAE5* L_0 = ((EmptyArray_1_tA32003E1DE04BE9767FD72E5DC4FEA8D1B834759_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
