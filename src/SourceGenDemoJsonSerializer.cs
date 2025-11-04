// Copyright (c) Matthias Wolf, Mawosoft.

using System.Text.Json.Serialization;

namespace FooLib;

public class SourceGenDemoJsonSerializer
{
    public int AutoProp1 { get; set; }
    public int AutoProp2 { get; set; }
}

[JsonSerializable(typeof(SourceGenDemoJsonSerializer))]
public partial class SourceGenDemoContext : JsonSerializerContext
{
}
