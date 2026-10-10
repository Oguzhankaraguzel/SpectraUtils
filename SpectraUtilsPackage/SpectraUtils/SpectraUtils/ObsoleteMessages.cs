namespace SpectraUtils;

/// <summary>
/// Messages and diagnostic ids for obsolete members.
/// </summary>
internal static class ObsoleteMessages
{
    public const string HashPasswordId = "SPECTRA001";

    public const string HashPassword =
        "Unsalted SHA-256 is not suitable for storing passwords. Use IPasswordHasher (PBKDF2) instead. " +
        "Keep this member only to verify legacy hashes during a migration; it will be removed in a future major version.";
}
