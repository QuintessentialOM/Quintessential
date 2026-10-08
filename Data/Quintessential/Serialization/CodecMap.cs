using System.Collections.Generic;
using System.Linq;

namespace Quintessential.Serialization;

public abstract class CodecMap<TData> {
    public abstract bool IsHumanReadable();
    public abstract string ReadString(TData data);
    public abstract int ReadInt(TData data);
    public abstract long ReadLong(TData data);
    public abstract float ReadFloat(TData data);
    public abstract double ReadDouble(TData data);
    public abstract bool ReadBoolean(TData data);
    public abstract byte ReadByte(TData data);
    public abstract TData WriteString(string value);
    public abstract TData WriteInt(int value);
    public abstract TData WriteLong(long value);
    public abstract TData WriteFloat(float value);
    public abstract TData WriteDouble(double value);
    public abstract TData WriteBoolean(bool value);
    public abstract TData WriteByte(byte value);
    public abstract List<TData> ReadList(TData data);
    public abstract TData WriteList(List<TData> value);
    public abstract Dictionary<string,TData> ReadObject(TData data);
    public abstract TData WriteObject(Dictionary<string,TData> value);
    public virtual TData WriteObject(params KeyValuePair<string,TData>[] pairs) {
        return WriteObject(pairs.Where(pair => pair.Key != "").ToDictionary());
    } // Actually used a lot in CodecCreate.cs
}
