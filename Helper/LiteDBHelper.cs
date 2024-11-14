using CarCareTracker.Models;
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
            if (db.UserVersion == 0)
            {
                //migration required to convert ints to decimals
                var collections = db.GetCollectionNames();
                foreach (string collection in collections)
                {
                    var documents = db.GetCollection(collection);
                    foreach (var document in documents.FindAll())
                    {
                        if (document.ContainsKey(nameof(GenericRecord.Mileage)))
                        {
                            document[nameof(GenericRecord.Mileage)] = Convert.ToDecimal(document[nameof(GenericRecord.Mileage)].AsInt32);
                            //check for initial mileage as well
                            if (document.ContainsKey(nameof(OdometerRecord.InitialMileage)))
                            {
                                document[nameof(OdometerRecord.InitialMileage)] = Convert.ToDecimal(document[nameof(OdometerRecord.InitialMileage)].AsInt32);
                            }
                            documents.Update(document);
                        }
                    }
                }
                db.UserVersion = 1;
            }
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
