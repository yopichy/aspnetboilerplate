using Abp.ObjectMapping;
using Abp.TestBase;
using Riok.Mapperly.Abstractions;
using Shouldly;
using Xunit;

namespace Abp.Mapperly.Tests;

// ---------- Entities ----------

public class HookEntity
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class HookDto
{
    public string Id { get; set; }
    public string Name { get; set; }
}

// ---------- Mapper with lifecycle hooks ----------

[Mapper]
public partial class HookMapper : MapperBase<HookEntity, HookDto>
{
    public override partial HookDto Map(HookEntity source);

    public override HookDto MapTo(HookEntity source, HookDto destination)
    {
        destination.Id = source.Id;
        destination.Name = source.Name;
        return destination;
    }

    /// <summary>Prepends "BeforeMap:" to the source name before mapping starts.</summary>
    public override void BeforeMap(HookEntity source)
    {
        source.Name = "BeforeMap:" + source.Name;
    }

    /// <summary>Appends ":AfterMap" to the destination name after mapping finishes.</summary>
    public override void AfterMap(HookEntity source, HookDto destination)
    {
        destination.Name = destination.Name + ":AfterMap";
    }
}

// ---------- Tests ----------

public class BeforeAfterHooks_Tests : AbpIntegratedTestBase<AbpMapperlyTestModule>
{
    private readonly IObjectMapper _objectMapper;

    public BeforeAfterHooks_Tests()
    {
        _objectMapper = Resolve<IObjectMapper>();
    }

    [Fact]
    public void BeforeMap_And_AfterMap_Are_Called_When_Creating_New_Destination()
    {
        var entity = new HookEntity { Id = "1", Name = "Test" };

        var dto = _objectMapper.Map<HookDto>(entity);

        // BeforeMap prepends, Mapperly copies the modified name, AfterMap appends
        dto.Name.ShouldBe("BeforeMap:Test:AfterMap");
        dto.Id.ShouldBe("1");
    }

    [Fact]
    public void BeforeMap_And_AfterMap_Are_Called_When_Mapping_Onto_Existing_Destination()
    {
        var entity = new HookEntity { Id = "2", Name = "World" };
        var existing = new HookDto { Id = "old", Name = "old" };

        _objectMapper.Map<HookEntity, HookDto>(entity, existing);

        existing.Name.ShouldBe("BeforeMap:World:AfterMap");
        existing.Id.ShouldBe("2");
    }

    [Fact]
    public void Default_BeforeMap_And_AfterMap_Are_Noops_When_Not_Overridden()
    {
        // MyEntityMapper (in Mapperly_Tests.cs) does NOT override BeforeMap/AfterMap
        var entity = new MyEntity { Id = 99, Name = "NoHook" };

        var dto = _objectMapper.Map<MyDto>(entity);

        // Name is unchanged — no hooks modified it
        dto.Name.ShouldBe("NoHook");
        dto.Id.ShouldBe(99);
    }
}
