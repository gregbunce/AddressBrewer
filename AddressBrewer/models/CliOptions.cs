namespace AddressBrewer.models
{
    public class CliOptions
    {
        public string SdeConnectionPath { get; set; }
        public string OutputFile { get; set; }
        public OutputType OutputType { get; set; }
        public bool Verbose { get; set; }
    }
}
