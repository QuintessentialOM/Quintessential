using System;
using System.Collections.Generic;

namespace Quintessential.Components;

public interface IPuzzleComponent : IComponent<patch_Puzzle> { }
public interface ISolutionComponent : IComponent<patch_Solution> {
    public virtual void OnPlacementCheck(ref bool orig, Part part, HexIndex otherInputOutputIndex, HexIndex offset, HexRotation rotationOffset, ref string? errorMessage) { }
    public virtual void OnCreateSnapshot(ref Snapshot created) { }
    public virtual void OnRestoreSnapshot(Snapshot restored) { }
    public virtual void OnMakeCopyOfSolution(ref Solution copy) { } // Should've been created with serialize-deserialize no need for change
}
public interface IPartComponent : IComponent<patch_Part> {
    public abstract void OnClone(ref patch_Part cloned, Solution solution);
    public abstract void OnReset();
    public virtual void OnGetMoleculeForInputOutput(ref Molecule resoult, Solution solution) { }
    public virtual void OnAddTrackFront() { }
    public virtual void OnAddTrackBack() { }
    public virtual void OnRemoveTrackFront() { }
    public virtual void OnRemoveTrackBack() { }
    public virtual void OnSetHexPos(HexIndex newPos) { }
    public virtual void OnRotateBy(Solution solution, HexRotation rotation) { }
    public virtual void OnSetRotation(Solution solution, HexRotation rotation) { }
    public virtual void OnSetLength(int newLenght) { }
    //TODO: public virtual void OnRender();
}
public interface ISimComponent : IComponent<patch_Sim>, ISimCallbacks { }
public interface ISimStateComponent : IComponent<patch_PartSimState, Part, Sim?>, ISimCallbacks {
    public abstract void OnReset();
    public virtual void OnResetForGlyphs() { }
    public virtual void OnSpawnMolecules(HashSet<HexIndex> occupied) { }
    public virtual void OnInstruction(InstructionType instruction, Maybe<int> index, InstructionCycleState instructionState) { }
    public virtual void OnGrabStateChange(Maybe<Molecule> molecule, bool newState) { }
}
public interface IMoleculeComponent : IComponent<patch_Molecule>, ISimCallbacks {
    public abstract void OnClone(ref patch_Molecule cloned);
    public abstract void OnMerge(ref patch_Molecule resoult, patch_Molecule other, bool asPrimary);
    public abstract void OnSplit(List<patch_Molecule> resoult);
    public virtual void OnRepeatAsMonomer(ref patch_Molecule resoult, HexIndex repetitionPos) { }
    public delegate void OnAddAtomDelegate(patch_Atom atom, HexIndex hexPos);
    public delegate void OnReplaceAtomDelegate(AtomType atom, HexIndex hexPos);
    public delegate void OnRemoveAtomDelegate(HexIndex hexPos);
    public delegate void OnAddBondDelegate(patch_Bond bond);
    public delegate void OnRemoveBondDelegate(patch_Bond bond);
    public delegate void OnRotateDelegate(HexIndex center, HexRotation rotation);
    public delegate void OnTranslateDelegate(HexIndex translation);
    public virtual void OnRender(Action doRender, ref Vector2 offset, ref HexIndex hexPos, ref float rotationAngle, ref float opacityMultiplier, ref float height, ref float shadowStrength, bool isOutputRender, SolutionEditorBase solutionEditor) { }
}
public interface IAtomComponent : IComponent<patch_Atom, patch_Molecule?>, ISimCallbacks {
    public abstract void OnClone(ref patch_Atom cloned);
    public abstract void OnReplace(AtomType newType); //> Call initializers
    public abstract void OnRemoveFromMolecule(patch_Molecule molecule, HexIndex hexPos);
    public abstract void OnAddToMolecule(patch_Molecule molecule, HexIndex hexPos);
    public virtual void OnRender(Action doRender, ref AtomType type, ref Vector2 translation, ref float scaleMultiplier, ref float opacityMultiplier, ref float height, ref float shadowStrength, ref float shadowOffset, ref float shadowAngle, Texture shadow, Texture overlayEffect, bool isOutputRender, SolutionEditorBase solutionEditor) { }
}
public interface IBondComponent : IComponent<patch_Bond, patch_Molecule?>, ISimCallbacks {
    public abstract void OnClone(ref patch_Bond cloned);
    public abstract void OnRemoveFromMolecule(patch_Molecule molecule);
    public abstract void OnAddToMolecule(patch_Molecule molecule);
    public virtual void OnRender(Action doRender, ref Vector2 offset, ref HexIndex hexOffset, ref float rotationAngle, ref float opacityMultiplier, ref float height, SolutionEditorBase solutionEditor) { }
}