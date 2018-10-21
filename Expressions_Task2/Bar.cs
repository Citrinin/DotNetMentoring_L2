namespace Expressions_Task2
{
    public class Bar
    {
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }
        public decimal DecimalProperty { get; set; }

        public override string ToString()
        {
            return $"Bar instance:\n{nameof(IntProperty)} - {IntProperty}\n{nameof(StringProperty)} - {StringProperty}\n{nameof(DecimalProperty)} - {DecimalProperty}\n";
        }
    }
}