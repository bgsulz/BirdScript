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
}