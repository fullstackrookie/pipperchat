using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using   Microsoft.EntityFrameworkCore;

namespace   PipperChat.Data
{
    public class PipperChatContext : DbContext
    {
        public  PipperChatContext(DbContextOptions<PipperChatContext> options)    :   base(options)   {}

        // Dataset  Definition
        public  DbSet<User> Users   {   get;    set;    }
        public DbSet<Message>   Messages    {   get;    set;    }

        public  DbSet<Group>    Groups  {   get;    set;}

        public  DbSet<GroupMember>  GroupMembers {   get;    set;}
    

    protected override    void    OnModelCreating(ModelBuilder    modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //  Confugure   relationships
        modelBuilder.Entity<Message>()
        .HasOne(m   =>  m.Sender)
        .WithMany(u =>  u.SentMessages)
        .HasForeignKey(m    =>  m.SenderId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupMember>()
        .HasOne(gm  =>  gm.User)
        .WithMany(u =>  u.GroupMembership)
        .HasForeignKey(gm   =>  gm.UserId)
        .OnDelete(DeleteBehavior.Cascade);
        }}
    

    public  class   User
    {
        public  int Id  {   get;    set;    }
        
        public  string  Username    {   get;    set;}   =   string.Empty;
        
        public  string  Email   {   get;    set;}   =   string.Empty;

        public  string?  PasswordHash    {   get;    set;}

        public  string?  ProfilePicture  {   get;    set;}

        public  DateTime    CreateAt    {   get;    set;}

        public  DateTime?   LastSeen    {   get;    set;}
        
        public  DateTime    DateofBirth {   get;    set;}

        // Navigation properties
        public  virtual ICollection<Message>    SentMessages    {   get;    set;}
        public  virtual ICollection<GroupMember>    GroupMembership {   get;    set;}

        public  User()
        {
            CreateAt    =   DateTime.Now;
            SentMessages=   new HashSet<Message>();
            GroupMembership    =   new HashSet<GroupMember>();
        }
    }

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

    public  class   Group
    {
        public  int Id  {   get;    set;}
        public  string  Name    {   get;    set;}   =   string.Empty;
        public  string? Description {   get;    set;}
        public  DateTime    CreateAt    {   get;    set;}
        public  bool    IsPrivate   {   get;    set;}

        // Navigate properties
        public  virtual ICollection<GroupMember>    Members {   get;    set;}
        public  virtual ICollection<Message>    Messages    {   get;    set;}  

        public  Group()
        {
            CreateAt    =   DateTime.Now;
            Members =   new HashSet<GroupMember>();
            Messages    =   new HashSet<Message>();
        }

    }
    public class GroupMember
    {
        public  int Id  {   get;    set;}
        public int  GroupId {   get;    set;}
        public  int UserId  {   get;    set;}
        public  GroupRole   Role    {   get;    set;}
        public  DateTime    JoinedAt    {   get;    set;}

        // Navigation   properties
        public  virtual Group   Group   {   get;    set;}   =   null!;
        public  virtual User    User    {   get;    set;}   =   null!;

        public  GroupMember()
        {
            JoinedAt    =   DateTime.Now;
            Role    =   GroupRole.Member;
        }
    }

    public  enum    GroupRole{
        Member,
        Moderator,
        Admin
    }
}
