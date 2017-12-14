using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        Geopoint gmitNew;
        Geopoint room993;
        public MainPage()
        {
            this.InitializeComponent();
            gmitMap.Loaded += MapConfigAsync;
        }

        public async void MapConfigAsync(object sender, RoutedEventArgs e)
        {
            gmitMap.LandmarksVisible = false;
            room993 = new Geopoint(new BasicGeoposition()
            {
                Latitude = 53.27784461170297,
                Longitude = -9.010525792837143,
                Altitude = 0
            });
            gmit = new Geopoint(new BasicGeoposition()
            {
                Latitude = 53.2785,
                Longitude = -9.01,
                Altitude = 0
            }, AltitudeReferenceSystem.Surface);
            gmitNew = new Geopoint(new BasicGeoposition()
            {
                Latitude = 53.278038,
                Longitude = -9.00876,
                Altitude = 0
            }, AltitudeReferenceSystem.Surface);

            int index = await AddMapOverlayAsync(Util.Building.New.NORTH, Util.Building.New.WEST, new Uri("ms-appx:///Assets/dgmit0.png"));
            Debug.WriteLine(index);
            index = AddMarker(53.27784461170297, -9.010525792837143, "993");
            index = AddMarker(53.27798093343066, -9.011201709672605, "994");
            index = AddMarker(Util.Building.New.NORTH, Util.Building.New.WEST, "N-W");
            index = AddMarker(Util.Building.New.SOUTH, Util.Building.New.EAST, "S-E");
            Debug.WriteLine(index);
            //gmitMap.Layers.ElementAt(index).Visible= false;
        }

        public async Task<int> AddMapOverlayAsync(Double latitude, Double longtitude, Uri img)
        {
            // Defining position for image overlay. Map View ZoomLevel defines the images size relative to map
            // calls 2x - a temp solution for inconsistency when Setting the view
            await gmitMap.TrySetViewAsync(gmit, 28D, 0, 0);
            await gmitMap.TrySetViewAsync(gmit, 28D, 0, 0);
            Geopoint position = new Geopoint(new BasicGeoposition()
            {
                Latitude = latitude,
                Longitude = longtitude,
                Altitude = 0
            }, AltitudeReferenceSystem.Surface);

            // Create MapBillboard.
            RandomAccessStreamReference imgStream =
                RandomAccessStreamReference.CreateFromUri(img);
            Debug.WriteLine(gmitMap.ZoomLevel);
            var mapBillboard = new MapBillboard(gmitMap.ActualCamera)
            {
                Location = position,
                NormalizedAnchorPoint = new Point(0D, 0D),
                Image = imgStream,
            };
            int index = gmitMap.Layers.Count;
            var GmitFloorMaps = new List<MapElement>();
            GmitFloorMaps.Add(mapBillboard);  
            var LandmarksPhotoLayer = new MapElementsLayer
            {
                ZIndex = index,
                MapElements = GmitFloorMaps,
                Visible = true
            };
            gmitMap.Layers.Add(LandmarksPhotoLayer);
            Debug.WriteLine(gmitMap.ZoomLevel);
            await gmitMap.TrySetViewAsync(gmit, 19D, 0, 0);
            return index;
        }

        public int AddMarker(Double latitude, Double longtitude, String title)
        {
            int index = gmitMap.Layers.Count;
            var MyLandmarks = new List<MapElement>();
            Geopoint snPoint = new Geopoint(new BasicGeoposition()
            {
                Latitude = latitude,
                Longitude = longtitude,
                Altitude = 0
            }, AltitudeReferenceSystem.Surface);
            var spaceNeedleIcon = new MapIcon
            {
                Location = snPoint,
                NormalizedAnchorPoint = new Point(0.5, 1.0),
                ZIndex = 0,
                Title = title
            };
            MyLandmarks.Add(spaceNeedleIcon);
            var LandmarksLayer = new MapElementsLayer
            {
                ZIndex = index,
                MapElements = MyLandmarks
            };
            gmitMap.Layers.Add(LandmarksLayer);
            return index;
        }
    }
}


