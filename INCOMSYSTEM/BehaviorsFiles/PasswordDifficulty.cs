using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace INCOMSYSTEM.BehaviorsFiles
{
    public static class PasswordDifficulty
    {
        public static DifficultyPassword CheckDifficultyPassword(string password)
        {
            if (IsEasyPassword(password)) return DifficultyPassword.Easy;

            if (password.ToLower().Trim().Contains("password")
                || password.ToLower().Trim().Contains("пароль")
                || password.ToLower().Trim().Contains("йцукен")
                || password.ToLower().Trim().Contains("qwerty"))
                return DifficultyPassword.Easy;

            if (IsRepeatContains(password)) return DifficultyPassword.Easy;
            
            var hasUpper = Regex.IsMatch(password, "[A-ZА-Я]");
            var hasLower = Regex.IsMatch(password, "[a-zа-я]");
            var hasDigit = Regex.IsMatch(password, @"[\d]");
            var hasSpecial = Regex.IsMatch(password, @"!|@|#|\$|%|\^|&|\*|\(|\)");

            var kDiff = 0;

            if (!hasDigit || !hasUpper || !hasLower) return DifficultyPassword.Easy;
            
            kDiff = !hasSpecial ? 1 : 2;

            if (password.Length < 12 && kDiff == 1) return DifficultyPassword.Medium;
            if (password.Length < 12 && kDiff == 2) return DifficultyPassword.Medium;

            return DifficultyPassword.Hard;
        }

        private static bool IsRepeatContains(string password)
        {
            var array = new List<char>();
            var count = 0;
            
            foreach (var ch in password)
            {
                if (!array.Contains(ch)) array.Add(ch);
                else count++;
            }

            return count > 1;
        }

        private static bool IsEasyPassword(string password)
        {
            if (password.Length < 6) return true;

            var startPass = password.Substring(0, password.Length / 2);
            var endPass = password.Substring(password.Length / 2, password.Length / 2);

            if (startPass.ToLower().Trim() == endPass.ToLower().Trim()) return true;
            return false;
        }
    }
    
    public enum DifficultyPassword
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }
}