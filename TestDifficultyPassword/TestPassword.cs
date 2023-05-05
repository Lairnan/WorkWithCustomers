using System;
using INCOMSYSTEM.BehaviorsFiles;
using NUnit.Framework;

namespace TestDifficultyPassword
{
    [TestFixture]
    public class TestPassword
    {
        [Test]
        public void Test1()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("Z#d$NmJwRDK"), DifficultyPassword.Hard);
        }
        [Test]
        public void Test2()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("k5OjB9DQO6"), DifficultyPassword.Hard);
        }
        [Test]
        public void Test3()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("X1JMF1t"), DifficultyPassword.Easy);
        }
        [Test]
        public void Test4()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("123321"), DifficultyPassword.Easy);
        }
        [Test]
        public void Test5()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("k5OjB9DQO6"), DifficultyPassword.Medium);
        }
        [Test]
        public void Test6()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("Asm0GH37uKd"), DifficultyPassword.Medium);
        }
    }
}