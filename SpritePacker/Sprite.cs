using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SpritePacker
{
    public class SpriteAtlas
    {
        public List<SpriteAtlasElement> SpriteAtlasElements;
        
        public int AtlasWidth { get; private set; }
        
        public int AtlasHeight { get; private set; }


        private List<SpriteModel> m_spritesKit;

        private List<SpriteModel> m_processedSprites;
        
        private int m_padding;

        
        public SpriteAtlas(List<SpriteModel> sprites, int padding = 0)
        {
            SpriteAtlasElements = new List<SpriteAtlasElement>();
            
            m_padding = padding;

            m_spritesKit = sprites;

            Packaging(sprites);
        }
        
        private void Packaging(List<SpriteModel> spriteKit)
        {
            Region region = new Region();

            m_processedSprites = new List<SpriteModel>();

            m_spritesKit = spriteKit.OrderByDescending(value => value.SpriteWidth).ToList();

            AtlasWidth = 0;

            SetAtlasHeight();
            
            while (m_spritesKit.Count > 0)
            {
                region = new Region
                (
                    new Point(AtlasWidth, 0),
                    new Point(m_spritesKit[0].SpriteWidth + AtlasWidth, AtlasHeight)
                );
                
                RegionFilling(region);
                
                AtlasWidth += region.Width;

                ClearProcessedSprites();
            }
        }
        
        private void RegionFilling(Region region)
        {
            Point point = new Point(region.TopLeft.X, region.TopLeft.Y);
            
            int fillingHeight = 0;
            
            for (int i = 0; i < m_spritesKit.Count; i++)
            {
                if (m_processedSprites.Contains(m_spritesKit[i]))
                {
                    continue;
                }
                
                if (fillingHeight + m_spritesKit[i].SpriteHeight > region.Height)
                {
                    continue;
                }

                if (m_spritesKit[i].SpriteWidth > region.Width)
                {
                    continue;
                }
                
                m_processedSprites.Add(m_spritesKit[i]);

                SpriteAtlasElements.Add
                (
                    new SpriteAtlasElement(new Point(point.X, point.Y), m_spritesKit[i])
                );

                if (region.Width - m_spritesKit[i].SpriteWidth != 0)
                {
                    RegionFilling
                    (
                        new Region
                        (
                            new Point(point.X + m_spritesKit[i].SpriteWidth, point.Y),
                            new Point(region.Width + point.X, point.Y + m_spritesKit[i].SpriteHeight)
                        )
                    );
                }
                
                point = new Point(point.X, point.Y + m_spritesKit[i].SpriteHeight);

                fillingHeight += m_spritesKit[i].SpriteHeight;
            }
        }
        
        private void SetAtlasHeight()
        {
            int amountRectangle = 0;
            
            foreach (var sprite in m_spritesKit)
            {
                amountRectangle += (sprite.SpriteHeight + m_padding * 2) * (sprite.SpriteWidth + m_padding * 2);
            }
            
            double value = Math.Sqrt(amountRectangle);

            AtlasHeight = (int)Math.Ceiling(value);
        }
        
        private void ClearProcessedSprites()
        {
            List<SpriteModel> spriteList = new List<SpriteModel>();

            foreach (var value in m_spritesKit)
            {
                if (!m_processedSprites.Contains(value))
                {
                    spriteList.Add(value);
                }
            }

            m_spritesKit = spriteList;
        }
    }

    public struct SpriteAtlasElement
    {
        public Point Location;

        public SpriteModel Sprite;

        public SpriteAtlasElement(Point location, SpriteModel sprite)
        {
            Location = location;

            Sprite = sprite;
        }
    }
    
    
    struct Region
    {
        public Point TopLeft;

        public Point BottomRight;

        public int Width => BottomRight.X - TopLeft.X;
    
        public int Height => BottomRight.Y - TopLeft.Y;

        public Region(Point topLeft, Point bottomRight)
        {
            TopLeft = topLeft;

            BottomRight = bottomRight;
        }
    }
    
    
    public class SpriteModel
    {
        public string SpriteName;

        public int SpriteWidth => m_size.Width;

        public int SpriteHeight => m_size.Height;


        private Size m_size;

        
        public SpriteModel(string spriteName, Size size)
        {
            SpriteName = spriteName;
            
            m_size = size;
        }
    }
    
    
    
    
    
    
    
    
    
    
    
    [JsonObject(MemberSerialization.OptIn)]
    public class SpriteInfo1
    {
        [JsonProperty(PropertyName = "Name")] 
        public string SpriteName { get; private set; }

        [JsonProperty(PropertyName = "Width")] 
        public int Width { get; private set; }

        [JsonProperty(PropertyName = "Height")]
        public int Height { get; private set; }

        [JsonProperty(PropertyName = "Top")] 
        public int Top => SpriteLocation.Top;

        [JsonProperty(PropertyName = "Bottom")]
        public int Bottom => SpriteLocation.Bottom;

        [JsonProperty(PropertyName = "Left")] 
        public int Left => SpriteLocation.Left;

        [JsonProperty(PropertyName = "Right")] 
        public int Right => SpriteLocation.Right;

        public int Square => Height * Width;

        public int Padding { get; private set; }

        public Rectangle SpriteLocation { get; private set; }

        public SpriteInfo1(string spriteName, int width, int height, int padding = 0)
        {
            SpriteName = spriteName;
            Width = width;
            Height = height;
            Padding = padding;
        }

        public void SetLocation(Point topLeft, Point bottomRight)
        {
            SpriteLocation = new Rectangle
            (
                new Point(topLeft.X + Padding, topLeft.Y + Padding),
                new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y)
            );
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class Sprite1 : IComparable<Sprite1>
    {
        [JsonProperty(PropertyName = "name")]
        public string SpriteName { get; private set; }

        public Bitmap BitSprite { get; private set; }

        public int Square => Height * Width;
        
        public int Width => BitSprite.Width;
        
        public int Height => BitSprite.Height;

        public int Padding { get; private set; }

        [JsonProperty(PropertyName = "geometry")]
        public Rectangle SpriteLocation { get; private set; }
        
        [JsonProperty(PropertyName = "slices")]
        public Rectangle SliceLocation { get; private set; }

        public Sprite1(string fileLocation, int padding = 0)
        {
            Padding = padding;
            
            SpriteName = Path.GetFileName(fileLocation);
            
            if (padding > 0)
            {
                BitSprite = Resize(fileLocation, padding);
            }

            else
            {
                BitSprite = new Bitmap(fileLocation);
            }
        }

        private void GetSlice()
        {
            string pattern = @"[(](\d+)[x](\d+)[;](\d+)[x](\d+)[)]";
            
            Regex regex = new Regex(pattern);
            
            MatchCollection matchedAuthors = regex.Matches(SpriteName);

            int topLeftX;
            int topLeftY;
            int bottomRightX;
            int bottomRightY;

            if (matchedAuthors.Count <= 0)
            {
                SliceLocation = SpriteLocation;
            }

            else
            {
                Int32.TryParse(matchedAuthors[0].Groups[1].Value, out topLeftX);
                Int32.TryParse(matchedAuthors[0].Groups[2].Value, out topLeftY);
                Int32.TryParse(matchedAuthors[0].Groups[3].Value, out bottomRightX);
                Int32.TryParse(matchedAuthors[0].Groups[4].Value, out bottomRightY);

                SliceLocation = new Rectangle
                (
                    new Point(SpriteLocation.X + topLeftX, SpriteLocation.Y + topLeftY),
                    new Size(bottomRightX - topLeftX, bottomRightY - topLeftY)
                );
            }
        }

        public void SetLocation(Point topLeft, Point bottomRight)
        {
            SpriteLocation = new Rectangle
            (
                new Point(topLeft.X + Padding, topLeft.Y + Padding), 
                new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y)
            );
            
            GetSlice();
        }
        
        private Bitmap Resize(string fileLocation, int padding)
        {
            Bitmap m_bitSprite = new Bitmap(fileLocation);

            Bitmap newSprite = new Bitmap(m_bitSprite.Width + padding * 2, m_bitSprite.Height + padding * 2);

            Graphics spriteGraphics = Graphics.FromImage(newSprite);

            spriteGraphics.DrawImage(m_bitSprite, new Point(padding, padding));

            return newSprite;
        }
        
        public int CompareTo(Sprite1 obj)
        {
            int m_width = this.Width.CompareTo(obj.Width);

            return m_width;
        }
    }
}