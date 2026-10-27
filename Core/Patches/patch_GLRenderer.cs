using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using Quintessential;
using System;
using System.IO;
using System.Linq;

[MonoModPatch("GLRenderer")]
class patch_GLRenderer {

    [MonoModIgnore]
    private static extern uint LinkShader(uint vertexShaderHandle, uint pixelShaderHandle);
    [MonoModIgnore]
    public extern void DestroyShader(IntPtr shaderHandle);


    public bool ReloadShaderIfChanged(Shader shader) {
        string vertexShaderPath = Path.Combine("Content", shader.vertexShaderName + ".glsl");
        string pixelShaderPath = Path.Combine("Content", shader.pixelShaderName + ".glsl");
        string VSPath = null;
        string PSPath = null;

        for (int i = QuintessentialLoader.ModContentDirectories.Count - 1; i >= 0; i--) {
            string contentDir = QuintessentialLoader.ModContentDirectories[i];
            string path = Path.Combine(contentDir, vertexShaderPath);
            if (VSPath == null && File.Exists(path)) {
                VSPath = path;
                if (PSPath != null) break; 
            }
            path = Path.Combine(contentDir, pixelShaderPath);
            if (PSPath == null && File.Exists(path)) {
                PSPath = path;
                if (VSPath != null) break;
            }
        }
        VSPath ??= vertexShaderPath;
        PSPath ??= pixelShaderPath;

        if (!File.Exists(VSPath)) throw new Exception("Failed to find vertex shader at " + VSPath);
        if (!File.Exists(PSPath)) throw new Exception("Failed to find pixel shader at " + PSPath);
        DateTime dateTime = new(Math.Max(File.GetLastWriteTime(VSPath).Ticks, File.GetLastWriteTime(PSPath).Ticks));
        if (dateTime > shader.creationTime) {
            uint num = LinkShader(LoadShaderFileUint(35633U, VSPath), LoadShaderFileUint(35632U, PSPath));
            if (num != 0U) {
                if (shader.shader != IntPtr.Zero) {
                    DestroyShader(shader.shader);
                }
                shader.shader = (IntPtr)num;
                shader.creationTime = dateTime;
            }
            return true;
        }
        return false;
    }
    private uint LoadShaderFileUint(uint shaderType, string shaderFilePath) {
        throw new Exception("ILInject failed to apply");
    }

    [MonoModILInject("LoadShaderFileUint")]
    static void PatchLoadShaderFile(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition orig = method.DeclaringType.Methods.First(m => m.Name.Equals("LoadShaderFile"));
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitLdarg2();
        cursor.EmitCall(orig);
        cursor.EmitRet();
        while (cursor.Next != null) {
            cursor.Remove();
        }
    }
}