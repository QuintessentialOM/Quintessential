using System;
#pragma warning disable CS9113 // Parameter is unread

namespace MonoMod;

// TODO: move parts to online documentation and add a link
/// <summary>
/// Wraps a call to a method, field access, object init,<br/>
/// or a constant with the method the attribue is applied to.<br/>
/// Wrapping constant objects might cause issues.
/// </summary>
/// <param name="TargetMethodName">The name, or id of the method that makes the call.</param>
/// <param name="targetPoint">
/// The type of operation to map. May be one of the following: <br/>
/// <c>Call</c> , <c>Field-Read</c> , <c>Field-Write</c> , <c>New</c>,<br/>
/// <c>Literal-String</c> , <c>Literal-Numeric</c> , <c>Literal-Enum</c><br/>
/// </param>
/// <param name="TargetMetadata">
/// The target of the wrap.<br/>
/// For <c>Call</c>, <c>Field</c> and <c>New</c> as:<br/>
/// Name.space.Class/Nested::Method, Class::Field<br/>
/// For <c>Literal-Enum</c>: Enum.Value<br/>
/// While for other <c>Literal</c>s as their value.<br/>
/// </param>
[MonoMod__SafeToCopy__]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MonoModWrapOperation(string TargetMethodName, string targetPoint, string TargetMetadata) : Attribute {

}
