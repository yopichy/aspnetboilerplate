using System;
using System.Collections.Generic;
using System.Linq;
using Abp.ObjectMapping;
using Abp.TestBase;
using Riok.Mapperly.Abstractions;
using Shouldly;
using Xunit;

namespace Abp.Mapperly.Tests;

// ---------- Entities ----------

public class OrderEntity
{
    public int Id { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
}

// ---------- Mapper implementing both IAbpMapper and IAbpMapperlyQueryableMapper ----------

[Mapper]
public partial class OrderMapper : MapperBase<OrderEntity, OrderDto>,
                                   IAbpMapperlyQueryableMapper<OrderEntity, OrderDto>
{
    public override partial OrderDto Map(OrderEntity source);

    public override OrderDto MapTo(OrderEntity source, OrderDto destination)
    {
        destination.Id = source.Id;
        destination.Description = source.Description;
        destination.Quantity = source.Quantity;
        return destination;
    }

    /// <inheritdoc/>
    public IQueryable<OrderDto> ProjectTo(IQueryable<OrderEntity> source)
        => source.Select(e => Map(e));
}

// ---------- Tests ----------

public class QueryableMapper_Tests : AbpIntegratedTestBase<AbpMapperlyTestModule>
{
    private readonly IObjectMapper _objectMapper;

    public QueryableMapper_Tests()
    {
        _objectMapper = Resolve<IObjectMapper>();
    }

    [Fact]
    public void IObjectMapper_ProjectTo_Should_Throw_NotSupportedException()
    {
        var query = new List<OrderEntity>().AsQueryable();

        Should.Throw<NotSupportedException>(() => _objectMapper.ProjectTo<OrderDto>(query));
    }

    [Fact]
    public void IAbpMapperlyQueryableMapper_ProjectTo_Returns_Projected_Queryable()
    {
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, Description = "First",  Quantity = 3 },
            new OrderEntity { Id = 2, Description = "Second", Quantity = 7 }
        }.AsQueryable();

        var mapper = LocalIocManager.Resolve<IAbpMapperlyQueryableMapper<OrderEntity, OrderDto>>();
        try
        {
            var result = mapper.ProjectTo(orders).ToList();

            result.Count.ShouldBe(2);
            result[0].Id.ShouldBe(1);
            result[0].Quantity.ShouldBe(3);
            result[1].Id.ShouldBe(2);
            result[1].Quantity.ShouldBe(7);
        }
        finally
        {
            LocalIocManager.Release(mapper);
        }
    }

    [Fact]
    public void IAbpMapperlyQueryableMapper_Supports_Filtering_On_Projected_Queryable()
    {
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = 1, Description = "Alpha", Quantity = 1 },
            new OrderEntity { Id = 2, Description = "Beta",  Quantity = 5 },
            new OrderEntity { Id = 3, Description = "Gamma", Quantity = 10 }
        }.AsQueryable();

        var mapper = LocalIocManager.Resolve<IAbpMapperlyQueryableMapper<OrderEntity, OrderDto>>();
        try
        {
            var result = mapper.ProjectTo(orders)
                               .Where(d => d.Quantity > 4)
                               .ToList();

            result.Count.ShouldBe(2);
            result.ShouldAllBe(d => d.Quantity > 4);
        }
        finally
        {
            LocalIocManager.Release(mapper);
        }
    }

    [Fact]
    public void Module_Registers_IAbpMapperlyQueryableMapper_In_IoC()
    {
        var mapper = LocalIocManager.Resolve<IAbpMapperlyQueryableMapper<OrderEntity, OrderDto>>();

        mapper.ShouldNotBeNull();
        mapper.ShouldBeOfType<OrderMapper>();

        LocalIocManager.Release(mapper);
    }

    [Fact]
    public void OrderMapper_Also_Registered_As_IAbpMapper()
    {
        var mapper = LocalIocManager.Resolve<IAbpMapper<OrderEntity, OrderDto>>();

        mapper.ShouldNotBeNull();
        mapper.ShouldBeOfType<OrderMapper>();

        LocalIocManager.Release(mapper);
    }
}
