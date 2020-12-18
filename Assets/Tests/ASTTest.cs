using NUnit.Framework;
using Macaca;

public class ASTTest
{
    [Test]
    public void ASTTestPasses()
    {
        var program = new Program()
        {
            statements = new[] { new LetStatement() {
                Token = new Token() {Type = TokenType.LET, Literal = "let"},
                Name = new Identifier() {
                    Token = new Token() { Type = TokenType.IDENT, Literal = "myVar"},
                    Value = "myVar",
                },
                Value = new Identifier() {
                    Token = new Token() {Type = TokenType.IDENT, Literal = "anotherVar"},
                    Value = "anotherVar",
                },
            }}
        };

        Assert.AreEqual("let myVar = anotherVar;", program.String, $"program.String() wrong. got={program.String}");
    }
}
