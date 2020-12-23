namespace Macaca
{
    public enum ObjectType
    {
        NULL,
        ERROR,
        INTEGER,
        BOOLEAN,
        RETURN_VALUE,
        FUNCTION,
    }

    public interface Object
    {
        ObjectType Type { get; }

        string Inspect();
    }

    public class Integer : Object
    {
        public System.Int64 Value { get; set; }
        public ObjectType Type { get => ObjectType.INTEGER; }

        public string Inspect()
        {
            return this.Value.ToString();
        }
    }

    public class Bool : Object
    {
        public bool Value { get; set; }
        public ObjectType Type => ObjectType.BOOLEAN;

        public string Inspect()
        {
            return this.Value.ToString();
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
}
