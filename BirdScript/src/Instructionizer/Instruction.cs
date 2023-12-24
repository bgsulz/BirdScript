using BirdScript.Tokenizer;

namespace BirdScript.Instructionizer
{
    public interface IInstruction
    {
        int Line { get; }
    }

    public interface ICommandInstruction
    {
        Command Type { get; }
    }

    public interface IBufferableInstruction
    {
        void BeatmapFromBuffer(float start, float end);
    }

    public abstract record Instruction() : IInstruction
    {
        public int Line { get; init; } = -1;
    }

    public record JumpInstruction(float Beat) : Instruction
    {
        public override string ToString() => $"Jump: {Beat}";
    }

    public abstract record TimedInstruction : Instruction, IComparable<TimedInstruction>
    {
        protected bool _hasProcessed = false;

        public float ActivateBeat { get; protected set; }
        protected virtual int Priority { get; } = 0;

        public virtual void BeatmapFromActivation(float beat)
        {
            _hasProcessed = true;
            ActivateBeat = beat;
        }

        protected abstract string InfoString();
        protected virtual string TimingString() => $"On {ActivateBeat}";

        public sealed override string ToString() => InfoString() + (_hasProcessed ? (" | " + TimingString()) : "");

        public int CompareTo(TimedInstruction? other)
        {
            if (other == null)
                return 1;

            int beatComparison = ActivateBeat.CompareTo(other.ActivateBeat);
            return beatComparison != 0 ? beatComparison : Priority.CompareTo(other.Priority);
        }
    }

    public record BPMInstruction(float Value) : TimedInstruction
    {
        protected override int Priority => -3;

        protected override string InfoString() => $"BPM: {Value}";
    }

    public record AlignInstruction(float Moment) : TimedInstruction
    {
        protected override int Priority => -2;

        protected override string InfoString() => $"Align: {Moment}";
    }

    public record StartInstruction(Command Type) : TimedInstruction, ICommandInstruction
    {
        protected override int Priority => -1;

        protected override string InfoString() => $"Start: {Type}";
    }

    public record EndInstruction(Command Type) : TimedInstruction, ICommandInstruction
    {
        protected override int Priority => 2;

        protected override string InfoString() => $"End: {Type}";
    }

    public record WaitInstruction(float Duration) : TimedInstruction
    {
        protected override int Priority => 1;

        public float EndBeat { get; private set; }

        public override void BeatmapFromActivation(float beat)
        {
            base.BeatmapFromActivation(beat);
            EndBeat = beat + Duration;
        }

        protected override string InfoString() => $"Wait: {Duration}";
        protected override string TimingString() => $"From {ActivateBeat} to {EndBeat}";
    }

    public record DropInstruction(Command Type, int X, int Y) : TimedInstruction, ICommandInstruction
    {
        public float DropBeat { get; protected set; }
        public float LandBeat { get; protected set; }

        public override void BeatmapFromActivation(float beat)
        {
            base.BeatmapFromActivation(beat - 2); // Cue appears
            DropBeat = beat - 1;
            LandBeat = beat;
        }

        protected override string InfoString() => $"{Type}: {X}, {Y}";
        protected sealed override string TimingString() => $"Cue {ActivateBeat}, drop {DropBeat}, land {LandBeat}";
    }

    public record GemInstruction(int X, int Y) : DropInstruction(Command.Gem, X, Y), IBufferableInstruction
    {
        public override void BeatmapFromActivation(float beat)
        {
            ActivateBeat = beat; // Cue appears
            _hasProcessed = true;
        }

        public void BeatmapFromBuffer(float start, float end)
        {
            LandBeat = end + (ActivateBeat - start);
            DropBeat = LandBeat - 1;
        }
    }
}