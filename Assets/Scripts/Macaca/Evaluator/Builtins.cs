using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Macaca
{
    public static class Builtins
    {
        public static Dictionary<string, Builtin> Functions = new Dictionary<string, Builtin>()
        {
            {"len", new Builtin() { BuiltinFunction = Len } },
            {"puts", new Builtin() { BuiltinFunction = Puts} },
            {"first", new Builtin() { BuiltinFunction = First} },
            {"last", new Builtin() { BuiltinFunction = Last} },
            {"rest", new Builtin() { BuiltinFunction = Rest} },
            {"push", new Builtin() { BuiltinFunction = Push} }
        };

        private static Object Len(params Object[] args)
        {
            if (args.Length != 1)
            {
                return new Error() { Message = $"Wrong number of arguments. got={args.Length}, want=1" };
            }

            switch (args[0])
            {
                case Array array:
                    return new Integer() { Value = array.Elements.Length };
                case String str:
                    return new Integer() { Value = str.Value.Length };
                default:
                    return new Error() { Message = $"Argument to `len` not supported, got {args[0].Type}" };
            }
        }

        private static Object Puts(params Object[] args)
        {
            foreach (var obj in args)
            {
                System.Console.WriteLine(obj.Inspect());
            }

            return Evaluator.Null;
        }

        private static Object First(params Object[] args)
        {
            if (args.Length != 1)
            {
                return new Error() { Message = $"Wrong number of arguments. got={args.Length}, want=1" };
            }

            if (args[0].Type != ObjectType.ARRAY)
            {
                return new Error() { Message = $"Argument to `first` must be ARRAY, got {args[0].Type}" };
            }

            var array = args[0] as Array;

            if (array.Elements.Length > 0)
            {
                return array.Elements[0];
            }

            return Evaluator.Null;
        }

        private static Object Last(params Object[] args)
        {
            if (args.Length != 1)
            {
                return new Error() { Message = $"Wrong number of arguments. got={args.Length}, want=1" };
            }

            if (args[0].Type != ObjectType.ARRAY)
            {
                return new Error() { Message = $"Argument to `last` must be ARRAY, got {args[0].Type}" };
            }

            var array = args[0] as Array;

            if (array.Elements.Length > 0)
            {
                return array.Elements[array.Elements.Length - 1];
            }

            return Evaluator.Null;
        }

        private static Object Rest(params Object[] args)
        {
            if (args.Length != 1)
            {
                return new Error() { Message = $"Wrong number of arguments. got={args.Length}, want=1" };
            }

            if (args[0].Type != ObjectType.ARRAY)
            {
                return new Error() { Message = $"Argument to `rest` must be ARRAY, got {args[0].Type}" };
            }

            var array = args[0] as Array;

            if (array.Elements.Length > 0)
            {
                var elements = new Object[array.Elements.Length - 1];
                System.Array.Copy(array.Elements, 1, elements, 0, array.Elements.Length - 1);
                return new Array() { Elements = elements };
            }

            return Evaluator.Null;
        }

        private static Object Push(params Object[] args)
        {
            if (args.Length != 2)
            {
                return new Error() { Message = $"Wrong number of arguments. got={args.Length}, want=2" };
            }

            if (args[0].Type != ObjectType.ARRAY)
            {
                return new Error() { Message = $"Argument to `push` must be ARRAY, got {args[0].Type}" };
            }

            var array = args[0] as Array;

            var elements = new Object[array.Elements.Length + 1];
            System.Array.Copy(array.Elements, 0, elements, 0, array.Elements.Length);
            elements[array.Elements.Length] = args[1];
            return new Array() { Elements = elements };
        }
    }
}
