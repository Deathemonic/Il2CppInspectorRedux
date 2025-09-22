using Il2CppInspector.Model;
using Il2CppInspector.Outputs;
using Il2CppInspector.Reflection;
using Il2CppInspectorRedux.CLI.Helpers;

namespace Il2CppInspectorRedux.CLI.Processor;

public class OutputDisassemblerMetadata(string disassembler = "IDA") : IOutputProcessor
{
    public async Task Process(Il2CppInspector.Il2CppInspector inspector, TypeModel typeModel, AppModel? appModel,
        string outputPath)
    {
        if (appModel == null)
        {
            Log.Error("AppModel is required for disassembler metadata output");
            return;
        }

        await Task.Run(() =>
        {
            Log.Info("Generating disassembler metadata...");

            var pyPath = Path.Join(outputPath, $"il2cpp_{disassembler.ToLower()}.py");
            var jsonPath = Path.Join(outputPath, "metadata.json");
            var cppHeaderPath = Path.Join(outputPath, "cpp", "appdata", "il2cpp-types.h");

            new JSONMetadata(appModel).Write(jsonPath);

            new PythonScript(appModel).WriteScriptToFile(pyPath, disassembler, cppHeaderPath, jsonPath);

            Log.Success("Disassembler metadata generated successfully");
        });
    }
}