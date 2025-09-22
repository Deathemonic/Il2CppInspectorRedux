using Il2CppInspector.Model;
using Il2CppInspector.Reflection;
using Il2CppInspectorRedux.CLI.Helpers;

namespace Il2CppInspectorRedux.CLI.Processor;

public class ExtractIl2CppFiles : IOutputProcessor
{
    public async Task Process(Il2CppInspector.Il2CppInspector inspector, TypeModel typeModel, AppModel? appModel,
        string outputPath)
    {
        await Task.Run(() =>
        {
            Log.Info("Extracting IL2CPP files...");

            Directory.CreateDirectory(outputPath);

            var binaryPath = Path.Join(outputPath, inspector.BinaryImage.DefaultFilename);
            inspector.SaveBinaryToFile(binaryPath);

            var metadataPath = Path.Join(outputPath, "global-metadata.dat");
            inspector.SaveMetadataToFile(metadataPath);

            Log.Success("IL2CPP files extracted successfully");
        });
    }
}