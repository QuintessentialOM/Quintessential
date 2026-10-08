using System;
using System.Collections;
using System.Collections.Generic;

namespace Quintessential.Serialization;

public class SealedCodec<TReturn, T> {
    readonly string PropertyName;
    readonly Func<T, TReturn> Getter;
    readonly Codec<TReturn> Codec;
    Maybe<TReturn> DefautValue;
    public bool IsDefaultNull = false;
    Maybe<Func<T, bool>> DefautValueSkip;
    public bool IsReadOnly = false;

    public virtual KeyValuePair<string, TData> EncodeAsProperty<TData>(CodecMap<TData> map, T item) {
        var ret = Getter(item);
        if (IsReadOnly || IsDefaultNull && ret == null)
            return KeyValuePair.Create("", default(TData)); // Discarded value, don't add to serialized file
        if (DefautValue.HasValue() && (!map.IsHumanReadable() || !DefautValueSkip.HasValue() || !DefautValueSkip.GetValue().Invoke(item))) {
            if (ret is ICollection collection && DefautValue.GetValue() is ICollection defColl) {
                if (collection.Count == defColl.Count) {
                    var retEn = collection.GetEnumerator();
                    var defEn = defColl.GetEnumerator();
                    bool equals = true;
                    while (retEn.MoveNext() && defEn.MoveNext()) {
                        if (!retEn.Current.Equals(defEn.Current)) {
                            equals = false;
                            break;
                        }
                    }
                    if (equals)
                        return KeyValuePair.Create("", default(TData)); // Discarded value, don't add to serialized file
                }
            }
            if (ret.Equals(DefautValue.GetValue()))
                return KeyValuePair.Create("", default(TData)); // Discarded value, don't add to serialized file
        }
        return KeyValuePair.Create(PropertyName, Codec.Encode(map, ret));
    }
    public virtual TReturn DecodeAsProperty<TData>(CodecMap<TData> map, Dictionary<string, TData> obj) {
        TData data;
        if (DefautValue.HasValue()) {
            if (!obj.TryGetValue(PropertyName, out data))
                return DefautValue.GetValue();
        } else
            data = obj[PropertyName];
        return Codec.Decode(map, data);
    }

    internal SealedCodec(Codec<TReturn> codec, string propertyName, Func<T, TReturn> getter) {
        Getter = getter;
        PropertyName = propertyName;
        Codec = codec;
        DefautValue = MaybeHelper.empty;
        DefautValueSkip = MaybeHelper.empty;
    }
    internal SealedCodec(Codec<TReturn> codec, string propertyName, Func<T, TReturn> getter, TReturn? defautValue) {
        Getter = getter;
        PropertyName = propertyName;
        Codec = codec;
        WithDefaut(defautValue);
    }
    public SealedCodec<TReturn, T> WithDefaut(TReturn? defautValue) {
        if (DefautValue.HasValue()) throw new InvalidOperationException("The Codec already has a default value!");
        DefautValue = new Maybe<TReturn>(hasValue: true, defautValue);
        IsDefaultNull = defautValue == null;
        return this;
    }
    public SealedCodec<TReturn, T> WriteDefautIf(Func<T, bool> predicate) {
        if (DefautValueSkip.HasValue()) throw new InvalidOperationException("The Codec already has a default value predicate!");
        DefautValueSkip = predicate;
        return this;
    }
    public SealedCodec<TReturn, T> AsReadOnly() {
        IsReadOnly = true;
        return this;
    }
}