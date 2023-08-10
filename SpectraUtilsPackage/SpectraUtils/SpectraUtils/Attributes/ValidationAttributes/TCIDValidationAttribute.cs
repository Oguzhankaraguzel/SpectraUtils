using System.ComponentModel.DataAnnotations;

namespace SpectraUtils.Attributes.ValidationAttributes
{
    public class TCIDValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult(ErrorMessage = "You must write a valid ID!"); ; // Null değerlere izin ver
            }

            string tcKimlik = value.ToString();

            if (TcKimlikDogrulama(tcKimlik))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(ErrorMessage = "Invalid ID!");
            }
        }

        private bool TcKimlikDogrulama(string pTCKimlik)
        {
            if (pTCKimlik.Length != 11 || pTCKimlik[0] == '0')
                return false;

            int toplam1 = Convert.ToInt32(pTCKimlik[0].ToString()) + Convert.ToInt32(pTCKimlik[2].ToString()) +
                          Convert.ToInt32(pTCKimlik[4].ToString()) + Convert.ToInt32(pTCKimlik[6].ToString()) +
                          Convert.ToInt32(pTCKimlik[8].ToString());
            int toplam2 = Convert.ToInt32(pTCKimlik[1].ToString()) + Convert.ToInt32(pTCKimlik[3].ToString()) +
                          Convert.ToInt32(pTCKimlik[5].ToString()) + Convert.ToInt32(pTCKimlik[7].ToString());

            int sonuc = ((toplam1 * 7) - toplam2) % 10;

            if (sonuc.ToString() != pTCKimlik[9].ToString())
                return false;

            int toplam3 = 0;
            for (int i = 0; i < 10; i++)
                toplam3 += Convert.ToInt32(pTCKimlik[i].ToString());

            return (toplam3 % 10).ToString() == pTCKimlik[10].ToString();
        }
    }
}
