using System;
using System.Collections.Generic;

namespace LogExpert;

public class Column : IColumn
{
    #region Fields

    private const int MAXLENGTH = 4678 - 3;
    private const string REPLACEMENT = "...";

    private static readonly IEnumerable<Func<string, string>> _replacements;

    private string _fullValue;

    #endregion

    #region cTor

    static Column ()
    {
        var replacements = new List<Func<string, string>>(
            [
                //replace tab with 3 spaces, from old coding. Needed???
                input => input.Replace("\t", "  ", StringComparison.Ordinal),

                //shorten string if it exceeds maxLength
                input => input.Length > MAXLENGTH
                        ? string.Concat(input.AsSpan(0, MAXLENGTH), REPLACEMENT)
                        : input
            ]);

        if (Environment.Version >= Version.Parse("6.2"))
        {
            //Win8 or newer support full UTF8 chars with the preinstalled fonts.
            //Replace null char with UTF8 Symbol U+2400 (␀)
            replacements.Add(input => input.Replace("\0", "␀", StringComparison.Ordinal));
        }
        else
        {
            //Everything below Win8 the installed fonts seems to not to support reliabel
            //Replace null char with space
            replacements.Add(input => input.Replace("\0", " ", StringComparison.Ordinal));
        }

        _replacements = replacements;

        EmptyColumn = new Column { FullValue = string.Empty };
    }

    #endregion

    #region Properties

    public static IColumn EmptyColumn { get; }

    public IColumnizedLogLine Parent { get; set; }

    public string FullValue
    {
        get => _fullValue;
        set
        {
            _fullValue = value;

            var temp = FullValue;

            foreach (var replacement in _replacements)
            {
                temp = replacement(temp);
            }

            DisplayValue = temp;
        }
    }

    public string DisplayValue { get; private set; }

    string ITextValue.Text => DisplayValue;

    #endregion

    #region Public methods

    public static Column[] CreateColumns (int count, IColumnizedLogLine parent)
    {
        return CreateColumns(count, parent, string.Empty);
    }

    public static Column[] CreateColumns (int count, IColumnizedLogLine parent, string defaultValue)
    {
        var output = new Column[count];

        for (var i = 0; i < count; i++)
        {
            output[i] = new Column { FullValue = defaultValue, Parent = parent };
        }

        return output;
    }

    public override string ToString ()
    {
        return DisplayValue ?? string.Empty;
    }

    #endregion
}