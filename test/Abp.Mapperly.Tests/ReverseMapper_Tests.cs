using Abp.ObjectMapping;
using Abp.TestBase;
using Riok.Mapperly.Abstractions;
using Shouldly;
using Xunit;

namespace Abp.Mapperly.Tests;

// ---------- Entities ----------

public class PersonEntity
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class PersonDto
{
    public string Id { get; set; }
    public string Name { get; set; }
}

// ---------- Two-way mapper with reverse hooks ----------

[Mapper]
public partial class PersonMapper : ReverseMapperBase<PersonEntity, PersonDto>
{
    // Forward: PersonEntity → PersonDto (Mapperly generates)
    public override partial PersonDto Map(PersonEntity source);

    public override PersonDto MapTo(PersonEntity source, PersonDto destination)
    {
        destination.Id = source.Id;
        destination.Name = source.Name;
        return destination;
    }

    // Reverse: PersonDto → PersonEntity (Mapperly generates)
    public override partial PersonEntity ReverseMap(PersonDto destination);

    public override void ReverseMapTo(PersonDto destination, PersonEntity source)
    {
        source.Id = destination.Id;
        source.Name = destination.Name;
    }

    // Reverse lifecycle hooks
    public override void BeforeReverseMap(PersonDto destination)
    {
        destination.Name = "BeforeReverse:" + destination.Name;
    }

    public override void AfterReverseMap(PersonDto destination, PersonEntity source)
    {
        source.Name = source.Name + ":AfterReverse";
    }
}

// ---------- Tests ----------

public class ReverseMapper_Tests : AbpIntegratedTestBase<AbpMapperlyTestModule>
{
    private readonly IObjectMapper _objectMapper;

    public ReverseMapper_Tests()
    {
        _objectMapper = Resolve<IObjectMapper>();
    }

    // -------------------------------------------------------------------------
    // Forward direction
    // -------------------------------------------------------------------------

    [Fact]
    public void Forward_Map_Creates_New_Dto()
    {
        var entity = new PersonEntity { Id = "1", Name = "Alice" };

        var dto = _objectMapper.Map<PersonDto>(entity);

        dto.Id.ShouldBe("1");
        dto.Name.ShouldBe("Alice");
    }

    [Fact]
    public void Forward_MapTo_Updates_Existing_Dto()
    {
        var entity = new PersonEntity { Id = "2", Name = "Bob" };
        var dto = new PersonDto { Id = "old", Name = "old" };

        _objectMapper.Map<PersonEntity, PersonDto>(entity, dto);

        dto.Id.ShouldBe("2");
        dto.Name.ShouldBe("Bob");
    }

    // -------------------------------------------------------------------------
    // Reverse direction via Map<TDestination>(object source) — finds reverse mapper
    // -------------------------------------------------------------------------

    [Fact]
    public void Reverse_Map_Creates_New_Entity_And_Calls_Hooks()
    {
        var dto = new PersonDto { Id = "3", Name = "Carol" };

        // IObjectMapper does not know about "reverse" — it just looks for
        // IAbpReverseMapper<PersonEntity, PersonDto> as a fallback.
        var entity = _objectMapper.Map<PersonEntity>(dto);

        // BeforeReverseMap prepends, Mapperly copies, AfterReverseMap appends
        entity.Id.ShouldBe("3");
        entity.Name.ShouldBe("BeforeReverse:Carol:AfterReverse");
    }

    [Fact]
    public void Reverse_MapTo_Updates_Existing_Entity_And_Calls_Hooks()
    {
        var dto = new PersonDto { Id = "4", Name = "Dave" };
        var entity = new PersonEntity { Id = "old", Name = "old" };

        _objectMapper.Map<PersonDto, PersonEntity>(dto, entity);

        entity.Id.ShouldBe("4");
        entity.Name.ShouldBe("BeforeReverse:Dave:AfterReverse");
    }

    // -------------------------------------------------------------------------
    // IoC registration
    // -------------------------------------------------------------------------

    [Fact]
    public void Module_Registers_Both_IAbpMapper_And_IAbpReverseMapper()
    {
        var forwardMapper = LocalIocManager.Resolve<IAbpMapper<PersonEntity, PersonDto>>();
        var reverseMapper = LocalIocManager.Resolve<IAbpReverseMapper<PersonEntity, PersonDto>>();

        forwardMapper.ShouldNotBeNull();
        reverseMapper.ShouldNotBeNull();
        forwardMapper.ShouldBeOfType<PersonMapper>();
        reverseMapper.ShouldBeOfType<PersonMapper>();

        LocalIocManager.Release(forwardMapper);
        LocalIocManager.Release(reverseMapper);
    }
}
