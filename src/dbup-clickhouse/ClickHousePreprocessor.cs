using DbUp.Engine;

namespace DbUp.ClickHouse;

/// <summary>
/// Pass-through preprocessor for ClickHouse scripts.
/// </summary>
public class ClickHousePreprocessor : IScriptPreprocessor
{
    /// <summary>
    /// Processes the script contents. This implementation returns the contents unchanged.
    /// </summary>
    /// <param name="contents">The script contents to process.</param>
    /// <returns>The processed script contents.</returns>
    public string Process(string contents) => contents;
}

