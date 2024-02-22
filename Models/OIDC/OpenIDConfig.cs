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
        public string State { get { return Guid.NewGuid().ToString().Substring(0, 8); } }
        public string RemoteAuthURL { get { return $"{AuthURL}?client_id={ClientId}&response_type=code&redirect_uri={RedirectURL}&scope={Scope}&state={State}"; } }
    }
}
