using SpectraUtils.Abstract;

namespace SpectraUtils
{
    public interface ISpectraUtil
    {
        INameEdit NameEdit { get; }
        IPasswordHelper PasswordHelper { get; }
    }
}
