using BirdScript.Tokenizing;
using BirdScript.Instructionizing;
using BirdScript.Beatmapping;

namespace BirdScript.Pipeline
{
    public static class Pipeline
    {
        public static BeatmappedInstructionList Compile(string source)
        {
            var t = new Tokenizer(source).Tokenize();
            var i = new Instructionizer(t).Instructionize();
            var b = new Beatmapper(i).Beatmap();
            return b;
        }
    }
}