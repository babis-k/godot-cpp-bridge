using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using GlmSharp;
using Godot;

public partial class NativePluginBindings : Node
{
	public static class Constants
	{
		public const string PlaceHolderLibraryName = "Native";
		public const string WindowsAssemblyName = "native.dll";
		public const string LinuxAssemblyName = "libnative.so";
	}
	
	// This declares the C++ function we will call. C# arrays get marshalled to pointers, so you don't have length info, that's why we provide gridSize so that C++ know how much it can allocate
	[DllImport (Constants.PlaceHolderLibraryName)]
	public static extern void GenerateTerrain([Out] Vector3[] vertices, [Out] Vector3[] normals, [Out] Vector2[] uv, [Out] int[] indices, [In] int gridSize);
	
	// As above, but we're passing a function pointer
	[DllImport (Constants.PlaceHolderLibraryName)]
	static extern void InitBindings(IntPtr debugLog);

	static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		var platformDependentName = GetLibraryName(libraryName);
		platformDependentName = ProjectSettings.GlobalizePath($"res://{platformDependentName}");
		if (!NativeLibrary.TryLoad(platformDependentName, assembly, searchPath, out var handle))
			GD.PrintErr($"Failed to load dynamic library {platformDependentName}");
		return handle;
	}
	
	static string GetLibraryName(string libraryName) => libraryName switch
	{
		Constants.PlaceHolderLibraryName => System.Environment.OSVersion.Platform switch
		{
			PlatformID.Win32NT => Constants.WindowsAssemblyName,
			_ => Constants.LinuxAssemblyName,
		},
		_ => libraryName,
	};

    /*
        C# functions callable from c++
    */
    delegate void DebugLogDelegate(string str);
    
    private static DebugLogDelegate _debugLogDelegate = new DebugLogDelegate(DebugLog);

    ////////////////////////////////////////////////////////////////
    // C# functions callable from C++
    ////////////////////////////////////////////////////////////////

    static void DebugLog(string str) => GD.Print(str);

    public override void _Ready()
    {
	    base._Ready();
	    NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
	    InitBindings(
		    Marshal.GetFunctionPointerForDelegate(_debugLogDelegate)
	    );
    }
}	