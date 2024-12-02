namespace Cyb_lab.Ciphers;

public static class VigenereCipher
{
	/// <summary>
	/// This function generates the key in a cyclic manner
	/// until it's length isn't equal to the length of original text
	/// </summary>
	/// <param name="plainText"></param>
	/// <param name="key"></param>
	/// <returns></returns>
	public static string GenerateKey(string plainText, string key)
	{
		int textLength = plainText.Length;

		for (int i = 0; ; i++)
		{
			if (i == textLength)
			{
				i = 0;
			}
			if (key.Length == textLength)
			{
				break;
			}

			key += key[i];
		}

		return key;
	}

    /// <summary>
    /// This function returns the encrypted text
    /// generated with the help of the key
    /// </summary>
    /// <param name="plainText"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Encrypt(string plainText, string key)
	{
		string cipheredText = string.Empty;

		int textLength = plainText.Length;

		for (int i = 0; i < textLength; i++)
		{
			// converting in range 0-25
			int x = (plainText[i] + key[i]) % 26;

			// convert into alphabets(ASCII)
			x += 'A';

			cipheredText += (char)x;
		}

		return cipheredText;
	}

    /// <summary>
    /// This function decrypts the encrypted text
    /// and returns the original text
    /// </summary>
    /// <param name="cipheredText"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Decrypt(string cipheredText, string key)
	{
		string plainText = string.Empty;

		int cipherLength = cipheredText.Length;
		int keyLength = key.Length;

		for (int i = 0; i < cipherLength && i < keyLength; i++)
		{
			// converting in range 0-25
			int x = (cipheredText[i] - key[i] + 26) % 26;

			// convert into alphabets(ASCII)
			x += 'A';

			plainText += (char)x;
		}

		return plainText;
	}
}
