using System.Collections.Generic;

namespace Macaca
{
    public class Evaluator
    {
        public static Null Null = new Null();
        public static Bool True = new Bool() { Value = true };
        public static Bool False = new Bool() { Value = false };

        private Node node;
        private Environment environment = new Environment();

        public Evaluator(Node node)
        {
            this.node = node;
        }

        public Object Eval()
        {
            return this.Eval(this.node, this.environment);
        }

        private Object Eval(Node node, Environment env)
        {
            switch (node)
            {
                case Program program:
                    return this.EvalProgram(program, env);
                case BlockStatement block:
                    return this.EvalBlock(block, env);
                case ExpressionStatement expression:
                    return this.Eval(expression.Expression, env);
                case ReturnStatement returnStatement:
                    {
                        var value = this.Eval(returnStatement.ReturnValue, env);

                        if (this.IsError(value))
                        {
                            return value;
                        }

                        return new ReturnValue() { Value = value };
                    }
                case LetStatement let:
                    {
                        var value = this.Eval(let.Value, env);

                        if (this.IsError(value))
                        {
                            return value;
                        }

                        env[let.Name.Value] = value;
                        break;
                    }
                case IntegerLiteral integer:
                    return new Integer() { Value = integer.Value };
                case StringLiteral stringLiteral:
                    return new String() { Value = stringLiteral.Value };
                case Boolean boolean:
                    return boolean.Value ? True : False;
                case ArrayLiteral array:
                    var elements = this.EvalExpressions(array.Elements, env);
                    if (elements.Length == 1 && this.IsError(elements[0]))
                    {
                        return elements[0];
                    }
                    return new Array() { Elements = elements };
                case HashLiteral hash:
                    return this.EvalHashLiteral(hash, env);
                case PrefixExpression prefix:
                    {
                        var right = this.Eval(prefix.Right, env);

                        if (this.IsError(right))
                        {
                            return right;
                        }

                        return this.EvalPrefixExpression(prefix.Operator, right);
                    }
                case InfixExpression infix:
                    {
                        var right = this.Eval(infix.Right, env);

                        if (this.IsError(right))
                        {
                            return right;
                        }

                        var left = this.Eval(infix.Left, env);

                        if (this.IsError(left))
                        {
                            return left;
                        }

                        return this.EvalInfixExpression(infix.Operator, left, right);
                    }
                case IfExpression ifExpression:
                    return this.EvalIfExpression(ifExpression, env);
                case Identifier identifier:
                    return this.EvalIdentifier(identifier, env);
                case FunctionLiteral function:
                    {
                        var param = function.Parameter;
                        var body = function.Body;

                        return new Function() { Parameters = param, Body = body, Env = env };
                    }
                case CallExpression call:
                    {
                        var function = this.Eval(call.Function, env);

                        if (this.IsError(function))
                        {
                            return function;
                        }

                        var args = this.EvalExpressions(call.Arguments, env);

                        if (args.Length == 1 && this.IsError(args[0]))
                        {
                            return args[0];
                        }

                        return this.ApplyFunction(function, args);
                    }
                case IndexExpresssion indexExpresssion:
                    {
                        var left = this.Eval(indexExpresssion.Left, env);
                        if (this.IsError(left))
                        {
                            return left;
                        }
                        var index = this.Eval(indexExpresssion.Index, env);
                        if (this.IsError(index))
                        {
                            return index;
                        }
                        return this.EvalIndexExpression(left, index);
                    }
                default:
                    return null;
            }

            return null;
        }

        private Object EvalProgram(Program program, Environment env)
        {
            Object result = null;
            foreach (var statement in program.statements)
            {
                result = this.Eval(statement, env);
                switch (result)
                {
                    case ReturnValue returnValue:
                        return returnValue.Value;
                    case Error error:
                        return result;
                }

            }
            return result;
        }

        private Object EvalBlock(BlockStatement block, Environment env)
        {
            Object result = null;
            foreach (var statement in block.statements)
            {
                result = this.Eval(statement, env);
                if (result != null)
                {
                    var type = result.Type;
                    if (type == ObjectType.RETURN_VALUE || type == ObjectType.ERROR)
                    {
                        return result;
                    }
                }
            }
            return result;
        }

        private Object EvalPrefixExpression(string op, Object right)
        {
            switch (op)
            {
                case "!":
                    return this.EvalBangOperatorExpression(right);
                case "-":
                    return this.EvalMinusOperatorExpression(right);
                default:
                    return new Error() { Message = $"Unknown operator {op}{right.Type}" };
            }
        }

        private Object EvalInfixExpression(string op, Object light, Object right)
        {

            if (light.Type == ObjectType.INTEGER && right.Type == ObjectType.INTEGER)
            {
                return this.EvalIntegerInfixExpression(op, light, right);
            }
            else if (light.Type == ObjectType.STRING && right.Type == ObjectType.STRING)
            {
                return this.EvalStringInfixExpression(op, light, right);
            }
            else if (op == "==")
            {
                return light == right ? True : False;
            }
            else if (op == "!=")
            {
                return light != right ? True : False;
            }
            else if (light.Type != right.Type)
            {
                return new Error() { Message = $"Type mismatch: {light.Type} {op} {right.Type}" };
            }
            else
            {
                return new Error() { Message = $"Unknown operator: {light.Type} {op} {right.Type}" };
            }
        }

        private Object EvalBangOperatorExpression(Object right)
        {
            switch (right)
            {
                case Bool value when value == True:
                    return False;
                case Bool value when value == False:
                    return True;
                case Null _:
                    return True;
                default:
                    return False;
            }
        }

        private Object EvalMinusOperatorExpression(Object right)
        {
            if (right.Type != ObjectType.INTEGER)
            {
                return new Error() { Message = $"Unknown operator: -{right.Type}" };
            }

            var interger = right as Integer;

            return new Integer() { Value = -interger.Value };
        }

        private Object EvalIntegerInfixExpression(string op, Object light, Object right)
        {
            var lightValue = light as Integer;
            var rightValue = right as Integer;

            switch (op)
            {
                case "+":
                    return new Integer() { Value = lightValue.Value + rightValue.Value };
                case "-":
                    return new Integer() { Value = lightValue.Value - rightValue.Value };
                case "*":
                    return new Integer() { Value = lightValue.Value * rightValue.Value };
                case "/":
                    return new Integer() { Value = lightValue.Value / rightValue.Value };
                case "<":
                    return lightValue.Value < rightValue.Value ? True : False;
                case ">":
                    return lightValue.Value > rightValue.Value ? True : False;
                case "==":
                    return lightValue.Value == rightValue.Value ? True : False;
                case "!=":
                    return lightValue.Value != rightValue.Value ? True : False;
                default:
                    return new Error() { Message = $"Unknown operator: {light.Type} {op} {right.Type}" };
            }
        }

        private Object EvalStringInfixExpression(string op, Object left, Object right)
        {
            if (op != "+")
            {
                return new Error() { Message = $"Unknown operator: {left.Type} {op} {right.Type}" };
            }
            var leftString = left as String;
            var rightString = right as String;

            return new String() { Value = $"{leftString.Value}{rightString.Value}" };
        }

        private Object EvalIfExpression(IfExpression expression, Environment env)
        {
            var condition = this.Eval(expression.Condition, env);

            if (this.IsError(condition))
            {
                return condition;
            }

            if (this.IsTruthy(condition))
            {
                return this.Eval(expression.Cons, env);
            }
            else if (expression.Els != null)
            {
                return this.Eval(expression.Els, env);
            }
            else
            {
                return Null;
            }
        }

        private Object EvalIdentifier(Identifier identifier, Environment env)
        {
            Object obj = null;

            if (env.TryGetValue(identifier.Value, out obj))
            {
                return obj;
            }

            Builtin builtin = null;

            if (Builtins.Functions.TryGetValue(identifier.Value, out builtin))
            {
                return builtin;
            }

            return new Error() { Message = $"Identifier not found: {identifier.Value}" };
        }

        private Object EvalIndexExpression(Object left, Object index)
        {
            if (left.Type == ObjectType.ARRAY && index.Type == ObjectType.INTEGER)
            {
                return this.EvalArrayIndexExpression(left, index);
            }
            else if (left.Type == ObjectType.HASH)
            {
                return this.EvalHashIndexExpression(left, index);
            }
            else
            {
                return new Error() { Message = $"Index operator not supported: {left.Type}" };
            }
        }

        private Object EvalArrayIndexExpression(Object left, Object index)
        {
            var array = left as Array;
            var i = (index as Integer).Value;
            var max = array.Elements.Length - 1;

            if (i < 0 || i > max)
            {
                return Null;
            }

            return array.Elements[i];
        }

        private Object EvalHashLiteral(HashLiteral hash, Environment env)
        {
            var pairs = new Dictionary<HashKey, HashPair>();

            foreach (var kvp in hash.Pairs)
            {
                var key = this.Eval(kvp.Key, env);

                if (this.IsError(key))
                {
                    return key;
                }

                var hashKey = key as Hashable;

                if (hashKey == null)
                {
                    return new Error() { Message = $"Unusable as hash key: {key.Type}" };
                }

                var value = this.Eval(kvp.Value, env);

                if (this.IsError(value))
                {
                    return value;
                }

                var hashed = hashKey.HashKey();
                pairs[hashed] = new HashPair() { Key = key, Value = value };
            }

            return new Hash() { Pairs = pairs };
        }

        private Object EvalHashIndexExpression(Object left, Object index)
        {
            var hash = left as Hash;
            var key = index as Hashable;

            if (key == null)
            {
                return new Error() { Message = $"Unusable as hash key: {index.Type}" };
            }

            HashPair pair = null;

            if (hash.Pairs.TryGetValue(key.HashKey(), out pair))
            {
                return pair.Value;
            }

            return Null;
        }

        private bool IsError(Object obj)
        {
            return obj != null && obj.Type == ObjectType.ERROR;
        }

        private bool IsTruthy(Object obj)
        {
            switch (obj)
            {
                case Null nul:
                    return false;
                case Bool value when value == True:
                    return true;
                case Bool value when value == False:
                    return false;
                default:
                    return true;
            }
        }

        private Object[] EvalExpressions(Expression[] expressions, Environment env)
        {
            var result = new List<Object>();

            foreach (var expression in expressions)
            {
                var evaluated = this.Eval(expression, env);

                if (this.IsError(evaluated))
                {
                    return new[] { evaluated };
                }

                result.Add(evaluated);
            }

            return result.ToArray();
        }

        private Object ApplyFunction(Object obj, Object[] args)
        {
            Environment ExtendFunctionEnv(Function function, Object[] args)
            {
                var env = new Environment(function.Env);

                for (var i = 0; i < function.Parameters.Length; i++)
                {
                    var param = function.Parameters[i];
                    env[param.Value] = args[i];
                }

                return env;
            }
            Object UnwrapReturnValue(Object obj)
            {
                var returnValue = obj as ReturnValue;

                if (returnValue != null)
                {
                    return returnValue.Value;
                }

                return obj;
            }
            switch (obj)
            {
                case Function function:
                    var extendEnv = ExtendFunctionEnv(function, args);
                    var evaluated = this.Eval(function.Body, extendEnv);

                    return UnwrapReturnValue(evaluated);
                case Builtin builtin:
                    return builtin.BuiltinFunction(args);
                default:
                    return new Error() { Message = $"Not a function: {obj.Type}" };
            }
        }
    }
}
