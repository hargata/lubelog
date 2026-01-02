using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class EquipmentRecordDataAccess : IEquipmentRecordDataAccess
    {
        private ILiteDBHelper _liteDB { get; set; }
        private static string tableName = "equipmentrecords";
        public EquipmentRecordDataAccess(ILiteDBHelper liteDB)
        {
            _liteDB = liteDB;
        }
        public List<EquipmentRecord> GetEquipmentRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<EquipmentRecord>(tableName);
            var equipmentRecords = table.Find(Query.EQ(nameof(EquipmentRecord.VehicleId), vehicleId));
            return equipmentRecords.ToList() ?? new List<EquipmentRecord>();
        }
        public EquipmentRecord GetEquipmentRecordById(int equipmentRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<EquipmentRecord>(tableName);
            return table.FindById(equipmentRecordId);
        }
        public bool DeleteEquipmentRecordById(int equipmentRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<EquipmentRecord>(tableName);
            table.Delete(equipmentRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveEquipmentRecordToVehicle(EquipmentRecord equipmentRecord)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<EquipmentRecord>(tableName);
            table.Upsert(equipmentRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllEquipmentRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<EquipmentRecord>(tableName);
            var equipmentRecords = table.DeleteMany(Query.EQ(nameof(EquipmentRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}
