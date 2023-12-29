﻿using FluentValidation;
using MediatR;

namespace OpenNos.GameObject.Modules.Bazaar.Queries
{
    public class GetStateQuery : IRequest<bool>
    {
        public long Id { get; set; }
    }

    public class GetStateQueryValidator : AbstractValidator<GetStateQuery>
    {
        public GetStateQueryValidator()
        {
            RuleFor(m => m.Id).NotEmpty();
        }
    }
}
