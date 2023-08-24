using BassBoom.Native.Interop.Init;
using System;

namespace BassBoom.Basolia
{
    public class BasoliaException : Exception
    {
        public BasoliaException(mpg123_errors error) :
            base($"General Basolia error\n" +
                 $"MPG123 returned the following error: [{error}]")
        { }

        public BasoliaException(string message, mpg123_errors error) :
            base($"{message}\n" +
                 $"MPG123 returned the following error: [{error}]")
        { }

        public BasoliaException(string message, Exception innerException, mpg123_errors error) :
            base($"{message}\n" +
                 $"MPG123 returned the following error: [{error}]", innerException)
        { }
    }
}
