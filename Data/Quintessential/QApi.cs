using Quintessential.Components;
using System;

namespace Quintessential;
public static class QApi {

    // TODO link to online documentation
    /// <summary>
    /// Adds a recipe to the game.
    /// </summary>
	/// <param name="mod">The mod that adds the recipe.</param>
    /// <param name="recipe">The recipe to be added</param>
    /// <param name="recipeId">The <see cref="Identifier"/> of the added recipe. If previously set it will be overwritten.</param>
    /// <param name="partId">The <see cref="Identifier"/> of the glyph the recipe is added to.</param>
    /// <exception cref="Exception">If recipe with the given Ids is already present.</exception>
    public static void AddRecipe(this QuintessentialMod mod, GlyphRecipe recipe, Identifier recipeId, Identifier partId) {
        recipe.RecipeId = recipeId;
        recipe.RecipeGlyphId = partId;
        if(!GlyphRecipe.Recipes.TryGetValue(partId, out var glyphRecipes)) {
            glyphRecipes = []; 
            GlyphRecipe.Recipes[partId] = glyphRecipes;
        }
        if (glyphRecipes.ContainsKey(recipeId))
            throw new Exception($"Recipe with id '{recipeId}' cannot be added, recipe already contained in recipes for part '{partId}'");
        glyphRecipes.Add(recipeId, recipe);
    }


    // TODO link to online documentation
    /// <summary>
    /// Adds the code run by the glyph to process recipes and transmutations.
    /// </summary>
    /// <param name="partType">The part to bind the delegate to.</param>
    /// <param name="cycleDelegate">A delegate containing the code to run each half cycle.</param>
    /// <exception cref="Exception">If part already has delegate, or is a vanilla part.</exception>
    public static void BindGlyphCylce(PartType partType, PartCycleDelegate cycleDelegate) {
        if (partType.Id.namespc == "om") throw new Exception("Cannot bind cycle code to part type from the base game: '" + partType.Id + "'");
        if (((patch_PartType)(object)partType).CycleDelegate != null) throw new Exception("A delegate was already bound to PartType: '" + partType.Id + "'");
        ((patch_PartType)(object)partType).CycleDelegate = cycleDelegate;
    }

    /// <summary>
    /// Add an event to be called each simulation cycle.
    /// </summary>
    /// <param name="cycleEvent">The event to be called.</param>
    public static void AddCycleEvent(CycleEvent cycleEvent) {
        patch_Sim.CycleEvents.Add(cycleEvent);
    }

    public static void AddBondType(this QuintessentialMod mod, BondType type, bool isUnbondable = true) {
        BondTypes.RegisterBondType(type, isUnbondable);
    }


    public static void AddComponentToAtom(Identifier atomTypeID, Func<patch_Atom, IAtomComponent> componentCtor) {
        if(!patch_Atom.CtorsByID.TryGetValue(atomTypeID, out var ctors)) {
            ctors = [];
            patch_Atom.CtorsByID[atomTypeID] = ctors;
        }
        ctors.Add(componentCtor);
    }
    public static void AddComponentToAtomOnReplace(Identifier atomTypeID, Func<patch_Atom, AtomType, IAtomComponent> componentCtor) {
        if (!patch_Atom.CtorsByIDAfterReplace.TryGetValue(atomTypeID, out var ctors)) {
            ctors = [];
            patch_Atom.CtorsByIDAfterReplace[atomTypeID] = ctors;
        }
        ctors.Add(componentCtor);
    }
    public static void AddComponentToBond(Identifier[] bondTypeIDs, Func<patch_Bond, IBondComponent> componentCtor) {
        patch_Bond.CtorsByID.Add(new(bondTypeIDs, componentCtor));
    }
    public static void AddComponentToPart(Identifier partTypeID, Func<patch_Part, IPartComponent> componentCtor) {
        if (!patch_Part.CtorsByID.TryGetValue(partTypeID, out var ctors)) {
            ctors = [];
            patch_Part.CtorsByID[partTypeID] = ctors;
        }
        ctors.Add(componentCtor);
    }
    public static void AddComponentToSimState(Identifier partTypeID, Func<patch_PartSimState, Part, Sim?, ISimStateComponent> componentCtor) {
        if (!patch_PartSimState.CtorsByID.TryGetValue(partTypeID, out var ctors)) {
            ctors = [];
            patch_PartSimState.CtorsByID[partTypeID] = ctors;
        }
        ctors.Add(componentCtor);
    }


    /// <summary>
    /// Invokes a recipe clearing and setting <see cref="patch_Sim.RecipeInputs"/> and <see cref="patch_Sim.RecipeOutputs"/>.<br/>
    /// This should only be used as the first recipe invoke of a part cycle.
    /// </summary>
    /// <param name="del">The predicate of the recipe to invoke.</param>
    /// <param name="sim">The current simulation.</param>
    /// <param name="part">The part that's running the transmutation.</param>
    /// <returns>If the recipe is a success, the glyph should process the inputs into the outputs.</returns>
    public static bool InvokeAndClear(this RecipePredicate del, patch_Sim sim, Part part) {
        sim.RecipeInputs.Clear();
        sim.RecipeOutputs.Clear();
        return del.Invoke(sim, part);
    }
}
