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
            var g = Augment(f, "ok");
            Dump(g);
        }

        static Expression<Func<X, bool>> Augment<X>(Expression<Func<X, string>> e, string query)
        {
            Console.WriteLine(Expression.Call(e.Body, typeof(String).GetMethod("Contains"), Expression.Constant(query)));
            return x => e.Compile()(x).Contains(query);
        }

        static void Dump<X, Y>(Expression<Func<X, Y>> e)
        {
            Console.WriteLine("vvvvv");
            Console.WriteLine(e);
            DumpExpression(e);
            Console.WriteLine("^^^^^");
        }

        static void DumpExpression(Expression e, string indent = "")
        {
            var nextIndent = indent + "  ";
            Console.WriteLine(e.NodeType);
            Console.WriteLine(e.Type);
        }

        // This seemed like the easiest way to do `var f = x => x.Ok`.
        static new Expression<Func<X, Y>> F<X, Y>(Expression<Func<X, Y>> f)
        {
            return f;
        }
    }
}
