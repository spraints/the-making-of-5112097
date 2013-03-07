using System;
using System.Linq.Expressions;

namespace ConsoleApplication1
{
    class Program
    {
        class Thing
        {
            public string Ok { get; set; }
        }

        static void Main(string[] args)
        {
            Dump<Thing, string>(x => x.Ok);
        }

        static void Dump<X, Y>(Expression<Func<X, Y>> e)
        {
            Console.WriteLine(e.Type);
            Console.WriteLine(e.Parameters);
            Console.WriteLine(e.NodeType);
            Console.WriteLine(e.Body);
            Console.WriteLine(e);
        }
    }
}
