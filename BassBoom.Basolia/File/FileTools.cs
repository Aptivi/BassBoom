using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Play;
using BassBoom.Native.Runtime;

namespace BassBoom.Basolia.File
{
    public static class FileTools
    {
        private static bool isOpened = false;

        /// <summary>
        /// Is the file open?
        /// </summary>
        public static bool IsOpened => isOpened;

        /// <summary>
        /// Opens a media file
        /// </summary>
        public static void OpenFile(string path)
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (IsOpened)
                throw new BasoliaException("Can't open this file while the current file is still open", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we provided a path
            if (string.IsNullOrEmpty(path))
                throw new BasoliaException("Provide a path to a music file", mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                // Open the file
                var handle = Mpg123Instance._mpg123Handle;
                int openStatus = NativeInput.mpg123_open(handle, path);
                if (openStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't open file", mpg123_errors.MPG123_ERR);
                isOpened = true;
            }
        }

        /// <summary>
        /// Closes a currently opened media file
        /// </summary>
        public static void CloseFile()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened)
                throw new BasoliaException("Can't close a file that's already closed", mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                // Open the file
                var handle = Mpg123Instance._mpg123Handle;
                int openStatus = NativeInput.mpg123_close(handle);
                if (openStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't close file", mpg123_errors.MPG123_ERR);
                isOpened = false;
            }
        }
    }
}
