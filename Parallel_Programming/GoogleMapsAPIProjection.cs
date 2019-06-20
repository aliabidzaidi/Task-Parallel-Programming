using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace Parallel_Programming
{
    public class GoogleMapsAPIProjection
    {
        private readonly double PixelTileSize = 256d;
        private readonly double DegreesToRadiansRatio = 180d / Math.PI;
        private readonly double RadiansToDegreesRatio = Math.PI / 180d;
        private readonly PointF PixelGlobeCenter;
        private readonly double XPixelsToDegreesRatio;
        private readonly double YPixelsToRadiansRatio;

        public GoogleMapsAPIProjection(double zoomLevel)
        {
            var pixelGlobeSize = this.PixelTileSize * Math.Pow(2d, zoomLevel);
            this.XPixelsToDegreesRatio = pixelGlobeSize / 360d;
            this.YPixelsToRadiansRatio = pixelGlobeSize / (2d * Math.PI);
            var halfPixelGlobeSize = Convert.ToSingle(pixelGlobeSize / 2d);
            this.PixelGlobeCenter = new PointF(
                halfPixelGlobeSize, halfPixelGlobeSize);
        }

        public PointF FromCoordinatesToPixel(PointF coordinates)
        {
            var x = Math.Round(this.PixelGlobeCenter.X
                + (coordinates.X * this.XPixelsToDegreesRatio));
            var f = Math.Min(
                Math.Max(
                     Math.Sin(coordinates.Y * RadiansToDegreesRatio),
                    -0.9999d),
                0.9999d);
            var y = Math.Round(this.PixelGlobeCenter.Y + .5d *
                Math.Log((1d + f) / (1d - f)) * -this.YPixelsToRadiansRatio);
            return new PointF(Convert.ToSingle(x), Convert.ToSingle(y));
        }

        public PointF FromPixelToCoordinates(PointF pixel)
        {
            var longitude = (pixel.X - this.PixelGlobeCenter.X) /
                this.XPixelsToDegreesRatio;
            var latitude = (2 * Math.Atan(Math.Exp(
                (pixel.Y - this.PixelGlobeCenter.Y) / -this.YPixelsToRadiansRatio))
                - Math.PI / 2) * DegreesToRadiansRatio;
            return new PointF(
                Convert.ToSingle(latitude),
                Convert.ToSingle(longitude));
        }

        public void MainCode()
        {

            GoogleMapsAPIProjection mapsObject = new GoogleMapsAPIProjection(50);

            PointF pointX = new PointF { X = -87.64999999999998f, Y = 41.85f };

            PointF point00 = new PointF { X = 73.087527f, Y = 33.733423f };

            PointF point10 = new PointF { X = 73.088845f, Y = 33.734091f };

            PointF point01 = new PointF { X = 73.088162f, Y = 33.732593f };

            PointF point11 = new PointF { X = 73.089452f, Y = 33.733208f };


            PointF pixelX = mapsObject.FromCoordinatesToPixel(pointX);

            PointF pixel00 = mapsObject.FromCoordinatesToPixel(point00);
            PointF pixel01 = mapsObject.FromCoordinatesToPixel(point01);
            PointF pixel10 = mapsObject.FromCoordinatesToPixel(point10);
            PointF pixel11 = mapsObject.FromCoordinatesToPixel(point11);

            //Console.WriteLine("Longitude {0} = X {1}",point00.X, pixel00.X);
            //Console.WriteLine("Latitude {0} = Y {1}",point00.Y, pixel00.Y);

            Console.WriteLine("Point00 (x,y)=({0},{1})", Convert.ToDecimal(pixelX.X), Convert.ToDecimal(pixelX.Y));


            Console.WriteLine("Point00 (x,y)=({0},{1})", Convert.ToDecimal(pixel00.X) * 0.001M, Convert.ToDecimal(pixel00.Y) * 0.001M);
            Console.WriteLine("Point01 (x,y)=({0},{1})", Convert.ToDecimal(pixel01.X) * 0.001M, Convert.ToDecimal(pixel01.Y) * 0.001M);
            Console.WriteLine("Point10 (x,y)=({0},{1})", Convert.ToDecimal(pixel10.X) * 0.001M, Convert.ToDecimal(pixel10.Y) * 0.001M);
            Console.WriteLine("Point11 (x,y)=({0},{1})", Convert.ToDecimal(pixel11.X) * 0.001M, Convert.ToDecimal(pixel11.Y) * 0.001M);

        }
    }
}
