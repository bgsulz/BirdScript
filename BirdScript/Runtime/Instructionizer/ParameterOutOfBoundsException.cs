using System;
using BirdScript.Tokenizer;

namespace BirdScript.Errors
{
    [Serializable]
    internal class ParameterOutOfBoundsException : InstructionizerException
    {
        public ParameterOutOfBoundsException(int index, Command type, int value, int min, int max, int line) 
            : base($"Parameter at index {index} in {type} command is {value}; must be min {min}, max {max}", line)
        {

        }

        public ParameterOutOfBoundsException(int index, Command type, float value, float min, float max, int line) 
            : base($"Parameter at index {index} in {type} command is {value}; must be min {min}, max {max}", line)
        {

        }
    }
}