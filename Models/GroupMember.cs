namespace   PipperChat.Models{
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