using Il2CppInspector.Model;
using Il2CppInspector.Outputs;
using Il2CppInspector.Reflection;
using Il2CppInspectorRedux.CLI.Helpers;

namespace Il2CppInspectorRedux.CLI.Processor;

public class OutputDummyDlls(bool suppressMetadata = false) : IOutputProcessor
{
    public async Task Process(Il2CppInspector.Il2CppInspector inspector, TypeModel typeModel, AppModel? appModel,
        string outputPath)
    {
        await Task.Run(() =>
        {
            Log.Info("Generating .NET assembly shim DLLs...");

            var dllPath = Path.Join(outputPath, "dll");
            Directory.CreateDirectory(dllPath);

            var shims = new AssemblyShims(typeModel)
            {
                SuppressMetadata = suppressMetadata
            };

            shims.Write(dllPath, (_, message) => Log.Info(message));

            Log.Success("Dummy DLLs generated successfully");
        });
    }
}