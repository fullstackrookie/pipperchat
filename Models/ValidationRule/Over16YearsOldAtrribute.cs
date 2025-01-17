using   System;
using   System.ComponentModel.DataAnnotations;

public  class   Over16YearsOldAtrribute :   ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if  (value  is  DateTime    dateofBirth)
        {
            var age =   DateTime.Today.Year -   dateofBirth.Year;
        if(dateofBirth.Date >   DateTime.Today.AddYears(-age))  age--;

        return  age >=  16
            ?   ValidationResult.Success
            :   new ValidationResult(ErrorMessage   ??  "You    must    be  at  least   16  years   old");
        }
        return  new ValidationResult("Invalid   date    format");
    }
}
