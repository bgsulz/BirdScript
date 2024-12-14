using System;
using System.Collections.Generic;
using BirdScript.Instructionizing;
using BirdScript.Tokenizing;

namespace BirdScript.Beatmapping
{
    public class BeatmappedInstructionList : List<TimedInstruction>
    {
        public Metadata Data { get; set; } = new();

        public class Metadata
        {
            public string? Title { get; private set; }
            public string? Author { get; private set; }
            public Location Location { get; private set; } = Location.DesertDay;
            public float BPM { get; set; }

            private Dictionary<Type, Instructionizing.Metadata> _metadataCache = new();

            public void Add(Instructionizing.Metadata data)
            {
                switch (data)
                {
                    case TitleMetadata title:
                        if (AlreadyContains(title)) break;
                        Title = title.Title; break;
                    case AuthorMetadata byline:
                        if (AlreadyContains(byline)) break;
                        Author = byline.Author; break;
                    case LocationMetadata location:
                        if (AlreadyContains(location)) break;
                        Location = location.Location; break;
                }
            }

            private bool AlreadyContains<T>(T data) where T : Instructionizing.Metadata
            {
                var type = typeof(T);
                if (_metadataCache.TryGetValue(type, out var existingData))
                    throw new MetadataException(
                        $"Duplicate metadata of type {type.Name}; existing on line {existingData.Line}", 
                        data.Line
                    );
                _metadataCache.Add(type, data);
                return false;
            }

            public void ClearCache() => _metadataCache.Clear();
        }
    }
}