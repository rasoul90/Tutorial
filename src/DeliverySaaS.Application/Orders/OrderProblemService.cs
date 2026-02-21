using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;

namespace DeliverySaaS.Application.Orders;

public class OrderProblemService : IOrderProblemService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRequestContext _requestContext;

    public OrderProblemService(IOrderRepository orderRepository, IRequestContext requestContext)
    {
        _orderRepository = orderRepository;
        _requestContext = requestContext;
    }

    public async Task<Guid> CreateProblemAsync(Guid orderId, Guid problemCatalogId, string? notes, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException("Order not found.");

        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");

        var problem = new OrderProblem
        {
            OrderId = orderId,
            ProblemCatalogId = problemCatalogId,
            Notes = notes,
            BranchId = branchId,
            Status = ProblemStatus.Open
        };

        order.HasProblem = true;
        order.ProblemStatus = ProblemStatus.Open;

        await _orderRepository.AddOrderProblemAsync(problem, cancellationToken);
        await _orderRepository.AddOrderEventAsync(new OrderEvent
        {
            OrderId = orderId,
            EventType = "ProblemCreated",
            Notes = notes,
            EventAt = DateTime.UtcNow,
            BranchId = branchId
        }, cancellationToken);

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return problem.Id;
    }

    public async Task ResolveProblemAsync(Guid problemId, DynamicResolutionType resolutionType, string? phone, decimal? amountToCollect, string? address, string? note, CancellationToken cancellationToken = default)
    {
        var problem = await _orderRepository.GetProblemByIdAsync(problemId, cancellationToken)
            ?? throw new InvalidOperationException("Order problem not found.");

        if (problem.Status == ProblemStatus.Resolved)
        {
            throw new InvalidOperationException("Order problem is already resolved.");
        }

        var order = await _orderRepository.GetByIdAsync(problem.OrderId, cancellationToken)
            ?? throw new InvalidOperationException("Order not found.");

        var patches = new List<string>();

        switch (resolutionType)
        {
            case DynamicResolutionType.PHONE_CHANGE:
                if (string.IsNullOrWhiteSpace(phone)) throw new InvalidOperationException("phone is required for PHONE_CHANGE.");
                patches.Add($"CustomerPhone: '{order.CustomerPhone}' -> '{phone}'");
                order.CustomerPhone = phone;
                break;
            case DynamicResolutionType.PRICE_CHANGE:
                if (!amountToCollect.HasValue) throw new InvalidOperationException("amountToCollect is required for PRICE_CHANGE.");
                patches.Add($"AmountToCollect: '{order.AmountToCollect}' -> '{amountToCollect.Value}'");
                order.AmountToCollect = amountToCollect.Value;
                break;
            case DynamicResolutionType.ADDRESS_CHANGE:
                if (string.IsNullOrWhiteSpace(address)) throw new InvalidOperationException("address is required for ADDRESS_CHANGE.");
                patches.Add($"Address: '{order.Address}' -> '{address}'");
                order.Address = address;
                break;
            case DynamicResolutionType.NOTE_ONLY:
                patches.Add($"InternalNote: '{order.InternalNote}' -> '{note}'");
                order.InternalNote = note;
                break;
            default:
                throw new InvalidOperationException("Unsupported resolution type.");
        }

        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");

        await _orderRepository.AddOrderEventAsync(new OrderEvent
        {
            OrderId = order.Id,
            EventType = "FieldPatch",
            Notes = string.Join("; ", patches),
            EventAt = DateTime.UtcNow,
            BranchId = branchId
        }, cancellationToken);

        problem.Status = ProblemStatus.Resolved;
        problem.ResolutionType = resolutionType;
        problem.ResolvedAt = DateTime.UtcNow;

        var hasOpenProblems = await _orderRepository.HasOpenProblemsAsync(order.Id, cancellationToken);
        order.HasProblem = hasOpenProblems;
        order.ProblemStatus = hasOpenProblems ? ProblemStatus.Open : ProblemStatus.Resolved;

        await _orderRepository.SaveChangesAsync(cancellationToken);
    }
}
