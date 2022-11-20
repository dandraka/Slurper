using System;

namespace Dandraka.Slurper;

public class ValueConversionException : Exception
{
    public ValueConversionException(Type t, string value) : base($"Cannot convert {value} to type {t.FullName}") { }
}
