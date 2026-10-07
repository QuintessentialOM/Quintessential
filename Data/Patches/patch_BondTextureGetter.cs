using MonoMod;
using System;

[MonoModPatch("BondTextureGetter")]
[Obsolete]
[MonoModRemove]
static class patch_BondTextureGetter {

    [Obsolete]
    [MonoModRemove]
    public static BondTexture GetBondTexture(this BondTypeEnum type) { return null; }

}
