using   PipperChat.Services;
using   PipperChat.Models;
using   PipperChat.Controllers;
using   PipperChat.Data;
using   System.Security.Cryptography;
using   System.Text;

namespace   PipperChat.Services
{
    //  Implementing    Secure  Passowrd    hashing
    public  class   PasswordService :   IPasswordService
    {
        private const   int SaltSize    =   16;
        private const   int KeySize     =   32;
        private const   int Iterations  =   10000;
        private static  readonly    HashAlgorithmName   HashAlgorithm   =   HashAlgorithmName.SHA256;
        private const   char    Delimiter   =   ':';

        public  string  HashPassword(string password)
        {
            var salt    =   RandomNumberGenerator.GetBytes(SaltSize);

            //  Generate    the hash
            var hash    =   Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithm,
                KeySize);

            return  string.Join(
                Delimiter,
                Convert.ToBase64String(hash),
                Convert.ToBase64String(salt),
                Iterations,
                HashAlgorithm.Name
            );
        }  

        public  bool    VerifyPasswordHash(string   password,   string  storedHash)
        {
            try
            {
                //  Split   the stored  hash    string  into    its components
                var parts   =   storedHash.Split(Delimiter);
                if  (parts.Length   !=  4)
                    return  false;

                //  Extract all components
                var hash    =   Convert.FromBase64String(parts[0]);
                var salt    =   Convert.FromBase64String(parts[1]);
                var iterations  =   int.Parse(parts[2]);
                var algorithm   =   new HashAlgorithmName(parts[3]);

                //  Compute a   new hash    with    the same    parameters
                var verificationHash    =   Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    salt,
                    iterations,
                    algorithm,
                    hash.Length
                );
                return  CryptographicOperations.FixedTimeEquals(hash,   verificationHash);

            }
            catch{
                return  false;
            }
        }  
    }
}