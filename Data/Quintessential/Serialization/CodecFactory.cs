using System;
using System.Collections.Generic;
using System.Linq;

namespace Quintessential.Serialization;

public class CodecFactory {
    private static readonly ListOrDictCodec<Identifier> _tagInternal = ListOrDictCodec<Identifier>.Create(Codecs.ID);
    public static Codec<T> CreateTag<T>(Func<Identifier, bool, T> ctor) where T : Tag {
        return Codec<T>.Create(
            Codecs.ID.Seal("Id", (T t) => t.Id),
            Codecs.BOOL.Seal("IsTable", (T t) => t.IsTable),
            _tagInternal.Seal("Tags", (T t) => new(
                t.IsTable ? null : [.. t.Entries],
                t.IsTable ? new(t.TableEntries.Select(item => KeyValuePair.Create((string)item.Key, item.Value.Value))) : null,
                t.IsTable)
            ),
            (id, isTable, elements) => {
                T tag = ctor(id, isTable);
                if (isTable) {
                    foreach (var items in elements.Item2)
                        tag.Add(items.Key, items.Value);
                } else
                    foreach (var items in elements.Item1)
                        tag.Add(items);
                return tag;
            }
        );
    }
}
