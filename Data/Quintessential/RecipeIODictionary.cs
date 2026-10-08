using System;
using System.Collections.Generic;

namespace Quintessential;

///  <inheritdoc cref="RecipeIODictionary{TKey, TValue}"/>
public class RecipeInputDictionary<TKey, TValue> : RecipeIODictionary<TKey, TValue> where TValue : IRecipeInput {
    protected override string Name { get; init; } = "inputs";
}
///  <inheritdoc cref="RecipeIODictionary{TKey, TValue}"/>
public class RecipeOutputDictionary<TKey, TValue> : RecipeIODictionary<TKey, TValue> where TValue : IRecipeOutput {
    protected override string Name { get; init; } = "outputs";
}

/// <summary>
/// A dictionary with better recipe-specific error handling.
/// </summary>
public abstract class RecipeIODictionary<TKey, TValue> : Dictionary<TKey, TValue> {
    protected abstract string Name { get; init; }
    public GlyphRecipe recipe;

    public new TValue this[TKey key] {
        get {
            try {
                return base[key];
            } catch (KeyNotFoundException e) {
                throw new Exception($"Failed to find IO for recipe '{recipe?.RecipeId ?? "null"}' in '{Name}' at '{key}'", e);
            }
        }
        set {
            base[key] = value;
        }
    }
}
