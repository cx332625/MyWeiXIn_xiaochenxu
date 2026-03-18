namespace aspnetapp.Models.Shared
{
    public static class SignalRMethods
    {
        public const string ReceivePrintJob = "ReceivePrintJob";
        public const string PrintStatusUpdated = "PrintStatusUpdated";
        public const string DeviceRegistered = "DeviceRegistered";
        public const string DeviceDisconnected = "DeviceDisconnected";
    }

    public static class SignalRGroups
    {
        public const string AllDevices = "AllDevices";
    }
}
