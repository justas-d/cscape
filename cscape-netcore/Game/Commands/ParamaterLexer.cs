using System;
using System.Linq;

namespace CScape.Game.Commands
{
    public class ParamaterLexer
    {
        public class ParamNotFoundException : Exception
        {
            public string ParamName { get; }

            public ParamNotFoundException(string paramName)
            {
                ParamName = paramName;
            }
        }

        public class ParamParseException : Exception
        {
            public string ParamName { get; }
            public Type ParamType { get; }

            public ParamParseException(string paramName, Type paramType)
            {
                ParamName = paramName;
                ParamType = paramType;
            }
        }

        private readonly string[] _words;
        private int _wordIdx = 0;

        private bool IsWordEof => _wordIdx >= _words.Length;

        public ParamaterLexer(string data)
        {
            _words = data.Split();
        }

        private string Word()
        {
            string word = null;

            while (string.IsNullOrEmpty(word) && !IsWordEof)
                word = _words[_wordIdx++].Trim();

            return word;
        }

        public void ReadNumber<T>(string name, ref T result, bool isOptional = false) where T : struct
        {
            var rawData = "";
            ReadWord(name, ref rawData, true);

            if (string.IsNullOrEmpty(rawData) && !isOptional)
                throw new ParamNotFoundException(name);

            if (!long.TryParse(rawData, out long data))
            {
                if (isOptional)
                    return;
                else
                    throw new ParamParseException(name, typeof(int));
            }

            result = (T)Convert.ChangeType(data, typeof(T));
        }

        public void ReadWord(string name, ref string result, bool isOptional = false)
        {
            var data = Word();

            if (data == null)
            {
                if (isOptional)
                    return;
                else
                    throw new ParamNotFoundException(name);
            }

            result = data;
        }

        /// <summary>
        /// Returns what has been left unparsed so far.
        /// </summary>
        public string ReadRemaining()
        {
            if (IsWordEof)
                return string.Empty;

            return string.Join(" ", Enumerable.Skip<string>(_words, _wordIdx + 1));
        }
    }
}