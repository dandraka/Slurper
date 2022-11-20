using System.Collections.Generic;
using System.Dynamic;

namespace Dandraka.Slurper;

public delegate string ToStringFunc();

public sealed class ToStringExpandoObject : DynamicObject
{
    public IDictionary<string, object> Members { get; private set; }

    public ToStringExpandoObject()
    {
        this.Members = new Dictionary<string, object>();
    }

    public override bool TryDeleteMember(DeleteMemberBinder binder)
    {
        return this.Members.Remove(binder.Name);
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        return this.Members.TryGetValue(binder.Name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        this.Members[binder.Name] = value;
        return true;
    }

    public static implicit operator string(ToStringExpandoObject e) => e.ToString();

    public static implicit operator bool? (ToStringExpandoObject e)
    {
        if (bool.TryParse(e.ToString(), out bool b))
        {
            return b;
        }
        return null;
    }

    public static implicit operator int? (ToStringExpandoObject e)
    {
        if (int.TryParse(e.ToString(), out int b))
        {
            return b;
        }
        return null;
    }

    public static implicit operator decimal? (ToStringExpandoObject e)
    {
        if (decimal.TryParse(e.ToString(), out decimal b))
        {
            return b;
        }
        return null;
    }

    public static implicit operator double? (ToStringExpandoObject e)
    {
        if (double.TryParse(e.ToString(), out double b))
        {
            return b;
        }
        return null;
    }

    public static implicit operator bool (ToStringExpandoObject e)
    {
        var b = (bool?)e;
        if (b.HasValue)
        {
            return b.Value;
        }
        throw new ValueConversionException(typeof(bool), e);
    }

    public static implicit operator int(ToStringExpandoObject e)
    {
        var b = (int?)e;
        if (b.HasValue)
        {
            return b.Value;
        }
        throw new ValueConversionException(typeof(int), e);
    }

    public static implicit operator decimal(ToStringExpandoObject e)
    {
        var b = (decimal?)e;
        if (b.HasValue)
        {
            return b.Value;
        }
        throw new ValueConversionException(typeof(decimal), e);
    }

    public static implicit operator double(ToStringExpandoObject e)
    {
        var b = (double?)e;
        if (b.HasValue)
        {
            return b.Value;
        }
        throw new ValueConversionException(typeof(double), e);
    }

    public override string ToString()
    {
        //see if we defined a ToString member
        //if not, use the base implementation
        object methodObj;
        this.Members.TryGetValue("ToString", out methodObj);
        ToStringFunc method = methodObj as ToStringFunc;
        if (method == null)
            return base.ToString();

        return method();
    }
}