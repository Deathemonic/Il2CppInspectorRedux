using Il2CppInspector.Model;
using Il2CppInspector.Outputs;
using Il2CppInspector.Reflection;
using Il2CppInspectorRedux.CLI.Helpers;

namespace Il2CppInspectorRedux.CLI.Processor;

public class OutputCppScaffolding : IOutputProcessor
{
    public async Task Process(Il2CppInspector.Il2CppInspector inspector, TypeModel typeModel, AppModel? appModel,
        string outputPath)
    {
        if (appModel == null)
        {
            Log.Error("AppModel is required for C++ scaffolding output");
            return;
        }

        await Task.Run(() =>
        {
            Log.Info("Generating C++ scaffolding...");

            var cppPath = Path.Join(outputPath, "cpp");
            Directory.CreateDirectory(cppPath);

            new CppScaffolding(appModel).Write(cppPath);

            Log.Success("C++ scaffolding generated successfully");
        });
    }
}