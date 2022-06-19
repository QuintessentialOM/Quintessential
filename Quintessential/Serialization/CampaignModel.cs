using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace Quintessential.Serialization;

public class CampaignModel {

	public string Name, Title, Music;

	public IList<ChapterModel> Chapters { get; set; }

	[YamlIgnore]
	public string Path = "";
}

public class ChapterModel {

	public string Title, Subtitle, Place, Background;
	public bool IsLeftSide;

	// TODO: wheel icons

	public IList<EntryModel> Entries;
}

public class EntryModel {

	// TODO: multiple requirements, solitaires and documents, tutorials

	public string ID, Title, Song, Fanfare, Puzzle, Solitaire, Requires;
	public CutsceneModel Cutscene;
}
public class CutsceneModel
{

	// TODO: multiple requirements, solitaires and documents, tutorials

	public string Location, Background;
	public bool SlowFade;
}
