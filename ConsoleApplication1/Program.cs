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
            var f = F<Thing, string>(x => x.Ok);
            Dump(f);
            var g = Augment(f);
            Dump(g);
        }

        static Expression<Func<X, bool>> Augment<X>(Expression<Func<X, string>> e)
        {
            return x => e.Compile()(x).Contains("ok");
        }

        static void Dump<X, Y>(Expression<Func<X, Y>> e)
        {
            Console.WriteLine("-----");
            Console.WriteLine(e);
            Console.WriteLine(e.Type);
            Console.WriteLine(e.Parameters);
            Console.WriteLine(e.NodeType);
            Console.WriteLine(e.Body);
        }

        // This seemed like the easiest way to do `var f = x => x.Ok`.
        static new Expression<Func<X, Y>> F<X, Y>(Expression<Func<X, Y>> f)
        {
            return f;
        }
    }
}
