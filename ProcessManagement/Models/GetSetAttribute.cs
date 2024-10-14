using static ProcessManagement.Models.PhieuXacDinhCD;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;

namespace ProcessManagement.Models
{
    public class GetSetAttribute
    {
        [IsSubmit(true)]
        public string Name { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "1000000")]
        public decimal Price { get; set; } = decimal.Zero;

        // Attribute sẽ được thêm động
        public string Description { get; set; } = string.Empty;

        [IsSubmit(false)]
        public string Category { get; set; } = string.Empty;


        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public class IsSubmitAttribute : Attribute
        {
            public bool Value { get; set; }

            public IsSubmitAttribute(bool value)
            {
                Value = value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        public class RangeAttribute : Attribute
        {
            public Type TypeId { get; }
            public string Minimum { get; }
            public string Maximum { get; }

            public RangeAttribute(Type type, string minimum, string maximum)
            {
                TypeId = type;
                Minimum = minimum;
                Maximum = maximum;
            }

            public override string ToString()
            {
                return $"Type: {TypeId}, Min: {Minimum}, Max: {Maximum}";
            }
        }

        static void DisplayAttributeValue(GetSetAttribute product, string propertyName, string attributeName)
        {
            var prop = typeof(GetSetAttribute).GetProperty(propertyName);
            var attr = prop.GetCustomAttribute(Type.GetType($"{attributeName}Attribute"));
            Console.WriteLine($"{propertyName} - {attributeName}: {attr}");
        }

        static void ChangeAttributeValue(GetSetAttribute product, string propertyName, string attributeName, object newValue)
        {
            var prop = typeof(GetSetAttribute).GetProperty(propertyName);
            var attr = prop.GetCustomAttribute(Type.GetType($"{attributeName}Attribute"));

            // Lấy trường Value của attribute
            var field = attr.GetType().GetField("Value");
            if (field != null)
            {
                // Thay đổi giá trị của trường
                field.SetValue(attr, newValue);
                Console.WriteLine($"Đã thay đổi giá trị {attributeName} cho {propertyName}");
            }
        }
    }

}
