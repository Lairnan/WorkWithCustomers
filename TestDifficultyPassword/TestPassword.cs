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
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("Ht9Ym$F5&pL@#dKqW7"), DifficultyPassword.Hard);
        }
        [Test]
        public void Test2()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("37V#sGf4*5QmaR8NlJ"), DifficultyPassword.Hard);
        }
        [Test]
        public void Test3()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("pL!jkf#3@Uqwe$rt"), DifficultyPassword.Hard);
        }
        [Test]
        public void Test4()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("Kq2@pWcD1n"), DifficultyPassword.Medium);
        }
        [Test]
        public void Test5()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("5fR$loBoP9"), DifficultyPassword.Medium);
        }
        [Test]
        public void Test6()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("Bvw87*ekdGp"), DifficultyPassword.Medium);
        }
        [Test]
        public void Test7()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("sunshine"), DifficultyPassword.Easy);
        }
        [Test]
        public void Test8()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("123456789"), DifficultyPassword.Easy);
        }
        [Test]
        public void Test9()
        {
            Assert.AreEqual(PasswordDifficulty.CheckDifficultyPassword("Password123"), DifficultyPassword.Easy);
        }
    }
}