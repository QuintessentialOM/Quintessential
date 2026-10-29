using MonoMod;
using System.Collections.Generic;

internal class patch_EditableProgram {
    [MonoModIgnore]
    private SortedDictionary<int, InstructionType> instructions;

    public IReadOnlyDictionary<int, InstructionType> GetInstructions() {
        return instructions;
    }
}
