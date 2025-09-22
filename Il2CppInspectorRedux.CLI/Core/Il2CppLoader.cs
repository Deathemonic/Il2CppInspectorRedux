using Il2CppInspectorRedux.CLI.Helpers;

namespace Il2CppInspectorRedux.CLI.Core;

public static class Il2CppLoader
{
    public static async Task<Il2CppInspector.Il2CppInspector> Load(string? il2Cpp, string? metadata)
    {
        return await Task.Run(() =>
        {
            Log.Info("Loading IL2CPP data...");

            if (metadata != null)
            {
                var inspectors = Il2CppInspector.Il2CppInspector.LoadFromFile(il2Cpp!, metadata);
                return GetInspectorOrExit(inspectors);
            }

            var packageInspectors = Il2CppInspector.Il2CppInspector.LoadFromPackage([il2Cpp!]);
            if (packageInspectors != null)
            {
                return GetInspectorOrExit(packageInspectors);
            }

            var metadataPath = Path.Combine(Path.GetDirectoryName(il2Cpp!)!, "global-metadata.dat");
            if (!File.Exists(metadataPath))
            {
                Log.Error($"Could not find metadata file at {metadataPath}. Please specify --metadata parameter.");
                Environment.Exit(1);
                return null!;
            }

            var fileInspectors = Il2CppInspector.Il2CppInspector.LoadFromFile(il2Cpp!, metadataPath);
            return GetInspectorOrExit(fileInspectors);
        });
    }

    private static Il2CppInspector.Il2CppInspector GetInspectorOrExit(List<Il2CppInspector.Il2CppInspector>? inspectors)
    {
        if (inspectors == null || inspectors.Count == 0)
        {
            Log.Error("Failed to load IL2CPP data from input files");
            Environment.Exit(1);
            return null!;
        }

        var inspector = inspectors[0];
        Log.Info($"Loaded IL2CPP data: {inspector.BinaryImage.Arch} / {inspector.BinaryImage.Bits}-bit");
        return inspector;
    }   
}