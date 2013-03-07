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
            Dump<Thing>(x => x.Ok);
        }

        static void Dump<X>(Expression<Func<X, string>> e)
        {
        }
    }
}
