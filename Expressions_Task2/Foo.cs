namespace Expressions_Task2
{
    public class Foo
    {
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }
        public double DoubleProperty { get; set; }

        public override string ToString()
        {
            return $"Foo instance:\n{nameof(IntProperty)} - {IntProperty}\n{nameof(StringProperty)} - {StringProperty}\n{nameof(DoubleProperty)} - {DoubleProperty}\n";
        }
    }
}