// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo.Naming;

public class SpecialMemberNames
{
    private static readonly SpecialMemberNames s_default = new();

    public static SpecialMemberNames Default => s_default;

    public MyString Data1 { get; set; }

    public MyString Data2
    {
        get;
        set;
    }

    public MyString this[MyInt index] { get => ""; set => _ = value; }

    public MyString this[MyString index]
    {
        get => "";
        set => _ = value;
    }

    public SpecialMemberNames()
    {
        Data1 = "";
    }

    public SpecialMemberNames(MyString value)
    {
        Data1 = value;
    }

    ~SpecialMemberNames()
    {
        Data1 = "";
    }

    public static implicit operator SpecialMemberNames(MyString value) => new(value);

    public static explicit operator MyString(SpecialMemberNames value) => value.Data1;

    public static explicit operator int(SpecialMemberNames value) => value.Data1.Value.Length;
    public static SpecialMemberNames operator +(SpecialMemberNames left, SpecialMemberNames right)
        => (MyString)string.Concat(left.Data1, right.Data1);
    public static SpecialMemberNames operator ++(SpecialMemberNames value)
    {
        value.Data1 = string.Concat(value.Data1, "");
        return value;
    }
}
