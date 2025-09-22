using Il2CppInspector.Model;
using Il2CppInspector.Outputs;
using Il2CppInspector.Reflection;
using Il2CppInspectorRedux.CLI.Helpers;

namespace Il2CppInspectorRedux.CLI.Processor;

public class OutputVsSolution(string? unityPath = null, string? unityAssembliesPath = null) : IOutputProcessor
{
    public async Task Process(Il2CppInspector.Il2CppInspector inspector, TypeModel typeModel, AppModel? appModel,
        string outputPath)
    {
        await Task.Run(() =>
        {
            Log.Info("Generating Visual Studio solution...");

            var vsPath = Path.Join(outputPath, "vs");
            Directory.CreateDirectory(vsPath);

            var writer = new CSharpCodeStubs(typeModel)
            {
                MustCompile = true
            };

            writer.WriteSolution(vsPath, unityPath ?? "", unityAssembliesPath ?? "");

            if (writer.GetAndClearLastException() is { } ex)
            {
                Log.Error($"An error occurred while generating Visual Studio solution: {ex.Message}");
                return;
            }

            Log.Success("Visual Studio solution generated successfully");
        });
    }
}