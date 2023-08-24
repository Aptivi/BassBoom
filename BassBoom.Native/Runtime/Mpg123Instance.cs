using BassBoom.Native.Exceptions;
using BassBoom.Native.Interop;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BassBoom.Native.Runtime
{
    /// <summary>
    /// mpg123 instance class to enable Basolia to perform sound operations
    /// </summary>
    public unsafe class Mpg123Instance
    {
        internal static string runtimesPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/" + (
#if WINDOWS
            RuntimeInformation.OSArchitecture == Architecture.X64   ? "runtimes/win-x64/native/" :
            RuntimeInformation.OSArchitecture == Architecture.X86   ? "runtimes/win-x86/native/" :
            RuntimeInformation.OSArchitecture == Architecture.Arm   ? "runtimes/win-arm/native/" :
                                                                      "runtimes/win-arm64/native/");
#elif MACOS
            RuntimeInformation.OSArchitecture == Architecture.X64   ? "runtimes/osx-x64/native/" :
                                                                      "runtimes/osx-arm/native/");
#else
            RuntimeInformation.OSArchitecture == Architecture.X64   ? "runtimes/linux-x64/native/" :
            RuntimeInformation.OSArchitecture == Architecture.X86   ? "runtimes/linux-x86/native/" :
            RuntimeInformation.OSArchitecture == Architecture.Arm   ? "runtimes/linux-arm/native/" :
                                                                      "runtimes/linux-arm64/native/");
#endif
#if WINDOWS
        internal static string mpg123LibPath = runtimesPath + "mpg123-0.dll";
        internal static string out123LibPath = runtimesPath + "out123-0.dll";
#elif MACOS
        internal static string mpg123LibPath = runtimesPath + "libmpg123.dylib";
        internal static string out123LibPath = runtimesPath + "libout123.dylib";
#else
        internal static string mpg123LibPath = runtimesPath + "libmpg123.so";
        internal static string out123LibPath = runtimesPath + "libout123.so";
#endif

        /// <summary>
        /// Singleton of the mpg123 instance class
        /// </summary>
        public static Mpg123Instance Instance { get; } = new Mpg123Instance();

        internal static mpg123_handle* _mpg123Handle;
        internal static out123_handle* _out123Handle;

        /// <summary>
        /// Initializes the mpg123 library
        /// </summary>
        public static void InitializeLibrary() =>
            InitializeLibrary(mpg123LibPath, out123LibPath);

        /// <summary>
        /// Initializes the mpg123 library
        /// </summary>
        /// <param name="libPath">Absolute path to the mpg123 library</param>
        /// <param name="libPathOut">Absolute path to the out123 library</param>
        public static void InitializeLibrary(string libPath, string libPathOut)
        {
            // Check to see if we have this path
            if (!File.Exists(libPath))
                throw new BasoliaNativeLibraryException($"mpg123 library path {libPath} doesn't exist.");
            if (!File.Exists(libPathOut))
                throw new BasoliaNativeLibraryException($"out123 library path {libPath} doesn't exist.");

            // Set the library path
            string oldLibPath = mpg123LibPath;
            string oldLibPathOut = out123LibPath;
            mpg123LibPath = libPath;
            out123LibPath = libPathOut;
            NativeLibrary.SetDllImportResolver(typeof(NativeInit).Assembly, ResolveLibrary);
            string libPluginsPath = Path.GetDirectoryName(oldLibPath) + "/plugins/";
#if WINDOWS
            Environment.SetEnvironmentVariable("MPG123_MODDIR", libPluginsPath);
#else
            int result = LibraryTools.setenv("MPG123_MODDIR", libPluginsPath, 1);
            if (result != 0)
                throw new BasoliaNativeLibraryException("Can't set environment variable MPG123_MODDIR");
#endif

            // Verify that we've actually loaded the library!
            try
            {
                // mpg123 returns 0 on init.
                _ = LibraryTools.mpg123_init();
                var handle = NativeInit.mpg123_new(null, null);
                _mpg123Handle = handle;
            }
            catch (Exception ex)
            {
                mpg123LibPath = oldLibPath;
                throw new BasoliaNativeLibraryException($"mpg123 library path {libPath} doesn't contain a valid mpg123 library. mpg123_init() was called. {ex.Message}");
            }

            // Do the same for the out123 library!
            try
            {
                var handle = NativeOutputLib.out123_new();
                _out123Handle = handle;
            }
            catch (Exception ex)
            {
                out123LibPath = oldLibPathOut;
                throw new BasoliaNativeLibraryException($"out123 library path {libPathOut} doesn't contain a valid out123 library. out123_new() was called. {ex.Message}");
            }
        }

        private static nint ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            IntPtr libHandle = IntPtr.Zero;
            if (libraryName == LibraryTools.LibraryName)
                libHandle = NativeLibrary.Load(mpg123LibPath);
            else if (libraryName == LibraryTools.LibraryNameOut)
                libHandle = NativeLibrary.Load(out123LibPath);
            return libHandle;
        }

        internal protected Mpg123Instance()
        { }
    }
}
