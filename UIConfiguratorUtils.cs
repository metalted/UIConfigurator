using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UIConfigurator
{    
    public static class UIConfiguratorUtils
    {
        private static string mover64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADfSURBVEhL3ZVBEoMgDEVtj9JTwVU4LByFxoZRCRR+EBbtW8kI7w8k6CPGuK1kDwghpNFsnHPP9Ijx+pAGGIqAQ63KQAOEFM+AAqo6MKMf0BAhGbouYqP3nodd1F00wF8GgO1RUl0oA4btTLk8C7hpZ4TkDJhiZzIV3QO8r1WQ1hizvIvOm3zd17c98Zz2W4bnZDd54kFdVdkRTckQElmDmxnl8kqRhzOqC5d30e8H9P9o7U9Iu2DQH62hQNoBOqKqCLETaA2EDrQTiiIfUtxO6LqI1Co7sXeRtTaNprNtb4jdcZdwHID6AAAAAElFTkSuQmCC";
        private static string scaler64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADASURBVEhL3ZZBEsIgEATRp/ArvsJj4Sk4FpRGxAV2Nh7ShxRcumG55FZKcWfyDOSc286aGOO9LU+DvYH3vq0+SSnh+48bsIF6UgGDG8gNg8CvZ6iwAdkOqEBnH85KHxjavxvKgHB2rI9bTWBlMi+2A1t2sBfYtYONgMIOVgM6O1gKqO1gHmDsYBIg7UAK8HbwDkB3NJrYQX+D6rWyg8GIDO1g8sikHUgB3g6kQDcrHZMR8Y1JgOca/6YhhLYzx7kHwO9hWRWBT8gAAAAASUVORK5CYII=";
        private static string reset64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADZSURBVEhL7ZbhDYQgDEbxRmErVmFYGAWrNoZAKf2I5nLJvV8UaZ8FTdhKKe5NDkHOmaOniTF+ePgasMB7zyMbmACtTgCCheqEVbBWnTAJmuoUXnCsAh9yjUVjEqSUeHRCYT2jO6wdNA6i1igOYIt6BzF1YGew4IAPWXf0CILph6HQ57aC5eqjJuAtQvkLpnxJsPAtjVJagfLLWOjTh1sENaEsFgT3Wxgd9zKxe7kDu0OvTgy3qHaImnp+VJ2Y3OzE0g1K9fnNjpKVfP3pxe/fTY8OQggcPY5zO9a7a/F3f5AmAAAAAElFTkSuQmCC";

        public static string[] colors = new string[] { "Black", "Blue", "Cyan", "Gray", "Green", "Magenta", "Red", "White", "Yellow" };

        private static Sprite moverSprite;
        private static Sprite scalerSprite;
        private static Sprite resetSprite;

        public static Sprite GetSprite(string name)
        {
            switch (name)
            {
                default:
                    return null;
                case "MoveHandle":
                    if(moverSprite == null)
                    {
                        moverSprite = Base64ToSprite(mover64);
                    }
                    return moverSprite;
                case "ScaleHandle":
                    if(scalerSprite == null)
                    {
                        scalerSprite = Base64ToSprite(scaler64);
                    }
                    return scalerSprite;
                case "ResetHandle":
                    if(resetSprite == null)
                    {
                        resetSprite = Base64ToSprite(reset64);
                    }
                    return resetSprite;
            }
        }

        private static Sprite Base64ToSprite(string base64)
        {
            // Convert the base64 string to a byte array
            byte[] imageBytes = Convert.FromBase64String(base64);

            // Create a new texture and load the image bytes
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(imageBytes);

            // Create a new sprite using the texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            return sprite;
        }

        public static Color GetColor(string colorName)
        {
            switch (colorName)
            {
                case "Black":
                    return Color.black;
                case "Blue":
                    return Color.blue;
                case "Cyan":
                    return Color.cyan;
                case "Gray":
                    return Color.gray;
                case "Green":
                    return Color.green;
                case "Magenta":
                    return Color.magenta;
                case "Red":
                    return Color.red;
                default:
                case "White":
                    return Color.white;
                case "Yellow":
                    return Color.yellow;
            }
        }
    }
}
