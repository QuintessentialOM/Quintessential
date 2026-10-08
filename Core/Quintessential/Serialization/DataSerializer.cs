using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quintessential.Serialization;

/// <summary>
/// A class to serialize objects into data files. As of now it<br/>
/// supports writing and reading <c>json</c>, <c>jsonc</c>, and <c>yaml</c> files.
/// </summary>
public static class DataSerializer {

    private static bool MultilineFormat;
    internal static bool WasInit = false;

    private static readonly JsonSerializerOptions initOptions = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        AllowTrailingCommas = true,
    };
    private static readonly JsonSerializerOptions compactOptions = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        AllowTrailingCommas = true,
        WriteIndented = false,
    };
    private static readonly JsonSerializerOptions multilineOptions = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        AllowTrailingCommas = true,
        WriteIndented = true,
    };

    public static void SetMultilineFormat(bool multilineFormat) {
        MultilineFormat = multilineFormat;
    }
    public static void AssignConverter(JsonConverter converter) {
        compactOptions.Converters.Add(converter);
        multilineOptions.Converters.Add(converter);
    }

    public static object Deserialize(string filePath, Type type) {
        try {
            string filename = Path.GetFileName(filePath);

            if (filename.EndsWith(".yaml")) {
                using StreamReader reader = new(filePath);
                return YamlHelper.Deserializer.Deserialize(reader, type);
            }

            if (filename.EndsWith(".json") || filename.EndsWith(".jsonc")) {
                string data = File.ReadAllText(filePath, Encoding.UTF8);
                if (filename.EndsWith(".jsonc")) data = JsoncToJson(data);

                return JsonSerializer.Deserialize(data, type, GetCurrentOption());
            }

        } catch (Exception ex) {
            throw new SerializationException("Failed to deserialize file: " + filePath, ex);
        }
        throw new SerializationException("Invalid file extension at: " + filePath);
    }
    public static T Deserialize<T>(string filePath) {
        try {
            string filename = Path.GetFileName(filePath);

            if (filename.EndsWith(".yaml")) {
                using StreamReader reader = new(filePath);
                return YamlHelper.Deserializer.Deserialize<T>(reader);
            }

            if (filename.EndsWith(".json") || filename.EndsWith(".jsonc")) {
                string data = File.ReadAllText(filePath, Encoding.UTF8);
                if (filename.EndsWith(".jsonc")) data = JsoncToJson(data);

                return JsonSerializer.Deserialize<T>(data, GetCurrentOption());
            }

        } catch (Exception ex) {
            throw new SerializationException("Failed to deserialize file: " + filePath, ex);
        }
        throw new SerializationException("Invalid file extension at: " + filePath);
    }
    public static T Deserialize<T>(Stream fileDataStream, string filePath) {
        try {
            string filename = Path.GetFileName(filePath);

            if (filename.EndsWith(".yaml")) {
                using StreamReader reader = new(fileDataStream);
                return YamlHelper.Deserializer.Deserialize<T>(reader);
            }

            if (filename.EndsWith(".jsonc") || filename.EndsWith(".jsonc")) {
                using var reader = new StreamReader(fileDataStream, Encoding.UTF8);
                string data = reader.ReadToEnd();
                if (filename.EndsWith(".jsonc")) data = JsoncToJson(data);

                return JsonSerializer.Deserialize<T>(data, GetCurrentOption());
            }

        } catch (Exception ex) {
            throw new SerializationException("Failed to deserialize file: " + filePath, ex);
        }
        throw new SerializationException("Invalid file extension while reading from stream.");
    }

    public static void Serialize<T>(this T data, string filePath) {
        try {
            string filename = Path.GetFileName(filePath);

            if (filename.EndsWith(".yaml")) {
                string serializedData = YamlHelper.Serializer.Serialize(data);
                File.WriteAllText(filePath, serializedData);
                return;
            }

            if (filename.EndsWith(".json") || filename.EndsWith(".jsonc")) {
                using FileStream fileStream = new(filePath, FileMode.OpenOrCreate);
                JsonSerializer.Serialize(fileStream, data, GetCurrentOption());
                return;
            }

        } catch (Exception ex) {
            throw new SerializationException("Failed to serialize file: " + filePath, ex);
        }
        throw new SerializationException("Invalid file extension while serializing: " + filePath);
    }

    public static string JsoncToJson(string jsoncData) {
        StringBuilder jsonData = new();

        bool isComment = false;
        bool isMultiLineComment = false;
        bool stringData = false;
        bool addedComma = false;
        string c = "";

        for (int i = 0; i < jsoncData.Length; i++) {
            string c0 = c;
            c = jsoncData.Substring(i, 1);

            if (stringData) {
                if (c == "\"" && c0 != "\\") stringData = false;
                jsonData.Append(c);
                continue;
            }
            if (isComment) {
                if (c == "\n") isComment = false;
                continue;
            }
            if (isMultiLineComment) {
                if (c0 == "*" && c == "/") isMultiLineComment = false;
                continue;
            }

            if (addedComma && (c == "]" || c == "}")) jsonData.Remove(jsonData.Length - 1, 1);
            if (c != " " && c != "\n" && c != "\r" && c != "\t") {
                jsonData.Append(c);
                addedComma = c == ",";
            }

            if (c == "\"") {
                stringData = true;
                addedComma = false;
            } else if (c0 == "/" && c == "/") {
                isComment = true;
                jsonData.Remove(jsonData.Length - 2, 2);
            } else if (c0 == "/" && c == "*") {
                isMultiLineComment = true;
                jsonData.Remove(jsonData.Length - 2, 2);
            }
        }
        return jsonData.ToString();
    }
    private static JsonSerializerOptions GetCurrentOption() {
        return WasInit ? MultilineFormat ? multilineOptions : compactOptions : initOptions;
    }

    public class SerializationException : Exception {
        public SerializationException(string message) : base(message) { }
        public SerializationException(string message, Exception innerException) : base(message, innerException) { }

    }
}
