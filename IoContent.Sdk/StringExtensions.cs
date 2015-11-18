using System;
using System.Text;

namespace IoContent.Sdk
{
	internal static class StringExtensions
	{
		/// <summary>
		/// Converts any string to pascal case, handling special characters
		/// and digits also.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string ToPascalCase(this string str)
		{
			char[] chars = str.ToCharArray();

			var sb = new StringBuilder();

			bool previousCharWasUpper = false;
			bool lastOperationWasToLower = false;

			int startPos = 0;

			for (int i = startPos; i < chars.Length; i++)
			{
				char character = chars[i];

				if (char.IsLetter(character))
				{
					if (char.IsLower(character))
					{
						bool toUpper = false;

						if (i > 0)
						{
							// Look at the previous char to see if not a letter

							if (!Char.IsLetter(chars[i - 1]))
							{
								toUpper = true;
							}
						}

						if (i == 0 || toUpper)
						{
							character = Char.ToUpper(character);

							lastOperationWasToLower = false;
						}
					}
					else // IsUpper = true
					{
						if (previousCharWasUpper || lastOperationWasToLower)
						{
							character = Char.ToLower(character);

							lastOperationWasToLower = true;
						}
					}

					previousCharWasUpper = Char.IsUpper(character);

					sb.Append(character);
				}
				else
				{
					if (Char.IsDigit(character))
					{
						sb.Append(character);

						previousCharWasUpper = false;
						lastOperationWasToLower = false;
					}
				}
			}

			return sb.ToString();
		}
	}
}
