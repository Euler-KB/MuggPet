using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MuggPet.Utils
{
    [Flags]
    public enum StringFormatOptions
    {
        None,

        /// <summary>
        /// Seperate change in cases with a white space character
        /// </summary>
        CaseSeperation = 0x100,

        /// <summary>
        /// Capitalizes only the very first character
        /// </summary>
        CapsFirstLetter = 0x200,

        /// <summary>
        /// Capitalizes each first character after white space
        /// </summary>
        CapsAllFirstLetter = 0x400,

        /// <summary>
        /// Capitalizes all characters. Not advisable to combine with AllLower flag
        /// </summary>
        AllUppper = 0x800,

        /// <summary>
        /// Lowers all characters. Not advisable to combine with AllUpper flag
        /// </summary>
        AllLower = 0x010
    }

    public static class StringUtil
    {
        static int GetCasingMode(string str)
        {
            if (str.Length >= 2)
            {
                // Pascal casing
                if (char.IsUpper(str[0]) && char.IsLower(str[1]))
                    return 1;

                //  Camel casing
                if (char.IsLower(str[0]) && char.IsLower(str[1]))
                    return 2;
            }

            return -1;
        }

        static int IndexOf(char[] array, char c, int startIndex)
        {
            for (; startIndex < array.Length; startIndex++)
            {
                if (array[startIndex].Equals(c))
                    return startIndex;
            }

            return -1;
        }

        public static string FormatString(string str, StringFormatOptions option)
        {
            if (option == 0)
                return str;

            var cArray = str.ToArray();
            if (option.HasFlag(StringFormatOptions.CaseSeperation))
            {
                int casingMode = GetCasingMode(str);

                //  Pascal
                if (casingMode == 1)
                {
                    List<char> data = new List<char>() { str[0], str[1] };
                    bool isUpper = char.IsUpper(cArray[1]);
                    for (int i = 2; i < str.Length; i++)
                    {
                        if (char.IsUpper(cArray[i]) != isUpper)
                        {
                            data.Add(' ');

                            if (i + 1 < cArray.Length)
                                isUpper = char.IsUpper(cArray[i + 1]);
                        }

                        data.Add(cArray[i]);
                    }

                    cArray = data.ToArray();
                }

                //  Camel
                else if (casingMode == 2)
                {
                    List<char> data = new List<char>() { str[0] };
                    bool isUpper = char.IsUpper(cArray[0]);
                    for (int i = 1; i < str.Length; i++)
                    {
                        if (char.IsUpper(cArray[i]) != isUpper)
                        {
                            data.Add(' ');

                            if (i + 1 < cArray.Length)
                                isUpper = char.IsUpper(cArray[i + 1]);
                        }

                        data.Add(cArray[i]);
                    }

                    cArray = data.ToArray();
                }
            }

            if (option.HasFlag(StringFormatOptions.AllLower))
            {
                for (int i = 0; i < cArray.Length; i++)
                    cArray[i] = char.ToLower(cArray[i]);
            }

            if (option.HasFlag(StringFormatOptions.AllUppper))
            {
                for (int i = 0; i < cArray.Length; i++)
                    cArray[i] = char.ToUpper(cArray[i]);
            }

            if (option.HasFlag(StringFormatOptions.CapsFirstLetter))
                cArray[0] = char.ToUpper(cArray[0]);

            if (option.HasFlag(StringFormatOptions.CapsAllFirstLetter))
            {
                cArray[0] = char.ToUpper(cArray[0]);
                int lIndex = IndexOf(cArray, ' ', 1);
                while (lIndex != -1)
                {
                    if (lIndex + 1 < cArray.Length)
                        cArray[lIndex + 1] = char.ToUpper(cArray[lIndex + 1]);

                    lIndex = IndexOf(cArray, ' ', lIndex + 1);
                }
            }

            return new string(cArray);
        }

    }
}