using LiteDB;

namespace CarCareTracker.Helper;

public interface ILiteDBHelper
{
    LiteDatabase GetLiteDB();
    void DisposeLiteDB();
}
public class LiteDBHelper: ILiteDBHelper
{
    public LiteDatabase db { get; set; }
    public LiteDBHelper()
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
