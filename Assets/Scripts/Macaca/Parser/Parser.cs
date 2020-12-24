using System;
using System.Collections.Generic;

namespace Macaca
{
    using PrefixParser = System.Func<Expression>;
    using InfixParser = System.Func<Expression, Expression>;

    public enum Precedence : int
    {
        LOWEST = 1,
        EQUALS,
        LESSGREATER,
        SUM,
        PRODUCT,
        PREFIX,
        CALL,
        INDEX
    }

    public class Parser
    {
        public static Dictionary<TokenType, Precedence> precedences = new Dictionary<TokenType, Precedence>() {
            {TokenType.EQ , Precedence.EQUALS },
            {TokenType.NOT_EQ, Precedence.EQUALS },
            {TokenType.LT, Precedence.LESSGREATER },
            {TokenType.GT, Precedence.LESSGREATER },
            {TokenType.PLUS, Precedence.SUM },
            {TokenType.MINUS, Precedence.SUM },
            {TokenType.SLASH, Precedence.PRODUCT },
            {TokenType.ASTERISK, Precedence.PRODUCT },
            {TokenType.LPAREN, Precedence.CALL },
            {TokenType.LBRACKET, Precedence.INDEX }
        };

        private Lexer lexer;
        private Token currentToken;
        private Token peekToken;

        private Dictionary<TokenType, PrefixParser> prefixParseFunctions = new Dictionary<TokenType, PrefixParser>();
        private Dictionary<TokenType, InfixParser> infixParseFunctions = new Dictionary<TokenType, InfixParser>();

        private string[] errors;

        public Parser(Lexer lexer)
        {
            this.lexer = lexer;

            RegisterPrefix(TokenType.IDENT, ParseIdentifier);
            RegisterPrefix(TokenType.INT, ParseIntegerLitral);
            RegisterPrefix(TokenType.STRING, ParseStringLiteral);
            RegisterPrefix(TokenType.BANG, ParsePrefixExpression);
            RegisterPrefix(TokenType.MINUS, ParsePrefixExpression);
            RegisterPrefix(TokenType.TRUE, ParseBoolean);
            RegisterPrefix(TokenType.FALSE, ParseBoolean);
            RegisterPrefix(TokenType.LPAREN, ParseGroupedExpression);
            RegisterPrefix(TokenType.IF, ParseIfExpression);
            RegisterPrefix(TokenType.FUNCTION, ParseFunctionLiteral);
            RegisterPrefix(TokenType.LBRACKET, ParseArrayLiteral);
            RegisterPrefix(TokenType.LBRACE, ParseHashLiteral);

            RegisterInfix(TokenType.PLUS, ParseInfixExpression);
            RegisterInfix(TokenType.MINUS, ParseInfixExpression);
            RegisterInfix(TokenType.SLASH, ParseInfixExpression);
            RegisterInfix(TokenType.ASTERISK, ParseInfixExpression);
            RegisterInfix(TokenType.EQ, ParseInfixExpression);
            RegisterInfix(TokenType.NOT_EQ, ParseInfixExpression);
            RegisterInfix(TokenType.LT, ParseInfixExpression);
            RegisterInfix(TokenType.GT, ParseInfixExpression);

            RegisterInfix(TokenType.LPAREN, ParseCallExpression);
            RegisterInfix(TokenType.LBRACKET, ParseIndexExpression);

            this.NextToken();
            this.NextToken();
        }

        public void NextToken()
        {
            this.currentToken = this.peekToken;
            this.peekToken = this.lexer.NextToken();
        }

        public bool IsCurrentTokenTypeEqualTo(TokenType type) => this.currentToken.Type == type;

        public bool IsPeekTokenTypeEqualTo(TokenType type) => this.peekToken.Type == type;

        public bool Expect(TokenType type)
        {
            if (this.IsPeekTokenTypeEqualTo(type))
            {
                this.NextToken();
                return true;
            }
            else
            {
                return false;
            }
        }

        public Program ParseProgram()
        {
            var program = new Program();
            var list = new List<Statement>();

            while (!this.IsCurrentTokenTypeEqualTo(TokenType.EOF))
            {
                var statement = this.ParseStatement();

                if (statement != null)
                {
                    list.Add(statement);
                }

                this.NextToken();
            }

            program.statements = list.ToArray();

            return program;
        }

        public Statement ParseStatement()
        {
            switch (this.currentToken.Type)
            {
                case TokenType.LET:
                    return this.ParseLetStatement();
                case TokenType.RETURN:
                    return this.ParseReturnStatement();
                default:
                    return this.ParseExpressionStatement();
            }
        }

        public LetStatement ParseLetStatement()
        {
            var statement = new LetStatement()
            {
                Token = this.currentToken
            };

            if (!this.Expect(TokenType.IDENT))
            {
                return null;
            }

            statement.Name = new Identifier()
            {
                Token = this.currentToken,
                Value = this.currentToken.Literal
            };

            if (!this.Expect(TokenType.ASSIGN))
            {
                return null;
            }

            this.NextToken();

            statement.Value = this.ParseExpression(Precedence.LOWEST);

            if (this.IsPeekTokenTypeEqualTo(TokenType.SEMICOLON))
            {
                this.NextToken();
            }

            return statement;
        }

        public ReturnStatement ParseReturnStatement()
        {
            var statement = new ReturnStatement()
            {
                Token = this.currentToken
            };

            this.NextToken();

            statement.ReturnValue = this.ParseExpression(Precedence.LOWEST);

            if (this.IsPeekTokenTypeEqualTo(TokenType.SEMICOLON))
            {
                this.NextToken();
            }

            return statement;
        }

        public ExpressionStatement ParseExpressionStatement()
        {
            var statement = new ExpressionStatement()
            {
                Token = this.currentToken,
                Expression = this.ParseExpression(Precedence.LOWEST)
            };

            if (this.IsPeekTokenTypeEqualTo(TokenType.SEMICOLON))
            {
                this.NextToken();
            }

            return statement;
        }

        public Expression ParseExpression(Precedence precedence)
        {
            if (!this.prefixParseFunctions.ContainsKey(this.currentToken.Type))
            {
                return null;
            }

            var prefix = this.prefixParseFunctions[this.currentToken.Type];
            var leftExpression = prefix();

            while (!this.IsPeekTokenTypeEqualTo(TokenType.SEMICOLON) && precedence < this.PeekPrecedence())
            {
                var infix = this.infixParseFunctions[this.peekToken.Type];

                if (infix == null)
                {
                    return leftExpression;
                }

                this.NextToken();

                leftExpression = infix(leftExpression);
            }

            return leftExpression;
        }

        public Precedence PeekPrecedence()
        {
            Precedence precedence;
            if (precedences.TryGetValue(this.peekToken.Type, out precedence))
            {
                return precedence;
            }

            return Precedence.LOWEST;
        }


        public Precedence CurrentPrecedence()
        {
            Precedence precedence;
            if (precedences.TryGetValue(this.currentToken.Type, out precedence))
            {
                return precedence;
            }

            return Precedence.LOWEST;
        }

        public Expression ParseIdentifier() => new Identifier()
        {
            Token = this.currentToken,
            Value = this.currentToken.Literal
        };

        public Expression ParseIntegerLitral()
        {
            var expression = new IntegerLiteral()
            {
                Token = this.currentToken
            };

            Int64 result;
            if (!Int64.TryParse(this.currentToken.Literal, out result))
            {

            }

            expression.Value = result;

            return expression;
        }

        public Expression ParseStringLiteral()
        {
            return new StringLiteral() { Token = this.currentToken, Value = this.currentToken.Literal };
        }

        public Expression ParseArrayLiteral()
        {
            return new ArrayLiteral()
            {
                Token = this.currentToken,
                Elements = this.ParseExpressionList(TokenType.RBRACKET)
            };
        }

        public Expression ParseHashLiteral()
        {
            var hash = new HashLiteral() { Token = this.currentToken, Pairs = new Dictionary<Expression, Expression>() };

            while (!this.IsPeekTokenTypeEqualTo(TokenType.RBRACE))
            {
                this.NextToken();
                var key = this.ParseExpression(Precedence.LOWEST);

                if (!this.Expect(TokenType.COLON))
                {
                    return null;
                }

                this.NextToken();
                var value = this.ParseExpression(Precedence.LOWEST);
                hash.Pairs[key] = value;

                if (!this.IsPeekTokenTypeEqualTo(TokenType.RBRACE) && !this.Expect(TokenType.COMMA))
                {
                    return null;
                }
            }

            if (!this.Expect(TokenType.RBRACE))
            {
                return null;
            }

            return hash;
        }

        public Expression ParsePrefixExpression()
        {
            var expression = new PrefixExpression()
            {
                Token = this.currentToken,
                Operator = this.currentToken.Literal
            };

            this.NextToken();

            expression.Right = this.ParseExpression(Precedence.PREFIX);

            return expression;
        }

        public Expression ParseInfixExpression(Expression left)
        {
            var expression = new InfixExpression()
            {
                Token = this.currentToken,
                Operator = this.currentToken.Literal,
                Left = left
            };

            var precedence = this.CurrentPrecedence();

            this.NextToken();
            expression.Right = this.ParseExpression(precedence);

            return expression;
        }

        public Expression ParseBoolean() => new Boolean()
        {
            Token = this.currentToken,
            Value = this.IsCurrentTokenTypeEqualTo(TokenType.TRUE)
        };

        public Expression ParseGroupedExpression()
        {
            this.NextToken();

            var expression = this.ParseExpression(Precedence.LOWEST);

            if (!this.Expect(TokenType.RPAREN))
            {
                return null;
            }

            return expression;
        }

        public Expression ParseIfExpression()
        {
            var expression = new IfExpression()
            {
                Token = this.currentToken
            };

            if (!this.Expect(TokenType.LPAREN))
            {
                return null;
            }

            this.NextToken();
            expression.Condition = this.ParseExpression(Precedence.LOWEST);

            if (!this.Expect(TokenType.RPAREN))
            {
                return null;
            }

            if (!this.Expect(TokenType.LBRACE))
            {
                return null;
            }

            expression.Cons = this.ParseBlockStatement();

            if (this.IsPeekTokenTypeEqualTo(TokenType.ELSE))
            {
                this.NextToken();

                if (!this.Expect(TokenType.LBRACE))
                {
                    return null;
                }

                expression.Els = this.ParseBlockStatement();
            }

            return expression;
        }

        public BlockStatement ParseBlockStatement()
        {
            var block = new BlockStatement()
            {
                Token = this.currentToken
            };
            var list = new List<Statement>();

            this.NextToken();

            while (!this.IsCurrentTokenTypeEqualTo(TokenType.RBRACE) && !this.IsCurrentTokenTypeEqualTo(TokenType.EOF))
            {
                var statement = this.ParseStatement();

                if (statement != null)
                {
                    list.Add(statement);
                }
                this.NextToken();
            }

            block.statements = list.ToArray();

            return block;
        }

        public Expression ParseFunctionLiteral()
        {
            var func = new FunctionLiteral() { Token = this.currentToken };

            if (!this.Expect(TokenType.LPAREN))
            {
                return null;
            }

            func.Parameter = this.ParseFunctionParameters();

            if (!this.Expect(TokenType.LBRACE))
            {
                return null;
            }

            func.Body = this.ParseBlockStatement();

            return func;
        }

        public Identifier[] ParseFunctionParameters()
        {
            var list = new List<Identifier>();

            if (this.IsPeekTokenTypeEqualTo(TokenType.RPAREN))
            {
                this.NextToken();
                return list.ToArray();
            }

            this.NextToken();

            var identifier = new Identifier()
            {
                Token = this.currentToken,
                Value = this.currentToken.Literal
            };

            list.Add(identifier);

            while (this.IsPeekTokenTypeEqualTo(TokenType.COMMA))
            {
                this.NextToken();
                this.NextToken();

                identifier = new Identifier()
                {
                    Token = this.currentToken,
                    Value = this.currentToken.Literal
                };

                list.Add(identifier);
            }

            if (!this.Expect(TokenType.RPAREN))
            {
                return null;
            }

            return list.ToArray();
        }

        public Expression ParseCallExpression(Expression function)
        {
            return new CallExpression()
            {
                Token = this.currentToken,
                Function = function,
                Arguments = this.ParseExpressionList(TokenType.RPAREN)
            };
        }

        public Expression[] ParseExpressionList(TokenType end)
        {
            var list = new List<Expression>();

            if (this.IsPeekTokenTypeEqualTo(end))
            {
                this.NextToken();
                return list.ToArray();
            }

            this.NextToken();
            list.Add(this.ParseExpression(Precedence.LOWEST));

            while (this.IsPeekTokenTypeEqualTo(TokenType.COMMA))
            {
                this.NextToken();
                this.NextToken();
                list.Add(this.ParseExpression(Precedence.LOWEST));
            }

            if (!this.Expect(end))
            {
                return null;
            }

            return list.ToArray();
        }

        public Expression ParseIndexExpression(Expression left)
        {
            var expression = new IndexExpresssion() { Token = this.currentToken, Left = left };

            this.NextToken();
            expression.Index = this.ParseExpression(Precedence.LOWEST);

            if (!this.Expect(TokenType.RBRACKET))
            {
                return null;
            }

            return expression;
        }

        private void RegisterPrefix(TokenType type, PrefixParser prefix)
        {
            this.prefixParseFunctions[type] = prefix;
        }

        private void RegisterInfix(TokenType type, InfixParser infix)
        {
            this.infixParseFunctions[type] = infix;
        }
    }
}