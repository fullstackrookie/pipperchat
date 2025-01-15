using System.Diagnostics.CodeAnalysis;
using   Microsoft.EntityFrameworkCore;

namespace   PipperChat.Data
{
    public class PipperChatContext : DbContext
    {
        public  PipperChatContext(DbContextOptions<PipperChatContext> options)    :   base(options)   {}

        // Dataset  Definition
        public  DbSet<User> Users   {   get;    set;    }
        public DbSet<Message>   Messages    {   get;    set;    }
    }

    public  class   User
    {
        public  int Id  {   get;    set;    }
        public  string  Username    {   get;    set;}   =   string.Empty;
        public  string  Email   {   get;    set;}   =   string.Empty;

        public  string?  PasswordHash    {   get;    set;}

        public  string?  ProfilePicture  {   get;    set;}

        public  DateTime    CreateAt    {   get;    set;}

        public  User()
        {
            CreateAt    =   DateTime.Now;
        }
    }

    public  class   Message
    {
        public  int Id  {   get;    set;}
        public  string  Content {   get;    set;}
        public  DateTime    Timestamp   {   get;    set;}

        public Message()
        {
            Content =   string.Empty;
        }
    }
}
