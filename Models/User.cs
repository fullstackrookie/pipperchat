using System.ComponentModel.DataAnnotations;
using PipperChat.Data;
using PipperChat.Models;

namespace PipperChat.Models 
{
    public class User
    {
        public int Id { get; set; }
        
        public string Username { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string? PasswordHash { get; set; }
        public  string? PasswordSalt    {   get;    set;}
    
        public string? ProfilePicture { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastSeen { get; set; }
        
        public DateTime DateOfBirth { get; set; }

        public  string  RefreshToken    {   get;    set;}   =   string.Empty;
        public  DateTime  RefreshTokenExpiryTime  {   get;    set;}  

        
        //GoogleId property
        public string? GoogleId { get; set; }
        
        // Navigation properties
        public virtual ICollection<Message> SentMessages { get; set; }
        public virtual ICollection<Message> ReceivedMessages { get; set; }
        public virtual ICollection<GroupMember> GroupMembership { get; set; }
        
        public User()
        {
            CreatedAt = DateTime.Now;
            SentMessages = new HashSet<Message>();
            ReceivedMessages = new HashSet<Message>();
            GroupMembership = new HashSet<GroupMember>();
        }
    }
}