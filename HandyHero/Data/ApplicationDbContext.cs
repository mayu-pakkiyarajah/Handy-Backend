using HandyHero.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace HandyHero.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

            
        }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chat { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<Complaint> Complaint { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<FieldWorker> FieldWorker { get; set; }
       public DbSet<Project> Project { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Review> Review { get; set; }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Chat Configurations
            modelBuilder.Entity<User>()
                .HasMany(u => u.ChatParticipants)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Participants)
                .WithOne(p => p.Chat)
                .HasForeignKey(p => p.ChatId);

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Chat)
                .HasForeignKey(m => m.ChatId);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            base.OnModelCreating(modelBuilder);
        }

    }
   
    }
