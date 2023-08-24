using BassBoom.Native.Interop.Output;
using System;

namespace BassBoom.Basolia
{
    public class BasoliaOutException : Exception
    {
        public BasoliaOutException(out123_error error) :
            base($"General Basolia output system error\n" +
                 $"OUT123 returned the following error: [{error}]")
        { }

        public BasoliaOutException(string message, out123_error error) :
            base($"{message}\n" +
                 $"OUT123 returned the following error: [{error}]")
        { }

        public BasoliaOutException(string message, Exception innerException, out123_error error) :
            base($"{message}\n" +
                 $"OUT123 returned the following error: [{error}]", innerException)
        { }
    }
}
