namespace CarCareTracker.Models
{
    public enum AutomatedEvent
    {
        AllReminder = 0,
        ReminderStateChanged = 1,
        BackupEmail = 2,
        UpdateRecurringTax = 3,
        CleanTempFile = 4,
        DeepClean = 5
    }
}