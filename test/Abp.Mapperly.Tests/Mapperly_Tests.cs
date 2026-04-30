using System;
using System.Collections.Generic;
using System.Linq;
using Abp.ObjectMapping;
using Abp.TestBase;
using Riok.Mapperly.Abstractions;
using Shouldly;
using Xunit;

namespace Abp.Mapperly.Tests;

// ---------- Test entities ----------

public class MyEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string InternalNote { get; set; }
}

public class MyDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// ---------- Mapperly mapper ----------

[Mapper]
public partial class MyEntityMapper : IAbpMapper<MyEntity, MyDto>
{
    [MapperIgnoreSource(nameof(MyEntity.InternalNote))]
    public partial MyDto Map(MyEntity source);

    public MyDto MapTo(MyEntity source, MyDto destination)
    {
        destination.Id = source.Id;
        destination.Name = source.Name;
        return destination;
    }
}

// ---------- Tests ----------

public class Mapperly_Tests : AbpIntegratedTestBase<AbpMapperlyTestModule>
{
    private readonly IObjectMapper _objectMapper;

    public Mapperly_Tests()
    {
        _objectMapper = Resolve<IObjectMapper>();
    }

    [Fact]
    public void Map_Should_Create_New_Destination()
    {
        var entity = new MyEntity { Id = 1, Name = "Test", InternalNote = "secret" };

        var dto = _objectMapper.Map<MyDto>(entity);

        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("Test");
    }

    [Fact]
    public void Map_Null_Should_Return_Default()
    {
        MyEntity entity = null;

        var dto = _objectMapper.Map<MyDto>(entity);

        dto.ShouldBeNull();
    }

    [Fact]
    public void Map_Existing_Should_Update_Destination_And_Return_Same_Instance()
    {
        var entity = new MyEntity { Id = 42, Name = "Updated" };
        var dto = new MyDto { Id = 0, Name = "Old" };

        var result = _objectMapper.Map<MyEntity, MyDto>(entity, dto);

        result.Id.ShouldBe(42);
        result.Name.ShouldBe("Updated");
        result.ShouldBeSameAs(dto);
    }

    [Fact]
    public void ProjectTo_Should_Throw_NotSupportedException()
    {
        var query = new List<MyEntity>().AsQueryable();

        Should.Throw<NotSupportedException>(() => _objectMapper.ProjectTo<MyDto>(query));
    }

    [Fact]
    public void Module_Should_Auto_Register_Mapper_In_IoC()
    {
        var mapper = LocalIocManager.Resolve<IAbpMapper<MyEntity, MyDto>>();

        mapper.ShouldNotBeNull();
        mapper.ShouldBeOfType<MyEntityMapper>();

        LocalIocManager.Release(mapper);
    }
}
