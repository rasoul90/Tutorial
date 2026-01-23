using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Rasad.Web.Resources;

namespace Rasad.Web.Identity;

public class ArabicIdentityErrorDescriber : IdentityErrorDescriber
{
    private readonly IStringLocalizer<IdentityErrors> _localizer;

    public ArabicIdentityErrorDescriber(IStringLocalizer<IdentityErrors> localizer)
    {
        _localizer = localizer;
    }

    public override IdentityError DuplicateEmail(string email)
        => new() { Code = nameof(DuplicateEmail), Description = _localizer[nameof(DuplicateEmail)] };

    public override IdentityError DuplicateUserName(string userName)
        => new() { Code = nameof(DuplicateUserName), Description = _localizer[nameof(DuplicateUserName)] };

    public override IdentityError InvalidEmail(string email)
        => new() { Code = nameof(InvalidEmail), Description = _localizer[nameof(InvalidEmail)] };

    public override IdentityError InvalidUserName(string userName)
        => new() { Code = nameof(InvalidUserName), Description = _localizer[nameof(InvalidUserName)] };

    public override IdentityError PasswordMismatch()
        => new() { Code = nameof(PasswordMismatch), Description = _localizer[nameof(PasswordMismatch)] };

    public override IdentityError PasswordTooShort(int length)
        => new() { Code = nameof(PasswordTooShort), Description = _localizer[nameof(PasswordTooShort)] };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = _localizer[nameof(PasswordRequiresNonAlphanumeric)] };

    public override IdentityError PasswordRequiresDigit()
        => new() { Code = nameof(PasswordRequiresDigit), Description = _localizer[nameof(PasswordRequiresDigit)] };

    public override IdentityError PasswordRequiresLower()
        => new() { Code = nameof(PasswordRequiresLower), Description = _localizer[nameof(PasswordRequiresLower)] };

    public override IdentityError PasswordRequiresUpper()
        => new() { Code = nameof(PasswordRequiresUpper), Description = _localizer[nameof(PasswordRequiresUpper)] };

    public override IdentityError ConcurrencyFailure()
        => new() { Code = nameof(ConcurrencyFailure), Description = _localizer[nameof(ConcurrencyFailure)] };

    public override IdentityError DefaultError()
        => new() { Code = nameof(DefaultError), Description = _localizer[nameof(DefaultError)] };
}
