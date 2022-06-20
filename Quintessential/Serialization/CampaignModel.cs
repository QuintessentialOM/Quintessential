using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace Quintessential.Serialization;

public class CampaignModel {

	public string Name, Title, Music, ButtonBase;
	public IList<ChapterModel> Chapters { get; set; }

	[YamlIgnore]
	public string Path = "";
}

public class ChapterModel {

	public string Title, Subtitle, Place, Background;
	public bool IsLeftSide;
	public ChapterButtonModel Button;
	public IList<EntryModel> Entries;
}

public class ChapterButtonModel
{
	public string Hover, Locked, Unlocked, Gem, Position;
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
