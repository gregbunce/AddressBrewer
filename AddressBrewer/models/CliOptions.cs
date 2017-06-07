namespace AddressBrewer.models
{
    public class CliOptions
    {
        public string SGIDServer { get; set; }
        public string SGIDDatabase { get; set; }
        public string SGIDID { get; set; }
        public string SdeConnectionPath { get; set; }
        public string OutputFile { get; set; }
        public OutputType OutputType { get; set; }
        public bool Verbose { get; set; }
        public string DatabaseName { get; set; }
        public string Server { get; set; }
    }
}
