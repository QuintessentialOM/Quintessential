namespace Quintessential;

/// <summary>
/// A general <b>unique</b>, string convertable identifier.
/// </summary>
public readonly struct Identifier {
    /// <summary>
    /// The <see cref="Identifier"/> is formatted as <c>namespc:name</c>.
    /// </summary>
    public readonly string namespc, name;

    public Identifier(string namespc, string name) {
        if (namespc.Contains(':') || name.Contains(':'))
            throw new OpusMagnumException($"Identifier constructor '{namespc + ":" + name}' contains invalid number of ':'");
        this.namespc = namespc;
        this.name = name;
    }
    public Identifier(string identifier) {
        var sections = identifier.Split(':');
        if (sections.Length != 2)
            throw new OpusMagnumException($"Identifier constructor '{identifier}' contains invalid number of ':'");

        namespc = sections[0];
        name = sections[1];
    }

    public static implicit operator Identifier(string identifier) {
        return new Identifier(identifier);
    }
    public static implicit operator string(Identifier identifier) {
        return identifier.ToString();
    }

    public readonly override string ToString() {
        return namespc + ":" + name;
    }
    public static bool operator ==(Identifier i1, Identifier i2) {
        return i1.name == i2.name && i1.namespc == i2.namespc;
    }
    public static bool operator !=(Identifier i1, Identifier i2) {
        return i1.name != i2.name || i1.namespc != i2.namespc;
    }
    public readonly override bool Equals(object obj) {
        if (obj is Identifier i) {
            return name == i.name && namespc == i.namespc;
        }
        return false;
    }
    public readonly override int GetHashCode() {
        return ToString().GetHashCode();
    }
}