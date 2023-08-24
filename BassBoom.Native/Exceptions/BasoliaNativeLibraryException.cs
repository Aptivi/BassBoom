using BassBoom.Native.Runtime;
using System;
using System.Runtime.Serialization;

namespace BassBoom.Native.Exceptions
{
    public class BasoliaNativeLibraryException : Exception
    {
        public BasoliaNativeLibraryException() :
            base($"Native library error\n" +
                 $"Library path is {Mpg123Instance.mpg123LibPath}")
        { }

        public BasoliaNativeLibraryException(string message) :
            base($"{message}\n" +
                 $"Library path is {Mpg123Instance.mpg123LibPath}")
        { }

        public BasoliaNativeLibraryException(string message, Exception innerException) :
            base($"{message}\n" +
                 $"Library path is {Mpg123Instance.mpg123LibPath}", innerException)
        { }

        protected BasoliaNativeLibraryException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        { }
    }
}
