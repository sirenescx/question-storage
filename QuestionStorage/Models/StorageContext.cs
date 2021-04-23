using Microsoft.EntityFrameworkCore;
using QuestionStorage.Models.Courses;
using QuestionStorage.Models.Questions;
using QuestionStorage.Models.Quizzes;
using QuestionStorage.Models.Roles;
using QuestionStorage.Models.Tags;
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

        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<AnswerOption> AnswerOptions { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Quiz> Quizzes { get; set; }
        public virtual DbSet<QuizzesQuestions> QuizzesQuestions { get; set; }
        public virtual DbSet<QuizzesAnswerOptions> QuizzesAnswerOptions { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<TagsQuestions> TagsQuestions { get; set; }
        public virtual DbSet<Types.Type> Types { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UsersCourses> UsersCourses { get; set; }
        public virtual DbSet<RestorationToken> RestorationTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("courses");

                entity.HasKey(e => e.Id)
                    .HasName("PK_Courses");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<AnswerOption>(entity =>
            {
                entity.ToTable("answer_options");

                entity.HasKey(e => e.Id)
                    .HasName("PK_AnswerOptions");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.QuestionId)
                    .HasColumnName("question_id");

                entity.Property(e => e.Text)
                    .HasColumnName("text")
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.IsCorrect)
                    .HasColumnName("is_correct");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.AnswerOptions)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AnswerOptions_Questions");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("questions");

                entity.HasKey(e => e.Id)
                    .HasName("FK_Questions");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.TypeId)
                    .HasColumnName("type_id");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Text)
                    .HasColumnName("text");

                entity.Property(e => e.Xml)
                    .HasColumnName("xml");

                entity.Property(e => e.IsTemplate)
                    .HasColumnName("is_template")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.VersionId)
                    .HasColumnName("version_id")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.SourceId)
                    .HasColumnName("source_question_id");

                entity.Property(e => e.AuthorId)
                    .HasColumnName("author_id");

                entity.Property(e => e.CourseId)
                    .HasColumnName("course_id");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Questions_Types");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Questions_Courses");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Questions_Users");
            });

            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.ToTable("quizzes");

                entity.HasKey(e => e.Id)
                    .HasName("PK_Quizzes");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CourseId)
                    .HasColumnName("course_id");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Comment)
                    .HasColumnName("comment");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(64);

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Quizzes)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Quizzes_Courses");
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("tags");

                entity.HasKey(e => e.Id)
                    .HasName("PK_Tags");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ParentId)
                    .HasColumnName("parent_id");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.CourseId)
                    .HasColumnName("course_id");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Tags_Tags");
            });

            modelBuilder.Entity<TagsQuestions>(entity =>
            {
                entity.ToTable("tags_questions");

                entity.HasKey(e => new {e.TagId, QuestId = e.QuestionId})
                    .HasName("PK_Tags_Questions");

                entity.Property(e => e.TagId)
                    .HasColumnName("tag_id");

                entity.Property(e => e.QuestionId)
                    .HasColumnName("question_id");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.TagsQuestions)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Tags_Questions_Questions");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.TagsQuestions)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Tags_Questions_Tags");
            });

            modelBuilder.Entity<Types.Type>(entity =>
            {
                entity.ToTable("types");

                entity.HasKey(e => e.id)
                    .HasName("FK_Types");

                entity.Property(e => e.id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Comment)
                    .HasColumnName("comment")
                    .HasMaxLength(256);

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(e => e.Id)
                    .HasName("FK_Users");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .IsRequired();

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .IsRequired();

                entity.Property(e => e.RoleId)
                    .HasColumnName("role_id")
                    .HasDefaultValueSql("((2))");
            });

            modelBuilder.Entity<UsersCourses>(entity =>
            {
                entity.ToTable("users_courses");
                
                entity.HasKey(e => new {e.UserId, e.CourseId})
                    .HasName("PK_Users_Courses");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id");

                entity.Property(e => e.CourseId)
                    .HasColumnName("course_id");

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

            modelBuilder.Entity<RestorationToken>(entity =>
            {
                entity.ToTable("restoration_tokens");
                
                entity.HasKey(e => e.Id)
                    .HasName("PK_Restoration_Tokens");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Expired)
                    .HasColumnName("is_expired")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();
                
                entity.HasOne(d => d.User)
                    .WithMany(p => p.RestorationTokens)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Restoration_Tokens_Users");
            });

            modelBuilder.Entity<QuizzesQuestions>(entity =>
            {
                entity.ToTable("quizzes_questions");
                
                entity.HasKey(e => new {e.QuizId, QuestId = e.QuestionId})
                    .HasName("PK_Quizzes_Questions");

                entity.Property(e => e.QuizId)
                    .HasColumnName("quiz_id");

                entity.Property(e => e.QuestionId)
                    .HasColumnName("question_id");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.QuizzesQuestions)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Quizzes_Questions_Questions");

                entity.HasOne(d => d.Quiz)
                    .WithMany(p => p.QuizzesQuestions)
                    .HasForeignKey(d => d.QuizId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Quizzes_Quizzes");
            });

            modelBuilder.Entity<QuizzesAnswerOptions>(entity =>
            {
                entity.ToTable("quizzes_answer_options");
                
                entity.HasKey(e => new {e.QuizId, VariantId = e.AnswerOptionId})
                    .HasName("PK_Quizzes_Answer_Options");

                entity.Property(e => e.QuizId)
                    .HasColumnName("quiz_id");

                entity.Property(e => e.AnswerOptionId)
                    .HasColumnName("answer_option_id");

                entity.HasOne(d => d.Quiz)
                    .WithMany(p => p.QuizzesAnswerOptions)
                    .HasForeignKey(d => d.QuizId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Quizzes_Answer_Options_Quizzes");

                entity.HasOne(d => d.Option)
                    .WithMany(p => p.QuizzesAnswerOptions)
                    .HasForeignKey(d => d.AnswerOptionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Quizzes_Answer_Options_Answer_Options");
            });
        }
    }
}