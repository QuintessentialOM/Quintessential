using Mono.Cecil;
using MonoMod;

public static class patch_Editor {

    [MonoModILInject("RenderBond")]
    static void PatchRenderMolecule(MethodDefinition method, CustomAttribute attribute) {
        method.SetPublic(true);
    }
}
