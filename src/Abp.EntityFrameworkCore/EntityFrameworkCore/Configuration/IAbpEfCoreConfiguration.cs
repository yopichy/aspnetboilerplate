using System;
using Microsoft.EntityFrameworkCore;

namespace Abp.EntityFrameworkCore.Configuration;

public interface IAbpEfCoreConfiguration
{
    public bool UseAbpQueryCompiler { get; set; }

    /// <summary>
    /// When enabled, soft-deleting an <see cref="Abp.Domain.Entities.ISoftDelete"/> entity also
    /// cascades to its <see cref="DeleteBehavior.Cascade"/> / <see cref="DeleteBehavior.ClientCascade"/>
    /// children: soft-delete children are soft-deleted, others are hard-deleted. Children are
    /// discovered through the parent's collection / reference navigations, so declare both sides
    /// of the relationship — a foreign key without a principal-side navigation will be missed.
    /// Unloaded navigations are loaded synchronously even on the <c>SaveChangesAsync</c> path;
    /// preload them with <c>Include</c> when blocking I/O is a concern. Default: <c>false</c>.
    /// See <c>AbpDbContext.CascadeSoftDelete</c> for the exact walk and the known limitations.
    /// </summary>
    public bool EnableSoftDeleteCascade { get; set; }

    void AddDbContext<TDbContext>(Action<AbpDbContextConfiguration<TDbContext>> action)
        where TDbContext : DbContext;
}

public class NullAbpEfCoreConfiguration : IAbpEfCoreConfiguration
{
    /// <summary>
    /// Gets single instance of <see cref="NullAbpEfCoreConfiguration"/> class.
    /// </summary>
    public static NullAbpEfCoreConfiguration Instance { get; } = new NullAbpEfCoreConfiguration();

    public bool UseAbpQueryCompiler { get; set; }

    public bool EnableSoftDeleteCascade { get; set; }

    public void AddDbContext<TDbContext>(Action<AbpDbContextConfiguration<TDbContext>> action) where TDbContext : DbContext
    {

    }
}