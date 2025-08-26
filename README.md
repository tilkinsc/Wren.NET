
# Wren.NET
![Logo](https://raw.githubusercontent.com/tilkinsc/Wren.NET/main/Wren.NET.Logo.png)  

C# .NET Core 9.0
Lua.NET contains full bindings to Wren

https://github.com/tilkinsc/Wren.NET  
Copyright © Cody Tilkins 2025 MIT License  

Notable Changes:
* 2.0.0 - wrenInitConfiguration requires a pass by ref, Linux fix
* 1.1.0 - Nullable structs and function arguments
* 1.0.2 - Doc Comments
* 1.0.1 - Correct native string (for non-windows users)
* 1.0.0 - Initial Release

Example Usage:
```cs
using System.Text;
using WrenNET;
using static WrenNET.Wren;

class Program
{
	public static string RootDir = Directory.GetCurrentDirectory();
	
	public static void WriteFn(WrenVM vm, string text) =>
		Console.Write(text);

	public static string? MainModule = string.Empty;
	public static readonly Dictionary<string, string> ModulePaths = [];
	
	public static void ErrorFn(WrenVM vm, WrenErrorType errorType, string module, int line, string msg)
	{
		Console.WriteLine($"Module: {module}");
		if (string.IsNullOrEmpty(module))
		{
			module = MainModule!;
		}
		else
		{
			ModulePaths.TryGetValue(module, out var pathHint);
			if (!string.IsNullOrEmpty(pathHint))
			{
				module = pathHint;
			}
		}
		switch (errorType)
		{
			case WrenErrorType.WREN_ERROR_COMPILE:
				Console.WriteLine($"{module}:{line}:1: [error] {msg}");
				break;
			case WrenErrorType.WREN_ERROR_STACK_TRACE:
				Console.WriteLine($"{module}:{line}:1: [trace] {msg}");
				break;
			case WrenErrorType.WREN_ERROR_RUNTIME:
				Console.WriteLine($"[runtime error] {msg}");
				break;
		}
	}
	
	public static WrenLoadModuleResult LoadModuleFn(WrenVM vm, string module)
	{
		string path = Path.Combine(RootDir, module.Replace('.', Path.DirectorySeparatorChar) + ".wren");
		if (!File.Exists(path))
		{
			return new WrenLoadModuleResult { source = null };
		}

		ModulePaths[module] = Path.GetFullPath(path);
		string source = File.ReadAllText(path, Encoding.UTF8);
		return new WrenLoadModuleResult { source = source };
	}
	
	public static void Main(string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine("Usage: dotnet run <script.wren>");
			return;
		}
		
		string entryPath = Path.GetFullPath(args[0]);
		if (!File.Exists(entryPath))
		{
			Console.WriteLine($"File not found: {entryPath}");
			return;
		}
		
		RootDir = Path.GetDirectoryName(entryPath)!;
		
		string entrySource = File.ReadAllText(entryPath, Encoding.UTF8);
		string moduleName = Path.GetFileName(entryPath);
		MainModule = moduleName;
		
		WrenConfiguration config = new();
		wrenInitConfiguration(config);
		config.writeFn = WriteFn;
		config.errorFn = ErrorFn;
		config.loadModuleFn = LoadModuleFn;
		
		WrenVM vm = wrenNewVM(config);
		
		WrenInterpretResult result = wrenInterpret(vm, moduleName, entrySource);
		Console.WriteLine(result.ToString());
		
		wrenFreeVM(vm);
	}
}

```
