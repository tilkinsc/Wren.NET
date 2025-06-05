namespace WrenNET;

using System.Runtime.InteropServices;
using static Wren;

using size_t = UInt64;

/// <summary>
/// A single virtual machine for executing Wren code.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WrenVM : IEquatable<WrenVM>
{
	/// <summary>
	/// The native handle to the WrenVM.
	/// </summary>
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

/// <summary>
/// This lets code outside of the VM hold a persistent reference to an object.
/// After a handle is acquired, and until it is released, this ensures the
/// garbage collector will not reclaim the object it references.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WrenHandle : IEquatable<WrenHandle>
{
	/// <summary>
	/// The native handle to the WrenHandle.
	/// </summary>
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

/// <summary>
/// The result of a loadModuleFn call.
/// <list type="bullet">
/// <item>
/// <description>
/// <c>source</c> is the source code for the module, or NULL if the module is not found.
/// </description>
/// </item>
/// <item>
/// <description>
/// <c>onComplete</c> an optional callback that will be called once Wren is done with the result.
/// </description>
/// </item>
/// </list>
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct WrenLoadModuleResult
{
	[MarshalAs(UnmanagedType.LPStr)]
	public string? source;
	public WrenLoadModuleCompleteFn? onComplete;
	public nuint userData;
}

public enum WrenErrorType
{
	/// <summary>
	/// A syntax or resolution error detected at compile time.
	/// </summary>
	WREN_ERROR_COMPILE,
	/// <summary>
	/// The error message for a runtime error.
	/// </summary>
	WREN_ERROR_RUNTIME,
	/// <summary>
	/// One entry of a runtime error's stack trace.
	/// </summary>
	WREN_ERROR_STACK_TRACE
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WrenForeignClassMethods
{
	/// <summary>
	/// The callback invoked when the foreign object is created.
	/// <para>
	/// This must be provided. Inside the body of this, it must call
	/// <c>wrenSetSlotNewForeign()</c> exactly once.
	/// </para>
	/// </summary>
	public WrenForeignMethodFn allocate;
	
	/// <summary>
	/// The callback invoked when the garbage collector is about to collect a
	/// foreign object's memory.
	/// <para>
	/// This may be `NULL` if the foreign class does not need to finalize.
	/// </para>
	/// </summary>
	public WrenFinalizerFn? finalize;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WrenConfiguration
{
	/// <summary>
	/// The callback Wren will use to allocate, reallocate, and deallocate memory.
	/// If `NULL`, defaults to a built-in function that uses `realloc` and `free`.
	/// </summary>
	public WrenReallocateFn? reallocateFn;
	
	/// <summary>
	/// The callback Wren uses to resolve a module name.
	/// <para>
	/// Some host applications may wish to support "relative" imports, where the
	/// meaning of an import string depends on the module that contains it. To
	/// support that without baking any policy into Wren itself, the VM gives the
	/// host a chance to resolve an import string.
	/// </para>
	/// <para>
	/// Before an import is loaded, it calls this, passing in the name of the
	/// module that contains the import and the import string. The host app can
	/// look at both of those and produce a new "canonical" string that uniquely
	/// identifies the module. This string is then used as the name of the module
	/// going forward. It is what is passed to <c>loadModuleFn</c>, how duplicate
	/// imports of the same module are detected, and how the module is reported in
	/// stack traces.
	/// </para>
	/// <para>
	/// If you leave this function NULL, then the original import string is
	/// treated as the resolved string.
	/// </para>
	/// <para>
	/// If an import cannot be resolved by the embedder, it should return NULL and
	/// Wren will report that as a runtime error.
	/// </para>
	/// <para>
	/// Wren will take ownership of the string you return and free it for you, so
	/// it should be allocated using the same allocation function you provide
	/// above.
	/// </para>
	/// </summary>
	public WrenResolveModuleFn? resolveModuleFn;
	
	/// <summary>
	/// The callback Wren uses to load a module.
	/// <para>
	/// Since Wren does not talk directly to the file system, it relies on the
	/// embedder to physically locate and read the source code for a module. The
	/// first time an import appears, Wren will call this and pass in the name of
	/// the module being imported. The method will return a result, which contains
	/// the source code for that module. Memory for the source is owned by the 
	/// host application, and can be freed using the onComplete callback.
	/// </para>
	/// <para>
	/// This will only be called once for any given module name. Wren caches the
	/// result internally so subsequent imports of the same module will use the
	/// previous source and not call this.
	/// </para>
	/// <para>
	/// If a module with the given name could not be found by the embedder, it
	/// should return NULL and Wren will report that as a runtime error.
	/// </para>
	/// </summary>
	public WrenLoadModuleFn? loadModuleFn;
	
	/// <summary>
	/// The callback Wren uses to find a foreign method and bind it to a class.
	/// <para>
	/// When a foreign method is declared in a class, this will be called with the
	/// foreign method's module, class, and signature when the class body is
	/// executed. It should return a pointer to the foreign function that will be
	/// bound to that method.
	/// </para>
	/// <para>
	/// If the foreign function could not be found, this should return NULL and
	/// Wren will report it as runtime error.
	/// </para>
	/// </summary>
	public WrenBindForeignMethodFn? bindForeignMethodFn;
	
	/// <summary>
	/// The callback Wren uses to find a foreign class and get its foreign methods.
	/// <para>
	/// When a foreign class is declared, this will be called with the class's
	/// module and name when the class body is executed. It should return the
	/// foreign functions uses to allocate and (optionally) finalize the bytes
	/// stored in the foreign object when an instance is created.
	/// </para>
	/// </summary>
	public WrenBindForeignClassFn? bindForeignClassFn;
	
	/// <summary>
	/// The callback Wren uses to display text when `System.print()` or the other
	/// related functions are called.
	/// <para>
	/// If this is `NULL`, Wren discards any printed text.
	/// </para>
	/// </summary>
	public WrenWriteFn? writeFn;
	
	/// <summary>
	/// The callback Wren uses to report errors.
	/// <para>
	/// When an error occurs, this will be called with the module name, line
	/// number, and an error message. If this is `NULL`, Wren doesn't report any
	/// errors.
	/// </para>
	/// </summary>
	public WrenErrorFn? errorFn;
	
	/// <summary>
	/// The number of bytes Wren will allocate before triggering the first garbage collection.
	/// <para>
	/// If zero, defaults to 10MB.
	/// </para>
	/// </summary>
	public size_t initialHeapSize;
	
	/// <summary>
	/// After a collection occurs, the threshold for the next collection is
	/// determined based on the number of bytes remaining in use. This allows Wren
	/// to shrink its memory usage automatically after reclaiming a large amount
	/// of memory.
	/// <para>
	/// This can be used to ensure that the heap does not get too small, which can
	/// in turn lead to a large number of collections afterwards as the heap grows
	/// back to a usable size.
	/// </para>
	/// <para>
	/// If zero, defaults to 1MB.
	/// </para>
	/// </summary>
	public size_t minHeapSize;
	
	/// <summary>
	/// Wren will resize the heap automatically as the number of bytes
	/// remaining in use after a collection changes. This number determines the
	/// amount of additional memory Wren will use after a collection, as a
	/// percentage of the current heap size.
	/// <para>
	/// For example, say that this is 50. After a garbage collection, when there
	/// are 400 bytes of memory still in use, the next collection will be triggered
	/// after a total of 600 bytes are allocated (including the 400 already in
	/// use.)
	/// </para>
	/// <para>
	/// Setting this to a smaller number wastes less memory, but triggers more
	/// frequent garbage collections.
	/// </para>
	/// <para>
	/// If zero, defaults to 50.
	/// </para>
	/// </summary>
	public int heapGrowthPercent;
	
	/// <summary>
	/// User-defined data associated with the VM.
	/// </summary>
	public nuint userData;
}

public enum WrenInterpretResult
{
	WREN_RESULT_SUCCESS,
	WREN_RESULT_COMPILE_ERROR,
	WREN_RESULT_RUNTIME_ERROR
}

/// <summary>
/// The type of an object stored in a slot.
/// <para>
/// This is not necessarily the object's *class*, but instead its low level
/// representation type.
/// </para>
/// </summary>
public enum WrenType
{
	WREN_TYPE_BOOL,
	WREN_TYPE_NUM,
	WREN_TYPE_FOREIGN,
	WREN_TYPE_LIST,
	WREN_TYPE_MAP,
	WREN_TYPE_NULL,
	WREN_TYPE_STRING,
	
	/// <summary>
	/// The object is of a type that isn't accessible by the C# API.
	/// </summary>
	WREN_TYPE_UNKNOWN
}

public static class Wren
{

	private const string DllName = "libwren";
	private const CallingConvention Convention = CallingConvention.Cdecl;
	
	/// <summary>
	/// The Wren semantic version number components.
	/// </summary>
	public const int WREN_VERSION_MAJOR = 0;
	/// <summary>
	/// The Wren semantic version number components.
	/// </summary>
	public const int WREN_VERSION_MINOR = 4;
	/// <summary>
	/// The Wren semantic version number components.
	/// </summary>
	public const int WREN_VERSION_PATCH = 0;
	
	/// <summary>
	/// A human-friendly string representation of the version.
	/// </summary>
	public const string WREN_VERSION_STRING = "0.4.0";
	
	/// <summary>
	/// A monotonically increasing numeric representation of the version number. Use
	/// this if you want to do range checks over versions.
	/// </summary>
	public const int WREN_VERSION_NUMBER = (
		WREN_VERSION_MAJOR * 1000000 +
		WREN_VERSION_MINOR * 1000 +
		WREN_VERSION_PATCH
	);
	
	/// <summary>
	/// A generic allocation function that handles all explicit memory management
	/// used by Wren. It's used like so:
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// To allocate new memory, <c>memory</c> is null and <c>newSize</c> is the desired
	/// size. It should return the allocated memory or null on failure.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// To attempt to grow an existing allocation, <c>memory</c> is the memory, and
	/// <c>newSize</c> is the desired size. It should return <c>memory</c> if it was able
	/// to grow it in place, or a new pointer if it had to move it.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// To shrink memory, <c>memory</c> and <c>newSize</c> are the same as above but it will
	/// always return <c>memory</c>.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// To free memory, <c>memory</c> will be the memory to free and <c>newSize</c> will be
	/// zero. It should return null.
	/// </description>
	/// </item>
	/// </list>
	/// </summary>
	public delegate nuint WrenReallocateFn(nuint memory, size_t newSize, nuint userData);
	
	/// <summary>
	/// A function callable from Wren code, but implemented in C#.
	/// </summary>
	public delegate void WrenForeignMethodFn(WrenVM vm);
	
	/// <summary>
	/// A finalizer function for freeing resources owned by an instance of a foreign
	/// class. Unlike most foreign methods, finalizers do not have access to the VM
	/// and should not interact with it since it's in the middle of a garbage
	/// collection.
	/// </summary>
	public delegate void WrenFinalizerFn(nuint data);
	
	/// <summary>
	/// Gives the host a chance to canonicalize the imported module name,
	/// potentially taking into account the (previously resolved) name of the module
	/// that contains the import. Typically, this is used to implement relative
	/// imports.
	/// </summary>
	public delegate string WrenResolveModuleFn(WrenVM vm, string importer, string name);
	
	/// <summary>
	/// Called after loadModuleFn is called for module <c>name</c>. The original returned result
	/// is handed back to you in this callback, so that you can free memory if appropriate.
	/// </summary>
	public delegate void WrenLoadModuleCompleteFn(WrenVM vm, string name, WrenLoadModuleResult result);
	
	/// <summary>
	/// Loads and returns the source code for the module <c>name</c>.
	/// </summary>
	public delegate WrenLoadModuleResult WrenLoadModuleFn(WrenVM vm, string name);
	
	/// <summary>
	/// Returns a pointer to a foreign method on <c>className</c> in <c>module</c> with <c>signature</c>.
	/// </summary>
	public delegate WrenForeignMethodFn WrenBindForeignMethodFn(WrenVM vm, string module, string className, bool isStatic, string signature);
	
	/// <summary>
	/// Displays a string of text to the user.
	/// </summary>
	public delegate void WrenWriteFn(WrenVM vm, string text);
	
	///<summary>
	/// Reports an error to the user.
	/// <para>
	/// An error detected during compile time is reported by calling this once with
	/// <c>type</c> <c>WREN_ERROR_COMPILE</c>, the resolved name of the <c>module</c> and <c>line</c>
	/// where the error occurs, and the compiler's error <c>message</c>.
	/// </para>
	/// <para>
	/// A runtime error is reported by calling this once with <c>type</c>
	/// <c>WREN_ERROR_RUNTIME</c>, no <c>module</c> or <c>line</c>, and the runtime error's
	/// <c>message</c>. After that, a series of <c>type</c> <c>WREN_ERROR_STACK_TRACE</c> calls are
	/// made for each line in the stack trace. Each of those has the resolved
	/// <c>module</c> and <c>line</c> where the method or function is defined and <c>message</c> is
	/// the name of the method or function.
	/// </para>
	/// </summary>
	public delegate void WrenErrorFn(WrenVM vm, WrenErrorType type, string module, int line, string message);
	
	/// <summary>
	/// Returns a pair of pointers to the foreign methods used to allocate and
	/// finalize the data for instances of <c>className</c> in resolved <c>module</c>.
	/// </summary>
	public delegate WrenForeignClassMethods WrenBindForeignClassFn(WrenVM vm, string module, string className);
	
	/// <summary>
	/// Get the current wren version number.
	/// <para>
	/// Can be used to range checks over versions.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern int wrenGetVersionNumber();
	
	/// <summary>
	/// Initializes <c>configuration</c> with all of its default values.
	/// <para>
	/// Call this before setting the particular fields you care about.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenInitConfiguration(WrenConfiguration configuration);

	/// <summary>
	/// Creates a new Wren virtual machine using the given <c>configuration</c>. Wren
	/// will copy the configuration data, so the argument passed to this can be
	/// freed after calling this. If <c>configuration</c> is `NULL`, uses a default
	/// configuration.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern WrenVM wrenNewVM(WrenConfiguration? configuration);
	
	/// <summary>
	/// Disposes of all resources is use by <c>vm</c>, which was previously created by a
	/// call to <c>wrenNewVM</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenFreeVM(WrenVM vm);
	
	/// <summary>
	/// Immediately run the garbage collector to free unused memory.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenCollectGarbage(WrenVM vm);
	
	/// <summary>
	/// Runs <c>source</c>, a string of Wren source code in a new fiber in <c>vm</c> in the
	/// context of resolved <c>module</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention, CharSet = CharSet.Ansi)]
	public static extern WrenInterpretResult wrenInterpret(WrenVM vm, string module, string source);
	
	/// <summary>
	/// Creates a handle that can be used to invoke a method with <c>signature</c> on
	/// using a receiver and arguments that are set up on the stack.
	/// <para>
	/// This handle can be used repeatedly to directly invoke that method from C
	/// code using <c>wrenCall</c>.
	/// </para>
	/// <para>
	/// When you are done with this handle, it must be released using
	/// <c>wrenReleaseHandle</c>.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention, CharSet = CharSet.Ansi)]
	public static extern WrenHandle wrenMakeCallHandle(WrenVM vm, string signature);
	
	/// <summary>
	/// Calls <c>method</c>, using the receiver and arguments previously set up on the
	/// stack.
	/// <para>
	/// <c>method</c> must have been created by a call to <c>wrenMakeCallHandle</c>. The
	/// arguments to the method must be already on the stack. The receiver should be
	/// in slot 0 with the remaining arguments following it, in order. It is an
	/// error if the number of arguments provided does not match the method's
	/// signature.
	/// </para>
	/// <para>
	/// After this returns, you can access the return value from slot 0 on the stack.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern WrenInterpretResult wrenCall(WrenVM vm, WrenHandle method);
	
	/// <summary>
	/// Releases the reference stored in <c>handle</c>. After calling this, <c>handle</c> can
	/// no longer be used.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenReleaseHandle(WrenVM vm, WrenHandle handle);
	
	/// <summary>
	/// Returns the number of slots available to the current foreign method.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern int wrenGetSlotCount(WrenVM vm);
	
	/// <summary>
	/// Ensures that the foreign method stack has at least <c>numSlots</c> available for
	/// use, growing the stack if needed.
	/// <para>
	/// Does not shrink the stack if it has more than enough slots.
	/// </para>
	/// <para>
	/// It is an error to call this from a finalizer.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenEnsureSlots(WrenVM vm, int numSlots);
	
	/// <summary>
	/// Gets the type of the object in <c>slot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern WrenType wrenGetSlotType(WrenVM vm, int slot);
	
	/// <summary>
	/// Reads a boolean value from <c>slot</c>.
	/// <para>
	/// It is an error to call this if the slot does not contain a boolean value.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern bool wrenGetSlotBool(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention, EntryPoint = "wrenGetSlotBytes")]
	private static extern nuint _wrenGetSlotBytes(WrenVM vm, int slot, ref int length);
	
	/// <summary>
	/// Reads a byte array from <c>slot</c>.
	/// <para>
	/// The memory for the returned string is owned by Wren. You can inspect it
	/// while in your foreign method, but cannot keep a pointer to it after the
	/// function returns, since the garbage collector may reclaim it.
	/// </para>
	/// <para>
	/// Returns a pointer to the first byte of the array and fill <c>length</c> with the
	/// number of bytes in the array.
	/// </para>
	/// <para>
	/// It is an error to call this if the slot does not contain a string.
	/// </para>
	/// </summary>
	public static unsafe ReadOnlySpan<byte> wrenGetSlotBytes(WrenVM vm, int slot)
	{
		int length = 0;
		nuint ptr = _wrenGetSlotBytes(vm, slot, ref length);
		return new ReadOnlySpan<byte>((void*)ptr, length);
	}
	
	/// <summary>
	/// Reads a number from <c>slot</c>.
	/// <para>
	/// It is an error to call this if the slot does not contain a number.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern double wrenGetSlotDouble(WrenVM vm, int slot);
	
	/// <summary>
	/// Reads a foreign object from <c>slot</c> and returns a pointer to the foreign data
	/// stored with it.
	/// <para>
	/// It is an error to call this if the slot does not contain an instance of a
	/// foreign class.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern nuint wrenGetSlotForeign(WrenVM vm, int slot);
	
	[DllImport(DllName, CallingConvention = Convention, EntryPoint = "wrenGetSlotString")]
	private static extern nint _wrenGetSlotString(WrenVM vm, int slot);
	
	/// <summary>
	/// Reads a string from <c>slot</c>.
	/// <para>
	/// The memory for the returned string is owned by Wren. You can inspect it
	/// while in your foreign method, but cannot keep a pointer to it after the
	/// function returns, since the garbage collector may reclaim it.
	/// </para>
	/// <para>
	/// It is an error to call this if the slot does not contain a string.
	/// </para>
	/// </summary>
	public static string wrenGetSlotString(WrenVM vm, int slot)
	{
		return Marshal.PtrToStringAnsi(_wrenGetSlotString(vm, slot))!;
	}
	
	/// <summary>
	/// Creates a handle for the value stored in <c>slot</c>.
	/// <para>
	/// This will prevent the object that is referred to from being garbage collected
	/// until the handle is released by calling <c>wrenReleaseHandle()</c>.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern WrenHandle wrenGetSlotHandle(WrenVM vm, int slot);
	
	/// <summary>
	/// Stores the boolean <c>value</c> in <c>slot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotBool(WrenVM vm, int slot, bool value);
	
	[DllImport(DllName, CallingConvention = Convention, EntryPoint = "wrenSetSlotBytes")]
	private static extern void _wrenSetSlotBytes(WrenVM vm, int slot, nuint bytes, size_t length);
	
	/// <summary>
	/// Stores the array <c>length</c> of <c>bytes</c> in <c>slot</c>.
	/// <para>
	/// The bytes are copied to a new string within Wren's heap, so you can free
	/// memory used by them after this is called.
	/// </para>
	/// </summary>
	public static unsafe void wrenSetSlotBytes(WrenVM vm, int slot, Span<byte> bytes)
	{
		fixed (byte* bytes_ptr = bytes)
		{
			_wrenSetSlotBytes(vm, slot, (nuint)bytes_ptr, (size_t)bytes.Length);
		}
	}
	
	/// <summary>
	/// Stores the numeric <c>value</c> in <c>slot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotDouble(WrenVM vm, int slot, double value);
	
	/// <summary>
	/// Creates a new instance of the foreign class stored in <c>classSlot</c> with <c>size</c>
	/// bytes of raw storage and places the resulting object in <c>slot</c>.
	/// <para>
	/// This does not invoke the foreign class's constructor on the new instance. If
	/// you need that to happen, call the constructor from Wren, which will then
	/// call the allocator foreign method. In there, call this to create the object
	/// and then the constructor will be invoked when the allocator returns.
	/// </para>
	/// <para>
	/// Returns a pointer to the foreign object's data.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern nuint wrenSetSlotNewForeign(WrenVM vm, int slot, int classSlot, size_t size);
	
	/// <summary>
	/// Stores a new empty list in <c>slot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotNewList(WrenVM vm, int slot);
	
	/// <summary>
	/// Stores a new empty map in <c>slot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotNewMap(WrenVM vm, int slot);
	
	/// <summary>
	/// Stores null in <c>slot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotNull(WrenVM vm, int slot);
	
	/// <summary>
	/// Stores the string <c>text</c> in <c>slot</c>.
	/// <para>
	/// The <c>text</c> is copied to a new string within Wren's heap, so you can free
	/// memory used by it after this is called. The length is calculated using
	/// <c>strlen()</c>. If the string may contain any null bytes in the middle, then you
	/// should use <c>wrenSetSlotBytes()</c> instead.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention, CharSet = CharSet.Ansi)]
	public static extern void wrenSetSlotString(WrenVM vm, int slot, string text);
	
	/// <summary>
	/// Stores the value captured in <c>handle</c> in <c>slot</c>.
	/// <para>
	/// This does not release the handle for the value.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetSlotHandle(WrenVM vm, int slot, WrenHandle handle);
	
	/// <summary>
	/// Returns the number of elements in the list stored in <c>slot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern int wrenGetListCount(WrenVM vm, int slot);
	
	/// <summary>
	/// Reads element <c>index</c> from the list in <c>listSlot</c> and stores it in
	/// <c>elementSlot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenGetListElement(WrenVM vm, int listSlot, int index, int elementSlot);
	
	/// <summary>
	/// Sets the value stored at <c>index</c> in the list at <c>listSlot</c>, 
	/// to the value from <c>elementSlot</c>. 
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetListElement(WrenVM vm, int listSlot, int index, int elementSlot);
	
	/// <summary>
	/// Takes the value stored at <c>elementSlot</c> and inserts it into the list stored
	/// at <c>listSlot</c> at <c>index</c>.
	/// <para>
	/// As in Wren, negative indexes can be used to insert from the end. To append
	/// an element, use `-1` for the index.
	/// </para>
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenInsertInList(WrenVM vm, int listSlot, int index, int elementSlot);
	
	/// <summary>
	/// Returns the number of entries in the map stored in <c>slot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern int wrenGetMapCount(WrenVM vm, int slot);
	
	/// <summary>
	/// Returns true if the key in <c>keySlot</c> is found in the map placed in <c>mapSlot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern bool wrenGetMapContainsKey(WrenVM vm, int mapSlot, int keySlot);
	
	/// <summary>
	/// Retrieves a value with the key in <c>keySlot</c> from the map in <c>mapSlot</c> and
	/// stores it in <c>valueSlot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenGetMapValue(WrenVM vm, int mapSlot, int keySlot, int valueSlot);
	
	/// <summary>
	/// Takes the value stored at <c>valueSlot</c> and inserts it into the map stored
	/// at <c>mapSlot</c> with key <c>keySlot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetMapValue(WrenVM vm, int mapSlot, int keySlot, int valueSlot);
	
	/// <summary>
	/// Removes a value from the map in <c>mapSlot</c>, with the key from <c>keySlot</c>,
	/// and place it in <c>removedValueSlot</c>. If not found, <c>removedValueSlot</c> is
	/// set to null, the same behaviour as the Wren Map API.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenRemoveMapValue(WrenVM vm, int mapSlot, int keySlot, int removedValueSlot);
	
	/// <summary>
	/// Looks up the top level variable with <c>name</c> in resolved <c>module</c> and stores
	/// it in <c>slot</c>.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention, CharSet = CharSet.Ansi)]
	public static extern void wrenGetVariable(WrenVM vm, string module, string name, int slot);
	
	/// <summary>
	/// Looks up the top level variable with <c>name</c> in resolved <c>module</c>, 
	/// returns false if not found. The module must be imported at the time, 
	/// use wrenHasModule to ensure that before calling.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention, CharSet = CharSet.Ansi)]
	public static extern bool wrenHasVariable(WrenVM vm, string module, string name);
	
	/// <summary>
	/// Returns true if <c>module</c> has been imported/resolved before, false if not.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention, CharSet = CharSet.Ansi)]
	public static extern bool wrenHasModule(WrenVM vm, string module);
	
	/// <summary>
	/// Sets the current fiber to be aborted, and uses the value in <c>slot</c> as the
	/// runtime error object.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenAbortFiber(WrenVM vm, int slot);
	
	/// <summary>
	/// Returns the user data associated with the WrenVM.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern nuint wrenGetUserData(WrenVM vm);
	
	/// <summary>
	/// Sets user data associated with the WrenVM.
	/// </summary>
	[DllImport(DllName, CallingConvention = Convention)]
	public static extern void wrenSetUserData(WrenVM vm, nuint userData);
}
