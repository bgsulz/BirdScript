using BirdScript.Instructionizer;
using BirdScript.Beatmapper;
using BirdScript.Tokenizer;
using System.Diagnostics;

string path;
do
{
    Console.WriteLine("Enter path.");
    path = Console.ReadLine() ?? "";
} while (!Path.Exists(path));

var stopwatch = new Stopwatch();
stopwatch.Start();

var text = File.ReadAllText(path);

var readingIsDone = stopwatch.ElapsedMilliseconds;

var tokens = new Tokenizer(text).Tokenize();

var tokenizerIsDone = stopwatch.ElapsedMilliseconds;
var tokenizerTime = tokenizerIsDone - readingIsDone;

var instructions = new Instructionizer(tokens).Instructionize();

var instructionizerIsDone = stopwatch.ElapsedMilliseconds;
var instructionizerTime = instructionizerIsDone - tokenizerIsDone;

var beatmappedInstructions = new Beatmapper(instructions).Beatmap();

var allIsDone = stopwatch.ElapsedMilliseconds;
var beatmapperTime = allIsDone - instructionizerIsDone;

stopwatch.Stop();

Console.WriteLine($"Read file in: {readingIsDone} ms");
Console.WriteLine($"Tokenizer completed in: {tokenizerTime} ms");
Console.WriteLine($"Instructionizer completed in: {instructionizerTime} ms");
Console.WriteLine($"Beatmapper completed in: {beatmapperTime} ms");
Console.WriteLine($"Total time: {allIsDone} ms");