namespace EduCare.Application.Models.Core
{
    public class SalaryScaleVm
    {
        public string Year { get; set; } = null!;
        public int Category { get; set; }
        public string Zone { get; set; } = null!;
        public string Echelon { get; set; } = null!;
        public double Basic { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
