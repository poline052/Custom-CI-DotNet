namespace Com.CI.Infrastructure
{
    public class InstallSetting
    {
        public string ServiceName { get; set; }
        public string PhysicalPath { get; set; }
        public ServiceTypes ServiceType { get; set; }
        public string ServiceDescription { get; set; }
        public string WebServiceHostName { get; set; }
        public string BindingType { get; set; }
        public int Port { get; set; }
    }
}
