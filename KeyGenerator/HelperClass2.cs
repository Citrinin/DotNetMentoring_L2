namespace KeyGenerator
{
    public static class HelperClass2
    {
        public static int ConditionalMultiply(int x)
        {
            return x <= 999 ? x * 10 : x;
        }

        public static bool IfNumberEqualsZero(int x)
        {
            return x == 0;
        }
    }
}