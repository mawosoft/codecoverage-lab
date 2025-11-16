// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo;

public class Class12CoverageDemoProperties
{
    private int _fullProp;

    public int AutoProp01SingleLineFullCoverage { get; set; }
    public int AutoProp02SingleLineGetCoverage { get; set; }
    public int AutoProp03SingleLineSetCoverage { get; set; }

    public int AutoProp04MultiLineFullCoverage
    {
        get;
        set;
    }

    public int AutoProp05MultiLineGetCoverage
    {
        get;
        set;
    }

    public int AutoProp06MultiLineSetCoverage
    {
        get;
        set;
    }

    public int AutoProp07InitSingleLineFullCoverage { get; init; }
    public int AutoProp08InitSingleLineInitCoverage { get; init; }

    public int AutoProp09InitMultiLineFullCoverage
    {
        get;
        init;
    }

    public int AutoProp10InitMultiLineInitCoverage
    {
        get;
        init;
    }

    public int FullProp1SingleLineFullCoverage { get => _fullProp; set => _fullProp = value; }
    public int FullProp2SingleLineGetCoverage { get => _fullProp; set => _fullProp = value; }
    public int FullProp3SingleLineSetCoverage { get => _fullProp; set => _fullProp = value; }

    public int FullProp4MultiLineFullCoverage
    {
        get => _fullProp;
        set => _fullProp = value;
    }

    public int FullProp5MultiLineGetCoverage
    {
        get => _fullProp;
        set => _fullProp = value;
    }

    public int FullProp6MultiLineSetCoverage
    {
        get => _fullProp;
        set => _fullProp = value;
    }
}
