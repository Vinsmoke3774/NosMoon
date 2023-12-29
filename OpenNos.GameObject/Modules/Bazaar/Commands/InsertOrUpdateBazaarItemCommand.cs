using FluentValidation;
using MediatR;
using OpenNos.Data;

namespace OpenNos.GameObject.Modules.Bazaar.Commands
{
    public class InsertOrUpdateBazaarItemCommand : IRequest<long>
    {
        public BazaarItemDTO BazaarItem { get; set; }
    }

    public class InsertOrUpdateBazaarItemCommandValidator : AbstractValidator<InsertOrUpdateBazaarItemCommand>
    {
        public InsertOrUpdateBazaarItemCommandValidator()
        {
            RuleFor(m => m.BazaarItem).NotNull();
        }
    }
}
