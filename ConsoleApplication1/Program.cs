using System;
using System.Linq.Expressions;
using System.Reflection;

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
            var q = "ok";
            Describe(f);
            Describe(CheckForQuery(f, q));
            Describe(NullSafeString(f));
            Describe(AllInOne(f, q));
            Describe(f.Wrap(s => (s ?? "").Contains(q)));
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

        // This:
        //    var expr = new Expression<Func<Thing, string>>(thing => thing.Ok);
        //    var final = expr.Wrap(s => (s ?? "").Contains("ok"));
        // should be equivalent to:
        //    var final = new Expression<Func<Thing, bool>>(thing => (thing.Ok ?? "").Contains("ok"));
        // Note: using inner twice (e.g. `expr.Wrap(s => s + s)`) will execute the inner lambda twice.
        static Expression<Func<A, C>> Wrap<A, B, C>(this Expression<Func<A, B>> inner, Expression<Func<B, C>> outer)
        {
            var injector = new ExpressionInjector(outer.Parameters[0].Name, inner.Body);
            var converted = injector.Visit(outer.Body);
            return Expression.Lambda<Func<A, C>>(converted, inner.Parameters);
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

    class ExpressionInjector : ExpressionVisitor
    {
        string placeholder;
        Expression expansion;

        public ExpressionInjector(string placeholder, Expression expansion)
        {
            this.placeholder = placeholder;
            this.expansion = expansion;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.Constant)
            {
                var constant = ((ConstantExpression)node.Expression).Value;
                if (node.Member is PropertyInfo)
                {
                    return Expression.Constant(((PropertyInfo)node.Member).GetValue(constant, new object[0]));
                }
                else if (node.Member is FieldInfo)
                {
                    return Expression.Constant(((FieldInfo)node.Member).GetValue(constant));
                }
                else
                {
                    // Not sure what type of member this is?
                }
            }
            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Name == placeholder)
            {
                return expansion;
            }
            else
            {
                return node;
            }
        }
    }
}
