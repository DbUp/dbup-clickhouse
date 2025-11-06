using DbUp.Engine;

namespace DbUp.ClickHouse;

/// <summary>
/// Pass-through preprocessor for ClickHouse scripts.
/// </summary>
public class ClickHousePreprocessor : IScriptPreprocessor
{
    public string Process(string contents) => contents;
}

