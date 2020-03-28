using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models
{
    public partial class RubricsInfo
    {
        public RubricsInfo()
        {
            InverseParent = new HashSet<RubricsInfo>();
            QuestionsInfo = new HashSet<QuestionsInfo>();
        }

        [Key]
        [Column("RubricID")]
        public int RubricId { get; set; }
        [Column("ParentID")]
        public int? ParentId { get; set; }
        [Column("MainID")]
        public int MainId { get; set; }
        public int LevelNum { get; set; }
        public int LeafFlag { get; set; }
        [Required]
        [StringLength(32)]
        public string Code { get; set; }
        [Required]
        [StringLength(256)]
        public string RubricName { get; set; }

        [ForeignKey(nameof(ParentId))]
        [InverseProperty(nameof(RubricsInfo.InverseParent))]
        public virtual RubricsInfo Parent { get; set; }
        [InverseProperty(nameof(RubricsInfo.Parent))]
        public virtual ICollection<RubricsInfo> InverseParent { get; set; }
        [InverseProperty("Rubric")]
        public virtual ICollection<QuestionsInfo> QuestionsInfo { get; set; }
    }
}
