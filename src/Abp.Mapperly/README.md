# Abp.Mapperly

Integration of [Mapperly](https://github.com/riok/mapperly) into the ABP framework as an `IObjectMapper` implementation.

Mapperly is a .NET source generator — mapping code is generated at **compile time**, not via runtime reflection. The result: faster performance, zero runtime overhead, and AoT/trimming safe.

> **Coexistence**: This package does not automatically replace `Abp.AutoMapper`. Both can be used side by side. Migration can be done incrementally, one mapper at a time.

---

## Installation

Add the NuGet package:

```
dotnet add package AbpLts.Mapperly
```

Register the module in your startup module:

```csharp
// Replace:
[DependsOn(typeof(AbpAutoMapperModule))]

// With:
[DependsOn(typeof(AbpMapperlyModule))]
```

---

## Basic Usage

### 1. Create a mapper class

Create a `partial class` with Mapperly's `[Mapper]` attribute and implement `IAbpMapper<TSource, TDestination>`:

```csharp
using Riok.Mapperly.Abstractions;
using Abp.Mapperly;

[Mapper]
public partial class UserMapper : IAbpMapper<User, UserDto>
{
    public partial UserDto Map(User source);

    public UserDto MapTo(User source, UserDto destination)
    {
        destination.Id = source.Id;
        destination.Name = source.Name;
        return destination;
    }
}
```

Mapperly generates the `Map()` implementation automatically at build time. `MapTo()` must be implemented manually since it maps onto an existing object.

### 2. Use via `IObjectMapper` (no changes on the consumer side)

```csharp
public class UserAppService : ApplicationService
{
    private readonly IObjectMapper _objectMapper;

    public UserAppService(IObjectMapper objectMapper)
    {
        _objectMapper = objectMapper;
    }

    public UserDto GetUser(int id)
    {
        var user = ...; // from repository
        return _objectMapper.Map<UserDto>(user);  // unchanged
    }
}
```

### 3. Inject the mapper directly (optional)

```csharp
public class UserAppService : ApplicationService
{
    private readonly IAbpMapper<User, UserDto> _mapper;

    public UserAppService(IAbpMapper<User, UserDto> mapper)
    {
        _mapper = mapper;
    }
}
```

### 4. Queryable projection (opt-in replacement for `ProjectTo`)

`IObjectMapper.ProjectTo<T>()` is not supported by Mapperly. Use `IAbpMapperlyQueryableMapper<TSource, TDest>` instead:

```csharp
[Mapper]
public partial class UserMapper : IAbpMapper<User, UserDto>,
                                  IAbpMapperlyQueryableMapper<User, UserDto>
{
    public partial UserDto Map(User source);

    public UserDto MapTo(User source, UserDto destination) { ... }

    public IQueryable<UserDto> ProjectTo(IQueryable<User> source)
        => source.Select(u => Map(u));
}
```

Then inject it directly in your Application Service:

```csharp
private readonly IAbpMapperlyQueryableMapper<User, UserDto> _queryMapper;

public IQueryable<UserDto> GetUsersQuery()
    => _queryMapper.ProjectTo(_userRepository.GetAll());
```

---

## Migrating from `Abp.AutoMapper`

### What does NOT need to change

| Code | Status |
|---|---|
| `IObjectMapper` injection | No change required |
| `_objectMapper.Map<TDest>(source)` | No change required |
| `_objectMapper.Map<TSrc, TDest>(src, dest)` | No change required |

### What MUST change

#### 1. `[AutoMap*]` attributes → `[Mapper] partial class`

**Before (AutoMapper):**

```csharp
[AutoMapFrom(typeof(User))]
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

**After (Mapperly):**

Remove the attribute from the DTO and create a separate mapper class:

```csharp
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

[Mapper]
public partial class UserMapper : IAbpMapper<User, UserDto>
{
    public partial UserDto Map(User source);

    public UserDto MapTo(User source, UserDto destination)
    {
        destination.Id = source.Id;
        destination.Name = source.Name;
        return destination;
    }
}
```

#### 2. `Configurators.Add` with inline lambda

**Before (AutoMapper):**

```csharp
Configuration.Modules.AbpAutoMapper().Configurators.Add(config =>
{
    config.CreateMap<User, UserDto>()
        .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FirstName + " " + s.LastName))
        .ForMember(d => d.InternalNote, opt => opt.Ignore());
});
```

Delete the entire `Configurators.Add(...)` call. Each `CreateMap<A, B>()` becomes its own `IAbpMapper<,>` class.

**After (Mapperly):**

```csharp
[Mapper]
public partial class UserMapper : IAbpMapper<User, UserDto>
{
    [MapperIgnoreTarget(nameof(UserDto.InternalNote))]
    public partial UserDto Map(User source);

    // Mapperly detects this private method and uses it to populate FullName
    private string MapFullName(User source) => source.FirstName + " " + source.LastName;

    public UserDto MapTo(User source, UserDto destination) { ... }
}
```

#### 3. `Configurators.Add` with method reference

**Before (AutoMapper):**

```csharp
// In PreInitialize():
Configuration.Modules.AbpAutoMapper().Configurators.Add(CreateMappings);

private void CreateMappings(IMapperConfigurationExpression cfg)
{
    cfg.CreateMap<User, UserDto>();
    cfg.CreateMap<Role, RoleDto>();
}
```

Delete both the `Configurators.Add(...)` call and the method. Each `CreateMap<A, B>()` line becomes its own mapper class.

**After (Mapperly):**

```csharp
[Mapper]
public partial class UserMapper : IAbpMapper<User, UserDto>
{
    public partial UserDto Map(User source);
    public UserDto MapTo(User source, UserDto destination) { ... }
}

[Mapper]
public partial class RoleMapper : IAbpMapper<Role, RoleDto>
{
    public partial RoleDto Map(Role source);
    public RoleDto MapTo(Role source, RoleDto destination) { ... }
}
```

`AbpMapperlyModule` auto-discovers all classes implementing `IAbpMapper<,>` — no registration needed.

#### 4. `Configurators.Add` with AutoMapper `Profile` class

**Before (AutoMapper):**

```csharp
Configuration.Modules.AbpAutoMapper().Configurators.Add(cfg => cfg.AddProfile<MyMappingProfile>());

public class MyMappingProfile : Profile
{
    public MyMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.First + " " + s.Last))
            .ForMember(d => d.Secret, opt => opt.Ignore());
    }
}
```

Delete the `Configurators.Add(...)` call and the `Profile` class entirely.

**After (Mapperly):**

```csharp
[Mapper]
public partial class UserMapper : IAbpMapper<User, UserDto>
{
    [MapperIgnoreTarget(nameof(UserDto.Secret))]
    public partial UserDto Map(User source);

    private string MapFullName(User source) => source.First + " " + source.Last;

    public UserDto MapTo(User source, UserDto destination) { ... }
}
```

### `.ForMember()` attribute equivalents

| AutoMapper | Mapperly |
|---|---|
| `opt.Ignore()` | `[MapperIgnoreTarget(nameof(D.Prop))]` on `Map()` |
| `opt.MapFrom(s => s.OtherProp)` | `[MapProperty(nameof(S.OtherProp), nameof(D.Prop))]` on `Map()` |
| `opt.MapFrom(s => expr)` | Private method `private T MapPropName(Source s) => expr` |
| `opt.Condition(s => ...)` | Not supported natively — implement `MapTo()` manually |

#### 5. `IObjectMapper.ProjectTo<T>()` → `IAbpMapperlyQueryableMapper<,>`

**Before:**

```csharp
var query = _objectMapper.ProjectTo<UserDto>(_userRepository.GetAll());
```

**After:**

```csharp
// Inject IAbpMapperlyQueryableMapper<User, UserDto> in the constructor
var query = _queryMapper.ProjectTo(_userRepository.GetAll());
```

### Incremental migration strategy

Since `Abp.AutoMapper` and `Abp.Mapperly` can coexist, migration can be done module by module:

1. Add `AbpLts.Mapperly` as a dependency
2. Pick one bounded context / module
3. Create mapper classes for the DTOs in that module
4. Remove `[AutoMap*]` attributes from those DTOs
5. Test, then move on to the next module
6. Once all modules are migrated, swap the module dependency and remove `AbpLts.AutoMapper`

---

## References

- [Mapperly Documentation](https://mapperly.riok.app/docs/intro/)
- [Mapperly GitHub](https://github.com/riok/mapperly)
- [ABP Framework](https://aspnetboilerplate.com/)
