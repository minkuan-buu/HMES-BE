using System;
using System.ComponentModel.DataAnnotations;
using HMES.Data.DTO.Custom;

public class CustomModRoleValidateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is string role)
        {
            if (role != "Technician" && role != "Consultant")
            {
                throw new CustomException("Role is not valid");
            }
        }
        return true;
    }
}