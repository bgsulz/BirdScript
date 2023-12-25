using BirdScript.Tokenizer;

namespace BirdScript.Errors
{
    [Serializable]
    internal class ParameterException : InstructionizerException
    {
        public ParameterException(InfoToken<Command> head, List<IToken> arguments) 
            : base($"Unexpected parameters for command of type {head.Type}: {string.Join(", ", arguments)}", head.Line)
        {

        }
    }
}