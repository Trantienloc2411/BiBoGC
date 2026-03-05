/// Generic wrapper for paginated API responses.
///
/// [hasMore] is derived from whether there are more items beyond the current page.
class PagedResult<T> {
  final List<T> items;
  final int totalCount;
  final int page;
  final int pageSize;

  const PagedResult({
    required this.items,
    required this.totalCount,
    required this.page,
    required this.pageSize,
  });

  /// Whether there are additional pages to load.
  bool get hasMore => page * pageSize < totalCount;

  factory PagedResult.fromJson(
    Map<String, dynamic> json,
    T Function(Object? json) fromItemJson,
  ) {
    final itemsList = (json['items'] as List<dynamic>? ?? [])
        .map(fromItemJson)
        .toList();
    return PagedResult<T>(
      items: itemsList,
      totalCount: (json['totalCount'] as num?)?.toInt() ?? 0,
      page: (json['page'] as num?)?.toInt() ?? 1,
      pageSize: (json['pageSize'] as num?)?.toInt() ?? 20,
    );
  }
}
