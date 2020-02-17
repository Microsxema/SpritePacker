using System;
using System.Collections.Generic;
using System.Drawing;
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

            Initialization(sprites, padding);
            
            Packaging();
        }
        
        private void Packaging()
        {
            Region region = new Region();

            while (m_spritesKit.Count > 0)
            {
                region = new Region
                (
                    new Point(AtlasWidth, 0),
                    new Point(m_spritesKit[0].SpriteWidth + AtlasWidth, AtlasHeight)
                );
                
                RegionFilling(region);
                
                AtlasWidth += region.Width;

                m_spritesKit = m_spritesKit.Where(value => !m_processedSprites.Contains(value)).ToList();
            }
        }
        
        private void RegionFilling(Region region)
        {
            Point pointer = new Point(region.TopLeft.X, region.TopLeft.Y);
            
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
                    new SpriteAtlasElement(pointer, m_spritesKit[i])
                );

                if (region.Width - m_spritesKit[i].SpriteWidth != 0)
                {
                    RegionFilling
                    (
                        new Region
                        (
                            new Point(m_spritesKit[i].SpriteWidth + pointer.X, pointer.Y),
                            new Point(region.Width + pointer.X, m_spritesKit[i].SpriteHeight + pointer.Y)
                        )
                    );
                }
                
                pointer = new Point(pointer.X, m_spritesKit[i].SpriteHeight + pointer.Y);

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

        private void Initialization(List<SpriteModel> sprites, int padding)
        {
            m_padding = padding;
            
            m_processedSprites = new List<SpriteModel>();

            m_spritesKit = sprites.OrderByDescending(value => value.SpriteWidth).ToList();

            SetAtlasHeight();
            
            AtlasWidth = 0;
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
}