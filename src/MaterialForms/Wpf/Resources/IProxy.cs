﻿using System.Windows;

namespace MaterialForms.Wpf.Resources
{
    public interface IProxy
    {
        object Value { get; }
    }

    public interface IStringProxy
    {
        string Value { get; }
    }

    internal class PlainObject : IProxy
    {
        public PlainObject(object value)
        {
            Value = value;
        }

        public object Value { get; }
    }

    internal class PlainString : IStringProxy
    {
        public PlainString(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
