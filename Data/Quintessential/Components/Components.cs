using System;
using System.Collections.Generic;

namespace Quintessential.Components;

/// <summary>
/// A component interface for storing data on a <see cref="Puzzle"/>
/// </summary>
public interface IPuzzleComponent : IComponent<patch_Puzzle> { }
/// <summary>
/// A component interface for storing data on a <see cref="Solution"/>
/// </summary>
public interface ISolutionComponent : IComponent<patch_Solution> {
    /// <summary>
    /// Called after it's checked for a <see cref="Part"/> whether it can be placed on a given position.
    /// </summary>
    /// <param name="orig">The original return value. You can set this.</param>
    /// <param name="part">The checked Part.</param>
    /// <param name="otherInputOutputIndex">A position inside the source chamber for the part.</param>
    /// <param name="offset">The position the part was attempted to be placed down.</param>
    /// <param name="rotationOffset">The new rotation the part was attempted to be placed with.</param>
    /// <param name="errorMessage">The original error message returned. You can set this.</param>
    public virtual void OnPlacementCheck(ref bool orig, Part part, HexIndex otherInputOutputIndex, HexIndex offset, HexRotation rotationOffset, ref string? errorMessage) { }
    /// <summary>
    /// Called after a <see cref="Snapshot"/> is created.
    /// </summary>
    /// <param name="created">The original return value. You can edit this.</param>
    public virtual void OnCreateSnapshot(ref Snapshot created) { }
    /// <summary>
    /// Called before a <see cref="Snapshot"/> is restored.
    /// </summary>
    /// <param name="restored">The <see cref="Snapshot"/> to be restored.</param>
    public virtual void OnRestoreSnapshot(Snapshot restored) { }
    /// <summary>
    /// Called after a copy of the current <see cref="Solution"/> is created.
    /// </summary>
    /// <param name="copy">The original return value. You can edit this.</param>
    public virtual void OnMakeCopyOfSolution(ref Solution copy) { } // Should've been created with serialize-deserialize no need for change
}
/// <summary>
/// A component interface for storing data on a <see cref="Part"/>
/// </summary>
public interface IPartComponent : IComponent<patch_Part> {
    /// <summary>
    /// Called after a clone of the current <see cref="Part"/> is created.
    /// </summary>
    /// <param name="cloned">The original return value. You can edit this.</param>
    /// <param name="solution">The <see cref="Solution"/> this part was cloned with.</param>
    public abstract void OnClone(ref patch_Part cloned, Solution solution);
    /// <summary>
    /// Called before the part is reset by from the <see cref="PartType"/>.
    /// </summary>
    public abstract void OnReset();
    /// <summary>
    /// Called after a the input/output <see cref="Molecule"/> is searched for this part.
    /// </summary>
    /// <param name="resoult">The original return value. You can edit this.</param>
    /// <param name="solution">The <see cref="Solution"/> this molecule is from.</param>
    public virtual void OnGetMoleculeForInputOutput(ref Molecule resoult, Solution solution) { }
    /// <summary>
    /// Called after a new <see cref="HexIndex"/> is added to the end of the track.
    /// </summary>
    public virtual void OnAddTrackFront() { }
    /// <summary>
    /// Called after a new <see cref="HexIndex"/> is added to the front of the track.
    /// </summary>
    public virtual void OnAddTrackBack() { }
    /// <summary>
    /// Called before a <see cref="HexIndex"/> is removed from the end of the track.
    /// </summary>
    public virtual void OnRemoveTrackFront() { }
    /// <summary>
    /// Called before a <see cref="HexIndex"/> is removed from the end of the track.
    /// </summary>
    public virtual void OnRemoveTrackBack() { }
    /// <summary>
    /// Called before the position of the part is set to a new value.
    /// </summary>
    /// <param name="newPos">The new position of the part.</param>
    public virtual void OnSetHexPos(HexIndex newPos) { }
    /// <summary>
    /// Called before the part is rotated.
    /// </summary>
    /// <param name="solution">The associated <see cref="Solution"/>.</param>
    /// <param name="rotation">The change in rotation.</param>
    public virtual void OnRotateBy(Solution solution, HexRotation rotation) { }
    /// <summary>
    /// Called before the rotation of the part is set to a new value.
    /// </summary>
    /// <param name="solution">The associated <see cref="Solution"/>.</param>
    /// <param name="rotation">The new rotation of the part.</param>
    public virtual void OnSetRotation(Solution solution, HexRotation rotation) { }
    /// <summary>
    /// Called before the length of the part is set to a new value.
    /// </summary>
    /// <param name="newLenght">The new lenght of the part.</param>
    public virtual void OnSetLength(int newLenght) { }
    //TODO: public virtual void OnRender();
}
/// <summary>
/// A component interface for storing data on a <see cref="Sim"/>
/// </summary>
public interface ISimComponent : IComponent<patch_Sim>, ISimCallbacks { }
/// <summary>
/// A component interface for storing data on a <see cref="PartSimState"/>
/// </summary>
public interface ISimStateComponent : IComponent<patch_PartSimState, Part, Sim?>, ISimCallbacks {
    /// <summary>
    /// Called before the <see cref="PartSimState"/> is reset, at the start of the Cycle.
    /// </summary>
    public abstract void OnReset();
    /// <summary>
    /// Called before the <see cref="PartSimState"/> is reset, at the end of the Cycle.
    /// </summary>
    public virtual void OnResetForGlyphs() { }
    /// <summary>
    /// Called after Inputs create the new <see cref="Molecule"/>s.
    /// </summary>
    /// <param name="occupied">Positions occupied by previously existing <see cref="Molecule"/>s.</param>
    public virtual void OnSpawnMolecules(HashSet<HexIndex> occupied) { }
    /// <summary>
    /// Called before an <see cref="InstructionType"/> is executed for the given part.<br/>
    /// Will be called twice for each Instruction. Once before and once after glyphs.
    /// </summary>
    /// <param name="instruction">The instruction to be executed.</param>
    /// <param name="index">The index of the instruction.</param>
    /// <param name="instructionState">The time when the execution is happeding.</param>
    public virtual void OnInstruction(InstructionType instruction, Maybe<int> index, InstructionCycleState instructionState) { }
    /// <summary>
    /// Called when the grab state of the part is changed, befor the <see cref="Molecule"/> is released or after it's grabbed.
    /// </summary>
    /// <param name="molecule">The molecule held by the part, if any.</param>
    /// <param name="newState">The new grab state.</param>
    public virtual void OnGrabStateChange(Maybe<Molecule> molecule, bool newState) { }
}
/// <summary>
/// A component interface for storing data on a <see cref="Molecule"/>
/// </summary>
public interface IMoleculeComponent : IComponent<patch_Molecule>, ISimCallbacks {
    /// <summary>
    /// Called after a clone of the current <see cref="Molecule"/> is created.
    /// </summary>
    /// <param name="cloned">The original return value. You can edit this.</param>
    public abstract void OnClone(ref patch_Molecule cloned);
    /// <summary>
    /// Called after the molecule is merged with another one.<br/>
    /// For both molecules, first the primary, then the other.
    /// </summary>
    /// <param name="resoult">The original return value. You can edit this.</param>
    /// <param name="other">The other molecule.</param>
    /// <param name="asPrimary">Whether this is the primary molecule of the merge.</param>
    public abstract void OnMerge(ref patch_Molecule resoult, patch_Molecule other, bool asPrimary);
    /// <summary>
    /// Called after the <see cref="Molecule"/> is split into disconnected ones.
    /// </summary>
    /// <param name="resoult">The resulting molecules.</param>
    public abstract void OnSplit(List<patch_Molecule> resoult);
    /// <summary>
    /// Called after this molecule is repeated as a monomer.
    /// </summary>
    /// <param name="resoult">The resulting molecule.</param>
    /// <param name="repetitionPos">The position the repetition happened relative to.</param>
    public virtual void OnRepeatAsMonomer(ref patch_Molecule resoult, HexIndex repetitionPos) { }
    /// <summary>
    /// Called after an <see cref="Atom"/> is added to the <see cref="Molecule"/>.
    /// </summary>
    /// <param name="atom">The added <see cref="Atom"/>.</param>
    /// <param name="hexPos">The position the atom was added to.</param>
    public delegate void OnAddAtomDelegate(patch_Atom atom, HexIndex hexPos);
    /// <summary>
    /// Called before an <see cref="Atom"/>s Type is replaced.
    /// </summary>
    /// <param name="atom">The new <see cref="AtomType"/>.</param>
    /// <param name="hexPos">The targeted position.</param>
    public delegate void OnReplaceAtomDelegate(AtomType atom, HexIndex hexPos);
    /// <summary>
    /// Called before an <see cref="Atom"/> gets removed from the <see cref="Molecule"/>.
    /// </summary>
    /// <param name="hexPos">The targeted position.</param>
    public delegate void OnRemoveAtomDelegate(HexIndex hexPos);
    /// <summary>
    /// Called after a <see cref="Bond"/> is added to the <see cref="Molecule"/>.
    /// </summary>
    /// <param name="bond">The added <see cref="Bond"/>.</param>
    public delegate void OnAddBondDelegate(patch_Bond bond);
    /// <summary>
    /// Called before a <see cref="Bond"/> gets removed from the <see cref="Molecule"/>.
    /// </summary>
    /// <param name="bond">The removed <see cref="Bond"/>.</param>
    public delegate void OnRemoveBondDelegate(patch_Bond bond);
    /// <summary>
    /// Called before the <see cref="Molecule"/> gets rotated.
    /// </summary>
    /// <param name="center">The pivot of the rotation.</param>
    /// <param name="rotation">The amount of rotation.</param>
    public delegate void OnRotateDelegate(HexIndex center, HexRotation rotation);
    /// <summary>
    /// Called before the <see cref="Molecule"/> gets tranlated.
    /// </summary>
    /// <param name="translation">The amount of translation.</param>
    public delegate void OnTranslateDelegate(HexIndex translation);
    /// <summary>
    /// Called each time the <see cref="Molecule"/> is rendered.<br/>
    /// The original render code <b>must</b> be called precisely once.
    /// </summary>
    /// <param name="doRender">Call the original render code.<br/><b>This should be called precisely once!</b></param>
    /// <param name="offset">The offset to render the molecule at. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="hexPos">The position to render the molecule at. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="rotationAngle">The angle to render the molecule with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="opacityMultiplier">The opacity to render the molecule with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="height">The height to render the molecule with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="shadowStrength">The shadow strength to render the molecule with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="isOutputRender">Whether this is an output render.</param>
    /// <param name="solutionEditor">The solution editor used for rendering.</param>
    public virtual void OnRender(Action doRender, ref Vector2 offset, ref HexIndex hexPos, ref float rotationAngle, ref float opacityMultiplier, ref float height, ref float shadowStrength, bool isOutputRender, SolutionEditorBase solutionEditor) { doRender(); }
}
/// <summary>
/// A component interface for storing data on an <see cref="Atom"/>
/// </summary>
public interface IAtomComponent : IComponent<patch_Atom, patch_Molecule?>, ISimCallbacks {
    /// <summary>
    /// Called after a clone of the current <see cref="Atom"/> is created.
    /// </summary>
    /// <param name="cloned">The original return value. You can edit this.</param>
    public abstract void OnClone(ref patch_Atom cloned);
    /// <summary>
    /// Called before the type of this atom is replaced with another.
    /// </summary>
    /// <param name="newType">The new type of this atom.</param>
    public abstract void OnReplace(AtomType newType);
    /// <summary>
    /// Called before the atom gets removed from a <see cref="Molecule"/>.
    /// </summary>
    /// <param name="molecule">The <see cref="Molecule"/> this atom gets removed from.</param>
    /// <param name="hexPos">The position of this atom in the molecule.</param>
    public abstract void OnRemoveFromMolecule(patch_Molecule molecule, HexIndex hexPos);
    /// <summary>
    /// Called after the atom gets added to a <see cref="Molecule"/>.
    /// </summary>
    /// <param name="molecule">The <see cref="Molecule"/> this atom got added to.</param>
    /// <param name="hexPos">The position of this atom in the molecule.</param>
    public abstract void OnAddToMolecule(patch_Molecule molecule, HexIndex hexPos);
    /// <summary>
    /// Called each time the <see cref="Atom"/> is rendered.<br/>
    /// The original render code <b>must</b> be called precisely once.
    /// </summary>
    /// <param name="doRender">Call the original render code.<br/><b>This should be called precisely once!</b></param>
    /// <param name="type">The <see cref="AtomType"/> to render the atom with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="translation">The position to render the atom at. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="scaleMultiplier">The scale to render the molecule with. You can set this to change it.<br/>Calling the original method might change this value.</param>
    /// <param name="opacityMultiplier">The opacity to render the molecule with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="height">The height to render the molecule with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="shadowStrength">The shadow strength to render the molecule with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="shadowOffset">The shadow offset to render the molecule with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="shadowAngle">The shadow angle to render the molecule with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="shadow">The shadow texture for the render.</param>
    /// <param name="overlayEffect">The overlay texture for the render.</param>
    /// <param name="isOutputRender">Whether this is an output render.</param>
    /// <param name="solutionEditor">The solution editor used for rendering.</param>
    public virtual void OnRender(Action doRender, ref AtomType type, ref Vector2 translation, ref float scaleMultiplier, ref float opacityMultiplier, ref float height, ref float shadowStrength, ref float shadowOffset, ref float shadowAngle, Texture shadow, Texture overlayEffect, bool isOutputRender, SolutionEditorBase solutionEditor) { doRender(); }
}
/// <summary>
/// A component interface for storing data on a <see cref="Bond"/>
/// </summary>
public interface IBondComponent : IComponent<patch_Bond, patch_Molecule?>, ISimCallbacks {
    /// <summary>
    /// Called after a clone of the current <see cref="Bond"/> is created.
    /// </summary>
    /// <param name="cloned">The original return value. You can edit this.</param>
    public abstract void OnClone(ref patch_Bond cloned);
    /// <summary>
    /// Called before the bond gets removed from a <see cref="Molecule"/>.
    /// </summary>
    /// <param name="molecule">The <see cref="Molecule"/> this bond gets removed from.</param>
    public abstract void OnRemoveFromMolecule(patch_Molecule molecule);
    /// <summary>
    /// Called after the bond gets added to a <see cref="Molecule"/>.
    /// </summary>
    /// <param name="molecule">The <see cref="Molecule"/> this bond got added to.</param>
    public abstract void OnAddToMolecule(patch_Molecule molecule);
    /// <summary>
    /// Called each time the <see cref="Bond"/> is rendered.<br/>
    /// The original render code <b>must</b> be called precisely once.
    /// </summary>
    /// <param name="doRender">Call the original render code.<br/><b>This should be called precisely once!</b></param>
    /// <param name="offset">The offset to render the bond at. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="hexOffset">The position to render the bond at. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="rotationAngle">The angle to render the bond with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="opacityMultiplier">The opacity to render the bond with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="height">The height to render the bond with. You can set this.<br/>Calling the original method might change this value.</param>
    /// <param name="solutionEditor">The solution editor used for rendering.</param>
    public virtual void OnRender(Action doRender, ref Vector2 offset, ref HexIndex hexOffset, ref float rotationAngle, ref float opacityMultiplier, ref float height, SolutionEditorBase solutionEditor) { doRender(); }
}
