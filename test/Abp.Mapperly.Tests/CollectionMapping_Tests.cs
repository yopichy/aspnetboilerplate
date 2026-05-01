using System.Collections.Generic;
using System.Linq;
using Abp.ObjectMapping;
using Abp.TestBase;
using Shouldly;
using Xunit;

namespace Abp.Mapperly.Tests;

// Uses MyEntity / MyDto / MyEntityMapper already defined in Mapperly_Tests.cs.
// The element mapper IAbpMapper<MyEntity, MyDto> is already registered in IoC,
// so MapperlyObjectMapper can use it to map individual elements within collections.

public class CollectionMapping_Tests : AbpIntegratedTestBase<AbpMapperlyTestModule>
{
    private readonly IObjectMapper _objectMapper;

    public CollectionMapping_Tests()
    {
        _objectMapper = Resolve<IObjectMapper>();
    }

    // -------------------------------------------------------------------------
    // List<T>
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Map_List_To_List()
    {
        var entities = new List<MyEntity>
        {
            new MyEntity { Id = 1, Name = "A" },
            new MyEntity { Id = 2, Name = "B" }
        };

        var dtos = _objectMapper.Map<List<MyDto>>(entities);

        dtos.Count.ShouldBe(2);
        dtos[0].Id.ShouldBe(1);
        dtos[0].Name.ShouldBe("A");
        dtos[1].Id.ShouldBe(2);
        dtos[1].Name.ShouldBe("B");
    }

    [Fact]
    public void Should_Map_Empty_List_To_Empty_List()
    {
        var entities = new List<MyEntity>();

        var dtos = _objectMapper.Map<List<MyDto>>(entities);

        dtos.ShouldNotBeNull();
        dtos.Count.ShouldBe(0);
    }

    // -------------------------------------------------------------------------
    // Array
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Map_Array_To_Array()
    {
        var entities = new[]
        {
            new MyEntity { Id = 10, Name = "X" },
            new MyEntity { Id = 20, Name = "Y" }
        };

        var dtos = _objectMapper.Map<MyDto[]>(entities);

        dtos.Length.ShouldBe(2);
        dtos[0].Id.ShouldBe(10);
        dtos[1].Id.ShouldBe(20);
    }

    // -------------------------------------------------------------------------
    // IReadOnlyList<T>
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Map_IReadOnlyList_To_List()
    {
        IReadOnlyList<MyEntity> entities = new List<MyEntity>
        {
            new MyEntity { Id = 3, Name = "C" }
        };

        var dtos = _objectMapper.Map<List<MyDto>>(entities);

        dtos.Count.ShouldBe(1);
        dtos[0].Id.ShouldBe(3);
    }

    // -------------------------------------------------------------------------
    // Collection of collections (reverse mapper used for elements)
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Map_List_Of_Entities_To_Array_Of_Dtos()
    {
        var entities = new List<MyEntity>
        {
            new MyEntity { Id = 5, Name = "E" },
            new MyEntity { Id = 6, Name = "F" }
        };

        var dtos = _objectMapper.Map<MyDto[]>(entities);

        dtos.Length.ShouldBe(2);
        dtos.Select(d => d.Id).ShouldBe(new[] { 5, 6 });
    }

    // -------------------------------------------------------------------------
    // Two-way element mapper — collection maps in reverse direction
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Map_List_Of_Dtos_To_List_Of_Entities_Via_Reverse_Mapper()
    {
        // PersonMapper is a ReverseMapperBase registered for IAbpReverseMapper<PersonEntity, PersonDto>
        // So _objectMapper.Map<List<PersonEntity>>(listOfPersonDto) uses the reverse mapper for each element.
        var dtos = new List<PersonDto>
        {
            new PersonDto { Id = "r1", Name = "Rev" }
        };

        var entities = _objectMapper.Map<List<PersonEntity>>(dtos);

        entities.Count.ShouldBe(1);
        // BeforeReverseMap prepends, AfterReverseMap appends (defined in ReverseMapper_Tests.cs)
        entities[0].Id.ShouldBe("r1");
        entities[0].Name.ShouldBe("BeforeReverse:Rev:AfterReverse");
    }
}
