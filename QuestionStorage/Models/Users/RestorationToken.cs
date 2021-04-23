using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.Users
{
    public class RestorationToken
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public bool Expired { get; set; }
        
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Users.User.RestorationTokens))]
        public virtual User User { get; set; }
    }
}