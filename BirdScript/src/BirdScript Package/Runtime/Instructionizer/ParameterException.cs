using System;
using System.Collections.Generic;
using BirdScript.Tokenizing;

namespace BirdScript.Errors
{
    [Serializable]
    internal class ParameterException : InstructionizerException
    {
        public ParameterException(InfoToken<Command> head, List<IToken> arguments, string message = "") 
            : base($"Unexpected parameters for command of type {head.Type}: {string.Join(", ", arguments)}{(message != null ? $" -- {message}" : string.Empty)}", head.Line)
        {

        }
    }
}