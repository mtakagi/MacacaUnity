using System.Text;

namespace Macaca
{
    public interface Node
    {
        string TokenLiteral { get; }
        string String { get; }
    }

    public interface Statement : Node { }

    public interface Expression : Node { }

    public class Program : Node
    {
        public Statement[] statements;

        public string TokenLiteral
        {
            get
            {
                if (statements.Length > 0)
                {
                    return statements[0].TokenLiteral;
                }
                else
                {
                    return "";
                }
            }
        }
        public string String
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var statement in statements)
                {
                    sb.Append(statement.String);
                }

                return sb.ToString();
            }
        }
    }

    public class LetStatement : Statement
    {
        public Token Token { get; set; }
        public Identifier Name { get; set; }
        public Expression Value { get; set; }

        public string TokenLiteral => this.Token.Literal;

        public string String
        {
            get
            {
                var sb = new StringBuilder($"{this.TokenLiteral} {this.Name.String} = ");

                if (this.Value != null)
                {
                    sb.Append(this.Value.String);
                }

                sb.Append(";");

                return sb.ToString();
            }
        }
    }

    public class ReturnStatement : Statement
    {
        public Token Token { get; set; }
        public Expression ReturnValue { get; set; }

        public string TokenLiteral => this.Token.Literal;

        public string String
        {
            get
            {
                var sb = new StringBuilder($"{this.TokenLiteral} ");

                if (this.ReturnValue != null)
                {
                    sb.Append(this.ReturnValue.String);
                }

                sb.Append(";");

                return sb.ToString();
            }
        }
    }

    public class ExpressionStatement : Statement
    {
        public Token Token { get; set; }
        public Expression Expression { get; set; }

        public string TokenLiteral => this.Token.Literal;

        public string String => this.Expression != null ? this.Expression.String : "";
    }

    public class BlockStatement : Statement
    {
        public Token Token { set; get; }

        public string TokenLiteral => this.Token.Literal;

        public string String
        {
            get
            {
                var sb = new StringBuilder();

                foreach (var statement in this.statements)
                {
                    sb.Append(statement.String);
                }

                return sb.ToString();
            }
        }

        public Statement[] statements;
    }

    public class Identifier : Expression
    {
        public Token Token { get; set; }
        public string Value { get; set; }

        public string TokenLiteral => this.Token.Literal;

        public string String => this.Value;
    }

    public class Boolean : Expression
    {
        public Token Token { get; set; }
        public bool Value { get; set; }

        public string TokenLiteral => this.Token.Literal;

        public string String => this.Token.Literal;
    }

    public class IntegerLiteral : Expression
    {
        public Token Token { get; set; }
        public System.Int64 Value { get; set; }

        public string TokenLiteral => this.Token.Literal;

        public string String => this.Token.Literal;
    }

    public class PrefixExpression : Expression
    {
        public Token Token { get; set; }
        public string Operator { get; set; }
        public Expression Right { get; set; }

        public string TokenLiteral => this.Token.Literal;

        public string String => $"({this.Operator}{this.Right.String})";
    }

    public class InfixExpression : Expression
    {
        public Token Token { get; set; }
        public string Operator { get; set; }
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public string TokenLiteral => this.Token.Literal;

        public string String => $"({this.Left.String} {this.Operator} {this.Right.String})";
    }

    public class IfExpression : Expression
    {
        public Token Token { get; set; }
        public Expression Condition { get; set; }
        public BlockStatement Cons { get; set; }
        public BlockStatement Els { get; set; }

        public string TokenLiteral => this.Token.Literal;

        public string String
        {
            get
            {
                var sb = new StringBuilder($"if {this.Condition.String} {this.Cons.String}");

                if (this.Els != null)
                {
                    sb.Append($" else {this.Els.String}");
                }

                return sb.ToString();
            }
        }
    }

    public class FunctionLiteral : Expression
    {
        public Token Token { get; set; }
        public Identifier[] Parameter { get; set; }
        public BlockStatement Body { get; set; }

        public string TokenLiteral => this.Token.Literal;

        public string String
        {
            get
            {
                var sb = new StringBuilder($"{this.TokenLiteral}(");

                for (var i = 0; i < this.Parameter.Length; i++)
                {
                    if (i + 1 == this.Parameter.Length)
                    {
                        sb.Append(this.Parameter[i].String);
                    }
                    else
                    {
                        sb.Append($"{this.Parameter[i].String}, ");
                    }

                }

                sb.Append($"){this.Body.String}");

                return sb.ToString();
            }
        }
    }

    public class CallExpression : Expression
    {
        public Token Token { get; set; }
        public Expression Function { get; set; }
        public Expression[] Arguments { get; set; }

        public string TokenLiteral => this.Token.Literal;

        public string String
        {
            get
            {
                var sb = new StringBuilder($"{this.Function.String}(");

                for (var i = 0; i < this.Arguments.Length; i++)
                {
                    if (i + 1 == this.Arguments.Length)
                    {
                        sb.Append(this.Arguments[i].String);
                    }
                    else
                    {
                        sb.Append($"{this.Arguments[i].String}, ");
                    }

                }

                sb.Append(")");

                return sb.ToString();
            }
        }
    }
}