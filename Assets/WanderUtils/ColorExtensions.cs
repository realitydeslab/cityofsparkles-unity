using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WanderUtils
{
    public static class ColorExtensions 
    {
        public static Color ColorWithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
