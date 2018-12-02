using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    class StudentGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Student> Students { get; set; }

    }

    class Student
    {
        public string Name { get; set; }

        public Student(string name)
        {
            Name = name;
        }
    }

    [TestFixture]
    public class PrintingCollectionTests
    {
        [Test]
        [Category("SimpleTest")]
        public void ShouldCorrectPrintList()
        {
            var testList = new List<int>() { 1, 2, 3 };
            var serialize = testList.PrintToString();
            serialize.Should().Be($"List`1 :{Environment.NewLine}" +
                                  $"\t   1,{Environment.NewLine}" +
                                  $"\t   2,{Environment.NewLine}" +
                                  $"\t   3,{Environment.NewLine}\t");
        }

        [Test]
        [Category("SimpleTest")]
        public void Printing_ShouldCorrectPrintingArray()
        {
            var arr = new [] {1,2,3};
            var serialize = arr.PrintToString();
            serialize.Should().Be($"Int32[] :{Environment.NewLine}" +
                                  $"\t   1,{Environment.NewLine}" +
                                  $"\t   2,{Environment.NewLine}" +
                                  $"\t   3,{Environment.NewLine}\t");
        }

        [Test]
        [Category("SimpleTest")]
        public void Printing_ShouldCorrectPrintingDictionary()
        {
            var dict = new Dictionary<int, string>()
            {
                {0,"zero"},
                {1,"odin"},
                {2,"two"},
            };
            var serialize = dict.PrintToString();
            serialize.Should().Be($"Dictionary`2 :{Environment.NewLine}" +
                                  $"\t   KeyValuePair`2{Environment.NewLine}" +
                                  $"\t\tKey = 0{Environment.NewLine}" +
                                  $"\t\tValue = zero,{Environment.NewLine}" +
                                  $"\t   KeyValuePair`2{Environment.NewLine}" +
                                  $"\t\tKey = 1{Environment.NewLine}" +
                                  $"\t\tValue = odin,{Environment.NewLine}" +
                                  $"\t   KeyValuePair`2{Environment.NewLine}" +
                                  $"\t\tKey = 2{Environment.NewLine}" +
                                  $"\t\tValue = two,{Environment.NewLine}\t");
        }

        [Test]
        [Category("SimpleTest")]
        public void Printing_ShouldCorrectPrintingStack()
        {
            var stack = new Stack<int>();
            stack.Push(2);
            stack.Push(3);
            stack.Push(4);
            var serialize = stack.PrintToString();
            serialize.Should().Be($"Stack`1 :{Environment.NewLine}" +
                                  $"\t   4,{Environment.NewLine}" +
                                  $"\t   3,{Environment.NewLine}" +
                                  $"\t   2,{Environment.NewLine}\t");
        }

        [Test]
        [Category("SimpleTest")]
        public void Printing_ShouldCorrectPrintingQueque()
        {
            var q = new Queue<int>();
            q.Enqueue(1);
            q.Enqueue(2);
            q.Enqueue(3);
            var serialize = q.PrintToString();
            serialize.Should().Be($"Queue`1 :{Environment.NewLine}" +
                                  $"\t   1,{Environment.NewLine}" +
                                  $"\t   2,{Environment.NewLine}" +
                                  $"\t   3,{Environment.NewLine}\t");
        }

        [Test]
        [Category("ComplicatedTest")]
        public void Printing_ShouldCorrectPrintingObjectWithNestingCollection()
        {
            var sg = new StudentGroup()
            {
                Students = new List<Student>()
                {
                    new Student("Вася"), new Student("Петя"), new Student(""),
                } ,
                Id = 8,
                Name = "МЕН - 272201"
            };
            var serialize = sg.PrintToString(cnf => cnf
                .Printing<string>()
                .Using(s => s.ToLower())
                .Apply()
                .Printing(g => g.Students)
                .Using(s => $"[{s.PrintToString()}]")
                .Apply());
            serialize.Should().Be($"StudentGroup{Environment.NewLine}" +
                                  $"\tId = 8{Environment.NewLine}" +
                                  $"\tName = мен - 272201{Environment.NewLine}" +
                                  $"\tStudents = [List`1 :{Environment.NewLine}" +
                                  $"\t   Student{Environment.NewLine}" +
                                  $"\t\tName = Вася,{Environment.NewLine}" +
                                  $"\t   Student{Environment.NewLine}" +
                                  $"\t\tName = Петя,{Environment.NewLine}" +
                                  $"\t   Student{Environment.NewLine}" +
                                  $"\t\tName = ,{Environment.NewLine}\t]");
        }




    }
}
