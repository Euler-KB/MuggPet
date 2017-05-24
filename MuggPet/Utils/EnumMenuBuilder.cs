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
    public static class EnumMenuBuilder
    {
        /// <summary>
        /// Populates the menu with values of the enumerated data type
        /// </summary>
        /// <param name="enumType">The enum type</param>
        /// <param name="menu">The menu to be populated</param>
        /// <param name="showAsAction">The show as action for each menu item</param>
        public static bool Populate(Type enumType, IMenu menu, StringFormatOptions options = StringFormatOptions.CaseSeperation, ShowAsAction showAsAction = ShowAsAction.Always | ShowAsAction.WithText)
        {
            if (!enumType.IsEnum)
                return false;

            foreach (var item in Enum.GetValues(enumType))
            {
                menu.Add(0, Convert.ToInt32(item), 0, StringUtil.FormatString(item.ToString(), options)).SetShowAsAction(showAsAction);
            }

            return true;
        }


    }
}