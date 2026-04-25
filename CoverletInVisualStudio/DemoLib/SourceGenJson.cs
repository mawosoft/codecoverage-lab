// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Text.Json;

namespace DemoLib;

public class SourceGenJson
{
    public DateTime AutoProp1 { get; set; }

    public object? Method1()
    {
#if DISABLE_SOURCEGEN_JSONSERIALIZER
        var typeinfo = (System.Text.Json.Serialization.Metadata.JsonTypeInfo<SourceGenJson>)JsonSerializerOptions.Default.GetTypeInfo(typeof(SourceGenJson));
#else
        var typeinfo = SourceGenJsonContext.Default.SourceGenJson;
#endif
        var json = JsonSerializer.Serialize(this, typeinfo);
        var result = JsonSerializer.Deserialize(json, typeinfo);
        return result;
    }
}

#if !DISABLE_SOURCEGEN_JSONSERIALIZER
[System.Text.Json.Serialization.JsonSerializable(typeof(SourceGenJson))]
internal sealed partial class SourceGenJsonContext : System.Text.Json.Serialization.JsonSerializerContext
{
}
#endif
