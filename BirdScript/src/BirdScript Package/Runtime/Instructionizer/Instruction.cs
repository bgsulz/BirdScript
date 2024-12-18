#nullable enable

using System;
using System.Linq;
using BirdScript.Tokenizing;
using Location = BirdScript.Tokenizing.Location;

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

    public record SetInstruction(float Beat) : Instruction
    {
        public override string ToString() => $"Set: {Beat}";
    }

    public abstract record TimedInstruction : Instruction, IComparable<TimedInstruction>
    {
        public bool HasProcessed { get; protected set; } = false;

        public float ActivateBeat { get; set; }
        protected virtual int Priority { get; } = 0;

        // Should this instruction be removed from active items?
        // This is false for e.g. beach ball, which resolves on each bounce.
        public virtual bool IsFinalResolution => true;

        public virtual void BeatmapFromActivation(float beat)
        {
            HasProcessed = true;
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

    public record MarkerInstruction(string Name) : TimedInstruction
    {
        protected override string InfoString() => $"Marker: {Name}";
    }

    public record EndOfChartInstruction(bool ShouldEndEarly = false) : TimedInstruction
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

    public record DebugInstruction : TimedInstruction
    {
        protected override string InfoString() => "Debug";
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
        // This is not redundant: as gems are bufferable, they activate on beat, not beat - 2!
        public override void BeatmapFromActivation(float beat)
        {
            HasProcessed = true;
            ActivateBeat = beat;
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

    public abstract record Metadata(Command Type) : Instruction { }

    public abstract record Metadata<T>(T Value, Command Type) : Metadata(Type)
    {
        public override string ToString() => $"${Type}: {Value}";
    }

    public record StringMetadata : Metadata<string>
    {
        public StringMetadata(string Value, Command Type) : base(Value, Type) { }
    }

    public record LocationMetadata : Metadata<Location>
    {
        public LocationMetadata(Location Value, Command Type) : base(Value, Type) { }
    }
}