using Quintessential.Serialization;
using System;
using System.Text.Json.Serialization;

namespace Quintessential;

/// <summary>
/// A code mod to be loaded by <see cref="Quintessential"/>.
/// </summary>
public abstract class QuintessentialMod {
    /// <summary>
    /// The <b>unique</b> id of the code mod specified in the modMeta.jsonc file.
    /// </summary>
    public abstract string ModId { get; }
    /// <summary>
    /// The settings type for the mod to be displayed and edited in the <see cref="ModsScreen"/>.
    /// </summary>
	public virtual Type SettingsType => typeof(object);
    /// <summary>
    /// Gets an <see cref="Identifier"/> with the provided <paramref name="name"/> and the <b>ModId</b> as the namespace. 
    /// </summary>
    /// <param name="name">The name of the <see cref="Identifier"/>.</param>
    public virtual Identifier GetIdentifier(string name) => new(ModId, name);
    /// <summary>
    /// Translates the translation key with the <b>ModId</b> as a base root.
    /// </summary>
    /// <param name="key">The higher levels for the translation key.</param>
    /// <returns>The localized result.</returns>
	public virtual LocString Translate(string key = "") => key == "" ? Translations.Translate(ModId) : Translations.Translate(ModId + "." + key);

    /// <summary>
    /// The meta data associated with this mod.
    /// </summary>
    public ModMeta Meta;
    /// <summary>
    /// The current settings for the mod.
    /// </summary>
	public object Settings;

    /// <summary>
    /// The first method to be called by <see cref="Quintessential"/> in the<br/>
    /// dependency order, just before the pre-Init stage is complete.
    /// <para>
    /// Mods should <b>not</b> load content at this point.
    /// </para>
    /// However <see cref="JsonConverter"/> types can be added to the <see cref="DataSerializer"/>.
    /// </summary>
	public abstract void Load();

    /// <summary>
    /// The first point when mods are able to add content to the game.<br/>
    /// Called just after vanilla content was initialized in dependency order.
    /// <para>
    /// Add anything not related to other mods, or only related to dependencies at this step.
    /// </para>
    /// </summary> //TODO mention that tags and other data-content is added between the two
	public abstract void LoadContent();
    /// <summary>
    /// The second point when mods are able to add content to the game.<br/>
    /// Called in dependency order.
    /// <para>
    /// Add anything compat related, that requires other mods to have added their content before.
    /// </para>
    /// </summary>
    public abstract void LoadCompatContent();
    /// <summary>
    /// The last point when mods are able to add content to the game. Called in <br/>
    /// dependency order, after modded puzzles and campaigns where constructed.<br/>
    /// Should be handled with <b>care</b>!
    /// <para>
    /// Make any final changes that should not be overridden.
    /// </para>
    /// </summary>
    public abstract void FinaliseContent();

    /// <summary>
    /// Called after all content has been loaded, in dependency order.
    /// </summary>
    public abstract void PostLoad();

    /// <summary>
    /// Called before the game unloads, in dependency order.
    /// </summary>
	public abstract void Unload();

    /// <summary>
    /// Called to apply the mod settings to the game. Either before<br/>
    /// <see cref="PostLoad"/>, or when saving them in the <see cref="ModsScreen"/>.
    /// </summary>
	public virtual void ApplySettings() {

	}
}
