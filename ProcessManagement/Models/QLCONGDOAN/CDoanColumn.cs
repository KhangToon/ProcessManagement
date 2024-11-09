using ProcessManagement.Commons;

namespace ProcessManagement.Models.QLCONGDOAN
{
    public class CDoanColumn
    {
        public Propertyy TenCDoan { get; set; } = new() { DisplayName = DispName.TenCDoan, Type = typeof(string), AlowDatabase = true };

        public List<CDoanColumnCa> CDoanColumnCas = new();

        public static class DispName
        {
            public const string TenCDoan = "Tên công đoạn";
        }

    }

    public class CDoanColumnCa
    {

    }
}
