namespace Il2CppInspectorRedux.CLI.Commands;

public static class Args
{
    /// <summary>
    ///     Il2CppInspectorRedux
    /// </summary>
    /// <param name="input">Paths to the input files. Will be subsequently loaded until binary and metadata were found.</param>
    /// <param name="output">-o, Path to the output folder</param>
    /// <param name="outputCppScaffolding">Generate C++ scaffolding output</param>
    /// <param name="unityVersion">-u, Specify a Unity version</param>
    /// <param name="compilerType">-c, Specify compiler type</param>
    /// <param name="outputCsharpStub">-s, Generate C# stub output</param>
    /// <param name="layout">-l, Specify layout options</param>
    /// <param name="flattenHierarchy">-f, Flatten class hierarchy</param>
    /// <param name="sortingMode">Specify sorting mode</param>
    /// <param name="suppressMetadata">Suppress metadata output</param>
    /// <param name="compilable">Generate compilable output</param>
    /// <param name="separateAssemblyAttributes">Separate assembly attributes</param>
    /// <param name="outputDisassemblerMetadata">-m, Output disassembler metadata</param>
    /// <param name="disassembler">-d, Specify disassembler options</param>
    /// <param name="outputDummyDlls">Generate dummy DLLs</param>
    /// <param name="outputVsSolution">Generate Visual Studio solution</param>
    /// <param name="unityPath">Path to Unity installation</param>
    /// <param name="unityAssembliesPath">Path to Unity assemblies</param>
    /// <param name="extractIl2CppFiles">Extract IL2CPP files</param>
    public static void Run(
        string input,
        string? output = null,
        bool outputCppScaffolding = false,
        string? unityVersion = null,
        string? compilerType = null,
        bool outputCsharpStub = false,
        string? layout = null,
        bool flattenHierarchy = false,
        string? sortingMode = null,
        bool suppressMetadata = false,
        bool compilable = false,
        bool separateAssemblyAttributes = false,
        bool outputDisassemblerMetadata = false,
        string? disassembler = null,
        bool outputDummyDlls = false,
        bool outputVsSolution = false,
        string? unityPath = null,
        string? unityAssembliesPath = null,
        bool extractIl2CppFiles = false)
    {
        Parse.Execute(input, output, outputCppScaffolding, unityVersion, compilerType, outputCsharpStub,
            layout, flattenHierarchy, sortingMode, suppressMetadata, compilable, separateAssemblyAttributes,
            outputDisassemblerMetadata, disassembler, outputDummyDlls, outputVsSolution, unityPath,
            unityAssembliesPath, extractIl2CppFiles);
    }
}