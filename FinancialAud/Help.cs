namespace Bank
{
    public static class Help
    {
        public static string Truncate(this string value, int maxCharacters)
        {
            return value.Length <= maxCharacters ? value : value[..(maxCharacters - 2)] + "..";
        }
    };
    
}
