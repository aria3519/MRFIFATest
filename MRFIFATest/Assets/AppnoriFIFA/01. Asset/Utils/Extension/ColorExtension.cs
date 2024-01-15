using UnityEngine;
using System.Collections;
using System;

namespace Jisu.Utils
{
    public static class Colors
    {
        public static readonly Color Gold = new Color (1.00f, 0.80f, 0.00f);
        public static readonly Color Orange = new Color (1.00f, 0.50f, 0.00f);
        public static readonly Color Pink = new Color (1.00f, 0.00f, 1.00f);
        public static readonly Color Brown = new Color (0.50f, 0.25f, 0.00f);
        public static readonly Color Violet = new Color (0.50f, 0.00f, 1.00f);
        public static readonly Color Lime = new Color (0.75f, 1.00f, 0.00f);
        public static readonly Color Mint = new Color (0.00f, 1.00f, 0.50f);
        public static readonly Color Cyan = new Color (0.00f, 1.00f, 1.00f);
        public static readonly Color Sky = new Color (0.00f, 1.00f, 1.00f);

        public static readonly Color LightRed = new Color (1.00f, 0.50f, 0.50f);
        public static readonly Color LightBlue = new Color (0.50f, 0.50f, 1.00f);
        public static readonly Color LightGreen = new Color (0.50f, 1.00f, 0.50f);

        public static readonly Color LightYellow = new Color (1.00f, 1.00f, 0.75f);
        public static readonly Color LightOrange = new Color (1.00f, 0.75f, 0.50f);
        public static readonly Color LightPink = new Color (1.00f, 0.50f, 1.00f);
        public static readonly Color LightBrown = new Color (0.75f, 0.50f, 0.25f);
        public static readonly Color LightViolet = new Color (0.75f, 0.50f, 1.00f);
        public static readonly Color LightMint = new Color (0.50f, 1.00f, 0.75f);
        public static readonly Color LightSky = new Color (0.50f, 1.00f, 1.00f);
        public static readonly Color LightBlack = new Color (0.10f, 0.10f, 0.10f);

        public static readonly Color WhitePink = new Color (1.00f, 0.75f, 1.00f);

        public static readonly Color DarkRed = new Color (0.50f, 0.00f, 0.00f);
        public static readonly Color DarkGreen = new Color (0.00f, 0.50f, 0.00f);
        public static readonly Color DarkBlue = new Color (0.00f, 0.00f, 0.50f);

        public static readonly Color DarkBrown = new Color (0.25f, 0.12f, 0.00f);
        public static readonly Color DarkMint = new Color (0.00f, 0.75f, 0.50f);
        public static readonly Color DarkGray = new Color (0.20f, 0.20f, 0.20f);

        public static Color ChangeAlpha(in Color color, in float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static Color Random(in float alpha = 1f)
        {
            return new Color(
                UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(0f, 1f),
                alpha);
        }

        public static Color RandomHue(in float alpha = 1f)
        {
            Color hsv = Color.HSVToRGB(
                UnityEngine.Random.Range(0f, 1f), 1f, 1f);

            return new Color(hsv.r, hsv.g, hsv.b, alpha);
        }
    }

    public static class ColorExtension
    {
        /// <summary>
        /// <para/> Alpha 값 변경 후 리턴
        /// </summary>
        public static Color SetAlpha(this Color color, float alpha)
        {
            if (alpha > 1) alpha = 1f;
            else if (alpha < 0) alpha = 0f;

            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// <para/> RGB값 더한 후 리턴
        /// </summary>
        public static Color Plus(this Color color, float r, float g, float b)
        {
            return new Color(color.r + r, color.g + g, color.b + b);
        }

        /// <summary>
        /// <para/> RGBA값 더한 후 리턴
        /// </summary>
        public static Color Plus(this Color color, float r, float g, float b, float a)
        {
            return new Color(color.r + r, color.g + g, color.b + b, color.a + a);
        }

        /// <summary>
        /// <para/> RGB값 더한 후 리턴
        /// </summary>
        public static Color Ex_PlusRGB(this Color color, float rgb)
        {
            return new Color(color.r + rgb, color.g + rgb, color.b + rgb, color.a);
        }

        /// <summary>
        /// <para/> RGBA값 더한 후 리턴
        /// </summary>
        public static Color Ex_PlusRGBA(this Color color, float rgba)
        {
            return new Color(color.r + rgba, color.g + rgba, color.b + rgba, color.a + rgba);
        }

        //Converts an RGB color to an HSV color.
        public static Vector3 ConvertRgbToHsv(this in Color rgbColor)
        {
            var r = rgbColor.r;
            var g = rgbColor.g;
            var b = rgbColor.b;

            var min = Mathf.Min(r, Mathf.Min(g, b));
            var max = Mathf.Max(r, Mathf.Max(g, b));
            var diff = max - min;

            float h;
            if (max > min)
            {
                if (g == max)
                    h = (b - r) / diff * 60f + 120f;
                else if (b == max)
                    h = (r - g) / diff * 60f + 240f;
                else if (b > g)
                    h = (g - b) / diff * 60f + 360f;
                else
                    h = (g - b) / diff * 60f;

                if (h < 0)
                    h += 360f;
            }
            else
                h = 0;

            Vector3 hsvColor = new()
            {
                x = h * 0.0028f,
                y = diff / max,
                z = max
            };

            return hsvColor;
        }

        //Converts an RGB color to an HSV color.
        public static Vector3 ConvertRgbToHsv(in float r, in float g, in float b)
        {
            var min = Mathf.Min(r, Mathf.Min(g, b));
            var max = Mathf.Max(r, Mathf.Max(g, b));
            var diff = max - min;

            float h;
            if (max > min)
            {
                if (g == max)
                    h = (b - r) / diff * 60f + 120f;
                else if (b == max)
                    h = (r - g) / diff * 60f + 240f;
                else if (b > g)
                    h = (g - b) / diff * 60f + 360f;
                else
                    h = (g - b) / diff * 60f;

                if (h < 0)
                    h += 360f;
            }
            else
                h = 0;

            Vector3 hsvColor = new()
            {
                x = h * 0.0028f,
                y = diff / max,
                z = max
            };

            return hsvColor;
        }

        /// <summary>
        /// Converts an HSV color to an RGB color. 
        /// </summary>
        /// <param name="h">0~360</param>
        /// <param name="s">0~1</param>
        /// <param name="v">0~1</param>
        /// <param name="alpha">0~1</param>
        public static Color ConvertHsvToRgb(double h, double s, double v, float alpha)
        {
            double r, g, b;

            if (s.Equals(0))
            {
                r = v;
                g = v;
                b = v;
            }

            else
            {
                int i;
                double f, p, q, t;


                if (h.Equals(360))
                    h = 0;
                else
                    h /= 60;

                i = (int)(h);
                f = h - i;

                p = v * (1.0 - s);
                q = v * (1.0 - (s * f));
                t = v * (1.0 - (s * (1.0f - f)));


                switch (i)
                {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;

                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;

                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;

                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;

                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;

                    default:
                        r = v;
                        g = p;
                        b = q;
                        break;
                }

            }

            return new Color((float)r, (float)g, (float)b, alpha);
        }
    }
}