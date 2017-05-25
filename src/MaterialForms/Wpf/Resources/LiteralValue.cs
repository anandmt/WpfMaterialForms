using System.Windows;
using System.Windows.Data;

namespace MaterialForms.Wpf.Resources
{
    public sealed class LiteralValue : Resource
    {
        public LiteralValue(object value)
            : this(value, null)
        {
        }

        public LiteralValue(object value, string valueConverter)
            : base(valueConverter)
        {
            Value = value;
        }

        public object Value { get; }

        public override bool IsDynamic => false;

        public override BindingBase GetBinding(FrameworkElement element)
        {
            return new Binding
            {
                Source = Value,
                Converter = GetValueConverter(element),
                Mode = BindingMode.OneTime
            };
        }

        public override Resource Rewrap(string valueConverter)
        {
            return new LiteralValue(Value, valueConverter);
        }

        public override bool Equals(Resource other)
        {
            if (other is LiteralValue resource)
            {
                return Equals(Value, resource.Value)
                    && ValueConverter == other.ValueConverter;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }
}
