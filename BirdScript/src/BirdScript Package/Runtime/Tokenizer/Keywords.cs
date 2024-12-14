namespace BirdScript.Tokenizing
{
    public enum Command
    {
        // Timing
        Wait, Jump, BPM, Align,

        // Buffers
        Start, End,

        // Items
        Water, Rock, Bolt, Boulder, Gem, Ghost, Ball, Energy, Toxin, Nectar,
        // Energy: totem good, bird bad
        // Toxin: swap controls
        // Nectar: hold notes

        // Metadata
        By, Title, Location
    }

    public enum RowColumn
    {
        Row,
        Column
    }

    public enum Location
    {
        DesertDay,
        DesertNight,
        DesertStorm,
        SkyDay,
        Space
    }
}