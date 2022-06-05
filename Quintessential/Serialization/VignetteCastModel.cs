﻿using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Quintessential.Serialization;

public class VignetteCastModel {

	public List<VignetteActorModel> Cast = new();

	[YamlIgnore]
	public string Path = "";
}
public class VignetteActorModel
{
	public string nameInVignette, nameInGame, smallPortrait, largePortrait;

	public int color;

	public bool isOnLeft;
}