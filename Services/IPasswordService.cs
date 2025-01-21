using   PipperChat.Services;
using   PipperChat.Models;
using   PipperChat.Controllers;
using   PipperChat.Data;

namespace   PipperChat.Services
{
    //  Interface   defines     the contract    for password    hashing
    public  interface   IPasswordService
    {
        //  Contains    the hash,salt   and parameters

        string  HashPassword(string password);
        //  Verifies    a   password    against a   stored  hash    string
        bool    VerifyPasswordHash(string   password,   string  storedHash);

    }
}