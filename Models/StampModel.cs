namespace Sammlerplattform.Models
{
    public class StampModel
    {
        public StampPotential StampPotential { get; set; } = new StampPotential();
        public StampEntity StampEntity { get; set; } = new StampEntity();
        public StampScan StampScan { get; set; } = new StampScan();
        public string ColorStamp { get; set; } = string.Empty;
    }
}
