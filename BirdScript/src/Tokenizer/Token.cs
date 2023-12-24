namespace BirdScript.Tokenizer
{
    public interface IToken
    {
        TokenType Type { get; }
        int Line { get; }
    }

    public readonly struct StructureToken : IToken
    {
        public TokenType Type { get; }
        public int Line { get; }

        public StructureToken(TokenType type, int line)
        {
            Type = type;
            Line = line;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }

    public readonly struct InfoToken<T> : IToken
    {
        public TokenType Type { get; }
        public int Line { get; }
        public T Value { get; }
        
        public InfoToken(TokenType type, int line, T value)
        {
            Type = type;
            Line = line;
            Value = value;
        }

        public override string ToString()
        {
            return Type + $"({Value})";
        }
    }
}