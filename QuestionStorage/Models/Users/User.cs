using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuestionStorage.Models.Roles;

namespace QuestionStorage.Models.Users
 {
     public class User 
     {
         public int Id { get; set; }
         [Required(ErrorMessage = "E-mail is required")]
         [MinLength(1)]
         public string Email { get; set; }
         public string Password { get; set; }
         public int? RoleId { get; set; }
         [ForeignKey(nameof(RoleId))]
         [InverseProperty(nameof(Roles.Role.Users))]
         public Role Role { get; set; }
     }
 }