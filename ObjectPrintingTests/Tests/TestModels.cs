using System;

namespace ObjectPrintingTests.Tests
{
    public class Node
    {
        public string Name { get; set; } = "";
        public Node? Next { get; set; }
    }

    public class TreeNode
    {
        public string Name { get; set; } = "";
        public TreeNode? Father { get; set; }
    }

    public class Lesson
    {
        public string Title { get; set; } = "";
        public DateTime Date { get; set; }
    }

    public class Contact
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string PublicNickname = "";
    }

    public class Account
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public double Balance { get; set; }
        public int Score { get; set; }
        public int Age { get; set; }
    }
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
    }
}