using System;

namespace BirdScript.Errors
{
    [Serializable]
    internal abstract class ExceptionOnLine : Exception
    {
        public ExceptionOnLine(string message, int line) : base($"Line {line}: {message}")
        {
        }
    }
}