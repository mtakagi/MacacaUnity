using System.Collections.Generic;

namespace Macaca
{
    public enum TokenType
    {
        ILLEGAL,
        EOF,
        IDENT,
        INT,
        STRING,
        ASSIGN,
        PLUS,
        MINUS,
        BANG,
        ASTERISK,
        SLASH,
        LT,
        GT,
        EQ,
        NOT_EQ,
        COMMA,
        COLON,
        SEMICOLON,
        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,
        LBRACKET,
        RBRACKET,
        FUNCTION,
        LET,
        TRUE,
        FALSE,
        IF,
        ELSE,
        RETURN,
    }

    public class Token
    {
        private static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
    {
        {"fn", TokenType.FUNCTION},
        {"let", TokenType.LET},
        {"true", TokenType.TRUE},
        {"false", TokenType.FALSE},
        {"if", TokenType.IF},
        {"else", TokenType.ELSE},
        {"return", TokenType.RETURN}
    };

        public TokenType Type { get; set; }
        public string Literal { get; set; }

        public static TokenType LookupIdent(string ident)
        {
            TokenType type = TokenType.ILLEGAL;
            if (keywords.TryGetValue(ident, out type))
            {
                return type;
            }

            return TokenType.IDENT;
        }
    }
}