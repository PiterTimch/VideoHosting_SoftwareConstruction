using Domain.Entities.Portfolio;
using Domain.Entities.Comments;
using Domain.Entities.Identity;
using Domain.Entities.Video;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Domain;

public class AppDbContext
    : IdentityDbContext<
        UserEntity,
        RoleEntity,
        long,
        IdentityUserClaim<long>,
        UserRoleEntity,
        UserLoginEntity,
        IdentityRoleClaim<long>,
        IdentityUserToken<long>
    >
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<CommentsEntity> Comments { get; set; }
    public DbSet<VideoEntity> Videos { get; set; }
    public DbSet<VideoPrivacyEntity> VideoPrivacies { get; set; }
    public DbSet<PortfolioEntity> Portfolios { get; set; }
    public DbSet<PortfolioSubscriberEntity> PortfolioSubscribers { get; set; }
    public DbSet<VideoReactionEntity> VideoReactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRoleEntity>(ur =>
        {
            ur.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(r => r.RoleId)
                .IsRequired();

            ur.HasOne(ur => ur.User)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(u => u.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<UserLoginEntity>(b =>
        {
            b.HasOne(l => l.User)
                .WithMany(u => u.UserLogins)
                .HasForeignKey(l => l.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<PortfolioEntity>().Property(c => c.Id).ValueGeneratedNever();

        modelBuilder.Entity<PortfolioEntity>(c =>
        {
            c.HasOne(c => c.Freelancer)
                .WithOne(u => u.Portfolio)
                .HasForeignKey<PortfolioEntity>(c => c.Id)
                .IsRequired();
        });

        modelBuilder.Entity<PortfolioSubscriberEntity>(cs =>
        {
            cs.HasKey(x => new { x.PortfolioId, x.UserId });

            cs.HasOne(x => x.Portfolio)
                .WithMany(c => c.Subscribers)
                .HasForeignKey(x => x.PortfolioId)
                .IsRequired();

            cs.HasOne(x => x.User)
                .WithMany(u => u.SubscribedPortfolios)
                .HasForeignKey(x => x.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<CommentsEntity>(entity =>
        {
            entity
                .HasOne(c => c.Video)
                .WithMany(v => v.Comments)
                .HasForeignKey(c => c.VideoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(c => c.Parent)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VideoReactionEntity>(vr =>
        {
            vr.HasOne(x => x.Video)
                .WithMany(v => v.VideoReactions)
                .HasForeignKey(x => x.VideoId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            vr.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
    }
}
