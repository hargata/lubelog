using CarCareTracker.Helper;
using CarCareTracker.Models;

namespace CarCareTracker.Logic
{
    public class AutomatedEventLogic: BackgroundService
    {
        private readonly INotificationLogic _notificationLogic;
        private readonly IConfigHelper _config;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1);
        private TimeSpan _targetTime;
        private DateTime _nextRunTime;
        private ILogger<AutomatedEventLogic> _logger;
        public AutomatedEventLogic(INotificationLogic notificationLogic, IConfigHelper config, ILogger<AutomatedEventLogic> logger)
        {
            _notificationLogic = notificationLogic;
            _config = config;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //configure time
            NotificationConfig notificationConfig = _config.GetNotificationConfig();
            int targetHour = notificationConfig.HourToCheck;
            int targetMinute = notificationConfig.MinuteToCheck;
            _targetTime = new TimeSpan(targetHour, targetMinute, 0);
            var currentTime = DateTime.Now;
            _nextRunTime = currentTime.Date + _targetTime;
            if (currentTime > _nextRunTime)
            {
                _nextRunTime = _nextRunTime.AddDays(1);
            }
            // Calculate time until the next minute (top of the minute)
            var nextMinute = currentTime.AddMinutes(1).AddSeconds(-currentTime.Second).AddMilliseconds(-currentTime.Millisecond);
            var initialDelay = nextMinute - currentTime;
            // Wait until the next minute starts, so that automated events always start at a new minute
            await Task.Delay(initialDelay, cancellationToken);

            //initialized log
            _logger.LogInformation($"Automated Events Enabled - Current Time: {DateTime.Now.ToString()} Next Run Time: {_nextRunTime.ToString()}");
            using PeriodicTimer timer = new PeriodicTimer(_period);
            try
            {
                while (!cancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync(cancellationToken))
                {
                    if (DateTime.Now > _nextRunTime)
                    {
                        _nextRunTime = _nextRunTime.AddDays(1);
                        _logger.LogInformation("Running Automated Events");
                        await _notificationLogic.RunAutomatedEvents();
                        _logger.LogInformation($"Automated Events Completed - Current Time: {DateTime.Now.ToString()} Next Run Time: {_nextRunTime.ToString()}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // This exception is expected when the stoppingToken is canceled
                _logger.LogInformation("Automated Events Background Task Is Stopping Due To Cancellation.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An Error Cccurred In The Automated Events Background Task: {ex.Message}");
            }
        }
    }
}