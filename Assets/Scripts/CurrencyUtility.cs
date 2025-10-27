// Static class for handling number format (12345678 -> 12.345.678)
public static class CurrencyUtility
{
    public static string CurrencyFormat(long value)
    {
        int characterCount = 0;
        int comparator = 1;

        // Count how many character the value is
        do
        {
            characterCount++;
            comparator *= 10;
        } while (comparator <= value);

        string tempResult = "";

        // Insert value to a string in reverse, and add "." every 3 characters
        for (int i = 1; i <= characterCount; i++)
        {
            tempResult += value % 10;
            value /= 10;
            if (i % 3 == 0 && value > 0) tempResult += ".";
        }

        string result = "";

        // Reverse the string
        for (int i = 1; i <= tempResult.Length; i++)
        {
            result += tempResult[tempResult.Length - i];
        }

        return result;
    }
}
