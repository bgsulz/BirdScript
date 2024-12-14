using System;
using BirdScript.Errors;

namespace BirdScript.Beatmapping
{
    [Serializable]
    internal class BeatmapperException : ExceptionOnLine
    {
        public BeatmapperException(string message, int line) : base(message, line)
        {
        }
    }

    [Serializable]
    internal class MetadataException : ExceptionOnLine
    {
        public MetadataException(string message, int line) : base(message, line)
        {
        }
    }
}