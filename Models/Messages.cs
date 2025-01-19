using PipperChat.Models;
using System.ComponentModel.DataAnnotations;
using PipperChat.Data;
  
  namespace PipperChat.Models{
   public  class   Message
    {
        public  int Id  {   get;    set;}
        public  string  Content {   get;    set;}   =   string.Empty;
        public  DateTime    Timestamp   {   get;    set;}
        public  bool    IsEdited    {   get;    set;}
        public  DateTime?   EditedAt    {   get;    set;}

        // Foreign Keys
        public  int SenderId    {   get;    set;}
        public  int GroupId {   get;    set;}
        public  int RecipientId {   get;    set;}

        //  Navigation properties
        public  virtual User    Sender  {   get;    set;}  =    null!;
        public  virtual Group?  Group   {   get;    set;}
        public  virtual User?   Recipient   {   get;    set;}

        public Message()
        {
            Content =   string.Empty;
            Timestamp   =   DateTime.Now;
        }
    }
  }