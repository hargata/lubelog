using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class TokenRecordDataAccess : ITokenRecordDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "tokenrecords";
        public List<Token> GetTokens()
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Token>(tableName);
                return table.FindAll().ToList();
            };
        }
        public Token GetTokenRecordByBody(string tokenBody)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Token>(tableName);
                var tokenRecord = table.FindOne(Query.EQ(nameof(Token.Body), tokenBody));
                return tokenRecord ?? new Token();
            };
        }
        public Token GetTokenRecordByEmailAddress(string emailAddress)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Token>(tableName);
                var tokenRecord = table.FindOne(Query.EQ(nameof(Token.EmailAddress), emailAddress));
                return tokenRecord ?? new Token();
            };
        }
        public bool CreateNewToken(Token token)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Token>(tableName);
                table.Insert(token);
                return true;
            };
        }
        public bool DeleteToken(int tokenId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Token>(tableName);
                table.Delete(tokenId);
                return true;
            };
        }
    }
}