using System;
using static Quintessential.PartCycleDelegate;

namespace Quintessential;

/// <summary>
/// A delegate corresponding to part's cycle specific transmutation code.<br/>
/// This delegate should handle all non-visual elements.<br/>
/// Bind to a glyph with <see cref="QApi.BindGlyphCylce"/>.
/// </summary>
/// <param name="executionType">
/// The specific timing this delegate is meant to be invoked with in the list of all part cycles.<br/>
/// Supports enum flags.
/// </param>
/// <param name="delegate">The delegate to execute.</param>
public class PartCycleDelegate(PartCycleExecutionType executionType, RecipeCallDelegate @delegate) {

    /// <summary>
    /// The specific timing this delegate is meant to be invoked with in the list of all part cycles.<br/>
    /// Supports enum flags.
    /// </summary>
    public readonly PartCycleExecutionType ExecutionType = executionType;
    /// <summary>
    /// A delegate corresponding to part's cycle specific transmutation code.
    /// </summary>
    public readonly RecipeCallDelegate Delegate = @delegate;

    /// <summary>
    /// Timing for part's self-contained execution order.
    /// </summary>
    [Flags]
    public enum PartCycleExecutionType {
        /// <summary>
        /// The delegate will never be invoked.
        /// </summary>
        None = 0,
        /// <summary>
        /// The delegate will be invoked before the bonders on the given part.
        /// </summary>
        Normal = 1,
        /// <summary>
        /// The delegate will be invoked after the bonders on the given part.
        /// </summary>
        AfterBonder = 2,
    }
    // TODO link to online documentation
    /// <summary>
    /// The delegate that should contain the given part's cycle specific code. Will be invoked once in each half cycle.
    /// </summary>
    /// <param name="sim">The current simulation.</param>
    /// <param name="part">The specific instance of the part.</param>
    /// <param name="simState">The simulation state of the <paramref name="part"/>.</param>
    /// <param name="recipe">The recipe the part should attempt to check.<br/> See <seealso cref="GlyphRecipe.Predicate"/> to check if the recipe applies.</param>
    /// <param name="isCycleStart">Wheter it is the invocation in the first half of the cycle.</param>
    /// <returns>
    /// Return true if the glyph should not be activated more times this half cycle.<br/>
    /// Do this if the glyph was triggered by a <see cref="GlyphRecipe"/>, or if <see cref="PartSimState.isProcessing"/> was resolved.
    /// </returns>
    public delegate bool RecipeCallDelegate(patch_Sim sim, Part part, PartSimState simState, GlyphRecipe recipe, bool isCycleStart);
}