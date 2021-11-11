using DataAccess.Interface;
using Entity.Common.Helper;
using Entity.Entities.DTO;
using Entity.Entities.Security;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domains.Validation.Security
{
    public class UserValidation : AbstractValidator<UserEntity>
    {
        private readonly IBaseRepository<UserEntity> baseRepository;
        public UserValidation(IBaseRepository<UserEntity> _baseRepository)
        {
            baseRepository = _baseRepository;
            RuleFor(a => a.Username).NotNull().NotEmpty().WithMessage("UserName is Required");
            RuleFor(a => a.Password).NotNull().NotEmpty().WithMessage("Password is Required");
            RuleFor(a => a.MobileNumber).NotNull().NotEmpty().WithMessage("MobileNumber is Required").MaximumLength(10);
            RuleFor(a => a.Name).NotNull().NotEmpty();
            RuleFor(a => a.Email).NotNull().NotEmpty().EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
            RuleFor(x => x).Custom((x, context) =>
            {
                if (string.IsNullOrEmpty(x.Email))
                {
                    context.AddFailure("Email is required");
                }
                //var user = baseRepository.FindOneByQuery(a => a.Email == x.Email && a.IsActive);
                //if (user.Result != null)
                //{
                //    context.AddFailure("User already exist with this email address");
                //}
            });

        }

      
    }

    public class UpdateUserValidation : AbstractValidator<UserEntity>
    {
        private readonly IBaseRepository<UserEntity> baseRepository;
        public UpdateUserValidation(IBaseRepository<UserEntity> _baseRepository)
        {
            baseRepository = _baseRepository;
            RuleFor(a => a.Username).NotNull().NotEmpty().WithMessage("UserName is Required");
            RuleFor(a => a.Password).NotNull().NotEmpty().WithMessage("Password is Required");
            RuleFor(a => a.MobileNumber).NotNull().NotEmpty().WithMessage("MobileNumber is Required").MaximumLength(10);
            RuleFor(a => a.Name).NotNull().NotEmpty();
            RuleFor(a => a.Email).NotNull().NotEmpty().EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
            
        }


    }

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordDetails>
    {
        private readonly IBaseRepository<UserEntity> baseRepository;
        public ResetPasswordValidator(IBaseRepository<UserEntity> _baseRepository)
        {
            baseRepository = _baseRepository;
            RuleFor(x => x.RequestID).NotNull().NotEmpty();
            RuleFor(x => x.Email).NotNull().NotEmpty();
            RuleFor(x => x.NewPassword).NotNull().NotEmpty();
            RuleFor(x => x.ConfirmPassword).NotNull().NotEmpty().Equal(c => c.NewPassword).WithMessage("Passwords do not match");
            RuleFor(x => x).Custom((x, context) =>
            {
                if (!x.NewPassword.ValidateAppPasswordPolicy())
                {
                    context.AddFailure("Password does not match with password policy");
                }
                var user = baseRepository.FindOneByQuery(p => p.Email == x.Email && p.IsActive);
                if (user == null)
                {
                    context.AddFailure("Invalid email");
                }
            });

        }
    }

    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDTO>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.OldPassword).NotNull().NotEmpty();
            RuleFor(x => x.NewPassword).NotNull().NotEmpty();
            RuleFor(x => x.ConfirmPassword).NotNull().NotEmpty().Equal(c => c.NewPassword).WithMessage("Passwords do not match");
        }
    }
}
