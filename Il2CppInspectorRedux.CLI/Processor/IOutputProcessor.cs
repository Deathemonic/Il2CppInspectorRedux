using Il2CppInspector.Model;
using Il2CppInspector.Reflection;

namespace Il2CppInspectorRedux.CLI.Processor;

public interface IOutputProcessor
{
    Task Process(Il2CppInspector.Il2CppInspector inspector, TypeModel typeModel, AppModel? appModel, string outputPath);
}