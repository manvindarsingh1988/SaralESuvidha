using Fido2NetLib;

namespace SaralESuvidha.Models
{
    public class FidoCredential
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string CredentialId { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserHandle { get; set; }
    }
    public class RegisterVerifyRequest
    {
        public AuthenticatorAttestationRawResponse Credential { get; set; }
        public string UserName { get; set; }
    }


    public class CredentialRequest
    {
        public string UserName { get; set; }
    }

    public class VerifyRequest
    {
        public AuthenticatorAssertionRawResponse Credential { get; set; }
        public string UserName { get; set; }
    }
}
