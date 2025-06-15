using System;
using System.Collections.Generic;
using System.Linq;
using Interfaz.Language.AST;

namespace Interfaz.Language
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _currentTokenIndex;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _currentTokenIndex = 0;
        }

        private Token CurrentToken => _tokens[_currentTokenIndex];

        private Token PeekToken(int offset = 1)
        {
            if (_currentTokenIndex + offset >= _tokens.Count)
            {
                return new Token(TokenType.EOF, "EOF", CurrentToken.Line, CurrentToken.Column + 1);
            }
            return _tokens[_currentTokenIndex + offset];
        }

        private void Advance()
        {
            _currentTokenIndex++;
        }

        private Token Eat(TokenType expectedType)
        {
            if (CurrentToken.Type == expectedType)
            {
                Token consumedToken = CurrentToken;
                Advance();
                return consumedToken;
            }
            else
            {
                throw new Exception($"Error de sintaxis en Línea {CurrentToken.Line}, Columna {CurrentToken.Column}: Se esperaba un token de tipo {expectedType}, pero se encontró {CurrentToken.Type} ('{CurrentToken.Value}')");
            }
        }

        public ProgramNode ParseProgram()
        {
            List<Statement> statements = new List<Statement>();
            while (CurrentToken.Type != TokenType.EOF)
            {
                while (CurrentToken.Type == TokenType.NEWLINE)
                {
                    Advance();
                }

                if (CurrentToken.Type == TokenType.EOF) break;

                statements.Add(ParseStatement());
            }
            return new ProgramNode(statements, _tokens.Count > 0 ? _tokens[0].Line : 1, _tokens.Count > 0 ? _tokens[0].Column : 1);
        }

        private Statement ParseStatement()
        {
            Statement statement = null;
            Token current = CurrentToken;
            int line = CurrentToken.Line;   
            int column = CurrentToken.Column;

            switch (current.Type)
            {
                case TokenType.FUNC:
                    return ParseFunctionDefinition();
                case TokenType.RETURN:
                    return ParseReturnStatement();
                case TokenType.SPAWN:
                case TokenType.COLOR:
                case TokenType.DRAWLINE:
                case TokenType.DRAWCIRCLE:
                    statement = ParseFunctionLikeStatement();
                    break;
                case TokenType.DRAWRECTANGLE:
                    Token drawRectangleToken = Eat(TokenType.DRAWRECTANGLE); // Consume el token DRAWRECTANGLE
                    Eat(TokenType.LEFT_PAREN); // Consume (

                    Expression dxExpr = ParseExpression();
                    Eat(TokenType.COMMA); // Consume la primera coma
                    Expression dyExpr = ParseExpression();
                    Eat(TokenType.COMMA); // Consume la segunda coma
                    Expression widthExpr = ParseExpression();
                    Eat(TokenType.COMMA); // Consume la tercera coma
                    Expression heightExpr = ParseExpression();

                    Eat(TokenType.RIGHT_PAREN); // Consume )

                return new DrawRectangleStatement(dxExpr, dyExpr, widthExpr, heightExpr, drawRectangleToken.Line, drawRectangleToken.Column);
                case TokenType.SIZE:
                    statement = ParseFunctionLikeStatement();
                    break;
                case TokenType.MOVE:
                case TokenType.TURN:
                case TokenType.FORWARD:
                case TokenType.BACKWARD:
                    statement = ParseFunctionLikeStatement();
                    break;
                case TokenType.IF:
                    return ParseIfStatement();
                case TokenType.LOOP:
                    return ParseLoopStatement();
                case TokenType.CALL:
                case TokenType.PRINT:
                case TokenType.FILL:
                case TokenType.RAND:
                    statement = ParseFunctionLikeStatement();
                    break;

                case TokenType.SET:
                    statement = ParseAssignmentStatement();
                    break;

                case TokenType.GOTO:
                    statement = ParseGoToStatement();
                    break;

                case TokenType.IDENTIFIER:
                    // Asignación tipo i <- expr
                    if (PeekToken().Type == TokenType.ASSIGN_LEFT)
                    {
                        statement = ParseLeftAssignStatement();
                        break;
                    }
                    // Si la línea es solo un identificador seguido de NEWLINE o EOF, es una etiqueta
                    if (PeekToken().Type == TokenType.NEWLINE || PeekToken().Type == TokenType.EOF)
                    {
                        statement = ParseLabelStatement();
                        break;
                    }
                    goto default;

                default:
                    throw new Exception($"Error de sintaxis en Línea {current.Line}, Columna {current.Column}: Token inesperado '{current.Value}' de tipo {current.Type}. Se esperaba el inicio de una instrucción.");
            }

            while (CurrentToken.Type == TokenType.NEWLINE)
            {
                Advance();
            }

            return statement;
        }

        private FunctionDefinition ParseFunctionDefinition()
        {
            Token funcToken = Eat(TokenType.FUNC);
            Token nameToken = Eat(TokenType.IDENTIFIER);
            Eat(TokenType.LEFT_PAREN);

            var parameters = new List<string>();
            if (CurrentToken.Type != TokenType.RIGHT_PAREN)
            {
                parameters.Add(Eat(TokenType.IDENTIFIER).Value);
                while (CurrentToken.Type == TokenType.COMMA)
                {
                    Eat(TokenType.COMMA);
                    parameters.Add(Eat(TokenType.IDENTIFIER).Value);
                }
            }
            Eat(TokenType.RIGHT_PAREN);

            while (CurrentToken.Type == TokenType.NEWLINE)
                Advance();

            var body = new List<Statement>();
            while (CurrentToken.Type != TokenType.ENDFUNC && CurrentToken.Type != TokenType.EOF)
            {
                while (CurrentToken.Type == TokenType.NEWLINE)
                    Advance();
                if (CurrentToken.Type == TokenType.ENDFUNC || CurrentToken.Type == TokenType.EOF)
                    break;
                body.Add(ParseStatement());
            }
            Eat(TokenType.ENDFUNC);

            return new FunctionDefinition(nameToken.Value, parameters, body, funcToken.Line, funcToken.Column);
        }

        private ReturnStatement ParseReturnStatement()
        {
            Token returnToken = Eat(TokenType.RETURN);
            Expression value = ParseExpression();
            return new ReturnStatement(value, returnToken.Line, returnToken.Column);
        }

        private IfStatement ParseIfStatement()
        {
            Token ifToken = Eat(TokenType.IF);
            Expression condition = ParseExpression();

            var trueBlock = new List<Statement>();
            var falseBlock = new List<Statement>();

            // Salta líneas vacías antes del bloque verdadero
            while (CurrentToken.Type == TokenType.NEWLINE)
                Advance();

            while (CurrentToken.Type != TokenType.ELSE && CurrentToken.Type != TokenType.ENDIF && CurrentToken.Type != TokenType.EOF)
            {
                // Salta líneas vacías dentro del bloque
                while (CurrentToken.Type == TokenType.NEWLINE)
                    Advance();

                if (CurrentToken.Type == TokenType.ELSE || CurrentToken.Type == TokenType.ENDIF || CurrentToken.Type == TokenType.EOF)
                    break;

                trueBlock.Add(ParseStatement());
            }

            if (CurrentToken.Type == TokenType.ELSE)
            {
                Eat(TokenType.ELSE);

                // Salta líneas vacías antes del bloque falso
                while (CurrentToken.Type == TokenType.NEWLINE)
                    Advance();

                while (CurrentToken.Type != TokenType.ENDIF && CurrentToken.Type != TokenType.EOF)
                {
                    // Salta líneas vacías dentro del bloque
                    while (CurrentToken.Type == TokenType.NEWLINE)
                        Advance();

                    if (CurrentToken.Type == TokenType.ENDIF || CurrentToken.Type == TokenType.EOF)
                        break;

                    falseBlock.Add(ParseStatement());
                }
            }

            Eat(TokenType.ENDIF);

            return new IfStatement(condition, trueBlock, falseBlock, ifToken.Line, ifToken.Column);
        }

        private LoopStatement ParseLoopStatement()
        {
            Token loopToken = Eat(TokenType.LOOP);
            Token iterator = Eat(TokenType.IDENTIFIER);
            Eat(TokenType.ASSIGN);
            Expression from = ParseExpression();
            Eat(TokenType.TO);
            Expression to = ParseExpression();

            // Salta líneas vacías antes del cuerpo
            while (CurrentToken.Type == TokenType.NEWLINE)
                Advance();

            var body = new List<Statement>();
            while (CurrentToken.Type != TokenType.ENDLOOP && CurrentToken.Type != TokenType.EOF)
            {
                while (CurrentToken.Type == TokenType.NEWLINE)
                    Advance();
                if (CurrentToken.Type == TokenType.ENDLOOP || CurrentToken.Type == TokenType.EOF)
                    break;
                body.Add(ParseStatement());
            }
            Eat(TokenType.ENDLOOP);

            return new LoopStatement(iterator.Value, from, to, body, loopToken.Line, loopToken.Column);
        }

        private LabelStatement ParseLabelStatement()
        {
            Token labelToken = Eat(TokenType.IDENTIFIER);
            int line = labelToken.Line;
            int column = labelToken.Column;
            return new LabelStatement(labelToken.Value, line, column);
        }

        private GoToStatement ParseGoToStatement()
        {
            Token gotoToken = Eat(TokenType.GOTO);
            int line = gotoToken.Line;
            int column = gotoToken.Column;

            Eat(TokenType.LEFT_BRACKET);
            Token labelToken = Eat(TokenType.IDENTIFIER);
            Eat(TokenType.RIGHT_BRACKET);
            Eat(TokenType.LEFT_PAREN);
            Expression condition = ParseExpression();
            Eat(TokenType.RIGHT_PAREN);

            return new GoToStatement(labelToken.Value, condition, line, column);
        }

        private Statement ParseFunctionLikeStatement()
        {
            Token funcNameToken = Eat(CurrentToken.Type);
            string functionName = funcNameToken.Value;
            int line = funcNameToken.Line;
            int column = funcNameToken.Column;

            Eat(TokenType.LEFT_PAREN);

            List<Expression> arguments = new List<Expression>();
            if (CurrentToken.Type != TokenType.RIGHT_PAREN)
            {
                arguments.Add(ParseExpression());
                while (CurrentToken.Type == TokenType.COMMA)
                {
                    Eat(TokenType.COMMA);
                    arguments.Add(ParseExpression());
                }
            }

            Eat(TokenType.RIGHT_PAREN);

            if (funcNameToken.Type == TokenType.PRINT)
            {
                if (arguments.Count != 1)
                {
                    throw new Exception($"Error de sintaxis en Línea {line}, Columna {column}: La instrucción 'Print' espera exactamente 1 argumento.");
                }
                return new PrintStatement(arguments[0], line, column);
            }

            return new FunctionCallStatement(new FunctionCall(functionName, arguments, line, column), line, column);}

        private Assignment ParseLeftAssignStatement()
        {
            Token varToken = Eat(TokenType.IDENTIFIER);
            int line = varToken.Line;
            int column = varToken.Column;
            Eat(TokenType.ASSIGN_LEFT);
            Expression value = ParseExpression();
            return new Assignment(varToken.Value, value, line, column);
        }

        private Assignment ParseAssignmentStatement()
        {
            Token setToken = Eat(TokenType.SET);
            int line = setToken.Line;
            int column = setToken.Column;

            Eat(TokenType.LEFT_PAREN);
            Token varNameToken = Eat(TokenType.IDENTIFIER);
            Eat(TokenType.COMMA);
            Expression value = ParseExpression();
            Eat(TokenType.RIGHT_PAREN);

            return new Assignment(varNameToken.Value, value, line, column);
        }

        // --- Cambia la jerarquía de precedencia para incluir operadores lógicos ---

        private Expression ParseExpression()
        {
            return ParseLogicalOr();
        }

        private Expression ParseLogicalOr()
        {
            Expression expr = ParseLogicalAnd();
            while (CurrentToken.Type == TokenType.OR)
            {
                Token op = CurrentToken;
                Advance();
                expr = new BinaryExpression(expr, op.Type, ParseLogicalAnd(), op.Line, op.Column);
            }
            return expr;
        }

        private Expression ParseLogicalAnd()
        {
            Expression expr = ParseComparison();
            while (CurrentToken.Type == TokenType.AND)
            {
                Token op = CurrentToken;
                Advance();
                expr = new BinaryExpression(expr, op.Type, ParseComparison(), op.Line, op.Column);
            }
            return expr;
        }

        private Expression ParseComparison()
        {
            Expression expr = ParseAdditive();

            while (CurrentToken.Type == TokenType.EQUALS ||
                   CurrentToken.Type == TokenType.NOT_EQUALS ||
                   CurrentToken.Type == TokenType.LESS ||
                   CurrentToken.Type == TokenType.GREATER ||
                   CurrentToken.Type == TokenType.LESS_EQUALS ||
                   CurrentToken.Type == TokenType.GREATER_EQUALS)
            {
                Token op = CurrentToken;
                Advance();
                expr = new BinaryExpression(expr, op.Type, ParseAdditive(), op.Line, op.Column);
            }
            return expr;
        }

        private Expression ParseAdditive()
        {
            Expression expr = ParseMultiplicative();

            while (CurrentToken.Type == TokenType.PLUS || CurrentToken.Type == TokenType.MINUS)
            {
                Token op = CurrentToken;
                Advance();
                expr = new BinaryExpression(expr, op.Type, ParseMultiplicative(), op.Line, op.Column);
            }
            return expr;
        }

        private Expression ParseMultiplicative()
        {
            Expression expr = ParseUnary();

            while (CurrentToken.Type == TokenType.MULTIPLY || CurrentToken.Type == TokenType.DIVIDE || CurrentToken.Type == TokenType.MODULO)
            {
                Token op = CurrentToken;
                Advance();
                expr = new BinaryExpression(expr, op.Type, ParseUnary(), op.Line, op.Column);
            }
            return expr;
        }

        private Expression ParseUnary()
{
    if (CurrentToken.Type == TokenType.NOT || 
        CurrentToken.Type == TokenType.MINUS || 
        CurrentToken.Type == TokenType.PLUS)
    {
        Token op = CurrentToken;
        Advance();
        return new UnaryExpression(op.Type, ParseUnary(), op.Line, op.Column);
    }
    return ParsePrimary();
}

        private Expression ParsePrimary()
        {
            Token token = CurrentToken;
            switch (token.Type)
            {
                case TokenType.INTEGER_LITERAL:
                    Eat(TokenType.INTEGER_LITERAL);
                    return new IntegerLiteral(int.Parse(token.Value), token.Line, token.Column);

                case TokenType.STRING_LITERAL:
                    Eat(TokenType.STRING_LITERAL);
                    return new StringLiteral(token.Value, token.Line, token.Column);

                case TokenType.IDENTIFIER:
                    Eat(TokenType.IDENTIFIER);

                    // Comprobar si es llamada a función: IDENTIFIER seguido de LEFT_PAREN
                    if (CurrentToken.Type == TokenType.LEFT_PAREN)
                    {
                        Eat(TokenType.LEFT_PAREN);
                        List<Expression> args = new();

                        if (CurrentToken.Type != TokenType.RIGHT_PAREN)
                        {
                            args.Add(ParseExpression());
                            while (CurrentToken.Type == TokenType.COMMA)
                            {
                                Eat(TokenType.COMMA);
                                args.Add(ParseExpression());
                            }
                        }

                        Eat(TokenType.RIGHT_PAREN);
                        return new FunctionCall(token.Value, args, token.Line, token.Column);
                    }

                    return new IdentifierExpression(token.Value, token.Line, token.Column);

                case TokenType.LEFT_PAREN:
                    Eat(TokenType.LEFT_PAREN);
                    Expression expr = ParseExpression();
                    Eat(TokenType.RIGHT_PAREN);
                    return expr;

                case TokenType.RAND:
                    Eat(TokenType.RAND);
                    Eat(TokenType.LEFT_PAREN);
                    Expression min = ParseExpression();
                    Eat(TokenType.COMMA);
                    Expression max = ParseExpression();
                    Eat(TokenType.RIGHT_PAREN);
                    return new RandExpression(min, max, token.Line, token.Column);
                case TokenType.GETACTUALX:
                    Token getActualXToken = Eat(TokenType.GETACTUALX);
                    Eat(TokenType.LEFT_PAREN);
                    Eat(TokenType.RIGHT_PAREN);
                    // Pass the token's value (the function name) to the FunctionCall constructor
                    return new FunctionCall(getActualXToken.Value, new List<Expression>(), getActualXToken.Line, getActualXToken.Column);

                case TokenType.GETACTUALY:
                    Token getActualYToken = Eat(TokenType.GETACTUALY);
                    Eat(TokenType.LEFT_PAREN);
                    Eat(TokenType.RIGHT_PAREN);
                    // Pass the token's value (the function name) to the FunctionCall constructor
                   return new FunctionCall(getActualYToken.Value, new List<Expression>(), getActualYToken.Line, getActualYToken.Column);

                default:
                    throw new Exception($"Error de sintaxis en Línea {token.Line}, Columna {token.Column}: Token inesperado '{token.Value}' de tipo {token.Type}. Se esperaba un literal, identificador, o expresión.");
            }
        }

    }
}