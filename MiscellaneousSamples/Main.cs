using System;

namespace MiscellaneousSamples
{
    public static class Samples
    {
        public static void Main()
        {
            var rsi = new SecondaryIndices();
            rsi.KeysIndex();

            var rs = new RiakSearch();
            rs.NumberSearch();
            rs.DateSearch();
        }
    }
}

