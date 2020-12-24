using System.Collections.Generic;
using NUnit.Framework;
using Macaca;

public class ParserTest
{
    [Test]
    public void ParseLetTest()
    {
        var tests = new[] {
            new { input = "let x = 5;", identifier = "x", value = (object)5},
            new { input = "let y = true;", identifier = "y", value = (object)true},
            new { input = "let foobar = y;", identifier = "foobar", value = (object)"y"}
        };

        foreach (var test in tests)
        {
            var lexer = new Lexer(test.input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();

            Assert.AreEqual(program.statements.Length, 1);
            TestLetStatement(program.statements[0], test.identifier);
            TestLiteralExpression((program.statements[0] as LetStatement).Value, test.value);
        }
    }

    [Test]
    public void ParseReturnTest()
    {
        var tests = new[] {
            new { input = "return 5;", value = (object)5},
            new { input = "return true;", value = (object)true},
            new { input = "return foobar;", value = (object)"foobar"}
        };

        foreach (var test in tests)
        {
            var lexer = new Lexer(test.input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();

            Assert.AreEqual(program.statements.Length, 1);
            var returnStatement = program.statements[0] as ReturnStatement;
            Assert.NotNull(returnStatement);
            Assert.AreEqual(returnStatement.TokenLiteral, "return");
            TestLiteralExpression(returnStatement.ReturnValue, test.value);
        }
    }

    [Test]
    public void ParseIntegerLiteralTest()
    {
        var input = "5;";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();

        Assert.That(program.statements.Length, Is.EqualTo(1));

        var expression = program.statements[0] as ExpressionStatement;

        Assert.NotNull(expression);

        var literal = expression.Expression as IntegerLiteral;

        Assert.NotNull(literal);
        Assert.That(literal.Value, Is.EqualTo(5));
        Assert.That(literal.TokenLiteral, Is.EqualTo("5"));
    }

    [Test]
    public void ParsePrefixExpressionTest()
    {
        var tests = new[] {
            new { input = "!5;", op = "!", value = (object)5},
            new {input = "-15;", op = "-", value = (object)15},
            new {input = "!foobar;",op =  "!", value = (object)"foobar"},
            new {input = "-foobar;", op = "-", value = (object)"foobar"},
            new {input = "!true;", op = "!", value = (object)true},
            new {input = "!false;", op = "!", value = (object)false},
        };

        foreach (var test in tests)
        {
            var lexer = new Lexer(test.input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();

            Assert.That(program.statements.Length, Is.EqualTo(1));

            var statement = program.statements[0] as ExpressionStatement;

            Assert.NotNull(statement);

            var expression = statement.Expression as PrefixExpression;

            Assert.NotNull(expression);
            Assert.AreEqual(expression.Operator, test.op);
            TestLiteralExpression(expression.Right, test.value);
        }
    }

    [Test]
    public void ParseInfixExpressionTest()
    {
        var tests = new[] {
            new {input = "5 + 5;", left = (object) 5, op = "+", right = (object) 5},
            new {input = "5 - 5;", left = (object) 5, op = "-", right = (object)5},
            new {input = "5 * 5;", left = (object) 5, op = "*",right = (object) 5},
            new {input = "5 / 5;", left = (object) 5, op = "/",right = (object) 5},
            new {input = "5 > 5;", left = (object) 5, op = ">",right = (object) 5},
            new {input = "5 < 5;", left = (object) 5, op = "<", right = (object)5},
            new {input = "5 == 5;", left = (object) 5,op =  "==", right = (object)5},
            new {input = "5 != 5;", left = (object) 5, op = "!=", right = (object)5},
            new {input = "foobar + barfoo;", left = (object) "foobar", op = "+", right = (object)"barfoo"},
            new {input = "foobar - barfoo;", left = (object)"foobar", op = "-", right = (object)"barfoo"},
            new {input = "foobar * barfoo;", left = (object)"foobar", op = "*", right = (object)"barfoo"},
            new {input = "foobar / barfoo;",left = (object) "foobar", op = "/", right = (object)"barfoo"},
            new {input = "foobar > barfoo;", left = (object)"foobar", op = ">", right = (object)"barfoo"},
            new {input = "foobar < barfoo;", left = (object)"foobar", op = "<", right = (object)"barfoo"},
            new {input = "foobar == barfoo;", left = (object)"foobar",op =  "==", right = (object)"barfoo"},
            new {input = "foobar != barfoo;", left = (object)"foobar", op = "!=", right = (object)"barfoo"},
            new {input = "true == true;", left = (object)true,op =  "==", right = (object)true},
            new {input = "true != false;", left = (object)true, op = "!=", right = (object)false},
            new {input = "false == false;", left = (object)false, op =  "==", right = (object)false},
        };

        foreach (var test in tests)
        {
            var lexer = new Lexer(test.input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();

            Assert.That(program.statements.Length, Is.EqualTo(1));

            var statement = program.statements[0] as ExpressionStatement;

            Assert.NotNull(statement);
        }
    }

    [Test]
    public void ParseOperatorPrecedeceTest()
    {
        var tests = new[] {
            new { input = "-a * b", expected = "((-a) * b)" },
            new { input = "!-a", expected = "(!(-a))" },
            new { input = "a + b + c", expected = "((a + b) + c)" },
            new { input = "a + b - c", expected = "((a + b) - c)" },
            new { input = "a * b * c", expected = "((a * b) * c)" },
            new { input = "a * b / c", expected = "((a * b) / c)" },
            new { input = "a + b / c", expected = "(a + (b / c))" },
            new { input = "a + b * c + d / e - f", expected = "(((a + (b * c)) + (d / e)) - f)" },
            new { input = "3 + 4; -5 * 5", expected = "(3 + 4)((-5) * 5)" },
            new { input = "5 > 4 == 3 < 4", expected = "((5 > 4) == (3 < 4))" },
            new { input = "5 < 4 != 3 > 4", expected = "((5 < 4) != (3 > 4))" },
            new { input = "3 + 4 * 5 == 3 * 1 + 4 * 5", expected = "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))" },
            new { input = "true", expected = "true" },
            new { input = "false", expected = "false" },
            new { input = "3 > 5 == false", expected = "((3 > 5) == false)" },
            new { input = "3 < 5 == true", expected = "((3 < 5) == true)" },
            new { input = "1 + (2 + 3) + 4", expected = "((1 + (2 + 3)) + 4)" },
            new { input = "(5 + 5) * 2", expected = "((5 + 5) * 2)" },
            new { input = "2 / (5 + 5)", expected = "(2 / (5 + 5))" },
            new { input = "(5 + 5) * 2 * (5 + 5)", expected = "(((5 + 5) * 2) * (5 + 5))" },
            new { input = "-(5 + 5)", expected = "(-(5 + 5))" },
            new { input = "!(true == true)", expected = "(!(true == true))" },
            new { input = "a + add(b * c) + d", expected = "((a + add((b * c))) + d)" },
            new { input = "add(a, b, 1, 2 * 3, 4 + 5, add(6, 7 * 8))", expected = "add(a, b, 1, (2 * 3), (4 + 5), add(6, (7 * 8)))" },
            new { input = "add(a + b + c * d / f + g)", expected = "add((((a + b) + ((c * d) / f)) + g))" },
            new { input = "a * [1, 2, 3, 4][b * c] * d", expected = "((a * ([1, 2, 3, 4][(b * c)])) * d)" },
            new { input = "add(a * b[2], b[1], 2 * [1, 2][1])", expected = "add((a * (b[2])), (b[1]), (2 * ([1, 2][1])))" }
        };

        foreach (var test in tests)
        {
            var lexer = new Lexer(test.input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            var actual = program.String;

            Assert.AreEqual(test.expected, actual);
        }
    }

    [Test]
    public void ParseBooleanExpressionTest()
    {
        var tests = new[] {
            new { input = "true;", expected = true },
            new { input = "false;", expected = false }
        };

        foreach (var test in tests)
        {
            var lexer = new Lexer(test.input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();

            Assert.That(program.statements.Length, Is.EqualTo(1));

            var statement = program.statements[0] as ExpressionStatement;

            Assert.NotNull(statement);

            var boolean = statement.Expression as Boolean;

            Assert.NotNull(boolean);
            Assert.AreEqual(test.expected, boolean.Value);
        }
    }

    [Test]
    public void ParseIfExpressionTest()
    {
        var input = "if (x < y) { x }";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();

        Assert.That(program.statements.Length, Is.EqualTo(1));

        var statement = program.statements[0] as ExpressionStatement;

        Assert.NotNull(statement);

        var ifExpression = statement.Expression as IfExpression;

        Assert.NotNull(ifExpression);
        TestInfixExpression(ifExpression.Condition, "x", "<", "y");
        Assert.That(ifExpression.Cons.statements.Length, Is.EqualTo(1));

        var cons = ifExpression.Cons.statements[0] as ExpressionStatement;

        TestIdentifier(cons.Expression, "x");
        Assert.Null(ifExpression.Els);
    }

    [Test]
    public void ParseIfElseExpressionTest()
    {
        var input = "if (x < y) { x } else { y }";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();

        Assert.That(program.statements.Length, Is.EqualTo(1));

        var statement = program.statements[0] as ExpressionStatement;

        Assert.NotNull(statement);

        var ifExpression = statement.Expression as IfExpression;

        Assert.NotNull(ifExpression);
        TestInfixExpression(ifExpression.Condition, "x", "<", "y");
        Assert.That(ifExpression.Cons.statements.Length, Is.EqualTo(1));

        var cons = ifExpression.Cons.statements[0] as ExpressionStatement;

        TestIdentifier(cons.Expression, "x");
        Assert.That(ifExpression.Els.statements.Length, Is.EqualTo(1));

        var els = ifExpression.Els.statements[0] as ExpressionStatement;

        TestIdentifier(els.Expression, "y");
    }

    [Test]
    public void ParseFuncLiteralTest()
    {
        var input = "fn(x, y) { x + y; }";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();

        Assert.That(program.statements.Length, Is.EqualTo(1));

        var statement = program.statements[0] as ExpressionStatement;

        Assert.NotNull(statement);

        var func = statement.Expression as FunctionLiteral;

        Assert.NotNull(func);
        Assert.That(func.Parameter.Length, Is.EqualTo(2));
        TestLiteralExpression(func.Parameter[0], "x");
        TestLiteralExpression(func.Parameter[1], "y");
        Assert.That(func.Body.statements.Length, Is.EqualTo(1));

        var body = func.Body.statements[0] as ExpressionStatement;

        Assert.NotNull(body);
        TestInfixExpression(body.Expression, "x", "+", "y");
    }

    [Test]
    public void ParseFuncParamTest()
    {
        var tests = new[] {
            new { input = "fn() {};", expected = new string[] {}},
            new { input = "fn(x) {};", expected = new []{ "x" }},
            new { input = "fn(x, y, z) {};", expected = new []{ "x", "y", "z"}},
        };

        foreach (var test in tests)
        {
            var lexer = new Lexer(test.input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();

            Assert.That(program.statements.Length, Is.EqualTo(1));

            var expression = program.statements[0] as ExpressionStatement;

            Assert.NotNull(expression);

            var func = expression.Expression as FunctionLiteral;

            Assert.NotNull(func);
            Assert.That(func.Parameter.Length, Is.EqualTo(test.expected.Length));

            for (var i = 0; i < test.expected.Length; i++)
            {
                TestLiteralExpression(func.Parameter[i], test.expected[i]);
            }
        }
    }

    [Test]
    public void ParseCallExpressionTest()
    {
        var input = "add(1, 2 * 3, 4 + 5);";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();

        Assert.That(program.statements.Length, Is.EqualTo(1));

        var expression = program.statements[0] as ExpressionStatement;

        Assert.NotNull(expression);

        var call = expression.Expression as CallExpression;

        Assert.NotNull(call);
        TestIdentifier(call.Function, "add");
        Assert.That(call.Arguments.Length, Is.EqualTo(3));
        TestLiteralExpression(call.Arguments[0], 1);
        TestInfixExpression(call.Arguments[1], 2, "*", 3);
        TestInfixExpression(call.Arguments[2], 4, "+", 5);
    }

    [Test]
    public void ParseCallExpressionParamTest()
    {
        var tests = new[] {
            new { input = "add();", identifier = "add", expected = new string[]{ } },
            new { input = "add(1);", identifier = "add", expected = new []{ "1" } },
            new { input = "add(1, 2 * 3, 4 + 5);", identifier = "add", expected = new []{ "1", "(2 * 3)", "(4 + 5)" } }
        };

        foreach (var test in tests)
        {
            var lexer = new Lexer(test.input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();

            Assert.That(program.statements.Length, Is.EqualTo(1));

            var expression = program.statements[0] as ExpressionStatement;

            Assert.NotNull(expression);

            var call = expression.Expression as CallExpression;

            Assert.NotNull(call);
            TestIdentifier(call.Function, test.identifier);
            Assert.That(call.Arguments.Length, Is.EqualTo(test.expected.Length));

            for (var i = 0; i < test.expected.Length; i++)
            {
                Assert.AreEqual(test.expected[i], call.Arguments[i].String);
            }
        }
    }

    [Test]
    public void ParseStringLiteralTest()
    {
        var input = "\"hello world\";";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        var statement = program.statements[0] as ExpressionStatement;
        var literal = statement.Expression as StringLiteral;

        Assert.NotNull(literal);
        Assert.AreEqual("hello world", literal.Value);
    }

    [Test]
    public void ParseEmptyArrayLiteralTest()
    {
        var input = "[]";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        var statement = program.statements[0] as ExpressionStatement;
        var array = statement.Expression as ArrayLiteral;

        Assert.NotNull(array);
        Assert.That(array.Elements.Length, Is.EqualTo(0));
    }

    [Test]
    public void ParseArrayLiteralTest()
    {
        var input = "[1, 2 * 2, 3 + 3]";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        var statement = program.statements[0] as ExpressionStatement;
        var array = statement.Expression as ArrayLiteral;

        Assert.NotNull(array);
        Assert.That(array.Elements.Length, Is.EqualTo(3));
        TestIntegerLiteral(array.Elements[0], 1);
        TestInfixExpression(array.Elements[1], 2, "*", 2);
        TestInfixExpression(array.Elements[2], 3, "+", 3);
    }

    [Test]
    public void ParseIndexExpressionTest()
    {
        var input = "myArray[1 + 1]";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        var statement = program.statements[0] as ExpressionStatement;
        var expression = statement.Expression as IndexExpresssion;

        Assert.NotNull(expression);
        TestIdentifier(expression.Left, "myArray");
        TestInfixExpression(expression.Index, 1, "+", 1);
    }

    [Test]
    public void ParseEmptyHashLiteralTest()
    {
        var input = "{}";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        var statement = program.statements[0] as ExpressionStatement;
        var hash = statement.Expression as HashLiteral;

        Assert.NotNull(hash);
        Assert.That(hash.Pairs.Count, Is.EqualTo(0));
    }

    [Test]
    public void ParseHashLiteralTest()
    {
        var input = "{\"one\" : 1, \"two\" : 2, \"three\" : 3}";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        var statement = program.statements[0] as ExpressionStatement;
        var hash = statement.Expression as HashLiteral;
        var expected = new Dictionary<string, long>()
        {
            {"one", 1},
            {"two", 2},
            {"three", 3},
        };

        Assert.NotNull(hash);
        Assert.That(hash.Pairs.Count, Is.EqualTo(expected.Count));

        foreach (var kvp in hash.Pairs)
        {
            var literal = kvp.Key as StringLiteral;

            Assert.NotNull(literal);
            TestIntegerLiteral(kvp.Value, expected[literal.Value]);
        }
    }

    [Test]
    public void ParseBooleanHashLiteralTest()
    {
        var input = "{true : 1, false : 2}";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        var statement = program.statements[0] as ExpressionStatement;
        var hash = statement.Expression as HashLiteral;
        var expected = new Dictionary<string, long>()
        {
            {"true", 1},
            {"false", 2},
        };

        Assert.NotNull(hash);
        Assert.That(hash.Pairs.Count, Is.EqualTo(expected.Count));

        foreach (var kvp in hash.Pairs)
        {
            var boolean = kvp.Key as Boolean;

            Assert.NotNull(boolean);
            TestIntegerLiteral(kvp.Value, expected[boolean.String]);
        }
    }

    [Test]
    public void ParseIntegerHashLiteralTest()
    {
        var input = "{1 : 1, 2 : 2, 3 : 3}";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        var statement = program.statements[0] as ExpressionStatement;
        var hash = statement.Expression as HashLiteral;
        var expected = new Dictionary<string, long>()
        {
            {"1", 1},
            {"2", 2},
            {"3", 3},
        };

        Assert.NotNull(hash);
        Assert.That(hash.Pairs.Count, Is.EqualTo(expected.Count));

        foreach (var kvp in hash.Pairs)
        {
            var integer = kvp.Key as IntegerLiteral;

            Assert.NotNull(integer);
            TestIntegerLiteral(kvp.Value, expected[integer.String]);
        }
    }

    [Test]
    public void ParseHashLiteralWithExpressionTest()
    {
        var input = "{\"one\" : 0 + 1, \"two\" : 10 - 8, \"three\" : 15 / 5}";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        var statement = program.statements[0] as ExpressionStatement;
        var hash = statement.Expression as HashLiteral;
        var expected = new Dictionary<string, System.Action<Expression>>()
        {
            {"one", (expression) => {
                TestInfixExpression(expression, 0, "+", 1);
            }},
            {"two", (expression) => {
                TestInfixExpression(expression, 10, "-", 8);
            }},
            {"three", (expression) => {
                TestInfixExpression(expression, 15, "/", 5);
            }},
        };

        Assert.NotNull(hash);
        Assert.That(hash.Pairs.Count, Is.EqualTo(3));

        foreach (var kvp in hash.Pairs)
        {
            var literal = kvp.Key as StringLiteral;

            Assert.NotNull(literal);
            expected[literal.String](kvp.Value);
        }
    }

    private void TestLetStatement(Statement statement, string name)
    {
        var let = statement as LetStatement;

        Assert.AreEqual(statement.TokenLiteral, "let");
        Assert.NotNull(let);
        Assert.AreEqual(let.Name.Value, name);
        Assert.AreEqual(let.Name.TokenLiteral, name);
    }

    private void TestInfixExpression(Expression expression, object left, string op, object right)
    {
        var infixExpression = expression as InfixExpression;

        Assert.NotNull(infixExpression);
        Assert.AreEqual(infixExpression.Operator, op);
        TestLiteralExpression(infixExpression.Left, left);
        TestLiteralExpression(infixExpression.Right, right);
    }

    private void TestLiteralExpression(Expression expression, object value)
    {
        if (value is long)
        {
            TestIntegerLiteral(expression, (long)value);
        }
        else if (value is int)
        {
            TestIntegerLiteral(expression, (int)value);
        }
        else if (value is string)
        {
            TestIdentifier(expression, value as string);
        }
        else if (value is bool)
        {
            TestBool(expression, (bool)value);
        }
    }

    private void TestIntegerLiteral(Expression expression, long value)
    {
        var literal = expression as IntegerLiteral;

        Assert.NotNull(literal);
        Assert.AreEqual(literal.Value, value);
        Assert.AreEqual(literal.TokenLiteral, value.ToString());
    }

    private void TestIdentifier(Expression expression, string value)
    {
        var identifier = expression as Identifier;

        Assert.NotNull(identifier);
        Assert.AreEqual(identifier.Value, value);
        Assert.AreEqual(identifier.TokenLiteral, value);
    }

    private void TestBool(Expression expression, bool value)
    {
        var boolean = expression as Boolean;

        Assert.NotNull(boolean);
        Assert.AreEqual(boolean.Value, value);
        Assert.AreEqual(boolean.TokenLiteral, value.ToString().ToLower());
    }
}
