using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Word = Microsoft.Office.Interop.Word;

namespace INCOMSYSTEM.BehaviorsFiles
{
    public static class Extension
    {
        public static bool ReplaceWordText(this Word.Document document, string findText, object replaceText, bool replaceAll = true)
        {
            var range = document.Content;
            range.Find.ClearFormatting();
            return range.Find.Execute(FindText: findText, ReplaceWith: replaceText, Forward: true,
                Replace: replaceAll ? Word.WdReplace.wdReplaceAll : Word.WdReplace.wdReplaceOne);
        }

        public static string ConvertDay(this int value)
        {
            var valStr = value.ToString();
            string str;
            var newVal = value;
            if (valStr.Length > 1) newVal = int.Parse(valStr[valStr.Length - 1].ToString());

            if (value == 14) str = $"{value} дней";
            else if (newVal == 1) str = $"{value} день";
            else if (newVal > 0 && newVal < 5) str = $"{value} дня";
            else str = $"{value} дней";
            
            return str;
        }
        
        public static string ConvertFileSizeToString(this long fileSizeInBytes)
        {
            if (fileSizeInBytes < 0) return "Invalid size";

            string[] sizes = { "Байт", "КБ", "МБ", "ГБ", "ТБ", "ПБ" };
            var order = 0;

            while (fileSizeInBytes >= 1024 && order < sizes.Length - 1)
            {
                fileSizeInBytes /= 1024;
                order++;
            }

            return $"{(fileSizeInBytes == 0 ? "0" : fileSizeInBytes.ToString("#.#"))} {sizes[order]}";
        }
        
        public static IEnumerable<string> SplitStringToObservableCollection(this string text)
        {
            var items = new ObservableCollection<string>();
            if (string.IsNullOrEmpty(text)) return items;
            var arr = text.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in arr) items.Add(item.Trim());
            
            return items;
        }
    }
}