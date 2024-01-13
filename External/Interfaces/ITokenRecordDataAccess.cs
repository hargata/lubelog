using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface ITokenRecordDataAccess
    {
        public List<Token> GetTokens();
        public Token GetTokenRecordByBody(string tokenBody);
        public bool CreateNewToken(Token token);
        public bool DeleteToken(int tokenId);
    }
}
