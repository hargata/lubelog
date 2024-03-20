using CarCareTracker.Helper;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public interface ILiteDBInjection
    {
        LiteDatabase GetLiteDB();
        void DisposeLiteDB();
    }
    public class LiteDBInjection: ILiteDBInjection
    {
        public LiteDatabase db { get; set; }
        public LiteDBInjection()
        {
            if (db == null)
            {
                db = new LiteDatabase(StaticHelper.DbName);
            }
        }
        public LiteDatabase GetLiteDB()
        {
            if (db == null)
            {
                db = new LiteDatabase(StaticHelper.DbName);
            }
            return db;
        }
        public void DisposeLiteDB()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }
    }
}
