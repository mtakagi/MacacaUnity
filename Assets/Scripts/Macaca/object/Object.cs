using System.Collections.Generic;
using System.Text;

namespace Macaca
{
    public enum ObjectType
    {
        NULL,
        ERROR,
        INTEGER,
        BOOLEAN,
        STRING,
        RETURN_VALUE,
        FUNCTION,
        BUILTIN,
        ARRAY,
        HASH,
    }

    public interface Object
    {
        ObjectType Type { get; }

        string Inspect();
    }

    public interface Hashable
    {
        HashKey HashKey();
    }

    public class HashKey
    {
        public ObjectType Type { get; set; }
        public ulong Value { get; set; }

        public override int GetHashCode()
        {
            return (int)this.Value;
        }

        public override bool Equals(object obj)
        {
            var other = obj as HashKey;
            if (other == null)
            {
                return false;
            }

            return this.Type == other.Type && this.Value == other.Value;
        }
    }

    public class Integer : Object, Hashable
    {
        public System.Int64 Value { get; set; }
        public ObjectType Type { get => ObjectType.INTEGER; }

        public string Inspect()
        {
            return this.Value.ToString();
        }

        public HashKey HashKey()
        {
            return new HashKey() { Type = this.Type, Value = (ulong)this.Value };
        }
    }

    public class Bool : Object, Hashable
    {
        public bool Value { get; set; }
        public ObjectType Type => ObjectType.BOOLEAN;

        public string Inspect()
        {
            return this.Value.ToString();
        }

        public HashKey HashKey()
        {
            return new HashKey() { Type = this.Type, Value = this.Value ? (ulong)1 : (ulong)0 };
        }
    }

    public class String : Object, Hashable
    {
        public string Value { get; set; }
        public ObjectType Type => ObjectType.STRING;

        public string Inspect()
        {
            return this.Value;
        }

        public HashKey HashKey()
        {
            return new HashKey() { Type = this.Type, Value = (ulong)this.Value.GetHashCode() };
        }
    }

    public class Array : Object
    {
        public Object[] Elements { get; set; }
        public ObjectType Type => ObjectType.ARRAY;

        public string Inspect()
        {
            var sb = new StringBuilder("[");

            for (var i = 0; i < this.Elements.Length; i++)
            {
                if (i == this.Elements.Length - 1)
                {
                    sb.Append(this.Elements[i].Inspect());
                }
                else
                {
                    sb.Append($"{this.Elements[i].Inspect()}, ");
                }
            }

            return sb.Append("]").ToString();
        }
    }

    public class HashPair
    {
        public Object Key { get; set; }
        public Object Value { get; set; }
    }

    public class Hash : Object
    {
        public Dictionary<HashKey, HashPair> Pairs { get; set; }
        public ObjectType Type => ObjectType.HASH;

        public string Inspect()
        {
            var sb = new StringBuilder("{");
            var i = 0;

            foreach (var kvp in this.Pairs)
            {
                if (i == this.Pairs.Count - 1)
                {
                    sb.Append($"{kvp.Value.Key.Inspect()}: {kvp.Value.Value.Inspect()}");
                }
                else
                {
                    sb.Append($"{kvp.Value.Key.Inspect()}: {kvp.Value.Value.Inspect()}, ");
                }
            }

            return sb.Append("}").ToString();
        }
    }

    public class Null : Object
    {
        public ObjectType Type => ObjectType.NULL;

        public string Inspect()
        {
            return "null";
        }
    }

    public class ReturnValue : Object
    {
        public Object Value { get; set; }
        public ObjectType Type => ObjectType.RETURN_VALUE;

        public string Inspect()
        {
            return this.Value.Inspect();
        }
    }

    public class Error : Object
    {
        public string Message { get; set; }

        public ObjectType Type => ObjectType.ERROR;

        public string Inspect()
        {
            return $"ERROR: {this.Message}";
        }
    }

    public class Function : Object
    {
        public Identifier[] Parameters { get; set; }
        public BlockStatement Body { get; set; }

        public Environment Env { get; set; }

        public ObjectType Type => ObjectType.FUNCTION;

        public string Inspect()
        {
            var sb = new System.Text.StringBuilder("fn(");

            for (var i = 0; i < this.Parameters.Length; i++)
            {
                if (i + 1 == this.Parameters.Length)
                {
                    sb.Append(this.Parameters[i].String);
                }
                else
                {
                    sb.Append($"{this.Parameters[i].String}, ");
                }
            }

            sb.Append(") {\n");
            sb.Append(this.Body.String);
            sb.Append("\n}");

            return sb.ToString();
        }
    }

    public delegate Object BuiltinFunction(params Object[] args);

    public class Builtin : Object
    {
        public BuiltinFunction BuiltinFunction { get; set; }

        public ObjectType Type => ObjectType.BUILTIN;

        public string Inspect()
        {
            return "builtin function";
        }
    }
}
