using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;
using Terraria.Localization;
using Terraria.UI;

namespace MagicStorage.Common.UI
{
    public class UIButtonChoice : UIElement
    {
        private const int size = 32;
        private const int padding = 8;
        private int hoverButton = -1;

        private static readonly Asset<Texture2D> backTexture = MagicStorage.Instance.Assets.Request<Texture2D>("Assets/SortButtonBackground");
        private static readonly Asset<Texture2D> backTextureActive = MagicStorage.Instance.Assets.Request<Texture2D>("Assets/SortButtonBackgroundActive");

        private Asset<Texture2D>[] textures;
        private LocalizedText[] labels;

        public int choice = 0;

        public UIButtonChoice(Asset<Texture2D>[] textures, LocalizedText[] labels)
        {
            this.textures = textures;
            this.labels = labels;

            int width = size * textures.Length + padding * (textures.Length - 1);
            Width.Set(width, 0f);
            MinWidth.Set(width, 0f);
            Height.Set(size, 0f);
            MinHeight.Set(size, 0f);
            OnClick += SelectButton;
        }

        public override void Update(GameTime gameTime)
        {
            hoverButton = MouseButton();
        }

        private void SelectButton(UIEvent e, UIElement _)
        {
            int choice = MouseButton();
            if (choice != -1)
            {
                this.choice = choice;
            }
        }

        public int MouseButton()
        {
            if (!IsMouseHovering) return -1;

            CalculatedStyle dim = GetDimensions();
            float x = Main.mouseX - dim.X;
            float y = Main.mouseY - dim.Y;

            int column = (int)(x / (size + padding));

            // Padding Check
            if ((column + 1) * (size + padding) - padding < x)
            {
                return -1;
            }

            return column;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dim = GetDimensions();
            for (int k = 0; k < textures.Length; k++)
            {
                Asset<Texture2D> texture = k == choice ? backTextureActive : backTexture;
                Vector2 drawPos = new Vector2(dim.X + k * (size + padding), dim.Y);
                Color color = hoverButton != -1 && hoverButton == k ? Color.Silver : Color.White;

                Main.spriteBatch.Draw(texture.Value, drawPos, color);
                Main.spriteBatch.Draw(textures[k].Value, drawPos + new Vector2(1f), Color.White);
            }
        }

        public void DrawText(SpriteBatch spriteBatch)
        {
            if (hoverButton != -1 && hoverButton < labels.Length)
            {
                Main.instance.MouseText(labels[hoverButton].Value);
            }
        }
    }
}
