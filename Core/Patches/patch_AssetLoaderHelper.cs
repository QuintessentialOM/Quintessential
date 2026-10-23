using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
internal class patch_AssetLoaderHelper {
    [MonoModILInject("LoadSound")]
    static void PatchSoundId(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));

        method.Body.Variables.Add(new VariableDefinition(method.Parameters[0].ParameterType));
        var origParam = method.Body.Variables[^1];

        cursor.EmitLdarg0();
        cursor.EmitStloc(origParam);
        cursor.GotoNext(instr => instr.MatchCall(out var call) && call.Name.Equals("GetFileNameWithoutExtension"));
        cursor.Index--;
        cursor.RemoveRange(2);
        cursor.EmitLdloc(origParam);
    }
    [MonoModIgnore]
    public extern static Sound orig_LoadSound(string path);
    public static Sound LoadSound(string path) {
        var sound = orig_LoadSound(path);
        patch_SoundAssets.Sounds.Add(sound);
        return sound;
    }

    public static Shader LoadShader(string vertexShaderPath, string pixelShaderPath) {
        return Renderer.LoadShader(null, vertexShaderPath, pixelShaderPath);
    }
}
