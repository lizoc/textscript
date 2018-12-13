using System.Collections;
using System.Text;

namespace Lizoc.TextScript
{
    internal class StringHelper
    {
        public static string Join(string separator, IEnumerable items)
        {
            StringBuilder builder = new StringBuilder();
            bool isFirst = true;
            foreach (var item in items)
            {
                if (!isFirst)
                    builder.Append(separator);

                builder.Append(item);
                isFirst = false;
            }

            return builder.ToString();
        }
    }
}