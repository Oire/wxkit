using WxKit.Generator.Doxygen;

DoxygenApi api = DoxygenReader.Read(args[0]);
Console.WriteLine($"{api.Classes.Count} classes, {api.Constants.Count} constants");
foreach (string name in new[] { "wxButton", "wxTextCtrl", "wxFrame", "wxTopLevelWindow", "wxMessageDialog", "wxTextEntry" }) {
	DoxygenClass c = api.Classes[name];
	Console.WriteLine($"{c.Name} : {string.Join(", ", c.Bases)} | {c.Members.Count} members | styles {string.Join(" ", c.Styles.Select(static s => s.Name))}");
	foreach (DoxygenEvent e in c.Events) Console.WriteLine($"  event {e.Macro} {e.EventType} {e.EventClass}");
}
foreach (string constant in new[] { "wxTE_MULTILINE", "wxDEFAULT_FRAME_STYLE", "wxBORDER_NONE", "wxOK", "wxCENTRE", "wxBOTH", "wxID_ANY" }) Console.WriteLine($"{constant} = {api.Constants.GetValueOrDefault(constant)}");
