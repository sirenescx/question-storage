namespace QuestionStorage.Models.Users
{
    public class RestorationTokens
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        
        public bool Expired { get; set; }
    }
}