namespace DeliverySaaS.Application.Common.Interfaces;

public interface IHmacSignatureService
{
    string ComputeSignature(string payload, string secret);
    bool ValidateSignature(string payload, string secret, string? signature);
}
