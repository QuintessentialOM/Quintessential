using System;
using MonoMod;
using Quintessential;

[MonoModPatch("InstructionTypes")]
class patch_InstructionTypes {

    [MonoModInternalM] [MonoModIgnore]
    [Obsolete("This shouldn't be used. Use `Id` instead.")]
    public static extern Maybe<InstructionType> GetById(char instructionId);

    public static Maybe<InstructionType> GetById(Identifier id) {
        return InstructionTypes.instructions.FirstMatching(partType => ((patch_InstructionType)(object)partType).Id == id);
    }

}