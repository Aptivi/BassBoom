using BassBoom.Native.Interop.Init;
using BassBoom.Native.Runtime;

namespace BassBoom.Basolia
{
    /// <summary>
    /// Initialization code
    /// </summary>
    public static class InitBasolia
    {
        private static bool _basoliaInited = false;
        
        /// <summary>
        /// Initializes the MPG123 library for Basolia to function
        /// </summary>
        public static void Init()
        {
            Mpg123Instance.InitializeLibrary();
            _basoliaInited = true;
        }

        public static void CheckInited()
        {
            if (!_basoliaInited)
                throw new BasoliaException("Basolia didn't initialize the MPG123 library yet!", mpg123_errors.MPG123_NOT_INITIALIZED);
        }
    }
}
