using WxKit.Generator.Binding;
using WxKit.Generator.Doxygen;
using WxKit.Generator.Emitting;

// Usage: WxKit.Generator <Doxygen XML directory> <repository root>
if (args.Length != 2) {
	Console.Error.WriteLine("Usage: WxKit.Generator <Doxygen XML directory> <repository root>");
	return 2;
}
string xml = args[0];
string root = args[1];
DoxygenApi api = DoxygenReader.Read(xml);
Tweaks tweaks = new();
WxKit.Generator.Tweaks.Core.Apply(tweaks);
BindingResult result = new Binder(api, tweaks).Bind();
string shim = Path.Combine(root, "native", "shim");
string managed = Path.Combine(root, "src", "WxKit", "Generated");
Write(Path.Combine(shim, "include", "wxkit_generated.h"), ShimEmitter.Header(result));
Write(Path.Combine(shim, "generated", "generated.cpp"), ShimEmitter.Source(result));
Write(Path.Combine(root, "src", "WxKit", "Interop", "WxKitNative.g.cs"), InteropEmitter.Source(result));
// Every generated class file is rewritten, so one for a class no longer bound must not linger.
if (Directory.Exists(managed)) {
	foreach (string stale in Directory.GetFiles(managed, "*.g.cs")) File.Delete(stale);
}
foreach ((string fileName, string text) in ManagedEmitter.Files(result)) Write(Path.Combine(managed, fileName), text);
int functions = result.Classes.Sum(static bound => bound.Functions.Count());
Console.WriteLine($"Bound {result.Classes.Count(static bound => !bound.Tweak.IsHandwritten)} classes, {functions} functions, and {result.EventTypes.Count} event types.");
string report = Path.Combine(root, "artifacts", "generator-report.txt");
Write(report, string.Join('\n', result.Skipped) + "\n");
Console.WriteLine($"Skipped {result.Skipped.Count} members; see {report}.");
return 0;

static void Write(string path, string text) {
	Directory.CreateDirectory(Path.GetDirectoryName(path)!);
	File.WriteAllText(path, text.ReplaceLineEndings("\n"));
}
