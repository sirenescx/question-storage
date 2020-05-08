using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace QuestionStorage.Models.QuizzesQuestionsModels
{
    public partial class HSE_QuestContext : DbContext
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
        public virtual DbSet<QuizesInfo> QuizesInfo { get; set; }
        public virtual DbSet<QuizesInfoQuestionsInfo> QuizesInfoQuestionsInfo { get; set; }
        public virtual DbSet<QuizesInfoQuestionsInfoQuestionAnswerVariants> QuizesInfoQuestionsInfoQuestionAnswerVariants { get; set; }
        public virtual DbSet<RubricsInfo> RubricsInfo { get; set; }
        public virtual DbSet<TagsInfo> TagsInfo { get; set; }
        public virtual DbSet<TagsQuestions> TagsQuestions { get; set; }
        public virtual DbSet<TypesInfo> TypesInfo { get; set; }

//         protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//             if (!optionsBuilder.IsConfigured)
//             {
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
//                 optionsBuilder.UseSqlServer(/* connection string from appsettings.json*/);
//             }
//         }

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

                entity.HasOne(d => d.Rubric)
                    .WithMany(p => p.QuestionsInfo)
                    .HasForeignKey(d => d.RubricId)
                    .HasConstraintName("FK_QuestionsInfo_RubricsInfo");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.QuestionsInfo)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuestionsInfo_TypesInfo");
            });

            modelBuilder.Entity<QuizesInfo>(entity =>
            {
                entity.Property(e => e.QuizId).ValueGeneratedNever();

                entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<QuizesInfoQuestionsInfo>(entity =>
            {
                entity.HasKey(e => new { e.QuizId, e.QuestId });

                entity.Property(e => e.CodeSort).IsUnicode(false);

                entity.HasOne(d => d.Quest)
                    .WithMany(p => p.QuizesInfoQuestionsInfo)
                    .HasForeignKey(d => d.QuestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuizesInfo_QuestionsInfo_QuestQuest");

                entity.HasOne(d => d.Quiz)
                    .WithMany(p => p.QuizesInfoQuestionsInfo)
                    .HasForeignKey(d => d.QuizId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuizesInfo_QuestionsInfo_QuestQuizInfo");
            });

            modelBuilder.Entity<QuizesInfoQuestionsInfoQuestionAnswerVariants>(entity =>
            {
                entity.HasKey(e => new { e.QuizId, e.VariantId });

                entity.HasOne(d => d.Quiz)
                    .WithMany(p => p.QuizesInfoQuestionsInfoQuestionAnswerVariants)
                    .HasForeignKey(d => d.QuizId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuizesInfo_QuestionsInfo_QuestionAnswerVariants_QuizesInfo");

                entity.HasOne(d => d.Variant)
                    .WithMany(p => p.QuizesInfoQuestionsInfoQuestionAnswerVariants)
                    .HasForeignKey(d => d.VariantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuizesInfo_QuestionsInfo_QuestionAnswerVariants_QuestionAnswerVariants");
            });

            modelBuilder.Entity<RubricsInfo>(entity =>
            {
                entity.Property(e => e.RubricId).ValueGeneratedNever();

                entity.Property(e => e.Code)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.RubricName)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_RubricsInfo_RubricsInfo");
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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
