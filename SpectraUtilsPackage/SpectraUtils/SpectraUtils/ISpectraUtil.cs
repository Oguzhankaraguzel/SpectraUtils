using SpectraUtils.Abstract;

namespace SpectraUtils;

/// <summary>
/// Interface for the main utility class providing access to name editing and password management services.
/// </summary>
public interface ISpectraUtil
{
    /// <summary>
    /// Gets the name editing utility for correcting and formatting names.
    /// </summary>
    INameEdit NameEdit { get; }

    /// <summary>
    /// Gets the password helper utility for creating and hashing passwords.
    /// </summary>
    IPasswordHelper PasswordHelper { get; }
}
