using System.Collections.Generic;
using BirdScript.Instructionizing;

namespace BirdScript.Beatmapping
{
    public class BeatmappedInstructionList : List<TimedInstruction>
    {
        public Metadata Data { get; set; } = new();

        public class Metadata
        {
            public string? Title { get; private set; }
            public string? Author { get; private set; }
            public float BPM { get; set; }

            public void Add(Instructionizing.Metadata data)
            {
                switch (data)
                {
                    case TitleMetadata title:
                        Title = title.Title; break;
                    case AuthorMetadata byline:
                        Author = byline.Author; break;
                }
            }
        }
    }
}