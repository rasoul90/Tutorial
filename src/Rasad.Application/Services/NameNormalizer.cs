namespace Rasad.Application.Services;

public static class NameNormalizer
{
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var normalized = input.Trim();
        normalized = normalized.Replace("ـ", string.Empty);
        normalized = normalized.Replace("أ", "ا").Replace("إ", "ا").Replace("آ", "ا");
        normalized = normalized.Replace("ى", "ي");
        normalized = normalized.Replace("ة", "ه");
        normalized = CollapseSpaces(normalized);
        return normalized;
    }

    private static string CollapseSpaces(string input)
    {
        var result = new List<char>(input.Length);
        var previousWasSpace = false;
        foreach (var ch in input)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!previousWasSpace)
                {
                    result.Add(' ');
                    previousWasSpace = true;
                }

                continue;
            }

            result.Add(ch);
            previousWasSpace = false;
        }

        return new string(result.ToArray());
    }
}
