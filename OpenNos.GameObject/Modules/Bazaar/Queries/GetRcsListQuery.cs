using FluentValidation;
using MediatR;
using NosByte.Shared;

namespace OpenNos.GameObject.Modules.Bazaar.Queries
{
    public class GetRcsListQuery : IRequest<string>
    {
        public RcsPacketModel Model { get; set; }
    }

    public class GetRcsListQueryValidator : AbstractValidator<GetRcsListQuery>
    {
        public GetRcsListQueryValidator()
        {
            RuleFor(m => m.Model).NotNull();
        }
    }
}
