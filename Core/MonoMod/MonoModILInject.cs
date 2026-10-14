using System;
#pragma warning disable CS9113 // Parameter is unread

namespace MonoMod;

// TODO: move parts to online documentation and add a link
// TODO: Allow this being applied to fields in Mutatum
/// <summary>
/// Injects IL code into a method. Method parameters should be: <br />
/// <b>MethodDefinition</b> <c><i>method</i></c>,
/// <b>CustomAttribute</b> <c><i>attrib</i></c> (from Mono.Cecil)
/// </summary>
/// <param name="TargetMethodName">The name, or id of the method that makes the call.</param>
[MonoMod__SafeToCopy__]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MonoModILInject(string TargetMethodName) : Attribute {

}
