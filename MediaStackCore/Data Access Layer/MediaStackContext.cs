using MediaStackCore.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaStackCore.Data_Access_Layer
{
    public class MediaStackContext : DbContext
    {
        public DbSet<Media> Media { get; set; }

        public DbSet<Album> Albums { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Artist> Artists { get; set; }

        public MediaStackContext() => base.Database.EnsureCreated();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Media>()
                .HasMany(media => media.Tags);

            modelBuilder.Entity<Media>()
                .Property(entity => entity.Hash)
                .IsRequired();

            modelBuilder.Entity<Media>()
                .HasIndex(entity => entity.Hash)
                .IsUnique();

            modelBuilder.Entity<Media>()
                .Property(entity => entity.Type)
                .IsRequired();

            modelBuilder.Entity<Tag>()
                .HasKey(tag => tag.ID);

            modelBuilder.Entity<Tag>()
                .HasIndex(entity => entity.Name)
                .IsUnique();

            modelBuilder.Entity<Tag>()
                .Property(entity => entity.Name)
                .IsRequired();

            modelBuilder.Entity<Album>()
                .HasIndex(entity => new { entity.Name, entity.ArtistID })
                .IsUnique();

            modelBuilder.Entity<Album>()
                .Property(entity => entity.Name)
                .IsRequired();

            modelBuilder.Entity<Album>()
                .Property(entity => entity.ArtistID)
                .IsRequired();

            modelBuilder.Entity<Artist>()
                .HasIndex(entity => entity.Name)
                .IsUnique();

            modelBuilder.Entity<Artist>()
                .Property(entity => entity.Name)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .HasIndex(entity => entity.Name)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .Property(entity => entity.Name)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                optionsBuilder.UseSqlite($"Data Source=MediaStack.db;Cache=Shared");

            }

            base.OnConfiguring(optionsBuilder);
        }
    }
}
