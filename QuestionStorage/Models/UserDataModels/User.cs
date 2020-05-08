using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.UserDataModels
{
    public class User 
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int? RoleId { get; set; }
        [ForeignKey(nameof(RoleId))]
        [InverseProperty(nameof(UserDataModels.Role.Users))]
        public Role Role { get; set; }
    }
}