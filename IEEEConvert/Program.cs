using System;

///
/// https://en.wikipedia.org/wiki/IEEE_754
///

namespace IEEEConvert
{
    class Program
    {
        static void Main(string[] args)
        {

            // Test your values here
            float temp = Convert_IEEE_Float((uint)32832, (uint)39836);
            Console.WriteLine(temp.ToString());

            double dtemp = Pack64BitLongLong(36964, 46100, 46100, 46100);// 46236, 32785);  //   32785, 46236, 0, 3);
            Console.WriteLine(dtemp.ToString());

        }

        /// <summary>
        /// Convert_IEEE_Float:
        /// This is just some IEEE wizardry for float conversion from (2) 16 bit registers.
        /// Information detailing the steps is in the comments below.
        /// Note: Register 1 is the low word.
        /// </summary>
        /// <param name="reg1">first 16 bit modbus register (upper)</param>
        /// <param name="reg2">second 16 bit modbus register (lower)</param>
        /// <returns></returns>
        public static float Convert_IEEE_Float(uint reg1, uint reg2)
        {
            // This is just a quick equation converted from an IEEE float sheet in excel
            // Information detailing the steps
            // Register 2 (high word)
            ulong reg2_high = reg2 >> 8;
            ulong reg2_low = reg2 & 0xff;
            int exponent;
            float fvalue;
            float inverse;
            // Clear leading bit of the high word (flag for if we need to inverse)
            // Check first bit of high word, will be used to invert the value
            if ((reg2_high >> 7) == 1)
            {
                inverse = -1.0f;
            }
            else
            {
                inverse = 1.0f;
            }
            reg2_high &= ~(1UL << 7);
            // Get Exponent
            // Shift high register over one and then concatenate with first bit of low register
            // then subtract 0b1111111
            exponent = ((Convert.ToInt32(reg2_high) << 1) | (Convert.ToInt32(reg2_low >> 7))) - 0b1111111;
            // clear first bit from the low register
            reg2_low &= ~(1UL << 7);
            // get final 23 bits by concatenating the remaning bits from the low register of the high word, and
            // with the final 16 bits from the low word
            fvalue = (reg2_low << 16) | reg1;
            if (exponent > 1)
            {
                // if we have an exponent greater than 0
                // find our mantessa (significand)
                // take our previous value and divide by 800000hex and add 1
                fvalue = (float)((fvalue / 0x800000) + 1.0f);
                // find our float by taking the significand and multiplying it by 2 times our exponent
                fvalue = (float)(fvalue * Math.Pow(2.0, exponent) * inverse);
                return (fvalue);
            }
            else
            {
                // if exponent is 0, divide by 40000hex and add 1
                fvalue = (float)((fvalue / 0x40000) + 1.0f);
                fvalue = (float)(fvalue * Math.Pow(2.0, exponent) * inverse);
                return (fvalue);
            }
        }

        /// <summary>
        /// Convert int a double an IEEE754 long integer value from vour IEEE754 16 bit integer registers
        /// </summary>
        /// <param name="intValue1">: least significant 16 bits</param>
        /// <param name="intValue2">: second least 16 bits</param>
        /// <param name="intValue3">: third 16 bits</param>
        /// <param name="intValue4">: most significant 16 bits</param>
        /// <returns>a double type representation of the 64 bit integer value</returns>
        static double Pack64BitLongLong(uint intValue1, uint intValue2, uint intValue3, uint intValue4)
        {
            // create one 64 bit integer from the four 16 bit registers sent.
            // at least for the purposes of this program, the fourth register is
            // the 'most significant byte', so they get shifted 4, 3, 2, 1.
            // this 'push-shift' routine is how it had to be done as the pushed
            // values are too small to shift.
            ulong intValue = intValue4;
            intValue <<= 16;
            intValue += intValue3;
            intValue <<= 16;
            intValue += intValue2;
            intValue <<= 16;
            intValue += intValue1;

            // now we'll return it, but convert it to a double on the way
            return (intValue);
        }
    }
}
