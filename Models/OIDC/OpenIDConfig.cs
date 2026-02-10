namespace CarCareTracker.Models
{
    public class OpenIDConfig
    {
        public string Name { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string AuthURL { get; set; } = string.Empty;
        public string TokenURL { get; set; } = string.Empty;
        public string RedirectURL { get; set; } = string.Empty;
        public string Scope { get; set; } = "openid email";
        public string State { get; set; } = string.Empty;
        public string CodeChallenge { get; set; } = string.Empty;
        public bool ValidateState { get; set; } = false;
        public bool DisableRegularLogin { get; set; } = false;
        public bool UsePKCE { get; set; } = false;
        public string LogOutURL { get; set; } = "";
        public string UserInfoURL { get; set; } = "";
        public string JwksURL { get; set; } = "";
        public string RemoteAuthURL { get {
                var redirectUrl = $"{AuthURL}?client_id={ClientId}&response_type=code&redirect_uri={RedirectURL}&scope={Scope}&state={State}";
                if (UsePKCE)
                {
                    redirectUrl += $"&code_challenge={CodeChallenge}&code_challenge_method=S256";
                }
                return redirectUrl; 
            } }
    }
}
