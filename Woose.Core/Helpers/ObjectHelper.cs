using System.Text;

namespace Woose.Core
{
    public static class ObjectHelper
    {
        public static string AddSlashBeforeUppercase(this string input)
        {
            StringBuilder result = new StringBuilder();

            if (input.Length > 1)
            {
                result.Append(input[0]);
                for (int i = 1; i < input.Length; i++)
                {
                    char currentChar = input[i];

                    if (char.IsUpper(currentChar))
                    {
                        // 대문자이면 앞에 '/'를 추가
                        result.Append('/');
                    }

                    // 현재 문자를 결과에 추가
                    result.Append(currentChar);
                }
            }

            return result.ToString();
        }
    }
}
