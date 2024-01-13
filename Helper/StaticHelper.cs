namespace CarCareTracker.Helper
{
    /// <summary>
    /// helper method for static vars
    /// </summary>
    public static class StaticHelper
    {
        public static string DbName = "data/cartracker.db";
        public static string UserConfigPath = "config/userConfig.json";
        public static string GenericErrorMessage = "An error occurred, please try again later";

        public static string TruncateStrings(string input, int maxLength = 25)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }
            if (input.Length > maxLength)
            {
                return (input.Substring(0, maxLength) + "...");
            } else
            {
                return input;
            }
        }
    }
}
