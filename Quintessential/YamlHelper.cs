﻿using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectFactories;

namespace Quintessential;

// From https://github.com/EverestAPI/Everest/blob/dev/Celeste.Mod.mm/Mod/Helpers/YamlHelper.cs
// Credit goes to 0x0ade, max4805, and coloursofnoise
public static class YamlHelper {

	public static IDeserializer Deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
	public static ISerializer Serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve).Build();

	/// <summary>
	/// Builds a deserializer that will provide YamlDotNet with the given object instead of creating a new one.
	/// This will make YamlDotNet update this object when deserializing.
	/// </summary>
	/// <param name="objectToBind">The object to set fields on</param>
	/// <returns>The newly-created deserializer</returns>
	public static IDeserializer DeserializerUsing(object objectToBind) {
		IObjectFactory defaultObjectFactory = new DefaultObjectFactory();
		Type objectType = objectToBind.GetType();

		return new DeserializerBuilder()
			.IgnoreUnmatchedProperties()
			// provide the given object if type matches, fall back to default behavior otherwise.
			.WithObjectFactory(type => type == objectType ? objectToBind : defaultObjectFactory.Create(type))
			.Build();
	}
}
