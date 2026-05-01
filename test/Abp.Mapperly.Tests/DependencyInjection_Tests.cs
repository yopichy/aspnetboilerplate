using System;
using Abp.Dependency;
using Abp.ObjectMapping;
using Abp.TestBase;
using Riok.Mapperly.Abstractions;
using Shouldly;
using Xunit;

namespace Abp.Mapperly.Tests;

// ---------- Entities ----------

public class ProductEntity
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal RawPrice { get; set; }
}

public class ProductDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// ---------- Supporting service (auto-registered via ITransientDependency) ----------

/// <summary>
/// A transient service injected into <see cref="ProductMapper"/> to demonstrate that
/// mappers participate fully in Castle Windsor's DI lifecycle.
/// </summary>
public class PriceCalculatorService : ITransientDependency
{
    public decimal CalculatePrice(decimal rawPrice) => Math.Round(rawPrice * 1.1m, 2);
}

// ---------- Mapper with constructor injection ----------

[Mapper]
public partial class ProductMapper : MapperBase<ProductEntity, ProductDto>
{
    private readonly PriceCalculatorService _priceCalculator;

    public ProductMapper(PriceCalculatorService priceCalculator)
    {
        _priceCalculator = priceCalculator;
    }

    [MapperIgnoreTarget(nameof(ProductDto.Price))]
    public override partial ProductDto Map(ProductEntity source);

    public override ProductDto MapTo(ProductEntity source, ProductDto destination)
    {
        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Price = _priceCalculator.CalculatePrice(source.RawPrice);
        return destination;
    }

    public override void AfterMap(ProductEntity source, ProductDto destination)
    {
        // Apply the calculated price after the generated Map() runs.
        destination.Price = _priceCalculator.CalculatePrice(source.RawPrice);
    }
}

// ---------- Tests ----------

public class DependencyInjection_Tests : AbpIntegratedTestBase<AbpMapperlyTestModule>
{
    private readonly IObjectMapper _objectMapper;

    public DependencyInjection_Tests()
    {
        _objectMapper = Resolve<IObjectMapper>();
    }

    [Fact]
    public void Mapper_Can_Accept_Injected_Services_Via_Constructor()
    {
        var entity = new ProductEntity { Id = "p1", Name = "Widget", RawPrice = 100m };

        var dto = _objectMapper.Map<ProductDto>(entity);

        dto.Id.ShouldBe("p1");
        dto.Name.ShouldBe("Widget");
        dto.Price.ShouldBe(110m); // 100 * 1.1 = 110
    }

    [Fact]
    public void Mapper_Injected_Service_Is_Used_In_MapTo_Overload()
    {
        var entity = new ProductEntity { Id = "p2", Name = "Gadget", RawPrice = 50m };
        var existing = new ProductDto();

        _objectMapper.Map<ProductEntity, ProductDto>(entity, existing);

        existing.Price.ShouldBe(55m); // 50 * 1.1 = 55
    }

    [Fact]
    public void Mapper_Is_Resolved_As_Transient()
    {
        var mapper1 = LocalIocManager.Resolve<IAbpMapper<ProductEntity, ProductDto>>();
        var mapper2 = LocalIocManager.Resolve<IAbpMapper<ProductEntity, ProductDto>>();

        // Transient scope: each resolution returns a different instance
        ReferenceEquals(mapper1, mapper2).ShouldBeFalse();

        LocalIocManager.Release(mapper1);
        LocalIocManager.Release(mapper2);
    }
}
