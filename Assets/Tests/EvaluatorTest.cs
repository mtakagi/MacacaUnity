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
            new { input = "true + false + true + false;", expected = "unknown operator: BOOLEAN + BOOLEAN" },
            new { input = "5; true + false; 5", expected = "unknown operator: BOOLEAN + BOOLEAN" },
            new { input = "if (10 > 1) { true + false; }", expected = "unknown operator: BOOLEAN + BOOLEAN" },
            new { input = "if (10 > 1) { if (10 > 1) { return true + false; } return 1; }" , expected = "unknown operator: BOOLEAN + BOOLEAN" },
            new { input = "foobar", expected = "identifier not found: foobar" },
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
}
