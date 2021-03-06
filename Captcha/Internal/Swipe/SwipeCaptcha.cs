using Captcha.Configuration;
using Captcha.Interface;
using Captcha.Internal.Model.SwipeCaptcha;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Captcha.Internal.Swipe
{
    public class SwipeCaptcha : ISwipeCaptcha
    {
        /// <summary>
        /// 注入配置项
        /// </summary>
        public SwipeCaptcha(IOptionsMonitor<CaptchaOptions> options)
        {
            _options = options.CurrentValue;
        }
        private readonly CaptchaOptions _options;

        private static Random _rand = new Random();
        private static DrawingOptions _antialiasDrawOptions => new DrawingOptions()
        {
            GraphicsOptions = new GraphicsOptions
            {
                Antialias = true,
                AntialiasSubpixelDepth = 200
            }
        };
        private static DrawingOptions _destOutDrawOptions => new DrawingOptions()
        {
            GraphicsOptions = new GraphicsOptions
            {
                Antialias = true,
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
            }
        };

        /// <summary>
        /// 返回处理后验证码图片
        /// </summary>
        /// <param name="bgFilePath"></param>
        /// <returns>item1：背景图片，item2：拼图图片, item3: 拼图顶点X坐标（从float四舍五入到int）</returns>
        public async Task<(Image, Image, int)> GenerateCaptchaAsync(string bgFilePath)
        {
            Image image = await Image.LoadAsync(bgFilePath);
            var path = GetCaptchaPath(image);

            //剪切拼图块
            var imagePathBuilder = new PathBuilder();
            imagePathBuilder.AddLine(0, 0, 0, image.Height);
            imagePathBuilder.AddLine(0, image.Height, image.Width, image.Height);
            imagePathBuilder.AddLine(image.Width, image.Height, image.Width, 0);
            imagePathBuilder.CloseAllFigures();
            var imagePath = imagePathBuilder.Build();

            var sm = imagePath.Clip(path);
            var swipeImg = image.Clone(x => x.Fill(_destOutDrawOptions, Color.Red, sm));
            swipeImg.Mutate(x => x.Draw(_antialiasDrawOptions, Color.ParseHex(_options.Swipe.SpPuzzlePathColor).WithAlpha(_options.Swipe.SpPuzzlePathAlpha), _options.Swipe.SpPuzzlePathWidth, path));
            swipeImg.Mutate(x => x.Crop(new Rectangle(new Point((int)Math.Ceiling(path.Bounds.Left), 0), new Size((int)Math.Ceiling(path.Bounds.Width), image.Height))));


            //处理背景图片
            image.Mutate(x => x.Fill(_antialiasDrawOptions, Color.ParseHex(_options.Swipe.BgPuzzleColor).WithAlpha(_options.Swipe.BgPuzzleAlpha), path));
            image.Mutate(x => x.Draw(_antialiasDrawOptions, Color.ParseHex(_options.Swipe.BgPuzzlePathColor).WithAlpha(_options.Swipe.BgPuzzlePathAlpha), _options.Swipe.BgPuzzlePathWidth, path));

            //处理背景混淆图片
            if (_options.Swipe.GenerateMix)
            {
                var mixPath = GetMixPath(image, path);
                image.Mutate(x => x.Fill(_antialiasDrawOptions, Color.ParseHex(_options.Swipe.BgPuzzleColor).WithAlpha(_options.Swipe.BgPuzzleAlpha), mixPath));
                image.Mutate(x => x.Draw(_antialiasDrawOptions, Color.ParseHex(_options.Swipe.BgPuzzlePathColor).WithAlpha(_options.Swipe.BgPuzzlePathAlpha), _options.Swipe.BgPuzzlePathWidth, mixPath));
            }
            return (image, swipeImg, (int)Math.Round(path.Bounds.X));
        }

        /// <summary>
        /// 生成验证码拼图Path
        /// </summary>
        /// <param name="image">背景图片</param>
        /// <returns></returns>
        private IPath GetCaptchaPath(Image image)
        {
            //拼图形状：2到4个耳朵，可全部为凹也可能全部为凸
            //1.随机生成耳朵数量
            //2.根据耳朵的数量，随机生成耳朵的朝向
            //3.随机生成耳朵所在的边
            //4.计算每一边耳朵的起始坐标

            ////随机生成耳朵信息
            var earNum = _rand.Next(3, 5);
            var ears = new List<BezierLineInfo>();
            for (int i = 0; i < earNum; i++)
            {
                var ear = new BezierLineInfo();
                //如果是有四个耳朵，则按顺序归到4条边
                if (earNum == 4)
                {
                    ear.LineSide = Enum.Parse<BezierLineSide>((i + 1).ToString());
                }
                else
                {
                    ear.LineSide = Enum.Parse<BezierLineSide>(_rand.Next(1, 5).ToString());
                    //如果对应边已有耳朵，则重新随机生成
                    while (ears.Where(a => a.LineSide == ear.LineSide).Any())
                    {
                        ear.LineSide = Enum.Parse<BezierLineSide>(_rand.Next(1, 5).ToString());
                    }
                }
                ear.Towards = Enum.Parse<BezierLineTowards>(_rand.Next(1, 3).ToString());
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
            var maxX = width - length - (ears.Where(a => a.LineSide == BezierLineSide.Right && a.Towards == BezierLineTowards.Outside).Any() ? earlength : 0);
            //计算最小Y值，如果拼图上方有耳朵且为朝外，则加上这部分Y值（其中1为预留位置，为了好看些~）
            var minY = 1 + (ears.Where(a => a.LineSide == BezierLineSide.Top && a.Towards == BezierLineTowards.Outside).Any() ? earlength : 0);
            //计算最大Y值，如果拼图上方有耳朵且为朝外，则减去这部分Y值
            var maxY = height - length - (ears.Where(a => a.LineSide == BezierLineSide.Bottom && a.Towards == BezierLineTowards.Outside).Any() ? earlength : 0);
            //随机生成顶点坐标
            var point1X = _rand.Next((int)minX, (int)maxX);
            var point1Y = _rand.Next((int)minY, (int)maxY);
            //计算拼图主体四个顶点坐标
            var point1 = new PointF(point1X, point1Y);
            var point2 = new PointF(point1.X, point1.Y + length);
            var point3 = new PointF(point1.X + length, point1.Y + length);
            var point4 = new PointF(point1.X + length, point1.Y);
            //绘制Path
            var builder = new PathBuilder();
            //左边曲线
            if (ears.Any(x => x.LineSide == BezierLineSide.Left))
            {
                var towards = ears.Where(a => a.LineSide == BezierLineSide.Left).Select(x => x.Towards).First();
                var earPoint = new PointF(point1.X, point1.Y + earlength);
                var earPointEnd = AddBezierPath(builder, BezierLineSide.Left, earPoint, earlength, towards);
                builder.AddLine(earPointEnd, point2);
            }
            else
            {
                builder.AddLine(point1, point2);
            }
            //下边曲线
            if (ears.Any(x => x.LineSide == BezierLineSide.Bottom))
            {
                var towards = ears.Where(a => a.LineSide == BezierLineSide.Bottom).Select(x => x.Towards).First();
                var earPoint = new PointF(point2.X + earlength, point2.Y);
                var earPointEnd = AddBezierPath(builder, BezierLineSide.Bottom, earPoint, earlength, towards);
                builder.AddLine(earPointEnd, point3);
            }
            else
            {
                builder.AddLine(point2, point3);
            }
            //右边曲线
            if (ears.Any(x => x.LineSide == BezierLineSide.Right))
            {
                var towards = ears.Where(a => a.LineSide == BezierLineSide.Right).Select(x => x.Towards).First();
                var earPoint = new PointF(point3.X, point3.Y - earlength);
                var earPointEnd = AddBezierPath(builder, BezierLineSide.Right, earPoint, earlength, towards);
                builder.AddLine(earPointEnd, point4);
            }
            else
            {
                builder.AddLine(point3, point4);
            }
            //上边曲线
            if (ears.Any(x => x.LineSide == BezierLineSide.Top))
            {
                var towards = ears.Where(a => a.LineSide == BezierLineSide.Top).Select(x => x.Towards).First();
                var earPoint = new PointF(point4.X - earlength, point4.Y);
                var earPointEnd = AddBezierPath(builder, BezierLineSide.Top, earPoint, earlength, towards);
                builder.AddLine(earPointEnd, point1);
            }
            else
            {
                builder.AddLine(point4, point1);
            }
            builder.CloseAllFigures();
            var path = builder.Build();

            //随机判断混淆路径是否旋转
            if (_options.Swipe.PuzzleRandomRota)
            {
                if (_rand.Next(1, 3) == 1)
                {
                    path = path.RotateDegree(_rand.Next(0, 361));
                }
            }
            //随机判断是否需要进行缩放
            if (_options.Swipe.PuzzleRandomScaled)
            {
                if (_rand.Next(1, 6) == 1)
                {
                    path = path.Scale(_rand.Next(8, 11) / 10F);
                }
            }

            return path;
        }

        /// <summary>
        /// 获取混淆Path
        /// </summary>
        /// <param name="image">背景图片</param>
        /// <param name="path">拼图路径</param>
        /// <returns></returns>
        private IPath GetMixPath(Image image, IPath path)
        {
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
                lrType = _rand.Next(1, 3);
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
            mixPoint.X = _rand.Next(mixMinX, mixMaxX + 1);

            //混淆图片是在上方还是下方
            var btType = 1;
            var mixMinY = 0;
            var mixMaxY = 0;
            var mixLimitOffsetY = path.Bounds.Height / 2;
            var topLength = path.Bounds.Y - mixLimitOffsetY;
            var bottomLength = image.Height - path.Bounds.Y - mixLimitOffsetY - path.Bounds.Height;
            if (topLength > path.Bounds.Height && bottomLength > path.Bounds.Height)
            {
                btType = _rand.Next(1, 3);
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
            mixPoint.Y = _rand.Next(mixMinY, mixMaxY + 1);
            var mixPath = path.Translate(mixPoint.X - path.Bounds.X, mixPoint.Y - path.Bounds.Y);
            //随机判断混淆路径是否旋转
            if (_options.Swipe.MixRandomRota)
            {
                if (_rand.Next(1, 3) == 1)
                {
                    mixPath = mixPath.RotateDegree(_rand.Next(0, 361));
                }
            }
            //随机判断是否需要进行缩放
            if (_options.Swipe.MixRandomScaled)
            {
                if (_rand.Next(1, 6) == 1)
                {
                    mixPath = mixPath.Scale(_rand.Next(8, 11) / 10F);
                }
            }

            return mixPath;
        }

        /// <summary>
        /// 添加贝塞尔曲线（拼图耳朵）
        /// </summary>
        /// <param name="builder">PathBuilder</param>
        /// <param name="lineSide">所在边</param>
        /// <param name="startPoint">起点</param>
        /// <param name="length">长度</param>
        /// <param name="towards">朝向（朝内或朝外）</param>
        private PointF AddBezierPath(PathBuilder builder, BezierLineSide lineSide, PointF startPoint, float length, BezierLineTowards towards)
        {
            var offsetLength = length / 3;
            PointF endPoint = new PointF(0, 0);
            PointF controlPoint1 = new PointF(0, 0);
            PointF controlPoint2 = new PointF(0, 0);
            switch (lineSide)
            {
                case BezierLineSide.Left:
                    endPoint = new PointF(startPoint.X, startPoint.Y + length);
                    controlPoint1 = new PointF(towards == BezierLineTowards.Inner ? startPoint.X + length : startPoint.X - length, startPoint.Y - offsetLength);
                    controlPoint2 = new PointF(towards == BezierLineTowards.Inner ? endPoint.X + length : endPoint.X - length, endPoint.Y + offsetLength);
                    break;
                case BezierLineSide.Bottom:
                    endPoint = new PointF(startPoint.X + length, startPoint.Y);
                    controlPoint1 = new PointF(startPoint.X - offsetLength, towards == BezierLineTowards.Inner ? startPoint.Y - length : startPoint.Y + length);
                    controlPoint2 = new PointF(endPoint.X + offsetLength, towards == BezierLineTowards.Inner ? startPoint.Y - length : startPoint.Y + length);
                    break;
                case BezierLineSide.Right:
                    endPoint = new PointF(startPoint.X, startPoint.Y - length);
                    controlPoint1 = new PointF(towards == BezierLineTowards.Inner ? startPoint.X - length : startPoint.X + length, startPoint.Y + offsetLength);
                    controlPoint2 = new PointF(towards == BezierLineTowards.Inner ? endPoint.X - length : endPoint.X + length, endPoint.Y - offsetLength);
                    break;
                case BezierLineSide.Top:
                    endPoint = new PointF(startPoint.X - length, startPoint.Y);
                    controlPoint1 = new PointF(startPoint.X + offsetLength, towards == BezierLineTowards.Inner ? startPoint.Y + length : startPoint.Y - length);
                    controlPoint2 = new PointF(endPoint.X - offsetLength, towards == BezierLineTowards.Inner ? endPoint.Y + length : endPoint.Y - length);
                    break;
            }
            builder.AddBezier(startPoint, controlPoint1, controlPoint2, endPoint);
            return endPoint;
        }
    }
}
