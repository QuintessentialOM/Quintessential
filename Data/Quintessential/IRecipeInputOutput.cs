
namespace Quintessential;

/// <summary>
/// A possible input for a recipe.
/// By default <see cref="Molecule"/>s, <see cref="AtomType"/>s and <see cref="Bond"/>s.
/// </summary>
public interface IRecipeOutput {
    // Molecule, AtomType, Bond implements this
    
}

/// <summary>
/// A possible output for a recipe.<br/>
/// By default <see cref="Molecule"/>s and <see cref="AtomReference"/>s.
/// </summary>
public interface IRecipeInput {
    // Molecule, AtomReference implements this

}
