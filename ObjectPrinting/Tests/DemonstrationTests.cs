using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class DemonstrationTests
    {
        [Test]
        public void Demo_TreeNodePrinting()
        {
            var root = new TreeNode(0);
            root.LeftNode = new TreeNode(1);
            root.RightNode = new TreeNode(2);
            var printer = ObjectPrinter.For<TreeNode>();
            var result = printer.PrintToString(root);
            var path = AppDomain.CurrentDomain.BaseDirectory + "DemoTree.txt";
            WriteToFile(result, path);
            Console.WriteLine($"The result is written to {path}!");
        }

        [Test]
        public void Demo_AnonymousObjectPrinting()
        {
            var obj = new {Name = "Tom", Age = 34, list = new List<int> {1, 2, 3}};
            var printer = ObjectPrinter.For<object>();
            var result = printer.PrintToString(obj);
            var path = AppDomain.CurrentDomain.BaseDirectory + "DemoAnonymous.txt";
            WriteToFile(result, path);
            Console.WriteLine($"The result is written to {path}!");
        }

        private void WriteToFile(string text, string path)
        {
            var textFile = new System.IO.StreamWriter(path);
            textFile.Write(text);
            textFile.Close();
        }
    }
}