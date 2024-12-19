using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using BirdScript.Errors;
using BirdScript.Tokenizing;
using Location = BirdScript.Tokenizing.Location;

namespace BirdScript.Instructionizing
{
    public static class InstructionFactory
    {
        private delegate IInstruction ArgumentCreator(InfoToken<Command> head, List<IToken> arguments);

        private static readonly Dictionary<Command, ArgumentCreator> commandToCreator = new()
        {
            [Command.BPM] = BPMCreator,
            [Command.Wait] = WaitCreator,
            [Command.Align] = AlignCreator,
            [Command.Marker] = MarkerCreator,
            [Command.Stop] = StopCreator,
            [Command.Water] = SimpleDropCreator,
            [Command.Skill] = SimpleDropCreator,
            [Command.Rock] = SimpleDropCreator,
            [Command.Ghost] = SimpleDropCreator,
            [Command.Boulder] = SimpleDropCreator,
            [Command.Ball] = BallCreator,
            [Command.Bolt] = BoltCreator,
            [Command.Gem] = GemCreator,
            [Command.Set] = SetCreator,
            [Command.Start] = StartCreator,
            [Command.End] = EndCreator,
            [Command.Debug] = DebugCreator,
            [Command.By] = StringMetadataCreator,
            [Command.Title] = StringMetadataCreator,
            [Command.Location] = LocationMetadataCreator,
        };

        private static IInstruction BPMCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<float>(out var number))
                return new BPMInstruction(number) { Line = head.Line };
            throw new ParameterException(head, arguments);
        }

        private static IInstruction AlignCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<float>(out var time))
                return new AlignInstruction(time) { Line = head.Line };
            if (arguments.Match<float, float>(out var beatTime, out var beat))
            {
                var instr = new AlignInstruction(beatTime) { Line = head.Line };
                instr.BeatmapFromActivation(beat);
                return instr;
            }
            throw new ParameterException(head, arguments);
        }

        private static IInstruction MarkerCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<string>(out var name))
                return new MarkerInstruction(name) { Line = head.Line };
            throw new ParameterException(head, arguments);
        }

        private static IInstruction StopCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match())
                return new EndOfChartInstruction(ShouldEndEarly: true);
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

        private static IInstruction SetCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<float>(out var number))
                return new SetInstruction(number) { Line = head.Line };
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

        private static IInstruction BoltCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<RowColumn, int>(out var rcFirst, out var coordSecond))
            {
                if (ArgumentValidator.ValidateBounds(head, coordSecond))
                    return new BoltInstruction(rcFirst, coordSecond) { Line = head.Line };
            }

            if (arguments.Match<int, RowColumn>(out var coordFirst, out var rcSecond))
            {
                if (ArgumentValidator.ValidateBounds(head, coordFirst))
                    return new BoltInstruction(rcSecond, coordFirst) { Line = head.Line };
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

        private static IInstruction BallCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Count % 2 != 0)
                throw new ParameterException(head, arguments, "Number of coordinates must be even");

            if (arguments.MatchAll<int>(out var xs))
            {
                if (xs.All(x => ArgumentValidator.ValidateBounds(head, x)))
                {
                    var coords = new (int, int)[xs.Length / 2];
                    for (int i = 0; i < xs.Length; i += 2)
                        coords[i / 2] = (xs[i], xs[i + 1]);

                    return new BallInstruction(coords) { Line = head.Line };
                }
            }
            throw new ParameterException(head, arguments);
        }

        private static IInstruction StringMetadataCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<string>(out var str))
                return new StringMetadata(str, head.Value) { Line = head.Line };
            throw new ParameterException(head, arguments);
        }

        private static IInstruction LocationMetadataCreator(InfoToken<Command> head, List<IToken> arguments)
        {
            if (arguments.Match<Location>(out var loc))
                return new LocationMetadata(loc, head.Value) { Line = head.Line };
            throw new ParameterException(head, arguments);
        }

        private static IInstruction DebugCreator(InfoToken<Command> head, List<IToken> arguments)
            => new DebugInstruction();

        public static IInstruction Create(InfoToken<Command> head, IEnumerable<IToken> arguments)
            => commandToCreator[head.Value].Invoke(head, arguments.ToList());
    }
}