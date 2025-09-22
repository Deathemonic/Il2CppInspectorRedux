using Il2CppInspector.Model;
using Il2CppInspector.Outputs;
using Il2CppInspector.Reflection;
using Il2CppInspectorRedux.CLI.Helpers;

namespace Il2CppInspectorRedux.CLI.Processor;

public class OutputCsharpStub(
    bool suppressMetadata = false,
    bool mustCompile = false,
    string layout = "SingleFile",
    bool flattenHierarchy = false,
    string sortingMode = "Alphabetical",
    bool separateAssemblyAttributes = false)
    : IOutputProcessor
{
    public async Task Process(Il2CppInspector.Il2CppInspector inspector, TypeModel typeModel, AppModel? appModel,
        string outputPath)
    {
        await Task.Run(() =>
        {
            Log.Info("Generating C# code stubs...");

            var csPath = Path.Join(outputPath, "cs");
            Directory.CreateDirectory(csPath);

            var writer = new CSharpCodeStubs(typeModel)
            {
                SuppressMetadata = suppressMetadata,
                MustCompile = mustCompile
            };

            switch (layout.ToLower(), sortingMode.ToLower())
            {
                case ("singlefile", "index"):
                    writer.WriteSingleFile(Path.Join(csPath, "types.cs"), t => t.Index);
                    break;
                case ("singlefile", "name") or ("singlefile", "alphabetical"):
                    writer.WriteSingleFile(Path.Join(csPath, "types.cs"), t => t.Name);
                    break;

                case ("namespace", "index"):
                    writer.WriteFilesByNamespace(csPath, t => t.Index, flattenHierarchy);
                    break;
                case ("namespace", "name") or ("namespace", "alphabetical"):
                    writer.WriteFilesByNamespace(csPath, t => t.Name, flattenHierarchy);
                    break;

                case ("assembly", "index"):
                    writer.WriteFilesByAssembly(csPath, t => t.Index, separateAssemblyAttributes);
                    break;
                case ("assembly", "name") or ("assembly", "alphabetical"):
                    writer.WriteFilesByAssembly(csPath, t => t.Name, separateAssemblyAttributes);
                    break;

                case ("class", _):
                    writer.WriteFilesByClass(csPath, flattenHierarchy);
                    break;

                case ("tree", _):
                    writer.WriteFilesByClassTree(csPath, separateAssemblyAttributes);
                    break;

                default:
                    writer.WriteSingleFile(Path.Join(csPath, "types.cs"), t => t.Index);
                    break;
            }

            if (writer.GetAndClearLastException() is { } ex)
            {
                Log.Error($"An error occurred while generating C# stubs: {ex.Message}");
                return;
            }

            Log.Success("C# stubs generated successfully");
        });
    }
}