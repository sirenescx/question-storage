using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace QuestionStorage.Models.QuizzesQuestionsModels
{
    public class HSE_QuestContext : DbContext
    {
        public HSE_QuestContext()
        {
       
        }

        public HSE_QuestContext(DbContextOptions<HSE_QuestContext> options)
            : base(options)
        {
        }

        public virtual DbSet<QuestionAnswerVariants> QuestionAnswerVariants { get; set; }
        public virtual DbSet<QuestionsInfo> QuestionsInfo { get; set; }

        public virtual DbSet<TagsInfo> TagsInfo { get; set; }
        public virtual DbSet<TagsQuestions> TagsQuestions { get; set; }
        public virtual DbSet<TypesInfo> TypesInfo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QuestionAnswerVariants>(entity =>
            {
                entity.Property(e => e.VariantId).ValueGeneratedNever();

                entity.HasOne(d => d.Quest)
                    .WithMany(p => p.QuestionAnswerVariants)
                    .HasForeignKey(d => d.QuestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuestionAnswerVariants_QuestionsInfo");
            });

            modelBuilder.Entity<QuestionsInfo>(entity =>
            {
                entity.Property(e => e.QuestId).ValueGeneratedNever();

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.QuestionsInfo)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuestionsInfo_TypesInfo");
            });

            modelBuilder.Entity<TagsInfo>(entity =>
            {
                entity.Property(e => e.TagId).ValueGeneratedNever();

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_TagsInfo_TagsInfo");
            });

            modelBuilder.Entity<TagsQuestions>(entity =>
            {
                entity.HasKey(e => new { e.TagId, e.QuestId });

                entity.HasOne(d => d.Quest)
                    .WithMany(p => p.TagsQuestions)
                    .HasForeignKey(d => d.QuestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tags_Questions_QuestionsInfo");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.TagsQuestions)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tags_Questions_TagsInfo");
            });

            modelBuilder.Entity<TypesInfo>(entity =>
            {
                entity.Property(e => e.TypeId).ValueGeneratedNever();
            });
        }
    }
}
