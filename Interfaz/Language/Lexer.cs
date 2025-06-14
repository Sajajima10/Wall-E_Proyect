using System;
using System.Collections.Generic;

namespace Interfaz.Language
{
    public class Lexer
    {
        private readonly string _input;
        private int _position;
        private int _line;
        private int _column;

        public Lexer(string input)
        {
            _input = input.Replace("\r\n", "\n");
            _position = 0;
            _line = 1;
            _column = 1;
        }

        private char CurrentChar => _position < _input.Length ? _input[_position] : '\0';

        private void Advance()
        {
            if (CurrentChar == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }
            _position++;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_position < _input.Length)
            {
                char c = CurrentChar;
                int startLine = _line;
                int startColumn = _column;

                if (char.IsWhiteSpace(c) && c != '\n')
                {
                    Advance();
                    continue;
                }

                if (c == '\n')
                {
                    tokens.Add(new Token(TokenType.NEWLINE, "\\n", startLine, startColumn));
                    Advance();
                    continue;
                }

                if (char.IsDigit(c))
                {
                    string number = "";
                    while (char.IsDigit(CurrentChar))
                    {
                        number += CurrentChar;
                        Advance();
                    }
                    tokens.Add(new Token(TokenType.INTEGER_LITERAL, number, startLine, startColumn));
                    continue;
                }

                if (c == '"')
                {
                    Advance();
                    string str = "";
                    while (CurrentChar != '"' && CurrentChar != '\0')
                    {
                        str += CurrentChar;
                        Advance();
                    }
                    Advance();
                    tokens.Add(new Token(TokenType.STRING_LITERAL, str, startLine, startColumn));
                    continue;
                }

                if (char.IsLetter(c) || c == '_')
                {
                    string ident = "";
                    while (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_')
                    {
                        ident += CurrentChar;
                        Advance();
                    }
                    tokens.Add(LexerUtils.MatchKeywordOrIdentifier(ident, startLine, startColumn));
                    continue;
                }

                // Operadores y símbolos
                if (c == '+')
                {
                    tokens.Add(new Token(TokenType.PLUS, "+", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (c == '-')
                {
                    tokens.Add(new Token(TokenType.MINUS, "-", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (c == '*')
                {
                    tokens.Add(new Token(TokenType.MULTIPLY, "*", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (c == '/')
                {
                    tokens.Add(new Token(TokenType.DIVIDE, "/", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (c == '%')
                {
                    tokens.Add(new Token(TokenType.MODULO, "%", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (c == '=')
                {
                    if (Peek() == '=')
                    {
                        Advance(); Advance();
                        tokens.Add(new Token(TokenType.EQUALS, "==", startLine, startColumn));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.ASSIGN, "=", startLine, startColumn));
                        Advance();
                    }
                    continue;
                }
                if (c == '!')
                {
                    if (Peek() == '=')
                    {
                        Advance(); Advance();
                        tokens.Add(new Token(TokenType.NOT_EQUALS, "!=", startLine, startColumn));
                        continue;
                    }
                }
                if (c == '<')
                {
                    if (Peek() == '=')
                    {
                        Advance(); Advance();
                        tokens.Add(new Token(TokenType.LESS_EQUALS, "<=", startLine, startColumn));
                    }
                    else if (Peek() == '-')
                    {
                        Advance(); Advance();
                        tokens.Add(new Token(TokenType.ASSIGN_LEFT, "<-", startLine, startColumn));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.LESS, "<", startLine, startColumn));
                        Advance();
                    }
                    continue;
                }
                if (c == '>')
                {
                    if (Peek() == '=')
                    {
                        Advance(); Advance();
                        tokens.Add(new Token(TokenType.GREATER_EQUALS, ">=", startLine, startColumn));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.GREATER, ">", startLine, startColumn));
                        Advance();
                    }
                    continue;
                }
                if (c == '(')
                {
                    tokens.Add(new Token(TokenType.LEFT_PAREN, "(", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (c == ')')
                {
                    tokens.Add(new Token(TokenType.RIGHT_PAREN, ")", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (c == '[')
                {
                    tokens.Add(new Token(TokenType.LEFT_BRACKET, "[", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (c == ']')
                {
                    tokens.Add(new Token(TokenType.RIGHT_BRACKET, "]", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (c == ',')
                {
                    tokens.Add(new Token(TokenType.COMMA, ",", startLine, startColumn));
                    Advance();
                    continue;
                }

                // Si no se reconoce el carácter
                tokens.Add(new Token(TokenType.UNKNOWN, c.ToString(), startLine, startColumn));
                Advance();
            }

            tokens.Add(new Token(TokenType.EOF, "EOF", _line, _column));
            return tokens;
        }

        private char Peek()
        {
            return _position + 1 < _input.Length ? _input[_position + 1] : '\0';
        }
    }

    public static class LexerUtils
    {
        public static Token MatchKeywordOrIdentifier(string ident, int line, int column)
        {
            switch (ident.ToUpper())
            {
                case "SPAWN": return new Token(TokenType.SPAWN, ident, line, column);
                case "COLOR": return new Token(TokenType.COLOR, ident, line, column);
                case "DRAWLINE": return new Token(TokenType.DRAWLINE, ident, line, column);
                case "MOVE": return new Token(TokenType.MOVE, ident, line, column);
                case "TURN": return new Token(TokenType.TURN, ident, line, column);
                case "FORWARD": return new Token(TokenType.FORWARD, ident, line, column);
                case "BACKWARD": return new Token(TokenType.BACKWARD, ident, line, column);
                case "LOOP": return new Token(TokenType.LOOP, ident, line, column);
                case "TO": return new Token(TokenType.TO, ident, line, column);
                case "ENDLOOP": return new Token(TokenType.ENDLOOP, ident, line, column);
                case "IF": return new Token(TokenType.IF, ident, line, column);
                case "ELSE": return new Token(TokenType.ELSE, ident, line, column);
                case "ENDIF": return new Token(TokenType.ENDIF, ident, line, column);
                case "FUNC": return new Token(TokenType.FUNC, ident, line, column);
                case "ENDFUNC": return new Token(TokenType.ENDFUNC, ident, line, column);
                case "CALL": return new Token(TokenType.CALL, ident, line, column);
                case "RETURN": return new Token(TokenType.RETURN, ident, line, column);
                case "PRINT": return new Token(TokenType.PRINT, ident, line, column);
                case "SET": return new Token(TokenType.SET, ident, line, column);
                case "RAND": return new Token(TokenType.RAND, ident, line, column);
                case "AND": return new Token(TokenType.AND, ident, line, column);
                case "OR": return new Token(TokenType.OR, ident, line, column);
                case "NOT": return new Token(TokenType.NOT, ident, line, column);
                case "GOTO": return new Token(TokenType.GOTO, ident, line, column);
                default: return new Token(TokenType.IDENTIFIER, ident, line, column);
            }
        }
    }
}