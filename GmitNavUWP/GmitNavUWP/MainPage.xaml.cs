using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace GmitNavUWP
{

    public sealed partial class MainPage : Page
    {
        Geopoint gmit;
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_LoadedAsync;
        }

        public async void MainPage_LoadedAsync(object sender, RoutedEventArgs e)
        {
            gmit = new Geopoint(new BasicGeoposition()
            {
                Latitude = 53.2785,
                Longitude = -9.01,
                Altitude = 0
            }, AltitudeReferenceSystem.Surface);
            gmitMap.ZoomLevel = 18;
            gmitMap.LandmarksVisible = false;
            gmitMap.Center = new Geopoint(new BasicGeoposition()
            {
                Latitude = 0,
                Longitude = 0
            });
            await gmitMap.TrySetViewAsync(gmit, 17D, 0, 0);

            try
            {
                AddMapOverlayAsync();
                //addOverlay();
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine("Exception");
            }

        }

        public void AddMapOverlayAsync()
        {
            // Create MapBillboard.
            RandomAccessStreamReference imgStream =
                RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/dgmit0.png"));
            var imgUri = new Uri("ms-appx:///Assets/d2.jpg");
            var _img = new Image();
            _img.Stretch = Stretch.Fill;
            _img.Source = new BitmapImage(imgUri);
            var gmitCamera = new MapCamera(gmit);
            gmitMap.ZoomLevel = 19;
            var actCamera = gmitMap.ActualCamera;
            Debug.WriteLine(gmitMap.ZoomLevel.ToString());
            


            var mapBillboard = new MapBillboard(actCamera)
            {
                Location = gmit,
                NormalizedAnchorPoint = new Point(0.5, 0.5),
                Image = imgStream,
            };
            var GmitFloorMaps = new List<MapElement>();
            GmitFloorMaps.Add(mapBillboard);  
            var LandmarksPhotoLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = GmitFloorMaps,
                Visible = true
            };

            gmitMap.Layers.Add(LandmarksPhotoLayer);
            
        }

        public void addOverlay()
        {
            BasicGeoposition northWestCorner =
                new BasicGeoposition() { Latitude = Util.Building.New.SOUTH, Longitude = Util.Building.New.EAST };
            BasicGeoposition southEastCorner =
                new BasicGeoposition() { Latitude = Util.Building.New.NORTH, Longitude = Util.Building.New.WEST };
            GeoboundingBox boundingNewBuilding = new GeoboundingBox(northWestCorner, southEastCorner);
            //BasicGeoposition northWestCorner =
            //   new BasicGeoposition() { Latitude = 48.38544, Longitude = -124.667360 };
            //BasicGeoposition southEastCorner =
            //    new BasicGeoposition() { Latitude = 25.26954, Longitude = -80.30182 };
            //GeoboundingBox boundingBox = new GeoboundingBox(northWestCorner, southEastCorner);


            LocalMapTileDataSource dataSource =
                new LocalMapTileDataSource("ms-appx:///Assets/dgmit0.png");
            MapZoomLevelRange range;
            range.Min = 17;
            range.Max = 17;
            MapTileSource tileSource = new MapTileSource(dataSource)
            {
                Bounds = boundingNewBuilding,
                ZoomLevelRange = range,
                AllowOverstretch = true,
                Layer = MapTileLayer.BackgroundReplacement
            };
            gmitMap.TileSources.Add(tileSource);
        }

        
        //gmitMap.Children.Add(mapBillboard);
        // gmitMap.MapElements.Add(mapBillboard);
        //gmitMap.MapElements.Add(mapBillboard);

        // gmitMap.Children.Clear();

        // gmitMap.Children.Add(_img);


    }
}


