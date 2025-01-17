namespace   PipperChat.Models{

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
}