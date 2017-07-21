using System;
using System.Collections.Generic;
using MaterialForms.Wpf.Resources;
using MaterialForms.Wpf.Validation;

namespace MaterialForms.Wpf.Fields
{
    /// <summary>
    /// Base class for all input fields.
    /// </summary>
    public abstract class DataFormField : FormField
    {
        protected DataFormField(string key, Type propertyType)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            PropertyType = propertyType;
            Validators = new List<IValidatorProvider>();
            BindingOptions = new BindingOptions();
        }

        protected internal override void Freeze()
        {
            base.Freeze();
            if (CreateBinding)
            {
                if (IsDirectBinding)
                {
                    Resources.Add("Value", new DirectBinding(BindingOptions, Validators));
                }
                else
                {
                    Resources.Add("Value", new DataBinding(Key, BindingOptions, Validators));
                }
            }

            Resources.Add(nameof(IsReadOnly), IsReadOnly ?? LiteralValue.False);
            Resources.Add(nameof(DefaultValue), DefaultValue ?? new LiteralValue(null));
            Resources.Add(nameof(SelectOnFocus), SelectOnFocus ?? LiteralValue.True);
        }

        public Type PropertyType { get; }

        public IValueProvider IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the default value for this field.
        /// </summary>
        public IValueProvider DefaultValue { get; set; }

        public BindingOptions BindingOptions { get; }

        public List<IValidatorProvider> Validators { get; set; }

        public IValueProvider SelectOnFocus { get; set; }

        protected bool IsDirectBinding { get; set; }

        protected bool CreateBinding { get; set; } = true;

        public virtual object GetDefaultValue(IResourceContext context)
        {
            return DefaultValue?.GetValue(context).Value;
        }
    }
}
