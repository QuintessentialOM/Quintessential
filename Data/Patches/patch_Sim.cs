#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using Quintessential.Components;
using Quintessential.Internal;
using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Quintessential.CycleEvent;
using static Quintessential.PartCycleDelegate;

public class patch_Sim : Sim, IComponentHolder<patch_Sim, ISimComponent> {

    #region ComponentSystem

    private Dictionary<Identifier, ISimComponent> Components;

    public void AddComponent(ISimComponent toAdd) {
        if (Components.ContainsKey(toAdd.Id))
            throw new Exception("A component with the same " + toAdd.Id + " was already added to this Sim.");
        toAdd.OnBind(this);
        Components.Add(toAdd.Id, toAdd);
    }
    public void AddComponentSafe(Identifier id, Func<ISimComponent> ctor) {
        if (Components.ContainsKey(id)) return;
        var component = ctor();
        if (id != component.Id) throw new Exception($"Id of created component '{component.Id}' not matching provided '{id}'.");
        Components.Add(id, component);
    }
    public bool TryGetComponent(Identifier toGet, out ISimComponent extension) {
        return Components.TryGetValue(toGet, out extension);
    }
    public ISimComponent GetComponent(Identifier toGet) {
        if (!TryGetComponent(toGet, out var ext)) {
            throw new Exception("Identifier was not contained on object.");
        }
        return ext;
    }
    public bool RemoveComponent(Identifier toRemove) {
        if (Components.TryGetValue(toRemove, out ISimComponent value))
            value.OnUnbind(this);
        return Components.Remove(toRemove);
    }
    public bool HasComponent(Identifier id) { return Components.ContainsKey(id); }

    #endregion

    #region ComponentCalls
    [MonoModIgnore]
    public Dictionary<patch_Part, patch_PartSimState> simulationDict;

    protected void OnSpawnMolecules(HashSet<HexIndex> occupied) {
        foreach (var keyValuePair in simulationDict) {
            keyValuePair.Value.OnSpawnMolecules(occupied);
        }
    }

    [MonoModILInject("ResetSimStates")]
    static void PatchResetSimStates(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition onCall = MonoModRule.Modder.FindType("PartSimState").Resolve().Methods.First(f => f.Name.Equals("OnReset"));

        cursor.GotoNext(MoveType.After, instr => instr.MatchCall(out var method) && method.Name.Equals("get_Value"));
        cursor.EmitDup();
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("ResetGlyphs")]
    static void PatchResetGlyphs(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition onCall = MonoModRule.Modder.FindType("PartSimState").Resolve().Methods.First(f => f.Name.Equals("OnResetForGlyphs"));

        cursor.GotoNext(MoveType.After, instr => instr.MatchCall(out var method) && method.Name.Equals("get_Value"));
        cursor.EmitDup();
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("SpawnMolecules")]
    static void PatchSpawnMolecules(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition onCall = method.DeclaringType.Methods.First(f => f.Name.Equals("OnSpawnMolecules"));

        cursor.GotoNext(MoveType.After, instr => instr.MatchRet());
        cursor.Prev.OpCode = OpCodes.Ldarg_0;
        cursor.EmitLdloc0();
        cursor.EmitCall(onCall);
        cursor.EmitRet();
    }
    [MonoModILInject("RunCycleInstructions")]
    static void PatchOnInstruction(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition onCall = MonoModRule.Modder.FindType("PartSimState").Resolve().Methods.First(f => f.Name.Equals("OnInstruction"));

        cursor.GotoNext(MoveType.After, instr => instr.MatchStloc3());
        Instruction start = cursor.Prev;
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdfld("Sim", "simulationDict"));
        FieldReference simulationDict = cursor.Prev.Operand as FieldReference;
        MethodReference getItem = cursor.Next.Next.Operand as MethodReference;
        cursor.Goto(start, MoveType.After);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(simulationDict);
        cursor.EmitLdloc1();
        cursor.EmitCallvirt(getItem);
        cursor.EmitLdloc3();
        cursor.EmitLdloc2();
        cursor.EmitLdarg1();
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("Grab")]
    static void PatchGrab(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition onCall = MonoModRule.Modder.FindType("PartSimState").Resolve().Methods.First(f => f.Name.Equals("OnGrabStateChange"));

        cursor.GotoNext(MoveType.Before, instr => instr.MatchDup());
        cursor.EmitDup();
        cursor.GotoNext(MoveType.Before, instr => instr.MatchRet());
        FieldReference heldMolecule = cursor.Previous.Operand as FieldReference;
        cursor.EmitDup();
        cursor.EmitLdfld(heldMolecule);
        cursor.EmitLdcI4(1);
        cursor.EmitCall(onCall);
    }
    [MonoModILInject("Drop")]
    static void PatchDrop(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition onCall = MonoModRule.Modder.FindType("PartSimState").Resolve().Methods.First(f => f.Name.Equals("OnGrabStateChange"));

        cursor.GotoNext(MoveType.Before, instr => instr.MatchRet());
        FieldReference heldMolecule = cursor.Previous.Operand as FieldReference;
        cursor.Index = 0;
        cursor.GotoNext(MoveType.Before, instr => instr.MatchDup());
        cursor.EmitDup();
        cursor.EmitDup();
        cursor.EmitLdfld(heldMolecule);
        cursor.EmitLdcI4(0);
        cursor.EmitCall(onCall);
    }

    #endregion

    #region RecipeSystem

    public RecipeInputDictionary<HexIndex, IRecipeInput> RecipeInputs;
    public RecipeOutputDictionary<HexIndex, IRecipeOutput> RecipeOutputs;
    public List<Part> HoldingParts;

    public bool GetAtomReference(Part part, HexIndex offset, bool allowPartAttachedAtoms, out AtomReference atomReference) {
        return GetAtomReference(part, offset, HoldingParts, allowPartAttachedAtoms).GetOrDefault(out atomReference);
    }
    public bool HasAtomAt(Part part, HexIndex offset, bool allowPartAttachedAtoms) {
        return GetAtomReference(part, offset, HoldingParts, allowPartAttachedAtoms).HasValue();
    }

    [MonoModILInject("RunCycleGlyphs")]
    static void PatchRecipeSystem(MethodDefinition method, CustomAttribute attribute) {

        MonoModRule.Modder.Log("Patching Recipe System init.");
        if (!method.HasBody) {
            throw new Exception("Unable to patch Recipe System init. (no body)");
        }
        ILCursor cursor = new(new ILContext(method));


        // --- Replace loc0 with field
        FieldDefinition holdingParts = MonoModRule.Modder.FindType("Sim").Resolve().Fields.First(f => f.Name.Equals("HoldingParts"));
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt("Sim", "SpawnMolecules"));
        cursor.EmitLdarg0();
        cursor.EmitLdloc0();
        cursor.EmitStfld(holdingParts);

        cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchCallvirt("Sim", "GetSolution"),
            instr => instr.MatchLdfld("Solution", "parts"),
            instr => instr.OpCode == OpCodes.Callvirt,
            instr => instr.MatchStloc2(),
            instr => instr.OpCode == OpCodes.Br);

        // --- General setup
        Instruction loopEnd = (Instruction)cursor.Prev.Operand;
        Instruction upperHead = null; // the current position of the edits ( need this since we have to jump around a lot )
        MethodDefinition referenceCode = MonoModRule.Modder.FindType("Sim").Resolve().Methods.First(f => f.Name.Equals("PatchRecipeSystemCodeReference"));
        ILCursor referenceCulsor = new(new ILContext(referenceCode));

        cursor.TryGotoNext(MoveType.Before,
            instr => instr.MatchLdloc(6),
            instr => instr.MatchLdfld("Sim/ReferredPart", "part"),
            instr => instr.MatchCallvirt("Part", "GetType"),
            instr => instr.MatchLdsfld("PartTypes", "calcificationGlyph"));

        // --- Get the list of recipes for the part
        TypeReference recipeDictionaryType = referenceCode.Body.Variables[0].VariableType;
        method.Body.Variables.Add(new VariableDefinition(recipeDictionaryType));
        var recipeDictionary = method.Body.Variables[^1];
        FieldReference recipesField = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchLdsfld(out recipesField));
        MethodReference tryGetValueRef = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchCallvirt(out tryGetValueRef));
        cursor.EmitLdsfld(recipesField);
            // * Go and fetch `referredPart.part.GetType().Id`
            upperHead = cursor.Prev; // > Save position
            cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("Sim/ReferredPart", "part"), instr => instr.MatchCallvirt("Part", "GetType"));
            FieldReference referencePart = (FieldReference)cursor.Previous.Previous.Operand;
            MethodReference getPartType = (MethodReference)cursor.Previous.Operand;
            cursor.Goto(upperHead, MoveType.After); // > Restore position
            cursor.EmitLdloc(6);
            cursor.EmitLdfld(referencePart);
            cursor.EmitCallvirt(getPartType);
            MethodDefinition partTypeId = MonoModRule.Modder.FindType("PartType").Resolve().Methods.First(f => f.Name.Equals("get_Id"));
            cursor.EmitCall(partTypeId);
        cursor.EmitLdloca(recipeDictionary);
        cursor.EmitCallvirt(tryGetValueRef);
        cursor.Emit(OpCodes.Brfalse, loopEnd);

        // --- Get the reference for the loop code
        TypeReference enumeratorType = referenceCode.Body.Variables[2].VariableType;
        MethodReference getEnumeratior = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchCallvirt(out getEnumeratior));
        MethodReference moveNextEnumeratior = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchCall(out moveNextEnumeratior));
        MethodReference getCurrent = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchCall(out getCurrent));

        // --- Create new local dictionary &  Start creating the loop
        method.Body.Variables.Add(new VariableDefinition(enumeratorType));
        var enumeratorVar = method.Body.Variables[^1];

        cursor.EmitLdloc(recipeDictionary);
        // var first = cursor.Prev;
        cursor.EmitCallvirt(getEnumeratior);
        cursor.EmitStloc(enumeratorVar);
            // * We should cursor.EmitBr(); but first get the target instruction
            // * Meanwhile doing the back of the loop
            upperHead = cursor.Prev; // > Save position
            cursor.Goto(loopEnd);

            Instruction continueTarget = (Instruction)cursor.Next.Next.Next.Operand; // BrtrueS
            MethodReference dispose = (MethodReference)cursor.Next.Next.Next.Next.Next.Next.Next.Operand; // call virt

            cursor.EmitLdloca(enumeratorVar);
            Instruction last = cursor.Prev; // > Start of the foreach loop end section
            int lastIndex = cursor.Index;
            cursor.EmitCall(moveNextEnumeratior);
            cursor.Emit(OpCodes.Brtrue, continueTarget); // Target will be set later
            var breakTarget = cursor.Next; // Must fix after adding finally block
            // TODO get the finally block working, does that bastard allways throw unfixable errors?
            // Hopefully commenting this out won't blow up anyones computer...
            //cursor.Emit(OpCodes.Leave, loopEnd);
            //cursor.EmitLdloca(enumeratorVar);
            //var tryEnd_handleStart = cursor.Prev;
            //cursor.EmitConstrained(enumeratorType);
            //cursor.EmitCallvirt(dispose.Resolve());
            //cursor.EmitEndfinally();
            //var handleEnd = cursor.Next;
            //method.Body.ExceptionHandlers.Insert(1, new(ExceptionHandlerType.Finally) {
            //    TryStart = tryStart,
            //    TryEnd = tryEnd_handleStart,
            //    HandlerStart = tryEnd_handleStart,
            //    HandlerEnd = handleEnd
            //});

            cursor.Goto(upperHead, MoveType.After); // > Restore position
            cursor.Emit(OpCodes.Br, last);
        //var tryStart = cursor.Prev;
        cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Brfalse && instr.Operand == loopEnd); // Fix a problem in the middle of the iteration
        cursor.Next.Operand = last;
        cursor.Goto(upperHead, MoveType.After); // > Restore position
        cursor.Index++;
        // Finished with the loop creation

        TypeReference currentValueType = referenceCode.Body.Variables[3].VariableType;
        method.Body.Variables.Add(new VariableDefinition(currentValueType)); // Add local for varriable
        var recipePairVar = method.Body.Variables[^1];
        cursor.EmitLdloca(enumeratorVar);
        var newBegining = cursor.Prev;
        cursor.EmitCall(getCurrent);
        cursor.EmitStloc(recipePairVar);
            // * Change the start of the foreach loop to include the new instructions
            upperHead = cursor.Prev; // > Save position
            cursor.Goto(loopEnd); 
            cursor.TryGotoPrev(MoveType.Before, instr => instr.OpCode == OpCodes.Brtrue);
            cursor.Next.Operand = newBegining;
            cursor.Goto(upperHead, MoveType.After); // > Restore position

        // --- Add break conditions
        cursor.EmitLdloc(7);
        cursor.EmitLdfld(MonoModRule.Modder.FindType("PartSimState").Resolve().Fields.First(f => f.Name.Equals("wasActivated")));
        cursor.Emit(OpCodes.Brtrue, breakTarget); // break;
        upperHead = cursor.Prev; // > Save position


        // --- Add separate variable
        MethodReference getValue = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchCall(out getValue));
        TypeDefinition recipeType = MonoModRule.Modder.FindType("Quintessential.GlyphRecipe").Resolve();
        method.Body.Variables.Add(new VariableDefinition(recipeType)); // Add local for recipe
        var recipeVar = method.Body.Variables[^1];
        cursor.EmitLdloc(recipePairVar);
        cursor.EmitCall(getValue);
        cursor.EmitStloc(recipeVar);

        // --- Replace Vanilla Recipe Calls
        FieldReference predicate = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchLdfld(out predicate));
        MethodReference invokeAndClear = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchCall(out invokeAndClear));
        FieldReference inputs = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchLdfld(out inputs));
        FieldReference outputs = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchLdfld(out outputs));
        MethodReference hexCtor = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchNewobj(out hexCtor));
        MethodReference getIRecipeIO = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchCallvirt(out getIRecipeIO));
        TypeReference atomRef = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchIsinst(out atomRef));
        FieldReference processingAtoms = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchLdfld(out processingAtoms));
        FieldReference dictionaryRecipe = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchStfld(out dictionaryRecipe));
        FieldReference atomTags = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchLdsfld(out atomTags));
        MethodReference stringToId = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchCall(out stringToId));
        MethodReference getTag = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchCallvirt(out getTag));
        MethodReference hasAtomTag = null;
        referenceCulsor.TryGotoNext(instr => instr.MatchCallvirt(out hasAtomTag));
        TypeDefinition atomType = MonoModRule.Modder.FindType("AtomType").Resolve();
        method.Body.Variables.Add(new VariableDefinition(atomType));
        VariableDefinition oldAtomType = method.Body.Variables[^1];

        // --- Set recipe for IO dictionaries
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdloc(recipeVar);
        cursor.EmitStfld(dictionaryRecipe);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdloc(recipeVar);
        cursor.EmitStfld(dictionaryRecipe);

        // Calcification
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt("Sim", "GetAtomReference"));
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("AtomType", "isGlassy"));
        var removeIndex = cursor.Index;
        cursor.TryGotoPrev(MoveType.Before, instr => instr.MatchLdarg0());
        cursor.RemoveRange(removeIndex - cursor.Index);
        cursor.EmitLdloc(recipeVar);
        cursor.EmitLdfld(predicate);
        cursor.EmitLdarg0(); // Sim
        cursor.EmitLdloc(6); // ReferencePart::part
        cursor.EmitLdfld(referencePart);
        cursor.EmitCall(invokeAndClear);
        cursor.Index++;
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(8);
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "salt"));
        cursor.Remove();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);

        // Duplication
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt("Sim", "GetAtomReference"));
        cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdarg0());
        var start = cursor.Prev;
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall("AtomType", "op_Equality"));
        removeIndex = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(removeIndex - cursor.Index);
        cursor.EmitLdloc(recipeVar);
        cursor.EmitLdfld(predicate);
        cursor.EmitLdarg0(); // Sim
        cursor.EmitLdloc(6); // ReferencePart::part
        cursor.EmitLdfld(referencePart);
        cursor.EmitCall(invokeAndClear);
        cursor.Index++;
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(10);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(1);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(11);
        start = cursor.Prev;
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("AtomReference", "atomType"));
        cursor.Prev.MatchLdfld(out FieldReference atomRefAtomType);
        cursor.Goto(start, MoveType.After);
        cursor.EmitLdloc(11);
        cursor.EmitLdfld(atomRefAtomType);
        cursor.EmitStloc(oldAtomType);
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "salt"));
        cursor.Remove();
        cursor.EmitLdloc(oldAtomType);

        // Projection
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt("Sim", "GetAtomReference"));
        cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdarg0());
        start = cursor.Prev;
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("AtomReference", "isHeldByArm"));
        removeIndex = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(removeIndex - cursor.Index);
        cursor.EmitLdloc(recipeVar);
        cursor.EmitLdfld(predicate);
        cursor.EmitLdarg0(); // Sim
        cursor.EmitLdloc(6); // ReferencePart::part
        cursor.EmitLdfld(referencePart);
        cursor.EmitCall(invokeAndClear);
        if (cursor.Next.OpCode == OpCodes.Brtrue) cursor.Next.OpCode = OpCodes.Brfalse;
        cursor.Index++;
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(1);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(15);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(16);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);
        cursor.EmitStloc(17);
        cursor.EmitLdloc(7);
        cursor.EmitLdcI4(1);
        cursor.EmitNewarr(atomType);
        cursor.EmitDup();
        cursor.EmitLdcI4(0);
        cursor.EmitLdloc(16);
        cursor.EmitLdfld(atomRefAtomType);
        cursor.EmitStelemRef();
        cursor.EmitStfld(processingAtoms);

        // Rejection
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt("Sim", "GetAtomReference"));
        cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdarg0());
        start = cursor.Prev;
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdloca(22));
        cursor.Index++;
        removeIndex = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(removeIndex - cursor.Index);
        cursor.EmitLdloc(recipeVar);
        cursor.EmitLdfld(predicate);
        cursor.EmitLdarg0(); // Sim
        cursor.EmitLdloc(6); // ReferencePart::part
        cursor.EmitLdfld(referencePart);
        cursor.EmitCall(invokeAndClear);
        cursor.Index++;
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdloc(19);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(21);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdloc(19);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);
        cursor.EmitStloc(22);
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "quicksilver"));
        cursor.Remove();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdloc(20);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "quicksilver"));
        cursor.Remove();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdloc(20);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);

        // Purification
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt("Sim", "GetAtomReference"));
        cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdarg0());
        start = cursor.Prev;
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("AtomReference", "isHeldByArm"));
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("AtomReference", "isHeldByArm"));
        removeIndex = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(removeIndex - cursor.Index);
        cursor.EmitLdloc(recipeVar);
        cursor.EmitLdfld(predicate);
        cursor.EmitLdarg0(); // Sim
        cursor.EmitLdloc(6); // ReferencePart::part
        cursor.EmitLdfld(referencePart);
        cursor.EmitCall(invokeAndClear);
        if (cursor.Next.OpCode == OpCodes.Brtrue) cursor.Next.OpCode = OpCodes.Brfalse;
        cursor.Index++;
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(28);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(1);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(29);
        cursor.RemoveRange(2);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdloc(27);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdflda("AtomType", "successorMetal"));
        cursor.RemoveRange(2);

        // Division
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt("Sim", "GetAtomReference"));
        cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdarg0());
        start = cursor.Prev;
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("AtomReference", "isHeldByArm"));
        removeIndex = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(removeIndex - cursor.Index);
        cursor.EmitLdloc(recipeVar);
        cursor.EmitLdfld(predicate);
        cursor.EmitLdarg0(); // Sim
        cursor.EmitLdloc(6); // ReferencePart::part
        cursor.EmitLdfld(referencePart);
        cursor.EmitCall(invokeAndClear);
        if (cursor.Next.OpCode == OpCodes.Brtrue) cursor.Next.OpCode = OpCodes.Brfalse;
        cursor.Index++;
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(39);
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdloc(40));
        while (!cursor.Next.MatchStelemRef()) cursor.Remove();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdcI4(-1);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdloc(40));
        while (!cursor.Next.MatchStelemRef()) cursor.Remove();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdcI4(1);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);

        // Proliferation
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt("Sim", "GetAtomReference"));
        cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdarg0());
        start = cursor.Prev;
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("AtomReference", "isHeldByArm"));
        removeIndex = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(removeIndex - cursor.Index);
        cursor.EmitLdloc(recipeVar);
        cursor.EmitLdfld(predicate);
        cursor.EmitLdarg0(); // Sim
        cursor.EmitLdloc(6); // ReferencePart::part
        cursor.EmitLdfld(referencePart);
        cursor.EmitCall(invokeAndClear);
        if (cursor.Next.OpCode == OpCodes.Brtrue) cursor.Next.OpCode = OpCodes.Brfalse;
        cursor.Index++;
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdloc(47);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(50);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdloc(49);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(51);
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchStelemRef());
        cursor.EmitDup();
        cursor.Emit(OpCodes.Ldc_I4_1);
        cursor.EmitLdloc(50);
        cursor.EmitLdfld(atomRefAtomType);
        cursor.EmitStelemRef();
        cursor.GotoPrev(MoveType.Before, instr => instr.OpCode == OpCodes.Ldc_I4_1);
        cursor.GotoPrev(MoveType.Before, instr => instr.OpCode == OpCodes.Ldc_I4_1);
        cursor.Remove();
        cursor.Emit(OpCodes.Ldc_I4_2);

        // Animismus
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt("Sim", "GetAtomReference"));
        cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdarg0());
        start = cursor.Prev;
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("AtomReference", "isHeldByArm"));
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("AtomReference", "isHeldByArm"));
        removeIndex = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(removeIndex - cursor.Index);
        cursor.EmitLdloc(recipeVar);
        cursor.EmitLdfld(predicate);
        cursor.EmitLdarg0(); // Sim
        cursor.EmitLdloc(6); // ReferencePart::part
        cursor.EmitLdfld(referencePart);
        cursor.EmitCall(invokeAndClear);
        if (cursor.Next.OpCode == OpCodes.Brtrue) cursor.Next.OpCode = OpCodes.Brfalse;
        cursor.Index++;
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(59);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(1);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(60);
        cursor.EmitLdloc(7);
        cursor.EmitLdcI4(4);
        cursor.EmitNewarr(atomType);
        cursor.EmitDup();
        cursor.EmitLdcI4(0);
        cursor.EmitLdloc(59);
        cursor.EmitLdfld(atomRefAtomType);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(1);
        cursor.EmitLdloc(60);
        cursor.EmitLdfld(atomRefAtomType);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(2);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdloc(57);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(3);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdloc(58);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);
        cursor.EmitStelemRef();
        cursor.EmitStfld(processingAtoms);
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "vitae"));
        cursor.Remove();
        cursor.EmitLdloc(7);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(2);
        cursor.EmitLdelemRef();
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "mors"));
        cursor.Remove();
        cursor.EmitLdloc(7);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(3);
        cursor.EmitLdelemRef();

        // Unification
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt("Sim", "GetAtomReference"));
        cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdarg0());
        start = cursor.Prev;
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdftn("Sim/LambdaGeneratedClass", "IsAir"));
        cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Bne_Un);
        var oldTarget = (Instruction)cursor.Prev.Operand;
        removeIndex = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(removeIndex - cursor.Index);
        cursor.EmitLdloc(recipeVar);
        cursor.EmitLdfld(predicate);
        cursor.EmitLdarg0(); // Sim
        cursor.EmitLdloc(6); // ReferencePart::part
        cursor.EmitLdfld(referencePart);
        cursor.EmitCall(invokeAndClear);
        cursor.Emit(OpCodes.Brfalse, oldTarget);
        cursor.EmitLdcI4(4);
        cursor.EmitNewarr(atomRef);
        cursor.EmitDup();
        cursor.EmitLdcI4(0);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(-1);
        cursor.EmitLdcI4(1);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(1);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(1);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(2);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(-1);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(3);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(1);
        cursor.EmitLdcI4(-1);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStelemRef();
        cursor.EmitStloc(67);
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdftn("Sim/LambdaGeneratedClass", "GetType"));
        cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdloc(67));
        start = cursor.Prev;
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchStfld("PartSimState", "processingAtoms"));
        removeIndex = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(removeIndex - cursor.Index);
        cursor.EmitLdcI4(5);
        cursor.EmitNewarr(atomType);
        cursor.EmitDup();
        cursor.EmitLdcI4(0);
        cursor.EmitLdloc(67);
        cursor.EmitLdcI4(0);
        cursor.EmitLdelemRef();
        cursor.EmitLdfld(atomRefAtomType);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(1);
        cursor.EmitLdloc(67);
        cursor.EmitLdcI4(1);
        cursor.EmitLdelemRef();
        cursor.EmitLdfld(atomRefAtomType);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(2);
        cursor.EmitLdloc(67);
        cursor.EmitLdcI4(2);
        cursor.EmitLdelemRef();
        cursor.EmitLdfld(atomRefAtomType);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(3);
        cursor.EmitLdloc(67);
        cursor.EmitLdcI4(3);
        cursor.EmitLdelemRef();
        cursor.EmitLdfld(atomRefAtomType);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(4);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);
        cursor.EmitStelemRef();
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "quintessence"));
        cursor.Remove();
        cursor.EmitLdloc(7);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(4);
        cursor.EmitLdelemRef();

        // Dispersion
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt("Sim", "GetAtomReference"));
        cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdarg0());
        start = cursor.Prev;
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("AtomReference", "isHeldByArm"));
        removeIndex = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(removeIndex - cursor.Index);
        cursor.EmitLdloc(recipeVar);
        cursor.EmitLdfld(predicate);
        cursor.EmitLdarg0(); // Sim
        cursor.EmitLdloc(6); // ReferencePart::part
        cursor.EmitLdfld(referencePart);
        cursor.EmitCall(invokeAndClear);
        if (cursor.Next.OpCode == OpCodes.Brtrue) cursor.Next.OpCode = OpCodes.Brfalse;
        cursor.Index++;
        cursor.EmitLdarg0();
        cursor.EmitLdfld(inputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomRef);
        cursor.EmitStloc(71);
        cursor.EmitLdloc(7);
        cursor.EmitLdcI4(5);
        cursor.EmitNewarr(atomType);
        cursor.EmitDup();
        cursor.EmitLdcI4(0);
        cursor.EmitLdloc(71);
        cursor.EmitLdfld(atomRefAtomType);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(1);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdcI4(-1);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(2);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdcI4(0);
        cursor.EmitLdcI4(-1);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(3);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdcI4(1);
        cursor.EmitLdcI4(-1);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);
        cursor.EmitStelemRef();
        cursor.EmitDup();
        cursor.EmitLdcI4(4);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(outputs);
        cursor.EmitLdcI4(1);
        cursor.EmitLdcI4(0);
        cursor.EmitNewobj(hexCtor);
        cursor.EmitCallvirt(getIRecipeIO);
        cursor.EmitIsinst(atomType);
        cursor.EmitStelemRef();
        cursor.EmitStfld(processingAtoms);
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "air"));
        cursor.Remove();
        cursor.EmitLdloc(7);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(1);
        cursor.EmitLdelemRef();
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "fire"));
        cursor.Remove();
        cursor.EmitLdloc(7);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(2);
        cursor.EmitLdelemRef();
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "water"));
        cursor.Remove();
        cursor.EmitLdloc(7);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(3);
        cursor.EmitLdelemRef();
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld("AtomTypes", "earth"));
        cursor.Remove();
        cursor.EmitLdloc(7);
        cursor.EmitLdfld(processingAtoms);
        cursor.EmitLdcI4(4);
        cursor.EmitLdelemRef();

        // Disposal
        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt("Sim", "GetAtomReference"));
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("AtomReference", "isHeldByArm"));
        cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Brtrue);
        var target = (Instruction)cursor.Prev.Operand;
        cursor.EmitLdsfld(atomTags);
        cursor.EmitLdstr("om:indisposable");
        cursor.EmitCall(stringToId);
        cursor.EmitCallvirt(getTag);
        cursor.EmitLdloc(76);
        cursor.EmitCallvirt(hasAtomTag);
        cursor.Emit(OpCodes.Brtrue, target);

        referenceCode.DeclaringType.Methods.Remove(referenceCode);
    }
    private void PatchRecipeSystemCodeReference(Identifier id, PartSimState simState) { // this method is oly used as a source to copy relevant IL code from
        if (GlyphRecipe.Recipes.TryGetValue(id, out var recipes)) {
            var enumerator = recipes.GetEnumerator();
            enumerator.MoveNext();
            var current = enumerator.Current;
            GetValue(current).Predicate.InvokeAndClear(null, null);
            var @in = RecipeInputs;
            var atomRefOut = RecipeOutputs[new HexIndex(0, 0)] as AtomReference;
            var procAtoms = simState.processingAtoms;
            @in.recipe = null;
            AtomTag.AtomTags["om:calcifiable"].HasAtom(atomRefOut);
        }
    }
    private static GlyphRecipe GetValue(KeyValuePair<Identifier, GlyphRecipe> pair) => pair.Value; // Workaround for the weirdest internal CLR error ever

    [MonoModILInject("ProcessInputs")]
    public static void PatchProcessInputsRejection(MethodDefinition method, CustomAttribute attrib) {
        ILCursor cursor = new(new ILContext(method));

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdfld("Sim", "simulationDict"));
        FieldReference simulationDict = cursor.Next.Operand as FieldReference;
        VariableReference part = cursor.Next.Next.Operand as VariableReference;
        MethodReference getItem = cursor.Next.Next.Next.Operand as MethodReference;
        FieldDefinition wasActivated = MonoModRule.Modder.FindType("PartSimState").Resolve().Fields.First(field => field.Name == "wasActivated");

        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt("Sim", "GetAtomReference"));
        cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdarg0());
        Instruction start = cursor.Prev;
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld("AtomType", "predecessorMetal"));
        cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Brfalse_S);
        int removeIndex = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(removeIndex - cursor.Index);

        cursor.EmitLdarg0();
        cursor.EmitLdfld(simulationDict);
        cursor.EmitLdloc(part);
        cursor.EmitCallvirt(getItem);
        cursor.EmitLdfld(wasActivated);
    }

    #endregion

    #region PartCycleDelegates

    public void RunPartCycleDelegate(ReferredPart referredPart, PartSimState simState, bool isCycleStart, GlyphRecipe recipe, PartCycleExecutionType executionType) {
        PartCycleDelegate cycleDelegate = ((patch_PartType)(object)referredPart.part.GetType()).CycleDelegate;
        if (cycleDelegate != null && cycleDelegate.ExecutionType.HasFlag(executionType)) {
            bool delegateActivated = cycleDelegate.Delegate.Invoke(this, referredPart.part, simState, recipe, isCycleStart);
            simState.wasActivated = delegateActivated || simState.wasActivated;
        }
    }

    [MonoModILInject("RunCycleGlyphs")]
    public static void PatchGlyphBehaviour(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching glyph Behaviour");
        if (!method.HasBody) {
            Console.WriteLine("Unable to patch glyph behaviour (no body)");
            throw new Exception();
        }
        ILCursor cursor = new(new ILContext(method));

        if (!cursor.TryGotoNext(MoveType.Before,
            instr => instr.MatchLdloc(6),
            instr => instr.MatchLdfld(out FieldReference f) && f.Name == "part",
            instr => instr.MatchCallvirt(out MethodReference m) && m.Name == "GetType",
            instr => instr.MatchLdfld(out FieldReference f) && f.Name == "bonders",
            instr => instr.MatchLdlen()
        )) {
            Console.WriteLine("Unable to patch glyph behaviour (no bonder check)");
            throw new Exception();
        }

        TypeDefinition holder = MonoModRule.Modder.FindType("Sim").Resolve();
        MethodDefinition to = holder.Methods.First(m => m.Name.Equals("RunPartCycleDelegate"));
        VariableReference recipe = method.Body.Variables.First(var => var.VariableType.FullName == "Quintessential.GlyphRecipe");
        Instruction oldTarget = cursor.Next;
        cursor.EmitLdarg0();
        Instruction newTarget = cursor.Previous;
        cursor.EmitLdloc(6);
        cursor.EmitLdloc(7);
        cursor.EmitLdarg1();
        cursor.EmitLdloc(recipe);
        cursor.EmitLdcI4(1); // PartCycleExecutionType - 1
        cursor.EmitCall(to);
        // I don't know why it never works, but MonoMod's goto and branch handling is not functional, or I don't know how it works.
        // -- Whoever wrote this didn't understand IL cursor branch handling. -- Fate
        foreach (var v in cursor.Instrs.Where(v => v.Operand is Instruction t && t == oldTarget)) {
            v.Operand = newTarget;
        }

        cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Brfalse);
        Instruction toEdit = cursor.Previous;
        cursor.Goto((Instruction)cursor.Previous.Operand, MoveType.Before);
        cursor.EmitLdarg0();
        newTarget = cursor.Previous;
        cursor.EmitLdloc(6);
        cursor.EmitLdloc(7);
        cursor.EmitLdarg1();
        cursor.EmitLdloc(recipe);
        cursor.EmitLdcI4(2); // PartCycleExecutionType - 2
        cursor.EmitCall(to);

        cursor.Goto(toEdit);
        cursor.Next.Operand = newTarget;
    }

    #endregion

    #region CycleEvents

    public static List<CycleEvent> CycleEvents = [];
    public void RunCycleEvents(CycleEventExecutionType executionType) {
        foreach (var cycleDelegate in CycleEvents) {
            if (cycleDelegate.ExecutionType.HasFlag(executionType)) {
                cycleDelegate.Delegate.Invoke(this, executionType);
            }
        }
        foreach (var simState in simulationDict) {
            ((patch_PartSimState)(object)simState.Value).OnCycleCallback(this, executionType);
        }
        foreach (var molecule in molecules) {
            ((patch_Molecule)(object)molecule).OnCycleCallback(this, executionType);
        }
        foreach (var component in Components) {
            component.Value.OnCycleCallback(this, executionType);
        }
        //if (QuintessentialDataSettings.Puzzle == null) {

        //    if (executionType == CycleEventExecutionType.First && GetCycle() == 0) {
        //        QuintessentialDataSettings.Puzzle = Codecs.PUZZLE.Encode(JsonCodecMap.Instance, solutionEditor.GetSolution().GetPuzzle());
        //        Logger.LogNoTime(QuintessentialDataSettings.Puzzle.ToString());
        //    }
        //    if (executionType == CycleEventExecutionType.First && GetCycle() == 0) {
        //        QuintessentialDataSettings.Solution = Codecs.SOLUTION.Encode(JsonCodecMap.Instance, solutionEditor.GetSolution());
        //        Logger.LogNoTime(QuintessentialDataSettings.Solution.ToString());
        //    }
        //}
        //if (executionType == CycleEventExecutionType.First && GetCycle() < solutionEditor.GetSolution().parts.Count) {
        //    var json = patch_Part.PART.Encode(JsonCodecMap.Instance, solutionEditor.GetSolution().parts[GetCycle()]);
        //    Logger.LogNoTime(json.ToString());
        //}
    }

    [MonoModILInject("BeginCycle")]
    public static void PatchCycleEventsBegin(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching Cycle Events");
        if (!method.HasBody) {
            Console.WriteLine("Unable to patch Cycle Events (no body)");
            throw new Exception();
        }
        ILCursor cursor = new(new ILContext(method));
        TypeDefinition holder = MonoModRule.Modder.FindType("Sim").Resolve();
        MethodDefinition to = holder.Methods.First(m => m.Name.Equals("RunCycleEvents"));

        cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt("Sim", "ResetSimStates"));
        cursor.EmitLdarg0();
        cursor.EmitLdcI4(1); // CycleEventExecutionType - 1
        cursor.EmitCall(to);

        cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt("Sim", "RunCycleInstructions"));
        cursor.EmitLdarg0();
        cursor.EmitLdcI4(2); // CycleEventExecutionType - 2
        cursor.EmitCall(to);

        cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt("Sim", "RunCycleGlyphs"));
        cursor.EmitLdarg0();
        cursor.EmitLdcI4(4); // CycleEventExecutionType - 4
        cursor.EmitCall(to);

        cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt("Sim", "RunCycleInstructions"));
        cursor.EmitLdarg0();
        cursor.EmitLdcI4(8); // CycleEventExecutionType - 8
        cursor.EmitCall(to);
    }
    [MonoModILInject("EndCycle")]
    public static void PatchCycleEventsEnd(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching Cycle Events");
        if (!method.HasBody) {
            Console.WriteLine("Unable to patch Cycle Events (no body)");
            throw new Exception();
        }
        ILCursor cursor = new(new ILContext(method));
        TypeDefinition holder = MonoModRule.Modder.FindType("Sim").Resolve();
        MethodDefinition to = holder.Methods.First(m => m.Name.Equals("RunCycleEvents"));

        cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Brtrue_S);
        Instruction toEdit = cursor.Previous;

        cursor.TryGotoNext(MoveType.After, instr => instr.MatchThrow());
        cursor.EmitLdarg0();
        toEdit.Operand = cursor.Previous;
        cursor.EmitLdcI4(16); // CycleEventExecutionType - 16
        cursor.EmitCall(to);

        cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt("Sim", "RunCycleGlyphs"));
        cursor.EmitLdarg0();
        cursor.EmitLdcI4(32); // CycleEventExecutionType - 32
        cursor.EmitCall(to);

        cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt("Sim", "MeasureArmFootprint"));
        cursor.EmitLdarg0();
        cursor.EmitLdcI4(64); // CycleEventExecutionType - 64
        cursor.EmitCall(to);
    }

    #endregion

    #region Ctor

    private void InitObjectsInCtor() {
        RecipeInputs = [];
        RecipeOutputs = [];
        Components = [];
    }

    [MonoModILInject(".ctor")]
    static void PatchCtor(MethodDefinition method, CustomAttribute attribute) {

        MonoModRule.Modder.Log("Patching Sim init.");
        if (!method.HasBody) {
            throw new Exception("Unable to patch Sim init. (no body)");
        }
        ILCursor cursor = new(new ILContext(method));
        MethodReference init = method.DeclaringType.Methods.First(f => f.Name.Equals("InitObjectsInCtor"));
        cursor.EmitLdarg0();
        cursor.EmitCall(init);
    }

    #endregion
}
