using System.Collections.Generic;

namespace Quintessential;

/// <summary>
/// A recipe representing a set of allowed transmutations for a glyph.
/// </summary>
public class GlyphRecipe {

    public GlyphRecipe() { }
    public GlyphRecipe(RecipePredicate predicate) { Predicate = predicate; }

    /// <summary>
    /// The collection of all recipes sorted by glyph then recipe <see cref="Identifier"/>.
    /// </summary>
    public static Dictionary<Identifier, OrderedDictionary<Identifier, GlyphRecipe>> Recipes = [];

    /// <summary>
    /// The <see cref="Identifier"/> of the specific recipe.
    /// </summary>
    public Identifier RecipeId;
    /// <summary>
    /// The <see cref="Identifier"/> of the glyph the recipe is for.
    /// </summary>
    public Identifier RecipeGlyphId;
    /// <summary>
    /// A predicate to check whether the recipe applies and to set the inputs and outputs.
    /// </summary>
    public RecipePredicate Predicate;
    /// <summary>
    /// Custom per-glyph specific data to be used.
    /// </summary>
    public object CustomData;

}

// TODO link to online documentation
/// <summary>
/// A predicate to check whether the recipe applies and to set the inputs and outputs.<br/>
/// The first, and only the first invocation of this delegate, should be a<br/>
/// <see cref="QApi.InvokeAndClear(RecipePredicate, patch_Sim, Part)"/> in order to clear the inputs and outputs lists.
/// </summary>
/// <param name="sim">The current simulation.</param>
/// <param name="part">The part that's running the transmutation.</param>
/// <returns>If the recipe is a success, the glyph should process the inputs into the outputs.</returns>
public delegate bool RecipePredicate(patch_Sim sim, Part part);
