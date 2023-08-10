using SpectraUtils.Abstract;
using SpectraUtils.Concerete;
using SpectraUtils.Concrete;

namespace SpectraUtils
{
    public class SpectraUtil : ISpectraUtil
    {
        public INameEdit NameEdit { get; }

        public IPasswordHelper PasswordHelper { get; }

        public SpectraUtil()
        {
            NameEdit = new NameEdit();
            PasswordHelper = new PasswordHelper();
        }

    }
}
