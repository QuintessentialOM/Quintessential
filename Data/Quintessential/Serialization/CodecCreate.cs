using System;

namespace Quintessential.Serialization;

public partial class Codec<T> {
    public static Codec<T> Create<T1>(SealedCodec<T1, T> codec1, Func<T1, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2>(SealedCodec<T1, T> codec1, SealedCodec<T2, T> codec2, Func<T1, T2, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        Func<T1, T2, T3, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        Func<T1, T2, T3, T4, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4, T5>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        SealedCodec<T5, T> codec5,
        Func<T1, T2, T3, T4, T5, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t),
                    codec5.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj),
                    codec5.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4, T5, T6>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        SealedCodec<T5, T> codec5,
        SealedCodec<T6, T> codec6,
        Func<T1, T2, T3, T4, T5, T6, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t),
                    codec5.EncodeAsProperty(map, t),
                    codec6.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj),
                    codec5.DecodeAsProperty(map, obj),
                    codec6.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4, T5, T6, T7>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        SealedCodec<T5, T> codec5,
        SealedCodec<T6, T> codec6,
        SealedCodec<T7, T> codec7,
        Func<T1, T2, T3, T4, T5, T6, T7, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t),
                    codec5.EncodeAsProperty(map, t),
                    codec6.EncodeAsProperty(map, t),
                    codec7.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj),
                    codec5.DecodeAsProperty(map, obj),
                    codec6.DecodeAsProperty(map, obj),
                    codec7.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        SealedCodec<T5, T> codec5,
        SealedCodec<T6, T> codec6,
        SealedCodec<T7, T> codec7,
        SealedCodec<T8, T> codec8,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t),
                    codec5.EncodeAsProperty(map, t),
                    codec6.EncodeAsProperty(map, t),
                    codec7.EncodeAsProperty(map, t),
                    codec8.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj),
                    codec5.DecodeAsProperty(map, obj),
                    codec6.DecodeAsProperty(map, obj),
                    codec7.DecodeAsProperty(map, obj),
                    codec8.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        SealedCodec<T5, T> codec5,
        SealedCodec<T6, T> codec6,
        SealedCodec<T7, T> codec7,
        SealedCodec<T8, T> codec8,
        SealedCodec<T9, T> codec9,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t),
                    codec5.EncodeAsProperty(map, t),
                    codec6.EncodeAsProperty(map, t),
                    codec7.EncodeAsProperty(map, t),
                    codec8.EncodeAsProperty(map, t),
                    codec9.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj),
                    codec5.DecodeAsProperty(map, obj),
                    codec6.DecodeAsProperty(map, obj),
                    codec7.DecodeAsProperty(map, obj),
                    codec8.DecodeAsProperty(map, obj),
                    codec9.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        SealedCodec<T5, T> codec5,
        SealedCodec<T6, T> codec6,
        SealedCodec<T7, T> codec7,
        SealedCodec<T8, T> codec8,
        SealedCodec<T9, T> codec9,
        SealedCodec<T10, T> codec10,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t),
                    codec5.EncodeAsProperty(map, t),
                    codec6.EncodeAsProperty(map, t),
                    codec7.EncodeAsProperty(map, t),
                    codec8.EncodeAsProperty(map, t),
                    codec9.EncodeAsProperty(map, t),
                    codec10.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj),
                    codec5.DecodeAsProperty(map, obj),
                    codec6.DecodeAsProperty(map, obj),
                    codec7.DecodeAsProperty(map, obj),
                    codec8.DecodeAsProperty(map, obj),
                    codec9.DecodeAsProperty(map, obj),
                    codec10.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        SealedCodec<T5, T> codec5,
        SealedCodec<T6, T> codec6,
        SealedCodec<T7, T> codec7,
        SealedCodec<T8, T> codec8,
        SealedCodec<T9, T> codec9,
        SealedCodec<T10, T> codec10,
        SealedCodec<T11, T> codec11,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t),
                    codec5.EncodeAsProperty(map, t),
                    codec6.EncodeAsProperty(map, t),
                    codec7.EncodeAsProperty(map, t),
                    codec8.EncodeAsProperty(map, t),
                    codec9.EncodeAsProperty(map, t),
                    codec10.EncodeAsProperty(map, t),
                    codec11.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj),
                    codec5.DecodeAsProperty(map, obj),
                    codec6.DecodeAsProperty(map, obj),
                    codec7.DecodeAsProperty(map, obj),
                    codec8.DecodeAsProperty(map, obj),
                    codec9.DecodeAsProperty(map, obj),
                    codec10.DecodeAsProperty(map, obj),
                    codec11.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        SealedCodec<T5, T> codec5,
        SealedCodec<T6, T> codec6,
        SealedCodec<T7, T> codec7,
        SealedCodec<T8, T> codec8,
        SealedCodec<T9, T> codec9,
        SealedCodec<T10, T> codec10,
        SealedCodec<T11, T> codec11,
        SealedCodec<T12, T> codec12,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t),
                    codec5.EncodeAsProperty(map, t),
                    codec6.EncodeAsProperty(map, t),
                    codec7.EncodeAsProperty(map, t),
                    codec8.EncodeAsProperty(map, t),
                    codec9.EncodeAsProperty(map, t),
                    codec10.EncodeAsProperty(map, t),
                    codec11.EncodeAsProperty(map, t),
                    codec12.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj),
                    codec5.DecodeAsProperty(map, obj),
                    codec6.DecodeAsProperty(map, obj),
                    codec7.DecodeAsProperty(map, obj),
                    codec8.DecodeAsProperty(map, obj),
                    codec9.DecodeAsProperty(map, obj),
                    codec10.DecodeAsProperty(map, obj),
                    codec11.DecodeAsProperty(map, obj),
                    codec12.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        SealedCodec<T5, T> codec5,
        SealedCodec<T6, T> codec6,
        SealedCodec<T7, T> codec7,
        SealedCodec<T8, T> codec8,
        SealedCodec<T9, T> codec9,
        SealedCodec<T10, T> codec10,
        SealedCodec<T11, T> codec11,
        SealedCodec<T12, T> codec12,
        SealedCodec<T13, T> codec13,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t),
                    codec5.EncodeAsProperty(map, t),
                    codec6.EncodeAsProperty(map, t),
                    codec7.EncodeAsProperty(map, t),
                    codec8.EncodeAsProperty(map, t),
                    codec9.EncodeAsProperty(map, t),
                    codec10.EncodeAsProperty(map, t),
                    codec11.EncodeAsProperty(map, t),
                    codec12.EncodeAsProperty(map, t),
                    codec13.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj),
                    codec5.DecodeAsProperty(map, obj),
                    codec6.DecodeAsProperty(map, obj),
                    codec7.DecodeAsProperty(map, obj),
                    codec8.DecodeAsProperty(map, obj),
                    codec9.DecodeAsProperty(map, obj),
                    codec10.DecodeAsProperty(map, obj),
                    codec11.DecodeAsProperty(map, obj),
                    codec12.DecodeAsProperty(map, obj),
                    codec13.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        SealedCodec<T5, T> codec5,
        SealedCodec<T6, T> codec6,
        SealedCodec<T7, T> codec7,
        SealedCodec<T8, T> codec8,
        SealedCodec<T9, T> codec9,
        SealedCodec<T10, T> codec10,
        SealedCodec<T11, T> codec11,
        SealedCodec<T12, T> codec12,
        SealedCodec<T13, T> codec13,
        SealedCodec<T14, T> codec14,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t),
                    codec5.EncodeAsProperty(map, t),
                    codec6.EncodeAsProperty(map, t),
                    codec7.EncodeAsProperty(map, t),
                    codec8.EncodeAsProperty(map, t),
                    codec9.EncodeAsProperty(map, t),
                    codec10.EncodeAsProperty(map, t),
                    codec11.EncodeAsProperty(map, t),
                    codec12.EncodeAsProperty(map, t),
                    codec13.EncodeAsProperty(map, t),
                    codec14.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj),
                    codec5.DecodeAsProperty(map, obj),
                    codec6.DecodeAsProperty(map, obj),
                    codec7.DecodeAsProperty(map, obj),
                    codec8.DecodeAsProperty(map, obj),
                    codec9.DecodeAsProperty(map, obj),
                    codec10.DecodeAsProperty(map, obj),
                    codec11.DecodeAsProperty(map, obj),
                    codec12.DecodeAsProperty(map, obj),
                    codec13.DecodeAsProperty(map, obj),
                    codec14.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        SealedCodec<T5, T> codec5,
        SealedCodec<T6, T> codec6,
        SealedCodec<T7, T> codec7,
        SealedCodec<T8, T> codec8,
        SealedCodec<T9, T> codec9,
        SealedCodec<T10, T> codec10,
        SealedCodec<T11, T> codec11,
        SealedCodec<T12, T> codec12,
        SealedCodec<T13, T> codec13,
        SealedCodec<T14, T> codec14,
        SealedCodec<T15, T> codec15,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t),
                    codec5.EncodeAsProperty(map, t),
                    codec6.EncodeAsProperty(map, t),
                    codec7.EncodeAsProperty(map, t),
                    codec8.EncodeAsProperty(map, t),
                    codec9.EncodeAsProperty(map, t),
                    codec10.EncodeAsProperty(map, t),
                    codec11.EncodeAsProperty(map, t),
                    codec12.EncodeAsProperty(map, t),
                    codec13.EncodeAsProperty(map, t),
                    codec14.EncodeAsProperty(map, t),
                    codec15.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj),
                    codec5.DecodeAsProperty(map, obj),
                    codec6.DecodeAsProperty(map, obj),
                    codec7.DecodeAsProperty(map, obj),
                    codec8.DecodeAsProperty(map, obj),
                    codec9.DecodeAsProperty(map, obj),
                    codec10.DecodeAsProperty(map, obj),
                    codec11.DecodeAsProperty(map, obj),
                    codec12.DecodeAsProperty(map, obj),
                    codec13.DecodeAsProperty(map, obj),
                    codec14.DecodeAsProperty(map, obj),
                    codec15.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
    public static Codec<T> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
        SealedCodec<T1, T> codec1,
        SealedCodec<T2, T> codec2,
        SealedCodec<T3, T> codec3,
        SealedCodec<T4, T> codec4,
        SealedCodec<T5, T> codec5,
        SealedCodec<T6, T> codec6,
        SealedCodec<T7, T> codec7,
        SealedCodec<T8, T> codec8,
        SealedCodec<T9, T> codec9,
        SealedCodec<T10, T> codec10,
        SealedCodec<T11, T> codec11,
        SealedCodec<T12, T> codec12,
        SealedCodec<T13, T> codec13,
        SealedCodec<T14, T> codec14,
        SealedCodec<T15, T> codec15,
        SealedCodec<T16, T> codec16,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T> ctor) {
        Codec<T> codec = new() {
            Encoder = (map, t) => {
                return map.WriteObject(
                    codec1.EncodeAsProperty(map, t),
                    codec2.EncodeAsProperty(map, t),
                    codec3.EncodeAsProperty(map, t),
                    codec4.EncodeAsProperty(map, t),
                    codec5.EncodeAsProperty(map, t),
                    codec6.EncodeAsProperty(map, t),
                    codec7.EncodeAsProperty(map, t),
                    codec8.EncodeAsProperty(map, t),
                    codec9.EncodeAsProperty(map, t),
                    codec10.EncodeAsProperty(map, t),
                    codec11.EncodeAsProperty(map, t),
                    codec12.EncodeAsProperty(map, t),
                    codec13.EncodeAsProperty(map, t),
                    codec14.EncodeAsProperty(map, t),
                    codec15.EncodeAsProperty(map, t),
                    codec16.EncodeAsProperty(map, t)
                );
            },
            Decoder = (map, encoding) => {
                dynamic obj = map.ReadObject(encoding); //> Dictionary<string, TData>
                return ctor(
                    codec1.DecodeAsProperty(map, obj),
                    codec2.DecodeAsProperty(map, obj),
                    codec3.DecodeAsProperty(map, obj),
                    codec4.DecodeAsProperty(map, obj),
                    codec5.DecodeAsProperty(map, obj),
                    codec6.DecodeAsProperty(map, obj),
                    codec7.DecodeAsProperty(map, obj),
                    codec8.DecodeAsProperty(map, obj),
                    codec9.DecodeAsProperty(map, obj),
                    codec10.DecodeAsProperty(map, obj),
                    codec11.DecodeAsProperty(map, obj),
                    codec12.DecodeAsProperty(map, obj),
                    codec13.DecodeAsProperty(map, obj),
                    codec14.DecodeAsProperty(map, obj),
                    codec15.DecodeAsProperty(map, obj),
                    codec16.DecodeAsProperty(map, obj)
                );
            },
        };
        return codec;
    }
}
