using   System;
using   System.ComponentModel.DataAnnotations;
using   PipperChat.DTOs;
using   PipperChat.Data;
using   PipperChat.Controllers;
using   PipperChat.Services; 

namespace   PipperChat.DTOs
{
public  class   UserRegistrationDto
{
    [Required]
    public  string  Email   {   get;    set;} 
    [Required]
    public  string  Password    {   get;    set;}  
    public  string  Username    {   get;    set;}

    [Required]
    [DataType(DataType.Date)]
    [Over16YearsOldAtrribute(ErrorMessage    =   "You    must    be  at  least   16  years   old to  register.")]
    public  DateTime    DateOfBirth {   get;    set;}

    public  UserRegistrationDto()
    {
        Email   =   string.Empty;
        Password    =   string.Empty;
        Username    =   string.Empty;

    }
}
}