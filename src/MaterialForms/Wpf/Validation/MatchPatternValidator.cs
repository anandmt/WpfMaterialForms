﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using MaterialForms.Wpf.Resources;

namespace MaterialForms.Wpf.Validation
{
    public class MatchPatternValidator : FieldValidator
    {
        protected MatchPatternValidator(IStringProxy argument, IErrorStringProvider errorProvider, IBoolProxy isEnforced, IValueConverter valueConverter)
            : base(errorProvider, isEnforced, valueConverter)
        {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        public IStringProxy Argument { get; }

        protected override bool ValidateValue(object value, CultureInfo cultureInfo)
        {
            var pattern = Argument.Value;
            if (pattern == null)
            {
                return true;
            }

            if (value is string s)
            {
                return Regex.IsMatch(s, pattern);
            }

            return false;
        }
    }
}
