using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Globalization;
using System.Collections.ObjectModel;

namespace Swisscard.MS.Helpers
{
    public abstract class BaseValidationRule : ValidationRule
    {
        protected BaseValidationRule()
        {
            IsActive = true;
        }
        public bool IsActive { get; set; }

        protected abstract ValidationResult DoValidate(object value, CultureInfo cultureInfo);

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!IsActive)
            {
                return ValidationResult.ValidResult;
            }
            return DoValidate(value, cultureInfo);
        }
    }

    class NotEmptyFieldValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {

                if (string.IsNullOrEmpty((string)value))
                {
                    return new ValidationResult(false, "");
                    //Properties.Resources.ResourceManager.GetString("StringMustBeNonEmptyMessage"));
                }
                else
                {
                    return new ValidationResult(true, null);
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, e.Message);
            }
        }
    }
    
    public class NotEmptyValidationRule : BaseValidationRule
    {
        protected override ValidationResult DoValidate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value == null)
                {
                    return new ValidationResult(false, "");
                    //Properties.Resources.ResourceManager.GetString("ValueMustBeNonNullMessage"));
                }
                else
                {
                    return new ValidationResult(true, null);
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, e.Message);
            }
        }
    }

}
