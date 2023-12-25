using System;

namespace BirdScript.Errors
{
    [Serializable]
    internal class InstructionizerException : ExceptionOnLine
    {
        public InstructionizerException(string? message, int line) : base(message, line)
        {
        }
    }
}