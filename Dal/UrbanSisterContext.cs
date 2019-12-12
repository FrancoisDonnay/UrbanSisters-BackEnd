using Microsoft.EntityFrameworkCore;
using UrbanSisters.Model;

namespace UrbanSisters.Dal
{
    public partial class UrbanSisterContext : DbContext
    {
        public UrbanSisterContext()
        {
        }

        public UrbanSisterContext(DbContextOptions<UrbanSisterContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Appointment> Appointment { get; set; }
        public virtual DbSet<Availability> Availability { get; set; }
        public virtual DbSet<ChatMessage> ChatMessage { get; set; }
        public virtual DbSet<Event> Event { get; set; }
        public virtual DbSet<Participation> Participation { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<PortfolioPicture> PortfolioPicture { get; set; }
        public virtual DbSet<Relookeuse> Relookeuse { get; set; }
        public virtual DbSet<Tarif> Tarif { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("appointment");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Accepted).HasColumnName("accepted");

                entity.Property(e => e.CancelRaison)
                    .HasColumnName("cancel_raison")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("date");

                entity.Property(e => e.Finished).HasColumnName("finished");

                entity.Property(e => e.Makeup).HasColumnName("makeup");

                entity.Property(e => e.Mark).HasColumnName("mark");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.RelookeuseId).HasColumnName("relookeuse_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Appointment)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__appointme__perso__6C190EBB");

                entity.HasOne(d => d.Relookeuse)
                    .WithMany(p => p.Appointment)
                    .HasForeignKey(d => d.RelookeuseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__appointme__reloo__6D0D32F4");
            });

            modelBuilder.Entity<Availability>(entity =>
            {
                entity.ToTable("availability");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");

                entity.Property(e => e.EndTime)
                    .HasColumnName("end_time")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.RelookeuseId).HasColumnName("relookeuse_id");

                entity.Property(e => e.StartTime)
                    .HasColumnName("start_time")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.HasOne(d => d.Relookeuse)
                    .WithMany(p => p.Availability)
                    .HasForeignKey(d => d.RelookeuseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__availabil__reloo__66603565");
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.ToTable("chat_message");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("date");

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasColumnName("message")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.SenderId).HasColumnName("sender_id");

                entity.HasOne(d => d.Appointment)
                    .WithMany(p => p.ChatMessage)
                    .HasForeignKey(d => d.AppointmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__chat_mess__appoi__6FE99F9F");

                entity.HasOne(d => d.Sender)
                    .WithMany(p => p.ChatMessage)
                    .HasForeignKey(d => d.SenderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__chat_mess__sende__70DDC3D8");
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("event");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("date");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Picture)
                    .HasColumnName("picture")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("date");
            });

            modelBuilder.Entity<Participation>(entity =>
            {
                entity.HasKey(e => new { e.EventId, e.UserId })
                    .HasName("PK__particip__C63373AA14716218");

                entity.ToTable("participation");

                entity.Property(e => e.EventId).HasColumnName("event_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("date");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.Participation)
                    .HasForeignKey(d => d.EventId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__participa__event__5CD6CB2B");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Participation)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__participa__perso__5DCAEF64");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.HasIndex(e => e.Email)
                    .HasName("UQ__user__AB6E61646FD4C4D0")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("first_name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IsAdmin).HasColumnName("is_admin");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("last_name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PortfolioPicture>(entity =>
            {
                entity.ToTable("portfolio_picture");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Picture)
                    .IsRequired()
                    .HasColumnName("picture")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RelookeuseId).HasColumnName("relookeuse_id");

                entity.HasOne(d => d.Relookeuse)
                    .WithMany(p => p.PortfolioPicture)
                    .HasForeignKey(d => d.RelookeuseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__portfolio__reloo__693CA210");
            });

            modelBuilder.Entity<Relookeuse>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK__relookeu__543848DFE04B6146");

                entity.ToTable("relookeuse");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IsPro).HasColumnName("is_pro");

                entity.Property(e => e.Picture)
                    .HasColumnName("picture")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Relookeuse)
                    .HasForeignKey<Relookeuse>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__relookeus__perso__60A75C0F");
            });

            modelBuilder.Entity<Tarif>(entity =>
            {
                entity.HasKey(e => new { e.RelookeuseId, e.Service })
                    .HasName("PK__tarif__CA21AF477B4F0D44");

                entity.ToTable("tarif");

                entity.Property(e => e.RelookeuseId).HasColumnName("relookeuse_id");

                entity.Property(e => e.Service)
                    .HasColumnName("service")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.Relookeuse)
                    .WithMany(p => p.Tarif)
                    .HasForeignKey(d => d.RelookeuseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__tarif__relookeus__6383C8BA");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
