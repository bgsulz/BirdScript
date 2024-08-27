#nullable enable

using System;
using System.Linq;
using BirdScript.Tokenizing;

namespace BirdScript.Instructionizing
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

    public abstract record Metadata : Instruction { }

    public record JumpInstruction(float Beat) : Instruction
    {
        public override string ToString() => $"Jump: {Beat}";
    }

    public abstract record TimedInstruction : Instruction, IComparable<TimedInstruction>
    {
        protected bool _hasProcessed = false;

        public float ActivateBeat { get; set; }
        protected virtual int Priority { get; } = 0;

        // Should this instruction be removed from active items?
        // This is false for e.g. beach ball, which resolves on each bounce.
        public virtual bool IsFinalResolution => true;

        public virtual void BeatmapFromActivation(float beat)
        {
            _hasProcessed = true;
            ActivateBeat = beat;
        }

        protected abstract string InfoString();
        protected virtual string TimingString() => $"On {ActivateBeat}";

        public static string TimedString(string info, string timing) => info + " | " + timing;
        public override string ToString() => TimedString(InfoString(), TimingString());

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
        public override string ToString() => TimedString(InfoString(), TimingString());
    }

    public record AlignInstruction(float Moment) : TimedInstruction
    {
        protected override int Priority => -2;

        protected override string InfoString() => $"Align: {Moment}";
        public override string ToString() => TimedString(InfoString(), TimingString());
    }

    public record EndOfChartInstruction : TimedInstruction
    {
        protected override int Priority => 3;

        protected override string InfoString() => $"End of Chart";
        public override string ToString() => TimedString(InfoString(), TimingString());
    }

    public record StartInstruction(Command Type) : TimedInstruction, ICommandInstruction
    {
        protected override int Priority => -1;

        protected override string InfoString() => $"Start: {Type}";
        public override string ToString() => TimedString(InfoString(), TimingString());
    }

    public record EndInstruction(Command Type) : TimedInstruction, ICommandInstruction
    {
        protected override int Priority => 2;

        protected override string InfoString() => $"End: {Type}";
        public override string ToString() => TimedString(InfoString(), TimingString());
    }

    public record WaitInstruction(float Duration) : TimedInstruction
    {
        protected override int Priority => 1;

        public float EndBeat { get; set; }

        public override void BeatmapFromActivation(float beat)
        {
            base.BeatmapFromActivation(beat);
            EndBeat = beat + Duration;
        }

        protected override string InfoString() => $"Wait: {Duration}";
        protected override string TimingString() => $"From {ActivateBeat} to {EndBeat}";
        public override string ToString() => TimedString(InfoString(), TimingString());
    }

    public abstract record ItemInstruction(Command Type) : TimedInstruction, ICommandInstruction
    {
        public float DropBeat { get; set; }
        public float LandBeat { get; set; }
    }

    public record DropInstruction(Command Type, int X, int Y) : ItemInstruction(Type), ICommandInstruction
    {
        public override void BeatmapFromActivation(float beat)
        {
            base.BeatmapFromActivation(beat - 2); // Cue appears
            DropBeat = beat - 1;
            LandBeat = beat;
        }

        protected override string InfoString() => $"{Type}: {X}, {Y}";
        protected sealed override string TimingString() => $"Cue {ActivateBeat}, drop {DropBeat}, land {LandBeat}";
        public override string ToString() => TimedString(InfoString(), TimingString());
    }

    public record BoltInstruction(RowColumn RowColumn, int Coord) : ItemInstruction(Command.Bolt), ICommandInstruction
    {
        public override void BeatmapFromActivation(float beat)
        {
            base.BeatmapFromActivation(beat - 2); // Cue appears
            DropBeat = beat - 1;
            LandBeat = beat;
        }

        protected override string InfoString() => $"{Type}: {RowColumn} {Coord}";
        protected sealed override string TimingString() => $"Cue {ActivateBeat}, drop {DropBeat}, land {LandBeat}";
        public override string ToString() => TimedString(InfoString(), TimingString());
    }

    public record GemInstruction(int X, int Y) : DropInstruction(Command.Gem, X, Y), ICommandInstruction, IBufferableInstruction
    {
        public override void BeatmapFromActivation(float beat)
        {
            _hasProcessed = true;
            ActivateBeat = beat; // Cue appears
        }

        public void BeatmapFromBuffer(float start, float end)
        {
            LandBeat = end + (ActivateBeat - start);
            DropBeat = LandBeat - 1;
        }

        public override string ToString() => TimedString(InfoString(), TimingString());
    }

    public record BallInstruction(params (int X, int Y)[] CoordsList) : ItemInstruction(Command.Ball), ICommandInstruction
    {
        public int Index { get; set; }
        public (int X, int Y) Position => CoordsList[Math.Min(CoordsList.Length - 1, Index)];
        public bool HasBurst => Index >= CoordsList.Length;

        public override void BeatmapFromActivation(float beat)
        {
            base.BeatmapFromActivation(beat - 2);
            DropBeat = beat - 1;
            LandBeat = beat;
        }

        protected override string InfoString() => $"{Type}: {string.Join("; ", CoordsList.Select(p => $"{p.X}, {p.Y}"))}";
        protected sealed override string TimingString() => $"Cue {ActivateBeat}, drop {DropBeat}, land {LandBeat}";
        public override string ToString() => TimedString(InfoString(), TimingString());
    }

    public record AuthorMetadata(string Author) : Metadata;
    public record TitleMetadata(string Title) : Metadata;
}