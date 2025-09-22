using Il2CppInspector.Cpp;
using Il2CppInspector.Cpp.UnityHeaders;
using Il2CppInspector.Model;
using Il2CppInspector.Reflection;
using Il2CppInspectorRedux.CLI.Helpers;

namespace Il2CppInspectorRedux.CLI.Core;

public static class ModelBuilder
{
    public static async Task<TypeModel> BuildTypeModel(Il2CppInspector.Il2CppInspector inspector)
    {
        return await Task.Run(() =>
        {
            Log.Info("Building .NET type model...");
            return new TypeModel(inspector);
        });
    }

    public static async Task<AppModel?> BuildAppModel(
        TypeModel typeModel,
        string? unityVersion,
        string? compilerType,
        bool needsAppModel)
    {
        if (!needsAppModel)
            return null;

        return await Task.Run(() =>
        {
            Log.Info("Building C++ application model...");

            var unityVer = !string.IsNullOrEmpty(unityVersion)
                ? new UnityVersion(unityVersion)
                : null;

            var compiler = Enum.TryParse<CppCompilerType>(compilerType, true, out var comp)
                ? comp
                : CppCompilerType.GCC;

            return new AppModel(typeModel, false).Build(unityVer, compiler);
        });
    }
}