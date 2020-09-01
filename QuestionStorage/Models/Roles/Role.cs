using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using QuestionStorage.Models.Users;

namespace QuestionStorage.Models.Roles
{
    public class Role
    {
        public Role()
        {
            Users = new List<User>();
        }
        
        public int Id { get; set; }
        public string Name { get; set; }
        [InverseProperty("Role")]
        public virtual ICollection<User> Users { get; set; }
    }
}