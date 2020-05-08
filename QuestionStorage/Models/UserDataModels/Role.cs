using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.UserDataModels
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [InverseProperty("Role")]
        public virtual ICollection<User> Users { get; set; }
    }
}