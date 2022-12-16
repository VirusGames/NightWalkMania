 //------------------------------------------------------------
// FlourSharp
// Homepage: https://github.com/Starpelly/FlourSharp
// MIT License (you have to mention that you use this)
//------------------------------------------------------------


using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace Bread2Unity
{
    [System.Serializable]
    public class BCCAD
    {
        public uint timestamp;
        public ushort textureWidth;
        public ushort textureHeight;
        public List<BreadSprite> sprites = new List<BreadSprite>();
        public List<BreadAnimation> animations = new List<BreadAnimation>();

        public static BCCAD FromBCCAD(string fileLocation)
        {
            BCCAD bCCAD = new BCCAD();

            byte[] data = File.ReadAllBytes(fileLocation);
            using (Stream stream = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(stream);
                try
                {
                    bCCAD.timestamp = reader.ReadUInt32();
                    bCCAD.textureWidth = reader.ReadUInt16();
                    bCCAD.textureHeight = reader.ReadUInt16();
                    var spriteCount = reader.ReadUInt32();
                    for (int i = 0; i < spriteCount; i++)
                    {
                        BreadSprite sprite = new BreadSprite();
                        var partsCount = reader.ReadUInt32();
                        for (int j = 0; j < partsCount; j++)
                        {
                            SpritePart part = new SpritePart();
                            PosInTexture posInTexture = new PosInTexture();
                            posInTexture.x = reader.ReadUInt16();
                            posInTexture.y = reader.ReadUInt16();
                            posInTexture.width = reader.ReadUInt16();
                            posInTexture.height = reader.ReadUInt16();
                            part.texturePos = posInTexture;

                            part.posX = reader.ReadInt16();
                            part.posY = reader.ReadInt16();
                            part.scaleX = reader.ReadSingle();
                            part.scaleY = reader.ReadSingle();
                            part.rotation = reader.ReadSingle();
                            part.flipHorizontal = reader.ReadBoolean();
                            part.flipVertical = reader.ReadBoolean();

                            part.multiplyColor = new Colour(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                            part.screenColor = new Colour(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

                            part.opacity = reader.ReadByte();

                            for (int k = 0; k < 12; k++)
                            {
                                reader.ReadByte(); // unknown data
                            }

                            part.designationID = reader.ReadByte();
                            reader.ReadInt16(); // unknown
                            StereoDepth stereoDepth = new StereoDepth();
                            stereoDepth.topLeft = reader.ReadSingle();
                            stereoDepth.bottomLeft = reader.ReadSingle();
                            stereoDepth.topRight = reader.ReadSingle();
                            stereoDepth.bottomRight = reader.ReadSingle();
                            part.depth = stereoDepth;

                            sprite.parts.Add(part);
                        }
                        bCCAD.sprites.Add(sprite);
                    }

                    var animCount = reader.ReadInt32();
                    for (int i = 0; i < animCount; i++)
                    {
                        List<byte> bytes = new List<byte>();
                        var size = reader.ReadByte();
                        for (int k = 0; k < size; k++)
                        {
                            bytes.Add(reader.ReadByte());
                        }
                        var paddingSize = 4 - ((size + 1) % 4);
                        for (int k = 0; k < paddingSize; k++)
                        {
                            reader.ReadByte();
                        }
                        string final = Encoding.UTF8.GetString(bytes.ToArray());

                        BreadAnimation animation = new BreadAnimation();
                        animation.name = final;
                        animation.interpolation = reader.ReadInt32();
                        var stepCount = reader.ReadUInt32();
                        for (int j = 0; j < stepCount; j++)
                        {
                            AnimationStep step = new AnimationStep();
                            step.sprite = reader.ReadUInt16();
                            step.duration = reader.ReadUInt16();
                            step.posX = reader.ReadInt16();
                            step.posY = reader.ReadInt16();
                            step.depth = reader.ReadSingle();
                            step.scaleX = reader.ReadSingle();
                            step.scaleY = reader.ReadSingle();
                            step.rotation = reader.ReadSingle();
                            step.multiplyColor = new Colour(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                            reader.ReadByte();
                            reader.ReadByte();
                            reader.ReadByte();
                            step.opacity = reader.ReadUInt16();
                            animation.steps.Add(step);
                        }
                        bCCAD.animations.Add(animation);
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            return bCCAD;
        }

        public static void JSONFromBCCAD(string fileLocation)
        {
            BCCAD bccad = BCCAD.FromBCCAD(fileLocation);
            string data = JsonUtility.ToJson(bccad, true);
            string[] list = fileLocation.Split("/");
            string name = list[list.Length-1];
            name = name.Substring(0, name.Length-6);
            System.IO.File.WriteAllText(Application.dataPath + "/"+name+".json", data);

        }

        public static void SliceTexture(TextAsset json, Texture2D source)
        {
            if(AssetDatabase.IsValidFolder($"Assets/BreadAnimation/Raw")==false)
            {
                AssetDatabase.CreateFolder("Assets", "BreadAnimation");
                AssetDatabase.CreateFolder("Assets/BreadAnimation", "Raw");
                AssetDatabase.CreateFolder("Assets/BreadAnimation", "Sprites");
            }
            BCCAD bccad = JsonUtility.FromJson<BCCAD>(json.text);
            List<PosInTexture> uniqueTextures = new List<PosInTexture>();
            for(int i=0; i<bccad.sprites.Count; i++)
            {
                for(int j=0; j<bccad.sprites[i].parts.Count; j++)
                {
                    if(!uniqueTextures.Contains(bccad.sprites[i].parts[j].texturePos))
                    {
                        uniqueTextures.Add(bccad.sprites[i].parts[j].texturePos);
                    }
                }
            }
            //Debug.Log(uniqueTextures.Count);
            string assetPath = AssetDatabase.GetAssetPath(source);
            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.mipmapEnabled = false;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.SaveAndReimport();
            PosInTextureList sat = new PosInTextureList();
            for(int n=0; n<uniqueTextures.Count; n++)
            {
                var rect = new Rect(uniqueTextures[n].x, bccad.textureHeight-uniqueTextures[n].y-uniqueTextures[n].height, uniqueTextures[n].width, uniqueTextures[n].height);
                sat.pos.Add(rect);
                var sprite = Sprite.Create(source, rect, Vector2.one * 0.5f);
                Texture2D newText = new Texture2D((int)sprite.rect.width,(int)sprite.rect.height);
                Color[] newColors = sprite.texture.GetPixels((int)sprite.rect.x,
                                             (int)sprite.rect.y,
                                             (int)sprite.rect.width,
                                             (int)sprite.rect.height );
                newText.SetPixels(newColors);
                newText.Apply();
                System.IO.File.WriteAllBytes(Application.dataPath + "/BreadAnimation/raw/"+json.name+n.ToString()+".png", newText.EncodeToPNG());
            }
            string data = JsonUtility.ToJson(sat, true);
            System.IO.File.WriteAllText(Application.dataPath + "/UniqueTextures.json", data);
        }

        public static void CompileSprites(TextAsset json) //for now, I have no idea how to do this...
        {
            GameObject tempObject = new GameObject("Bread2Unity", typeof(SpriteRenderer));
            Transform transform = tempObject.GetComponent<Transform>();
            SpriteRenderer render = tempObject.GetComponent<SpriteRenderer>();
            //SpriteRenderer spriteRenderer = 
            BCCAD bccad = JsonUtility.FromJson<BCCAD>(json.text);
            List<PosInTexture> uniqueTextures = new List<PosInTexture>();
            for(int i=0; i<bccad.sprites.Count; i++)
            {
                for(int j=0; j<bccad.sprites[i].parts.Count; j++)
                {
                    if(!uniqueTextures.Contains(bccad.sprites[i].parts[j].texturePos))
                    {
                        uniqueTextures.Add(bccad.sprites[i].parts[j].texturePos);
                    }
                }
            }
            Texture2D sprite = new Texture2D((int)bccad.sprites[0].parts[0].texturePos.width,(int)bccad.sprites[0].parts[0].texturePos.height);
            sprite.LoadImage(File.ReadAllBytes(Path.Combine(Application.dataPath + "/BreadAnimation/raw/", json.name+
            uniqueTextures.FindIndex(texpos => EqualityComparer<PosInTexture>.Default.Equals(texpos, bccad.sprites[0].parts[0].texturePos)).ToString()+".png")));
            var rect = new Rect(0, 0, uniqueTextures[0].width, uniqueTextures[0].height);
            render.sprite = Sprite.Create(sprite, rect, Vector2.one * 0.5f);
            transform.localScale = new Vector3(8, 2, 1);
            Debug.Log(render.sprite.rect.width+","+render.sprite.rect.height);
            /*for(int i=0; i<bccad.sprites.Count; i++)
            {
                Texture2D clip = new Texture2D(512,512);
                for(int x=0; x<clip.width; x++)
                {
                    for(int y=0; y<clip.height; y++)
                    {
                        clip.SetPixel(x, y, new Color(1, 1, 1, 0));
                    }
                }
                for(int j=0; j<bccad.sprites[i].parts.Count; j++)
                {
                    Texture2D sprite = new Texture2D((int)bccad.sprites[i].parts[j].texturePos.width,(int)bccad.sprites[i].parts[j].texturePos.height);
                    sprite.LoadImage(File.ReadAllBytes(Path.Combine("/BreadAnimation/raw/", json.name+
                    uniqueTextures.FindIndex(texpos => EqualityComparer<PosInTexture>.Default.Equals(texpos, bccad.sprites[i].parts[j].texturePos)).ToString()+".png")));
                    Color[] newColors = sprite.GetPixels();
                    for(int x=0; x<clip.width; x++)
                    {
                        for(int y=0; y<clip.height; y++)
                        {
                            var color = sprite.GetPixel(x,y).a==0 ?
                                clip.GetPixel(x, y):
                                sprite.GetPixel(x,y);
                            clip.SetPixel(bccad.sprites[i].parts[j].posX-240, bccad.sprites[i].parts[j].posY-240, color);
                        }
                    }
                    //Debug.Log(Path.Combine("/BreadAnimation/raw/", json.name+uniqueTextures.FindIndex(texpos => EqualityComparer<PosInTexture>.Default.Equals(texpos, bccad.sprites[i].parts[j].texturePos)).ToString()+".png"));
                }
            }*/
        }

        public static int DirCount(DirectoryInfo d)
        {
            int i = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                if (fi.Extension.Contains("png"))
                    i++;
            }
            return i;
        }


        public static byte[] ToBCCAD()
        {
            return new byte[0];
        }
    }

    [System.Serializable]
    public class BreadSprite
    {
        public List<SpritePart> parts = new List<SpritePart>();
    }

    [System.Serializable]
    public class PosInTextureList
    {
        public List<Rect> pos = new List<Rect>();
    }

    [System.Serializable]
    public struct SpritePart
    {
        public PosInTexture texturePos;
        public short posX;
        public short posY;
        public float scaleX;
        public float scaleY;
        public float rotation;
        public bool flipHorizontal;
        public bool flipVertical;
        public Colour multiplyColor;
        public Colour screenColor;
        public byte opacity;
        public byte designationID;
        public StereoDepth depth;
    }

    [System.Serializable]
    public struct PosInTexture
    {
        public uint x;
        public uint y;
        public uint width;
        public uint height;
    }

    [System.Serializable]
    public struct StereoDepth
    {
        public float topLeft;
        public float topRight;
        public float bottomLeft;
        public float bottomRight;
    }

    [System.Serializable]
    public class BreadAnimation
    {
        public string? name;
        public int interpolation;
        public List<AnimationStep> steps = new List<AnimationStep>();
    }

    [System.Serializable]
    public struct AnimationStep
    {
        public ushort sprite;
        public ushort duration;
        public short posX;
        public short posY;
        public float depth;
        public float scaleX;
        public float scaleY;
        public float rotation;
        public Colour multiplyColor;
        public byte unknown1;
        public byte unknown2;
        public ushort opacity;
    }

    [System.Serializable]
    public struct Colour
    {
        public byte red;
        public byte green;
        public byte blue;

        public Colour(byte red, byte green, byte blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }
    }
}
