using System.Collections.Generic;
using System.Linq;
using BirdScript.Tokenizing;
using BirdScript.Instructionizing;

namespace BirdScript.Beatmapping
{
    public class Beatmapper
    {
        private record Buffer(StartInstruction StartInstruction, List<IBufferableInstruction> Instructions)
        {
            public Buffer(StartInstruction start) : this(start, new()) { }
        }

        private float _beat;
        private readonly InstructionList _source;
        private readonly BeatmappedInstructionList _processed = new();

        private readonly Dictionary<Command, Buffer> _commandToBuffer = new();

        public Beatmapper(InstructionList source)
        {
            _source = source;
        }

        public BeatmappedInstructionList Beatmap()
        {
            AssignActivateBeats();
            AssignBufferBeats();
            PostProcess();

            return _processed;
        }

        private void AssignActivateBeats()
        {
            _beat = 0;

            foreach (var instruction in _source)
            {
                switch (instruction)
                {
                    case Metadata data:
                        _processed.Data.Add(data);
                        break;
                    case TimedInstruction timed:
                        {
                            if (!timed.HasProcessed)
                                timed.BeatmapFromActivation(_beat);

                            if (timed is WaitInstruction wait)
                                _beat += wait.Duration;
                            else
                            {
                                int insertIndex = _processed.BinarySearch(timed);
                                _processed.Insert(insertIndex < 0 ? ~insertIndex : insertIndex, timed);
                            }

                            break;
                        }
                    case SetInstruction set:
                        _beat = set.Beat;
                        break;
                    default:
                        throw new BeatmapperException($"How did a {instruction.GetType()} get in here?", instruction.Line);
                }
            }

            var stop = new EndOfChartInstruction();
            stop.BeatmapFromActivation(_beat);

            _processed.Add(stop);
            _processed.Data.ClearCache();
        }

        private void AssignBufferBeats()
        {
            foreach (var instruction in _processed)
            {
                switch (instruction)
                {
                    case StartInstruction start:
                        if (_commandToBuffer.TryGetValue(start.Type, out var existingBuffer))
                            throw new BeatmapperException(
                                $"Attemped to open buffer of type {start.Type}; "
                                + $"one is already open from line {existingBuffer.StartInstruction.Line}",
                                start.Line);

                        _commandToBuffer[start.Type] = new(start);
                        break;
                    case EndInstruction end:
                        if (!_commandToBuffer.TryGetValue(end.Type, out var buffer))
                            throw new BeatmapperException(
                                $"Attempted to close buffer of type {end.Type}; none is open",
                                end.Line);

                        foreach (var item in buffer.Instructions)
                            item.BeatmapFromBuffer(buffer.StartInstruction.ActivateBeat, end.ActivateBeat);

                        _commandToBuffer.Remove(end.Type);
                        break;
                    default:
                        if (instruction is IBufferableInstruction asBuffer
                            && instruction is ICommandInstruction asCommand
                            && _commandToBuffer.TryGetValue(asCommand.Type, out var openBuffer))
                            openBuffer.Instructions.Add(asBuffer);
                        break;
                }
            }

            if (_commandToBuffer.Any())
            {
                var unclosedBuffer = _commandToBuffer.First();
                throw new BeatmapperException(
                    $"Unclosed buffer of type {unclosedBuffer.Key}",
                    unclosedBuffer.Value.StartInstruction.Line);
                // TODO: Instructions should have lines for error passthrough.
                // Do with abstract base class?
            }
        }

        private void PostProcess()
        {
            // TODO: How to handle BPM and alignment?
            if (!_processed.Any(x => x is BPMInstruction))
                throw new BeatmapperException("No BPM command in chart.", 0);
            if (!_processed.Any(x => x is AlignInstruction))
                throw new BeatmapperException("No Align command in chart.", 0);

            // Fix BPM -- not necessary?
            // if (_processed.First() is not BPMInstruction)
            // {
            //     var firstTimed = _processed.First(x => x is TimedInstruction);
            //     var firstBeat = firstTimed.ActivateBeat;

            //     var firstBPM = _processed.FirstOrDefault(x => x is BPMInstruction) as BPMInstruction;

            //     if (firstBPM == null)
            //     {
            //         throw new BeatmapperException("No BPM instruction in chart.", 0);
            //     }
            //     else
            //     {
            //         var insertion = new BPMInstruction(firstBPM.Value) { ActivateBeat = firstBeat };
            //         _processed.Insert(0, insertion);
            //     }
            // }
        }
    }
}