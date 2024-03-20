namespace CarCareTracker.Models
{
    public class OpenIDConfig
    {
        public string Name { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthURL { get; set; }
        public string TokenURL { get; set; }
        public string RedirectURL { get; set; }
        public string Scope { get; set; }
        public string State { get; set; }
        public bool ValidateState { get; set; } = false;
        public bool DisableRegularLogin { get; set; } = false;
        public string LogOutURL { get; set; } = "";
        public string RemoteAuthURL { get { return $"{AuthURL}?client_id={ClientId}&response_type=code&redirect_uri={RedirectURL}&scope={Scope}&state={State}"; } }
    }
}
