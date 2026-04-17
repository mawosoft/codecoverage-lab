// Copyright (c) Matthias Wolf, Mawosoft.

using System.Text.Json;

namespace CoverageDemo;

public class Class09SourceGenDemoJsonSerializer
{
    public int AutoProp1 { get; set; }
    public int AutoProp2 { get; set; }

    public int Method1(int value)
    {
#if DISABLE_SOURCEGEN_JSONSERIALIZER
        var typeinfo = (System.Text.Json.Serialization.Metadata.JsonTypeInfo<Class09SourceGenDemoJsonSerializer>)JsonSerializerOptions.Default.GetTypeInfo(typeof(Class09SourceGenDemoJsonSerializer));
#else
        var typeinfo = SourceGenDemoContext.Default.Class09SourceGenDemoJsonSerializer;
#endif
        var json = JsonSerializer.Serialize(this, typeinfo);
        var demo2 = JsonSerializer.Deserialize(json, typeinfo);
        return demo2?.AutoProp2 ?? 0;
    }
}


#if !DISABLE_SOURCEGEN_JSONSERIALIZER
[System.Text.Json.Serialization.JsonSerializable(typeof(Class09SourceGenDemoJsonSerializer))]
internal sealed partial class SourceGenDemoContext : System.Text.Json.Serialization.JsonSerializerContext
{
}
#endif
