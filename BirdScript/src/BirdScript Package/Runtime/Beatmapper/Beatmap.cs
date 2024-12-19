using System.Collections.Generic;
using BirdScript.Instructionizing;
using BirdScript.Tokenizing;

namespace BirdScript.Beatmapping
{
    public class Beatmap : List<TimedInstruction>
    {
        public Metadata Data { get; set; } = new();
        public List<Marker> Markers { get; } = new();

        public class Metadata
        {
            public string? Title { get; private set; }
            public string? Author { get; private set; }
            public Location Location { get; private set; } = Location.DesertDay;
            public float BPM { get; set; }

            private Dictionary<Command, Instructionizing.Metadata> _metadataCache = new();

            public void Add(Instructionizing.Metadata data)
            {
                if (AlreadyContains(data)) return;
                switch (data)
                {
                    case Metadata<string> str:
                        switch (str.Type)
                        {
                            case Command.Title:
                                Title = str.Value; break;
                            case Command.By:
                                Author = str.Value; break;
                        }
                        break;
                    case Metadata<Location> loc:
                        Location = loc.Value; break;
                }
            }

            private bool AlreadyContains(Instructionizing.Metadata data)
            {
                var type = data.Type;
                if (_metadataCache.TryGetValue(type, out var existingData))
                    throw new MetadataException(
                        $"Duplicate metadata of type {type}; existing on line {existingData.Line}",
                        data.Line
                    );
                _metadataCache.Add(type, data);
                return false;
            }

            public void ClearCache() => _metadataCache.Clear();
        }

        public record Marker(string Name, float Beat);
    }
}