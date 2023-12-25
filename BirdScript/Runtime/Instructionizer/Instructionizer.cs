using BirdScript.Tokenizer;
using BirdScript.Errors;
using System.Collections.Generic;

namespace BirdScript.Instructionizer
{
    public class Instructionizer
    {
        private int _scan;
        private InstructionList _instructions = new();
        private TokenList _source;

        public Instructionizer(TokenList input)
        {
            _source = input;
        }

        public InstructionList Instructionize()
        {
            _scan = 0;

            while (!IsAtEnd())
            {
                Gather();
            }

            return _instructions;
        }

        private void Gather()
        {
            var t = Advance();

            switch (t.Type)
            {
                case TokenType.Command:
                    {
                        if (t is InfoToken<Command> token)
                            GatherCommand(token);
                        else
                            throw new TokenizerException($"Failed to convert token of type {t.Type}", t.Line);
                        break;
                    }
                case TokenType.Terminator or TokenType.EndOfFile: 
                    break;
                default:
                    throw new InstructionizerException($"Expecting command token; found {t.Type}", t.Line);
            }
        }

        private void GatherCommand(InfoToken<Command> head)
        {
            var arguments = GetArguments();
            _instructions.Add(InstructionFactory.Create(head, arguments));
        }

        private IEnumerable<IToken> GetArguments()
        {
            while (Peek().Type is not TokenType.Terminator and not TokenType.EndOfFile)
                yield return Advance();
        }

        private IToken Advance() => _source[_scan++];
        private IToken Peek() => _source[_scan];

        private bool IsAtEnd() => _source[_scan].Type == TokenType.EndOfFile;
    }
}