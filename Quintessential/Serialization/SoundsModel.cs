using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Quintessential.Serialization;

public class SoundsModel {

	public List<SoundModel> Sounds = new();
	public List<SongModel> Songs = new();
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