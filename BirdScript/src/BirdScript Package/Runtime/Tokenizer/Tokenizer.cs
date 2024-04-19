using System;
using System.Data;
using System.Linq;
using BirdScript.Errors;

namespace BirdScript.Tokenizing
{
    public class Tokenizer
    {
        private readonly DataTable table = new();

        private int _brace, _scan, _line;
        private TokenList _tokens = new();
        private string _source;

        public Tokenizer(string source) => _source = source;

        public TokenList Tokenize()
        {
            _brace = _scan = 0;
            _line = 1;

            while (!IsAtEnd())
            {
                _brace = _scan;
                Gather();
            }

            _tokens.Add(new StructureToken(TokenType.EndOfFile, _line));
            return _tokens;
        }

        private void Gather()
        {
            var c = Advance();

            switch (c)
            {
                case ',' or ' ' or '\t' or '\r':
                    break;
                case ';' or '\0':
                    _tokens.Add(new StructureToken(TokenType.Terminator, _line));
                    break;
                case '\n':
                    _tokens.Add(new StructureToken(TokenType.Terminator, _line));
                    _line++;
                    break;
                case '(':
                    GatherExpression();
                    break;
                case '/':
                    if (Peek() == '/')
                    {
                        Advance();
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                        if (!IsAtEnd())
                        {
                            Advance(); // Consume \n at end of comment.
                            _line++;
                        }
                    }
                    else
                    {
                        throw new TokenizerException(
                        "Unexpected token: " + c + " -- did you mean to comment?", _line);
                    }
                    break;
                default:
                    if (IsNumberParseable(c) || c is '.' or '-')
                    {
                        GatherNumber();
                    }
                    else if (IsLetter(c))
                    {
                        GatherWord();
                    }
                    else
                    {
                        throw new TokenizerException(
                        "Unexpected token: " + c, _line);
                    }
                    break;
            }
        }

        private void GatherWord()
        {
            for (var scanning = Peek(); !IsSeparator(scanning) && !IsTerminator(scanning); scanning = Peek())
            {
                Advance();
            }

            var value = _source[_brace.._scan];
            if (Enum.TryParse<Command>(value, true, out var command))
                _tokens.Add(new InfoToken<Command>(TokenType.Command, _line, command));
            else if (Enum.TryParse<RowColumn>(value, true, out var keyword))
                _tokens.Add(new InfoToken<RowColumn>(TokenType.Keyword, _line, keyword));
            else
                _tokens.Add(new InfoToken<string>(TokenType.Identifier, _line, value));
        }

        private void GatherExpression()
        {
            var parenLevel = 0;

            for (var scanning = Peek(); IsExpression(scanning); scanning = Peek())
            {
                Advance();
                if (scanning == '(')
                    parenLevel++;
                else if (scanning == ')')
                {
                    if (parenLevel == 0)
                        break;
                    parenLevel--;
                }
            }

            var expression = _source[(_brace + 1)..(_scan - 1)];

            // TODO: throw if fails. Returns type `object?` -- simple null check?
            var value = table.Compute(expression, "");

            if (value is int i)
                _tokens.Add(new InfoToken<float>(TokenType.Number, _line, i));
            else if (value is double d)
                _tokens.Add(new InfoToken<float>(TokenType.Number, _line, (float)d));
            else if (value is float f)
                _tokens.Add(new InfoToken<float>(TokenType.Number, _line, f));
            // TODO: else throw
        }

        private void GatherNumber()
        {
            var hasSeenDot = false;

            for (var scanning = Peek(); IsNumberParseable(scanning); scanning = Peek())
            {
                Advance();
                if (scanning == '.')
                {
                    if (!hasSeenDot) hasSeenDot = true;
                    else throw new TokenizerException(
                        "Numbers cannot have more than one decimal point.", _line);
                }
            }

            var value = float.Parse(_source[_brace.._scan]);
            _tokens.Add(new InfoToken<float>(TokenType.Number, _line, value));
        }

        private char Advance()
        {
            return _source[_scan++];
        }

        private char Peek()
        {
            if (IsAtEnd())
                return '\0';
            return _source[_scan];
        }

        private bool IsAtEnd() => _scan >= _source.Length;

        private bool IsTerminator(char c) => c == ';' || c == '\n' || c == '\0';
        private bool IsWhitespace(char c) => char.IsWhiteSpace(c);
        private bool IsSeparator(char c) => IsWhitespace(c) || c == ',';
        private bool IsExpression(char c) => IsNumberParseable(c) || IsMath(c);
        private bool IsNumberParseable(char c) => char.IsNumber(c) || c == '.';
        private bool IsMath(char c) => new char[] { '+', '-', '*', '/', '(', ')' }.Contains(c);
        private bool IsLetter(char c) => char.IsLetter(c);
    }
}