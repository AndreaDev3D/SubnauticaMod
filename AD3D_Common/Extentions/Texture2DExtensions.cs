using UnityEngine;

namespace AD3D_Common.Extentions
{
    public static class Texture2DExtensions
    {
        public static Sprite ToSprite(this Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public static Sprite ToSprite(this Sprite atlasSprite)
        {
            if (atlasSprite == null)
            {
                return null;
            }

            Texture2D texture = atlasSprite.texture;

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
