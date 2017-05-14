﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Windows.Data;
using MaterialForms.Wpf.Resources;

namespace MaterialForms.Wpf
{
    public class BoundExpression
    {
        public BoundExpression(string value)
        {
            StringFormat = value ?? throw new ArgumentNullException(nameof(value));
        }

        public BoundExpression(Resource resource) : this(resource, null)
        {
        }

        public BoundExpression(Resource resource, string stringFormat)
        {
            Resources = new List<Resource>(1) { resource ?? throw new ArgumentNullException(nameof(resource)) };
            StringFormat = stringFormat;
        }

        public BoundExpression(IEnumerable<Resource> resources, string stringFormat)
        {
            Resources = resources?.ToList() ?? throw new ArgumentNullException(nameof(resources));
            if (Resources.Count != 1)
            {
                StringFormat = stringFormat ?? throw new ArgumentNullException(nameof(stringFormat));
            }
            else
            {
                StringFormat = stringFormat;
            }
        }

        public string StringFormat { get; }

        public IReadOnlyList<Resource> Resources { get; }

        public void SetValue(FrameworkElement container, DependencyObject element, DependencyProperty property)
        {
            if (Resources == null || Resources.Count == 0)
            {
                element.SetValue(property, StringFormat);
                return;
            }

            if (Resources.Count == 1)
            {
                var resource = Resources[0];
                var binding = resource.GetBinding(container);
                if (StringFormat != null)
                {
                    binding.StringFormat = StringFormat;
                }

                BindingOperations.SetBinding(element, property, binding);
                return;
            }

            var multiBinding = new MultiBinding
            {
                StringFormat = StringFormat
            };

            foreach (var binding in Resources.Select(resource => resource.GetBinding(container)))
            {
                multiBinding.Bindings.Add(binding);
            }

            BindingOperations.SetBinding(element, property, multiBinding);
        }

        public BindingProxy GetValue(FrameworkElement container)
        {
            var proxy = new BindingProxy();
            SetValue(container, proxy, BindingProxy.ValueProperty);
            return proxy;
        }

        public static BoundExpression Parse(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var resources = new List<Resource>();
            var stringFormat = new StringBuilder();
            var resourceType = new StringBuilder();
            var resourceName = new StringBuilder();
            var resourceFormat = new StringBuilder();
            var length = expression.Length;
            var i = 0;
            char c;
            outside:
            if (i == length)
            {
                var format = stringFormat.ToString();
                return new BoundExpression(resources, format == "{0}" ? null : format);
            }

            c = expression[i];
            if (c == '{')
            {
                stringFormat.Append('{');
                if (++i == expression.Length)
                {
                    throw new FormatException("Invalid unescaped '{' at end of input.");
                }

                if (expression[i] == '{')
                {
                    i++;
                    stringFormat.Append('{');
                    goto outside;
                }

                goto readResource;
            }

            if (c == '}')
            {
                stringFormat.Append('}');
                if (++i == expression.Length)
                {
                    throw new FormatException("Invalid unescaped '}' at end of input.");
                }

                if (expression[i] == '}')
                {
                    i++;
                    stringFormat.Append('}');
                    goto outside;
                }

                throw new FormatException("Invalid unescaped '}'.");
            }

            stringFormat.Append(c);
            i++;
            goto outside;

            readResource:
            while (char.IsLetter(c = expression[i]))
            {
                resourceType.Append(c);
                if (++i == length)
                {
                    throw new FormatException("Unexpected end of input (1).");
                }
            }

            while (char.IsWhiteSpace(expression[i]))
            {
                if (++i == length)
                {
                    throw new FormatException("Unexpected end of input (2).");
                }
            }

            while ((c = expression[i]) != ',' && c != ':')
            {
                if (char.IsWhiteSpace(c))
                {
                    break;
                }

                if (c == '}')
                {
                    if (++i == length)
                    {
                        goto addResource;
                    }

                    if (expression[i] != '}')
                    {
                        goto addResource;
                    }

                    resourceName.Append('}');
                }

                resourceName.Append(c);
                if (++i == length)
                {
                    throw new FormatException("Unexpected end of input (3).");
                }
            }

            while (char.IsWhiteSpace(expression[i]))
            {
                if (++i == length)
                {
                    throw new FormatException("Unexpected end of input (4).");
                }
            }

            c = expression[i];
            if (c == '}')
            {
                if (++i == length)
                {
                    goto addResource;
                }

                if (expression[i] != '}')
                {
                    goto addResource;
                }

                throw new FormatException("Invalid '}}' sequence.");
            }

            if (c == ',' || c == ':')
            {
                resourceFormat.Append(c);
                while (true)
                {
                    if (++i == length)
                    {
                        throw new FormatException("Unexpected end of input (5).");
                    }

                    c = expression[i];
                    if (c == '}')
                    {
                        if (++i == length)
                        {
                            goto addResource;
                        }

                        if (expression[i] != '}')
                        {
                            goto addResource;
                        }

                        resourceFormat.Append("}}");
                    }
                    else
                    {
                        resourceFormat.Append(c);
                    }
                }
            }

            addResource:
            var key = resourceName.ToString();
            Resource resource;
            switch (resourceType.ToString())
            {
                case "Binding":
                    resource = new PropertyBinding(key, false);
                    break;
                case "Property":
                    resource = new PropertyBinding(key, true);
                    break;
                case "StaticResource":
                    resource = new StaticResource(key);
                    break;
                case "DynamicResource":
                    resource = new DynamicResource(key);
                    break;
                case "ContextBinding":
                    resource = new ContextPropertyBinding(key, false);
                    break;
                case "ContextProperty":
                    resource = new ContextPropertyBinding(key, true);
                    break;
                default:
                    throw new FormatException("Invalid resource type.");
            }

            var index = resources.IndexOf(resource);
            if (index == -1)
            {
                index = resources.Count;
                resources.Add(resource);
            }

            stringFormat.Append(index);
            if (resourceFormat.Length != 0)
            {
                stringFormat.Append(resourceFormat);
            }

            stringFormat.Append('}');

            resourceType.Clear();
            resourceName.Clear();
            resourceFormat.Clear();
            goto outside;
        }

        public static implicit operator BoundExpression(string expression)
        {
            return Parse(expression);
        }
    }
}