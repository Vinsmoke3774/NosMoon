using FluentValidation;
using MediatR;
using NosByte.Packets.ClientPackets;

namespace OpenNos.GameObject.Modules.Bazaar.Queries
{
    public class GetRcbListQuery : IRequest<string>
    {
        public CBListPacket Packet { get; set; }
    }

    public class GetRcbListQueryValidator : AbstractValidator<GetRcbListQuery>
    {
        public GetRcbListQueryValidator()
        {
            RuleFor(m => m.Packet).NotNull();
        }
    }
}
