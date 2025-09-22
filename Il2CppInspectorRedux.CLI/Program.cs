using ConsoleAppFramework;
using Il2CppInspectorRedux.CLI.Commands;

var app = ConsoleApp.Create();
app.Add("", Args.Run);
await app.RunAsync(args);