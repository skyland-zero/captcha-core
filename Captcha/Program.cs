using LDFCore.Common.Captcha.Dtos;
using LDFCore.Common.Captcha.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Captcha
{
    class Program
    {
        static Random rand = new Random();
        static ShapeGraphicsOptions antialiasShapeGraphicsOptions => new ShapeGraphicsOptions()
        {
            GraphicsOptions = new GraphicsOptions
            {
                Antialias = true,
                AntialiasSubpixelDepth = 200
            }
        };
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var imgPath = System.IO.Path.Combine(AppContext.BaseDirectory, "bg4.png");
            var tempFolder = System.IO.Path.Combine(AppContext.BaseDirectory, "temp");

            if (File.Exists(imgPath))
            {
                Console.WriteLine("已找到背景图片");
            }
            

            for (int i = 0; i < 100; i++)
            {

                var path = GetBgImagePath();

                var img = GenerateCaptcha(path);

                var encoder = new PngEncoder()
                {
                    ColorType = PngColorType.RgbWithAlpha,
                    TransparentColorMode = PngTransparentColorMode.Clear
                };

                img.Item1.Save(System.IO.Path.Combine(AppContext.BaseDirectory, "temp", "main_" + i + ".png"), encoder);
                if (img.Item1 != null)
                {
                    img.Item1.Dispose();
                }
                img.Item2.Save(System.IO.Path.Combine(AppContext.BaseDirectory, "temp", "main_" + i + "_sw.png"), encoder);
                if (img.Item2 != null)
                {
                    img.Item2.Dispose();
                }
            }

            Console.Write("处理完成");


        }

        /// <summary>
        /// 获取随机背景图片
        /// </summary>
        /// <returns></returns>
        private static string GetBgImagePath()
        {
            var folder = System.IO.Path.Combine(AppContext.BaseDirectory, "bgs");
            if (!Directory.Exists(folder))
            {
                return null;
            }
            var files = Directory.GetFiles(folder);
            if (files.Length == 0)
            {
                return null;
            }
            var index = rand.Next(0, files.Length);
            var name = files[index];
            return System.IO.Path.Combine(folder, name);
        }

        /// <summary>
        /// 返回处理后验证码图片
        /// </summary>
        /// <param name="bgFilePath"></param>
        /// <returns></returns>
        private static (Image, Image, int) GenerateCaptcha(string bgFilePath)
        {
            Image image = Image.Load(bgFilePath);
            var allPath = GetCaptchaPath(image);
            var path = allPath.Item1;
            var pathMix = allPath.Item2;

            //剪切拼图块
            var imagePathBuilder = new PathBuilder();
            imagePathBuilder.AddLine(0, 0, 0, image.Height);
            imagePathBuilder.AddLine(0, image.Height, image.Width, image.Height);
            imagePathBuilder.AddLine(image.Width, image.Height, image.Width, 0);
            imagePathBuilder.CloseAllFigures();
            var imagePath = imagePathBuilder.Build();

            var sm = imagePath.Clip(path);
           
            var swipeImg = image.Clone(x => x.Fill(new ShapeGraphicsOptions()
            {
                GraphicsOptions = new GraphicsOptions
                {
                    Antialias = true,
                    AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
                }
            }, Color.Red, sm));
            swipeImg.Mutate(x => x.Draw(antialiasShapeGraphicsOptions, Color.Black.WithAlpha(0.5F), 1.5F, path));
            swipeImg.Mutate(x => x.Crop(new Rectangle(new Point((int)Math.Ceiling(path.Bounds.Left), 0), new Size((int)Math.Ceiling(path.Bounds.Width), image.Height))));


            //处理背景图片
            image.Mutate(x => x.Fill(antialiasShapeGraphicsOptions, Color.Black.WithAlpha(0.5F), path));
            image.Mutate(x => x.Draw(antialiasShapeGraphicsOptions, Color.White.WithAlpha(0.5F), 1.5F, path));

            //处理背景混淆图片
            image.Mutate(x => x.Fill(antialiasShapeGraphicsOptions, Color.Black.WithAlpha(0.5F), pathMix));
            image.Mutate(x => x.Draw(antialiasShapeGraphicsOptions, Color.White.WithAlpha(0.5F), 1.5F, pathMix));

            return (image, swipeImg, (int)Math.Round(path.Bounds.X));
        }

        /// <summary>
        /// 生成验证码拼图Path
        /// </summary>
        /// <returns></returns>
        private static (IPath, IPath) GetCaptchaPath(Image image)
        {
            //拼图形状：2到4个耳朵，可全部为凹也可能全部为凸
            //1.随机生成耳朵数量
            //2.根据耳朵的数量，随机生成耳朵的朝向
            //3.随机生成耳朵所在的边
            //4.计算每一边耳朵的起始坐标

            ////随机生成耳朵信息
            var earNum = rand.Next(3, 5);
            var ears = new List<CaptchaEarInfo>();
            for (int i = 0; i < earNum; i++)
            {
                var ear = new CaptchaEarInfo();
                //如果是有四个耳朵，则按顺序归到4条边
                if (earNum == 4)
                {
                    ear.LineSide = Enum.Parse<CaptchaBezierLineSide>((i + 1).ToString());
                }
                else
                {
                    ear.LineSide = Enum.Parse<CaptchaBezierLineSide>(rand.Next(1, 5).ToString());
                    //如果对应边已有耳朵，则重新随机生成
                    while (ears.Where(a => a.LineSide == ear.LineSide).Any())
                    {
                        ear.LineSide = Enum.Parse<CaptchaBezierLineSide>(rand.Next(1, 5).ToString());
                    }
                }
                ear.Towards = Enum.Parse<CaptchaBezierTowards>(rand.Next(1, 3).ToString());
                ears.Add(ear);
            }

            ////计算顶点位置
            var width = image.Width;
            var height = image.Height;
            //拼图主体边长
            var length = height / 4F;
            var earlength = length / 3F;
            //计算顶点可随机范围
            var minX = width / 6F;
            //计算X最大值（如果拼图右边有耳朵且为朝外，则减去这部分X值）
            var maxX = width - length - (ears.Where(a => a.LineSide == CaptchaBezierLineSide.Right && a.Towards == CaptchaBezierTowards.Outside).Any() ? earlength : 0);
            //计算最小Y值，如果拼图上方有耳朵且为朝外，则加上这部分Y值（其中1为预留位置，为了好看些~）
            var minY = 1 + (ears.Where(a => a.LineSide == CaptchaBezierLineSide.Top && a.Towards == CaptchaBezierTowards.Outside).Any() ? earlength : 0);
            //计算最大Y值，如果拼图上方有耳朵且为朝外，则减去这部分Y值
            var maxY = height - length - (ears.Where(a => a.LineSide == CaptchaBezierLineSide.Bottom && a.Towards == CaptchaBezierTowards.Outside).Any() ? earlength : 0);
            //随机生成顶点坐标
            var point1X = rand.Next((int)minX, (int)maxX);
            var point1Y = rand.Next((int)minY, (int)maxY);
            //计算拼图主体四个顶点坐标
            var point1 = new PointF(point1X, point1Y);
            var point2 = new PointF(point1.X, point1.Y + length);
            var point3 = new PointF(point1.X + length, point1.Y + length);
            var point4 = new PointF(point1.X + length, point1.Y);
            //绘制Path
            var builder = new PathBuilder();
            //左边曲线
            if (ears.Any(x => x.LineSide == CaptchaBezierLineSide.Left))
            {
                var towards = ears.Where(a => a.LineSide == CaptchaBezierLineSide.Left).Select(x => x.Towards).First();
                var earPoint = new PointF(point1.X, point1.Y + earlength);
                var earPointEnd = AddBezierPath(builder, CaptchaBezierLineSide.Left, earPoint, earlength, towards);
                builder.AddLine(earPointEnd, point2);
            }
            else
            {
                builder.AddLine(point1, point2);
            }
            //下边曲线
            if (ears.Any(x => x.LineSide == CaptchaBezierLineSide.Bottom))
            {
                var towards = ears.Where(a => a.LineSide == CaptchaBezierLineSide.Bottom).Select(x => x.Towards).First();
                var earPoint = new PointF(point2.X + earlength, point2.Y);
                var earPointEnd = AddBezierPath(builder, CaptchaBezierLineSide.Bottom, earPoint, earlength, towards);
                builder.AddLine(earPointEnd, point3);
            }
            else
            {
                builder.AddLine(point2, point3);
            }
            //右边曲线
            if (ears.Any(x => x.LineSide == CaptchaBezierLineSide.Right))
            {
                var towards = ears.Where(a => a.LineSide == CaptchaBezierLineSide.Right).Select(x => x.Towards).First();
                var earPoint = new PointF(point3.X, point3.Y - earlength);
                var earPointEnd = AddBezierPath(builder, CaptchaBezierLineSide.Right, earPoint, earlength, towards);
                builder.AddLine(earPointEnd, point4);
            }
            else
            {
                builder.AddLine(point3, point4);
            }
            //上边曲线
            if (ears.Any(x => x.LineSide == CaptchaBezierLineSide.Top))
            {
                var towards = ears.Where(a => a.LineSide == CaptchaBezierLineSide.Top).Select(x => x.Towards).First();
                var earPoint = new PointF(point4.X - earlength, point4.Y);
                var earPointEnd = AddBezierPath(builder, CaptchaBezierLineSide.Top, earPoint, earlength, towards);
                builder.AddLine(earPointEnd, point1);
            }
            else
            {
                builder.AddLine(point4, point1);
            }
            builder.CloseAllFigures();
            var path = builder.Build();

            //生成混淆图片路径
            var mixPoint = new PointF(0, 0);
            //随机生成根据X限制还是根据Y限制
            //根据X限制图片位置
            var mixLimitOffset = path.Bounds.Width / 2;

            //判断图片左边和右边剩余位置是否够位置放置混淆图片
            var leftLength = path.Bounds.Left - path.Bounds.Width - path.Bounds.Width;
            var rightLength = image.Width - path.Bounds.Right - path.Bounds.Width - mixLimitOffset;
            //如果左右都够位置，则随机一边进行混淆
            var mixMinX = 0;
            var mixMaxX = 0;
            var lrType = 1;  //混淆图片放左边还是右边
            if (leftLength > path.Bounds.Width && rightLength > path.Bounds.Width)
            {
                lrType = rand.Next(1, 3);
            }
            else if (leftLength > path.Bounds.Width)
            {
                lrType = 1;
            }
            else
            {
                lrType = 2;
            }

            if (lrType == 1)
            {
                mixMinX = (int)mixLimitOffset;
                mixMaxX = (int)(leftLength - mixLimitOffset);
            }
            else
            {
                mixMinX = (int)(image.Width - rightLength - mixLimitOffset);
                mixMaxX = (int)(image.Width - mixLimitOffset - path.Bounds.Width);
                if (mixMinX > mixMaxX)
                {
                    mixMaxX = mixMinX;
                }
            }
            mixPoint.X = rand.Next(mixMinX, mixMaxX + 1);

            //混淆图片是在上方还是下方
            var btType = 1;
            var mixMinY = 0;
            var mixMaxY = 0;
            var mixLimitOffsetY = path.Bounds.Height / 2;
            var topLength = path.Bounds.Y - mixLimitOffsetY;
            var bottomLength = image.Height - path.Bounds.Y - mixLimitOffsetY - path.Bounds.Height;
            if (topLength > path.Bounds.Height && bottomLength > path.Bounds.Height)
            {
                btType = rand.Next(1, 3);
            }
            else if (topLength > path.Bounds.Height)
            {
                btType = 1;
            }
            else
            {
                btType = 2;
            }

            if (btType == 1)
            {
                mixMinY = 0;
                mixMaxY = (int)topLength;
            }
            else
            {
                mixMinY = (int)(path.Bounds.Y + mixLimitOffsetY);
                mixMaxY = (int)(image.Height - path.Bounds.Height);
                if (mixMinY > mixMaxY)
                {
                    mixMaxY = mixMinY;
                }
            }
            mixPoint.Y = rand.Next(mixMinY, mixMaxY + 1);
            var mixPath = path.Translate(mixPoint.X - path.Bounds.X, mixPoint.Y - path.Bounds.Y);
            //随机判断混淆路径是否旋转
            if (rand.Next(1, 3) == 1)
            {
                mixPath = mixPath.RotateDegree(rand.Next(0, 361));
            }
            //随机判断是否需要进行缩放
            if (rand.Next(1, 6) == 1)
            {
                mixPath = mixPath.Scale(rand.Next(8, 11) / 10F);
            }
            return (path, mixPath);
        }

        /// <summary>
        /// 添加贝塞尔曲线（拼图耳朵）
        /// </summary>
        /// <param name="builder">PathBuilder</param>
        /// <param name="lineSide">所在边</param>
        /// <param name="startPoint">起点</param>
        /// <param name="length">长度</param>
        /// <param name="towards">朝向（朝内或朝外）</param>
        private static PointF AddBezierPath(PathBuilder builder, CaptchaBezierLineSide lineSide, PointF startPoint, float length, CaptchaBezierTowards towards)
        {
            var offsetLength = length / 3;
            PointF endPoint = new PointF(0, 0);
            PointF controlPoint1 = new PointF(0, 0);
            PointF controlPoint2 = new PointF(0, 0);
            switch (lineSide)
            {
                case CaptchaBezierLineSide.Left:
                    endPoint = new PointF(startPoint.X, startPoint.Y + length);
                    controlPoint1 = new PointF(towards == CaptchaBezierTowards.Inner ? startPoint.X + length : startPoint.X - length, startPoint.Y - offsetLength);
                    controlPoint2 = new PointF(towards == CaptchaBezierTowards.Inner ? endPoint.X + length : endPoint.X - length, endPoint.Y + offsetLength);
                    break;
                case CaptchaBezierLineSide.Bottom:
                    endPoint = new PointF(startPoint.X + length, startPoint.Y);
                    controlPoint1 = new PointF(startPoint.X - offsetLength, towards == CaptchaBezierTowards.Inner ? startPoint.Y - length : startPoint.Y + length);
                    controlPoint2 = new PointF(endPoint.X + offsetLength, towards == CaptchaBezierTowards.Inner ? startPoint.Y - length : startPoint.Y + length);
                    break;
                case CaptchaBezierLineSide.Right:
                    endPoint = new PointF(startPoint.X, startPoint.Y - length);
                    controlPoint1 = new PointF(towards == CaptchaBezierTowards.Inner ? startPoint.X - length : startPoint.X + length, startPoint.Y + offsetLength);
                    controlPoint2 = new PointF(towards == CaptchaBezierTowards.Inner ? endPoint.X - length : endPoint.X + length, endPoint.Y - offsetLength);
                    break;
                case CaptchaBezierLineSide.Top:
                    endPoint = new PointF(startPoint.X - length, startPoint.Y);
                    controlPoint1 = new PointF(startPoint.X + offsetLength, towards == CaptchaBezierTowards.Inner ? startPoint.Y + length : startPoint.Y - length);
                    controlPoint2 = new PointF(endPoint.X - offsetLength, towards == CaptchaBezierTowards.Inner ? endPoint.Y + length : endPoint.Y - length);
                    break;
            }
            builder.AddBezier(startPoint, controlPoint1, controlPoint2, endPoint);
            return endPoint;
        }



    }

    public class CaptchaEar
    {
        public BezierLineSide LineSide { get; set; }

        public BezierTowards Towards { get; set; }
    }
}