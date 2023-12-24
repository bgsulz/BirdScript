using BirdScript.Errors;
using BirdScript.Tokenizer;

namespace BirdScript.Instructionizer
{
    public static class InstructionFactory
    {
        private delegate IInstruction ArgumentCreator(InfoToken<Command> head, List<IToken> arguments);

        private static readonly Dictionary<Command, ArgumentCreator> commandToCreator = new()
        {
            [Command.BPM] = BPMCreator,
            [Command.Wait] = WaitCreator,
            [Command.Align] = AlignCreator,
            [Command.Water] = SimpleDropCreator,
            [Command.Rock] = SimpleDropCreator,
            [Command.Boulder] = SimpleDropCreator,
            [Command.Gem] = GemCreator,
            [Command.Jump] = JumpCreator,
            [Command.Start] = StartCreator,
            [Command.End] = EndCreator
        };

        private static IInstruction BPMCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<float>(out var number))
                return new BPMInstruction(number) { Line = head.Line };
            throw new ParameterException(head, arguments);
        }

        private static IInstruction AlignCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<float>(out var number))
                return new AlignInstruction(number) { Line = head.Line };
            throw new ParameterException(head, arguments);
        }

        private static IInstruction WaitCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<float>(out var number))
                return new WaitInstruction(number) { Line = head.Line };
            throw new ParameterException(head, arguments);
        }

        private static IInstruction StartCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<Command>(out var command))
                return new StartInstruction(command) { Line = head.Line };
            throw new ParameterException(head, arguments);
        }

        private static IInstruction EndCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<Command>(out var command))
                return new EndInstruction(command) { Line = head.Line };
            throw new ParameterException(head, arguments);
        }

        private static IInstruction JumpCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<float>(out var number))
                return new JumpInstruction(number) { Line = head.Line };
            throw new ParameterException(head, arguments);
        }

        private static IInstruction SimpleDropCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<int, int>(out var x, out var y))
            {
                if (ArgumentValidator.ValidateBounds(head, x, y))
                    return new DropInstruction(head.Value, x, y) { Line = head.Line };
            }
            throw new ParameterException(head, arguments);
        }

        private static IInstruction GemCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<int, int>(out var x, out var y))
            {
                if (ArgumentValidator.ValidateBounds(head, x, y))
                    return new GemInstruction(x, y) { Line = head.Line };
            }
            throw new ParameterException(head, arguments);
        }

        public static IInstruction Create(InfoToken<Command> head, IEnumerable<IToken> arguments)
            => commandToCreator[head.Value].Invoke(head, arguments.ToList());
    }
}