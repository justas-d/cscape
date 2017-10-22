using System;
using System.Linq;

namespace CScape.Basic.Commands
{
    public class ParamaterLexer
    {
        private readonly string[] _words;
        private int _wordIdx = 0;

        private bool IsWordEof => _wordIdx >= _words.Length;

        public bool DidFail { get; private set; }

        public string FailedOnParam { get; private set; }
        [CanBeNull] public Type FailParamExpected { get; private set; }

        private void SignalFail([NotNull] string onParamName, Type paramType = null)
        {
            if (DidFail)
                return;

            DidFail = true;

            FailedOnParam = onParamName;
            FailParamExpected = paramType;
        }

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


        public void ReadBoolean(string name, ref bool result, bool isOptional = false)
        {
            if (DidFail)
                return;

            var rawData = "";
            ReadWord(name, ref rawData, true);

            if (string.IsNullOrEmpty(rawData) && !isOptional)
            {
                SignalFail(name);
                return;
            }

            if (!bool.TryParse(rawData, out bool data))
            {
                if (isOptional)
                    return;
                else
                    SignalFail(name, typeof(bool));
            }

            result = data;
        }
        public void ReadNumber<T>(string name, ref T result, bool isOptional = false) where T : struct
        {
            if (DidFail)
                return;

            var rawData = "";
            ReadWord(name, ref rawData, true);

            if (string.IsNullOrEmpty(rawData) && !isOptional)
            {
                SignalFail(name);
                return;
            }

            if (!long.TryParse(rawData, out long data))
            {
                if (isOptional)
                    return;
                else
                    SignalFail(name, typeof(T));
            }

            result = (T)Convert.ChangeType(data, typeof(T));
        }

        public void ReadWord(string name, ref string result, bool isOptional = false)
        {
            if (DidFail)
                return;

            var data = Word();

            if (data == null)
            {
                if (isOptional)
                    return;
                else
                    SignalFail(name);
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

            return string.Join(" ", _words.Skip(_wordIdx + 1));
        }
    }
}