using NUnit.Framework;
using Macaca;

public class LexerTest
{
    private static string input = @"
let five = 5;
let ten = 10;

let add = fn(x, y) {
  x + y;
};

let result = add(five, ten);
!-/*5;
5 < 10 > 5;

if (5 < 10) {
	return true;
} else {
	return false;
}

10 == 10;
10 != 9;
";

    private static Token[] tokens = new[] {
        new Token() {Type = TokenType.LET, Literal = "let"},
        new Token() {Type = TokenType.IDENT, Literal = "five"},
        new Token() {Type = TokenType.ASSIGN, Literal = "="},
        new Token() {Type = TokenType.INT, Literal = "5"},
        new Token() {Type = TokenType.SEMICOLON, Literal = ";"},
        new Token() {Type = TokenType.LET, Literal = "let"},
        new Token() {Type = TokenType.IDENT, Literal = "ten"},
        new Token() {Type = TokenType.ASSIGN, Literal = "="},
        new Token() {Type = TokenType.INT, Literal = "10"},
        new Token() {Type = TokenType.SEMICOLON, Literal = ";"},
        new Token() {Type = TokenType.LET, Literal = "let"},
        new Token() {Type = TokenType.IDENT, Literal = "add"},
        new Token() {Type = TokenType.ASSIGN, Literal = "="},
        new Token() {Type = TokenType.FUNCTION, Literal = "fn"},
        new Token() {Type = TokenType.LPAREN, Literal = "("},
        new Token() {Type = TokenType.IDENT, Literal = "x"},
        new Token() {Type = TokenType.COMMA, Literal = ","},
        new Token() {Type = TokenType.IDENT, Literal = "y"},
        new Token() {Type = TokenType.RPAREN, Literal = ")"},
        new Token() {Type = TokenType.LBRACE, Literal = "{"},
        new Token() {Type = TokenType.IDENT, Literal = "x"},
        new Token() {Type = TokenType.PLUS, Literal = "+"},
        new Token() {Type = TokenType.IDENT, Literal = "y"},
        new Token() {Type = TokenType.SEMICOLON, Literal = ";"},
        new Token() {Type = TokenType.RBRACE, Literal = "}"},
        new Token() {Type = TokenType.SEMICOLON, Literal = ";"},
        new Token() {Type = TokenType.LET, Literal = "let"},
        new Token() {Type = TokenType.IDENT, Literal = "result"},
        new Token() {Type = TokenType.ASSIGN, Literal = "="},
        new Token() {Type = TokenType.IDENT, Literal = "add"},
        new Token() {Type = TokenType.LPAREN, Literal = "("},
        new Token() {Type = TokenType.IDENT, Literal = "five"},
        new Token() {Type = TokenType.COMMA, Literal = ","},
        new Token() {Type = TokenType.IDENT, Literal = "ten"},
        new Token() {Type = TokenType.RPAREN, Literal = ")"},
        new Token() {Type = TokenType.SEMICOLON, Literal = ";"},
        new Token() {Type = TokenType.BANG, Literal = "!"},
        new Token() {Type = TokenType.MINUS, Literal = "-"},
        new Token() {Type = TokenType.SLASH, Literal = "/"},
        new Token() {Type = TokenType.ASTERISK, Literal = "*"},
        new Token() {Type = TokenType.INT, Literal = "5"},
        new Token() {Type = TokenType.SEMICOLON, Literal = ";"},
        new Token() {Type = TokenType.INT, Literal = "5"},
        new Token() {Type = TokenType.LT, Literal = "<"},
        new Token() {Type = TokenType.INT, Literal = "10"},
        new Token() {Type = TokenType.GT, Literal = ">"},
        new Token() {Type = TokenType.INT, Literal = "5"},
        new Token() {Type = TokenType.SEMICOLON, Literal = ";"},
        new Token() {Type = TokenType.IF, Literal = "if"},
        new Token() {Type = TokenType.LPAREN, Literal = "("},
        new Token() {Type = TokenType.INT, Literal = "5"},
        new Token() {Type = TokenType.LT, Literal = "<"},
        new Token() {Type = TokenType.INT, Literal = "10"},
        new Token() {Type = TokenType.RPAREN, Literal = ")"},
        new Token() {Type = TokenType.LBRACE, Literal = "{"},
        new Token() {Type = TokenType.RETURN, Literal = "return"},
        new Token() {Type = TokenType.TRUE, Literal = "true"},
        new Token() {Type = TokenType.SEMICOLON, Literal = ";"},
        new Token() {Type = TokenType.RBRACE, Literal = "}"},
        new Token() {Type = TokenType.ELSE, Literal = "else"},
        new Token() {Type = TokenType.LBRACE, Literal = "{"},
        new Token() {Type = TokenType.RETURN, Literal = "return"},
        new Token() {Type = TokenType.FALSE, Literal = "false"},
        new Token() {Type = TokenType.SEMICOLON, Literal = ";"},
        new Token() {Type = TokenType.RBRACE, Literal = "}"},
        new Token() {Type = TokenType.INT, Literal = "10"},
        new Token() {Type = TokenType.EQ, Literal = "=="},
        new Token() {Type = TokenType.INT, Literal = "10"},
        new Token() {Type = TokenType.SEMICOLON, Literal = ";"},
        new Token() {Type = TokenType.INT, Literal = "10"},
        new Token() {Type = TokenType.NOT_EQ, Literal = "!="},
        new Token() {Type = TokenType.INT, Literal = "9"},
        new Token() {Type = TokenType.SEMICOLON, Literal = ";"},
        new Token() {Type = TokenType.EOF, Literal = ""},
    };

    [Test]
    public void LexerTestPass()
    {
        var lexer = new Lexer(input);

        for (var i = 0; i < tokens.Length; i++)
        {
            var expect = tokens[i];
            var token = lexer.NextToken();

            Assert.AreEqual(expect.Type, token.Type, $"Type: {token.Type}, Literal: {token.Literal}");
            Assert.AreEqual(expect.Literal, token.Literal);
        }
    }
}
