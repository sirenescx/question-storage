using Microsoft.EntityFrameworkCore;
using QuestionStorage.Models.Courses;
using QuestionStorage.Models.Questions;
using QuestionStorage.Models.Quizzes;
using QuestionStorage.Models.Roles;
using QuestionStorage.Models.Tags;
using QuestionStorage.Models.Types;
using QuestionStorage.Models.Users;

namespace QuestionStorage.Models
{
    public class StorageContext : DbContext
    {
        public StorageContext()
        {
        }

        public StorageContext(DbContextOptions<StorageContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CoursesInfo> CoursesInfo { get; set; }
        public virtual DbSet<QuestionAnswerVariants> QuestionAnswerVariants { get; set; }
        public virtual DbSet<QuestionsInfo> QuestionsInfo { get; set; }
        public virtual DbSet<QuizzesInfo> QuizzesInfo { get; set; }
        public virtual DbSet<QuizzesInfoQuestionsInfo> QuizzesInfoQuestionsInfo { get; set; }

        public virtual DbSet<QuizzesInfoQuestionsInfoQuestionAnswerVariants>
            QuizzesInfoQuestionsInfoQuestionAnswerVariants { get; set; }

        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<TagsInfo> TagsInfo { get; set; }
        public virtual DbSet<TagsQuestions> TagsQuestions { get; set; }
        public virtual DbSet<TypesInfo> TypesInfo { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UsersCourses> UsersCourses { get; set; }
        
        public virtual DbSet<RestorationTokens> RestorationTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CoursesInfo>(entity =>
            {
                entity.HasKey(e => e.CourseId)
                    .HasName("PK__CoursesI__C92D7187DA609270");

                entity.Property(e => e.CourseId)
                    .HasColumnName("CourseID")
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<QuestionAnswerVariants>(entity =>
            {
                entity.HasKey(e => e.VariantId);

                entity.Property(e => e.VariantId)
                    .HasColumnName("VariantID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Answer)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.QuestId).HasColumnName("QuestID");

                entity.HasOne(d => d.Quest)
                    .WithMany(p => p.QuestionAnswerVariants)
                    .HasForeignKey(d => d.QuestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuestionAnswerVariants_QuestionsInfo");
            });

            modelBuilder.Entity<QuestionsInfo>(entity =>
            {
                entity.HasKey(e => e.QuestId);

                entity.Property(e => e.QuestId)
                    .HasColumnName("QuestID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AuthorId).HasDefaultValueSql("((1))");

                entity.Property(e => e.CourseId)
                    .HasColumnName("CourseID")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.QuestionName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.SourceQuestId).HasColumnName("SourceQuestID");

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.Property(e => e.VersionId).HasColumnName("VersionID");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.QuestionsInfo)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuestionsInfo_TypesInfo");
            });

            modelBuilder.Entity<QuizzesInfo>(entity =>
            {
                entity.HasKey(e => e.QuizId)
                    .HasName("PK_QuizesInfo");

                entity.Property(e => e.QuizId)
                    .HasColumnName("QuizID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Date)
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name)
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<QuizzesInfoQuestionsInfo>(entity =>
            {
                entity.HasKey(e => new {e.QuizId, e.QuestId})
                    .HasName("PK_QuizesInfo_QuestionsInfo");

                entity.ToTable("QuizzesInfo_QuestionsInfo");

                entity.Property(e => e.QuizId).HasColumnName("QuizID");

                entity.Property(e => e.QuestId).HasColumnName("QuestID");

                entity.Property(e => e.CodeSort).HasMaxLength(16);

                entity.HasOne(d => d.Quest)
                    .WithMany(p => p.QuizzesInfoQuestionsInfo)
                    .HasForeignKey(d => d.QuestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuizesInfo_QuestionsInfo_QuestQuest");

                entity.HasOne(d => d.Quiz)
                    .WithMany(p => p.QuizzesInfoQuestionsInfo)
                    .HasForeignKey(d => d.QuizId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuizesInfo_QuestionsInfo_QuestQuizInfo");
            });

            modelBuilder.Entity<QuizzesInfoQuestionsInfoQuestionAnswerVariants>(entity =>
            {
                entity.HasKey(e => new {e.QuizId, e.VariantId})
                    .HasName("PK_QuizesInfo_QuestionsInfo_QuestionAnswerVariants");

                entity.ToTable("QuizzesInfo_QuestionsInfo_QuestionAnswerVariants");

                entity.Property(e => e.QuizId).HasColumnName("QuizID");

                entity.Property(e => e.VariantId).HasColumnName("VariantID");

                entity.HasOne(d => d.Quiz)
                    .WithMany(p => p.QuizzesInfoQuestionsInfoQuestionAnswerVariants)
                    .HasForeignKey(d => d.QuizId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuizesInfo_QuestionsInfo_QuestionAnswerVariants_QuizesInfo");

                entity.HasOne(d => d.Variant)
                    .WithMany(p => p.QuizzesInfoQuestionsInfoQuestionAnswerVariants)
                    .HasForeignKey(d => d.VariantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QuizesInfo_QuestionsInfo_QuestionAnswerVariants_QuestionAnswerVariants");
            });

            modelBuilder.Entity<TagsInfo>(entity =>
            {
                entity.HasKey(e => e.TagId);

                entity.Property(e => e.TagId)
                    .HasColumnName("TagID")
                    .ValueGeneratedNever();

                entity.Property(e => e.CourseId)
                    .HasColumnName("CourseID")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.ParentId).HasColumnName("ParentID");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_TagsInfo_TagsInfo");
            });

            modelBuilder.Entity<TagsQuestions>(entity =>
            {
                entity.HasKey(e => new {e.TagId, e.QuestId});

                entity.ToTable("Tags_Questions");

                entity.Property(e => e.TagId).HasColumnName("TagID");

                entity.Property(e => e.QuestId).HasColumnName("QuestID");

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
                entity.HasKey(e => e.TypeId);

                entity.Property(e => e.TypeId)
                    .HasColumnName("TypeID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Comment).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<User>(entity => { entity.Property(e => e.Id).ValueGeneratedNever(); });

            modelBuilder.Entity<UsersCourses>(entity =>
            {
                entity.HasKey(e => new {e.UserId, e.CourseId});

                entity.ToTable("Users_Courses");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.CourseId).HasColumnName("CourseID");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.UsersCourses)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Courses_Courses");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UsersCourses)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Users_Users");
            });

            modelBuilder.Entity<RestorationTokens>(entity =>
                {
                    entity.HasKey(e => e.Id).
                        HasName("PK__Restorat__3214EC0784C0132D");

                    entity.ToTable("RestorationTokens");

                    entity.Property(e => e.Id)
                        .ValueGeneratedOnAdd();
                    
                    entity.Property(e => e.Expired)
                        .ValueGeneratedOnAdd();
                }
            );
        }
    }
}