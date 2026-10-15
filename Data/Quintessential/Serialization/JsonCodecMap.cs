using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Quintessential.Serialization;

public class JsonCodecMap : CodecMap<JsonNode> {
    public override bool IsHumanReadable() => true;
    private JsonCodecMap() { }
    public static JsonCodecMap Instance {
        get {
            field ??= new JsonCodecMap();
            return field;
        }
    }

    public override string ReadString(JsonNode data) {
        if (data is JsonValue value) {
            if (value.GetValueKind() != JsonValueKind.String)
                throw new Exception("Not String value: " + value.GetValueKind());
            return value.GetValue<string>();
        }
        throw new Exception("Not Value Json Node!");
    }

    public override int ReadInt(JsonNode data) {
        if (data is JsonValue value) {
            if (value.GetValueKind() != JsonValueKind.Number)
                throw new Exception("Not Number value: " + value.GetValueKind());
            return value.GetValue<int>();
        }
        throw new Exception("Not Value Json Node!");
    }

    public override long ReadLong(JsonNode data) {
        if (data is JsonValue value) {
            if (value.GetValueKind() != JsonValueKind.Number)
                throw new Exception("Not Number value: " + value.GetValueKind());
            return value.GetValue<long>();
        }
        throw new Exception("Not Value Json Node!");
    }

    public override float ReadFloat(JsonNode data) {
        if (data is JsonValue value) {
            if (value.GetValueKind() != JsonValueKind.Number)
                throw new Exception("Not Number value: " + value.GetValueKind());
            return value.GetValue<float>();
        }
        throw new Exception("Not Value Json Node!");
    }

    public override double ReadDouble(JsonNode data) {
        if (data is JsonValue value) {
            if (value.GetValueKind() != JsonValueKind.Number)
                throw new Exception("Not Number value: " + value.GetValueKind());
            return value.GetValue<double>();
        }
        throw new Exception("Not Value Json Node!");
    }

    public override bool ReadBoolean(JsonNode data) {
        if (data is JsonValue value) {
            if (value.GetValueKind() == JsonValueKind.True) return true;
            if (value.GetValueKind() == JsonValueKind.False) return false;
            throw new Exception("Not Boolean value: " + value.GetValueKind());
        }
        throw new Exception("Not Value Json Node!");
        throw new NotImplementedException();
    }

    public override byte ReadByte(JsonNode data) {
        if (data is JsonValue value) {
            if (value.GetValueKind() != JsonValueKind.Number)
                throw new Exception("Not Byte value: " + value.GetValueKind());
            return value.GetValue<byte>();
        }
        throw new Exception("Not Value Json Node!");
    }

    public override JsonNode WriteString(string value) {
        return JsonValue.Create(value);
    }

    public override JsonNode WriteInt(int value) {
        return JsonValue.Create(value);
    }

    public override JsonNode WriteLong(long value) {
        return JsonValue.Create(value);
    }

    public override JsonNode WriteFloat(float value) {
        return JsonValue.Create(value);
    }

    public override JsonNode WriteDouble(double value) {
        return JsonValue.Create(value);
    }

    public override JsonNode WriteBoolean(bool value) {
        return JsonValue.Create(value);
    }

    public override JsonNode WriteByte(byte value) {
        return JsonValue.Create(value);
    }

    public override List<JsonNode> ReadList(JsonNode data) {
        if (data is JsonArray array) {
            return [.. array];
        }
        throw new Exception("Not Array Json Node!");
    }

    public override JsonNode WriteList(List<JsonNode> value) {
        return new JsonArray([.. value]);
    }

    public override Dictionary<string, JsonNode> ReadObject(JsonNode data) {
        if (data is JsonObject obj) {
            return obj.ToDictionary();
        }
        throw new Exception("Not Object Json Node!");
    }

    public override JsonNode WriteObject(Dictionary<string, JsonNode> value) {
        return new JsonObject(value);
    }
}
