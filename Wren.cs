namespace WrenNET;

using System.Runtime.InteropServices;
using static Wren;

using size_t = UInt64;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WrenVM : IEquatable<WrenVM>
{
	public nuint Handle;
	
	public readonly bool IsNull => Handle == 0;
	public readonly bool IsNotNull => Handle != 0;
	
	public static bool operator !(WrenVM vm) => vm.Handle == 0;
	public static bool operator ==(WrenVM vm1, WrenVM vm2) => vm1.Handle == vm2.Handle;
	public static bool operator ==(WrenVM vm1, int handle) => vm1.Handle == (nuint) handle;
	public static bool operator !=(WrenVM vm1, WrenVM vm2) => vm1.Handle != vm2.Handle;
	public static bool operator !=(WrenVM vm1, int handle) => vm1.Handle != (nuint) handle;
	
	public readonly bool Equals(WrenVM other) => Handle == other.Handle;
	public override readonly bool Equals(object? other) => other is WrenVM state && Equals(state);
	public override readonly int GetHashCode() => Handle.GetHashCode();
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WrenHandle : IEquatable<WrenHandle>
{
	public nuint Handle;
	
	public readonly bool IsNull => Handle == 0;
	public readonly bool IsNotNull => Handle != 0;
	
	public static bool operator !(WrenHandle vm) => vm.Handle == 0;
	public static bool operator ==(WrenHandle vm1, WrenHandle vm2) => vm1.Handle == vm2.Handle;
	public static bool operator ==(WrenHandle vm1, int handle) => vm1.Handle == (nuint) handle;
	public static bool operator !=(WrenHandle vm1, WrenHandle vm2) => vm1.Handle != vm2.Handle;
	public static bool operator !=(WrenHandle vm1, int handle) => vm1.Handle != (nuint) handle;
	
	public readonly bool Equals(WrenHandle other) => Handle == other.Handle;
	public override readonly bool Equals(object? other) => other is WrenHandle state && Equals(state);
	public override readonly int GetHashCode() => Handle.GetHashCode();
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct WrenLoadModuleResult
{
	[MarshalAs(UnmanagedType.LPStr)]
	public string source;
	public WrenLoadModuleCompleteFn onComplete;
	public nuint userData;
}

public enum WrenErrorType
{
	WREN_ERROR_COMPILE,
	WREN_ERROR_RUNTIME,
	WREN_ERROR_STACK_TRACE
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WrenForeignClassMethods
{
	public WrenForeignMethodFn allocate;
	public WrenFinalizerFn finalize;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WrenConfiguration
{
	public WrenReallocateFn reallocateFn;
	public WrenResolveModuleFn resolveModuleFn;
	public WrenLoadModuleFn loadModuleFn;
	public WrenBindForeignMethodFn bindForeignMethodFn;
	public WrenBindForeignClassFn bindForeignClassFn;
	public WrenWriteFn writeFn;
	public WrenErrorFn errorFn;
	public size_t initialHeapSize;
	public size_t minHeapSize;
	public int heapGrowthPercent;
	public nuint userData;
}

public enum WrenInterpretResult
{
	WREN_RESULT_SUCCESS,
	WREN_RESULT_COMPILE_ERROR,
	WREN_RESULT_RUNTIME_ERROR
}

public enum WrenType
{
	WREN_TYPE_BOOL,
	WREN_TYPE_NUM,
	WREN_TYPE_FOREIGN,
	WREN_TYPE_LIST,
	WREN_TYPE_MAP,
	WREN_TYPE_NULL,
	WREN_TYPE_STRING,
	WREN_TYPE_UNKNOWN
}

public static class Wren
{
	
	private const string DllName = "runtimes/win-x64/native/libwren.dll";
	private const CallingConvention Convention = CallingConvention.Cdecl;
	
	public const int WREN_VERSION_MAJOR = 0;
	public const int WREN_VERSION_MINOR = 4;
	public const int WREN_VERSION_PATCH = 0;
	
	public const string WREN_VERSION_STRING = "0.4.0";
	
	public const int WREN_VERSION_NUMBER = (
		WREN_VERSION_MAJOR * 1000000 +
		WREN_VERSION_MINOR * 1000 +
		WREN_VERSION_PATCH
	);
	
	public delegate nuint WrenReallocateFn(nuint memory, size_t newSize, nuint userData);
	public delegate void WrenForeignMethodFn(WrenVM vm);
	public delegate void WrenFinalizerFn(nuint data);
	public delegate string WrenResolveModuleFn(WrenVM vm, string importer, string name);
	public delegate void WrenLoadModuleCompleteFn(WrenVM vm, string name, WrenLoadModuleResult result);
	public delegate WrenLoadModuleResult WrenLoadModuleFn(WrenVM vm, string name);
	public delegate WrenForeignMethodFn WrenBindForeignMethodFn(WrenVM vm, string module, string className, bool isStatic, string signature);
	public delegate void WrenWriteFn(WrenVM vm, string text);
	public delegate void WrenErrorFn(WrenVM vm, WrenErrorType type, string module, int line, string message);
	public delegate WrenForeignClassMethods WrenBindForeignClassFn(WrenVM vm, string module, string className);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern int wrenGetVersionNumber();
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenInitConfiguration(WrenConfiguration configuration);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern WrenVM wrenNewVM(WrenConfiguration configuration);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenFreeVM(WrenVM vm);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenCollectGarbage(WrenVM vm);
	
	[DllImport(DllName, CallingConvention = Convention, CharSet = CharSet.Ansi)]
	public static extern WrenInterpretResult wrenInterpret(WrenVM vm, string module, string source);
	
	[DllImport(DllName, CallingConvention = Convention, CharSet = CharSet.Ansi)]
	public static extern WrenHandle wrenMakeCallHandle(WrenVM vm, string signature);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern WrenInterpretResult wrenCall(WrenVM vm, WrenHandle method);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenReleaseHandle(WrenVM vm, WrenHandle handle);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern int wrenGetSlotCount(WrenVM vm);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenEnsureSlots(WrenVM vm, int numSlots);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern WrenType wrenGetSlotType(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern bool wrenGetSlotBool(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention, EntryPoint = "wrenGetSlotBytes")]
	private static extern nuint _wrenGetSlotBytes(WrenVM vm, int slot, ref int length);
	public static unsafe ReadOnlySpan<byte> wrenGetSlotBytes(WrenVM vm, int slot)
	{
		int length = 0;
		nuint ptr = _wrenGetSlotBytes(vm, slot, ref length);
		return new ReadOnlySpan<byte>((void*) ptr, length);
	}
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern double wrenGetSlotDouble(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern nuint wrenGetSlotForeign(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention, EntryPoint = "wrenGetSlotString")]
	private static extern nint _wrenGetSlotString(WrenVM vm, int slot);
	public static string wrenGetSlotString(WrenVM vm, int slot)
	{
		return Marshal.PtrToStringAnsi(_wrenGetSlotString(vm, slot))!;
	}
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern WrenHandle wrenGetSlotHandle(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotBool(WrenVM vm, int slot, bool value);
	
	[DllImport(DllName, CallingConvention = Convention, EntryPoint = "wrenSetSlotBytes")]
	private static extern void _wrenSetSlotBytes(WrenVM vm, int slot, nuint bytes, size_t length);
	public static unsafe void wrenSetSlotBytes(WrenVM vm, int slot, Span<byte> bytes)
	{
		fixed (byte* bytes_ptr = bytes)
		{
			_wrenSetSlotBytes(vm, slot, (nuint) bytes_ptr, (size_t) bytes.Length);
		}
	}
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotDouble(WrenVM vm, int slot, double value);
	
	// TODO: is this the best way to leak access to this memory?
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern nuint wrenSetSlotNewForeign(WrenVM vm, int slot, int classSlot, size_t size);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotNewList(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotNewMap(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotNull(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotString(WrenVM vm, int slot, string text);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotHandle(WrenVM vm, int slot, WrenHandle handle);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern int wrenGetListCount(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenGetListElement(WrenVM vm, int listSlot, int index, int elementSlot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetListElement(WrenVM vm, int listSlot, int index, int elementSlot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenInsertInList(WrenVM vm, int listSlot, int index, int elementSlot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern int wrenGetMapCount(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern bool wrenGetMapContainsKey(WrenVM vm, int mapSlot, int keySlot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenGetMapValue(WrenVM vm, int mapSlot, int keySlot, int valueSlot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetMapValue(WrenVM vm, int mapSlot, int keySlot, int valueSlot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenRemoveMapValue(WrenVM vm, int mapSlot, int keySlot, int removedValueSlot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenGetVariable(WrenVM vm, string module, string name, int slot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern bool wrenHasVariable(WrenVM vm, string module, string name);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern bool wrenHasModule(WrenVM vm, string module);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenAbortFiber(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern nuint wrenGetUserData(WrenVM vm);
	
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetUserData(WrenVM vm, nuint userData);
}
