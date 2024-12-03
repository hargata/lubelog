using CarCareTracker.External.Interfaces;
using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using CarCareTracker.Models.API.v2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace CarCareTracker.Controllers
{
    [Authorize]
    public class APIv2Controller : Controller
    {
        //private readonly string DateFormat = "yyyy-MM-dd HH:mm";
        private readonly string DateFormat = "yyyy-MM-dd";
        private readonly IVehicleDataAccess _dataAccess;
        private readonly INoteDataAccess _noteDataAccess;
        private readonly IServiceRecordDataAccess _serviceRecordDataAccess;
        private readonly IGasRecordDataAccess _gasRecordDataAccess;
        private readonly ICollisionRecordDataAccess _collisionRecordDataAccess;
        private readonly ITaxRecordDataAccess _taxRecordDataAccess;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IUpgradeRecordDataAccess _upgradeRecordDataAccess;
        private readonly IOdometerRecordDataAccess _odometerRecordDataAccess;
        private readonly ISupplyRecordDataAccess _supplyRecordDataAccess;
        private readonly IPlanRecordDataAccess _planRecordDataAccess;
        private readonly IPlanRecordTemplateDataAccess _planRecordTemplateDataAccess;
        private readonly IUserAccessDataAccess _userAccessDataAccess;
        private readonly IUserRecordDataAccess _userRecordDataAccess;
        private readonly IReminderHelper _reminderHelper;
        private readonly IGasHelper _gasHelper;
        private readonly IUserLogic _userLogic;
        private readonly IVehicleLogic _vehicleLogic;
        private readonly IOdometerLogic _odometerLogic;
        private readonly IFileHelper _fileHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConfigHelper _config;
        public APIv2Controller(IVehicleDataAccess dataAccess,
            IGasHelper gasHelper,
            IReminderHelper reminderHelper,
            INoteDataAccess noteDataAccess,
            IServiceRecordDataAccess serviceRecordDataAccess,
            IGasRecordDataAccess gasRecordDataAccess,
            ICollisionRecordDataAccess collisionRecordDataAccess,
            ITaxRecordDataAccess taxRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IUpgradeRecordDataAccess upgradeRecordDataAccess,
            IOdometerRecordDataAccess odometerRecordDataAccess,
            ISupplyRecordDataAccess supplyRecordDataAccess,
            IPlanRecordDataAccess planRecordDataAccess,
            IPlanRecordTemplateDataAccess planRecordTemplateDataAccess,
            IUserAccessDataAccess userAccessDataAccess,
            IUserRecordDataAccess userRecordDataAccess,
            IMailHelper mailHelper,
            IFileHelper fileHelper,
            IConfigHelper config,
            IUserLogic userLogic,
            IVehicleLogic vehicleLogic,
            IOdometerLogic odometerLogic)
        {
            _dataAccess = dataAccess;
            _noteDataAccess = noteDataAccess;
            _serviceRecordDataAccess = serviceRecordDataAccess;
            _gasRecordDataAccess = gasRecordDataAccess;
            _collisionRecordDataAccess = collisionRecordDataAccess;
            _taxRecordDataAccess = taxRecordDataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _upgradeRecordDataAccess = upgradeRecordDataAccess;
            _odometerRecordDataAccess = odometerRecordDataAccess;
            _supplyRecordDataAccess = supplyRecordDataAccess;
            _planRecordDataAccess = planRecordDataAccess;
            _planRecordTemplateDataAccess = planRecordTemplateDataAccess;
            _userAccessDataAccess = userAccessDataAccess;
            _userRecordDataAccess = userRecordDataAccess;
            _mailHelper = mailHelper;
            _gasHelper = gasHelper;
            _reminderHelper = reminderHelper;
            _userLogic = userLogic;
            _odometerLogic = odometerLogic;
            _vehicleLogic = vehicleLogic;
            _fileHelper = fileHelper;
            _config = config;
        }
        public IActionResult Index()
        {
            return View();
        }
        private int GetUserID()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        private decimal DecimalParse(string s)
        {
            // culture invariant decimal conversion
            decimal ret;
            decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out ret);
            return ret;
        }
        private void AddOdometerRecord(int vehicleId, DateTime date, string notes, int odometer)
        {
            if (_config.GetUserConfig(User).EnableAutoOdometerInsert)
            {
                var odometerRecord = new OdometerRecord()
                {
                    VehicleId = vehicleId,
                    Date = date,
                    Notes = notes,
                    Mileage = odometer
                };
                _odometerLogic.AutoInsertOdometerRecord(odometerRecord);
            }
        }       
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/v2/vehicle/servicerecords")]
        public IActionResult ServiceRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            // Returns Mileage instead of Odometer!
            // return Json(vehicleRecords);
            var result = vehicleRecords.Select(x => new GenericRecordApiModel { Date = x.Date.ToString(DateFormat), Description = x.Description, Cost = x.Cost, Notes = x.Notes, Odometer = x.Mileage, ExtraFields = x.ExtraFields });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/v2/vehicle/servicerecords/add")]
        public IActionResult AddServiceRecord(int vehicleId, [FromBody] GenericRecordApiModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                (input.Odometer < 0) ||
                (input.Cost < 0.0M))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Date, Description, Odometer, and Cost cannot be empty."));
            }
            try
            {
                var serviceRecord = new ServiceRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    Mileage = input.Odometer,
                    Description = input.Description,
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    // Cost = decimal.Parse(input.Cost),
                    Cost = input.Cost,
                    ExtraFields = input.ExtraFields,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _serviceRecordDataAccess.SaveServiceRecordToVehicle(serviceRecord);
                AddOdometerRecord(vehicleId, DateTime.Parse(input.Date), string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes, input.Odometer);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, $"Added Service Record via API - Description: {serviceRecord.Description}");
                return Json(OperationResponse.Succeed("Service Record Added"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/v2/vehicle/repairrecords")]
        public IActionResult RepairRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            // Returns Mileage instead of Odometer!
            // return Json(vehicleRecords);
            var result = vehicleRecords.Select(x => new GenericRecordApiModel { Date = x.Date.ToString(DateFormat), Description = x.Description, Cost = x.Cost, Notes = x.Notes, Odometer = x.Mileage, ExtraFields = x.ExtraFields });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/v2/vehicle/repairrecords/add")]
        public IActionResult AddRepairRecord(int vehicleId, [FromBody] GenericRecordApiModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                (input.Odometer < 0) ||
                (input.Cost < 0.0M))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Date, Description, Odometer, and Cost cannot be empty."));
            }
            try
            {
                var repairRecord = new CollisionRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    Mileage = input.Odometer,
                    Description = input.Description,
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    // Cost = decimal.Parse(input.Cost),
                    Cost = input.Cost,
                    ExtraFields = input.ExtraFields,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _collisionRecordDataAccess.SaveCollisionRecordToVehicle(repairRecord);
                AddOdometerRecord(vehicleId, DateTime.Parse(input.Date), string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes, input.Odometer);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, $"Added Repair Record via API - Description: {repairRecord.Description}");
                return Json(OperationResponse.Succeed("Repair Record Added"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/v2/vehicle/upgraderecords")]
        public IActionResult UpgradeRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            // Returns Mileage instead of Odometer!
            // return Json(vehicleRecords);
            var result = vehicleRecords.Select(x => new GenericRecordApiModel { Date = x.Date.ToString(DateFormat), Description = x.Description, Cost = x.Cost, Notes = x.Notes, Odometer = x.Mileage, ExtraFields = x.ExtraFields });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/v2/vehicle/upgraderecords/add")]
        public IActionResult AddUpgradeRecord(int vehicleId, GenericRecordApiModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                (input.Odometer < 0) ||
                (input.Cost < 0.0M))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Date, Description, Odometer, and Cost cannot be empty."));
            }
            try
            {
                var upgradeRecord = new UpgradeRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    Mileage = input.Odometer,
                    Description = input.Description,
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    // Cost = decimal.Parse(input.Cost),
                    Cost = input.Cost,
                    ExtraFields = input.ExtraFields,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(upgradeRecord);
                AddOdometerRecord(vehicleId, DateTime.Parse(input.Date), string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes, input.Odometer);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, $"Added Upgrade Record via API - Description: {upgradeRecord.Description}");
                return Json(OperationResponse.Succeed("Upgrade Record Added"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/v2/vehicle/taxrecords")]
        public IActionResult TaxRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            // return Json(_taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId));
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId).Select(x => new TaxRecordApiModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost, Notes = x.Notes, ExtraFields = x.ExtraFields });
            return Json(result);
        }
    [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/v2/vehicle/taxrecords/add")]
        public IActionResult AddTaxRecord(int vehicleId, TaxRecordApiModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                (input.Cost < 0.0M))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Date, Description, and Cost cannot be empty."));
            }
            try
            {
                var taxRecord = new TaxRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    Description = input.Description,
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    // Cost = decimal.Parse(input.Cost),
                    Cost = input.Cost,
                    ExtraFields = input.ExtraFields,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _taxRecordDataAccess.SaveTaxRecordToVehicle(taxRecord);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, $"Added Tax Record via API - Description: {taxRecord.Description}");
                return Json(OperationResponse.Succeed("Tax Record Added"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }        
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/v2/vehicle/odometerrecords")]
        public IActionResult OdometerRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            //determine if conversion is needed.
            if (vehicleRecords.All(x => x.InitialMileage == default))
            {
                vehicleRecords = _odometerLogic.AutoConvertOdometerRecord(vehicleRecords);
            }
            // Returns Mileage instead of Odometer and InitialMileage instead of InitialOdometer!
            // return Json(vehicleRecords);
            var result = vehicleRecords.Select(x => new OdometerRecordApiModel { Date = x.Date.ToShortDateString(), InitialOdometer = x.InitialMileage, Odometer = x.Mileage, Notes = x.Notes, ExtraFields = x.ExtraFields });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/v2/vehicle/odometerrecords/add")]
        public IActionResult AddOdometerRecord(int vehicleId, [FromBody] OdometerRecordApiModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                (input.Odometer < 0))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Date and Odometer cannot be empty."));
            }
            try
            {
                var odometerRecord = new OdometerRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    InitialMileage = (input.InitialOdometer < 0) ? _odometerLogic.GetLastOdometerRecordMileage(vehicleId, new List<OdometerRecord>()) : input.InitialOdometer,
                    Mileage = input.Odometer,
                    ExtraFields = input.ExtraFields,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _odometerRecordDataAccess.SaveOdometerRecordToVehicle(odometerRecord);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, $"Added Odometer Record via API - Mileage: {odometerRecord.Mileage.ToString()}");
                return Json(OperationResponse.Succeed("Odometer Record Added"));
            } catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        public List<GasRecordViewModel> GetGasRecordViewModels(List<GasRecord> result, bool useMPG, bool useUKMPG)
        {
            //need to order by to get correct results
            result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            var computedResults = new List<GasRecordViewModel>();
            int previousMileage = 0;
            decimal unFactoredConsumption = 0.00M;
            int unFactoredMileage = 0;
            //perform computation.
            for (int i = 0; i < result.Count; i++)
            {
                var currentObject = result[i];
                decimal convertedConsumption;
                if (useUKMPG && useMPG)
                {
                    //if we're using UK MPG and the user wants imperial calculation insteace of l/100km
                    //if UK MPG is selected then the gas consumption are stored in liters but need to convert into UK gallons for computation.
                    convertedConsumption = currentObject.Gallons / 4.546M;
                }
                else
                {
                    convertedConsumption = currentObject.Gallons;
                }
                if (i > 0)
                {
                    var deltaMileage = currentObject.Mileage - previousMileage;
                    if (deltaMileage < 0)
                    {
                        deltaMileage = 0;
                    }
                    var gasRecordViewModel = new GasRecordViewModel()
                    {
                        Id = currentObject.Id,
                        VehicleId = currentObject.VehicleId,
                        MonthId = currentObject.Date.Month,
                        Date = currentObject.Date.ToString(DateFormat),
                        Mileage = currentObject.Mileage,
                        Gallons = convertedConsumption,
                        Cost = currentObject.Cost,
                        DeltaMileage = deltaMileage,
                        CostPerGallon = convertedConsumption > 0.00M ? currentObject.Cost / convertedConsumption : 0,
                        IsFillToFull = currentObject.IsFillToFull,
                        MissedFuelUp = currentObject.MissedFuelUp,
                        Notes = currentObject.Notes,
                        Tags = currentObject.Tags,
                        ExtraFields = currentObject.ExtraFields
                    };
                    if (currentObject.MissedFuelUp)
                    {
                        //if they missed a fuel up, we skip MPG calculation.
                        gasRecordViewModel.MilesPerGallon = 0;
                        //reset unFactored vars for missed fuel up because the numbers wont be reliable.
                        unFactoredConsumption = 0;
                        unFactoredMileage = 0;
                    }
                    else if (currentObject.IsFillToFull)
                    {
                        //if user filled to full.
                        if (convertedConsumption > 0.00M && deltaMileage > 0)
                        {
                            try
                            {
                                gasRecordViewModel.MilesPerGallon = useMPG ? (unFactoredMileage + deltaMileage) / (unFactoredConsumption + convertedConsumption) : 100 / ((unFactoredMileage + deltaMileage) / (unFactoredConsumption + convertedConsumption));
                            }
                            catch (Exception ex)
                            {
                                gasRecordViewModel.MilesPerGallon = 0;
                            }
                        }
                        //reset unFactored vars
                        unFactoredConsumption = 0;
                        unFactoredMileage = 0;
                    }
                    else
                    {
                        unFactoredConsumption += convertedConsumption;
                        unFactoredMileage += deltaMileage;
                        gasRecordViewModel.MilesPerGallon = 0;
                    }
                    computedResults.Add(gasRecordViewModel);
                }
                else
                {
                    computedResults.Add(new GasRecordViewModel()
                    {
                        Id = currentObject.Id,
                        VehicleId = currentObject.VehicleId,
                        MonthId = currentObject.Date.Month,
                        Date = currentObject.Date.ToString(DateFormat),
                        Mileage = currentObject.Mileage,
                        Gallons = convertedConsumption,
                        Cost = currentObject.Cost,
                        DeltaMileage = 0,
                        MilesPerGallon = 0,
                        CostPerGallon = convertedConsumption > 0.00M ? currentObject.Cost / convertedConsumption : 0,
                        IsFillToFull = currentObject.IsFillToFull,
                        MissedFuelUp = currentObject.MissedFuelUp,
                        Notes = currentObject.Notes,
                        Tags = currentObject.Tags,
                        ExtraFields = currentObject.ExtraFields
                    });
                }
                previousMileage = currentObject.Mileage;
            }
            return computedResults;
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/v2/vehicle/gasrecords")]
        public IActionResult GasRecords(int vehicleId, bool useMPG, bool useUKMPG)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            // Returns GasRecord directly with Mileage instead of Odometer!
            // return Json(GetGasRecordViewModels(vehicleRecords, useMPG, useUKMPG));
            // GasRecordApiModel
            var result = GetGasRecordViewModels(vehicleRecords, useMPG, useUKMPG)
                .Select(x => new GasRecordApiModel
                {
                    Date = x.Date,
                    Odometer = x.Mileage,
                    Cost = x.Cost,
                    FuelConsumed = x.Gallons,
                    FuelEconomy = x.MilesPerGallon,
                    IsFillToFull = x.IsFillToFull,
                    MissedFuelUp = x.MissedFuelUp,
                    Notes = x.Notes,
                    ExtraFields = x.ExtraFields
                });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/v2/vehicle/gasrecords/add")]
        public IActionResult AddGasRecord(int vehicleId, [FromBody] GasRecordApiModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                (input.Odometer < 0) ||
                (input.FuelConsumed < 0.00M) ||
                (input.Cost < 0.00M) /*||
                string.IsNullOrWhiteSpace(input.IsFillToFull) ||
                string.IsNullOrWhiteSpace(input.MissedFuelUp)*/
                )
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Date, Odometer, FuelConsumed, IsFillToFull, MissedFuelUp, and Cost cannot be empty."));
            }
            try
            {
                var gasRecord = new GasRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    Mileage = input.Odometer,
                    // Gallons = decimal.Parse(input.FuelConsumed),
                    Gallons = input.FuelConsumed,
                    IsFillToFull = input.IsFillToFull,
                    MissedFuelUp = input.MissedFuelUp,
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    Cost = input.Cost,
                    ExtraFields = input.ExtraFields,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _gasRecordDataAccess.SaveGasRecordToVehicle(gasRecord);
                AddOdometerRecord(vehicleId, DateTime.Parse(input.Date), string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes, input.Odometer);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, $"Added Gas record via API - Mileage: {gasRecord.Mileage.ToString()}");
                return Json(OperationResponse.Succeed("Gas Record Added"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }        
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/v2/vehicle/reminders")]
        public IActionResult Reminders(int vehicleId)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            // Returns Date instead of DueDate and Mileage instead of DueOdometer!
            //return Json(_reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now));
            var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now).Select(x => new ReminderApiModel { Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), Notes = x.Notes, DueDate = x.Date.ToString(DateFormat), DueOdometer = x.Mileage });
            return Json(results);
        }        
    }
}
