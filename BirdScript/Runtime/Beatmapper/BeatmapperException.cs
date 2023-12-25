using System;
using BirdScript.Errors;

namespace BirdScript.Beatmapper
{
    [Serializable]
    internal class BeatmapperException : ExceptionOnLine
    {
        public BeatmapperException(string message, int line) : base(message, line)
        {
        }
    }
}