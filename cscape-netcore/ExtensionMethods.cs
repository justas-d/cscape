using System;
using System.Text;

namespace CScape
{
    public static class ExtensionMethods
    {
        public static bool TryReadString(this Blob blob, int maxLength, out string rsString)
        {
            var builder = new StringBuilder(maxLength);
            var retval = true;
            const byte nullTerm = 10;

            try
            {
                byte c;
                while ((c = blob.ReadByte()) != nullTerm)
                    builder.Append(Convert.ToChar(c));
            }
            catch (ArgumentOutOfRangeException)
            {
                retval = false;
            }
            rsString = builder.ToString();
            return retval;
        }
    }
}
