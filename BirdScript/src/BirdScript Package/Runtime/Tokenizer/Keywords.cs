namespace BirdScript.Tokenizing
{
    public enum Command
    {
        // Timing
        Wait, Set, BPM, Align,

        // Buffers
        Start, End,

        // Items
        Water, Gem, Ghost, Ball, Nectar,
        Rock, Bolt, Boulder, 
        Skill,
        Energy, Toxin,
        // Energy: totem good, bird bad
        // Toxin: swap controls
        // Nectar: hold notes

        // Metadata
        By, Title, Location,

        // Debug
        Debug
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
        SpaceSplit
    }
}