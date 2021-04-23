using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using QuestionStorage.Models.Questions;

namespace QuestionStorage.Models.Types
{
    public class Type
    {
        public Type()
        {
            Questions = new HashSet<Question>();
        }

        public int id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        [InverseProperty("Type")]
        public virtual ICollection<Question> Questions { get; set; }
    }
}
