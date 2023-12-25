using System;

namespace BirdScript.Errors
{
    [Serializable]
    internal class TokenizerException : ExceptionOnLine
    {
        public TokenizerException(string? message, int line) : base(message, line)
        {
        }
    }
}