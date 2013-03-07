using System;
using System.Linq.Expressions;

namespace ConsoleApplication1
{
    class Thing
    {
        public string Ok { get; set; }
    }

    static class Program
    {
        static void Main(string[] args)
        {
            var f = F<Thing, string>(x => x.Ok);
            Describe(f);
            Describe(CheckForQuery(f, "ok"));
            Describe(NullSafeString(f));
            Describe(AllInOne(f, "ok"));
        }

        static Expression<Func<X, bool>> CheckForQuery<X>(Expression<Func<X, string>> e, string query)
        {
            var queryCheck = Expression.Call(e.Body, typeof(String).GetMethod("Contains"), Expression.Constant(query));
            return Expression.Lambda<Func<X, bool>>(queryCheck, e.Parameters);
        }

        static Expression<Func<X, string>> NullSafeString<X>(Expression<Func<X, string>> e)
        {
            var nullSafe = Expression.Coalesce(e.Body, Expression.Constant("")); // Coalesce is the ?? operator
            return Expression.Lambda<Func<X, string>>(nullSafe, e.Parameters);
        }

        static Expression<Func<X, bool>> AllInOne<X>(Expression<Func<X, string>> e, string query)
        {
            return Expression.Lambda<Func<X, bool>>(
                Expression.Call(
                    Expression.Coalesce(e.Body, Expression.Constant("")),
                    typeof(string).GetMethod("Contains"),
                    Expression.Constant(query)),
                e.Parameters);
        }

        static void Describe<Y>(Expression<Func<Thing, Y>> e)
        {
            Console.WriteLine("");
            Console.WriteLine("type: " + e.Type);
            Console.WriteLine("code: " + e);
            Console.WriteLine("root expression: " + e.Body.NodeType + "(" + e.Body.GetType() + ")");
            var f = e.Compile();
            WriteResult(f, "Ok=(null)", new Thing());
            WriteResult(f, "Ok=\"not\"", new Thing { Ok = "not" });
            WriteResult(f, "Ok=\"ok\"", new Thing { Ok = "ok" });
        }

        static void WriteResult<X, Y>(Func<X, Y> f, string label, X x)
        {
            object result;
            try
            {
                result = f(x);
                if (result is String)
                    result = "\"" + result + "\"";
                if (result == null)
                    result = "(null)";
            }
            catch (Exception e)
            {
                result = e;
            }
            Console.WriteLine(label + " => " + result);
        }

        // This seemed like the easiest way to do something like `var f = x => x.Ok`.
        static Expression<Func<X, Y>> F<X, Y>(Expression<Func<X, Y>> f)
        {
            return f;
        }
    }
}
