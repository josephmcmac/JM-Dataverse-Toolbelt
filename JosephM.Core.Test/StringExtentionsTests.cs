#region

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Extentions;

#endregion

namespace JosephM.Core.Test
{
    [TestClass]
    public class StringExtentionsTests
    {
        [TestMethod]
        public void StringExtentionsReplaceIgnoreCaseTest()
        {
            Assert.AreEqual(null, ((string)null).ReplaceIgnoreCase("a", "b"));
            Assert.AreEqual("abcdefghijkl", "abcdefghijkl".ReplaceIgnoreCase("zz", "yy"));
            Assert.AreEqual("bbbbbbbbbbbbbb", "aaaaaaaaaaaaaa".ReplaceIgnoreCase("a", "b"));
            Assert.AreEqual("wtf wtf wtf", "abcd wtf abcd".ReplaceIgnoreCase("abcd", "wtf"));
            Assert.AreEqual("wtf wtf wtf", "abcd wtf ABCD".ReplaceIgnoreCase("abcd", "wtf"));
            Assert.AreEqual("wtf wtf wtf", "wtf aBcD wtf".ReplaceIgnoreCase("abcd", "wtf"));
        }

        [TestMethod]
        public void StringExtentionsParseEnumTest()
        {
            Assert.AreEqual(TestEnum.Enum1, TestEnum.Enum1.ToString().ParseEnum<TestEnum>());
        }

        [TestMethod]
        public void StringExtentionsSplitCamelCaseTest()
        {
            Assert.IsTrue(((string) null).SplitCamelCase() == null);
            Assert.IsTrue("Foo".SplitCamelCase() == "Foo");
            Assert.IsTrue("FooBar".SplitCamelCase() == "Foo Bar");
            Assert.IsTrue("Foo Bar".SplitCamelCase() == "Foo Bar");
            Assert.IsTrue("FooBarBlah".SplitCamelCase() == "Foo Bar Blah");
        }

        [TestMethod]
        public void StringExtentionsLeftTest()
        {
            Assert.IsNull(((string) null).Left(20));
            Assert.AreEqual("01234", "0123456789".Left(5));
            Assert.AreEqual("01234", "01234".Left(10));
        }

        [TestMethod]
        public void StringExtentionsLikeTest()
        {
            Assert.IsTrue("01234".Like("01234"));
            Assert.IsTrue("01234".Like("%01234"));
            Assert.IsTrue("01234".Like("01234%"));
            Assert.IsTrue("01234".Like("%01234%"));
            Assert.IsTrue("01234".Like("%1234"));
            Assert.IsTrue("01234".Like("0123%"));
            Assert.IsTrue("01234".Like("%123%"));
            Assert.IsFalse("01234".Like("001234"));
            Assert.IsFalse("01234".Like("012345"));
            Assert.Inconclusive("Unsure Of Remaining Test Cases");
            Assert.IsFalse("01234".Like("123"));
            Assert.IsFalse("01234".Like("%123"));
            Assert.IsFalse("01234".Like("123%"));
            Assert.IsFalse(((string) null).Like("1"));
        }

        [TestMethod]
        public void StringExtentionsParseRangesTest()
        {
            CheckFor(new[] { 1 }, "1");
            CheckFor(new[] { 1, 2, 3 }, "1,2,3");
            CheckFor(new[] { 1, 2, 3, 4, 5 }, "1-5");
            CheckFor(new[] { 1, 2, 3, 4, 5, 10 }, "1-5,10");
            CheckFor(new[] { 1, 2, 10, 11, 12, 13, 14, 15, 30 }, "1,2,10-15,30");
            CheckFor(new[] { 1, 2, 3, 4 }, "2,3,1-4");
            CheckError("1abcde30");
            CheckError("1--2");
            CheckError("2-1,1");
            CheckError("1,2-1");
        }

        private void CheckError(string theString)
        {
            try
            {
                var result = theString.ParseRanges();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsFalse(ex is AssertFailedException);
            }
        }

        private void CheckFor(int[] expected, string theString)
        {
            var result = theString.ParseRanges();
            Assert.AreEqual(expected.Count(), result.Count());
            Assert.IsTrue(expected.All(i => result.Contains(i)));
        }
    }
}