namespace DeliverySaaS.Domain.Operations.Enums;

public enum OperationalState
{
    New = 1,
    InPickupAgent = 2,
    InSortingHub = 3,
    InDeliveryAgent = 4,
    Delivered = 5,
    ReturnedOrders = 6,
    ReturnSortingHub = 7,
    ReturnedToMerchant = 8
}
