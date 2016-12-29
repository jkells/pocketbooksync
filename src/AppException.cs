using System;

namespace PocketBookSync
{
    public class AppException : Exception
    {
        public AppException(string message) : base(message)
        {
        }
    }
}