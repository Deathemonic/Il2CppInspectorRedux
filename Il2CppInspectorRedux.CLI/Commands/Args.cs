using Il2CppInspectorRedux.CLI.Helpers;

namespace Il2CppInspectorRedux.CLI.Commands;

public static class Args
{
    /// <summary>
    ///     Il2CppInspectorRedux - Modern IL2CPP reverse engineering tool
    /// </summary>
    /// <param name="il2cpp">Path to IL2CPP binary (libil2cpp.so, GameAssembly.dll) or package (APK/IPA/AAB)</param>
    /// <param name="metadata">Path to global-metadata.dat file (auto-detected for packages)</param>
    /// <param name="output">-o, Output directory for generated files (default: ./output)</param>
    /// <param name="unityVersion">-u, Unity version (e.g. "2022.3.0f1") - auto-detected if not specified</param>
    /// <param name="compilerType">-c, Target compiler: GCC, MSVC (default: GCC)</param>
    /// <param name="outputCsharpStub">-s, Generate C# type definitions and stubs</param>
    /// <param name="layout">-l, C# output layout: SingleFile, Namespace, Assembly, Class, Tree (default: SingleFile)</param>
    /// <param name="flattenHierarchy">-f, Flatten namespace hierarchy into single folder</param>
    /// <param name="outputDisassemblerMetadata">-m, Generate disassembler scripts and metadata</param>
    /// <param name="disassembler">-d, Target disassembler: IDA, Ghidra (default: IDA)</param>
    /// <param name="outputCppScaffolding">Generate C++ headers and scaffolding for DLL injection</param>
    /// <param name="sortingMode">C# type sorting: Index, Name, Alphabetical (default: Alphabetical)</param>
    /// <param name="suppressMetadata">Suppress IL2CPP metadata from outputs</param>
    /// <param name="compilable">Generate compilable C# code</param>
    /// <param name="separateAssemblyAttributes">Place assembly attributes in separate files</param>
    /// <param name="outputDummyDlls">Generate .NET dummy assemblies with type definitions</param>
    /// <param name="outputVsSolution">Generate complete Visual Studio solution with project files</param>
    /// <param name="unityPath">Path to Unity Editor installation (required for VS solution)</param>
    /// <param name="unityAssembliesPath">Path to Unity assemblies folder (required for VS solution)</param>
    /// <param name="extractIl2CppFiles">Extract original IL2CPP binary and metadata files</param>
    public static async Task Run(
        string il2cpp,
        string? metadata = null,
        string? output = null,
        string? unityVersion = null,
        string? compilerType = null,
        bool outputCsharpStub = false,
        string? layout = null,
        bool flattenHierarchy = false,
        bool outputDisassemblerMetadata = false,
        string? disassembler = null,
        bool outputCppScaffolding = false,
        string? sortingMode = null,
        bool suppressMetadata = false,
        bool compilable = false,
        bool separateAssemblyAttributes = false,
        bool outputDummyDlls = false,
        bool outputVsSolution = false,
        string? unityPath = null,
        string? unityAssembliesPath = null,
        bool extractIl2CppFiles = false)
    {
        if (!File.Exists(il2cpp))
        {
            Log.Error($"IL2CPP file '{il2cpp}' does not exist.");
            Environment.Exit(1);
            return;
        }

        if (metadata != null && !File.Exists(metadata))
        {
            Log.Error($"Metadata file '{metadata}' does not exist.");
            Environment.Exit(1);
            return;
        }

        if (!outputCppScaffolding && !outputCsharpStub && !outputDisassemblerMetadata &&
            !outputDummyDlls && !outputVsSolution && !extractIl2CppFiles)
        {
            Log.Error("At least one output format must be specified.");
            Environment.Exit(1);
            return;
        }

        await Parse.Execute(il2cpp, metadata, output, unityVersion, compilerType, outputCsharpStub,
            layout, flattenHierarchy, outputDisassemblerMetadata, disassembler, outputCppScaffolding, sortingMode,
            suppressMetadata, compilable, separateAssemblyAttributes,
            outputDummyDlls, outputVsSolution, unityPath,
            unityAssembliesPath, extractIl2CppFiles);
    }
}