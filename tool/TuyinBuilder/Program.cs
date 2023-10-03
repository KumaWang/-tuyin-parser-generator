using librule;

var dir = Path.GetDirectoryName(args[0]);
var output = Path.Combine(dir, $"{Path.GetFileNameWithoutExtension(args[0])}.cs");
File.WriteAllText(output, ModelGenerator.Generate(File.ReadAllText(args[0]), false, out var debugGraph3));
