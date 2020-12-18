using System.IO;

namespace Macaca
{
    public class Lexer
    {
        private StringReader reader;
        private string line;
        private int maxLength;
        private int spaces;
        private int lineNumber;
        private int currentIndex;

        public Lexer(string input)
        {
            this.reader = new StringReader(input);
        }

        private void ReadLine()
        {
            while ((this.line = this.reader.ReadLine()) != null)
            {
                this.lineNumber++;

                if ((this.maxLength = this.line.Length) > 0)
                {
                    this.currentIndex = 0;
                    break;
                }
            }
        }

        private char ReadChar()
        {
            if (this.currentIndex < this.maxLength)
            {
                return this.line[this.currentIndex++];
            }
            else
            {
                return ' ';
            }
        }

        private char PeekChar()
        {
            if (this.currentIndex < this.maxLength)
            {
                return this.line[this.currentIndex];
            }
            else
            {
                return '\0';
            }
        }

        private bool IsLetter(char c)
        {
            return 'a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || c == '_';
        }

        private bool IsDigit(char c)
        {
            return '0' <= c && c <= '9';
        }

        private string ReadIdentifier()
        {
            var index = this.currentIndex - 1;

            while (this.currentIndex < this.maxLength && IsLetter(this.line[this.currentIndex]))
            {
                this.currentIndex++;
            }

            return this.line.Substring(index, this.currentIndex - index);
        }

        private string ReadNumber()
        {
            var index = this.currentIndex - 1;

            while (this.currentIndex < this.maxLength && IsDigit(this.line[this.currentIndex]))
            {
                this.currentIndex++;
            }

            return this.line.Substring(index, this.currentIndex - index);
        }

        public Token NextToken()
        {
            char nextChar;

            while (true)
            {
                if (this.currentIndex == this.maxLength)
                {
                    this.ReadLine();

                    if (this.line == null)
                    {
                        return new Token()
                        {
                            Type = TokenType.EOF,
                            Literal = ""
                        };
                    }
                }

                nextChar = this.ReadChar();

                if (nextChar == ' ')
                {
                    this.spaces++;
                }
                else if (nextChar == '\t')
                {
                    this.spaces += 5;
                }
                else
                {
                    break;
                }
            }

            switch (nextChar)
            {
                case '=':
                    if (this.PeekChar() == '=')
                    {
                        this.currentIndex++;
                        return new Token()
                        {
                            Type = TokenType.EQ,
                            Literal = "=="
                        };
                    }
                    else
                    {
                        return new Token()
                        {
                            Type = TokenType.ASSIGN,
                            Literal = nextChar.ToString()
                        };
                    }
                case '+':
                    return new Token()
                    {
                        Type = TokenType.PLUS,
                        Literal = nextChar.ToString()
                    };
                case '-':
                    return new Token()
                    {
                        Type = TokenType.MINUS,
                        Literal = nextChar.ToString()
                    };
                case '!':
                    if (this.PeekChar() == '=')
                    {
                        this.currentIndex++;
                        return new Token()
                        {
                            Type = TokenType.NOT_EQ,
                            Literal = "!="
                        };
                    }
                    else
                    {
                        return new Token()
                        {
                            Type = TokenType.BANG,
                            Literal = nextChar.ToString()
                        };
                    }
                case '/':
                    return new Token()
                    {
                        Type = TokenType.SLASH,
                        Literal = nextChar.ToString()
                    };
                case '*':
                    return new Token()
                    {
                        Type = TokenType.ASTERISK,
                        Literal = nextChar.ToString()
                    };
                case '<':
                    return new Token()
                    {
                        Type = TokenType.LT,
                        Literal = nextChar.ToString()
                    };
                case '>':
                    return new Token()
                    {
                        Type = TokenType.GT,
                        Literal = nextChar.ToString()
                    };
                case ';':
                    return new Token()
                    {
                        Type = TokenType.SEMICOLON,
                        Literal = nextChar.ToString()
                    };
                case ',':
                    return new Token()
                    {
                        Type = TokenType.COMMA,
                        Literal = nextChar.ToString()
                    };
                case '{':
                    return new Token()
                    {
                        Type = TokenType.LBRACE,
                        Literal = nextChar.ToString()
                    };
                case '}':
                    return new Token()
                    {
                        Type = TokenType.RBRACE,
                        Literal = nextChar.ToString()
                    };
                case '(':
                    return new Token()
                    {
                        Type = TokenType.LPAREN,
                        Literal = nextChar.ToString()
                    };
                case ')':
                    return new Token()
                    {
                        Type = TokenType.RPAREN,
                        Literal = nextChar.ToString()
                    };
                default:
                    if (this.IsLetter(nextChar))
                    {
                        var literal = this.ReadIdentifier();
                        return new Token()
                        {
                            Type = Token.LookupIdent(literal),
                            Literal = literal
                        };
                    }
                    else if (this.IsDigit(nextChar))
                    {
                        var num = this.ReadNumber();
                        return new Token()
                        {
                            Type = TokenType.INT,
                            Literal = num
                        };
                    }
                    else
                    {
                        return new Token()
                        {
                            Type = TokenType.ILLEGAL,
                            Literal = nextChar.ToString()
                        };
                    }
            }
        }
    }
}
