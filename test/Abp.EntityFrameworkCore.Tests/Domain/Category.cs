using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Abp.EntityFrameworkCore.Tests.Domain;

public class Category : FullAuditedEntity
{
    public string Name { get; set; }

    public ICollection<Product> Products { get; set; }

    // Owned dependent. EF Core lifetime-couples it with Category, so CascadeSoftDelete
    // must not enumerate it as a cascade child.
    public CategoryProfile Profile { get; set; }

    // Restrict-configured navigation. CascadeSoftDelete must skip this navigation even
    // when EnableSoftDeleteCascade is on.
    public ICollection<CategoryAudit> Audits { get; set; }

    // Self-reference: a Category may have a parent Category and its own Children. Exercises
    // the BFS cycle guard and the descent into same-typed soft-delete descendants.
    public int? ParentId { get; set; }

    public Category Parent { get; set; }

    public ICollection<Category> Children { get; set; }

    public Category()
    {
        Products = new List<Product>();
        Audits = new List<CategoryAudit>();
        Children = new List<Category>();
    }
}

// Soft-deletable child with a required relationship to Category (=> Cascade delete behavior).
public class Product : FullAuditedEntity
{
    public string Name { get; set; }

    [Required]
    public Category Category { get; set; }

    public int CategoryId { get; set; }

    // Hard-delete child (plain Entity) with a required relationship to Product (=> Cascade).
    public ICollection<ProductTag> Tags { get; set; }

    public Product()
    {
        Tags = new List<ProductTag>();
    }
}

public class ProductTag : Entity
{
    public string Name { get; set; }

    [Required]
    public Product Product { get; set; }

    public int ProductId { get; set; }
}

// Owned value object on Category. Must be skipped by CascadeSoftDelete.
public class CategoryProfile
{
    public string Description { get; set; }
}

// Restrict-configured dependent. Even with EnableSoftDeleteCascade on, the cascade walk
// must not touch this entity.
public class CategoryAudit : Entity
{
    public string Action { get; set; }

    [Required]
    public Category Category { get; set; }

    public int CategoryId { get; set; }
}

// Three-level chain used to pin the "soft -> non-soft -> soft" known limitation: Order is
// soft-deletable, OrderLine is a plain Entity (hard-delete leaf for EF Core), and
// OrderLineNote is soft-deletable again. EnableSoftDeleteCascade cannot rescue the
// OrderLineNote here: OrderLine stays Deleted, EF Core's OnSaveChanges cascade then
// re-asserts cascade on OrderLineNote and hard-deletes it together with OrderLine.
public class Order : FullAuditedEntity
{
    public string Code { get; set; }

    public ICollection<OrderLine> Lines { get; set; }

    public Order()
    {
        Lines = new List<OrderLine>();
    }
}

public class OrderLine : Entity
{
    public string Sku { get; set; }

    [Required]
    public Order Order { get; set; }

    public int OrderId { get; set; }

    public ICollection<OrderLineNote> Notes { get; set; }

    public OrderLine()
    {
        Notes = new List<OrderLineNote>();
    }
}

public class OrderLineNote : FullAuditedEntity
{
    public string Text { get; set; }

    [Required]
    public OrderLine Line { get; set; }

    public int LineId { get; set; }
}
