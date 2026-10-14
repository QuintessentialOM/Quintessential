using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quintessential.Serialization;

public class VersionJsonConverter : JsonConverter<Version> {
    public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return Version.Parse(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }
}
public class VersionRangeJsonConverter : JsonConverter<VersionRange> {
    public override VersionRange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        try {
            return VersionRange.Parse(reader.GetString());
        } catch (Exception e) {
            throw new JsonException("Failed to Parse VersionRange: ", e);
        }
    }

    public override void Write(Utf8JsonWriter writer, VersionRange value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
        return;
    }
}
public class LocalisationLayerJsonConverter : JsonConverter<LocalisationLayer> {
    private System.Collections.Generic.Stack<LocalisationLayer> layerStack = [];

    public override LocalisationLayer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        
        if (layerStack.Count == 0) layerStack.Push( LocalisationLayer.GlobalLayer );

        if (reader.TokenType != JsonTokenType.StartObject) {
            if (reader.TokenType == JsonTokenType.String) {
                layerStack.Peek().locDictionary[LocalisationLayer.CurrentFileLanguage] = reader.GetString();
                reader.Read();
                return layerStack.Pop();
            }
            throw new FormatException();
        }

        reader.Read();
        var current = layerStack.Peek();
        while (reader.TokenType != JsonTokenType.EndObject) {
            string key = reader.GetString();
            reader.Read();

            if (key == "") {
                current.locDictionary[LocalisationLayer.CurrentFileLanguage] = reader.GetString();
                reader.Read();

            } else {
                if (current.subLayers.TryGetValue(key, out LocalisationLayer value)) layerStack.Push(value);
                else layerStack.Push(new());

                current.subLayers[key] = Read(ref reader, typeToConvert, options);
            }
        }
        reader.Read();
        return layerStack.Pop();
    }

    public override void Write(Utf8JsonWriter writer, LocalisationLayer value, JsonSerializerOptions options) {
        throw new NotImplementedException();
    }
}

public class BasicEnumJsonConverter<T> : JsonConverter<T> where T : Enum {
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var value = reader.GetString();
        for (int i = 1; i < typeToConvert.GetFields().Length; i++) {
            if (value == (typeToConvert.GetFields()[i].Name)) return (T)typeToConvert.GetFields()[i].GetRawConstantValue();
        }
        throw new JsonException($"The provided Enum ('{typeToConvert.Name}') value was not recognised as a valid value.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }
}
public class FlagEnumJsonConverter<T> : JsonConverter<T> where T : Enum {
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var values = reader.GetString().Split(", ");
        var fileds = typeToConvert.GetFields()[1..];
        int sum = 0;

        foreach (var value in values) {
            sum += (int)(fileds.SingleOrDefault(field => field.Name == value, null)?.GetRawConstantValue() ??
                throw new JsonException($"The provided Enum value ('{value}') value was not recognised as valid."));
        }

        return (T)(object)sum;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }
}
