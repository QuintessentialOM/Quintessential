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

    /// <summary>
    /// Adds a bond type to the list of all bond types.
    /// </summary>
    /// <param name="mod">The mod that adds the bond.</param>
    /// <param name="type">The bond type to be added.</param>
    /// <param name="isUnbondable">Whether the bond should be able to be unbonded by the vanilla unbonder.</param>
    public static void AddBondType(this QuintessentialMod mod, BondType type, bool isUnbondable = true) {
        BondTypes.RegisterBondType(type, isUnbondable);
    }


    /// <summary>
    /// Adds a component to every new <see cref="Atom"/> instance of the give <see cref="AtomType"/>.
    /// </summary>
    /// <param name="atomTypeID">The identifier of the <see cref="AtomType"/> to create the new component on.</param>
    /// <param name="componentCtor">A function returning a new component.</param>
    public static void AddComponentToAtom(Identifier atomTypeID, Func<patch_Atom, IAtomComponent> componentCtor) {
        if(!patch_Atom.CtorsByID.TryGetValue(atomTypeID, out var ctors)) {
            ctors = [];
            patch_Atom.CtorsByID[atomTypeID] = ctors;
        }
        ctors.Add(componentCtor);
    }
    /// <summary>
    /// Adds a component to an <see cref="Atom"/> when the <see cref="AtomType"/> gets replaced by the provided one.
    /// </summary>
    /// <param name="atomTypeID">The identifier of the <see cref="AtomType"/> to create the new component on.</param>
    /// <param name="componentCtor">
    /// A function returning a new component.<br/>
    /// The arguments are the <see cref="Atom"/> and the old <see cref="AtomType"/>.
    /// </param>
    public static void AddComponentToAtomOnReplace(Identifier atomTypeID, Func<patch_Atom, AtomType, IAtomComponent> componentCtor) {
        if (!patch_Atom.CtorsByIDAfterReplace.TryGetValue(atomTypeID, out var ctors)) {
            ctors = [];
            patch_Atom.CtorsByIDAfterReplace[atomTypeID] = ctors;
        }
        ctors.Add(componentCtor);
    }
    /// <summary>
    /// Adds a component to a <see cref="Bond"/> when the given <see cref="BondType"/> gets added.
    /// </summary>
    /// <param name="bondTypeIDs">The identifiers of the <see cref="BondType"/>s to create the new component on.</param>
    /// <param name="componentCtor">A function returning a new component.</param>
    public static void AddComponentToBond(Identifier[] bondTypeIDs, Func<patch_Bond, IBondComponent> componentCtor) {
        patch_Bond.CtorsByID.Add(new(bondTypeIDs, componentCtor));
    }
    /// <summary>
    /// Adds a component to every new <see cref="Part"/> instance of the give <see cref="PartType"/>.
    /// </summary>
    /// <param name="partTypeID">The identifier of the <see cref="PartType"/> to create the new component on.</param>
    /// <param name="componentCtor">A function returning a new component.</param>
    public static void AddComponentToPart(Identifier partTypeID, Func<patch_Part, IPartComponent> componentCtor) {
        if (!patch_Part.CtorsByID.TryGetValue(partTypeID, out var ctors)) {
            ctors = [];
            patch_Part.CtorsByID[partTypeID] = ctors;
        }
        ctors.Add(componentCtor);
    }
    /// <summary>
    /// Adds a component to every new <see cref="PartSimState"/> that gets created for the provided <see cref="PartType"/>.
    /// </summary>
    /// <param name="partTypeID">The identifier of the <see cref="PartType"/> to create the new component on.</param>
    /// <param name="componentCtor">
    /// A function returning a new component.<br/>
    /// The arguments are the <see cref="PartSimState"/>, the <see cref="Part"/>, and the <see cref="Sim"/>, if present.
    /// </param>
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
