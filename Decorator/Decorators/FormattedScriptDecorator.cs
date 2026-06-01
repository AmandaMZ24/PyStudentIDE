using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace DecoratorApp.Decorators;

public class FormattedScriptDecorator : ScriptDecorator
{
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "def", "class", "import", "from", "as", "for", "while", "if", "elif", "else", "try", "except",
        "with", "return", "pass", "break", "continue", "lambda", "True", "False", "None", "in", "is", "and", "or", "not"
    };

    public FormattedScriptDecorator(IScript inner) : base(inner) { }

    public IReadOnlyList<HighlightedSegment> GetHighlightedSegments()
    {
        var segments = new List<HighlightedSegment>();
        using var reader = new StringReader(GetText());
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            AppendLineSegments(line, segments);
            segments.Add(new HighlightedSegment("\n", Brushes.WhiteSmoke));
        }
        return segments;
    }

    private static void AppendLineSegments(string line, List<HighlightedSegment> segments)
    {
        var (codePart, commentPart) = SplitComment(line);
        AppendCodeSegments(codePart, segments);
        if (!string.IsNullOrEmpty(commentPart))
        {
            segments.Add(new HighlightedSegment(commentPart, Brushes.ForestGreen));
        }
    }

    private static (string code, string comment) SplitComment(string line)
    {
        var inString = false;
        var stringChar = '\0';
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (inString)
            {
                if (ch == stringChar && (i == 0 || line[i - 1] != '\\'))
                    inString = false;
                continue;
            }

            if (ch == '\'' || ch == '"')
            {
                inString = true;
                stringChar = ch;
                continue;
            }

            if (ch == '#')
                return (line.Substring(0, i), line.Substring(i));
        }

        return (line, string.Empty);
    }

    private static void AppendCodeSegments(string code, List<HighlightedSegment> segments)
    {
        var i = 0;
        while (i < code.Length)
        {
            var ch = code[i];
            if (ch == '\'' || ch == '"')
            {
                var end = FindStringEnd(code, i);
                var text = code.Substring(i, end - i + 1);
                segments.Add(new HighlightedSegment(text, Brushes.Orange));
                i = end + 1;
                continue;
            }

            if (char.IsLetter(ch) || ch == '_')
            {
                var start = i;
                while (i < code.Length && (char.IsLetterOrDigit(code[i]) || code[i] == '_'))
                    i++;
                var token = code.Substring(start, i - start);
                var color = Keywords.Contains(token) ? Brushes.DeepSkyBlue : Brushes.WhiteSmoke;
                segments.Add(new HighlightedSegment(token, color));
                continue;
            }

            if (char.IsDigit(ch))
            {
                var start = i;
                while (i < code.Length && (char.IsDigit(code[i]) || code[i] == '.'))
                    i++;
                var token = code.Substring(start, i - start);
                segments.Add(new HighlightedSegment(token, Brushes.Goldenrod));
                continue;
            }

            segments.Add(new HighlightedSegment(ch.ToString(), Brushes.WhiteSmoke));
            i++;
        }
    }

    private static int FindStringEnd(string text, int start)
    {
        var quote = text[start];
        for (var i = start + 1; i < text.Length; i++)
        {
            if (text[i] == quote && text[i - 1] != '\\')
                return i;
        }
        return text.Length - 1;
    }
}
