namespace CarCareTracker.Models
{
    public class AdminViewModel
    {
        public List<UserData> Users { get; set; } = new List<UserData>();
        public List<Token> Tokens { get; set; } = new List<Token>();
    }
}
