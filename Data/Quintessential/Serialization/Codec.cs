using System;

namespace Quintessential.Serialization;

/* This code uses -dynamic-!
 * Why?
 * Although dynamic is horrible all my research led me to believe that
 * generic lambda function can't be created. E.g.:
 * //> private Func<CodecMap<TData>, TData, T> Decoder<TData>;
 * We'd need such a type, sice TData isn't known when the Codec is being constructed.
 * TData comes from Encode/Decode.
 * In an ideal world such a generic lambda could be made.
 * This means that the type of the -dynamic- parameters is actually known,
 * it just has to be hidden from the compiler.
 * Still: // TODO: Remove dynamic somehow!
 */

/// <summary>
/// A class for encoding-decoding and object into various formats.
/// </summary>
/// <typeparam name="T">The type of object to encode and decode.</typeparam>
public partial class Codec<T> {
    protected internal Codec() { }
    private Func<dynamic, T, dynamic> Encoder; // aguments of //> Func<CodecMap<TData>, T, TData>
    private Func<dynamic, dynamic, T> Decoder; // aguments of //> Func<CodecMap<TData>, TData, T>

    /// <summary>
    /// Encode the object into the dataformat provided by the map.
    /// </summary>
    /// <typeparam name="TData">The data type.</typeparam>
    /// <param name="map">The CodecMap providing the format to encode the object to.<br/>See <see cref="JsonCodecMap"/> for an example.</param>
    /// <param name="item">The object to encode.</param>
    /// <returns>The resulting encoded data.</returns>
    public virtual TData Encode<TData>(CodecMap<TData> map, T item) {
        return Encoder(map, item);
    }
    /// <summary>
    /// Decode the object from the dataformat provided by the map.
    /// </summary>
    /// <typeparam name="TData">The data type.</typeparam>
    /// <param name="map">The CodecMap providing the format to decode the object from.<br/>See <see cref="JsonCodecMap"/> for an example.</param>
    /// <param name="encoding">The endoded versiom of the object.</param>
    /// <returns>An object generated from the encoded data.</returns>
    public virtual T Decode<TData>(CodecMap<TData> map, TData encoding) {
        return Decoder(map, encoding);
    }

    /// <summary>
    /// Seal the codec into a properity on an object.
    /// </summary>
    /// <typeparam name="TTransform">The type of the object providing the value.</typeparam>
    /// <param name="properityName">The name of the properity.</param>
    /// <param name="getter">A function to get the coded value from another object.</param>
    /// <returns>A sealed codec used for creating complex codecs.</returns>
    public SealedCodec<T, TTransform> Seal<TTransform>(string properityName, Func<TTransform, T> getter) =>
        new(this, properityName, getter);
    /// <summary>
    /// Seal the codec into a properity on an object.
    /// </summary>
    /// <typeparam name="TTransform">The type of the object providing the value.</typeparam>
    /// <param name="properityName">The name of the properity.</param>
    /// <param name="getter">A function to get the coded value from another object.</param>
    /// <param name="defautValue">The default value of the properity.</param>
    /// <returns>A sealed codec used for creating complex codecs.</returns>
    public SealedCodec<T, TTransform> Seal<TTransform>(string properityName, Func<TTransform, T> getter, T? defautValue) =>
        new(this, properityName, getter, defautValue);
}
