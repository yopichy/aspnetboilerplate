using Abp.EntityFrameworkCore.Tests.Domain;
using Abp.EntityFrameworkCore.ValueConverters;
using Microsoft.EntityFrameworkCore;

namespace Abp.EntityFrameworkCore.Tests.Ef;

public class BloggingDbContext : AbpDbContext
{
    public DbSet<Blog> Blogs { get; set; }

    public DbSet<BlogView> BlogView { get; set; }

    public DbSet<Post> Posts { get; set; }

    public DbSet<Comment> Comments { get; set; }

    public DbSet<BlogCategory> BlogCategories { get; set; }

    public DbSet<SubBlogCategory> SubBlogCategories { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<ProductTag> ProductTags { get; set; }

    public DbSet<CategoryAudit> CategoryAudits { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderLine> OrderLines { get; set; }

    public DbSet<OrderLineNote> OrderLineNotes { get; set; }

    public BloggingDbContext(DbContextOptions<BloggingDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(b =>
        {
            b.OwnsOne(t => t.BlogTime, x =>
                {
                    x.Property(p => p.LastAccessTime).HasConversion(new AbpDateTimeValueConverter());
                });
        });

        modelBuilder
            .Entity<BlogView>()
            .HasNoKey()
            .ToView("BlogView");

        modelBuilder.Entity<Category>(b =>
        {
            b.OwnsOne(c => c.Profile);
            b.HasMany(c => c.Audits)
                .WithOne(a => a.Category)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasMany(c => c.Children)
                .WithOne(c => c.Parent)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.ClientCascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}