using System.Linq.Expressions;

namespace DevTKSS.MyManufacturerERP.Application.Common.Mappings;

public static class MappingExtensions
{
    public static Task<PaginatedList<TDestination>> PaginatedListAsync<TDestination>(this IQueryable<TDestination> queryable, int pageNumber, int pageSize, CancellationToken cancellationToken = default) where TDestination : class
        => PaginatedList<TDestination>.CreateAsync(queryable.AsNoTracking(), pageNumber, pageSize, cancellationToken);

    public static Task<List<TDestination>> ProjectToListAsync<TSource, TDestination>(this IQueryable<TSource> queryable, Expression<Func<TSource, TDestination>> projector, CancellationToken cancellationToken = default)
        where TSource : class
        where TDestination : class
        => queryable.AsNoTracking().Select(projector).ToListAsync(cancellationToken);

    public static Task<List<TDestination>> ProjectToTypeListAsync<TSource, TDestination>(this IQueryable<TSource> queryable, TypeAdapterConfig config, CancellationToken cancellationToken = default)
        where TSource : class
        where TDestination : class, new()
        => queryable.ProjectToType<TDestination>(config).AsNoTracking().ToListAsync(cancellationToken);
}
