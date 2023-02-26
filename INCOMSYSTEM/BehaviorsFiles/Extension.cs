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
    }
}