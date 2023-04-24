﻿using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Audio.BnkCompiler.Validation
{
    public class ActionValidator : AbstractValidator<Action>
    {
        private readonly List<string> ValidActionTypes = new List<string>() { "Play" };

        public ActionValidator(List<IAudioProjectHircItem> allItems)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Item is missing ID");
            RuleFor(x => x.ChildId).Must(x => ValidateChildReference(x, allItems)).WithMessage($"ActionChild has invalid reference");
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("ActionChild has no type")
                .Must(ValidateChildActionType).WithMessage(x => $"ActionChild has invalid type '{x.Type}'. Valid values are {string.Join(", ", ValidActionTypes)}");
        }

        private bool ValidateChildReference(string id, List<IAudioProjectHircItem> allItems) => allItems.Any(x => x.Name == id);
        private bool ValidateChildActionType(string childType) => ValidActionTypes.Contains(childType, StringComparer.InvariantCultureIgnoreCase);
    }
}