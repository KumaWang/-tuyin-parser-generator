// See https://aka.ms/new-console-template for more information
using librule;
using System.Diagnostics;

var thisFileRoot = Path.GetDirectoryName(Helper.GetThisFilePath());
var compilerRoot = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(thisFileRoot))), "aiwriter\\AI.Writer\\AI.Writer\\packages\\richtext\\format\\parsers");

var sw = new Stopwatch();
sw.Start();

File.WriteAllText($"{compilerRoot}\\TuyinParser.cs", ModelGenerator.Generate(File.ReadAllText($"{thisFileRoot}/dsls/tuyin.txt"), false, out var debugGraph3));
File.WriteAllText($"G:\\a.project\\compiler\\cil\\Tuyin.IR.Analysis\\Parser\\TuyinIRParser.cs", ModelGenerator.Generate(File.ReadAllText($"{thisFileRoot}/dsls/ir.txt"), false, out var debugGraph4));
//File.WriteAllText(@"E:\a.tuyin\aiwriter\AI.Writer\AI.Writer\packages\richtext\format\parsers\AVGScriptParser.cs", ModelGenerator.Generate(File.ReadAllText($"{thisFileRoot}/tests/avgscript.txt"), out var debugGraph));
//File.WriteAllText(@"E:\a.tuyin\compiler\tool\dsl.builder\TuyinParser.cs", ModelGenerator.Generate(File.ReadAllText($"{thisFileRoot}/dsls/formatscript.txt"), out var debugGraph));
//File.WriteAllText($"{compilerRoot}\\JsonParser.cs", ModelGenerator.Generate(File.ReadAllText($"{thisFileRoot}/dsls/json.txt"), false, out var debugGraph2));
//File.WriteAllText($"{compilerRoot}\\XmlParser.cs", ModelGenerator.Generate(File.ReadAllText($"{thisFileRoot}/dsls/xml.txt"), false, out var debugGraph));
//File.WriteAllText(@"E:\a.tuyin\aiwriter\AI.Writer\AI.Writer\packages\richtext\format\parsers\TestParser.cs", ModelGenerator.Generate(File.ReadAllText($"{thisFileRoot}/dsls/test.txt"), out var debugGraph2));
sw.Stop();
Console.WriteLine(sw.ElapsedMilliseconds);

//ModelGenerator.Generate(File.ReadAllText($"{thisFileRoot}/dsls/test.txt"), true, out var debugGraph);

if (debugGraph4 != null)
    Helper.ShowGraph(Helper.CreateDotGraph(debugGraph4));
