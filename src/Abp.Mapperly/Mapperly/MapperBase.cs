namespace Abp.Mapperly;

/// <summary>
/// Convenient abstract base class for one-way Mapperly mappers.
/// Provides empty default implementations for <see cref="IAbpMapper{TSource,TDestination}.BeforeMap"/>
/// and <see cref="IAbpMapper{TSource,TDestination}.AfterMap"/> so you only override what you need.
/// </summary>
/// <example>
/// <code>
/// [Mapper]
/// public partial class UserMapper : MapperBase&lt;User, UserDto&gt;
/// {
///     public override partial UserDto Map(User source);
///
///     public override UserDto MapTo(User source, UserDto destination)
///     {
///         destination.Id   = source.Id;
///         destination.Name = source.Name;
///         return destination;
///     }
/// }
/// </code>
/// </example>
public abstract class MapperBase<TSource, TDestination> : IAbpMapper<TSource, TDestination>
{
    /// <inheritdoc/>
    public abstract TDestination Map(TSource source);

    /// <inheritdoc/>
    public abstract TDestination MapTo(TSource source, TDestination destination);

    /// <inheritdoc/>
    public virtual void BeforeMap(TSource source) { }

    /// <inheritdoc/>
    public virtual void AfterMap(TSource source, TDestination destination) { }
}
