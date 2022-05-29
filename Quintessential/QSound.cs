using System.Collections.Generic;
using System.IO;
using MonoMod.Utils;

namespace Quintessential;

public class QSound
{
	public static readonly List<QSound> AllQSounds = new();

	public Sound sound;
	public float DefaultVolume = 1f;

	/// <summary>
	/// Loads a .wav file from disk. Returns the new QSound.
	/// </summary>
	/// <param name="path">The file path to the sound.</param>
	/// <param name="defaultVolume">The default volume of the sound, between 0.0f and 1.0f inclusive.</param>
	public static QSound load(string path, float defaultVolume = 1f)
	{
		var q = new QSound();
		q.DefaultVolume = defaultVolume;
		string filePath = Path.Combine("Content", path) + ".wav";
		for (int i = QuintessentialLoader.ModContentDirectories.Count - 1; i >= 0; i--)
		{
			string dir = Path.Combine(QuintessentialLoader.ModContentDirectories[i],"Content");
			string str = Path.Combine(dir, path) + ".wav";
			if (File.Exists(str))
			{
				filePath = str;
				break;
			}
		}
		q.sound = new Sound() { field_4061 = class_158.method_375(filePath) };
		AllQSounds.Add(q);
		return q;
	}

	/// <summary>
	/// Plays a Qsound using the given volume.
	/// </summary>
	/// <param name="volume">Desired volume, between 0.0f (muted) and 1.0f (full volume).</param>
	public void play(float volume)
	{
		sound.method_2148(volume);
		//sound.field_4062 = false; // debug - remove once method_504() patch is working
	}

	/// <summary>
	/// Plays a Qsound at its default volume.
	/// </summary>
	public void play() { play(DefaultVolume); }

	/// <summary>
	/// Returns a volume factor of either 0.0f, 0.5f or 1.0f.
	/// Useful for achieving the volume reduction that occurs when a simulation is running in QuickMode or the GIF recorder.
	/// Providing no arguments will return 1.0f.
	/// </summary>
	/// <param name="sim">The current Sim. If specified, the "seb" parameter is ignored.</param>
	/// <param name="seb">The current SolutionEditorBase.</param>
	public static float getVolumeFactor(Sim sim = null, SolutionEditorBase seb = null)
	{
		if (sim != null) seb = new DynamicData(sim).Get<SolutionEditorBase>("field_3818");
		if (seb != null)
		{
			if (seb is class_194) { return 0.0f; } // muted during GIF recording
			else if (seb is SolutionEditorScreen)
			{
				var seb_dyn = new DynamicData(seb);
				bool isQuickMode = seb_dyn.Get<Maybe<int>>("field_4030").method_1085();
				return isQuickMode ? 0.5f : 1f;
			}
		}
		return 1f;
	}

	public enum OMSoundID : int
	{
		click_button,
		click_deselect,
		click_select,
		click_story,
		close_enter,
		close_leave,
		code_button,
		code_failure,
		code_success,
		fanfare_solving1,
		fanfare_solving2,
		fanfare_solving3,
		fanfare_solving4,
		fanfare_solving5,
		fanfare_solving6,
		fanfare_story1,
		fanfare_story2,
		glyph_animismus,
		glyph_bonding,
		glyph_calcification,
		glyph_dispersion,
		glyph_disposal,
		glyph_duplication,
		glyph_projection,
		glyph_purification,
		glyph_triplex1,
		glyph_triplex2,
		glyph_triplex3,
		glyph_unbonding,
		glyph_unification,
		instruction_pickup,
		instruction_place,
		instruction_remove,
		piece_modify,
		piece_pickup,
		piece_place,
		piece_remove,
		piece_rotate,
		release_button,
		sim_error,
		sim_start,
		sim_step,
		sim_stop,
		solitaire_end,
		solitaire_match,
		solitaire_select,
		solitaire_start,
		solution,
		title,
		ui_complete,
		ui_fade,
		ui_modal,
		ui_modal_close,
		ui_paper,
		ui_paper_back,
		ui_transition,
		ui_transition_back,
		ui_unlock,
	}

	private static readonly Dictionary<OMSoundID, Sound> OMSounds = new()
	{
		{ OMSoundID.click_button, class_238.field_1991.field_1821 },
		{ OMSoundID.click_deselect, class_238.field_1991.field_1822 },
		{ OMSoundID.click_select, class_238.field_1991.field_1823 },
		{ OMSoundID.click_story, class_238.field_1991.field_1824 },
		{ OMSoundID.close_enter, class_238.field_1991.field_1825 },
		{ OMSoundID.close_leave, class_238.field_1991.field_1826 },
		{ OMSoundID.code_button, class_238.field_1991.field_1827 },
		{ OMSoundID.code_failure, class_238.field_1991.field_1828 },
		{ OMSoundID.code_success, class_238.field_1991.field_1829 },
		{ OMSoundID.fanfare_solving1, class_238.field_1991.field_1830 },
		{ OMSoundID.fanfare_solving2, class_238.field_1991.field_1831 },
		{ OMSoundID.fanfare_solving3, class_238.field_1991.field_1832 },
		{ OMSoundID.fanfare_solving4, class_238.field_1991.field_1833 },
		{ OMSoundID.fanfare_solving5, class_238.field_1991.field_1834 },
		{ OMSoundID.fanfare_solving6, class_238.field_1991.field_1835 },
		{ OMSoundID.fanfare_story1, class_238.field_1991.field_1836 },
		{ OMSoundID.fanfare_story2, class_238.field_1991.field_1837 },
		{ OMSoundID.glyph_animismus, class_238.field_1991.field_1838 },
		{ OMSoundID.glyph_bonding, class_238.field_1991.field_1839 },
		{ OMSoundID.glyph_calcification, class_238.field_1991.field_1840 },
		{ OMSoundID.glyph_dispersion, class_238.field_1991.field_1841 },
		{ OMSoundID.glyph_disposal, class_238.field_1991.field_1842 },
		{ OMSoundID.glyph_duplication, class_238.field_1991.field_1843 },
		{ OMSoundID.glyph_projection, class_238.field_1991.field_1844 },
		{ OMSoundID.glyph_purification, class_238.field_1991.field_1845 },
		{ OMSoundID.glyph_triplex1, class_238.field_1991.field_1846 },
		{ OMSoundID.glyph_triplex2, class_238.field_1991.field_1847 },
		{ OMSoundID.glyph_triplex3, class_238.field_1991.field_1848 },
		{ OMSoundID.glyph_unbonding, class_238.field_1991.field_1849 },
		{ OMSoundID.glyph_unification, class_238.field_1991.field_1850 },
		{ OMSoundID.instruction_pickup, class_238.field_1991.field_1851 },
		{ OMSoundID.instruction_place, class_238.field_1991.field_1852 },
		{ OMSoundID.instruction_remove, class_238.field_1991.field_1853 },
		{ OMSoundID.piece_modify, class_238.field_1991.field_1854 },
		{ OMSoundID.piece_pickup, class_238.field_1991.field_1855 },
		{ OMSoundID.piece_place, class_238.field_1991.field_1856 },
		{ OMSoundID.piece_remove, class_238.field_1991.field_1857 },
		{ OMSoundID.piece_rotate, class_238.field_1991.field_1858 },
		{ OMSoundID.release_button, class_238.field_1991.field_1859 },
		{ OMSoundID.sim_error, class_238.field_1991.field_1860 },
		{ OMSoundID.sim_start, class_238.field_1991.field_1861 },
		{ OMSoundID.sim_step, class_238.field_1991.field_1862 },
		{ OMSoundID.sim_stop, class_238.field_1991.field_1863 },
		{ OMSoundID.solitaire_end, class_238.field_1991.field_1864 },
		{ OMSoundID.solitaire_match, class_238.field_1991.field_1865 },
		{ OMSoundID.solitaire_select, class_238.field_1991.field_1866 },
		{ OMSoundID.solitaire_start, class_238.field_1991.field_1867 },
		{ OMSoundID.solution, class_238.field_1991.field_1868 },
		{ OMSoundID.title, class_238.field_1991.field_1869 },
		{ OMSoundID.ui_complete, class_238.field_1991.field_1870 },
		{ OMSoundID.ui_fade, class_238.field_1991.field_1871 },
		{ OMSoundID.ui_modal, class_238.field_1991.field_1872 },
		{ OMSoundID.ui_modal_close, class_238.field_1991.field_1873 },
		{ OMSoundID.ui_paper, class_238.field_1991.field_1874 },
		{ OMSoundID.ui_paper_back, class_238.field_1991.field_1875 },
		{ OMSoundID.ui_transition, class_238.field_1991.field_1876 },
		{ OMSoundID.ui_transition_back, class_238.field_1991.field_1877 },
		{ OMSoundID.ui_unlock, class_238.field_1991.field_1878 },
	};

	/// <summary>
	/// Returns the vanilla Sound associated with the OMSoundID.
	/// </summary>
	/// <param name="id">The ID of the sound.</param>
	public static Sound fetch(OMSoundID id) { return OMSounds[id]; }

	/// <summary>
	/// Plays the vanilla Sound assocated with the OMSoundID at the specified volume.
	/// </summary>
	/// <param name="id">The ID of the sound.</param>
	/// <param name="volume">Desired volume, between 0.0f (muted) and 1.0f (full volume). Defaults to 1.0f</param>
	public static void play(OMSoundID id, float volume = 1f) { OMSounds[id].method_28(volume); }
}