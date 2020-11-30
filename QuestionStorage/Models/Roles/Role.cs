using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using QuestionStorage.Models.Users;

namespace QuestionStorage.Models.Roles
{
    [Table("Roles")]
    public class Role
    {
        public Role()
        {
            Users = new List<User>();
        }
        
        [Column("Id")]
        public int Id { get; set; }
        
        [Column("Name")]
        public string Name { get; set; }
        
        [InverseProperty("Role")]
        public virtual ICollection<User> Users { get; set; }
    }
}
