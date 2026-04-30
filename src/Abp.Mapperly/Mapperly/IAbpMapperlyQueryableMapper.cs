using System.Linq;

namespace Abp.Mapperly;

/// <summary>
/// Optional interface for Mapperly mappers that support typed queryable projection.
/// Use this as an opt-in alternative to <see cref="Abp.ObjectMapping.IObjectMapper.ProjectTo{TDestination}"/>,
/// which is not supported by Mapperly.
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
public interface IAbpMapperlyQueryableMapper<TSource, TDestination>
{
    /// <summary>Projects a strongly-typed queryable to <typeparamref name="TDestination"/>.</summary>
    IQueryable<TDestination> ProjectTo(IQueryable<TSource> source);
}
