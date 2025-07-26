﻿/*
    Copyright 2020-2021 Katy Coe - http://www.djkaty.com - https://github.com/djkaty

    All rights reserved.
*/

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInspector.Next;
using Il2CppInspector.Next.BinaryMetadata;
using VersionedSerialization;

namespace Il2CppInspector
{
    partial class Il2CppBinary
    {
        // Boyer-Moore-Horspool
        public IEnumerable<uint> FindAllBytes(byte[] blob, byte[] signature, uint requiredAlignment = 1)
        { 
            var badBytes = ArrayPool<uint>.Shared.Rent(256);

            var signatureLength = (uint) signature.Length;

            for (uint i = 0; i < 256; i++)
            {
                badBytes[(int)i] = signatureLength;
            }

            var lastSignatureIndex = signatureLength - 1;

            for (uint i = 0; i < lastSignatureIndex; i++)
            {
                badBytes[signature[(int)i]] = lastSignatureIndex - i;
            }

            var blobLength = blob.Length;

            var currentIndex = 0u;

            while (currentIndex <= blobLength - signatureLength)
            {
                for (uint i = lastSignatureIndex; blob[currentIndex + i] == signature[(int)i]; i--)
                {
                    if (i == 0)
                    {
                        yield return currentIndex;
                        break;
                    }
                }

                currentIndex += badBytes[blob[currentIndex + lastSignatureIndex]];

                var alignment = currentIndex % requiredAlignment;
                if (alignment != 0)
                    currentIndex += requiredAlignment - alignment;
            }

            ArrayPool<uint>.Shared.Return(badBytes);
        }

        // Find strings
        private IEnumerable<uint> FindAllStrings(byte[] blob, string str) => FindAllBytes(blob, Encoding.ASCII.GetBytes(str), 1);

        // Find 32-bit words
        private IEnumerable<uint> FindAllDWords(byte[] blob, uint word) => FindAllBytes(blob, BitConverter.GetBytes(word), 4);

        // Find 64-bit words
        private IEnumerable<uint> FindAllQWords(byte[] blob, ulong word) => FindAllBytes(blob, BitConverter.GetBytes(word), 8);

        // Find words for the current binary size
        private IEnumerable<uint> FindAllWords(byte[] blob, ulong word)
            => Image.Bits switch {
                32 => FindAllDWords(blob, (uint) word),
                64 => FindAllQWords(blob, word),
                _ => throw new InvalidOperationException("Invalid architecture bit size")
            };

        // Find all valid virtual address pointers to a virtual address
        private IEnumerable<ulong> FindAllMappedWords(byte[] blob, ulong va) {
            var fileOffsets = FindAllWords(blob, va);
            foreach (var offset in fileOffsets)
                if (Image.TryMapFileOffsetToVA(offset, out va))
                    yield return va;
        }

        // Find all valid virtual address pointers to a set of virtual addresses
        private IEnumerable<ulong> FindAllMappedWords(byte[] blob, IEnumerable<ulong> va) => va.SelectMany(a => FindAllMappedWords(blob, a));

        // Find all valid pointer chains to a set of virtual addresses with the specified number of indirections
        private IEnumerable<ulong> FindAllPointerChains(byte[] blob, ulong va, int indirections) {
            foreach (var vas in FindAllMappedWords(blob, va))
            {
                if (indirections == 1)
                {
                    yield return vas;
                }
                else
                {
                    foreach (var foundPointer in FindAllPointerChains(blob, vas, indirections - 1))
                    {
                        yield return foundPointer;
                    }
                }
            }

            //IEnumerable<ulong> vas = va;
            //for (int i = 0; i < indirections; i++)
            //    vas = FindAllMappedWords(blob, vas);
            //return vas;
        }

        // Scan the image for the needed data structures
        private (ulong, ulong) ImageScan(Metadata metadata) {
            Image.Position = 0;
            var imageBytes = Image.ReadBytes((int) Image.Length);

            var ptrSize = (uint) Image.Bits / 8;
            ulong codeRegistration = 0;
            IEnumerable<ulong> vas;

            // Find CodeRegistration
            // >= 24.2
            if (metadata.Version >= MetadataVersions.V242) {

                // < 27: mscorlib.dll is always the first CodeGenModule
                // >= 27: mscorlib.dll is always the last CodeGenModule (Assembly-CSharp.dll is always the first but non-Unity builds don't have this DLL)
                //        NOTE: winrt.dll + other DLLs can come after mscorlib.dll so we can't use its location to get an accurate module count
                ulong FindCodeRegistration()
                {
                    var imagesCount = Metadata.Images.Length;

                    foreach (var offset in FindAllStrings(imageBytes, "mscorlib.dll\0"))
                    {
                        if (!Image.TryMapFileOffsetToVA(offset, out var va))
                            continue;

                        // Unwind from string pointer -> CodeGenModule -> CodeGenModules + x
                        foreach (var potentialCodeGenModules in FindAllPointerChains(imageBytes, va, 2))
                        {
                            if (metadata.Version >= MetadataVersions.V270)
                            {
                                for (int i = imagesCount - 1; i >= 0; i--)
                                {
                                    foreach (var potentialCodeRegistrationPtr in FindAllPointerChains(imageBytes,
                                                 potentialCodeGenModules - (ulong) i * ptrSize, 1))
                                    {
                                        var expectedImageCountPtr = potentialCodeRegistrationPtr - ptrSize;
                                        var expectedImageCount = Image.ReadMappedWord(expectedImageCountPtr);
                                        if (expectedImageCount == imagesCount)
                                            return potentialCodeRegistrationPtr;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < imagesCount; i++)
                                {
                                    foreach (var potentialCodeRegistrationPtr in FindAllPointerChains(imageBytes,
                                                 potentialCodeGenModules - (ulong)i * ptrSize, 1))
                                    {
                                        return potentialCodeRegistrationPtr;
                                    }
                                }
                            }
                        }
                    }

                    return 0;
                }

                /*
                // We'll work back one pointer width at a time trying to find the first CodeGenModule
                // Let's hope there aren't more than 200 DLLs in any given application :)
                var maxCodeGenModules = 200;

                for (int backtrack = 0; backtrack < maxCodeGenModules && (codeRegVas?.Count() ?? 0) != 1; backtrack++) {
                    // Unwind from CodeGenModules + x -> CodeRegistration + y
                    codeRegVas = FindAllMappedWords(imageBytes, vas);

                    // The previous word must be the number of CodeGenModules
                    if (codeRegVas.Count() == 1) {
                        var codeGenModuleCount = Image.ReadMappedWord(codeRegVas.First() - ptrSize);

                        // Basic validity check
                        if (codeGenModuleCount <= 0 || codeGenModuleCount > maxCodeGenModules)
                            codeRegVas = Enumerable.Empty<ulong>();
                    }

                    // Move to the previous CodeGenModule if the above fails
                    vas = vas.Select(va => va - ptrSize);
                }

                if (!codeRegVas.Any())
                    return (0, 0);

                if (codeRegVas.Count() > 1)
                    throw new InvalidOperationException("More than one valid pointer chain found during data heuristics");
                */

                var codeRegVa = FindCodeRegistration();

                if (codeRegVa == 0)
                    return (0, 0);


                var codeGenEndPtr = codeRegVa + ptrSize;
                // pCodeGenModules is the last field in CodeRegistration so we subtract the size of one pointer from the struct size
                var version = Image.Version;
                var is32Bit = Image.Bits == 32;
                codeRegistration = codeGenEndPtr - (ulong)new Il2CppCodeRegistration().Size(in version, is32Bit);

                // In v24.3, windowsRuntimeFactoryTable collides with codeGenModules. So far no samples have had windowsRuntimeFactoryCount > 0;
                // if this changes we'll have to get smarter about disambiguating these two.
                var cr = Image.ReadMappedVersionedObject<Il2CppCodeRegistration>(codeRegistration);

                if (Image.Version == MetadataVersions.V242 && cr.InteropDataCount == 0) {
                    Image.Version = MetadataVersions.V243;
                    version = Image.Version;
                    codeRegistration = codeGenEndPtr - (ulong)new Il2CppCodeRegistration().Size(in version, is32Bit);
                }

                if (Image.Version == MetadataVersions.V270 && cr.ReversePInvokeWrapperCount > 0x30000)
                {
                    // If reversePInvokeWrapperCount is a pointer, then it's because we're actually on 27.1 and there's a genericAdjustorThunks pointer interfering.
                    // We need to bump version to 27.1 and back up one more pointer.
                    Image.Version = MetadataVersions.V271;
                    version = Image.Version;
                    codeRegistration = codeGenEndPtr - (ulong)new Il2CppCodeRegistration().Size(in version, is32Bit);
                    cr = Image.ReadMappedVersionedObject<Il2CppCodeRegistration>(codeRegistration);
                }

                // genericAdjustorThunks was inserted before invokerPointersCount in 24.5 and 27.1
                // pointer expected if we need to bump version
                if (Image.Version == MetadataVersions.V244 && 
                    (cr.InvokerPointersCount > 0x50000 || cr.ReversePInvokeWrapperCount > cr.ReversePInvokeWrappers))
                {
                    Image.Version = MetadataVersions.V245;
                    version = Image.Version;
                    codeRegistration = codeGenEndPtr - (ulong)new Il2CppCodeRegistration().Size(in version, is32Bit);
                    cr = Image.ReadMappedVersionedObject<Il2CppCodeRegistration>(codeRegistration);
                }

                if ((Image.Version == MetadataVersions.V290 || Image.Version == MetadataVersions.V310) &&
                    cr.GenericMethodPointersCount >= cr.GenericMethodPointers)
                {
                    Image.Version = new StructVersion(Image.Version.Major, 0, MetadataVersions.Tag2022);
                    version = Image.Version;
                    codeRegistration = codeGenEndPtr - (ulong)new Il2CppCodeRegistration().Size(in version, is32Bit);
                }
            }

            // Find CodeRegistration
            // <= 24.1
            else {
                // The first item in CodeRegistration is the total number of method pointers
                vas = FindAllMappedWords(imageBytes, (ulong) metadata.Methods.Count(m => (uint) m.MethodIndex != 0xffff_ffff));

                // The count of method pointers will be followed some bytes later by
                // the count of custom attribute generators; the distance between them
                // depends on the il2cpp version so we just use ReadMappedObject to simplify the math
                foreach (var va in vas) {
                    var cr = Image.ReadMappedVersionedObject<Il2CppCodeRegistration>(va);

                    if (cr.CustomAttributeCount == metadata.AttributeTypeRanges.Length)
                        codeRegistration = va;
                }

                if (codeRegistration == 0)
                    return (0, 0);
            }

            // Find MetadataRegistration
            // >= 19
            var metadataRegistration = 0ul;

            // Find TypeDefinitionsSizesCount (4th last field) then work back to the start of the struct
            // This saves us from guessing where metadataUsagesCount is later
            var mrVersion = Image.Version;
            var mrIs32Bit = Image.Bits == 32;
            var mrSize = (ulong)new Il2CppMetadataRegistration().Size(in mrVersion, mrIs32Bit);
            var typesLength = (ulong) metadata.Types.Length;

            vas = FindAllMappedWords(imageBytes, typesLength).Select(a => a - mrSize + ptrSize * 4);

            // >= 19 && < 27
            if (Image.Version < MetadataVersions.V270)
                foreach (var va in vas)
                {
                    var mr = Image.ReadMappedVersionedObject<Il2CppMetadataRegistration>(va);
                    if (mr.MetadataUsagesCount == (ulong) metadata.MetadataUsageLists.Length)
                        metadataRegistration = va;
                }

            // plagiarism. noun - https://www.lexico.com/en/definition/plagiarism
            //   the practice of taking someone else's work or ideas and passing them off as one's own.
            // Synonyms: copying, piracy, theft, strealing, infringement of copyright

            // >= 27
            else
            {
                foreach (var va in vas)
                {
                    var mr = Image.ReadMappedVersionedObject<Il2CppMetadataRegistration>(va);
                    if (mr.TypeDefinitionsSizesCount == metadata.Types.Length
                        && mr.FieldOffsetsCount == metadata.Types.Length)
                    {
                        metadataRegistration = va;
                        break;
                    }
                }
            }
            if (metadataRegistration == 0)
                return (0, 0);

            return (codeRegistration, metadataRegistration);
        }
    }
}
