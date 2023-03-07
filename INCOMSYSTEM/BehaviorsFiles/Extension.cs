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
    }
}