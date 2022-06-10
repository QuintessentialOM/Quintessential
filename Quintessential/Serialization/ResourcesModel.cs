using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Quintessential.Serialization;

public class ResourcesModel
{
	public List<SoundModel> Sounds = new();
	public List<SongModel> Songs = new();
	public List<VignetteActorModel> Characters = new();
	public List<LocationModel> Locations = new();
}
public class SoundModel
{
	public string Path;
	public float Volume;
}
public class SongModel
{
	public string Path;
}
public class VignetteActorModel
{
	public string ID, Name, SmallPortrait, LargePortrait;

	public int Color;

	public bool IsOnLeft;
}

public class LocationModel
{
	public string Name, Background;
}
