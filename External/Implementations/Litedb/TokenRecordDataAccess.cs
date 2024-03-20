using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class TokenRecordDataAccess : ITokenRecordDataAccess
    {
        private ILiteDBInjection _liteDB { get; set; }
        private static string tableName = "tokenrecords";
        public TokenRecordDataAccess(ILiteDBInjection liteDB)
        {
           _liteDB = liteDB;
        }
        public List<Token> GetTokens()
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Token>(tableName);
            return table.FindAll().ToList();
        }
        public Token GetTokenRecordByBody(string tokenBody)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Token>(tableName);
            var tokenRecord = table.FindOne(Query.EQ(nameof(Token.Body), tokenBody));
            return tokenRecord ?? new Token();
        }
        public Token GetTokenRecordByEmailAddress(string emailAddress)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Token>(tableName);
            var tokenRecord = table.FindOne(Query.EQ(nameof(Token.EmailAddress), emailAddress));
            return tokenRecord ?? new Token();
        }
        public bool CreateNewToken(Token token)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Token>(tableName);
            table.Insert(token);
            db.Checkpoint();
            return true;
        }
        public bool DeleteToken(int tokenId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Token>(tableName);
            table.Delete(tokenId);
            db.Checkpoint();
            return true;
        }
    }
}