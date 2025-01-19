using PipperChat.Models;
using System.ComponentModel.DataAnnotations;
using PipperChat.Data;

namespace PipperChat.Models  
{
    public class ExternalAuth
    {
        public int Id { get; set; }
    
        public string Provider { get; set; } = string.Empty; 
        
        public string ProviderUserId { get; set; } = string.Empty; 

        public int UserId { get; set; }
        
        public User User { get; set; } = null!; 

        public ExternalAuth() 
        {
            // Initialize 
            Provider = string.Empty;
            ProviderUserId = string.Empty;
            User = null!;
        }
    }
}
