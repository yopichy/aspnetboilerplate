namespace Abp.Mapperly;

/// <summary>
/// Abstract base class for two-way Mapperly mappers (forward + reverse direction).
/// Provides empty default implementations of all hook methods.
/// </summary>
/// <example>
/// <code>
/// [Mapper]
/// public partial class UserMapper : ReverseMapperBase&lt;User, UserDto&gt;
/// {
///     public override partial UserDto Map(User source);
///     public override partial User ReverseMap(UserDto destination);
///
///     public override UserDto MapTo(User source, UserDto destination) { ... }
///     public override void ReverseMapTo(UserDto destination, User source) { ... }
/// }
/// </code>
/// </example>
public abstract class ReverseMapperBase<TSource, TDestination>
    : MapperBase<TSource, TDestination>, IAbpReverseMapper<TSource, TDestination>
{
    /// <inheritdoc/>
    public abstract TSource ReverseMap(TDestination destination);

    /// <inheritdoc/>
    public abstract void ReverseMapTo(TDestination destination, TSource source);

    /// <inheritdoc/>
    public virtual void BeforeReverseMap(TDestination destination) { }

    /// <inheritdoc/>
    public virtual void AfterReverseMap(TDestination destination, TSource source) { }
}
