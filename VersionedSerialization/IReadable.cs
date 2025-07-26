namespace VersionedSerialization;

public interface IReadable
{
    public void Read<TReader>(ref TReader reader, in StructVersion version = default) where TReader : IReader;
    public void ReadFromSpan(ref SpanReader reader, in StructVersion version = default);
    public int Size(in StructVersion version = default, bool is32Bit = false);
}