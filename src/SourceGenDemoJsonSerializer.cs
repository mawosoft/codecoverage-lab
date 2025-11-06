// Copyright (c) Matthias Wolf, Mawosoft.

using System.Text.Json;

namespace FooLib;

public class SourceGenDemoJsonSerializer
{
    public int AutoProp1 { get; set; }
    public int AutoProp2 { get; set; }

    public int Method1(int value)
    {
#if DISABLE_SOURCEGEN_JSONSERIALIZER
        var typeinfo = (System.Text.Json.Serialization.Metadata.JsonTypeInfo<SourceGenDemoJsonSerializer>)JsonSerializerOptions.Default.GetTypeInfo(typeof(SourceGenDemoJsonSerializer));
#else
        var typeinfo = SourceGenDemoContext.Default.SourceGenDemoJsonSerializer;
#endif
        var json = JsonSerializer.Serialize(this, typeinfo);
        var demo2 = JsonSerializer.Deserialize(json, typeinfo);
        return demo2?.AutoProp2 ?? 0;
    }
}


#if !DISABLE_SOURCEGEN_JSONSERIALIZER
[System.Text.Json.Serialization.JsonSerializable(typeof(SourceGenDemoJsonSerializer))]
internal sealed partial class SourceGenDemoContext : System.Text.Json.Serialization.JsonSerializerContext
{
}
#endif
