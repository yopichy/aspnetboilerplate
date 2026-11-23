using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Configuration;
using Abp.EntityFrameworkCore.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Abp.EntityFrameworkCore.Tests.Tests;

public class SoftDeleteCascade_Tests : EntityFrameworkCoreModuleTestBase
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductTag> _productTagRepository;
    private readonly IRepository<CategoryAudit> _categoryAuditRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderLine> _orderLineRepository;
    private readonly IRepository<OrderLineNote> _orderLineNoteRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IAbpEfCoreConfiguration _efCoreConfiguration;

    public SoftDeleteCascade_Tests()
    {
        _categoryRepository = Resolve<IRepository<Category>>();
        _productRepository = Resolve<IRepository<Product>>();
        _productTagRepository = Resolve<IRepository<ProductTag>>();
        _categoryAuditRepository = Resolve<IRepository<CategoryAudit>>();
        _orderRepository = Resolve<IRepository<Order>>();
        _orderLineRepository = Resolve<IRepository<OrderLine>>();
        _orderLineNoteRepository = Resolve<IRepository<OrderLineNote>>();
        _unitOfWorkManager = Resolve<IUnitOfWorkManager>();
        _efCoreConfiguration = Resolve<IAbpEfCoreConfiguration>();
    }

    private async Task<int> CreateCategoryWithChildrenAsync()
    {
        var categoryId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var category = new Category { Name = "cat-1" };
            var product = new Product { Name = "p1" };
            product.Tags.Add(new ProductTag { Name = "tag-1" });
            category.Products.Add(product);
            category.Products.Add(new Product { Name = "p2" });

            categoryId = await _categoryRepository.InsertAndGetIdAsync(category);
        });

        return categoryId;
    }

    [Fact]
    public async Task Should_Cascade_Soft_Delete_To_Children_When_Enabled()
    {
        _efCoreConfiguration.EnableSoftDeleteCascade = true;

        var categoryId = await CreateCategoryWithChildrenAsync();

        await WithUnitOfWorkAsync(async () => { await _categoryRepository.DeleteAsync(categoryId); });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                (await _categoryRepository.GetAsync(categoryId)).IsDeleted.ShouldBeTrue();

                var products = await _productRepository.GetAllListAsync();
                products.Count.ShouldBe(2);
                products.ShouldAllBe(p => p.IsDeleted);

                (await _productTagRepository.GetAllListAsync()).ShouldBeEmpty();
            }
        });
    }

    [Fact]
    public async Task Should_Not_Touch_Children_When_Disabled()
    {
        _efCoreConfiguration.EnableSoftDeleteCascade = false;

        var categoryId = await CreateCategoryWithChildrenAsync();

        await WithUnitOfWorkAsync(async () => { await _categoryRepository.DeleteAsync(categoryId); });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                (await _categoryRepository.GetAsync(categoryId)).IsDeleted.ShouldBeTrue();

                var products = await _productRepository.GetAllListAsync();
                products.Count.ShouldBe(2);
                products.ShouldAllBe(p => !p.IsDeleted);
            }
        });
    }

    [Fact]
    public async Task Should_Document_Known_Limitation_For_NonSoft_Middle_Node()
    {
        // Known limitation: ISoftDelete grandchildren reachable only through a non-soft middle node
        // cannot be rescued by EnableSoftDeleteCascade. The middle node stays Deleted, EF Core
        // re-asserts cascade on its dependents inside SaveChanges and hard-deletes the grandchild.
        // This test pins that behaviour so a future "fix" that claims to handle the chain has to
        // either update the docs or change this assertion deliberately.
        _efCoreConfiguration.EnableSoftDeleteCascade = true;

        var orderId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var order = new Order { Code = "ord-1" };
            var line = new OrderLine { Sku = "sku-1" };
            line.Notes.Add(new OrderLineNote { Text = "note-1" });
            order.Lines.Add(line);

            orderId = await _orderRepository.InsertAndGetIdAsync(order);
        });

        await WithUnitOfWorkAsync(async () => { await _orderRepository.DeleteAsync(orderId); });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                (await _orderRepository.GetAsync(orderId)).IsDeleted.ShouldBeTrue();

                // Middle node is gone (hard-deleted via EF Core's cascade), and so is the soft
                // grandchild it carried with it.
                (await _orderLineRepository.GetAllListAsync()).ShouldBeEmpty();
                (await _orderLineNoteRepository.GetAllListAsync()).ShouldBeEmpty();
            }
        });
    }

    [Fact]
    public async Task Should_Not_Cascade_Through_Restrict_Navigation()
    {
        _efCoreConfiguration.EnableSoftDeleteCascade = true;

        var categoryId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var category = new Category { Name = "cat-restrict" };
            category.Audits.Add(new CategoryAudit { Action = "created" });
            categoryId = await _categoryRepository.InsertAndGetIdAsync(category);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var category = await _categoryRepository.GetAsync(categoryId);
            await _categoryRepository.DeleteAsync(category);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                (await _categoryRepository.GetAsync(categoryId)).IsDeleted.ShouldBeTrue();
                (await _categoryAuditRepository.GetAllListAsync()).Count.ShouldBe(1);
            }
        });
    }

    [Fact]
    public async Task Should_Not_Treat_Owned_Navigation_As_Cascade_Child()
    {
        // Owned dependents share the principal's lifetime; the BFS must not try to cascade
        // them as if they were regular children. Reaching SaveChanges without an exception
        // is the contract this test guards.
        _efCoreConfiguration.EnableSoftDeleteCascade = true;

        var categoryId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var category = new Category
            {
                Name = "cat-owned",
                Profile = new CategoryProfile { Description = "desc" }
            };
            categoryId = await _categoryRepository.InsertAndGetIdAsync(category);
        });

        await WithUnitOfWorkAsync(async () => { await _categoryRepository.DeleteAsync(categoryId); });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                (await _categoryRepository.GetAsync(categoryId)).IsDeleted.ShouldBeTrue();
            }
        });
    }

    [Fact]
    public async Task Should_Document_Known_Limitation_For_HardDeleted_Parent()
    {
        // Known limitation: a hard-delete parent of ISoftDelete children cannot keep those
        // children alive. The parent really issues DELETE, EF Core's OnSaveChanges cascade then
        // forces every cascade-configured dependent to Deleted (regardless of any state ABP tried
        // to set) and they are hard-deleted with it. Pinning this behaviour here keeps the
        // limitation visible.
        _efCoreConfiguration.EnableSoftDeleteCascade = true;

        var categoryId = await CreateCategoryWithChildrenAsync();

        await WithUnitOfWorkAsync(async () =>
        {
            var category = await _categoryRepository.GetAsync(categoryId);
            await _categoryRepository.HardDeleteAsync(category);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                (await _categoryRepository.FirstOrDefaultAsync(categoryId)).ShouldBeNull();
                (await _productRepository.GetAllListAsync()).ShouldBeEmpty();
                (await _productTagRepository.GetAllListAsync()).ShouldBeEmpty();
            }
        });
    }

    [Fact]
    public async Task Should_Not_Restamp_Already_SoftDeleted_Child()
    {
        _efCoreConfiguration.EnableSoftDeleteCascade = true;

        var categoryId = await CreateCategoryWithChildrenAsync();

        await WithUnitOfWorkAsync(async () =>
        {
            var p1 = (await _productRepository.GetAllListAsync()).Single(p => p.Name == "p1");
            await _productRepository.DeleteAsync(p1);
        });

        System.DateTime? firstDeletionTime = null;
        long? firstDeleterUserId = null;
        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var p1 = (await _productRepository.GetAllListAsync()).Single(p => p.Name == "p1");
                firstDeletionTime = p1.DeletionTime;
                firstDeleterUserId = p1.DeleterUserId;
                firstDeletionTime.ShouldNotBeNull();
            }
        });

        // Bumping DeleterUserId proves the BFS would have restamped if it had touched the child
        // again. The early "already IsDeleted" guard in TrySetCascadeSoftDeleteChildAsDeleted is
        // supposed to keep both DeletionTime and DeleterUserId frozen at their first-deletion
        // values, independent of clock precision.
        AbpSession.UserId = 42;

        await WithUnitOfWorkAsync(async () =>
        {
            // Disable the soft-delete filter and pre-load Category.Products so the already-deleted
            // p1 actually reaches the BFS. With the filter enabled, p1 would be skipped at load
            // time and the "already IsDeleted" guard in TrySetCascadeSoftDeleteChildAsDeleted
            // would never get a chance to fire — making this test pass for the wrong reason.
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var category = await _categoryRepository.GetAll()
                    .Include(c => c.Products)
                    .SingleAsync(c => c.Id == categoryId);

                category.Products.ShouldContain(p => p.Name == "p1" && p.IsDeleted);

                await _categoryRepository.DeleteAsync(category);
            }
        });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var p1AfterParentDelete = (await _productRepository.GetAllListAsync())
                    .Single(p => p.Name == "p1");
                p1AfterParentDelete.IsDeleted.ShouldBeTrue();
                p1AfterParentDelete.DeletionTime.ShouldBe(firstDeletionTime);
                p1AfterParentDelete.DeleterUserId.ShouldBe(firstDeleterUserId);
            }
        });
    }

    [Fact]
    public async Task Should_Cascade_When_Navigation_Already_Loaded()
    {
        // When the navigation is already in memory the BFS takes the IsLoaded short-circuit
        // and must still cascade correctly without triggering an extra load.
        _efCoreConfiguration.EnableSoftDeleteCascade = true;

        var categoryId = await CreateCategoryWithChildrenAsync();

        await WithUnitOfWorkAsync(async () =>
        {
            var category = await _categoryRepository.GetAll()
                .Include(c => c.Products)
                .ThenInclude(p => p.Tags)
                .SingleAsync(c => c.Id == categoryId);

            category.Products.Count.ShouldBe(2);

            await _categoryRepository.DeleteAsync(category);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                (await _categoryRepository.GetAsync(categoryId)).IsDeleted.ShouldBeTrue();
                (await _productRepository.GetAllListAsync()).ShouldAllBe(p => p.IsDeleted);
                (await _productTagRepository.GetAllListAsync()).ShouldBeEmpty();
            }
        });
    }

    [Fact]
    public async Task Should_Detach_Added_Children_During_Cascade()
    {
        // Mirrors EF Core's CascadeDelete: an Added dependent of a deleted principal is detached
        // (the staged INSERT is rolled back) rather than persisted. So adding a Product to a
        // Category and deleting the Category in the same UoW leaves nothing behind.
        _efCoreConfiguration.EnableSoftDeleteCascade = true;

        var categoryId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            categoryId = await _categoryRepository.InsertAndGetIdAsync(new Category { Name = "added-child-cat" });
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var category = await _categoryRepository.GetAll()
                .Include(c => c.Products)
                .SingleAsync(c => c.Id == categoryId);

            category.Products.Add(new Product { Name = "added-child-p1" });

            await _categoryRepository.DeleteAsync(category);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                (await _categoryRepository.GetAsync(categoryId)).IsDeleted.ShouldBeTrue();

                (await _productRepository.GetAllListAsync())
                    .SingleOrDefault(p => p.Name == "added-child-p1")
                    .ShouldBeNull();
            }
        });
    }

    [Fact]
    public async Task Should_Document_That_Pending_Modification_Is_Dropped_On_Cascaded_Child()
    {
        // Matches ABP's existing behaviour for an explicit DeleteAsync(modifiedSoftDeleteEntity):
        // CancelDeletionForSoftDelete calls entry.Reload(), so a user's in-flight field changes on
        // a cascaded child are overwritten by the DB values; only the deletion audit survives.
        // Pinning this so a future change to CancelDeletionForSoftDelete is a conscious decision.
        _efCoreConfiguration.EnableSoftDeleteCascade = true;

        var categoryId = await CreateCategoryWithChildrenAsync();
        int p1Id = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            p1Id = (await _productRepository.GetAllListAsync()).Single(p => p.Name == "p1").Id;
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var p1 = await _productRepository.GetAsync(p1Id);
            p1.Name = "modified-by-user";
            await _productRepository.UpdateAsync(p1);

            await _categoryRepository.DeleteAsync(categoryId);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var p1 = await _productRepository.GetAsync(p1Id);
                p1.IsDeleted.ShouldBeTrue();
                p1.Name.ShouldBe("p1"); // user's "modified-by-user" was dropped by Reload()
            }
        });
    }

    [Fact]
    public async Task Should_Cascade_Self_Reference_Without_Loop()
    {
        // Self-referencing Category (Parent / Children navigation). Deleting the root must
        // terminate (BFS visited set keeps the cycle bounded), soft-delete every reachable
        // descendant Category, and leave unrelated siblings alone.
        _efCoreConfiguration.EnableSoftDeleteCascade = true;

        int rootId = 0, midId = 0, leafId = 0, sideId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var root = new Category { Name = "root" };
            var mid = new Category { Name = "mid", Parent = root };
            var leaf = new Category { Name = "leaf", Parent = mid };
            root.Children.Add(mid);
            mid.Children.Add(leaf);
            rootId = await _categoryRepository.InsertAndGetIdAsync(root);
            midId = mid.Id;
            leafId = leaf.Id;

            sideId = await _categoryRepository.InsertAndGetIdAsync(new Category { Name = "side" });
        });

        await WithUnitOfWorkAsync(async () => { await _categoryRepository.DeleteAsync(rootId); });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                (await _categoryRepository.GetAsync(rootId)).IsDeleted.ShouldBeTrue();
                (await _categoryRepository.GetAsync(midId)).IsDeleted.ShouldBeTrue();
                (await _categoryRepository.GetAsync(leafId)).IsDeleted.ShouldBeTrue();
                (await _categoryRepository.GetAsync(sideId)).IsDeleted.ShouldBeFalse();
            }
        });
    }

    [Fact]
    public void Should_Cascade_Through_Sync_SaveChanges_Path()
    {
        // Drives CascadeSoftDelete through the sync SaveChanges entry point.
        _efCoreConfiguration.EnableSoftDeleteCascade = true;

        var categoryId = 0;
        UsingDbContext(context =>
        {
            var category = new Category { Name = "sync-cat" };
            var product = new Product { Name = "sync-p1" };
            product.Tags.Add(new ProductTag { Name = "sync-tag-1" });
            category.Products.Add(product);
            context.Categories.Add(category);
            context.SaveChanges();
            categoryId = category.Id;
        });

        UsingDbContext(context =>
        {
            var category = context.Categories.Single(c => c.Id == categoryId);
            context.Categories.Remove(category);
            context.SaveChanges();
        });

        UsingDbContext(context =>
        {
            var deletedCategory = context.Categories.IgnoreQueryFilters().Single(c => c.Id == categoryId);
            deletedCategory.IsDeleted.ShouldBeTrue();

            var products = context.Products.IgnoreQueryFilters()
                .Where(p => p.CategoryId == categoryId).ToList();
            products.Count.ShouldBe(1);
            products.ShouldAllBe(p => p.IsDeleted);

            context.ProductTags.IgnoreQueryFilters()
                .Where(t => t.Product.CategoryId == categoryId).ToList().ShouldBeEmpty();
        });
    }

}
