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
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using GmitNavUWP.Service;
using Windows.Data.Json;
using System.Collections;
using Windows.UI.Notifications;

namespace GmitNavUWP
{

    public sealed partial class MainPage : Page
    {
        List<Room> rooms = new List<Room>();
        public MainPage()
        {
            this.InitializeComponent();
            gmitMap.Loaded += MapConfigAsync;
            gmitMap.ZoomLevelChanged += MapZoomControl;
            gmitMap.CenterChanged += CenterBoundries;
            Task.Run( () => GetRooms());
        }

        public async void GetRooms()
        {
            Neo4jDb db = new Neo4jDb();
            try
            {
                String response = await db.CypherAsync("MATCH (r:Room) RETURN r", "");
                if (response == null)
                {
                    ShowToastNotification("Network Error", "There seems to a problem conneting to the server.");
                }
                else
                {
                    var theData = JsonObject.Parse(response);
                    JsonArray data = theData.GetNamedArray("results").GetObjectAt(0).GetNamedArray("data");
                    for (int i = 0; i < data.Count; i++)
                    {
                        JsonArray nodeJSON = data.GetObjectAt(Convert.ToUInt32(i)).GetNamedArray("row");
                        JsonObject node = nodeJSON.GetObjectAt(0);
                        rooms.Add(JsonConvert.DeserializeObject<Room>(node.ToString()));
                    }
                    Debug.WriteLine(rooms.Count.ToString());
                }
            }
            catch(Exception)
            {
                ShowToastNotification("Network Error", "There seems to a problem conneting to the server.");
            }
        }

        private void CenterBoundries(MapControl sender, object args)
        {
            Debug.WriteLine(gmitMap.Center.Position.Latitude);
            Debug.WriteLine(gmitMap.Center.Position.Longitude);
            var gmit = new Geopoint(new BasicGeoposition()
            {
                Latitude = Util.Building.Old.NORTH,
                Longitude = Util.Building.Old.WEST
            });
            if (gmitMap.Center.Position.Latitude > 53.28
                || gmitMap.Center.Position.Latitude < 53.27
                || gmitMap.Center.Position.Longitude > -9.01
                || gmitMap.Center.Position.Latitude < -9.013)
                gmitMap.Center = gmit; //await gmitMap.TrySetViewAsync(gmit, 19D, 0, 0);
        }

        private async void CameraBoundriesAsync(MapControl sender, MapTargetCameraChangedEventArgs args)
        {
            var gmit = new Geopoint(new BasicGeoposition()
            {
                Latitude = Util.Building.Old.NORTH,
                Longitude = Util.Building.Old.WEST
            });
            if (gmitMap.ActualCamera.Location.Position.Latitude > 53.28 
                || gmitMap.ActualCamera.Location.Position.Latitude < 53.27 
                || gmitMap.ActualCamera.Location.Position.Longitude > -9.006 
                || gmitMap.ActualCamera.Location.Position.Latitude < -9.02)
                    await gmitMap.TrySetViewAsync(gmit, 19D, 0, 0);
        }

        private void MapZoomControl(MapControl sender, object args)
        {
            if (sender.ZoomLevel < 18) sender.ZoomLevel = 18;
        }

        public async void MapConfigAsync(object sender, RoutedEventArgs e)
        {
            gmitMap.LandmarksVisible = false;
            
            //index = await AddMapOverlayAsync(Util.Building.Old.NORTH, Util.Building.Old.WEST, new Uri("ms-appx:///Assets/dgmit0.png")); //SubLevel
            int index = await AddMapOverlayAsync(Util.Building.Old.NORTH, Util.Building.Old.WEST, new Uri("ms-appx:///Assets/dgmit0.png")); //GroundLevel
            //index = await AddMapOverlayAsync(Util.Building.Old.NORTH, Util.Building.Old.WEST, new Uri("ms-appx:///Assets/dgmit0.png")); // First Level
            //index = await AddMapOverlayAsync(Util.Building.Old.NORTH, Util.Building.Old.WEST, new Uri("ms-appx:///Assets/dgmit0.png")); // Second Level
            // dev only
            gmitMap.Layers.ElementAt(index).Visible = true; //Making Layer visible
            index = AddMarker(53.27784461170297, -9.010525792837143, "993");
            index = AddMarker(53.27798093343066, -9.011201709672605, "994");
            index = AddMarker(53.27782616819727, -9.01072964080413, "966");
            index = AddMarker(53.27927436396021, -9.009757339964608, "105");
            index = AddMarker(53.27941629407834, -9.011182934045792, "145");
        }

        public async Task<int> AddMapOverlayAsync(Double latitude, Double longtitude, Uri img)
        {
            // Defining position for image overlay. Map View ZoomLevel defines the images size relative to map
            //MapControl tmp = gmitMap;
            Geopoint position = new Geopoint(new BasicGeoposition()
            {
                Latitude = latitude,
                Longitude = longtitude,
                Altitude = 0
            }, AltitudeReferenceSystem.Surface);
            //Making sure the view ZoomLevel is correct for inserting image as overlay.
            while (gmitMap.ZoomLevel < 18.9)
            {
                await gmitMap.TrySetViewAsync(position, 19D, 0, 0);
                Debug.WriteLine(gmitMap.ZoomLevel);
            }
            // Create MapBillboard Image.
            RandomAccessStreamReference imgStream =
                RandomAccessStreamReference.CreateFromUri(img);
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
                Visible = false
            };
            gmitMap.Layers.Add(LandmarksPhotoLayer);
            await gmitMap.TrySetViewAsync(position, 19D, 0, 0);
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
                NormalizedAnchorPoint = new Point(0D, 0D),
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

        private void ShowToastNotification(string title, string stringContent)
        {
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(stringContent));
            Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = DateTime.Now.AddSeconds(4);
            ToastNotifier.Show(toast);
        }
    }
}


