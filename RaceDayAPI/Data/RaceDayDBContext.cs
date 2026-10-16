
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Model;

namespace RaceDayAPI.Data
{
    public class RaceDayDBContext : DbContext
    {
        public RaceDayDBContext(
            DbContextOptions<RaceDayDBContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Participant> Participants => Set<Participant>();
        public DbSet<Organizer> Organizers => Set<Organizer>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Route_> Routes => Set<Route_>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<EventCategory> EventCategories => Set<EventCategory>();
        public DbSet<Entry> Entries => Set<Entry>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================
            // TABLE NAMES
            // =========================================

            modelBuilder.Entity<Account>().ToTable("Account");
            modelBuilder.Entity<Participant>().ToTable("Participant");
            modelBuilder.Entity<Organizer>().ToTable("Organizer");
            modelBuilder.Entity<Event>().ToTable("Event");
            modelBuilder.Entity<Route_>().ToTable("Route");
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<EventCategory>().ToTable("Event_Category");
            modelBuilder.Entity<Entry>().ToTable("Entry");
            modelBuilder.Entity<Result>().ToTable("Result");


            // =========================================
            // ACCOUNT
            // =========================================

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(x => x.AccountID);

                entity.Property(x => x.AccountID)
                    .ValueGeneratedOnAdd();

                entity.Property(x => x.UserName)
                    .HasMaxLength(55)
                    .IsRequired();

                entity.Property(x => x.PasswordHash)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.UserEmail)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.AccountType)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasIndex(x => x.UserName)
                    .IsUnique();

                entity.HasIndex(x => x.UserEmail)
                    .IsUnique();

                entity.ToTable(table =>
                    table.HasCheckConstraint(
                        "CK_Account_Type",
                        "[AccountType] IN ('Organizer', 'Participant')"));
            });


            // =========================================
            // PARTICIPANT
            // =========================================

            modelBuilder.Entity<Participant>(entity =>
            {
                entity.HasKey(x => x.ParticipantID);

                entity.Property(x => x.ParticipantID)
                    .ValueGeneratedOnAdd();

                entity.Property(x => x.Name)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Surname)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasIndex(x => x.AccountID)
                    .IsUnique();

                entity.ToTable(table =>
                    table.HasCheckConstraint(
                        "CK_Participant_Age",
                        "[Age] >= 18"));

                // One Account has at most one Participant profile.
                entity.HasOne(x => x.Account)
                    .WithOne(x => x.Participant)
                    .HasForeignKey<Participant>(x => x.AccountID)
                    .OnDelete(DeleteBehavior.NoAction);

                // One Participant can have multiple Entries.
                entity.HasMany(x => x.Entries)
                    .WithOne(x => x.Participant)
                    .HasForeignKey(x => x.ParticipantID)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // =========================================
            // ORGANIZER
            // =========================================

            modelBuilder.Entity<Organizer>(entity =>
            {
                entity.HasKey(x => x.OrganizerID);

                entity.Property(x => x.OrganizerID)
                    .ValueGeneratedOnAdd();

                entity.Property(x => x.OrganizationName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Address)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(x => x.AccountID)
                    .IsUnique();

                // One Account has at most one Organizer profile.
                entity.HasOne(x => x.Account)
                    .WithOne(x => x.Organizer)
                    .HasForeignKey<Organizer>(x => x.AccountID)
                    .OnDelete(DeleteBehavior.NoAction);

                // One Organizer can create multiple Events.
                entity.HasMany(x => x.Events)
                    .WithOne(x => x.Organizer)
                    .HasForeignKey(x => x.OrganizerID)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // =========================================
            // EVENT
            // =========================================

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(x => x.EventID);

                entity.Property(x => x.EventID)
                    .ValueGeneratedOnAdd();

                entity.Property(x => x.EventName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.Location)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.EventDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.OpenDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.ClosingDate)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(x => x.EventType)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.Distance)
                    .HasPrecision(6, 2)
                    .IsRequired();

                entity.ToTable(table =>
                    table.HasCheckConstraint(
                        "CK_Event_Type",
                        "[EventType] IN ('Run', 'Walk', 'Cycle')"));

                // One Organizer can own multiple Events.
                entity.HasOne(x => x.Organizer)
                    .WithMany(x => x.Events)
                    .HasForeignKey(x => x.OrganizerID)
                    .OnDelete(DeleteBehavior.NoAction);

                // One Event can have multiple Routes.
                entity.HasMany(x => x.Routes)
                    .WithOne(x => x.Event)
                    .HasForeignKey(x => x.EventID)
                    .OnDelete(DeleteBehavior.NoAction);

                // One Event can have multiple Entries.
                entity.HasMany(x => x.Entries)
                    .WithOne(x => x.Event)
                    .HasForeignKey(x => x.EventID)
                    .OnDelete(DeleteBehavior.NoAction);

                // Events and Categories have a many-to-many
                // relationship through EventCategory.
                entity.HasMany(x => x.EventCategories)
                    .WithOne(x => x.Event)
                    .HasForeignKey(x => x.EventID)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // =========================================
            // ROUTE
            // =========================================

            modelBuilder.Entity<Route_>(entity =>
            {
                entity.HasKey(x => x.RouteID);

                entity.Property(x => x.RouteID)
                    .ValueGeneratedOnAdd();

                entity.Property(x => x.RouteName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Difficulty)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(x => x.Distance)
                    .HasPrecision(6, 2)
                    .IsRequired();

                // Each Route belongs to one Event.
                entity.HasOne(x => x.Event)
                    .WithMany(x => x.Routes)
                    .HasForeignKey(x => x.EventID)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // =========================================
            // CATEGORY
            // =========================================

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(x => x.CategoryID);

                entity.Property(x => x.CategoryID)
                    .ValueGeneratedOnAdd();

                entity.Property(x => x.CategoryName)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Distance)
                    .HasPrecision(6, 2);

                // MinimumAge, MaximumAge and Distance
                // are nullable category properties.
                //
                // No Result relationship is configured here.
                // The Category is accessed through Entry.Category.

                entity.HasMany(x => x.EventCategories)
                    .WithOne(x => x.Category)
                    .HasForeignKey(x => x.CategoryID)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // =========================================
            // EVENT_CATEGORY JUNCTION TABLE
            // =========================================

            modelBuilder.Entity<EventCategory>(entity =>
            {
                // Composite primary key.
                entity.HasKey(x => new
                {
                    x.EventID,
                    x.CategoryID
                });

                entity.HasOne(x => x.Event)
                    .WithMany(x => x.EventCategories)
                    .HasForeignKey(x => x.EventID)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.Category)
                    .WithMany(x => x.EventCategories)
                    .HasForeignKey(x => x.CategoryID)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // =========================================
            // ENTRY
            // =========================================

            modelBuilder.Entity<Entry>(entity =>
            {
                entity.HasKey(x => x.EntryID);

                entity.Property(x => x.EntryID)
                    .ValueGeneratedOnAdd();

                entity.Property(x => x.Price)
                    .HasPrecision(10, 2)
                    .IsRequired();

                // A participant can register for an event
                // only once.
                entity.HasIndex(x => new
                {
                    x.EventID,
                    x.ParticipantID
                }).IsUnique();

                // Each Entry belongs to one Event.
                entity.HasOne(x => x.Event)
                    .WithMany(x => x.Entries)
                    .HasForeignKey(x => x.EventID)
                    .OnDelete(DeleteBehavior.NoAction);

                // Each Entry belongs to one Participant.
                entity.HasOne(x => x.Participant)
                    .WithMany(x => x.Entries)
                    .HasForeignKey(x => x.ParticipantID)
                    .OnDelete(DeleteBehavior.NoAction);

                // Each Entry selects one Category.
                entity.HasOne(x => x.Category)
                    .WithMany()
                    .HasForeignKey(x => x.CategoryID)
                    .OnDelete(DeleteBehavior.NoAction);

                // An Entry can optionally select one Route.
                entity.HasOne(x => x.Route)
                    .WithMany()
                    .HasForeignKey(x => x.RouteID)
                    .OnDelete(DeleteBehavior.NoAction);

                // One Entry can have at most one Result.
                entity.HasOne(x => x.Result)
                    .WithOne(x => x.Entry)
                    .HasForeignKey<Result>(x => x.EntryID)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // =========================================
            // RESULT
            // =========================================

            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(x => x.ResultID);

                entity.Property(x => x.ResultID)
                    .ValueGeneratedOnAdd();

                entity.Property(x => x.TimeCompleted)
                    .HasColumnType("time")
                    .IsRequired();

                entity.Property(x => x.FinishingPosition)
                    .IsRequired();

                // EntryID must be unique because one Entry
                // can have only one Result.
                entity.HasIndex(x => x.EntryID)
                    .IsUnique();

                // Result belongs to Entry.
                // The Category is available through Entry.Category.
            });
        }
    }
}






