using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PipperChat.Models;
using PipperChat.Data;
using System.ComponentModel.DataAnnotations;

 public  class   LoginViews
    {
        [Required(ErrorMessage =    "Email  is  required")]
        [EmailAddress(ErrorMessage =    "Invalid    email   address")]
        public  string  Email   {   get;    set;    }

        [Required(ErrorMessage =    "Password   is  required")]
        [DataType(DataType.Password)]
        public  string  Password    {   get;    set;}

        [Display(Name = "Remember me?")]
        public  bool    RememberMe  {   get;    set;    }

    }