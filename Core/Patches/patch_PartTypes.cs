using System;
using MonoMod;
using Quintessential;

[MonoModPatch("PartTypes")]
class patch_PartTypes{

    [MonoModInternalM] [MonoModIgnore]
    [Obsolete("This shouldn't be used. Use `Id` instead.")]
    public static extern Maybe<PartType> GetById(string id);

    public static Maybe<PartType> GetById(Identifier id) {
        return PartTypes.partTypes.FirstMatching(partType => ((patch_PartType)(object)partType).Id == id);
    }

}