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
	
	[DllImport (Constants.PlaceHolderLibraryName)]
	public static extern void GenerateSomeGeometry(Vector3[] vertices, Vector3[] normals, Vector2[] uv, int maxVerts, int[] indices, int maxTris);

	static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		var platformDependentName = GetLibraryName(libraryName);
		//GD.PrintErr($"Attempting to load dynamic library {platformDependentName}");
		IntPtr handle=IntPtr.Zero;
		if (!NativeLibrary.TryLoad(platformDependentName, assembly, DllImportSearchPath.ApplicationDirectory, out handle))
		{
			//GD.Print($"Failed to load dynamic library {platformDependentName} -- retrying with path globalization");
			platformDependentName = ProjectSettings.GlobalizePath($"res://{platformDependentName}");
			if (!NativeLibrary.TryLoad(platformDependentName, assembly, searchPath, out handle))
				GD.PrintErr($"Failed AGAIN to load dynamic library {platformDependentName}");
		}
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
	
	[DllImport (Constants.PlaceHolderLibraryName)]
	static extern void InitBindings(IntPtr debugLog);

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