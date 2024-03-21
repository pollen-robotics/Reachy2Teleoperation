namespace TeleopReachy
{
    public class Maths
    {
        public static bool isApproxEqual(float a, float b, float epsilon)
        {
            if (a >= b - epsilon && a <= b + epsilon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
