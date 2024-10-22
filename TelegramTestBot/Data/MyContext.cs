using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TelegramTestBot.Models;

namespace TelegramTestBot.Data
{
    public class MyContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DbSet<UserProfile> Users { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<ActionButton> Buttons { get; set; }
        public DbSet<UserEvent?> UserEvents { get; set; }

        public MyContext(DbContextOptions<MyContext> options, IConfiguration configuration)
            : base(options)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserProfile>()
                .HasMany(p => p.Invites)
                .WithOne(i => i.CreatedByUserProfile)
                .HasForeignKey(i => i.CreatedByUserProfileId);

            modelBuilder.Entity<UserProfile>()
                .HasMany(u => u.UserEvents)
                .WithOne(ue => ue.User)
                .HasForeignKey(ue => ue.UserId);

            modelBuilder.Entity<ActionButton>().Property(_ => _.ButtonEvent)
                .HasConversion(new EnumToStringConverter<EventType>());
            modelBuilder.Entity<ActionButton>().Property(_ => _.BusinessProcess)
                .HasConversion(new EnumToStringConverter<BusinessProcess>());

            modelBuilder.Entity<UserEvent>().Property(_ => _.EventType)
                .HasConversion(new EnumToStringConverter<EventType>());
            modelBuilder.Entity<UserEvent>().Property(_ => _.BusinessProcess)
                .HasConversion(new EnumToStringConverter<BusinessProcess>());

            modelBuilder.Entity<UserProfile>().Property(_ => _.ModerationStatus)
                .HasConversion(new EnumToStringConverter<ProfileModerationStatus>());
            modelBuilder.Entity<UserProfile>().Property(_ => _.Role)
                .HasConversion(new EnumToStringConverter<UserRole>());

            modelBuilder.Entity<Invite>().Property(_ => _.Role)
                .HasConversion(new EnumToStringConverter<UserRole>());
        }
    }
}