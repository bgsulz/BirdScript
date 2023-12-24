using BirdScript.Instructionizer;
using BirdScript.Tokenizer;

namespace BirdScript.Beatmapper
{
    public class Beatmapper
    {
        private record Buffer(StartInstruction StartInstruction, List<IBufferableInstruction> Instructions)
        {
            public Buffer(StartInstruction start) : this(start, new()) { }
        }

        private float _beat;
        private InstructionList _source;
        private BeatmappedInstructionList _processed = new();

        private Dictionary<Command, Buffer> _commandToBuffer = new();

        public Beatmapper(InstructionList source)
        {
            _source = source;
        }

        public BeatmappedInstructionList Beatmap()
        {
            AssignActivateBeats();
            AssignBufferBeats();

            return _processed;
        }

        private void AssignActivateBeats()
        {
            _beat = 0;

            foreach (var instruction in _source)
            {
                switch (instruction)
                {
                    case TimedInstruction timed:
                        {
                            timed.BeatmapFromActivation(_beat);

                            if (timed is WaitInstruction wait)
                                _beat += wait.Duration;

                            int insertIndex = _processed.BinarySearch(timed);
                            _processed.Insert(insertIndex < 0 ? ~insertIndex : insertIndex, timed);

                            break;
                        }
                    case JumpInstruction jump:
                        _beat = jump.Beat;
                        break;
                    default:
                        throw new BeatmapperException($"How did an {instruction.GetType()} get in here?", instruction.Line);
                }
            }
        }

        private void AssignBufferBeats()
        {
            foreach (var instruction in _processed)
            {
                switch (instruction)
                {
                    case StartInstruction start:
                        if (_commandToBuffer.ContainsKey(start.Type))
                            throw new BeatmapperException(
                                $"Attemped to open buffer of type {start.Type}; " 
                                + $"one is already open on line {_commandToBuffer[start.Type].StartInstruction.Line}", 
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
    }
}