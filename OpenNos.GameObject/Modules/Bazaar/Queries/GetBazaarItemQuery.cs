using FluentValidation;
using MediatR;
using OpenNos.Data;

namespace OpenNos.GameObject.Modules.Bazaar.Queries
{
    public class GetBazaarItemQuery : IRequest<BazaarItemDTO>
    {
        public long Id { get; set; }
    }

    public class GetBazaarItemValidator : AbstractValidator<GetBazaarItemQuery>
    {
        public GetBazaarItemValidator()
        {
            RuleFor(m => m.Id).NotEmpty();
        }
    }
}
