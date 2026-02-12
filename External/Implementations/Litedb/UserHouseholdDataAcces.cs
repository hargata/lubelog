using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;

namespace CarCareTracker.External.Implementations
{
    public class UserHouseholdDataAccess : IUserHouseholdDataAccess
    {
        private ILiteDBHelper _liteDB { get; set; }
        private static string tableName = "userhouseholdrecords";
        public UserHouseholdDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<UserHousehold> GetUserHouseholdByParentUserId(int parentUserId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UserHousehold>(tableName);
            return table.Find(x => x.Id.ParentUserId == parentUserId).ToList();
        }
        public List<UserHousehold> GetUserHouseholdByChildUserId(int childUserId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UserHousehold>(tableName);
            return table.Find(x => x.Id.ChildUserId == childUserId).ToList();
        }
        public UserHousehold GetUserHouseholdByParentAndChildUserId(int parentUserId, int childUserId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UserHousehold>(tableName);
            return table.Find(x => x.Id.ParentUserId == parentUserId && x.Id.ChildUserId == childUserId).FirstOrDefault();
        }
        public bool SaveUserHousehold(UserHousehold userHousehold)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UserHousehold>(tableName);
            table.Upsert(userHousehold);
            db.Checkpoint();
            return true;
        }
        public bool DeleteUserHousehold(int parentUserId, int childUserId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UserHousehold>(tableName);
            table.DeleteMany(x => x.Id.ParentUserId == parentUserId && x.Id.ChildUserId == childUserId);
            db.Checkpoint();
            return true;
        }
        /// <summary>
        /// Delete all household records when a parent user is deleted.
        /// </summary>
        /// <param name="parentUserId"></param>
        /// <returns></returns>
        public bool DeleteAllHouseholdRecordsByParentUserId(int parentUserId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UserHousehold>(tableName);
            table.DeleteMany(x => x.Id.ParentUserId == parentUserId);
            db.Checkpoint();
            return true;
        }
        /// <summary>
        /// Delete all household records when a child user is deleted.
        /// </summary>
        /// <param name="childUserId"></param>
        /// <returns></returns>
        public bool DeleteAllHouseholdRecordsByChildUserId(int childUserId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UserHousehold>(tableName);
            table.DeleteMany(x => x.Id.ChildUserId == childUserId);
            db.Checkpoint();
            return true;
        }
    }
}