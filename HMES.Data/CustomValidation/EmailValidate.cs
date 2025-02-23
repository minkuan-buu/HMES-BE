using System;
using System.ComponentModel.DataAnnotations;
using HMES.Data.DTO.Custom;

public class CustomEmailValidateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is string email)
        {
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
            {
                throw new CustomException("Email is not valid");
            }
        }
        return true;
    }
}