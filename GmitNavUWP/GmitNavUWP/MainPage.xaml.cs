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
            gmitNew = new Geopoint(new BasicGeoposition()
            {
                Latitude = 53.278038,
                Longitude = -9.00876,
                Altitude = 0
            }, AltitudeReferenceSystem.Surface);
            //gmitMap.ZoomLevel = 19;
            gmitMap.LandmarksVisible = false;
            // gmitMap.Center = gmit;
            

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

        public async void AddMapOverlayAsync()
        {
            // Create MapBillboard.
            await gmitMap.TrySetViewAsync(gmitNew, 20D, 0, 0);
            await gmitMap.TrySetViewAsync(gmitNew, 20D, 0, 0);
            RandomAccessStreamReference imgStream =
                RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/dgmit0.png"));
            MapControl originalCamera = gmitMap;
            Debug.WriteLine(gmitMap.ZoomLevel);
            var mapBillboard = new MapBillboard(gmitMap.ActualCamera)
            {
                Location = gmitNew,
                NormalizedAnchorPoint = new Point(1D, 0D),
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
            //gmitMap.ZoomLevel = 15;
            Debug.WriteLine(gmitMap.ZoomLevel);
            await gmitMap.TrySetViewAsync(gmitNew, 19D, 0, 0);
        }

        //gmitMap.Children.Add(mapBillboard);
        // gmitMap.MapElements.Add(mapBillboard);
        //gmitMap.MapElements.Add(mapBillboard);

        // gmitMap.Children.Clear();

        // gmitMap.Children.Add(_img);


    }
}


