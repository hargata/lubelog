using CarCareTracker.Helper;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public interface ILiteDBInjection
    {
        LiteDatabase GetLiteDB();    
    }
    public class LiteDBInjection: ILiteDBInjection
    {
        public LiteDatabase db { get; set; }
        public LiteDBInjection()
        {
            db = new LiteDatabase(StaticHelper.DbName);
        }
        public LiteDatabase GetLiteDB()
        {
            return db;
        }
    }
}
