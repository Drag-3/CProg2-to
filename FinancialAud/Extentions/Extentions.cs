namespace Bank.Extentions
{
    public static class Help
    {
        public static string Truncate(this string value, int maxCharacters)
        {
            return value.Length <= maxCharacters ? value : value[..(maxCharacters - 2)] + "..";
        }

        public static bool IsAlphaNumeric(this char value)
        {
            return char.IsDigit(value) || char.IsLetter(value);
        }
    };
    
}
