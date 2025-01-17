namespace   PipperChat.Models
{
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
}