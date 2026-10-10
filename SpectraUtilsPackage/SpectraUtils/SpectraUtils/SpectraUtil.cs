using SpectraUtils.Abstractions;
using SpectraUtils.Services;

namespace SpectraUtils;

/// <summary>
/// Default <see cref="ISpectraUtil"/> implementation. All services are stateless and thread-safe,
/// so a single instance can be shared (it is registered as a singleton by <c>AddSpectraUtils</c>).
/// </summary>
public sealed class SpectraUtil : ISpectraUtil
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpectraUtil"/> class with default implementations.
    /// </summary>
    public SpectraUtil()
        : this(new NameEdit(), new PasswordHelper(), new Pbkdf2PasswordHasher(), new SecureTokenGenerator(), new OneTimeCodeGenerator())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectraUtil"/> class with custom implementations.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <see langword="null"/>.</exception>
    public SpectraUtil(INameEdit nameEdit, IPasswordHelper passwordHelper,
                       IPasswordHasher passwordHasher, ISecureTokenGenerator tokenGenerator,
                       IOneTimeCodeGenerator oneTimeCodeGenerator)
    {
        ArgumentNullException.ThrowIfNull(nameEdit);
        ArgumentNullException.ThrowIfNull(passwordHelper);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(tokenGenerator);
        ArgumentNullException.ThrowIfNull(oneTimeCodeGenerator);

        NameEdit = nameEdit;
        PasswordHelper = passwordHelper;
        PasswordHasher = passwordHasher;
        TokenGenerator = tokenGenerator;
        OneTimeCodeGenerator = oneTimeCodeGenerator;
    }

    /// <inheritdoc />
    public INameEdit NameEdit { get; }

    /// <inheritdoc />
    public IPasswordHelper PasswordHelper { get; }

    /// <inheritdoc />
    public IPasswordHasher PasswordHasher { get; }

    /// <inheritdoc />
    public ISecureTokenGenerator TokenGenerator { get; }

    /// <inheritdoc />
    public IOneTimeCodeGenerator OneTimeCodeGenerator { get; }
}
