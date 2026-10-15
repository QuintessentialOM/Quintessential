class patch_PuzzleInputOutput{

	/// <summary>
	/// When >0, the overrides the required amount for this output.
	/// <para>
	/// For example, if the puzzle has an output multiplier of 2, but this field is set to 7 for an output, that output
	/// only requires 7 products to be placed on it to validate.
	/// </para>
	/// This has no effect on inputs.
	/// </summary>
	public int AmountOverride = 0;
}