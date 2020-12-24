using System.Collections.Generic;
using NUnit.Framework;

public class EvaluatorTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void EvaluateIntegerExpressionTest()
    {
        var tests = new[]{
            new { input = "5", expected = 5},
            new { input = "10", expected = 10},
            new { input = "-5", expected = -5},
            new { input = "-10", expected = -10},
            new { input = "5 + 5 + 5 + 5 - 10", expected = 10},
            new { input = "2 * 2 * 2 * 2 * 2", expected = 32},
            new { input = "-50 + 100 + -50", expected = 0},
            new { input = "5 * 2 + 10", expected = 20},
            new { input = "5 + 2 * 10", expected = 25},
            new { input = "20 + 2 * -10", expected = 0},
            new { input = "50 / 2 * 2 + 10", expected = 60},
            new { input = "2 * (5 + 10)", expected = 30},
            new { input = "3 * 3 * 3 + 10", expected = 37},
            new { input = "3 * (3 * 3) + 10", expected = 37},
            new { input = "(5 + 10 * 2 + 15 / 3) * 2 + -10", expected = 50},
        };

        foreach (var test in tests)
        {
            var result = TestEvaluate(test.input);
            TestIntegerValue(result, test.expected);
        }
    }

    [Test]
    public void EvaluateBoolExpressionTest()
    {
        var tests = new[] {
            new { input = "true", expected =  true},
            new { input = "false", expected =  false},
            new { input = "1 < 2", expected =  true},
            new { input = "1 > 2", expected =  false},
            new { input = "1 < 1", expected =  false},
            new { input = "1 > 1", expected =  false},
            new { input = "1 == 1", expected =  true},
            new { input = "1 != 1", expected =  false},
            new { input = "1 == 2", expected =  false},
            new { input = "1 != 2", expected =  true},
            new { input = "true == true", expected =  true},
            new { input = "false == false", expected =  true},
            new { input = "true == false", expected =  false},
            new { input = "true != false", expected =  true},
            new { input = "false != true", expected =  true},
            new { input = "(1 < 2) == true", expected =  true},
            new { input = "(1 < 2) == false", expected =  false},
            new { input = "(1 > 2) == true", expected =  false},
            new { input = "(1 > 2) == false", expected =  true},
        };

        foreach (var test in tests)
        {
            var result = TestEvaluate(test.input);
            TestBoolValue(result, test.expected);
        }
    }

    [Test]
    public void EvaluateBangOperationTest()
    {
        var tests = new[] {
            new { input = "!true", expected = false},
            new { input = "!false", expected = true},
            new { input = "!5", expected = false},
            new { input = "!!true", expected = true},
            new { input = "!!false", expected = false},
            new { input = "!!5", expected = true},
        };

        foreach (var test in tests)
        {
            var result = TestEvaluate(test.input);
            TestBoolValue(result, test.expected);
        }
    }

    [Test]
    public void EvaluateIfElseExpressionTest()
    {
        var tests = new[] {
            new { input = "if (true) { 10 }", expected = (object)10},
            new { input = "if (false) { 10 }", expected = (object)null},
            new { input = "if (1) { 10 }", expected = (object)10},
            new { input = "if (1 < 2) { 10 }", expected = (object)10},
            new { input = "if (1 > 2) { 10 }", expected = (object)null},
            new { input = "if (1 > 2) { 10 } else { 20 }", expected = (object)20},
            new { input = "if (1 < 2) { 10 } else { 20 }", expected = (object)10},
        };

        foreach (var test in tests)
        {
            var result = TestEvaluate(test.input);
            if (test.expected is int)
            {
                TestIntegerValue(result, (int)test.expected);
            }
            else
            {
                Assert.AreEqual(Macaca.Evaluator.Null, result);
            }
        }
    }

    [Test]
    public void EvaluateReturnStatementTest()
    {
        var tests = new[] {
            new { input = "return 10;", expected = 10},
            new { input = "return 10; 9;", expected = 10},
            new { input = "return 2 * 5; 9;", expected = 10},
            new { input = "9; return 2 * 5; 9;", expected = 10},
            new { input = "if (10 > 1) { return 10; }", expected = 10},
            new { input = "if (10 > 1) { if (10 > 1) { return 10; } return 1; }", expected = 10 },
            new { input = "let f = fn(x) { return x; x + 10; }; f(10);", expected = 10 },
            new { input = "let f = fn(x) { let result = x + 10; return result; return 10; }; f(10);", expected = 20 },
        };

        foreach (var test in tests)
        {
            var result = TestEvaluate(test.input);
            TestIntegerValue(result, test.expected);
        }
    }

    [Test]
    public void EvaluateErrorHandlingTest()
    {
        var tests = new[] {
            new { input = "5 + true;", expected = "type mismatch: INTEGER + BOOLEAN" },
            new { input = "5 + true; 5;", expected = "type mismatch: INTEGER + BOOLEAN" },
            new { input = "-true", expected = "unknown operator: -BOOLEAN" },
            new { input = "true + false;", expected = "unknown operator: BOOLEAN + BOOLEAN" },
            new { input = "\"Hello\" - \"World\"", expected = "unknown operator: STRING - STRING" },
            new { input = "true + false + true + false;", expected = "unknown operator: BOOLEAN + BOOLEAN" },
            new { input = "5; true + false; 5", expected = "unknown operator: BOOLEAN + BOOLEAN" },
            new { input = "if (10 > 1) { true + false; }", expected = "unknown operator: BOOLEAN + BOOLEAN" },
            new { input = "if (10 > 1) { if (10 > 1) { return true + false; } return 1; }" , expected = "unknown operator: BOOLEAN + BOOLEAN" },
            new { input = "foobar", expected = "identifier not found: foobar" },
            new { input = "{\"name\": \"Monkey\"}[fn(x) { x }];", expected = "unusable as hash key: FUNCTION" },
            new { input = "999[1]", expected = "index operator not supported: INTEGER" },
        };

        foreach (var test in tests)
        {
            var result = TestEvaluate(test.input);
            var error = result as Macaca.Error;

            Assert.NotNull(error);
            Assert.AreEqual(test.expected.ToLower(), error.Message.ToLower());
        }
    }

    [Test]
    public void EvaluateLetStatementTest()
    {
        var tests = new[]
        {
            new { input = "let a = 5; a;", expected = 5},
            new { input = "let a = 5 * 5; a;", expected = 25},
            new { input = "let a = 5; let b = a; b;", expected = 5},
            new { input = "let a = 5; let b = a; let c = a + b + 5; c;", expected = 15},
        };

        foreach (var test in tests)
        {
            var result = TestEvaluate(test.input);
            TestIntegerValue(result, test.expected);
        }
    }

    [Test]
    public void EvaluateFunctionObjectTest()
    {
        var input = "fn(x) { x + 2; };";
        var result = TestEvaluate(input);
        var function = result as Macaca.Function;

        Assert.NotNull(function);
        Assert.That(function.Parameters.Length, Is.EqualTo(1));
        Assert.That(function.Parameters[0].String, Is.EqualTo("x"));
        Assert.That(function.Body.String, Is.EqualTo("(x + 2)"));
    }

    [Test]
    public void EvaluateFunctionApplication()
    {
        var tests = new[] {
            new { input = "let identity = fn(x) { x; }; identity(5);", expected = 5},
            new { input = "let identity = fn(x) { return x; }; identity(5);", expected = 5},
            new { input = "let double = fn(x) { x * 2; }; double(5);", expected = 10},
            new { input = "let add = fn(x, y) { x + y; }; add(5, 5);", expected = 10},
            new { input = "let add = fn(x, y) { x + y; }; add(5 + 5, add(5, 5));", expected = 20},
            new { input = "fn(x) { x; }(5)", expected = 5},
        };

        foreach (var test in tests)
        {
            var result = TestEvaluate(test.input);
            TestIntegerValue(result, test.expected);
        }
    }

    [Test]
    public void EvalutateEnclosingEnvironments()
    {
        var input = @"let first = 10;
let second = 10;
let third = 10;

let ourFunction = fn(first) {
  let second = 20;

  first + second + third;
};

ourFunction(20) + first + second;";
        var result = TestEvaluate(input);
        TestIntegerValue(result, 70);
    }

    [Test]
    public void EvaluateClosureTest()
    {
        var input = @"let newAdder = fn(x) {
  fn(y) { x + y };
};
let addTwo = newAdder(2);
addTwo(2);";
        var result = TestEvaluate(input);
        TestIntegerValue(result, 4);
    }

    [Test]
    public void EvaluateStringLiteralTest()
    {
        var input = "\"Hello World!\"";
        var result = TestEvaluate(input);
        TestStringValue(result, "Hello World!");
    }

    [Test]
    public void EvaluateStringConcatTest()
    {
        var input = "\"Hello\" + \" \" + \"World!\"";
        var result = TestEvaluate(input);
        TestStringValue(result, "Hello World!");
    }

    [Test]
    public void EvaluateBuiltinFunctionTest()
    {
        var tests = new[] {
            new { input = "len(\"\")", expected = (object)0},
            new { input = "len(\"four\")", expected = (object)4},
            new { input = "len(\"hello world\")", expected = (object)11},
            new { input = "len(1)", expected = (object)"argument to `len` not supported, got INTEGER"},
            new { input = "len(\"one\", \"two\")", expected = (object)"wrong number of arguments. got=2, want=1"},
            new { input = "len([1, 2, 3])", expected = (object)3},
            new { input = "len([])", expected = (object)0},
            new { input = "puts(\"hello\", \"world!\")", expected = (object)null},
            new { input = "first([1, 2, 3])", expected = (object)1},
            new { input = "first([])", expected = (object)null},
            new { input = "first(1)", expected = (object)"argument to `first` must be ARRAY, got INTEGER"},
            new { input = "last([1, 2, 3])", expected = (object)3},
            new { input = "last([])", expected = (object)null},
            new { input = "last(1)", expected = (object)"argument to `last` must be ARRAY, got INTEGER"},
            new { input = "rest([1, 2, 3])", expected = (object)new int[]{2, 3}},
            new { input = "rest([])", expected = (object)null },
            new { input = "push([], 1)", expected = (object)new int[]{ 1 }},
            new { input = "push(1, 1)", expected = (object)"argument to `push` must be ARRAY, got INTEGER" },
        };

        foreach (var test in tests)
        {
            var evaluated = TestEvaluate(test.input);

            switch (test.expected)
            {
                case int i:
                    TestIntegerValue(evaluated, i);
                    break;
                case string str:
                    var error = evaluated as Macaca.Error;
                    Assert.NotNull(error);
                    Assert.AreEqual(str.ToLower(), error.Message.ToLower());
                    break;
                case int[] array:
                    TestIntArrayValue(evaluated, array);
                    break;
                default:
                    if (test.expected == null)
                    {
                        Assert.AreEqual(Macaca.Evaluator.Null, evaluated);
                    }
                    break;
            }
        }
    }

    [Test]
    public void EvaluateArrayLitralTest()
    {
        var input = "[1, 2 * 2, 3 + 3]";
        var result = TestEvaluate(input);
        TestIntArrayValue(result, new int[] { 1, 4, 6 });
    }

    [Test]
    public void EvaluateArrayIndexTest()
    {
        var tests = new[] {
            new { input = "[1, 2, 3][0]", expected = (object)1 },
            new { input = "[1, 2, 3][1]", expected = (object)2 },
            new { input = "[1, 2, 3][2]", expected = (object)3 },
            new { input = "let i = 0; [1][i];", expected = (object)1 },
            new { input = "[1, 2, 3][1 + 1];", expected = (object)3 },
            new { input = "let myArray = [1, 2, 3]; myArray[2];", expected = (object)3 },
            new { input = "let myArray = [1, 2, 3]; myArray[0] + myArray[1] + myArray[2];", expected = (object)6 },
            new { input = "let myArray = [1, 2, 3]; let i = myArray[0]; myArray[i]", expected = (object)2 },
            new { input = "[1, 2, 3][3]", expected = (object)null },
            new { input = "[1, 2, 3][-1]", expected = (object)null},
        };

        foreach (var test in tests)
        {
            var evaluated = TestEvaluate(test.input);

            switch (test.expected)
            {
                case int i:
                    TestIntegerValue(evaluated, i);
                    break;
                default:
                    if (test.expected == null)
                    {
                        Assert.AreEqual(Macaca.Evaluator.Null, evaluated);
                    }
                    break;
            }
        }
    }

    [Test]
    public void EvaluateHashLiteralTest()
    {
        var input = @"let two = ""two"";
    {
        ""one"": 10 - 9,

        two: 1 + 1,
		""thr"" + ""ee"": 6 / 2,

        4: 4,
		true: 5,
		false: 6
    }";
        var result = TestEvaluate(input) as Macaca.Hash;
        var expected = new Dictionary<Macaca.HashKey, int>()
        {
            { new Macaca.String() {Value = "one" }.HashKey(), 1},
            { new Macaca.String() {Value = "two" }.HashKey(), 2},
            { new Macaca.String() {Value = "three" }.HashKey(), 3},
            { new Macaca.Integer() {Value = 4 }.HashKey(), 4},
            { Macaca.Evaluator.True.HashKey(), 5},
            { Macaca.Evaluator.False.HashKey(), 6}
        };
        Assert.NotNull(result);
        Assert.That(result.Pairs.Count, Is.EqualTo(expected.Count));

        foreach (var kvp in expected)
        {
            var pair = result.Pairs[kvp.Key];

            TestIntegerValue(pair.Value, kvp.Value);
        }
    }

    [Test]
    public void EvaluateHashIndexTest()
    {
        var tests = new[] {
            new { input = "{\"foo\": 5}[\"foo\"]", expected = (object)5 },
            new { input = "{\"foo\": 5}[\"bar\"]", expected = (object)null },
            new { input = "let key = \"foo\"; {\"foo\": 5}[key]", expected = (object)5 },
            new { input = "{}[\"foo\"]", expected = (object)null },
            new { input = "{5: 5}[5]", expected = (object)5 },
            new { input = "{true: 5}[true]", expected = (object)5 },
            new { input = "{false: 5}[false]", expected = (object)5 },
        };

        foreach (var test in tests)
        {
            var evaluated = TestEvaluate(test.input);

            switch (test.expected)
            {
                case int i:
                    TestIntegerValue(evaluated, i);
                    break;
                default:
                    if (test.expected == null)
                    {
                        Assert.AreEqual(Macaca.Evaluator.Null, evaluated);
                    }
                    break;
            }
        }
    }

    private Macaca.Object TestEvaluate(string input)
    {
        var lexer = new Macaca.Lexer(input);
        var parser = new Macaca.Parser(lexer);
        var evaluator = new Macaca.Evaluator(parser.ParseProgram());

        return evaluator.Eval();
    }

    private void TestIntegerValue(Macaca.Object obj, long expected)
    {
        var result = obj as Macaca.Integer;

        Assert.NotNull(result);
        Assert.That(result.Value, Is.EqualTo(expected));
    }

    private void TestBoolValue(Macaca.Object obj, bool expected)
    {
        var result = obj as Macaca.Bool;

        Assert.NotNull(result);
        Assert.That(result.Value, Is.EqualTo(expected));
    }

    private void TestStringValue(Macaca.Object obj, string expected)
    {
        var result = obj as Macaca.String;

        Assert.NotNull(result);
        Assert.That(result.Value, Is.EqualTo(expected));
    }

    private void TestIntArrayValue(Macaca.Object obj, int[] expected)
    {
        var result = obj as Macaca.Array;

        Assert.NotNull(result);
        Assert.That(result.Elements.Length, Is.EqualTo(expected.Length));
        for (var i = 0; i < result.Elements.Length; i++)
        {
            TestIntegerValue(result.Elements[i], expected[i]);
        }
    }
}
