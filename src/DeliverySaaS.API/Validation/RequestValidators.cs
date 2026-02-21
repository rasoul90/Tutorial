using DeliverySaaS.API.Controllers;
using DeliverySaaS.API.Controllers.Mobile;
using DeliverySaaS.Application.Orders;
using FluentValidation;

namespace DeliverySaaS.API.Validation;

public class CreateSettlementRequestValidator : AbstractValidator<CreateSettlementRequest>
{
    public CreateSettlementRequestValidator()
    {
        RuleFor(x => x.MerchantId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class GenerateInvoiceRequestValidator : AbstractValidator<GenerateInvoiceRequest>
{
    public GenerateInvoiceRequestValidator()
    {
        RuleFor(x => x.MerchantId).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThan(0);
    }
}

public class CreateReconciliationRequestValidator : AbstractValidator<CreateReconciliationRequest>
{
    public CreateReconciliationRequestValidator()
    {
        RuleFor(x => x.DeliveryAgentId).NotEmpty();
        RuleFor(x => x.CollectedAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DeliveredAmount).GreaterThanOrEqualTo(0);
    }
}

public class CreatePayrollRequestValidator : AbstractValidator<CreatePayrollRequest>
{
    public CreatePayrollRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequest>
{
    public CreateExpenseRequestValidator()
    {
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class TransitionOrderRequestValidator : AbstractValidator<TransitionOrderRequest>
{
    public TransitionOrderRequestValidator()
    {
        RuleFor(x => x.ToState).IsInEnum();
    }
}

public class CreateOrderProblemRequestValidator : AbstractValidator<CreateOrderProblemRequest>
{
    public CreateOrderProblemRequestValidator()
    {
        RuleFor(x => x.ProblemCatalogId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class ResolveOrderProblemRequestValidator : AbstractValidator<ResolveOrderProblemRequest>
{
    public ResolveOrderProblemRequestValidator()
    {
        RuleFor(x => x.ResolutionType).IsInEnum();
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}

public class CreatePartnerConnectionRequestValidator : AbstractValidator<CreatePartnerConnectionRequest>
{
    public CreatePartnerConnectionRequestValidator()
    {
        RuleFor(x => x.PartnerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.BaseUrl).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ApiKey).NotEmpty().MaximumLength(200);
    }
}

public class OutboundHandoffRequestValidator : AbstractValidator<OutboundHandoffRequest>
{
    public OutboundHandoffRequestValidator()
    {
        RuleFor(x => x.GovernorateId).NotEmpty();
    }
}

public class ProcessOutboxRequestValidator : AbstractValidator<ProcessOutboxRequest>
{
    public ProcessOutboxRequestValidator()
    {
        RuleFor(x => x.Take).InclusiveBetween(1, 500);
    }
}


public class CreateOrderByReservedQrRequestValidator : AbstractValidator<CreateOrderByReservedQrRequest>
{
    public CreateOrderByReservedQrRequestValidator()
    {
        RuleFor(x => x.ReservedQr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MerchantId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerPhone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.AmountToCollect).GreaterThanOrEqualTo(0);
    }
}

public class MerchantSettlementRequestMobileRequestValidator : AbstractValidator<MerchantSettlementRequestMobileRequest>
{
    public MerchantSettlementRequestMobileRequestValidator()
    {
        RuleFor(x => x.MerchantId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class PickupQrTransitionRequestValidator : AbstractValidator<PickupQrTransitionRequest>
{
    public PickupQrTransitionRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ToState).IsInEnum();
    }
}

public class HubTransitionRequestValidator : AbstractValidator<HubTransitionRequest>
{
    public HubTransitionRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
