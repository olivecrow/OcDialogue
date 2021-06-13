using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcUtility
{
    public static class ColorExtension
    {
        public static Color SetR(this Color source, float value)
        {
            source.r = value;
            return source;
        }

        public static Color SetG(this Color source, float value)
        {
            source.g = value;
            return source;
        }

        public static Color SetB(this Color source, float value)
        {
            source.b = value;
            return source;
        }

        public static Color SetA(this Color source, float value)
        {
            source.a = value;
            return source;
        }
    }

}