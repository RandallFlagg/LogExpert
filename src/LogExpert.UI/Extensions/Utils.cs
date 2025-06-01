using System.Drawing;

namespace LogExpert.UI.Extensions
{
    internal static class Utils
    {
        public static string GetWordFromPos(int xPos, string text, Graphics g, Font font)
        {
            string[] words = text.Split([' ', '.', ':', ';']);

            int index = 0;

            List<CharacterRange> crList = [];

            for (int i = 0; i < words.Length; ++i)
            {
                crList.Add(new CharacterRange(index, words[i].Length));
                index += words[i].Length;
            }

            CharacterRange[] crArray = [.. crList];

            StringFormat stringFormat = new(StringFormat.GenericTypographic)
            {
                Trimming = StringTrimming.None,
                FormatFlags = StringFormatFlags.NoClip
            };

            stringFormat.SetMeasurableCharacterRanges(crArray);

            RectangleF rect = new(0, 0, 3000, 20);
            Region[] stringRegions = g.MeasureCharacterRanges(text, font, rect, stringFormat);

            bool found = false;

            int y = 0;

            foreach (Region regio in stringRegions)
            {
                if (regio.IsVisible(xPos, 3, g))
                {
                    found = true;
                    break;
                }

                y++;
            }

            if (found)
            {
                return words[y];
            }
            else
            {
                return null;
            }
        }
    }
}
