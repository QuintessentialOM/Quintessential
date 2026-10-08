using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quintessential;
/// <summary>
/// A general collection of  <see cref="Identifier"/>s.
/// </summary>
public abstract class Tag {

    /// <summary>
    /// The <see cref="Identifier"/> of the tag
    /// </summary>
    public readonly Identifier Id;
    /// <summary>
    /// Whether the tag is a table-tag
    /// </summary>
    public readonly bool IsTable;
    /// <summary>
    /// The entries of a non-table tag.
    /// </summary>
    public readonly HashSet<Identifier> Entries;
    /// <summary>
    /// The entries of a table tag.
    /// </summary>
    public readonly Dictionary<Identifier, Identifier?> TableEntries;

    /// <summary>
    /// Create a new tag.
    /// </summary>
    /// <param name="id">The id of the new tag.</param>
    /// <param name="isTable">Whether the tag should be a table-tag.</param>
    /// <exception cref="Exception">If table tag name has wron formatting.</exception>
    public Tag(Identifier id, bool isTable = false) {
        Id = id;
        IsTable = isTable;
        if (isTable)    // Do not init unused fields
            TableEntries = [];
        else Entries = [];
        if (IsTable && !id.name.StartsWith('$')) throw new Exception("Tag-table names must start with '$', found violation in: " + Id);
        else if (!IsTable && id.name.StartsWith('$')) throw new Exception("Non Tag-table names must not start with '$', found violation in: " + Id);
    }

    /// <summary>
    /// Dumps all give tags of type <typeparamref name="T"/>, in the file <paramref name="fileName"/>, to
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tags">The Tag-dictionary of the type to dump.</param>
    /// <param name="fileName">The file to dump the tags to.</param>
    public static void DumpTags<T>(Dictionary<Identifier, T> tags, string fileName) where T : Tag { // "tags.jsonc"
        DataSerializer.SetMultilineFormat(true);
        string outDir = Path.Combine(QuintessentialLoader.PathModSaves, "Quintessential", "DumpedTags");
        Directory.CreateDirectory(outDir);
        tags.Serialize(Path.Combine(outDir, fileName));
    }

    /// <summary>
    /// Whether the <see cref="Tag"/> has a specific <see cref="Identifier"/>.
    /// </summary>
    /// <param name="id">The <see cref="Identifier"/> to check for.</param>
    public bool HasEntry(Identifier id) {
        return IsTable ? TableEntries.ContainsKey(id) : Entries.Contains(id);
    }
    /// <summary>
    /// <inheritdoc cref="HasEntry(Identifier)"/><br/>
    /// Additionally returns the mapped value from table tags or null if the tag isn't one.
    /// </summary>
    /// <param name="id"><inheritdoc cref="HasEntry(Identifier)"/></param>
    /// <param name="mapped">The value returned from the table or null for non table tags.</param>
    public bool HasEntry(Identifier id, out Identifier? mapped) {
        mapped = IsTable && TableEntries.TryGetValue(id, out Identifier? value) ? value : null;
        return IsTable ? TableEntries.ContainsKey(id) : Entries.Contains(id);
    }
    /// <summary>
    /// Returns the mapped value from table tags or null if the tag isn't one.
    /// </summary>
    /// <param name="id"><inheritdoc cref="HasEntry(Identifier)"/></param>
    public Identifier? GetMapped(Identifier id) {
        return IsTable && TableEntries.TryGetValue(id, out Identifier? value) ? value : null;
    }

    /// <summary>
    /// Adds an <see cref="Identifier"/> to the table.
    /// </summary>
    /// <param name="id">The <see cref="Identifier"/> to add to the tag.</param>
    /// <param name="mapped">The mapped value in the table.<br/> Must be null for non table tags.</param>
    /// <exception cref="Exception">When <paramref name="mapped"/> is null for table-tags, or non null for non table ones.</exception>
    public void Add(Identifier id, Identifier? mapped = null) {
        if (IsTable ^ mapped != null) throw new Exception("When calling Tag.Add() the 'mapped' value should be null if and only if the tag isn't a table-tag.");
        if (IsTable) {
            TableEntries.Add(id, mapped);
        } else Entries.Add(id);
    }
    /// <summary>
    /// Removes the specified <see cref="Identifier"/>.
    /// </summary>
    /// <param name="id">The <see cref="Identifier"/> to remove.</param>
    public void Remove(Identifier id) {
        if (IsTable) TableEntries.Remove(id);
        else Entries.Remove(id);
    }
}

/// <summary>
/// A general converter for a <see cref="Tag"/> type into a json object with the standard format.<br/>
/// See <see cref="AtomTagJsonConverter"/> for a usecase.
/// </summary>
/// <typeparam name="T">The type of <see cref="Tag"/> to convert.</typeparam>
public abstract class TagJsonConverter<T> : JsonConverter<Dictionary<Identifier, T>> where T : Tag {
    private readonly Dictionary<Identifier, T> GlobalTags;
    private readonly Func<Identifier, bool, T> CtorForType;
    protected TagJsonConverter(Dictionary<Identifier, T> GlobalTags, Func<Identifier, bool, T> CtorForType) {
        this.GlobalTags = GlobalTags;
        this.CtorForType = CtorForType;
    }

    public override Dictionary<Identifier, T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject) {
            if (reader.TokenType == JsonTokenType.PropertyName) {
                ReadSingle(ref reader, GlobalTags);
                return GlobalTags;
            }
            throw new FormatException("Tags file must begin with '{'");
        }
        reader.Read();
        while (reader.TokenType != JsonTokenType.EndObject) {
            ReadSingle(ref reader, GlobalTags);
        }
        reader.Read();
        return GlobalTags;
    }
    public void ReadSingle(ref Utf8JsonReader reader, Dictionary<Identifier, T> tagSet) {

        string id = reader.GetString();
        reader.Read();
        bool isTable = reader.TokenType == JsonTokenType.StartObject;
        if (!tagSet.TryGetValue(id, out var toEdit)) {
            toEdit = CtorForType.Invoke(id, isTable);
            tagSet[id] = toEdit;
        }
        if (reader.TokenType == JsonTokenType.StartArray) {
            if (toEdit.IsTable) throw new JsonException($"Attempted to edit tag-table '{toEdit.Id}' as an non table");

            reader.Read();
            while (reader.TokenType != JsonTokenType.EndArray) {
                string element = reader.GetString();
                if (element.StartsWith('-')) {
                    if (toEdit.Entries.Contains(element[1..])) toEdit.Entries.Remove(element[1..]);
                } else {
                    toEdit.Entries.Add(element);
                }
                reader.Read();
            }
            reader.Read();
        } else if (isTable) {
            if (!toEdit.IsTable) throw new JsonException($"Attempted to edit a non tag-table '{toEdit.Id}' as one.");

            reader.Read();
            while (reader.TokenType != JsonTokenType.EndObject) {
                string element = reader.GetString();
                reader.Read();
                string mapped = null;
                if (reader.TokenType == JsonTokenType.String) mapped = reader.GetString();
                reader.Read();

                if (mapped == null) {
                    if (toEdit.TableEntries.ContainsKey(element)) toEdit.TableEntries.Remove(element);
                } else {
                    toEdit.TableEntries[element] = mapped;
                }
            }
            reader.Read();
        } else
            throw new JsonException($"Tag '{id}' contains unecpected characters.");
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<Identifier, T> tags, JsonSerializerOptions options) {
        writer.WriteStartObject();
        foreach (var tag in tags) {
            if (tag.Key != tag.Value.Id) throw new JsonException($"Tag id mismatch found, '{tag.Value.Id}' stored as '{tag.Key}'");

            writer.WritePropertyName(tag.Key);
            if (tag.Value.IsTable) {

                writer.WriteStartObject();
                foreach (var entry in tag.Value.TableEntries) {
                    writer.WriteString(entry.Key, entry.Value);
                }
                writer.WriteEndObject();
            } else {

                writer.WriteStartArray();
                foreach (var entry in tag.Value.Entries) {
                    writer.WriteStringValue(entry);
                }
                writer.WriteEndArray();
            }
        }
        writer.WriteEndObject();
    }
}
