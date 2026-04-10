namespace CarCareTracker.Logic
{
    public class AutomatedEventLogic: BackgroundService
    {
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1);
        private TimeSpan _targetTime;
        private DateTime _nextRunTime;
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {

        }
    }
}
