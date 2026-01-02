using SpectraUtils.Abstract;
using SpectraUtils.Concerete;
using SpectraUtils.Concrete;

namespace SpectraUtils;

/// <summary>
/// Provides utility methods for name editing and password management.
/// This class can be registered as a singleton in dependency injection containers.
/// </summary>
public sealed class SpectraUtil : ISpectraUtil
{
    private readonly Lazy<INameEdit> _nameEdit;
    private readonly Lazy<IPasswordHelper> _passwordHelper;

    /// <summary>
    /// Gets the name editing utility for correcting and formatting names.
    /// </summary>
    public INameEdit NameEdit => _nameEdit.Value;

    /// <summary>
    /// Gets the password helper utility for creating and hashing passwords.
    /// </summary>
    public IPasswordHelper PasswordHelper => _passwordHelper.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectraUtil"/> class with default implementations.
    /// </summary>
    public SpectraUtil()
        : this(new Lazy<INameEdit>(() => new NameEdit()), new Lazy<IPasswordHelper>(() => new PasswordHelper()))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectraUtil"/> class with custom implementations.
    /// Useful for dependency injection scenarios.
    /// </summary>
    /// <param name="nameEdit">The name editing utility instance.</param>
    /// <param name="passwordHelper">The password helper utility instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public SpectraUtil(INameEdit nameEdit, IPasswordHelper passwordHelper)
        : this(new Lazy<INameEdit>(() => nameEdit), new Lazy<IPasswordHelper>(() => passwordHelper))
    {
        ArgumentNullException.ThrowIfNull(nameEdit);
        ArgumentNullException.ThrowIfNull(passwordHelper);
    }

    private SpectraUtil(Lazy<INameEdit> nameEdit, Lazy<IPasswordHelper> passwordHelper)
    {
        _nameEdit = nameEdit;
        _passwordHelper = passwordHelper;
    }
}
