#nullable enable
using System.Collections.Generic;

namespace DbUp.ClickHouse;

/// <summary>
/// Provides SQL query parsing functionality for ClickHouse scripts, capable of splitting multi-statement SQL
/// into individual statements while properly handling comments, string literals, and quoted identifiers.
/// </summary>
internal static class ClickHouseQueryParser
{
    /// <summary>
    /// Represents the current parsing state when processing SQL text.
    /// </summary>
    private enum ParseState
    {
        /// <summary>Normal SQL parsing state.</summary>
        Normal,

        /// <summary>Inside a single-quoted string literal.</summary>
        SingleQuote,

        /// <summary>Inside a back-tick quoted identifier.</summary>
        BackTickQuote,

        /// <summary>Inside a line comment (-- style).</summary>
        LineComment,

        /// <summary>Inside a block comment (/* */ style).</summary>
        BlockComment,
    }

    /// <summary>
    /// Maintains parsing context and position information during SQL processing.
    /// </summary>
    private record ParseContext
    {
        /// <summary>Current character position in the SQL string.</summary>
        public int Position { get; set; }

        /// <summary>The starting position of the current SQL statement being parsed.</summary>
        public int StatementStart { get; set; }

        /// <summary>Current nesting level of parentheses to avoid splitting on semicolons within function calls or subqueries.</summary>
        public int ParenthesisLevel { get; set; }

        /// <summary>Current nesting level of block comments to handle nested /* */ comments correctly.</summary>
        public int BlockCommentLevel { get; set; }

        /// <summary>Current parsing state indicating the type of content being processed.</summary>
        public ParseState State { get; set; } = ParseState.Normal;
    }

    /// <summary>
    /// Parses raw SQL text and splits it into individual executable statements.
    /// </summary>
    /// <param name="sql">The SQL text to parse, which may contain multiple statements separated by semicolons.</param>
    /// <returns>
    /// A read-only collection of individual SQL statements. Empty or whitespace-only statements are excluded.
    /// Returns an empty collection if the input is null or empty.
    /// </returns>
    /// <remarks>
    /// This parser correctly handles:
    /// - Semicolons within string literals ('text; with semicolon')
    /// - Semicolons within quoted identifiers (`identifier; with semicolon`)
    /// - Semicolons within line comments (-- comment; with semicolon)
    /// - Semicolons within block comments (/* comment; with semicolon */)
    /// - Nested block comments (/* outer /* inner */ outer */)
    /// - Semicolons within parentheses for function calls and subqueries
    /// </remarks>
    public static IReadOnlyCollection<string> ParseRawQuery(string sql)
    {
        if (string.IsNullOrEmpty(sql))
            return new List<string>();

        var statements = new List<string>();
        var context = new ParseContext();

        while (context.Position < sql.Length)
        {
            if (TryParseStatement(sql, context, out var statement))
            {
                statements.Add(statement);
            }
        }

        // Add a final statement if any content remains
        if (context.StatementStart < sql.Length)
        {
            var finalStatement = sql.Substring(context.StatementStart);
            statements.Add(finalStatement);
        }

        return statements;
    }

    private static bool TryParseStatement(string sql, ParseContext context, out string statement)
    {
        statement = string.Empty;

        while (context.Position < sql.Length)
        {
            var ch = sql[context.Position];
            context.Position++;

            if (context.State == ParseState.Normal)
            {
                if (HandleNormalState(sql, context, ch, out statement))
                    return true;
            }
            else
            {
                HandleSpecialStates(sql, context, ch);
            }
        }

        return false;
    }

    private static bool HandleNormalState(
        string sql,
        ParseContext context,
        char ch,
        out string statement
    )
    {
        statement = string.Empty;

        switch (ch)
        {
            case '\'':
                context.State = ParseState.SingleQuote;
                break;
            case '`':
                context.State = ParseState.BackTickQuote;
                break;
            case '/':
                if (TryStartBlockComment(sql, context))
                    context.State = ParseState.BlockComment;
                break;
            case '-':
                if (TryStartLineComment(sql, context))
                    context.State = ParseState.LineComment;
                break;
            case '(':
                context.ParenthesisLevel++;
                break;
            case ')':
                context.ParenthesisLevel--;
                break;
            case ';':
                if (context.ParenthesisLevel == 0)
                {
                    statement = ExtractStatement(sql, context);
                    SkipWhitespaceAndStartNext(sql, context);
                    return !string.IsNullOrEmpty(statement);
                }
                break;
        }

        return false;
    }

    private static void HandleSpecialStates(string sql, ParseContext context, char ch)
    {
        switch (context.State)
        {
            case ParseState.SingleQuote:
                if (ch == '\'')
                    context.State = ParseState.Normal;
                break;
            case ParseState.BackTickQuote:
                if (ch == '`')
                    context.State = ParseState.Normal;
                break;
            case ParseState.LineComment:
                if (ch is '\r' or '\n')
                    context.State = ParseState.Normal;
                break;
            case ParseState.BlockComment:
                HandleBlockComment(sql, context, ch);
                break;
        }
    }

    private static bool TryStartBlockComment(string sql, ParseContext context)
    {
        if (context.Position < sql.Length && sql[context.Position] == '*')
        {
            context.Position++;
            context.BlockCommentLevel = 1;
            return true;
        }
        return false;
    }

    private static bool TryStartLineComment(string sql, ParseContext context)
    {
        if (context.Position < sql.Length && sql[context.Position] == '-')
        {
            context.Position++;
            return true;
        }
        return false;
    }

    private static void HandleBlockComment(string sql, ParseContext context, char ch)
    {
        switch (ch)
        {
            case '/' when context.Position < sql.Length && sql[context.Position] == '*':
                context.Position++;
                context.BlockCommentLevel++;
                break;
            case '*' when context.Position < sql.Length && sql[context.Position] == '/':
            {
                context.Position++;
                context.BlockCommentLevel--;
                if (context.BlockCommentLevel == 0)
                    context.State = ParseState.Normal;
                break;
            }
        }
    }

    private static string ExtractStatement(string sql, ParseContext context)
    {
        var statementLength = context.Position - context.StatementStart - 1;
        return statementLength <= 0
            ? string.Empty
            : sql.Substring(context.StatementStart, statementLength);
    }

    private static void SkipWhitespaceAndStartNext(string sql, ParseContext context)
    {
        while (context.Position < sql.Length && char.IsWhiteSpace(sql[context.Position]))
        {
            context.Position++;
        }
        context.StatementStart = context.Position;
    }
}
