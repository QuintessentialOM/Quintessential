using System;
using System.Collections.Generic;
using System.Linq;

namespace Quintessential.Serialization;

public abstract class PrimitiveCodec<T> : Codec<T> {
    public abstract override TData Encode<TData>(CodecMap<TData> map, T item);
    public abstract override T Decode<TData>(CodecMap<TData> map, TData encoding);
}

internal class StringCodec : PrimitiveCodec<string> {
    public override string Decode<TData>(CodecMap<TData> map, TData encoding) {
        return map.ReadString(encoding);
    }
    public override TData Encode<TData>(CodecMap<TData> map, string item) {
        return map.WriteString(item);
    }
}
internal class IntCodec : PrimitiveCodec<int> {
    public override int Decode<TData>(CodecMap<TData> map, TData encoding) {
        return map.ReadInt(encoding);
    }
    public override TData Encode<TData>(CodecMap<TData> map, int item) {
        return map.WriteInt(item);
    }
}
internal class LongCodec : PrimitiveCodec<long> {
    public override long Decode<TData>(CodecMap<TData> map, TData encoding) {
        return map.ReadLong(encoding);
    }
    public override TData Encode<TData>(CodecMap<TData> map, long item) {
        return map.WriteLong(item);
    }
}
internal class FloatCodec : PrimitiveCodec<float> {
    public override float Decode<TData>(CodecMap<TData> map, TData encoding) {
        return map.ReadFloat(encoding);
    }
    public override TData Encode<TData>(CodecMap<TData> map, float item) {
        return map.WriteFloat(item);
    }
}
internal class DoubleCodec : PrimitiveCodec<double> {
    public override double Decode<TData>(CodecMap<TData> map, TData encoding) {
        return map.ReadDouble(encoding);
    }
    public override TData Encode<TData>(CodecMap<TData> map, double item) {
        return map.WriteDouble(item);
    }
}
internal class BoolCodec : PrimitiveCodec<bool> {
    public override bool Decode<TData>(CodecMap<TData> map, TData encoding) {
        return map.ReadBoolean(encoding);
    }
    public override TData Encode<TData>(CodecMap<TData> map, bool item) {
        return map.WriteBoolean(item);
    }
}
internal class ByteCodec : PrimitiveCodec<byte> {
    public override byte Decode<TData>(CodecMap<TData> map, TData encoding) {
        return map.ReadByte(encoding);
    }
    public override TData Encode<TData>(CodecMap<TData> map, byte item) {
        return map.WriteByte(item);
    }
}

public class ListCodec<T> : PrimitiveCodec<List<T>> {
    private ListCodec(Codec<T> innerCodec) { InnerCodec = innerCodec; }
    public static ListCodec<T> Create(Codec<T> innerCodec) => new(innerCodec);

    private readonly Codec<T> InnerCodec;

    public override List<T> Decode<TData>(CodecMap<TData> map, TData encoding) {
        List<TData> list = map.ReadList(encoding);
        return [.. list.Select(data => InnerCodec.Decode(map, data))];
    }
    public override TData Encode<TData>(CodecMap<TData> map, List<T> items) {
        List<TData> list = [.. items.Select(item => InnerCodec.Encode(map, item))];
        return map.WriteList(list);
    }
}
public class DictCodec<T> : PrimitiveCodec<Dictionary<string, T>> {
    private DictCodec(Codec<T> innerCodec) { InnerCodec = innerCodec; }
    public static DictCodec<T> Create(Codec<T> innerCodec) => new(innerCodec);

    private readonly Codec<T> InnerCodec;

    public override Dictionary<string, T> Decode<TData>(CodecMap<TData> map, TData encoding) {
        Dictionary<string, TData> dict = map.ReadObject(encoding);
        return new(dict.Select(item => KeyValuePair.Create(item.Key, InnerCodec.Decode(map, item.Value))));
    }
    public override TData Encode<TData>(CodecMap<TData> map, Dictionary<string, T> items) {
        Dictionary<string, TData> dict = new(items.Select(item => KeyValuePair.Create(item.Key, InnerCodec.Encode(map, item.Value))));
        return map.WriteObject(dict);
    }
}
public class DictCodec<TKey, TValue> : PrimitiveCodec<Dictionary<TKey, TValue>> {
    private DictCodec(Codec<TValue> innerCodec, Func<TKey, string> keyEncoding, Func<string, TKey> keyDecoding) { InnerCodec = innerCodec; KeyEncoding = keyEncoding; KeyDecoding = keyDecoding; }
    public static DictCodec<TKey, TValue> Create(Codec<TValue> innerCodec, Func<TKey, string> keyEncoding, Func<string, TKey> keyDecoding) => new(innerCodec, keyEncoding, keyDecoding);

    private readonly Codec<TValue> InnerCodec;
    private readonly Func<TKey, string> KeyEncoding;
    private readonly Func<string, TKey> KeyDecoding;

    public override Dictionary<TKey, TValue> Decode<TData>(CodecMap<TData> map, TData encoding) {
        Dictionary<string, TData> dict = map.ReadObject(encoding);
        return new(dict.Select(item => KeyValuePair.Create(KeyDecoding(item.Key), InnerCodec.Decode(map, item.Value))));
    }
    public override TData Encode<TData>(CodecMap<TData> map, Dictionary<TKey, TValue> items) {
        Dictionary<string, TData> dict = new(items.Select(item => KeyValuePair.Create(KeyEncoding(item.Key), InnerCodec.Encode(map, item.Value))));
        return map.WriteObject(dict);
    }
}
public class CatalogueCodec<TKey, TValue> : PrimitiveCodec<Dictionary<TKey, TValue>> {
    private CatalogueCodec(Dictionary<TKey, Codec<TValue>> innerCodecCatalogue, Func<TKey, string> keyEncoding, Func<string, TKey> keyDecoding) { InnerCodecCatalogue = innerCodecCatalogue; KeyEncoding = keyEncoding; KeyDecoding = keyDecoding; }
    public static CatalogueCodec<TKey, TValue> Create(Dictionary<TKey, Codec<TValue>> innerCodecCatalogue, Func<TKey, string> keyEncoding, Func<string, TKey> keyDecoding) => new(innerCodecCatalogue, keyEncoding, keyDecoding);

    private readonly Dictionary<TKey, Codec<TValue>> InnerCodecCatalogue;
    private readonly Func<TKey, string> KeyEncoding;
    private readonly Func<string, TKey> KeyDecoding;

    public override Dictionary<TKey, TValue> Decode<TData>(CodecMap<TData> map, TData encoding) {
        Dictionary<string, TData> dict = map.ReadObject(encoding);
        return new(dict.Select(item => {
            TKey key = KeyDecoding(item.Key);
            if (!InnerCodecCatalogue.TryGetValue(key, out Codec<TValue> value))
                throw new Exception("Failed to decode key from catalogue: " + item.Key);
            return KeyValuePair.Create(key, value.Decode(map, item.Value));
        }));
    }
    public override TData Encode<TData>(CodecMap<TData> map, Dictionary<TKey, TValue> items) {
        Dictionary<string, TData> dict = new(items.Select(item => {
            if (!InnerCodecCatalogue.TryGetValue(item.Key, out Codec<TValue> value))
                throw new Exception("Failed to encode key from catalogue: " + KeyEncoding(item.Key));
            return KeyValuePair.Create(KeyEncoding(item.Key), value.Encode(map, item.Value));
        }));
        return map.WriteObject(dict);
    }
}
public class ListOrDictCodec<T> : PrimitiveCodec<Tuple<List<T>, Dictionary<string, T>, bool>> {
    private ListOrDictCodec(Codec<T> innerCodec) { InnerCodec = innerCodec; }
    public static ListOrDictCodec<T> Create(Codec<T> innerCodec) => new(innerCodec);

    private readonly Codec<T> InnerCodec;

    public override Tuple<List<T>, Dictionary<string, T>, bool> Decode<TData>(CodecMap<TData> map, TData encoding) {
        try {
            var dict = map.ReadObject(encoding);
            return new([], new(dict.Select(item => KeyValuePair.Create(item.Key, InnerCodec.Decode(map, item.Value)))), true);
        } catch (Exception) { }
        var list = map.ReadList(encoding);
        return new([.. list.Select(data => InnerCodec.Decode(map, data))], [], false);
    }

    public override TData Encode<TData>(CodecMap<TData> map, Tuple<List<T>, Dictionary<string, T>, bool> items) {
        if (items.Item3) {
            Dictionary<string, TData> dict = new(items.Item2.Select(item => KeyValuePair.Create(item.Key, InnerCodec.Encode(map, item.Value))));
            return map.WriteObject(dict);
        } else {
            List<TData> list = [.. items.Item1.Select(item => InnerCodec.Encode(map, item))];
            return map.WriteList(list);
        }
        throw new NotImplementedException();
    }
}
public class EnumCodec<T> : PrimitiveCodec<T> where T : struct, Enum {
    private EnumCodec(bool useTextEncoding) { UseTextEncoding = useTextEncoding; }
    public static EnumCodec<T> Create(bool useTextEncoding = false) => new(useTextEncoding);

    readonly bool UseTextEncoding;

    public override T Decode<TData>(CodecMap<TData> map, TData encoding) {
        if (UseTextEncoding && map.IsHumanReadable()) {
            if (Enum.TryParse(map.ReadString(encoding), out T result))
                return result;
        }
        return (T)(object)map.ReadInt(encoding);
    }

    public override TData Encode<TData>(CodecMap<TData> map, T item) {
        if (UseTextEncoding && map.IsHumanReadable()) {
            return map.WriteString(item.ToString());
        }
        return map.WriteInt((int)(object)item);
    }
}
public class ConverterCodec<TResult, TOriginal> : PrimitiveCodec<TResult> {
    private ConverterCodec(Codec<TOriginal> innerCodec, Func<TResult, TOriginal> encodingConversion, Func<TOriginal, TResult> decodingConversion) { InnerCodec = innerCodec; EncodingConversion = encodingConversion; DecodingConversion = decodingConversion; }
    public static ConverterCodec<TResult, TOriginal> Create(Codec<TOriginal> innerCodec, Func<TResult, TOriginal> encodingConversion, Func<TOriginal, TResult> decodingConversion) => new(innerCodec, encodingConversion, decodingConversion);

    private readonly Codec<TOriginal> InnerCodec;
    private readonly Func<TResult, TOriginal> EncodingConversion;
    private readonly Func<TOriginal, TResult> DecodingConversion;

    public override TResult Decode<TData>(CodecMap<TData> map, TData encoding) {
        return DecodingConversion(InnerCodec.Decode(map, encoding));
    }
    public override TData Encode<TData>(CodecMap<TData> map, TResult item) {
        return InnerCodec.Encode(map, EncodingConversion(item));
    }
}

internal class IdentifierCodec : PrimitiveCodec<Identifier> {
    public override Identifier Decode<TData>(CodecMap<TData> map, TData encoding) {
        return map.ReadString(encoding);
    }
    public override TData Encode<TData>(CodecMap<TData> map, Identifier item) {
        return map.WriteString(item);
    }
}
internal class AtomTypeCodec : PrimitiveCodec<AtomType> {
    public override AtomType Decode<TData>(CodecMap<TData> map, TData encoding) {
        return AtomTypes.GetByID(map.ReadString(encoding));
    }
    public override TData Encode<TData>(CodecMap<TData> map, AtomType item) {
        return map.WriteString(item.Id);
    }
}
internal class PartTypeCodec : PrimitiveCodec<PartType> {
    public override PartType Decode<TData>(CodecMap<TData> map, TData encoding) {
        Identifier id = map.ReadString(encoding);
        return PartTypes.partTypes.First(part => part.Id == id);
    }
    public override TData Encode<TData>(CodecMap<TData> map, PartType item) {
        return map.WriteString(item.Id);
    }
}
internal class MLocStringCodec : PrimitiveCodec<Maybe<LocString>> {
    public override Maybe<LocString> Decode<TData>(CodecMap<TData> map, TData encoding) {
        var key = map.ReadString(encoding);
        if (string.IsNullOrEmpty(key)) return MaybeHelper.empty;
        return Translations.TranslateByType(key);
    }
    public override TData Encode<TData>(CodecMap<TData> map, Maybe<LocString> item) {
        if (!item.HasValue()) return map.WriteString("");
        return map.WriteString(item.GetValue().Key);
    }
}
internal class LocStringCodec : PrimitiveCodec<LocString> {
    public override LocString Decode<TData>(CodecMap<TData> map, TData encoding) {
        var key = map.ReadString(encoding);
        return Translations.TranslateByType(key);
    }
    public override TData Encode<TData>(CodecMap<TData> map, LocString item) {
        return map.WriteString(item.Key);
    }
}

internal class ModMetaCodec : PrimitiveCodec<ModMeta> {
    public override ModMeta Decode<TData>(CodecMap<TData> map, TData encoding) {
        return QuintessentialLoader.ModById(map.ReadString(encoding));
    }
    public override TData Encode<TData>(CodecMap<TData> map, ModMeta item) {
        return map.WriteString(item.ModId);
    }
}
internal class VersionCodec : PrimitiveCodec<Version> {
    public override Version Decode<TData>(CodecMap<TData> map, TData encoding) {
        return Version.Parse(map.ReadString(encoding));
    }
    public override TData Encode<TData>(CodecMap<TData> map, Version item) {
        return map.WriteString(item.ToString());
    }
}
internal class VersionRangeCodec : PrimitiveCodec<VersionRange> {
    public override VersionRange Decode<TData>(CodecMap<TData> map, TData encoding) {
        return VersionRange.Parse(map.ReadString(encoding));
    }
    public override TData Encode<TData>(CodecMap<TData> map, VersionRange item) {
        return map.WriteString(item.ToString());
    }
}
