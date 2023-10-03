// See https://aka.ms/new-console-template for more information
using Antlr4.Runtime;
using ParserGeneratorTest;
using System.Diagnostics;
using Tuitor.packages.richtext.format.parsers.markdown;

var small = File.ReadAllText("small.md");
var large = File.ReadAllText("large.md");
var small_json = File.ReadAllText("small_json.json");
var large_json = File.ReadAllText("large_json.json");
Console.WriteLine($"解析small.md({small.Length})与large.md({large.Length})");

Stopwatch sw = new Stopwatch();
sw.Restart();
var inputStream4 = new AntlrInputStream(small_json);
var lexer4 = new JSONLexer(inputStream4);
var tokenStream4 = new CommonTokenStream(lexer4);
var antlr4Parser4 = new JSONParser(tokenStream4);
antlr4Parser4.json();
sw.Stop();
Console.WriteLine($"Antlr4解析small_json.json耗时 {sw.ElapsedMilliseconds}ms/{sw.ElapsedTicks}ticks");

sw.Restart();
var json4 = Newtonsoft.Json.JsonConvert.DeserializeObject(small_json);
sw.Stop();
Console.WriteLine($"Newtonsoft.Json解析small_json.json耗时 {sw.ElapsedMilliseconds}ms/{sw.ElapsedTicks}ticks");

sw.Restart();
var tuyinParser4 = new JsonParser();
var json = tuyinParser4.Parse(small_json) as JsonItem;
sw.Stop();
Console.WriteLine($"Tuyin解析small_json.json耗时 {sw.ElapsedMilliseconds}ms/{sw.ElapsedTicks}ticks");

sw.Restart();
var inputStream5 = new AntlrInputStream(large_json);
var lexer5 = new JSONLexer(inputStream5);
var tokenStream5 = new CommonTokenStream(lexer5);
var antlr4Parser5 = new JSONParser(tokenStream5);
antlr4Parser5.json();
sw.Stop();
Console.WriteLine($"Antlr4解析large_json.json耗时 {sw.ElapsedMilliseconds}ms/{sw.ElapsedTicks}ticks");

sw.Restart();
var json5 = Newtonsoft.Json.JsonConvert.DeserializeObject(large_json);
sw.Stop();
Console.WriteLine($"Newtonsoft.Json解析large_json.json耗时 {sw.ElapsedMilliseconds}ms/{sw.ElapsedTicks}ticks");

sw.Restart();
var tuyinParser5 = new JsonParser();
var json2 = tuyinParser5.Parse(large_json) as JsonItem;
sw.Stop();
Console.WriteLine($"Tuyin解析large_json.json耗时 {sw.ElapsedMilliseconds}ms/{sw.ElapsedTicks}ticks");

sw.Restart();
var inputStream = new AntlrInputStream(small);
var lexer = new MarkdownLexer(inputStream);
var tokenStream = new CommonTokenStream(lexer);
var antlr4Parser = new MarkdownParser(tokenStream);
antlr4Parser.markdown();
sw.Stop();
Console.WriteLine($"Antlr4解析small.md耗时 {sw.ElapsedMilliseconds}ms/{sw.ElapsedTicks}ticks");

sw.Restart();
var tuyinParser = new TuyinMarkdownParser();
var markdown = tuyinParser.Parse(small) as Markdown;
sw.Stop();
Console.WriteLine($"Tuyin解析small.md耗时 {sw.ElapsedMilliseconds}ms/{sw.ElapsedTicks}ticks");

sw.Restart();
var inputStream2 = new AntlrInputStream(large);
var lexer2 = new MarkdownLexer(inputStream2);
var tokenStream2 = new CommonTokenStream(lexer2);
var antlr4Parser2 = new MarkdownParser(tokenStream2);
antlr4Parser2.markdown();
sw.Stop();
Console.WriteLine($"Antlr4解析large.md耗时 {sw.ElapsedMilliseconds}ms/{sw.ElapsedTicks}ticks");

sw.Restart();
var tuyinParser2 = new TuyinMarkdownParser();
var markdown2 = tuyinParser2.Parse(large) as Markdown;
sw.Stop();
Console.WriteLine($"Tuyin解析large.md耗时 {sw.ElapsedMilliseconds}ms/{sw.ElapsedTicks}ticks");

sw.Restart();
var tuyinParser3 = new JsonParser();
tuyinParser3.Parse1000000(small_json);
sw.Stop();
Console.WriteLine($"Tuyin解析100万次small.md耗时 {sw.ElapsedMilliseconds}ms/{sw.ElapsedTicks}ticks");
