using Il2CppInspectorRedux.CLI.Core;
using Il2CppInspectorRedux.CLI.Helpers;
using Il2CppInspectorRedux.CLI.Processor;

namespace Il2CppInspectorRedux.CLI.Commands;

public static class Parse
{
    public static async Task Execute(
        string? il2Cpp,
        string? metadata,
        string? output,
        string? unityVersion,
        string? compilerType,
        bool outputCsharpStub,
        string? layout,
        bool flattenHierarchy,
        bool outputDisassemblerMetadata,
        string? disassembler,
        bool outputCppScaffolding,
        string? sortingMode,
        bool suppressMetadata,
        bool compilable,
        bool separateAssemblyAttributes,
        bool outputDummyDlls,
        bool outputVsSolution,
        string? unityPath,
        string? unityAssembliesPath,
        bool extractIl2CppFiles)
    {
        try
        {
            var inspector = await Il2CppLoader.Load(il2Cpp, metadata);

            output ??= Path.Combine(Directory.GetCurrentDirectory(), "output");
            Directory.CreateDirectory(output);

            var typeModel = await ModelBuilder.BuildTypeModel(inspector);
            var needsAppModel = outputCppScaffolding || outputDisassemblerMetadata || outputVsSolution;
            var appModel = await ModelBuilder.BuildAppModel(typeModel, unityVersion, compilerType, needsAppModel);

            var processors = new List<IOutputProcessor>();

            if (extractIl2CppFiles)
                processors.Add(new ExtractIl2CppFiles());

            if (outputDummyDlls)
                processors.Add(new OutputDummyDlls(suppressMetadata));

            if (outputCsharpStub)
                processors.Add(new OutputCsharpStub(suppressMetadata, compilable, layout ?? "SingleFile",
                    flattenHierarchy, sortingMode ?? "Alphabetical", separateAssemblyAttributes));

            if (outputCppScaffolding)
                processors.Add(new OutputCppScaffolding());

            if (outputDisassemblerMetadata)
                processors.Add(new OutputDisassemblerMetadata(disassembler ?? "IDA"));

            if (outputVsSolution)
                processors.Add(new OutputVsSolution(unityPath, unityAssembliesPath));

            foreach (var processor in processors) await processor.Process(inspector, typeModel, appModel, output);

            Log.Global.LogExportComplete();
        }
        catch (Exception ex)
        {
            Log.Error("An error occurred during processing", ex);
            Environment.Exit(1);
        }
    }
}