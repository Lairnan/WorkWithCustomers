namespace INCOMSYSTEM.BehaviorsFiles
{
    public static class PasswordDifficulty
    {
        public static DifficultyPassword CheckDifficultyPassword(string password)
        {
            return DifficultyPassword.Hard;
        }
    }
    
    public enum DifficultyPassword
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }
}