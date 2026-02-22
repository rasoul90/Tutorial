namespace DeliverySaaS.Application.Common.Models;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int Size);
